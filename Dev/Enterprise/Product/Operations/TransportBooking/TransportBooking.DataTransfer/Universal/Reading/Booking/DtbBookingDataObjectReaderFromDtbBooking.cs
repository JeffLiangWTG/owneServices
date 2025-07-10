using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.Messaging.Integration;
using Enterprise.Packing.DataTransfer.Universal;
using Enterprise.TransportBookings.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.TransportBookings.DataTransfer.Universal
{
	class DtbBookingDataObjectReaderFromDtbBooking : DtbBookingDataObjectReader
	{
		public DtbBookingDataObjectReaderFromDtbBooking(UniversalShipment bookingDataObject, IXmlImportLogger logger, UniversalObjectFactory factory, DtbBookingConsolidation consolidation, UniversalShipment topLevelDO, DtbBooking booking = null, PkgPackageJobDataObjectReader packageJobReader = null)
			: base(bookingDataObject, logger, factory, consolidation, topLevelDO)
		{
			PackageJobReader = packageJobReader;
			Booking = booking;
		}

		readonly PkgPackageJobDataObjectReader PackageJobReader;
		readonly DtbBooking Booking;

		protected override DtbBooking GetExistingBusinessObjectUsingModuleSpecificBusinessRules()
		{
			return Booking;
		}

		protected override void BeforePopulateBusinessObject(DtbBooking booking)
		{
			base.BeforePopulateBusinessObject(booking);
			MarkAsAgentBookingForCBABilledTB(booking);
		}

		void MarkAsAgentBookingForCBABilledTB(DtbBooking booking)
		{
			var consolSingle = booking.ConsolidationSingleJob;
			if (consolSingle == null || consolSingle.KB_ParentTableCode != ZString.Empty || consolSingle.KB_ParentID != ZGuid.Empty)
			{
				return;
			}
			(var interchangeType, var sender) = DtbDataObjectReaderHelper.GetInterchangeTypeAndSenderFromLogger(logger);
			var isCbaBilled = ((TopLevelDataObject)logger.TopLevelDataObject).HasRecipientRoleAndService(RecipientRoleType.TPC, ServiceCodeType.CBA);
			var isSenderAuthorised = interchangeType == EDIInterchangeTransportTypeList.Codes.eHub && DtbAgentBooking.IsAuthorisedCarrierBookingAgent(sender);
			if (isCbaBilled && isSenderAuthorised || DtbDataObjectReaderHelper.MessageIsFromTestCba(sender, logger.TopLevelDataObject as TopLevelDataObject))
			{
				MarkAsAgentBooking(booking);
			}
			else if (isCbaBilled && !isSenderAuthorised)
			{
				logger.Log(Integration.LogType.Warning, Res.GetString("5418D99A-268C-4402-998C-69621F0A4F97", "The sender is not authorized to use the service code \"{0}\".", ServiceCodeType.CBA));
			}
		}

		void MarkAsAgentBooking(DtbBooking booking)
		{
			booking.KM_IsAgentBooking = true;
		}

		protected override DtbBooking GetNewBusinessObject()
		{
			return Consolidation.Bookings.AddNew();
		}

		protected override void SetTemplateCode(DtbBooking booking, ZString? templateCode)
		{
			using (new SemaphoreManager(booking.AddingInstructionsFromTemplateSemaphore))
			{
				base.SetTemplateCode(booking, templateCode);
			}
		}

		protected override void PopulateRelatedEntitiesCore(DtbBooking booking)
		{
			PopulateInstructions(booking);
		}

		void PopulateInstructions(DtbBooking booking)
		{
			if (dataObject.InstructionCollection != null)
			{
				var instructionCollectionReader = new InstructionDataObjectCollectionReader(this, booking, dataObject.InstructionCollection);
				instructionCollectionReader.ReadIntoCollection();
			}
		}

		public class InstructionDataObjectCollectionReader : DataObjectCollectionReader<Instruction, DtbBookingInstruction>
		{
			public InstructionDataObjectCollectionReader(DtbBookingDataObjectReaderFromDtbBooking reader, DtbBooking booking, DataObjectList<Instruction> instructionDataObjects)
				: base(instructionDataObjects)
			{
				Reader = reader;
				Booking = booking;
			}

			readonly DtbBookingDataObjectReaderFromDtbBooking Reader;
			readonly DtbBooking Booking;

			protected override void AddToCollection(DtbBookingInstruction instruction)
			{
				var row = GetColumnIndexerFromRow(instruction);
				Reader.SetValue(row, DtbBookingInstructionSchema.KN_KM_BookingMovement, Booking.PK);
			}

			protected override DtbBookingInstruction[] BusinessObjects
			{
				get { return Booking.Instructions.ToArray(); }
			}

			protected override DtbBookingInstruction FindMatchingBusinessObject(Instruction instructionDataObject)
			{
				return null;
			}

			protected override DtbBookingInstruction ReadIntoBusinessObject(Instruction instructionDataObject, DtbBookingInstruction instruction)
			{
				return new DtbBookingInstructionDataObjectReader(instructionDataObject, Reader.logger, Reader.factory, Booking, Reader.PackageJobReader).ReadIntoBusinessObject();
			}

			protected override void RemoveFromCollection(DtbBookingInstruction instruction)
			{
				Booking.Instructions.Delete(instruction);
			}
		}

		protected override bool LogChildTopLevelObjectsOnImport
		{
			get { return Booking == null; }
		}
	}
}
