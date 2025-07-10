using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.LVS.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Customs.US.LVS.DataTransfer.Universal
{
	public class eTailUSLVConsignmentDataObjectReader : USLVConsignmentDataReader
	{
		public eTailUSLVConsignmentDataObjectReader(UniversalShipment shipmentDataObject, IXmlImportLogger logger, UniversalObjectFactory factory, CusUSLVClearance clearance, CusUSLVConsignment existingConsignment)
			: base(shipmentDataObject, logger, factory, clearance, existingConsignment)
		{
		}

		#region Override

		protected override void PopulateBusinessObject(CusUSLVConsignment consignment)
		{
			base.PopulateBusinessObject(consignment);
			PopulateContainerNumber(consignment);
			PopulateDeclarationReferance(consignment);
		}

		protected override void PopulateBusinessObjectWithDelaySetters(CusUSLVConsignment targetBO, Dictionary<string, ValueSetter> delaySetters)
		{
			base.PopulateBusinessObjectWithDelaySetters(targetBO, delaySetters);
			SetValue(targetBO, CusUSLVConsignmentSchema.ULB_ConsigneeIdentifier, dataObject.ConsigneeIdentifier, delaySetters);
			SetValue(targetBO, CusUSLVConsignmentSchema.ULB_SellerIdentifier, dataObject.VendorIdentifier, delaySetters);
		}

		void PopulateContainerNumber(CusUSLVConsignment consignment)
		{
			if (dataObject.PackingLineCollection != null)
			{
				var containerNumbers = dataObject.PackingLineCollection.Where(p => !string.IsNullOrEmpty(p.ContainerNumber)).Select(p => p.ContainerNumber).Distinct();
				if (containerNumbers.Count() == 1)
				{
					consignment.ULB_EquipmentNumber = containerNumbers.Single().Value;
				}
				else
				{
					consignment.ULB_EquipmentNumber = ZString.Empty;
				}
			}
		}

		void PopulateDeclarationReferance(CusUSLVConsignment consignment)
		{
			if (dataObject.CustomsReferenceCollection != null && dataObject.CustomsReferenceCollection.Count != 0)
			{
				var declarationReference = dataObject.CustomsReferenceCollection.FirstOrDefault(r => r.Type.Code.GetValueOrDefault().Equals(nameof(Core.Constants.DataContext.Declaration)));

				if (declarationReference != null && !string.IsNullOrEmpty(declarationReference.Reference))
				{
					consignment.CE_EntryLineReference = declarationReference.Reference.ToString();
					consignment.ULB_IsActive = false;
				}
			}
		}

		protected override void PopulateItem(CusUSLVItem item, CommercialInvoiceHeader invoice, CommercialInvoiceLine invoiceLine, Dictionary<string, ValueSetter> delaySetters)
		{
			var clearance = item.Consignment.Shipment;
			var state = clearance.PortOfLoading?.CountryStates?.RW_Code ?? ZString.Empty;

			SetValue(item, CusUSLVItemSchema.ULI_PartNo, invoiceLine.PartNo, delaySetters);
			SetValue(item, CusUSLVItemSchema.ULI_RX_NKCurrency, dataObject.GoodsValueCurrency, delaySetters);
			SetValue(item,
					CusUSLVItemSchema.ULI_RN_NKCountryOfOrigin,
					IsClearanceFromCanada(clearance, invoiceLine)
						? (ZString)CanadaProvinceTerritoryCodes.GetCodeFromNormalStateCode(state)
						: invoiceLine.CustomsSupportingInformationCollection?.FirstOrDefault()?.Country.GetNullableCodeAsUpperCase(),
					delaySetters);
			SetValue(item, CusUSLVItemSchema.ULI_GoodsDescription, GetGoodsDescriptionWithFallback());
			SetValue(item, CusUSLVItemSchema.ULI_Tariff, ImportExportHelper.IsImport(item.Consignment.ULB_RN_NKSellerCountry, item.Consignment.ULB_RN_NKConsigneeCountry) ? invoiceLine.HarmonisedCode : invoiceLine.CustomsSupportingInformationCollection?.FirstOrDefault()?.Tariff, delaySetters);
			SetValue(item, CusUSLVItemSchema.ULI_GoodsValue, invoiceLine.CustomsValue ?? dataObject.GoodsValue, delaySetters);

			string GetGoodsDescriptionWithFallback()
			{
				var result = invoiceLine.Description;
				if (string.IsNullOrEmpty(result))
				{
					result = GetLinkedPackedItem()?.Description;
				}

				if (string.IsNullOrEmpty(result))
				{
					result = dataObject.GoodsDescription;
				}

				return result;
			}

			PackedItem GetLinkedPackedItem() =>
				dataObject.
				PackingLineCollection?.
				SelectMany(line => line.PackedItemCollection).
				FirstOrDefault(p => p.CommercialInvoiceLineLink == invoiceLine.Link.GetValueOrDefault());
		}

		bool IsClearanceFromCanada(CusUSLVClearance clearance, CommercialInvoiceLine invoiceLine)
		{
			return (clearance.PortOfLoading?.RL_RN_NKCountryCode.Equals(CountryCodes.Canada) ?? false)
				&& (invoiceLine.CountryOfOrigin?.Code.Equals(CountryCodes.Canada) ?? false);
		}

		protected override string ConsigneeAddressType => nameof(DocAddressType.ConsigneeDocumentaryAddress);

		protected override string ConsignorAddressType => nameof(DocAddressType.ConsignorDocumentaryAddress);

		#endregion
	}
}
