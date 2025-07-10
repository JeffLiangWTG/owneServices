using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	public interface IComplianceJobDirectionProvider : IImportExport
	{
		ZBool IsInternational { get; }
	}
}
