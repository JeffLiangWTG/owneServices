using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Freight.Business.Testing;
using Enterprise.Registry.Business;
using Module = Enterprise.Registry.Business.Module;

namespace Enterprise.Freight.Agency.Business.Testing
{
	public abstract class DefaultNumberOfDecimalsSupporterForShippingTest : DefaultNumberOfDecimalsSupporterForFreightTest
	{
		protected override void AssertDefaultNumberOfDecimalsAttribute(BusinessObject bizObj, string weightVolumePropertyName, string unitOfMeasureValue)
		{
			AgencyRegistry.Instance.DefaultNumberOfDecimalPlaces.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new DefaultNumberOfDecimalsCollection(Module.Shipping));
			var weightVolumeInfo = bizObj.FindPropertyInfo(weightVolumePropertyName);
			var decimalsAttribute = MetaData.GetMetaData(bizObj, weightVolumeInfo.PropertyDescriptor, MetaDataTypes.DecimalPlaces);
			AssertEquals("Weight/Volume properties should have 3 decimals by default.", 3, decimalsAttribute);
			var collection = new DefaultNumberOfDecimalsCollection(Module.Shipping);
			var defaultNumberOfDecimals_Hectograms = collection.AddNew();
			defaultNumberOfDecimals_Hectograms.UnitOfMeasure = Core.Constants.Weight.Hectograms;
			defaultNumberOfDecimals_Hectograms.NumberOfDecimals = 2;
			defaultNumberOfDecimals_Hectograms.RoundingMode = RoundingModes.Up;
			AgencyRegistry.Instance.DefaultNumberOfDecimalPlaces.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);
			decimalsAttribute = MetaData.GetMetaData(bizObj, weightVolumeInfo.PropertyDescriptor, MetaDataTypes.DecimalPlaces);
			AssertNotEquals("Weight/Volume property should NOT take value from registry.", 2, decimalsAttribute);
			AssertEquals("Weight/Volume properties should have 3 decimals by default.", 3, decimalsAttribute);
			var defaultNumberOfDecimals = collection.AddNew();
			defaultNumberOfDecimals.UnitOfMeasure = unitOfMeasureValue;
			defaultNumberOfDecimals.NumberOfDecimals = 1;
			defaultNumberOfDecimals.RoundingMode = RoundingModes.Up;
			AgencyRegistry.Instance.DefaultNumberOfDecimalPlaces.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);
			decimalsAttribute = MetaData.GetMetaData(bizObj, weightVolumeInfo.PropertyDescriptor, MetaDataTypes.DecimalPlaces);
			AssertEquals("Weight/Volume property should have 1 decimal place as set in registry.", 1, decimalsAttribute);
		}
	}
}
