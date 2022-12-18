using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Loupedeck.GoXLR.Utility.Plugin.Enums;

namespace Loupedeck.GoXLR.Utility.Plugin.Actions
{
    public abstract class FaderSwitchFolder : PluginDynamicFolder
    {
        private GoXLRUtilityPlugin _plugin;
        private GoXlrUtilityClient Client => _plugin?.Client;
        
        protected ChannelName _channelName;
        private ChannelName _previousChannelName;
        
        protected abstract FaderName FaderName { get; }

        public override bool Load()
        {
            _plugin = (GoXLRUtilityPlugin)Plugin;
            Client.PatchEvent += IsFaderChangedPatchEvent;
            return base.Load();
        }

        public override bool Unload()
        {
            Client.PatchEvent -= IsFaderChangedPatchEvent;
            return base.Unload();
        }
        
        private void IsFaderChangedPatchEvent(object sender, Patch patch)
        {
            if (!Regex.IsMatch(patch.Path, $@"/mixers/(?<serial>\w+)/fader_status/{FaderName}/channel"))
                return;

            _previousChannelName = _channelName; 
            _channelName = patch.Value.ToObject<ChannelName>();
            
            CommandImageChanged($"channel|{FaderName}|{_previousChannelName}");
            CommandImageChanged($"channel|{FaderName}|{_channelName}");
        }

        public override IEnumerable<string> GetButtonPressActionNames(DeviceType deviceType)
        {
            return new List<string>
            {
                CreateCommandName($"channel|{FaderName}|{ChannelName.Mic}"),
                CreateCommandName($"channel|{FaderName}|{ChannelName.LineIn}"),
                CreateCommandName($"channel|{FaderName}|{ChannelName.Console}"),
                CreateCommandName($"channel|{FaderName}|{ChannelName.System}"),
                CreateCommandName($"channel|{FaderName}|{ChannelName.Game}"),
                CreateCommandName($"channel|{FaderName}|{ChannelName.Chat}"),
                CreateCommandName($"channel|{FaderName}|{ChannelName.Sample}"),
                CreateCommandName($"channel|{FaderName}|{ChannelName.Music}"),
                CreateCommandName($"channel|{FaderName}|{ChannelName.Headphones}"),
                CreateCommandName($"channel|{FaderName}|{ChannelName.MicMonitor}"),
                CreateCommandName($"channel|{FaderName}|{ChannelName.LineOut}")
            };
        }

        //TODO Should set the Folder Display name but it wont refresh after change, can't find the Methode to Invoke a change
        //It will only be used for initialization
        public override string GetButtonDisplayName(PluginImageSize imageSize)
        {
            return _channelName.ToString();
        }

        //TODO This will always trigger but without the new name
        public override BitmapImage GetButtonImage(PluginImageSize imageSize)
        {
            return base.GetButtonImage(imageSize);
        }

        public override string GetCommandDisplayName(string actionParameter, PluginImageSize imageSize)
        {
            var parameter = actionParameter.Split("|");
            return base.GetCommandDisplayName(parameter[2], imageSize);
        }

        public override void RunCommand(string actionParameter)
        {
            if (actionParameter is null)
                return;

            var parameter = actionParameter.Split("|");

            var command = new
            {
                SetFader = new object[]
                {
                    parameter[1],
                    parameter[2]
                }
            };

            if (!Enum.TryParse(parameter[2], out ChannelName channelName))
                return;

            Client.SendCommand(command);
            
            _previousChannelName = _channelName;
            _channelName = channelName;
            
            CommandImageChanged($"channel|{FaderName}|{_previousChannelName}");
            CommandImageChanged($"channel|{FaderName}|{_channelName}");
            Close();
        }
    }

    public class FaderA_ChannelChange : FaderSwitchFolder
    {
        protected override FaderName FaderName => FaderName.A;

        public FaderA_ChannelChange()
        {
            DisplayName = "Fader A";
            GroupName = "Fader Switch Folder";
            Description = "IMPORTANT: Switching the Fader will reset its mute state, and for the Full Device stay as Level 0";
        }
    }
    
    public class FaderB_ChannelChange : FaderSwitchFolder
    {
        protected override FaderName FaderName => FaderName.B;

        public FaderB_ChannelChange()
        {
            DisplayName = "Fader B";
            GroupName = "Fader Switch Folder";
            Description = "IMPORTANT: Switching the Fader will reset its mute state, and for the Full Device stay as Level 0";
        }
    }
    
    public class FaderC_ChannelChange : FaderSwitchFolder
    {
        protected override FaderName FaderName => FaderName.C;

        public FaderC_ChannelChange()
        {
            DisplayName = "Fader C";
            GroupName = "Fader Switch Folder";
            Description = "IMPORTANT: Switching the Fader will reset its mute state, and for the Full Device stay as Level 0";
        }
    }
    
    public class FaderD_ChannelChange : FaderSwitchFolder
    {
        protected override FaderName FaderName => FaderName.D;

        public FaderD_ChannelChange()
        {
            DisplayName = "Fader D";
            GroupName = "Fader Switch Folder";
            Description = "IMPORTANT: Switching the Fader will reset its mute state, and for the Full Device stay as Level 0";
        }
    }
}
