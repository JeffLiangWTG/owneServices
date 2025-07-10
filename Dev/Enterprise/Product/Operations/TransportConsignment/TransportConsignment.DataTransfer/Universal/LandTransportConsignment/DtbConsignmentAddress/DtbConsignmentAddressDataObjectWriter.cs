using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.TransportConsignment.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.TransportConsignment.DataTransfer.Universal
{
	internal class DtbConsignmentAddressDataObjectWriter : DataObjectWriter<DtbConsignmentAddress, Instruction>
	{
		readonly Dictionary<ZGuid, ZInt> packageLinksDictionary;

		internal DtbConsignmentAddressDataObjectWriter(IDataWritingManager manager, Dictionary<ZGuid, ZInt> packageLinksDictionary) : base(manager)
		{
			this.packageLinksDictionary = packageLinksDictionary;
		}

		protected override Instruction PopulateDataObject(DtbConsignmentAddress address)
		{
			var instructionDataObject = new Instruction(writeManager.WriterStrategy);
			PopulateData(address, instructionDataObject);
			PopulateConsignmentAction(address, instructionDataObject);
			PopulateAddress(address, instructionDataObject);

			return instructionDataObject;
		}

		void PopulateConsignmentAction(DtbConsignmentAddress address, Instruction instructionDataObject)
		{
			if (!address.Actions.Any())
			{
				return;
			}

			instructionDataObject.SetInstructionPackingLineLinkCollection(() => GetInstructionPackingLineLinkCollection(address));
		}

		List<InstructionPackingLineLink> GetInstructionPackingLineLinkCollection(DtbConsignmentAddress address)
		{
			var consignmentActionDataObjectWriter = new DtbConsignmentActionDataObjectWriter(writeManager, packageLinksDictionary);

			return new[] {
				new InstructionPackingLineLink
				{
					ConfirmationCollection = ProcessCollection(address.Actions, consignmentActionDataObjectWriter, CollectionContent.Complete).ToList()
				}
			}.ToList();
		}

		static void PopulateData(DtbConsignmentAddress address, Instruction instructionDataObject)
		{
			instructionDataObject.DropMode = ListHelper.GetWithDescription<DropMode>(address.LTS_DropMode, address.Lookups.BindToLists.DropModes);
			instructionDataObject.Sequence = address.LTS_Sequence;
			instructionDataObject.Status = ListHelper.GetWithDescription<CodeDescriptionPair>(address.LTS_Status, address.Lookups.BindToLists.Statuses);
			instructionDataObject.Type = ListHelper.GetWithDescription<CodeDescriptionPair>(address.LTS_InstructionType, address.Lookups.BindToLists.InstructionTypes);

			var equipment = address.Factory.Load<RefEquipment>(address.LTS_RQ_RequiredEquipment);
			if (equipment != null)
			{
				instructionDataObject.Equipment = equipment.RQ_ShortCode;
			}
		}

		void PopulateAddress(DtbConsignmentAddress address, Instruction instructionDataObject)
		{
			instructionDataObject.Address = new JobDocAddressDataObjectWriter(writeManager).GetDataObject(address.Address);
		}
	}
}
