using System.Drawing;
using System.Windows.Forms.Layout;
using WinzorFramework.Extensions;

namespace System.Windows.Forms;

public class UserControl : ContainerControl
{
	BorderStyle _borderStyle = BorderStyle.None;

	public AutoSizeMode AutoSizeMode
	{
		get => GetAutoSizeMode();
		set
		{
			if (GetAutoSizeMode() != value)
			{
				SetAutoSizeMode(value);
				var toLayout = DesignMode || Parent is null ? this : Parent;

				if (toLayout is not null)
				{
					// DefaultLayout does not keep anchor information until it needs to.  When
					// AutoSize became a common property, we could no longer blindly call into
					// DefaultLayout, so now we do a special InitLayout just for DefaultLayout.
					if (toLayout.LayoutEngine == DefaultLayout.Instance)
					{
						toLayout.LayoutEngine.InitLayout(this, BoundsSpecified.Size);
					}

					LayoutTransaction.DoLayout(toLayout, this, PropertyNames.AutoSize);
				}
			}
		}
	}

	public BorderStyle BorderStyle
	{
		get => _borderStyle;
		set
		{
			if (UpdateProperty(ref _borderStyle, value))
			{
				UpdateStyles();
				UpdateBounds(Bounds.X, Bounds.Y, Bounds.Width, Bounds.Height);
			}
		}
	}

	internal override void AdjustWindowRectEx(ref Interop.RECT rect)
	{
		base.AdjustWindowRectEx(ref rect);

		if (BorderStyle == BorderStyle.FixedSingle)
		{
			rect = new Interop.RECT(rect.left - 1, rect.top - 1, rect.right + 1, rect.bottom + 1);
		}
		else if (BorderStyle == BorderStyle.Fixed3D)
		{
			rect = new Interop.RECT(rect.left - 2, rect.top - 2, rect.right + 2, rect.bottom + 2);
		}
	}

	protected override void OnCreateControl()
	{
		base.OnCreateControl();
		OnLoad(EventArgs.Empty);
	}

	protected virtual void OnLoad(EventArgs e)
	{
		Load?.Invoke(this, e);
	}

	/// <summary>
	///  The default size for this user control.
	/// </summary>
	protected override Size DefaultSize => new Size(150, 150);

	public event EventHandler? Load;

	protected override void WmSetFocus()
	{
		if (ActiveControl is null)
		{
			SelectNextControl(null, true, true, true, false);
		}

		if (!ValidationCancelled)
		{
			base.WmSetFocus();
		}
	}

	protected override void OnMouseDown(MouseEventArgs e)
	{
		if (!ContainsFocus)
		{
			Focus();
		}
		base.OnMouseDown(e);
	}

	string BorderStyleClass
	{
		get => BorderStyle switch
		{
			BorderStyle.None => string.Empty,
			BorderStyle.FixedSingle => "usercontrol--border-fixedsingle",
			BorderStyle.Fixed3D => "usercontrol--border-fixed3d",
			_ => string.Empty,
		};
	}

	protected override string ClassName
	{
		get
		{
			string result = BorderStyleClass;

			string baseClassName = base.ClassName;
			if (!string.IsNullOrEmpty(baseClassName))
			{
				result += $" {baseClassName}";
			}

			string backgroundLayoutClass = this.BackgroundImageLayoutClass();
			if (!string.IsNullOrEmpty(backgroundLayoutClass))
			{
				result += $" {backgroundLayoutClass}";
			}

			return result;
		}
	}
}
