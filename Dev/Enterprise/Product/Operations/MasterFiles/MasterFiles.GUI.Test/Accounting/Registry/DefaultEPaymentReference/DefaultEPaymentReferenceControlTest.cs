using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(DefaultEPaymentReferenceControl))]
	public class DefaultEPaymentReferenceControlTest : RegistryZUserControlTestCase
	{
		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity) => control.ReadOnly;

		protected override IBusiness GetNewBusinessEntity()
		{
			var collection = new DefaultEPaymentReferenceCollection();
			collection.AddNew();
			return collection;
		}
	}
}
