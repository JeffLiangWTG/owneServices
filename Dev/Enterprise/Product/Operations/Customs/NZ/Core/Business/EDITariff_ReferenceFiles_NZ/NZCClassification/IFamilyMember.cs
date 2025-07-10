using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.NZ.Business.EDITariff_ReferenceFiles_NZ
{
	public interface IFamilyMember : CargoWise.EntityFramework.IFamilyMember
	{
		ZString WrappedLongDescription { get; }
		ZPropertyInfo WrappedLongDescriptionInfo { get; }

		ZString StatUnit { get; }
		ZPropertyInfo StatUnitInfo { get; }

		ZString SuppUnit { get; }
		ZPropertyInfo SuppUnitInfo { get; }
	}
}
