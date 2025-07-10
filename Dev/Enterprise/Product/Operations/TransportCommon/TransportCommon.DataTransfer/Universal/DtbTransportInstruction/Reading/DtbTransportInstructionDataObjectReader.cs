using System;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.Integration;
using Enterprise.Packing.DataTransfer.Universal;
using Enterprise.TransportCommon.Business;
using Enterprise.TransportCommon.Shared;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.TransportCommon.DataTransfer.Universal
{
	public abstract class DtbTransportInstructionDataObjectReader<TInstruction> : DataObjectReader<Instruction, TInstruction>
		where TInstruction : DtbTransportInstruction
	{
		protected DtbTransportInstructionDataObjectReader(Instruction instructionDataObject, IXmlImportLogger logger, UniversalObjectFactory factory, DtbTransport transport, PkgPackageJobDataObjectReader packageJobReader = null)
			: base(instructionDataObject, logger, factory)
		{
			this.transport = Argument.NotNull(transport, "DtbTransport transport");
			PackageJobReader = packageJobReader;
		}

		protected readonly PkgPackageJobDataObjectReader PackageJobReader;

		protected IColumnIndexer Parent
		{
			get { return GetColumnIndexerFromRow(Transport); }
		}

		protected DtbTransport Transport
		{
			get { return transport; }
		}

		readonly DtbTransport transport;

		#region PopulateBusinessObject

		protected override void PopulateBusinessObject(TInstruction instruction)
		{
			PopulateData(instruction);
			PopulateRelatedEntities(instruction);
		}

		#region PopulateData

		void PopulateData(TInstruction instruction)
		{
			var row = GetColumnIndexerFromRow(instruction);
			SetValue(row, DtbBookingInstructionSchema.KN_DropMode, dataObject.DropMode);
			SetValue(row, DtbBookingInstructionSchema.KN_InstructionType, dataObject.Type);
			SetValue(row, DtbBookingInstructionSchema.KN_IsContainerRateable, dataObject.IsContainerRateable);
			SetValue(row, DtbBookingInstructionSchema.KN_IsLooseRateable, dataObject.IsLooseRateable);
			SetValue(row, DtbBookingInstructionSchema.KN_IsAuthorisedToLeave, dataObject.IsAuthorisedToLeave);

			if (PopulateSequence)
			{
				SetValue(row, DtbBookingInstructionSchema.KN_Sequence, dataObject.Sequence);
			}

			SetValue(row, DtbBookingInstructionSchema.KN_ServiceInstruction, dataObject.ServiceInstruction);

			PopulateDataCore(instruction);

			instruction.HasChanges = true;
		}

		protected virtual bool PopulateSequence
		{
			get { return true; }
		}

		protected virtual void PopulateDataCore(TInstruction instruction)
		{
		}

		#endregion

		#region PopulateRelatedEntities

		void PopulateRelatedEntities(TInstruction instruction)
		{
			PopulateEquipment(instruction);
			PopulateAddress(instruction);
			PopulatePackageDivots(instruction);
			PopulateConfirmations(instruction);
		}

		#region PopulateEquipment

		void PopulateEquipment(TInstruction instruction)
		{
			var equipmentCode = dataObject.Equipment.GetValueOrDefault();
			var equipment = GetColumnIndexerFromRow(factory.RowFactory.LoadFromNaturalKey(RefEquipmentSchema.Constants.TableName, RefEquipmentSchema.RQ_ShortCode, equipmentCode, false));
			if (equipment != null)
			{
				var row = GetColumnIndexerFromRow(instruction);
				SetValue(row, DtbBookingInstructionSchema.KN_RQ_Equipment, equipment.GetValue(RefEquipmentSchema.PK));
			}
		}

		#endregion

		#region PopulateAddress

		void PopulateAddress(TInstruction instruction)
		{
			if (dataObject.Address != null)
			{
				var reader = new OrganisationDataObjectReader(dataObject.Address, logger, factory);
				var docAddressType = OrganisationDataObjectReader.GetDocAddressType(dataObject.Address.AddressType.GetValueOrDefault());
				var instructionAddress = dataObject.Address.HasTypeOnly()
					? null // If Address is empty it means no address was specified, but we want to Create an empty JobDocAddress to have an Address Type.
					: reader.GetMatched(Transport, docAddressType.GetOrganisationType(), docAddressType.GetOrganisationSubType());

				var instructionRow = GetColumnIndexerFromRow(instruction);
				DeleteExistingJobDocAddresses(instructionRow);

				var jobDocAddress = factory.New<JobDocAddress>(); // required for PopulateJobDocAddress()
				PopulateJobDocAddressData(docAddressType, instructionAddress, instructionRow, jobDocAddress);
				ReloadDocAddresses(instruction, jobDocAddress);

				if (instructionAddress != null || !dataObject.Address.HasTypeOnly())
				{
					// currently the below uses the JobDocAddress bizO setters which invokes OnDocAddressChanged
					// which can incorrectly change values or calculate things we don't want, so we suspend the
					// behaviour. This can be removed once Universal does not use bizOs.
					using (instruction.SuspendOnDocAddressChanged())
					{
						reader.PopulateJobDocAddress(instructionAddress, jobDocAddress);
					}
				}
				else
				{
					jobDocAddress.MakePersistentEvenIfEmpty(); // We want Job Doc Address to be saved even if it was empty
				}

				PopulateAddressCore(instructionRow, jobDocAddress);
			}
		}

		void DeleteExistingJobDocAddresses(IColumnIndexer instructionRow)
		{
			var existingDocAddresses = factory.Load<JobDocAddress>(new ZQuery(JobDocAddressSchema.E2_ParentID, instructionRow.GetValue(DtbBookingInstructionSchema.PK)));

			foreach (var existingJobDocAddress in existingDocAddresses)
			{
				existingJobDocAddress.HasChanges = true; // for proper delete, see WIx, once core fixes we should be able to remove this hack
			}

			instructionRow.DeleteAllJobDocAddresses(factory.RowFactory, DtbBookingInstructionSchema.PK);
		}

		void PopulateJobDocAddressData(DocAddressType docAddressType, OrgAddress instructionAddress, IColumnIndexer instructionRow, JobDocAddress jobDocAddress)
		{
			var row = GetColumnIndexerFromRow(jobDocAddress);
			SetValue(row, JobDocAddressSchema.E2_ParentID, instructionRow.GetValue(DtbBookingInstructionSchema.PK));
			SetValue(row, JobDocAddressSchema.E2_ParentTableCode, DtbBookingInstructionSchema.Constants.Prefix);

			var organisation = instructionAddress != null ? instructionAddress.Header : null; // needed for figuring out Address Type
			var addressType = GetDocAddressTypeCodeToSet(instructionRow, docAddressType, organisation);
			SetValue(row, JobDocAddressSchema.E2_AddressType, addressType);
		}

		static void ReloadDocAddresses(TInstruction instruction, JobDocAddress jobDocAddress)
		{
			// Unfortunately some of the Business Code during the Universal Import requires that the DocAddresses collection have
			// the new JobDocAddress. This Collection is not Active so it will not update automatically, therefore add it manually.
			var docAddresses = instruction.DocAddresses;
			if (!docAddresses.Contains(jobDocAddress))
			{
				docAddresses.Load();

				if (!docAddresses.Contains(jobDocAddress))
				{
					throw new InvalidOperationException("Could not add JobDocAddress to Instruction.");
				}
			}
		}

		protected virtual string GetDocAddressTypeCodeToSet(IColumnIndexer instruction, DocAddressType importedAddressType, OrgHeader organisation)
		{
			return DocAddressTypes.GetCode(factory.BOFactory, importedAddressType);
		}

		protected virtual void PopulateAddressCore(IColumnIndexer instruction, JobDocAddress jobDocAddress)
		{
		}

		#endregion

		#region PopulatePackageDivots

		protected abstract void PopulatePackageDivots(TInstruction instruction);

		#endregion

		#region PopulateConfirmations

		protected abstract void PopulateConfirmations(TInstruction instruction);

		protected IEnumerable<Confirmation> GetCommonConfirmationsInContainerAndPackingLineDivots()
		{
			return dataObject.GetConfirmationsThatApplyToAllPackagesOnly(PackageJobReader);
		}

		#endregion

		#endregion

		#endregion
	}
}
