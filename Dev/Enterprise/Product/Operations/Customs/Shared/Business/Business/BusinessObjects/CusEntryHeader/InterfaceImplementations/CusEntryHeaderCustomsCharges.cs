using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Customs;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business
{
	public interface ICustomsChargeEntry
	{
		JobHeader Job { get; }

		ZString UniqueNumber { get; }

		ZString PreviousUniqueNumber { get; }

		EntryChargeTypeList EntryChargeTypeList { get; }

		ZDecimal GetTotalChargeValueFor(EntryChargeType chargeType, ZString methodOfPaymentCode);

		/// <summary>
		/// You'll be asked this question for each registry-defined fee code, for each payment code given by GetMethodsOfPaymentThatCanInfluenceAutoRating (which may be just one blank string).
		/// Allows for fee AAA to paid by the broker when the method of payment is X, but not when it's Y.
		/// For real-world example, see GB.
		/// </summary>
		/// <returns></returns>
		bool IsFeePaidByBroker(string chargeType, ZString methodOfPaymentCode, ILogger logger);

		BusinessObjectFactory Factory { get; }

		ZString ReferenceNumber { get; }

		CustomsCharge[] GetNonFeeCountrySpecificCharges();

		bool HasBeenWithdrawn { get; }

		ZGuid CreditorPK { get; }

		bool EntryReferenceInChargeDescSupported { get; }

		ZString LocalCurrencyCode { get; }

		/// <summary>
		/// If your implementation of autorating needs to allow different answers to the question IsFeePaidByBroker based on different method of payment codes, override this to allow you to be asked this question for each fee, for each payment type.
		/// Otherwise, return a single empty string if the methods of payment won't vary by fee, or won't influence your answer.
		/// Don't return an empty array.
		/// </summary>
		/// <returns></returns>
		ZString[] GetMethodsOfPaymentThatCanInfluenceAutoRating();
	}

	public class OrganizationReferenceKey
	{
		public OrganizationReferenceKey(OrgHeader organization, ZString reference)
		{
			Organization = organization;
			Reference = reference;
			Reference2 = ZString.Empty;
		}

		public OrganizationReferenceKey(OrgHeader organization, ZString reference, ZString reference2)
		{
			Organization = organization;
			Reference = reference;
			Reference2 = reference2;
		}

		public OrgHeader Organization { get; private set; }
		public ZString Reference { get; private set; }
		public ZString Reference2 { get; private set; }

		public override bool Equals(object obj)
		{
			var rhs = (OrganizationReferenceKey)obj;
			bool result = true;
			result &= rhs != null;
			if (Organization != null)
			{
				result &= Organization.PK == rhs.Organization.PK;
			}

			result &= Reference == rhs.Reference;
			result &= Reference2 == rhs.Reference2;
			return result;
		}

		public override int GetHashCode()
		{
			var result = Reference.GetHashCode();
			if (Organization != null)
			{
				result ^= Organization.GetHashCode();
			}

			result ^= Reference2.GetHashCode();
			return result;
		}

		public override string ToString()
		{
			string result = Reference.ToString();
			if (Organization != null)
			{
				result += Organization.PK.ToStringKey();
			}

			result += Reference2.ToString();
			return result;
		}
	}
}

namespace Enterprise.Customs.Business.InterfaceImplementations
{
	public class CusEntryHeaderCustomsCharges : ICustomsCharges
	{
		public CusEntryHeaderCustomsCharges(ICustomsChargeEntry entryHeader)
		{
			this.entryHeader = entryHeader;
		}
		protected readonly ICustomsChargeEntry entryHeader;

		CustomsCharge[] ICustomsCharges.GetCustomsCharges(ILogger logger)
		{
			return GetCustomsCharges(logger);
		}

