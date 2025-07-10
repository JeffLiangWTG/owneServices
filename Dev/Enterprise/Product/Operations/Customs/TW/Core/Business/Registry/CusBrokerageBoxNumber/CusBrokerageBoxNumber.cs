using System.Xml;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.TW.Business
{
	[XmlSerializerAssembly("Enterprise.Customs.TW.Business.XmlSerializers")]
	public class CusBrokerageBoxNumber : RegistryBusinessObjectTemplate
	{
		#region Constructors and Schema

		public CusBrokerageBoxNumber() : base()
		{
		}

		public CusBrokerageBoxNumber(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory)
		{
		}

		public new BusinessObjectFactory Factory => CurrentFactory ?? new BusinessObjectFactory { NameForDebugging = "TWCustomsRegistryCusBrokerageBoxNumber" };

		protected abstract class Schema
		{
			public const string BoxNumber = "BoxNumber";
			public const string CustomsOfficeArea = "CustomsOfficeArea";
			public const string IsDefaultBoxNumber = "IsDefaultBoxNumber";

			public const int BoxNumberMaxLength = 3;
			public const int CustomsOfficeAreaMaxLength = 1;
		}

		#endregion

		#region Overrides

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory) => new CusBrokerageBoxNumber(fallbackLevel, factory);

		protected sealed override void ReadElements(XmlReaderWrapper reader)
		{
			BoxNumber = reader.ReadElementString(Schema.BoxNumber);
			CustomsOfficeArea = reader.ReadElementString(Schema.CustomsOfficeArea);
			IsDefaultBoxNumber = reader.ReadElementStringAsZBool(Schema.IsDefaultBoxNumber);
		}

		protected sealed override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);
			writer.WriteElementString(Schema.BoxNumber, BoxNumber);
			writer.WriteElementString(Schema.CustomsOfficeArea, CustomsOfficeArea);
			writer.WriteElementString(Schema.IsDefaultBoxNumber, ZStringTypeConverter.Instance.ConvertToString(IsDefaultBoxNumber));
		}

		#endregion

		#region Properties
		#region CustomsOfficeArea
		[Mandatory]
		[List(nameof(Lookups) + "." + nameof(CusBrokerageBoxNumberLookups.CustomsOfficeAreaList))]
		[MaxLength(Schema.CustomsOfficeAreaMaxLength)]
		[ResourceStringData("Enterprise.Customs.TW.Business.CusBrokerageBoxNumber|CustomsOfficeArea", Caption = "Customs District")]
		public ZString CustomsOfficeArea
		{
			get => fCustomsOfficeArea;
			set
			{
				if (CustomsOfficeArea != value)
				{
					SetNonPersistentPropertyValue(CustomsOfficeAreaInfo, ref fCustomsOfficeArea, value);
					if (!IsValidationSuspended)
					{
						Validation.ValidateCustomsOfficeArea();
						Validation.ValidateBoxNumber();
						Validation.ValidateIsDefaultBoxNumber();
					}
					CustomsOfficeAreaInfo.RefreshBinding();
				}
			}
		}
		ZString fCustomsOfficeArea;

		public ZPropertyInfo CustomsOfficeAreaInfo => GetZPropertyInfo(Schema.CustomsOfficeArea);
		#endregion

		#region BoxNumber
		[Mandatory]
		[MaxLength(Schema.BoxNumberMaxLength)]
		[ResourceStringData("Enterprise.Customs.TW.Business.CusBrokerageBoxNumber|BoxNumber", Caption = "Box Number")]
		public ZString BoxNumber
		{
			get => fBoxNumber;
			set
			{
				if (BoxNumber != value)
				{
					SetNonPersistentPropertyValue(BoxNumberInfo, ref fBoxNumber, value);
					if (!IsValidationSuspended)
					{
						Validation.ValidateBoxNumber();
					}

					BoxNumberInfo.RefreshBinding();
				}
			}
		}
		ZString fBoxNumber;

		public ZPropertyInfo BoxNumberInfo => GetZPropertyInfo(Schema.BoxNumber);
		#endregion

		#region IsDefaultBoxNumber
		[ResourceStringData("Enterprise.Customs.TW.Business.CusBrokerageBoxNumber|IsDefaultBoxNumber", Caption = "Is Default")]
		public ZBool IsDefaultBoxNumber
		{
			get => fIsDefaultBoxNumber;
			set
			{
				if (IsDefaultBoxNumber != value)
				{
					SetNonPersistentPropertyValue(IsDefaultBoxNumberInfo, ref fIsDefaultBoxNumber, value);
					if (!IsValidationSuspended)
					{
						Validation.ValidateIsDefaultBoxNumber();
					}

					IsDefaultBoxNumberInfo.RefreshBinding();
				}
			}
		}
		ZBool fIsDefaultBoxNumber;

		public ZPropertyInfo IsDefaultBoxNumberInfo => GetZPropertyInfo(Schema.IsDefaultBoxNumber);
		#endregion
		#endregion

		#region Collection
		CusBrokerageBoxNumberCollection fCollection;

		public CusBrokerageBoxNumberCollection Collection => fCollection ?? (fCollection = (CusBrokerageBoxNumberCollection)base.GetParentCollection(this, typeof(CusBrokerageBoxNumberCollection)));

		#endregion

		#region Validation
		CusBrokerageBoxNumberValidation fValidation;

		public CusBrokerageBoxNumberValidation Validation => fValidation ?? (fValidation = new CusBrokerageBoxNumberValidation(this));

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			Validation.ValidateAll();
		}

		#endregion

		#region Lookup
		CusBrokerageBoxNumberLookups fLookups;

		public CusBrokerageBoxNumberLookups Lookups => fLookups ?? (fLookups = new CusBrokerageBoxNumberLookups(this));
		#endregion
	}
}
