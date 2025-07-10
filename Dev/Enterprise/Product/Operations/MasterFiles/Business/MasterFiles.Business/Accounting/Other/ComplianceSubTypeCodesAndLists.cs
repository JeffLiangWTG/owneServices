using System;
using System.Globalization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Registry.Business.ComplianceSubTypeCodesAndLists.CodesAndDescriptions;

namespace Enterprise.Registry.Business
{
	public static class ComplianceSubTypeCodesAndLists
	{
		public static class CodesAndDescriptions
		{
			public static class TaxInvoiceRuleCodes
			{
				public const string All = "ALL";
				public const string ContainsAnAmountOfTax = "AMT";
				public const string ContainsAtLeastOneTaxID = "TID";
				public const string ContainsAtLeastOneTaxIDExcludingNOT = "TXN";
				public const string ContainsAtLeastOneTaxIDExcludingNOTAndEXL = "TNE";
				public const string ContainsAtLeastOneTaxIDAndNoAmountOfTax = "TXX";
				public const string ContainsAtLeastOneTaxIDAndAmountOfTax = "TXA";
				public const string ContainsAtLeastOneTaxIDExcludingReverseChargeTaxIDs = "TXR";
				public const string ContainsAtLeastOneSuspendedTaxID = "SUS";
				public const string ContainsAtLeastOneTaxIDExcludingSuspended = "TXS";
				public const string ContainsReverseChargeTaxIDsOnly = "RVS";
				public const string ContainsExcludeChargeTaxIDsOnly = "EXL";
				public const string ContainsNoTaxIDs = "NON";
				public const string AllWithRatedTaxIDAndZeroTaxAmount = "ATZ";
				public const string SpecificTaxIDs = "STI";
				public const string AllWithExemptTaxIDs = "EXT";
				public const string AllWithNoReportTaxIDs = "NOT";
			}

			public static class TaxInvoiceRuleDescriptions
			{
				public static string All
				{
					get { return Res.GetString("4c20bb5d-a53b-4d96-9412-311116a687a8", "All Transactions, even without Tax"); }
				}
				public static string ContainsAnAmountOfTax
				{
					get { return Res.GetString("905f6270-d54a-4c0c-830f-7b66a4eb2a6a", "Transaction contains an Amount of Tax"); }
				}
				public static string ContainsAtLeastOneTaxID
				{
					get { return Res.GetString("a1eff897-8af3-4e54-912b-d0f530a30550", "Transaction contains at least one Tax ID"); }
				}
				public static string ContainsAtLeastOneTaxIDExcludingNOT
				{
					get { return Res.GetString("0f2ed575-250d-4ab9-a7a2-a955dd0780e4", "Transaction contains at least one Tax ID’s (excluding ‘NOT’ types)"); }
				}

				public static string ContainsAtLeastOneTaxIDExcludingNOTAndEXL
				{
					get { return Res.GetString("D31969B8-636C-4354-84A6-5085A0C91C71", "Transaction contains at least one Tax ID’s (excluding ‘NOT’ and ‘EXL’ types)"); }
				}

				public static string ContainsAtLeastOneTaxIDAndNoAmountOfTax
				{
					get { return Res.GetString("71163b89-9287-471e-8cf6-0d957a8aed20", "Transaction contains at least one Tax ID and No amount of Tax"); }
				}
				public static string ContainsAtLeastOneTaxIDAndAmountOfTax
				{
					get { return Res.GetString("489754ff-cda7-41ff-a64d-26c968b406f2", "Transaction contains at least one Tax ID and contains an Amount of Tax"); }
				}
				public static string ContainsAtLeastOneTaxIDExcludingReverseChargeTaxIDs
				{
					get { return Res.GetString("c4428863-199e-4f9c-ac79-8aa7a5d86f0f", "Transaction Contains At Least One Tax ID Excluding Reverse Charge Tax IDs"); }
				}

