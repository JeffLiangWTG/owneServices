using System.Reflection;
using System.Runtime.CompilerServices;
using Enterprise.Customs.NZ.Business;
using Enterprise.ZArchitecture.Business.UniversalCopy;

[assembly: AssemblyTitle("NZCustoms Business")]
[assembly: AssemblyDescription("NZCustoms Business")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCulture("")]
[assembly: UniversalCopyAddInfoPropertyDefinition(typeof(AutoNZAddInfo.Schema), Enterprise.Core.Constants.CountryCodes.NewZealand)]
#if DEBUG
[assembly: InternalsVisibleTo("Enterprise.Customs.NZ.Business.Test, PublicKey=" + CommonAssemblyInfo.PublicKey)]
#endif
