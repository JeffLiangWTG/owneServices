using System;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.DeniedPartyScreening.GUI
{
	public class DeniedPartyScreeningStatusDropEdit : ZDropEdit
	{
		protected override void OnTextChanged(EventArgs e)
		{
			base.OnTextChanged(e);
			SetControlColor();
		}

		DeniedPartyGridColorHelper GridColorHelper => gridColorHelper ?? (gridColorHelper = new DeniedPartyGridColorHelper());
		DeniedPartyGridColorHelper gridColorHelper;

		void SetControlColor()
		{
			var backColor = GridColorHelper.GetColorForResultStatus(CodeBox.Text);

			DescriptionBox.ColorChanger.ForceBackColor(backColor);
			CodeBox.ColorChanger.ForceBackColor(backColor);
		}
	}
}
