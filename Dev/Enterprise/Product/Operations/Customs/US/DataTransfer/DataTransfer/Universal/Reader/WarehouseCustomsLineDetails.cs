using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.DataTransfer.Universal.Extensions;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using UniversalAddInfo = Enterprise.UniversalDataBuss.DataObjects.Universal.AddInfo;

namespace Enterprise.Customs.US.DataTransfer.Universal
{
	public class WarehouseCustomsLineDetails : Customs.DataTransfer.Universal.WarehouseCustomsLineDetails, IUSWarehouseCustomsLineDetails
	{
		internal WarehouseCustomsLineDetails(BusinessObjectFactory factory, CommercialInvoiceLine invoiceLine, CommercialInvoiceHeader invoice, IDictionary<ZString, ZInt> packageDetailDictionary, DeclarationDetail declarationDetail)
			: base(factory, invoiceLine, declarationDetail)
		{
			this.invoice = Argument.NotNull(invoice, "invoice");
			this.packageDetailDictionary = Argument.NotNull(packageDetailDictionary, "packageDetailDictionary");
		}
		readonly CommercialInvoiceHeader invoice;
		readonly IDictionary<ZString, ZInt> packageDetailDictionary;

		DeclarationDetail declarationDetail
		{
			get { return (DeclarationDetail)FallbackDetail; }
		}

		public class DeclarationDetail : WarehouseCustomsFallbackDetail
		{
			public bool IsFTZAdmission;
			public bool IsConsumptionFTZ;
			public bool IsImportByExternalBroker;
			public bool IsInBond;
			public ZString? EntryFilerCode;
			public ZString? EntryNumber;
			public ZString? WHSEntryFilerCode;
			public ZString? WHSEntryNumber;
			public ZString? FTZAdmissionNumber;
			public ZString? InBondNumber;
			public ZBool? FromOtherFTZ;
			public OutwardType? OutwardType;

			protected override WarehouseCustomsFallbackDetail CloneCore()
			{
				return new DeclarationDetail()
				{
					IsExWarehouse = this.IsExWarehouse,
					IsConsumptionFTZ = this.IsConsumptionFTZ,
					IsFTZAdmission = this.IsFTZAdmission,
					IsImportByExternalBroker = this.IsImportByExternalBroker,
					IsInBond = this.IsInBond,
					EntryFilerCode = this.EntryFilerCode,
					EntryNumber = this.EntryNumber,
					WHSEntryFilerCode = this.WHSEntryFilerCode,
					WHSEntryNumber = this.WHSEntryNumber,
					FTZAdmissionNumber = this.FTZAdmissionNumber,
					FromOtherFTZ = this.FromOtherFTZ,
					InBondNumber = this.InBondNumber,
					OutwardType = this.OutwardType,
					SupplierAddress = this.SupplierAddress,
					InvoiceLineAddInfosApplicableForInwardWarehousing = this.InvoiceLineAddInfosApplicableForInwardWarehousing
				};
			}
		}

		public ZString? ZoneStatus
		{
			get
			{
				ZString? result = null;
				if (declarationDetail.IsFTZAdmission && InvoiceLine != null)
				{
					result = InvoiceLine.AddInfoCollection?.GetZStringValue(JobComInvoiceLine.Schema.US_ZoneStatus.Substring(3));
				}
				return result;
			}
		}

		public ZBool? FromOtherFTZ => declarationDetail.FromOtherFTZ;

		public OutwardType? OutwardType => declarationDetail.OutwardType;

		public override Country CountryOfOrigin
		{
			get
			{
				Country result = GetCountryFrom(InvoiceLine.AddInfoCollection, JobComInvoiceLine.Schema.US_UC_NKCountryOfOrigin.Substring(3));
				if (result == null && invoice != null)
				{
					result = GetCountryFrom(invoice.AddInfoCollection, JobComInvoiceHeader.Schema.US_UC_NKCountryOfOrigin.Substring(3));
				}
				return result;
			}
		}

		Country GetCountryFrom(List<UniversalAddInfo> addInfoCollection, string addInfoKey)
		{
			Country result = null;
			if (addInfoCollection != null)
			{
				var countryCode = addInfoCollection.GetZStringValue(JobComInvoiceHeader.Schema.US_UC_NKCountryOfOrigin.Substring(3)).GetValueOrDefault();
				if (!countryCode.IsEmpty)
				{
					result = new Country() { Code = countryCode };
				}
			}
			return result;
		}

