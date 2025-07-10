rem
rem schemas innfoerselsavgift.xsd and utfoerselsavgift.xsd
rem are identical, and saved as avgiftliste.xsd here.
rem
rem schemas tollavgiftssats.xsd and raavaretollavgiftssats.xsd
rem are identical, and saved as tollavgiftssats.xsd.
rem

xsd.exe avgiftliste.xsd /c /f /namespace:CargoWise.RefDbRepo.NOReferenceData.Services.Tariff.Avgiftliste
xsd.exe tollavgiftssats.xsd /c /f /namespace:CargoWise.RefDbRepo.NOReferenceData.Services.Tariff.Tollsats
xsd.exe varenummer.xsd /c /f /namespace:CargoWise.RefDbRepo.NOReferenceData.Services.Tariff.Varenummer
xsd.exe customstariffstructure.xsd /c /f /namespace:CargoWise.RefDbRepo.NOReferenceData.Services.Tariff.Customstariffstructure
xsd.exe tolltariffstruktur.xsd /c /f /namespace:CargoWise.RefDbRepo.NOReferenceData.Services.Tariff.Tolltariffstruktur
