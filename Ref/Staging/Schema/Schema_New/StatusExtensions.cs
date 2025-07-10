using System;
using System.Linq;

namespace CargoWise.RefDbRepo.Staging.Schema_New;

public static class StatusExtensions
{

	#region SourceData 
	public static bool IsSourceDataMergedStatus(this SourceData sourceData)
	{
		return StatusProvider.GetSourceDataMergedStatuses().Contains(sourceData.SDA_Status);
	}

	public static bool IsSourceDataProcessedStatus(this SourceData sourceData)
	{
		return sourceData.SDA_Status == StatusProvider.GetPRSStatus();
	}

	public static bool IsSourceDataQueuedStatus(this SourceData sourceData)
	{
		return sourceData.SDA_Status == StatusProvider.GetQUEStatus();
	}

	public static bool IsSourceDataFinalStatus(this SourceData sourceData)
	{
		return sourceData.SDA_Status == StatusProvider.GetFINStatus();
	}

	public static bool IsSourceDataFinalWithErrorsStatus(this SourceData sourceData)
	{
		return sourceData.SDA_Status == StatusProvider.GetFIEStatus();
	}

	public static bool IsSourceDataErrorStatus(this SourceData sourceData)
	{
		return StatusProvider.GetSourceDataErrorStatuses().Contains(sourceData.SDA_Status);
	}

	public static bool IsSourceDataDuplicatedStatus(this SourceData sourceData)
	{
		return sourceData.SDA_Status == StatusProvider.GetDUPStatus();
	}

	#endregion

	#region ProcessorStatus

	public static bool IsProcessorStatusErrorStatus(this ProcessorStatus processorStatus)
	{
		return StatusProvider.GetProcessorStatusErrorStatuses().Contains(processorStatus.PRC_Status);
	}

	public static bool IsProcessorStatusProcessedStatus(this ProcessorStatus processorStatus)
	{
		return processorStatus.PRC_Status == StatusProvider.GetPRSStatus();
	}

	#endregion

	#region DataProcessingInformation

	public static bool IsDataProcessingInformationErrorStatus(this DataProcessingInformation dataProcessingInformation)
	{
		return StatusProvider.GetDataProcessingInformationErrorStatuses().Contains(dataProcessingInformation.DPI_Status);
	}

	public static bool IsDataProcessingInformationProcessedStatus(this DataProcessingInformation dataProcessingInformation)
	{
		return dataProcessingInformation.DPI_Status == StatusProvider.GetPRSStatus();
	}

	#endregion

	#region DataChangeCapture

	public static bool IsDataChangeCaptureErrorStatus(this DataChangeCapture dataChangeCapture)
	{
		return StatusProvider.GetDataChangeCaptureErrorStatuses().Contains(dataChangeCapture.DCC_NewValue);
	}

	public static bool IsDataChangeCaptureMergedStatus(this DataChangeCapture dataChangeCapture)
	{
		return dataChangeCapture.DCC_NewValue == StatusProvider.GetMERStatus();
	}

	#endregion
}
