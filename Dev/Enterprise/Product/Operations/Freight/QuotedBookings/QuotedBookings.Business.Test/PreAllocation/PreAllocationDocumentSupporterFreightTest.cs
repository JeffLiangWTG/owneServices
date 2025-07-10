using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocumentSupport.Testing;
using Enterprise.Freight.Business.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.QuotedBookings.Business.Test
{
	public class PreAllocationDocumentSupporterFreightTest : BaseFreightTest
	{
		public void TestSupportedDataContext()
		{
			IDocumentSupportable preAllocation = GetNewPreAllocation();
			Assert("Core.Constants.DataContext.Shipment is Supported", preAllocation.DocumentSupporter.IsDataContextSupported(new DataContextValueForTesting(Core.Constants.DataContext.Shipment)));
		}

		public void TestSupportedChildBusinessContexts()
		{
			IDocumentSupportable preAllocation = GetNewPreAllocation();
			AssertCollectionContains("needed to set up related docs", BusinessContext.Shipment, preAllocation.DocumentSupporter.SupportedChildBusinessContexts);
		}

		public void TestGetChildCollection()
		{
			PreAllocation preAllocation = GetNewPreAllocation();
			try
			{
				OrgHeader client = Factory.NewWithValidTestData<OrgHeader>();
				client.MiscServ.OM_EXPreAllocPrefix = "ZXY";
				preAllocation.QuotedBooking.ClientPK = client.PK;
				preAllocation.QuotedBooking.Mode = Core.Constants.RateMode.FCL;
				preAllocation.QuotedBooking.ConsignorDocumentaryAddress.E2_OA_Address = Factory.LoadTop1<OrgAddress>(new ZQuery()).PK;
				preAllocation.QuotedBooking.ConsigneeDocumentaryAddress.E2_OA_Address = Factory.LoadTop1<OrgAddress>(new ZQuery(OrgAddressSchema.PK, SQLComparisonOperator.NotEqual, preAllocation.QuotedBooking.ConsignorDocumentaryAddress.E2_OA_Address)).PK;
				preAllocation.QuotedBooking.Origin = "AUSYD";
				preAllocation.QuotedBooking.Destination = "AUBNE";
				preAllocation.HouseBillCount = 5;
				preAllocation.RunPreSaveValidation();
				preAllocation.Create();
				IDocumentSupportable iDoc = preAllocation;
				AssertEquals("should return 5 bookings", 5, iDoc.DocumentSupporter.GetChildCollection(null, BusinessContext.Shipment, null).Length);
			}
			finally
			{
				foreach (ForwardingShipment shipment in preAllocation.Bookings)
				{
					if (shipment.ShipmentJobHeader != null)
					{
						shipment.ShipmentJobHeader.Dispose();
					}
				}
			}
		}

		#region Implementation
		PreAllocation GetNewPreAllocation()
		{
			ForwardingShipment booking = QuotedBooking.CreateNewBooking(Factory);
			QuotedBooking quotedBooking = QuotedBooking.New(ZGuid.Empty, booking.PK, Factory);
			return new PreAllocation(quotedBooking, PreAllocation.PreAllocationState.New);
		}
		#endregion
	}
}
