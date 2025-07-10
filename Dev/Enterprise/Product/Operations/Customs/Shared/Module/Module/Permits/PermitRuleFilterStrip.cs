using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.Module
{
	public partial class PermitRuleFilterStrip : ZUserControl
	{
		public PermitRuleFilterStrip()
		{
			InitializeComponent();
		}

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);
			var filter = CurrentDataItem as PermitRuleModuleFilter;
			if (filter != null)
			{
				filter.Property1Info.ValueChanged += Property1Info_ValueChanged;
				Property1Info_ValueChanged(null, null);
			}
		}

		void Property1Info_ValueChanged(object sender, EventArgs e)
		{
			var filter = CurrentDataItem as PermitRuleModuleFilter;
			if (filter != null)
			{
				Property2Text.GetExtension<ILabelCaptionRenderer>().Caption = filter.PermitRuleCodes.GetShortDescriptionFromCode(filter.Property1);
				Property2TextDropEdit.GetExtension<ILabelCaptionRenderer>().Caption = filter.PermitRuleCodes.GetShortDescriptionFromCode(filter.Property1);
				Property2TextCodeFindBox.GetExtension<ILabelCaptionRenderer>().Caption = filter.PermitRuleCodes.GetShortDescriptionFromCode(filter.Property1);
				ChangeControlsVisibility(filter);
			}
		}

		protected void ChangeControlsVisibility(PermitRuleModuleFilter filter)
		{
			var controlsToHide = Controls.OfType<Control>().Where(x => x.Name.StartsWith("Property2", StringComparison.Ordinal));
			foreach (var controlToHide in controlsToHide)
			{
				controlToHide.Visible = false;
			}

			var foundControlToShow = false;

			var controlToShow = Controls.Find("Property2" + filter.Property2_FieldType, true)?.FirstOrDefault();
			if (controlToShow != null)
			{
				controlToShow.Visible = true;
				foundControlToShow = true;
			}

			if (!foundControlToShow)
			{
				Property2Text.Visible = true;
			}
		}
	}
}
