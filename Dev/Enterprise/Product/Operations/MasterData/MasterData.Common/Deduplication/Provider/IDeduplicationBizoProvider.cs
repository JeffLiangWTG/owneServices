using System;

namespace Enterprise.MasterData.Common
{
	public interface IDeduplicationBizoProvider : IDeduplicationMultiSourceProvider, IDeduplicationProvider
	{
		Type BusinessObjectType { get; }
		string TablePrefix { get; }
		string GetHeading(IDeduplicationGlowObject source);
		IDeduplicationGlowObject GetChildObject(IDeduplicationMaster headerParent, Guid childObjectPK);
	}
}
