using System.Linq;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.MasterFiles.Module;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;

namespace Enterprise.Freight.Module
{
	public class WorkflowFilterStripWithRoutingSupport : WorkflowFilterStrip
	{
		protected override Control[] GetCurrentFilterControls(ModuleFilter currentModuleFilter)
		{
			Control[] result;
			if (currentModuleFilter is WorkflowModuleFilterWithRoutingSupport)
			{
				var filter = currentModuleFilter as WorkflowModuleFilterWithRoutingSupport;
				var strip = new WorkflowFilterStripControlWithRoutingSupport(this)
				{
					DatesToFilterDropEdit = { Visible = filter.UseDatesToFilter }
				};

				FilterControlBindingSource.SetBindingMember(strip, ".");
				result = new Control[] { strip };
				PreferredHeight = result[0].Height + ControlDpiScalingHelper.OnePixel;
			}
			else if (currentModuleFilter is WorkflowModuleTextFilterWithRoutingSupport)
			{
				result = GetWorkflowStringFilterControlsWithRouting();
				PreferredHeight = ControlDpiScalingHelper.ScaleToCurrentDpiY(67);
			}
			else
			{
				result = base.GetCurrentFilterControls(currentModuleFilter);
			}

			return result;
		}

		Control[] GetWorkflowStringFilterControlsWithRouting()
		{
			var controls = GetWorkflowStringFilterControls().ToList();

			ZLabel originLabel = new ZLabel();
			originLabel.AutoSize = true;
			originLabel.Location = ControlDpiScalingHelper.NewScaledPoint(292, 50);
			originLabel.Name = "OriginLabel";
			originLabel.Size = ControlDpiScalingHelper.NewScaledSize(50, 13);
			originLabel.CaptionResourceString = Res.GetData("WorkflowStringFilterControls|Origin", "Origin:");

			ZLabel destinationLabel = new ZLabel();
			destinationLabel.AutoSize = true;
			destinationLabel.Location = ControlDpiScalingHelper.NewScaledPoint(447, 50);
			destinationLabel.Name = "DestinationLabel";
			destinationLabel.Size = ControlDpiScalingHelper.NewScaledSize(63, 13);
			destinationLabel.CaptionResourceString = Res.GetData("WorkflowStringFilterControls|Destination", "Destination:");

			ZCodeFindBox originFindbox = new ZCodeFindBox();
			originFindbox.BindTo = "Origin";
			originFindbox.Location = ControlDpiScalingHelper.NewScaledPoint(347, 47);
			originFindbox.Name = "OriginFindbox";
			originFindbox.ShowDescriptionBox = false;
			originFindbox.Size = ControlDpiScalingHelper.NewScaledSize(96, 20);
			originFindbox.TabIndex = 4;
			FilterControlBindingSource.SetBindingMember(originFindbox, originFindbox.BindTo);
			MissingResourceStringChecker.ExcludeFromTest(originFindbox);

			ZCodeFindBox destinationFindBox = new ZCodeFindBox();
			destinationFindBox.BindTo = "Destination";
			destinationFindBox.Location = ControlDpiScalingHelper.NewScaledPoint(520, 47);
			destinationFindBox.Name = "DestinationFindBox";
			destinationFindBox.ShowDescriptionBox = false;
			destinationFindBox.Size = ControlDpiScalingHelper.NewScaledSize(96, 20);
			destinationFindBox.TabIndex = 5;
			FilterControlBindingSource.SetBindingMember(destinationFindBox, destinationFindBox.BindTo);
			MissingResourceStringChecker.ExcludeFromTest(destinationFindBox);

			controls.Add(originLabel);
			controls.Add(destinationLabel);
			controls.Add(originFindbox);
			controls.Add(destinationFindBox);

			return controls.ToArray();
		}
	}
}
