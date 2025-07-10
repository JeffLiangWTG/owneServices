using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Rating.Business
{
	public class QuoteLogs : Logs
	{
		public QuoteLogs(Quote quote)
			: base(quote)
		{
		}

		public new Quote Parent
		{
			get { return (Quote)base.Parent; }
		}

		protected override BusinessObjectCollection GetNewElementsCollection()
		{
			var additionalQuery = new ZQuery(StmALogSchema.SL_Table, RatingHeaderSchema.Constants.TableName);
			return new StmALogDependentCollection(Parent, additionalQuery);
		}
	}
}

