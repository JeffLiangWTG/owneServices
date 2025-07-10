using System.Text.RegularExpressions;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.IEReferenceData.CodeLists.Services;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.IEReferenceData.CodeLists.Tests
{
	abstract class RevenueCodeListDetailsTest
	{
		[Test]
		public void ApplicationType()
		{
			Assert.That(CodeListDetails.ApplicationType, Is.EqualTo(ExpectedApplicationType));
		}

		[Test]
		public void Code()
		{
			Assert.That(CodeListDetails.Code, Is.EqualTo(ExpectedCode));
		}

		[Test]
		public void NameInFile()
		{
			Assert.That(CodeListDetails.NameInFile, Is.EqualTo(ExpectedNameInFile));
		}

		[Test]
		public void TableTitleInFile()
		{
			Assert.That(CodeListDetails.TableTitleInFile, Is.EqualTo(ExpectedTableTitleInFile));
		}

		[Test]
		public void CodeFormattingRegularExpression()
		{
			var codeFormattingRegularExpression = CodeListDetails.CodeFormattingRegularExpression;
			Assert.Multiple(() =>
			{
				Assert.That(codeFormattingRegularExpression, Is.EqualTo(ExpectedCodeFormattingRegularExpression), "Formatting regular expression text");
				Assert.DoesNotThrow(() => { var regex = new Regex(codeFormattingRegularExpression); }, "Formatting regular expression check");
			});
		}

		[Test]
		public void TableAllowCombination()
		{
			Assert.That(CodeListDetails.AllowCombination, Is.EqualTo(ExpectedAllowCombination));
		}

		[Test]
		public void TableIsPublished()
		{
			Assert.That(CodeListDetails.IsPublished, Is.EqualTo(ExpectedIsPublished));
		}

		[Test]
		public void TableUpdateType()
		{
			Assert.That(CodeListDetails.UpdateType, Is.EqualTo(ExpectedUpdateType));
		}

		protected abstract IEReferenceData.Services.ApplicationType ExpectedApplicationType { get; }

		protected abstract string ExpectedCode { get; }

		protected abstract string ExpectedNameInFile { get; }

		protected abstract string ExpectedTableTitleInFile { get; }

		protected abstract string ExpectedCodeFormattingRegularExpression { get; }

		protected abstract bool ExpectedAllowCombination { get; }

		protected abstract bool ExpectedIsPublished { get; }

		protected virtual UpdateType ExpectedUpdateType => UpdateType.Full;

		protected abstract IRevenueCodeListDetails GetCodeListDetails();

		protected IRevenueCodeListDetails CodeListDetails => codeListDetails ?? (codeListDetails = GetCodeListDetails());
		IRevenueCodeListDetails codeListDetails;
	}
}
