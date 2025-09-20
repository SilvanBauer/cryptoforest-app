# Export config page
This page lets you select the levels to export and enter a password for the encrypted config. To select a level you can check a checkbox. As all levels are connected with their parent the parents will automatically also be selected. It is not possible to export a level without their parent level with exception of the base level which does not have a parent.

To select all levels the base level can be checked or unchecked to deselect all levels. As the parent levels are automatically selected it is recommended to select the child levels first. **It should also be noted that the UI does not support only exporting the base level if it has sublevels.**

Before being able to export the config you will also need to enter a password. The password is needed to protect the config as it contains the levels and their config keys. The password is hashed with SHA256.

With the export button you can export a config containing the selected levels. This config then can be used to open the levels of this CryptoForest again. All levels that were not selected will not be available anymore after opening with the exported config. **It is highly encuraged that one config is exported that contains all levels as data could be lost otherwise.**

Once the config has been exported you will automatically be redirected to the [Main page](Main.md). You can also get back to the [Main page](Main.md) at any time by clicking the back button. All entered data will be lost if the back button is used.
