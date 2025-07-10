using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.NZ.Business
{
	public interface ICusStorageDocPivotParent : ILinkable, ICusStorageDocPivotTypeSupporter
	{
		CusStorageDocPivotCollection EDocPivotCollection { get; }
	}
}
