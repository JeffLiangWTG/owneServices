using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	[ModuleID(ModuleId.NumericCodeRefAirline)]
	public class NumericCodeRefAirlineCollection : ActiveBusinessObjectCollection<NumericCodeRefAirline>
	{
		public NumericCodeRefAirlineCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		public NumericCodeRefAirlineCollection(BusinessObjectFactory factory, ZQuery filter) : base(factory, filter)
		{
		}

		protected override ZQuery CreateRelationshipFilter()
		{
			ZQuery filter = base.CreateRelationshipFilter();
			filter.AddToFilter(RefAirlineSchema.RM_EagleAddedAirlinePrefixOrAccountingCode, SQLComparisonOperator.NotEqual, ZString.Empty);
			return filter;
		}

		protected override IFindBoxListProvider FindBoxListProvider
		{
			get { return new NumericCodeRefAirlineFindBoxListProvider(this); }
		}
	}
}
