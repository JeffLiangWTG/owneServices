using WTG.StaticAnalysis.Annotation;

namespace Enterprise.MasterFiles.Business.Accounting.ProcessLogging
{
	/// <summary>
	/// Loads log details from the storage.
	/// </summary>
	[CodeAlive("Will be implemented in a future workitem")]
	public interface IAccProcessLogLoader
	{
		IAccProcessLog[] Load(ISupportAccProcessLogging parent);
	}
}
