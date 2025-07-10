using CargoWise.Common;
using Enterprise.MasterFiles.Business.Testing;

[assembly: CountrySpecificTest(Enterprise.Core.Constants.CountryCodes.Singapore)]
[assembly: PreventAssemblyReferences(allowedReferencePartialPaths: [
	"Enterprise.Customs.SG.V4.GUI.Test",
	"Enterprise.Customs.SG.V4.GUI.Test.Winzor",
	"Enterprise.Customs.SG.Access.Business.Test",
	"ZClientUPE.Test"
])]
