using System.Reflection;
[assembly: AssemblyTitle("NL Service Tasks Test")]
[assembly: AssemblyDescription("NL Service Tasks Test Project")]

#if DEBUG
[assembly: Enterprise.MasterFiles.Business.Testing.CountrySpecificTest(Enterprise.Core.Constants.CountryCodes.Netherlands)]
#endif
