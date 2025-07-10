using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.Testing
{
	public abstract class IApportionmentMethodOverrideHelperTest<T> : TestCaseWithFactory where T : IApportionmentMethodOverride
	{
		public void TestSpecificityScore()
		{
			var methodMatchedModule = CreateBusinessObject(new DummyConsolCostApportionmentMethodSetter
			{
				Module = "FOR"
			});

			var methodMatchedDirection = CreateBusinessObject(new DummyConsolCostApportionmentMethodSetter
			{
				Direction = "IMP"
			});

			var methodMatchedTransportMode = CreateBusinessObject(new DummyConsolCostApportionmentMethodSetter
			{
				TransportMode = "AIR"
			});

			var methodMatchedContainerMode = CreateBusinessObject(new DummyConsolCostApportionmentMethodSetter
			{
				ContainerMode = "LSE"
			});

			var methodMatchedConsolType = CreateBusinessObject(new DummyConsolCostApportionmentMethodSetter
			{
				ConsolType = "AGT"
			});

			AssertSpecificityScoreComparing("Test Top Condition, Module", methodMatchedModule
				, methodMatchedDirection, methodMatchedTransportMode, methodMatchedContainerMode, methodMatchedConsolType);

			AssertSpecificityScoreComparing("Test Second Condition, ConsolType", methodMatchedConsolType
				, methodMatchedDirection, methodMatchedTransportMode, methodMatchedContainerMode);

			AssertSpecificityScoreComparing("Test Third Condition, TransportMode", methodMatchedTransportMode
				, methodMatchedDirection, methodMatchedContainerMode);

			AssertSpecificityScoreComparing("Test Fourth Condition, Direction", methodMatchedDirection
				, methodMatchedContainerMode);

			void AssertSpecificityScoreComparing(string comment, T highWeight, params T[] comparingItems)
			{
				Assert(comment, highWeight.GetSpecificityScore() > comparingItems.Sum(x => x.GetSpecificityScore()));
			}
		}

		#region Implementation

		protected abstract T CreateBusinessObject(DummyConsolCostApportionmentMethodSetter setter);

		#endregion

		protected class DummyConsolCostApportionmentMethodSetter : IApportionmentMethodOverride
		{
			public ZString Module { get; set; }

			public ZString ConsolType { get; set; }

			public ZString ApportionmentMethod { get; set; }

			public ZString ContainerMode { get; set; }

			public ZString TransportMode { get; set; }

			public ZString Direction { get; set; }
		}
	}

	class IApportionmentMethodOverrideHelperTest_Dummy : IApportionmentMethodOverrideHelperTest<IApportionmentMethodOverrideHelperTest_Dummy.DummyConsolCostApportionmentMethod>
	{
		protected override DummyConsolCostApportionmentMethod CreateBusinessObject(DummyConsolCostApportionmentMethodSetter setter)
		{
			return new DummyConsolCostApportionmentMethod
			{
				Module = setter.Module,
				ConsolType = setter.ConsolType,
				ApportionmentMethod = setter.ApportionmentMethod,
				ContainerMode = setter.ContainerMode,
				TransportMode = setter.TransportMode,
				Direction = setter.Direction,
			};
		}

		internal class DummyConsolCostApportionmentMethod : IApportionmentMethodOverride
		{
			public ZString Module { get; set; }

			public ZString ConsolType { get; set; }

			public ZString ApportionmentMethod { get; set; }

			public ZString ContainerMode { get; set; }

			public ZString TransportMode { get; set; }

			public ZString Direction { get; set; }
		}
	}
}
