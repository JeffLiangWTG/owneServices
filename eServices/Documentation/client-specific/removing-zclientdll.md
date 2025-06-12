# Removing ZClient DLLs

WiseTech used to develop per client customizations that run within CW1.
However they are now in maintenence mode and when a client no longer wants to pay for their extensions we get to delete it.
Less legacy code, YAY!

Each clients customizations live in a single DLL, named `ZClientXXX.dll` where XXX is the clients code, hence why they are reffered to as ZClient DLLs.
Note that Dave and Kirsten refer to it as Legacy SQL for some reason.

## Steps To remove

Here is an example [shelveset](http://tfs.wtg.zone:8080/tfs/CargoWise/_versionControl/shelveset?ss=WI00190831-r2%3BCORP%5CLucas.Kent) to remove ZClientLEP.

But I will still break down what to do below:
1.  In Source Control Explorer delete the folder containing the ZClient solution in `C:\dev\Enterprise\ClientExtensions`.
2.  Remove the relevant Solution tag from `C:/dev/Build.xml`, `C:/Enterprise/Product/Core/Builder/Generator/Testing/TestBuild.xml`
3.  Add the relevant `ZClientXXX` to `DeletedClientDLL` in `C:/dev/Enterprise/Architecture/Modules/ClientHook/Clients.cs`. This ensures that the clients CW1 will continue to run when the DLL disappears missing.
4.  Delete the relevant `ZClientXXX` from the folders `C:/dev/Common/Tools/CodeAnalysis/Baseline/Enterprise.CodeAnalysis.Rules` and `C:/dev/Common/Tools/CodeAnalysis/Baseline/SystemRulesSystem`.
5.  Before sending it off for review, run a full build of Dev on QuickGetLatest to make sure you didnt break anything.