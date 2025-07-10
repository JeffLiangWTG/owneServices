namespace Enterprise.Customs.TR.Business.Declaration
{
	public partial class CusEntryPayInfo : Customs.Business.CusEntryPayInfo
	{
		public new CusEntryPayInfoLookups Lookups => (CusEntryPayInfoLookups)base.Lookups;

		protected override Customs.Business.CusEntryPayInfoLookups GetNewLookups() => new CusEntryPayInfoLookups(this);
	}
}
