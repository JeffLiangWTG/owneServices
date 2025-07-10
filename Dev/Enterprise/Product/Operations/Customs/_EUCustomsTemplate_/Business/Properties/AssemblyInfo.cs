using System.Reflection;
using Enterprise.Customs.EU.Business;
using Enterprise.ZArchitecture.Business.UniversalCopy;

[assembly: AssemblyTitle("_EUCustomsTemplate_ Customs Business")]
[assembly: AssemblyDescription("_EUCustomsTemplate_ Customs Business Project")]
[assembly: UniversalCopyAddInfoPropertyDefinition(typeof(AutoEUAddInfo.Schema), Enterprise.Core.Constants.CountryCodes._EUTemplateCountryName_)]