		protected override ZString? GetEntryNumber()
		{
			ZString? result = null;
			if (InvoiceLine != null)
			{
				if (declarationDetail.IsFTZAdmission)
				{
					result = declarationDetail.FTZAdmissionNumber;
				}
				else if (declarationDetail.IsInBond)
				{
					result = declarationDetail.InBondNumber;
				}
				else
				{
					var entryNumber = InvoiceLine.EntryNumber.GetValueOrDefault();
					result = declarationDetail.EntryFilerCode.GetValueOrDefault() + "-" + (entryNumber.IsEmpty ? declarationDetail.EntryNumber.GetValueOrDefault() : entryNumber);
				}
			}
			return result;
		}

		protected override ZString? GetPreviousEntryNumber()
		{
			ZString? result = null;
			if (InvoiceLine != null)
			{
				if (declarationDetail.IsExWarehouse)
				{
					result = declarationDetail.WHSEntryFilerCode.GetValueOrDefault() + "-" + declarationDetail.WHSEntryNumber.GetValueOrDefault();
				}
				else if (declarationDetail.IsConsumptionFTZ)
				{
					result = InvoiceLine.AddInfoCollection == null ? null : InvoiceLine.AddInfoCollection.GetZStringValue(JobComInvoiceLine.Schema.US_WHSEntryNumber.Substring(3));
				}
				else if (declarationDetail.IsInBond)
				{
					ZString? whsEntryFilerCode = null;
					ZString whsEntryNumber = null;
					if (InvoiceLine.AddInfoCollection != null)
					{
						whsEntryFilerCode = InvoiceLine.AddInfoCollection.GetZStringValue(JobDeclaration.Schema.US_WHSEntryFilerCode.Substring(3));
						whsEntryNumber = InvoiceLine.AddInfoCollection.GetZStringValue(JobComInvoiceLine.Schema.US_WHSEntryNumber.Substring(3)).GetValueOrDefault();
					}

					if (whsEntryFilerCode.HasValue)
					{
						result = whsEntryFilerCode.GetValueOrDefault() + "-" + whsEntryNumber;
					}
					else if (!whsEntryNumber.IsEmpty)
					{
						result = whsEntryNumber;
					}
				}
			}
			return result;
		}

		protected override ZShort? GetEntryLineNumber()
		{
			ZShort? result = null;
			if (declarationDetail.IsImportByExternalBroker)
			{
				result = InvoiceLine.AddInfoCollection == null ? null : InvoiceLine.AddInfoCollection.GetZShortValue(JobComInvoiceLine.Schema.US_WHSEntryLineNo.Substring(3));
			}
			else
			{
				result = base.GetEntryLineNumber();
			}
			return result;
		}

		protected override ZShort? GetPreviousEntryLineNumber()
		{
			ZShort? result = null;
			if (declarationDetail.IsExWarehouse || declarationDetail.IsConsumptionFTZ || declarationDetail.IsInBond)
			{
				result = InvoiceLine.AddInfoCollection == null ? null : InvoiceLine.AddInfoCollection.GetZShortValue(JobComInvoiceLine.Schema.US_WHSEntryLineNo.Substring(3));
			}
			return result;
		}

		protected override OrganizationAddress GetManufacturerAddress()
		{
			OrganizationAddress result = null;
			if (InvoiceLine != null && InvoiceLine.OrganizationAddressCollection != null)
			{
				result = InvoiceLine.OrganizationAddressCollection.FirstOrDefault(nameof(DocAddressType.Manufacturer));
			}

			return result;
		}

