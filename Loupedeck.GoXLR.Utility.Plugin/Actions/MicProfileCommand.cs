using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Loupedeck.GoXLR.Utility.Plugin.Enums;

namespace Loupedeck.GoXLR.Utility.Plugin.Actions
{
    public class MicProfileCommand : PluginDynamicCommand
    {
        private GoXLRUtilityPlugin _plugin;

        private GoXlrUtilityClient Client => _plugin?.Client;

        private List<string> _micProfiles = new List<string>();
        private string _selectedMicProfile;

        public MicProfileCommand()
        {
            DisplayName = "Mic Profile Set";
            GroupName = "";
            Description = "Select mic profile.";

            MakeProfileAction("list;Mic Profile:");
        }

        protected override bool OnLoad()
        {
            _plugin = (GoXLRUtilityPlugin)Plugin;
            Client.PatchEvent += IsMicProfileNameIndexPatchEvent;
            Client.PatchEvent += IsMicProfileNamePatchEvent;
            Client.PatchEvent += IsMicProfileListPatchEvent;

            return true;
        }

        protected override bool OnUnload()
        {
            Client.PatchEvent -= IsMicProfileNameIndexPatchEvent;
            Client.PatchEvent -= IsMicProfileNamePatchEvent;
            Client.PatchEvent -= IsMicProfileListPatchEvent;

            return true;
        }

        private void IsMicProfileNamePatchEvent(object sender, Patch patch)
        {
            if (!Regex.IsMatch(patch.Path, @"/mixers/(?<serial>\w+)/mic_profile_name"))
                return;

            _selectedMicProfile = patch.Value.ToObject<string>();
            ActionImageChanged();
        }

        private void IsMicProfileNameIndexPatchEvent(object sender, Patch patch)
        {
            var match = Regex.Match(patch.Path, @"/files/mic_profiles/(?<index>\d+)");
            if (!match.Success)
                return;

            var stringIndex = match.Groups["index"].Value;
            if (!int.TryParse(stringIndex, out var index))
                return;

            var value = patch.Value?.ToObject<string>();
            switch (patch.Op)
            {
                case OpPatchEnum.Add:
                    _micProfiles.Add(value);
                    break;
                case OpPatchEnum.Remove:
                    _micProfiles.RemoveAt(index);
                    break;
                case OpPatchEnum.Replace:
                    _micProfiles[index] = value;
                    break;
            }
        }

        private void IsMicProfileListPatchEvent(object sender, Patch patch)
        {
            if (!Regex.IsMatch(patch.Path, @"/files/mic_profiles"))
                return;

            _micProfiles = patch.Value.ToObject<List<string>>();
        }

        protected override void RunCommand(string actionParameter)
        {
            Client.SendCommand("LoadMicProfile", actionParameter);
        }

        protected override PluginActionParameter[] GetParameters() =>
            _micProfiles
                .Select(profileName => new PluginActionParameter(profileName, profileName, string.Empty))
                .ToArray();

        protected override BitmapImage GetCommandImage(string actionParameter, PluginImageSize imageSize)
        {
            if (actionParameter is null)
                return null;
            
            using (var bitmapBuilder = new BitmapBuilder(imageSize))
            {
                var color = actionParameter == _selectedMicProfile
                    ? new BitmapColor(0x00, 0x50, 0x00)
                    : BitmapColor.Black;

                bitmapBuilder.Clear(color);
                bitmapBuilder.DrawText(actionParameter);

                return bitmapBuilder.ToImage();
            }
        }
    }
}
