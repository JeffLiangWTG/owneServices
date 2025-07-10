using System.Reflection;
using Enterprise.Customs.EU.Business;
using Enterprise.ZArchitecture.Business.UniversalCopy;

[assembly: AssemblyTitle("SE Customs Business")]
[assembly: AssemblyDescription("SE Customs Business Project")]
[assembly: UniversalCopyAddInfoPropertyDefinition(typeof(AutoEUAddInfo.Schema), Enterprise.Core.Constants.CountryCodes.Sweden)]
