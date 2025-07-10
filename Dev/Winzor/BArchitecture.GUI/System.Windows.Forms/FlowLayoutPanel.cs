using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Forms.Layout;

namespace System.Windows.Forms;

public class FlowLayoutPanel : Panel, IExtenderProvider
{
	readonly FlowLayoutSettings flowLayoutSettings;

	public FlowLayoutPanel()
	{
		flowLayoutSettings = new FlowLayoutSettings(this);
	}

	public override LayoutEngine LayoutEngine => FlowLayout.Instance;

	public FlowDirection FlowDirection
	{
		get => flowLayoutSettings.FlowDirection;
		set
		{
			flowLayoutSettings.FlowDirection = value;
			Debug.Assert(FlowDirection == value, "FlowDirection should be the same as we set it");
			NotifyRenderRequired();
		}
	}

	public bool WrapContents
	{
		get => flowLayoutSettings.WrapContents;
		set
		{
			flowLayoutSettings.WrapContents = value;
			Debug.Assert(WrapContents == value, "WrapContents should be the same as we set it");
			NotifyRenderRequired();
		}
	}

	bool IExtenderProvider.CanExtend(object obj) => obj is Control control && control.Parent == this;

	public bool GetFlowBreak(Control control)
	{
		ArgumentNullException.ThrowIfNull(control);

		return flowLayoutSettings.GetFlowBreak(control);
	}

	public void SetFlowBreak(Control control, bool value)
	{
		ArgumentNullException.ThrowIfNull(control);

		flowLayoutSettings.SetFlowBreak(control, value);
		NotifyRenderRequired();
	}
}
