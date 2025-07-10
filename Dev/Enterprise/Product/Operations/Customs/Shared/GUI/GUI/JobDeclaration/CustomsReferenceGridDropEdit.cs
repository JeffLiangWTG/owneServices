using System.Collections;
using System.Linq;
using Enterprise.Integration.Freight;
using Enterprise.ZArchitecture.GUI.Internal;

namespace Enterprise.Customs.GUI
{
	class CustomsReferenceGridDropEdit : ZGridDropEdit
	{
		protected override IList GetFilteredListForDropDown()
		{
			return List?.Cast<object>().Where(item => !((item as ICustomsNumberTypeCodeDescription)?.IsAutomation ?? false)).ToList();
		}
	}
}
