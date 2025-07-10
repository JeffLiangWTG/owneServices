using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.TransportCommon.Shared;

namespace Enterprise.TransportConsignment.Business.Testing
{
	sealed class ConsignmentHelperTest : DtbBookingConsignmentTestCaseWithFactory
	{
		#region TestGetInstructionDocAddressTypeToChangeTo

		public void TestGetInstructionDocAddressTypeToChangeTo()
		{
			var unpackCFS = Helper.CreateOrganisation("UCFS");
			var packCFS = Helper.CreateOrganisation("PCFS");
			var cnrWithCFS = Helper.CreateOrganisation("FCNR");
			var cneWithCFS = Helper.CreateOrganisation("FCNE");
			var cnrWithCNE = Helper.CreateOrganisation("RNE");
			var whs = Helper.CreateOrganisation("WHS");

			packCFS.OH_IsMiscFreightServices = true;
			packCFS.OH_IsPackDepot = true;
			unpackCFS.OH_IsMiscFreightServices = true;
			unpackCFS.OH_IsUnpackDepot = true;
			cneWithCFS.OH_IsMiscFreightServices = true;
			cneWithCFS.OH_IsUnpackDepot = true;
			cneWithCFS.OH_IsConsignee = true;
			cnrWithCFS.OH_IsMiscFreightServices = true;
			cnrWithCFS.OH_IsUnpackDepot = true;
			cnrWithCFS.OH_IsConsignor = true;
			cnrWithCNE.OH_IsConsignor = true;
			whs.OH_IsWarehouseClient = true;

			var address = Factory.New<JobDocAddress>();

			address.E2_OA_Address = packCFS.MainAddress.PK;
			address.DocAddressType = DocAddressType.LocalCartageExporter;
			AssertEquals(DocAddressType.LocalCartageExporter, ConsignmentHelper.GetInstructionDocAddressTypeToChangeTo(InstructionTypes.Codes.PickUp, address.DocAddressType));
			AssertEquals(DocAddressType.LocalCartageImporter, ConsignmentHelper.GetInstructionDocAddressTypeToChangeTo(InstructionTypes.Codes.Delivery, address.DocAddressType));
			AssertEquals(DocAddressType.LocalCartageExporter, ConsignmentHelper.GetInstructionDocAddressTypeToChangeTo(InstructionTypes.Codes.Multi, address.DocAddressType));
			AssertEquals(DocAddressType.LocalCartageImporter, ConsignmentHelper.GetInstructionDocAddressTypeToChangeTo(InstructionTypes.Codes.Multi, fallbackDocAddressType: DocAddressType.LocalCartageImporter));

			address.E2_OA_Address = unpackCFS.MainAddress.PK;
			AssertEquals(DocAddressType.LocalCartageExporter, ConsignmentHelper.GetInstructionDocAddressTypeToChangeTo(InstructionTypes.Codes.PickUp, address.DocAddressType));
			AssertEquals(DocAddressType.LocalCartageImporter, ConsignmentHelper.GetInstructionDocAddressTypeToChangeTo(InstructionTypes.Codes.Delivery, address.DocAddressType));
			AssertEquals(DocAddressType.LocalCartageExporter, ConsignmentHelper.GetInstructionDocAddressTypeToChangeTo(InstructionTypes.Codes.Multi, address.DocAddressType));
			AssertEquals(DocAddressType.LocalCartageImporter, ConsignmentHelper.GetInstructionDocAddressTypeToChangeTo(InstructionTypes.Codes.Multi, fallbackDocAddressType: DocAddressType.LocalCartageImporter));

			address.E2_OA_Address = cnrWithCFS.MainAddress.PK;
			AssertEquals(DocAddressType.LocalCartageExporter, ConsignmentHelper.GetInstructionDocAddressTypeToChangeTo(InstructionTypes.Codes.PickUp, address.DocAddressType));
			AssertEquals(DocAddressType.LocalCartageImporter, ConsignmentHelper.GetInstructionDocAddressTypeToChangeTo(InstructionTypes.Codes.Delivery, address.DocAddressType));
			AssertEquals(DocAddressType.LocalCartageExporter, ConsignmentHelper.GetInstructionDocAddressTypeToChangeTo(InstructionTypes.Codes.Multi, address.DocAddressType));
			AssertEquals(DocAddressType.LocalCartageImporter, ConsignmentHelper.GetInstructionDocAddressTypeToChangeTo(InstructionTypes.Codes.Multi, fallbackDocAddressType: DocAddressType.LocalCartageImporter));

			address.E2_OA_Address = cneWithCFS.MainAddress.PK;
			AssertEquals(DocAddressType.LocalCartageExporter, ConsignmentHelper.GetInstructionDocAddressTypeToChangeTo(InstructionTypes.Codes.PickUp, address.DocAddressType));
			AssertEquals(DocAddressType.LocalCartageImporter, ConsignmentHelper.GetInstructionDocAddressTypeToChangeTo(InstructionTypes.Codes.Delivery, address.DocAddressType));
			AssertEquals(DocAddressType.LocalCartageExporter, ConsignmentHelper.GetInstructionDocAddressTypeToChangeTo(InstructionTypes.Codes.Multi, address.DocAddressType));
			AssertEquals(DocAddressType.LocalCartageImporter, ConsignmentHelper.GetInstructionDocAddressTypeToChangeTo(InstructionTypes.Codes.Multi, fallbackDocAddressType: DocAddressType.LocalCartageImporter));

			address.E2_OA_Address = cnrWithCNE.MainAddress.PK;
			address.DocAddressType = DocAddressType.LocalCartageCFS;
			AssertEquals(DocAddressType.LocalCartageExporter, ConsignmentHelper.GetInstructionDocAddressTypeToChangeTo(InstructionTypes.Codes.PickUp, address.DocAddressType));
			AssertEquals(DocAddressType.LocalCartageImporter, ConsignmentHelper.GetInstructionDocAddressTypeToChangeTo(InstructionTypes.Codes.Delivery, address.DocAddressType));
			AssertEquals(DocAddressType.LocalCartageCFS, ConsignmentHelper.GetInstructionDocAddressTypeToChangeTo(InstructionTypes.Codes.Multi, address.DocAddressType));
			AssertEquals(DocAddressType.LocalCartageImporter, ConsignmentHelper.GetInstructionDocAddressTypeToChangeTo(InstructionTypes.Codes.Multi, fallbackDocAddressType: DocAddressType.LocalCartageImporter));

			address.E2_OA_Address = whs.MainAddress.PK;
			address.DocAddressType = DocAddressType.LocalCartageWarehouse;
			AssertEquals(DocAddressType.LocalCartageExporter, ConsignmentHelper.GetInstructionDocAddressTypeToChangeTo(InstructionTypes.Codes.PickUp, address.DocAddressType));
			AssertEquals(DocAddressType.LocalCartageImporter, ConsignmentHelper.GetInstructionDocAddressTypeToChangeTo(InstructionTypes.Codes.Delivery, address.DocAddressType));
			AssertEquals(DocAddressType.LocalCartageWarehouse, ConsignmentHelper.GetInstructionDocAddressTypeToChangeTo(InstructionTypes.Codes.Multi, address.DocAddressType));
			AssertEquals(DocAddressType.LocalCartageImporter, ConsignmentHelper.GetInstructionDocAddressTypeToChangeTo(InstructionTypes.Codes.Multi, fallbackDocAddressType: DocAddressType.LocalCartageImporter));
		}

		#endregion
	}
}
