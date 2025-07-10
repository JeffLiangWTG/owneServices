using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Telematics.Business.Test
{
	[TestedType(typeof(TelPreDriveChecklistEntry))]
	public class TelPreDriveChecklistEntryTests : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var o = (TelPreDriveChecklistEntry)base.GetNewBusinessObjectForDeleteTest(factory);
			o.Header.TPH_Type = TelPreDriveChecklistHeaderTypes.Codes.FTD;
			o.TPE_Index = 1;
			return o;
		}

		protected override BusinessObject GetBusinessObjectForFetchForLoad()
		{
			var o = (TelPreDriveChecklistEntry)base.GetBusinessObjectForFetchForLoad();
			o.Header.TPH_Type = TelPreDriveChecklistHeaderTypes.Codes.FTD;
			o.TPE_Index = 1;
			return o;
		}
	}
}
