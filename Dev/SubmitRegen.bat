@echo off
echo This will queue a shelf for the regen. Please check that you only have regen related files checked out. Ctrl-C to cancel.
pause
bin\GenerateDbUpgraderResources -AutoRegen -CheckIn
echo Regen queued