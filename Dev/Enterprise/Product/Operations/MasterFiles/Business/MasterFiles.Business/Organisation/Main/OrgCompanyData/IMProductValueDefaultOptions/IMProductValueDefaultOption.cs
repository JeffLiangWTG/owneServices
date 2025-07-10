using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using DefaultsOptions = Enterprise.Core.Constants.Customs.ASNRefreshDefaultsOptions;

namespace Enterprise.MasterFiles.Business
{
	public class IMProductValueDefaultOption : NonPersistentBusinessObject
	{
		public IMProductValueDefaultOption(BusinessObjectFactory factory) : base(factory)
		{
		}
		public IMProductValueDefaultOption(ZString fieldType, BusinessObjectFactory factory) : base(factory)
		{
			fFieldType = fieldType;
		}

		#region Properties

		[List("FieldTypes")]
		[MaxLength(5)]
		[CargoWiseOne.ResourceStrings.ResourceStringData("NPBO:Enterprise.MasterFiles.Business.IMProductValueDefaultOption|FieldType", Caption = "Field Type")]
		public ZString FieldType
		{
			get { return fFieldType; }
			set
			{
				if (value != fFieldType)
				{
					SetNonPersistentPropertyValue(FieldTypeInfo, ref fFieldType, value);

					if (!IsValidationSuspended)
					{
						Validation.ValidateFieldType();
					}
				}
			}
		}
		ZString fFieldType;

		public ZPropertyInfo FieldTypeInfo => GetZPropertyInfo(nameof(FieldType));

		#endregion

		#region Lookups
		public CodeDescriptionPairList FieldTypes
		{
			get
			{
				return Factory.GetCachedValue("IMProductValueDefaultOption|FieldTypes", () =>
				{
					var list = new CodeDescriptionPairList();
					list.AddPair(DefaultsOptions.Codes.Classification, DefaultsOptions.Descriptions.Classification);
					list.AddPair(DefaultsOptions.Codes.CountryOfOrigin, DefaultsOptions.Descriptions.CountryOfOrigin);
					list.AddPair(DefaultsOptions.Codes.Preference, DefaultsOptions.Descriptions.Preference);
					list.AddPair(DefaultsOptions.Codes.Tariff, DefaultsOptions.Descriptions.Tariff);
					return list;
				});
			}
		}

		#endregion

		#region Validation

		public IMProductValueDefaultOptionValidation Validation => new IMProductValueDefaultOptionValidation(this);

		#endregion
	}
}
