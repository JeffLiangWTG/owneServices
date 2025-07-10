using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	public interface ICommissionAgreementRelated<T>
		where T : BusinessObject
	{
		T ParentVersion { get; }
		OrgCommissionAgreement CommissionAgreement { get; }
	}

	public static class CommissionAgreementRelatedExtensions
	{
		public static T GetMainVersion<T>(this T commissionAgreementRelated)
			where T : BusinessObject, ICommissionAgreementRelated<T>
		{
			return commissionAgreementRelated.ParentVersion ?? commissionAgreementRelated;
		}

		public static ZBool IsMainVersion<T>(this T commissionAgreementRelated)
			where T : BusinessObject, ICommissionAgreementRelated<T>
		{
			return commissionAgreementRelated.ParentVersion == null;
		}

		public static ZBool IsUncommittedDraft<T>(this T commissionAgreementRelated)
			where T : BusinessObject, ICommissionAgreementRelated<T>
		{
			return commissionAgreementRelated.CommissionAgreement != null && commissionAgreementRelated.CommissionAgreement.IsUncommittedDraft;
		}

		public static ZBool IsDraft<T>(this T commissionAgreementRelated)
			where T : BusinessObject, ICommissionAgreementRelated<T>
		{
			if (commissionAgreementRelated.ParentVersion != null)
			{
				return true;
			}

			return commissionAgreementRelated.CommissionAgreement == null || commissionAgreementRelated.CommissionAgreement.IsDraft;
		}
	}
}
