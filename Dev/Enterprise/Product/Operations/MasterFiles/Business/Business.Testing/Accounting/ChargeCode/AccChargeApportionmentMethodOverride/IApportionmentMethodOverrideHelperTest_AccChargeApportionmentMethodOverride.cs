namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class IApportionmentMethodOverrideHelperTest_AccChargeApportionmentMethodOverride : IApportionmentMethodOverrideHelperTest<AccChargeApportionmentMethodOverride>
	{
		protected override AccChargeApportionmentMethodOverride CreateBusinessObject(DummyConsolCostApportionmentMethodSetter setter)
		{
			var result = Factory.NewWithValidTestData<AccChargeApportionmentMethodOverride>();
			result.AAM_Module = setter.Module;
			result.AAM_ConsolType = setter.ConsolType;
			result.AAM_ApportionmentMethod = setter.ApportionmentMethod;
			result.AAM_ContainerMode = setter.ContainerMode;
			result.AAM_TransportMode = setter.TransportMode;
			result.AAM_Direction = setter.Direction;

			return result;
		}
	}
}
