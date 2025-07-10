using System.Xml;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Universal;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.TW.Business
{
	[XmlSerializerAssembly("Enterprise.Customs.TW.Business.XmlSerializers")]
	public class CusGoodsLocation : RegistryBusinessObjectTemplate
	{
		#region Constructors and Schema

		public CusGoodsLocation() : base()
		{
		}

		public CusGoodsLocation(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory)
		{
		}

		public new BusinessObjectFactory Factory => CurrentFactory ?? new BusinessObjectFactory { NameForDebugging = "TWCustomsRegistryCusGoodsLocation" };

		protected abstract class Schema
		{
			public const string GoodsLocation = "GoodsLocation";
			public const string CustomsOffice = "CustomsOffice";
			public const string MessageType = "MessageType";

			public const int GoodsLocationMaxLength = 20;
			public const int CustomsOfficeMaxLength = 2;
			public const int MessageTypeMaxLength = 3;
		}

		#endregion

		#region Overrides

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory) => new CusGoodsLocation(fallbackLevel, factory);

		protected sealed override void ReadElements(XmlReaderWrapper reader)
		{
			GoodsLocation = reader.ReadElementString(Schema.GoodsLocation);
			CustomsOffice = reader.ReadElementString(Schema.CustomsOffice);
			MessageType = reader.ReadElementString(Schema.MessageType);
		}

		protected sealed override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);
			writer.WriteElementString(Schema.GoodsLocation, GoodsLocation);
			writer.WriteElementString(Schema.CustomsOffice, CustomsOffice);
			writer.WriteElementString(Schema.MessageType, MessageType);
		}

		#endregion

		#region Properties
		#region CustomsOffice
		[Mandatory]
		[List(nameof(Lookups) + "." + nameof(CusGoodsLocationLookups.CustomsOfficeList))]
		[RelatedBusinessObject("CustomsOfficeItem")]
		[ResourceStringData("Enterprise.Customs.TW.Business.CusGoodsLocation|CustomsOffice", Caption = "Customs Office")]
		[MaxLength(Schema.CustomsOfficeMaxLength)]
		public ZString CustomsOffice
		{
			get => fCustomsOffice;
			set
			{
				if (CustomsOffice != value)
				{
					SetNonPersistentPropertyValue(CustomsOfficeInfo, ref fCustomsOffice, value);
					if (!IsValidationSuspended)
					{
						Validation.ValidateCustomsOffice();
						Validation.ValidateGoodsLocation();
					}
					CustomsOfficeInfo.RefreshBinding();
				}
			}
		}
		ZString fCustomsOffice;

		public ZPropertyInfo CustomsOfficeInfo => GetZPropertyInfo(Schema.CustomsOffice);

		public ZZRefCusCodeListCombined CustomsOfficeItem => TWRefCusCodeListLoader.GetCustomsOffice(Factory, CustomsOffice, ZDateTime.Today);
		#endregion

		#region GoodsLocation
		[Mandatory]
		[MaxLength(Schema.GoodsLocationMaxLength)]
		[List(nameof(Lookups) + "." + nameof(CusGoodsLocationLookups.GoodsLocationList))]
		[RelatedBusinessObject("GoodsLocationItem")]
		[ResourceStringData("Enterprise.Customs.TW.Business.CusGoodsLocation|GoodsLocation", Caption = "Goods Location")]
		public ZString GoodsLocation
		{
			get => fGoodsLocation;
			set
			{
				if (GoodsLocation != value)
				{
					SetNonPersistentPropertyValue(GoodsLocationInfo, ref fGoodsLocation, value);
					if (!IsValidationSuspended)
					{
						Validation.ValidateCustomsOffice();
						Validation.ValidateGoodsLocation();
					}
					GoodsLocationInfo.RefreshBinding();
				}
			}
		}
		ZString fGoodsLocation;

		public ZPropertyInfo GoodsLocationInfo => GetZPropertyInfo(Schema.GoodsLocation);

		public ZZRefCusCodeListCombined GoodsLocationItem
		{
			get
			{
				var goodsLocation = ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(Factory, GoodsLocation, Core.Constants.CountryCodes.Taiwan, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, ZDateTime.Today, null, new ZString[] { RefCusCodeListAttributeTypes.Codes.CustomsOffice });
				return (goodsLocation?.HasAttribute(RefCusCodeListAttributeTypes.Codes.CustomsOffice, CustomsOffice) ?? false) ? goodsLocation : null;
			}
		}
		#endregion

		#region MessageType
		[Mandatory]
		[MaxLength(Schema.MessageTypeMaxLength)]
		[List(nameof(Lookups) + "." + nameof(CusGoodsLocationLookups.MessageTypeList))]
		[ResourceStringData("Enterprise.Customs.TW.Business.CusGoodsLocation|MessageType", Caption = "Message Type")]
		public ZString MessageType
		{
			get => fMessageType;
			set
			{
				if (MessageType != value)
				{
					SetNonPersistentPropertyValue(MessageTypeInfo, ref fMessageType, value);
					if (!IsValidationSuspended)
					{
						Validation.ValidateMessageType();
					}
					MessageTypeInfo.RefreshBinding();
				}
			}
		}
		ZString fMessageType;

		public ZPropertyInfo MessageTypeInfo => GetZPropertyInfo(Schema.MessageType);
		#endregion
		#endregion

		#region Collection
		CusGoodsLocationCollection fCollection;

		public CusGoodsLocationCollection Collection => fCollection ?? (fCollection = (CusGoodsLocationCollection)base.GetParentCollection(this, typeof(CusGoodsLocationCollection)));

		#endregion

		#region Validation
		CusGoodsLocationValidation fValidation;

		public CusGoodsLocationValidation Validation => fValidation ?? (fValidation = new CusGoodsLocationValidation(this));

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			Validation.ValidateAll();
		}

		#endregion

		#region Lookup
		CusGoodsLocationLookups fLookups;

		public CusGoodsLocationLookups Lookups => fLookups ?? (fLookups = new CusGoodsLocationLookups(this));
		#endregion
	}
}
