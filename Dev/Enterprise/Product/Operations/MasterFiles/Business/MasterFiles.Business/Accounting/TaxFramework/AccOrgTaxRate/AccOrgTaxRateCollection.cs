using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class AccOrgTaxRateCollection : ActiveBusinessObjectCollection<AccOrgTaxRate>
	{
		public AccOrgTaxRateCollection(AccOrgTaxConfiguration master)
			: base(master.Factory, master, null, AccOrgTaxRateSchema.OTR_OTC)
		{
		}
	}
}
