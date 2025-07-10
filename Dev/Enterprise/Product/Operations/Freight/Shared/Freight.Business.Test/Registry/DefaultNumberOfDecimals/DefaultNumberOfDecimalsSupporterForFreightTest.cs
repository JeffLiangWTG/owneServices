using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Module = Enterprise.Registry.Business.Module;

namespace Enterprise.Freight.Business.Testing
{
	public abstract class DefaultNumberOfDecimalsSupporterForFreightTest : TestCaseWithFactory
	{
		public void TestDefaultNumberOfDecimalsAttribute_WeightVolumeProperties()
		{
			var type = BizObj.GetType();

			if (MeasurePropertiesAndUnits.Count > 0)
			{
				foreach (var kvp in MeasurePropertiesAndUnits)
				{
					bool isValue = Core.Constants.Weight.ContainsCode(kvp.Value) || Core.Constants.Volume.ContainsCode(kvp.Value);
					var unit = type.GetProperty(kvp.Value.ToString());

					var unitOfMeasureValue = (isValue) ? kvp.Value : (ZString)unit.GetValue(BizObj, null);

					AssertDefaultNumberOfDecimalsAttribute(BizObj, kvp.Key.ToString(), unitOfMeasureValue);
				}
			}
			else
			{
				Assert(true);
			}
		}

		public void TestDefaultNumberOfDecimalsAttribute_NonWeightVolumeProperties()
		{
			var type = BizObj.GetType();
			ZPropertyInfo[] propertyInfos = null;

			try
			{
				propertyInfos = BizObj.ZPropertyInfoHash.GetPropertyInfos(PropertyInfoTypes.All).Cast<ZPropertyInfo>().ToArray();
			}
			catch (InvalidOperationException ex)
			{
				AssertContains("Something went wrong, unexpected exception happened: cannot proceed to the rest of the test", string.Format("returned null for BusinessObject type: {0}", type.FullName), ex.Message);

				var propertyNames = type.GetProperties().Select((p) => p.Name);
				var propertyInfoList = new List<ZPropertyInfo>();

				foreach (string propertyName in propertyNames)
				{
					var propertyInfo = BizObj.FindPropertyInfo(propertyName);
					if (propertyInfo != null)
					{
						propertyInfoList.Add(propertyInfo);
					}
				}

				propertyInfos = propertyInfoList.ToArray();
			}

			propertyInfos = (propertyInfos != null)
				? propertyInfos
					.Where((p) => p.PropertyType == typeof(ZDecimal)
						&& !MeasurePropertiesAndUnits.ContainsKey(p.Name)
						&& !NewMeasurePropertiesAndUnits.ContainsKey(p.Name)
						&& !PropertiesWithExternalUnitsToExcludeFromTesting.Contains(p.Name))
					.ToArray()
				: null;

			if (propertyInfos != null && propertyInfos.Length > 0)
			{
				var nonPersistentPropertiesWithoutDecimalPlacesAttribute = new List<string>();

				foreach (ZPropertyInfo propertyInfo in propertyInfos)
				{
					var wrappedPropertyInfo = propertyInfo as ZWrappedPropertyInfo;
					var propertyInfoToUse = (wrappedPropertyInfo != null) ? wrappedPropertyInfo.InnerInfo : propertyInfo;
					var bizObjToUse = (wrappedPropertyInfo != null) ? wrappedPropertyInfo.InnerInfo.BizObj : BizObj;

					int expectedDecimals;
					int actualDecimals = (int)MetaData.GetMetaData(bizObjToUse, propertyInfoToUse.PropertyDescriptor, MetaDataTypes.DecimalPlaces, true);

					if (actualDecimals < 0)
					{
						var propertySchemaColumn = ObjectFactory.Get<IApplicationSchemaResolver>().GetSchemaColumnSafe(propertyInfoToUse.Name, bizObjToUse.TableName) as SchemaDecimalColumn;

						if (propertySchemaColumn != null)
						{
							expectedDecimals = propertySchemaColumn.Scale;
							actualDecimals = (int)MetaData.GetMetaData(bizObjToUse, propertyInfoToUse.PropertyDescriptor, MetaDataTypes.DecimalPlaces, false);

							AssertEquals("Non-Measure properties without DecimalPlaces attribute should have decimals set based on the scale value in DB schema",
								expectedDecimals,
								actualDecimals);
						}
						else
						{
							nonPersistentPropertiesWithoutDecimalPlacesAttribute.Add(propertyInfoToUse.Name);
						}
					}
					else
					{
						var decimalPlacesAttribute = (DecimalPlacesAttribute)propertyInfoToUse.PropertyDescriptor.GetAttributeFromMostSpecificComponentType(typeof(DecimalPlacesAttribute));
						expectedDecimals = decimalPlacesAttribute.DecimalPlaces;

						AssertEquals("Non-Mesure properties should have decimals set based on the value of DecimalPlaces attribute",
								expectedDecimals,
								actualDecimals);
					}
				}

				string propertyNames = "";
				foreach (var property in nonPersistentPropertiesWithoutDecimalPlacesAttribute)
				{
					propertyNames = propertyNames + property + "\n";
				}

				AssertEquals(string.Format("Non-persisted properties: '{0}' have no DecimalPlaces attribute set", propertyNames), 0, nonPersistentPropertiesWithoutDecimalPlacesAttribute.Count);
			}
			else
			{
				Assert("No non-measure properties exist on this business object", true);
			}
		}

