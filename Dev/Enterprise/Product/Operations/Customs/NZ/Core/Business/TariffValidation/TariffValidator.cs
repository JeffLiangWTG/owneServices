using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.NZ.Business.EDITariff_ReferenceFiles_NZ;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.NZ.Business.TariffValidation
{
	public class TariffValidator
	{
		public TariffValidator(ITariffValidationData validationData)
		{
			if (validationData == null)
			{
				throw new ArgumentNullException(nameof(validationData));
			}
			this.validationData = validationData;
		}

		public void CheckMainTariff()
		{
			if (CheckTariffCodeEntered())
			{
				CheckTariffCodeIsExcise(validationData.TariffCodeInfo);
				if (!validationData.TariffCodeInfo.HasNotifications())
				{
					if (UniversalTariffHelper.UseRefDatabaseData)
					{
						CheckTariffCode();
					}
					else
					{
						CheckTariffCodeIsRightLengthIsRecognisedAndIsInDate(validationData.TariffCodeInfo);
					}
				}
				if (!validationData.TariffCodeInfo.HasNotifications())
				{
					CheckForPermitCodes();
				}
			}
			validationData.ValidatePartsOfTariffCode();
		}

		void CheckTariffCode()
		{
			if (validationData.TariffBO == null)
			{
				validationData.TariffCodeInfo.AddMessageError(MessageErrorInvalidTariffCode);
			}
		}

		public void CheckPartsOfTariff()
		{
			if (UniversalTariffHelper.UseRefDatabaseData)
			{
				var tariffBO = validationData.TariffBO as TariffView;
				var rateFormulaDerivedFrom = tariffBO?.Rates.FirstOrDefault(x => x.PreferenceCode == UniversalReferenceConstants.TariffCodes.NotQualifies)?.ZZ2_RateFormulaDerivedFrom ?? ZString.Empty;
				var isTariffManual = rateFormulaDerivedFrom.EqualsIgnoringCase("Manual calculation required");
				if (validationData.PartsOfTariffCode.IsEmpty)
				{
					if (tariffBO != null && isTariffManual)
					{
						validationData.PartsOfTariffCodeInfo.AddMessageError(MessageErrorPartsOfPleaseEnterATariffCode);
					}
				}
				else
				{
					if (tariffBO != null && !isTariffManual)
					{
						validationData.PartsOfTariffCodeInfo.AddMessageError(MessageErrorPartsOfDontNeedATariffCodeHere);
					}
					else
					{
						var partsOfTariffBO = validationData.PartsOfTariffBO as TariffView;
						if (partsOfTariffBO == null)
						{
							validationData.PartsOfTariffCodeInfo.AddMessageError(MessageErrorInvalidTariffCode);
						}
						else
						{
							rateFormulaDerivedFrom = partsOfTariffBO.Rates.FirstOrDefault(x => x.PreferenceCode == UniversalReferenceConstants.TariffCodes.NotQualifies)?.ZZ2_RateFormulaDerivedFrom ?? ZString.Empty;
							var isPartsOfTariffManual = rateFormulaDerivedFrom.EqualsIgnoringCase("Manual calculation required");
							if (isPartsOfTariffManual)
							{
								validationData.PartsOfTariffCodeInfo.AddMessageError(MessageErrorPartsOfCannotUsePartsOfTariffCodeHere);
							}
							else if (tariffBO == null)
							{
								validationData.PartsOfTariffCodeInfo.AddMessageError(MessageErrorPartsOfDontNeedATariffCodeHere);
							}
						}
					}
				}
			}
			else
			{
				var tariffBO = validationData.TariffBO as NZCClassification;
				if (validationData.PartsOfTariffCode.IsEmpty)
				{
					if (tariffBO != null && tariffBO.U0_IsManual)
					{
						validationData.PartsOfTariffCodeInfo.AddMessageError(MessageErrorPartsOfPleaseEnterATariffCode);
					}
				}
				else
				{
					if (tariffBO != null && !tariffBO.U0_IsManual)
					{
						validationData.PartsOfTariffCodeInfo.AddMessageError(MessageErrorPartsOfDontNeedATariffCodeHere);
					}
					else
					{
						CheckTariffCodeIsRightLengthIsRecognisedAndIsInDate(validationData.PartsOfTariffCodeInfo);
						if (!validationData.PartsOfTariffCodeInfo.HasMessageErrors())
						{
							var partsOfTariffBO = validationData.PartsOfTariffBO as NZCClassification;
							if (partsOfTariffBO != null)
							{
								if (partsOfTariffBO.U0_IsManual)
								{
									validationData.PartsOfTariffCodeInfo.AddMessageError(MessageErrorPartsOfCannotUsePartsOfTariffCodeHere);
								}
								else if (tariffBO == null)
								{
									validationData.PartsOfTariffCodeInfo.AddMessageError(MessageErrorPartsOfDontNeedATariffCodeHere);
								}
							}
						}
					}
				}
			}
		}

		#region Implementation

		bool CheckTariffCodeEntered()
		{
			if (!validationData.EmptyTariffIsAllowed && validationData.TariffCode.IsEmpty)
			{
				if (validationData.EmptyTariffIsFullError)
				{
					validationData.TariffCodeInfo.AddError(MessageErrorTariffCodeMissing);
				}
				else
				{
					validationData.TariffCodeInfo.AddMessageError(MessageErrorTariffCodeMissing);
				}
				return false;
			}
			return true;
		}

		void CheckTariffCodeIsRightLengthIsRecognisedAndIsInDate(ZPropertyInfo propertyInfo)
		{
			ZString tariffCode = (ZString)propertyInfo.Value;
			if (!tariffCode.IsEmpty)
			{
				if (!TariffCodeLengthIsOk(validationData.AllowableTariffCodeTypes, tariffCode))
				{
					propertyInfo.AddMessageError(MessageErrorTariffCodeInvalidLength);
				}
				else
				{
					NonDependentNZCClassificationCollection tariffs = new NonDependentNZCClassificationCollection(validationData.Factory);
					tariffs.Load(tariffCode);
					if (tariffs.Count == 0)
					{
						propertyInfo.AddMessageError(MessageErrorTariffCodeNotOnFile);
					}
					else
					{
						ZDateTime dateForDutyRate = validationData.DateForDutyRate.Date;
						if (dateForDutyRate > tariffs.DateRange.DateActiveTo)
						{
							propertyInfo.AddMessageError(MessageErrorTariffCodeOutOfDate + tariffs.DateRange.DateActiveTo.ToShortDateString() + ". " + MessageErrorAdditionalNotInDateString + dateForDutyRate.ToShortDateString() + ")");
						}
						else if (dateForDutyRate < tariffs.DateRange.DateActiveFrom)
						{
							propertyInfo.AddMessageError(MessageErrorTariffCodeNotInForceYet + tariffs.DateRange.DateActiveFrom.ToShortDateString() + ". " + MessageErrorAdditionalNotInDateString + dateForDutyRate.ToShortDateString() + ")");
						}
					}
				}
			}
		}

		void CheckTariffCodeIsExcise(ZPropertyInfo propertyInfo)
		{
			var tariffCode = (ZString)propertyInfo.Value;
			if (tariffCode.StartsWith("99") && validationData.AllowableTariffCodeTypes.ImportOrExport)
			{
				propertyInfo.AddMessageError(MessageErrorTariffCodeIsExciseForImpOrExp);
			}
		}

		bool TariffCodeLengthIsOk(AllowableTariffCodeTypes allowableTariffCodeTypes, ZString tariffCode)
		{
			int tariffLength = tariffCode.Length;
			bool result = false;
			if (allowableTariffCodeTypes.ImportOrExport)
			{
				result |= (tariffLength == 14);
			}

			if (allowableTariffCodeTypes.Excise)
			{
				result |= (tariffLength == 9);
			}

			return result;
		}

		void CheckForPermitCodes()
		{
			if (validationData.PermitCodeCount == 0)
			{
				NZCTariffsPermitsApplyTo permitsRequired = NZCTariffsPermitsApplyTo.Load(validationData.Factory, validationData.TariffCode, validationData.AllowableTariffCodeTypes.Import, validationData.AllowableTariffCodeTypes.Export);
				if (permitsRequired != null)
				{
					validationData.TariffCodeInfo.AddWarning(WarningPermitCodeMayBeRequiredForThisTariff + permitsRequired.U6_PermitCodes + ").");
				}
			}
		}

		readonly ITariffValidationData validationData;

		#endregion

		public const string MessageErrorInvalidTariffCode = "Invalid Tariff Code.";
		public const string MessageErrorTariffCodeMissing = "You must enter a Tariff Code.";
		public const string MessageErrorTariffCodeInvalidLength = "Invalid Tariff Code - Tariff Code is Incorrect Length.";
		public const string MessageErrorTariffCodeNotOnFile = "Invalid Tariff Code - Not Present in the Tariff Master File.";
		public const string MessageErrorTariffCodeNotInForceYet = "This Tariff Code will not come into effect until: ";
		public const string MessageErrorTariffCodeOutOfDate = "This Tariff Code expired on: ";
		public const string WarningPermitCodeMayBeRequiredForThisTariff = "According to the Tariff, Permit Codes may be required for this Tariff Item (for ";
		public const string MessageErrorAdditionalNotInDateString = "\r\n   (Based on a Duty Rate Date of: ";
		public const string MessageErrorTariffCodeIsExciseForImpOrExp = "Excise tariff numbers should not be used for Export or Import entry types. It should be used for Excise entry types.";

		public const string MessageErrorPartsOfPleaseEnterATariffCode = "You've used a 'Parts Of' Tariff Code on this Invoice Line. Please enter a Tariff Code for calculating the Duty Rate here.";
		public const string MessageErrorPartsOfCannotUsePartsOfTariffCodeHere = "Cannot use a 'Parts Of' Tariff Here. Please enter the 'Parts Of' Tariff Code in the 'Tariff' field and a Tariff Code for calculating the Duty Rate here.";
		public const string MessageErrorPartsOfDontNeedATariffCodeHere = "You don't need a Tariff Code here unless you enter a 'Parts Of' Tariff in the 'Tariff' field. Please remove this Tariff Code.";
	}
}
