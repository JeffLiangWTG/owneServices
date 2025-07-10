using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class RefUnlocoIATACollection : ActiveBusinessObjectCollection<RefUnlocoIATA>
	{
		public RefUnlocoIATACollection(BusinessObjectFactory factory)
			: base(factory)
		{ }

		protected override ZQuery CreateRelationshipFilter()
		{
			var filter = base.CreateRelationshipFilter();
			filter.AddToFilter(RefUNLOCOSchema.RL_IATA, SQLComparisonOperator.NotEqual, ZString.Empty);
			return filter;
		}

		protected override bool AllowNew
		{
			get { return false; }
		}
	}
}