		protected virtual void AssertDefaultNumberOfDecimalsAttribute(BusinessObject bizObj, string weightVolumePropertyName, string unitOfMeasureValue)
		{
			FreightConfigurationRegistry.Instance.DefaultNumberOfDecimalPlaces.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new DefaultNumberOfDecimalsCollection(Module.Freight));

			var weightVolumeInfo = bizObj.FindPropertyInfo(weightVolumePropertyName);
			var decimalsAttribute = MetaData.GetMetaData(bizObj, weightVolumeInfo.PropertyDescriptor, MetaDataTypes.DecimalPlaces);

			AssertEquals("Measure properties should have 3 decimals by default.", 3, decimalsAttribute);

			var collection = new DefaultNumberOfDecimalsCollection(Module.Freight);
			var defaultNumberOfDecimals_Sea = collection.AddNew();
			defaultNumberOfDecimals_Sea.UnitOfMeasure = unitOfMeasureValue;
			defaultNumberOfDecimals_Sea.TransportMode = Core.Constants.TransportModes.Sea;
			defaultNumberOfDecimals_Sea.NumberOfDecimals = 2;
			defaultNumberOfDecimals_Sea.RoundingMode = RoundingModes.Up;

			FreightConfigurationRegistry.Instance.DefaultNumberOfDecimalPlaces.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);
			decimalsAttribute = MetaData.GetMetaData(bizObj, weightVolumeInfo.PropertyDescriptor, MetaDataTypes.DecimalPlaces);

			AssertNotEquals("Measure property should not take value from registry.", 2, decimalsAttribute);
			AssertEquals("Measure properties should have 3 decimals by default.", 3, decimalsAttribute);

			var defaultNumberOfDecimals_Air = collection.AddNew();
			defaultNumberOfDecimals_Air.UnitOfMeasure = unitOfMeasureValue;
			defaultNumberOfDecimals_Air.TransportMode = Core.Constants.TransportModes.Air;
			defaultNumberOfDecimals_Air.NumberOfDecimals = 1;
			defaultNumberOfDecimals_Air.RoundingMode = RoundingModes.Up;

			FreightConfigurationRegistry.Instance.DefaultNumberOfDecimalPlaces.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);
			decimalsAttribute = MetaData.GetMetaData(bizObj, weightVolumeInfo.PropertyDescriptor, MetaDataTypes.DecimalPlaces);

			AssertEquals("Measure property should have 1 decimal place as set in registry.", 1, decimalsAttribute);
		}

		public virtual BusinessObject BizObj { get { return null; } }
		public virtual Dictionary<ZString, ZString> MeasurePropertiesAndUnits { get { return new Dictionary<ZString, ZString>(); } }
		public virtual Dictionary<ZString, ZString> NewMeasurePropertiesAndUnits { get { return new Dictionary<ZString, ZString>(); } }
		public virtual List<ZString> PropertiesWithExternalUnitsToExcludeFromTesting { get { return new List<ZString>(); } }

		public virtual void TestRoundingWhenDefaultNumberOfDecimalsChange()
		{
			Assert("Should be overriden", true);
		}

		protected void AssertDecimalPlacesAttribute(BusinessObject bizObj, PropertyDescriptor propertyDescriptor, int expectedDecimalPlaces, bool excludeMethodProvider)
		{
			AssertEquals(expectedDecimalPlaces, (int)MetaData.GetMetaData(bizObj, propertyDescriptor, MetaDataTypes.DecimalPlaces, excludeMethodProvider));
		}
	}
}
