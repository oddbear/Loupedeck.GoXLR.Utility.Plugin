using System.Text.RegularExpressions;
using Loupedeck.GoXLR.Utility.Plugin.Enums;

namespace Loupedeck.GoXLR.Utility.Plugin.Actions.Microphone
{
    //TODO: Command not implemented yet in in the goxlr utility.
    public class SetCoughMuteStateCommand : PluginDynamicCommand
    {
        private GoXLRUtilityPlugin _plugin;

        private GoXlrUtilityClient Client => _plugin?.Client;

        private MuteState _muteState;
        private MuteFunction _muteType;

        public SetCoughMuteStateCommand()
            : base("Cough", "Change cough state", "Microphone")
        {
            //
        }

        protected override bool OnLoad()
        {
            _plugin = (GoXLRUtilityPlugin)Plugin;
            Client.PatchEvent += IsCoughMuteTypeChangePatchEvent;
            Client.PatchEvent += IsCoughStateChangePatchEvent;

            return true;
        }

        protected override bool OnUnload()
        {
            Client.PatchEvent -= IsCoughMuteTypeChangePatchEvent;
            Client.PatchEvent -= IsCoughStateChangePatchEvent;

            return true;
        }
        
        private void IsCoughMuteTypeChangePatchEvent(object sender, Patch patch)
        {
            var match = Regex.Match(patch.Path, @"/mixers/(?<serial>\w+)/cough_button/mute_type");
            if (!match.Success)
                return;

            _muteType = patch.Value.ToObject<MuteFunction>();

            ActionImageChanged();
        }

        private void IsCoughStateChangePatchEvent(object sender, Patch patch)
        {
            var match = Regex.Match(patch.Path, @"/mixers/(?<serial>\w+)/cough_button/state");
            if (!match.Success)
                return;

            _muteState = patch.Value.ToObject<MuteState>();

            ActionImageChanged();
        }

        protected override void RunCommand(string actionParameter)
        {
            //If it's muted, then unmute:
            if (_muteState != MuteState.Unmuted)
            {
                Client.SendCommand("SetCoughMuteState", MuteState.Unmuted);
                return;
            }

            //Or mute by rule (is this correct to assume?):
            var muteState = _muteType == MuteFunction.All
                ? MuteState.MutedToAll
                : MuteState.MutedToX;

            Client.SendCommand("SetCoughMuteState", muteState);
        }

        protected override BitmapImage GetCommandImage(string actionParameter, PluginImageSize imageSize)
        {
            using (var bitmapBuilder = new BitmapBuilder(imageSize))
            {
                var color = _muteState == MuteState.Unmuted
                    ? new BitmapColor(0x00, 0x50, 0x00)
                    : BitmapColor.Black;

                bitmapBuilder.Clear(color);
                bitmapBuilder.DrawText(DisplayName);

                return bitmapBuilder.ToImage();
            }
        }
    }
}
