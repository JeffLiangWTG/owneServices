using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Telematics.Business.Test
{
	[TestedType(typeof(TelPreDriveChecklistTemplateHeader))]
	class TelPreDriveChecklistTemplateHeaderTests : EnterpriseBusinessObjectTestCase
	{
		protected override bool IsDeleteSupported()
		{
			return false;
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var o = (TelPreDriveChecklistTemplateHeader)base.GetNewBusinessObjectForDeleteTest(factory);
			return ObjectConfiguredToFollowConstraints(o);
		}

		protected override BusinessObject GetBusinessObjectForFetchForLoad()
		{
			var o = (TelPreDriveChecklistTemplateHeader)base.GetBusinessObjectForFetchForLoad();
			return ObjectConfiguredToFollowConstraints(o);
		}

		TelPreDriveChecklistTemplateHeader ObjectConfiguredToFollowConstraints(TelPreDriveChecklistTemplateHeader o)
		{
			o.TTH_Type = TelPreDriveChecklistHeaderTypes.Codes.FTD;
			var i = 1;
			foreach (var entry in o.Entries)
			{
				entry.TTE_Index = (short)i++;
			}
			return o;
		}
	}
}
