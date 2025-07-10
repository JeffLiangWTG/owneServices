using System.Data;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.MasterFiles.Business
{
	[Immutable]
	public class ProcessTaskRowFetchStrategy
	{
		public void FetchForLoad(BusinessObjectFactory factory, DataRow[] rows)
		{
			FetchForLoadCore(factory, rows);
		}

		protected virtual void FetchForLoadCore(BusinessObjectFactory factory, DataRow[] rows)
		{
			var schemaResolver = ObjectFactory.Get<IApplicationSchemaResolver>();
			foreach (var row in rows)
			{
				var schema = schemaResolver.GetTableSchemaFromColumnNamePrefix(row[ProcessTasksSchema.P9_ParentTableCode.Name].ToString());
				if (schema != null)
				{
					factory.AddFetchHint(schema.PK, new ZGuid(row[ProcessTasksSchema.P9_ParentID.Name]));
				}
			}
		}
	}
}
