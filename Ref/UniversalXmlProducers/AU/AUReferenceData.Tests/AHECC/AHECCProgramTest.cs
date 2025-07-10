using System;
using System.IO;
using CargoWise.RefDbRepo.AUReferenceData.Business;
using CargoWise.RefDbRepo.AUReferenceData.CmdLine;
using CargoWise.RefDbRepo.AUReferenceData.Services;
using CargoWise.RefDbRepo.UniversalXMLProducers.Common;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.AUReferenceData.Tests.AHECC
{
	internal class AHECCProgramTest
	{
		[Test]
		public void TestRunAHECCFullUpdate()
		{
			using (var stringWriter = new StringWriter())
			{
				var originalError = Console.Error;
				try
				{
					Console.SetError(stringWriter);

					var program = new AHECCProgramForTest();
					program.RunFullUpdate();

					var output = stringWriter.ToString().Trim();
					StringAssert.Contains("Cannot find AHECCSS-P1-EEMAIN", output);
					StringAssert.Contains("Cannot find AHECCSS-Q1-EEMAIN", output);
					StringAssert.DoesNotContain("Cannot find AHECCSS-P1-EDCHNG", output);
				}
				finally
				{
					Console.SetError(originalError);
				}
			}
		}

		[Test]
		public void TestRunAHECCFullUpdateFailuresAreHandled()
		{
			var program = new AHECCProgramForTest();
			program.exceptionToThrow = new ApplicationException("Boo");
			var ex = Assert.Throws<AggregateException>(() => program.RunFullUpdate());
			Assert.AreEqual(2, ex.InnerExceptions.Count);
			Assert.AreEqual("One or more errors occurred. (Boo) (Boo)", ex.Message);
		}

		[Test]
		public void TestRunAHECCPartialUpdate()
		{
			using (var stringWriter = new StringWriter())
			{
				var originalError = Console.Error;
				try
				{
					Console.SetError(stringWriter);

					var mockDateTimeProvider = new Mock<IDateTimeProvider>();
					mockDateTimeProvider.Setup(x => x.CurrentLocalDateTime).Returns(new DateTime(2024, 07, 28, 21, 38, 46));

					var program = new AHECCProgramForTest();
					program.DateTimeProviderForTest = mockDateTimeProvider.Object;
					program.RunPartialUpdate();

					var output = stringWriter.ToString().Trim();
					StringAssert.Contains("Cannot find AHECCSS-P1-EDCHNG", output);
					StringAssert.DoesNotContain("Cannot find AHECCSS-P1-EEMAIN", output);
					StringAssert.DoesNotContain("Cannot find AHECCSS-Q1-EEMAIN", output);
				}
				finally
				{
					Console.SetError(originalError);
				}
			}
		}

		class AHECCProgramForTest : AHECCProgram
		{
			internal Exception exceptionToThrow = new UnhandledApplicationException();

			public IDateTimeProvider DateTimeProviderForTest;
			protected override IDateTimeProvider DateTimeProvider => DateTimeProviderForTest ?? base.DateTimeProvider;

			protected override BaseAHECCParser FullParser => SetupHttpClient(base.FullParser);
			protected override BaseAHECCParser FullTestParser => SetupHttpClient(base.FullTestParser);
			protected override BaseAHECCParser PartialParser => SetupHttpClient(base.PartialParser);

			BaseAHECCParser SetupHttpClient(BaseAHECCParser parser)
			{
				var mockHttpClientHelper = new Mock<IHttpClientHelper>();
				mockHttpClientHelper.Setup(x => x.GetWebPageAsync(It.IsAny<string>()))
									.Returns<string>(x => throw exceptionToThrow);

				parser.HttpClientHelper = mockHttpClientHelper.Object;
				return parser;
			}
		}
	}
}
