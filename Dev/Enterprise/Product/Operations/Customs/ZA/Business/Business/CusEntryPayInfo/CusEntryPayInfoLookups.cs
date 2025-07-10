using CargoWise.Integration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ZA.Business
{
	public class CusEntryPayInfoLookups : Customs.Business.CusEntryPayInfoLookups
	{
		public CusEntryPayInfoLookups(CusEntryPayInfo parent) : base(parent) { }

		public OrgHeaderCollection Importers
		{
			get { return new OrgHeaderCollection(Factory); }
		}

		public ICodeDescriptionPairList CustomsOffices
		{
			get { return ((CusEntryPayInfo)Parent)?.Declaration?.Lookups.CustomsOfficeList; }
		}
	}
}
