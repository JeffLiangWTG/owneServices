using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.US.Business.MessageBuilders;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Input;

namespace Enterprise.Customs.US.Business
{
	public class QueryQuotaVisaOption : NonPersistentBusinessObject
		, IObsoleteValidation
		, IHaveAdditionalDataForBorderWise
	{
		public static class Schema
		{
			public const string US_QueryType = "US_QueryType";
			public const string US_QueryTypeList = "US_QueryTypeList";
			public const string US_UC_NKCountryOfOrigin = "US_UC_NKCountryOfOrigin";
			public const string US_FormattedTariffNumber = "US_FormattedTariffNumber";
			public const string US_CategoryNumber = "US_CategoryNumber";
			public const string US_VisaNumber = "US_VisaNumber";
			public const string US_SecondTariffFormattedNumber = "US_SecondTariffFormattedNumber";
		}

		public QueryQuotaVisaOption(BusinessObjectFactory factory, bool isACE)
			: base(factory)
		{
			this.isACE = isACE;
		}
		readonly bool isACE;

		public bool IsACE
		{
			get { return isACE; }
		}

		#region Message Sending

		public void SendQueryWithoutSaving()
		{
			var requester = new ReferenceFileRequester(Factory);

			if (IsBulkQuery)
			{
				requester.RequestBulkVisaQuery();
			}
			else
			{
				if (isACE)
				{
					new ACEQuotaQueryMessageBuilder().GenerateMessage(Factory, US_QueryType, US_TariffCategoryOrVisaNumber, US_SecondTariffUnformattedNumber, US_UC_NKCountryOfOrigin);
				}
				else
				{
					var u1 = new QTAU1();

					u1.CountryOfOrigin = US_UC_NKCountryOfOrigin;
					u1.TariffNumberTextileCategoryNumberOrVisaNumber = US_TariffCategoryOrVisaNumber;
					u1.SecondTariffNumber = US_SecondTariffUnformattedNumber;
					u1.VisaQueryIndicator = QueryTypeList.GetCustomsCode(US_QueryType);

					var message = requester.RequestSimple(ApplicationIdentifierCodeList.Codes.QueryQuota, u1, null, EM_MessageSubTypeList.Codes.QuotaVisaQuery);
				}
			}
		}

		bool IsBulkQuery
		{
			get { return QueryTypeList.IsBulkQuery(US_QueryType); }
		}

		#endregion

		#region BizObj Overrides

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ValidateUS_QueryType();
			ValidateUS_UC_NKCountryOfOrigin();
			ValidateUS_FormattedTariffNumber();
			ValidateUS_CategoryNumber();
			ValidateUS_SecondTariffFormattedNumber();
			ValidateUS_VisaNumber();
		}

		#endregion

		#region US_QueryType

		[MaxLength(1)]
		public ZString US_QueryType
		{
			get { return fUS_QueryType; }
			set
			{
				SetNonPersistentPropertyValue(US_QueryTypeInfo, ref fUS_QueryType, value);
				if (IsBulkQuery)
				{
					US_UC_NKCountryOfOrigin = ZString.Empty;
					US_FormattedTariffNumber = ZString.Empty;
					US_CategoryNumber = ZString.Empty;
					US_VisaNumber = ZString.Empty;
					US_SecondTariffFormattedNumber = ZString.Empty;
				}
				ValidateUS_QueryType();
			}
		}
		ZString fUS_QueryType;

		public ZPropertyInfo US_QueryTypeInfo
		{
			get { return GetZPropertyInfo(Schema.US_QueryType); }
		}

		//gets called from ValidateUS_TariffNumber, ValidateUS_CategoryNumber or ValidateUS_VisaNumber
		public void ValidateUS_QueryType()
		{
			if (!IsValidationSuspended)
			{
				US_QueryTypeInfo.ClearAllNotifications();
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(US_QueryTypeInfo, US_QueryTypeList);

				if (!US_QueryType.IsEmpty)
				{
					if (IsBulkQuery)
					{
						US_QueryTypeInfo.AddWarning("You are about to send a bulk query which might take a while to send and process.");
					}
					else if (US_QueryType == QueryTypeList.Codes.TariffNumber && US_UnformattedTariffNumber.IsEmpty)
					{
						US_QueryTypeInfo.AddMessageError(TariffNoRequired);
					}
					else if (US_QueryType == QueryTypeList.Codes.TextileCategoryNumber && US_CategoryNumber.IsEmpty)
					{
						US_QueryTypeInfo.AddMessageError(CategoryNoRequired);
					}
					else if (US_TariffCategoryOrVisaNumber.IsEmpty)
					{
						US_QueryTypeInfo.AddMessageError("You have selected a non-bulk query and you should enter a tariff number or category number or visa number.");
					}
				}

				ValidateUS_UC_NKCountryOfOrigin();
			}
		}
		internal const string TariffNoRequired = "Tariff Number is required.";
		internal const string CategoryNoRequired = "Category Number is required.";

