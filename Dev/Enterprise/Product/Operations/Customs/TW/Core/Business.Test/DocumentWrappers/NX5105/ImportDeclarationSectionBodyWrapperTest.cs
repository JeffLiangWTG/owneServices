using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(ImportDeclarationSectionBodyWrapper))]
	sealed class ImportDeclarationSectionBodyWrapperTest : TestCase
	{
		[ExpectNoExceptions]
		public void TestBox35DescriptionOfGoods_LineWithLineBreaks()
		{
			var importDeclarationSectionBodyWrapper = new ImportDeclarationSectionBodyWrapper();
			importDeclarationSectionBodyWrapper.Box35[0] = "\"SC-RC5-0001-0\r\nRC5 TX_SINGLE LINK-ETHERNET HDBaseT\r\n400-1139 REV. A\"\r\n";
			importDeclarationSectionBodyWrapper.Box35[1] = "\"SC-RC5-0002-0\r\nRC5 TX_SINGLE LINK-ETHERNET HDBaseT\r\n400-1139 REV. A\"\r\n";
			importDeclarationSectionBodyWrapper.Box35[2] = "\"SC-RC5-0003-0\r\nRC5 TX_SINGLE LINK-ETHERNET HDBaseT\r\n400-1139 REV. A\"\r\n";
			importDeclarationSectionBodyWrapper.Box35[3] = "\"SC-RC5-0004-0\r\nRC5 TX_SINGLE LINK-ETHERNET HDBaseT\r\n400-1139 REV. A\"\r\n";
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(importDeclarationSectionBodyWrapper.Box35DescriptionOfGoods_Line1, NUnit.Framework.Is.EqualTo("\"SC-RC5-0001-0 RC5 TX_SINGLE LINK-ETHERNET HDBaseT 400-1139 REV. A\" ").Using(CustomComparers.TypeComparison), "Box35DescriptionOfGoods_Line1");
				NUnit.Framework.Assert.That(importDeclarationSectionBodyWrapper.Box35DescriptionOfGoods_Line2, NUnit.Framework.Is.EqualTo("\"SC-RC5-0002-0 RC5 TX_SINGLE LINK-ETHERNET HDBaseT 400-1139 REV. A\" ").Using(CustomComparers.TypeComparison), "Box35DescriptionOfGoods_Line2");
				NUnit.Framework.Assert.That(importDeclarationSectionBodyWrapper.Box35DescriptionOfGoods_Line3, NUnit.Framework.Is.EqualTo("\"SC-RC5-0003-0 RC5 TX_SINGLE LINK-ETHERNET HDBaseT 400-1139 REV. A\" ").Using(CustomComparers.TypeComparison), "Box35DescriptionOfGoods_Line3");
				NUnit.Framework.Assert.That(importDeclarationSectionBodyWrapper.Box35DescriptionOfGoods_Line4, NUnit.Framework.Is.EqualTo("\"SC-RC5-0004-0 RC5 TX_SINGLE LINK-ETHERNET HDBaseT 400-1139 REV. A\" ").Using(CustomComparers.TypeComparison), "Box35DescriptionOfGoods_Line4");
			});
		}
	}
}
