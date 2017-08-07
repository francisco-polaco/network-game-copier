@echo OFF
echo Stopping old service version...
net stop "Network Game Copier Updater"
echo Uninstalling old service version...
sc delete "Network Game Copier Updater"

echo Installing service...
rem DO NOT remove the space after "binpath="!
sc create "Network Game Copier Updater" binpath= "UpdateService.exe" 
echo Starting server complete
pause