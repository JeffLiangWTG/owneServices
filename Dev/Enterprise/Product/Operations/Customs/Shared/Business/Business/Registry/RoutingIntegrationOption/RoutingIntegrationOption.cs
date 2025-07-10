using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.DataRegistry.Business
{
	[XmlSerializerAssembly("Enterprise.Customs.Business.XmlSerializers")]
	public class RoutingIntegrationOptions : RegistryBusinessObjectTemplate
	{
		public static class Schema
		{
			public const string AlwaysLink = "AlwaysLink";
			public const string NeverLink = "NeverLink";
			public const string ConditionalLink = "ConditionalLink";
		}

		#region Properties

		#region AlwaysLink

		public ZBool AlwaysLink
		{
			get { return alwaysLink; }
			set
			{
				SetNonPersistentPropertyValue(AlwaysLinkInfo, ref alwaysLink, value);
				if (AlwaysLink)
				{
					NeverLink = ConditionalLink = false;
				}
			}
		}
		ZBool alwaysLink;

		public ZPropertyInfo AlwaysLinkInfo
		{
			get { return GetZPropertyInfo(Schema.AlwaysLink); }
		}

		#endregion

		#region NeverLink

		public ZBool NeverLink
		{
			get { return neverLink; }
			set
			{
				SetNonPersistentPropertyValue(NeverLinkInfo, ref neverLink, value);
				if (NeverLink)
				{
					AlwaysLink = ConditionalLink = false;
				}
			}
		}
		ZBool neverLink;

		public ZPropertyInfo NeverLinkInfo
		{
			get { return GetZPropertyInfo(Schema.NeverLink); }
		}

		#endregion

		#region ConditionalLink

		public ZBool ConditionalLink
		{
			get { return conditionalLink; }
			set
			{
				SetNonPersistentPropertyValue(ConditionalLinkInfo, ref conditionalLink, value);
				if (ConditionalLink)
				{
					AlwaysLink = NeverLink = false;
				}
			}
		}
		ZBool conditionalLink;

		public ZPropertyInfo ConditionalLinkInfo
		{
			get { return GetZPropertyInfo(Schema.ConditionalLink); }
		}

		#endregion

		#endregion

		#region Implementation

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			var result = new RoutingIntegrationOptions();

			result.AlwaysLink = AlwaysLink;
			result.NeverLink = NeverLink;
			result.ConditionalLink = ConditionalLink;

			return result;
		}

		protected override void WriteElements(System.Xml.XmlWriter writer)
		{
			base.WriteElements(writer);
			writer.WriteElementString(Schema.AlwaysLink, AlwaysLink ? "Y" : "N");
			writer.WriteElementString(Schema.NeverLink, NeverLink ? "Y" : "N");
			writer.WriteElementString(Schema.ConditionalLink, ConditionalLink ? "Y" : "N");
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			AlwaysLink = reader.ReadElementString(Schema.AlwaysLink) == "Y";
			NeverLink = reader.ReadElementString(Schema.NeverLink) == "Y";
			ConditionalLink = reader.ReadElementString(Schema.ConditionalLink) == "Y";
		}

		#endregion
	}
}
