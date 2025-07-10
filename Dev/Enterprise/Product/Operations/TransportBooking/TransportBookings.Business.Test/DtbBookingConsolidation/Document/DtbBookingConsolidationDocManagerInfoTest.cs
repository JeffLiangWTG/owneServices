using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.TransportBookings.Business.Testing;
using Enterprise.TransportBookings.Shared;
using Enterprise.TransportCommon.Shared;
using NUnit.Framework;

namespace Enterprise.TransportBookings.Business.Test
{
	[TestedType(typeof(DtbBookingConsolidationDocManagerInfo))]
	public class DtbBookingConsolidationDocManagerInfoTest : DocManagerInfoTestCase
	{
		public override BusinessObject GetEmptyParentBusinessObject()
		{
			return Factory.New<DtbBookingConsolidation>();
		}

		public override BusinessObject GetPopulatedParentBusinessObject()
		{
			var template = Helper.CreateTransportBookingTemplate("IFCL", "FCL Import", Constants.CartageDirection.Import);
			Helper.AddInstructionToTemplate(template, OrganisationTypesList.Codes.CTO, InstructionTypes.Codes.PickUp);
			Helper.AddInstructionToTemplate(template, OrganisationTypesList.Codes.CNE, InstructionTypes.Codes.Delivery);
			Helper.AddInstructionToTemplate(template, OrganisationTypesList.Codes.CYD, InstructionTypes.Codes.Delivery);

			var consolidation = Helper.CreateConsolidation();
			var booking = consolidation.Bookings.AddNew();
			booking.KM_KT_NKBookingTemplate = template.KT_Code;

			return consolidation;
		}

		public void TestGetRelatedObjects()
		{
			var consolidation = Helper.CreateConsolidationMultiJob();
			var booking1 = consolidation.Bookings.AddNew();
			var booking2 = consolidation.Bookings.AddNew();

			var transportCo = Helper.CreateOrganisation("TRA");
			consolidation.Address.E2_OA_Address = transportCo.MainAddress.PK;

			var consolidationDocManagerInfoForTest = new DtbBookingConsolidationDocManagerInfoForTest(consolidation, ((IDocManagerSupport)consolidation).DocManagerInfo.DocManagerCode);

			var relatedObjects = consolidationDocManagerInfoForTest.GetRelatedObjectsForTest();
			AssertContainsExactElementsInAnyOrder(new BusinessObject[] { transportCo, booking1, booking2 }, relatedObjects);
		}

		public void TestGetRelatedObjects_WhenConsolidationParentIsNull()
		{
			var consolidation = Helper.CreateConsolidation();
			var booking = consolidation.Bookings.AddNew();
			var consolidationDocManagerInfoForTest = new DtbBookingConsolidationDocManagerInfoForTest(consolidation, ((IDocManagerSupport)consolidation).DocManagerInfo.DocManagerCode);

			DummyBaseBusinessObject.TypeDecider.TypeForLoadOverride = typeof(DummyBusinessObject);
			var parent = Factory.New<DummyBusinessObject>();
			consolidation.KB_ParentID = parent.PK;
			consolidation.KB_ParentTableCode = parent.TablePrefix;
			Assert("Precondition", !typeof(IDtbBookingParent).IsAssignableFrom(parent.GetType()));

			var relatedObjects = consolidationDocManagerInfoForTest.GetRelatedObjectsForTest();
			AssertContainsExactElementsInAnyOrder(new BusinessObject[] { booking }, relatedObjects);
		}

		class DtbBookingConsolidationDocManagerInfoForTest : DtbBookingConsolidationDocManagerInfo
		{
			public DtbBookingConsolidationDocManagerInfoForTest(DtbBookingConsolidation transportBooking, ZString docManagerCode)
			: base(transportBooking, docManagerCode)
			{
			}

			public BusinessObject[] GetRelatedObjectsForTest()
			{
				return base.GetRelatedObjects();
			}
		}

		TransportBookingTestHelper Helper
		{
			get { return helper ?? (helper = new TransportBookingTestHelper(Factory)); }
		}

		TransportBookingTestHelper helper;
	}
}
