using System.Reflection;
using System.Runtime.CompilerServices;
using CargoWise.Common;

[assembly: AssemblyTitle("SG Customs GUI")]
[assembly: AssemblyDescription("SG Customs GUI")]
#if DEBUG
[assembly: InternalsVisibleTo("Enterprise.Customs.SG.V4.GUI.Test, PublicKey=" + CommonAssemblyInfo.PublicKey)]
#endif
[assembly: PreventAssemblyReferences(allowedReferencePartialPaths: [
	"Enterprise.Customs.SG.V4.GUI.Test",
	"Enterprise.Customs.SG.V4.GUI.Test.Winzor",
	"Enterprise.Customs.SG.V4.Module",
	"Enterprise.Customs.SG.V4.Module.Winzor",
	"Enterprise.Customs.SG.Access.GUI",
	"Enterprise.Customs.SG.Access.GUI.Test"
])]
