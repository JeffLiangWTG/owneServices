#if DEBUG
using System.Collections.Generic;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.Testing
{
	public class DummyOrgMatchApproval : OrgMatchApproval
	{
		public DummyOrgMatchApproval(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		#region Bound Organisation Details

		protected override ZString ParentReference
		{
			get { return Parent.Z0_Description; }
		}

		protected override ZString ParentMasterBill
		{
			get { return "MasterBill"; }
		}

		protected override ZString ParentOwnerCode
		{
			get { return "OwnerCode"; }
		}

		protected override ZString ParentCompanyName
		{
			get { return "Organisation CompanyName"; }
		}

		protected override ZString ParentStreet
		{
			get { return "Street"; }
		}

		protected override ZString ParentStreet2
		{
			get { return "Street2"; }
		}

		protected override ZString ParentCity
		{
			get { return "City"; }
		}

		protected override ZString ParentUNLOCO
		{
			get { return "AUSYD"; }
		}

		protected override ZString ParentState
		{
			get { return "State"; }
		}

		protected override ZString ParentPostCode
		{
			get { return "PostCode"; }
		}

		protected override ZString ParentPhone
		{
			get { return "Phone"; }
		}

		protected override ZString ParentFax
		{
			get { return "Fax"; }
		}

		#endregion

		public override ZString OrganisationType
		{
			get { return "Org Type"; }
		}

		public bool IsCurrentUserSupervisorOverride = true;
		public override bool IsCurrentUserSupervisor
		{
			get { return IsCurrentUserSupervisorOverride; }
		}

		public void SetIsApprovedByOtherUsers(bool value)
		{
			Parent.Z0_Bool = value;
		}

		public override bool IsApprovedByOtherUsers
		{
			get { return base.IsApprovedByOtherUsers || Parent.Z0_Bool; }
		}

		public new DummyBusinessObject Parent
		{
			get { return (DummyBusinessObject)base.Parent; }
		}

		public override OrgMatchApprovalType MatchType
		{
			get { return OrgMatchApprovalType.DummyType; }
		}

		public new IEnumerable<IUniqueIndexFailureHandler> UniqueIndexFailureHandlers
		{
			get { return base.UniqueIndexFailureHandlers; }
		}

		public bool OnMatchApprovedCalled;

		protected override void OnMatchApproved(ZGuid orgMatchPK)
		{
			base.OnMatchApproved(orgMatchPK);
			Parent.Z0_Description = "match committed";
			OnMatchApprovedCalled = true;
		}
	}
}
#endif
