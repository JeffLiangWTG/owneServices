using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Internal;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.GUI.General
{
	class StmALogCreateEventRule : StmALogAsAddedByUser
	{
		public StmALogCreateEventRule(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public void HookupChanges(CreateEventRule rule)
		{
			using (SuspendSettingHasChanges())
			{
				if (rule.EventCode.IsEmpty)
				{
					rule.EventCode = CreateEventRule.DefaultEventCode;
				}
				SL_SE_NKEvent = rule.EventCode;
				SL_Reference = rule.EventReference;
				SL_IsEstimate = rule.IsEstimate;
			}

			SL_SE_NKEventInfo.ValueChanged += (s, e) => rule.EventCode = (ZString)((ValueChangedEventArgs)e).NewValue;
			SL_ReferenceInfo.ValueChanged += (s, e) => rule.EventReference = (ZString)((ValueChangedEventArgs)e).NewValue;
			SL_IsEstimateInfo.ValueChanged += (s, e) => rule.IsEstimate = (ZBool)((ValueChangedEventArgs)e).NewValue;
		}

		protected override string CustomEventsKey => nameof(StmALogCreateEventRule) + nameof(GetEventLookups);

		protected override ICodeDescriptionPairList GetEventLookups()
		{
			var list = new CodeDescriptionPairList();
			list.AddRange(Events.Customizables.ToArray());
			return list;
		}

		protected override StmALogValidation GetNewValidation() => new StmALogCreateEventRuleValidation(this);
	}

	class StmALogCreateEventRuleValidation : StmALogAsAddedByUserValidation
	{
		public StmALogCreateEventRuleValidation(StmALogCreateEventRule parent)
			: base(parent)
		{
		}
		protected override void CheckSL_EventTime()
		{
			// Check nothing. This is not relevant for this type.
		}
	}
}
