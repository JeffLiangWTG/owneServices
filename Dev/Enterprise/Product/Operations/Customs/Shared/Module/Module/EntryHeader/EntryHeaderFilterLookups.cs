using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Module
{
	public class EntryHeaderFilterLookups : CommonFilterLookups
	{
		public EntryHeaderFilterLookups(EntryHeaderFilterBusinessObject filterBizObj)
			: base(filterBizObj)
		{
		}

		protected new EntryHeaderFilterBusinessObject FilterBizObj => (EntryHeaderFilterBusinessObject)base.FilterBizObj;

		public CodeDescriptionPairList WarehouseTransactionStatusList => Factory.GetCachedValue<WarehouseTransactionStatusList>();

		public CodeDescriptionPairList TransportTypeList => Declaration.Lookups.TransportTypeList;

		public CodeDescriptionPairList ShipmentTypeList => JobMessageTypeList.GetCachedListFor(Factory, GlbCompany.CurrentCompany.Country.Code);

		public OrgAddressCollection DeclarantList => declarantList ?? (declarantList = new OrgAddressCollection(Factory));
		OrgAddressCollection declarantList;

		public virtual CodeDescriptionPairList MessageTypeList => EntryHeader.Lookups.CH_MessageTypeList;

		public virtual CodeDescriptionPairList EntryInstructionStyleList => EntryInstruction.Lookups.StyleList;

		CusEntryHeader EntryHeader => Factory.GetNull<CusEntryHeader>();

		CusEntryInstruction EntryInstruction => Factory.GetNull<CusEntryInstruction>();

		BaseJobDeclaration Declaration => Factory.GetNull<BaseJobDeclaration>();
	}
}
