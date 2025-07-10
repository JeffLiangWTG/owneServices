using System.Collections.Generic;
using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.GUI.Testing
{
	[TestedType(typeof(LayoutCustomsSupplierHeaderUserControl))]
	sealed class LayoutCustomsSupplierHeaderUserControlBaseOnlyTest : LayoutCustomsSupplierHeaderUserControlAbstractTest<LayoutCustomsSupplierHeaderUserControl, BaseJobDeclaration>
	{
		protected override IEnumerable<string> ExpectedControlList => DefaultControlList;
	}
}
