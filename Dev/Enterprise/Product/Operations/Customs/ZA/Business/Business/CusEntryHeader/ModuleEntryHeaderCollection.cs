using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ZA.Business
{
	[ModuleID(ModuleId.EntryHeader)]
	public class ModuleEntryHeaderCollection : Customs.Business.ModuleEntryHeaderCollection
	{
		public ModuleEntryHeaderCollection(BusinessObjectFactory factory)
			: base(factory, GlbCompany.CurrentCompany, GetDeclarationIsBuiltinFilter())
		{
		}

		static ZQuery GetDeclarationIsBuiltinFilter()
		{
			var entryHeaderQuery = new ZDBOnlyQuery(typeof(CusEntryHeader));
			var jobDeclarationQuery = new ZDBOnlySubQuery(typeof(JobDeclaration), CusEntryHeaderSchema.CH_JE);
			jobDeclarationQuery.AddToFilter(JobDeclarationSchema.JE_ApplicationCode, SQLComparisonOperator.Equal, Customs.Business.DeclarationApplicationCodeList.Codes.Builtin);
			entryHeaderQuery.AddSubQuery(jobDeclarationQuery, JoinCondition.And);

			return entryHeaderQuery;
		}

		public new CusEntryHeader this[int index]
		{
			get { return (CusEntryHeader)base[index]; }
		}

		public new CusEntryHeader AddNew()
		{
			return (CusEntryHeader)base.AddNew();
		}
	}
}
