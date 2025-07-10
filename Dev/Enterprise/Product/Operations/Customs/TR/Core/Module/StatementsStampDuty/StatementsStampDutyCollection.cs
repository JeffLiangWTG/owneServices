using CargoWise.EntityFramework;
using Enterprise.Customs.TR.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.TR.Module
{
	public class StatementsStampDutyCollection : BusinessObjectCollection<CusStatementHeader>
	{
		public StatementsStampDutyCollection(BusinessObjectFactory factory) : base(factory)
		{
		}
		protected override ZQuery CreateAdditionalFilter()
		{
			var result = base.CreateAdditionalFilter();
			result.AddToFilter(CusStatementHeaderSchema.B2_GC, GlbCompany.CurrentCompany.PK);
			return result;
		}
	}
}
