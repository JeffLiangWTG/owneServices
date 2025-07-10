using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Telematics.Business.Test
{
	[TestedType(typeof(TelPreDriveChecklistTemplateEntry))]
	class TelPreDriveChecklistTemplateEntryTests : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var o = (TelPreDriveChecklistTemplateEntry)base.GetNewBusinessObjectForDeleteTest(factory);
			o.Header.TTH_Type = TelPreDriveChecklistHeaderTypes.Codes.FTD;
			o.TTE_Index = 1;
			return o;
		}

		protected override BusinessObject GetBusinessObjectForFetchForLoad()
		{
			var o = (TelPreDriveChecklistTemplateEntry)base.GetBusinessObjectForFetchForLoad();
			o.Header.TTH_Type = TelPreDriveChecklistHeaderTypes.Codes.FTD;
			o.TTE_Index = 1;
			return o;
		}
	}
}
