using System.ComponentModel;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business
{
	[XmlSerializerAssembly("Enterprise.MasterFiles.Business.XmlSerializers")]
	public class EPaymentConfiguration : RegistryBusinessObjectTemplate
	{
		#region Schema

		public abstract class Schema
		{
			public const string CountryCode = nameof(CountryCode);
			public const string CountryDescription = nameof(CountryDescription);
			public const string OFXEPaymentEnabled = nameof(OFXEPaymentEnabled);
		}

		#endregion

		#region Bound Properties

		#region CountryCode

		[ResourceStringData("ba71efe8-7b55-4c2a-a3da-d03a23e2230e", Caption = "Country/Region Code")]
		[ReadOnly(true)]
		public ZString CountryCode
		{
			get => countryCode;
			set
			{
				SetNonPersistentPropertyValue(CountryCodeInfo, ref countryCode, value);
			}
		}
		ZString countryCode;

		public ZPropertyInfo CountryCodeInfo => GetZPropertyInfo(Schema.CountryCode);

		#endregion

		#region CountryDescription

		[ResourceStringData("d70a8c21-f095-4a65-912b-b5695af78fc4", Caption = "Country/Region Description")]
		[ReadOnly(true)]
		public ZString CountryDescription
		{
			get => countryDescription;
			set
			{
				SetNonPersistentPropertyValue(CountryDescriptionInfo, ref countryDescription, value);
			}
		}
		ZString countryDescription;

		public ZPropertyInfo CountryDescriptionInfo => GetZPropertyInfo(Schema.CountryDescription);

		#endregion

		#region EPaymentEnabledForOFX

		[ResourceStringData("c3a08d23-9c27-4a8e-b12d-481965f05784", Caption = "E-Payment Enabled For OFX")]
		public ZBool OFXEPaymentEnabled
		{
			get => ofxEPaymentEnabled;
			set
			{
				SetNonPersistentPropertyValue(OFXEPaymentEnabledInfo, ref ofxEPaymentEnabled, value);
			}
		}
		ZBool ofxEPaymentEnabled;

		public ZPropertyInfo OFXEPaymentEnabledInfo => GetZPropertyInfo(Schema.OFXEPaymentEnabled);

		#endregion

		#endregion

		#region CanDelete

		public override bool CanDelete => false;

		public override MultilingualString ReasonForNotAbleToDelete => ResString.GetMultilingualString("9ff93ed9-b327-44a6-a3f5-7f3e08472e0c", "Cannot delete the default E-Payment Configuration.");

		#endregion

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new EPaymentConfiguration();
		}

		#region Xml Serialisation

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);
			writer.WriteElementString(Schema.CountryCode, CountryCode);
			writer.WriteElementString(Schema.CountryDescription, CountryDescription);
			writer.WriteElementString(Schema.OFXEPaymentEnabled, OFXEPaymentEnabled.ToString());
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			CountryCode = reader.ReadElementString(Schema.CountryCode);
			CountryDescription = reader.ReadElementString(Schema.CountryDescription);
			OFXEPaymentEnabled = reader.ReadElementStringAsZBool(Schema.OFXEPaymentEnabled);
		}

		#endregion
	}
}
