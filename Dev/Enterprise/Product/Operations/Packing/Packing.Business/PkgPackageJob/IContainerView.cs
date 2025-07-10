using CargoWise.Types;

namespace Enterprise.Packing.Business
{
	public interface IContainerView
	{
		ZDateTime? PackCompleteDate { get; }
		ZDateTime? UnpackCompleteDate { get; }
	}
}
