using System.Xml;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Registry.Business;

namespace Enterprise.Packing.Business
{
	[XmlSerializerAssembly("Enterprise.Packing.Business.XmlSerializers")]
	public class PalletType : RegistryBusinessObject
	{
		#region Properties

		[ResourceStringData("2c22b9ef-8a6d-48fc-9e3a-66b37d797062", Caption = "Provider Code")]
		public ZString ProviderCode
		{
			get { return providerCode; }
			set { SetNonPersistentPropertyValue(ProviderCodeInfo, ref providerCode, value); }
		}

		ZString providerCode;

		public ZPropertyInfo ProviderCodeInfo
		{
			get { return GetZPropertyInfo(nameof(ProviderCode)); }
		}

		[ResourceStringData("4a61c9ef-8a6d-48fc-9e3a-66b37d797071", Caption = "Equipment Code")]
		public ZString EquipmentCode
		{
			get { return equipmentCode; }
			set { SetNonPersistentPropertyValue(EquipmentCodeInfo, ref equipmentCode, value); }
		}

		ZString equipmentCode;

		public ZPropertyInfo EquipmentCodeInfo
		{
			get { return GetZPropertyInfo(nameof(EquipmentCode)); }
		}

		protected override int MaxDescriptionLength
		{
			get { return 100; }
		}

		#endregion

		protected override RegistryBusinessObjectTemplate GetClone(ZArchitecture.Environment.FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new PalletType();
		}

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);
			writer.WriteElementString(ProviderCodeInfo.Name, ProviderCode);
			writer.WriteElementString(EquipmentCodeInfo.Name, EquipmentCode);
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			base.ReadElements(reader);
			ProviderCode = reader.ReadElementString(ProviderCodeInfo.Name);
			EquipmentCode = reader.ReadElementString(EquipmentCodeInfo.Name);
		}
	}
}
