
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Freight.Business
{
	public interface IContainer : IBusiness
	{
		ZGuid PK { get; }
		ZString ContainerMode { get; }
		ZBool IsQuarantineRequired { get; }
		ZBool IsCustomsHold { get; }
		ZBool IsFumigationRequired { get; }
	}
}
