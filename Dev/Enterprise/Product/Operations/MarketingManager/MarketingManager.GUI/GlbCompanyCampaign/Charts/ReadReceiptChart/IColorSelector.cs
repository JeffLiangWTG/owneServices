using System.Windows.Media;

namespace Enterprise.MarketingManager.GUI
{
	public interface IColorSelector
	{
		Brush SelectBrush(object item);
	}
}
