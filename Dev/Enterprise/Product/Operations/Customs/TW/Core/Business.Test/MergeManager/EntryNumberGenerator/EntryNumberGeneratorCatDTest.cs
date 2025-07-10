using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(EntryNumberGeneratorCatD))]
	sealed class EntryNumberGeneratorCatDTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestEntryNumberCategoryD()
		{
			NUnit.Framework.Assert.That(entryNumberGeneratorForTest.CategoryExposed, NUnit.Framework.Is.EqualTo(RangeTypeList.Codes.D).Using(CustomComparers.TypeComparison));
		}

		protected override void SetUp()
		{
			base.SetUp();
			header = (IEntryNumberGeneratorProvider)Factory.New<Integration.Customs.ASYCUDA.TWBriefCustomsDeclaration.IAsycudaManifestHeader>();
			entryNumberGeneratorForTest = new EntryNumberGeneratorCatDForTest(header);
		}

		[ExpectNoExceptions]
		public void TestAllowedGeneratorDescription()
		{
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(entryNumberGeneratorForTest.AllowedGeneratorDescription, NUnit.Framework.Is.EqualTo(ZString.Empty));
				((BusinessObject)header).RunPreSaveValidation();
				NUnit.Framework.Assert.That(entryNumberGeneratorForTest.AllowedGeneratorDescription, NUnit.Framework.Is.EqualTo(@"Customs Office:You have not entered a Customs Office.
Box Number:You have not entered a Box Number.").Using(CustomComparers.TypeComparison));
			});
		}

		IEntryNumberGeneratorProvider header;
		EntryNumberGeneratorCatDForTest entryNumberGeneratorForTest;

		class EntryNumberGeneratorCatDForTest : EntryNumberGeneratorCatD
		{
			public EntryNumberGeneratorCatDForTest(IEntryNumberGeneratorProvider provider) : base(provider)
			{
			}

			public ZString CategoryExposed => Category;
		}
	}
}
