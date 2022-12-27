using System.Text.RegularExpressions;

namespace Loupedeck.GoXLR.Utility.Plugin.Actions.Effects
{
    public abstract class EffectsToggleCommand : PluginDynamicCommand
    {
        private GoXLRUtilityPlugin _plugin;

        private GoXlrUtilityClient Client => _plugin?.Client;

        private bool _isEnabled;

        protected abstract string EffectPath { get; }

        protected abstract string CommandName { get; }

        protected EffectsToggleCommand(string displayName, string effectName)
            : base(displayName, $"Turns {effectName} effect on or off", "Effects")
        {
            //
        }

        protected override bool OnLoad()
        {
            _plugin = (GoXLRUtilityPlugin)Plugin;
            Client.PatchEvent += IsEffectEnabledChangePatchEvent;

            return true;
        }

        protected override bool OnUnload()
        {
            Client.PatchEvent -= IsEffectEnabledChangePatchEvent;

            return true;
        }

        private void IsEffectEnabledChangePatchEvent(object sender, Patch patch)
        {
            var match = Regex.Match(patch.Path, $@"/mixers/(?<serial>\w+)/effects/{EffectPath}");
            if (!match.Success)
                return;

            _isEnabled = patch.Value.ToObject<bool>();
            ActionImageChanged();
        }

        protected override void RunCommand(string actionParameter)
        {
            Client.SendCommand(CommandName, !_isEnabled);
        }
    }

    public class MegaphoneEffectCommand : EffectsToggleCommand
    {
        protected override string EffectPath => "current/megaphone/is_enabled";

        protected override string CommandName => "SetMegaphoneEnabled";

        public MegaphoneEffectCommand()
            : base("Megaphone", "megaphone")
        {
            //
        }
    }

    public class RobotEffectCommand : EffectsToggleCommand
    {
        protected override string EffectPath => "current/robot/is_enabled";

        protected override string CommandName => "SetRobotEnabled";

        public RobotEffectCommand()
            : base("Robot", "robot")
        {
            //
        }
    }

    public class HardTuneEffectCommand : EffectsToggleCommand
    {
        protected override string EffectPath => "current/hard_tune/is_enabled";

        protected override string CommandName => "SetHardTuneEnabled";

        public HardTuneEffectCommand()
            : base("Hardtune", "hard_tune")
        {
            //
        }
    }
    
    public class FXEffectCommand : EffectsToggleCommand
    {
        protected override string EffectPath => "is_enabled";

        protected override string CommandName => "SetFXEnabled";

        public FXEffectCommand()
            : base("FX", "effects")
        {
            //
        }
    }
}
