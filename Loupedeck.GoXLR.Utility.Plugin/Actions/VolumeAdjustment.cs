using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Loupedeck.GoXLR.Utility.Plugin.Actions
{
    // This class implements an example adjustment that counts the rotation ticks of a dial.
    public abstract class VolumeAdjustment : PluginDynamicAdjustment
    {
        private UtilityPlugin _plugin;

        private GoXlrUtiltyClient Client => _plugin?.Client;

        // This variable holds the current value of the counter.
        protected int _volume;
        protected int _muteVolume;

        protected abstract string ChannelName { get; }
        
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
            if (!IsVolumeChangePatchEvent(e))
                return;

            _volume = e.Value.ToObject<int>();
            AdjustmentValueChanged();
        }

        protected abstract bool IsVolumeChangePatchEvent(Patch patch);

        private void SetVolume(string channel, byte volume)
        {
            var command = new
            {
                SetVolume = new object[] {
                    channel,
                    volume
                }
            };

            Client.SendCommand(command);
        }

        // This method is called when the dial associated to the plugin is rotated.
        protected override void ApplyAdjustment(string actionParameter, int diff)
        {
            _volume += (int)Math.Round(diff * 2.55d); // Increase or decrease the counter by the number of ticks.
            if (_volume - byte.MaxValue > 0)
                _volume = byte.MaxValue;

            if (_volume < 0)
                _volume = 0;

            SetVolume(ChannelName, (byte)_volume);

            AdjustmentValueChanged(); // Notify the Loupedeck service that the adjustment value has changed.
        }

        // This method is called when the reset command related to the adjustment is executed.
        protected override void RunCommand(string actionParameter)
        {
            //Mute can only be done on fader level, not channel level.
            if (_muteVolume == 0)
            {
                _muteVolume = _volume;
                SetVolume(ChannelName, 0);
            }
            else
            {
                SetVolume(ChannelName, (byte)_muteVolume);
                _muteVolume = 0;
            }
            
            AdjustmentValueChanged(); // Notify the Loupedeck service that the adjustment value has changed.
        }

        // Returns the adjustment value that is shown next to the dial.
        protected override string GetAdjustmentValue(string actionParameter)
            => Math.Round(_volume * 100d / 0xFF).ToString(CultureInfo.InvariantCulture);
    }

    public class MicVolumeAdjustment : VolumeAdjustment
    {
        protected override string ChannelName => "Mic";

        public MicVolumeAdjustment()
            : base("Mic", "Mic") { }

        protected override bool IsVolumeChangePatchEvent(Patch patch)
        {
            var match = Regex.Match(patch.Path, $@"/mixers/(?<serial>\w+)/levels/volumes/{ChannelName}");
            //var serial = match.Groups["serial"];
            return match.Success;
        }
    }

    public class LineInVolumeAdjustment : VolumeAdjustment
    {
        protected override string ChannelName => "LineIn";

        public LineInVolumeAdjustment()
            : base("Line In", "Line In") { }

        protected override bool IsVolumeChangePatchEvent(Patch patch)
        {
            var match = Regex.Match(patch.Path, $@"/mixers/(?<serial>\w+)/levels/volumes/{ChannelName}");
            return match.Success;
        }
    }

    public class ConsoleVolumeAdjustment : VolumeAdjustment
    {
        protected override string ChannelName => "Console";

        public ConsoleVolumeAdjustment()
            : base("Console", "Console") { }

        protected override bool IsVolumeChangePatchEvent(Patch patch)
        {
            var match = Regex.Match(patch.Path, $@"/mixers/(?<serial>\w+)/levels/volumes/{ChannelName}");
            return match.Success;
        }
    }

    public class SystemVolumeAdjustment : VolumeAdjustment
    {
        protected override string ChannelName => "System";

        public SystemVolumeAdjustment()
            : base("System", "System") { }

        protected override bool IsVolumeChangePatchEvent(Patch patch)
        {
            var match = Regex.Match(patch.Path, $@"/mixers/(?<serial>\w+)/levels/volumes/{ChannelName}");
            return match.Success;
        }
    }

    public class GameVolumeAdjustment : VolumeAdjustment
    {
        protected override string ChannelName => "Game";

        public GameVolumeAdjustment()
            : base("Game", "Game") { }

        protected override bool IsVolumeChangePatchEvent(Patch patch)
        {
            var match = Regex.Match(patch.Path, $@"/mixers/(?<serial>\w+)/levels/volumes/{ChannelName}");
            return match.Success;
        }
    }

    public class ChatVolumeAdjustment : VolumeAdjustment
    {
        protected override string ChannelName => "Chat";

        public ChatVolumeAdjustment()
            : base("Chat", "Chat") { }

        protected override bool IsVolumeChangePatchEvent(Patch patch)
        {
            var match = Regex.Match(patch.Path, $@"/mixers/(?<serial>\w+)/levels/volumes/{ChannelName}");
            return match.Success;
        }
    }

    public class SampleVolumeAdjustment : VolumeAdjustment
    {
        protected override string ChannelName => "Sample";

        public SampleVolumeAdjustment()
            : base("Sample", "Sample") { }

        protected override bool IsVolumeChangePatchEvent(Patch patch)
        {
            var match = Regex.Match(patch.Path, $@"/mixers/(?<serial>\w+)/levels/volumes/{ChannelName}");
            return match.Success;
        }
    }

    public class MusicVolumeAdjustment : VolumeAdjustment
    {
        protected override string ChannelName => "Music";

        public MusicVolumeAdjustment()
            : base("Music", "Music") { }

        protected override bool IsVolumeChangePatchEvent(Patch patch)
        {
            var match = Regex.Match(patch.Path, $@"/mixers/(?<serial>\w+)/levels/volumes/{ChannelName}");
            return match.Success;
        }
    }

    public class HeadphonesVolumeAdjustment : VolumeAdjustment
    {
        protected override string ChannelName => "Headphones";

        public HeadphonesVolumeAdjustment()
            : base("Headphones", "Headphones") { }

        protected override bool IsVolumeChangePatchEvent(Patch patch)
        {
            var match = Regex.Match(patch.Path, $@"/mixers/(?<serial>\w+)/levels/volumes/{ChannelName}");
            return match.Success;
        }
    }

    public class MicMonitorVolumeAdjustment : VolumeAdjustment
    {
        protected override string ChannelName => "MicMonitor";

        public MicMonitorVolumeAdjustment()
            : base("Mic Monitor", "Mic Monitor") { }

        protected override bool IsVolumeChangePatchEvent(Patch patch)
        {
            var match = Regex.Match(patch.Path, $@"/mixers/(?<serial>\w+)/levels/volumes/{ChannelName}");
            return match.Success;
        }
    }

    public class LineOutVolumeAdjustment : VolumeAdjustment
    {
        protected override string ChannelName => "LineOut";

        public LineOutVolumeAdjustment()
            : base("Line Out", "Line Out") { }

        protected override bool IsVolumeChangePatchEvent(Patch patch)
        {
            var match = Regex.Match(patch.Path, $@"/mixers/(?<serial>\w+)/levels/volumes/{ChannelName}");
            return match.Success;
        }
    }
}
