using System;
using System.Globalization;
using System.Text.RegularExpressions;
using Loupedeck.GoXLR.Utility.Plugin.Enums;

namespace Loupedeck.GoXLR.Utility.Plugin.Actions.Mixer
{
    // This class implements an example adjustment that counts the rotation ticks of a dial.
    public abstract class VolumeAdjustment : PluginDynamicAdjustment
    {
        private GoXLRUtilityPlugin _plugin;

        private GoXlrUtilityClient Client => _plugin?.Client;

        // This variable holds the current value of the counter.
        private int _volumePercentage;
        private int _muteVolume;

        public abstract ChannelName ChannelName { get; }
        
        // Initializes the adjustment class.
        // When `hasReset` is set to true, a reset command is automatically created for this adjustment.
        protected VolumeAdjustment(string name, string displayName)
            : base(name, displayName, "Mixer", hasReset: true)
        {
            //
        }

        protected override bool OnLoad()
        {
            _plugin = (GoXLRUtilityPlugin)Plugin;
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
            var volume = patch.Value.ToObject<int>();
            _volumePercentage = (int)Math.Round(volume / 2.55d);
            AdjustmentValueChanged();
        }

        public void SetVolume(int diff)
        {
            var volumePercentage = _volumePercentage += diff;
            if (volumePercentage > 100) volumePercentage = _volumePercentage = 100;
            if (volumePercentage < 0) volumePercentage = _volumePercentage = 0;

            var volume = (int)Math.Round(volumePercentage * 2.55d); // Increase or decrease the counter by the number of ticks.

            if (volume > byte.MaxValue) volume = byte.MaxValue;
            if (volume < 0) volume = 0;
            
            var command = new
            {
                SetVolume = new object[] {
                    ChannelName.ToString(),
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
                _muteVolume = _volumePercentage;
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
            => _volumePercentage.ToString(CultureInfo.InvariantCulture);

        protected override string GetCommandDisplayName(string actionParameter, PluginImageSize imageSize)
            => $"{ChannelName} Mute";

        protected override string GetAdjustmentDisplayName(string actionParameter, PluginImageSize imageSize)
            => $"{ChannelName} Volume";
    }

    public class MicVolumeAdjustment : VolumeAdjustment
    {
        public override ChannelName ChannelName => ChannelName.Mic;

        public MicVolumeAdjustment()
            : base("Mic", "Mic") { }
    }

    public class LineInVolumeAdjustment : VolumeAdjustment
    {
        public override ChannelName ChannelName => ChannelName.LineIn;

        public LineInVolumeAdjustment()
            : base("Line In", "Line In") { }
    }

    public class ConsoleVolumeAdjustment : VolumeAdjustment
    {
        public override ChannelName ChannelName => ChannelName.Console;

        public ConsoleVolumeAdjustment()
            : base("Console", "Console") { }
    }

    public class SystemVolumeAdjustment : VolumeAdjustment
    {
        public override ChannelName ChannelName => ChannelName.System;

        public SystemVolumeAdjustment()
            : base("System", "System") { }
    }

    public class GameVolumeAdjustment : VolumeAdjustment
    {
        public override ChannelName ChannelName => ChannelName.Game;

        public GameVolumeAdjustment()
            : base("Game", "Game") { }
    }

    public class ChatVolumeAdjustment : VolumeAdjustment
    {
        public override ChannelName ChannelName => ChannelName.Chat;

        public ChatVolumeAdjustment()
            : base("Chat", "Chat") { }
    }

    public class SampleVolumeAdjustment : VolumeAdjustment
    {
        public override ChannelName ChannelName => ChannelName.Sample;

        public SampleVolumeAdjustment()
            : base("Sample", "Sample") { }
    }

    public class MusicVolumeAdjustment : VolumeAdjustment
    {
        public override ChannelName ChannelName => ChannelName.Music;

        public MusicVolumeAdjustment()
            : base("Music", "Music") { }
    }

    public class HeadphonesVolumeAdjustment : VolumeAdjustment
    {
        public override ChannelName ChannelName => ChannelName.Headphones;

        public HeadphonesVolumeAdjustment()
            : base("Headphones", "Headphones") { }
    }

    public class MicMonitorVolumeAdjustment : VolumeAdjustment
    {
        public override ChannelName ChannelName => ChannelName.MicMonitor;

        public MicMonitorVolumeAdjustment()
            : base("Mic Monitor", "Mic Monitor") { }
    }

    public class LineOutVolumeAdjustment : VolumeAdjustment
    {
        public override ChannelName ChannelName => ChannelName.LineOut;

        public LineOutVolumeAdjustment()
            : base("Line Out", "Line Out") { }
    }
}
