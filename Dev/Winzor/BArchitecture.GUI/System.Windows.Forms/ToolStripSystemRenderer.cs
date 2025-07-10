using System.Drawing;
using System.Windows.Forms.VisualStyles;

namespace System.Windows.Forms;

public class ToolStripSystemRenderer : ToolStripRenderer
{
	ToolStripRenderer? _toolStripHighContrastRenderer;

	public ToolStripSystemRenderer()
	{
	}

	internal ToolStripSystemRenderer(bool isDefault) : base(isDefault)
	{
	}

	internal override ToolStripRenderer? RendererOverride
	{
		get
		{
			if (DisplayInformation.HighContrast)
			{
				return HighContrastRenderer;
			}

			return null;
		}
	}

	internal ToolStripRenderer HighContrastRenderer
	{
		get
		{
			// If system in high contrast mode 'false' flag should be passed to render filled selected button background. This is in consistence with ToolStripProfessionalRenderer.
			_toolStripHighContrastRenderer ??= new ToolStripHighContrastRenderer(systemRenderMode: false);

			return _toolStripHighContrastRenderer;
		}
	}

	internal static void UpdateBackColor(IRendererControlledElement rControl, Color backColor)
	{
		rControl.RendererControlledBackColor = backColor;
	}

	/// <summary>
	///  translates the ToolStrip item state into a toolbar state, which is something the renderer understands
	/// </summary>
	static ToolBarState GetToolBarState(ToolStripItem item)
	{
		ToolBarState state = ToolBarState.Normal;

		if (!item.Enabled)
		{
			state = ToolBarState.Disabled;
		}

		if (item is ToolStripButton toolStripButton && toolStripButton.Checked)
		{
			if (toolStripButton.Selected)
			{
				state = ToolBarState.Hot; // we'd prefer HotChecked here, but Color Theme uses the same color as Checked
			}
			else
			{
				state = ToolBarState.Checked;
			}
		}
		else if (item.Pressed)
		{
			state = ToolBarState.Pressed;
		}
		else if (item.Selected)
		{
			state = ToolBarState.Hot;
		}

		return state;
	}

	/// <summary>
	///  Draw the ToolStrip background. ToolStrip users should override this if they want to draw differently.
	/// </summary>
	protected override void OnRenderToolStripBackground(ToolStripRenderEventArgs e)
	{
		ToolStrip toolStrip = e.ToolStrip;

		if (!ShouldPaintBackground(toolStrip))
		{
			return;
		}

		if (toolStrip is StatusStrip)
		{
			RenderStatusStripBackground(e);
		}
		else
		{
			if (DisplayInformation.HighContrast)
			{
				UpdateBackColor(toolStrip, SystemColors.ButtonFace);
			}
			else if (DisplayInformation.LowResolution)
			{
				UpdateBackColor(toolStrip, (toolStrip is ToolStripDropDown) ? SystemColors.ControlLight : e.BackColor);
			}
			else if (toolStrip.IsDropDown)
			{
				UpdateBackColor(toolStrip, (!ToolStripManager.VisualStylesEnabled) ?
									 e.BackColor : SystemColors.Menu);
			}
			else if (toolStrip is MenuStrip)
			{
				UpdateBackColor(toolStrip, (!ToolStripManager.VisualStylesEnabled) ?
										   e.BackColor : SystemColors.MenuBar);
			}
			else
			{
				UpdateBackColor(toolStrip, (!ToolStripManager.VisualStylesEnabled) ?
										   e.BackColor : SystemColors.MenuBar);
			}
		}
	}

	/// <summary>
	///  Draw the button background
	/// </summary>
	protected override void OnRenderButtonBackground(ToolStripItemRenderEventArgs e)
	{
		// If system in high contrast mode and specific renderer override is defined, use that.
		// For ToolStripSystemRenderer in High Contrast mode the RendererOverride property will be ToolStripHighContrastRenderer.
		if (RendererOverride is not null)
		{
			base.OnRenderButtonBackground(e);
			return;
		}

		RenderItemInternal(e);
	}

	protected override void OnRenderDropDownButtonBackground(ToolStripItemRenderEventArgs e)
	{
		// If system in high contrast mode and specific renderer override is defined, use that.
		// For ToolStripSystemRenderer in High Contrast mode the RendererOverride property will be ToolStripHighContrastRenderer.
		if (RendererOverride is not null)
		{
			base.OnRenderDropDownButtonBackground(e);
			return;
		}

		RenderItemInternal(e);
	}

	protected override void OnRenderOverflowButtonBackground(ToolStripItemRenderEventArgs e)
	{
		RenderItemInternal(e);
	}

	protected override void OnRenderSplitButtonBackground(ToolStripItemRenderEventArgs e)
	{
		// If system in high contrast mode and specific renderer override is defined, use that.
		// For ToolStripSystemRenderer in High Contrast mode the RendererOverride property will be ToolStripHighContrastRenderer.
		if (RendererOverride is not null)
		{
			base.OnRenderSplitButtonBackground(e);
			return;
		}

		ToolStripSplitButton? splitButton = e.Item as ToolStripSplitButton;
		if (splitButton is null)
		{
			return;
		}

		UpdateBackColor(splitButton, splitButton.BackColor);
	}

	static void RenderItemInternal(ToolStripItemRenderEventArgs e)
	{
		ToolStripItem item = e.Item;
		ToolBarState state = GetToolBarState(item);
		ToolStrip? parent = item.GetCurrentParent();

		if ((parent is not null) && (state != ToolBarState.Checked) && (item.BackColor != parent.BackColor))
		{
			UpdateBackColor(item, item.BackColor);
		} else
		{
			// In Winforms, item background will not be drawn under this condition 
			// So this is to revert the BackColor style auto-injected by our ControlStyleExtension
			UpdateBackColor(item, Color.FromArgb(0, 255, 255, 255));
		}
	}

	static void RenderStatusStripBackground(ToolStripRenderEventArgs e)
	{
	}
}
