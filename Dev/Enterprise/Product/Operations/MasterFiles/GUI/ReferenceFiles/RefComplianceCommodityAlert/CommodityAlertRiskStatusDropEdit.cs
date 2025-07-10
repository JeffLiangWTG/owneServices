using System;
using Enterprise.ComplianceRisk.GUI;
using Enterprise.ZArchitecture.GUI;
namespace Enterprise.MasterFiles.GUI
{
	public class CommodityAlertRiskStatusDropEdit : ZDropEdit
	{
		protected override void OnTextChanged(EventArgs e)
		{
			base.OnTextChanged(e);
			SetControlColor();
		}
		void SetControlColor()
		{
			var backColor = ComplianceRiskColorHelper.GetColorForRiskStatus(CodeBox.Text);
			DescriptionBox.ColorChanger.ForceBackColor(backColor);
			CodeBox.ColorChanger.ForceBackColor(backColor);
		}
	}
}

