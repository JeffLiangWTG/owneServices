using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using CargoWise.Types;
using CargoWise.Windows.UI;
#if WINZOR
using Enterprise.ZArchitecture;
#endif
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.LocalCartage.GUI
{
	/// <summary>Customized GroupBox representing a Tile.</summary> 	
	[Designer("System.Windows.Forms.Design.ParentControlDesigner, System.Design", typeof(IDesigner))]
	public class ZGroupBoxTile : ZUserControl
	{
		public ZGroupBoxTile()
		{
			InitializeStyles();
			InitializeTile();
			InitializePadding();
		}

		static class TileConstants
		{
			/// <summary>The sweep angle of the arc.</summary>
			public const int SweepAngle_Quarter = 90;
		}

		public enum TilesGradient
		{
			Horizontal = LinearGradientMode.Horizontal,
			Vertical = LinearGradientMode.Vertical,

			None = 10,
		}

		/// <summary>Back color of the Tile. </summary>
		[Category("Appearance"), Description("Back color of the control. (Default: Transparent)")]
		//[DefaultValue(Color.Transparent)]
		public override Color BackColor
		{
			get { return backColor; }
			set
			{
				backColor = value;
				Refresh();
			}
		}
		Color backColor = Color.Transparent;

		/// <summary>Show Title Bar.</summary>
		[Category("Appearance"), Description("Show Title Bar. (Default: False)")]
		[DefaultValue(false)]
		public bool ShowTitleBar
		{
			get { return showTitleBar; }
			set
			{
				showTitleBar = value;
				InitializePadding();
				Refresh();
			}
		}
		bool showTitleBar;

		/// <summary>16 x 16 image in the tile title bar.</summary>
		[Category("Appearance"), Description("16 x 16 image in the tile title bar.")]
		public Image TitleBarImage
		{
			get { return titleBarImage; }
			set
			{
				titleBarImage = value;
				Refresh();
			}
		}
		Image titleBarImage;

		/// <summary>TitleBarText.</summary>
		[Category("Appearance"), Description("TitleBarText. (Default: A Tile)")]
		[DefaultValue("A Tile")]
		public ZString TitleBarText
		{
			get { return titleBarText; }
			set
			{
				titleBarText = value;
				Refresh();
			}
		}
		ZString titleBarText = (NoResString)"A Tile";

		/// <summary>Tile Color, use with TileGradientColor for a gradient paint.</summary>
		[Category("Appearance"), Description("Tile Color, use with TileGradientColor for a gradient paint. (Default: White)")]
		//[DefaultValue(Color.White)]
		public Color TileColor
		{
			get { return tileColor; }
			set
			{
				tileColor = value;
				Refresh();
			}
		}
		Color tileColor = Color.White;

		/// <summary>Tile Gradient Color. Use with TileColor to create a gradient background.</summary>
		[Category("Appearance"), Description("Tile Gradient Color. Use with TileColor to create a gradient background. (Default: White)")]
		//[DefaultValue(Color.White)]
		public Color TileColorGradient
		{
			get { return tileGradientColor; }
			set
			{
				tileGradientColor = value;
				Refresh();
			}
		}
		Color tileGradientColor = Color.White;

		/// <summary>Gradient painting style.</summary>
		[Category("Appearance"), Description("Gradient painting style. (Default: None)")]
		[DefaultValue(TilesGradient.None)]
		public TilesGradient TileGradient
		{
			get { return tileGradient; }
			set
			{
				tileGradient = value;
				Refresh();
			}
		}
		TilesGradient tileGradient = TilesGradient.None;

		/// <summary>Corner radius.</summary>
		[Category("Appearance"), Description("Corner radius. (Default: 10)")]
		[DefaultValue(10)]
		[DpiState(DpiState.Unscaled)]
		public int RoundCorners
		{
			get { return roundCorners; }
			set
			{
				int min = 0;
				int max = 25;

				roundCorners = value < min ? min : value > max ? max : value;
				InitializePadding();

				Refresh();
			}
		}
		int roundCorners = 10;

		/// <summary>BorderColor.</summary>
		[Category("Appearance"), Description("BorderColor. (Default: Black)")]
		//[DefaultValue(Color.Black)]
		public Color BorderColor
		{
			get { return borderColor; }
			set
			{
				borderColor = value;
				Refresh();
			}
		}
		Color borderColor = Color.Black;

		/// <summary>BorderThickness. (Min: 1, Max: 3)</summary>
		[Category("Appearance"), Description("BorderThickness. (Default: 1, Min: 0, Max: 3)")]
		[DefaultValue(1)]
		public float BorderThickness
		{
			get { return borderThickness; }
			set
			{
				int min = 0;
				int max = 3;

				borderThickness = value < min ? min : value > max ? max : value;

				Refresh();
			}
		}
		float borderThickness = 1;

		[DpiState(DpiState.ScaleX)]
		protected int ScaledBorderThickness => ControlDpiScalingHelper.ScaleToCurrentDpiX((int)BorderThickness);

		/// <summary>Tile's shadow color.</summary>
		[Category("Appearance"), Description("Tile's shadow color. (Default: Dark Gray)")]
		//[DefaultValue(Color.DarkGray)]
		public Color ShadowColor
		{
			get { return shadowColor; }
			set
			{
				shadowColor = value;
				Refresh();
			}
		}
		Color shadowColor = Color.DarkGray;

		/// <summary>Shadow thickness.</summary>
		[Category("Appearance"), Description("Shadow thickness. (Default: 3)")]
		[DefaultValue(3)]
		public int ShadowThickness
		{
			get { return shadowThickness; }
			set
			{
				int min = 1;
				int max = 10;

				shadowThickness = value < min ? min : value > max ? max : value;

				InitializePadding();
				Refresh();
			}
		}
		int shadowThickness = 3;

		/// <summary>ShowTileShadow.</summary>
		[Category("Appearance"), Description("ShowTileShadow. (Default: False)")]
		[DefaultValue(false)]
		public bool ShowTileShadow
		{
			get { return showTileShadow; }
			set
			{
				showTileShadow = value;
				InitializePadding();
				Refresh();
			}
		}
		bool showTileShadow;

		/// <summary>ShowPanel.</summary>
		[Category("Appearance"), Description("ShowPanel. (Default: True)")]
		[DefaultValue(true)]
		public bool ShowPanel
		{
			get { return showPanel; }
			set
			{
				showPanel = value;
				Refresh();
			}
		}
		bool showPanel = true;

		/// <summary>PanelColor.</summary>
		[Category("Appearance"), Description("PanelColor. (Default: Transparent)")]
		//[DefaultValue(Color.Transparent)]
		public Color PanelColor
		{
			get { return panelColor; }
			set
			{
				panelColor = value;
				Refresh();
			}
		}
		Color panelColor = Color.Transparent;

		/// <summary>PanelColorGradient.</summary>
		[Category("Appearance"), Description("PanelColorGradient. (Default: Transparent)")]
		//[DefaultValue(Color.Transparent)]
		public Color PanelColorGradient
		{
			get { return panelColorGradient; }
			set
			{
				panelColorGradient = value;
				Refresh();
			}
		}
		Color panelColorGradient = Color.Transparent;

		/// <summary>PanelBorderColor.</summary>
		[Category("Appearance"), Description("PanelBorderColor. (Default: Black)")]
		//[DefaultValue(Color.Black)]
		public Color PanelBorderColor
		{
			get { return panelBorderColor; }
			set
			{
				panelBorderColor = value;
				Refresh();
			}
		}
		Color panelBorderColor = Color.Black;

		/// <summary>ShowBorderAsBottomLine.</summary>
		[Category("Appearance"), Description("ShowBorderAsBottomLine. (Default: False)")]
		[DefaultValue(false)]
		public bool ShowBorderAsBottomLine
		{
			get { return showBorderAsBottomLine; }
			set
			{
				showBorderAsBottomLine = value;
				Refresh();
			}
		}
		bool showBorderAsBottomLine;

		public Color[] TileColors
		{
			get { return TileColorsCore; }
		}

		protected virtual Color[] TileColorsCore
		{
			get { return new Color[] { TileColor, TileColorGradient }; }
		}

		protected virtual Color[] PanelColors
		{
			get { return new Color[] { PanelColor, PanelColorGradient }; }
		}

		public float[] TileColorPositions
		{
			get { return TileColorPositionsCore; }
		}

		protected virtual float[] TileColorPositionsCore
		{
			get { return new float[] { 0.5f }; }
		}

		protected virtual float[] PanelColorPositions
		{
			get { return new float[] { 0.5f }; }
		}

		protected virtual float BlendDistance
		{
			get { return 0.25f; }
		}

#if !WINZOR

		protected override void OnPaint(PaintEventArgs e)
		{
			if (SuspendRefreshSuspensionLevel == 0)
			{
				overriddenFont = OFont.GetFontBold();
				try
				{
					e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
					PaintBack(e.Graphics, e.ClipRectangle);
					if (e.ClipRectangle.Y < Padding.Top)
					{
						PaintTitleBar(e.Graphics);
					}
				}
				finally
				{
					overriddenFont = null;
				}
			}
		}

#endif

#if !WINZOR

		// Always Paint title in Bold Font
		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public override Font Font
		{
			get { return overriddenFont ?? base.Font; }
			set { base.Font = value; }
		}
		Font overriddenFont;

#endif

		public IDisposable SuspendRefresh()
		{
			return new SuspendRefreshSuspender(this);
		}

		sealed class SuspendRefreshSuspender : IDisposable
		{
			public SuspendRefreshSuspender(ZGroupBoxTile tile)
			{
				if (tile == null)
				{
					throw new ArgumentNullException(nameof(tile));
				}

				this.tile = tile;
				tile.suspendRefreshSuspensionLevel++;
			}

			public void Dispose()
			{
				if (!disposed)
				{
					disposed = true;
					tile.suspendRefreshSuspensionLevel--;
				}
			}

			bool disposed;

			readonly ZGroupBoxTile tile;
		}

		internal int SuspendRefreshSuspensionLevel
		{
			get { return suspendRefreshSuspensionLevel; }
		}
		int suspendRefreshSuspensionLevel;

		void InitializeStyles()
		{
			SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
			SetStyle(ControlStyles.AllPaintingInWmPaint, true);
			SetStyle(ControlStyles.UserPaint, true);
			SetStyle(ControlStyles.SupportsTransparentBackColor, true);
		}

#if WINZOR
		internal ZLabel titleLabel = new ZLabel();
#endif

		void InitializeTile()
		{
			components = new Container();
			Resize += new EventHandler(Tile_Resize);
			Name = (NoResString)"Tile";
#if WINZOR
			titleLabel.Location = ControlDpiScalingHelper.NewScaledPoint(5, 3, true);
			titleLabel.Size = ControlDpiScalingHelper.NewScaledSize(351, 16, true);
			titleLabel.Font = new Font(DefaultFont, FontStyle.Bold);
			titleLabel.BackColor = Color.FromArgb(0, 255, 255, 255);
			base.WinzorSpecificControls.Add(titleLabel);
			titleLabel.Text = TitleBarText.ToString();
#endif
		}

		void InitializePadding()
		{
			int sides = RoundCorners / 2 + 3;
			int top = ShowTitleBar ? 22 : sides;
			int allowForShadow = ShowTileShadow ? ShadowThickness : 0;

			ControlDpiScalingHelper.SetTop(DockPadding, top, true);
			ControlDpiScalingHelper.SetBottom(DockPadding, sides + allowForShadow, true);
			ControlDpiScalingHelper.SetRight(DockPadding, sides + allowForShadow, true);
			ControlDpiScalingHelper.SetLeft(DockPadding, sides, true);
		}

		void Tile_Resize(object sender, EventArgs e)
		{
			Refresh();
		}

#if !WINZOR

		void PaintBack(Graphics g, Rectangle c)
		{
			int canvasWidth = Width - ControlDpiScalingHelper.OnePixel;
			int canvasHeight = Height - ControlDpiScalingHelper.OnePixel;
			int allowForShadow = ShowTileShadow ? ShadowThickness : 0;

			// Shadow
			float sx1 = ShadowThickness;
			float sy1 = ShadowThickness;
			float sx2 = canvasWidth;
			float sy2 = canvasHeight;

			// Tile
			float tx1 = 0;
			float ty1 = 0;
			float tx2 = canvasWidth - allowForShadow;
			float ty2 = canvasHeight - allowForShadow;

			// Panel
			int panelBorderThickness = 1;
			float px1 = Padding.Left - panelBorderThickness;
			float py1 = Padding.Top - panelBorderThickness;
			float px2 = canvasWidth - Padding.Right + panelBorderThickness;
			float py2 = canvasHeight - Padding.Bottom + panelBorderThickness;

			bool isClipInsidePanel = px1 <= c.Left && py1 <= c.Top && px2 >= c.Right && py2 >= c.Bottom;
			if (!isClipInsidePanel)
			{
				if (ShowTileShadow)
				{
					DrawFilledRectangle(g, sx1, sy1, sx2, sy2, RoundCorners, ShadowColor);
				}

				// Tile
				DrawFilledRectangle(g, tx1, ty1, tx2, ty2, RoundCorners, TileGradient, BorderColor, BorderThickness, BlendDistance, TileColorPositions, TileColors);
			}

			// Inside Panel
			if (ShowPanel)
			{
				DrawFilledRectangle(g, px1, py1, px2, py2, 0, TilesGradient.Horizontal, PanelBorderColor, panelBorderThickness, BlendDistance, PanelColorPositions, PanelColors);
			}
		}

		void PaintTitleBar(Graphics g)
		{
			if (!ShowTitleBar || TitleBarText == "")
			{
				return;
			}

			SolidBrush textColorBrush = GetBrush(ForeColor);
			int textX = TitleBarImage != null ? 25 : 5;
			int textY = 4;
			TextRendererHelper.DrawText(g, titleBarText, Font, textX, textY, textColorBrush);

			if (TitleBarImage != null)
			{
				g.DrawImage(TitleBarImage, 5, 3, 16, 16);
			}
		}

		void DrawFilledRectangle(Graphics g, float x1, float y1, float x2, float y2, int cornerRadius, Color fillColor)
		{
			DrawFilledRectangle(g, x1, y1, x2, y2, cornerRadius, TilesGradient.None, Color.Transparent, 0, 0, null, new Color[] { fillColor });
		}

		void DrawFilledRectangle(Graphics g, float x1, float y1, float x2, float y2, int cornerRadius, TilesGradient gradient, Color borderRGB, float borderWidth, float blendDistance, float[] colorPositions, Color[] colors)
		{
			GraphicsPath path = null;
			int cornerDiameter = cornerRadius * 2;

			if (cornerRadius > 0)
			{
				path = new GraphicsPath();
				AddRoundTopLeft(path, x1, y1, cornerDiameter);
				AddRoundTopRight(path, y1, x2, cornerDiameter);
				AddRoundBottomRight(path, x2, y2, cornerDiameter);
				AddRoundBottomLeft(path, x1, y2, cornerDiameter);
				path.CloseAllFigures();
			}
			else
			{
				path = new GraphicsPath();
				path.AddRectangle(new RectangleF(x1, y1, x2 - x1, y2 - y1));
			}

			if (gradient == TilesGradient.None)
			{
				SolidBrush brush = GetBrush(colors[0]);
				g.FillPath(brush, path);
			}
			else
			{
				PointF topLeft = new PointF(x1, y1);
				PointF bottomRight = new PointF(x2, y2);

				List<Color> colorBlends = new List<Color>();
				foreach (var color in colors)
				{
					colorBlends.Add(color);
					colorBlends.Add(color);
				}

				ColorBlend blend = new ColorBlend();
				blend.Colors = colorBlends.ToArray();

				PointF p1, p2;

				if (gradient == TilesGradient.Horizontal)
				{
					p1 = new PointF(topLeft.X, topLeft.Y + (bottomRight.Y / 2));    // left
					p2 = new PointF(bottomRight.X, p1.Y);                           // right
				}
				else // if (gradient == TileGradients.Vertical)
				{
					p1 = new PointF(topLeft.X + (bottomRight.X / 2), topLeft.Y);    // top
					p2 = new PointF(p1.X, bottomRight.Y);                           // bottom
				}

				List<float> positions = new List<float>();
				positions.Add(0f);
				foreach (var position in colorPositions)
				{
					positions.Add(position - blendDistance);
					positions.Add(position + blendDistance);
				}
				positions.Add(1.0f);

				blend.Positions = positions.ToArray();
				using (var gradientBrush = new LinearGradientBrush(p1, p2, Color.White, Color.Black))
				{
					gradientBrush.InterpolationColors = blend;
					g.FillPath(gradientBrush, path);
				}
			}

			// Border
			if (borderWidth > 0)
			{
				if (ShowBorderAsBottomLine)
				{
					using (var linePath = new GraphicsPath())
					{
						linePath.AddLine(x1, y2, x2, y2);
						Brush lineBrush = GetBrush(borderRGB);
						using (var linePen = new Pen(lineBrush, 1))
						{
							g.DrawPath(linePen, linePath);
						}
					}
				}
				else
				{
					Brush borderBrush = GetBrush(borderRGB);
					using (var borderPen = new Pen(borderBrush, borderWidth))
					{
						g.DrawPath(borderPen, path);
					}
				}
			}

			path.Dispose();
		}

		void AddRoundTopLeft(GraphicsPath path, float x1, float y1, int cornerDiameter)
		{
			path.AddArc(x1, y1, cornerDiameter, cornerDiameter, 180, TileConstants.SweepAngle_Quarter);
		}

		void AddRoundTopRight(GraphicsPath path, float y1, float x2, int cornerDiameter)
		{
			path.AddArc(x2 - cornerDiameter, y1, cornerDiameter, cornerDiameter, 270, TileConstants.SweepAngle_Quarter);
		}

		void AddRoundBottomLeft(GraphicsPath path, float x1, float y2, int cornerDiameter)
		{
			path.AddArc(x1, y2 - cornerDiameter, cornerDiameter, cornerDiameter, 90, TileConstants.SweepAngle_Quarter);
		}

		void AddRoundBottomRight(GraphicsPath path, float x2, float y2, int cornerDiameter)
		{
			path.AddArc(x2 - cornerDiameter, y2 - cornerDiameter, cornerDiameter, cornerDiameter, 360, TileConstants.SweepAngle_Quarter);
		}

#endif

		Container components;

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				foreach (Brush brush in Brushes.Values)
				{
					brush.Dispose();
				}

				if (components != null)
				{
					components.Dispose();
				}
			}

			base.Dispose(disposing);
		}

#if !WINZOR

		SolidBrush GetBrush(Color color)
		{
			SolidBrush result;

			if (!Brushes.TryGetValue(color, out result))
			{
				result = new SolidBrush(color);
				Brushes.Add(color, result);
			}

			return result;
		}

#endif

		Dictionary<Color, SolidBrush> Brushes
		{
			get { return brushes ?? (brushes = new Dictionary<Color, SolidBrush>()); }
		}
		Dictionary<Color, SolidBrush> brushes;
	}
}
