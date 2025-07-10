using System.Linq;
using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class OrgCompanyDataDependentCollection : DependentBusinessObjectCollection<OrgCompanyData, OrgHeader>
	{
		public OrgCompanyDataDependentCollection(OrgHeader parent) : base(parent)
		{
		}

		protected override bool ElementCanBeAdded(BusinessObject bizO)
		{
			var companyData = (OrgCompanyData)bizO;

			if (this.Cast<OrgCompanyData>().Any(x => x.OB_GC == companyData.OB_GC && x.OB_OH == companyData.OB_OH && x.PK != companyData.PK))
			{
				bizO.AddRowError(Res.GetString("40f96166-0714-4168-96f1-07e98b9f224e", "This company ({0}) has already been added to the Organization ({1}).", companyData.Company?.GC_Code, companyData.Code));
			}

			return base.ElementCanBeAdded(bizO);
		}
	}
}
