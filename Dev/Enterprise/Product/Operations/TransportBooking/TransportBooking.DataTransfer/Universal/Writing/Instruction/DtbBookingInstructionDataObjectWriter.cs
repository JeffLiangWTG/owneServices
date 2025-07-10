using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.Integration;
using Enterprise.TransportBookings.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.TransportBookings.DataTransfer.Universal
{
	public class DtbBookingInstructionDataObjectWriter : DataObjectWriter<DtbBookingInstruction, Instruction>
	{
		public DtbBookingInstructionDataObjectWriter(Dictionary<ZGuid, ZInt> linksDictionary, IDataWritingManager manager)
			: base(manager)
		{
			LinksDictionary = linksDictionary;
		}

		protected override Instruction PopulateDataObject(DtbBookingInstruction instruction)
		{
			Argument.NotNull(instruction, "instruction");
			var instructionDataObject = new Instruction(writeManager.WriterStrategy);

			PopulateData(instruction, instructionDataObject);
			PopulateRelatedEntities(instruction, instructionDataObject);

			return instructionDataObject;
		}

		static void PopulateData(DtbBookingInstruction instruction, Instruction instructionDataObject)
		{
			instructionDataObject.DropMode = ListHelper.GetWithDescription<DropMode>(instruction.KN_DropMode, instruction.Lookups.DropModes);

			if (instruction.Equipment != null)
			{
				instructionDataObject.Equipment = instruction.Equipment.RQ_ShortCode;
			}

			instructionDataObject.IsContainerRateable = instruction.KN_IsContainerRateable;
			instructionDataObject.IsLooseRateable = instruction.KN_IsLooseRateable;
			instructionDataObject.IsAuthorisedToLeave = instruction.KN_IsAuthorisedToLeave;
			instructionDataObject.Sequence = instruction.KN_Sequence;
			instructionDataObject.ServiceInstruction = instruction.KN_ServiceInstruction;
			instructionDataObject.Status = ListHelper.GetWithDescription<CodeDescriptionPair>(instruction.KN_Status, instruction.Lookups.BindToLists.Statuses);
			instructionDataObject.Type = ListHelper.GetWithDescription<CodeDescriptionPair>(instruction.KN_InstructionType, instruction.Lookups.InstructionTypes);
		}

		void PopulateRelatedEntities(DtbBookingInstruction instruction, Instruction instructionDataObject)
		{
			PopulateAddress(instruction, instructionDataObject);
			PopulateDtbBookingInstructionPkgDivots(instructionDataObject, instruction);
		}

		void PopulateAddress(DtbBookingInstruction instruction, Instruction instructionDataObject)
		{
			if (instruction.Address.IsEmpty)
			{
				instructionDataObject.Address = new OrganizationAddress { AddressType = instruction.Address.DocAddressType.ToString() };
			}
			else
			{
				instructionDataObject.Address = new JobDocAddressDataObjectWriter(writeManager)
				{
					PopulateGeoLocation = true,
					PopulateValidationStatus = true,
				}.GetDataObject(instruction.Address);
			}
		}

		void PopulateDtbBookingInstructionPkgDivots(Instruction instructionDataObject, DtbBookingInstruction instruction)
		{
			var divots = instruction.PackageDivots;
			if (divots.Any())
			{
				PopulateInstructionPackageLinks(instructionDataObject, divots);
			}
			else if (instruction.Confirmations.Any(i => i.PackageDivot == null))
			{
				CreateDummyDivotForInstructionLevelConfirmations(instructionDataObject, instruction);
			}
		}

		void PopulateInstructionPackageLinks(Instruction instructionDataObject, DtbBookingInstructionPkgDivotCollection divots)
		{
			foreach (var instructionPkgDivot in divots)
			{
				if (instructionPkgDivot.Package.IsContainer)
				{
					PopulateInstructionContainerLink(instructionDataObject, instructionPkgDivot, instructionPkgDivot.Confirmations);
				}
				else
				{
					PopulateInstructionPackingLineLink(instructionDataObject, instructionPkgDivot, instructionPkgDivot.Confirmations);
				}
			}
		}

		void CreateDummyDivotForInstructionLevelConfirmations(Instruction instructionDataObject, DtbBookingInstruction instruction)
		{
			instructionDataObject.SetInstructionPackingLineLinkCollection(() =>
			{
				var instructionPackingLineLinkDataObject = new InstructionPackingLineLink();
				instructionPackingLineLinkDataObject.ConfirmationCollection = ProcessCollection(instruction.Confirmations.Where(i => i.PackageDivot == null),
					new DtbBookingConfirmationDataObjectWriter(writeManager));
				instructionPackingLineLinkDataObject.Quantity = 0;
				var result = instructionDataObject.InstructionPackingLineLinkCollection ?? new List<InstructionPackingLineLink>();
				result.Add(instructionPackingLineLinkDataObject);
				return result;
			});
		}

		void PopulateInstructionContainerLink(Instruction instructionDataObject, DtbBookingInstructionPkgDivot instructionPkgDivot,
			DtbBookingConfirmationCollection confirmationCollection)
		{
			instructionDataObject.SetInstructionContainerLinkCollection(() =>
			{
				var instructionContainerLinkDataObject = new InstructionContainerLink();

				if (LinksDictionary.ContainsKey(instructionPkgDivot.KD_KP_Package))
				{
					instructionContainerLinkDataObject.ContainerLink = LinksDictionary[instructionPkgDivot.KD_KP_Package];
				}

				instructionContainerLinkDataObject.Quantity = instructionPkgDivot.KD_Quantity;
				instructionContainerLinkDataObject.ConfirmationCollection = ProcessCollection(confirmationCollection,
					new DtbBookingConfirmationDataObjectWriter(writeManager, instructionPkgDivot));

				var result = instructionDataObject.InstructionContainerLinkCollection ?? new List<InstructionContainerLink>();
				result.Add(instructionContainerLinkDataObject);
				return result;
			});
		}

		void PopulateInstructionPackingLineLink(Instruction instructionDataObject, DtbBookingInstructionPkgDivot instructionPkgDivot,
			DtbBookingConfirmationCollection confirmationCollection)
		{
			instructionDataObject.SetInstructionPackingLineLinkCollection(() =>
			{
				var instructionPackingLineLinkDataObject = new InstructionPackingLineLink();

				if (LinksDictionary.ContainsKey(instructionPkgDivot.KD_KP_Package))
				{
					instructionPackingLineLinkDataObject.PackingLineLink = LinksDictionary[instructionPkgDivot.KD_KP_Package];
				}

				instructionPackingLineLinkDataObject.Quantity = instructionPkgDivot.KD_Quantity;
				instructionPackingLineLinkDataObject.ConfirmationCollection = ProcessCollection(confirmationCollection,
					new DtbBookingConfirmationDataObjectWriter(writeManager, instructionPkgDivot));

				if (instructionDataObject.InstructionPackingLineLinkCollection == null)
				{
					instructionDataObject.SetInstructionPackingLineLinkCollection(() => new List<InstructionPackingLineLink>());
				}
				var result = instructionDataObject.InstructionPackingLineLinkCollection ?? new List<InstructionPackingLineLink>();
				result.Add(instructionPackingLineLinkDataObject);
				return result;
			});
		}

		public Dictionary<ZGuid, ZInt> LinksDictionary
		{
			get { return linksDictionary ?? (linksDictionary = new Dictionary<ZGuid, ZInt>()); }
			private set { linksDictionary = value; }
		}

		Dictionary<ZGuid, ZInt> linksDictionary;

		protected override IEnumerable<IPropertyValue> GetUserDefinedValues(DtbBookingInstruction sourceBO)
		{
			return sourceBO.GetUserDefinedValues();
		}
	}
}
