using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.DataTransfer.Universal
{
	public class CusInBondBillDataObjectReader<TBill> : DataObjectReader<AdditionalBill, TBill>
		where TBill : CusInBondBill
	{
		public CusInBondBillDataObjectReader(AdditionalBill additionalBillDataObject, IXmlImportLogger logger, UniversalObjectFactory factory, InBondDataObjectReaderHelper helper, CusInBondHeader header)
			: base(additionalBillDataObject, logger, factory)
		{
			this.header = Argument.NotNull(header, "header");
			var headerRow = GetColumnIndexer(header);
			this.headerPK = headerRow.GetValue(CusInBondHeaderSchema.PK);
			this.readerHelper = Argument.NotNull(helper, "helper");
		}
		readonly CusInBondHeader header;
		readonly ZGuid headerPK;
		readonly InBondDataObjectReaderHelper readerHelper;

		protected InBondDataObjectReaderHelper Helper
		{
			get { return readerHelper; }
		}

		protected override TBill GetExistingBusinessObject()
		{
			TBill result = null;
			if (dataObject.BillNumber.HasValue)
			{
				var query = GetExistingBillQueryCore(headerPK);
				query.FetchOnlyFromLocalCache = !header.IsInDatabase;
				result = factory.LoadTop1<TBill>(query);
			}
			return result;
		}

		protected virtual ZQuery GetExistingBillQueryCore(ZGuid inBondHeaderPK)
		{
			var query = new ZQuery(CusInBondBillSchema.B0_BH, inBondHeaderPK);
			var wayBillType = dataObject.BillType.GetCodeAsUpperCase();
			var isMasterBill = wayBillType == WayBillTypeList.Codes.Master;
			var isHouseBill = wayBillType == WayBillTypeList.Codes.House;
			if (isMasterBill)
			{
				query.AddToFilter(CusInBondBillSchema.B0_MasterBillNumber, dataObject.BillNumber.GetValueOrDefault());
			}
			else if (isHouseBill)
			{
				query.AddToFilter(CusInBondBillSchema.B0_HouseBillNumber, dataObject.BillNumber.GetValueOrDefault());
				if (dataObject.ParentBillNumber.HasValue)
				{
					query.AddToFilter(CusInBondBillSchema.B0_MasterBillNumber, dataObject.ParentBillNumber.GetValueOrDefault());
				}
			}
			else
			{
				query.IsNoResultQuery = true;
			}
			return query;
		}

		protected override void PopulateBusinessObject(TBill billBO)
		{
			var billRow = GetColumnIndexer(billBO);
			SetValue(billRow, CusInBondBillSchema.B0_BH, headerPK);
			var delaySetters = IsDefaultingEnabled ? new Dictionary<string, ValueSetter>() : null;
			var wayBillType = dataObject.BillType.GetCodeAsUpperCase();
			if (wayBillType == WayBillTypeList.Codes.Master)
			{
				SetValue(billRow, CusInBondBillSchema.B0_MasterBillNumber, dataObject.BillNumber, delaySetters);
				SetValue(billRow, CusInBondBillSchema.B0_HouseBillNumber, ZString.Empty, delaySetters);
			}
			else if (wayBillType == WayBillTypeList.Codes.House)
			{
				SetValue(billRow, CusInBondBillSchema.B0_MasterBillNumber, dataObject.BillNumber, delaySetters);
				SetValue(billRow, CusInBondBillSchema.B0_HouseBillNumber, ZString.Empty, delaySetters);
			}

			if (dataObject.NoOfPacks.HasValue)
			{
				SetValue(billRow, CusInBondBillSchema.B0_ManifestQty, dataObject.NoOfPacks.Value.ToZInt(), delaySetters);
			}

			FillDataFromAddInfos(billRow);
			FillCustomsReferenceData(billRow);
			FillInBondSpecificData(billRow);
			FillOrganizationAddresses(billRow, billBO);
			delaySetters.SetValueInSpecificOrder(GetBillSettingOrder(billBO));
		}

		protected virtual void FillDataFromAddInfos(IColumnIndexer billRow)
		{
		}

		protected virtual void FillOrganizationAddresses(IColumnIndexer billRow, TBill billBO)
		{
			if (dataObject.OrganizationAddressCollection != null)
			{
				var addressParent = (IDocAddresses)billBO;
				var supportedAddressTypes = addressParent.SupportedAddressTypes;
				if (supportedAddressTypes != null && supportedAddressTypes.Count > 0)
				{
					foreach (var orgAddressDataObject in dataObject.OrganizationAddressCollection)
					{
						var addressType = orgAddressDataObject.AddressType.GetValueOrDefault();
						DocAddressType docAddressType;
						if (Enum.TryParse(addressType, false, out docAddressType) && supportedAddressTypes.Contains(docAddressType))
						{
							OrganisationDataObjectReader.MatchedOrNew(addressParent, orgAddressDataObject, logger, factory, null);
						}
					}
				}
			}
		}

		protected virtual IEnumerable<ZString> GetBillSettingOrder(TBill bill)
		{
			return Array.Empty<ZString>();
		}

		protected virtual void FillCustomsReferenceData(IColumnIndexer billRow)
		{
		}

		protected virtual void FillInBondSpecificData(IColumnIndexer billRow)
		{
		}
	}
}
