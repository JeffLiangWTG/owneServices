using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MarketingManager.GUI.Testing
{
	class TouchScheduleUserControlForTest : TouchScheduleUserControl
	{
		internal ZGroupBox GroupBoxOptionsExposed => base.groupBoxOptions;

		internal ZPanel PanelScheduleDateTimeExposed => base.panelScheduleDateTime;

		internal ZPanel PanelZonesExposed => base.panelZones;

		internal ZGroupBox GroupBoxContactLimitPerOrganizationExposed => base.groupBoxContactLimitPerOrganization;

		internal ZRadioButton RadioBatchRecurrencePatternExposed => base.radioBatchRecurrencePattern;

		internal ZPanel PanelBatchControlRulesExposed => base.panelBatchControlRules;

		internal BatchRecurrenceControl RecurrenceControlExposed => base.recurrenceControl;
	}
}
