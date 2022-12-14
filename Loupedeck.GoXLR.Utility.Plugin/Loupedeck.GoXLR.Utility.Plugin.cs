namespace Loupedeck.GoXLR.Utility.Plugin
{
    // This class contains the plugin-level logic of the Loupedeck plugin.

    public class UtilityPlugin : Loupedeck.Plugin
    {
        public GoXlrUtiltyClient Client { get; }

        // Gets a value indicating whether this is an Universal plugin or an Application plugin.
        public override bool UsesApplicationApiOnly => true;

        // Gets a value indicating whether this is an API-only plugin.
        public override bool HasNoApplication => true;

        public UtilityPlugin()
        {
            Client = new GoXlrUtiltyClient();
        }

        // This method is called when the plugin is loaded during the Loupedeck service start-up.
        public override void Load()
        {
            Client.Start();
            this.LoadPluginIcons();
        }

        // This method is called when the plugin is unloaded during the Loupedeck service shutdown.
        public override void Unload()
        {
            Client.Dispose();
        }

        private void LoadPluginIcons()
        {
            //var resources = this.Assembly.GetManifestResourceNames();
            this.Info.Icon16x16 = EmbeddedResources.ReadImage("Loupedeck.GoXLR.Utility.Plugin.metadata.Icon16x16.png");
            this.Info.Icon32x32 = EmbeddedResources.ReadImage("Loupedeck.GoXLR.Utility.Plugin.metadata.Icon32x32.png");
            this.Info.Icon48x48 = EmbeddedResources.ReadImage("Loupedeck.GoXLR.Utility.Plugin.metadata.Icon48x48.png");
            this.Info.Icon256x256 = EmbeddedResources.ReadImage("Loupedeck.GoXLR.Utility.Plugin.metadata.Icon256x256.png");
        }
    }
}
