using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.DataRegistry.Business
{
	[XmlSerializerAssembly("Enterprise.Customs.Business.XmlSerializers")]
	public class ASNRefreshOptionsConfig : RegistryBusinessObjectTemplate
	{
		public ASNRefreshOptionsConfig()
		{ }

		public ASNRefreshOptionsConfig(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory)
		{ }

		#region Schema

		public static class Schema
		{
			public const string FieldType = "FieldType";
		}

		#endregion

		#region Properties

		[List(nameof(Lookups) + "." + nameof(Lookups.FieldTypeList))]
		public ZString FieldType
		{
			get => fieldType;
			set
			{
				if (fieldType != value)
				{
					SetNonPersistentPropertyValue(FieldTypeInfo, ref fieldType, value);
					FieldTypeInfo.RefreshBinding();
					if (!IsValidationSuspended)
					{
						ValidateFieldType();
					}
				}
			}
		}
		ZString fieldType;

		public ZPropertyInfo FieldTypeInfo => GetZPropertyInfo(Schema.FieldType);

		public ZString FieldTypeDesc => Lookups.FieldTypeList.GetDescriptionFromCode(FieldType);

		public void ValidateFieldType()
		{
			FieldTypeInfo.ClearAllNotifications();

			ListValidation.ErrorIfInvalidCode(FieldTypeInfo);

			if (!FieldTypeInfo.HasErrors())
			{
				var collection = GetParentCollection(this, typeof(ASNRefreshOptionsConfigCollection));
				if (collection != null && collection.Cast<ASNRefreshOptionsConfig>().Any(x => x.FieldType == FieldType && x != this))
				{
					FieldTypeInfo.AddError(DuplicatedCodesError);
				}
			}
		}

		internal static string DuplicatedCodesError => Enterprise.Customs.Business.ResString.GetMultilingualString("3bfd16fd-9840-4aec-956c-50017552831c", "The codes cannot be duplicated.");

		#endregion

		#region Lookups

		public ASNRefreshOptionsConfigLookups Lookups => fLookups ?? (fLookups = new ASNRefreshOptionsConfigLookups(this, CurrentFactory));
		ASNRefreshOptionsConfigLookups fLookups;

		#endregion

		#region Override

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new ASNRefreshOptionsConfig(fallbackLevel, factory);
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			FieldType = reader.ReadElementString(Schema.FieldType);
		}

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);
			writer.WriteElementString(Schema.FieldType, FieldType);
		}

		#endregion
	}
}