		protected virtual CustomsCharge[] GetCustomsCharges(ILogger logger)
		{
			var customsCharges = new Dictionary<string, CustomsCharge>();

			var methodsOfPaymentThatCanInfluenceAutoRating = entryHeader.GetMethodsOfPaymentThatCanInfluenceAutoRating();
			foreach (EntryChargeType chargeType in entryHeader.EntryChargeTypeList)
			{
				if (chargeType.IsPaidWhenMessageClears)
				{
					foreach (var methodOfPayment in methodsOfPaymentThatCanInfluenceAutoRating)
					{
						var deduplicated = false;
						if (customsCharges.Count > 0)
						{
							if (customsCharges.Keys.Any(x => x.StartsWith(chargeType + "|", StringComparison.OrdinalIgnoreCase) && x.EndsWith("|" + methodOfPayment, StringComparison.OrdinalIgnoreCase)))
							{
								deduplicated = true;
							}
						}

						if (!deduplicated)
						{
							var chargeValuesByDebtor = GetTotalChargeValueByDebtorFor(chargeType, methodOfPayment, logger);
							foreach (var organisationReferenceKey in chargeValuesByDebtor.Keys)
							{
								var debtorPK = organisationReferenceKey.Organization?.PK ?? ZGuid.Empty;
								var chargeValue = chargeValuesByDebtor[organisationReferenceKey];

								if (!chargeValue.IsEmpty || ExistsJobCharges)
								{
									var stringKey = chargeType.ChargeCodeForRating + "|" + organisationReferenceKey + "|" + methodOfPayment;
									if (!customsCharges.TryGetValue(stringKey, out var customsCharge))
									{
										var isFeePaidByBroker = IsFeePaidByBroker(chargeType.ChargeCodeForRating, debtorPK, methodOfPayment, logger);
										customsCharge = new CustomsCharge(GetAccChargeCodeFor(chargeType), chargeType.Description, 0m, 0m
											, isFeePaidByBroker
											, GetDefaultCreditorPK(chargeType)
											, entryHeader.LocalCurrencyCode)
										{
											EntryReference = GetEntryReference(organisationReferenceKey),
											DebtorPK = debtorPK
										};

										if (!chargeValue.IsEmpty || isFeePaidByBroker)
										{
											customsCharges.Add(stringKey, customsCharge);
										}
									}

									if (!chargeValue.IsEmpty)
									{
										customsCharge.AddAmount(chargeType.IsRolledUpIntoAnotherChargeCodeAsGSTAtTimeOfInvoicing, chargeValue);
									}
								}
							}
						}
					}
				}
			}

			var countrySpecificCharges = entryHeader.GetNonFeeCountrySpecificCharges();
			var all = countrySpecificCharges.Concat(customsCharges.Values).ToArray();
			var result = KeepSuitableCustomsCharges(all).ToArray();
			return result;
		}

		ZString GetEntryReference(OrganizationReferenceKey organisationReferenceKey)
		{
			return organisationReferenceKey.Reference.IsEmpty ? GetEntryReferenceInChargeDesc(entryHeader) : organisationReferenceKey.Reference;
		}

		protected virtual ZGuid GetDefaultCreditorPK(EntryChargeType chargeType)
		{
			return entryHeader.CreditorPK;
		}

		protected virtual Dictionary<OrganizationReferenceKey, ZDecimal> GetTotalChargeValueByDebtorFor(EntryChargeType chargeType, ZString methodOfPaymentCode, ILogger logger)
		{
			var result = new Dictionary<OrganizationReferenceKey, ZDecimal>();
			var amount = entryHeader.GetTotalChargeValueFor(chargeType, methodOfPaymentCode);
			if (amount > 0)
			{
				logger?.Log(LogType.Information, $"Charge type = {chargeType}, MoP = {methodOfPaymentCode}, total due for {entryHeader.ReferenceNumber} is {amount}");
			}
			result.Add(new OrganizationReferenceKey(null, ZString.Empty), amount);
			return result;
		}

		protected virtual bool IsFeePaidByBroker(string feeCode, ZGuid importerPK, ZString methodOfPaymentCode, ILogger logger)
		{
			return entryHeader.IsFeePaidByBroker(feeCode, methodOfPaymentCode, logger);
		}

