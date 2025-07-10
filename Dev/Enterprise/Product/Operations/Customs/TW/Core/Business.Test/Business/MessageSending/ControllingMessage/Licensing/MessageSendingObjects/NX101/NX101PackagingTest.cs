using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class NX101PackagingTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestData()
		{
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(packaging.MarksNumbers.ToString(), NUnit.Framework.Is.Null.Or.Empty, "MarksNumbers - should be [null] or [empty]");
				NUnit.Framework.Assert.That(packaging.PackagingMaterialDescription.ToString(), NUnit.Framework.Is.Null.Or.Empty, "PackagingMaterialDescription - should be [null] or [empty]");
				NUnit.Framework.Assert.That(packaging.Combination.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Combination - should be [null] or [empty]");
				NUnit.Framework.Assert.That(packaging.TypeCode.ToString(), NUnit.Framework.Is.Null.Or.Empty, "TypeCode - should be [null] or [empty]");
				NUnit.Framework.Assert.That(packaging.QuantityQuantity, NUnit.Framework.Is.EqualTo(ZDecimal.Zero), "QuantityQuantity");
			});
		}

		[ExpectNoExceptions]
		public void TestMarksNumbers()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var entryInstruction = declaration.CusEntryInstruction;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			header = entryInstruction.ControllingMessageHeaders.AddNew();
			header.TW1_CertificateType = CertificateTypeList.Codes.Code1;
			packaging = new NX101Packaging(header);
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(packaging.MarksNumbers, NUnit.Framework.Is.EqualTo(Core.Constants.ContainerMarking.NoMarks).Using(CustomComparers.TypeComparison), "when TW1_CertificateType is not 15, get fom EntryHeader.MarksAndNumbers");
				header.TW1_CertificateType = CertificateTypeList.Codes.Code15;
				NUnit.Framework.Assert.That(packaging.MarksNumbers.ToString(), NUnit.Framework.Is.Null.Or.Empty, "do not output when TW1_CertificateType is 15 - should be [null] or [empty]");
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			header = declaration.CusEntryInstruction.ControllingMessageHeaders.AddNew();
			packaging = new NX101Packaging(header);
		}

		CusTWControllingMessageHeader header;
		NX101Packaging packaging;
	}
}
