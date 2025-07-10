using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class AccPOSChargeCodeGroupPivot : AutoAccPOSChargeCodeGroupPivotView
	{
		public AccPOSChargeCodeGroupPivot(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			GRP_GroupType = AccPOSChargeCodeGroup.POSChargeCodeGroupType;
			GRP_MemberTableCode = AccChargeCodeSchema.Constants.Prefix;
		}

		public new class Schema : AutoAccPOSChargeCodeGroupPivotView.Schema
		{
			public const string FK_UC__GRP_GRO_Group_GRP_MemberID = nameof(FK_UC__GRP_GRO_Group_GRP_MemberID);
			public const string NR_UX__GRP_GroupType_GRP_MemberID = nameof(NR_UX__GRP_GroupType_GRP_MemberID);
		}

		#region Overridden Properties

		[ResourceStringData("AccPOSChargeCodeGroupPivot|GRP_GroupType", Caption = "Group Type")]
		[ReadOnly(true)]
		public override ZString GRP_GroupType { get => base.GRP_GroupType; set => base.GRP_GroupType = value; }

		[ResourceStringData("AccPOSChargeCodeGroupPivot|GRP_GRO_Group", Caption = "Group")]
		[List(nameof(Lookups) + "." + nameof(AccPOSChargeCodeGroupPivotViewLookups.Groups))]
		public override ZGuid GRP_GRO_Group { get => base.GRP_GRO_Group; set => base.GRP_GRO_Group = value; }

		[ResourceStringData("AccPOSChargeCodeGroupPivot|GRP_MemberTableCode", Caption = "Member Table Code")]
		[ReadOnly(true)]
		public override ZString GRP_MemberTableCode { get => base.GRP_MemberTableCode; set => base.GRP_MemberTableCode = value; }

		[ResourceStringData("AccPOSChargeCodeGroupPivot|GRP_MemberID", Caption = "Charge Code")]
		[List(nameof(Lookups) + "." + nameof(AccPOSChargeCodeGroupPivotViewLookups.ChargeCodes))]
		public override ZGuid GRP_MemberID { get => base.GRP_MemberID; set => base.GRP_MemberID = value; }

		#endregion

		#region Related Business Objects

		public AccPOSChargeCodeGroup ChargeCodeGroup => Factory.Load<AccPOSChargeCodeGroup>(GRP_GRO_Group);

		public GlbCompany Company => ChargeCodeGroup?.Company;

		public AccChargeCode ChargeCode => Factory.Load<AccChargeCode>(GRP_MemberID);

		#endregion

		#region UniqueIndexFailureHandler

		protected override IEnumerable<IUniqueIndexFailureHandler> UniqueIndexFailureHandlers
		{
			get { yield return new POSGroupMemberUniqueIndexFailureHandler(this); }
		}

		class POSGroupMemberUniqueIndexFailureHandler : IUniqueIndexFailureHandler
		{
			public POSGroupMemberUniqueIndexFailureHandler(AccPOSChargeCodeGroupPivot pivot)
			{
				Pivot = pivot;
			}

			readonly AccPOSChargeCodeGroupPivot Pivot;

			public void NotifyUserAndAttemptToResolve(INotificationHandler notifier, string indexName)
			{
				var message = ZString.Empty;
				switch (indexName)
				{
					case Schema.FK_UC__GRP_GRO_Group_GRP_MemberID:
						message = Res.GetString("9cb55c98-7956-4c38-ad30-eb59f0baa574", "Charge Code '{0}' is already a member of this group.", Pivot.ChargeCode?.AC_Code);
						break;
					case Schema.NR_UX__GRP_GroupType_GRP_MemberID:
						message = Res.GetString("d4c725cf-f8d4-4412-af32-8d29379f954f", "Charge Code '{0}' has been already added to a different configuration group.", Pivot.ChargeCode?.AC_Code);
						break;
					default:
						message = Res.GetString("3b5c195e-dd15-4619-9753-0377605e6730", "Unique index violation. Index name: {0}", indexName);
						break;
				}

				notifier.ReportError(message, Res.GetString("cea1144b-bca2-4a02-baec-dee380f4cdad", "Charge Code Group for the Place of Supply configuration"));
			}

			public IEnumerable<string> HandledUniqueIndexNames
			{
				get
				{
					yield return Schema.FK_UC__GRP_GRO_Group_GRP_MemberID;
					yield return Schema.NR_UX__GRP_GroupType_GRP_MemberID;
				}
			}
		}

		#endregion
	}
}

