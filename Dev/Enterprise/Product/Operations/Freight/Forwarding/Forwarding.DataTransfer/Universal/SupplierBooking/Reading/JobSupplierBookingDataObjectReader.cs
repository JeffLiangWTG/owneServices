using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;
using UniversalDateType = Enterprise.UniversalDataBuss.DataObjects.Universal.DateType;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Freight.Forwarding.DataTransfer
{
	internal class JobSupplierBookingDataObjectReader : ShipmentDataObjectReader<JobSupplierBooking>
	{
		public JobSupplierBookingDataObjectReader(UniversalShipment dataObject, IXmlImportLogger logger, UniversalObjectFactory factory)
			: base(dataObject, logger, factory)
		{
		}

		public override DataContextType DataContextType => DataContextType.JobSupplierBooking;

		protected override IMatchingBusinessEntityFinder<JobSupplierBooking> GetCombinedReferenceMatcher() => null;

		protected override JobSupplierBooking GetExistingBusinessObjectUsingModuleSpecificBusinessRules() => null;

		protected override ZString GetReasonForNotAbleToUpdateFromDataSourceOrTargetBO(JobSupplierBooking targetBO)
		{
			if (!IsNewBO && targetBO != null && targetBO.ReadOnlyWhileImporting)
			{
				throw new DataObjectReadFailureException(Res.GetString("da4613be-18b0-428c-a206-5da180ee9e01", "Supplier booking in {0} state cannot be updated.", targetBO.JSB_Status));
			}

			var builder = new ZStringBuilder(base.GetReasonForNotAbleToUpdateFromDataSourceOrTargetBO(targetBO));

			if (dataObject.PortOfDischarge?.Code?.IsEmpty ?? true)
			{
				builder.AppendLine(Res.GetString("7e11fb3f-1190-475f-bfea-91354f37fc57", "Discharge Port should be not empty"));
			}

			if (dataObject.PortOfLoading?.Code?.IsEmpty ?? true)
			{
				builder.AppendLine(Res.GetString("3b2dd46b-2656-4fa1-a97e-c0b2363751dc", "Loading Port should be not empty"));
			}

			if ((dataObject.OrganizationAddressCollection?.Count ?? 0) == 0)
			{
				builder.AppendLine(Res.GetString("56204162-dbb5-4973-a7a1-560f4103b745", "There must be one organization at least"));
			}

			if (dataObject.LoadMode?.Code?.IsEmpty ?? true)
			{
				builder.AppendLine(Res.GetString("228da312-b6fe-404d-9cff-5c45d6bc7d1d", "Load Mode should not empty"));
			}
			else if (SupplierBookingLoadModeList.Codes.CY != dataObject.LoadMode.Code.Value && SupplierBookingLoadModeList.Codes.CFS != dataObject.LoadMode.Code.Value)
			{
				builder.AppendLine(Res.GetString("554fdc97-e3ce-4a38-8113-6c74186b3555", "Load Mode only accepts CY or CFS"));
			}

			var bookingPartyAddress = JobSupplierBookingDataObjectHelper.FindByDocAddressTypeAndGetMatched(dataObject, DocAddressType.BookingPartyDocumentaryAddress, logger, factory);
			if (bookingPartyAddress == null)
			{
				builder.AppendLine(Res.GetString("771c4591-214e-45e1-8e2c-bf3bd04b3a15", "There is no matched Booking Party"));
			}

			return builder.ToString();
		}

		protected override JobSupplierBooking GetNewBusinessObject()
		{
			var result = base.GetNewBusinessObject();
			result.JSB_LoadMode = SupplierBookingLoadModeList.Codes.CY;
			return result;
		}

		protected override void PopulateBusinessObject(JobSupplierBooking targetBO)
		{
			using (SuspendDataImporting(targetBO))
			{
				PopulateBusinessObjectCore(targetBO);
				PopulateCustomFields(targetBO);
				PopulateContainers(targetBO);
				PopulateDates(targetBO);
				PopulateOrganisations(targetBO);
				PopulateBusinessObjectSupplierBookingLines(targetBO);
				PopulateNotes(targetBO);
			}
		}

		void PopulateNotes(JobSupplierBooking targetBO)
		{
			if (dataObject.NoteCollection != null)
			{
				var noteTypesToSkip = new List<ZString> { };

				var noteTypesWithEmptyText = dataObject.NoteCollection
					.Where(n => n.NoteText.GetValueOrDefault().Trim().IsEmpty)
					.Select(n => n.Description.GetValueOrDefault()).ToArray();
				if (noteTypesWithEmptyText.Any())
				{
					var message = Res.GetString("343fd95c-62b2-467f-8d39-9730811c02b6",
						"Cannot import the following notes with empty text:{0}{1}",
						System.Environment.NewLine,
						ZString.Join(",", noteTypesWithEmptyText));
					logger.LogBoth(LogType.Warning, message);

					noteTypesToSkip.AddRange(noteTypesWithEmptyText);
				}

				new NotesCollectionReader(dataObject.NoteCollection, logger, factory, targetBO, noteTypesToSkip).ReadIntoCollection();
			}

			if (targetBO.JSB_DetailedGoodsDescription.IsEmpty)
			{
				targetBO.JSB_DetailedGoodsDescription = ZString.Empty;
			}
		}

		DisposableAction SuspendDataImporting(JobSupplierBooking targetBO)
		{
			ISupportDataImporting iSupportDataImporting = targetBO;

			return new DisposableAction(
				() => iSupportDataImporting.IsImportingData = true,
				() => iSupportDataImporting.IsImportingData = false);
		}

		void PopulateBusinessObjectSupplierBookingLines(JobSupplierBooking targetBO)
		{
			if ((dataObject.SubShipmentCollection?.Count ?? 0) == 0)
			{
				return;
			}
			targetBO.PopulateBookingIdIfNeeded();
			foreach (var bookingLineData in dataObject.SubShipmentCollection)
			{
				var supplierBookingLine = new JobSupplierBookingLineDataObjectReader(bookingLineData, targetBO, logger, factory).ReadIntoBusinessObject();
				if (supplierBookingLine != null)
				{
					targetBO.SupplierBookingLines.Add(supplierBookingLine);
					supplierBookingLine.LogEventOnShipmentWindowDatesIfNeeded();
				}
			}
		}

		void PopulateBusinessObjectCore(JobSupplierBooking targetBO)
		{
			SetValue(targetBO, JobSupplierBookingSchema.JSB_Status, dataObject.ShipmentStatus);
			SetValue(targetBO, JobSupplierBookingSchema.JSB_LoadMode, dataObject.LoadMode);
			SetValue(targetBO, JobSupplierBookingSchema.JSB_RL_NKDischargePort, dataObject.PortOfDischarge);
			SetValue(targetBO, JobSupplierBookingSchema.JSB_RL_NKLoadPort, dataObject.PortOfLoading);
			SetValue(targetBO, JobSupplierBookingSchema.JSB_IncoTerm, dataObject.ShipmentIncoTerm);
			SetValue(targetBO, JobSupplierBookingSchema.JSB_TransportMode, dataObject.TransportMode);
			SetValue(targetBO, JobSupplierBookingSchema.JSB_GoodsDescription, dataObject.GoodsDescription);
			SetValue(targetBO, JobSupplierBookingSchema.JSB_MarksAndNumbers, dataObject.MarksAndNumbers);
			SetValue(targetBO, JobSupplierBookingSchema.JSB_ContainerMode, dataObject.ContainerMode);
			SetValue(targetBO, JobSupplierBookingSchema.JSB_RL_NKOrigin, dataObject.PortOfOrigin);
			SetValue(targetBO, JobSupplierBookingSchema.JSB_RL_NKDestination, dataObject.PortOfDestination);
		}

		void PopulateContainers(JobSupplierBooking targetBO)
		{
			if (targetBO.JSB_LoadMode == Core.Constants.SupplierBookingLoadMode.ContainerYard)
			{
				if (dataObject.ContainerCollection?.Any() ?? false)
				{
					new JobSupplierBookingPlannedContainerDataObjectCollectionReader(targetBO, dataObject.ContainerCollection, this.logger, this.factory).ReadIntoCollection();
				}

				if (dataObject.RelatedShipmentCollection?.Any() ?? false)
				{
					if (targetBO.JSB_Status == Core.Constants.SupplierBookingStatus.Approved)
					{
						new JobSupplierBookingAllocatedContainerDataObjectCollectionReader(targetBO, new DataObjectList<UniversalShipment>(dataObject.RelatedShipmentCollection), this.logger).ReadIntoCollection();
					}
					else
					{
						logger.Log(LogType.Warning, Res.GetString("8d061548-71fd-4236-9eb8-ae7e08d4ab55", "Consol containers can only be allocated to approved CY supplier bookings."));
					}
				}
			}
		}

		void PopulateCustomFields(JobSupplierBooking targetBO)
		{
			PopulateWorkflowCustomFields(targetBO, dataObject, null);
		}

		void PopulateDates(JobSupplierBooking targetBO)
		{
			if ((dataObject.DateCollection?.Count ?? 0) == 0)
			{
				return;
			}

			foreach (var dateDataObject in dataObject.DateCollection)
			{
				switch (dateDataObject.Type)
				{
					case UniversalDateType.BookedOnDate:
						SetValue(targetBO, JobSupplierBookingSchema.JSB_BookedOnDate, dateDataObject.Value);
						break;
					case UniversalDateType.CargoAvailableDate:
						SetValue(targetBO, JobSupplierBookingSchema.JSB_CargoAvailableDate, dateDataObject.Value);
						break;
				}
			}
		}

		void PopulateOrganisations(JobSupplierBooking targetBO)
		{
			var bookingPartyAddress = JobSupplierBookingDataObjectHelper.FindByDocAddressTypeAndGetMatched(dataObject, DocAddressType.BookingPartyDocumentaryAddress, logger, factory);
			if (bookingPartyAddress != null)
			{
				targetBO.JSB_OH_BookingParty = bookingPartyAddress.OA_OH;
			}

			var supplierAddress = JobSupplierBookingDataObjectHelper.FindByDocAddressTypeAndGetMatched(dataObject, DocAddressType.Supplier, logger, factory);
			if (supplierAddress != null)
			{
				targetBO.SupplierAddress.E2_OA_Address = supplierAddress.PK;
			}

			var controllingCustomerAddress = JobSupplierBookingDataObjectHelper.FindByDocAddressTypeAndGetMatched(dataObject, DocAddressType.ControllingCustomer, logger, factory);
			if (controllingCustomerAddress != null)
			{
				targetBO.ControllingCustomerAddress.E2_OA_Address = controllingCustomerAddress.PK;
			}

			var localCartageCFSAddress = JobSupplierBookingDataObjectHelper.FindByDocAddressTypeAndGetMatched(dataObject, DocAddressType.LocalCartageCFS, logger, factory);
			if (localCartageCFSAddress != null)
			{
				targetBO.LocalCartageCFSAddress.E2_OA_Address = localCartageCFSAddress.PK;
			}

			var consigneeDocumentaryAddress = JobSupplierBookingDataObjectHelper.FindByDocAddressTypeAndGetMatched(dataObject, DocAddressType.ConsigneeDocumentaryAddress, logger, factory);
			if (consigneeDocumentaryAddress != null)
			{
				targetBO.ConsigneeDocumentaryAddress.E2_OA_Address = consigneeDocumentaryAddress.PK;
			}

			var arrivalCFSAddress = JobSupplierBookingDataObjectHelper.FindByDocAddressTypeAndGetMatched(dataObject, DocAddressType.ArrivalCFSAddress, logger, factory);
			if (arrivalCFSAddress != null)
			{
				targetBO.JSB_OA_CFSAddress = arrivalCFSAddress.PK;
			}
		}
	}
}