		public override IEnumerable<IWarehouseCustomsLinePackDetails> PackDetails
		{
			get
			{
				if (packDetails == null)
				{
					var list = new List<IWarehouseCustomsLinePackDetails>();
					if (IsInward && InvoiceLine.AddInfoGroupCollection != null)
					{
						foreach (var packingDetail in InvoiceLine.AddInfoGroupCollection.Where(x => x.Type.GetCodeAsUpperCase() == CusAddInfoTypeAttribute.Codes.USWHSPackLine && x.AddInfoCollection != null))
						{
							if (packingDetail.AddInfoCollection != null)
							{
								if (packingDetail.AddInfoCollection.GetZBoolValue(CusAddInfoTypeListProvider.AdditionalAddInfoType.InvoiceLinePackData.IsSimplePackagingStyle).GetValueOrDefault())
								{
									var packageQty = packingDetail.AddInfoCollection.GetZDecimalValue(CusAddInfoTypeListProvider.AdditionalAddInfoType.InvoiceLinePackData.PackageQty).GetValueOrDefault().ToZInt();
									var packedQty = packingDetail.AddInfoCollection.GetZDecimalValue(CusAddInfoTypeListProvider.AdditionalAddInfoType.InvoiceLinePackData.PackedQty).GetValueOrDefault();
									list.Add(new WarehouseCustomsLinePackDetails() { PackageQty = packageQty, PackedQty = packedQty, PackID = ZGuid.NewZGuid().ToStringKey() });
								}
								else
								{
									var packageID = packingDetail.AddInfoCollection.GetZStringValue(CusAddInfoTypeListProvider.AdditionalAddInfoType.InvoiceLinePackData.PackageID);
									if (packageID.HasValue)
									{
										ZInt packageQty;
										if (packageDetailDictionary.TryGetValue(packageID.Value, out packageQty))
										{
											var packedQty = packingDetail.AddInfoCollection.GetZDecimalValue(CusAddInfoTypeListProvider.AdditionalAddInfoType.InvoiceLinePackData.PackedQty).GetValueOrDefault();
											list.Add(new WarehouseCustomsLinePackDetails() { PackID = packageID.Value, PackageQty = packageQty, PackedQty = packedQty });
										}
									}
								}
							}
						}
					}
					packDetails = list.ToArray();
				}
				return packDetails;
			}
		}
		IWarehouseCustomsLinePackDetails[] packDetails;

		public override IEnumerable<ZString> ExtraClassificationDetails
		{
			get
			{
				if (extraClassificationDetails == null)
				{
					if (IsInward)
					{
						var lineNo = GetLineNo().GetValueOrDefault();
						if (!lineNo.IsEmpty)
						{
							extraClassificationDetails = GetExtraClassificationDetailsFor(lineNo);
						}
					}
					extraClassificationDetails = extraClassificationDetails ?? System.Array.Empty<ZString>();
				}
				return extraClassificationDetails;
			}
		}
		ZString[] extraClassificationDetails;

		bool IsInward
		{
			get { return !declarationDetail.IsExWarehouse && !declarationDetail.IsInBond && !declarationDetail.IsConsumptionFTZ; }
		}

		ZString[] GetExtraClassificationDetailsFor(ZInt lineNo)
		{
			ZString[] result = null;
			List<ZString> extraClassificationDetails = null;
			if (ParentLineNoToExtraClassificationDetailsMap.TryGetValue(lineNo, out extraClassificationDetails))
			{
				result = extraClassificationDetails.ToArray();
			}
			return result;
		}

		ZInt? GetLineNo()
		{
			return InvoiceLine.LineNo;
		}

		Dictionary<ZInt, List<ZString>> ParentLineNoToExtraClassificationDetailsMap
		{
			get
			{
				Dictionary<ZInt, List<ZString>> result = null;
				var invoiceMap = factory.GetCachedValue<Dictionary<CommercialInvoiceHeader, Dictionary<ZInt, List<ZString>>>>();
				if (!invoiceMap.TryGetValue(invoice, out result))
				{
					result = new Dictionary<ZInt, List<ZString>>();
					if (invoice.CommercialInvoiceLineCollection != null)
					{
						foreach (var invoiceLine in invoice.CommercialInvoiceLineCollection)
						{
							var parentLineNo = GetParentLineNo(invoiceLine);
							if (!parentLineNo.IsEmpty)
							{
								List<ZString> extraClassificationDetails = null;
								if (!result.TryGetValue(parentLineNo, out extraClassificationDetails))
								{
									extraClassificationDetails = new List<ZString>();
									result.Add(parentLineNo, extraClassificationDetails);
								}
								extraClassificationDetails.Add(GetExtraClassificationDetails(invoiceLine));
							}
						}
					}
					invoiceMap.Add(invoice, result);
				}
				return result;
			}
		}

