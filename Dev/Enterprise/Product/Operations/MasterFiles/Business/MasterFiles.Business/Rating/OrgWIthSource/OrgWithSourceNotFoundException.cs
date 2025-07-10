using System;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	[Serializable]
	public class OrgWithSourceNotFoundException : Exception
	{
		public string HumanReadablePropertyName { get; private set; }

		public OrgWithSourceNotFoundException(ZPropertyInfo propertyInfo, ZGuid orgWithSourcePK)
			: this(propertyInfo.Name, propertyInfo.HasHumanReadableName, (string)propertyInfo.HumanReadableName, orgWithSourcePK)
		{
		}

		public OrgWithSourceNotFoundException(string propertyName, bool hasHumanReadablePropertyName, string humanReadablePropertyName, ZGuid orgWithSourcePK)
			: base(FormattableString.Invariant($"The Business Object could not be loaded: '{propertyName}' / '{orgWithSourcePK}'"))
		{
			HumanReadablePropertyName = hasHumanReadablePropertyName ? humanReadablePropertyName : propertyName;
		}

#if NETFRAMEWORK
		protected OrgWithSourceNotFoundException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context)
		{
		}
#endif
	}
}
