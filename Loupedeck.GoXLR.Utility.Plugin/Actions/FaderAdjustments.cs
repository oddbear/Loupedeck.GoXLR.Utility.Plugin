using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace Loupedeck.GoXLR.Utility.Plugin.Actions
{
    public abstract class FaderAdjustments : PluginDynamicAdjustment
    {
        private UtilityPlugin _plugin;

        private GoXlrUtiltyClient Client => _plugin?.Client;
        
        private string _channelName;
        private MuteFunction _muteType;
        private int _volume;

        protected abstract string FaderName { get; }
        
        protected FaderAdjustments(string name, string displayName)
            : base(name, displayName, "Adjust Fader", hasReset: true)
        {
            //
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
            if (IsChannelChangedPatchEvent(e))
            {
                _channelName = e.Value.ToObject<string>();
                AdjustmentValueChanged();
            }

            if (IsMuteTypePatchEvent(e))
            {
                _muteType = e.Value.ToObject<MuteFunction>();
            }
            
            if (IsVolumeChangePatchEvent(e))
            {
                _volume = e.Value.ToObject<int>();
                AdjustmentValueChanged();
            }
        }
        
        private bool IsChannelChangedPatchEvent(Patch patch)
            => Regex.IsMatch(patch.Path, $@"/mixers/(?<serial>\w+)/fader_status/{FaderName}/channel");

        private bool IsMuteTypePatchEvent(Patch patch)
            => Regex.IsMatch(patch.Path, $@"/mixers/(?<serial>\w+)/fader_status/{FaderName}/mute_type");

        private bool IsVolumeChangePatchEvent(Patch patch)
            => Regex.IsMatch(patch.Path, $@"/mixers/(?<serial>\w+)/levels/volumes/{_channelName}");

        // This method is called when the dial associated to the plugin is rotated.
        protected override void ApplyAdjustment(string actionParameter, int diff)
        {
            var volumeAdjustment = _plugin
                .DynamicAdjustments
                ?.OfType<VolumeAdjustment>()
                .FirstOrDefault(adjustment => adjustment.ChannelName == _channelName);

            if (volumeAdjustment is null)
                return;

            volumeAdjustment.SetVolume(diff);

            AdjustmentValueChanged(); // Notify the Loupedeck service that the adjustment value has changed.
        }

        protected override void RunCommand(string actionParameter)
        {
            AdjustmentValueChanged();
        }

        protected override string GetCommandDisplayName(string actionParameter, PluginImageSize imageSize)
        {
            return $"Fader {FaderName}";
        }

        //TODO: Do I need to double check against: _plugin.DynamicAdjustments.OfType<VolumeAdjustment>();
        // ... if this is for some reason set before Faders? At this time this should not happen.
        protected override string GetAdjustmentValue(string actionParameter)
            => Math.Round(_volume * 100d / 0xFF).ToString(CultureInfo.InvariantCulture);
    }

    public enum MuteFunction
    {
        All,
        ToStream,
        ToVoiceChat,
        ToPhones,
        ToLineOut,
    }

    public class FaderA_Adjustment : FaderAdjustments
    {
        protected override string FaderName => "A";

        public FaderA_Adjustment()
            : base("Fader A", "Fader A") { }
    }

    public class FaderB_Adjustment : FaderAdjustments
    {
        protected override string FaderName => "B";

        public FaderB_Adjustment()
            : base("Fader B", "Fader B") { }
    }

    public class FaderC_Adjustment : FaderAdjustments
    {
        protected override string FaderName => "C";

        public FaderC_Adjustment()
            : base("Fader C", "Fader C") { }
    }

    public class FaderD_Adjustment : FaderAdjustments
    {
        protected override string FaderName => "D";

        public FaderD_Adjustment()
            : base("Fader D", "Fader D") { }
    }
}
