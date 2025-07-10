using System;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business.Accounting.Helpers;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class AccChargeGLPostingOverrideCollection : DependentBusinessObjectCollection<AccChargeGLPostingOverride, AccChargeCode>
	{
		public AccChargeGLPostingOverrideCollection(AccChargeCode chargeCode)
			: base(chargeCode)
		{
		}

		#region Master Charge Code

		public AccChargeCode ChargeCode
		{
			get { return Master; }
		}

		#endregion

		protected override bool AllowNewCore
		{
			get { return base.AllowNewCore && ChargeCode.AC_ChargeType != Constants.ChargeType.Comment; }
		}

		protected override ZQuery CreateRelationshipFilter()
		{
			var filter = base.CreateRelationshipFilter();

			filter.AddToFilter(AccChargeGLPostingOverrideSchema.Y1_AC, Master.PK);

			return filter;
		}

		public AccChargeGLPostingOverride GetGLPostingOverride(ZString categoryClass, GlbDepartment department, ZString jobType, ZString transportMode, ZString direction, ZString consolContainerMode, ZString masterPaymentType, ZString housePaymentType)
		{
			var ranker = new ColumnValueRanker();
			ranker.Add(AccChargeGLPostingOverrideSchema.Y1_ConsolidationAccountingCategoryClass, categoryClass, (ZString)ConsolidatedAccountingCategoryClassList.Codes.All);
			ranker.Add(AccChargeGLPostingOverrideSchema.Y1_GE, department == null ? ZGuid.Empty : department.PK, ZGuid.Empty);
			ranker.Add(AccChargeGLPostingOverrideSchema.Y1_JobType, jobType, (ZString)AccChargeGLPostingOverrideLookups.JobTypeAdditionalCodes.All);
			ranker.Add(AccChargeGLPostingOverrideSchema.Y1_TransportMode, transportMode, (ZString)AccChargeGLPostingOverrideLookups.TransportModeAdditionalCodes.All);
			ranker.Add(AccChargeGLPostingOverrideSchema.Y1_Direction, direction, (ZString)Constants.FreightShipmentDirection.Code.All);
			ranker.Add(AccChargeGLPostingOverrideSchema.Y1_ConsolContainerMode, consolContainerMode, (ZString)AccChargeGLPostingOverrideLookups.ConsolContainerModeAdditionalCodes.All);
			ranker.Add(AccChargeGLPostingOverrideSchema.Y1_MasterPaymentType, masterPaymentType, (ZString)AccChargeGLPostingOverrideLookups.MasterPaymentTypeAdditionalCodes.All);
			ranker.Add(AccChargeGLPostingOverrideSchema.Y1_HousePaymentType, housePaymentType, (ZString)AccChargeGLPostingOverrideLookups.HousePaymentTypeAdditionalCodes.All);

			return ranker.GetBestMatch<AccChargeGLPostingOverride>(Factory, CreateRelationshipFilter());
		}

		#region BusinessObjectCollectionSnapShotter

		BusinessObjectCollectionSnapshotManager businessObjectCollectionSnapShotter;

		public BusinessObjectCollectionSnapshotManager SnapShotter
		{
			get
			{
				if (businessObjectCollectionSnapShotter == null)
				{
					businessObjectCollectionSnapShotter = new BusinessObjectCollectionSnapshotManager(new AccChargeGLPostingOverrideCopier(), this);
				}
				return businessObjectCollectionSnapShotter;
			}
		}

		#endregion
	}

	public class AccChargeGLPostingOverrideCopier : BusinessObjectCollectionCopier
	{
		public AccChargeGLPostingOverrideCopier()
			: base(
				new SchemaColumn[] {
					AccChargeGLPostingOverrideSchema.Y1_GE },
				new SchemaColumn[] {
						AccChargeGLPostingOverrideSchema.Y1_AG_ACR,
						AccChargeGLPostingOverrideSchema.Y1_AG_CST,
						AccChargeGLPostingOverrideSchema.Y1_AG_REV,
						AccChargeGLPostingOverrideSchema.Y1_AG_WIP,
						AccChargeGLPostingOverrideSchema.Y1_AG_REV_Clearing,
						AccChargeGLPostingOverrideSchema.Y1_AG_CST_Clearing,
						AccChargeGLPostingOverrideSchema.Y1_ConsolidationAccountingCategoryClass,
						AccChargeGLPostingOverrideSchema.Y1_JobType,
						AccChargeGLPostingOverrideSchema.Y1_TransportMode,
						AccChargeGLPostingOverrideSchema.Y1_Direction,
						AccChargeGLPostingOverrideSchema.Y1_ConsolContainerMode,
						AccChargeGLPostingOverrideSchema.Y1_MasterPaymentType,
						AccChargeGLPostingOverrideSchema.Y1_HousePaymentType,
				})
		{
		}
		static readonly Lazy<AccChargeGLPostingOverrideCopier> instance = new Lazy<AccChargeGLPostingOverrideCopier>();
		public static AccChargeGLPostingOverrideCopier Instance { get { return instance.Value; } }
	}
}
