using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using UniversalAddInfo = Enterprise.UniversalDataBuss.DataObjects.Universal.AddInfo;

namespace Enterprise.Customs.DataTransfer.Universal
{
	public class WarehouseCustomsLineDetails : IWarehouseCustomsLineDetails
	{
		internal protected WarehouseCustomsLineDetails(BusinessObjectFactory factory, CommercialInvoiceLine invoiceLine, WarehouseCustomsFallbackDetail fallbackDetail)
		{
			this.factory = Argument.NotNull(factory, "factory");
			this.invoiceLine = invoiceLine;
			this.fallbackDetail = Argument.NotNull(fallbackDetail, "fallbackDetail");
		}

		internal protected WarehouseCustomsLineDetails(BusinessObjectFactory factory, CommercialInvoiceLine invoiceLine, WarehouseCustomsFallbackDetail fallbackDetail, Shipment shipment)
			: this(factory, invoiceLine, fallbackDetail)
		{
			this.shipment = Argument.NotNull(shipment, "shipment");
		}

		readonly protected BusinessObjectFactory factory;
		readonly CommercialInvoiceLine invoiceLine;
		readonly WarehouseCustomsFallbackDetail fallbackDetail;
		readonly protected Shipment shipment;

		public WarehouseCustomsFallbackDetail FallbackDetail => fallbackDetail;

		public CommercialInvoiceLine InvoiceLine => invoiceLine;

		public virtual Country CountryOfOrigin => invoiceLine.CountryOfOrigin;

		public virtual ZDecimal? CustomsQuantity => invoiceLine.CustomsQuantity;

		public virtual CodeDescriptionPair6Char CustomsQuantityUnit => invoiceLine.CustomsQuantityUnit;

		public ZString? EntryNumber
		{
			get
			{
				if (!entryNumber.HasValue)
				{
					entryNumber = GetEntryNumber();
					if (entryNumber.GetValueOrDefault().IsEmpty)
					{
						entryNumber = Constants.EntryNumberPlaceHolder;
					}
				}
				return entryNumber;
			}
		}
		ZString? entryNumber;

		protected virtual ZString? GetEntryNumber() => InvoiceLine?.EntryNumber;

		public ZShort? EntryLineNumber => CachedValueHelper.GetValue(ref entryLineNumber, GetEntryLineNumber);
		CachedValue<ZShort?> entryLineNumber;

		protected virtual ZShort? GetEntryLineNumber() => InvoiceLine?.EntryLineNumber;

		public ZString? PreviousEntryNumber => CachedValueHelper.GetValue(ref previousEntryNumber, GetPreviousEntryNumber);
		CachedValue<ZString?> previousEntryNumber;

		protected virtual ZString? GetPreviousEntryNumber() => InvoiceLine?.PreviousEntryNumber;

		public ZShort? PreviousEntryLineNumber => CachedValueHelper.GetValue(ref previousEntryLineNumber, GetPreviousEntryLineNumber);
		CachedValue<ZShort?> previousEntryLineNumber;

		protected virtual ZShort? GetPreviousEntryLineNumber() => InvoiceLine?.PreviousEntryLineNumber;

		public virtual ZString? OrderNumber => InvoiceLine?.BondedWHSOrderNumber;

		public virtual ZInt? OrderLineNo => InvoiceLine?.BondedWHSOrderLineNumber;

		public ZString AddInfos
		{
			get
			{
				if (!fAddInfos.HasValue)
				{
					var addInfos = IsOutward ? null : GetAddInfosApplicableForWarehousing();
					if (addInfos != null && addInfos.Any())
					{
						var builder = new ZStringBuilder(addInfos.Select<UniversalAddInfo, ZString>(addInfo => addInfo.Key.Value + "=" + addInfo.Value.Value));
						fAddInfos = builder.ToStringWithDelimiterBetweenAppends("*");
					}
					else
					{
						fAddInfos = ZString.Empty;
					}
				}
				return fAddInfos.Value;
			}
		}
		ZString? fAddInfos;

		public IEnumerable<IWarehouseCustomsLineAddInfo> AdditionalAddInfos => additionalAddInfos ?? (additionalAddInfos = GetAdditionalAddInfos());

		IEnumerable<IWarehouseCustomsLineAddInfo> additionalAddInfos;

