using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CargoWise.Common;
using Enterprise.Core;
using Enterprise.Integration.Compliance;
using Enterprise.Registry.Business;
using NUnit.Framework;
using static Enterprise.Registry.Business.ComplianceSubTypeCodesAndLists.CodesAndDescriptions;

namespace Enterprise.MasterFiles.Business.CountryCompliance.Testing
{
	[TestedType(typeof(DominicanRepublicComplianceInfo))]
	sealed class DominicanRepublicComplianceInfoTest : CountryComplianceInfoTest
	{
		protected override string CountryCode => Constants.CountryCodes.DominicanRepublic;

		protected override string[] ExpectedComplianceSubTypes => new[] { "TXG", "TCD", "TCR", "TXF", "TXI", "TXS", "VGM", "VIS", "XCL", "TEI", "TEF", "TED", "TEC", "VES", "VEM", "TEG", "TES" };

		protected override string[] ExpectedReceivablesComplianceSubTypes => new string[] { "TXG", "TCD", "TCR", "TXF", "TXI", "TXS", "XCL", "TEI", "TEF", "TED", "TEC", "TEG", "TES" };

		protected override string[] ExpectedPayablesComplianceSubTypes => new string[] { "TCD", "TCR", "TXF", "TXI", "TXS", "VGM", "VIS", "XCL", "TEI", "TEF", "TED", "TEC", "VES", "VEM", "TES" };

		protected override string[] ExpectedReceivablesInvoiceComplianceSubTypes => new string[] { "TXG", "TCD", "TXF", "TXI", "TXS", "XCL", "TEI", "TEF", "TED", "TEG", "TES" };

		protected override string[] ExpectedPayablesInvoiceComplianceSubTypes => new string[] { "TCD", "TXF", "TXI", "TXS", "VGM", "VIS", "XCL", "TEI", "TEF", "TED", "VES", "VEM", "TES" };

		protected override string[] ExpectedReceivablesCreditNoteComplianceSubTypes => new string[] { "TCR", "XCL", "TEC" };

		protected override string[] ExpectedPayablesCreditNoteComplianceSubTypes => new string[] { "TCR", "XCL", "TEC" };

		protected override Dictionary<string, LedgerOfUse> ExpectedComplianceLedgerOfUse =>
			new Dictionary<string, LedgerOfUse>
			{
				{ DominicanRepublicComplianceInfo.ComplianceSubTypeCodes.TXG, LedgerOfUse.AR },
				{ DominicanRepublicComplianceInfo.ComplianceSubTypeCodes.VGM, LedgerOfUse.AP },
				{ DominicanRepublicComplianceInfo.ComplianceSubTypeCodes.VIS, LedgerOfUse.AP },
				{ DominicanRepublicComplianceInfo.ComplianceSubTypeCodes.TEG, LedgerOfUse.AR },
				{ DominicanRepublicComplianceInfo.ComplianceSubTypeCodes.VES, LedgerOfUse.AP },
				{ DominicanRepublicComplianceInfo.ComplianceSubTypeCodes.VEM, LedgerOfUse.AP }
			};

		protected override IReadOnlyDictionary<string, TransactionTypeOfUse> ExpectedComplianceTransactionTypeOfUse => new Dictionary<string, TransactionTypeOfUse> {
			{ "TXI", TransactionTypeOfUse.INV }, { "TXF", TransactionTypeOfUse.INV }, { "TCD", TransactionTypeOfUse.INV }, { "TCR", TransactionTypeOfUse.CRD },
			{ "TXS", TransactionTypeOfUse.INV }, { "VIS", TransactionTypeOfUse.INV }, { "TXG", TransactionTypeOfUse.INV }, { "VGM", TransactionTypeOfUse.INV },
			{ "TEI", TransactionTypeOfUse.INV }, { "TEF", TransactionTypeOfUse.INV }, { "TED", TransactionTypeOfUse.INV }, { "TEC", TransactionTypeOfUse.CRD },
			{ "VES", TransactionTypeOfUse.INV }, { "VEM", TransactionTypeOfUse.INV }, { "TEG", TransactionTypeOfUse.INV }, { "TES", TransactionTypeOfUse.INV },
		};

		protected override string ExpectedComplianceSubTypeCodeForDescriptionTest => "TXG";

