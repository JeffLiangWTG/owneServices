using CargoWise.Types;

namespace Enterprise.Packing.Business
{
	public interface IPackageSequence
	{
		ZShort Sequence { get; set; }

		ZGuid PackageHeaderFK { get; }

		ZGuid PackageJobFK { get; }

		bool RequiresSequencing { get; }
	}
}
