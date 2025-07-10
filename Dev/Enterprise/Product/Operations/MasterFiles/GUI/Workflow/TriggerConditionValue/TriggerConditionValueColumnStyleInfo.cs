using System;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public class TriggerConditionValueColumnStyle : ZMultiControlColumnStyle
	{
		public TriggerConditionValueColumnStyle(string eventCode, TriggerConditionValueColumnStyleInfo info)
			: base(() => new TriggerConditionValueControl(eventCode), info)
		{
		}

		public TriggerConditionValueColumnStyle(TriggerConditionValueColumnStyleInfo info)
			: base(() => new TriggerConditionValueControl("Z00"), info)
		{
		}

		public new TriggerConditionValueControl EditControl
		{
			get { return (TriggerConditionValueControl)base.EditControl; }
		}
	}

	public class TriggerConditionValueColumnStyleInfo : ZMultiControlColumnStyleInfo
	{
		public override Type ColumnStyleType
		{
			get { return typeof(TriggerConditionValueColumnStyle); }
		}
	}
}
