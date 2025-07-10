using System.Reflection;
using System.Runtime.CompilerServices;
using Enterprise.Customs.EU.Business;
using Enterprise.ZArchitecture.Business.UniversalCopy;

[assembly: AssemblyTitle("TR Customs Business")]
[assembly: AssemblyDescription("TR Customs Business")]
[assembly: UniversalCopyAddInfoPropertyDefinition(typeof(AutoEUAddInfo.Schema), Enterprise.Core.Constants.CountryCodes.Turkey)]
#if DEBUG
[assembly: InternalsVisibleTo("Enterprise.Customs.TR.Business.Test, PublicKey=" + CommonAssemblyInfo.PublicKey)]
#endif
