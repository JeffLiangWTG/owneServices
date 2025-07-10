using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.eTail.Business.Testing
{
	[TestedType(typeof(HVLVConsignmentForStandAloneDeclarationConversionWrapper))]
	public class HVLVConsignmentForStandAloneDeclarationConversionWrapperTest : NonPersistentBusinessObjectTestCase
	{
		public void TestConsignment()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var consignment = Factory.New<HVLVConsignment>();
			consignment.HVC_WaybillNumber = "Waybill 001";
			consignment.HVC_JS_ManifestedOnShipment = shipment.PK;

			var consignmentForDeclaration = new HVLVConsignmentForStandAloneDeclarationConversionWrapper(consignment);

			AssertEquals("Expected consignment", consignment, consignmentForDeclaration.Consignment);
		}

		public void TestConvertToStandAloneDeclaration()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
			shipment.JS_RL_NKOrigin = "NZAKL";
			shipment.JS_RL_NKDestination = "USCHI";

			var departureConsol = shipment.Consols.AddNew();
			departureConsol.JK_RL_NKLoadPort = "NZAKL";
			departureConsol.JK_RL_NKDischargePort = "AUBNE";

			var arrivalConsol = shipment.Consols.AddNew();
			arrivalConsol.JK_RL_NKLoadPort = "AUBNE";
			arrivalConsol.JK_RL_NKDischargePort = "USCHI";

			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment.HVC_JS_ManifestedOnShipment = shipment.PK;
			consignment.HVC_GoodsDescription = "Goods";
			consignment.Items.AddNew();

			Factory.Save();

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.UnitedStates))
			{
				var consignmentForDeclaration = new HVLVConsignmentForStandAloneDeclarationConversionWrapper(consignment);
				var convertToStandAloneDeclarationService = new ConvertToStandAloneDeclarationService();

				AssertEquals("Precondition:", ZGuid.Empty, consignment.HVC_JE_ImportDeclaration);

				convertToStandAloneDeclarationService.ConvertToStandAloneDeclaration(consignmentForDeclaration.Consignment);
				Factory.Save();

				AssertNotEquals("Expected declaration reference on the consignment", ZGuid.Empty, consignment.HVC_JE_ImportDeclaration);
			}
		}
	}
}
