using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Loupedeck.GoXLR.Utility.Plugin.Actions
{
    // This class implements an example adjustment that counts the rotation ticks of a dial.
    public abstract class VolumeAdjustment : PluginDynamicAdjustment
    {
        private UtilityPlugin _plugin;

        private GoXlrUtilityClient Client => _plugin?.Client;

        // This variable holds the current value of the counter.
        protected int _volume;
        protected int _muteVolume;

        public abstract string ChannelName { get; }
        
        // Initializes the adjustment class.
        // When `hasReset` is set to true, a reset command is automatically created for this adjustment.
        protected VolumeAdjustment(string name, string displayName)
            : base(name, displayName, "Adjust Volume", hasReset: true)
        {
            //
        }

        protected override bool OnLoad()
        {
            _plugin = (UtilityPlugin)Plugin;
            Client.PatchEvent += IsVolumeChangePatchEvent;

            return true;
        }

        protected override bool OnUnload()
        {
            Client.PatchEvent -= IsVolumeChangePatchEvent;

            return true;
        }

        private void IsVolumeChangePatchEvent(object sender, Patch patch)
        {
            var match = Regex.Match(patch.Path, $@"/mixers/(?<serial>\w+)/levels/volumes/{ChannelName}");
            if (!match.Success)
                return;

            //var serial = match.Groups["serial"];
            _volume = patch.Value.ToObject<int>();
            AdjustmentValueChanged();
        }

        public void SetVolume(int diff)
        {
            var volume = _volume + (int)Math.Round(diff * 2.55d); // Increase or decrease the counter by the number of ticks.
            
            if (volume > byte.MaxValue) volume = byte.MaxValue;
            if (volume < 0) volume = 0;
            
            var command = new
            {
                SetVolume = new object[] {
                    ChannelName,
                    volume
                }
            };

            Client.SendCommand(command);
        }

        // This method is called when the dial associated to the plugin is rotated.
        protected override void ApplyAdjustment(string actionParameter, int diff)
        {
            SetVolume(diff);

            AdjustmentValueChanged(); // Notify the Loupedeck service that the adjustment value has changed.
        }

        // This method is called when the reset command related to the adjustment is executed.
        protected override void RunCommand(string actionParameter)
        {
            //Mute can only be done on fader level, not channel level.
            if (_muteVolume == 0)
            {
                _muteVolume = _volume;
                SetVolume(0);
            }
            else
            {
                SetVolume((byte)_muteVolume);
                _muteVolume = 0;
            }
            
            AdjustmentValueChanged(); // Notify the Loupedeck service that the adjustment value has changed.
        }

        // Returns the adjustment value that is shown next to the dial.
        protected override string GetAdjustmentValue(string actionParameter)
            => Math.Round(_volume * 100d / 0xFF).ToString(CultureInfo.InvariantCulture);

        protected override string GetCommandDisplayName(string actionParameter, PluginImageSize imageSize)
            => $"{ChannelName} Mute";

        protected override string GetAdjustmentDisplayName(string actionParameter, PluginImageSize imageSize)
            => $"{ChannelName} Volume";
    }

    public class MicVolumeAdjustment : VolumeAdjustment
    {
        public override string ChannelName => "Mic";

        public MicVolumeAdjustment()
            : base("Mic", "Mic") { }
    }

    public class LineInVolumeAdjustment : VolumeAdjustment
    {
        public override string ChannelName => "LineIn";

        public LineInVolumeAdjustment()
            : base("Line In", "Line In") { }
    }

    public class ConsoleVolumeAdjustment : VolumeAdjustment
    {
        public override string ChannelName => "Console";

        public ConsoleVolumeAdjustment()
            : base("Console", "Console") { }
    }

    public class SystemVolumeAdjustment : VolumeAdjustment
    {
        public override string ChannelName => "System";

        public SystemVolumeAdjustment()
            : base("System", "System") { }
    }

    public class GameVolumeAdjustment : VolumeAdjustment
    {
        public override string ChannelName => "Game";

        public GameVolumeAdjustment()
            : base("Game", "Game") { }
    }

    public class ChatVolumeAdjustment : VolumeAdjustment
    {
        public override string ChannelName => "Chat";

        public ChatVolumeAdjustment()
            : base("Chat", "Chat") { }
    }

    public class SampleVolumeAdjustment : VolumeAdjustment
    {
        public override string ChannelName => "Sample";

        public SampleVolumeAdjustment()
            : base("Sample", "Sample") { }
    }

    public class MusicVolumeAdjustment : VolumeAdjustment
    {
        public override string ChannelName => "Music";

        public MusicVolumeAdjustment()
            : base("Music", "Music") { }
    }

    public class HeadphonesVolumeAdjustment : VolumeAdjustment
    {
        public override string ChannelName => "Headphones";

        public HeadphonesVolumeAdjustment()
            : base("Headphones", "Headphones") { }
    }

    public class MicMonitorVolumeAdjustment : VolumeAdjustment
    {
        public override string ChannelName => "MicMonitor";

        public MicMonitorVolumeAdjustment()
            : base("Mic Monitor", "Mic Monitor") { }
    }

    public class LineOutVolumeAdjustment : VolumeAdjustment
    {
        public override string ChannelName => "LineOut";

        public LineOutVolumeAdjustment()
            : base("Line Out", "Line Out") { }
    }
}
