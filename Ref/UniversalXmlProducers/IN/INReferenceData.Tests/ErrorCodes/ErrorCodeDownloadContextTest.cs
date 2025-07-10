using CargoWise.RefDbRepo.INReferenceData.Services;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.INReferenceData.Tests
{
	sealed class ErrorCodeDownloadContextTest
	{
		[Test]
		public void Constructor_WithValidParameters_ShouldSetProperties()
		{
			const ErrorCodeType codeType = ErrorCodeType.BE;
			const string subUrl = "subUrl";
			const string tableXpath = "tableXpath";

			var context = new ErrorCodeDownloadContext(codeType, subUrl, tableXpath);

			Assert.AreEqual(codeType, context.CodeType);
			Assert.AreEqual(subUrl, context.SubUrl);
			Assert.AreEqual(tableXpath, context.TableXpath);
		}
	}
}
