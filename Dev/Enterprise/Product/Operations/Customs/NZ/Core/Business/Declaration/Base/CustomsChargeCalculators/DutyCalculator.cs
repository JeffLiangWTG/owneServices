using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.NZ.Business.EDITariff_ReferenceFiles_NZ;
using Enterprise.Customs.NZ.Registry;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.NZ.Business.Declaration.CustomsChargeCalculators
{
	public interface IHaveTheParametersRequiredToCalculateDuty
	{
		ZString ClassificationCode { get; }
		ZString ConcessionCode { get; }
		ZBool QualifiesForPreferentialDuty { get; }
		ZString CountryOfOrigin { get; }
		ZString PreferentialCountryGroup { get; }
		ZDecimal StatQty { get; }
		ZString StatUQ { get; }
		ZDecimal SuppQty { get; }
		ZString SuppUQ { get; }
		ZDecimal ValueForDuty { get; }
		ZDateTime DateForDutyRate { get; }
		ZString PartsOfClassification { get; }
		ZBool IsZeroRatedDuty { get; }
		ZBool IsZeroRatedExcise { get; }
		ZBool IsZeroRatedLevies { get; }
		BusinessObjectFactory Factory { get; }
	}

	public class DutyRateParameters : IHaveTheParametersRequiredToCalculateDuty
	{
		public DutyRateParameters(ZString classificationCode, ZString concessionCode, ZString partsOfClassification, BusinessObjectFactory factory)
		{
			this.classificationCode = classificationCode;
			this.concessionCode = concessionCode;
			this.partsOfClassification = partsOfClassification;
			this.factory = factory ?? throw new ArgumentNullException(nameof(factory), "BusinessObjectFactory factory");
			today = ZDateTime.Today;
		}
		readonly ZString classificationCode;
		readonly ZString concessionCode;
		readonly ZString partsOfClassification;
		readonly BusinessObjectFactory factory;
		readonly ZDateTime today;

		public ZString ClassificationCode
		{
			get { return classificationCode; }
		}

		public ZString ConcessionCode
		{
			get { return concessionCode; }
		}

		public ZBool QualifiesForPreferentialDuty
		{
			get { return false; }
		}

		public ZString CountryOfOrigin
		{
			get { return ZString.Empty; }
		}

		public ZString PreferentialCountryGroup
		{
			get { return ZString.Empty; }
		}

		public ZDecimal StatQty
		{
			get { return ZDecimal.Zero; }
		}

		public ZString StatUQ
		{
			get { return ZString.Empty; }
		}

		public ZDecimal SuppQty
		{
			get { return ZDecimal.Zero; }
		}

		public ZString SuppUQ
		{
			get { return ZString.Empty; }
		}

		public ZDecimal ValueForDuty
		{
			get { return ZDecimal.Zero; }
		}

		public ZDateTime DateForDutyRate
		{
			get { return today; }
		}

		public ZString PartsOfClassification
		{
			get { return partsOfClassification; }
		}

		public ZBool IsZeroRatedDuty
		{
			get { return false; }
		}

		public ZBool IsZeroRatedExcise
		{
			get { return false; }
		}

		public ZBool IsZeroRatedLevies
		{
			get { return false; }
		}

		public BusinessObjectFactory Factory
		{
			get { return factory; }
		}
	}

	public sealed class DutyCalculator
	{
		public class CusEntryLineWrapper : IHaveTheParametersRequiredToCalculateDuty
		{
			public CusEntryLineWrapper(CusEntryLine entryLine)
			{
				this.entryLine = entryLine;
			}
			readonly CusEntryLine entryLine;

			public ZString ClassificationCode
			{
				get { return entryLine.CL_AdValoremTariff; }
			}

			public ZString ConcessionCode
			{
				get { return entryLine.ConcessionCode; }
			}

			public ZBool QualifiesForPreferentialDuty
			{
				get { return (entryLine.PreferentialDutyIndicator == QualifiesForPreferentialDutyList.Codes.Qualifies); }
			}

			public ZString CountryOfOrigin
			{
				get { return entryLine.CountryOfOrigin; }
			}

			public ZString PreferentialCountryGroup
			{
				get { return entryLine.PreferentialDutyGroup; }
			}

			public ZDecimal StatQty
			{
				get { return entryLine.StatisticalQty; }
			}

			public ZString StatUQ
			{
				get { return entryLine.StatisticalUnit; }
			}

			public ZDecimal SuppQty
			{
				get { return entryLine.SupplementaryQty; }
			}

			public ZString SuppUQ
			{
				get { return entryLine.SupplementaryUQ; }
			}

			public ZDecimal ValueForDuty
			{
				get { return entryLine.VFDWholeNZD; }
			}

			public ZDateTime DateForDutyRate
			{
				get { return entryLine.Header.DateForDutyRate; }
			}

			public ZString PartsOfClassification
			{
				get { return entryLine.PartsOfClassification; }
			}

			public ZBool IsZeroRatedDuty
			{
				get { return entryLine.IsZeroRatedDuty; }
			}

			public ZBool IsZeroRatedExcise
			{
				get { return entryLine.IsZeroRatedExcise; }
			}

			public ZBool IsZeroRatedLevies
			{
				get { return entryLine.IsZeroRatedLevies; }
			}

			public BusinessObjectFactory Factory
			{
				get { return entryLine.Factory; }
			}
		}

		public class JobComInvoiceLineWrapper : IHaveTheParametersRequiredToCalculateDuty
		{
			public JobComInvoiceLineWrapper(JobComInvoiceLine invoiceLine)
			{
				this.invoiceLine = invoiceLine;
			}
			readonly JobComInvoiceLine invoiceLine;

			public ZString ClassificationCode
			{
				get { return invoiceLine.JI_Tariff; }
			}

			public ZString ConcessionCode
			{
				get { return invoiceLine.JI_ConcessionCode; }
			}

			public ZBool QualifiesForPreferentialDuty
			{
				get { return (invoiceLine.JI_EffectiveQualifiesForPreferentialDuty == QualifiesForPreferentialDutyList.Codes.Qualifies); }
			}

			public ZString CountryOfOrigin
			{
				get { return invoiceLine.JI_RN_NKEffectiveCountryOfOrigin; }
			}

			public ZString PreferentialCountryGroup
			{
				get { return invoiceLine.JI_EffectivePreferentialCountryGroup; }
			}

			public ZDecimal StatQty
			{
				get { return invoiceLine.JI_CustomsQuantity; }
			}

			public ZString StatUQ
			{
				get { return invoiceLine.JI_CustomsUnitQty; }
			}

			public ZDecimal SuppQty
			{
				get { return invoiceLine.JI_SupplementaryQty; }
			}

			public ZString SuppUQ
			{
				get { return invoiceLine.JI_SupplementaryUQ; }
			}

			public ZDecimal ValueForDuty
			{
				get { return invoiceLine.CustomsValueInLocalCurrencyRounded; }
			}

			public ZDateTime DateForDutyRate
			{
				get { return invoiceLine.DateForDutyRate; }
			}

			public ZString PartsOfClassification
			{
				get { return invoiceLine.JI_PartsOfClassification; }
			}

			public ZBool IsZeroRatedDuty
			{
				get { return invoiceLine.EffectiveIsZeroRatedDuty; }
			}

			public ZBool IsZeroRatedExcise
			{
				get { return invoiceLine.EffectiveIsZeroRatedExcise; }
			}

			public ZBool IsZeroRatedLevies
			{
				get { return invoiceLine.EffectiveIsZeroRatedLevies; }
			}

			public BusinessObjectFactory Factory
			{
				get { return invoiceLine.Factory; }
			}
		}

		#region Interface

		public DutyCalculator(CusEntryLine entryLine)
			: this(new CusEntryLineWrapper(entryLine))
		{
		}

		public DutyCalculator(JobComInvoiceLine invoiceLine)
			: this(new JobComInvoiceLineWrapper(invoiceLine))
		{
		}

		public DutyCalculator(IHaveTheParametersRequiredToCalculateDuty dutyRateParameters)
		{
			this.dutyRateParameters = dutyRateParameters ?? throw new ArgumentNullException(nameof(dutyRateParameters), "ICalculateDuty dutyRateParameters");
			factory = dutyRateParameters.Factory;
			cacheMgr = new CacheManager();
			GetDutyFromDutyRateParameters();
		}
		readonly IHaveTheParametersRequiredToCalculateDuty dutyRateParameters;
		readonly CacheManager cacheMgr;
		readonly BusinessObjectFactory factory;

		public void Update()
		{
			GetDutyFromDutyRateParameters();
		}

		#region Properties Returning Results
		public ZString ActualClassification
		{
			get { return fActualClassification; }
		}

		public ZDecimal TotalDutiesAndLevies
		{
			get
			{
				return
					fDutyAmount
					+ fALACLevyAmount
					+ fACCFuelLevyAmount
					+ fPFMLFuelLevyAmount
					+ fHERALevyAmount
					+ fSyntheticGreenhouseGasesLevyAmount;
			}
		}

		public ZString ActualPreferentialCountryGroup
		{
			get { return fActualPreferentialCountryGroup; }
		}
		ZString fActualPreferentialCountryGroup;

		public ZDecimal DutyRatePercent
		{
			get { return fDutyRatePercent; }
		}

		public ZDecimal DutyRateFlatRate
		{
			get { return fDutyRateFlatRate; }
		}

		public ZString DutyRateFlatUQ
		{
			get { return fDutyRateFlatUQ; }
		}

		public ZString DutyRateOnly
		{
			get { return fDutyRateOnly; }
		}

		public ZString DutyRateSource
		{
			get { return fDutyRateSource; }
		}

		public ZString DutyRateComplete
		{
			get { return fDutyRateSource + " = " + fDutyRateOnly; }
		}

		public ZDecimal DutyAmount
		{
			get { return fDutyAmount; }
		}

		public ZDecimal ALACLevyAmount
		{
			get { return fALACLevyAmount; }
		}

		public ZDecimal ACCFuelLevyAmount
		{
			get { return fACCFuelLevyAmount; }
		}

		public ZDecimal PFMLFuelLevyAmount
		{
			get { return fPFMLFuelLevyAmount; }
		}

		public ZDecimal SyntheticGreenhouseGasesLevyAmount
		{
			get { return fSyntheticGreenhouseGasesLevyAmount; }
		}

		public ZDecimal HERALevyAmount
		{
			get { return fHERALevyAmount; }
		}
		#endregion

		public const string NormalDutyRateGroupCode = "NML";
		public const string FreeDutyRateDescription = "FREE";

		#endregion

		#region Implementation

		void AppendDutyRateString(string dutyRate)
		{
			fDutyRateOnly += (fDutyRateOnly.IsEmpty ? "" : " ") + dutyRate;
		}

		void GetDutyFromDutyRateParameters()
		{
			if (UniversalTariffHelper.UseRefDatabaseData)
			{
				GetDutyForUniversalTariff(dutyRateParameters.ClassificationCode,
				dutyRateParameters.ConcessionCode,
				dutyRateParameters.QualifiesForPreferentialDuty,
				dutyRateParameters.CountryOfOrigin,
				dutyRateParameters.PreferentialCountryGroup,
				dutyRateParameters.StatQty,
				dutyRateParameters.StatUQ,
				dutyRateParameters.SuppQty,
				dutyRateParameters.SuppUQ,
				dutyRateParameters.ValueForDuty,
				dutyRateParameters.DateForDutyRate,
				dutyRateParameters.PartsOfClassification,
				dutyRateParameters.IsZeroRatedDuty,
				dutyRateParameters.IsZeroRatedExcise,
				dutyRateParameters.IsZeroRatedLevies);
			}
			else
			{
				GetDuty(dutyRateParameters.ClassificationCode,
				dutyRateParameters.ConcessionCode,
				dutyRateParameters.QualifiesForPreferentialDuty,
				dutyRateParameters.CountryOfOrigin,
				dutyRateParameters.PreferentialCountryGroup,
				dutyRateParameters.StatQty,
				dutyRateParameters.StatUQ,
				dutyRateParameters.SuppQty,
				dutyRateParameters.SuppUQ,
				dutyRateParameters.ValueForDuty,
				dutyRateParameters.DateForDutyRate,
				dutyRateParameters.PartsOfClassification,
				dutyRateParameters.IsZeroRatedDuty,
				dutyRateParameters.IsZeroRatedExcise,
				dutyRateParameters.IsZeroRatedLevies);
			}
		}

		#region Cache Management

		class CacheManager
		{
			public CacheManager()
			{
				lastClassificationCode = "";
				lastConcessionCode = "";
				lastQualifiesForPreferentialDuty = false;
				lastCountryOfOrigin = "";
				lastStatQty = 0m;
				lastStatUQ = "";
				lastSuppQty = 0m;
				lastSuppUQ = "";
				lastValueForDuty = 0m;
				lastDateForDutyRate = ZDateTime.Empty;
				lastPartsOfClassification = "";
				lastIsZeroRatedDuty = false;
				lastIsZeroRatedExcise = false;
				lastIsZeroRatedLevies = false;
			}

			public bool IsAlreadyLoaded(
				ZString classificationCode,
				ZString concessionCode,
				ZBool qualifiesForPreferentialDuty,
				ZString countryOfOrigin,
				ZString preferentialCountryGroup,
				ZDecimal statQty,
				ZString statUQ,
				ZDecimal suppQty,
				ZString suppUQ,
				ZDecimal valueForDuty,
				ZDateTime dateForDutyRate,
				ZString partsOfClassification,
				ZBool isZeroRatedDuty,
				ZBool isZeroRatedExcise,
				ZBool isZeroRatedLevies)
			{
				bool result = (
					lastClassificationCode == classificationCode &&
					lastConcessionCode == concessionCode &&
					lastQualifiesForPreferentialDuty == qualifiesForPreferentialDuty &&
					lastCountryOfOrigin == countryOfOrigin &&
					lastPreferentialCountryGroup == preferentialCountryGroup &&
					lastStatQty == statQty &&
					lastStatUQ == statUQ &&
					lastSuppQty == suppQty &&
					lastSuppUQ == suppUQ &&
					lastValueForDuty == valueForDuty &&
					lastDateForDutyRate == dateForDutyRate &&
					lastPartsOfClassification == partsOfClassification &&
					lastIsZeroRatedDuty == isZeroRatedDuty &&
					lastIsZeroRatedExcise == isZeroRatedExcise &&
					lastIsZeroRatedLevies == isZeroRatedLevies
					);
				if (!result)
				{
					lastClassificationCode = classificationCode;
					lastConcessionCode = concessionCode;
					lastQualifiesForPreferentialDuty = qualifiesForPreferentialDuty;
					lastCountryOfOrigin = countryOfOrigin;
					lastPreferentialCountryGroup = preferentialCountryGroup;
					lastStatQty = statQty;
					lastStatUQ = statUQ;
					lastSuppQty = suppQty;
					lastSuppUQ = suppUQ;
					lastValueForDuty = valueForDuty;
					lastDateForDutyRate = dateForDutyRate;
					lastPartsOfClassification = partsOfClassification;
					lastIsZeroRatedDuty = isZeroRatedDuty;
					lastIsZeroRatedExcise = isZeroRatedExcise;
					lastIsZeroRatedLevies = isZeroRatedLevies;
				}

				return result;
			}

			ZString lastClassificationCode;
			ZString lastConcessionCode;
			ZBool lastQualifiesForPreferentialDuty;
			ZString lastCountryOfOrigin;
			ZString lastPreferentialCountryGroup;
			ZDecimal lastStatQty;
			ZString lastStatUQ;
			ZDecimal lastSuppQty;
			ZString lastSuppUQ;
			ZDecimal lastValueForDuty;
			ZDateTime lastDateForDutyRate;
			ZString lastPartsOfClassification;
			ZBool lastIsZeroRatedDuty;
			ZBool lastIsZeroRatedExcise;
			ZBool lastIsZeroRatedLevies;
		}

		#endregion

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		void GetDuty(
			ZString classificationCode,
			ZString concessionCode,
			ZBool qualifiesForPreferentialDuty,
			ZString countryOfOrigin,
			ZString preferentialCountryGroup,
			ZDecimal statQty,
			ZString statUQ,
			ZDecimal suppQty,
			ZString suppUQ,
			ZDecimal valueForDuty,
			ZDateTime dateForDutyRate,
			ZString partsOfClassification,
			ZBool isZeroRatedDuty,
			ZBool isZeroRatedExcise,
			ZBool isZeroRatedLevies)
		{
			Predicate<NZCClassificationDutyRate> shouldCalculateRatedDuty = dutyRate => !dutyRate.U1_DutyRatePercent.IsEmpty && !isZeroRatedDuty;
			Predicate<NZCClassificationDutyRate> shouldCalculateRatedExcise = dutyRate => !dutyRate.U1_DutyRatePerUnit1.IsEmpty && !isZeroRatedExcise;
			Predicate<NZCClassificationDutyRate> shouldCalculateRatedLevies = dutyRate => !dutyRate.U1_DutyRatePerUnit2.IsEmpty && !isZeroRatedLevies;
			if (!cacheMgr.IsAlreadyLoaded(classificationCode, concessionCode, qualifiesForPreferentialDuty, countryOfOrigin, preferentialCountryGroup, statQty, statUQ, suppQty, suppUQ, valueForDuty, dateForDutyRate, partsOfClassification, isZeroRatedDuty, isZeroRatedExcise, isZeroRatedLevies))
			{
				ResetAllExposedValues();
				fActualClassification = classificationCode;

				NZCClassification baseClassification = NZCClassification.GetClassForCompleteCode(factory, classificationCode, dateForDutyRate);
				if (baseClassification != null)
				{
					ZString countryForPreferentialTreatment = "";
					ZString groupSelectedForPreferentialTreatment = "";
					if (qualifiesForPreferentialDuty)
					{
						countryForPreferentialTreatment = countryOfOrigin;
						groupSelectedForPreferentialTreatment = preferentialCountryGroup;
					}

					NZCClassification classificationForDutyRate = null;
					if (!partsOfClassification.IsEmpty)
					{
						classificationForDutyRate = NZCClassification.GetClassForCompleteCode(factory, partsOfClassification, dateForDutyRate);
					}
					if (classificationForDutyRate == null)
					{
						classificationForDutyRate = baseClassification;
					}

					QtyAndUnitManager qtysAndUnits = new QtyAndUnitManager(statQty, statUQ, suppQty, classificationForDutyRate.IsPetrolClassification ? new ZString(SupplementaryUQList.Codes.GramsOfLead) : suppUQ);

					ResetDutyAmountsAndRate();
					if (concessionCode.IsEmpty)
					{
						if (!classificationForDutyRate.U0_AlternateTariff.IsEmpty)
						{
							NZCClassification alternateClassification = NZCClassification.GetClassForCompleteCode(factory, classificationForDutyRate.U0_AlternateTariff, dateForDutyRate);
							if (alternateClassification != null)
							{
								CalculateTariffDuty(classificationForDutyRate, countryForPreferentialTreatment, groupSelectedForPreferentialTreatment, valueForDuty, qtysAndUnits, dateForDutyRate, shouldCalculateRatedDuty, shouldCalculateRatedExcise, shouldCalculateRatedLevies);
								ZDecimal primaryTotalAmount = TotalDutiesAndLevies;
								ResetDutyAmountsAndRate();

								CalculateTariffDuty(alternateClassification, countryForPreferentialTreatment, groupSelectedForPreferentialTreatment, valueForDuty, qtysAndUnits, dateForDutyRate, shouldCalculateRatedDuty, shouldCalculateRatedExcise, shouldCalculateRatedLevies);
								ZDecimal alternateTotalAmount = TotalDutiesAndLevies;
								ResetDutyAmountsAndRate();

								if (alternateTotalAmount > primaryTotalAmount)
								{
									classificationForDutyRate = alternateClassification;
									fActualClassification = alternateClassification.U0_Tariff;
								}
							}
						}

						CalculateTariffDuty(classificationForDutyRate, countryForPreferentialTreatment, groupSelectedForPreferentialTreatment, valueForDuty, qtysAndUnits, dateForDutyRate, shouldCalculateRatedDuty, shouldCalculateRatedExcise, shouldCalculateRatedLevies);
					}
					else
					{
						var concessions = factory.Load<NZCConcession>(new ZQuery(NZCConcessionSchema.U2_Code, concessionCode));

						NZCConcessionDutyRate concessionDutyRate = null;
						foreach (NZCConcession concession in concessions)
						{
							if (concession.U2_DateActiveFrom <= dateForDutyRate)
							{
								concessionDutyRate = GetConcessionDutyRate(concession, groupSelectedForPreferentialTreatment.IsEmpty ? GetFallbackCountryGroup(countryForPreferentialTreatment) : groupSelectedForPreferentialTreatment);

								if (concessionDutyRate != null && concessionDutyRate.U5_PreferentialCountryGroup != NormalDutyRateGroupCode)
								{
									fActualPreferentialCountryGroup = concessionDutyRate.U5_PreferentialCountryGroup;
								}

								CalculateConcessionDuty(concessionDutyRate, valueForDuty, qtysAndUnits, isZeroRatedDuty, isZeroRatedExcise);
								break;
							}
						}

						if (concessionDutyRate == null || (concessionDutyRate.U5_DutyRatePercent.IsEmpty && concessionDutyRate.U5_DutyRatePerUnit.IsEmpty))
						{
							CalculateTariffDuty(classificationForDutyRate, countryForPreferentialTreatment, groupSelectedForPreferentialTreatment, valueForDuty, qtysAndUnits, dateForDutyRate, _ => false, duty => !duty.U1_DutyRatePercent.IsEmpty && shouldCalculateRatedExcise(duty), _ => false);
						}
					}
					CalculateLevies(classificationForDutyRate, qtysAndUnits, isZeroRatedLevies, dateForDutyRate);
					string countryGroupOrCountryCodeUsedForDutyRateFound = fActualPreferentialCountryGroup.IsEmpty ? NormalDutyRateGroupCode : fActualPreferentialCountryGroup.ToString();
					fDutyRateSource = fActualClassification + (concessionCode.IsEmpty ? "" : "-" + concessionCode) + " @ " + countryGroupOrCountryCodeUsedForDutyRateFound;
					if (fDutyRateOnly.IsEmpty)
					{
						AppendDutyRateString(FreeDutyRateDescription);
					}
				}
			}
		}

		void CalculateTariffDuty(NZCClassification classification, ZString countryForPrefRates, ZString groupSelectedForPrefRates, ZDecimal valueForDuty, QtyAndUnitManager qtysAndUnits, ZDateTime dateForDutyRate, Predicate<NZCClassificationDutyRate> shouldCalculateRatedDuty, Predicate<NZCClassificationDutyRate> shouldCalculateRatedExcise, Predicate<NZCClassificationDutyRate> shouldCalculateRatedLevies)
		{
			NZCClassification classificationWithRateChildRows = NZCClassification.GetClassThatHasDutyRatesAgainstIt(factory, classification.U0_Tariff);
			NZCClassificationDutyRate dutyRate = null;
			bool gotARateResult = false;

			if (!countryForPrefRates.IsEmpty)
			{
				NZCGroup[] possibleGroups = NZCGroup.FromCountry(countryForPrefRates, dateForDutyRate, factory);

				NZCGroup onlyGroupPossible = null;

				if (!groupSelectedForPrefRates.IsEmpty)
				{
					onlyGroupPossible = Array.Find(possibleGroups, possibleGroup => possibleGroup.Q4_Code == groupSelectedForPrefRates);
				}

				if (onlyGroupPossible == null && possibleGroups.Length == 1)
				{
					onlyGroupPossible = possibleGroups[0];
				}

				if (onlyGroupPossible != null)
				{
					dutyRate = GetTariffDutyRate(classificationWithRateChildRows, onlyGroupPossible.Q4_Code, dateForDutyRate);
					if (dutyRate != null)
					{
						gotARateResult = true;
						fActualPreferentialCountryGroup = onlyGroupPossible.Q4_Code;
					}
				}
				else
				{
					if (NZCustomsDataRegistry.Instance.PreferentialCountryGroupCodeDefaulting.Value)
					{
						gotARateResult = GetBestDutyRate(ref dutyRate, valueForDuty, qtysAndUnits, dateForDutyRate, shouldCalculateRatedDuty, shouldCalculateRatedExcise, shouldCalculateRatedLevies, classificationWithRateChildRows, possibleGroups);
					}
				}
			}

			if (!gotARateResult)
			{
				dutyRate = GetTariffDutyRate(classificationWithRateChildRows, NormalDutyRateGroupCode, dateForDutyRate);
				fActualPreferentialCountryGroup = NormalDutyRateGroupCode;
			}

			CalculateTariffDuty(dutyRate, valueForDuty, qtysAndUnits, shouldCalculateRatedDuty, shouldCalculateRatedExcise, shouldCalculateRatedLevies);
		}

		bool GetBestDutyRate(ref NZCClassificationDutyRate dutyRateResult, ZDecimal valueForDuty, QtyAndUnitManager qtysAndUnits, ZDateTime dateForDutyRate, Predicate<NZCClassificationDutyRate> shouldCalculateRatedDuty, Predicate<NZCClassificationDutyRate> shouldCalculateRatedExcise, Predicate<NZCClassificationDutyRate> shouldCalculateRatedLevies, NZCClassification classificationWithRateChildRows, NZCGroup[] possibleGroups)
		{
			bool gotARateResult = false;
			ZDecimal bestDutyValue = decimal.MaxValue;
			NZCClassificationDutyRate bestDutyRate = null;
			ZString bestPreferentialCountryGroup = ZString.Empty;

			foreach (NZCGroup group in possibleGroups) // Should already be ordered to pickup the Country specific rate first, THEN the 2 x preferential rates.
			{
				NZCClassificationDutyRate groupDutyRate = GetTariffDutyRate(classificationWithRateChildRows, group.Q4_Code, dateForDutyRate);
				if (groupDutyRate == null && group.Q4_IsFreeUnlessOtherwiseIndicated)
				{
					dutyRateResult = null;
					gotARateResult = false;
					fActualPreferentialCountryGroup = group.Q4_Code;
					break;
				}

				if (groupDutyRate != null)
				{
					ZDecimal dutyAmount = GetDutyAmount(groupDutyRate, valueForDuty, qtysAndUnits, shouldCalculateRatedDuty, shouldCalculateRatedExcise, shouldCalculateRatedLevies);

					if (dutyAmount == 0)
					{
						dutyRateResult = groupDutyRate;
						gotARateResult = true;
						fActualPreferentialCountryGroup = group.Q4_Code;
						break;
					}

					if (dutyAmount < bestDutyValue)
					{
						bestDutyValue = dutyAmount;
						bestDutyRate = groupDutyRate;
						bestPreferentialCountryGroup = group.Q4_Code;
					}
				}
			}

			if (!gotARateResult && bestDutyRate != null)
			{
				dutyRateResult = bestDutyRate;
				fActualPreferentialCountryGroup = bestPreferentialCountryGroup;
				gotARateResult = true;
			}

			return gotARateResult;
		}

		void CalculateLevies(NZCClassification classification, QtyAndUnitManager qtysAndUnits, ZBool isZeroRatedLevies, ZDateTime dateForDutyRate)
		{
			if (!isZeroRatedLevies)
			{
				if (factory.GetCachedValue("NZCClassificationLevyRate.ExistsInCurrentDataVersion", delegate
				{
					return NZCClassificationLevyRate.ExistsInCurrentDataVersion;
				}))
				{
					var classificationForDutyRate = NZCClassification.GetClassThatHasDutyRatesAgainstIt(factory, classification.U0_Tariff);
					NZCClassificationLevyRate[] levyRates = factory.Load<NZCClassificationLevyRate>(NZCClassificationLevyRate.GetLevyRatesQuery(classificationForDutyRate.PK, dateForDutyRate));

					foreach (NZCClassificationLevyRate levyRate in levyRates)
					{
						ApplyLevyRate(qtysAndUnits, levyRate.L0_LevyCode, levyRate.L0_LevyRate, levyRate.L0_LevyUnit);
					}
				}
				else
				{
					ApplyLevyRate(qtysAndUnits, classification.U0_LevyCode, classification.U0_LevyRate, classification.U0_LevyUnit);
				}
			}
		}

		void ApplyLevyRate(QtyAndUnitManager qtysAndUnits, ZString levyCode, ZDecimal levyRate, ZString levyUnit)
		{
			if (levyCode == "AL")
			{
				CalculateAlcoholLevy(levyRate, levyUnit, qtysAndUnits);
				AppendDutyRateString("ALAC:$" + levyRate.ToString(4) + "/" + levyUnit);
			}
			else if (levyCode == "SL")
			{
				CalculateSteelLevy(levyRate, levyUnit, qtysAndUnits);
				AppendDutyRateString("HERA:$" + levyRate.ToString(4) + "/" + levyUnit);
			}
			else if (levyCode == "AC" || levyCode == "AU")
			{
				CalculateACCFuelLevy(levyRate, levyUnit, qtysAndUnits);
				AppendDutyRateString("ACC:$" + levyRate.ToString(4) + "/" + levyUnit);
			}
			else if (levyCode == "PF" || levyCode == "PS")
			{
				CalculatePFMLFuelLevy(levyRate, levyUnit, qtysAndUnits);
				AppendDutyRateString("PFML:$" + levyRate.ToString(5) + "/" + levyUnit);
			}
			else if (levyCode == "GG" || levyCode == "GS")
			{
				CalculateSyntheticGreenhouseGasesLevy(levyRate, levyUnit, qtysAndUnits);
				AppendDutyRateString("SGG:$" + levyRate.ToString(5) + "/" + levyUnit);
			}
		}

		ZString GetFallbackCountryGroup(ZString countryCode)
		{
			ZString result = "";
			NZCCountryCollection countries = new NZCCountryCollection(factory);
			ZQuery countryFilter = new ZQuery(NZCCountrySchema.U9_Code, countryCode);
			countries.Load(countryFilter);
			if (countries.Count == 1)
			{
				result = countries[0].U9_PreferentialCountryGroup;
			}
			return result;
		}

		#region QtyAndUnitManager - Unit management getting right quantity for right unit
		class QtyAndUnitManager
		{
			public QtyAndUnitManager(ZDecimal statQty, ZString statUQ, ZDecimal suppQty, ZString suppUQ)
			{
				StatQty = statQty;
				StatUQ = statUQ;
				SuppQty = suppQty;
				SuppUQ = suppUQ;
			}

			public ZDecimal GetQtyForUnit(ZString uQ)
			{
				ZDecimal result = GetRightQtyForUnitRequired(uQ);
				if (result.IsEmpty)
				{
					switch (uQ)
					{
						case StatisticalUQList.Codes.Kilograms:
							result = GetRightQtyForUnitRequired(StatisticalUQList.Codes.Tonnes) * 1000;
							break;
						case StatisticalUQList.Codes.Tonnes:
							result = GetRightQtyForUnitRequired(StatisticalUQList.Codes.Kilograms) / 1000;
							break;
					}
				}
				return result;
			}

			ZDecimal GetRightQtyForUnitRequired(ZString uQForQtyRequired)
			{
				ZDecimal result = 0m;
				if (StatUQ == uQForQtyRequired)
				{
					result = StatQty;
				}
				else if (SuppUQ == uQForQtyRequired)
				{
					result = SuppQty;
				}
				return result;
			}

			public readonly ZDecimal StatQty;
			public readonly ZString StatUQ;
			public readonly ZDecimal SuppQty;
			public readonly ZString SuppUQ;
		}
		#endregion

		#region Levy Calculators
		void CalculateAlcoholLevy(ZDecimal rate, ZString unit, QtyAndUnitManager qtysAndUnits)
		{
			fALACLevyAmount = ZArchitecture.Core.Utilities.Round(rate * qtysAndUnits.GetQtyForUnit(unit), 2);
		}

		void CalculateSteelLevy(ZDecimal rate, ZString unit, QtyAndUnitManager qtysAndUnits)
		{
			fHERALevyAmount = ZArchitecture.Core.Utilities.Round(rate * qtysAndUnits.GetQtyForUnit(unit), 2);
		}

		void CalculateACCFuelLevy(ZDecimal rate, ZString unit, QtyAndUnitManager qtysAndUnits)
		{
			fACCFuelLevyAmount = ZArchitecture.Core.Utilities.Round(rate * qtysAndUnits.GetQtyForUnit(unit), 2);
		}

		void CalculatePFMLFuelLevy(ZDecimal rate, ZString unit, QtyAndUnitManager qtysAndUnits)
		{
			fPFMLFuelLevyAmount = ZArchitecture.Core.Utilities.Round(rate * qtysAndUnits.GetQtyForUnit(unit), 2);
		}

		void CalculateSyntheticGreenhouseGasesLevy(ZDecimal rate, ZString unit, QtyAndUnitManager qtysAndUnits)
		{
			fSyntheticGreenhouseGasesLevyAmount = ZArchitecture.Core.Utilities.Round(rate * qtysAndUnits.GetQtyForUnit(unit), 2);
		}

		#endregion

		#region Concessional Duty Rates
		NZCConcessionDutyRate GetConcessionDutyRate(NZCConcession concession, ZString countryGroupForPreferentialTreatment)
		{
			NZCConcessionDutyRate result = null;

			concession.LoadDutyRates(countryGroupForPreferentialTreatment);
			foreach (NZCConcessionDutyRate dutyRate in concession.DutyRates)
			{
				if (countryGroupForPreferentialTreatment.IsEmpty)
				{
					if (dutyRate.U5_PreferentialCountryGroup == NormalDutyRateGroupCode)
					{
						result = dutyRate;
						break;
					}
				}
				else
				{
					if (dutyRate.U5_PreferentialCountryGroup == countryGroupForPreferentialTreatment)
					{
						result = dutyRate;
						break;
					}
					else if (dutyRate.U5_PreferentialCountryGroup == NormalDutyRateGroupCode)
					{
						result = dutyRate;
					}
				}
			}
			return result;
		}

		void CalculateConcessionDuty(NZCConcessionDutyRate dutyRate, ZDecimal valueForDuty, QtyAndUnitManager qtysAndUnits, ZBool isZeroRatedDuty, ZBool isZeroRatedExcise)
		{
			if (dutyRate != null)
			{
				if (!dutyRate.U5_DutyRatePercent.IsEmpty && !isZeroRatedDuty)
				{
					fDutyAmount += ZArchitecture.Core.Utilities.Round((valueForDuty * dutyRate.U5_DutyRatePercent / 100), 2);
					AppendDutyRateString(dutyRate.U5_DutyRatePercent.ToString(2) + "%");
					fDutyRatePercent = dutyRate.U5_DutyRatePercent;
				}
				if (!dutyRate.U5_DutyRatePerUnit.IsEmpty && !isZeroRatedExcise)
				{
					fDutyAmount += ZArchitecture.Core.Utilities.Round((qtysAndUnits.StatQty * dutyRate.U5_DutyRatePerUnit), 2);
					AppendDutyRateString("$" + dutyRate.U5_DutyRatePerUnit.ToString(4) + "/" + qtysAndUnits.StatUQ);
					fDutyRateFlatRate = dutyRate.U5_DutyRatePerUnit;
					fDutyRateFlatUQ = qtysAndUnits.StatUQ;
				}
			}
		}
		#endregion

		#region Conventional Tariff Duty Rates

		ZDecimal GetDutyAmount(NZCClassificationDutyRate dutyRate, ZDecimal valueForDuty, QtyAndUnitManager qtysAndUnits, Predicate<NZCClassificationDutyRate> shouldCalculateRatedDuty, Predicate<NZCClassificationDutyRate> shouldCalculateRatedExcise, Predicate<NZCClassificationDutyRate> shouldCalculateRatedLevies)
		{
			CalculateTariffDuty(dutyRate, valueForDuty, qtysAndUnits, shouldCalculateRatedDuty, shouldCalculateRatedExcise, shouldCalculateRatedLevies);
			ZDecimal result = fDutyAmount;
			ResetDutyAmountsAndRate();
			return result;
		}

		void CalculateTariffDuty(NZCClassificationDutyRate dutyRate, ZDecimal valueForDuty, QtyAndUnitManager qtysAndUnits, Predicate<NZCClassificationDutyRate> shouldCalculateRatedDuty, Predicate<NZCClassificationDutyRate> shouldCalculateRatedExcise, Predicate<NZCClassificationDutyRate> shouldCalculateRatedLevies)
		{
			if (dutyRate != null)
			{
				if (shouldCalculateRatedDuty(dutyRate))
				{
					fDutyAmount += ZArchitecture.Core.Utilities.Round((valueForDuty * dutyRate.U1_DutyRatePercent / 100), 2);
					AppendDutyRateString(string.Format(CultureInfo.InvariantCulture, "{0:0.00####}%", dutyRate.U1_DutyRatePercent));
					fDutyRatePercent = dutyRate.U1_DutyRatePercent;
				}
				if (shouldCalculateRatedExcise(dutyRate))
				{
					fDutyAmount += ZArchitecture.Core.Utilities.Round((qtysAndUnits.StatQty * dutyRate.U1_DutyRatePerUnit1), 2);
					AppendDutyRateString(string.Format(CultureInfo.InvariantCulture, "${0:0.0000##}/{1}", dutyRate.U1_DutyRatePerUnit1, qtysAndUnits.StatUQ));
					fDutyRateFlatRate = dutyRate.U1_DutyRatePerUnit1;
					fDutyRateFlatUQ = qtysAndUnits.StatUQ;
				}
				if (shouldCalculateRatedLevies(dutyRate))
				{
					fDutyAmount += ZArchitecture.Core.Utilities.Round((qtysAndUnits.SuppQty * dutyRate.U1_DutyRatePerUnit2), 2);
					AppendDutyRateString(string.Format(CultureInfo.InvariantCulture, "${0:0.0000##}/{1}", dutyRate.U1_DutyRatePerUnit2, qtysAndUnits.SuppUQ));
				}
			}
		}

		NZCClassificationDutyRate GetTariffDutyRate(NZCClassification classification, ZString countryGroupForPreferentialTreatment, ZDateTime dateForDutyRate)
		{
			NZCClassificationDutyRate result = null;
			NZCClassificationDutyRateCollection dutyRates = GetNZCClassificationDutyRates(classification, countryGroupForPreferentialTreatment);
			foreach (NZCClassificationDutyRate dutyRate in dutyRates)
			{
				if ((dutyRate.U1_DateActiveTo >= dateForDutyRate || dutyRate.U1_DateActiveTo.IsEmpty)
					&& (dutyRate.U1_DateActiveFrom <= dateForDutyRate || dutyRate.U1_DateActiveFrom.IsEmpty))
				{
					if (result == null || (dutyRate.U1_DateActiveTo < result.U1_DateActiveTo || result.U1_DateActiveTo.IsEmpty))
					{
						result = dutyRate;
					}
				}
			}
			return result;
		}

		NZCClassificationDutyRateCollection GetNZCClassificationDutyRates(NZCClassification classification, ZString countryGroup)
		{
			ZQuery additionalFilter = new ZQuery(NZCClassificationDutyRateSchema.U1_PreferentialCountryGroup, countryGroup);
			NZCClassificationDutyRateCollection result = new NZCClassificationDutyRateCollection(classification, factory);
			result.Load(additionalFilter);
			return result;
		}

		#endregion

		#region Internal Resulting Property Management

		void ResetAllExposedValues()
		{
			ResetDutyAmountsAndRate();
			fActualClassification = "";
			fActualPreferentialCountryGroup = ZString.Empty;
		}

		void ResetDutyAmountsAndRate()
		{
			fACCFuelLevyAmount = 0.00m;
			fPFMLFuelLevyAmount = 0.00m;
			fSyntheticGreenhouseGasesLevyAmount = 0.00m;
			fALACLevyAmount = 0.00m;
			fHERALevyAmount = 0.00m;
			fDutyAmount = 0.00m;
			fDutyRateOnly = "";
			fDutyRatePercent = 0.00m;
			fDutyRateFlatRate = 0.00m;
			fDutyRateFlatUQ = "";
			fDutyRateSource = "";
		}
		ZString fActualClassification;
		ZString fDutyRateOnly;

		ZDecimal fDutyRatePercent;
		ZDecimal fDutyRateFlatRate;
		ZString fDutyRateFlatUQ;

		ZString fDutyRateSource;
		ZDecimal fDutyAmount;
		ZDecimal fALACLevyAmount;
		ZDecimal fACCFuelLevyAmount;
		ZDecimal fPFMLFuelLevyAmount;
		ZDecimal fSyntheticGreenhouseGasesLevyAmount;
		ZDecimal fHERALevyAmount;

		#endregion

		#region Universal Tariff

		void GetDutyForUniversalTariff(
			ZString classificationCode,
			ZString concessionCode,
			ZBool qualifiesForPreferentialDuty,
			ZString countryOfOrigin,
			ZString preferentialCountryGroup,
			ZDecimal statQty,
			ZString statUQ,
			ZDecimal suppQty,
			ZString suppUQ,
			ZDecimal valueForDuty,
			ZDateTime dateForDutyRate,
			ZString partsOfClassification,
			ZBool isZeroRatedDuty,
			ZBool isZeroRatedExcise,
			ZBool isZeroRatedLevies)
		{
			if (!cacheMgr.IsAlreadyLoaded(classificationCode, concessionCode, qualifiesForPreferentialDuty, countryOfOrigin, preferentialCountryGroup, statQty, statUQ, suppQty, suppUQ, valueForDuty, dateForDutyRate, partsOfClassification, isZeroRatedDuty, isZeroRatedExcise, isZeroRatedLevies))
			{
				ResetAllExposedValues();
				fActualClassification = classificationCode;

				var baseClassification = UniversalTariffHelper.GetTariff(factory, classificationCode, dateForDutyRate) as TariffView;
				if (baseClassification != null)
				{
					var countryForPreferentialTreatment = qualifiesForPreferentialDuty ? countryOfOrigin : ZString.Empty;
					var groupSelectedForPreferentialTreatment = qualifiesForPreferentialDuty ? preferentialCountryGroup : ZString.Empty;

					TariffView classificationForDutyRate = null;
					if (!partsOfClassification.IsEmpty)
					{
						classificationForDutyRate = UniversalTariffHelper.GetTariff(factory, partsOfClassification, dateForDutyRate) as TariffView;
					}
					if (classificationForDutyRate == null)
					{
						classificationForDutyRate = baseClassification;
					}

					ResetDutyAmountsAndRate();
					var formulaVisitor = new RateFormulaCalculativeVisitor(new UniversalRateCalcDataWrapper(new RateCalcData(dutyRateParameters), new FormulaErrorListener()));
					if (!qualifiesForPreferentialDuty || concessionCode.IsEmpty)
					{
						CalculateTariffDuty(classificationForDutyRate, formulaVisitor, countryForPreferentialTreatment, groupSelectedForPreferentialTreatment, dateForDutyRate, isZeroRatedDuty, isZeroRatedExcise);
					}
					else
					{
						var concessionRate = GetConcessionRate(classificationForDutyRate, concessionCode, dateForDutyRate, groupSelectedForPreferentialTreatment);
						if (concessionRate == null || concessionRate.ZZ2_RateFormula == "0")
						{
							CalculateTariffDuty(classificationForDutyRate, formulaVisitor, countryForPreferentialTreatment, groupSelectedForPreferentialTreatment, dateForDutyRate, isZeroRatedDuty: true, isZeroRatedExcise, shouldCheckDutyRateForExcise: true);
						}
						else
						{
							CalculateDuty(concessionRate, formulaVisitor, isZeroRatedDuty, isZeroRatedExcise);
						}
					}

					CalculateLevies(classificationForDutyRate, formulaVisitor, isZeroRatedLevies, dateForDutyRate);

					var countryGroupOrCountryCodeUsedForDutyRateFound = fActualPreferentialCountryGroup.IsEmpty ? UniversalTariffHelper.StandardDutyRateGroupCode : fActualPreferentialCountryGroup.ToString();
					fDutyRateSource = fActualClassification + (concessionCode.IsEmpty ? "" : "-" + concessionCode) + " @ " + countryGroupOrCountryCodeUsedForDutyRateFound;
					if (fDutyRateOnly.IsEmpty)
					{
						AppendDutyRateString(FreeDutyRateDescription);
					}
				}
			}
		}

		RateView GetConcessionRate(TariffView classificationForDutyRate, ZString concessionCode, ZDateTime dateForDutyRate, ZString groupSelectedForPreferentialTreatment)
		{
			RateView concessionRate = null;
			var gotMatchedConcessionRate = false;
			foreach (var rate in classificationForDutyRate.Rates)
			{
				if (gotMatchedConcessionRate)
				{
					break;
				}

				if (rate.PreferenceCode == UniversalReferenceConstants.TariffCodes.Qualifies)
				{
					foreach (var applicability in rate.RateApplicabilities)
					{
						if (applicability.ZZT_OrderNumber == concessionCode && applicability.ZZT_StartDate <= dateForDutyRate && applicability.ZZT_EndDate >= dateForDutyRate)
						{
							if (applicability.TradeGroupCode == groupSelectedForPreferentialTreatment)
							{
								concessionRate = rate;
								fActualPreferentialCountryGroup = groupSelectedForPreferentialTreatment;
								gotMatchedConcessionRate = true;
								break;
							}
							else if (applicability.TradeGroupCode == UniversalTariffHelper.StandardDutyRateGroupCode)
							{
								concessionRate = rate;
							}
						}
					}
				}
			}
			return concessionRate;
		}

		void CalculateTariffDuty(TariffView tariff, RateFormulaCalculativeVisitor formulaVisitor, ZString countryForPrefRates, ZString groupSelectedForPrefRates, ZDateTime dateForDutyRate, bool isZeroRatedDuty, bool isZeroRatedExcise, bool shouldCheckDutyRateForExcise = false)
		{
			RateView dutyRate = null;
			var gotARateResult = false;
			if (!countryForPrefRates.IsEmpty)
			{
				var possibleGroups = UniversalTariffHelper.GetTradeGroupsFromCountry(countryForPrefRates, dateForDutyRate, factory);
				CusRefTradeGroupView onlyGroupPossible = null;
				if (!groupSelectedForPrefRates.IsEmpty)
				{
					onlyGroupPossible = possibleGroups.FirstOrDefault(x => x.ZZA_TradeGroup == groupSelectedForPrefRates);
				}
				if (onlyGroupPossible == null && possibleGroups.Length == 1)
				{
					onlyGroupPossible = possibleGroups[0];
				}

				if (onlyGroupPossible != null)
				{
					dutyRate = tariff.Rates.FirstOrDefault(x => x.PreferenceCode == UniversalReferenceConstants.TariffCodes.Qualifies && x.RateApplicabilities.Any(y => UniversalTariffHelper.ApplicabilityPredict(y, ZString.Empty, onlyGroupPossible.ZZA_TradeGroup, dateForDutyRate)));
					if (dutyRate != null)
					{
						gotARateResult = true;
						fActualPreferentialCountryGroup = onlyGroupPossible.ZZA_TradeGroup;
					}
				}
				else if (NZCustomsDataRegistry.Instance.PreferentialCountryGroupCodeDefaulting.Value)
				{
					gotARateResult = GetBestDutyRate(ref dutyRate, tariff, formulaVisitor, dateForDutyRate, isZeroRatedDuty, isZeroRatedExcise, shouldCheckDutyRateForExcise, possibleGroups);
				}
			}

			if (!gotARateResult)
			{
				dutyRate = tariff.Rates.FirstOrDefault(x => x.PreferenceCode == UniversalReferenceConstants.TariffCodes.NotQualifies && x.RateApplicabilities.Any(y => UniversalTariffHelper.ApplicabilityPredict(y, ZString.Empty, UniversalTariffHelper.StandardDutyRateGroupCode, dateForDutyRate)));
				fActualPreferentialCountryGroup = UniversalTariffHelper.StandardDutyRateGroupCode;
			}

			CalculateDuty(dutyRate, formulaVisitor, isZeroRatedDuty, isZeroRatedExcise, shouldCheckDutyRateForExcise);
		}

		bool GetBestDutyRate(ref RateView dutyRateResult, TariffView tariff, RateFormulaCalculativeVisitor formulaVisitor, ZDateTime dateForDutyRate, bool isZeroRatedDuty, bool isZeroRatedExcise, bool shouldCheckDutyRateForExcise, IEnumerable<CusRefTradeGroupView> possibleGroups)
		{
			var gotARateResult = false;
			var bestDutyValue = decimal.MaxValue;
			var bestPreferentialCountryGroup = ZString.Empty;
			RateView bestDutyRate = null;
			foreach (var group in possibleGroups)
			{
				var groupDutyRate = tariff.Rates.FirstOrDefault(x => x.PreferenceCode == UniversalReferenceConstants.TariffCodes.Qualifies && x.RateApplicabilities.Any(y => UniversalTariffHelper.ApplicabilityPredict(y, ZString.Empty, group.ZZA_TradeGroup, dateForDutyRate)));
				if (groupDutyRate != null)
				{
					CalculateDuty(groupDutyRate, formulaVisitor, isZeroRatedDuty, isZeroRatedExcise, shouldCheckDutyRateForExcise);
					var dutyAmount = fDutyAmount;
					ResetDutyAmountsAndRate();

					if (dutyAmount == 0)
					{
						dutyRateResult = groupDutyRate;
						gotARateResult = true;
						fActualPreferentialCountryGroup = group.ZZA_TradeGroup;
						break;
					}
					else if (dutyAmount < bestDutyValue)
					{
						bestDutyValue = dutyAmount;
						bestDutyRate = groupDutyRate;
						bestPreferentialCountryGroup = group.ZZA_TradeGroup;
					}
				}
			}

			if (!gotARateResult && bestDutyRate != null)
			{
				dutyRateResult = bestDutyRate;
				fActualPreferentialCountryGroup = bestPreferentialCountryGroup;
				gotARateResult = true;
			}

			return gotARateResult;
		}

		void CalculateDuty(RateView universalRate, RateFormulaCalculativeVisitor formulaVisitor, bool isZeroRatedDuty, bool isZeroRatedExcise, bool shouldCheckDutyRateForExcise = false)
		{
			if (universalRate != null)
			{
				var dutyAmount = Utilities.Round(RateFormulaCalculator.CalculateDuty(formulaVisitor, universalRate.ZZ2_RateFormula, isZeroRatedDuty, isZeroRatedExcise, shouldCheckDutyRateForExcise), 2);
				var dutyRate = formulaVisitor.VFDRate;
				var exciseRate = formulaVisitor.UOMRate;
				var exciseUnit = formulaVisitor.UOMCode;

				if (!dutyRate.IsEmpty && !isZeroRatedDuty)
				{
					fDutyRatePercent = dutyRate;
					AppendDutyRateString($"{dutyRate.ToString(2)}%");
				}
				if (!exciseRate.IsEmpty && !isZeroRatedExcise)
				{
					if (shouldCheckDutyRateForExcise && dutyRate.IsEmpty)
					{
						dutyAmount = 0m;
					}
					else
					{
						fDutyRateFlatRate = exciseRate;
						fDutyRateFlatUQ = exciseUnit;
						AppendDutyRateString($"${exciseRate.ToString(4)}/{exciseUnit}");
					}
				}

				fDutyAmount += dutyAmount;
			}
		}

		void CalculateLevies(TariffView tariff, RateFormulaCalculativeVisitor formulaVisitor, bool isZeroRatedLevies, ZDateTime dateForDutyRate)
		{
			if (!isZeroRatedLevies)
			{
				foreach (var rate in tariff.Rates)
				{
					if (rate.ZZ2_ZZR_RateTypeCode == Constants.RateTypes.Levy && rate.ZZ2_StartDate <= dateForDutyRate && rate.ZZ2_EndDate >= dateForDutyRate)
					{
						var levyAmount = Utilities.Round(RateFormulaCalculator.CalculateDuty(formulaVisitor, rate.ZZ2_RateFormula, omitVFDCalculation: false, omitUOMCalculation: false), 2);
						var levyRate = formulaVisitor.UOMRate;
						var levyUnit = formulaVisitor.UOMCode;
						switch (rate.RateCode)
						{
							case "AL":
								fALACLevyAmount = levyAmount;
								AppendDutyRateString($"ALAC:${levyRate.ToString(4)}/{levyUnit}");
								break;
							case "SL":
								fHERALevyAmount = levyAmount;
								AppendDutyRateString($"HERA:${levyRate.ToString(4)}/{levyUnit}");
								break;
							case "AC":
							case "AU":
								fACCFuelLevyAmount = levyAmount;
								AppendDutyRateString($"ACC:${levyRate.ToString(4)}/{levyUnit}");
								break;
							case "PF":
							case "PS":
								fPFMLFuelLevyAmount = levyAmount;
								AppendDutyRateString($"PFML:${levyRate.ToString(5)}/{levyUnit}");
								break;
							case "GG":
							case "GS":
								fSyntheticGreenhouseGasesLevyAmount = levyAmount;
								AppendDutyRateString($"SGG:${levyRate.ToString(5)}/{levyUnit}");
								break;
						}
					}
				}
			}
		}

		#endregion

		#endregion
	}
}