		protected AccChargeCode GetAccChargeCodeFor(EntryChargeType chargeType)
		{
			AccChargeCode accChargeCode = null;
			var accChargeCodePK = GetAccChargeCode(chargeType);
			if (!accChargeCodePK.IsEmpty)
			{
				accChargeCode = entryHeader.Factory.Load<AccChargeCode>(accChargeCodePK);
			}

			return accChargeCode;
		}

		protected virtual ZGuid GetAccChargeCode(EntryChargeType chargeType)
		{
			var chargeTypeSetting = chargeType.GetChargeTypeSpecificRegistrySettings();
			return chargeTypeSetting != null ? chargeTypeSetting.AC_ChargeCode : ZGuid.Empty;
		}

		ZBool ICustomsCharges.IsActive => true;

		IEnumerable<CustomsCharge> KeepSuitableCustomsCharges(CustomsCharge[] customsCharges)
		{
			if (ExistsJobCharges)
			{
				var cusDsbCustomsCharges = customsCharges.Where(x => x.IsPaidByBroker).ToArray();
				foreach (var group in cusDsbCustomsCharges.GroupBy(x => new CustomsChargeKey(GetCusDsbChargeCodePK(x), x.EntryReference, x.DebtorPK)))
				{
					var groupKey = group.Key;
					var isEmptyCusDsbChargeCode = new ZDecimal(group.Sum(x => x.Amount)).IsEmpty;
					if (isEmptyCusDsbChargeCode && ExistsNonZeroAmountCusDsbJobCharges(groupKey.ChargeCodePK))
					{
						var first = group.First();
						yield return new CustomsCharge(groupKey.ChargeCodePK, ZString.Empty, 0m, 0m, first.IsPaidByBroker, first.CreditorPK, first.OverrideCurrency, first.EntryReference, first.DebtorPK);
					}
					else
					{
						foreach (var customsCharge in group.Where(x => !IsEmptyCustomsCharge(x)))
						{
							yield return customsCharge;
						}
					}
				}

				var remainingCustomsCharges = customsCharges.Except(cusDsbCustomsCharges).ToArray();
				foreach (var customsCharge in remainingCustomsCharges.Where(x => !IsEmptyCustomsCharge(x)))
				{
					yield return customsCharge;
				}
			}
			else
			{
				foreach (var customsCharge in customsCharges.Where(x => !IsEmptyCustomsCharge(x)))
				{
					yield return customsCharge;
				}
			}
		}

		ZBool ExistsJobCharges => AllJobCharges.Length > 0;

		JobCharge[] AllJobCharges
		{
			get => allJobCharges ?? (allJobCharges = GetAllJobCharges(entryHeader.Job).ToArray());
		}
		JobCharge[] allJobCharges;

		static ZString GetEntryReferenceInChargeDesc(ICustomsChargeEntry entryHeader) => entryHeader.EntryReferenceInChargeDescSupported
			? entryHeader.ReferenceNumber
			: ZString.Empty;

		bool ExistsNonZeroAmountCusDsbJobCharges(ZGuid chargeCodePK)
		{
			var existingJobCharges = GetExistingJobCharges(AllJobCharges, chargeCodePK, entryHeader.UniqueNumber, entryHeader.PreviousUniqueNumber, GetEntryReferenceInChargeDesc(entryHeader));
			var sumOfExistingJobCharges = (ZDecimal)existingJobCharges.Sum(x => x.JR_LocalCostAmt);
			return !sumOfExistingJobCharges.IsEmpty;
		}

		bool IsEmptyCustomsCharge(CustomsCharge customsCharge) => customsCharge.Amount.IsEmpty && customsCharge.GST.IsEmpty;

		IEnumerable<JobCharge> GetAllJobCharges(JobHeader job)
		{
			var result = Enumerable.Empty<JobCharge>();
			if (job != null)
			{
				var query = new ZQuery(JobChargeSchema.JR_JH, job.PK);
				var allCharges = job.Factory.Load<JobCharge>(query);
				result = allCharges;
			}

			return result;
		}

