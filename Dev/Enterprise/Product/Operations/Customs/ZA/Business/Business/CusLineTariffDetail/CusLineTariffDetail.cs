using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.ZA.Business
{
	[SystemDefinedValues]
	public class CusLineTariffDetail : AutoZACusLineTariffDetail, Integration.Customs.ZA.ICusLineTariffDetail, ITariffDetail
	{
		public CusLineTariffDetail(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Schema

		public new class Schema : Customs.Business.CusLineTariffDetail.Schema
		{
			public const string BZ_TypeDesc = "BZ_TypeDesc";
			public const string FormulaSpecificQuestion = "FormulaSpecificQuestion";
			public const string FormulaSpecificValue = "FormulaSpecificValue";
			public const string FormulaSpecificStoredValue = "FormulaSpecificStoredValue";
			public const string BZ_TariffAndCheckDigit = "BZ_TariffAndCheckDigit";
		}

		#endregion

		#region New Properties

		#region FormulaSpecific Data

		[ResourceStringData("Enterprise.Customs.ZA.Business.CusLineTariffDetail|FormulaSpecificQuestion", Caption = "Description of Value")]
		public ZString FormulaSpecificQuestion => Factory.GetValue(ref formulaSpecificQuestionCached, () =>
					{
						return GetQuestion();
					});

		CachedProperty<ZString> formulaSpecificQuestionCached;

		public ZPropertyInfo FormulaSpecificQuestionInfo
		{
			get { return GetZPropertyInfo(Schema.FormulaSpecificQuestion); }
		}

		int FormulaSpecificValuePrecision
		{
			get
			{
				LoadFormulaSpecificStoredValueIfNeeded();
				return formulaSpecificValuePrecision;
			}
		}
		int formulaSpecificValuePrecision;

		public int FormulaSpecificValueScale
		{
			get
			{
				LoadFormulaSpecificStoredValueIfNeeded();
				return formulaSpecificValueScale;
			}
		}
		int formulaSpecificValueScale;

		[ResourceStringData("Enterprise.Customs.ZA.Business.CusLineTariffDetail|FormulaSpecificValue", Caption = "Value")]
		[BusinessObjectTestExclude]
		public ZString FormulaSpecificValue
		{
			get
			{
				LoadFormulaSpecificStoredValueIfNeeded();
				return formulaSpecificValue;
			}
			set
			{
				var oldValue = FormulaSpecificValue;
				if (oldValue != value)
				{
					SetFormulaSpecificStoredValue(oldValue, value, FormulaSpecificValuePrecision, FormulaSpecificValueScale);
				}
				if (!IsValidationSuspended)
				{
					Validation.ValidateFormulaSpecificValue();
				}
			}
		}

		ZString formulaSpecificValue;

		public ZPropertyInfo FormulaSpecificValueInfo
		{
			get { return GetZPropertyInfo(Schema.FormulaSpecificValue); }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "This property is implicitly referenced by ZAttribute")]
		bool FormulaSpecificValue_ReadOnly
		{
			get
			{
				var result = formulaSpecificValuePrecision < 1;

				var tariff = Factory.GetCusTariff(BZ_Type, BZ_Tariff, EffectiveAssessmentDate);
				if (tariff != null && tariff.FilteredRates.Any(x => x.ZZ2_RateFormula.ToUpper() == "{\"REBATE AMOUNT\"}" && x.RateCode == BZ_Type))
				{
					result = false;
				}

				return result;
			}
		}

		void SetFormulaSpecificStoredValue(ZString oldValue, ZString value, int precision, int scale, bool clearValue = false)
		{
			ZString newValue;
			if (clearValue)
			{
				formulaSpecificValue = ZString.Empty;
				formulaSpecificValuePrecision = 0;
				formulaSpecificValueScale = 0;
				newValue = ZString.Empty;
			}
			else
			{
				formulaSpecificValue = value.IsEmpty ? string.Empty : Utilities.FormatNumber(Utilities.Round(Utilities.ConvertToDecimal(value.ToString()), scale), scale, Culture.CurrentCompanyCountryCulture);
				formulaSpecificValuePrecision = precision;
				formulaSpecificValueScale = scale;
				newValue = ZString.Format(@"{{{0},{1}}}{2}", precision, scale, formulaSpecificValue);
			}
			this.SetSystemDefinedValue(Schema.FormulaSpecificStoredValue, newValue);
			if (oldValue != formulaSpecificValue)
			{
				FormulaSpecificValueInfo.RefreshBinding(oldValue);
			}
		}

		void LoadFormulaSpecificStoredValueIfNeeded()
		{
			if (!hasLoadFormulaSpecificStoredValue)
			{
				hasLoadFormulaSpecificStoredValue = true;
				var value = this.GetSystemDefinedValue<ZString>(Schema.FormulaSpecificStoredValue);
				if (!value.IsEmpty)
				{
					var match = new Regex(FormulaSpecificStoredValueFormat).Match(value);
					if (match.Success)
					{
						formulaSpecificValuePrecision = ZInt.ParseSafe(match.Groups[1].Value, ZInt.Zero);
						formulaSpecificValueScale = ZInt.ParseSafe(match.Groups[2].Value, ZInt.Zero);
						formulaSpecificValue = Utilities.FormatNumber(Utilities.Round(Utilities.ConvertToDecimal(match.Groups[3].Value), formulaSpecificValueScale), formulaSpecificValueScale, Culture.CurrentCompanyCountryCulture);
					}
				}
			}
		}
		bool hasLoadFormulaSpecificStoredValue;

		const string FormulaSpecificStoredValueFormat = @"^{(\d+),(\d+)}(.+)$";

		[List(nameof(Lookups) + "." + nameof(CusLineTariffDetailLookups.TariffCollection))]
		[ResourceStringData("Enterprise.Customs.ZA.Business.CusLineTariffDetail|BZ_TariffAndCheckDigit", Caption = "Item/Code")]
		[MaxLength(12)]
		[BusinessObjectTestExclude()]
		public ZString BZ_TariffAndCheckDigit
		{
			get
			{
				var rateType = UniversalTariff?.Rates.FirstOrDefault()?.ZZ2_ZZR_RateTypeCode ?? ZString.Empty;
				var tariffCode = (rateType == Enterprise.Customs.Universal.Constants.RateTypes.Duty ? BZ_Tariff.PadRight(8, '0') : BZ_Tariff).PadRight(10, ' ');

				return ZString.Format("{0}{1}", tariffCode, BZ_CheckDigit).Trim();
			}
			set
			{
				BZ_Tariff = value.SubstringSafe(0, 10).Trim();
				BZ_CheckDigit = value.SubstringSafe(10, 2).Trim();
				BZ_TariffAndCheckDigitInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo BZ_TariffAndCheckDigitInfo
		{
			get { return GetZPropertyInfo(Schema.BZ_TariffAndCheckDigit); }
		}

		#endregion

		#endregion

		public new JobComInvoiceLine InvoiceLine => (JobComInvoiceLine)base.InvoiceLine;

		[ResourceStringData("Enterprise.Customs.ZA.Business.CusLineTariffDetail|BZ_Type", Caption = "Part")]
		public override ZString BZ_Type
		{
			get { return base.BZ_Type; }
			set
			{
				var oldValue = BZ_Type;
				base.BZ_Type = value;
				if (!IsCopying && oldValue != BZ_Type && !IsDefaultingFromTariffTypeSuspended)
				{
					DefaultTariff();
					DefaultRelatedCusLineTariffDetails(BZ_Type, BZ_Tariff, NewUsed);
					RefreshFormulaSpecific();
					DefaultCusLineTariffDetailUQ();
				}
			}
		}

		protected override Customs.Business.AdditionalDutiesTariffTypeList GetAdditionalDutiesTariffTypeList()
			=> CusLineTariffDetailLookups.GetAdditionalDutiesTariffTypeList(Factory);

		public override TariffView UniversalTariff
		{
			get
			{
				var parentTariff = InvoiceLine?.JI_Tariff ?? ZString.Empty;

				var result = Factory.GetCusTariffIncludingCheckDigit(BZ_Type, BZ_Tariff, EffectiveAssessmentDate, BZ_CheckDigit, parentTariff)
								?? Factory.GetCusTariff(BZ_Type, BZ_Tariff, EffectiveAssessmentDate);

				if (result == null)
				{
					return null;
				}

				if (!result.RelatedTariffs.Any(x => x.ZZH_ZZI_TariffTypeCode == "1P1" && (x.ZZH_TariffCode.IsEmpty || x.ZZH_TariffCode == "0")) && !(UniversalTariffType?.IsRefund() ?? false))
				{
					result = Factory.GetCusTariffIncludingCheckDigit(BZ_Type, BZ_Tariff, EffectiveAssessmentDate, BZ_CheckDigit, parentTariff);
				}
				return result;
			}
		}

		[List(nameof(Lookups) + "." + nameof(CusLineTariffDetailLookups.TariffCollection))]
		[ResourceStringData("Enterprise.Customs.ZA.Business.CusLineTariffDetail|BZ_Tariff", Caption = "Item/Code")]
		public override ZString BZ_Tariff
		{
			get { return base.BZ_Tariff; }
			set
			{
				var oldValue = BZ_Tariff;
				base.BZ_Tariff = value;
				if (!IsCopying && oldValue != BZ_Tariff)
				{
					DefaultTariffTypeIfNeeded();
					DefaultRelatedCusLineTariffDetails(BZ_Type, BZ_Tariff, NewUsed);
					RefreshFormulaSpecific();
					DefaultCusLineTariffDetailUQ();
				}
			}
		}

		protected ZString NewUsed => InvoiceLine?.JI_NewUsed ?? ZString.Empty;

		void DefaultCusLineTariffDetailUQ()
		{
			var newUQ = ZString.Empty;
			if (IsValidTariffTypeForDefaultingUOM)
			{
				newUQ = UniversalTariff?.GetSpecificUOM(UniversalReferenceConstants.RefCusTariffUOMTypes.StatisticalUOMType) ?? ZString.Empty;
			}
			BZ_UQ1 = newUQ;
		}

		void DefaultTariffTypeIfNeeded()
		{
			if (UniversalTariff == null)
			{
				var candidate = new TariffView.Loader(Factory).LoadMostRecentCachedTariff(Core.Constants.CountryCodes.SouthAfrica, BZ_Tariff, EffectiveAssessmentDate);
				var candidateTariffType = candidate?.ZZ1_ZZI_TariffTypeCode ?? ZString.Empty;
				var candidateTariffSchedule = candidateTariffType.Left(1);
				var currentTariffType = BZ_Type;
				if (!candidateTariffType.IsEmpty && candidateTariffType != currentTariffType && candidateTariffSchedule != UniversalReferenceConstants.Schedule._1 && (currentTariffType.IsEmpty || candidateTariffSchedule == currentTariffType.Left(1)))
				{
					using (SuspentTariffTypeDefaulting())
					{
						BZ_Type = candidateTariffType;
					}
				}
			}
		}

		public ZString BZ_TypeDesc
		{
			get
			{
				var result = ZString.Empty;
				var type = BZ_Type;
				if (!type.IsEmpty)
				{
					result = Lookups.TariffTypeList.GetDescriptionFromCode(type);
					if (string.IsNullOrEmpty(result))
					{
						result = string.Format(CultureInfo.InvariantCulture, "Schedule '{0}'", type);
					}
				}
				return result;
			}
		}

		public ZPropertyInfo BZ_TypeDescInfo => GetZPropertyInfo(Schema.BZ_TypeDesc);

		public override ZString BZ_UQ1
		{
			get { return base.BZ_UQ1; }
			set
			{
				var oldValue = BZ_UQ1;
				base.BZ_UQ1 = value;
				if (!IsCopying && oldValue != BZ_UQ1)
				{
					InvoiceLine?.DefaultUOMFromTariffAndCusLineTariffDetailsIfNeeded();
				}
			}
		}

		internal ZBool IsValidTariffTypeForDefaultingUOM => UniversalTariff?.IsPayableDuty() ?? false;

		#region Lookups

		public new CusLineTariffDetailLookups Lookups
		{
			get { return (CusLineTariffDetailLookups)base.Lookups; }
		}

		protected override Customs.Business.CusLineTariffDetailLookups GetNewLookups()
		{
			return new CusLineTariffDetailLookups(this);
		}

		#endregion

		#region Validation

		public new CusLineTariffDetailValidation Validation
		{
			get { return (CusLineTariffDetailValidation)base.Validation; }
		}

		protected override Customs.Business.CusLineTariffDetailValidation GetNewValidation()
		{
			return new CusLineTariffDetailValidation(this);
		}

		#endregion

		#region Suspend Tariff Defaulting

		internal bool IsTariffDefaultingSuspended
		{
			get { return tarifflDefaultingSuspenderIndex > 0; }
		}

		internal IDisposable SuspendCusLineTariffDetailDefaulting()
		{
			return new TariffDefaultingSuspender(this);
		}

		int tarifflDefaultingSuspenderIndex;

		class TariffDefaultingSuspender : IDisposable
		{
			public TariffDefaultingSuspender(CusLineTariffDetail tariffDetail)
			{
				this.tariffDetail = tariffDetail;
				this.tariffDetail.tarifflDefaultingSuspenderIndex++;
			}

			readonly CusLineTariffDetail tariffDetail;

			#region IDisposable Members

			public void Dispose()
			{
				tariffDetail.tarifflDefaultingSuspenderIndex--;
			}

			#endregion
		}
		#endregion

		#region Implementation

		void RefreshFormulaSpecific()
		{
			var question = FormulaSpecificQuestion;
			if (question.IsEmpty)
			{
				SetFormulaSpecificStoredValue(FormulaSpecificValue, ZString.Empty, 0, 0, true);
			}
			else if (!FormulaSpecificValue.IsEmpty)
			{
				FormulaSpecificValue = ZString.Empty;
			}
			FormulaSpecificQuestionInfo.RefreshBinding();
		}

		void DefaultTariff()
		{
			if (!IsTariffDefaultingSuspended)
			{
				TariffView tariff = null;
				var result = ZString.Empty;
				var tariffType = UniversalTariffType;
				if (tariffType != null)
				{
					var parentInvoiceLine = InvoiceLine;
					if (parentInvoiceLine != null)
					{
						var applicableTariffs = GetApplicableTariffs(tariffType, parentInvoiceLine);
						if (applicableTariffs.Count == 1)
						{
							tariff = applicableTariffs[0];
							var checkDigit = tariff.GetAttribute(UniversalReferenceConstants.TariffAttributes.CheckDigit);
							result = string.Concat(tariff.ZZ1_TariffCode.PadRight(10, ' '), checkDigit?.ZZ3_Value);
						}
					}
				}
				BZ_TariffAndCheckDigit = result;
			}
		}

		internal IList<TariffView> GetApplicableTariffs(RefCusTariffType typeToMatch, JobComInvoiceLine parentInvoiceLine)
		{
			var result = new List<TariffView>();
			var cusProcedure = parentInvoiceLine.CusProcedure;
			var assessmentDate = parentInvoiceLine.EffectiveAssessmentDate;
			GatherTariff(result, typeToMatch, cusProcedure, UniversalReferenceConstants.CusTariffCode.Schedule1Part1, parentInvoiceLine.JI_Tariff, assessmentDate);
			foreach (CusLineTariffDetail child in parentInvoiceLine.CusLineTariffDetails.Where(x => x != this))
			{
				GatherTariff(result, typeToMatch, cusProcedure, child.BZ_Type, child.BZ_Tariff, assessmentDate);
			}
			return result;
		}

		void GatherTariff(List<TariffView> tariffs, RefCusTariffType typeToMatch, RefCusProcedure cusProcedure, ZString tariffType, ZString tariff, ZDateTime assessmentDate)
		{
			var dictionary = cusProcedure.GetValidRefCusTariffSortedDictionary(tariffType, tariff, assessmentDate);
			List<TariffView> list;
			if (dictionary.TryGetValue(typeToMatch, out list))
			{
				foreach (var relatedTariff in list)
				{
					if (!tariffs.Contains(relatedTariff))
					{
						tariffs.Add(relatedTariff);
					}
				}
			}
		}

		void DefaultRelatedCusLineTariffDetails(ZString type, ZString tariff, ZString newUsed)
		{
			if (!type.IsEmpty && !tariff.IsEmpty)
			{
				var invoiceLineToCheck = InvoiceLine;
				if (invoiceLineToCheck != null && !invoiceLineToCheck.IsCusLineTariffDetailDefaultingSuspended)
				{
					using (invoiceLineToCheck.SuspendUOMDefaulting())
					using (invoiceLineToCheck.SuspendCusLineTariffDetailDefaulting())
					{
						var assessmentDate = invoiceLineToCheck.EffectiveAssessmentDate;
						if (Factory.GetCusTariff(type, tariff, assessmentDate) != null)
						{
							invoiceLineToCheck.DefaultCusLineTariffDetails(invoiceLineToCheck.CusProcedure, type, tariff, assessmentDate, type, newUsed);
						}
					}
					invoiceLineToCheck.DefaultUOMFromTariffAndCusLineTariffDetailsIfNeeded();
				}
			}
		}

		ZString GetQuestion()
		{
			var result = ZString.Empty;
			var invoiceLineToCheck = InvoiceLine;
			if (invoiceLineToCheck != null)
			{
				var universalTariff = UniversalTariff;
				if (universalTariff != null)
				{
					var rate = universalTariff.GetApplicableRate(invoiceLineToCheck.RebateRateSelectionCriteria);

					if (rate != null)
					{
						var questionForFormulaSpecificValue = rate.GetQuestionForFormulaSpecificValues(invoiceLineToCheck).FirstOrDefault();
						if (questionForFormulaSpecificValue != null)
						{
							result = questionForFormulaSpecificValue.Question;
							var precision = questionForFormulaSpecificValue.Precision;
							var scale = questionForFormulaSpecificValue.Scale;
							if (questionForFormulaSpecificValue.IsDefaultPrecisionScale)
							{
								precision = DefaultPrecision;
								scale = DefaultScale;
							}
							if (precision != FormulaSpecificValuePrecision || scale != FormulaSpecificValueScale)
							{
								var value = FormulaSpecificValue;
								SetFormulaSpecificStoredValue(value, value, precision, scale);
							}
						}
					}
				}
			}
			return result;
		}

		const int DefaultPrecision = 12;
		const int DefaultScale = 2;

		#endregion

		ZString ITariffDetail.Tariff => BZ_Tariff;
		ZString ITariffDetail.Type => BZ_Type;
		ZBool ITariffDetail.ShouldBeExcluded => (BZ_Type == UniversalReferenceConstants.CusTariffCode.Schedule1Part3D && NewUsed != ZString.Empty && NewUsed != GoodsTypeList.Codes.N)
														|| (InvoiceLineHasPermitOfTypeVALAOrPRC()
															&& (UniversalTariff?.Attributes.Any(x => x.ZZ3_Name == UniversalReferenceConstants.TariffAttributes.PRCC) ?? false));

		ZBool InvoiceLineHasPermitOfTypeVALAOrPRC() => (InvoiceLine?.CusEntryLine?.EntryInstruction?.RCCCertificates.Cast<RCCCertificate>().Any(x => x.PermitType == PermitTypeList.Codes.VALA) ?? false)
															|| (InvoiceLine?.CusEntryLine?.EntryInstruction?.DutyRebateCertificates.Cast<DutyRebateCertificate>().Any(x => x.PermitType == PermitTypeList.Codes.PRC) ?? false);

		#region Suspend TariffType Defaulting

		internal bool IsDefaultingFromTariffTypeSuspended
		{
			get { return defaultingFromTariffTypeSuspendedIndex > 0; }
		}

		internal IDisposable SuspentTariffTypeDefaulting()
		{
			return new DefaultingFromTariffTypeSuspender(this);
		}

		int defaultingFromTariffTypeSuspendedIndex;

		class DefaultingFromTariffTypeSuspender : IDisposable
		{
			public DefaultingFromTariffTypeSuspender(CusLineTariffDetail lineTariffDetail)
			{
				this.lineTariffDetail = lineTariffDetail;
				this.lineTariffDetail.defaultingFromTariffTypeSuspendedIndex++;
			}

			readonly CusLineTariffDetail lineTariffDetail;

			#region IDisposable Members

			public void Dispose()
			{
				lineTariffDetail.defaultingFromTariffTypeSuspendedIndex--;
			}

			#endregion
		}
		#endregion
	}
}
