using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.Accounting.ProcessLogging
{
	public interface ISupportAccProcessLogging
	{
		ZGuid ParentId { get; }

		string ParentTableCode { get; }

		bool ShouldLog { get; }

		IAccProcessLog[] Logs { get; }

		IAccProcessLogger Logger { get; }
	}
}
