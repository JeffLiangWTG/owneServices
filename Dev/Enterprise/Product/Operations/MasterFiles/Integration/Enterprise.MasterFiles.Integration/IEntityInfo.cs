using System;

namespace Enterprise.MasterFiles.Integration
{
	public interface IEntityInfo
	{
		Guid InternalPK { get; }
		string TableName { get; }
		Type Type { get; }
	}
}