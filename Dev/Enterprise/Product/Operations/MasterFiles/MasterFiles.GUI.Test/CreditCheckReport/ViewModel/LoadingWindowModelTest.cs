using System.Threading.Tasks;
using CargoWise.Async;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Tests
{
	public class LoadingWindowModelTest : TestCase
	{
		public void TestConstructor()
		{
			var model = new LoadingFormModel<string>("WoHaHa", new Task<string>(() => "123"));
			CombineAssertions(() =>
			{
				AssertEquals("WoHaHa", model.Title);
				AssertEquals(ResourceStringHelper.LoadingContent, model.LoadingContent);
			});
		}

		public void TestRunTask()
		{
			AsyncTaskSynchronizer.Run(AssertRunTask);
		}

		async Task AssertRunTask()
		{
			var isCompleted = false;
			var model = new LoadingFormModel<string>("WoHaHa", new Task<string>(() => "123"));
			model.RunWorkerCompleted += Model_RunWorkerCompleted;
			await model.StartRunning();

			Assert(isCompleted);
			AssertNotNull(model.LoadingResult);

			CombineAssertions(() =>
			{
				AssertEquals("123", model.LoadingResult.Result);
				AssertNull(model.LoadingResult.Exception);
			});

			void Model_RunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
			{
				isCompleted = true;
			}
		}
	}
}
