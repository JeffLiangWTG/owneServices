using System.Reflection;
using System.Runtime.CompilerServices;
using CargoWise.Definitions;
using Enterprise.Customs.US.Business;
using Enterprise.ZArchitecture.Business.UniversalCopy;

[assembly: AssemblyTitle("US Customs Business")]
[assembly: AssemblyDescription("US Customs Business")]

#pragma warning disable IDE0001 // Prevent name simplification to AutoUSAddInfo
[assembly: UniversalCopyAddInfoPropertyDefinition(typeof(AddInfo.Schema), Enterprise.Core.Constants.CountryCodes.UnitedStates)]
#pragma warning restore IDE0001 // Prevent name simplification to AutoUSAddInfo

#if DEBUG
[assembly: InternalsVisibleTo("Enterprise.Customs.US.Business.Test, PublicKey=" + CommonAssemblyInfo.PublicKey)]
#endif

[assembly: ApplicationConfiguration("Enterprise.Customs.US.Business", "Enterprise.Customs.US.Business.USEnterpriseApplicationConfiguration.xml")]
