using CargoWise.EntityFramework;
using Enterprise.Customs.Business.BusinessObjects.Interfaces;

namespace Enterprise.Customs.TR.Business.Declaration
{
	public partial class CusEntryHeader
	{
		public new JobDeclaration Declaration => (JobDeclaration)base.Declaration;

		[ChildEditable(true)]
		public new Customs.Business.IAllCusEntryLineCollection<CusEntryLine> AllEntryLines => (Customs.Business.IAllCusEntryLineCollection<CusEntryLine>)base.AllEntryLines;

		protected override Customs.Business.IAllCusEntryLineCollection<Customs.Business.CusEntryLine> GetAllEntryLinesCollection() => new Customs.Business.AllCusEntryLineCollection<CusEntryLine>(this);

		[ChildEditable(true)]
		public new Customs.Business.CusEntryPayInfoCollection<CusEntryPayInfo> EntryPayInfos => (Customs.Business.CusEntryPayInfoCollection<CusEntryPayInfo>)base.EntryPayInfos;

		protected override ICusEntryPayInfoCollection<Customs.Business.CusEntryPayInfo> CreateNewEntryPayInfosCollection()
		{
			return new Customs.Business.CusEntryPayInfoCollection<CusEntryPayInfo>(this);
		}

		public new CusEntryHeaderLookups Lookups
		{
			get { return (CusEntryHeaderLookups)base.Lookups; }
		}

		protected override Customs.Business.CusEntryHeaderLookups GetNewLookups()
		{
			return new CusEntryHeaderLookups(this);
		}
	}
}
