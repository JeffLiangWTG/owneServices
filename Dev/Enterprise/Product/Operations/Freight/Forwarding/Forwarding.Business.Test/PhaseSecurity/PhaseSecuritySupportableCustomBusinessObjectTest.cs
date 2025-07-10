using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	[TestedType(typeof(PhaseSecuritySupportableCustomBusinessObject))]
	sealed class PhaseSecuritySupportableCustomBusinessObjectTest : CustomBusinessObjectTest
	{
		public void TestReadOnly()
		{
			var dummy = Factory.New<DummyPhaseSecuritySupportable>();

			var values = new Dictionary<string, object>();
			var customProperties = new CustomPropertyCollectionImpl(
				propertyName =>
				{
					object value;
					return values.TryGetValue(propertyName, out value) ? value : null;
				},
				(propertyName, value) =>
				{
					values[propertyName] = value;
					return true;
				});

			customProperties.Add(typeof(ZString), "ZZZ_String", DynamicMetaData.ListDataSource(new[] { "A" }), DynamicMetaData.MaxLength(20));
			customProperties.Add(typeof(ZDecimal), "ZZZ_Decimal", DynamicMetaData.DecimalPlaces(1));
			customProperties.Add(typeof(ZDecimal), "ZZZ_Bool", true);

			dummy.PhaseSecurityReadOnlyPropertyNames.Add("ZZZ_String");

			var customBusinessObject = new PhaseSecuritySupportableCustomBusinessObject(Factory, dummy, customProperties, dummy);

			AssertEquals("ZZZ_String is readonly due to Phase security", true, customBusinessObject.ZPropertyInfoHash["ZZZ_String"].ReadOnly);
			AssertEquals("ZZZ_Decimal is not readonly", false, customBusinessObject.ZPropertyInfoHash["ZZZ_Decimal"].ReadOnly);
			AssertEquals("ZZZ_Bool is readonly due to metadata", true, customBusinessObject.ZPropertyInfoHash["ZZZ_Bool"].ReadOnly);
		}

		protected override CustomBusinessObject GetCustomBusinessObject(CustomPropertyCollectionImpl propertyCollection)
		{
			return new PhaseSecuritySupportableCustomBusinessObject(Factory, null, propertyCollection, Factory.New<ForwardingShipment>());
		}

		#region Implementation

		class DummyPhaseSecuritySupportable : DummyBusinessObject, IPhaseSecuritySupportable
		{
			public DummyPhaseSecuritySupportable(BusinessObjectFactory factory, System.Data.DataRow row)
				: base(factory, row)
			{
			}

			public ICollection<string> PhaseSecurityReadOnlyPropertyNames
			{
				get { return phaseSecurityReadOnlyPropertyNames ?? (phaseSecurityReadOnlyPropertyNames = new List<string>()); }
			}

			List<string> phaseSecurityReadOnlyPropertyNames;

			bool IPhaseSecuritySupportable.IsPropertyReadOnlyDueToPhase(ZString propertyName)
			{
				return PhaseSecurityReadOnlyPropertyNames.Contains(propertyName);
			}

			bool IPhaseSecuritySupportable.IsReadOnlyDueToPhase
			{
				get { return false; }
			}
		}

		#endregion
	}
}
