using System.Collections;
using System.Linq;
using Enterprise.Integration.Freight;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GUI
{
	public class CustomsReferenceDropEdit : ZDropEdit
	{
		protected override IList GetFilteredListForDropDown()
		{
			return List?.Cast<object>().Where(item => !((item as ICustomsNumberTypeCodeDescription)?.IsAutomation ?? false)).ToList();
		}
	}
}
