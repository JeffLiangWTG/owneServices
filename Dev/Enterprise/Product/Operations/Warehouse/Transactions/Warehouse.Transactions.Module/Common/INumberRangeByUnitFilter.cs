using System.Windows.Forms;
using Enterprise.ZArchitecture.Business.Internal;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Warehouse.Transactions.Module
{
	public interface INumberRangeByUnitFilter
	{
		Control[] GetFilterControl(FilterStrip currentDataItem, Control control, ZBindingSource bindingSource);
	}
}
