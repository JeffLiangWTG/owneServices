using System.Linq;
using NUnit.Framework;
using NUnit.Framework.Internal;

namespace CargoWise.RefDbRepo.MXRefLocoMap.Business.Test
{
	[TestFixture]
	public class RefLocoMapTest
	{
		[Test]
		public void ToCodeListsShouldReturnOneItem()
		{
			var refLocoMap = new CargoWise.RefDbRepo.MXRefLocoMap.Business.RefLocoMap
			{
				RY_LocalPortCode = "5300",
				RY_RL_NKLocoPort = "MXG6H",
				RY_SystemUsage = "CUS",
				RY_RN_NKCountryCode = "MX"
			};
			var codeLists = refLocoMap.ToCodeListLocoMap().ToList();

			Assert.AreEqual(1, codeLists.Count);

			Assert.AreEqual("5300", codeLists[0].RY_LocalPortCode);
			Assert.AreEqual("MXG6H", codeLists[0].RY_RL_NKLocoPort);
			Assert.AreEqual("CUS", codeLists[0].RY_SystemUsage);
			Assert.AreEqual("MX", codeLists[0].RY_RN_NKCountryCode);
		}

		[Test]
		public void BackupMapper()
		{
			Assert.IsFalse(RefLocoMap.BackupMapper(null, null), "When excelHeader & propertyName are null");
			Assert.IsFalse(RefLocoMap.BackupMapper("", ""), "When excelHeader & propertyName are empty");
			Assert.IsFalse(RefLocoMap.BackupMapper("RY_RN", "RY_RN_NKCountryCo"), "When excelHeader is empty & propertyName is not valid");
			Assert.IsFalse(RefLocoMap.BackupMapper("", "RY_RN_NKCountryCode"), "When excelHeader is empty & propertyName is valid");
			Assert.IsFalse(RefLocoMap.BackupMapper(null, "RY_RN_NKCountryCode"), "When excelHeader is null & propertyName is valid");
			Assert.IsFalse(RefLocoMap.BackupMapper("InvalidExcelHeader", "RY_RN_NKCountryCode"), "When excelHeader not start with RY_RN & propertyName is valid");
			Assert.IsTrue(RefLocoMap.BackupMapper("RY_RN", "RY_RN_NKCountryCode"), "When excelHeader start with RY_RN & propertyName is valid");
		}
	}
}