				public static string ContainsAtLeastOneSuspendedTaxID
				{
					get { return Res.GetString("1951ce9e-75a0-4615-90fc-0a772c449188", "Transaction contains at least one Suspended Tax ID"); }
				}
				public static string ContainsAtLeastOneTaxIDExcludingSuspended
				{
					get { return Res.GetString("bd98d46c-e7a7-40bc-bd0d-61a56c0986ea", "Transaction contains at least one Tax ID excluding Suspended Tax IDs"); }
				}

				public static string ContainseverseChargeTaxIDsOnly
				{
					get { return Res.GetString("b65b2904-b630-48e3-b6f7-63cdb0cfd150", "Transaction Contains Reverse Charge Tax IDs Only"); }
				}
				public static string ContainsExcludeChargeTaxIDsOnly
				{
					get { return Res.GetString("e3f812ba-52f5-4a4c-926e-abca78e80e5e", "Transaction Contains Excluded Tax IDs Only"); }
				}
				public static string ContainsNoTaxIDs
				{
					get { return Res.GetString("779D3611-DFEE-4A29-AF1A-F5B65555EA78", "Transaction Contains No Tax IDs"); }
				}
				public static string AllWithRatedTaxIDAndZeroTaxAmount
				{
					get { return Res.GetString("9F2BE579-9E3C-4E7C-90BF-41FD6AA124EF", "All Transaction lines have a Rated Tax ID, Including Zero Amount Tax IDs ('RAT', 'CAP' and 'INT' types)"); }
				}
				public static string SpecificTaxIDs
				{
					get { return Res.GetString("e8a35c3c-e274-4ad5-ac1a-a29131168666", "Transaction containing Specific Tax IDs Only in the column 'Tax Invoice Rule"); }
				}
				public static string AllWithExemptTaxIDs
				{
					get { return Res.GetString("4BA72DB5-E1FF-47D7-BDC1-CA7632BD20ED", "All Transaction lines have Exempt Tax IDs ('EXT' type)"); }
				}
				public static string AllWithNoReportTaxIDs
				{
					get { return Res.GetString("92CF511A-AEDB-4BA4-80AF-1E8E62ECD8A7", "All Transaction lines have Not Report Tax IDs ('NOT' type)"); }
				}
			}

			public static class OriginalRuleCodes
			{
				public const string AllTransactions = "ALL";
				public const string AmendingTransactionOnly = "ATO";
				public const string ReversalTransactionOnly = "RTO";
				public const string AmendingReversalOnly = "ARO";
				public const string OriginalTransactionOnly = "OTO";
			}

			public static class OriginalRuleDescriptions
			{
				public static string AllTransactions
					=> Res.GetString("50778051-a882-4388-bddf-1b64a5f01a3c", "All Transactions including Original, Amending and Reversal");
				public static string AmendingTransactionOnly
					=> Res.GetString("6E22CBEC-D6F4-4205-B635-1C740D6F30C9", "Amending Transactions Only");
				public static string ReversalTransactionOnly
					=> Res.GetString("3BD7EC93-C10B-4D0F-842D-31735181D471", "Reversal Transactions Only");
				public static string AmendingReversalOnly
					=> Res.GetString("0fb6f018-5712-4eb4-a082-778dd80d414b", "Amending/Reversal Transactions Only");
				public static string OriginalTransactionOnly
					=> Res.GetString("bab77269-e8f9-4470-a1ae-672ef685f0e4", "Original Transactions Only");
			}

			public static class DisbursementRuleCodes
			{
				public const string AllTransactions = "ALL";
				public const string DisbursementOnly = "DSB";
				public const string NonDisbursementOnly = "NDB";
			}

