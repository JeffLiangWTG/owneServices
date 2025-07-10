using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.Integration.TransportBooking;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transit.Business.Testing.DispatchConsignment
{
	[TestedType(typeof(WhsItemDispatchConsignmentDocManagerInfo))]
	internal class WhsItemDispatchConsignmentDocManagerInfoTest : DocManagerInfoTestCase
	{
		public void TestGetRelatedObjects()
		{
			var data = new TransitTestDataSimpleEnvironment(Factory);
			var dcn = Helper.CreateDispatchConsignment("DC001", data.Whs1.PK);

			var consolidation = (IDtbBookingConsolidation)Factory.New(ObjectFactory.GetType<IDtbBookingConsolidation>());
			consolidation.KB_ParentTableCode = dcn.TablePrefix;
			consolidation.KB_ParentID = dcn.PK;

			var dcnDocManagerInfo = new WhsItemDispatchConsignmentDocManagerInfo(dcn, dcn.DocManagerInfo.DocManagerCode);
			AssertContainsExactElementsInAnyOrder(new BusinessObject[] { (BusinessObject)consolidation }, dcnDocManagerInfo.RelatedObjects);
		}
		WhsTransitTestHelper Helper
		{
			get { return helper ?? (helper = new WhsTransitTestHelper(Factory)); }
		}

		WhsTransitTestHelper helper;

		public override BusinessObject GetEmptyParentBusinessObject()
		{
			return Factory.New<WhsItemDispatchConsignment>();
		}

		public override BusinessObject GetPopulatedParentBusinessObject()
		{
			return Factory.New<WhsItemDispatchConsignment>();
		}
	}
}
