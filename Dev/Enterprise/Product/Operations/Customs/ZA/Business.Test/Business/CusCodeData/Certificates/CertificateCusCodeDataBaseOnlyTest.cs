using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.Business.Testing
{
	sealed class CertificateCusCodeDataBaseOnlyTest : TestCaseWithFactory
	{
		public void TestSetDefaultValues()
		{
			AssertEquals(CusEntryInstructionSchema.Constants.Prefix, certificateCusCodeDataTestClass.CY_ParentTableCode);
			AssertEquals("XXX", certificateCusCodeDataTestClass.CY_Type);
		}

		[TestDate(2017, 02, 09)]
		public void TestExpiryDate()
		{
			AssertEquals(ZDate.Today.AddDays(1), certificateCusCodeDataTestClass.ExpiryDate);
		}

		public void TestRemainingValue()
		{
			AssertEquals(1000m, certificateCusCodeDataTestClass.RemainingValue);
		}

		public void TestRemainingValueExcludingThisDeclaration()
		{
			AssertEquals(1000m, certificateCusCodeDataTestClass.RemainingValueExcludingThisDeclaration);

			var entry = declaration.ActiveEntryHeaders.AddNew();
			entry.CH_BGMReference = "12345";

			var helper = new PermitTestDataHelper(Factory);
			var transaction = helper.CreatePermitLineTransaction(permitHeader, entry, null, "CUM", "TRA", -200, 0, "CON");
			Factory.Save();
			AssertEquals(800m, certificateCusCodeDataTestClass.RemainingValueExcludingThisDeclaration);

			transaction.CPL_TransactionStatus = "DEL";
			Factory.Save();
			AssertEquals(1000m, certificateCusCodeDataTestClass.RemainingValueExcludingThisDeclaration);
		}

		public void TestPermitType()
		{
			AssertEquals("XXX", certificateCusCodeDataTestClass.PermitType);
			certificateCusCodeDataTestClass.CY_Code = "";
			AssertEquals("XXX", certificateCusCodeDataTestClass.PermitType);
		}

		protected override void SetUp()
		{
			base.SetUp();

			var importer = Factory.NewWithValidTestData<OrgHeader>();

			var helper = new PermitTestDataHelper(Factory);
			permitHeader = helper.CreatePermitHeader(importer.PK, "TEST", ZDate.Today, ZDate.Today.AddDays(1), PermitQtyValIndicatorList.Codes.VAL, "XXX", ZString.Empty, 0m, 1000m);
			Factory.Save();

			declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Importer = importer.PK;
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();

			certificateCusCodeDataTestClass = Factory.New<CertificateCusCodeDataTestClass>();
			certificateCusCodeDataTestClass.Parent = entryInstruction;
			certificateCusCodeDataTestClass.CY_Code = permitHeader.CPH_Number;
		}

		CertificateCusCodeDataTestClass certificateCusCodeDataTestClass;
		JobDeclaration declaration;
		CusPermitHeader permitHeader;
	}

	class CertificateCusCodeDataTestClass : CertificateCusCodeData
	{
		public CertificateCusCodeDataTestClass(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override ZString CusCodeDataTypeCode => "XXX";

		public new ZString PermitType => "XXX";

		protected override IEnumerable<CertificateCusCodeData> GetCertificateCollectionCore(CusEntryInstruction instruction) => instruction.RCCCertificates.OfType<CertificateCusCodeData>();

		protected override void RecalculateWhenAboutToBeDetachedOrDeleted(CusEntryInstruction instruction)
		{
			instruction?.RCCCertificateLineNumberGenerator.RecalculateWhenAboutToBeDetachedOrDeleted(this);
		}

		protected override void RecalculateWhenAdded()
		{
			EntryInstruction?.RCCCertificateLineNumberGenerator.RecalculateWhenAdded(this);
		}

		protected override void RecalculateWhenRenumbered(ZShort oldValue)
		{
			EntryInstruction?.RCCCertificateLineNumberGenerator.RecalculateWhenRenumbered(this, oldValue);
		}
	}
}
