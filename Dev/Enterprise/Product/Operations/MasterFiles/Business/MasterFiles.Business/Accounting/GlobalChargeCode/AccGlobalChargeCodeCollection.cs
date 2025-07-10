using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	[ModuleID(ModuleId.AccChargeCode)]
	public class AccGlobalChargeCodeCollection : ActiveBusinessObjectCollection<AccChargeCode>
	{
		public AccGlobalChargeCodeCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		public AccGlobalChargeCodeCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter)
		{
		}

		protected override ZQuery CreateRelationshipFilter()
		{
			return new ZQuery(AccChargeCodeSchema.AC_GC, SQLComparisonOperator.Equal, null);
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
