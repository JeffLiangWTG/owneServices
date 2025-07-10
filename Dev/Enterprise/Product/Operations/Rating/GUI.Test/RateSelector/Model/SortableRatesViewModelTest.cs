using Enterprise.Integration;
using Enterprise.Rating.Business.RateSelector;
using Enterprise.Rating.GUI.RateSelector.Models;
using NUnit.Framework;

namespace Enterprise.Rating.GUI.Testing.RateSelector.Model
{
	class SortableRatesViewModelTest : TestCase
	{
		public void TestLogsChanged()
		{
			var logger = new MemoryLogger();
			var sortableRatesViewModelForTest = new SortableRatesViewModelForTest(logger);

			var counter = 0;
			sortableRatesViewModelForTest.PropertyChanged += (sender, args) => { counter++; };

			logger.Log(LogType.Warning, "Test1");
			AssertEquals("Counter before dispose", 3, counter); // 3x: Logger_LogsChanged: LogText, WarningsCount, WarningsCountText

			sortableRatesViewModelForTest.Dispose();
			logger.Log(LogType.Warning, "Test1");

			AssertEquals("Counter after dispose", 3, counter);
		}

		class SortableRatesViewModelForTest : SortableRatesViewModel
		{
			public SortableRatesViewModelForTest(MemoryLogger logger = null)
				: base(logger) { }

			protected override object GetRatesSource() => null;
			protected override object GetSortProperty(object o, SortOptionViewModel selectedSort) => null;
		}
	}
}
