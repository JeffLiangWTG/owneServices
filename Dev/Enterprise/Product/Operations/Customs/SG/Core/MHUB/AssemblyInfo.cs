using System.Reflection;
using System.Runtime.CompilerServices;
using CargoWise.Common;

[assembly: AssemblyTitle("MHUB")]
[assembly: AssemblyDescription("MHUB")]
#if DEBUG
[assembly: InternalsVisibleTo("Enterprise.Customs.SG.MHUB.Test, PublicKey=" + CommonAssemblyInfo.PublicKey)]
#endif
[assembly: PreventAssemblyReferences(allowedReferencePartialPaths: [
	"Enterprise.Customs.SG.MHUB.Test",
	"Enterprise.Customs.SG.V4.Business",
])]
