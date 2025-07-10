using System;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules.DocumentScanning;

[assembly: AssemblyDataProvider(
	typeof(GlobalCompanyTariffData),
	Enterprise.Core.Constants.DocManagerCodes.GlobalCompanyTariff)]

namespace Enterprise.Rating.Business
{
	public class GlobalCompanyTariffData : CompanyTariffData
	{
		public override Type BusinessObjectType { get { return typeof(GlobalTariff); } }
		public override MultilingualString HumanReadableName { get { return ResString.GetMultilingualString("5cff03c3-8204-4257-88f7-951f37b49a35", "Global Company Tariff"); } }
		public override IEDocsViaUniversalXmlSupport GetEDocsViaUniversalXmlSupport() => new RatingHeaderEDocsViaUniversalXmlSupport(this, RatingConstants.RatingHeaderTypes.Tariff, true);
	}
}
