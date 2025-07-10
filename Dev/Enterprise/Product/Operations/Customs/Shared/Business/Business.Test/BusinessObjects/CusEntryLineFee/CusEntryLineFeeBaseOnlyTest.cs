using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Business.Testing
{
	sealed class CusEntryLineFeeBaseOnlyTest : CargoWise.EntityFramework.Testing.TestCaseWithFactory
	{
		public void TestIsConfirmed()
		{
			CusEntryLineFee lineFee = Factory.New<CusEntryLineFee>();
			lineFee.CF_Source = CusEntryLineFeeSourceCodeList.Codes.CUS;
			Assert(lineFee.IsConfirmed);
			Assert(!lineFee.SupportsClone());

			lineFee.CF_Source = CusEntryLineFeeSourceCodeList.Codes.CW1;
			Assert(!lineFee.IsConfirmed);
		}

		public void TestDeletedFromFactoryWhenValueIsSetToZeroAfterBeingSaved()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entry.MergedLines.AddNew();
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_CU_RelatedHouseBill = ZGuid.Empty;
			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;

			var fee = entryLine.Fees.AddNew();
			fee.EntryLine.CL_LineNumber = 123;
			fee.CF_ChargeAmount = 123;
			Factory.Save();

			AssertNotNull("Precondition : Exists in DB", new BusinessObjectFactory().Load<CusEntryLineFee>(fee.PK));

			fee.CF_ChargeAmount = 0;
			Factory.Save();

			AssertNull("Should not exist in DB", new BusinessObjectFactory().Load<CusEntryLineFee>(fee.PK));
		}

		public void TestIsNotSavedWhenValueIsZero()
		{
			var lineFee = Factory.New<CusEntryLineFee>();
			Factory.Save();

			AssertNull("Should not exist in DB", new BusinessObjectFactory().Load<CusEntryLineFee>(lineFee.PK));
		}

		public void TestIsSavedWhenValueIsZeroButNotDeletingZeroLines()
		{
			var entryLine = Factory.NewWithValidTestData<CusEntryLine>();
			var lineFee = Factory.New<CusEntryLineFee_ForTesting>();
			lineFee.CF_CL = entryLine.PK;
			Factory.Save();

			AssertNotNull("Should exist in DB", new BusinessObjectFactory().Load<CusEntryLineFee>(lineFee.PK));
		}

		public void TestCF_BaseValueDecimalPlacesAttribute()
		{
			AssertHasCustomAttribute<DecimalPlacesAttribute>(typeof(CusEntryLineFee), nameof(CusEntryLineFee.CF_BaseValue), true, x => x.DecimalPlacesMember == nameof(CusEntryLineFee.CF_BaseValueDecimalPlaces));
		}

		public void TestITypeDeciderContext()
		{
			var nzCompany = Factory.New<GlbCompany>();
			nzCompany.GC_Code = "CNZ";
			nzCompany.GC_Name = "NZ Company";
			nzCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.NewZealand;
			var nzBranch = nzCompany.Branches.AddNew();
			nzBranch.GB_Code = "BNZ";

			CombineAssertions(() =>
			{
				AssertEquals("From CurrentCompany", "ER", (Factory.New<CusEntryLineFee>() as ITypeDeciderContext).Country);

				var declaration = (BaseJobDeclaration)Factory.New<Integration.Customs.NZ.IJobDeclaration>();
				declaration.JE_GB = nzBranch.PK;
				var entry = declaration.ActiveEntryHeaders.AddNew();
				var entryLine = entry.MergedLines.AddNew();
				var fee = entryLine.Fees.AddNew();
				AssertEquals("From EntryLine", "NZ", (fee as ITypeDeciderContext).Country);
			});
		}

		class CusEntryLineFee_ForTesting : CusEntryLineFee
		{
			public CusEntryLineFee_ForTesting(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
			{
			}

			protected override bool ShouldDeleteIfChargeAmountIsZero => false;
		}
	}
}
