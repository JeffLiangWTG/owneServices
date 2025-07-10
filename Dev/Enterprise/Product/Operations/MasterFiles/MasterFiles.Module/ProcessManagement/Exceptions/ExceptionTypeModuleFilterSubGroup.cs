using System;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.MasterFiles.Module
{
	sealed class ExceptionTypeModuleFilterSubGroup : ModuleFilterSubGroup
	{
		readonly Type type;
		readonly SchemaColumn keyColumn;

		public ExceptionTypeModuleFilterSubGroup(Type type, SchemaColumn keyColumn)
		{
			this.type = type;
			this.keyColumn = keyColumn;
		}

		public override ZQuery GetSubQuery(ZQuery filter)
		{
			var query = new ZDBOnlyQuery(typeof(ProcessWorkflowExceptionType));
			var subQuery = new ZDBOnlySubQuery(type, keyColumn);
			subQuery.AddToFilter(filter);
			query.AddSubQuery(subQuery, JoinCondition.And);
			return query;
		}
	}
}
