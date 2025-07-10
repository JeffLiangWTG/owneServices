using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business
{
	[ModuleID(ModuleId.EntryHeader)]
	public class ModuleEntryHeaderCollection : Customs.Business.ModuleEntryHeaderCollection
	{
		public ModuleEntryHeaderCollection(BusinessObjectFactory factory)
			: base(factory, GlbCompany.CurrentCompany, null)
		{
		}

		public ModuleEntryHeaderCollection(BusinessObjectFactory factory, ICollectionRelationship relationship)
			: base(factory, relationship)
		{
		}

		public static class USFilterConstants
		{
			public const string EntryNumber = "Entry Number (ENS)";
			public const string ImportationDate = "Import Date";
			public const string ReconIssue = "Recon Issue";
			public const string ImporterOfRecord = "Importer of Record";
			public const string PortOfEntry = "Entry Port";
			public const string SuretyCode = "Surety Code";
			public const string EntryType = "Entry Type";
			public const string ImportSource = "Import Source";
			public const string HouseBill = "House Bill";
			public const string MasterBill = "Master Bill";
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
			ZQuery result = base.CreateRelationshipFilter();

			ZQuery messageTypeQuery = new ZQuery(CusEntryHeaderSchema.CH_MessageType, CusEntryHeaderMessageTypeList.Codes.EntrySummary);
			messageTypeQuery.AddToFilter(JoinCondition.Or, CusEntryHeaderSchema.CH_MessageType, SQLComparisonOperator.Equal, CusEntryHeaderMessageTypeList.Codes.ReconEntry);

			result.AddToFilter(messageTypeQuery);
			return result;
		}
	}
}
