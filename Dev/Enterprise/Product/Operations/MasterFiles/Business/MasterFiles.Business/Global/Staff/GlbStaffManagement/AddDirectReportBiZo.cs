using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class AddDirectReportsBizO : NonPersistentBusinessObject, IObsoleteValidation
	{
		public abstract class Schema
		{
			public const string ManagerType = "ManagerType";
			public const string EffectiveDate = "EffectiveDate";
		}

		public AddDirectReportsBizO(GlbStaff parent, ZString managerType) : base(parent.Factory)
		{
			Parent = parent;
			ManagerType = managerType;
		}

		#region Properties

		public GlbStaff Parent { get; }

		#region ManagerType

		[List("StaffReportingRoles")]
		public ZString ManagerType
		{
			get => managerType;
			set
			{
				SetNonPersistentPropertyValue(ManagerTypeInfo, ref managerType, value);
				ValidateManagerType();
			}
		}

		ZString managerType;

		public ZPropertyInfo ManagerTypeInfo
		{
			get { return GetZPropertyInfo(Schema.ManagerType); }
		}

		public StaffReportingRole ReportingRole => (StaffReportingRole)SystemDataRegistry.Instance.StaffReportingRoles.Value.FindByCode(ManagerType);

		public ICodeDescriptionBoolList StaffReportingRoles
		{
			get
			{
				return Factory.GetCachedValue("StaffReportingRoles", delegate
				{
					var list = new StaffReportingRoleCollection();
					list.AddRange(SystemDataRegistry.Instance.StaffReportingRoles.Value.OfType<StaffReportingRole>().Where(x => x.Bool));
					return list;
				});
			}
		}

		#endregion

		#region EffectiveDate

		public ZDateTime EffectiveDate
		{
			get => effectiveDate;
			set => SetNonPersistentPropertyValue(EffectiveDateInfo, ref effectiveDate, value);
		}
		ZDateTime effectiveDate = ZDateTime.Today;

		public ZPropertyInfo EffectiveDateInfo
		{
			get { return GetZPropertyInfo(Schema.EffectiveDate); }
		}

		#endregion

		#endregion

		#region Validation

		public void ValidateAll()
		{
			ValidateManagerType();
		}

		public void ValidateManagerType()
		{
			ManagerTypeInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(ManagerTypeInfo);
			ListValidation.ErrorIfInvalidCode(ManagerTypeInfo);
		}

		#endregion

		#region Manager

		public bool AddNewManagerForStaff(GlbStaff directReportStaff, bool shouldSupersede = false)
		{
			if (shouldSupersede)
			{
				if (IsEncompassingExistingManagerInThePast(directReportStaff))
				{
					return false;
				}

				DeleteExistingManagersEncompassedInTheFuture(directReportStaff);
			}

			var newManager = CreateNewManager(directReportStaff);

			if (shouldSupersede)
			{
				EndExistingManagerEffectiveness(newManager);
			}

			return true;
		}

		public GlbStaffManager CreateNewManager(GlbStaff directReportStaff)
		{
			var newManager = Factory.New<GlbStaffManager>();
			newManager.GSM_GS_Staff = directReportStaff.PK;
			newManager.GSM_GS_Manager = Parent.PK;
			newManager.GSM_ManagerType = ManagerType;
			newManager.GSM_EffectiveDate = EffectiveDate;
			return newManager;
		}

		public static void EndExistingManagerEffectiveness(GlbStaffManager newManager)
		{
			foreach (var existingManagerWithOverlappingPeriod in newManager.GetOtherManagersWithOverlappingPeriods())
			{
				existingManagerWithOverlappingPeriod.GSM_EndDate = newManager.GSM_EffectiveDate.AddDays(-1);
			}
		}

		#region Queries

		public ZQuery GetEncompassesExistingManagerRecordQuery(GlbStaff directReportStaff)
		{
			var query = new ZQuery(GlbStaffManagerSchema.GSM_GS_Staff, directReportStaff.PK);
			query.AddToFilter(GlbStaffManagerSchema.GSM_ManagerType, ManagerType);
			query.AddToFilter(JoinCondition.And, GlbStaffManagerSchema.GSM_EffectiveDate, SQLComparisonOperator.GreaterThanOrEqualTo, EffectiveDate);
			return query;
		}

		public bool IsEncompassingExistingManagerInThePast(GlbStaff directReportStaff)
		{
			var query = GetEncompassesExistingManagerRecordQuery(directReportStaff);
			query.AddToFilter(JoinCondition.And, GlbStaffManagerSchema.GSM_EffectiveDate, SQLComparisonOperator.LessThan, ZDateTime.Today);
			return Factory.Load<GlbStaffManager>(query).Any();
		}

		public void DeleteExistingManagersEncompassedInTheFuture(GlbStaff directReportStaff)
		{
			var query = GetEncompassesExistingManagerRecordQuery(directReportStaff);
			query.AddToFilter(JoinCondition.And, GlbStaffManagerSchema.GSM_EffectiveDate, SQLComparisonOperator.GreaterThanOrEqualTo, ZDateTime.Today);
			Factory.Load<GlbStaffManager>(query).ToList().ForEach(x => x.Delete());
		}

		#endregion

		#endregion
	}
}
