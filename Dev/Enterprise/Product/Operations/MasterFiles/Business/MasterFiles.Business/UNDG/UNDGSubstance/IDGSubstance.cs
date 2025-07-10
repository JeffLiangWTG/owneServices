using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	public interface IDGSubstance
	{
		ZString UNNO { get; }
		ZString Variant { get; }
		ZString Standard { get; }
	}
}
