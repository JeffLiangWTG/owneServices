using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.GUI;

namespace Enterprise.Freight.Forwarding.Routing.S8.GUI.Testing
{
	public class BulkConsolCreationFormForTest : BulkConsolCreationForm
	{
		public BulkConsolCreationFormForTest(MultiDaysSelection multiDaysSelection)
			: base(multiDaysSelection)
		{
		}

		public MultiDaysSelection MultiDaysSelectionData => MultiDaysSelection;
	}
}
