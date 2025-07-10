using System.Collections.Generic;
using Enterprise.Customs._CustomsTemplate_.Business;
using Enterprise.Customs.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs._CustomsTemplate_.GUI.Testing
{
	[TestedType(typeof(BaseCustomsSupplierHeaderUserControl))]
	sealed class BaseCustomsSupplierHeaderUserControlTest : LayoutCustomsSupplierHeaderUserControlAbstractTest<BaseCustomsSupplierHeaderUserControl, JobDeclaration>
	{
		protected override IEnumerable<string> ExpectedControlList => DefaultControlList;
	}
}
