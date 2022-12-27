using System.Text.RegularExpressions;
using Loupedeck.GoXLR.Utility.Plugin.Enums;

namespace Loupedeck.GoXLR.Utility.Plugin.Actions.Faders
{
    //TODO: Move to Fader Adjustments?
    public abstract class FaderMuteCommand : PluginDynamicCommand
    {
        private GoXLRUtilityPlugin _plugin;

        private GoXlrUtilityClient Client => _plugin?.Client;

        protected abstract FaderName FaderName { get; }

        private MuteFunction _muteType;
        private MuteState _muteState;

        protected FaderMuteCommand(string displayName, string description)
            : base(displayName, $"Fader {description} mute toggle", "Faders")
        {
            //
        }

        protected override bool OnLoad()
        {
            _plugin = (GoXLRUtilityPlugin)Plugin;
            Client.PatchEvent += IsMuteTypeChangePatchEvent;
            Client.PatchEvent += IsMuteStateChangePatchEvent;

            return true;
        }

        protected override bool OnUnload()
        {
            Client.PatchEvent -= IsMuteTypeChangePatchEvent;
            Client.PatchEvent -= IsMuteStateChangePatchEvent;

            return true;
        }

        private void IsMuteTypeChangePatchEvent(object sender, Patch patch)
        {
            var match = Regex.Match(patch.Path, $@"/mixers/(?<serial>\w+)/fader_status/{FaderName}/mute_type");
            if (!match.Success)
                return;
            
            _muteType = patch.Value.ToObject<MuteFunction>();
            ActionImageChanged();
        }

        private void IsMuteStateChangePatchEvent(object sender, Patch patch)
        {
            var match = Regex.Match(patch.Path, $@"/mixers/(?<serial>\w+)/fader_status/{FaderName}/mute_state");
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
                Client.SendCommand("SetFaderMuteState", FaderName, MuteState.Unmuted);
                return;
            }

            //Or mute by rule (is this correct to assume?):
            var muteState = _muteType == MuteFunction.All
                ? MuteState.MutedToAll
                : MuteState.MutedToX;

            Client.SendCommand("SetFaderMuteState", FaderName, muteState);
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

    public class FaderA_MuteCommand : FaderMuteCommand
    {
        protected override FaderName FaderName => FaderName.A;

        public FaderA_MuteCommand()
            : base("Fader A Mute", "A") { }
    }

    public class FaderB_MuteCommand : FaderMuteCommand
    {
        protected override FaderName FaderName => FaderName.B;

        public FaderB_MuteCommand()
            : base("Fader B Mute", "B") { }
    }

    public class FaderC_MuteCommand : FaderMuteCommand
    {
        protected override FaderName FaderName => FaderName.C;

        public FaderC_MuteCommand()
            : base("Fader C Mute", "C") { }
    }

    public class FaderD_MuteCommand : FaderMuteCommand
    {
        protected override FaderName FaderName => FaderName.D;
        
        public FaderD_MuteCommand()
            : base("Fader D Mute", "D") { }
    }
}
