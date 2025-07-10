using System;
using System.Collections.ObjectModel;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Core.Constants;

namespace Enterprise.MasterFiles.Business
{
	public static class AccountingMasterFilesTaxFrameworkConstants
	{
		public sealed class TaxAuthorityTypeList : CodeDescriptionPairList
		{
			public TaxAuthorityTypeList()
			{
				Add(National);
				Add(Regional);
				Add(State);
				Add(Municipal);
			}

			public static CodeDescriptionPair National { get { return new CodeDescriptionPair("NAT", ResString.GetMultilingualString("4CFF30C5-0F29-4E36-99D7-8A5998C40363", "National")); } }
			public static CodeDescriptionPair Regional { get { return new CodeDescriptionPair("REG", ResString.GetMultilingualString("B1682888-471A-4349-A4F5-A8D5F09865F3", "Regional")); } }
			public static CodeDescriptionPair State { get { return new CodeDescriptionPair("STA", ResString.GetMultilingualString("71294DC4-AE06-4D2D-B057-C20CE46CF810", "State")); } }
			public static CodeDescriptionPair Municipal { get { return new CodeDescriptionPair("MUN", ResString.GetMultilingualString("71A3D476-D4C9-4955-9CD4-CB28B74DB0BC", "Municipal")); } }
		}

		public sealed class TaxConfigurationLedgers : CodeDescriptionPairList
		{
			public TaxConfigurationLedgers()
			{
				Add(AccountsPayable);
				Add(AccountsReceivable);
			}

			public static CodeDescriptionPair AccountsPayable { get { return new CodeDescriptionPair(LedgerTypes.AccountsPayable, ResString.GetMultilingualString("0AB16540-7721-4532-97DB-87368F940387", "Accounts Payable")); } }
			public static CodeDescriptionPair AccountsReceivable { get { return new CodeDescriptionPair(LedgerTypes.AccountsReceivable, ResString.GetMultilingualString("B98C9909-11D5-439F-922C-F558ABA0735F", "Accounts Receivable")); } }
		}

		public sealed class TaxRealisationMethods : CodeDescriptionPairList
		{
			public TaxRealisationMethods()
			{
				Add(PostDate);
				Add(MatchDate);
				Add(PostDateOfMatchTransaction);
			}

			public static CodeDescriptionPair PostDate { get { return new CodeDescriptionPair("PDT", ResString.GetMultilingualString("8A4F27A3-E33C-4FCF-9A5E-47A7D11F0AF5", "Post Date")); } }
			public static CodeDescriptionPair MatchDate { get { return new CodeDescriptionPair("MDT", ResString.GetMultilingualString("82BA66A1-48FC-4643-A3F1-0D0B182841D1", "Match Date")); } }
			public static CodeDescriptionPair PostDateOfMatchTransaction { get { return new CodeDescriptionPair("PTM", ResString.GetMultilingualString("EF6AFB21-9375-4311-8EE3-568DB10FC2FA", "Post Date of Match Transaction")); } }
		}

		public sealed class TaxRecordCreationTrigger : CodeDescriptionPairList
		{
			public TaxRecordCreationTrigger()
			{
				Add(PostDate);
				Add(MatchDate);
			}

			public static CodeDescriptionPair PostDate { get { return new CodeDescriptionPair("PDT", ResString.GetMultilingualString("8A4F27A3-E33C-4FCF-9A5E-47A7D11F0AF5", "Post Date")); } }
			public static CodeDescriptionPair MatchDate { get { return new CodeDescriptionPair("MDT", ResString.GetMultilingualString("82BA66A1-48FC-4643-A3F1-0D0B182841D1", "Match Date")); } }
		}