		ZString GetExtraClassificationDetails(CommercialInvoiceLine invoiceLine)
		{
			var extraClassificationDetails = new Dictionary<ZString, ZString>();
			AddExtraClassificationDetails(extraClassificationDetails, Constants.AddInfoKeys.InvoiceLine.LineNo, invoiceLine.LineNo);
			AddExtraClassificationDetails(extraClassificationDetails, Constants.AddInfoKeys.InvoiceLine.Tariff, invoiceLine.HarmonisedCode);
			AddExtraClassificationDetails(extraClassificationDetails, Constants.AddInfoKeys.InvoiceLine.InvoiceQuantity, invoiceLine.InvoiceQuantity);
			if (invoiceLine.InvoiceQuantityUnit != null)
			{
				AddExtraClassificationDetails(extraClassificationDetails, Constants.AddInfoKeys.InvoiceLine.InvoiceQuantityUnit, invoiceLine.InvoiceQuantityUnit.Code);
			}
			AddExtraClassificationDetails(extraClassificationDetails, Constants.AddInfoKeys.InvoiceLine.CustomsQuantity, invoiceLine.CustomsQuantity);
			if (invoiceLine.CustomsQuantityUnit != null)
			{
				AddExtraClassificationDetails(extraClassificationDetails, Constants.AddInfoKeys.InvoiceLine.CustomsQuantityUnit, invoiceLine.CustomsQuantityUnit.Code);
			}

			AddExtraClassificationDetails(extraClassificationDetails, Constants.AddInfoKeys.InvoiceLine.CustomsSecondQuantity, invoiceLine.CustomsSecondQuantity);
			if (invoiceLine.CustomsSecondQuantityUnit != null)
			{
				AddExtraClassificationDetails(extraClassificationDetails, Constants.AddInfoKeys.InvoiceLine.CustomsSecondQuantityUnit, invoiceLine.CustomsSecondQuantityUnit.Code);
			}

			AddExtraClassificationDetails(extraClassificationDetails, Constants.AddInfoKeys.InvoiceLine.CustomsThirdQuantity, invoiceLine.CustomsThirdQuantity);
			if (invoiceLine.CustomsThirdQuantityUnit != null)
			{
				AddExtraClassificationDetails(extraClassificationDetails, Constants.AddInfoKeys.InvoiceLine.CustomsThirdQuantityUnit, invoiceLine.CustomsThirdQuantityUnit.Code);
			}

			AddExtraClassificationDetails(extraClassificationDetails, Constants.AddInfoKeys.InvoiceLine.LinePrice, invoiceLine.LinePrice);
			if (invoiceLine.AddInfoCollection != null && invoiceLine.AddInfoCollection.Count > 0)
			{
				var supportedAddInfoKeys = new List<string>(factory.GetInvoiceLineAddInfosApplicableForInwardWarehousing());
				foreach (var invoiceLineAddInfo in invoiceLine.AddInfoCollection)
				{
					var key = invoiceLineAddInfo.Key.GetValueOrDefault();
					if (!key.IsEmpty && supportedAddInfoKeys.Contains(key))
					{
						AddExtraClassificationDetails(extraClassificationDetails, key, invoiceLineAddInfo.Value);
					}
				}
				var parentProductLineNo = invoiceLine.AddInfoCollection.GetZIntValue(Constants.AddInfoKeys.InvoiceLine.ParentProductLineNo).GetValueOrDefault();
				if (parentProductLineNo > ZInt.Zero)
				{
					AddExtraClassificationDetails(extraClassificationDetails, Constants.AddInfoKeys.InvoiceLine.ParentProductLineNo, parentProductLineNo);
				}
			}
			return AddInfoParser.Serialise(extraClassificationDetails).Left(CusAddInfo.Schema.B7_AddInfoDataMaxLength);
		}

		void AddExtraClassificationDetails(Dictionary<ZString, ZString> extraClassificationDetails, string addInfoKey, IZType value)
		{
			if (value != null && !extraClassificationDetails.ContainsKey(addInfoKey))
			{
				extraClassificationDetails.Add(addInfoKey, Enterprise.Customs.US.Business.AddInfo.GetStringRepresentation(value));
			}
		}

		ZInt GetParentLineNo(CommercialInvoiceLine invoiceLine)
		{
			var result = invoiceLine.ParentLineNo.GetValueOrDefault();
			if (result.IsEmpty && invoiceLine.AddInfoCollection != null)
			{
				result = invoiceLine.AddInfoCollection.GetZIntValue(Constants.AddInfoKeys.InvoiceLine.ParentProductLineNo).GetValueOrDefault();
			}
			return result;
		}
	}
}
