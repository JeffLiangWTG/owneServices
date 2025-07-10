using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.TransportCommon.Shared;
using Enterprise.TransportConsignment.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Writing;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.TransportConsignment.DataTransfer.Universal.Testing
{
	class DtbConsignmentAddressDataObjectWriterTest : DtbConsignmentUniversalTestCase
	{
		#region TestPopulateConsignmentAction
		public void TestPopulateConsignmentAction()
		{
			var refEquipment = Factory.New<RefEquipment>();
			refEquipment.RQ_ShortCode = "TRUCK";
			var organisation = Factory.New<OrgHeader>();
			organisation.OH_FullName = "Some CO";
			var address = Factory.New<DtbConsignmentAddress>();
			address.LTS_Sequence = 2;
			address.LTS_InstructionType = InstructionTypes.Codes.PickUp;
			address.LTS_DropMode = Constants.LCLAIREquipmentNeeded.Premise;
			address.LTS_Status = "AVL";
			address.LTS_RQ_RequiredEquipment = refEquipment.PK;
			address.Address.OrganisationPK = organisation.PK;
			address.Actions.Add(Factory.New<DtbConsignmentAction>());
			address.Actions.Add(Factory.New<DtbConsignmentAction>());

			var actionDataObject = new DtbConsignmentAddressDataObjectWriter(new DataWritingManager(new DummyActionInfo()), new Dictionary<ZGuid, ZInt>()).GetDataObject(address);

			AssertEquals("actionDataObject.DropMode.Code", Constants.LCLAIREquipmentNeeded.Premise, actionDataObject.DropMode.Code);
			AssertEquals("actionDataObject.DropMode.Description", LCLAIREquipmentNeededList.Descriptions.Premise, actionDataObject.DropMode.Description);
			AssertEquals("actionDataObject.Type.Code", InstructionTypes.Codes.PickUp, actionDataObject.Type.Code);
			AssertEquals("actionDataObject.Type.Description", InstructionTypes.Descriptions.PickUp, actionDataObject.Type.Description);
			AssertEquals("actionDataObject.Sequence", 2, actionDataObject.Sequence);
			AssertEquals("actionDataObject.Equipment", "TRUCK", actionDataObject.Equipment);
			AssertEquals("actionDataObject.Status.Code", "AVL", actionDataObject.Status.Code);
			AssertEquals("actionDataObject.Status.Description", "Available", actionDataObject.Status.Description);
			AssertEquals("Some CO", actionDataObject.Address.CompanyName);
			AssertEquals(1, actionDataObject.InstructionPackingLineLinkCollection.Count);
			AssertEquals(2, actionDataObject.InstructionPackingLineLinkCollection[0].ConfirmationCollection.Count);
		}
		#endregion
	}
}
