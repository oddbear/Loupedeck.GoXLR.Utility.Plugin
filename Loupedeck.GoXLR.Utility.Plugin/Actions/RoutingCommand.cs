using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Loupedeck.GoXLR.Utility.Plugin.Enums;

namespace Loupedeck.GoXLR.Utility.Plugin.Actions
{
    public class RoutingCommand : PluginDynamicCommand
    {
        private readonly Dictionary<string, bool> _states = new Dictionary<string, bool>();

        private UtilityPlugin _plugin;

        private GoXlrUtilityClient Client => _plugin?.Client;

        public RoutingCommand()
        {
            DisplayName = "Routing Toggle";
            GroupName = "";
            Description = "Select input and output to toggle.";

            MakeProfileAction("tree");
        }

        protected override bool OnLoad()
        {
            _plugin = (UtilityPlugin)Plugin;
            Client.PatchEvent += IsRouteChangePatchEvent;

            return true;
        }

        protected override bool OnUnload()
        {
            Client.PatchEvent -= IsRouteChangePatchEvent;

            return true;
        }

        private void IsRouteChangePatchEvent(object sender, Patch patch)
        {
            var match = Regex.Match(patch.Path, @"/mixers/(?<serial>\w+)/router/(?<input>\w+)/(?<output>\w+)");
            if (!match.Success)
                return;

            var stringInput = match.Groups["input"].Value;
            var stringOutput = match.Groups["output"].Value;

            if (!Enum.TryParse<InputDevice>(stringInput, out var input))
                return;

            if (!Enum.TryParse<OutputDevice>(stringOutput, out var output))
                return;
            
            _states[$"{input}|{output}"] = patch.Value.ToObject<bool>();
            AdjustmentValueChanged($"{input}|{output}");
        }
        
        protected override PluginProfileActionData GetProfileActionData()
        {
            var tree = new PluginProfileActionTree("Routing Tree");

            tree.AddLevel("Inputs");
            tree.AddLevel("Outputs");
            
            foreach (var input in Enum.GetNames(typeof(InputDevice)))
            {
                var node = tree.Root.AddNode(input);
                
                foreach (var output in Enum.GetNames(typeof(OutputDevice)))
                {
                    node.AddItem($"{input}|{output}", output, $"{input}|{output}");
                }
            }

            return tree;
        }

        protected override void RunCommand(string actionParameter)
        {
            if (actionParameter is null)
                return;

            if (!_states.TryGetValue(actionParameter, out var state))
                return;

            var routing = actionParameter.Split('|');

            var command = new
            {
                SetRouter = new object[]
                {
                    routing[0],
                    routing[1],
                    !state
                }
            };

            Client.SendCommand(command);
        }
    }
}