		public sealed class CancellationPolicyMethods : CodeDescriptionPairList
		{
			public CancellationPolicyMethods()
			{
				Add(NotAllowed);
				Add(CalendarMonth);
				Add(CalendarYear);
				Add(NoRestriction);
			}
			public static CodeDescriptionPair NotAllowed { get { return new CodeDescriptionPair("NAL", ResString.GetMultilingualString("F4D620B1-1C18-437E-8949-F52DF6A11A5D", "Not Allowed")); } }
			public static CodeDescriptionPair CalendarMonth { get { return new CodeDescriptionPair("CMO", ResString.GetMultilingualString("06CD8499-68B2-4AA0-BFC2-76CBB2DE033C", "Calendar Month")); } }
			public static CodeDescriptionPair CalendarYear { get { return new CodeDescriptionPair("CYR", ResString.GetMultilingualString("80122ECD-03DD-4356-A846-5082A1AAC537", "Calendar Year")); } }
			public static CodeDescriptionPair NoRestriction { get { return new CodeDescriptionPair("NRE", ResString.GetMultilingualString("B21616A5-CC12-4F9B-B4A7-EABAED1E3608", "No Restriction")); } }
		}

		public class RateSourceMethods : CodeDescriptionPairList
		{
			public RateSourceMethods() : base()
			{
				Add(ManualOverride);
				Add(Monthly);
				Add(Quarterly);
			}
			public static CodeDescriptionPair ManualOverride { get { return new CodeDescriptionPair("MOV", ResString.GetMultilingualString("44705B54-7076-40C1-BD9E-F5F60B9D9D2A", "Manual Override")); } }
			public static CodeDescriptionPair Monthly { get { return new CodeDescriptionPair("MON", ResString.GetMultilingualString("5F5087CD-FEB0-4832-8455-EEB009C5A101", "Monthly")); } }
			public static CodeDescriptionPair Quarterly { get { return new CodeDescriptionPair("QUA", ResString.GetMultilingualString("24BBDF1A-0327-4797-8EB5-95B0829A4D48", "Quarterly")); } }
		}

		public sealed class TaxSuperTypeList : CodeDescriptionPairList
		{
			public TaxSuperTypeList()
			{
				Add(Perceptions);
				Add(RetentionInInvoice);
				Add(SalesTax);
				Add(StandardPaymentRetention);
				Add(TurnoverTax);
				Add(ValueAddedTax);
			}

			public static CodeDescriptionPair Perceptions { get { return new CodeDescriptionPair("PER", ResString.GetMultilingualString("39B68A8A-659A-48DF-A046-17DFCDC406FE", "Perceptions")); } }
			public static CodeDescriptionPair RetentionInInvoice { get { return new CodeDescriptionPair("RII", ResString.GetMultilingualString("4637CB83-90FB-4E4D-B834-D50D4C845383", "Retention in Invoice")); } }
			public static CodeDescriptionPair SalesTax { get { return new CodeDescriptionPair("SLX", ResString.GetMultilingualString("C1E890B4-5C82-4971-A0F3-33C80DF240E5", "Sales Tax")); } }
			public static CodeDescriptionPair StandardPaymentRetention { get { return new CodeDescriptionPair("SPR", ResString.GetMultilingualString("F22673C1-151B-4F1A-856E-EAEF9D996D9C", "Standard Payment Retention")); } }
			public static CodeDescriptionPair TurnoverTax { get { return new CodeDescriptionPair("TRX", ResString.GetMultilingualString("1B0FAD72-D9A2-4713-B160-3154EB964B60", "Turnover Tax")); } }
			public static CodeDescriptionPair ValueAddedTax { get { return new CodeDescriptionPair("VAT", ResString.GetMultilingualString("9D5BBA9C-FD45-480C-9185-CD701E5009D7", "Value Added Tax")); } }
		}

		public sealed class TaxSystemRegistrationLevels : CodeDescriptionPairList
		{
			public TaxSystemRegistrationLevels()
			{
				Add(Company);
				Add(Branch);
			}

			public static CodeDescriptionPair Company { get { return new CodeDescriptionPair("CMP", ResString.GetMultilingualString("F60C3C39-98CE-4B21-999F-158F7F9636BF", "Company")); } }
			public static CodeDescriptionPair Branch { get { return new CodeDescriptionPair("BRN", ResString.GetMultilingualString("CC4A10AD-BBC5-4831-9933-967DFE54AAB3", "Branch")); } }
		}

		public sealed class TaxCalculationAdjustmentSigns : CodeDescriptionPairList
		{
			public TaxCalculationAdjustmentSigns()
			{
				Add(Positive);
				Add(Negative);
			}

