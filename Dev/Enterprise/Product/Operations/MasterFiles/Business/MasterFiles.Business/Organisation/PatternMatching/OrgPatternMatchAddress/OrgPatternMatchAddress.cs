using System.Collections.Generic;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class OrgPatternMatchAddress : AutoOrgPatternMatchAddress
	{
		public static class Constants
		{
			public static class AddressType
			{
				public const string AirCargoConsignee = "ACE";
				public const string AirCargoConsignor = "ACR";
				public const string AirCargoImporter = "ACI";
				public const string AirCargoConsigneeOverride = "ACO";
			}
		}

		public OrgPatternMatchAddress(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public override ZGuid P3_OH_MatchOrg
		{
			get { return base.P3_OH_MatchOrg; }
			set
			{
				if (base.P3_OH_MatchOrg != value)
				{
					base.P3_OH_MatchOrg = value;
					if (value.IsValid)
					{
						ZQuery filter = new ZQuery(OrgMatchApprovalSchema.P2_ParentID, PK);
						OrgMatchApproval matchApproval = (OrgMatchApproval)Factory.LoadTop1(typeof(OrgMatchApproval), filter);
						if (matchApproval != null && !matchApproval.IsApproved)
						{
							matchApproval.P2_OH_MatchOrg1 = value;
							matchApproval.P2_MatchUser1 = GlbStaff.CurrentUser.GS_Code;
							matchApproval.P2_OH_MatchOrg2 = value;
							matchApproval.P2_MatchUser2 = GlbStaff.CurrentUser.GS_Code;
						}
					}
				}
			}
		}

		#region UniqueIndexFailureHandler

		protected override IEnumerable<IUniqueIndexFailureHandler> UniqueIndexFailureHandlers
		{
			get { yield return new AddressUniqueIndexFailureHandler(); }
		}

		class AddressUniqueIndexFailureHandler : IUniqueIndexFailureHandler
		{
			public IEnumerable<string> HandledUniqueIndexNames
			{
				get { yield return OrgPatternMatchAddressSchema.Constants.Indexes.NR_UC__P3_ParentID_P3_AddressType; }
			}

			public void NotifyUserAndAttemptToResolve(INotificationHandler notifier, string indexName)
			{
				notifier.ReportError(
					Res.GetString("bce1c49e-12fc-4118-a562-49c737255564", "Another user has already made changes to this record. You must re-open this form and re-apply your changes to continue."),
					Res.GetString("3a29d147-e3c6-4a8e-a9ee-698af92c223c", "Another user has changed this record"));
			}
		}

		#endregion
	}
}
