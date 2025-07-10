using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	public interface IParentDocManagerSupport : IDocManagerSupport
	{
		ZGuid ParentGuid { get; }
		ZString ParentTableName { get; }
	}
}