			public static CodeDescriptionPair Positive { get { return new CodeDescriptionPair("POS", ResString.GetMultilingualString("CF71C1CD-43CD-44CA-ACD1-C865C065FB42", "Positive")); } }
			public static CodeDescriptionPair Negative { get { return new CodeDescriptionPair("NEG", ResString.GetMultilingualString("B5A5D910-18D4-439F-B188-F053346B9F4A", "Negative")); } }
		}

		public sealed class TaxBaseCalculationMethods : CodeDescriptionPairList
		{
			public TaxBaseCalculationMethods()
			{
				Add(InvoiceLineAmount);
			}

			public static CodeDescriptionPair InvoiceLineAmount { get { return new CodeDescriptionPair("ILA", ResString.GetMultilingualString("7F4CC96D-F5E0-4B05-81C8-761BF830E42A", "Invoice Line Amount")); } }
		}

		public sealed class TaxAmountCalculationMethods : CodeDescriptionPairList
		{
			public TaxAmountCalculationMethods()
			{
				Add(BaseTimesRate);
			}

			public static CodeDescriptionPair BaseTimesRate { get { return new CodeDescriptionPair("BTR", ResString.GetMultilingualString("8D05BE02-3DEF-4CA8-B4C4-A232F076BF1F", "Base * Rate")); } }
		}

		public sealed class ThresholdRulesForTaxCalculation : CodeDescriptionPairList
		{
			public ThresholdRulesForTaxCalculation()
			{
				Add(NoThreshold);
			}

			public static CodeDescriptionPair NoThreshold { get { return new CodeDescriptionPair("NTR", ResString.GetMultilingualString("F0F874B7-1373-44AA-9448-62B489D7DB7A", "No threshold")); } }
		}

		public sealed class TaxRateSources : CodeDescriptionPairList
		{
			public TaxRateSources()
			{
				Add(OrganisationOnly);
				Add(OrganisationFallbackToTaxGroup);
				Add(OrganisationFallbackToTaxID);
				Add(TaxGroupOnly);
				Add(TaxIDOnly);
			}
			public static CodeDescriptionPair OrganisationOnly { get { return new CodeDescriptionPair("ORG", ResString.GetMultilingualString("7D2FC96A-B233-4B39-93F8-6CEA02D58737", "Organization Only")); } }
			public static CodeDescriptionPair OrganisationFallbackToTaxGroup { get { return new CodeDescriptionPair("OFT", ResString.GetMultilingualString("0ED9A6DF-A038-4EF1-A18A-6B54967CB82E", "Organization Fallback to Tax Group")); } }
			public static CodeDescriptionPair OrganisationFallbackToTaxID { get { return new CodeDescriptionPair("OFI", ResString.GetMultilingualString("A6A47745-0DFE-483A-AF0D-C589E9360395", "Organization Fallback to Tax ID")); } }
			public static CodeDescriptionPair TaxGroupOnly { get { return new CodeDescriptionPair("TGR", ResString.GetMultilingualString("B807FF0F-703A-4155-B57A-EB7DA2A0F23B", "Tax Group Only")); } }
			public static CodeDescriptionPair TaxIDOnly { get { return new CodeDescriptionPair("TID", ResString.GetMultilingualString("3E24A83C-AF70-4559-87B5-172A424BF2F6", "Tax ID Only")); } }
		}

		public sealed class TaxBasisList : CodeDescriptionPairList
		{
			public TaxBasisList()
			{
				Add(Posting);
				Add(Matching);
				Add(PostingOnMatching);
			}

			public static CodeDescriptionPair Posting { get { return new CodeDescriptionPair("PST", ResString.GetMultilingualString("36923340-6705-475E-A1DE-4F4A0525870E", "Posting")); } }
			public static CodeDescriptionPair Matching { get { return new CodeDescriptionPair("MAT", ResString.GetMultilingualString("46EDE415-A93D-4643-B5AC-63D33855ABB3", "Matching")); } }
			public static CodeDescriptionPair PostingOnMatching { get { return new CodeDescriptionPair("PTM", ResString.GetMultilingualString("CAD77746-32BB-475C-B12E-0AA9CAD01B95", "Posting on Matching")); } }
		}

