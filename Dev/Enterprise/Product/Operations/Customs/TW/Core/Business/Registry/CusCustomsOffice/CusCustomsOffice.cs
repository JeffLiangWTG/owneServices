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
	public class CusCustomsOffice : RegistryBusinessObjectTemplate
	{
		#region Constructors and Schema

		public CusCustomsOffice() : base()
		{
		}

		public CusCustomsOffice(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory)
		{
		}

		public new BusinessObjectFactory Factory => CurrentFactory ?? new BusinessObjectFactory { NameForDebugging = "TWCustomsRegistryCusCustomsOffice" };

		public abstract class Schema
		{
			public const string CustomsOfficeCode = "CustomsOfficeCode";
		}

		#endregion

		#region Overrides

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory) => new CusCustomsOffice(fallbackLevel, factory);

		protected sealed override void ReadElements(XmlReaderWrapper reader)
		{
			CustomsOfficeCode = reader.ReadElementString(Schema.CustomsOfficeCode);
		}

		protected sealed override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);
			writer.WriteElementString(Schema.CustomsOfficeCode, CustomsOfficeCode);
		}

		#endregion

		#region Properties
		#region CustomsOfficeCode
		[Mandatory]
		[List(nameof(Lookups) + "." + nameof(CusCustomsOfficeLookups.CustomsOfficeList))]
		[ResourceStringData("Enterprise.Customs.TW.Business.CusCustomsOffice|CustomsOfficeCode", Caption = "Customs Office")]
		public ZString CustomsOfficeCode
		{
			get => fCustomsOfficeCode;
			set
			{
				CheckMaximumLength(CustomsOfficeCodeInfo, value);
				if (CustomsOfficeCode != value)
				{
					SetNonPersistentPropertyValue(CustomsOfficeCodeInfo, ref fCustomsOfficeCode, value);
					if (!IsValidationSuspended)
					{
						Validation.ValidateCustomsOfficeCode();
					}
					CustomsOfficeCodeInfo.RefreshBinding();
				}
			}
		}
		ZString fCustomsOfficeCode;

		public ZPropertyInfo CustomsOfficeCodeInfo => GetZPropertyInfo(Schema.CustomsOfficeCode);
		#endregion

		#endregion

		#region Validation
		CusCustomsOfficeValidation fValidation;

		public CusCustomsOfficeValidation Validation => fValidation ?? (fValidation = new CusCustomsOfficeValidation(this));

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			Validation.ValidateAll();
		}

		#endregion

		#region Lookup
		CusCustomsOfficeLookups fLookups;

		public CusCustomsOfficeLookups Lookups => fLookups ?? (fLookups = new CusCustomsOfficeLookups(this));
		#endregion
	}
}
