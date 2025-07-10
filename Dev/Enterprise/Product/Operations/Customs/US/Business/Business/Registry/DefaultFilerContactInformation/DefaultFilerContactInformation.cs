using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.US.DataRegistry.Business
{
	[XmlSerializerAssembly("Enterprise.Customs.US.Business.XmlSerializers")]
	public class DefaultFilerContactInformation : RegistryBusinessObjectTemplate
	{
		#region Schema
		public static class Schema
		{
			public const string ContactName = "ContactName";
			public const string ContactPhone = "ContactPhone";
			public const int ContactPhoneMaxLength = 15;
			public const int ContactNameMaxLength = 40;
		}
		#endregion

		public DefaultFilerContactInformation()
			: base()
		{
		}

		[MaxLength(Schema.ContactNameMaxLength)]
		public ZString ContactName
		{
			get => contactName;
			set
			{
				SetNonPersistentPropertyValue(ContactNameInfo, ref contactName, value);
			}
		}

		public ZPropertyInfo ContactNameInfo => GetZPropertyInfo(Schema.ContactName);

		ZString contactName;

		[MaxLength(Schema.ContactPhoneMaxLength)]
		public ZString ContactPhone
		{
			get => contactPhone;
			set
			{
				SetNonPersistentPropertyValue(ContactPhoneInfo, ref contactPhone, value);
			}
		}

		public ZPropertyInfo ContactPhoneInfo => GetZPropertyInfo(Schema.ContactPhone);

		ZString contactPhone;

		protected override void WriteElements(System.Xml.XmlWriter writer)
		{
			base.WriteElements(writer);
			writer.WriteElementString(Schema.ContactName, ContactName);
			writer.WriteElementString(Schema.ContactPhone, ContactPhone);
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			ContactName = reader.ReadElementString(Schema.ContactName);
			ContactPhone = reader.ReadElementString(Schema.ContactPhone);
		}

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new DefaultFilerContactInformation();
		}

		protected override void CopyValuesToClone(RegistryBusinessObjectTemplate clone)
		{
			base.CopyValuesToClone(clone);
			DefaultFilerContactInformation target = (DefaultFilerContactInformation)clone;

			target.ContactName = ContactName;
			target.ContactPhone = ContactPhone;
		}
	}
}
