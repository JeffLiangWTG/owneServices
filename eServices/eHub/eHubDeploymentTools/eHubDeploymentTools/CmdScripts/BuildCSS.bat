cd "[[BuildContainerTrackingSystem:DirectorySelector:ContainerSubscriptionService]]"
"[[BuildContainerTrackingSystem:FileSelector:MsBuildExe]]" .\Shared\PersistentQueue\src\PersistentQueue.sln /t:Build /p:Configuration="[[BuildContainerTrackingSystem:TextSelector:Configuration]]"
"[[BuildContainerTrackingSystem:FileSelector:MsBuildExe]]" .\Shared\ServiceTaskHost\src\ServiceTaskHost.sln /t:Build /p:Configuration="[[BuildContainerTrackingSystem:TextSelector:Configuration]]"
"[[BuildContainerTrackingSystem:FileSelector:MsBuildExe]]" .\CTS\ContainerSubscriptionService\src\ContainerSubscriptionService.sln /t:Build /p:Configuration="[[BuildContainerTrackingSystem:TextSelector:Configuration]]"
"[[BuildContainerTrackingSystem:FileSelector:MsBuildExe]]" .\CTS\ContainerSubscriptionService\build\CSSDeployer\CSSDeployer.sln /t:Build /p:Configuration="[[BuildContainerTrackingSystem:TextSelector:Configuration]]"
"[[BuildContainerTrackingSystem:FileSelector:MsBuildExe]]" .\CTS\CSSPortal\CSSPortal.sln /t:Build /p:Configuration="[[BuildContainerTrackingSystem:TextSelector:Configuration]]"
pause