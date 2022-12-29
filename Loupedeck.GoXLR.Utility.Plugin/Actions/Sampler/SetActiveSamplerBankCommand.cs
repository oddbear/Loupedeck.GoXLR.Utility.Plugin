using System;
using Loupedeck.GoXLR.Utility.Plugin.Enums;

namespace Loupedeck.GoXLR.Utility.Plugin.Actions.Sampler
{
    public class SetActiveSamplerBankCommand : PluginDynamicCommand
    {
        private GoXLRUtilityPlugin _plugin;

        private GoXlrUtilityClient Client => _plugin?.Client;
        
        public SetActiveSamplerBankCommand()
        {
            var values = Enum.GetNames(typeof(SampleBank));
            foreach (var value in values)
            {
                AddParameter(value, $"Set bank {value}", "Sampler");
            }
        }

        protected override bool OnLoad()
        {
            _plugin = (GoXLRUtilityPlugin)Plugin;

            //TODO: Is there a event happening on change?

            return true;
        }

        protected override bool OnUnload()
        {
            return true;
        }

        protected override void RunCommand(string actionParameter)
        {
            if (actionParameter is null)
                return;

            //TODO: The command seems right, but nothing is happening:
            Client.SendCommand("SetActiveSamplerBank", actionParameter);
        }
    }
}
