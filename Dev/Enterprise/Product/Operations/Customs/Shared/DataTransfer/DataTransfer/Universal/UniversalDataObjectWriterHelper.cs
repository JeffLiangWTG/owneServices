using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;
using UniversalCustoms = Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;

namespace Enterprise.Customs.DataTransfer.Universal
{
	public class UniversalDataObjectWriterHelper : UniversalCommonHelper
	{
		public UniversalDataObjectWriterHelper(BusinessObjectFactory factory, ZString countryCode)
			: base(factory)
		{
			this.countryCode = countryCode;
			this.SupportAdditionalInvoiceLineEntryLineLink = AdditionalInvoiceLineEntryLineLinkSupporter.DoesSupport(countryCode);
		}

		public readonly bool SupportAdditionalInvoiceLineEntryLineLink;
		internal readonly ZString countryCode;

		#region List

		public WayBillTypeList WayBillTypeList
		{
			get { return factory.GetCachedValue<WayBillTypeList>(); }
		}

		public virtual WayBillType GetWayBillType(ZString billType)
		{
			return ListHelper.GetWithDescription<WayBillType>(Constants.GetWayBillType(billType), WayBillTypeList);
		}

		public ICodeDescriptionPairList GetCusSupportingInfoCSI_TypeList(ZString tablePrefix, string dataContext)
		{
			return GetCusSupportingInfoCSI_TypeList(countryCode, tablePrefix, dataContext);
		}

		public ICodeDescriptionPairList GetCusAddInfoB7_TypeList(ZString tablePrefix, string dataContext)
		{
			return GetCusAddInfoB7_TypeList(countryCode, tablePrefix, dataContext);
		}

		public ICodeDescriptionPairList GetCusCodeDataCY_TypeList(ZString tablePrefix, string dataContext)
		{
			return GetCusCodeDataCY_TypeList(countryCode, tablePrefix, dataContext);
		}

		public ICodeDescriptionPairList GetCusCodeDataCY_CodeList(ZString tablePrefix, string dataContext)
		{
			return GetCusCodeDataCY_CodeList(countryCode, tablePrefix, dataContext);
		}

		public ICustomLabelsProvider GetJobComInvoiceLineCustomLabelsProvider(CusEntryInstruction entryInstruction)
		{
			return GetJobComInvoiceLineCustomLabelsProviderFromProvider(entryInstruction);
		}

		public ICustomLabelsProvider GetJobComInvoiceLineCustomLabelsProvider(BaseJobDeclaration declaration)
		{
			return GetJobComInvoiceLineCustomLabelsProviderFromProvider(declaration);
		}

		public ICustomLabelsProvider GetCusContainerCustomLabelsProvider(BaseJobDeclaration declaration)
		{
			return GetCusContainerCustomLabelsProviderFromDeclaration(declaration);
		}

		public Freight.Common.Business.BindToLists BindToLists
		{
			get { return Freight.Common.Business.BindToLists.GetCachedLists(factory); }
		}

		public ZString[] GetSupportedCusSupportingInfoCSI_TypesFor(ZString parentTableCode, string dataContext)
		{
			return GetSupportedCusSupportingInfoCSI_TypesFor(countryCode, parentTableCode, dataContext);
		}

		public ZString[] GetSupportedCusAddInfoB7_TypesFor(ZString parentTableCode, string dataContext)
		{
			return GetSupportedCusAddInfoB7_TypesFor(countryCode, parentTableCode, dataContext);
		}

		public ZString[] GetSupportedCusCodeDataCY_TypesFor(ZString parentTableCode, string dataContext)
		{
			return GetSupportedCusCodeDataCY_TypesFor(countryCode, parentTableCode, dataContext);
		}

		public ZString[] GetSupportedCusReferenceCFR_TypesFor(ZString parentTableCode, string dataContext)
		{
			return GetSupportedCusReferenceCFR_TypesFor(countryCode, parentTableCode, dataContext);
		}

		#endregion

