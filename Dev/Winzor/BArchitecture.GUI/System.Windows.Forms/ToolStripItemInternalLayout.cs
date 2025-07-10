using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Windows.Forms.ButtonInternal;
using System.Windows.Forms.Layout;
using static System.Windows.Forms.ButtonInternal.ButtonBaseAdapter;

namespace System.Windows.Forms;

public partial class ToolStripItem
{
	internal partial class ToolStripItemInternalLayout
	{
		ToolStripItemLayoutOptions? currentLayoutOptions;
		readonly ToolStripItem ownerItem;
		const int BorderWidth = 2;
		LayoutData? layoutData;
		ToolStripLayoutData? parentLayoutData;

		protected virtual ToolStripItem Owner => ownerItem;
		public Size PreferredImageSize => Owner.PreferredImageSize;

		Size lastPreferredSize = new Size(int.MinValue, int.MinValue);

		public ToolStripItemInternalLayout(ToolStripItem ownerItem)
		{
			this.ownerItem = ownerItem ?? throw new ArgumentNullException(nameof(ownerItem));
		}

		public virtual Rectangle ImageRectangle
		{
			get
			{
				Rectangle imageRect = LayoutData.ImageBounds;
				imageRect.Intersect(LayoutData.Field);
				return imageRect;
			}
		}

		protected virtual ToolStrip? ParentInternal => ownerItem?.ParentInternal;

		public virtual Size GetPreferredSize(Size proposedSize)
		{
			Size preferredSize = Size.Empty;
			if (currentLayoutOptions == null)
			{
				currentLayoutOptions = CommonLayoutOptions();
			}
			// we would prefer not to be larger than the ToolStrip itself.
			// so we'll ask the ButtonAdapter layout guy what it thinks
			// its preferred size should be - and we'll tell it to be no
			// bigger than the ToolStrip itself.  Note this is "Parent" not
			// "Owner" because we care in this instance what we're currently displayed on.

			if (ownerItem is not null)
			{
				lastPreferredSize = currentLayoutOptions.GetPreferredSizeCore(proposedSize);
				return lastPreferredSize;
			}

			return Size.Empty;
		}

		protected virtual ToolStripItemLayoutOptions CommonLayoutOptions()
		{
			ToolStripItemLayoutOptions layoutOptions = new ToolStripItemLayoutOptions();
			Rectangle bounds = new Rectangle(Point.Empty, ownerItem.Size);

			layoutOptions.Client = bounds;

			layoutOptions.GrowBorderBy1PxWhenDefault = false;

			layoutOptions.BorderSize = BorderWidth;
			layoutOptions.PaddingSize = 0;
			layoutOptions.MaxFocus = true;
			layoutOptions.FocusOddEvenFixup = false;
			layoutOptions.Font = Owner.Font;
			layoutOptions.Text = ((Owner.DisplayStyle & ToolStripItemDisplayStyle.Text) == ToolStripItemDisplayStyle.Text) ? Owner.Text : string.Empty;
			layoutOptions.ImageSize = PreferredImageSize;
			layoutOptions.CheckSize = 0;
			layoutOptions.CheckPaddingSize = 0;
			layoutOptions.CheckAlign = ContentAlignment.TopLeft;
			layoutOptions.ImageAlign = Owner.ImageAlign;
			layoutOptions.TextAlign = Owner.TextAlign;
			layoutOptions.HintTextUp = false;
			layoutOptions.LayoutRTL = RightToLeft.Yes == Owner.RightToLeft;
			layoutOptions.TextImageRelation = Owner.TextImageRelation;
			// Set textImageInset to 0 since we don't draw 3D border for ToolStripItems.
			layoutOptions.TextImageInset = 0;
			layoutOptions.DotNetOneButtonCompat = false;
			// Support RTL
			layoutOptions.GdiTextFormatFlags = ContentAlignToTextFormat(Owner.TextAlign, Owner.RightToLeft == RightToLeft.Yes);

			return layoutOptions;
		}

		internal static TextFormatFlags ContentAlignToTextFormat(ContentAlignment alignment, bool rightToLeft)
		{
			TextFormatFlags textFormat = TextFormatFlags.Default;
			if (rightToLeft)
			{
				//We specifically do not want to turn on TextFormatFlags.Right.
				textFormat |= TextFormatFlags.RightToLeft;
			}

			// Calculate Text Positioning
			textFormat |= ControlPaint.TranslateAlignmentForGDI(alignment);
			textFormat |= ControlPaint.TranslateLineAlignmentForGDI(alignment);
			return textFormat;
		}

		internal class ToolStripItemLayoutOptions : LayoutOptions
		{
		Size _cachedSize = LayoutUtils.s_invalidSize;
		Size _cachedProposedConstraints = LayoutUtils.s_invalidSize;

			// override GetTextSize to provide simple text caching.
			protected override Size GetTextSize(Size proposedConstraints)
			{
				if (_cachedSize != LayoutUtils.s_invalidSize
					&& (_cachedProposedConstraints == proposedConstraints
					|| _cachedSize.Width <= proposedConstraints.Width))
				{
					return _cachedSize;
				}

				_cachedSize = base.GetTextSize(proposedConstraints);
				_cachedProposedConstraints = proposedConstraints;
				return _cachedSize;
			}
		}

		[MemberNotNull(nameof(layoutData))]
		internal void PerformLayout()
		{
			layoutData = GetLayoutData();
			ToolStrip? parent = ParentInternal;

			if (parent is not null)
			{
				parentLayoutData = new ToolStripLayoutData(parent);
			}
			else
			{
				parentLayoutData = null;
			}
		}

		internal LayoutData LayoutData
		{
			get
			{
				EnsureLayout();
				return layoutData;
			}
		}

		LayoutData GetLayoutData()
		{
			currentLayoutOptions = CommonLayoutOptions();

			if (Owner.TextDirection != ToolStripTextDirection.Horizontal)
			{
				currentLayoutOptions.VerticalText = true;
			}

			LayoutData data = currentLayoutOptions.Layout();
			return data;
		}

		[MemberNotNull(nameof(layoutData))]
		bool EnsureLayout()
		{
			if (layoutData is null || parentLayoutData is null || !parentLayoutData.IsCurrent(ParentInternal))
			{
				PerformLayout();
				return true;
			}

			return false;
		}
	}
}
