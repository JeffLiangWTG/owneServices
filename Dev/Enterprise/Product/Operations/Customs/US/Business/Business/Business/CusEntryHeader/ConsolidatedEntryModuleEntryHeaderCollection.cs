using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business
{
	[ModuleID(ModuleId.EntryHeader)]
	public class ConsolidatedEntryModuleEntryHeaderCollection : Customs.Business.ModuleEntryHeaderCollection
	{
		public ConsolidatedEntryModuleEntryHeaderCollection(BusinessObjectFactory factory)
			: base(factory, GlbCompany.CurrentCompany, null)
		{
		}

		public new CusEntryHeader this[int index]
		{
			get { return (CusEntryHeader)base[index]; }
		}

		public new CusEntryHeader AddNew()
		{
			return (CusEntryHeader)base.AddNew();
		}

		protected override ZQuery CreateRelationshipFilter()
		{
			var result = base.CreateRelationshipFilter();
			var messageTypeQuery = new ZQuery(CusEntryHeaderSchema.CH_MessageType, CusEntryHeaderMessageTypeList.Codes.ACECargoRelease);
			result.AddToFilter(messageTypeQuery);
			return result;
		}
	}
}