		public CusEntryNumber LoadCusEntryNumber(ZGuid parentPK, ZString entryType, ZString countryCode)
		{
			ZQuery filter = new ZQuery(CusEntryNumSchema.CE_ParentID, SQLComparisonOperator.Equal, parentPK);
			filter.AddToFilter(JoinCondition.And, CusEntryNumSchema.CE_EntryType, SQLComparisonOperator.Equal, entryType);
			filter.AddToFilter(JoinCondition.And, CusEntryNumSchema.CE_RN_NKCountryCode, SQLComparisonOperator.Equal, countryCode);

			return factory.LoadTop1<CusEntryNumber>(filter);
		}

		public void UpdateOrganizationAddressCollection(IOrganizationAddressCollectionParent parent, Business.MultiLineAddInfos.CusAddInfo cusAddInfo, IDataWritingManager writeManager)
		{
			UpdateOrganizationAddressCollectionCore(parent, cusAddInfo, writeManager);
		}

		protected virtual void UpdateOrganizationAddressCollectionCore(IOrganizationAddressCollectionParent parent, Business.MultiLineAddInfos.CusAddInfo cusAddInfo, IDataWritingManager writeManager)
		{
		}

		public IAdditionalAddInfoGroupCollectionDataObjectWriter GetAdditionalAddInfoGroupCollectionSupportFor(BusinessObject bizObj, IDataWritingManager writeManager)
		{
			return GetAdditionalAddInfoGroupCollectionSupportForCore(bizObj, writeManager);
		}

		protected virtual IAdditionalAddInfoGroupCollectionDataObjectWriter GetAdditionalAddInfoGroupCollectionSupportForCore(BusinessObject bizObj, IDataWritingManager writeManager)
		{
			return null;
		}

		#region PackingLineLink

		public ZInt GetPackingLineLink(BasePackage pack)
		{
			ZInt result;
			if (!PackingLinkDictionary.TryGetValue(pack, out result))
			{
				result = PackingLinkDictionary.Count + 1;
				PackingLinkDictionary.Add(pack, result);
			}
			return result;
		}

		Dictionary<BasePackage, ZInt> PackingLinkDictionary
		{
			get { return packingLineLinkDictionary ?? (packingLineLinkDictionary = new Dictionary<BasePackage, ZInt>()); }
		}
		Dictionary<BasePackage, ZInt> packingLineLinkDictionary;

		#endregion

		#region CommercialInvoiceLineLink

		public ZInt GetCommercialInvoiceLineLink(BaseJobComInvoiceLine invoiceLine)
		{
			ZInt result;
			if (!InvoiceLineLinkDictionary.TryGetValue(invoiceLine, out result))
			{
				result = InvoiceLineLinkDictionary.Count + 1;
				InvoiceLineLinkDictionary.Add(invoiceLine, result);
			}
			return result;
		}

		Dictionary<BaseJobComInvoiceLine, ZInt> InvoiceLineLinkDictionary
		{
			get { return invoiceLineLinkDictionary ?? (invoiceLineLinkDictionary = new Dictionary<BaseJobComInvoiceLine, ZInt>()); }
		}
		Dictionary<BaseJobComInvoiceLine, ZInt> invoiceLineLinkDictionary;

		#endregion

		#region EntryInstructionLink

		public ZInt? GetAllocatedEntryInstructionLink(ZGuid sourcePK)
		{
			ZInt? result = null;
			if (sourcePK.IsValid && entryInstructionLinkMap.ContainsKey(sourcePK))
			{
				result = entryInstructionLinkMap[sourcePK];
			}
			return result;
		}

		internal ZInt? AllocateEntryInstructionLink(ZGuid sourcePK)
		{
			ZInt? result = null;
			if (sourcePK.IsValid)
			{
				if (entryInstructionLinkMap.ContainsKey(sourcePK))
				{
					result = entryInstructionLinkMap[sourcePK];
				}
				else
				{
					result = entryInstructionLinkMap.Count + 1;
					entryInstructionLinkMap[sourcePK] = (int)result;
				}
			}
			return result;
		}
		readonly Dictionary<ZGuid, int> entryInstructionLinkMap = new Dictionary<ZGuid, int>();

