rem
rem schemas innfoerselsreferanse.xsd and utfoerselsreferanse.xsd
rem are identical, and only one instance is saved here.
rem

xsd.exe referanse.xsd /c /f /namespace:CargoWise.RefDbRepo.NOReferenceData.Services.ReferenceCodes
xsd.exe feilmelding.xsd /c /f /namespace:CargoWise.RefDbRepo.NOReferenceData.Services.ErrorCodes
