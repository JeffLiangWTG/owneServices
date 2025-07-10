using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.Agency.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.GUI.Testing
{
	internal class UnsafeAgencyBookingFormTest : TestCase
	{
		[UseSnapshotProtection]
		public void TestSaving_DeDuplicate()
		{
			RatingDataRegistry.Instance.ShouldShowAutoRatingNotRunWarning.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			BusinessObjectFactory factory = new BusinessObjectFactory();
			OrgHeader principal = BaseAgencyTest.NewPrincipal(factory);
			principal.OH_Code = "PRINCIPAL";
			OrgHeader bookingParty = factory.NewWithValidTestData<OrgHeader>();
			bookingParty.OH_Code = "BPARTY";
			bookingParty.OH_IsConsignor = true;
			AgencyBooking shipment = (AgencyBooking)BaseAgencyTest.NewShipment(factory, null, null, false, false);
			shipment.JS_RL_NKOrigin = "AUBNE";
			shipment.JS_RL_NKDestination = "SGSIN";
			shipment.BookingPartyDocumentaryAddress.E2_OA_Address = bookingParty.MainAddress.PK;
			shipment.JS_OH_DeliveryAgent = principal.PK;
			shipment.JS_GoodsDescription = "supermagic";
			AgencyShipmentContainer container1 = shipment.BookedContainers.AddNew();
			container1.JC_ContainerNum = "TEST4100013";
			container1.JC_RC = factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
			AgencyShipmentContainer container2 = shipment.BookedContainers.AddNew();
			container2.JC_ContainerNum = "TEST4100029";
			container2.JC_RC = factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20RE").PK;
			AgencyShipmentPackLine line = shipment.OuterPackLines.AddNew();
			line.JL_JC = ZGuid.Empty;
			line.JL_ContainerPackingOrder = 1;
			factory.Save();
			BusinessObjectFactory otherFactory = new BusinessObjectFactory();
			using (TestCaseWithFactory.GetFactoryIsolater(factory))
			using (TestCaseWithFactory.GetFactoryIsolater(otherFactory))
			{
				AgencyShipmentPackLine lineInOtherFactory = otherFactory.Load<AgencyShipmentPackLine>(line.PK);
				lineInOtherFactory.JL_JC = container1.PK;
				lineInOtherFactory.JL_ContainerPackingOrder = 1;
				otherFactory.Save();
			}

			using (AgencyBookingForm form = new AgencyBookingForm(shipment))
			{
				form.Show();
				Application.DoEvents();
				line.JL_JC = container2.PK;
				line.JL_ContainerPackingOrder = 1;
				shipment.RunPreSaveValidation();
				TestCaseWithFactory.AssertNoErrors("precondition: should have no errors", shipment);
				form.FireSaveButton();
				CombineAssertions(delegate
				{
					AssertEquals("Should not have been saved", true, shipment.HasChanges);
					AssertEquals("Should have shown the conflict dialog", "Information Another user has made changes to the packing that conflicts with your own changes. Please verify that the packing is correct and try again.", UnitTestUserNotification.Instance.LastMessage.ToString());
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				});
				form.FireSaveButton();
				CombineAssertions(delegate
				{
					AssertEquals("Should have been saved", false, shipment.HasChanges);
					AssertEquals("Should not have shown a dialog", "None ", UnitTestUserNotification.Instance.LastMessage.ToString());
				});
			}

			BusinessObjectFactory loadFactory = new BusinessObjectFactory();
			AgencyShipmentPackLine lineInLoadFactory = loadFactory.Load<AgencyShipmentPackLine>(line.PK);
			AssertContainsExactElementsInAnyOrder("saved line should only be linked to container2", BusinessObjectEqualityComparer<AgencyShipmentContainer>.PKOnlyComparer, (c) => c.JC_ContainerNum, new AgencyShipmentContainer[] { container2 }, lineInLoadFactory.Containers.ToArray<AgencyShipmentContainer>());
		}
	}
}
