using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Loupedeck.GoXLR.Utility.Plugin.Actions
{
    public class ProfileCommand : PluginDynamicCommand
    {
        private UtilityPlugin _plugin;

        private GoXlrUtilityClient Client => _plugin?.Client;

        private List<string> _profiles = new List<string>();
        private string _selectedProfile;

        public ProfileCommand()
        {
            DisplayName = "Profile Set";
            GroupName = "";
            Description = "Select profile.";

            MakeProfileAction("list;Profile:");
        }

        protected override bool OnLoad()
        {
            _plugin = (UtilityPlugin)Plugin;
            Client.PatchEvent += IsProfileNameIndexPatchEvent;
            Client.PatchEvent += IsProfileNamePatchEvent;
            Client.PatchEvent += IsProfileListPatchEvent;

            return true;
        }

        protected override bool OnUnload()
        {
            Client.PatchEvent -= IsProfileNameIndexPatchEvent;
            Client.PatchEvent -= IsProfileNamePatchEvent;
            Client.PatchEvent -= IsProfileListPatchEvent;

            return true;
        }

        private void IsProfileNamePatchEvent(object sender, Patch patch)
        {
            if (!Regex.IsMatch(patch.Path, @"/mixers/(?<serial>\w+)/profile_name"))
                return;

            _selectedProfile = patch.Value.ToObject<string>();
            ActionImageChanged();
        }

        private void IsProfileNameIndexPatchEvent(object sender, Patch patch)
        {
            var match = Regex.Match(patch.Path, @"/files/profiles/(?<index>\d+)");
            if (!match.Success)
                return;

            var stringIndex = match.Groups["index"].Value;
            if (!int.TryParse(stringIndex, out var index))
                return;
            
            var value = patch.Value?.ToObject<string>();
            switch (patch.Op)
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

        private void IsProfileListPatchEvent(object sender, Patch patch)
        {
            if (!Regex.IsMatch(patch.Path, @"/files/profiles"))
                return;

            _profiles = patch.Value.ToObject<List<string>>();
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

        protected override BitmapImage GetCommandImage(string actionParameter, PluginImageSize imageSize)
        {
            if (actionParameter is null)
                return null;

            using (var bitmapBuilder = new BitmapBuilder(imageSize))
            {
                var color = actionParameter == _selectedProfile
                    ? new BitmapColor(0x00, 0x50, 0x00)
                    : BitmapColor.Black;

                bitmapBuilder.Clear(color);
                bitmapBuilder.DrawText(actionParameter);

                return bitmapBuilder.ToImage();
            }
        }
    }
}
