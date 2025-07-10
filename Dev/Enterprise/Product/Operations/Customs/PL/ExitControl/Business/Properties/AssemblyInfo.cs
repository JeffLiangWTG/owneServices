using System.Reflection;
using System.Runtime.CompilerServices;
using CargoWise.Common;

[assembly: AssemblyTitle("PL.ExitControl Customs Business")]
[assembly: AssemblyDescription("PL.ExitControl Customs Business")]
#if DEBUG
[assembly: InternalsVisibleTo("Enterprise.Customs.PL.ExitControl.Business.Test, PublicKey=" + CommonAssemblyInfo.PublicKey)]
#endif
[assembly: PreventAssemblyReferences(allowedReferencePartialPaths: [
	"Enterprise.Customs.PL.ExitControl.Business.Test",
	"Enterprise.Customs.PL.ExitControl.GUI",
	"Enterprise.Customs.PL.ExitControl.Module",
	"Enterprise.Customs.PL.ExitControl.Module.Test"])]

#if DEBUG
[assembly: InternalsVisibleTo("Enterprise.Customs.PL.ExitControl.Business.Test, PublicKey=" + CommonAssemblyInfo.PublicKey)]
#endif