			public static class DisbursementRuleDescriptions
			{
				public static string AllTransactions
				{
					get { return Res.GetString("07cf2b0a-bd4d-4923-a996-54ed575cac0b", "All Transactions including Disbursement"); }
				}
				public static string DisbursementOnly
				{
					get { return Res.GetString("8def8426-d4bb-4a23-a4fb-b8b19b39f20f", "Disbursement Only"); }
				}
				public static string NonDisbursementOnly
				{
					get { return Res.GetString("f048bdfa-ed3d-4fc4-b210-1e9ed8ace5c4", "Non Disbursement Only"); }
				}
			}

			public static class TaxRegistrationLocationRuleCodes
			{
				public const string EUExcludingLoginCountry = AccChargeTaxOverride.EuropeanUnionExcludingLoginCountry;
				public const string NotEU = AccChargeTaxOverride.NotEuropeanUnion;
			}

			public static class TaxRegistrationLocationRuleDescriptions
			{
				public static string EUExcludingLoginCountry
				{
					get { return Res.GetString("3452639e-c930-42e5-9b13-b7edbe24b111", "EU Excluding Configuration Country/Region"); }
				}

				public static string NotEU
				{
					get { return Res.GetString("75f73e8d-3f8b-49e3-9986-f7bf9be36810", "Not EU"); }
				}
			}

			public static class ExporterExemptionCodes
			{
				public const string Exempt = "EXV";
				public const string NotExempt = "NON";
			}

			public static class ExporterExemptionDescriptions
			{
				public static string Exempt => Res.GetString("404BD4FB-94ED-478B-84B4-EFE884C98232", "Organizations with valid Exporter Exemption in this country/region");
				public static string NotExempt => Res.GetString("543D4EC0-4493-4950-B934-929E32E4C34B", "Organizations without valid Exporter Exemption in this country/region");
			}

			public static class SelfBillingRuleCodes
			{
				public const string SelfBillingTransactions = "SBI";
				public const string StandardTransactions = "STD";
			}

			public static class SelfBillingRuleDescriptions
			{
				public static string SelfBillingTransactions
				{
					get { return Res.GetString("4d261e01-fd12-48df-b95e-767bbf54a3be", "Self Billing Transactions Only"); }
				}

				public static string StandardTransactions
				{
					get { return Res.GetString("2128c704-3831-42ba-bcc2-b665ecf5bc27", "Non Self Billing Transactions Only"); }
				}
			}

			public static class VatGroupListCodes
			{
				public const string VATGroupMember = "VGM";
				public const string ExcludeVATGroupMembers = "EVG";
			}

			public static class VatGroupListDescriptions
			{
				public static string VATGroupMember
				{
					get { return Res.GetString("8d76e9de-4818-44a5-b7f5-4e5b23a6ce6f", "VAT Group Members Only"); }
				}

				public static string ExcludeVATGroupMembers
				{
					get { return Res.GetString("facfda9a-5f90-4d81-b43d-4859f32b44fc", "Exclude VAT Group Members"); }
				}
			}

			public static class TaxRegistrationTypeCodes
			{
				public const string NotRegisteredOrganizations = "NON";
				public const string SVATOrganizations = "SVT";
				public const string Recoverable = "REC";
				public const string NotRecoverable = "NOT";
				public const string Individual = "DNI";
				public const string OrgRegisteredForTaxInPeru = "RUC";
				public const string OrgRegisteredForTaxInElSalvador = "NRC";
				public const string BasicEInvoiceRegisteredOrganizationTurkey = "ERO";
				public const string CommercialEInvoiceRegisteredOrganizationTurkey = "ERC";
				public const string EInvoiceRegisteredOrganizationTurkey = "ERG";
				public const string eInvoiceNotRegisteredOrganizationTurkey = "NER";
				public const string AllOrganizations = "ALL";
				public const string ForeignOrganisation = "FOR";
			}

