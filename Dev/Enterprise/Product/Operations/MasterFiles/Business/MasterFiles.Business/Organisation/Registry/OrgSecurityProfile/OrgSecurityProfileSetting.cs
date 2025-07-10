using System.ComponentModel;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business
{
	[XmlSerializerAssembly("Enterprise.MasterFiles.Business.XmlSerializers")]
	public class OrgSecurityProfileSetting : RegistryBusinessObjectTemplate
	{
		public abstract class Schema
		{
			public const string SecurityKey = "SecurityKey";
			public const string Granted = "Granted";
			public const string CustomerManaged = "CustomerManaged";
			public const string SecurityItemNameForDisplay = "SecurityItemNameForDisplay";
		}

		public ZString SecurityKey { get; set; }

		[ReadOnly(true)]
		public ZString SecurityItemNameForDisplay
		{
			get { return securityItemNameForDisplay; }
			set
			{
				SetNonPersistentPropertyValue(SecurityItemNameForDisplayInfo, ref securityItemNameForDisplay, value);
			}
		}

		public ZPropertyInfo SecurityItemNameForDisplayInfo => GetZPropertyInfo(Schema.SecurityItemNameForDisplay);

		ZString securityItemNameForDisplay;

		public ZBool Granted
		{
			get { return granted; }
			set
			{
				SetNonPersistentPropertyValue(GrantedInfo, ref granted, value);
			}
		}

		public ZPropertyInfo GrantedInfo => GetZPropertyInfo(Schema.Granted);

		ZBool granted;

		public ZBool CustomerManaged
		{
			get { return customerManaged; }
			set
			{
				SetNonPersistentPropertyValue(CustomerManagedInfo, ref customerManaged, value);
			}
		}

		public ZPropertyInfo CustomerManagedInfo => GetZPropertyInfo(Schema.CustomerManaged);

		ZBool customerManaged;

		protected override void CopyValuesToClone(RegistryBusinessObjectTemplate clone)
		{
			base.CopyValuesToClone(clone);

			var setting = (OrgSecurityProfileSetting)clone;
			setting.SecurityKey = SecurityKey;
			setting.SecurityItemNameForDisplay = SecurityItemNameForDisplay;
			setting.Granted = Granted;
			setting.CustomerManaged = CustomerManaged;
		}

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);

			writer.WriteElementString(Schema.SecurityKey, SecurityKey);
			writer.WriteElementString(Schema.Granted, Granted.ToString());
			writer.WriteElementString(Schema.CustomerManaged, CustomerManaged.ToString());
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			SecurityKey = reader.ReadElementString(Schema.SecurityKey);
			Granted = new ZBool(reader.ReadElementString(Schema.Granted));
			CustomerManaged = new ZBool(reader.ReadElementString(Schema.CustomerManaged));
		}

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new OrgSecurityProfileSetting();
		}
	}
}
