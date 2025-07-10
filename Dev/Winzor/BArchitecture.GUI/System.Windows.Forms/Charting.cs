using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;

namespace System.Windows.Forms.DataVisualization.Charting;

public class ElementPosition
{
	public float X { get; set; }
	public float Y { get; set; }
	public bool Auto { get; set; }
	public float Width { get; set; }
	public float Height { get; set; }
}
public class ChartArea
{
	public ChartArea3DStyle Area3DStyle { get; set; } = new ChartArea3DStyle();
	public Color BackColor { get; set; }
	public string? Name { get; set; }
	public ElementPosition Position { get; set; } = new ElementPosition();
}
public class Legend
{
	public Color BackColor { get; set; }
	public string? Name { get; set; }
	public ElementPosition Position { get; set; } = new ElementPosition();
}
public class Series
{
	public string? Legend { get; set; }
	public string? Name { get; set; }
	public string? ChartArea { get; set; }
	public SeriesChartType ChartType { get; set; }

	public DataPointCollection Points { get; set; } = new DataPointCollection();
}
public class ChartArea3DStyle
{
	public bool Enable3D { get; set; }
	public int Inclination { get; set; }
	public int PointDepth { get; set; }
}
public partial class Chart : Control, ISupportInitialize, IDisposable
{
	public Chart()
	{
		chartPicture = new ChartImage();
		_dataManager = new DataManager();
	}
	public ChartColorPalette Palette { get; set; }
	public class ChartImage : ChartPicture
	{ }
	readonly ChartImage chartPicture;
	readonly DataManager _dataManager;
	public ChartAreaCollection ChartAreas => chartPicture.ChartAreas;
	public LegendCollection Legends => chartPicture.Legends;
	public SeriesCollection Series => _dataManager.Series;

	public void BeginInit()
	{
	}

	public void EndInit()
	{
	}
}
public class ChartPicture
{
	public ChartPicture()
	{
		_chartAreas = new ChartAreaCollection();
		_legends = new LegendCollection();
	}
	readonly ChartAreaCollection _chartAreas;
	readonly LegendCollection _legends;
	public ChartAreaCollection ChartAreas => _chartAreas;
	public LegendCollection Legends => _legends;
}
public class ChartAreaCollection : ChartNamedElementCollection<ChartArea>
{
	public ChartArea? Add(string name)
	{
		return null;
	}
}
public abstract class ChartNamedElementCollection<T> : ChartElementCollection<T>
{ }
public abstract class ChartElementCollection<T> : Collection<T>
{ }
public class LegendCollection : ChartNamedElementCollection<Legend>
{ }
public class SeriesCollection : ChartNamedElementCollection<Series>
{ }
public class DataManager : ChartElement
{
	public DataManager()
	{
		_series = new SeriesCollection();
	}
	readonly SeriesCollection _series;
	public SeriesCollection Series => _series;
}
public class ChartElement
{ }
public enum SeriesChartType
{
	Point,
	FastPoint,
	Bubble,
	Line,
	Spline,
	StepLine,
	FastLine,
	Bar,
	StackedBar,
	StackedBar100,
	Column,
	StackedColumn,
	StackedColumn100,
	Area,
	SplineArea,
	StackedArea,
	StackedArea100,
	Pie,
	Doughnut,
	Stock,
	Candlestick,
	Range,
	SplineRange,
	RangeBar,
	RangeColumn,
	Radar,
	Polar,
	ErrorBar,
	BoxPlot,
	Renko,
	ThreeLineBreak,
	Kagi,
	PointAndFigure,
	Funnel,
	Pyramid
}
public enum ChartColorPalette
{
	None,
	Bright,
	Grayscale,
	Excel,
	Light,
	Pastel,
	EarthTones,
	SemiTransparent,
	Berry,
	Chocolate,
	Fire,
	SeaGreen,
	BrightPastel
}
public class DataPointCollection : ChartElementCollection<DataPoint>
{
	public DataPointCollection() { }
	public DataPoint Add(double y)
	{
		DataPoint dataPoint = new DataPoint(0.0, y);
		Add(dataPoint);
		return dataPoint;
	}
}
public class DataPoint
{
	public DataPoint() { }
	public DataPoint(double x,double y)
	{
		this.x = x;
		this.y = y;
	}
	public double x;
	public double y;
	public void SetValueY(double y) { this.y = y; }
	public void SetValueX(double x) { this.x = x; }
	public Color Color { get; set; }
	public string? LegendText { get; set; }
}
