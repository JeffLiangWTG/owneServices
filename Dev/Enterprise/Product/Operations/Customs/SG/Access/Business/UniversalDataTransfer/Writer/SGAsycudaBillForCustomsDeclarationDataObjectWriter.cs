using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business.UniversalDataTransfer;
using Enterprise.Customs.Common.SG;
using Enterprise.Customs.SG.V4.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.SG.Access.Business.UniversalDataTransfer
{
	public class SGAsycudaForCustomsDeclarationDataObjectWriter : AsycudaForCustomsDeclarationDataObjectWriter<AsycudaBill, AsycudaPack, AsycudaPackedItem>
	{
		public SGAsycudaForCustomsDeclarationDataObjectWriter(IDataWritingManager manager, SGAsycudaManifestHeaderDataObjectWriterHelper helper)
			: base(manager, helper)
		{
		}

		protected override void PopulateHouseBill(AsycudaBill sourceBill, Shipment uxml)
		{
			if (sourceBill.IsExport)
			{
				AddAddInfo(uxml, Customs.SG.V4.Business.AddInfo.GetDBNameMapped(SGAddInfoSchema.Constants.SG_OutwardHAWB), sourceBill.ABL_BillNumber);
				AddAddInfo(uxml, Customs.SG.V4.Business.AddInfo.GetDBNameMapped(SGAddInfoSchema.Constants.SG_OutwardMAWB), helper.Header.AMA_MasterBill);
				uxml.AddOrgAddress(writeManager, helper.Header.ShippingAgent, Constants.AddressType.OutwardCarrierAgent);
			}
			else
			{
				base.PopulateHouseBill(sourceBill, uxml);

				if (!sourceBill.GSTNReferenceNo.IsEmpty)
				{
					AddAddInfo(uxml, Customs.SG.V4.Business.AddInfo.GetDBNameMapped(SGAddInfoSchema.Constants.SG_US_NKPlaceOfReceipt), Constants.OverseasVendorRegistration);
				}

				uxml.AddOrgAddress(writeManager, helper.Header.ShippingAgent, Constants.AddressType.InwardCarrierAgent);
			}
		}

		protected override void PopulateVesselAndVoyage(AsycudaBill sourceBill, Shipment uxml)
		{
			if (sourceBill.IsExport)
			{
				AddAddInfo(uxml, SG.V4.Business.AddInfo.GetDBNameMapped(SGAddInfoSchema.Constants.SG_OutwardVesselName), helper.Header.AMA_VesselName);
				AddAddInfo(uxml, SG.V4.Business.AddInfo.GetDBNameMapped(SGAddInfoSchema.Constants.SG_OutwardVoyageFlightNo), helper.Header.AMA_Voyage);
			}
			else
			{
				base.PopulateVesselAndVoyage(sourceBill, uxml);
			}
		}

		protected override CommercialInvoiceLine CreateInvoiceLineData(AsycudaPack sourcePack, AsycudaPackedItem packedItem, ZInt lineNo)
		{
			var invoiceLineData = base.CreateInvoiceLineData(sourcePack, packedItem, lineNo);
			invoiceLineData.PartNo = sourcePack.MatchingReference;

			if (packedItem.GSTPaid == Customs.Business.YesNoList.Codes.Yes)
			{
				invoiceLineData.AdditionalLineTariffDetailCollection = new List<AdditionalLineTariffDetail>()
				{
					new AdditionalLineTariffDetail() { Type = new CodeDescriptionPair5Char() { Code = ZString.Empty }, Tariff = Constants.OverseasVendorRegistration, Value = ZDecimal.Zero }
				};
			}

			var gstnReferenceNo = packedItem.Pack?.Bill?.GSTNReferenceNo ?? ZString.Empty;
			if (!gstnReferenceNo.IsEmpty)
			{
				invoiceLineData.CustomsReferenceCollection = new List<CustomsReference>()
				{
					new CustomsReference
					{
						Type = new CodeDescriptionPair() { Code = Common.SG.CusCodeDataTypeList.Codes.CASCode1 },
						Reference = gstnReferenceNo
					}
				};
			}

			return invoiceLineData;
		}

		protected override void PopulateTransportMode(AsycudaBill sourceBill, Shipment uxml)
		{
			if (sourceBill.IsExport)
			{
				AddAddInfo(uxml, SG.V4.Business.AddInfo.GetDBNameMapped(SGAddInfoSchema.Constants.SG_OutwardTransportMode), helper.Header.AMA_TransportMode);
				uxml.TransportMode = null;
			}
			else
			{
				base.PopulateTransportMode(sourceBill, uxml);
			}
		}

		protected override void PopulateContainerMode(AsycudaBill sourceBill, Shipment uxml)
		{
			uxml.CustomsContainerMode = new ContainerMode { Code = CargoPackingCodeList.Codes.PackingType5 };
		}

		protected override void PopulateContainerCount(AsycudaBill sourceBill, Shipment uxml)
		{
			var header = helper.Header;
			if (header.IsAir || header.IsRoad)
			{
				uxml.ContainerCount = ZInt.Zero;
			}
		}

		protected override void PopulateMessageType(AsycudaBill sourceBill, Shipment uxml)
		{
			if (sourceBill.IsImport)
			{
				uxml.MessageType = new CodeDescriptionPair { Code = MessageTypeCodeList.Codes.INP };

				if (!sourceBill.GSTNReferenceNo.IsEmpty)
				{
					uxml.MessageSubType = new CodeDescriptionPair { Code = DeclarationTypeCodeList.Codes.APS };
				}
				else
				{
					uxml.MessageSubType = new CodeDescriptionPair { Code = DeclarationTypeCodeList.Codes.SFZ };
				}
			}
			else if (sourceBill.IsExport)
			{
				uxml.MessageType = new CodeDescriptionPair { Code = MessageTypeCodeList.Codes.OUT };
				uxml.MessageSubType = new CodeDescriptionPair { Code = DeclarationTypeCodeList.Codes.DRT };
			}
			else
			{
				base.PopulateMessageType(sourceBill, uxml);
			}
		}
	}
}
