using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Customs.NZ.Business.Declaration.Testing
{
	sealed class JobDeclarationCartageTest : TestCaseWithFactory
	{
		public void TestDefaultDeliveryCartageFromImporter()
		{
			var importer = Factory.New<OrgHeader>();

			var cntCartage = Factory.NewWithValidTestData<OrgHeader>();
			cntCartage.OH_IsShippingProvider = true;
			cntCartage.OH_IsLocalTransport = true;

			importer.SetRelatedParty(cntCartage, RelatedPartyTypeList.Codes.LocalTransport, RelatedPartyDirectionList.Codes.Delivery, Core.Constants.TransportModes.Sea, ZString.Empty);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_OH_Importer = importer.PK;

			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			AssertEquals("JE_ContainerMode empty", ZString.Empty, declaration.JE_ContainerMode);
			AssertEquals("Default DeliveryCartageCo is set without containers", cntCartage, declaration.DocsAndCartage.DeliveryCartageCo);

			var container = declaration.CusContainers.AddNew();
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			AssertEquals("JE_ContainerMode CNT", Core.Constants.ContainerModes.Containerised, declaration.JE_ContainerMode);
			AssertEquals("Default DeliveryCartageCo is set for default container mode", cntCartage, declaration.DocsAndCartage.DeliveryCartageCo);
		}

		public void TestDefaultDeliveryCartageFromConsignee_Air()
		{
			var consignee = Factory.New<OrgHeader>();

			var cntCartageAIR = Factory.NewWithValidTestData<OrgHeader>();
			cntCartageAIR.OH_IsShippingProvider = true;
			cntCartageAIR.OH_IsLocalTransport = true;
			consignee.SetRelatedParty(cntCartageAIR, RelatedPartyTypeList.Codes.LocalTransport, RelatedPartyDirectionList.Codes.Delivery, Core.Constants.TransportModes.Air, ZString.Empty);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_OH_Importer = consignee.PK;

			AssertEquals("DeliveryCartageCo is default", null, declaration.DocsAndCartage.DeliveryCartageCo);

			declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			AssertEquals("Cartage", cntCartageAIR, declaration.DocsAndCartage.DeliveryCartageCo);
		}

		public void TestDefaultDeliveryCartageFromConsignee_Post()
		{
			var consignee = Factory.New<OrgHeader>();

			var cntCartagePOST = Factory.NewWithValidTestData<OrgHeader>();
			cntCartagePOST.OH_IsShippingProvider = true;
			cntCartagePOST.OH_IsLocalTransport = true;
			consignee.SetRelatedParty(cntCartagePOST, RelatedPartyTypeList.Codes.LocalTransport, RelatedPartyDirectionList.Codes.Delivery, "PST", ZString.Empty);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_OH_Importer = consignee.PK;

			AssertEquals("Cartage", null, declaration.DocsAndCartage.DeliveryCartageCo);

			declaration.JE_TransportMode = "PST";
			AssertEquals("Cartage", cntCartagePOST, declaration.DocsAndCartage.DeliveryCartageCo);
		}

		public void TestDefaultDeliveryCartageFromConsignee_Sea()
		{
			var consignee = Factory.New<OrgHeader>();

			var cntCartageCNT = Factory.NewWithValidTestData<OrgHeader>();
			cntCartageCNT.OH_IsShippingProvider = true;
			cntCartageCNT.OH_IsLocalTransport = true;
			consignee.SetRelatedParty(cntCartageCNT, RelatedPartyTypeList.Codes.LocalTransport, RelatedPartyDirectionList.Codes.Delivery, Core.Constants.TransportModes.Sea, Core.Constants.ContainerModes.Containerised);

			var cntCartageFCL = Factory.NewWithValidTestData<OrgHeader>();
			cntCartageFCL.OH_IsShippingProvider = true;
			cntCartageFCL.OH_IsLocalTransport = true;
			consignee.SetRelatedParty(cntCartageFCL, RelatedPartyTypeList.Codes.LocalTransport, RelatedPartyDirectionList.Codes.Delivery, Core.Constants.TransportModes.Sea, Core.Constants.ContainerModes.FCL);

			var cntCartageLCL = Factory.NewWithValidTestData<OrgHeader>();
			cntCartageLCL.OH_IsShippingProvider = true;
			cntCartageLCL.OH_IsLocalTransport = true;
			consignee.SetRelatedParty(cntCartageLCL, RelatedPartyTypeList.Codes.LocalTransport, RelatedPartyDirectionList.Codes.Delivery, Core.Constants.TransportModes.Sea, Core.Constants.ContainerModes.LCL);

			var cntCartageBLK = Factory.NewWithValidTestData<OrgHeader>();
			cntCartageBLK.OH_IsShippingProvider = true;
			cntCartageBLK.OH_IsLocalTransport = true;
			consignee.SetRelatedParty(cntCartageBLK, RelatedPartyTypeList.Codes.LocalTransport, RelatedPartyDirectionList.Codes.Delivery, Core.Constants.TransportModes.Sea, Core.Constants.ContainerModes.Bulk);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_OH_Importer = consignee.PK;

			var container = declaration.CusContainers.AddNew();
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			AssertEquals("DeliveryCartageCo CNT is set with containers", cntCartageCNT, declaration.DocsAndCartage.DeliveryCartageCo);

			container.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.FCL;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			AssertEquals("DeliveryCartageCo is cntCartageFCL", cntCartageFCL, declaration.DocsAndCartage.DeliveryCartageCo);

			container.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.LCL;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			AssertEquals("DeliveryCartageCo is cntCartageLCL", cntCartageLCL, declaration.DocsAndCartage.DeliveryCartageCo);

			container.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.Bulk;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			AssertEquals("DeliveryCartageCo is cntCartageBLK", cntCartageBLK, declaration.DocsAndCartage.DeliveryCartageCo);
		}

		public void TestDefaultDeliveryCartageFromConsignee_Sea_MultipleContainers()
		{
			var consignee = Factory.New<OrgHeader>();

			var cntCartage = Factory.NewWithValidTestData<OrgHeader>();
			cntCartage.OH_IsShippingProvider = true;
			cntCartage.OH_IsLocalTransport = true;
			consignee.SetRelatedParty(cntCartage, RelatedPartyTypeList.Codes.LocalTransport, RelatedPartyDirectionList.Codes.Delivery, Core.Constants.TransportModes.Sea, ZString.Empty);

			var cntCartageFCL = Factory.NewWithValidTestData<OrgHeader>();
			cntCartageFCL.OH_IsShippingProvider = true;
			cntCartageFCL.OH_IsLocalTransport = true;
			consignee.SetRelatedParty(cntCartageFCL, RelatedPartyTypeList.Codes.LocalTransport, RelatedPartyDirectionList.Codes.Delivery, Core.Constants.TransportModes.Sea, Core.Constants.ContainerModes.FCL);

			var cntCartageLCL = Factory.NewWithValidTestData<OrgHeader>();
			cntCartageLCL.OH_IsShippingProvider = true;
			cntCartageLCL.OH_IsLocalTransport = true;
			consignee.SetRelatedParty(cntCartageLCL, RelatedPartyTypeList.Codes.LocalTransport, RelatedPartyDirectionList.Codes.Delivery, Core.Constants.TransportModes.Sea, Core.Constants.ContainerModes.LCL);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_OH_Importer = consignee.PK;

			var container = declaration.CusContainers.AddNew();
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			AssertEquals("DeliveryCartageCo is default", cntCartage, declaration.DocsAndCartage.DeliveryCartageCo);

			container.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.FCL;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			AssertEquals("DeliveryCartageCo is cntCartageFCL", cntCartageFCL, declaration.DocsAndCartage.DeliveryCartageCo);

			var container2 = declaration.CusContainers.AddNew();
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			AssertEquals("DeliveryCartageCo is cntCartageFCL", cntCartageFCL, declaration.DocsAndCartage.DeliveryCartageCo);

			container2.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.LCL;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			AssertEquals("DeliveryCartageCo is cntCartageFCL", cntCartageFCL, declaration.DocsAndCartage.DeliveryCartageCo);

			container.CO_FCL_LCL_AIR = ZString.Empty;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			AssertEquals("DeliveryCartageCo is default", cntCartageLCL, declaration.DocsAndCartage.DeliveryCartageCo);

			declaration.CusContainers.RemoveAndDeleteAll();
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			AssertEquals("DeliveryCartageCo is default", cntCartage, declaration.DocsAndCartage.DeliveryCartageCo);
		}

		public void TestDefaultPickupCartageFromSupplier()
		{
			var supplier = Factory.New<OrgHeader>();

			var cntCartage = Factory.NewWithValidTestData<OrgHeader>();
			cntCartage.OH_IsShippingProvider = true;
			cntCartage.OH_IsLocalTransport = true;
			supplier.SetRelatedParty(cntCartage, RelatedPartyTypeList.Codes.LocalTransport, RelatedPartyDirectionList.Codes.Pickup, Core.Constants.TransportModes.Sea, ZString.Empty);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			declaration.JE_OH_Supplier = supplier.PK;

			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			AssertEquals("JE_ContainerMode empty", ZString.Empty, declaration.JE_ContainerMode);
			AssertEquals("Default PickupCartageCo is set without containers", cntCartage, declaration.DocsAndCartage.PickupCartageCo);

			var container = declaration.CusContainers.AddNew();
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			AssertEquals("JE_ContainerMode CNT", Core.Constants.ContainerModes.Containerised, declaration.JE_ContainerMode);
			AssertEquals("Default PickupCartageCo is set for default container mode", cntCartage, declaration.DocsAndCartage.PickupCartageCo);
		}

		public void TestDefaultPickupCartageFromConsignor_Air()
		{
			var consignor = Factory.New<OrgHeader>();

			var cntCartageAIR = Factory.NewWithValidTestData<OrgHeader>();
			cntCartageAIR.OH_IsShippingProvider = true;
			cntCartageAIR.OH_IsLocalTransport = true;
			consignor.SetRelatedParty(cntCartageAIR, RelatedPartyTypeList.Codes.LocalTransport, RelatedPartyDirectionList.Codes.Pickup, Core.Constants.TransportModes.Air, ZString.Empty);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			declaration.JE_OH_Supplier = consignor.PK;

			AssertEquals("PickupCartageCo is default", null, declaration.DocsAndCartage.PickupCartageCo);

			declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			AssertEquals("Cartage", cntCartageAIR, declaration.DocsAndCartage.PickupCartageCo);
		}

		public void TestDefaultPickupCartageFromConsignor_Post()
		{
			var consignor = Factory.New<OrgHeader>();

			var cntCartagePOST = Factory.NewWithValidTestData<OrgHeader>();
			cntCartagePOST.OH_IsShippingProvider = true;
			cntCartagePOST.OH_IsLocalTransport = true;
			consignor.SetRelatedParty(cntCartagePOST, RelatedPartyTypeList.Codes.LocalTransport, RelatedPartyDirectionList.Codes.Pickup, "PST", ZString.Empty);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			declaration.JE_OH_Supplier = consignor.PK;

			AssertEquals("Cartage", null, declaration.DocsAndCartage.PickupCartageCo);

			declaration.JE_TransportMode = "PST";
			AssertEquals("Cartage", cntCartagePOST, declaration.DocsAndCartage.PickupCartageCo);
		}

		public void TestDefaultPickupCartageFromConsignor_Sea()
		{
			var consignor = Factory.New<OrgHeader>();

			var cntCartageCNT = Factory.NewWithValidTestData<OrgHeader>();
			cntCartageCNT.OH_IsShippingProvider = true;
			cntCartageCNT.OH_IsLocalTransport = true;
			consignor.SetRelatedParty(cntCartageCNT, RelatedPartyTypeList.Codes.LocalTransport, RelatedPartyDirectionList.Codes.Pickup, Core.Constants.TransportModes.Sea, Core.Constants.ContainerModes.Containerised);

			var cntCartageFCL = Factory.NewWithValidTestData<OrgHeader>();
			cntCartageFCL.OH_IsShippingProvider = true;
			cntCartageFCL.OH_IsLocalTransport = true;
			consignor.SetRelatedParty(cntCartageFCL, RelatedPartyTypeList.Codes.LocalTransport, RelatedPartyDirectionList.Codes.Pickup, Core.Constants.TransportModes.Sea, Core.Constants.ContainerModes.FCL);

			var cntCartageLCL = Factory.NewWithValidTestData<OrgHeader>();
			cntCartageLCL.OH_IsShippingProvider = true;
			cntCartageLCL.OH_IsLocalTransport = true;
			consignor.SetRelatedParty(cntCartageLCL, RelatedPartyTypeList.Codes.LocalTransport, RelatedPartyDirectionList.Codes.Pickup, Core.Constants.TransportModes.Sea, Core.Constants.ContainerModes.LCL);

			var cntCartageBLK = Factory.NewWithValidTestData<OrgHeader>();
			cntCartageBLK.OH_IsShippingProvider = true;
			cntCartageBLK.OH_IsLocalTransport = true;
			consignor.SetRelatedParty(cntCartageBLK, RelatedPartyTypeList.Codes.LocalTransport, RelatedPartyDirectionList.Codes.Pickup, Core.Constants.TransportModes.Sea, Core.Constants.ContainerModes.Bulk);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			declaration.JE_OH_Supplier = consignor.PK;

			var container = declaration.CusContainers.AddNew();
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			AssertEquals("PickupCartageCo CNT is set with containers", cntCartageCNT, declaration.DocsAndCartage.PickupCartageCo);

			container.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.FCL;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			AssertEquals("PickupCartageCo is cntCartageFCL", cntCartageFCL, declaration.DocsAndCartage.PickupCartageCo);

			container.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.LCL;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			AssertEquals("PickupCartageCo is cntCartageLCL", cntCartageLCL, declaration.DocsAndCartage.PickupCartageCo);

			container.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.Bulk;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			AssertEquals("PickupCartageCo is cntCartageBLK", cntCartageBLK, declaration.DocsAndCartage.PickupCartageCo);
		}

		public void TestDefaultPickupCartageFromConsignor_Sea_MultipleContainers()
		{
			var consignee = Factory.New<OrgHeader>();

			var cntCartage = Factory.NewWithValidTestData<OrgHeader>();
			cntCartage.OH_IsShippingProvider = true;
			cntCartage.OH_IsLocalTransport = true;
			consignee.SetRelatedParty(cntCartage, RelatedPartyTypeList.Codes.LocalTransport, RelatedPartyDirectionList.Codes.Pickup, Core.Constants.TransportModes.Sea, ZString.Empty);

			var cntCartageFCL = Factory.NewWithValidTestData<OrgHeader>();
			cntCartageFCL.OH_IsShippingProvider = true;
			cntCartageFCL.OH_IsLocalTransport = true;
			consignee.SetRelatedParty(cntCartageFCL, RelatedPartyTypeList.Codes.LocalTransport, RelatedPartyDirectionList.Codes.Pickup, Core.Constants.TransportModes.Sea, Core.Constants.ContainerModes.FCL);

			var cntCartageLCL = Factory.NewWithValidTestData<OrgHeader>();
			cntCartageLCL.OH_IsShippingProvider = true;
			cntCartageLCL.OH_IsLocalTransport = true;
			consignee.SetRelatedParty(cntCartageLCL, RelatedPartyTypeList.Codes.LocalTransport, RelatedPartyDirectionList.Codes.Pickup, Core.Constants.TransportModes.Sea, Core.Constants.ContainerModes.LCL);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			declaration.JE_OH_Supplier = consignee.PK;

			var container = declaration.CusContainers.AddNew();
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			AssertEquals("PickupCartageCo is default", cntCartage, declaration.DocsAndCartage.PickupCartageCo);

			container.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.FCL;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			AssertEquals("PickupCartageCo is cntCartageFCL", cntCartageFCL, declaration.DocsAndCartage.PickupCartageCo);

			var container2 = declaration.CusContainers.AddNew();
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			AssertEquals("PickupCartageCo is cntCartageFCL", cntCartageFCL, declaration.DocsAndCartage.PickupCartageCo);

			container2.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.LCL;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			AssertEquals("PickupCartageCo is cntCartageFCL", cntCartageFCL, declaration.DocsAndCartage.PickupCartageCo);

			container.CO_FCL_LCL_AIR = ZString.Empty;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			AssertEquals("PickupCartageCo is default", cntCartageLCL, declaration.DocsAndCartage.PickupCartageCo);

			declaration.CusContainers.RemoveAndDeleteAll();
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			AssertEquals("PickupCartageCo is default", cntCartage, declaration.DocsAndCartage.PickupCartageCo);
		}
	}
}
