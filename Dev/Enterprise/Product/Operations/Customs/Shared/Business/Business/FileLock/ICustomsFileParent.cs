using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.Business
{
	public interface ICustomsFileParent : IStmALogParent
	{
		ZBool IsLocked { get; }

		ZString DeclarationType { get; }

		ZGuid BranchPk { get; }

		ZPropertyInfo DeclarationTypeInfo { get; }

		void LockFile(ZString reference);

		void UnlockFile(ZString reference);
	}
}