			public static class TaxRegistrationTypeDescriptions
			{
				public static string NotRegisteredOrganizations
					=> Res.GetString("6083777E-5D9D-4CD4-9775-CF22E4D0CE40", "Not Registered for VAT");
				public static string SVATOrganizations
					=> Res.GetString("AA776423-1CA2-4319-9A70-3BAB944148FC", "SVAT Registered Organization");
				public static string Recoverable
					=> Res.GetString("7EC640F8-72DA-43AA-A791-3D9AB4B902C4", "VAT Recoverable Organization");
				public static string NotRecoverable
					=> Res.GetString("CB30999C-B4D3-4D39-AB92-65535E762AB0", "VAT Not Recoverable Organization");
				public static string Individual
					=> Res.GetString("77700fb9-3db5-48d4-a13f-7bdedbb2efa6", "Individual / Natural Person");
				public static string OrgRegisteredForTax
					=> Res.GetString("c8234213-e825-4219-b01a-f6e25342dd12", "Organization Registered for Tax");
				public static string eInvoiceRegisteredOrganizationTurkey
					=> Res.GetString("963C932B-1F24-4EFB-83BF-0ADDE93A234C", "e-Invoice Registered Organization");
				public static string BasicEInvoiceRegisteredOrganizationTurkey
					=> Res.GetString("F0B11A40-EDBE-4C9C-B451-07138C9DF53A", "e-Invoice Registered Organization {0}", string.Format(CultureInfo.InvariantCulture, "(Temel/{0})", Res.GetString("721C1758-D257-4163-8D74-8C5741D77F0B", "Basic")));
				public static string CommercialEInvoiceRegisteredOrganizationTurkey
					=> Res.GetString("0013CEE9-9B62-4E63-B390-F2A1C051B7F3", "e-Invoice Registered Organization {0}", string.Format(CultureInfo.InvariantCulture, "(Ticari/{0})", Res.GetString("EFCBFE8F-D9E6-43E8-80D6-C5BDA87EBAF2", "Commercial")));
				public static string EInvoiceRegisteredOrganizationTurkey
					=> Res.GetString("6A4DBBAB-7244-444F-A996-2EE9EF69C1B4", "e-Invoice Registered Organization {0}", string.Format(CultureInfo.InvariantCulture, "(Temel veya Ticari/{0})", Res.GetString("32186B37-19AE-4377-B108-BE691B689A13", "Basic or Commercial")));
				public static string eInvoiceNotRegisteredOrganizationTurkey
					=> Res.GetString("4417D55F-2838-422A-BE1A-0C352243E89C", "Not Registered as e-Invoice Organization");
				public static string AllOrganizations
					=> Res.GetString("17565B6A-6679-44AE-8ED0-7068E4325702", "All Organizations");
				public static string ForeignOrganisation
					=> Res.GetString("F4D3D3A4-3A3D-4A3D-8A3D-3A3D3A3D3A3D", "Foreign Organization");
			}
		}

		public static class ReportingDateCodes
		{
			public const string PostDate = "POS";
			public const string InvoiceDate = "INV";
			public const string DocumentReceivedDate = "DRD";
			public const string EarliestTaxDate = "TDE";
			public const string LatestTaxDate = "TDL";
		}

		public static class ReportingDateDescriptions
		{
			public static string PostDate => Res.GetString("b9a59eb9-49b2-4120-acbb-55d92e13e076", "Post Date");
			public static string InvoiceDate => Res.GetString("89b6e488-4144-442e-84ef-e162380cb738", "Invoice Date");
			public static string DocumentReceivedDate => Res.GetString("ae43306d-e6ac-4368-83b0-ad231b8180a6", "Document Received (fallback to Invoice) Date");
			public static string EarliestTaxDate => Res.GetString("6129de5d-30dc-4df1-bf41-f389e0d6a758", "Tax Date - Earliest");
			public static string LatestTaxDate => Res.GetString("b9edf9dd-7b22-48c8-80d0-ba79105bc015", "Tax Date - Latest");
		}

