using System;
using System.Data;

using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	class EmptyOrgMatchApproval : OrgMatchApproval
	{
		public EmptyOrgMatchApproval(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		#region Parent Organisation Properties Mapping

		protected override ZString ParentReference
		{
			get { return ""; }
		}

		protected override ZString ParentOwnerCode
		{
			get { return ""; }
		}

		protected override ZString ParentCompanyName
		{
			get { return ""; }
		}

		protected override ZString ParentStreet
		{
			get { return ""; }
		}

		protected override ZString ParentStreet2
		{
			get { return ""; }
		}

		protected override ZString ParentCity
		{
			get { return ""; }
		}

		protected override ZString ParentUNLOCO
		{
			get { return ""; }
		}

		protected override ZString ParentState
		{
			get { return ""; }
		}

		protected override ZString ParentPostCode
		{
			get { return ""; }
		}

		protected override ZString ParentPhone
		{
			get { return ""; }
		}

		protected override ZString ParentFax
		{
			get { return ""; }
		}

		#endregion

		public override ZString OrganisationType
		{
			get { return ""; }
		}

		public override OrgMatchApprovalType MatchType
		{
			get { return OrgMatchApprovalType.Empty; }
		}

		[BusinessObjectTestExclude]
		public sealed override ZGuid P2_ParentID
		{
			get { return base.P2_ParentID; }
			set { throw new NotSupportedException("Setting P2_ParentID is not supported"); }
		}

		protected override void OnMatchApproved(ZGuid orgMatchPK)
		{
		}
	}
}
