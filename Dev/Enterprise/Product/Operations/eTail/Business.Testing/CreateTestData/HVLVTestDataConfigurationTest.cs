using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Business;
using NUnit.Framework;

namespace Enterprise.eTail.Business.Testing;

[TestedType(typeof(HVLVTestDataConfiguration))]
public class HVLVTestDataConfigurationTest : NonPersistentBusinessObjectTestCase
{
	protected override BusinessObject GetNewBusinessObject() => new HVLVTestDataConfiguration(Factory.New<ForwardingConsol>());
}
