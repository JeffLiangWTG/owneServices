using System.Reflection;
using System.Runtime.CompilerServices;
using CargoWise.Common;
using Enterprise.Customs.EU.Business;
using Enterprise.ZArchitecture.Business.UniversalCopy;

[assembly: AssemblyTitle("PL Customs Business")]
[assembly: AssemblyDescription("PL Customs Business")]
[assembly: UniversalCopyAddInfoPropertyDefinition(typeof(AutoEUAddInfo.Schema), Enterprise.Core.Constants.CountryCodes.Poland)]
#if DEBUG
[assembly: InternalsVisibleTo("Enterprise.Customs.PL.Business.Test, PublicKey=" + CommonAssemblyInfo.PublicKey)]
[assembly: InternalsVisibleTo("Enterprise.Customs.PL.ExitControl.Business.Test, PublicKey=" + CommonAssemblyInfo.PublicKey)]
[assembly: InternalsVisibleTo("Enterprise.Customs.PL.NCTS.Business.Test, PublicKey=" + CommonAssemblyInfo.PublicKey)]
[assembly: InternalsVisibleTo("Enterprise.Customs.PL.ExitControl.Business.Test, PublicKey=" + CommonAssemblyInfo.PublicKey)]
#endif
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2, PublicKey=0024000004800000940000000602000000240000525341310004000001000100c547cac37abd99c8db225ef2f6c8a3602f3b3606cc9891605d02baa56104f4cfc0734aa39b93bf7852f7d9266654753cc297e7d2edfe0bac1cdcf9f717241550e0a7b191195b7667bb4f64bcb8e2121380fd1d9d46ad2d92d2d15605093924cceaf74c4861eff62abf69b9291ed0a340e113be11e6a7d3113e92484cf7045cc7")]

[assembly: PreventAssemblyReferences(allowedReferencePartialPaths: [
	"Enterprise.Customs.PL.Business.XmlSerializers",
	"Enterprise.Customs.PL.GUI",
	"Enterprise.Customs.PL.Module",
	"Enterprise.Customs.PL.ServiceTasks",
	"Enterprise.Customs.PL.Business.Test",
	"Enterprise.Customs.PL.GUI.Test",
	"Enterprise.Customs.PL.Module.Test",
	"Enterprise.Customs.PL.ServiceTasks.Test",
	"Enterprise.Customs.PL.NCTS.Business",
	"Enterprise.Customs.PL.NCTS.GUI",
	"Enterprise.Customs.PL.NCTS.Business.Test",
	"Enterprise.Customs.PL.NCTS.GUI.Test",
	"Enterprise.Customs.PL.ExitControl.Business",
	"Enterprise.Customs.PL.ExitControl.Business.Test",
])]
