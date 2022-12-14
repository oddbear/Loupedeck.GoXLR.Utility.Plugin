using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Loupedeck.GoXLR.Utility.Plugin.Actions
{
    public class ChangeProfileCommand : PluginDynamicCommand
    {
        private UtilityPlugin _plugin;

        private GoXlrUtiltyClient Client => _plugin?.Client;

        private readonly List<string> _profiles = new List<string>();

        public ChangeProfileCommand()
        {
            this.DisplayName = "Profile Set";
            this.GroupName = "";
            this.Description = "Select profile.";

            this.MakeProfileAction("list;Profile:");
        }

        protected override bool OnLoad()
        {
            _plugin = (UtilityPlugin)Plugin;
            Client.PatchEvent += ClientOnPatchEvent;

            return true;
        }

        protected override bool OnUnload()
        {
            Client.PatchEvent -= ClientOnPatchEvent;

            return true;
        }
        
        private void ClientOnPatchEvent(object sender, Patch e)
        {
            if (IsProfileNamePatchEvent(e))
            {
                var value = e.Value.ToObject<string>();
                _profiles.Add(value);
            }

            var (isProfileNameIndex, index) = IsProfileNameIndexPatchEvent(e);
            if (isProfileNameIndex)
            {
                var value = e.Value.ToObject<string>();
                switch (e.Op)
                {
                    case OpPatchEnum.Add:
                        _profiles.Add(value);
                        break;
                    case OpPatchEnum.Remove:
                        _profiles.RemoveAt(index);
                        break;
                    case OpPatchEnum.Replace:
                        _profiles[index] = value;
                        break;
                }
            }
        }

        private bool IsProfileNamePatchEvent(Patch patch)
            => Regex.IsMatch(patch.Path, @"/mixers/(?<serial>\w+)/profile_name");

        //TODO: I cannot get this to work in the Web UI... is it /profile_nameprofile_name/<index>, or just /profile_nameprofile_name
        private (bool, int) IsProfileNameIndexPatchEvent(Patch patch)
        {
            var match = Regex.Match(patch.Path, @"/mixers/(?<serial>\w+)/profile_name/(?<index>\d+)");
            if (!match.Success)
                return (false, default);

            var stringIndex = match.Groups["index"].Value;
            return int.TryParse(stringIndex, out var index)
                ? (true, index)
                : (false, default);
        }

        protected override void RunCommand(string actionParameter)
        {
            var command = new
            {
                LoadProfile = actionParameter
            };
            
            Client.SendCommand(command);
        }

        protected override PluginActionParameter[] GetParameters() =>
            _profiles
                .Select(profileName => new PluginActionParameter(profileName, profileName, string.Empty))
                .ToArray();
    }
}
