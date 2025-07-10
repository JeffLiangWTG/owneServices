using System.Collections.Generic;
using Enterprise.Customs.ASYCUDA.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.ForwarderManifest.Business
{
	public class USManifestType : ManifestType
	{
		public USManifestType(string code, string description, IEnumerable<string> applicableTransportModes, IEnumerable<string> applicableManifestStyles, MessageLevel messageLevel)
			: base(code, description, applicableTransportModes, applicableManifestStyles, messageLevel)
		{
		}

		public USManifestType(string code, string description, string applicableTransportMode, string applicableManifestStyle, MessageLevel messageLevel)
			: base(code, description, applicableTransportMode, applicableManifestStyle, messageLevel)
		{
		}

		public USManifestType(string code, string description, string applicableTransportMode, IEnumerable<string> applicableManifestStyles, MessageLevel messageLevel)
			: base(code, description, applicableTransportMode, applicableManifestStyles, messageLevel)
		{
		}

		public USManifestType(string code, string description, IEnumerable<string> applicableTransportModes, string applicableManifestStyle, MessageLevel messageLevel)
			: base(code, description, applicableTransportModes, applicableManifestStyle, messageLevel)
		{
		}

		public USManifestType(string code, string description, IEnumerable<string> applicableTransportMode, IEnumerable<string> applicableManifestStyles, MessageLevel messageLevel, CodeDescriptionPairList manifestNatures)
			: base(code, description, applicableTransportMode, applicableManifestStyles, messageLevel, manifestNatures)
		{
		}

		public USManifestType(string code, string description, string applicableTransportMode, IEnumerable<string> applicableManifestStyles, MessageLevel messageLevel, CodeDescriptionPairList manifestNatures)
			: base(code, description, applicableTransportMode, applicableManifestStyles, messageLevel, manifestNatures)
		{
		}
	}
}
