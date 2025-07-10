if (!$args[0]) {
    Write-Host "Please provide a path to the project directory"
    return;
}

$CurrentDirectory = Get-Location
$BuildDirectory = $args[0]

if (Test-Path $CurrentDirectory\wwwroot\CargoWise.Winzor.combined.css) {
    Remove-Item $CurrentDirectory\wwwroot\CargoWise.Winzor.combined.css
}

if ((Test-Path $BuildDirectory\wwwroot\_cssBuild) -eq $false) {
    New-Item -Path "$BuildDirectory\wwwroot" -Name "_cssBuild" -ItemType "directory"
}

Copy-Item -Path $BuildDirectory\wwwroot\_content\Aga.Controls\css\_gen\default.css -Destination $BuildDirectory\wwwroot\_cssBuild\Aga.Controls.scss
Copy-Item -Path $BuildDirectory\wwwroot\_content\WinzorFramework\css\_gen\default.css -Destination $BuildDirectory\wwwroot\_cssBuild\WinzorFramework.scss
Copy-Item -Path $BuildDirectory\wwwroot\_content\CargoWise.GUI.TileBar\css\_gen\cargowise.gui.tilebar.css -Destination $BuildDirectory\wwwroot\_cssBuild\CargoWise.GUI.TileBar.scss
Copy-Item -Path $BuildDirectory\wwwroot\_content\Enterprise.DocumentEngine.GUI\css\_gen\default.css -Destination $BuildDirectory\wwwroot\_cssBuild\Enterprise.DocumentEngine.GUI.scss
Copy-Item -Path $BuildDirectory\wwwroot\_content\CargoWise.Windows.UI\css\_gen\default.css -Destination $BuildDirectory\wwwroot\_cssBuild\CargoWise.Windows.UI.scss
Copy-Item -Path $BuildDirectory\wwwroot\_content\WTG.Z.Blazor.Diagrams\style.min.css -Destination $BuildDirectory\wwwroot\_cssBuild\WTG.Z.Blazor.Diagrams.style.scss
Copy-Item -Path $BuildDirectory\wwwroot\_content\WTG.Z.Blazor.Diagrams\default.styles.min.css -Destination $BuildDirectory\wwwroot\_cssBuild\WTG.Z.Blazor.Diagrams.default.scss
Copy-Item -Path $BuildDirectory\wwwroot\_content\CargoWise.NetworkVisualisation.GUI\default.css -Destination $BuildDirectory\wwwroot\_cssBuild\CargoWise.NetworkVisualisation.GUI.scss
Copy-Item -Path $BuildDirectory\wwwroot\_content\Enterprise.ZArchitecture.GUI\css\_gen\default.css -Destination $BuildDirectory\wwwroot\_cssBuild\Enterprise.ZArchitecture.GUI.scss
Copy-Item -Path $BuildDirectory\wwwroot\_content\CargoWiseNext.Blazor.Components\css\_gen\cargowise.blazor.components.css -Destination $BuildDirectory\wwwroot\_cssBuild\CargoWise.Blazor.Components.scss
Copy-Item -Path $BuildDirectory\wwwroot\_content\CargoWise.Main.Navigation\css\_gen\cargowise.main.navigation.css -Destination $BuildDirectory\wwwroot\_cssBuild\CargoWise.Main.Navigation.scss
$AppServerCss = Get-Content $BuildDirectory\wwwroot\CargoWise.Winzor.AppServer.styles.css

New-Item -Path $BuildDirectory\wwwroot\_cssBuild -Name "default.scss" -ItemType "file" -Value ("
@import 'Aga.Controls';
@import 'CargoWise.GUI.TileBar';
@import 'CargoWise.Windows.UI';
@import 'WinzorFramework';
@import 'Enterprise.DocumentEngine.GUI';
@import 'Enterprise.ZArchitecture.GUI';
@import 'WTG.Z.Blazor.Diagrams.style';
@import 'WTG.Z.Blazor.Diagrams.default';
@import 'CargoWise.NetworkVisualisation.GUI';
@import 'CargoWise.Blazor.Components';
@import 'CargoWise.Main.Navigation';" + $AppServerCss)

dotnet tool run webcompiler -- -r $BuildDirectory\wwwroot\_cssBuild\default.scss -c $CurrentDirectory\excubo-webcompiler.json -o $BuildDirectory\wwwroot\_cssBuild -m enable
Get-ChildItem $BuildDirectory\wwwroot\_cssBuild

if ((Test-Path $CurrentDirectory\wwwroot\css\_gen) -eq $false) {
    New-Item -Path "$CurrentDirectory\wwwroot\css" -Name "_gen" -ItemType "directory"
}

if (Test-Path $BuildDirectory\wwwroot\_cssBuild\default.css) {
    Copy-Item $BuildDirectory\wwwroot\_cssBuild\default.css $CurrentDirectory\wwwroot\CargoWise.Winzor.combined.css
    Copy-Item $BuildDirectory\wwwroot\_cssBuild\default.css $BuildDirectory\wwwroot\CargoWise.Winzor.combined.css
}
elseif (Test-Path $BuildDirectory\wwwroot\_cssBuild\default.min.css) {
    Copy-Item $BuildDirectory\wwwroot\_cssBuild\default.min.css $CurrentDirectory\wwwroot\CargoWise.Winzor.combined.css
    Copy-Item $BuildDirectory\wwwroot\_cssBuild\default.min.css $BuildDirectory\wwwroot\CargoWise.Winzor.combined.css
}

Remove-Item $BuildDirectory\wwwroot\_cssBuild -Recurse
