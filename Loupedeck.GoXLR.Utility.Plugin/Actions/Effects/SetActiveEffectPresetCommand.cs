using System;
using System.Text.RegularExpressions;
using Loupedeck.GoXLR.Utility.Plugin.Enums;

namespace Loupedeck.GoXLR.Utility.Plugin.Actions.Effects
{
    public class SetActiveEffectPresetCommand : PluginDynamicCommand
    {
        private GoXLRUtilityPlugin _plugin;

        private GoXlrUtilityClient Client => _plugin?.Client;

        private EffectBankPresets _activeEffectBankPresets;

        public SetActiveEffectPresetCommand()
        {
            var values = Enum.GetNames(typeof(EffectBankPresets));
            foreach (var value in values)
            {
                AddParameter(value, $"Set bank {value}", "Effects");
            }
        }

        protected override bool OnLoad()
        {
            _plugin = (GoXLRUtilityPlugin)Plugin;
            Client.PatchEvent += IsEffectBankPresetsChangePatchEvent;

            return true;
        }

        protected override bool OnUnload()
        {
            Client.PatchEvent -= IsEffectBankPresetsChangePatchEvent;

            return true;
        }

        private void IsEffectBankPresetsChangePatchEvent(object sender, Patch patch)
        {
            var match = Regex.Match(patch.Path, @"/mixers/(?<serial>\w+)/effects/active_preset");
            if (!match.Success)
                return;

            _activeEffectBankPresets = patch.Value.ToObject<EffectBankPresets>();
            ActionImageChanged();
        }

        protected override void RunCommand(string actionParameter)
        {
            if (actionParameter is null)
                return;

            Client.SendCommand("SetActiveEffectPreset", actionParameter);
        }

        protected override BitmapImage GetCommandImage(string actionParameter, PluginImageSize imageSize)
        {
            if (actionParameter is null)
                return null;

            if (!Enum.TryParse<EffectBankPresets>(actionParameter, out var effectBankPresets))
                return null;

            var displayName = base.GetCommandDisplayName(actionParameter, imageSize);

            using (var bitmapBuilder = new BitmapBuilder(imageSize))
            {
                var color = _activeEffectBankPresets == effectBankPresets
                    ? new BitmapColor(0x00, 0x50, 0x00)
                    : BitmapColor.Black;

                bitmapBuilder.Clear(color);
                bitmapBuilder.DrawText(displayName);

                return bitmapBuilder.ToImage();
            }
        }
    }
}
