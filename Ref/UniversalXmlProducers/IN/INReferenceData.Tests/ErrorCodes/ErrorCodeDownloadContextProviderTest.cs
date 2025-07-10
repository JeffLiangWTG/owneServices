using System;
using CargoWise.RefDbRepo.INReferenceData.Services;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.INReferenceData.Tests
{
	sealed class ErrorCodeDownloadContextProviderTest
	{
		[Test]
		public void TestGetContext()
		{
			Assert.Multiple(() =>
			{
				Assert.Throws<ArgumentException>(() => ContextProvider.GetContext((ErrorCodeType)10));
				AssertContext(ErrorCodeType.BE, AppConfig.ErrorCodes.BeUrl, AppConfig.ErrorCodes.BeXpath);
				AssertContext(ErrorCodeType.AirCgm, AppConfig.ErrorCodes.AirCgmUrl, AppConfig.ErrorCodes.AirCgmXpath);
				AssertContext(ErrorCodeType.SeaCgm, AppConfig.ErrorCodes.SeaCgmUrl, AppConfig.ErrorCodes.SeaCgmXpath);
			});

		}

		void AssertContext(ErrorCodeType codeType, string expectedUrl, string expectedXpath)
		{
			var context = ContextProvider.GetContext(codeType);
			Assert.NotNull(context, "Context is null");
			Assert.AreEqual(codeType, context.CodeType, "Error Code Type");
			Assert.AreEqual(expectedUrl, context.SubUrl, "Sub Url");
			Assert.AreEqual(expectedXpath, context.TableXpath, "Table XPath");
		}

		IErrorCodeDownloadContextProvider ContextProvider => contextProvider ?? (contextProvider = new ErrorCodeDownloadContextProvider());
		IErrorCodeDownloadContextProvider contextProvider;
	}
}
