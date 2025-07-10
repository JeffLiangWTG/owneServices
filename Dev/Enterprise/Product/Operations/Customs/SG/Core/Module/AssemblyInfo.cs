using System.Reflection;
using System.Runtime.CompilerServices;
using CargoWise.Common;

[assembly: AssemblyTitle("SG Customs Module")]
[assembly: AssemblyDescription("SG Customs Module")]
#if DEBUG
[assembly: InternalsVisibleTo("Enterprise.Customs.SG.V4.Module.Test, PublicKey=" + CommonAssemblyInfo.PublicKey)]
#endif
[assembly: PreventAssemblyReferences(allowedReferencePartialPaths: [
	"Enterprise.Customs.SG.V4.Module.Test",
	"Enterprise.Customs.SG.V4.Module.Test.Winzor",
	"ZClientCCP"
])]
