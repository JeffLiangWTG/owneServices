using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Schema;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.DataTransfer
{
	static class ForwardingConsolMatchService
	{
		public static List<(bool? isCoLoad, ConsolFetchRule consolFetchRule)> GetConsolFetchRule(ConsolFetchParam fetchParam)
		{
			var ruleGroups = new List<(bool?, ConsolFetchRule)>();

			if (IsGoodMatchCondition(fetchParam.CoLoadMasterBillNumber) || IsGoodMatchCondition(fetchParam.CoLoadBookingConfirmationReference))
			{
				int priority1 = -1;
				var rules1 = new ConsolFetchRule();
				if (IsGoodMatchCondition(fetchParam.CoLoadMasterBillNumber))
				{
					rules1.MatchRules.Add(new ConsolFetchRule.MatchRule(JobConsolSchema.JK_CoLoadMasterBill, fetchParam.CoLoadMasterBillNumber));

					AddMasterBillOrBookingReference(rules1, JobConsolSchema.JK_CoLoadMasterBill, fetchParam.CoLoadMasterBillNumber, ++priority1);
				}

				AddC1COrSCAC(rules1, true, fetchParam, ++priority1);

				if (IsGoodMatchCondition(fetchParam.CoLoadBookingConfirmationReference))
				{
					rules1.MatchRules.Add(new ConsolFetchRule.MatchRule(JobConsolSchema.JK_CoLoadBookingReference, fetchParam.CoLoadBookingConfirmationReference));

					AddMasterBillOrBookingReference(rules1, JobConsolSchema.JK_CoLoadBookingReference, fetchParam.CoLoadBookingConfirmationReference, ++priority1);
				}

				ruleGroups.Add((true, rules1));
			}

			if (IsGoodMatchCondition(fetchParam.MasterBillNumber) || IsGoodMatchCondition(fetchParam.BookingConfirmationReference))
			{
				int priority2 = -1;
				var rules2 = new ConsolFetchRule(true);

				if (IsGoodMatchCondition(fetchParam.MasterBillNumber))
				{
					rules2.MatchRules.Add(new ConsolFetchRule.MatchRule(JobConsolSchema.JK_CoLoadMasterBill, fetchParam.MasterBillNumber));

					AddMasterBillOrBookingReference(rules2, JobConsolSchema.JK_CoLoadMasterBill, fetchParam.MasterBillNumber, ++priority2);
				}

				AddC1COrSCAC(rules2, true, fetchParam, ++priority2);

				if (IsGoodMatchCondition(fetchParam.BookingConfirmationReference))
				{
					rules2.MatchRules.Add(new ConsolFetchRule.MatchRule(JobConsolSchema.JK_CoLoadBookingReference, fetchParam.BookingConfirmationReference));

					AddMasterBillOrBookingReference(rules2, JobConsolSchema.JK_CoLoadBookingReference, fetchParam.BookingConfirmationReference, ++priority2);
				}

				ruleGroups.Add((true, rules2));

				int priority3 = -1;
				var rules3 = new ConsolFetchRule();

				if (IsGoodMatchCondition(fetchParam.MasterBillNumber))
				{
					rules3.MatchRules.Add(new ConsolFetchRule.MatchRule(JobConsolSchema.JK_MasterBillNum, fetchParam.MasterBillNumber));

					AddMasterBillOrBookingReference(rules3, JobConsolSchema.JK_MasterBillNum, fetchParam.MasterBillNumber, ++priority3);
				}

				AddC1COrSCAC(rules3, false, fetchParam, ++priority3);

				if (IsGoodMatchCondition(fetchParam.BookingConfirmationReference))
				{
					rules3.MatchRules.Add(new ConsolFetchRule.MatchRule(JobConsolSchema.JK_BookingReference, fetchParam.BookingConfirmationReference));

					AddMasterBillOrBookingReference(rules3, JobConsolSchema.JK_BookingReference, fetchParam.BookingConfirmationReference, ++priority3);
				}

				ruleGroups.Add((null, rules3));
			}

			return ruleGroups;
		}

		static void AddMasterBillOrBookingReference(ConsolFetchRule rule, SchemaColumn matchKeyName, string matchKeyValue, int priority)
		{
			rule.ScoreRules.Add(new ConsolFetchRule.ScoreRule(delegate (ForwardingConsol consol)
			{
				var actualValue = consol[matchKeyName];
				if (actualValue != null && actualValue.ToString() == matchKeyValue)
				{
					return true;
				}

				return false;
			}, priority));
		}

		static void AddC1COrSCAC(ConsolFetchRule rule, bool isCoLoad, ConsolFetchParam fetchParam, int priority)
		{
			if (isCoLoad)
			{
				if (!fetchParam.CoLoadSCAC.IsNullOrEmpty() || !fetchParam.CoLoadC1C.IsNullOrEmpty())
				{
					rule.ScoreRules.Add(new ConsolFetchRule.ScoreRule(delegate (ForwardingConsol consol)
					{
						return HasC1COrSCAC(consol.Creditor, fetchParam.CoLoadSCAC, fetchParam.CoLoadC1C);
					}, priority));
				}
			}
			else
			{
				if (!fetchParam.SCAC.IsNullOrEmpty() || !fetchParam.C1C.IsNullOrEmpty())
				{
					rule.ScoreRules.Add(new ConsolFetchRule.ScoreRule(delegate (ForwardingConsol consol)
					{
						return HasC1COrSCAC(consol.ShippingLine, fetchParam.SCAC, fetchParam.C1C);
					}, priority));
				}
			}
		}

		static bool HasC1COrSCAC(OrgHeader org, string scac, string c1c)
		{
			return org != null &&
				(
					(!scac.IsNullOrEmpty() && org.CustomsCodes.Cast<OrgCusCode>()
					.Any(c => c.OK_CodeType == OrgCusCode.CodeTypes.CarrierCode
						&& c.OK_RN_NKCodeCountry == Core.Constants.CountryCodes.UnitedStates
						&& c.OK_CustomsRegNo.ToUpper() == scac.ToUpper())
					)
					||
					(!c1c.IsNullOrEmpty() &&
					org.CustomsCodes.Cast<OrgCusCode>()
					.Any(c => c.OK_CodeType == OrgCusCode.CodeTypes.CargoWiseOneCarrierCode
						&& c.OK_CustomsRegNo.ToUpper() == c1c.ToUpper())
					)
				);
		}

		static bool IsGoodMatchCondition(string conditionValue)
		{
			return !(conditionValue.IsNullOrEmpty());
		}
	}
}
