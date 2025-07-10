using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	public interface ICommissionAgreementConflict
	{
		OrgCommissionAgreement WinnerCommissionAgreement { get; }
		OrgCommissionAgreement LoserCommissionAgreement { get; }

		ZBool IsDeleted { get; }
	}

	public interface ICommissionAgreementItemConflict : ICommissionAgreementConflict
	{
		OrgCommissionAgreementItem AgreementItem { get; }
		OrgCommissionAgreementItem WinnerAgreementItem { get; }
		OrgCommissionAgreementItem LoserAgreementItem { get; }
	}
}