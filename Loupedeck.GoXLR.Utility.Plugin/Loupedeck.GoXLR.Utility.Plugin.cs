namespace Loupedeck.GoXLR.Utility.Plugin
{
    // This class contains the plugin-level logic of the Loupedeck plugin.

    public class UtilityPlugin : Loupedeck.Plugin
    {
        public GoXlrUtilityClient Client { get; }

        // Gets a value indicating whether this is an Universal plugin or an Application plugin.
        public override bool UsesApplicationApiOnly => true;

        // Gets a value indicating whether this is an API-only plugin.
        public override bool HasNoApplication => true;

        public UtilityPlugin()
        {
            Client = new GoXlrUtilityClient();
        }

        // This method is called when the plugin is loaded during the Loupedeck service start-up.
        public override void Load()
        {
            Client.Start();
            LoadPluginIcons();
        }

        // This method is called when the plugin is unloaded during the Loupedeck service shutdown.
        public override void Unload()
        {
            Client.Dispose();
        }

        private void LoadPluginIcons()
        {
            //var resources = this.Assembly.GetManifestResourceNames();
            Info.Icon16x16 = EmbeddedResources.ReadImage("Loupedeck.GoXLR.Utility.Plugin.metadata.Icon16x16.png");
            Info.Icon32x32 = EmbeddedResources.ReadImage("Loupedeck.GoXLR.Utility.Plugin.metadata.Icon32x32.png");
            Info.Icon48x48 = EmbeddedResources.ReadImage("Loupedeck.GoXLR.Utility.Plugin.metadata.Icon48x48.png");
            Info.Icon256x256 = EmbeddedResources.ReadImage("Loupedeck.GoXLR.Utility.Plugin.metadata.Icon256x256.png");
        }
    }
}