		public static class Lists
		{
			public static CodeDescriptionPairList GetTaxInvoiceRuleList(IComplianceSubTypeListAdditionalDataProvider provider)
			{
				var result = new CodeDescriptionPairList();
				result.AddPair(TaxInvoiceRuleCodes.ContainsAnAmountOfTax, TaxInvoiceRuleDescriptions.ContainsAnAmountOfTax);
				result.AddPair(TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID, TaxInvoiceRuleDescriptions.ContainsAtLeastOneTaxID);
				result.AddPair(TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDExcludingNOT, TaxInvoiceRuleDescriptions.ContainsAtLeastOneTaxIDExcludingNOT);
				result.AddPair(TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDExcludingNOTAndEXL, TaxInvoiceRuleDescriptions.ContainsAtLeastOneTaxIDExcludingNOTAndEXL);
				result.AddPair(TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDAndNoAmountOfTax, TaxInvoiceRuleDescriptions.ContainsAtLeastOneTaxIDAndNoAmountOfTax);
				result.AddPair(TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDAndAmountOfTax, TaxInvoiceRuleDescriptions.ContainsAtLeastOneTaxIDAndAmountOfTax);
				result.AddPair(TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDExcludingReverseChargeTaxIDs, TaxInvoiceRuleDescriptions.ContainsAtLeastOneTaxIDExcludingReverseChargeTaxIDs);
				result.AddPair(TaxInvoiceRuleCodes.ContainsAtLeastOneSuspendedTaxID, TaxInvoiceRuleDescriptions.ContainsAtLeastOneSuspendedTaxID);
				result.AddPair(TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDExcludingSuspended, TaxInvoiceRuleDescriptions.ContainsAtLeastOneTaxIDExcludingSuspended);
				result.AddPair(TaxInvoiceRuleCodes.ContainsReverseChargeTaxIDsOnly, TaxInvoiceRuleDescriptions.ContainseverseChargeTaxIDsOnly);
				result.AddPair(TaxInvoiceRuleCodes.ContainsExcludeChargeTaxIDsOnly, TaxInvoiceRuleDescriptions.ContainsExcludeChargeTaxIDsOnly);
				result.AddPair(TaxInvoiceRuleCodes.AllWithRatedTaxIDAndZeroTaxAmount, TaxInvoiceRuleDescriptions.AllWithRatedTaxIDAndZeroTaxAmount);
				result.AddPair(TaxInvoiceRuleCodes.SpecificTaxIDs, TaxInvoiceRuleDescriptions.SpecificTaxIDs);
				result.AddPair(TaxInvoiceRuleCodes.AllWithExemptTaxIDs, TaxInvoiceRuleDescriptions.AllWithExemptTaxIDs);
				result.AddPair(TaxInvoiceRuleCodes.AllWithNoReportTaxIDs, TaxInvoiceRuleDescriptions.AllWithNoReportTaxIDs);
				result.AddPair(TaxInvoiceRuleCodes.ContainsNoTaxIDs, TaxInvoiceRuleDescriptions.ContainsNoTaxIDs);
				return AddAdditionalPairsToResult(result, () => provider?.GetAdditionalTaxInvoiceRuleList());
			}

			public static CodeDescriptionPairList OriginalRuleList
			{
				get
				{
					var result = new CodeDescriptionPairList();
					result.AddPair(OriginalRuleCodes.AllTransactions, OriginalRuleDescriptions.AllTransactions);
					result.AddPair(OriginalRuleCodes.AmendingTransactionOnly, OriginalRuleDescriptions.AmendingTransactionOnly);
					result.AddPair(OriginalRuleCodes.ReversalTransactionOnly, OriginalRuleDescriptions.ReversalTransactionOnly);
					result.AddPair(OriginalRuleCodes.AmendingReversalOnly, OriginalRuleDescriptions.AmendingReversalOnly);
					result.AddPair(OriginalRuleCodes.OriginalTransactionOnly, OriginalRuleDescriptions.OriginalTransactionOnly);
					return result;
				}
			}

