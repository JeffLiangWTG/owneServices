using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.SG.V4.Business
{
	public class AdditionalMessageInformationLookups : ZLookups
	{
		public AdditionalMessageInformationLookups(AdditionalMessageInformation additionalMessageInformation)
			: base(additionalMessageInformation)
		{
		}

		#region Lists

		#region RefundCodeList

		public ReasonForRefundCodeList RefundCodeList
		{
			get { return refundCodeList ?? (refundCodeList = new ReasonForRefundCodeList()); }
		}
		ReasonForRefundCodeList refundCodeList;

		#endregion

		#region UpdateIndicatorCodeList

		public UpdateIndicatorCodeList UpdateIndicatorList
		{
			get
			{
				if (fUpdateIndicatorList == null)
				{
					fUpdateIndicatorList = new UpdateIndicatorCodeList();
					fUpdateIndicatorList.RemoveCode(SGConstants.UpdateIndicators.CNL);
				}
				return fUpdateIndicatorList;
			}
		}
		UpdateIndicatorCodeList fUpdateIndicatorList;

		#endregion

		#region CancellationCodeList

		public ReasonForCancellationCodeList CancellationCodeList
		{
			get { return cancellationCodeList ?? (cancellationCodeList = new ReasonForCancellationCodeList()); }
		}
		ReasonForCancellationCodeList cancellationCodeList;

		#endregion

		#region Brokers

		public GlbStaffCollection Brokers
		{
			get
			{
				if (brokers == null)
				{
					ZDBOnlyQuery filter = new ZDBOnlyQuery(typeof(GlbStaff));
					ZDBOnlySubQuery subQuery = new ZDBOnlySubQuery(typeof(GlbExternalPassword), GlbExternalPasswordSchema.GP_GS);
					subQuery.AddToFilter(GlbExternalPasswordSchema.GP_PasswordType, GetBrokerPasswordType());
					subQuery.AddToFilter(GlbExternalPasswordSchema.GP_UserID, SQLComparisonOperator.NotEqual, "");
					subQuery.AddToFilter(GlbExternalPasswordSchema.GP_CurrentPassword, SQLComparisonOperator.NotEqual, "");
					filter.AddSubQuery(subQuery, JoinCondition.And);

					brokers = new GlbStaffCollection(Factory, filter);
				}

				return brokers;
			}
		}
		GlbStaffCollection brokers;

		ZString GetBrokerPasswordType()
		{
			return PasswordTypesList.Codes.SG4;
		}

		#endregion

		#endregion
	}
}
