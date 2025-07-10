using System;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.ZA.Manifest.Business
{
	public class CusPerson : ASYCUDA.Business.CusPerson
		, Integration.Customs.ASYCUDA.ZAManifest.ICusPerson
	{
		public CusPerson(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new class Schema : ASYCUDA.Business.CusPerson.Schema
		{
			public const string TravelDocumentTypeInZA = "TravelDocumentTypeInZA";
			public const string TravellerTypeInZA = "TravellerTypeInZA";
			public const string ReasonForMovementInZA = "ReasonForMovementInZA";
			public const string OccupationInZA = "OccupationInZA";
			public const int TravelDocumentTypeInZAMaxLength = 1;
		}

		public CodeDescriptionPairList TravellerTypeValues => GetZACodeDescriptionPairList(ZaDataTypes.Codes.TravellerType);

		[List(nameof(TravellerTypeValues))]
		[MaxLength(CusPersonCountry.Schema.CPC_ValueMaxLength)]
		[BusinessObjectTestExclude]
		[ResourceStringData("ZACusPerson.TravellerTypeInZA", Caption = "Traveler Type")]
		public ZString TravellerTypeInZA
		{
			get
			{
				return GetCountryValue(Core.Constants.CountryCodes.SouthAfrica, ZaDataTypes.Codes.TravellerType);
			}
			set
			{
				var oldValue = TravellerTypeInZA;
				SetCountryValue(Core.Constants.CountryCodes.SouthAfrica, ZaDataTypes.Codes.TravellerType, value);
				TravellerTypeInZAInfo.RefreshBinding(oldValue);
				if (!IsValidationSuspended)
				{
					Validation.ValidateTravellerTypeInZA();
				}
			}
		}

		public ZPropertyInfo TravellerTypeInZAInfo
		{
			get { return GetZPropertyInfo(Schema.TravellerTypeInZA); }
		}

		public CodeDescriptionPairList ReasonForMovementValues => GetZACodeDescriptionPairList(ZaDataTypes.Codes.ReasonForMovement);

		[MaxLength(CusPersonCountry.Schema.CPC_ValueMaxLength)]
		[List(nameof(ReasonForMovementValues))]
		[BusinessObjectTestExclude]
		[ResourceStringData("ZACusPerson.ReasonForMovementInZA", Caption = "Reason for Movement")]
		public ZString ReasonForMovementInZA
		{
			get
			{
				return GetCountryValue(Core.Constants.CountryCodes.SouthAfrica, ZaDataTypes.Codes.ReasonForMovement);
			}
			set
			{
				var oldValue = ReasonForMovementInZA;
				SetCountryValue(Core.Constants.CountryCodes.SouthAfrica, ZaDataTypes.Codes.ReasonForMovement, value);
				ReasonForMovementInZAInfo.RefreshBinding(oldValue);
				if (!IsValidationSuspended)
				{
					Validation.ValidateReasonForMovementInZA();
				}
			}
		}

		public ZPropertyInfo ReasonForMovementInZAInfo
		{
			get { return GetZPropertyInfo(Schema.ReasonForMovementInZA); }
		}

		public CodeDescriptionPairList TravelDocumentTypes => GetZACodeDescriptionPairList(ZaDataTypes.Codes.TravelDocumentType);

		[MaxLength(CusPerson.Schema.TravelDocumentTypeInZAMaxLength)]
		[List(nameof(TravelDocumentTypes))]
		[BusinessObjectTestExclude]
		[ResourceStringData("ZACusPerson.TravelDocumentTypeInZA", Caption = "Travel Document Type")]
		public ZString TravelDocumentTypeInZA
		{
			get => GetCountryValue(Core.Constants.CountryCodes.SouthAfrica, ZaDataTypes.Codes.TravelDocumentType);
			set
			{
				var oldValue = TravelDocumentTypeInZA;
				CheckMaximumLength(TravelDocumentTypeInZAInfo, value);
				SetCountryValue(Core.Constants.CountryCodes.SouthAfrica, ZaDataTypes.Codes.TravelDocumentType, value);
				TravelDocumentTypeInZAInfo.RefreshBinding(oldValue);
				if (!IsValidationSuspended)
				{
					Validation.ValidateTravelDocumentTypeInZA();
				}
			}
		}

		public ZPropertyInfo TravelDocumentTypeInZAInfo => GetZPropertyInfo(Schema.TravelDocumentTypeInZA);

		public CodeDescriptionPairList OccupationValues => GetZACodeDescriptionPairList(ZaDataTypes.Codes.Occupation);

		[MaxLength(CusPersonCountry.Schema.CPC_ValueMaxLength)]
		[List(nameof(OccupationValues))]
		[BusinessObjectTestExclude]
		[ResourceStringData("ZACusPerson.OccupationInZA", Caption = "Occupation")]
		public ZString OccupationInZA
		{
			get
			{
				return GetCountryValue(Core.Constants.CountryCodes.SouthAfrica, ZaDataTypes.Codes.Occupation);
			}
			set
			{
				var oldValue = OccupationInZA;
				SetCountryValue(Core.Constants.CountryCodes.SouthAfrica, ZaDataTypes.Codes.Occupation, value);
				OccupationInZAInfo.RefreshBinding(oldValue);
				if (!IsValidationSuspended)
				{
					Validation.ValidateOccupationInZA();
				}
			}
		}

		public ZPropertyInfo OccupationInZAInfo
		{
			get { return GetZPropertyInfo(Schema.OccupationInZA); }
		}

		ZString GetCountryValue(string country, string dataType)
		{
			return Countries.OfType<CusPersonCountry>().FirstOrDefault(c => c.CPC_RN_NKCountry == country && c.CPC_Type == dataType)?.CPC_Value ?? ZString.Empty;
		}

		void SetCountryValue(string country, string dataType, ZString value)
		{
			var cusPersonCountry = Countries.OfType<CusPersonCountry>().FirstOrDefault(c => c.CPC_RN_NKCountry == country && c.CPC_Type == dataType);
			if (cusPersonCountry == null && !value.IsEmpty)
			{
				cusPersonCountry = Countries.AddNew();
				cusPersonCountry.CPC_Type = dataType;
				cusPersonCountry.CPC_RN_NKCountry = country;
			}
			if (cusPersonCountry != null)
			{
				if (value.IsEmpty)
				{
					Countries.RemoveAndDelete(cusPersonCountry);
				}
				else
				{
					cusPersonCountry.CPC_Value = value;
				}
			}
		}

		CodeDescriptionPairList GetZACodeDescriptionPairList(string dataType)
		{
			return Factory.GetCachedValue("CusPersonCountryLookups.DataValues." + dataType, delegate
			{
				switch (dataType)
				{
					case ZaDataTypes.Codes.ReasonForMovement:
						return new ZaReasonForMovement();

					case ZaDataTypes.Codes.TravellerType:
						return new ZaTravellerTypes();

					case ZaDataTypes.Codes.Occupation:
						return new ZaOccupations();

					case ZaDataTypes.Codes.TravelDocumentType:
						return new ZaTravelDocumentTypes();

					default:
						return new CodeDescriptionPairList();
				}
			});
		}

		public new AsycudaManifestHeader Header => (AsycudaManifestHeader)base.Header;

		public new Customs.Business.ICusPersonCountryCollection<CusPersonCountry> Countries => (CusPersonCountryCollection<CusPersonCountry>)base.Countries;

		protected override Customs.Business.ICusPersonCountryCollection<Customs.Business.CusPersonCountry> CreateNewCusPersonCountryCollection() => new CusPersonCountryCollection<CusPersonCountry>(this);

		protected override Type GetPersonCountryTypeCore() => typeof(CusPersonCountry);
		public new CusPersonValidation Validation => (CusPersonValidation)base.Validation;
		protected override Customs.Business.CusPersonValidation GetNewValidation() => new CusPersonValidation(this);
		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore() => new CusPersonFetchStrategy(this);
	}
}
