using System.Reflection;
using CargoWise.Common;

[assembly: AssemblyTitle("MHUB")]
[assembly: AssemblyDescription("MHUB")]

[assembly: Enterprise.MasterFiles.Business.Testing.CountrySpecificTest(Enterprise.Core.Constants.CountryCodes.Singapore)]
[assembly: PreventAssemblyReferences(allowedReferencePartialPaths: [
	"Enterprise.Customs.SG.V4.Business.Test",
])]
