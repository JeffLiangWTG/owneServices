using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business
{
	public interface IGlbPasswordHistoryParent : IPasswordStored
	{
		BusinessObject BusinessEntity { get; }
		int PasswordHistoryCount { get; }
		bool ShouldSavePasswordHistory { get; }
	}
}
