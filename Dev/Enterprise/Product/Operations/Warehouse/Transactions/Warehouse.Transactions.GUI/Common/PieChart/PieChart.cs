using System;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms.DataVisualization.Charting;

using CargoWise.Windows.UI;
#pragma warning disable WTG1001 // Do not use the 'private' keyword.

namespace Enterprise.Warehouse.Transactions.GUI.Common
{
	[ToolboxItem(true)]
	public class ZChart : KChart
	{
		#region Overrides

		#region OnParentBackColorChanged

		protected override void OnParentBackColorChanged(EventArgs e)
		{
			base.OnParentBackColorChanged(e);
			BackColor = Parent.BackColor;
		}

		#endregion

		#region BackColor

		public override Color BackColor
		{
			get { return base.BackColor; }
			set
			{
				base.BackColor = value;

				Array.ForEach(ChartAreas.ToArray(), a => a.BackColor = value);
				Array.ForEach(Legends.ToArray(), l => l.BackColor = value);
			}
		}

		#endregion

		#endregion

		#region New Properties

		#region ChartStyle

		/// <summary>
		/// Sets up Chart Style Defaults.
		/// </summary>
		[Browsable(true), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), DefaultValue(ChartStyle.UserDefined)]
		[Description("Sets up Chart Style Defaults.")]
		public ChartStyle ChartStyle
		{
			get { return ChartStylePersisted; }
			set
			{
				ChartStylePersisted = value;
				OnChartTypeChanged();
			}
		}

		#endregion

		#region ChartStylePersisted

		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Visible), DefaultValue(ChartStyle.UserDefined)]
		public ChartStyle ChartStylePersisted
		{
			get { return chartTypePersisted; }
			set { chartTypePersisted = value; }
		}
		ChartStyle chartTypePersisted;

		#endregion

		#endregion

		#region OnChartTypeChanged

		void OnChartTypeChanged()
		{
			if (ChartStyle != ChartStyle.UserDefined)
			{
				Array.ForEach(Series.ToArray(), s => Series.Remove(s));
				Array.ForEach(ChartAreas.ToArray(), c => ChartAreas.Remove(c));
				Array.ForEach(Legends.ToArray(), l => Legends.Remove(l));

				switch (ChartStyle)
				{
					case ChartStyle.Pie3D:
						Create3DPieChart();
						break;

					default:
						break;
				}
			}
		}

		void Create3DPieChart()
		{
			var series = Series.Add("Pie3D");
			series.ChartType = SeriesChartType.Pie;

			var area = ChartAreas.Add("Pie3D");
			area.Visible = true;
			area.Area3DStyle.Enable3D = true;
			area.Area3DStyle.Inclination = 45;
			area.Area3DStyle.PointDepth = 200;
			area.BackColor = BackColor;

			var legend = Legends.Add("Pie3D");
			legend.BackColor = BackColor;
		}

		#endregion

		#region UpdateLegendColoursAndData

		public void UpdateLegendColoursAndData(ChartDataPoint[] points)
		{
			var series = Series[0];

			for (var i = 0; i < points.Length; i++)
			{
				var point = points[i];
				var chartPoint = (series.Points.Count <= i) ? series.Points.Add(point.Data) : series.Points[i];
				chartPoint.SetValueY(point.Data);
				chartPoint.Color = point.Color;
				chartPoint.LegendText = point.Legend;
			}
		}

		#endregion
	}
}
