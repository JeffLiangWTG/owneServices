#if DEBUG

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
	public sealed class DefaultNumberOfDecimalsSupporterHelperForFreightTest : TestCaseWithFactory
	{
		public void TestDefaultNumberOfDecimalsForNonNumericProperties()
		{
			FreightConfigurationRegistry.Instance.DefaultNumberOfDecimalPlaces.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new DefaultNumberOfDecimalsCollection(Module.Freight));

			var dummy = Factory.New<DefaultNumberOfDecimalsSupporterBizObj>();
			var nonNumericProperties = dummy.ZPropertyInfoHash.GetPropertyInfos(PropertyInfoTypes.All)
				.Cast<ZPropertyInfo>()
				.Where((p) => p.PropertyDescriptor != null &&
					!typeof(INumericZType).IsAssignableFrom(p.PropertyDescriptor.PropertyType))
				.ToArray();

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
			FreightConfigurationRegistry.Instance.DefaultNumberOfDecimalPlaces.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new DefaultNumberOfDecimalsCollection(Module.Freight));

			var dummy = Factory.New<DefaultNumberOfDecimalsSupporterBizObj>();
			dummy.Z0_UnitOfMeasureForDecimal = "KG";

			var decimals = DefaultNumberOfDecimalsSupporterHelperForFreight.GetDefaultNumberOfDecimalsMetaDataProperty(dummy, dummy.Z0_DecimalInfo.PropertyDescriptor);
			var roundingMode = DefaultNumberOfDecimalsSupporterHelperForFreight.GetRoundingMode(dummy, dummy.Z0_DecimalInfo.PropertyDescriptor);
			var roundedValue = DefaultNumberOfDecimalsSupporterHelperForFreight.GetRoundedValue(dummy, dummy.Z0_DecimalInfo.PropertyDescriptor, 123.2673m);

			AssertEquals("Default value is used, as registry is empty.", 3, decimals);
			AssertEquals("Default value is used, as registry is empty.", RoundingModes.BankersRounding, roundingMode);
			AssertEquals("Rounded value is calculated using default values, as registry is empty.", 123.267m, roundedValue);

			var collection = new DefaultNumberOfDecimalsCollection(Module.Freight);
			var defaultNumberOfDecimals_SeaWeight = collection.AddNew();
			defaultNumberOfDecimals_SeaWeight.UnitOfMeasure = Core.Constants.Weight.Kilograms;
			defaultNumberOfDecimals_SeaWeight.TransportMode = Core.Constants.TransportModes.Sea;
			defaultNumberOfDecimals_SeaWeight.NumberOfDecimals = 2;
			defaultNumberOfDecimals_SeaWeight.RoundingMode = RoundingModes.Up;

			FreightConfigurationRegistry.Instance.DefaultNumberOfDecimalPlaces.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			decimals = DefaultNumberOfDecimalsSupporterHelperForFreight.GetDefaultNumberOfDecimalsMetaDataProperty(dummy, dummy.Z0_DecimalInfo.PropertyDescriptor);
			roundingMode = DefaultNumberOfDecimalsSupporterHelperForFreight.GetRoundingMode(dummy, dummy.Z0_DecimalInfo.PropertyDescriptor);
			roundedValue = DefaultNumberOfDecimalsSupporterHelperForFreight.GetRoundedValue(dummy, dummy.Z0_DecimalInfo.PropertyDescriptor, 123.2673m);

			AssertEquals("Decimals are taken from registry.", 2, decimals);
			AssertEquals("Rounding Mode is taken from registry.", RoundingModes.Up, roundingMode);
			AssertEquals("Rounded value is calculated using registry values.", 123.27m, roundedValue);
		}

		public void TestIsRegistryDecimalValueOverridden()
		{
			var dummy = Factory.New<DefaultNumberOfDecimalsSupporterBizObj>();
			dummy.Z0_UnitOfMeasureForDecimal = "KG";

			AssertEquals(false, DefaultNumberOfDecimalsSupporterHelperForFreight.IsRegistryDecimalValueOverridden(dummy, dummy.Z0_DecimalInfo.PropertyDescriptor));

			var collection = new DefaultNumberOfDecimalsCollection(Module.Freight);
			var defaultNumberOfDecimals = collection.AddNew();
			defaultNumberOfDecimals.UnitOfMeasure = Core.Constants.Weight.Kilograms;
			defaultNumberOfDecimals.TransportMode = Core.Constants.TransportModes.Sea;
			defaultNumberOfDecimals.NumberOfDecimals = 2;
			defaultNumberOfDecimals.RoundingMode = RoundingModes.Up;

			FreightConfigurationRegistry.Instance.DefaultNumberOfDecimalPlaces.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			AssertEquals(true, DefaultNumberOfDecimalsSupporterHelperForFreight.IsRegistryDecimalValueOverridden(dummy, dummy.Z0_DecimalInfo.PropertyDescriptor));
		}

		public void TestGetDefaultNumberOfDecimalsWhenParentIsDeleted()
		{
			var dummy = Factory.New<DefaultNumberOfDecimalsSupporterBizObj>();
			dummy.Delete();

			AssertEquals("Should return default number of decimals without exception (Should not be accessing a property on a deleted business object.)",
				DefaultNumberOfDecimals.Schema.DefaultNumberOfDecimalsForWeightAndVolumeUnits,
				DefaultNumberOfDecimalsSupporterHelperForFreight.GetDefaultNumberOfDecimalsMetaDataProperty(dummy, dummy.Z0_DecimalInfo));
		}

		#region Implementation

		public class DefaultNumberOfDecimalsSupporterBizObj : DummyBusinessObject, IDefaultNumberOfDecimalsSupporter
		{
			public DefaultNumberOfDecimalsSupporterBizObj(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0001:Simplify Names", Justification = "Simplification hides desired base class")]
			public new abstract class Schema : DummyBusinessObject.Schema
			{
				public const string Z0_UnitOfMeasureForDecimal = "Z0_UnitOfMeasureForDecimal";
			}

			public ZString Z0_UnitOfMeasureForDecimal
			{
				get;
				set;
			}

			public ZPropertyInfo Z0_UnitOfMeasureForDecimalInfo
			{
				get { return GetZPropertyInfo(Schema.Z0_UnitOfMeasureForDecimal); }
			}

			ZString IDefaultNumberOfDecimalsSupporter.TransportMode
			{
				get { return Core.Constants.TransportModes.Sea; }
			}

			public int GetDefaultNumberOfDecimals(PropertyDescriptor property)
			{
				return GetDefaultNumberOfDecimalsCore(property);
			}

			protected virtual int GetDefaultNumberOfDecimalsCore(PropertyDescriptor property)
			{
				return DefaultNumberOfDecimalsSupporterHelperForFreight.GetDefaultNumberOfDecimalsMetaDataProperty(this, property);
			}

			ZString IDefaultNumberOfDecimalsSupporter.GetUnitOfMeasure(PropertyDescriptor property)
			{
				var unitOfMeasure = ZString.Empty;
				var propertyName = DefaultNumberOfDecimals.GetBoundPropertyName(property.Name);

				if (propertyName == Schema.Z0_Decimal)
				{
					unitOfMeasure = Z0_UnitOfMeasureForDecimal;
				}

				return unitOfMeasure;
			}

			ZDecimal IDefaultNumberOfDecimalsSupporter.GetRoundedValue(PropertyDescriptor property, ZDecimal value)
			{
				return GetRoundedValueCore(property, value);
			}

			protected virtual ZDecimal GetRoundedValueCore(PropertyDescriptor property, ZDecimal value)
			{
				return DefaultNumberOfDecimalsSupporterHelperForFreight.GetRoundedValue(this, property, value);
			}

			void IDefaultNumberOfDecimalsSupporter.RoundMeasurePropertiesOnTransportModeChanged()
			{ }
		}

		#endregion

	}
}

#endif