		protected virtual IEnumerable<IWarehouseCustomsLineAddInfo> GetAdditionalAddInfos() => Enumerable.Empty<IWarehouseCustomsLineAddInfo>();

		public virtual ZDecimal TILV => ZDecimal.Zero;

		public ZDecimal ValueForDuty => GetValueForDuty();

		protected virtual ZDecimal GetValueForDuty() => InvoiceLine.CustomsValue.GetValueOrDefault();

		public OrganizationAddress SupplierAddress => CachedValueHelper.GetValue(ref supplierAddress, GetSupplierAddress);
		CachedValue<OrganizationAddress> supplierAddress;

		protected virtual OrganizationAddress GetSupplierAddress()
		{
			return InvoiceLine.OrganizationAddressCollection.FindBestSupplierMatch()
				?? FallbackDetail.SupplierAddress;
		}

		#region ManufacturerAddress

		public OrganizationAddress ManufacturerAddress => CachedValueHelper.GetValue(ref manufacturerAddress, GetManufacturerAddress);
		CachedValue<OrganizationAddress> manufacturerAddress;

		protected virtual OrganizationAddress GetManufacturerAddress()
		{
			return null;    // customs team will implement
		}

		#endregion

		public virtual IEnumerable<IWarehouseCustomsLinePackDetails> PackDetails => Enumerable.Empty<IWarehouseCustomsLinePackDetails>();

		public virtual IEnumerable<ZString> ExtraClassificationDetails => Enumerable.Empty<ZString>();

		protected virtual List<UniversalAddInfo> GetAddInfosApplicableForWarehousing()
		{
			var addInfoCollection = new List<UniversalAddInfo>();
			addInfoCollection.AddRange(GetThirdQuantityAsAddInfosForWarehousing());
			var invoiceLineAddInfoCollection = GetInvoiceLineAddInfoCollection();
			if (FallbackDetail.InvoiceLineAddInfosApplicableForInwardWarehousing != null)
			{
				foreach (var key in FallbackDetail.InvoiceLineAddInfosApplicableForInwardWarehousing)
				{
					var invoiceLineAddInfo = invoiceLineAddInfoCollection.FirstOrDefault(x => x.Key.GetValueOrDefault() == key);
					if (invoiceLineAddInfo == null)
					{
						if (FallbackDetail.FallbackAddInfos != null && FallbackDetail.FallbackAddInfos.TryGetValue(key, out invoiceLineAddInfo))
						{
							addInfoCollection.Add(invoiceLineAddInfo);
						}
					}
					else
					{
						invoiceLineAddInfoCollection.Remove(invoiceLineAddInfo);
						addInfoCollection.Add(invoiceLineAddInfo);
					}
				}
			}
			return addInfoCollection;
		}

		protected virtual List<UniversalAddInfo> GetThirdQuantityAsAddInfosForWarehousing()
		{
			var addInfoCollection = new List<UniversalAddInfo>();
			if (CustomsThirdQuantity != null)
			{
				addInfoCollection.Add(new UniversalAddInfo() { Key = BondedWarehousingHelper.Constants.CustomsThirdQuantity, Value = CustomsThirdQuantity.ToString() });
			}
			if (CustomsThirdQuantityUnit != null)
			{
				addInfoCollection.Add(new UniversalAddInfo() { Key = BondedWarehousingHelper.Constants.CustomsThirdQuantityUnit, Value = CustomsThirdQuantityUnit.Code });
			}
			return addInfoCollection;
		}

		protected virtual bool IsOutward => FallbackDetail.IsExWarehouse;

		protected List<UniversalAddInfo> GetInvoiceLineAddInfoCollection()
		{
			var invoiceLineAddInfoCollection = new List<UniversalAddInfo>();
			if (InvoiceLine.AddInfoCollection != null)
			{
				invoiceLineAddInfoCollection.AddRange(InvoiceLine.AddInfoCollection);
			}
			return invoiceLineAddInfoCollection;
		}

		public virtual ZDecimal? CustomsSecondQuantity => invoiceLine.CustomsSecondQuantity;

		public virtual CodeDescriptionPair6Char CustomsSecondQuantityUnit => invoiceLine.CustomsSecondQuantityUnit;

		public virtual ZString? Tariff => invoiceLine.HarmonisedCode;

		public virtual ZString? PrimaryPreference => invoiceLine.PrimaryPreference;

		public virtual ZDecimal? CustomsThirdQuantity => InvoiceLine.CustomsThirdQuantity;

