using CargoWise.EntityFramework;
using Enterprise.Freight.Business.Testing;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Tracking.Business
{
	[TestedType(typeof(TrackingOrder))]
	sealed class TrackingOrderBOTest : Freight.Forwarding.Orders.Business.Testing.OrderBOTest
	{
		protected override void SetUp()
		{
			base.SetUp();
			beforeSetUpState = Globals.IsWeb;
			Globals.IsWeb = true;
		}

		bool beforeSetUpState;

		protected override void TearDown()
		{
			SuppressionForTest.CacheObjectClear();
			Globals.IsWeb = beforeSetUpState;
			base.TearDown();
		}

		#region Not Applicable Overrides

		public override void TestSaveAndDeleteBusinessObject()
		{
			Assert("not applicable for read-only web objects", true);
		}

		public override void TestCustomLabelsMandatoryValidation__()
		{
			Assert("CustomLabels validation is disabled for web in JobOrderHeaderValidation at the moment", true);
		}

		#endregion TestSaveAndDelete

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			OrgHeader buyer = CreateNewOrg(Factory, "TBY");
			OrgHeader supplier = CreateNewOrg(Factory, "TSP");

			Order result = Factory.New<TrackingOrder>();
			result.BuyerPK = buyer.PK;
			result.SupplierPK = supplier.PK;
			result.JD_OrderNumber = "1234567890";

			return result;
		}

		#endregion
	}
}
