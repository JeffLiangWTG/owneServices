using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI.Grid.Internal;

namespace Enterprise.Freight.GUI.Testing
{
	sealed class PortReferenceControlTest
		: ZMultiCombinationControlTest
	{
		public new void TestShowingCodeFindBox()
		{
			RunBindToListTest(FieldType.TextCodeFindBox, typeof(PortCallLookupControl), false);
		}

		public override ZMultiCombinationControl GetNewMultiCombinationControl()
		{
			return new PortReferenceControl();
		}
	}
}