		public virtual CodeDescriptionPair6Char CustomsThirdQuantityUnit => InvoiceLine.CustomsThirdQuantityUnit;

		public IEnumerable<IWarehouseCustomsLineAllocationInfo> AllocationInfos => allocationInfos ?? (allocationInfos = GetAllocationInfos());
		IEnumerable<IWarehouseCustomsLineAllocationInfo> allocationInfos;

		protected virtual IEnumerable<IWarehouseCustomsLineAllocationInfo> GetAllocationInfos()
		{
			var result = Array.Empty<IWarehouseCustomsLineAllocationInfo>();
			var addInfoGroupCollection = InvoiceLine?.AddInfoGroupCollection;
			if (addInfoGroupCollection != null)
			{
				result = addInfoGroupCollection
					.Where(x => (string)x.Type.Code == CusAddInfoTypeAttribute.Codes.WarehouseAllocationInfo)
					.Select(x => new WarehouseCustomsLineAllocationInfo(x.AddInfoCollection))
					.ToArray();
			}
			return result;
		}

		public ZString? NewOwnerProductCode => CachedValueHelper.GetValue(ref newOwnerProductCode, GetNewOwnerProductCode);
		CachedValue<ZString?> newOwnerProductCode;

		ZString? GetNewOwnerProductCode() => GetInvoiceLineAddInfo(Constants.AddInfoKeys.InvoiceLine.NewOwnerProductCode)?.Value;

		public ZString? NewOwnerPartAttribute1 => CachedValueHelper.GetValue(ref newOwnerPartAttribute1, GetNewOwnerPartAttribute1);
		CachedValue<ZString?> newOwnerPartAttribute1;

		ZString? GetNewOwnerPartAttribute1() => GetInvoiceLineAddInfo(Constants.AddInfoKeys.InvoiceLine.NewOwnerPartAttribute1)?.Value;

		public ZString? NewOwnerPartAttribute2 => CachedValueHelper.GetValue(ref newOwnerPartAttribute2, GetNewOwnerPartAttribute2);
		CachedValue<ZString?> newOwnerPartAttribute2;

		ZString? GetNewOwnerPartAttribute2() => GetInvoiceLineAddInfo(Constants.AddInfoKeys.InvoiceLine.NewOwnerPartAttribute2)?.Value;

		public ZString? NewOwnerPartAttribute3 => CachedValueHelper.GetValue(ref newOwnerPartAttribute3, GetNewOwnerPartAttribute3);
		CachedValue<ZString?> newOwnerPartAttribute3;

		ZString? GetNewOwnerPartAttribute3() => GetInvoiceLineAddInfo(Constants.AddInfoKeys.InvoiceLine.NewOwnerPartAttribute3)?.Value;

		public ZString? NewOwnerSerialNumber => CachedValueHelper.GetValue(ref newOwnerSerialNumber, GetNewOwnerSerialNumber);
		CachedValue<ZString?> newOwnerSerialNumber;

		ZString? GetNewOwnerSerialNumber() => GetInvoiceLineAddInfo(Constants.AddInfoKeys.InvoiceLine.NewOwnerSerialNumber)?.Value;

		protected UniversalAddInfo GetInvoiceLineAddInfo(ZString key) => InvoiceLine.AddInfoCollection?.FirstOrDefault(x => x.Key.GetValueOrDefault() == key);

		public virtual ZDateTime? CustomsDeadline => ZDateTime.Empty;

		public virtual ZString? Style => ZString.Empty;

		public virtual ZString? Procedure => ZString.Empty;

		public ZString? DataImportMatchingKey => InvoiceLine?.DataImportMatchingKey ?? ZString.Empty;

		public virtual ZString? CountryOfDestination => null;

		public virtual ZString? Remarks => InvoiceLine?.BondedWarehouseRemarks;

		public virtual ZDecimal? CustomsFourthQuantity => InvoiceLine?.CustomsFourthQuantity;

		public virtual CodeDescriptionPair6Char CustomsFourthQuantityUnit => InvoiceLine?.CustomsFourthQuantityUnit;

		public virtual ZDecimal? CustomsFifthQuantity => InvoiceLine?.CustomsFifthQuantity;

		public virtual CodeDescriptionPair6Char CustomsFifthQuantityUnit => InvoiceLine?.CustomsFifthQuantityUnit;
	}
}
