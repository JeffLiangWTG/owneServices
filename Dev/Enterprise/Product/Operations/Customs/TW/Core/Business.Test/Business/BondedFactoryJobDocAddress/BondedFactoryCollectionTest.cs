using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(BondedFactoryCollection))]
	sealed class BondedFactoryCollectionTest : ActiveBusinessObjectCollectionTestCase<BondedFactoryCollection>
	{
		protected override BondedFactoryCollection GetCollectionToTest()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			return new BondedFactoryCollection(declaration);
		}

		[ExpectNoExceptions]
		public void TestAllowNew()
		{
			var dec = Factory.New<JobDeclarationForTest>();
			dec.JE_MessageType = "IMP";
			for (int i = 0; i < 9; i++)
			{
				NUnit.Framework.Assert.That(((BondedFactoryCollectionForTest)dec.BondedFactories).AllowNew, NUnit.Framework.Is.True);
				dec.BondedFactories.AddNew();
			}

			NUnit.Framework.Assert.That(!((BondedFactoryCollectionForTest)dec.BondedFactories).AllowNew, NUnit.Framework.Is.True);
			dec.JE_MessageType = "EXP";
			dec.BondedFactories.DeleteAll();
			for (int i = 0; i < 10; i++)
			{
				NUnit.Framework.Assert.That(((BondedFactoryCollectionForTest)dec.BondedFactories).AllowNew, NUnit.Framework.Is.True);
				dec.BondedFactories.AddNew();
			}

			NUnit.Framework.Assert.That(!((BondedFactoryCollectionForTest)dec.BondedFactories).AllowNew, NUnit.Framework.Is.True);
		}

		[ExpectNoExceptions]
		public void TestReinitializeAddressSequenceNumber()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var collection = new BondedFactoryCollectionForTest(declaration);
			var bondedFactory1 = collection.AddNew();
			var bondedFactory2 = collection.AddNew();
			var bondedFactory3 = collection.AddNew();
			var bondedFactory4 = collection.AddNew();
			var bondedFactory5 = collection.AddNew();
			NUnit.Framework.Assert.That(bondedFactory1.E2_AddressSequence, NUnit.Framework.Is.EqualTo((ZByte)0));
			NUnit.Framework.Assert.That(bondedFactory2.E2_AddressSequence, NUnit.Framework.Is.EqualTo((ZByte)1));
			NUnit.Framework.Assert.That(bondedFactory3.E2_AddressSequence, NUnit.Framework.Is.EqualTo((ZByte)2));
			NUnit.Framework.Assert.That(bondedFactory4.E2_AddressSequence, NUnit.Framework.Is.EqualTo((ZByte)3));
			NUnit.Framework.Assert.That(bondedFactory5.E2_AddressSequence, NUnit.Framework.Is.EqualTo((ZByte)4));
			collection.Delete(bondedFactory1);
			NUnit.Framework.Assert.That(bondedFactory2.E2_AddressSequence, NUnit.Framework.Is.EqualTo((ZByte)0));
			NUnit.Framework.Assert.That(bondedFactory3.E2_AddressSequence, NUnit.Framework.Is.EqualTo((ZByte)2));
			NUnit.Framework.Assert.That(bondedFactory4.E2_AddressSequence, NUnit.Framework.Is.EqualTo((ZByte)3));
			NUnit.Framework.Assert.That(bondedFactory5.E2_AddressSequence, NUnit.Framework.Is.EqualTo((ZByte)4));
			collection.Delete(bondedFactory3);
			collection.Delete(bondedFactory4);
			NUnit.Framework.Assert.That(bondedFactory2.E2_AddressSequence, NUnit.Framework.Is.EqualTo((ZByte)0));
			NUnit.Framework.Assert.That(bondedFactory5.E2_AddressSequence, NUnit.Framework.Is.EqualTo((ZByte)4));
			collection.Delete(bondedFactory2);
			NUnit.Framework.Assert.That(bondedFactory5.E2_AddressSequence, NUnit.Framework.Is.EqualTo((ZByte)0));
		}
	}
}
