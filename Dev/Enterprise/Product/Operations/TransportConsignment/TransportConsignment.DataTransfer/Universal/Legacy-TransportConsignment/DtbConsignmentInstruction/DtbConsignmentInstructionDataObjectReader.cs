using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.Integration;
using Enterprise.Packing.DataTransfer.Universal;
using Enterprise.Rating.Business;
using Enterprise.TransportCommon.DataTransfer.Universal;
using Enterprise.TransportCommon.Shared;
using Enterprise.TransportConsignment.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.TransportConsignment.DataTransfer.Universal
{
	class DtbConsignmentInstructionDataObjectReader : DtbTransportInstructionDataObjectReader<DtbConsignmentInstruction>
	{
		public DtbConsignmentInstructionDataObjectReader(Instruction instructionDataObject, IXmlImportLogger logger, UniversalObjectFactory factory, DtbBookingConsignment consignment, PkgPackageJobDataObjectReader packageJobReader = null)
			: base(instructionDataObject, logger, factory, consignment, packageJobReader)
		{
		}

		#region Matching

		protected override DtbConsignmentInstruction GetExistingBusinessObject()
		{
			DtbConsignmentInstruction result = null;

			// we want to update existing Instructions since Consignments will have only 1 Pick Up and 1 Delivery
			var instructionType = dataObject.Type.GetCodeAsUpperCase();
			if (instructionType != InstructionTypes.Codes.Multi)
			{
				var consignmentRow = GetColumnIndexerFromRow(Transport);
				var query = new ZQuery();
				query.AddToFilter(DtbBookingInstructionSchema.KN_InstructionType, instructionType);
				query.AddToFilter(DtbBookingInstructionSchema.KN_KM_BookingMovement, consignmentRow.GetValue(DtbBookingSchema.PK));

				var existingInstructions = factory.Load<DtbConsignmentInstruction>(query);
				result = existingInstructions.Length == 1 ? existingInstructions[0] : null;
			}

			return result;
		}

		#endregion

		#region PopulateBusinessObject

		protected override bool PopulateSequence
		{
			get { return false; }
		}

		#region PopulateAddress

		protected override void PopulateAddressCore(IColumnIndexer instruction, JobDocAddress jobDocAddress)
		{
			base.PopulateAddressCore(instruction, jobDocAddress);

			var location = LocationHelper.GetLocationFromIDocAddress(jobDocAddress, factory.BOFactory);
			var row = GetColumnIndexerFromRow(jobDocAddress);

			ZString countryCode = ZString.Empty;
			ZString postCode = ZString.Empty;
			ZString city = ZString.Empty;

			if (row.GetValue(JobDocAddressSchema.E2_AddressOverride))
			{
				countryCode = row.GetValue(JobDocAddressSchema.E2_RN_NKCountryCode);
				postCode = row.GetValue(JobDocAddressSchema.E2_Postcode);
				city = row.GetValue(JobDocAddressSchema.E2_City);
			}
			else
			{
				var addressRow = GetAddressRowFromJobDocAddress(row);
				if (addressRow != null)
				{
					countryCode = addressRow.GetValue(OrgAddressSchema.OA_RN_NKCountryCode);
					postCode = addressRow.GetValue(OrgAddressSchema.OA_PostCode);
					city = addressRow.GetValue(OrgAddressSchema.OA_City);
				}
			}

			var zone = RateTransportZone.GetOperationZone(factory.BOFactory, null, location, countryCode, null, postCode, city);
			if (zone != null)
			{
				SetValue(instruction, DtbBookingInstructionSchema.KN_TZ_DomesticZone, zone.PK);
			}
		}

		IColumnIndexer GetAddressRowFromJobDocAddress(IColumnIndexer jobDocAddress)
		{
			var orgAddressPK = jobDocAddress.GetValue(JobDocAddressSchema.E2_OA_Address);
			var orgAddress = factory.RowFactory.LoadFromPK(OrgAddressSchema.Constants.TableName, orgAddressPK);
			return GetColumnIndexerFromRow(orgAddress);
		}

		protected override string GetDocAddressTypeCodeToSet(IColumnIndexer instruction, DocAddressType importedAddressType, OrgHeader organisation)
		{
			var instructionDocAddressType = ConsignmentHelper.GetInstructionDocAddressTypeToChangeTo(instruction.GetValue(DtbBookingInstructionSchema.KN_InstructionType), importedAddressType);
			return DocAddressTypes.GetCode(factory.BOFactory, instructionDocAddressType);
		}

		#endregion

		#region PopulateConfirmations

		protected override void PopulateConfirmations(DtbConsignmentInstruction instruction)
		{
			// we do not want override existing confirmations if the user has already entered data for them on the Consignment side.
			if (IsNewBO)
			{
				ReadConfirmationCollection(instruction);
			}

			AddConfirmationIfInstructionHasNone(instruction);
		}

		void ReadConfirmationCollection(DtbConsignmentInstruction instruction)
		{
			var confirmations = GetCommonConfirmationsInContainerAndPackingLineDivots();
			if (confirmations != null && confirmations.Any())
			{
				new ConfirmationDataObjectCollectionReader(this, instruction, confirmations.ToArray()).ReadIntoCollection();
			}
		}

		void AddConfirmationIfInstructionHasNone(DtbConsignmentInstruction instruction)
		{
			var instructionPK = GetColumnIndexerFromRow(instruction).GetValue(DtbBookingInstructionSchema.PK);
			var query = new ZQuery(DtbBookingConfirmationSchema.KK_KN_BookingInstruction, instructionPK);
			if (factory.RowFactory.Load(DtbBookingConfirmationSchema.Constants.TableName, query).Length == 0)
			{
				var confirmation = factory.RowFactory.NewRowWithPK(DtbBookingConfirmationSchema.Instance);
				var utcNow = ZDateTime.UtcNow;
				SetValue(confirmation, DtbBookingConfirmationSchema.KK_KN_BookingInstruction, instructionPK);
				SetValue(confirmation, DtbBookingConfirmationSchema.KK_ConfirmationType, dataObject.Type);
				SetValue(confirmation, DtbBookingConfirmationSchema.KK_SystemCreateTimeUtc, utcNow);
				SetValue(confirmation, DtbBookingConfirmationSchema.KK_SystemCreateUser, GlbStaff.CurrentUser.GS_Code);
				SetValue(confirmation, DtbBookingConfirmationSchema.KK_SystemLastEditTimeUtc, utcNow);
				SetValue(confirmation, DtbBookingConfirmationSchema.KK_SystemLastEditUser, GlbStaff.CurrentUser.GS_Code);
			}
		}

		#endregion

		#region PopulatePackageDivots

		protected override void PopulatePackageDivots(DtbConsignmentInstruction instruction)
		{
			new InstructionDivotHelper(dataObject, logger, factory).PopulatePackageDivots(GetColumnIndexerFromRow(instruction), Parent);
		}

		#endregion

		#region ConfirmationDataObjectCollectionReader

		class ConfirmationDataObjectCollectionReader : DataObjectCollectionReader<Confirmation, DtbConsignmentConfirmation>
		{
			public ConfirmationDataObjectCollectionReader(DtbConsignmentInstructionDataObjectReader reader,
				IColumnIndexer instruction, Confirmation[] confirmationDataObjects)
				: base(confirmationDataObjects)
			{
				Reader = reader;
				Instruction = instruction;
			}

			readonly DtbConsignmentInstructionDataObjectReader Reader;
			readonly IColumnIndexer Instruction;

			protected override void AddToCollection(DtbConsignmentConfirmation confirmation)
			{
				var row = GetColumnIndexerFromRow(confirmation);
				Reader.SetValue(row, DtbBookingConfirmationSchema.KK_KN_BookingInstruction, Instruction.GetValue(DtbBookingInstructionSchema.PK));
			}

			protected override DtbConsignmentConfirmation[] BusinessObjects
			{
				get
				{
					var instructionConfirmationsQuery = new ZQuery(DtbBookingConfirmationSchema.KK_KN_BookingInstruction, Instruction[DtbBookingInstructionSchema.Constants.PK]);
					instructionConfirmationsQuery.AddToFilter(DtbBookingConfirmationSchema.KK_KD_BookingInstructionPkgDivot, DBNull.Value);

					return Reader.factory.Load<DtbConsignmentConfirmation>(instructionConfirmationsQuery);
				}
			}

			protected override DtbConsignmentConfirmation FindMatchingBusinessObject(Confirmation confirmationDataObject)
			{
				return null;
			}

			protected override DtbConsignmentConfirmation ReadIntoBusinessObject(Confirmation confirmationDataObject, DtbConsignmentConfirmation confirmation)
			{
				return new DtbConsignmentConfirmationDataObjectReader(confirmationDataObject, Reader.logger, Reader.factory, Instruction).ReadIntoBusinessObject();
			}

			protected override void RemoveFromCollection(DtbConsignmentConfirmation confirmation)
			{
				GetColumnIndexerFromRow(confirmation).DeleteRowAndSetHasChanges(confirmation);
			}

			protected override bool SkipEntity(Confirmation dataObject)
			{
				return dataObject.DateDescription.GetValueOrDefault().EqualsIgnoringCase(ConfirmationTypes.Codes.ConNoteNo);
			}
		}

		#endregion

		#endregion
	}
}


