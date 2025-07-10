using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	public class CusLiquidationCollection : ActiveBusinessObjectCollection<CusLiquidation>
	{
		public CusLiquidationCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public CusLiquidationCollection(JobDeclaration declaration)
			: base(declaration.Factory, declaration)
		{
		}

		public JobDeclaration Master
		{
			get { return (JobDeclaration)Relationship.Master; }
		}

		public ZDateTime GetMostRecentLiquidationDate()
		{
			var liquidation = GetMostRecentLiquidation();
			return liquidation != null ? liquidation.B8_LiquidationDate : ZDateTime.Empty;
		}

		public ZString GetMostRecentLiquidationType()
		{
			var liquidation = GetMostRecentLiquidation();
			return liquidation != null ? liquidation.B8_LiquidationType : ZString.Empty;
		}

		public CusLiquidation GetMostRecentLiquidation()
		{
			return this.OrderByDescending(x => x.B8_SystemCreateDate).FirstOrDefault();
		}
	}
}
