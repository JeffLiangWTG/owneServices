namespace Enterprise.MasterFiles.GUI
{
	using System;
	using System.Windows.Forms;
	using Enterprise.MasterFiles.Business;
	using Enterprise.ZArchitecture.Environment;
	using Enterprise.ZArchitecture.GUI;
	using Enterprise.ZArchitecture.GUI.Internal;

	public class TriggerConditionValueLookupControl : ZGridFindBox
	{
		public TriggerConditionValueLookupControl(string eventCode)
		{
			((IFindBoxUserControl)this).AutoCompleteDisabled = true;
			CodeBox.CharacterCasing = CharacterCasing.Normal;
			this.eventCode = eventCode;
		}

		readonly string eventCode;

		protected override IFindBoxPopup GetNewPopupForm()
		{
			var parameters = new TriggerConditionValueParameters(eventCode, CodeBox.Text);
			return new TriggerConditionValueLookupForm(parameters);
		}

		public override void SelectFromPopupForm(bool autoSelect)
		{
			try
			{
				base.SelectFromPopupForm(autoSelect);
			}
			catch (ArgumentException)
			{
				Globals.Message.ShowError(Res.GetString("abf64468-382b-454d-8947-0628a01a88da", "The trigger condition value you entered contains duplicate parameter code and cannot be processed by the system for RFP trigger condition."));
			}
		}
	}
}
