*****************************************************************************************************************
* dotnet-tools.json is the manifest file used to restore dotnet tools into the local build environment.
*
* You will need to update dotnet-tools.json if you want to update the tools versions or install additional tools.
*
* If you wish to add an additional tool, then you'll need to add an additional install command for it below.
*
* Run the following commands from the eServices\eHub directory if you need to update dotnet-tools.json:
*
*****************************************************************************************************************

dotnet new tool-manifest --force
dotnet tool install eServices.BuildTools.DatTool
