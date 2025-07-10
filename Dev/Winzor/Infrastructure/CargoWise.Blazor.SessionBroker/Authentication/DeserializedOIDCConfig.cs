using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using CargoWise.Types;

namespace CargoWise.Blazor.SessionBroker.Authentication
{
	[XmlRoot("OIDCConfig")]
	public class DeserializedOIDCConfig : Enterprise.Integration.IOIDCConfig
	{
		public ZString AuthorityURL { get; set; }

		[XmlArray("ArrayOfOIDCClaimsMapping")]
		[XmlArrayItem("OIDCClaimsMapping")]
		public List<DeserializedOIDCClaimsMapping> ClaimsMappings { get; set; } = new List<DeserializedOIDCClaimsMapping>();

		public ZString ClientIdentifier { get; set; }

		[XmlIgnore]
		public ZBool IsOIDCEnabled { get; set; }

		[XmlElement("IsOIDCEnabled")]
		public string IsOIDCEnabledSerialize
		{
			get => IsOIDCEnabled.ToString();
			set => IsOIDCEnabled = ZBool.ParseSafe(value, false);
		}

		public static class OIDCServerTypesList_Codes
		{
			public const string Azure = "AZU";
			public const string Generic = "GEN";
			public const string Okta = "OKT";
		}

		[System.Obsolete("use DomainHint instead.")]
		public Enterprise.Integration.OIDCServerTypes OIDCServerType
			=> (string)OIDCServerTypeCode switch
			{
				OIDCServerTypesList_Codes.Okta => Enterprise.Integration.OIDCServerTypes.Okta,
				OIDCServerTypesList_Codes.Azure => Enterprise.Integration.OIDCServerTypes.Azure,
				_ => Enterprise.Integration.OIDCServerTypes.Generic,
			};

		/// <summary>
		/// Obsolete: use DomainHint instead
		/// </summary>
		public ZString OIDCServerTypeCode { get; set; }

		public List<ZString> OIDCServerTypesList { get; set; } = new List<ZString>();

		[XmlArray("ArrayOfOIDCScope")]
		[XmlArrayItem("OIDCScope")]
		public List<DeserializedOIDCScope> Scopes { get; set; } = new List<DeserializedOIDCScope>();

		IEnumerable<Enterprise.Integration.IOIDCClaimsMapping> Enterprise.Integration.IOIDCConfig.ClaimsMappings
			=> this.ClaimsMappings.Select(i => i as Enterprise.Integration.IOIDCClaimsMapping);

		IEnumerable<ZString> Enterprise.Integration.IOIDCConfig.OIDCServerTypesList
			=> this.OIDCServerTypesList;

		IEnumerable<Enterprise.Integration.IOIDCScope> Enterprise.Integration.IOIDCConfig.Scopes
			=> this.Scopes.Select(i => i as Enterprise.Integration.IOIDCScope);
	}

	[XmlRoot("OIDCClaimsMapping")]
	public class DeserializedOIDCClaimsMapping : Enterprise.Integration.IOIDCClaimsMapping
	{
		public ZString ClaimName { get; set; }

		public ZString Identifier { get; set; }

		public List<ZString> OIDCClaimMappingIdentifiers { get; set; } = new List<ZString>();

		IEnumerable<ZString> Enterprise.Integration.IOIDCClaimsMapping.OIDCClaimMappingIdentifiers
			=> this.OIDCClaimMappingIdentifiers;
	}

	[XmlRoot("OIDCScope")]
	public class DeserializedOIDCScope : Enterprise.Integration.IOIDCScope
	{
		public ZString ScopeName { get; set; }
	}
}
