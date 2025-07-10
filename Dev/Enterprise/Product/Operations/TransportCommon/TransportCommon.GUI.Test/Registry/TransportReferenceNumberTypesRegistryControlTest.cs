using Enterprise.TransportCommon.GUI.Registry;
using Enterprise.TransportCommon.Registry;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(TransportReferenceNumberTypesRegistryControl))]
	public class TransportReferenceNumberTypesRegistryControlTest : RegistryZUserControlTestCase
	{
		public void TestData()
		{
			var collection = new TransportReferenceNumberTypeCollection();

			using (var control = new TransportReferenceNumberTypesRegistryControl())
			{
				control.Data = collection;
				AssertEquals(collection, control.Data);
			}
		}

		protected override CargoWise.EntityFramework.IBusiness GetNewBusinessEntity()
		{
			return null;
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, CargoWise.EntityFramework.IBusiness businessEntity)
		{
			return ((TransportReferenceNumberTypesRegistryControl)control).ReadOnly;
		}

		protected override RegistryZUserControl GetNewControl()
		{
			return new TransportReferenceNumberTypesRegistryControl();
		}
	}
}
