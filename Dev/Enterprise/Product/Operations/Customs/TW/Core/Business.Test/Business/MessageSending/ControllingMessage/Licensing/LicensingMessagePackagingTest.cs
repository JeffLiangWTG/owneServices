using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(LicensingMessagePackaging))]
	sealed class LicensingMessagePackagingTest : TestCaseWithFactory
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
			declaration.CustomsEntryHeaders.AddNew().CH_CEI_Instruction = declaration.CusEntryInstruction.PK;
			packaging = new LicensingMessagePackaging(header);
			NUnit.Framework.Assert.That(packaging.MarksNumbers, NUnit.Framework.Is.EqualTo("N/M").Using(CustomComparers.TypeComparison), "MarksNumbers");
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.NewWithValidTestData<JobDeclaration>();
			header = declaration.CusEntryInstruction.ControllingMessageHeaders.AddNew();
			packaging = new LicensingMessagePackaging(header);
		}

		JobDeclaration declaration;
		CusTWControllingMessageHeader header;
		LicensingMessagePackaging packaging;
	}
}
