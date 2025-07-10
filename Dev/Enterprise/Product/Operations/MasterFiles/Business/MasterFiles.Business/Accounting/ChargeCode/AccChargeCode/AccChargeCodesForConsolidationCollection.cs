using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	[ModuleID(ModuleId.AccGlobalChargeCode)]
	public class AccChargeCodesForConsolidationCollection : ActiveBusinessObjectCollection<AccChargeCode>
	{
		public AccChargeCodesForConsolidationCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		protected override ZQuery CreateRelationshipFilter()
		{
			return new ZQuery(AccChargeCodeSchema.AC_GC, SQLComparisonOperator.NotEqual, null);
		}

		protected override bool AllowNew
		{
			get
			{
				return false;
			}
		}
	}
}
