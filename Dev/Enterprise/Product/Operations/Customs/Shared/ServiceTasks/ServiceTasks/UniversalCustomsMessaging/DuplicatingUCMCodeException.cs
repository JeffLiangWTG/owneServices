using System;

namespace Enterprise.Customs.ServiceTasks.UniversalCustomsMessaging;

[Serializable]
public class DuplicatingUCMCodeException : Exception
{
	public DuplicatingUCMCodeException(string duplicatingApplicationCodesDescription, Type attributeType, string duplicateCodeName)
		: base($"Each [assembly:{attributeType.FullName}] instance must specify unique {duplicateCodeName}. Found duplicates: {duplicatingApplicationCodesDescription}")
	{ }

	public DuplicatingUCMCodeException(string duplicatingApplicationCodesDescription, Type attributeType, Exception ex, string duplicateCodeName)
		: base($"Each [assembly:{attributeType.FullName}] instance must specify unique {duplicateCodeName}. Found duplicates: {duplicatingApplicationCodesDescription}", ex)
	{ }

#if NETFRAMEWORK
	[Obsolete("SYSLIB0051: Legacy serialization support APIs are obsolete")]
	protected DuplicatingUCMCodeException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context)
	{ }
#endif
}
