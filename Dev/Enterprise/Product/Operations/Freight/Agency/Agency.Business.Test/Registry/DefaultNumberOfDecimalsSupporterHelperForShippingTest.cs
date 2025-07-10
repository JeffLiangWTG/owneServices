using System;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Registry.Business;

namespace Enterprise.Freight.Agency.Business.Testing
{
	internal class DefaultNumberOfDecimalsSupporterHelperForShippingTest : TestCaseWithFactory
	{
		public void TestDefaultNumberOfDecimalsForNonNumericProperties()
		{
			FreightConfigurationRegistry.Instance.DefaultNumberOfDecimalPlaces.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new DefaultNumberOfDecimalsCollection(Module.Freight));
			var dummy = Factory.New<DefaultNumberOfDecimalsSupporterBizObj_Shipping>();
			var nonNumericProperties = dummy.ZPropertyInfoHash.GetPropertyInfos(PropertyInfoTypes.All).Cast<ZPropertyInfo>().Where((p) => p.PropertyDescriptor != null && !typeof(INumericZType).IsAssignableFrom(p.PropertyDescriptor.PropertyType)).ToArray();
			foreach (var property in nonNumericProperties)
			{
				AssertEquals(-1, DefaultNumberOfDecimalsSupporterHelperForFreight.GetDefaultNumberOfDecimalsMetaDataProperty(dummy, property.PropertyDescriptor));
				if (typeof(ZString).IsAssignableFrom(property.PropertyType) && property.SetValueFromString("test.with.period"))
				{
					AssertEquals("test.with.period", property.Value);
				}
			}
		}

		public void TestGetRoundedValue()
		{
			AgencyRegistry.Instance.DefaultNumberOfDecimalPlaces.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new DefaultNumberOfDecimalsCollection(Module.Shipping));
			var dummy = Factory.New<DefaultNumberOfDecimalsSupporterBizObj_Shipping>();
			dummy.Z0_UnitOfMeasureForDecimal = "KG";
			var decimals = DefaultNumberOfDecimalsSupporterHelperForShipping.GetDefaultNumberOfDecimalsMetaDataProperty(dummy, dummy.Z0_DecimalInfo.PropertyDescriptor);
			var roundingMode = DefaultNumberOfDecimalsSupporterHelperForShipping.GetRoundingMode(dummy, dummy.Z0_DecimalInfo.PropertyDescriptor);
			var roundedValue = DefaultNumberOfDecimalsSupporterHelperForShipping.GetRoundedValue(dummy, dummy.Z0_DecimalInfo.PropertyDescriptor, 123.2673m);
			AssertEquals("Default value is used, as registry is empty.", 3, decimals);
			AssertEquals("Default value is used, as registry is empty.", RoundingModes.BankersRounding, roundingMode);
			AssertEquals("Rounded value is calculated using default values, as registry is empty.", 123.267m, roundedValue);
			var collection = new DefaultNumberOfDecimalsCollection(Module.Shipping);
			var defaultNumberOfDecimals_SeaWeight = collection.AddNew();
			defaultNumberOfDecimals_SeaWeight.UnitOfMeasure = Core.Constants.Weight.Kilograms;
			defaultNumberOfDecimals_SeaWeight.NumberOfDecimals = 2;
			defaultNumberOfDecimals_SeaWeight.RoundingMode = RoundingModes.Up;
			AgencyRegistry.Instance.DefaultNumberOfDecimalPlaces.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);
			decimals = DefaultNumberOfDecimalsSupporterHelperForShipping.GetDefaultNumberOfDecimalsMetaDataProperty(dummy, dummy.Z0_DecimalInfo.PropertyDescriptor);
			roundingMode = DefaultNumberOfDecimalsSupporterHelperForShipping.GetRoundingMode(dummy, dummy.Z0_DecimalInfo.PropertyDescriptor);
			roundedValue = DefaultNumberOfDecimalsSupporterHelperForShipping.GetRoundedValue(dummy, dummy.Z0_DecimalInfo.PropertyDescriptor, 123.2673m);
			AssertEquals("Decimals are taken from registry.", 2, decimals);
			AssertEquals("Rounding Mode is taken from registry.", RoundingModes.Up, roundingMode);
			AssertEquals("Rounded value is calculated using registry values.", 123.27m, roundedValue);
		}

		#region Implementation
		class DefaultNumberOfDecimalsSupporterBizObj_Shipping : DefaultNumberOfDecimalsSupporterHelperForFreightTest.DefaultNumberOfDecimalsSupporterBizObj, IDefaultNumberOfDecimalsSupporter
		{
			public DefaultNumberOfDecimalsSupporterBizObj_Shipping(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			protected override int GetDefaultNumberOfDecimalsCore(PropertyDescriptor property)
			{
				return DefaultNumberOfDecimalsSupporterHelperForShipping.GetDefaultNumberOfDecimalsMetaDataProperty(this, property);
			}

			protected override ZDecimal GetRoundedValueCore(PropertyDescriptor property, ZDecimal value)
			{
				return DefaultNumberOfDecimalsSupporterHelperForShipping.GetRoundedValue(this, property, value);
			}
		}
		#endregion
	}
}
