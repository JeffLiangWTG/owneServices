@echo off
pushd %~dp0

echo ==== signing WinForms.DataVisualization.dll ====
.\packages\Brutal.Dev.StrongNameSigner\build\StrongNameSigner.Console.exe -in .\packages\WinForms.DataVisualization\lib\net8.0-windows7.0\
if %errorlevel% neq 0 goto :end

echo ==== signing WeakEventListener.dll ====
.\packages\Brutal.Dev.StrongNameSigner\build\StrongNameSigner.Console.exe -a .\packages\SimpleWeakEventListener\lib\netstandard2.0\WeakEventListener.dll -out .\packages\SimpleWeakEventListener\lib\netstandard2.0
if %errorlevel% neq 0 goto :end

echo ==== signing ConvertApi.dll ====
.\packages\Brutal.Dev.StrongNameSigner\build\StrongNameSigner.Console.exe -a .\packages\ConvertApi\lib\netstandard2.0\ConvertApi.dll -out .\packages\ConvertApi\lib\netstandard2.0
if %errorlevel% neq 0 goto :end

echo ==== signing (Selenium.)WebDriver.dll ====
.\packages\Brutal.Dev.StrongNameSigner\build\StrongNameSigner.Console.exe -a .\packages\Selenium.WebDriver\lib\netstandard2.0\WebDriver.dll -out .\packages\Selenium.WebDriver\lib\netstandard2.0
if %errorlevel% neq 0 goto :end

rem add signing another dll's here if necessary

:end
popd
echo ==== signDeps.cmd end. errorlevel = %errorlevel% ====
exit /b %errorlevel%
