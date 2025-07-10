using System;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Module
{
	sealed class ExceptionTypeNestedModuleFilterSubGroup : ModuleFilterSubGroup
	{
		readonly Type type;
		readonly SchemaColumn keyColumn;
		readonly SchemaColumn parentReferenceKeyColumn;

		public ExceptionTypeNestedModuleFilterSubGroup(Type type, SchemaColumn keyColumn, SchemaColumn parentReferenceKeyColumn)
		{
			this.type = type;
			this.keyColumn = keyColumn;
			this.parentReferenceKeyColumn = parentReferenceKeyColumn;
		}

		public override ZQuery GetSubQuery(ZQuery filter)
		{
			var subSubQuery = new ZDBOnlySubQuery(type, keyColumn, parentReferenceKeyColumn);
			subSubQuery.AddToFilter(filter);

			var subQuery = new ZDBOnlySubQuery(typeof(ProcessWorkflowException), ProcessWorkflowExceptionSchema.WEX_P9_ProcessTask);
			subQuery.AddSubQuery(subSubQuery, JoinCondition.And);

			var query = new ZDBOnlyQuery(typeof(ProcessTask));
			query.AddSubQuery(subQuery, JoinCondition.And);
			return query;
		}
	}
}
