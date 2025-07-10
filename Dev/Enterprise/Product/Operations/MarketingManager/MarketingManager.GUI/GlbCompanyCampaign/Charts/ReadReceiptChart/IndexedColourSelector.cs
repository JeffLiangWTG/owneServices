using System.Windows;
using System.Windows.Media;
using Enterprise.MarketingManager.Business;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.MarketingManager.GUI
{
	[CodeAlive("Color selector for PieSlices Color")]
	class IndexedColourSelector : DependencyObject, IColorSelector
	{
		public Brush SelectBrush(object item)
		{
			return ColorCollection.GetColorBrush((item as TrackingStatusChartData)?.Status);
		}
	}
}
