using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.Business
{
	public interface ICusDispositionParent
	{
		ZString Type { get; }
		ZString GetStatusDescription(ZString status);
		ZString ParentTableCode { get; }
		BusinessObject CollectionMaster { get; }
	}
}
