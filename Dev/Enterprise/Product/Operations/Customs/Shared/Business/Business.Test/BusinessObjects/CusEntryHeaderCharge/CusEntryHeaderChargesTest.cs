using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business.ClusterKey;
using Enterprise.ZArchitecture.Business.ClusterKey.Testing;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(CusEntryHeaderCharges))]
	class CusEntryHeaderChargesTest : EnterpriseBusinessObjectTestCase
	{
		public void TestCheckChargeTypeUniqueness()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var entry = declaration.ActiveEntryHeaders.AddNew();
			var charge1 = entry.Charges.AddNew("A00");
			var charge2 = entry.Charges.AddNew("B00");
			charge2.C1_IsLandedCostOnly = true;
			var charge3 = entry.ConfirmedCharges.AddOrUpdate("C00");

			CombineAssertions(() =>
			{
				var charge4 = entry.Charges.AddNew();
				charge4.C1_ChargeType = "A00";
				AssertEquals("A00 is not unique in this CusEntryHeaderChargesCollection, but there is no report for Charges.", ZString.Empty, ErrorReporter.LastMessageReported);

				charge4.C1_ChargeType = "B00";
				AssertEquals("Can have LandedCostOnly charges with the same type", ZString.Empty, ErrorReporter.LastMessageReported);

				charge4.C1_ChargeType = "C00";
				AssertEquals("C00: no existed C00 Charges so no error", ZString.Empty, ErrorReporter.LastMessageReported);

				var charge5 = entry.ConfirmedCharges.AddNew();
				charge5.C1_ChargeType = "A00";
				AssertEquals("A00: no existed A00 Confirmed Charges so no error", ZString.Empty, ErrorReporter.LastMessageReported);

				charge5.C1_ChargeType = "B00";
				AssertEquals("B00: no existed A00 Confirmed Charges so no error", ZString.Empty, ErrorReporter.LastMessageReported);

				charge5.C1_ChargeType = "C00";
				AssertContains("C00 is not unique in this ConfirmedCusEntryHeaderChargesCollection.", ErrorReporter.LastMessageReported);
				ErrorReporter.Clear();
			});
		}

		public void TestCheckChargeTypeUniqueness_IsImportingData()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.IsImportingData = true;
			var entry = declaration.ActiveEntryHeaders.AddNew();
			var charge1 = entry.Charges.AddNew("A00");
			var charge2 = entry.Charges.AddNew("B00");
			charge2.C1_IsLandedCostOnly = true;
			var charge3 = entry.ConfirmedCharges.AddOrUpdate("C00");

			CombineAssertions(() =>
			{
				var charge4 = entry.Charges.AddNew();
				AssertExceptionThrown<DataObjectReadFailureException>("exception", "The charge code A00 is not unique in this CusEntryHeaderChargesCollection.", () => { charge4.C1_ChargeType = "A00"; });
				AssertNoExceptionThrown("Can have LandedCostOnly charges with the same type", () => { charge4.C1_ChargeType = "B00"; });
				AssertNoExceptionThrown("no existed C00 Charges so no error", () => { charge4.C1_ChargeType = "C00"; });

				var charge5 = entry.ConfirmedCharges.AddNew();
				AssertNoExceptionThrown("A00: no existed A00 Confirmed Charges so no error", () => { charge5.C1_ChargeType = "A00"; });
				AssertNoExceptionThrown("B00: no existed B00 Confirmed Charges so no error", () => { charge5.C1_ChargeType = "B00"; });
				AssertExceptionThrown<DataObjectReadFailureException>("The charge code C00 is not unique in this ConfirmedCusEntryHeaderChargesCollection.", () => { charge5.C1_ChargeType = "C00"; });
			});
		}

		public void TestIsConfirmed()
		{
			var charge = Factory.New<CusEntryHeaderCharges>();
			charge.C1_Source = CusEntryLineFeeSourceCodeList.Codes.CUS;
			AssertEquals("ConfirmedCharge", true, charge.IsConfirmed);

			charge.C1_Source = CusEntryLineFeeSourceCodeList.Codes.CW1;
			AssertEquals("CW1 Charge", false, charge.IsConfirmed);
		}

		public void TestSupportsClone()
		{
			var charge = Factory.New<CusEntryHeaderCharges>();
			charge.C1_Source = CusEntryLineFeeSourceCodeList.Codes.CUS;
			AssertEquals("ConfirmedCharge", false, charge.SupportsClone());

			charge.C1_Source = CusEntryLineFeeSourceCodeList.Codes.CW1;
			AssertEquals("CW1 Charge: false by default in base", false, charge.SupportsClone());
		}

		public void TestShouldResetToZero()
		{
			var charge = (CusEntryHeaderCharges)GetNewBusinessObject();
			AssertEquals("Should reset to zero on merging", true, charge.ShouldResetDataOnMerging);
		}

		public void TestDeleteIfAmountIsEmpty()
		{
			var charge = (CusEntryHeaderCharges)GetNewBusinessObject();
			var charge2 = (CusEntryHeaderCharges)GetNewBusinessObject();
			charge2.C1_ChargeAmount = 10m;

			Factory.Save();
			AssertEquals("charge with empty amount is Deleted", true, charge.IsDeleted);
			AssertEquals("charge with amount stays", false, charge2.IsDeleted);
		}

		public void TestEntryHeader()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var charge = entry.Charges.AddNew();
			AssertEquals("Entry Header not null", entry, charge.EntryHeader);
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
				AssertEquals("From CurrentCompany", "ER", (Factory.New<CusEntryHeaderCharges>() as ITypeDeciderContext).Country);

				var declaration = (BaseJobDeclaration)Factory.New<Integration.Customs.NZ.IJobDeclaration>();
				declaration.JE_GB = nzBranch.PK;
				var entryHeader = declaration.CustomsEntryHeaders.AddNew();
				var charge = entryHeader.Charges.AddNew();
				AssertEquals("From Declaration", "NZ", (charge as ITypeDeciderContext).Country);
			});
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return Factory.New<BaseJobDeclaration>().CustomsEntryHeaders.AddNew().Charges.AddNew();
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			CusEntryHeaderCharges charge = (CusEntryHeaderCharges)GetNewBusinessObject();
			charge.C1_ChargeAmount = 10m;
			return charge;
		}

		protected override BusinessObject GetBusinessObjectForFetchForLoad()
		{
			return GetNewBusinessObject();
		}

		public override List<ZString> GetExcludedColumns_OnlySomeSubclassesAreSetDefaultValues()
		{
			return new List<ZString>() { CusEntryHeaderCharges.Schema.C1_Source };
		}

		#endregion
	}

	[TestedType(typeof(CusEntryHeaderCharges))]
	class CusEntryHeaderChargesClusterKeyTest : ClusterKeyWorkerMandatoryTest
	{
		protected override IEnumerable<IClusterKeyWorker> PrepareDataAndGetExpectedClusterKeyChildren() => null;

		protected override IClusterKeyEntity NewClusterKeyEntity()
		{
			var entryHeader = (CusEntryHeader)NewParentObject();
			var charge = entryHeader.Charges.AddNew();
			charge.C1_ChargeAmount = 1;
			return charge;
		}

		protected override EnterpriseBusinessObject NewParentObject()
		{
			var dec = Factory.New<BaseJobDeclaration>();
			var entryHeader = Factory.New<CusEntryHeader>();
			entryHeader.CH_JE = dec.PK;
			return entryHeader;
		}
	}
}
