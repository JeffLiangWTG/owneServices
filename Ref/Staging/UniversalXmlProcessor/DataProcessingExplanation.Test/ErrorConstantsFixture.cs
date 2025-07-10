using System.Reflection;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.DataProcessingExplanation.Test
{
	[TestFixture]
	class ErrorConstantsFixture
	{
		[Test]
		public void GetErrorReferenceUrl_DefaultValue()
		{
			Assert.AreEqual("https://devops.wisetechglobal.com/wtg/RefDataRepo/_wiki/wikis/RefDataRepo.wiki/11393/How-to-resolve-issues", ErrorConstants.ErrorReferenceUrl);
		}

		[Test]
		public void GetErrorReferenceUrl_OverWriteValue()
		{
			ErrorConstants.AddJsonFile("CargoWise.RefDbRepo.DataProcessingExplanation.Test.config.json");
			Assert.AreEqual("https://devops.wisetechglobal.com/wtg/RefDataRepo/_wiki/wikis", ErrorConstants.ErrorReferenceUrl);
		}

		[Test]
		public void GetErrorNameByCode()
		{
			foreach(var fieldInfo in typeof(ErrorCodes).GetFields(BindingFlags.Public | BindingFlags.Static))
			{
				Assert.AreEqual(fieldInfo.Name, ErrorConstants.GetErrorNameByCode((string)fieldInfo.GetValue(null)));
			}
		}
	}
}
