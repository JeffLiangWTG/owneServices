using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.Integration;
using Enterprise.Packing.DataTransfer.Universal;
using Enterprise.TransportBookings.Business;
using Enterprise.TransportCommon.DataTransfer.Universal;
using Enterprise.TransportCommon.Shared;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.TransportBookings.DataTransfer.Universal
{
	sealed class DtbBookingInstructionDataObjectReader : DataObjectReader<Instruction, DtbBookingInstruction>
	{
		public DtbBookingInstructionDataObjectReader(Instruction instructionDataObject, IXmlImportLogger logger, UniversalObjectFactory factory, DtbBooking parentBooking, PkgPackageJobDataObjectReader packageJobReader = null)
			: base(instructionDataObject, logger, factory)
		{
			this.booking = Argument.NotNull(parentBooking, "DtbBooking booking");
			PackageJobReader = packageJobReader;
		}

		readonly PkgPackageJobDataObjectReader PackageJobReader;

		IColumnIndexer Parent
		{
			get { return GetColumnIndexerFromRow(Booking); }
		}

		DtbBooking Booking
		{
			get { return booking; }
		}

		readonly DtbBooking booking;

		protected override DtbBookingInstruction GetExistingBusinessObject()
		{
			var query = new ZQuery();
			query.AddToFilter(DtbBookingInstructionSchema.KN_KM_BookingMovement, Parent[DtbBookingSchema.Constants.PK]);
			query.AddToFilter(DtbBookingInstructionSchema.KN_Sequence, dataObject.Sequence.GetValueOrDefault());
			return factory.Load<DtbBookingInstruction>(query).SingleOrDefault();
		}

		protected override void PopulateBusinessObject(DtbBookingInstruction instruction)
		{
			PopulateData(instruction);
			PopulateRelatedEntities(instruction);
		}

		void PopulateData(DtbBookingInstruction instruction)
		{
			var row = GetColumnIndexerFromRow(instruction);
			SetValue(row, DtbBookingInstructionSchema.KN_DropMode, dataObject.DropMode);
			SetValue(row, DtbBookingInstructionSchema.KN_InstructionType, dataObject.Type);
			SetValue(row, DtbBookingInstructionSchema.KN_IsContainerRateable, dataObject.IsContainerRateable);
			SetValue(row, DtbBookingInstructionSchema.KN_IsLooseRateable, dataObject.IsLooseRateable);
			SetValue(row, DtbBookingInstructionSchema.KN_IsAuthorisedToLeave, dataObject.IsAuthorisedToLeave);
			SetValue(row, DtbBookingInstructionSchema.KN_Sequence, dataObject.Sequence);
			SetValue(row, DtbBookingInstructionSchema.KN_ServiceInstruction, dataObject.ServiceInstruction);

			PopulateWorkflowCustomFields(instruction, dataObject);

			instruction.HasChanges = true;
		}

		void PopulateRelatedEntities(DtbBookingInstruction instruction)
		{
			PopulateEquipment(instruction);
			PopulateAddress(instruction);
			PopulatePackageDivots(instruction);
			PopulateConfirmations(instruction);
		}

		void PopulateEquipment(DtbBookingInstruction instruction)
		{
			var equipmentCode = dataObject.Equipment.GetValueOrDefault();
			var equipment = GetColumnIndexerFromRow(factory.RowFactory.LoadFromNaturalKey(RefEquipmentSchema.Constants.TableName, RefEquipmentSchema.RQ_ShortCode, equipmentCode, false));
			if (equipment != null)
			{
				var row = GetColumnIndexerFromRow(instruction);
				SetValue(row, DtbBookingInstructionSchema.KN_RQ_Equipment, equipment.GetValue(RefEquipmentSchema.PK));
			}
		}

		void PopulateAddress(DtbBookingInstruction instruction)
		{
			if (dataObject.Address != null)
			{
				var reader = new OrganisationDataObjectReader(dataObject.Address, logger, factory);
				var docAddressType = OrganisationDataObjectReader.GetDocAddressType(dataObject.Address.AddressType.GetValueOrDefault());
				var instructionAddress = dataObject.Address.HasTypeOnly()
					? null // If Address is empty it means no address was specified, but we want to Create an empty JobDocAddress to have an Address Type.
					: reader.GetMatched(Booking, docAddressType.GetOrganisationType(), docAddressType.GetOrganisationSubType());

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

		static void ReloadDocAddresses(DtbBookingInstruction instruction, JobDocAddress jobDocAddress)
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

		string GetDocAddressTypeCodeToSet(IColumnIndexer instruction, DocAddressType importedAddressType, OrgHeader organisation)
		{
			return DocAddressTypes.GetCode(factory.BOFactory, importedAddressType);
		}

		void PopulateConfirmations(DtbBookingInstruction instruction)
		{
			new InstructionConfirmationCollectionReader(this, instruction, ConfirmationsToExclude).ReadIntoCollection();
		}

		Confirmation[] ConfirmationsToExclude;

		void PopulatePackageDivots(DtbBookingInstruction instruction)
		{
			if (PackageJobReader != null
				&& (dataObject.InstructionContainerLinkCollection != null || dataObject.InstructionPackingLineLinkCollection != null))
			{
				instruction.PackageDivots.DeleteAll();

				var confirmationsToExclude = dataObject.GetConfirmationsThatApplyToAllPackagesOnly(PackageJobReader);
				ReadContainerDivots(instruction, confirmationsToExclude);
				ReadPackingLineDivots(instruction, confirmationsToExclude);
				ConfirmationsToExclude = confirmationsToExclude?.ToArray();
			}
		}

		void ReadContainerDivots(DtbBookingInstruction instruction, IEnumerable<Confirmation> confirmationsToExclude)
		{
			if (dataObject.InstructionContainerLinkCollection != null)
			{
				var containerDivotCollectionReader = new InstructionPkgDivotDataObjectCollectionReaderForContainer(this, instruction,
					dataObject.InstructionContainerLinkCollection.ToArray(), confirmationsToExclude);
				containerDivotCollectionReader.ReadIntoCollectionRetainingUnmatchedElements();
			}
		}

		void ReadPackingLineDivots(DtbBookingInstruction instruction, IEnumerable<Confirmation> confirmationsToExclude)
		{
			if (dataObject.InstructionPackingLineLinkCollection != null)
			{
				var packageDivotCollectionReader = new InstructionPkgDivotDataObjectCollectionReaderForPackage(this, instruction,
					dataObject.InstructionPackingLineLinkCollection.ToArray(), confirmationsToExclude);
				packageDivotCollectionReader.ReadIntoCollectionRetainingUnmatchedElements();
			}
		}

		abstract class InstructionPkgDivotDataObjectCollectionReader<T> : DataObjectCollectionReader<T, DtbBookingInstructionPkgDivot>
			where T : IConfirmationParentDivot
		{
			protected InstructionPkgDivotDataObjectCollectionReader(DtbBookingInstructionDataObjectReader reader,
				DtbBookingInstruction instruction, T[] instructionLinkDataObjects, IEnumerable<Confirmation> confirmationDataObjectsToExclude)
				: base(instructionLinkDataObjects)
			{
				this.reader = reader;
				Instruction = instruction;
				ConfirmationDataObjectsToExclude = confirmationDataObjectsToExclude;
			}

			readonly DtbBookingInstruction Instruction;
			protected readonly IEnumerable<Confirmation> ConfirmationDataObjectsToExclude;

			protected DtbBookingInstructionDataObjectReader Reader
			{
				get { return reader; }
			}

			readonly DtbBookingInstructionDataObjectReader reader;

			protected override DtbBookingInstructionPkgDivot[] BusinessObjects
			{
				get { return Instruction.PackageDivots.ToArray(); }
			}

			protected override DtbBookingInstructionPkgDivot FindMatchingBusinessObject(T instructionLinkDataObject)
			{
				return null;
			}

			protected override void AddToCollection(DtbBookingInstructionPkgDivot instructionPkgDivot)
			{
				var divotRow = GetColumnIndexerFromRow(instructionPkgDivot);
				Reader.SetValue(divotRow, DtbBookingInstructionPkgDivotSchema.KD_KN_BookingInstruction, Instruction.PK);

				foreach (var confirmation in instructionPkgDivot.ConfirmationsDivotOnly)
				{
					var confirmationRow = GetColumnIndexerFromRow(confirmation);
					Reader.SetValue(confirmationRow, DtbBookingConfirmationSchema.KK_KN_BookingInstruction, Instruction.PK);

					// confirmation now has access to it's address to calculate time zone for utc
					confirmation.SetKK_RequiredFromUtc();
					confirmation.SetKK_RequiredToUtc();
					confirmation.SetKK_EstimatedUtc();
				}
			}

			protected override void RemoveFromCollection(DtbBookingInstructionPkgDivot instructionPkgDivot)
			{
				Instruction.PackageDivots.Delete(instructionPkgDivot);
			}
		}

		class InstructionPkgDivotDataObjectCollectionReaderForContainer : InstructionPkgDivotDataObjectCollectionReader<InstructionContainerLink>
		{
			public InstructionPkgDivotDataObjectCollectionReaderForContainer(DtbBookingInstructionDataObjectReader reader, DtbBookingInstruction instruction,
				InstructionContainerLink[] instructionContainerLinkDataObjects, IEnumerable<Confirmation> confirmationDataObjectsToExclude)
				: base(reader, instruction, instructionContainerLinkDataObjects, confirmationDataObjectsToExclude)
			{
			}

			protected override DtbBookingInstructionPkgDivot ReadIntoBusinessObject(InstructionContainerLink instructionContainerLinkDataObject, DtbBookingInstructionPkgDivot instructionPkgDivot)
			{
				return new DtbBookingInstructionPkgDivotDataObjectReaderForContainer(instructionContainerLinkDataObject, Reader.logger,
					Reader.factory, Reader.PackageJobReader.PackageContainerLinks, ConfirmationDataObjectsToExclude).ReadIntoBusinessObject();
			}

			protected override bool SkipEntity(InstructionContainerLink instructionContainerLinkDataObject)
			{
				return !instructionContainerLinkDataObject.ContainerLink.HasValue || !Reader.PackageJobReader.PackageContainerLinks.ContainsKey(instructionContainerLinkDataObject.ContainerLink.Value);
			}
		}

		class InstructionPkgDivotDataObjectCollectionReaderForPackage : InstructionPkgDivotDataObjectCollectionReader<InstructionPackingLineLink>
		{
			public InstructionPkgDivotDataObjectCollectionReaderForPackage(DtbBookingInstructionDataObjectReader reader, DtbBookingInstruction instruction,
				InstructionPackingLineLink[] instructionPackageLinkDataObjects, IEnumerable<Confirmation> confirmationDataObjectsToExclude)
				: base(reader, instruction, instructionPackageLinkDataObjects, confirmationDataObjectsToExclude)
			{
			}

			protected override DtbBookingInstructionPkgDivot ReadIntoBusinessObject(InstructionPackingLineLink instructionPackageLinkDataObject, DtbBookingInstructionPkgDivot instructionPkgDivot)
			{
				return new DtbBookingInstructionPkgDivotDataObjectReaderForPackage(instructionPackageLinkDataObject, Reader.logger, Reader.factory,
					Reader.PackageJobReader.PackageLinks, ConfirmationDataObjectsToExclude).ReadIntoBusinessObject();
			}

			protected override bool SkipEntity(InstructionPackingLineLink instructionPackageLinkDataObject)
			{
				return !instructionPackageLinkDataObject.PackingLineLink.HasValue || !Reader.PackageJobReader.PackageLinks.ContainsKey(instructionPackageLinkDataObject.PackingLineLink.Value);
			}
		}

		class InstructionConfirmationCollectionReader : DataObjectCollectionReader<Confirmation, DtbBookingConfirmation>
		{
			public InstructionConfirmationCollectionReader(DtbBookingInstructionDataObjectReader instructionDataObjectReader,
				DtbBookingInstruction instructionToAddConfirmations, Confirmation[] confirmationDataObjects)
				: base(confirmationDataObjects)
			{
				InstructionDataObjectReader = instructionDataObjectReader;
				InstructionToAddConfirmations = instructionToAddConfirmations;
			}
			readonly DtbBookingInstructionDataObjectReader InstructionDataObjectReader;
			readonly DtbBookingInstruction InstructionToAddConfirmations;

			protected override void AddToCollection(DtbBookingConfirmation confirmation)
			{
				InstructionToAddConfirmations.Confirmations.Add(confirmation);
			}

			protected override DtbBookingConfirmation[] BusinessObjects
			{
				get { return InstructionToAddConfirmations.Confirmations.Where(i => i.PackageDivot == null).ToArray(); }
			}

			protected override DtbBookingConfirmation FindMatchingBusinessObject(Confirmation dataObject)
			{
				return null;
			}

			protected override DtbBookingConfirmation ReadIntoBusinessObject(Confirmation confirmationDataObject, DtbBookingConfirmation confirmation)
			{
				return new DtbBookingConfirmationDataObjectReader(confirmationDataObject, InstructionDataObjectReader.logger,
					InstructionDataObjectReader.factory, GetColumnIndexerFromRow(InstructionToAddConfirmations)).ReadIntoBusinessObject();
			}

			protected override void RemoveFromCollection(DtbBookingConfirmation confirmation)
			{
				InstructionToAddConfirmations.Confirmations.Delete(confirmation);
			}
		}
	}
}
