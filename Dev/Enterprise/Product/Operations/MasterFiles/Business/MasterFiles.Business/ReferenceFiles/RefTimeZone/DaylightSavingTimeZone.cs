using System.ComponentModel;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	[ProvideMetaDataProperty("ReadOnly", MetaDataTypes.ReadOnly)]
	public class DaylightSavingTimeZone : RefTimeZone
	{
		public DaylightSavingTimeZone(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new DaylightSavingTimeZoneFetchStrategy(this);
		}

		class DaylightSavingTimeZoneFetchStrategy : EnterpriseBusinessObjectFetchStrategy
		{
			public DaylightSavingTimeZoneFetchStrategy(DaylightSavingTimeZone businessObject)
				: base(businessObject)
			{
			}

			protected override void FetchForLoadCore()
			{
				base.FetchForLoadCore();
				var queryStart = new ZQuery(RefTimeZoneRuleSchema.R4_StartOrEndRule, RefTimeZoneRule.StartRuleCode);
				var queryEnd = new ZQuery(RefTimeZoneRuleSchema.R4_StartOrEndRule, RefTimeZoneRule.EndRuleCode);
				Factory.AddFetchHint(RefTimeZoneRuleSchema.Instance, queryStart);
				Factory.AddFetchHint(RefTimeZoneRuleSchema.Instance, queryEnd);
			}
		}

		#region ReadOnly

		protected bool GetReadOnly(PropertyDescriptor property)
		{
			return RefTimeZoneSet == null || RefTimeZoneSet.R3_IsSystem || MetaData.GetReadOnlyExcludingMethodProvider(this, property);
		}

		#endregion

		#region Properties

		public RefTimeZoneSet RefTimeZoneSet
		{
			get
			{
				if (refTimeZoneSet == null)
				{
					var query = new ZQuery(RefTimeZoneSetSchema.R3_R2_DaylightSavingZone, SQLComparisonOperator.Equal, PK);
					refTimeZoneSet = Factory.LoadTop1<RefTimeZoneSet>(query);
				}

				return refTimeZoneSet;
			}
		}
		RefTimeZoneSet refTimeZoneSet;

		#region StartDateRules Collection

		[ChildEditable(true)]
		public RefTimeZoneStartRuleCollection StartDateRules
		{
			get
			{
				if (fStartDateRules == null)
				{
					fStartDateRules = new RefTimeZoneStartRuleCollection(this);
					RegisterEditableChildObject(fStartDateRules);
				}

				return fStartDateRules;
			}
		}
		RefTimeZoneStartRuleCollection fStartDateRules;

		#endregion

		#region EndDateRules Collection

		[ChildEditable(true)]
		public RefTimeZoneEndRuleCollection EndDateRules
		{
			get
			{
				if (fEndDateRules == null)
				{
					fEndDateRules = new RefTimeZoneEndRuleCollection(this);
					RegisterEditableChildObject(fEndDateRules);
				}

				return fEndDateRules;
			}
		}
		RefTimeZoneEndRuleCollection fEndDateRules;

		#endregion

		#endregion

		#region Delete

		public override void Delete()
		{
			StartDateRules.DeleteAll();
			EndDateRules.DeleteAll();
			base.Delete();
		}

		#endregion

		#region Validation

		protected override RefTimeZoneValidation GetNewValidation()
		{
			return new DaylightSavingZoneValidation(this);
		}

		public new DaylightSavingZoneValidation Validation
		{
			get { return (DaylightSavingZoneValidation)base.Validation; }
		}

		#endregion

	}
}
