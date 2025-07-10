using System.Drawing;

namespace Enterprise.Warehouse.Transactions.GUI.Common
{
	public struct ChartDataPoint
	{
		public ChartDataPoint(Color color, string legend, double data)
			: this()
		{
			Color = color;
			Legend = legend;
			Data = data;
		}

		public Color Color { get; set; }
		public string Legend { get; set; }
		public double Data { get; set; }
	}
}
