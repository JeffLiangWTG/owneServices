# Setup CW1

## Setup CW1 database

1.  In SSMS connect to your local machines SQL server. e.g. `SYDCO-WLKT-1`
2.  Right click Database -> Restore Database
3.  Select Source -> Device -> ... -> Add -> `C:\ProgramData\CargoWise edi\DAT\DBBackupCache\ALP.bak` -> OK -> OK
    *	Restore from DPR.bak when testing DPR, GP1 when testing GP1, STD.bak when testing STD.
4.  Give your database a unique name.
5.  OK
6.  Create a .bat script to run CW1 with your database. `start C:\dev\bin\CargoWiseOne SYDCO-WLKT-1 Alpha`
    1.  Change `SYDCO-WLKT-1` to the name of your local machine.
    2.  Change `Alpha` to the name of your new database.
    3.  Change the path to the correct release you are testing, `C:\dev\bin\CargoWiseOne` is used for alpha.
7.  Run the new script to run CW1 with the new database.
8.  Set the UI color for this CW1 setup, this helps to avoid confusing it with other running CW1 instances.
    1.  In CW1 -> Maintain -> System -> Registry -> System -> UI -> Color Theme
    2.  Tick Override Default.
    3.  Choose a unique color.
    4.  Save

## Connect CW1 instance to SBTS2

1.  Obtain CW1 license key
2.  In CW1 -> Help -> Register Product -> Enter license key
3.  In CW1 -> Registry -> eServices -> eHub -> eHub Gateway Server Address -> au2sp-shub-401.sand.wtg.zone
4.  In CW1 -> Registry -> eServices -> Enable eServices Verbose Logging -> Yes
5.  In CW1 -> Registry -> System -> Testing -> eHub Testing -> Yes
6.  In CW1 -> Service Tasks -> Process Controller -> $machineName.wtg.zone -> Install
7.  In CW1 ->  Service Tasks -> Process Controller -> $machineName.wtg.zone -> Start 
8.  If its not working check logs:
    1.  Gateway: C:\inetpub\wwwroot\eHubGateway\logs
    2.  CW1: Service Tasks -> Find -> EHI -> Log Files

## Connect CW1 instance to local VM

This is similar to connecting to SBTS2, but last time I did it, I had to manually create my eHubClient's.
This may have been fixed since then.
