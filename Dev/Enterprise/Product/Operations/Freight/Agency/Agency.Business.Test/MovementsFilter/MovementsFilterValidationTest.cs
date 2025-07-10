using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Freight.Agency.Business.Testing
{
	internal class MovementsFilterValidationTest : BusinessObjectValidationTestCase
	{
		public void TestMovementType()
		{
			Filter.MovementType = ContainerMovementTypes.Codes.WharfGateOut;
			AssertNoNotifications(Filter.MovementTypeInfo);
			Filter.MovementType = "XXX";
			AssertHasError(Filter.MovementTypeInfo, "Enter a valid Movement Type.");
			Filter.MovementType = "";
			AssertNoNotifications(Filter.MovementTypeInfo);
		}

		public void TestVessel()
		{
			BillOfLading shipment = Factory.New<BillOfLading>();
			Filter.Vessel = "MAJAPAHIT";
			AssertNoNotifications(Filter.VesselInfo);
			Filter.Vessel = "BLATICUS";
			AssertHasError(Filter.VesselInfo, "Enter a valid Vessel.");
			Filter.Vessel = "";
			AssertNoNotifications(Filter.VesselInfo);
		}

		public void TestDateOrder()
		{
			ZDateTime today = ZDateTime.Today;
			Filter.FromDate = ZDateTime.Empty;
			Filter.ToDate = ZDateTime.Empty;
			AssertNoNotifications(Filter.FromDateInfo);
			AssertNoNotifications(Filter.ToDateInfo);
			Filter.FromDate = today.AddDays(-3);
			Filter.ToDate = today.AddDays(-2);
			AssertNoNotifications(Filter.FromDateInfo);
			AssertNoNotifications(Filter.ToDateInfo);
			Filter.ToDate = today.AddDays(-4);
			AssertHasError(Filter.FromDateInfo, "From Date cannot be after To Date.");
			AssertHasError(Filter.ToDateInfo, "To Date cannot be before From Date.");
			Filter.FromDate = today.AddDays(-5);
			AssertNoNotifications(Filter.FromDateInfo);
			AssertNoNotifications(Filter.ToDateInfo);
			Filter.ToDate = today.AddDays(1);
			AssertHasWarning(Filter.ToDateInfo, "To Date is in the future.");
		}

		public void TestDateRange()
		{
			CombineAssertions(delegate
			{
				Filter.FromDate = ZDateTime.Today.AddMonths(-13);
				AssertNoNotifications("From Date: Don't add the 1-year-old warning.", Filter.FromDateInfo);
				Filter.FromDate = ZDateTime.Today.AddYears(-11);
				AssertNoNotifications("From Date: Don't add the 10-year-old error.", Filter.FromDateInfo);
				Filter.FromDate = ZDateTime.Empty;
				Filter.ToDate = ZDateTime.Today.AddMonths(-13);
				AssertNoNotifications("To Date: Don't add the 1-year-old warning.", Filter.ToDateInfo);
				Filter.FromDate = ZDateTime.Today.AddYears(-11);
				AssertNoNotifications("To Date: Don't add the 10-year-old error.", Filter.ToDateInfo);
			});
		}

		#region Implementation
		MovementsFilter Filter
		{
			get
			{
				return filter ?? (filter = new MovementsFilter(Factory, new CollectionRelationship(typeof(ContainerMovement))));
			}
		}

		MovementsFilter filter;
		#endregion
	}
}
