using System.Collections.ObjectModel;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.Messaging.Integration;
using Enterprise.Packing.DataTransfer.Universal;
using Enterprise.TransportConsignment.Business;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.TransportConsignment.DataTransfer.Universal
{
	class DtbCarrierBookingConsignmentDataObjectWriter : TopLevelDataObjectWriter<DtbCarrierBookingConsignment, UniversalShipment>
	{
		public DtbCarrierBookingConsignmentDataObjectWriter(IDataWritingManager manager)
			: base(manager)
		{
		}

		protected override ZString GetEDIMessageSubType()
		{
			return EDIMessageSubTypeList.Codes.XmlUniversalShipment;
		}

		protected override DataContextType GetTopLevelDataContextType()
		{
			return DataContextType.CarrierBookingConsignment;
		}
		protected override void PopulateDataObject(DtbCarrierBookingConsignment dtbCarrierBookingBO, UniversalShipment dataObject)
		{
			Argument.NotNull(dtbCarrierBookingBO, "DtbCarrierBooking dtbCarrierBookingBO");
			dataObject.Branch = new Branch()
			{
				Code = dtbCarrierBookingBO.Branch?.GB_Code,
				Name = dtbCarrierBookingBO.Branch?.GB_BranchName
			};
			dataObject.WayBillNumber = dtbCarrierBookingBO.LTC_ConnoteNumber;
			dataObject.SetOrganizationAddressCollection(() => ProcessCollection(dtbCarrierBookingBO.Addresses, new DtbCarrierBookingConsignmentAddressDataObjectWriter(writeManager)));
			PopulateNotesDateObject(dtbCarrierBookingBO, dataObject);
			PopulateCarrierDataObject(dtbCarrierBookingBO, dataObject);
			PopulatePackagesDataObject(dtbCarrierBookingBO, dataObject);
		}

		protected void PopulateNotesDateObject(DtbCarrierBookingConsignment dtbCarrierBookingBO, UniversalShipment dataObject)
		{
			var noteWriter = new NoteDataObjectWriter(writeManager);
			Collection<Note> notesCollection = new Collection<Note>();
			AddNotesToCollection(notesCollection, ProcessCollection(dtbCarrierBookingBO.Addresses, new DtbCarrierBookingConsignmentAddressInstructionDataObjectWriter(writeManager), CollectionContent.Partial));
			AddNotesToCollection(notesCollection, ProcessCollection(dtbCarrierBookingBO.Notes.GetAllNotesVisibleToCurrentCompany(), noteWriter, CollectionContent.Complete));
			if (notesCollection.Count > 0)
			{
				dataObject.SetNoteCollection(() => new DataObjectList<Note>(notesCollection));
			}
		}

		void AddNotesToCollection(Collection<Note> notesCollection, DataObjectList<Note> notes)
		{
			if (notes != null)
			{
				foreach (var note in notes)
				{
					notesCollection.Add(note);
				}
			}
		}

		protected void PopulateCarrierDataObject(DtbCarrierBookingConsignment dtbCarrierBookingBO, UniversalShipment dataObject)
		{
			PopulateCarrierServiceLevel(dtbCarrierBookingBO, dataObject);
			var carrierAccount = dtbCarrierBookingBO.CarrierAccount;
			if (carrierAccount != null)
			{
				dataObject.CarrierAccount = new CarrierAccount()
				{
					AccountNumber = carrierAccount.OAN_AccountNumber,
					MerchantNumber = carrierAccount.OAN_MerchantNumber,
					DepotID = carrierAccount.OAN_DepotID
				};
			}
		}

		protected void PopulatePackagesDataObject(DtbCarrierBookingConsignment dtbCarrierBookingBO, UniversalShipment dataObject)
		{
			var packageJobWriter = new PkgPackageJobDataObjectWriter(writeManager);
			var packageJobDataObject = packageJobWriter.GetDataObject(dtbCarrierBookingBO.PackageJob);
			var packingLines = new DataObjectList<PackingLine>();
			for (int i = 0; i < packageJobDataObject.PackingLineCollection.Count; i++)
			{
				if (packageJobDataObject.PackingLineCollection[i].PackQty != null && packageJobDataObject.PackingLineCollection[i].PackQty > 0)
				{
					packingLines.Add(packageJobDataObject.PackingLineCollection[i]);
				}
			}
			dataObject.SetPackingLineCollection(() => packingLines);
		}

		#region PopulateCarrierServiceLevel

		void PopulateCarrierServiceLevel(DtbCarrierBookingConsignment dtbCarrierBookingBO, UniversalShipment shipmentDataObject)
		{
			var carrierServiceLevel = GetCarrierServiceLevel(dtbCarrierBookingBO);
			if (carrierServiceLevel != null)
			{
				var writer = new CarrierServiceLevelDataObjectWriter(writeManager);
				shipmentDataObject.CarrierServiceLevel = writer.GetDataObject(carrierServiceLevel);
				shipmentDataObject.IsSignatureRequired = carrierServiceLevel.PL_IsSignatureRequired;
			}
		}

		#endregion

		#region GetCarrierServiceLevel

		OrgCarrierServiceLevel GetCarrierServiceLevel(DtbCarrierBookingConsignment dtbCarrierBookingBO)
		{
			OrgCarrierServiceLevel serviceLevel = null;

			var transportCo = dtbCarrierBookingBO.Carrier;
			if (transportCo != null)
			{
				var query = new ZQuery(OrgCarrierServiceLevelSchema.PL_Code, dtbCarrierBookingBO.LTC_PL_NKCarrierServiceLevel);
				query.AddToFilter(OrgCarrierServiceLevelSchema.PL_OM, transportCo.MiscServ.PK);
				serviceLevel = dtbCarrierBookingBO.Factory.LoadTop1<OrgCarrierServiceLevel>(query);
			}

			return serviceLevel;
		}

		#endregion
	}
}