		protected override string ExpectedComplianceSubTypeDescription => "Government Recipient Invoice";

		protected override string ExpectedComplianceSubTypeLocalDescription => "Comprobante Gubernamental";

		protected override Func<ComplianceSubTypeAttributionRuleConfiguration, string>[] ExpectedComplianceRuleFields =>
			new Func<ComplianceSubTypeAttributionRuleConfiguration, string>[]
			{
				x => x.Country,
				x => x.SubType,
				x => x.LedgerType,
				x => x.InvoiceType,
				x => x.TaxInvoiceRule,
				x => x.DisbursementRule,
				x => x.OriginalRule,
				x => x.OrganisationLocation,
				x => x.ParentTransactionSubType,
				x => x.TaxRegistrationType,
				x => x.RuleSetCode,
				x => x.RuleSetDescription,
				x => x.SelfBillingRule,
				x => x.TaxRegistrationLocationRule,
				x => x.VATGroupRule,
				x => x.TaxIDCode,
				x => x.RequiredTaxSystem,
				x => x.ExcludedTaxSystem,
				x => x.ExporterExemption,
			};

		protected override string ExpectedComplianceRules => @"DO,TXI,AR,INV,TID,ALL,OTO,,,RCS,1,Tax, Special Regime, Government , Final Consumer and Reimbursement Transactions,,,,,,,NON
DO,TCD,AR,INV,TID,ALL,ARO,,TXI,RCS,1,Tax, Special Regime, Government , Final Consumer and Reimbursement Transactions,,,,,,,NON
DO,TCR,AR,CRD,TID,ALL,ARO,,TXI,RCS,1,Tax, Special Regime, Government , Final Consumer and Reimbursement Transactions,,,,,,,NON
DO,TCD,AR,INV,TID,ALL,ARO,,TCR,RCS,1,Tax, Special Regime, Government , Final Consumer and Reimbursement Transactions,,,,,,,NON
DO,TCR,AR,CRD,TID,ALL,ARO,,TCD,RCS,1,Tax, Special Regime, Government , Final Consumer and Reimbursement Transactions,,,,,,,NON
DO,XCL,AR,INV,EXL,ALL,OTO,,,RCS,1,Tax, Special Regime, Government , Final Consumer and Reimbursement Transactions,,,,,,,NON
DO,XCL,AR,CRD,EXL,ALL,OTO,,,RCS,1,Tax, Special Regime, Government , Final Consumer and Reimbursement Transactions,,,,,,,NON
DO,XCL,AR,INV,EXL,ALL,ARO,,XCL,RCS,1,Tax, Special Regime, Government , Final Consumer and Reimbursement Transactions,,,,,,,NON
DO,XCL,AR,CRD,EXL,ALL,ARO,,XCL,RCS,1,Tax, Special Regime, Government , Final Consumer and Reimbursement Transactions,,,,,,,NON
DO,TXS,AR,INV,TXX,ALL,OTO,,,,1,Tax, Special Regime, Government , Final Consumer and Reimbursement Transactions,,,,,,,EXV
DO,TCD,AR,INV,TXX,ALL,ARO,,TXS,,1,Tax, Special Regime, Government , Final Consumer and Reimbursement Transactions,,,,,,,EXV
DO,TCR,AR,CRD,TXX,ALL,ARO,,TXS,,1,Tax, Special Regime, Government , Final Consumer and Reimbursement Transactions,,,,,,,EXV
DO,TCD,AR,INV,TXX,ALL,ARO,,TCR,,1,Tax, Special Regime, Government , Final Consumer and Reimbursement Transactions,,,,,,,EXV
DO,TCR,AR,CRD,TXX,ALL,ARO,,TCD,,1,Tax, Special Regime, Government , Final Consumer and Reimbursement Transactions,,,,,,,EXV
DO,XCL,AR,INV,EXL,ALL,OTO,,,,1,Tax, Special Regime, Government , Final Consumer and Reimbursement Transactions,,,,,,,EXV
DO,XCL,AR,CRD,EXL,ALL,OTO,,,,1,Tax, Special Regime, Government , Final Consumer and Reimbursement Transactions,,,,,,,EXV
DO,XCL,AR,INV,EXL,ALL,ARO,,XCL,,1,Tax, Special Regime, Government , Final Consumer and Reimbursement Transactions,,,,,,,EXV
DO,XCL,AR,CRD,EXL,ALL,ARO,,XCL,,1,Tax, Special Regime, Government , Final Consumer and Reimbursement Transactions,,,,,,,EXV
DO,TXF,AR,INV,TID,ALL,OTO,,,,1,Tax, Special Regime, Government , Final Consumer and Reimbursement Transactions,,,,,,,NON
DO,TCD,AR,INV,TID,ALL,ARO,,TXF,,1,Tax, Special Regime, Government , Final Consumer and Reimbursement Transactions,,,,,,,NON
DO,TCR,AR,CRD,TID,ALL,ARO,,TXF,,1,Tax, Special Regime, Government , Final Consumer and Reimbursement Transactions,,,,,,,NON
DO,TCD,AR,INV,TID,ALL,ARO,,TCR,,1,Tax, Special Regime, Government , Final Consumer and Reimbursement Transactions,,,,,,,NON
DO,TCR,AR,CRD,TID,ALL,ARO,,TCD,,1,Tax, Special Regime, Government , Final Consumer and Reimbursement Transactions,,,,,,,NON
DO,XCL,AR,INV,EXL,ALL,OTO,,,,1,Tax, Special Regime, Government , Final Consumer and Reimbursement Transactions,,,,,,,NON
DO,XCL,AR,CRD,EXL,ALL,OTO,,,,1,Tax, Special Regime, Government , Final Consumer and Reimbursement Transactions,,,,,,,NON
DO,XCL,AR,INV,EXL,ALL,ARO,,XCL,,1,Tax, Special Regime, Government , Final Consumer and Reimbursement Transactions,,,,,,,NON
DO,XCL,AR,CRD,EXL,ALL,ARO,,XCL,,1,Tax, Special Regime, Government , Final Consumer and Reimbursement Transactions,,,,,,,NON
DO,TXG,AR,INV,TID,ALL,OTO,,,REG,1,Tax, Special Regime, Government , Final Consumer and Reimbursement Transactions,,,,,,,NON
DO,TCD,AR,INV,TID,ALL,ARO,,TXG,REG,1,Tax, Special Regime, Government , Final Consumer and Reimbursement Transactions,,,,,,,NON
DO,TCR,AR,CRD,TID,ALL,ARO,,TXG,REG,1,Tax, Special Regime, Government , Final Consumer and Reimbursement Transactions,,,,,,,NON
DO,TCD,AR,INV,TID,ALL,ARO,,TCR,REG,1,Tax, Special Regime, Government , Final Consumer and Reimbursement Transactions,,,,,,,NON
DO,TCR,AR,CRD,TID,ALL,ARO,,TCD,REG,1,Tax, Special Regime, Government , Final Consumer and Reimbursement Transactions,,,,,,,NON
DO,XCL,AR,INV,EXL,ALL,OTO,,,REG,1,Tax, Special Regime, Government , Final Consumer and Reimbursement Transactions,,,,,,,NON
DO,XCL,AR,CRD,EXL,ALL,OTO,,,REG,1,Tax, Special Regime, Government , Final Consumer and Reimbursement Transactions,,,,,,,NON
DO,XCL,AR,INV,EXL,ALL,ARO,,XCL,REG,1,Tax, Special Regime, Government , Final Consumer and Reimbursement Transactions,,,,,,,NON
DO,XCL,AR,CRD,EXL,ALL,ARO,,XCL,REG,1,Tax, Special Regime, Government , Final Consumer and Reimbursement Transactions,,,,,,,NON
DO,XCL,AP,INV,EXL,ALL,OTO,,,,1,Tax, Special Regime, Government , Final Consumer and Reimbursement Transactions,,,,,,,
DO,XCL,AP,CRD,EXL,ALL,OTO,,,,1,Tax, Special Regime, Government , Final Consumer and Reimbursement Transactions,,,,,,,
DO,XCL,AP,INV,EXL,ALL,ARO,,XCL,,1,Tax, Special Regime, Government , Final Consumer and Reimbursement Transactions,,,,,,,
DO,XCL,AP,CRD,EXL,ALL,ARO,,XCL,,1,Tax, Special Regime, Government , Final Consumer and Reimbursement Transactions,,,,,,,
DO,TEI,AR,INV,TID,ALL,OTO,,,RCS,2,e-CF Electronic Tax, Special Regime, Government, Final Consumer and Reimb. Transactions,,,,,,,NON
DO,TEF,AR,INV,TID,ALL,OTO,,,,2,e-CF Electronic Tax, Special Regime, Government, Final Consumer and Reimb. Transactions,,,,,,,NON
DO,TED,AR,INV,TID,ALL,ARO,,TEI,RCS,2,e-CF Electronic Tax, Special Regime, Government, Final Consumer and Reimb. Transactions,,,,,,,NON
DO,TED,AR,INV,TID,ALL,ARO,,TEC,RCS,2,e-CF Electronic Tax, Special Regime, Government, Final Consumer and Reimb. Transactions,,,,,,,NON
DO,TED,AR,INV,TXX,ALL,ARO,,TES,,2,e-CF Electronic Tax, Special Regime, Government, Final Consumer and Reimb. Transactions,,,,,,,EXV
DO,TED,AR,INV,TXX,ALL,ARO,,TEC,,2,e-CF Electronic Tax, Special Regime, Government, Final Consumer and Reimb. Transactions,,,,,,,EXV
DO,TED,AR,INV,TID,ALL,ARO,,TEF,,2,e-CF Electronic Tax, Special Regime, Government, Final Consumer and Reimb. Transactions,,,,,,,NON
DO,TED,AR,INV,TID,ALL,ARO,,TEC,,2,e-CF Electronic Tax, Special Regime, Government, Final Consumer and Reimb. Transactions,,,,,,,NON
DO,TED,AR,INV,TID,ALL,ARO,,TEG,REG,2,e-CF Electronic Tax, Special Regime, Government, Final Consumer and Reimb. Transactions,,,,,,,NON
DO,TED,AR,INV,TID,ALL,ARO,,TEC,REG,2,e-CF Electronic Tax, Special Regime, Government, Final Consumer and Reimb. Transactions,,,,,,,NON
DO,TES,AR,INV,TXX,ALL,OTO,,,,2,e-CF Electronic Tax, Special Regime, Government, Final Consumer and Reimb. Transactions,,,,,,,EXV
DO,TEG,AR,INV,TID,ALL,OTO,,,REG,2,e-CF Electronic Tax, Special Regime, Government, Final Consumer and Reimb. Transactions,,,,,,,NON
DO,TED,AR,INV,TID,ALL,ARO,,TXI,RCS,2,e-CF Electronic Tax, Special Regime, Government, Final Consumer and Reimb. Transactions,,,,,,,NON
DO,TED,AR,INV,TID,ALL,ARO,,TCR,RCS,2,e-CF Electronic Tax, Special Regime, Government, Final Consumer and Reimb. Transactions,,,,,,,NON
DO,TED,AR,INV,TXX,ALL,ARO,,TXS,,2,e-CF Electronic Tax, Special Regime, Government, Final Consumer and Reimb. Transactions,,,,,,,EXV
DO,TED,AR,INV,TXX,ALL,ARO,,TCR,,2,e-CF Electronic Tax, Special Regime, Government, Final Consumer and Reimb. Transactions,,,,,,,EXV
DO,TED,AR,INV,TID,ALL,ARO,,TXF,,2,e-CF Electronic Tax, Special Regime, Government, Final Consumer and Reimb. Transactions,,,,,,,NON
DO,TED,AR,INV,TID,ALL,ARO,,TCR,,2,e-CF Electronic Tax, Special Regime, Government, Final Consumer and Reimb. Transactions,,,,,,,NON
DO,TED,AR,INV,TID,ALL,ARO,,TXG,REG,2,e-CF Electronic Tax, Special Regime, Government, Final Consumer and Reimb. Transactions,,,,,,,NON
DO,TED,AR,INV,TID,ALL,ARO,,TCR,REG,2,e-CF Electronic Tax, Special Regime, Government, Final Consumer and Reimb. Transactions,,,,,,,NON
DO,TEC,AR,CRD,TID,ALL,ARO,,TEI,RCS,2,e-CF Electronic Tax, Special Regime, Government, Final Consumer and Reimb. Transactions,,,,,,,NON
DO,TEC,AR,CRD,TID,ALL,ARO,,TED,RCS,2,e-CF Electronic Tax, Special Regime, Government, Final Consumer and Reimb. Transactions,,,,,,,NON
DO,TEC,AR,CRD,TXX,ALL,ARO,,TES,,2,e-CF Electronic Tax, Special Regime, Government, Final Consumer and Reimb. Transactions,,,,,,,EXV
DO,TEC,AR,CRD,TXX,ALL,ARO,,TED,,2,e-CF Electronic Tax, Special Regime, Government, Final Consumer and Reimb. Transactions,,,,,,,EXV
DO,TEC,AR,CRD,TID,ALL,ARO,,TEF,,2,e-CF Electronic Tax, Special Regime, Government, Final Consumer and Reimb. Transactions,,,,,,,NON
DO,TEC,AR,CRD,TID,ALL,ARO,,TED,,2,e-CF Electronic Tax, Special Regime, Government, Final Consumer and Reimb. Transactions,,,,,,,NON
DO,TEC,AR,CRD,TID,ALL,ARO,,TEG,REG,2,e-CF Electronic Tax, Special Regime, Government, Final Consumer and Reimb. Transactions,,,,,,,NON
DO,TEC,AR,CRD,TID,ALL,ARO,,TED,REG,2,e-CF Electronic Tax, Special Regime, Government, Final Consumer and Reimb. Transactions,,,,,,,NON
DO,TEC,AR,CRD,TID,ALL,ARO,,TXI,RCS,2,e-CF Electronic Tax, Special Regime, Government, Final Consumer and Reimb. Transactions,,,,,,,NON
DO,TEC,AR,CRD,TID,ALL,ARO,,TCD,RCS,2,e-CF Electronic Tax, Special Regime, Government, Final Consumer and Reimb. Transactions,,,,,,,NON
DO,TEC,AR,CRD,TXX,ALL,ARO,,TXS,,2,e-CF Electronic Tax, Special Regime, Government, Final Consumer and Reimb. Transactions,,,,,,,EXV
DO,TEC,AR,CRD,TXX,ALL,ARO,,TCD,,2,e-CF Electronic Tax, Special Regime, Government, Final Consumer and Reimb. Transactions,,,,,,,EXV
DO,TEC,AR,CRD,TID,ALL,ARO,,TXF,,2,e-CF Electronic Tax, Special Regime, Government, Final Consumer and Reimb. Transactions,,,,,,,NON
DO,TEC,AR,CRD,TID,ALL,ARO,,TCD,,2,e-CF Electronic Tax, Special Regime, Government, Final Consumer and Reimb. Transactions,,,,,,,NON
DO,TEC,AR,CRD,TID,ALL,ARO,,TXG,REG,2,e-CF Electronic Tax, Special Regime, Government, Final Consumer and Reimb. Transactions,,,,,,,NON
DO,TEC,AR,CRD,TID,ALL,ARO,,TCD,REG,2,e-CF Electronic Tax, Special Regime, Government, Final Consumer and Reimb. Transactions,,,,,,,NON
DO,XCL,AR,INV,EXL,ALL,OTO,,,RCS,2,e-CF Electronic Tax, Special Regime, Government, Final Consumer and Reimb. Transactions,,,,,,,NON
DO,XCL,AR,CRD,EXL,ALL,OTO,,,RCS,2,e-CF Electronic Tax, Special Regime, Government, Final Consumer and Reimb. Transactions,,,,,,,NON
DO,XCL,AR,INV,EXL,ALL,ARO,,XCL,RCS,2,e-CF Electronic Tax, Special Regime, Government, Final Consumer and Reimb. Transactions,,,,,,,NON
DO,XCL,AR,CRD,EXL,ALL,ARO,,XCL,RCS,2,e-CF Electronic Tax, Special Regime, Government, Final Consumer and Reimb. Transactions,,,,,,,NON
DO,XCL,AR,INV,EXL,ALL,OTO,,,,2,e-CF Electronic Tax, Special Regime, Government, Final Consumer and Reimb. Transactions,,,,,,,EXV
DO,XCL,AR,CRD,EXL,ALL,OTO,,,,2,e-CF Electronic Tax, Special Regime, Government, Final Consumer and Reimb. Transactions,,,,,,,EXV
DO,XCL,AR,INV,EXL,ALL,ARO,,XCL,,2,e-CF Electronic Tax, Special Regime, Government, Final Consumer and Reimb. Transactions,,,,,,,EXV
DO,XCL,AR,CRD,EXL,ALL,ARO,,XCL,,2,e-CF Electronic Tax, Special Regime, Government, Final Consumer and Reimb. Transactions,,,,,,,EXV
DO,XCL,AR,INV,EXL,ALL,OTO,,,,2,e-CF Electronic Tax, Special Regime, Government, Final Consumer and Reimb. Transactions,,,,,,,NON
DO,XCL,AR,CRD,EXL,ALL,OTO,,,,2,e-CF Electronic Tax, Special Regime, Government, Final Consumer and Reimb. Transactions,,,,,,,NON
DO,XCL,AR,INV,EXL,ALL,ARO,,XCL,,2,e-CF Electronic Tax, Special Regime, Government, Final Consumer and Reimb. Transactions,,,,,,,NON
DO,XCL,AR,CRD,EXL,ALL,ARO,,XCL,,2,e-CF Electronic Tax, Special Regime, Government, Final Consumer and Reimb. Transactions,,,,,,,NON
DO,XCL,AR,INV,EXL,ALL,OTO,,,REG,2,e-CF Electronic Tax, Special Regime, Government, Final Consumer and Reimb. Transactions,,,,,,,NON
DO,XCL,AR,CRD,EXL,ALL,OTO,,,REG,2,e-CF Electronic Tax, Special Regime, Government, Final Consumer and Reimb. Transactions,,,,,,,NON
DO,XCL,AR,INV,EXL,ALL,ARO,,XCL,REG,2,e-CF Electronic Tax, Special Regime, Government, Final Consumer and Reimb. Transactions,,,,,,,NON
DO,XCL,AR,CRD,EXL,ALL,ARO,,XCL,REG,2,e-CF Electronic Tax, Special Regime, Government, Final Consumer and Reimb. Transactions,,,,,,,NON
DO,XCL,AP,INV,EXL,ALL,OTO,,,,2,e-CF Electronic Tax, Special Regime, Government, Final Consumer and Reimb. Transactions,,,,,,,
DO,XCL,AP,CRD,EXL,ALL,OTO,,,,2,e-CF Electronic Tax, Special Regime, Government, Final Consumer and Reimb. Transactions,,,,,,,
DO,XCL,AP,INV,EXL,ALL,ARO,,XCL,,2,e-CF Electronic Tax, Special Regime, Government, Final Consumer and Reimb. Transactions,,,,,,,
DO,XCL,AP,CRD,EXL,ALL,ARO,,XCL,,2,e-CF Electronic Tax, Special Regime, Government, Final Consumer and Reimb. Transactions,,,,,,,
";
		protected override bool? ExpectedDefaultValueForDisplayRecipientTaxIDRegistry => true;

