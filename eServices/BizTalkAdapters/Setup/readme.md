# CargoWise.eHub.BizTalkAdapters.Setup

Builds setup MSI package for deploying custom BizTalk adapters.  

## To build from local source:
1.	Install Microsoft Visual Studio Installer Projects:
	1.  Download and run: https://marketplace.visualstudio.com/items?itemName=VisualStudioClient.MicrosoftVisualStudio2017InstallerProjects
	2.  When prompted select Visual Studio 2019
2.	QGL under `$/eServices/BizTalkAdapters`
3.  Open `$/eServices/BizTalkAdapters/CargoWise.eHub.BizTalkAdapters.sln` in Visual Studio 2019.
4.  Rebuild solution
5.  Open `$/eServices/BizTalkAdapters/CargoWise.eHub.BizTalkAdapters.Setup.sln` in Visual Studio 2019.
6.  Rebuild solution

It is possible for Visual Studio to request a Visual Studio 2010 CD-ROM.
This is a strange bug but there is a workaround
https://stackoverflow.com/questions/40867371/building-visual-studio-setup-deploy-project-asks-to-install-2010-shell-integrate
In cmd as admin run the following
```cmd
cd C:\Program Files (x86)\Common Files\microsoft shared\MSI Tools
regsvr32.exe /u mergemod.dll"
regsvr32.exe mergemod.dll"
```

## To build from DAT:
1.  Copy assemblies from DAT build of [\$/eServices/BizTalkAdapters](http://crikey.wtg.zone/?branch=$/eServices/BizTalkAdapters) to `..\bin`
2.  Build *$/eServices/BizTalkAdapters/Setup/CargoWise.eHub.BizTalkAdapters.Setup.vdproj* in Visual Studio

## To build for deployment:
1.  Copy assemblies from latest check-in DAT build of [\$/eServices/BizTalkAdapters](http://crikey.wtg.zone/?branch=$/eServices/BizTalkAdapters) to `..\bin`
2.  Open *$/eServices/BizTalkAdapters/CargoWise.eHub.BizTalkAdapters.Setup.sln* in Visual Studio
3.  Checkout *$/eServices/BizTalkAdapters/Setup/CargoWise.eHub.BizTalkAdapters.Setup.vdproj* for edit
4.  Select *CargoWise.eHub.BizTalkAdapters.Setup*
5.  In properties window (open with `F4`) increment `Version` and when prompted select `Yes` to change the ProductCode
6.  Build *$/eServices/BizTalkAdapters/Setup/CargoWise.eHub.BizTalkAdapters.Setup.vdproj*
7.  Copy build output file `..\bin\CargoWise.eHub.BizTalkAdapters.Setup.msi` to target machine and execute

## To add a new adapter to the installer:
1.  Right-click *CargoWise.eHub.BizTalkAdapters.Setup* project and select `View -> File System`
2.  Right-click *Application Folder* and select `Add -> Assembly...`
3.  Browse to the `..\bin` folder and select the assemblies to add then click OK
4.  Review the assemblies list to see if any extra assemblies from dependencies were included. If so select them to view their properties and change `Exclude` to `True`
5.  Right-click *Application Folder* and select `Add -> File...`
6.  Browse to the `..\bin` folder and select the `.pdb` debug symbols for the new assemblies
7.  Generate `.reg` file for adapter by either:
    * exporting the registry settings for an existing adapter to a file and manually editing it; or
    * using the Adapter Registry Wizard at [C:\Program Files (x86)\Microsoft BizTalk Server 2013 R2\SDK\Utilities\AdapterRegistryWizard\AdapterRegistryWizard.exe](file://C:/Program%20Files%20(x86)/Microsoft%20BizTalk%20Server%202013%20R2/SDK/Utilities/AdapterRegistryWizard/AdapterRegistryWizard.exe)
        * After generating the file with the wizard you will need to edit it manually to:
          * remove comment lines (those starting with a quote `'`); and
          * pad the "Constraints" dword value with leading zeroes to make it 8 digits long
8.  Right-click *CargoWise.eHub.BizTalkAdapters.Setup* project and select `View -> Registry`
9.  Right-click *Registry on Target Machine* and select `Import...`  

## To register an adapter in BizTalk:
1.  After successfully running installer, open *BizTalk Server Administration Console*
2.  Expand *BizTalk Group* then *Platform Settings*
3.  Right-click *Adapters* and select `New -> Adapter..`
4.  Select adapter from *Adapter* drop-down
    * If your adapter does not appear in the drop down then it is likely that the registry entries are incorrect
5.  Enter the adapter name (use same name as it appears in the dropdown) and click OK
6.  Expand adapters and select your new adapter
7.  Add or remove send and receive handlers as required
8.  Our custom adapters that use Handler Configurations (currently just the ones based on CargoWise.eHub.BizTalkAdapters.Transferrer.Adapter framework) have an issue where the handler's configuration

## To Do
* Fix handler setup for new Transferrer adapters so that default configuration is automatically set on install
* Configure for DAT build and packaging