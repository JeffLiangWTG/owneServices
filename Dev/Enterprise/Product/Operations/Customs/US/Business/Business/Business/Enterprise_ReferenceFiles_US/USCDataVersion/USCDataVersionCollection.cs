using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business
{
	public class USCDataVersionCollection : BusinessObjectCollection<USCDataVersion>
	{
		public USCDataVersionCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override ZQuery CreateRelationshipFilter()
		{
			var result = base.CreateRelationshipFilter();
			result.AddToFilter(JoinCondition.And, USCDataVersionSchema.UZ_Name, SQLComparisonOperator.NotEqual, ScheduleBDataVersion);
			return result;
		}

		const string ScheduleBDataVersion = "ScheduleBDataVersion";
	}
}
