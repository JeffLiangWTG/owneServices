using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.Business.Testing
{
	[TestedType(typeof(ModuleCusEntryPayInfoCollection))]
	sealed class ModuleCusEntryPayInfoCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestModuleCusEntryPayInfoCollection()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			var instruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			instruction.CEI_Style = UniversalReferenceConstants.ProcedureCodes._11;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = instruction.PK;
			invoiceLine.JI_Procedure = invoiceLine.EntryInstruction.CEI_Style + UniversalReferenceConstants.ProcedureCodes._00;
			Factory.Save();
			new LineMerger(declaration).DoMerge();
			var header = declaration.ActiveEntryHeaders[0];
			Factory.Save();
			TestCaseHelper.ClearTable(CusEntryPayInfo.Schema.TableName);
			var payInfo1 = Factory.New<CusEntryPayInfo>();
			payInfo1.C9_TransactionType = UniversalReferenceConstants.TaxOrFeeTypeCode.VAT;
			payInfo1.C9_PaymentAmount = new ZDecimal(12.1);
			payInfo1.C9_PaymentDate = ZDateTime.Today;
			payInfo1.C9_CH = header.PK;
			var payInfo2 = Factory.New<CusEntryPayInfo>();
			payInfo2.C9_TransactionType = UniversalReferenceConstants.TaxOrFeeTypeCode.VAT;
			payInfo2.C9_PaymentAmount = new ZDecimal(7.3);
			payInfo2.C9_CH = header.PK;
			var payInfo3 = Factory.New<CusEntryPayInfo>();
			payInfo3.C9_TransactionType = "AAA";
			payInfo3.C9_PaymentDate = ZDateTime.Today;
			payInfo3.C9_PaymentAmount = new ZDecimal(7.3);
			payInfo3.C9_CH = header.PK;
			var payInfo4 = Factory.New<CusEntryPayInfo>();
			payInfo4.C9_TransactionType = "AAA";
			payInfo4.C9_PaymentAmount = new ZDecimal(7.3);
			payInfo4.C9_CH = header.PK;
			var payInfo5 = Factory.New<CusEntryPayInfo>();
			payInfo5.C9_TransactionType = UniversalReferenceConstants.TaxOrFeeTypeCode.VAT;
			payInfo5.C9_PaymentAmount = new ZDecimal(12.1);
			payInfo5.C9_PaymentDate = ZDateTime.Today;
			payInfo5.C9_PaymentStatus = CusEntryPayInfoStatusList.Codes.Clear;
			payInfo5.C9_CH = header.PK;
			var payInfo6 = Factory.New<CusEntryPayInfo>();
			payInfo6.C9_TransactionType = UniversalReferenceConstants.TaxOrFeeTypeCode.VAT;
			payInfo6.C9_PaymentAmount = new ZDecimal(12.1);
			payInfo6.C9_PaymentDate = ZDateTime.Today;
			payInfo6.C9_PaymentStatus = CusEntryPayInfoStatusList.Codes.Pending;
			payInfo6.C9_CH = header.PK;
			Factory.Save();
			var collection = new ModuleCusEntryPayInfoCollection(Factory);
			collection.Load();
			AssertEquals(1, collection.Count);
			AssertContainsExactElementsInAnyOrder(new List<CusEntryPayInfo> { payInfo5 }, collection);
			using (GlbCompany.TemporaryLoginInNewCompanyForCountry(Core.Constants.CountryCodes.SouthAfrica))
			{
				collection = new ModuleCusEntryPayInfoCollection(Factory);
				collection.Load();
				AssertEquals(0, collection.Count);
			}
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new ModuleCusEntryPayInfoCollection(Factory);
		}
	}
}