			public static CodeDescriptionPairList DisbursementRuleList
			{
				get
				{
					var result = new CodeDescriptionPairList();
					result.AddPair(DisbursementRuleCodes.AllTransactions, DisbursementRuleDescriptions.AllTransactions);
					result.AddPair(DisbursementRuleCodes.DisbursementOnly, DisbursementRuleDescriptions.DisbursementOnly);
					result.AddPair(DisbursementRuleCodes.NonDisbursementOnly, DisbursementRuleDescriptions.NonDisbursementOnly);
					return result;
				}
			}

			public static CodeDescriptionPairList ExporterExemptionList
			{
				get
				{
					var result = new CodeDescriptionPairList();
					result.AddPair(ExporterExemptionCodes.Exempt, ExporterExemptionDescriptions.Exempt);
					result.AddPair(ExporterExemptionCodes.NotExempt, ExporterExemptionDescriptions.NotExempt);
					return result;
				}
			}

			public static CodeDescriptionPairList SelfBillingRuleList
			{
				get
				{
					var result = new CodeDescriptionPairList();
					result.AddPair(SelfBillingRuleCodes.SelfBillingTransactions, SelfBillingRuleDescriptions.SelfBillingTransactions);
					result.AddPair(SelfBillingRuleCodes.StandardTransactions, SelfBillingRuleDescriptions.StandardTransactions);
					return result;
				}
			}

			public static CodeDescriptionPairList VATGroupList
			{
				get
				{
					var result = new CodeDescriptionPairList();
					result.AddPair(VatGroupListCodes.VATGroupMember, VatGroupListDescriptions.VATGroupMember);
					result.AddPair(VatGroupListCodes.ExcludeVATGroupMembers, VatGroupListDescriptions.ExcludeVATGroupMembers);
					return result;
				}
			}

			public static CodeDescriptionPairList GetSubTypeList(ZString country) => AccComplianceSequenceLookups.SequenceClassListBaseOnCountryCode(country);

			public static CodeDescriptionPairList GetLedgerTypeList(IComplianceSubTypeListAdditionalDataProvider provider)
			{
				var result = new CodeDescriptionPairList();
				result.AddPair(LedgerTypes.AccountsReceivable, Res.GetString("c1384f9a-1eae-40ce-a5db-9a698a6c4a41", "Receivables"));
				result.AddPair(LedgerTypes.AccountsPayable, Res.GetString("68e38a95-b8bb-4136-a7f8-b557e6360b10", "Payables"));
				return AddAdditionalPairsToResult(result, () => provider?.GetAdditionalLedgerTypeList());
			}

			public static CodeDescriptionPairList GetInvoiceTypeList(IComplianceSubTypeListAdditionalDataProvider provider)
			{
				var result = new CodeDescriptionPairList();
				return AddAdditionalPairsToResult(result, () => provider?.GetAdditionalInvoiceTypeList());
			}

			public static RefCountryCollection GetCountryList(BusinessObjectFactory factory)
			{
				return new RefCountryCollection(factory);
			}

			public static CodeDescriptionPairList GetOrganisationLocationList(ZString country, BusinessObjectFactory factory)
			{
				var result = new CodeDescriptionPairList();
				var countryBeingConfigured = RefCountry.LoadFromCountryCode(factory, country);
				if (countryBeingConfigured != null)
				{
					if (countryBeingConfigured.IsPartOfEuropeanUnion)
					{
						result.AddPair(country, Res.GetString("a9c4e2dd-209b-490a-a933-7c785e579bd3", "This Country/Region"));
						result.AddPair(TaxRegistrationLocationRuleCodes.EUExcludingLoginCountry, TaxRegistrationLocationRuleDescriptions.EUExcludingLoginCountry);
						result.AddPair(TaxRegistrationLocationRuleCodes.NotEU, TaxRegistrationLocationRuleDescriptions.NotEU);
					}
					else
					{
						result.AddRange(AccountingTaxLocations.GetCountries(factory));
					}
				}
				return result;
			}

