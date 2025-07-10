using System.Collections.Generic;
using System.IO;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.VNReferenceData.Business;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.VNReferenceData.Tests
{
	[TestFixture]
	public class ParseExcelDataFixture
	{
		[TestCaseSource(nameof(TestCases))]
		public void TestParseExcelData(string xlsxFile, List<RefCusCodeList> expectedList)
		{
			var testDir = TestContext.CurrentContext.TestDirectory;
			var xlsxFilePath = Path.Join(testDir, "VNTestFiles", "CustomsOffice", xlsxFile);
			var actualList = CustomsOfficeHelper.ParseExcelData(xlsxFilePath).GetAwaiter().GetResult();

			Assert.That(actualList.Count, Is.EqualTo(expectedList.Count));
			for (int i = 0; i < expectedList.Count; i++)
			{
				Assert.That(expectedList[i].ZZD_Code, Is.EqualTo(actualList[i].ZZD_Code));
				Assert.That(expectedList[i].ZZD_Description, Is.EqualTo(actualList[i].ZZD_Description));
			}
		}

		public static IEnumerable<TestCaseData> TestCases()
		{
			yield return new TestCaseData(
				"Input_DuplicatedRows.xlsx",
				new List<RefCusCodeList>
				{
					new() { ZZD_Code = "01B1", ZZD_Description = "Hải quan cửa khẩu sân bay quốc tế Nội Bài" },
					new() { ZZD_Code = "01B2", ZZD_Description = "Hải quan cửa khẩu sân bay quốc tế Nội Bài" },
					new() { ZZD_Code = "01M1", ZZD_Description = "Hải quan Hòa Lạc" },
					new() { ZZD_Code = "01PL", ZZD_Description = "Hải quan Hòa Lạc" },
					new() { ZZD_Code = "01PR", ZZD_Description = "Hải quan Vĩnh Phúc" },
					new() { ZZD_Code = "01PJ", ZZD_Description = "Hải quan Phú Thọ" },
				}
			).SetName("Remove duplicated rows");

			yield return new TestCaseData(
				"Input_EmptyAndUnmergedRows.xlsx",
				new List<RefCusCodeList>
				{
					new() { ZZD_Code = "01E1", ZZD_Description = "Hải quan Bắc Hà Nội" },
					new() { ZZD_Code = "01NV", ZZD_Description = "Hải quan Khu công nghiệp Bắc Thăng Long" },
					new() { ZZD_Code = "01SI", ZZD_Description = "Hải quan ga đường sắt quốc tế Yên Viên" },
					new() { ZZD_Code = "01IK", ZZD_Description = "Hải quan Gia Thụy" },
					new() { ZZD_Code = "01B1", ZZD_Description = "Hải quan cửa khẩu sân bay quốc tế Nội Bài" },
					new() { ZZD_Code = "02B1", ZZD_Description = "Hải quan cửa khẩu sân bay quốc tế Tân Sơn Nhất" },
					new() { ZZD_Code = "02DS", ZZD_Description = "Hải quan Chuyển phát nhanh" },
				}
			).SetName("Remove empty rows and unmerge rows");
		}
	}
}