		protected override string ExpectedAccTransactionHeaderAuthorisationRecordType => AccTransactionHeaderAuthorisationRecordTypes.DominicanRepublic;

		#region OrgCusCodes

		public void TestComplianceSubTypeAttributionRuleSetDefaultValue()
		{
			var ruleSetProvider = CountryComplianceFactory.GetIComplianceSubTypeRulesWithMultipleRuleSetProvider(CountryCode);

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCode))
			{
				AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
				AssertEquals(DominicanRepublicComplianceInfo.RuleSetCodes.TaxDocumentsAndExcludedSupply, ruleSetProvider.GetDefaultRuleSet());

				AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
				AssertEquals(DominicanRepublicComplianceInfo.RuleSetCodes.ElectronicTaxsAndReimbTransaction, ruleSetProvider.GetDefaultRuleSet());
			}

			var ruleSet = ruleSetProvider.GetRuleSet();

			AssertEquals(2, ruleSet.Count);
			Assert(ruleSet.ContainsCode(DominicanRepublicComplianceInfo.RuleSetCodes.TaxDocumentsAndExcludedSupply));
			Assert(ruleSet.ContainsCode(DominicanRepublicComplianceInfo.RuleSetCodes.ElectronicTaxsAndReimbTransaction));
		}

		public void TestIsOrganizationIsTaxRegistrationTypeRuleApplicable()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var rule = new ComplianceSubTypeAttributionRuleConfiguration();

			var builder = new DominicanRepublicComplianceInfo() as IComplianceSubTypeTaxRegistrationTypeRuleProvider;

			var orgCusCodes = from field in typeof(DominicanRepublicOrgCusCodeInfo.OrgCusCodes).GetFields(BindingFlags.Static | BindingFlags.Public)
							  let codes = (string)field.GetValue(null)
							  select codes;

			var taxRegistrationTypeCodes = from field in typeof(TaxRegistrationTypeCodes).GetFields(BindingFlags.Static | BindingFlags.Public)
										   let codes = (string)field.GetValue(null)
										   select codes;
			taxRegistrationTypeCodes = taxRegistrationTypeCodes.Concat(new[] { string.Empty });

			foreach (var code in orgCusCodes)
			{
				orgHeader.CustomsCodes.RemoveAll();
				orgHeader.CustomsCodes.AddNew(code, "1111111", CountryCode);

				foreach (var taxRegistrationTypeCode in taxRegistrationTypeCodes)
				{
					rule.TaxRegistrationType = taxRegistrationTypeCode;
					var result = builder.IsTaxRegistrationTypeRuleApplicable(rule, orgHeader);

					if ((taxRegistrationTypeCode.IsNullOrEmpty() && (code != DominicanRepublicOrgCusCodeInfo.OrgCusCodes.REG || code != DominicanRepublicOrgCusCodeInfo.OrgCusCodes.RCS)) ||
						(taxRegistrationTypeCode == DominicanRepublicComplianceInfo.TaxRegistrationTypeCodes.GovernmentOrganizations && code == DominicanRepublicOrgCusCodeInfo.OrgCusCodes.REG) ||
						(taxRegistrationTypeCode == DominicanRepublicComplianceInfo.TaxRegistrationTypeCodes.CommonSimplifiedRegime && code == DominicanRepublicOrgCusCodeInfo.OrgCusCodes.RCS)
						)
					{
						Assert($"the tax registration type {taxRegistrationTypeCode} is applicable when its orgHeader code is {code}", result);
					}
					else
					{
						Assert($"the tax registration type {taxRegistrationTypeCode} is not applicable when its orgHeader code is {code}", !result);
					}
				}
			}
		}

		#endregion

		public void TestGetTaxRegistrationTypeCodesExistAndHaveDescription()
		{
			var list = CountryComplianceFactory.GetIComplianceSubTypeAdditionalTaxRegistrationTypeListProvider(CountryCode)?.GetTaxRegistrationTypeList();
			AssertEquals("DominicanRepublicComplianceInfo.GetTaxRegistrationTypeList", 2, list.Count);

			AssertDescription(DominicanRepublicComplianceInfo.TaxRegistrationTypeCodes.GovernmentOrganizations, DominicanRepublicComplianceInfo.TaxRegistrationTypeDescriptions.GovernmentOrganizations);
			AssertDescription(DominicanRepublicComplianceInfo.TaxRegistrationTypeCodes.CommonSimplifiedRegime, DominicanRepublicComplianceInfo.TaxRegistrationTypeDescriptions.CommonSimplifiedRegime);

			void AssertDescription(string code, string expectedDescription)
			{
				AssertEquals("Description", expectedDescription, list.GetDescriptionFromCode(code));
			}
		}

		#region ICountryComplianceElectronicInvoiceEligibleSubType

		protected override string[] ExpectedEInvoiceEligibleComplianceSubType => new[] {
			DominicanRepublicComplianceInfo.ComplianceSubTypeCodes.TEI,
			DominicanRepublicComplianceInfo.ComplianceSubTypeCodes.TEF,
			DominicanRepublicComplianceInfo.ComplianceSubTypeCodes.TEC,
			DominicanRepublicComplianceInfo.ComplianceSubTypeCodes.TED,
			DominicanRepublicComplianceInfo.ComplianceSubTypeCodes.TES,
			DominicanRepublicComplianceInfo.ComplianceSubTypeCodes.TEG,
		};

		#endregion

		#region IComplianceRegistryDefaultProvider

		protected override string ExpectedDefaultValueForComplianceDocumentNumberAllocation_ReceivablesRegistry_EInvoicingEnabled
			=> "PST";

		#endregion
	}
}
