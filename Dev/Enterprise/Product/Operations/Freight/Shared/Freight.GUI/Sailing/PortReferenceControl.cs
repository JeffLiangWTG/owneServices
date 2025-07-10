using System.Windows.Forms;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI.Grid.Internal;

namespace Enterprise.Freight.GUI
{
	public class PortReferenceControl : ZMultiCombinationControl
	{
		protected override IGridControl CreateGridControl(FieldType typeToCreate)
		{
			return typeToCreate == FieldType.TextCodeFindBox ? new PortCallLookupControl() : base.CreateGridControl(typeToCreate);
		}

		protected override void EditCore(CurrencyManager source, string mappingName)
		{
			if (!(CurrentEditor is PortCallLookupControl))
			{
				base.EditCore(source, mappingName);
			}
		}
	}
}
