using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.Module
{
	public class ImporterMatchDropEdit : ZDropEdit
	{
		public ImporterMatchDropEdit(Font font)
		{
			CodeBox.TextAlign = HorizontalAlignment.Center;
			CodeBox.Font = font;
		}
	}

	public class ImporterSupplierFilterStrip : ZFilterStrip
	{
		protected override Control[] GetCurrentFilterControls(ModuleFilter currentModuleFilter)
		{
			if (currentModuleFilter is ABCCategoryWarehouseFilter)
			{
				return new Control[] { new ABCCategoryWarehouseFilterControl() };
			}

			var controls = base.GetCurrentFilterControls(currentModuleFilter);
			if (currentModuleFilter is ImporterSupplierModuleGuidsWithListFilter)
			{
				var newControls = new List<Control>();
				((IResCaptionedControl)controls[0]).CaptionResourceString = Res.GetData("f560958e-6e17-4515-be31-d8614d3b4983", "Imp.");
				((IResCaptionedControl)controls[1]).CaptionResourceString = Res.GetData("e5799e72-2642-448f-86c5-ff69bb928a62", "Sup.");

				controls[0].Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(331, 1);
				controls[1].Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(481, 1);
				newControls.Add(GetDropDownListControl());
				newControls.AddRange(controls);
				controls = newControls.ToArray();
			}

			return controls;
		}

		Control GetDropDownListControl()
		{
			ImporterMatchDropEdit dropEdit = new ImporterMatchDropEdit(ComparisonOperatorFont);
			dropEdit.BindTo = "FilterCondition";
			dropEdit.TabIndex = 1;
			ControlDpiScalingHelper.SetLeft(ref dropEdit, FilterComparisonOperatorBoxStart, false);
			dropEdit.PreBoundMaxLength = FilterComparisonOperatorBoxPreBoundMaxLength;
			ControlDpiScalingHelper.SetTop(ref dropEdit, FilterControlTop, false);
			dropEdit.ShowDescriptionBox = false;
			dropEdit.CharacterCasing = CharacterCasing.Lower;
			FilterControlBindingSource.SetBindingMember(dropEdit, dropEdit.BindTo);
			return dropEdit;
		}
	}
}