		public sealed class TaxGLMovementTypeList : CodeDescriptionPairList
		{
			public TaxGLMovementTypeList()
			{
				Add(Normal);
				Add(Pending);
				Add(Realised);
			}

			public static CodeDescriptionPair Normal { get { return new CodeDescriptionPair("NRM", ResString.GetMultilingualString("66F8810A-006A-456F-B29F-507F4BB5B64B", "Normal")); } }
			public static CodeDescriptionPair Pending { get { return new CodeDescriptionPair("PND", ResString.GetMultilingualString("6E8B7ADE-53D4-4FE9-8DBC-22AA8B35295D", "Pending")); } }
			public static CodeDescriptionPair Realised { get { return new CodeDescriptionPair("RLS", ResString.GetMultilingualString("9A6BE62A-7DFC-45AD-B1E2-AD1B54E15A2F", "Realized")); } }
		}

		public sealed class TaxRecoveryMethods : CodeDescriptionPairList
		{
			public TaxRecoveryMethods()
			{
				Add(NoRecovery);
				Add(RecoverTaxExpense);
			}

			public static CodeDescriptionPair NoRecovery { get { return new CodeDescriptionPair("NOR", ResString.GetMultilingualString("B115156D-9099-490A-A8DE-B24D61BB3F50", "No Recovery of Tax Expense from Receivable Organization")); } }
			public static CodeDescriptionPair RecoverTaxExpense { get { return new CodeDescriptionPair("REC", ResString.GetMultilingualString("170C8F91-561D-4A8A-9922-72D2D203300E", "Recover Tax Expense as Revenue Line Attracting Tax")); } }
		}

		public sealed class ETC_ThresholdMethods : CodeDescriptionPairList
		{
			public ETC_ThresholdMethods()
			{
				Add(NoThreshold);
				Add(TransactionLevel);
				Add(TransactionLevelGroup);
				Add(TransactionLevelTaxBase);
			}

			public static CodeDescriptionPair NoThreshold { get { return new CodeDescriptionPair("NOT", ResString.GetMultilingualString("3071A9D6-D17E-4D47-B223-485882F0D230", "No Threshold")); } }
			public static CodeDescriptionPair TransactionLevel { get { return new CodeDescriptionPair("TRN", ResString.GetMultilingualString("BB3EEFC6-22D1-4283-837E-FE0C551B3FBC", "Transaction Level Tax Amount")); } }
			public static CodeDescriptionPair TransactionLevelGroup { get { return new CodeDescriptionPair("GRP", ResString.GetMultilingualString("29BC1792-289F-461A-ADFA-CCE59F147A27", "Transaction Level Group Tax Amount")); } }
			public static CodeDescriptionPair TransactionLevelTaxBase { get { return new CodeDescriptionPair("TRB", ResString.GetMultilingualString("2C2B56A5-2614-4A9E-B5AE-3F7DB26AE5F1", "Transaction Level Tax Base Amount")); } }
		}

		public sealed class TaxAmountRoundingMethods : CodeDescriptionPairList
		{
			public TaxAmountRoundingMethods()
			{
				Add(Standard);
				Add(RoundDownToMinorUnit);
			}

			public static CodeDescriptionPair Standard { get { return new CodeDescriptionPair("STD", ResString.GetMultilingualString("7ac283de-a38b-4a31-b679-ad7b9adbf9f5", "Round to nearest minor unit")); } }
			public static CodeDescriptionPair RoundDownToMinorUnit { get { return new CodeDescriptionPair("MDN", ResString.GetMultilingualString("26219db3-1470-418e-a8ba-d9fafc3a68f8", "Round down to nearest minor unit")); } }
		}

		public static ReadOnlyCollection<string> CountriesWithDefaultTaxFrameworkConfiguration => Array.AsReadOnly(new string[]
		{
			CountryCodes.Argentina,
			CountryCodes.Brazil,
			CountryCodes.Colombia,
			CountryCodes.India,
		});
	}
}
