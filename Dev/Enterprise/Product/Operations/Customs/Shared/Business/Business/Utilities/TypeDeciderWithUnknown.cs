using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Customs.Business.Extensions;
using Enterprise.Customs.Business.Interfaces;

namespace Enterprise.Customs.Business
{
	public abstract class TypeDeciderWithUnknown<T, TUnknown> : TypeDecider
		where T : BusinessObject, IOldParentIDProvider
		where TUnknown : BusinessObject, IOldParentIDProvider
	{
		public override Type GetTypeForLoad(DataRow row, BusinessObjectFactory factory)
		{
			Type result = null;
			if (row != null)
			{
				var parentID = factory.GetOldParentIDIfNeeded<T>(new ZGuid(row[ParentIDColumn.Name]), () => (Guid)row[PKColumn.Name]);
				result = GetTypeForLoad(new ZString(row[TypeColumn.Name]), new ZString(row[ParentTableCodeColumn.Name]), parentID, factory);
			}
			return result;
		}

		public Type GetTypeForLoad(IColumnIndexer row, BusinessObjectFactory factory)
		{
			Type result = null;
			if (row != null)
			{
				var parentID = factory.GetOldParentIDIfNeeded<T>(row.GetValue(ParentIDColumn), () => row.GetValue(PKColumn).ToGuid());
				result = GetTypeForLoad(row.GetValue(TypeColumn), row.GetValue(ParentTableCodeColumn), parentID, factory);
			}
			return result;
		}

		public Type GetTypeForLoad(ZString type, ZString parentTableCode, ZGuid parentPK, BusinessObjectFactory factory)
		{
			var result = GetTypeForLoadCore(type, parentTableCode, parentPK, factory);
			return factory.IsUnknownEnabled<T>() && (result == null || result.GetType().IsAbstract) ? typeof(TUnknown) : result;
		}

		protected abstract Type GetTypeForLoadCore(ZString type, ZString parentTableCode, ZGuid parentPK, BusinessObjectFactory factory);

		protected abstract SchemaGuidColumn PKColumn { get; }
		protected abstract SchemaGuidColumn ParentIDColumn { get; }
		protected abstract SchemaStringColumn ParentTableCodeColumn { get; }
		protected abstract SchemaStringColumn TypeColumn { get; }
	}
}
