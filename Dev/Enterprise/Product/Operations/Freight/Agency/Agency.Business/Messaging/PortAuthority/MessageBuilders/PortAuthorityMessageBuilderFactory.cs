using System;

namespace Enterprise.Freight.Agency.Business
{
	public static class PortAuthorityMessageBuilderFactory
	{
		public static IPortAuthorityMessageBuilder GetBuilder(string version)
		{
			if (version == null)
			{
				throw new ArgumentNullException(nameof(version));
			}

			switch (version)
			{
				case PortAuthorityVersionList.Codes.V11:
					return new IFCSUM11();
				case PortAuthorityVersionList.Codes.V20:
					return new IFCSUM20();
				default:
					throw new ArgumentOutOfRangeException(nameof(version), version, "Unsupported messaging version.");
			}
		}
	}
}