		IEnumerable<JobCharge> GetExistingJobCharges(IEnumerable<JobCharge> allCharges, ZGuid chargeCodePK, ZString uniqueNumber, ZString previousUniqueNumber, ZString entryReferenceInChargeDesc)
			=> allCharges.Where(x => x.JR_AC == chargeCodePK && !x.JR_E6.IsValid && MatchingEntryReferenceInChargeDesc(x, entryReferenceInChargeDesc) && MatchingUniqueNumber(x, uniqueNumber, previousUniqueNumber));

		ZBool MatchingEntryReferenceInChargeDesc(JobCharge jobCharge, ZString entryReferenceInChargeDesc)
		{
			return MatchingEntryReferenceInChargeDesc(jobCharge.JR_Desc, entryReferenceInChargeDesc);
		}

		static ZBool MatchingEntryReferenceInChargeDesc(ZString description, ZString entryReferenceInChargeDesc)
		{
			var result = entryReferenceInChargeDesc.IsEmpty;
			if (!result)
			{
				var chargeDesc = description.Split("\r\n");
				if (chargeDesc.Length > 0)
				{
					result = chargeDesc[0].Contains(entryReferenceInChargeDesc);
				}
			}

			return result;
		}

		ZBool MatchingUniqueNumber(JobCharge jobCharge, ZString uniqueNumber, ZString previousUniqueNumber)
		{
			var invoiceNum = jobCharge.JR_APInvoiceNum;
			return MatchingUniqueNumber(invoiceNum, uniqueNumber, previousUniqueNumber);
		}

		static ZBool MatchingUniqueNumber(ZString invoiceNum, ZString uniqueNumber, ZString previousUniqueNumber)
		{
			return invoiceNum.IsEmpty
					|| invoiceNum.StartsWith(uniqueNumber, StringComparison.OrdinalIgnoreCase)
					|| (!previousUniqueNumber.IsEmpty && invoiceNum.StartsWith(previousUniqueNumber, StringComparison.OrdinalIgnoreCase));
		}

		ZGuid GetCusDsbChargeCodePK(CustomsCharge charge)
		{
			var result = charge.ChargeCodePK;
			if (!charge.ChargeCodePK.IsValid && charge.IsPaidByBroker)
			{
				result = RatingDataRegistry.Instance.CustomsDisbursementChargeCode.Value;
			}
			return result;
		}

		public static bool MatchCustomsCharges(ICustomsChargeEntry entryHeader, ZString invoiceNum, ZString description)
		{
			var uniqueNum = entryHeader.UniqueNumber;
			var previousUniqueNumber = entryHeader.PreviousUniqueNumber;
			var entryReferenceInChargeDesc = GetEntryReferenceInChargeDesc(entryHeader);

			return MatchingEntryReferenceInChargeDesc(description, entryReferenceInChargeDesc) && MatchingUniqueNumber(invoiceNum, uniqueNum, previousUniqueNumber);
		}

		class CustomsChargeKey
		{
			public CustomsChargeKey(ZGuid chargeCodePK, ZString entryRef, ZGuid debtorPk)
			{
				ChargeCodePK = chargeCodePK;
				this.entryRef = entryRef;
				this.debtorPk = debtorPk;
			}

			public ZGuid ChargeCodePK { get; }
			readonly ZString entryRef;
			readonly ZGuid debtorPk;

			public override bool Equals(object obj)
			{
				return obj is CustomsChargeKey other
						&& ChargeCodePK == other.ChargeCodePK
						&& entryRef == other.entryRef
						&& debtorPk == other.debtorPk;
			}

			public override int GetHashCode()
			{
				return ChargeCodePK.GetHashCode()
						^ entryRef.GetHashCode()
						^ debtorPk.GetHashCode();
			}
		}

		public class CustomsChargeCache : ICustomsCharges
		{
			public CustomsChargeCache(ICustomsCharges iCustomsChargesImplementationToCache)
			{
				this.customsCharges = iCustomsChargesImplementationToCache.GetCustomsCharges(null);
				this.IsActive = iCustomsChargesImplementationToCache.IsActive;
			}

			readonly CustomsCharge[] customsCharges;
			public CustomsCharge[] GetCustomsCharges(ILogger logger) => customsCharges;
			public ZBool IsActive { get; }
		}
	}
}