		#endregion

		#region GoodsItemLink

		public ZInt GetGoodsItemLink(CusInBondCargoDesc goodsItem)
		{
			ZInt result;
			if (!GoodsItemDictionary.TryGetValue(goodsItem, out result))
			{
				result = GoodsItemDictionary.Count + 1;
				GoodsItemDictionary.Add(goodsItem, result);
			}
			return result;
		}

		Dictionary<CusInBondCargoDesc, ZInt> GoodsItemDictionary
		{
			get { return goodsItemLinkDictionary ?? (goodsItemLinkDictionary = new Dictionary<CusInBondCargoDesc, ZInt>()); }
		}
		Dictionary<CusInBondCargoDesc, ZInt> goodsItemLinkDictionary;

		#endregion

		#region EntryRelatedCommercialInfo

		internal void SetupEntryRelatedCommercialInfo(CusEntryHeader relatedEntry)
		{
			relatedEntryGroupInvoicePKs = relatedEntry.GroupInvoices.Select(x => x.PK).ToList();
			relatedEntryInvoicePKs = relatedEntry.InvoiceHeaders.Select(x => x.PK).ToList();
			relatedEntryInvoiceLinePKs = relatedEntry.InvoiceLines.Select(x => x.PK).ToList();
		}

		List<ZGuid> relatedEntryGroupInvoicePKs;
		List<ZGuid> relatedEntryInvoicePKs;
		List<ZGuid> relatedEntryInvoiceLinePKs;

		internal bool IsRelatedToEntry(BaseJobComInvoiceGroupHeader groupHeader)
		{
			return relatedEntryGroupInvoicePKs == null || relatedEntryGroupInvoicePKs.Contains(groupHeader.PK);
		}

		internal bool IsRelatedToEntry(BaseJobComInvoiceHeader invoiceHeader)
		{
			return relatedEntryInvoicePKs == null || relatedEntryInvoicePKs.Contains(invoiceHeader.PK);
		}

		internal bool IsRelatedToEntry(BaseJobComInvoiceLine invoiceLine)
		{
			return relatedEntryInvoiceLinePKs == null || relatedEntryInvoiceLinePKs.Contains(invoiceLine.PK);
		}

		#endregion

		public IEnumerable<UniversalCustoms.CustomsReference> GetAdditionalCustomsReferenceDataFor(BusinessObject bizObj, IDataWritingManager writeManager, string dataContext = null)
		{
			return GetAdditionalCustomsReferenceDataForCore(bizObj, writeManager, dataContext);
		}

		public virtual ZString? GetReferencedEntityDescriptionForCusCodeData(CusCodeData cusCodeData)
		{
			return null;
		}

		protected virtual IEnumerable<UniversalCustoms.CustomsReference> GetAdditionalCustomsReferenceDataForCore(BusinessObject bizObj, IDataWritingManager writeManager, string dataContext)
		{
			return null;
		}
	}

	public static class UniversalCommonHelperExtensions
	{
		[SuppressMessage("Microsoft.Performance", "CA1801:ReviewUnusedParameters")]
		public static OrganizationAddress CreateOrganizationAddressFromOrganization(this UniversalCommonHelper helper, IDataWritingManager writeManager, OrgHeader organization, ZString addressType)
		{
			return organization == null ? null : new OrganizationDataObjectWriter(writeManager, addressType).GetDataObject(organization.MainAddress);
		}

		[SuppressMessage("Microsoft.Performance", "CA1801:ReviewUnusedParameters")]
		public static OrganizationAddress CreateOrganizationAddressFromAddress(this UniversalCommonHelper helper, IDataWritingManager writeManager, OrgAddress address, ZString addressType)
		{
			return address == null ? null : new OrganizationDataObjectWriter(writeManager, addressType).GetDataObject(address);
		}
	}
}
