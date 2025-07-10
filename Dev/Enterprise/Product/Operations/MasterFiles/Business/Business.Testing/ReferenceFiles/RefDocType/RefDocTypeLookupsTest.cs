using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class RefDocTypeLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestDocumentReceivedEvents()
		{
			AssertEquals("Events included in list", true, DocType.Lookups.DocumentReceivedEvents.ContainsCode(Events.CargoAvailable.Code));
			AssertEquals("Events included in list", true, DocType.Lookups.DocumentReceivedEvents.ContainsCode(Events.Arrival.Code));
			AssertEquals("Change logs not included in list", false, DocType.Lookups.DocumentReceivedEvents.ContainsCode(Events.AddedARecordToTheSystem.Code));
		}

		public void TestDocumentReceivedEvents_ProductivityWiseEnabled()
		{
			DataRegistry.Instance.ProductivityWiseModeEnabled = true;
			AssertEquals("Event NOT included in list when in ProductivityWise mode", false, DocType.Lookups.DocumentReceivedEvents.ContainsCode(Events.CargoAvailable.Code));
			AssertEquals("Events included in list", true, DocType.Lookups.DocumentReceivedEvents.ContainsCode(Events.Arrival.Code));
			AssertEquals("Change logs not included in list", false, DocType.Lookups.DocumentReceivedEvents.ContainsCode(Events.AddedARecordToTheSystem.Code));
			AssertEquals(1, DocType.Lookups.DocumentReceivedEvents.GetAllCodes().Where(c => c == "Z42").Count());
		}

		#region Implementation

		RefDocType DocType
		{
			get
			{
				if (docType == null)
				{
					docType = Factory.New<RefDocType>();
				}
				return docType;
			}
		}
		RefDocType docType;

		#endregion
	}
}
