using Enterprise.Customs.US.ISF.Business;
using Enterprise.eTail.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.US.ISF.Module.Testing
{
	[TestedType(typeof(ISFController))]
	sealed class ISFControllerTest : ZControllerBasherTest
	{
		public void TestGetNewBusinessEntityInLocalFactory_WhenShipmentNumberIsNotNull_ReturnsISFHeader()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_UniqueConsignRef = "ShipmentNotTasty";
			shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment.HVC_JS_ManifestedOnShipment = shipment.PK;
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "ABCDE";
			shipment.ConsigneePK = org.PK;
			Factory.Save();
			var controller = new ISFController();
			using (controller.SetArgsForNewForm(new[] { shipment.JS_UniqueConsignRef.ToString() }))
			using (var form = controller.ShowNewForm())
			{
				var isfHeader = form.BusinessEntityForPersistingForm as CusISFHeader;
				AssertEquals(shipment.ConsigneePK, isfHeader.BF_OH_Importer);
			}
		}

		public override void TestDeleteForm()
		{
			Assert("Deleting is not currently supported", true);
		}

		protected override ControllerID GetControllerID() => ControllerIDs.ImporterSecurityFiling;
	}
}
