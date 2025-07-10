using System.Reflection;
using System.Runtime.CompilerServices;
using CargoWise.Common;
using Enterprise.Customs.SG.V4.Business;
using Enterprise.ZArchitecture.Business.UniversalCopy;

[assembly: AssemblyTitle("SG Customs Business")]
[assembly: AssemblyDescription("SG Customs Business")]
[assembly: UniversalCopyAddInfoPropertyDefinition(typeof(AutoSGAddInfo.Schema), Enterprise.Core.Constants.CountryCodes.Singapore)]
#if DEBUG
[assembly: InternalsVisibleTo("Enterprise.Customs.SG.V4.Business.Test, PublicKey=" + CommonAssemblyInfo.PublicKey)]
#endif
[assembly: PreventAssemblyReferences(allowedReferencePartialPaths: [
	"Enterprise.Customs.SG.MHUB.Test",
	"Enterprise.Customs.SG.V4.Business.Test",
	"Enterprise.Customs.SG.V4.DataTransfer",
	"Enterprise.Customs.SG.V4.DataTransfer.Test",
	"Enterprise.Customs.SG.V4.GUI",
	"Enterprise.Customs.SG.V4.GUI.Test",
	"Enterprise.Customs.SG.V4.GUI.Test.Winzor",
	"Enterprise.Customs.SG.V4.GUI.Winzor",
	"Enterprise.Customs.SG.V4.Module",
	"Enterprise.Customs.SG.V4.Module.Winzor",
	"Enterprise.Customs.SG.V4.ServiceTasks",
	"Enterprise.Customs.SG.V4.Business.XmlSerializers",
	"Enterprise.Customs.SG.Access.Business",
	"Enterprise.Customs.SG.Access.Business.Test",
	"Enterprise.Customs.SG.Access.GUI.Test",
	"ZClientUPE",
	"ZClientUPE.Test"
])]