		public QueryTypeList US_QueryTypeList
		{
			get
			{
				return Factory.GetCachedValue(Schema.US_QueryTypeList, delegate
				{
					var result = new QueryTypeList();
					if (isACE)
					{
						result.RemoveCode(QueryTypeList.Codes.AllVisaRecordsForAllCountries);
						result.RemoveCode(QueryTypeList.Codes.AllVisaRecordsForACountry);
						result.RemoveCode(QueryTypeList.Codes.SpecificVisaNumber);
						result.RemoveCode(QueryTypeList.Codes.AllVisaRecordsForCountryCategoryOrCountryTariff);
						result.RemoveCode(QueryTypeList.Codes.QuotaRecords);
					}
					else
					{
						result.RemoveCode(QueryTypeList.Codes.TariffNumber);
						result.RemoveCode(QueryTypeList.Codes.TextileCategoryNumber);
					}

					result.Sort();
					return result;
				});
			}
		}

		#endregion

		#region US_UC_NKCountryOfOrigin

		[MaxLength(2)]
		[ReadOnlyMember(nameof(IsBulkQuery))]
		public ZString US_UC_NKCountryOfOrigin
		{
			get { return fUS_UC_NKCountryOfOrigin; }
			set
			{
				SetNonPersistentPropertyValue(US_UC_NKCountryOfOriginInfo, ref fUS_UC_NKCountryOfOrigin, value);
				ValidateUS_UC_NKCountryOfOrigin();
			}
		}
		ZString fUS_UC_NKCountryOfOrigin;

		public ZPropertyInfo US_UC_NKCountryOfOriginInfo
		{
			get { return GetZPropertyInfo(Schema.US_UC_NKCountryOfOrigin); }
		}

		public void ValidateUS_UC_NKCountryOfOrigin()
		{
			if (!IsValidationSuspended)
			{
				US_UC_NKCountryOfOriginInfo.ClearAllNotifications();
				if (!IsBulkQuery)
				{
					ListValidation.MessageErrorIfInvalidCodeOrEmpty(US_UC_NKCountryOfOriginInfo, CountryOfOriginList);
				}
			}
		}

		public USCCountryCollection CountryOfOriginList
		{
			get { return new USCCountryCollection(Factory); }
		}

		#endregion

		#region TariffNumber/Textile Category/Visa Number

		ZString US_TariffCategoryOrVisaNumber
		{
			get
			{
				if (!US_UnformattedTariffNumber.IsEmpty)
				{
					return US_UnformattedTariffNumber;
				}
				else if (!US_CategoryNumber.IsEmpty)
				{
					return US_CategoryNumber;
				}
				else if (!US_VisaNumber.IsEmpty)
				{
					return US_VisaNumber;
				}

				return ZString.Empty;
			}
		}

		ZString US_UnformattedTariffNumber
		{
			get { return TariffFormatter.Format(US_FormattedTariffNumber); }
		}

		[BusinessObjectTestExclude]
		[MaxLength(13)]
		[ReadOnlyMember(nameof(IsBulkQuery))]
		public ZString US_FormattedTariffNumber
		{
			get { return fUS_FormattedTariffNumber; }
			set
			{
				SetNonPersistentPropertyValue(US_FormattedTariffNumberInfo, ref fUS_FormattedTariffNumber, TariffFormatter.DisplayFormat(value));
				ValidateUS_FormattedTariffNumber();
			}
		}
		ZString fUS_FormattedTariffNumber;

		TariffFormatter TariffFormatter
		{
			get { return fTariffFormatter ?? (fTariffFormatter = new TariffFormatter()); }
		}
		TariffFormatter fTariffFormatter;

		public ZPropertyInfo US_FormattedTariffNumberInfo
		{
			get { return GetZPropertyInfo(Schema.US_FormattedTariffNumber); }
		}

		public void ValidateUS_FormattedTariffNumber()
		{
			if (!IsValidationSuspended)
			{
				US_FormattedTariffNumberInfo.ClearAllNotifications();

				if (!US_UnformattedTariffNumber.IsEmpty)
				{
					USCTariff tariff = new USCTariff.Loader(Factory).LoadBestMatch(US_UnformattedTariffNumber, ZDateTime.Today);
					if (tariff == null)
					{
						US_FormattedTariffNumberInfo.AddMessageError(InvalidTariffNumber);
					}
					else if (!tariff.UE_QuotaIndicator)
					{
						US_FormattedTariffNumberInfo.AddWarning(TariffDoesNotHaveQuotaIndicator);
					}
				}

				ValidateUS_QueryType();
			}
		}

		internal const string InvalidTariffNumber = "You have entered an invalid tariff number.";

		internal const string TariffDoesNotHaveQuotaIndicator = "According to the reference files, this tariff number is not subject to quota.";

		public USCTariffCollection Tariffs
		{
			get { return new USCTariffCollection(Factory); }
		}

		[MaxLength(3)]
		[ReadOnlyMember(nameof(IsBulkQuery))]
		public ZString US_CategoryNumber
		{
			get { return fUS_CategoryNumber; }
			set
			{
				SetNonPersistentPropertyValue(US_CategoryNumberInfo, ref fUS_CategoryNumber, value);

				ValidateUS_CategoryNumber();
			}
		}
		ZString fUS_CategoryNumber;

