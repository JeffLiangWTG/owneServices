using System.ComponentModel;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business.CustomValues
{
	public class GenCustomAddOnRuleAck : AutoGenCustomAddOnRuleAck
	{
		public new class Schema : AutoGenCustomAddOnRuleAck.Schema
		{
			public const string XK_ParentTableColumn = "XK_ParentTableColumn";
		}

		public GenCustomAddOnRuleAck(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public ZString XK_ParentTableColumn
		{
			get
			{
				return XK_Warning;
			}
			set
			{
				XK_Warning = value;
				XK_ParentTableColumnInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo XK_ParentTableColumnInfo
		{
			get { return GetZPropertyInfo(Schema.XK_ParentTableColumn); }
		}

		protected override ZString HumanReadableNameCore => XK_RuleIDHumanReadableName + " - " + XK_ParentIDHumanReadableName;

		#region Form and Module Overrides

		[List("StaffList", AllowOnlyTheseValues = true)]
		[MaxLength(3)]
		public override ZString XK_SystemCreateUser
		{
			get
			{
				return base.XK_SystemCreateUser;
			}
			set
			{
				base.XK_SystemCreateUser = value;
			}
		}

		GlbStaffCollection staffList;

		[ReadOnly(true)]
		public GlbStaffCollection StaffList
		{
			get
			{
				if (staffList == null)
				{
					staffList = new GlbStaffCollection(Factory);
				}
				return staffList;
			}
		}

		public ZString XK_RuleIDHumanReadableName
		{
			get
			{
				if (Core.Constants.CargoWiseOneGenCustomAddOnRuleIDsList.ContainsKey(XK_RuleID.ToGuid()))
				{
					return Core.Constants.CargoWiseOneGenCustomAddOnRuleIDsList[XK_RuleID.ToGuid()].GetUnresolvedString();
				}
				else
				{
					// glow handling
					var query = new ZQuery(GenCustomAddOnRuleSchema.PK, XK_RuleID);
					var value = Factory.Load<BusinessObject>(query);

					return value[0] == null ? (ZString)ResString.GetMultilingualString("711647FC-4F93-4B3F-9838-CB92A48ED02E", "Invalid Rule") : value[0].HumanReadableName;
				}
			}
		}

		public ZString XK_ParentIDHumanReadableName
		{
			get
			{
				var row = Factory.Load(XK_ParentTableCode, XK_ParentID);
				return row != null ? row.HumanReadableName : XK_ParentTableCode;
			}
		}

		#endregion

		#region Logs

		void AddApplicableStatusChangeLogs()
		{
			if (this.XK_IsCancelledInfo.HasChanges)
			{
				this.Logs.AddNew(AutoEvents.UserMadeAdjustmentsToWarningAcknowledgement);
			}
		}

		#endregion

		#region Saving

		public override void OnSaving()
		{
			AddApplicableStatusChangeLogs();
			base.OnSaving();
		}

		#endregion

#if DEBUG
		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			XK_RuleID = Core.Constants.CargoWiseOneGenCustomAddOnRuleIDs.TestGuidForWarningAcknowledgment;

			if (kind.HasFlag(TestBusinessObjectKind.PopulateRelatedObjects))
			{
				var dummyBizO = Factory.New<ZArchitecture.Business.Testing.DummyBizOWithAutoLogs>();
				XK_ParentID = dummyBizO.PK;
				XK_ParentTableCode = dummyBizO.TablePrefix;
			}

			base.FillWithValidTestDataCore(kind, propertyPath);
		}
#endif
	}
}
