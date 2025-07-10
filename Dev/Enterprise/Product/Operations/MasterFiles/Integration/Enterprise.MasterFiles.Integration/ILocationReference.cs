using CargoWise.Types;

namespace Enterprise.MasterFiles.Integration
{
	public interface ILocationReference
	{
		bool IsLocalInRelationTo(ZString code);
	}
}