			public static CodeDescriptionPairList GetTaxRegistrationLocationRuleList(ZString country, BusinessObjectFactory factory)
			{
				var result = new CodeDescriptionPairList();
				var countryBeingConfigured = RefCountry.LoadFromCountryCode(factory, country);
				if (countryBeingConfigured != null)
				{
					result.AddPair(country, Res.GetString("a9c4e2dd-209b-490a-a933-7c785e579bd3", "This Country/Region"));
				}
				if (countryBeingConfigured != null && countryBeingConfigured.IsPartOfEuropeanUnion)
				{
					result.AddPair(TaxRegistrationLocationRuleCodes.EUExcludingLoginCountry, TaxRegistrationLocationRuleDescriptions.EUExcludingLoginCountry);
					result.AddPair(TaxRegistrationLocationRuleCodes.NotEU, TaxRegistrationLocationRuleDescriptions.NotEU);
				}
				return result;
			}

			public static CodeDescriptionPairList GetTaxRegistrationTypeList(IComplianceSubTypeListAdditionalDataProvider provider, ZString country)
			{
				var result = new CodeDescriptionPairList();
				if (country == Core.Constants.CountryCodes.Argentina || country == Core.Constants.CountryCodes.China)
				{
					result.AddPair(TaxRegistrationTypeCodes.Recoverable, TaxRegistrationTypeDescriptions.Recoverable);
					result.AddPair(TaxRegistrationTypeCodes.NotRecoverable, TaxRegistrationTypeDescriptions.NotRecoverable);
				}
				return AddAdditionalPairsToResult(result, () => provider?.GetAdditionalTaxRegistrationTypeList());
			}

			static CodeDescriptionPairList AddAdditionalPairsToResult(CodeDescriptionPairList result, Func<CodeDescriptionPairList> getAdditionalList)
			{
				var additionalList = getAdditionalList();
				if (additionalList != null && additionalList.Count > 0)
				{
					result.AddRange(additionalList);
				}
				return result;
			}

			public static CodeDescriptionPairList GetReportingDateList(ZString ledger)
			{
				var result = new CodeDescriptionPairList();

				switch (ledger)
				{
					case LedgerTypes.AccountsReceivable:
						result.AddPair(ReportingDateCodes.PostDate, ReportingDateDescriptions.PostDate);
						result.AddPair(ReportingDateCodes.InvoiceDate, ReportingDateDescriptions.InvoiceDate);
						result.AddPair(ReportingDateCodes.EarliestTaxDate, ReportingDateDescriptions.EarliestTaxDate);
						result.AddPair(ReportingDateCodes.LatestTaxDate, ReportingDateDescriptions.LatestTaxDate);
						return result;
					case LedgerTypes.AccountsPayable:
						result.AddPair(ReportingDateCodes.PostDate, ReportingDateDescriptions.PostDate);
						result.AddPair(ReportingDateCodes.InvoiceDate, ReportingDateDescriptions.InvoiceDate);
						result.AddPair(ReportingDateCodes.DocumentReceivedDate, ReportingDateDescriptions.DocumentReceivedDate);
						result.AddPair(ReportingDateCodes.EarliestTaxDate, ReportingDateDescriptions.EarliestTaxDate);
						result.AddPair(ReportingDateCodes.LatestTaxDate, ReportingDateDescriptions.LatestTaxDate);
						return result;
					default:
						return result;
				}
			}
		}

		public interface IComplianceSubTypeListAdditionalDataProvider
		{
			CodeDescriptionPairList GetAdditionalTaxInvoiceRuleList();
			CodeDescriptionPairList GetAdditionalLedgerTypeList();
			CodeDescriptionPairList GetAdditionalInvoiceTypeList();
			CodeDescriptionPairList GetAdditionalTaxRegistrationTypeList();
		}
	}
}
