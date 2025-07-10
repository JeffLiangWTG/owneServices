using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Warehouse.Transactions.GUI.Common.Testing
{
	public class TestUserControlWithPieChart : ZUserControl
	{
		public TestUserControlWithPieChart()
			: base()
		{
			InitializeComponent();
		}

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		void InitializeComponent()
		{
			System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea1 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
			System.Windows.Forms.DataVisualization.Charting.Legend legend1 = new System.Windows.Forms.DataVisualization.Charting.Legend();
			System.Windows.Forms.DataVisualization.Charting.Series series1 = new System.Windows.Forms.DataVisualization.Charting.Series();
			this.PieChart = new ZChart();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.PieChart)).BeginInit();
			this.SuspendLayout();
			// 
			// PieChart
			// 
			this.PieChart.BackColor = System.Drawing.Color.Empty;
			chartArea1.Area3DStyle.Enable3D = true;
			chartArea1.Area3DStyle.Inclination = 45;
			chartArea1.Area3DStyle.PointDepth = 200;
			chartArea1.Name = "Pie3D";
			this.PieChart.ChartAreas.Add(chartArea1);
			this.PieChart.ChartStylePersisted = Enterprise.Warehouse.Transactions.GUI.Common.ChartStyle.Pie3D;
			this.PieChart.Cursor = System.Windows.Forms.Cursors.SizeAll;
			this.PieChart.Dock = System.Windows.Forms.DockStyle.Fill;
			legend1.Name = "Pie3D";
			this.PieChart.Legends.Add(legend1);
			this.PieChart.Location = new System.Drawing.Point(0, 0);
			this.PieChart.Name = "PieChart";
			this.PieChart.Palette = System.Windows.Forms.DataVisualization.Charting.ChartColorPalette.SemiTransparent;
			series1.ChartArea = "Pie3D";
			series1.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Pie;
			series1.Legend = "Pie3D";
			series1.Name = "Pie3D";
			this.PieChart.Series.Add(series1);
			this.PieChart.Size = new System.Drawing.Size(271, 53);
			this.PieChart.TabIndex = 9;
			this.PieChart.Text = "chart1";
			// 
			// PieChartUserControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.PieChart);
			this.Name = "PieChartUserControl";
			this.Size = new System.Drawing.Size(271, 53);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.PieChart)).EndInit();
			this.ResumeLayout(false);
		}

		#endregion

		internal ZChart PieChart;
	}
}
