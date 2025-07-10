using System;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestsSubclassesOf(typeof(DataLoaderForm))]
	public abstract class DataLoaderFormTestCase : ImportFromCSVFormBaseTest
	{
		[RequiresSTA]
		public void TestGetNewDataLoader_IsOverriden()
		{
			using (var form = (DataLoaderForm)GetNewImportFromCSVForm())
			{
				AssertNoExceptionThrown(() =>
				{
					var loader = form.GetNewDataLoader_ExposedForTesting();
					AssertNotNull("DataLoader", loader);
				});
			}
		}

		[RequiresSTA]
		public void TestLoadSpecificDataType_CorrectType()
		{
			using (var form = GetNewImportFromCSVForm())
			{
				try
				{
					form.LoadSpecificDataType("");
				}
				catch (InvalidCastException castException)
				{
					Fail(string.Format("Invalid cast: {0}", castException.Message));
				}
				catch
				{
				}

				Assert("No Cast Exception Occured", true);
			}
		}
	}
}
