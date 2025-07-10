using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.eManifest.Business;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.eManifest.DataTransfer.Universal
{
	public class SupplierBookingLineReader : ShipmentDataObjectReader<SupplierBookingLine>
	{
		readonly SupplierBookingHeader supplierBookingHeader;
		readonly ZInt index;

		public SupplierBookingLineReader(UniversalShipment lineDataObject, IXmlImportLogger logger, UniversalObjectFactory factory, SupplierBookingHeader supplierBookingHeader, int index)
			: base(lineDataObject, logger, factory)
		{
			if (supplierBookingHeader == null)
			{
				throw new ArgumentNullException(nameof(supplierBookingHeader));
			}

			this.supplierBookingHeader = supplierBookingHeader;
			this.index = index;
		}

		public override DataContextType DataContextType
		{
			get { return DataContextType.eManifestLine; }
		}

		protected override IMatchingBusinessEntityFinder<SupplierBookingLine> GetCombinedReferenceMatcher()
		{
			return null;
		}

		protected override SupplierBookingLine GetExistingBusinessObjectUsingModuleSpecificBusinessRules()
		{
			if (dataObject.WayBillNumber.HasValue && !dataObject.WayBillNumber.Value.IsEmpty)
			{
				var query = new ZQuery(SupplierBookingLineSchema.DL_ConsigneeReference, dataObject.WayBillNumber);
				var line = supplierBookingHeader.BookingLines.Find(query).FirstOrDefault();

				if (line != null)
				{
					return (SupplierBookingLine)line;
				}
			}

			return null;
		}

		protected override void PopulateBusinessObject(SupplierBookingLine targetBO)
		{
			SetValue(targetBO, SupplierBookingLineSchema.DL_ConsigneeReference, dataObject.WayBillNumber);

			if (targetBO != null && !targetBO.DL_JS_ApprovedShipment.IsEmpty)
			{
				var shipment = factory.Load<CommonShipment>(targetBO.DL_JS_ApprovedShipment);

				if (dataObject.PortOfOrigin != null && dataObject.PortOfOrigin.Code.HasValue)
				{
					SetValue(shipment, JobShipmentSchema.JS_RL_NKOrigin, dataObject.PortOfOrigin.Code.Value);
				}

				if (dataObject.PortOfDestination != null && dataObject.PortOfDestination.Code.HasValue)
				{
					SetValue(shipment, JobShipmentSchema.JS_RL_NKDestination, dataObject.PortOfDestination.Code.Value);
				}
			}

			SetValue(targetBO, SupplierBookingLineSchema.DL_GoodsValue, dataObject.GoodsValue);
			if (dataObject.GoodsValueCurrency != null && dataObject.GoodsValueCurrency.Code.HasValue)
			{
				SetValue(targetBO, SupplierBookingLineSchema.DL_RX_NKGoodsValueCurrency, dataObject.GoodsValueCurrency.Code);
			}

			SetValue(targetBO, SupplierBookingLineSchema.DL_GrossWeight, dataObject.TotalWeight);
			SetValue(targetBO, SupplierBookingLineSchema.DL_GrossWeightUQ, dataObject.TotalWeightUnit);
			SetValue(targetBO, SupplierBookingLineSchema.DL_Cubic, dataObject.TotalVolume);
			SetValue(targetBO, SupplierBookingLineSchema.DL_CubicUQ, dataObject.TotalVolumeUnit);
			SetValue(targetBO, SupplierBookingLineSchema.DL_GoodsDescription, dataObject.GoodsDescription);
			SetValue(targetBO, SupplierBookingLineSchema.DL_Index, index);
			SetValue(targetBO, SupplierBookingLineSchema.DL_IsDeliveryTransportSelfBooked, dataObject.IsLastMileDeliverySelfBooked);
			SetValue(targetBO, SupplierBookingLineSchema.DL_IsHazardous, dataObject.IsHazardous);

			if (dataObject.ServiceLevel != null && dataObject.ServiceLevel.Code.HasValue)
			{
				SetValue(targetBO, SupplierBookingLineSchema.DL_RS_NKServiceLevel, dataObject.ServiceLevel.Code);
			}

			if (dataObject.VendorIdentifier.HasValue && !dataObject.VendorIdentifier.Value.IsEmpty)
			{
				targetBO.DL_VendorIdentifier = dataObject.VendorIdentifier.Value;
			}

			PopulateMarksAndNumbers(targetBO);
			SetValue(targetBO, SupplierBookingLineSchema.DL_PiecesManifested, dataObject.TotalNoOfPieces);
			if (dataObject.LocalProcessing != null && dataObject.LocalProcessing.OrderNumberCollection != null && dataObject.LocalProcessing.OrderNumberCollection.Count > 0)
			{
				SetValue(targetBO, SupplierBookingLineSchema.DL_OrderTrackingNumber, dataObject.LocalProcessing.OrderNumberCollection[0].OrderReference);
			}

			if (supplierBookingHeader != null)
			{
				SetValue(targetBO, SupplierBookingLineSchema.DL_DH_BookingHeader, supplierBookingHeader.PK);
				supplierBookingHeader.BookingLines.Add(targetBO);
			}

			PopulateConsignorAddress(targetBO);
			PopulateConsigneeAddress(targetBO);

			if (targetBO != null && !targetBO.IsInDatabase)
			{
				SetValue(targetBO, SupplierBookingLineSchema.DL_Status, Core.Constants.SupplierBookingLineStatus.Codes.Incomplete);
			}

			PopulatePackingLine(targetBO);
		}

		void PopulateConsignorAddress(SupplierBookingLine supplierBookingLine)
		{
			if (dataObject.OrganizationAddressCollection == null || dataObject.OrganizationAddressCollection.FirstOrDefault(nameof(DocAddressType.ConsignorDocumentaryAddress)) == null)
			{
				if (!supplierBookingLine.IsInDatabase)
				{
					PopulateConsignorAddressFromHeader(supplierBookingLine);
				}

				return;
			}

			var addressDataObject = dataObject.OrganizationAddressCollection.FirstOrDefault(nameof(DocAddressType.ConsignorDocumentaryAddress));
			SetValue(supplierBookingLine, SupplierBookingLineSchema.DL_ConsignorName, addressDataObject.CompanyName);
			SetValue(supplierBookingLine, SupplierBookingLineSchema.DL_ConsignorAddress1, addressDataObject.Address1);
			SetValue(supplierBookingLine, SupplierBookingLineSchema.DL_ConsignorAddress2, addressDataObject.Address2);
			SetValue(supplierBookingLine, SupplierBookingLineSchema.DL_ConsignorCity, addressDataObject.City);
			SetValue(supplierBookingLine, SupplierBookingLineSchema.DL_ConsignorState, (ZString?)addressDataObject.State);
			SetValue(supplierBookingLine, SupplierBookingLineSchema.DL_ConsignorPostCode, addressDataObject.Postcode);
			SetValue(supplierBookingLine, SupplierBookingLineSchema.DL_RN_NKConsignorCountryCode, addressDataObject.Country, addressDataObject.Port);
			SetValue(supplierBookingLine, SupplierBookingLineSchema.DL_ConsignorContact, addressDataObject.Contact);
			SetValue(supplierBookingLine, SupplierBookingLineSchema.DL_ConsignorEmail, addressDataObject.Email);
			SetValue(supplierBookingLine, SupplierBookingLineSchema.DL_ConsignorPhone, addressDataObject.Phone);
			SetValue(supplierBookingLine, SupplierBookingLineSchema.DL_ConsignorMobile, addressDataObject.Mobile);
			SetValue(supplierBookingLine, SupplierBookingLineSchema.DL_ConsignorFax, addressDataObject.Fax);
		}

		void PopulateConsignorAddressFromHeader(SupplierBookingLine line)
		{
			var header = line.BookingHeader;
			if (header == null)
			{
				return;
			}

			var consignorAddress = line.Factory.Load<OrgAddress>(header.DH_OA_Consignor);
			if (consignorAddress != null)
			{
				SetValue(line, SupplierBookingLineSchema.DL_ConsignorName, consignorAddress.OA_CompanyNameOverride);
				SetValue(line, SupplierBookingLineSchema.DL_ConsignorAddress1, consignorAddress.OA_Address1);
				SetValue(line, SupplierBookingLineSchema.DL_ConsignorAddress2, consignorAddress.OA_Address2);
				SetValue(line, SupplierBookingLineSchema.DL_ConsignorCity, consignorAddress.OA_City);
				SetValue(line, SupplierBookingLineSchema.DL_ConsignorState, consignorAddress.OA_State);
				SetValue(line, SupplierBookingLineSchema.DL_ConsignorPostCode, consignorAddress.OA_PostCode);
				SetValue(line, SupplierBookingLineSchema.DL_RN_NKConsignorCountryCode, consignorAddress.OA_RN_NKCountryCode);
				SetValue(line, SupplierBookingLineSchema.DL_ConsignorEmail, consignorAddress.OA_Email);
				SetValue(line, SupplierBookingLineSchema.DL_ConsignorPhone, consignorAddress.OA_Phone);
				SetValue(line, SupplierBookingLineSchema.DL_ConsignorMobile, consignorAddress.OA_Mobile);
				SetValue(line, SupplierBookingLineSchema.DL_ConsignorFax, consignorAddress.OA_Fax);
			}

			if (header.ConsignorContact != null)
			{
				SetValue(line, SupplierBookingLineSchema.DL_ConsignorContact, header.ConsignorContact.OC_ContactName);
			}
		}

		void PopulateConsigneeAddress(SupplierBookingLine supplierBookingLine)
		{
			if (dataObject.OrganizationAddressCollection == null)
			{
				return;
			}

			var addressDataObject = dataObject.OrganizationAddressCollection.FirstOrDefault(nameof(DocAddressType.ConsigneeAddress));
			if (addressDataObject == null)
			{
				return;
			}

			SetValue(supplierBookingLine, SupplierBookingLineSchema.DL_ConsigneeName, addressDataObject.CompanyName);
			SetValue(supplierBookingLine, SupplierBookingLineSchema.DL_ConsigneeAddress1, addressDataObject.Address1);
			SetValue(supplierBookingLine, SupplierBookingLineSchema.DL_ConsigneeAddress2, addressDataObject.Address2);
			SetValue(supplierBookingLine, SupplierBookingLineSchema.DL_ConsigneeCity, addressDataObject.City);
			SetValue(supplierBookingLine, SupplierBookingLineSchema.DL_ConsigneeState, (ZString?)addressDataObject.State);
			SetValue(supplierBookingLine, SupplierBookingLineSchema.DL_ConsigneePostCode, addressDataObject.Postcode);
			SetValue(supplierBookingLine, SupplierBookingLineSchema.DL_RN_NKConsigneeCountryCode, addressDataObject.Country, addressDataObject.Port);
			SetValue(supplierBookingLine, SupplierBookingLineSchema.DL_ConsigneeContact, addressDataObject.Contact);
			SetValue(supplierBookingLine, SupplierBookingLineSchema.DL_ConsigneeEmail, addressDataObject.Email);
			SetValue(supplierBookingLine, SupplierBookingLineSchema.DL_ConsigneePhone, addressDataObject.Phone);
			SetValue(supplierBookingLine, SupplierBookingLineSchema.DL_ConsigneeMobile, addressDataObject.Mobile);
			SetValue(supplierBookingLine, SupplierBookingLineSchema.DL_ConsigneeFax, addressDataObject.Fax);
		}

		void SetValue(SupplierBookingLine supplierBookingLine, SchemaColumn column, Country country, UNLOCO port)
		{
			var countryCode = country.GetCodeAsUpperCase();

			if (!countryCode.IsEmpty)
			{
				supplierBookingLine[column] = countryCode;
			}
			else if (port != null)
			{
				var portCode = port.GetUNLOCOAsUpperCase(supplierBookingLine.Factory);

				if (portCode.Length == 5)
				{
					supplierBookingLine[column] = portCode.SubstringSafe(0, 2);
				}
			}
		}

		void PopulateMarksAndNumbers(SupplierBookingLine targetBO)
		{
			if (dataObject.NoteCollection != null && dataObject.NoteCollection.Count > 0)
			{
				var marksAndNumbersNote =
					dataObject.NoteCollection.FirstOrDefault(
						n => n.Description.HasValue && n.Description.Value == PredefinedNoteTypes.Instance.MarksAndNumbers.Description);

				if (marksAndNumbersNote != null)
				{
					SetValue(targetBO, SupplierBookingLineSchema.DL_MarksAndNumbers, marksAndNumbersNote.NoteText);
				}
			}
		}

		void PopulatePackingLine(SupplierBookingLine targetBO)
		{
			var packingLineCollection = dataObject.PackingLineCollection;
			if (packingLineCollection != null)
			{
				var packingLine = packingLineCollection.FirstOrDefault();
				if (packingLine != null)
				{
					SetValue(targetBO, SupplierBookingLineSchema.DL_SignatureRequired, packingLine.IsSignatureRequired);
					SetValue(targetBO, SupplierBookingLineSchema.DL_F3_NKPackType, packingLine.PackType);
					if (dataObject.TotalNoOfPieces == null)
					{
						SetValue(targetBO, SupplierBookingLineSchema.DL_PiecesManifested, packingLine.PackQty);
					}

					SetValue(targetBO, SupplierBookingLineSchema.DL_RequiresFumigation, packingLine.RequiresFumigationCertificate);
					SetValue(targetBO, SupplierBookingLineSchema.DL_IsPersonalEffects, packingLine.IsPersonalEffects);
					SetValue(targetBO, SupplierBookingLineSchema.DL_IsTimber, packingLine.IsTimber);
					SetValue(targetBO, SupplierBookingLineSchema.DL_IsPerishable, packingLine.IsPerishable);
				}
			}
		}
	}
}
