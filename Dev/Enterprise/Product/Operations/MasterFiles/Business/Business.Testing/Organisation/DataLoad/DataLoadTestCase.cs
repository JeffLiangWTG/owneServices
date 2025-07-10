using System.IO;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestsSubclassesOf(typeof(DataLoad), ExcludePrivate = true)]
	public abstract class DataLoadTestCase<T> : TestCaseWithFactory
		where T : DataLoad
	{
		public void TestCSVTemplateHeading()
		{
			// import with csv template heading to ensure it passes validation

			var dataLoad = GetNewDataLoader();
			if (dataLoad.HasCSVTemplateHeading)
			{
				using (var testFile = TempFile.New())
				{
					using (var streamWriter = new StreamWriter(testFile.Filename))
					{
						streamWriter.WriteLine(dataLoad.CSVTemplateHeading);
						streamWriter.Flush();
					}

					ImportCSVTemplateHeading(dataLoad, testFile.Filename);

					Assert(string.Format(
	@"The CSV Template Heading for [{0}] is not valid.
Template Contents: [{1}]
", typeof(T).Name, dataLoad.CSVTemplateHeading),
			dataLoad.FileHeaderIsValid);

					AssertEquals(0, dataLoad.RunCounters.RecsCreated);
					AssertEquals(0, dataLoad.RunCounters.RecsUpdated);
					AssertEquals(0, dataLoad.RunCounters.RecsExcluded);
				}
			}
			else
			{
				Assert("Not applicable", true);
			}
		}

		protected virtual void ImportCSVTemplateHeading(T dataLoad, string dataLocation)
		{
			dataLoad.ImportData(dataLocation, typeof(T).Name);
		}

		protected abstract T GetNewDataLoader();
	}
}
