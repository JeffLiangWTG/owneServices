using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class AccSurchargeApplicationCollection : BusinessObjectCollection<AccSurchargeApplication>
	{
		public AccSurchargeApplicationCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		public AccSurchargeApplicationCollection(BusinessObjectFactory factory, ZGuid companyPK) : base(factory, GetZQuery(companyPK))
		{
			this.CompanyPK = companyPK;
		}

		ZGuid CompanyPK { get; }

		static ZQuery GetZQuery(ZGuid companyPK)
		{
			return new ZQuery(AccSurchargeApplicationSchema.ASP_GC_Company, companyPK);
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			var config = (AccSurchargeApplication)child;
			config.ASP_GC_Company = CompanyPK;
		}

		protected override void OnRemoved(BusinessObject bizO)
		{
			base.OnRemoved(bizO);
			var company = Factory.Load<GlbCompany>(CompanyPK);
			AccSurchargeApplicationLogHelper.AddLog(company, bizO, true);
		}
	}
}
