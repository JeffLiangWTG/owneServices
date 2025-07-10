using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Integration.Compliance;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.CountryCompliance.CountryComplianceInfoDisplay.Testing
{
	[TestedType(typeof(ComplianceSubTypeDisplay))]
	sealed class ComplianceSubTypeDisplayTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new ComplianceSubTypeDisplay("AU",
				new ComplianceSubType("XX",
					() => (NoResString)"Desc",
					() => "Local Desc",
					() => "Internal Note",
					LedgerOfUse.ALL,
					"01",
					TransactionTypeOfUse.ALL
					)
				);
		}

		public void TestNewComplianceSubTypeDisplay()
		{
			ComplianceSubType complianceSubType;

			var data1 = new { Code = "TXA", Description = ResString.GetMultilingualString("ARComplianceSubTypeCodeList|TXA", "01 Type A Invoice"), LocalDescription = "FACTURA TIPO A", InternalImplemenationNote = "InternalImplemenationNote", TaxStatusCode = "01" };
			complianceSubType = new ComplianceSubType(data1.Code, () => data1.Description, () => data1.LocalDescription, () => data1.InternalImplemenationNote, taxStatusCode: data1.TaxStatusCode);
			AssertTestNewComplianceSubTypeDisplay(Core.Constants.CountryCodes.Argentina, data1.Code, data1.Description, data1.LocalDescription, data1.InternalImplemenationNote, nameof(LedgerOfUse.ALL), complianceSubType, data1.TaxStatusCode, nameof(TransactionTypeOfUse.ALL));

			var data2 = new { Code = "TXG", Description = ResString.GetMultilingualString("DOComplianceSubTypeCodeList|TXG", "Government Recipient Invoice"), LocalDescription = "Comprobante Gubernamental", InternalImplemenationNote = "InternalImplemenationNote 1", LedgerOfUse = LedgerOfUse.AR, TaxStatusCode = "02", TransactionTypeOfUse = TransactionTypeOfUse.CRD };
			complianceSubType = new ComplianceSubType(data2.Code, () => data2.Description, () => data2.LocalDescription, () => data2.InternalImplemenationNote, data2.LedgerOfUse, data2.TaxStatusCode, data2.TransactionTypeOfUse);
			AssertTestNewComplianceSubTypeDisplay(Core.Constants.CountryCodes.DominicanRepublic, data2.Code, data2.Description, data2.LocalDescription, data2.InternalImplemenationNote, data2.LedgerOfUse.ToString(), complianceSubType, data2.TaxStatusCode, data2.TransactionTypeOfUse.ToString());

			var data3 = new { Code = "TXV", Description = ResString.GetMultilingualString("ECComplianceSubTypeCodeList|TXV", "Purchase Tax Voucher"), LocalDescription = "Liquidaci\u00f3n de Compras de Bienes y Prestaci\u00f3n de Servicios", InternalImplemenationNote = "InternalImplemenationNote 2", LedgerOfUse = LedgerOfUse.AP, TaxStatusCode = "03", TransactionTypeOfUse = TransactionTypeOfUse.INV };
			complianceSubType = new ComplianceSubType(data3.Code, () => data3.Description, () => data3.LocalDescription, () => data3.InternalImplemenationNote, data3.LedgerOfUse, data3.TaxStatusCode, data3.TransactionTypeOfUse);
			AssertTestNewComplianceSubTypeDisplay(Core.Constants.CountryCodes.Ecuador, data3.Code, data3.Description, data3.LocalDescription, data3.InternalImplemenationNote, data3.LedgerOfUse.ToString(), complianceSubType, data3.TaxStatusCode, data3.TransactionTypeOfUse.ToString());

			var data4 = new { Code = "TXB", Description = ResString.GetMultilingualString("ARComplianceSubTypeCodeList|TXB", "06 Type B Invoice"), LocalDescription = "FACTURA TIPO B", InternalImplemenationNote = "InternalImplemenationNote 3", LedgerOfUse = LedgerOfUse.ALL, TaxStatusCode = "04", TransactionTypeOfUse = TransactionTypeOfUse.ALL };
			complianceSubType = new ComplianceSubType(data4.Code, () => data4.Description, () => data4.LocalDescription, () => data4.InternalImplemenationNote, data4.LedgerOfUse, data4.TaxStatusCode, data4.TransactionTypeOfUse);
			AssertTestNewComplianceSubTypeDisplay(Core.Constants.CountryCodes.Argentina, data4.Code, data4.Description, data4.LocalDescription, data4.InternalImplemenationNote, data4.LedgerOfUse.ToString(), complianceSubType, data4.TaxStatusCode, data4.TransactionTypeOfUse.ToString());

			void AssertTestNewComplianceSubTypeDisplay(ZString countryCodeExpected, ZString codeExpected, IMultilingualString descriptionExpected, ZString localDescriptionExpected, ZString internalImplemenationNoteExpected, ZString ledgerOfUseExpected, ComplianceSubType complianceSubTypeExpected, ZString taxStatusCodeExpected, ZString transactionTypeOfUseExpected)
			{
				var complianceSubTypeDisplay = new ComplianceSubTypeDisplay(countryCodeExpected, complianceSubTypeExpected);

				AssertEquals("complianceSubTypeDisplay.CountryCode", countryCodeExpected, complianceSubTypeDisplay.CountryCode);
				AssertEquals("complianceSubTypeDisplay.Code", codeExpected, complianceSubTypeDisplay.Code);
				AssertEquals("complianceSubTypeDisplay.Description", descriptionExpected, complianceSubTypeDisplay.Description);
				AssertEquals("complianceSubTypeDisplay.LocalDescription", localDescriptionExpected, complianceSubTypeDisplay.LocalDescription);
				AssertEquals("complianceSubTypeDisplay.InternalImplemenationNote", internalImplemenationNoteExpected, complianceSubTypeDisplay.InternalImplemenationNote);
				AssertEquals("complianceSubTypeDisplay.LedgerOfUse", ledgerOfUseExpected, complianceSubTypeDisplay.LedgerOfUse);
				AssertEquals("complianceSubTypeDisplay.TransactionTypeOfUse", transactionTypeOfUseExpected, complianceSubTypeDisplay.TransactionTypeOfUse);
				AssertEquals("complianceSubTypeDisplay.TaxInvoiceCode", taxStatusCodeExpected, complianceSubTypeDisplay.TaxStatusCode);
			}
		}
	}
}
