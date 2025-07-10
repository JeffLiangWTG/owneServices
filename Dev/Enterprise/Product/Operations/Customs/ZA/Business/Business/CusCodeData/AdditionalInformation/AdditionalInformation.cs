using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.ZA.Business.MessageBuilders;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.ZA.Business
{
	public class AdditionalInformation : Customs.Business.CusCodeData, IAdditionalInformation
	{
		public AdditionalInformation(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Schema

		public new class Schema : Customs.Business.CusCodeData.Schema
		{
			public const string Grouping = "Grouping";
			public const string CY_FormattedData = "CY_FormattedData";
		}

		#endregion

		[MaxLength(3)]
		[CargoWiseOne.ResourceStrings.ResourceStringData("Enterprise.Customs.ZA.Business.AdditionalInformation|CY_Code", Caption = "Code")]
		public override ZString CY_Code
		{
			get { return base.CY_Code; }
			set
			{
				var oldValue = base.CY_Code;
				base.CY_Code = value.ToUpper();
				if (!value.EqualsIgnoringCase(oldValue))
				{
					FormatCY_Data();
					refCusCode = null;
					if (HasMultiplePairs)
					{
						AdditionalInformationCodes.RefreshBinding();
					}
				}
			}
		}

		void FormatCY_Data()
		{
			if (HasAmounts)
			{
				CY_Data = CY_FormattedData;
			}
		}

		public ZString CY_FormattedData_FieldType => GetInputControlType().ToString();

		public ZInt DecimalPlaces => ZInt.ParseSafe(Amounts.FirstOrDefault(), ZInt.Zero);

		ZString[] Amounts => ZARefCusCodeListTypes.GetAdditionalInformationAttributeValuesFor(Factory, ZDateTime.Today, CY_Code, RefCusCodeListAttributeTypes.Codes.Amount);

		ZBool HasAmounts
		{
			get
			{
				var amount = Amounts?.FirstOrDefault();
				return amount.HasValue && !amount.Value.IsEmpty;
			}
		}

		ZArchitecture.FieldType GetInputControlType()
		{
			if (!HasAmounts)
			{
				return ZArchitecture.FieldType.Text;
			}
			return DecimalPlaces == ZInt.Zero ? ZArchitecture.FieldType.Integer : ZArchitecture.FieldType.Decimal;
		}

		ZZRefCusCodeListCombined refCusCode;
		public ZZRefCusCodeListCombined RefCusCode =>
			refCusCode ?? (refCusCode = ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(Factory, CY_Code, Core.Constants.CountryCodes.SouthAfrica, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AdditionalInformation, Parent.DateOfAssessment));

		[MaxLength(32)]
		public ZString CY_FormattedData
		{
			get
			{
				return Factory.GetValue(ref cY_FormattedDataCached, () =>
				{
					var result = CY_Data;
					if (HasAmounts)
					{
						result = Utilities.FormatNumberNational(GetDataAsDecimal(result), DecimalPlaces);
					}

					return result;
				});
			}
			set => CY_Data = value;
		}

		CachedProperty<ZString> cY_FormattedDataCached;

		public ZPropertyInfo CY_FormattedDataInfo => GetWrappedZPropertyInfo(Schema.CY_FormattedData, (x) => CY_DataInfo);

		[MaxLength(32)]
		public override ZString CY_Data
		{
			get { return base.CY_Data; }
			set
			{
				base.CY_Data = HasAmounts ? new ZString(GetDataAsDecimal(value).ToString(DecimalPlaces)) : value;
			}
		}

		public override ZShort CY_Order
		{
			get { return base.CY_Order; }
			set
			{
				var oldValue = CY_Order;
				base.CY_Order = value;
				if (oldValue != CY_Order && HasPair)
				{
					Validation.ValidateCY_Code();
				}
			}
		}

		#region Lookups / Validation

		public new AdditionalInformationLookups Lookups
		{
			get { return (AdditionalInformationLookups)base.Lookups; }
		}

		public new AdditionalInformationValidation Validation
		{
			get { return (AdditionalInformationValidation)base.Validation; }
		}

		protected override Customs.Business.CusCodeDataLookups GetNewLookups()
		{
			return new AdditionalInformationLookups(this);
		}

		protected override Customs.Business.CusCodeDataValidation GetNewValidation()
		{
			return new AdditionalInformationValidation(this);
		}

		#endregion

		public new CusEntryLine Parent
		{
			get { return (CusEntryLine)base.Parent; }
		}

		public AdditionalInformationCollection AdditionalInformationCodes => Parent?.AdditionalInformationCodes;

		#region New Properties

		public override ZString Description
		{
			get
			{
				return base.Description + (RelatedAddInfo != null ? ZString.Format(" - {0}", RelatedAddInfo.CY_Data) : ZString.Empty);
			}
		}

		AdditionalInformation RelatedAddInfo => AdditionalInformationCodes?.OfType<AdditionalInformation>().FirstOrDefault(addInfo => addInfo.PK != PK && PairCode == addInfo.CY_Code && addInfo.CY_Order == CY_Order);

		#region Grouping

		[BusinessObjectTestExclude]
		public ZString Grouping
		{
			get
			{
				return HasMultiplePairs ? new ZString(CY_Order.ToString()) : ZString.Empty;
			}
			set
			{
				if (HasMultiplePairs)
				{
					CY_Order = ZShort.ParseSafe(value, ZShort.Zero);
				}
				GroupingInfo.RefreshBinding();
			}
		}

		public bool Grouping_ReadOnly
		{
			get { return !HasMultiplePairs; }
		}

		public virtual ZPropertyInfo GroupingInfo
		{
			get { return GetZPropertyInfo(Schema.Grouping); }
		}

		ZString PairCode
		{
			get
			{
				var result = ZString.Empty;
				if (pairCode.HasValue)
				{
					result = pairCode.Value;
				}
				else if (RefCusCode != null)
				{
					pairCode = RefCusCode.GetAttribute(RefCusCodeListAttributeTypes.Codes.Pair);
					result = pairCode.Value;
				}
				return result;
			}
		}
		ZString? pairCode;

		public ZBool HasPair
		{
			get
			{
				var result = ZBool.False;
				if (hasPair.HasValue)
				{
					result = hasPair.Value;
				}
				else if (RefCusCode != null)
				{
					hasPair = RefCusCode?.HasAttribute(RefCusCodeListAttributeTypes.Codes.Pair);
					result = hasPair.Value;
				}
				return result;
			}
		}
		ZBool? hasPair;

		public ZBool HasMultiplePairs
		{
			get
			{
				var result = ZBool.False;
				var additionalInformationCodes = AdditionalInformationCodes;
				if (additionalInformationCodes != null)
				{
					var addInfoCodes = additionalInformationCodes.OfType<AdditionalInformation>();
					result = HasPair && (addInfoCodes.Count(x => x.CY_Code == CY_Code) > 1 || addInfoCodes.Count(x => x.CY_Code == PairCode) > 1);
				}
				return result;
			}
		}

		#endregion

		public ZDecimal Amount => GetDataAsDecimal(CY_Data);
		#endregion

		#region Implementation

		ZDecimal GetDataAsDecimal(ZString data)
		{
			return new ZDecimal(Utilities.ConvertToDecimal(data.ToString())).Round(DecimalPlaces);
		}

		protected override TypeLoaderCollection parentLoaders
		{
			get { return new TypeLoaderCollection(typeof(CusEntryLine)); }
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CY_Type = CusCodeDataTypeList.Codes.AdditionalInformation;
		}

		#endregion

		#region IAdditionalInformation

		ZString IAdditionalInformation.Code => CY_Code;

		ZString IAdditionalInformation.Value => CY_Data;

		ZInt? IAdditionalInformation.Group
		{
			get
			{
				var group = Grouping;
				if (!group.IsEmpty)
				{
					ZInt result = 0;
					if (ZInt.TryParse(group, out result))
					{
						return result;
					}
				}
				return null;
			}
		}

		#endregion
	}
}
