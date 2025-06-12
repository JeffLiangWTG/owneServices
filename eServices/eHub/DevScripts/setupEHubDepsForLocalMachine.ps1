$ErrorActionPreference = "Stop"
cd ~

Write-Host "Copying installers"
Copy-Item \\Sydco-svid-1\dat\Installers\installers\biztalk -Destination biztalk_temp_installers -recurse
cd biztalk_temp_installers

Write-Host "Extracting biztalk iso"
7z x .\en_biztalk_server_2013_r2_developer_edition_x86_and_x64_dvd_4445857.iso -obiztalk_iso
mv BtsRedistW2K12EN64.cab "biztalk_iso/BizTalk Server"
cd "biztalk_iso/BizTalk Server"

Write-Host ""
Write-Host "Running biztalk installer"
./Setup.exe /COMPANYNAME WTG /USERNAME DAT /ADDLOCAL "BizTalk,WMI,SDK,Development,VSTools,BizTalkExtensions,BizTalkExplorer,Designer,OrchestrationDesigner,PipelineDesigner,XMLTools,AdapterImportWizard,DeploymentWizard,TrackingProfileEditor,Runtime,Engine,WCFAdapter" /norestart /CABPATH "$PWD/BtsRedistW2K12EN64.cab"

cd ../..
Write-Host "When setup process is complete:"
Write-Host "*   you can delete: $PWD"
Write-Host "*   You MUST restart your machine"