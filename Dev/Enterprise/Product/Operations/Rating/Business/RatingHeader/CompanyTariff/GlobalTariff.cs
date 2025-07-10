using System.Data;
using CargoWise.Definitions;
using CargoWise.EntityFramework;

namespace Enterprise.Rating.Business
{
	[BusinessContext(BusinessContext.Rating)]
	public class GlobalTariff : CompanyTariff
	{
		public GlobalTariff(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected override bool IsGlobalTariff
		{
			get { return true; }
		}
	}
}

