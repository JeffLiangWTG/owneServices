using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.Business.Testing
{
	sealed class EffectiveValueProviderTest : TestCaseWithFactory
	{
		public void TestEffectiveValue()
		{
			var dummyBO = Factory.New<DummyBusinessObjectWithEffectiveValueProvider>();
			dummyBO.Z0_VarCharMax = "PARENT TEST";
			AssertEquals("PARENT TEST", dummyBO.Z0_Description);
			AssertEquals(ZString.Empty, ((IBusinessObjectInternals)dummyBO).Row[DummyBusinessObject.Schema.Z0_Description]);
			dummyBO.Z0_Description = "CHILD TEST";
			AssertEquals("CHILD TEST", dummyBO.Z0_Description);
			AssertEquals("CHILD TEST", ((IBusinessObjectInternals)dummyBO).Row[DummyBusinessObject.Schema.Z0_Description]);
		}

		class DummyBusinessObjectWithEffectiveValueProvider : DummyBusinessObject
		{
			public DummyBusinessObjectWithEffectiveValueProvider(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public override ZString Z0_Description
			{
				get => EffectiveValueProvider.GetValueToReturn(base.Z0_Description, this, DummyBusinessObject.Schema.Z0_VarCharMax, DummyBusinessObject.Schema.Z0_Description);
				set => base.Z0_Description = EffectiveValueProvider.GetValueToSet(value, this, DummyBusinessObject.Schema.Z0_VarCharMax);
			}

			protected override void SetDefaultValues()
			{
				base.SetDefaultValues();
				Z0_Description = ZString.Empty; // Base class sets it to "Default";
			}

			EffectiveValueProvider effectiveValueProvider;
			EffectiveValueProvider EffectiveValueProvider => effectiveValueProvider ?? (effectiveValueProvider = new EffectiveValueProvider());
		}
	}
}
