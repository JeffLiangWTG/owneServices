using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.ZA.Business
{
	public class CusLineTariffDetailValidation : AutoZACusLineTariffDetailValidation
	{
		public CusLineTariffDetailValidation(CusLineTariffDetail parent)
			: base(parent)
		{
		}

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateFormulaSpecificValue();
		}

		public void ValidateFormulaSpecificValue()
		{
			ValidateCalculatedProperty(Parent.FormulaSpecificValueInfo);
		}

		protected void CheckFormulaSpecificValue()
		{
			if (Parent.FormulaSpecificValue.IsEmpty && !Parent.FormulaSpecificQuestion.IsEmpty)
			{
				Parent.FormulaSpecificValueInfo.AddWarning(QuestionForFormulaSpecificValue.ADefaultFormulaSpecificValueIsRequired(Parent.FormulaSpecificValueScale));
			}
		}

		protected new CusLineTariffDetail Parent
		{
			get { return (CusLineTariffDetail)base.Parent; }
		}

		protected override void CheckBZ_Type()
		{
			base.CheckBZ_Type();
			var targetInfo = Parent.BZ_TypeInfo;
			if (Parent.BZ_Type.IsEmpty)
			{
				targetInfo.AddMessageError(MandatoryValidation.YouHaveNotEnteredMessage("Part"));
			}
			else
			{
				var cusTariffType = Parent.UniversalTariffType;
				if (cusTariffType == null)
				{
					targetInfo.AddMessageError(ListValidation.InvalidCodeMessageError.ToString());
				}
				else
				{
					ValidateBZ_TypeIsNotDuplicatedAndIsValidForProcedure(targetInfo, cusTariffType);
				}

				ValidateTypeIsNot13DAndNotNew(targetInfo);
			}
			ValidateFormulaSpecificValue();
		}

		void ValidateTypeIsNot13DAndNotNew(ZPropertyInfo targetInfo)
		{
			if (Parent.BZ_Type == UniversalReferenceConstants.CusTariffCode.Schedule1Part3D && (Parent.InvoiceLine?.JI_NewUsed ?? ZString.Empty) != GoodsTypeList.Codes.N)
			{
				targetInfo.AddMessageError(Schedule1Part3DForNewError);
			}
		}

		void ValidateBZ_TypeIsNotDuplicatedAndIsValidForProcedure(ZPropertyInfo targetInfo, RefCusTariffType cusTariffType)
		{
			var invoiceLine = Parent.InvoiceLine;
			if (invoiceLine != null)
			{
				ValidateBZ_TypeIsNotDuplicated(invoiceLine, targetInfo, cusTariffType);
				if (!targetInfo.HasErrors() && !(Parent.BZ_Type.StartsWith(UniversalReferenceConstants.Schedule._6) && invoiceLine.IsExport))
				{
					ValidateTypeIsValidForProcedure(invoiceLine, targetInfo);
				}
			}
		}

		void ValidateTypeIsValidForProcedure(JobComInvoiceLine invoiceLine, ZPropertyInfo targetInfo)
		{
			var tariff = Parent.UniversalTariff;
			if (tariff != null)
			{
				var procedure = invoiceLine.CusProcedure;
				if (procedure != null && !tariff.IsApplicableForDutyCalculation(procedure))
				{
					targetInfo.AddMessageError(ValidationConstants.CusLineTariffDetail.ScheduleIsNotValidForProcedure(Parent.BZ_TypeDesc, procedure.ZZ6_ProcedureCode, procedure.ZZ6_PreviousProcedureCode));
				}
			}
		}

		void ValidateBZ_TypeIsNotDuplicated(JobComInvoiceLine invoiceLine, ZPropertyInfo targetInfo, RefCusTariffType cusTariffType)
		{
			var tariffTypePrefix = cusTariffType.IsPayableDutyExcludingAntiDumping() ? ZString.Empty : cusTariffType.ZZI_TariffType.Left(1);
			foreach (var tariffDetail in invoiceLine.CusLineTariffDetails.Cast<CusLineTariffDetail>().Where(x => x != Parent))
			{
				var otherCusTariffType = tariffDetail.UniversalTariffType;
				if (otherCusTariffType == cusTariffType)
				{
					targetInfo.AddError(ValidationConstants.CusLineTariffDetail.DuplicateAdditionalDutySchedule(Parent.BZ_TypeDesc));
					break;
				}
				else if (otherCusTariffType != null && !tariffTypePrefix.IsEmpty && tariffTypePrefix == otherCusTariffType.ZZI_TariffType.Left(1))
				{
					targetInfo.AddMessageError(ValidationConstants.CusLineTariffDetail.SimilarAdditionalDutyType(tariffTypePrefix));
					break;
				}
			}
		}

		protected override void CheckBZ_Tariff()
		{
			base.CheckBZ_Tariff();
			var targetInfo = Parent.BZ_TariffInfo;
			var tariff = Parent.BZ_Tariff;
			if (tariff.IsEmpty)
			{
				targetInfo.AddMessageError(MandatoryValidation.YouHaveNotEnteredMessage("Item/Code"));
			}
			else
			{
				var cusTariffType = Parent.UniversalTariffType;
				if (cusTariffType == null)
				{
					ListValidation.MessageErrorIfInvalidCode(targetInfo);
				}
				else
				{
					var invoiceLine = Parent.InvoiceLine;
					if (invoiceLine != null)
					{
						var effectiveAssessmentDate = invoiceLine.EffectiveAssessmentDate;
						var cusTariff = Parent?.UniversalTariff;
						if (cusTariff != null)
						{
							var criteria = invoiceLine.GetSpecificRateSelectionCriteria(ZString.Empty);
							var criteriaUsingRateCode = invoiceLine.GetSpecificRateSelectionCriteria(Parent.BZ_Type);
							var rateTypeDescription = RefCusRateType.Loader.GetRateTypeByRateCode(Parent.Factory, criteriaUsingRateCode.DataGrouping, criteriaUsingRateCode.RateCode)?.ZZR_Description ?? ZString.Empty;
							var applicableRates = cusTariff.GetApplicableRates(criteria);

							if (!applicableRates.Any())
							{
								if (!cusTariffType.IsRefund())
								{
									targetInfo.AddMessageError(Res.GetString("30C9B777-11D8-4561-AC68-434193EB6F21", "There is no applicable {0} rate for the Tariff '{1}' and Country/Region Of Origin '{2}' as at {3}.", rateTypeDescription, tariff, criteria.TradeGroupCountry, criteria.EffectiveDate));
								}
							}
							else
							{
								var groupsWithMoreThanOneRate = applicableRates.GroupBy(x => x.ZZ2_ZY1_RateCode).Where(x => x.Count() > 1);
								foreach (var group in groupsWithMoreThanOneRate)
								{
									targetInfo.AddMessageError(Res.GetString("F214809C-1114-4699-A318-6A2AE93D00DB", "There is more than one applicable {0} rate with rate code {1} for the Tariff '{2}' and Country/Region Of Origin '{3}' as at {4}.", rateTypeDescription, group.First().RateCode, tariff, criteria.TradeGroupCountry, criteria.EffectiveDate));
								}
							}
						}
						else
						{
							if (!Parent.InvoiceLine?.IsExport ?? true)
							{
								cusTariff = Parent.Factory.GetCusTariff(Parent.BZ_Type, tariff, effectiveAssessmentDate);
							}
							if (cusTariff == null)
							{
								targetInfo.AddMessageError(ValidationConstants.CusLineTariffDetail.ScheduleTariffDoesNotExists(Parent.BZ_TypeDesc, tariff));
							}
							else
							{
								var tariffTypes = new ZString[] { "3P1", "3P2", "4P1", "4P2", "4P3", "4P4", "4P5", "4P6" };
								var parentTariff = cusTariff.RelatedTariffs.FirstOrDefault(x => x.ZZH_ZZI_TariffTypeCode == "1P1" && !x.ZZH_TariffCode.IsEmpty && x.ZZH_TariffCode != "0");

								if (tariffTypes.Contains(cusTariff.ZZ1_ZZI_NKTariffType) && parentTariff == null)
								{
								}
								else if (tariffTypes.Contains(cusTariff.ZZ1_ZZI_NKTariffType) && parentTariff != null && invoiceLine.JI_Tariff.StartsWith(parentTariff.ZZH_TariffCode))
								{
									targetInfo.AddMessageError(ValidationConstants.CusLineTariffDetail.TariffNotValidForSchedule(Parent.BZ_TypeDesc, tariff));
								}
								else
								{
									var validTariffs = Parent.GetApplicableTariffs(cusTariffType, invoiceLine);
									if (!validTariffs.Any(x => x.ZZ1_TariffCode == tariff))
									{
										targetInfo.AddMessageError(ValidationConstants.CusLineTariffDetail.TariffNotValidForSchedule(Parent.BZ_TypeDesc, tariff));
									}
								}
							}
						}

						if (cusTariff != null && invoiceLine.DistinctAdditionalUOMsFromAllValidTariffs.Count() > 2)
						{
							targetInfo.AddMessageError(ValidationConstants.CusLineTariffDetail.UOMsExceed);
						}
					}
				}
			}
			ValidateFormulaSpecificValue();
		}

		public string Schedule1Part3DForNewError => Res.GetString("F9C54203-DFFC-4BA9-A9EE-D5F2C7D6EA10", "Only applicable when the Goods Type is N. Please refer to the notes as published in Schedule 1 Part 3D for more info about conditions and applicability of this Levy\nhttps://www.sars.gov.za/AllDocs/LegalDoclib/SCEA1964/LAPD-LPrim-Tariff-2012-11 - Schedule No 1 Part 3D.pdf");
	}
}
