using System.Linq;
using System.Text;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class StmALogEntryLocator
	{
		protected StmALogEntryLocator()
		{
		}

		public StmALog GetLastPostEventOfType(BusinessObject businessObject, params Event[] eventTypes)
		{
			return GetLastPostEventOfType(businessObject.PK, businessObject.Factory, eventTypes);
		}

		public StmALog GetLastPostEventOfType(ZGuid pk, BusinessObjectFactory factory, params Event[] eventTypes)
		{
			return GetLastPostEvent(pk.ToString(), eventTypes.Select(et => et.Code).ToArray(), null, factory);
		}

		public StmALog GetLastPostEvent(string parentBusinessObjectPkString, string[] typeCodes, string reference, BusinessObjectFactory factory)
		{
			Argument.NotNullOrEmpty(parentBusinessObjectPkString, "parentBusinessObjectPkString");
			Argument.NotNull(factory, "factory");
			// Create the result query.
			var resultQuery = new ZQuery { OrderBy = AutoStmALog.Schema.SL_EventTime + OrderByClause.Descending };
			// Add filter for "parentBusinessObjectPk".
			ZGuid parentBusinessObjectPk;
			if (!ZGuid.TryParse(parentBusinessObjectPkString, out parentBusinessObjectPk))
			{
				return null;
			}
			resultQuery.AddToFilter(StmALogSchema.SL_Parent, parentBusinessObjectPk);
			// Add filter for "typeCodes".
			var typeSubQuery = new ZQuery();
			typeSubQuery.AddToFilter(StmALogSchema.SL_SE_NKEvent, typeCodes);
			resultQuery.AddToFilter(typeSubQuery);
			if (!string.IsNullOrEmpty(reference))
			{
				// Add filter for "reference".
				reference = new StringBuilder(reference).Replace('*', '%').Replace('?', '_').ToString();
				resultQuery.AddToFilter(StmALogSchema.SL_Reference, SQLComparisonOperator.Like, reference);
			}
			// Fetch the event.
			return factory.LoadTop1<StmALog>(resultQuery);
		}

		public static readonly StmALogEntryLocator Instance = new StmALogEntryLocator();
	}
}