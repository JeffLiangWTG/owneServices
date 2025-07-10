using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class GenApprovalRequest : AutoGenApprovalRequest
	{
		public GenApprovalRequest(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(XP_ApprovalStatus), ConcurrencyPolicy.Strict);
		}

		[List("Lookups.SystemCreateUserList")]
		public override ZString XP_SystemCreateUser
		{
			get { return base.XP_SystemCreateUser; }
			set { base.XP_SystemCreateUser = value; }
		}

		public ZDateTime CreatedTimeLocal
		{
			get
			{
				return XP_SystemCreateTimeUtc.ToLocalBranchTime();
			}
		}

		public GlbStaff CreatedUser
		{
			get
			{
				return (GlbStaff)Factory.LoadFromNaturalKey(typeof(GlbStaff), GlbStaffSchema.GS_Code, this.XP_SystemCreateUser);
			}
		}

		public ZString CreatedUser_FullName
		{
			get
			{
				return this.CreatedUser == null ? ZString.Empty : this.CreatedUser.GS_FullName;
			}
		}

		public ZString ApprovedUser_FullName
		{
			get
			{
				return this.ApprovingUser1 == null ? ZString.Empty : this.ApprovingUser1.GS_FullName;
			}
		}

		[List("Lookups.ApprovalStatusList")]
		public override ZString XP_ApprovalStatus
		{
			get { return base.XP_ApprovalStatus; }
			set { base.XP_ApprovalStatus = value; }
		}

		[List("Lookups.ReasonCodeList")]
		public override ZString XP_ReasonCode
		{
			get { return base.XP_ReasonCode; }
			set { base.XP_ReasonCode = value; }
		}

		protected override bool ShouldCreateAutoLogIfOnlyChildrenHaveChanges => true;
		protected override bool ShouldUpdateAuditFieldsIfOnlyChildrenHaveChanges => true;

		protected override ZString HumanReadableNameCore => Res.GetString("6FCFAD03-3179-4482-A0A0-ADF280B9D78B", "Approval Request - {0}", XP_RequestID);

		public List<ZGuid> GetApprovingUserPKs() => new[] { ApprovingUser1, ApprovingUser2, ApprovingUser3, ApprovingUser4, ApprovingUser5, ApprovingUser6 }.Where(x => x != null).Select(x => x.PK).ToList();
	}
}
