
namespace Enterprise.MasterFiles.Business
{
	/// <summary>
	/// If in your service task you need to create and save an eDoc, do NOT save it explicitly during the run or you will end
	/// up with a developer error.  Let the base message processor do the saving. Instead, implement this to QUEUE your eDoc for saving.
	/// 
	/// e.g. see C:\dev\Enterprise\Product\Operations\Customs\GB\Enterprise.Customs.GB.CNS\ServiceTasks\ICD\CnsIcdBaseMessageProcessor.cs
	/// and 
	/// C:\dev\Enterprise\Product\Operations\Customs\GB\Enterprise.Customs.GB.CNS\ServiceTasks\ICD\CnsIcdApplicationTypeMessageProcessor.cs
	/// for how to use it. 
	/// </summary> 
	public interface IEDocsDelayedSaver
	{
		void QueueForSaving(DocManagerInfo docManagerInfo);
	}
}