		public ZPropertyInfo US_CategoryNumberInfo
		{
			get { return GetZPropertyInfo(Schema.US_CategoryNumber); }
		}

		public void ValidateUS_CategoryNumber()
		{
			if (!IsValidationSuspended)
			{
				US_CategoryNumberInfo.ClearAllNotifications();

				if (!US_FormattedTariffNumber.IsEmpty && !US_CategoryNumber.IsEmpty)
				{
					US_CategoryNumberInfo.AddWarning(CategoryNumberEnteredWontBeUsed);
				}

				ValidateUS_QueryType();
			}
		}

		internal const string CategoryNumberEnteredWontBeUsed = "You should not enter a tariff and category number. You should query on only one of them.";

		[MaxLength(9)]
		[ReadOnlyMember(nameof(IsBulkQuery))]
		public ZString US_VisaNumber
		{
			get { return fUS_VisaNumber; }
			set
			{
				SetNonPersistentPropertyValue(US_VisaNumberInfo, ref fUS_VisaNumber, value);

				ValidateUS_VisaNumber();
			}
		}
		ZString fUS_VisaNumber;

		public ZPropertyInfo US_VisaNumberInfo
		{
			get { return GetZPropertyInfo(Schema.US_VisaNumber); }
		}

		public void ValidateUS_VisaNumber()
		{
			if (!IsValidationSuspended)
			{
				US_VisaNumberInfo.ClearAllNotifications();

				if (!US_VisaNumber.IsEmpty)
				{
					if (!US_FormattedTariffNumber.IsEmpty || !US_CategoryNumber.IsEmpty)
					{
						US_VisaNumberInfo.AddWarning(VisaNumberEnteredWontBeUsed);
					}
				}

				ValidateUS_QueryType();
			}
		}

		internal const string VisaNumberEnteredWontBeUsed = "You should not enter a visa number along with a tariff or category number. You should query on only one of them.";

		ZString US_SecondTariffUnformattedNumber
		{
			get { return TariffFormatter.Format(US_SecondTariffFormattedNumber); }
		}

		[BusinessObjectTestExclude]
		[MaxLength(13)]
		[ReadOnlyMember(nameof(IsBulkQuery))]
		public ZString US_SecondTariffFormattedNumber
		{
			get { return fUS_SecondTariffFormattedNumber; }
			set
			{
				SetNonPersistentPropertyValue(US_SecondTariffFormattedNumberInfo, ref fUS_SecondTariffFormattedNumber, TariffFormatter.DisplayFormat(value));

				ValidateUS_SecondTariffFormattedNumber();
			}
		}
		ZString fUS_SecondTariffFormattedNumber;

		public ZPropertyInfo US_SecondTariffFormattedNumberInfo
		{
			get { return GetZPropertyInfo(Schema.US_SecondTariffFormattedNumber); }
		}

		public void ValidateUS_SecondTariffFormattedNumber()
		{
			if (!IsValidationSuspended)
			{
				US_SecondTariffFormattedNumberInfo.ClearAllNotifications();

				if (US_FormattedTariffNumber.IsEmpty && !US_SecondTariffFormattedNumber.IsEmpty)
				{
					US_SecondTariffFormattedNumberInfo.AddMessageError(SecondTariffNumberEnteredWithoutFirstTariffNumber);
				}

				if (!US_SecondTariffUnformattedNumber.IsEmpty)
				{
					var tariff = new USCTariff.Loader(Factory).LoadBestMatch(US_SecondTariffUnformattedNumber, ZDateTime.Today);
					if (tariff == null)
					{
						US_SecondTariffFormattedNumberInfo.AddMessageError(InvalidTariffNumber);
					}

					if (isACE && (US_SecondTariffUnformattedNumber.StartsWith("98", StringComparison.CurrentCultureIgnoreCase) || US_SecondTariffUnformattedNumber.StartsWith("99", StringComparison.CurrentCultureIgnoreCase)))
					{
						US_SecondTariffFormattedNumberInfo.AddMessageError(SecondTariffNumber98_99);
					}
				}
			}
		}
		internal const string SecondTariffNumberEnteredWithoutFirstTariffNumber = "You have entered the second tariff number without the first tariff number.";
		internal const string SecondTariffNumber98_99 = "Second tariff number cannot be from chapter 98 or 99.";

		#endregion

		#region IHaveAdditionalDataForBorderWise Members

		AdditionalDataForBorderWise IHaveAdditionalDataForBorderWise.GetAdditionalDataForBorderWise(string bindingProperty)
		{
			return new AdditionalDataForBorderWise("I", ZDateTime.Today);
		}

		Type IHaveAdditionalDataForBorderWise.ExpectedBusinessObjectTypeForList
		{
			get { return Tariffs.TypeOfElements; }
		}

		#endregion

#if DEBUG
		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, System.ComponentModel.PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);
			US_QueryType = QueryTypeList.Codes.AllVisaRecordsForACountry;
			US_UC_NKCountryOfOrigin = "KR";
		}
#endif
	}
}
