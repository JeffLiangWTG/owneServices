using System.Reflection;
using System.Runtime.CompilerServices;
using Enterprise.Customs.NO.Business;
using Enterprise.ZArchitecture.Business.UniversalCopy;

[assembly: AssemblyTitle("NO Customs Business")]
[assembly: AssemblyDescription("NO Customs Business")]
[assembly: UniversalCopyAddInfoPropertyDefinition(typeof(AutoNOAddInfo.Schema), Enterprise.Core.Constants.CountryCodes.Norway)]
#if DEBUG
[assembly: InternalsVisibleTo("Enterprise.Customs.NO.Business.Test, PublicKey = " + CommonAssemblyInfo.PublicKey)]
#endif
