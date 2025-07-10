using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	public interface ICMRReferenceFileUpgrader
	{
		void ImportData(ZBlob fileToUnzip);
	}
}
