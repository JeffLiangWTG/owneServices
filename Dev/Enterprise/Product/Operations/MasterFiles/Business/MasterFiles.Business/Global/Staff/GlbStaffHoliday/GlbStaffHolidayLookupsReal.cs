using CargoWise.Definitions;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class GlbStaffHolidayLookupsReal : GlbStaffHolidayLookups
	{
		public GlbStaffHolidayLookupsReal(AutoGlbStaffHoliday parent)
			: base(parent)
		{
		}

		#region Types

		protected override CodeDescriptionBoolCollection GetTypes()
		{
			return SystemDataRegistry.Instance.StaffLeaveTypes.Value;
		}

		#endregion

		#region Approval Statuses

		public const string Requested = StaffHolidayApprovalCodes.Requested;
		public const string UnderConsideration = StaffHolidayApprovalCodes.UnderConsideration;
		public const string Approved = StaffHolidayApprovalCodes.Approved;
		public const string Declined = StaffHolidayApprovalCodes.Declined;
		public const string ConditionalApproval = StaffHolidayApprovalCodes.ConditionalApproval;
		public const string Cancelled = StaffHolidayApprovalCodes.Cancelled;
		public const string CancelRequested = StaffHolidayApprovalCodes.CancelRequested;
		public CodeDescriptionPairList Statuses
		{
			get
			{
				if (fStatuses == null)
				{
					fStatuses = new CodeDescriptionPairList();
					fStatuses.AddPair(Requested, Res.GetString("68987849-d529-4ecc-a798-2de1ad805c29", "Requested"));
					fStatuses.AddPair(UnderConsideration, Res.GetString("6369d5bb-02cf-491c-9634-cd3621261d88", "Under Consideration"));
					fStatuses.AddPair(Approved, Res.GetString("52684d39-c85a-4ab7-9524-4f57195fc13c", "Approved"));
					fStatuses.AddPair(Declined, Res.GetString("2b8279f4-94c4-4b58-9be5-959d5225dff7", "Declined"));
					fStatuses.AddPair(Cancelled, Res.GetString("afd991b1-902b-4e3a-bddb-8edfc2458cf2", "Canceled"));
					fStatuses.AddPair(CancelRequested, Res.GetString("ccdd6c87-b1fc-404e-874b-78b724ebf694", "Cancel Requested"));
					fStatuses.AddPair(ConditionalApproval, Res.GetString("a5ddae30-737e-42dc-ae0b-8823fa7fe4bf", "Conditional Approval"));
				}

				return fStatuses;
			}
		}

		CodeDescriptionPairList fStatuses;

		#endregion
	}
}
