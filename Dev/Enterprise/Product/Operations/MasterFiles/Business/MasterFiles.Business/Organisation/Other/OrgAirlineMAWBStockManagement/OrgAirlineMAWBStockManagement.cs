using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.MasterFiles.Business
{
	[CodeAlive("See WI00292226")]
	public class OrgAirlineMAWBStockManagement : AutoOrgAirlineMAWBStockManagement
	{
		public OrgAirlineMAWBStockManagement(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		[List("Lookups.AllowUseOtherBranchStocks")]
		public override ZString OHM_AllowUseOtherBranchStock
		{
			get { return base.OHM_AllowUseOtherBranchStock; }
			set
			{
				base.OHM_AllowUseOtherBranchStock = value;
				if (!IsValidationSuspended)
				{
					Validation.ValidateOHM_AllowUseOtherBranchStock();
				}
			}
		}
	}
}
