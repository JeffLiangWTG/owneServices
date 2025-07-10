using System;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.MasterFiles.Business
{
	public class EntityInfo : IEntityInfo
	{
		public static EntityInfo Empty = new EntityInfo("", Guid.Empty, null);

		public static EntityInfo New(BusinessObject businessObject)
		{
			if (businessObject == null)
			{
				return Business.EntityInfo.Empty;
			}

			return new EntityInfo(businessObject.TableName, businessObject.PK.ToGuid(), businessObject.GetType());
		}

		EntityInfo(string tableName, Guid internalPK, Type type)
		{
			this.TableName = tableName;
			this.InternalPK = internalPK;
			this.Type = type;
		}

		public string TableName { get; private set; }
		public Type Type { get; private set; }
		public Guid InternalPK { get; private set; }
	}
}
