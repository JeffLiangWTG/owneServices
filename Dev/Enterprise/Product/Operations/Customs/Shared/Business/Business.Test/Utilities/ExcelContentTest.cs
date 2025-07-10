using System.IO;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	public sealed class ExcelContentTest : TestCase
	{
		public static void AssertPrintJobContainsAndNotContainsText(string[] expectedBodyFragments, string[] bodyMustNotContainFragments, StmPrintJob printInQueue)
		{
			using (var sourceFile = TempFile.New())
			{
				File.WriteAllBytes(sourceFile.Filename, printInQueue.SP_CustomProperties);
				using (var excelComparerBensTool = new ExcelComparator.ComparableTextGenerator(sourceFile.Filename, 0))
				{
					excelComparerBensTool.SaveAsComparableTextTempFile();
					using (var outputFile = TempFile.NewFromFile(excelComparerBensTool.TempFileName))
					{
						var actualText = File.ReadAllText(outputFile.Filename);
						foreach (var fragment in expectedBodyFragments)
						{
							AssertEquals(string.Format("Expected actualText to contain '{0}'. Full text: {1}", fragment, actualText), true, actualText.Contains(fragment));
						}
						foreach (var fragment in bodyMustNotContainFragments)
						{
							AssertEquals(string.Format("Expected actualText to not contain '{0}'. Full text: {1}", fragment, actualText), false, actualText.Contains(fragment));
						}
					}
				}
			}
		}
	}
}
