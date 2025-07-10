using System;

namespace Enterprise.Warehouse.Transit.DataTransfer.Universal.Testing
{
	class WhsTransitAdditionalReferencesHelperTest : TransitUniversalTestCase
	{
		#region TestConstructor_FactoryNotNull

		public void TestConstructor_FactoryNotNull()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new WhsTransitAdditionalReferencesHelper(Logger, null));
		}

		#endregion
	}
}
