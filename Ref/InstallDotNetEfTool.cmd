ECHO Installing dotnet-ef tool...

dotnet tool install --global dotnet-ef --version 7.0.5

if %errorlevel% neq 0 (
    echo ==== dotnet-ef tool failed to install ====
) else (
    echo ==== dotnet-ef tool is already installed ====
)

EXIT /B 0
