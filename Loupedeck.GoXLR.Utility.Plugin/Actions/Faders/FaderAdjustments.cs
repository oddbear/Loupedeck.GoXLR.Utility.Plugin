using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using Loupedeck.GoXLR.Utility.Plugin.Actions.Mixer;
using Loupedeck.GoXLR.Utility.Plugin.Enums;

namespace Loupedeck.GoXLR.Utility.Plugin.Actions.Faders
{
    public abstract class FaderAdjustments : PluginDynamicAdjustment
    {
        private GoXLRUtilityPlugin _plugin;

        private GoXlrUtilityClient Client => _plugin?.Client;
        
        private ChannelName _channelName;
        private MuteFunction _muteType;
        private MuteState _muteState;
        private int _volume;

        protected abstract FaderName FaderName { get; }
        
        protected FaderAdjustments(string displayName, string description)
            : base(displayName, description, "Faders", hasReset: true)
        {
            //
        }

        protected override bool OnLoad()
        {
            _plugin = (GoXLRUtilityPlugin)Plugin;
            Client.PatchEvent += IsChannelChangedPatchEvent;
            Client.PatchEvent += IsMuteStatePatchEvent;
            Client.PatchEvent += IsMuteTypePatchEvent;
            Client.PatchEvent += IsVolumeChangePatchEvent;

            return true;
        }

        protected override bool OnUnload()
        {
            Client.PatchEvent -= IsChannelChangedPatchEvent;
            Client.PatchEvent -= IsMuteStatePatchEvent;
            Client.PatchEvent -= IsMuteTypePatchEvent;
            Client.PatchEvent -= IsVolumeChangePatchEvent;

            return true;
        }
        
        private void IsChannelChangedPatchEvent(object sender, Patch patch)
        {
            if (!Regex.IsMatch(patch.Path, $@"/mixers/(?<serial>\w+)/fader_status/{FaderName}/channel"))
                return;

            _channelName = patch.Value.ToObject<ChannelName>();
            AdjustmentValueChanged();
        }

        private void IsMuteStatePatchEvent(object sender, Patch patch)
        {
            if (!Regex.IsMatch(patch.Path, $@"/mixers/(?<serial>\w+)/fader_status/{FaderName}/mute_state"))
                return;

            _muteState = patch.Value.ToObject<MuteState>();
        }

        private void IsMuteTypePatchEvent(object sender, Patch patch)
        {
            if (!Regex.IsMatch(patch.Path, $@"/mixers/(?<serial>\w+)/fader_status/{FaderName}/mute_type"))
                return;

            _muteType = patch.Value.ToObject<MuteFunction>();
        }

        private void IsVolumeChangePatchEvent(object sender, Patch patch)
        {
            if (!Regex.IsMatch(patch.Path, $@"/mixers/(?<serial>\w+)/levels/volumes/{_channelName}"))
                return;

            _volume = patch.Value.ToObject<int>();
            AdjustmentValueChanged();
        }
        
        //Here it's possible to have a Dictionary for all channels instead.
        protected override void ApplyAdjustment(string actionParameter, int diff)
        {
            var volumeAdjustment = _plugin
                .DynamicAdjustments
                ?.OfType<VolumeAdjustment>()
                .FirstOrDefault(adjustment => adjustment.ChannelName == _channelName);

            if (volumeAdjustment is null)
                return;

            volumeAdjustment.SetVolume(diff);

            AdjustmentValueChanged();
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

    public class FaderA_Adjustment : FaderAdjustments
    {
        protected override FaderName FaderName => FaderName.A;

        public FaderA_Adjustment()
            : base("Fader A", "Fader A") { }
    }

    public class FaderB_Adjustment : FaderAdjustments
    {
        protected override FaderName FaderName => FaderName.B;

        public FaderB_Adjustment()
            : base("Fader B", "Fader B") { }
    }

    public class FaderC_Adjustment : FaderAdjustments
    {
        protected override FaderName FaderName => FaderName.C;

        public FaderC_Adjustment()
            : base("Fader C", "Fader C") { }
    }

    public class FaderD_Adjustment : FaderAdjustments
    {
        protected override FaderName FaderName => FaderName.D;

        public FaderD_Adjustment()
            : base("Fader D", "Fader D") { }
    }
}
