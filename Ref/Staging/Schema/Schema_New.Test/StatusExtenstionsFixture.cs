using NUnit.Framework;

namespace CargoWise.RefDbRepo.Staging.Schema_New.Test
{
	[TestFixture]
	class StatusExtenstionsFixture
	{
		[Test]
		public void IsSourceDataMergedStatusShouldReturnTrueWhenStatusIsMEROrFINorDUP()
		{
			var sourceData = new SourceData { SDA_Status = "MER" };
			Assert.IsTrue(sourceData.IsSourceDataMergedStatus());

			sourceData = new SourceData { SDA_Status = "FIN" };
			Assert.IsTrue(sourceData.IsSourceDataMergedStatus());

			sourceData = new SourceData { SDA_Status = "DUP" };
			Assert.IsTrue(sourceData.IsSourceDataMergedStatus());
		}

		[Test]
		public void IsSourceDataErrorStatusShouldReturnTrueWhenStatusIsERROrFIE()
		{
			var sourceData = new SourceData { SDA_Status = "ERR" };
			Assert.IsTrue(sourceData.IsSourceDataErrorStatus());

			sourceData = new SourceData { SDA_Status = "FIE" };
			Assert.IsTrue(sourceData.IsSourceDataErrorStatus());
		}

		[Test]
		public void IsSourceDataProcessedStatusShouldReturnTrueWhenStatusIsPRS()
		{
			var sourceData = new SourceData { SDA_Status = "PRS" };
			Assert.IsTrue(sourceData.IsSourceDataProcessedStatus());
		}

		[Test]
		public void IsSourceDataQueuedStatusShouldReturnTrueWhenStatusIsQUE()
		{
			var sourceData = new SourceData { SDA_Status = "QUE" };
			Assert.IsTrue(sourceData.IsSourceDataQueuedStatus());
		}

		[Test]
		public void IsSourceDataFinalStatusShouldReturnTrueWhenStatusIsFIN()
		{
			var sourceData = new SourceData { SDA_Status = "FIN" };
			Assert.IsTrue(sourceData.IsSourceDataFinalStatus());
		}

		[Test]
		public void IsSourceDataFinalWithErrorsStatusShouldReturnTrueWhenStatusIsFIE()
		{
			var sourceData = new SourceData { SDA_Status = "FIE" };
			Assert.IsTrue(sourceData.IsSourceDataFinalWithErrorsStatus());
		}

		[Test]
		public void IsSourceDataDuplicatedStatusShouldReturnTrueWhenStatusIsDUP()
		{
			var sourceData = new SourceData { SDA_Status = "DUP" };
			Assert.IsTrue(sourceData.IsSourceDataDuplicatedStatus());
		}

		[Test]
		public void IsProcessorStatusErrorStatusShouldReturnTrueWhenStatusIsERR()
		{
			var processorStatus = new ProcessorStatus { PRC_Status = "ERR" };
			Assert.IsTrue(processorStatus.IsProcessorStatusErrorStatus());
		}

		[Test]
		public void IsProcessorStatusProcessedStatusShouldReturnTrueWhenStatusIsPRS()
		{
			var processorStatus = new ProcessorStatus { PRC_Status = "PRS" };
			Assert.IsTrue(processorStatus.IsProcessorStatusProcessedStatus());
		}

		[Test]
		public void IsDataProcessingInformationErrorStatusShouldReturnTrueWhenStatusIsERR()
		{
			var dataProcessingInformation = new DataProcessingInformation { DPI_Status = "ERR" };
			Assert.IsTrue(dataProcessingInformation.IsDataProcessingInformationErrorStatus());
		}

		[Test]
		public void IsDataProcessingInformationProcessedStatusShouldReturnTrueWhenStatusIsPRS()
		{
			var dataProcessingInformation = new DataProcessingInformation { DPI_Status = "PRS" };
			Assert.IsTrue(dataProcessingInformation.IsDataProcessingInformationProcessedStatus());
		}

		[Test]
		public void IsDataChangeCaptureErrorStatusShouldReturnTrueWhenStatusIsERROrFIE()
		{
			var dataChangeCapture = new DataChangeCapture { DCC_NewValue = "ERR" };
			Assert.IsTrue(dataChangeCapture.IsDataChangeCaptureErrorStatus());

			dataChangeCapture = new DataChangeCapture { DCC_NewValue = "FIE" };
			Assert.IsTrue(dataChangeCapture.IsDataChangeCaptureErrorStatus());
		}

		[Test]
		public void IsDataChangeCaptureMergedStatusShouldReturnTrueWhenStatusIsMER()
		{
			var dataChangeCapture = new DataChangeCapture { DCC_NewValue = "MER" };
			Assert.IsTrue(dataChangeCapture.IsDataChangeCaptureMergedStatus());
		}
	}
}
