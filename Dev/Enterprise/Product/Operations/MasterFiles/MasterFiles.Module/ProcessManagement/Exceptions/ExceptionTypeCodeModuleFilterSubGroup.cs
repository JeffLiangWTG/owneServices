using System;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Module
{
	sealed class ExceptionTypeCodeModuleFilterSubGroup : ModuleFilterSubGroup
	{
		readonly Type type;
		readonly SchemaColumn keyColumn;

		public ExceptionTypeCodeModuleFilterSubGroup(Type type, SchemaColumn keyColumn)
		{
			this.type = type;
			this.keyColumn = keyColumn;
		}

		public override ZQuery GetSubQuery(ZQuery filter)
		{
			var subQuery = new ZDBOnlySubQuery(type, keyColumn, ProcessTasksSchema.P9_SE_NKExceptionEvent);
			subQuery.AddToFilter(filter);

			var query = new ZDBOnlyQuery(typeof(ProcessTask));
			query.AddSubQuery(subQuery, JoinCondition.And);
			return query;
		}
	}
}
