using System;
using Enterprise.Integration;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Rating.Business.Testing
{
	[TestedType(typeof(SellRatesDecimalsRegistryItem))]
	class SellRatesDecimalsRegistryItemTest : StronglyTypedRegistryItemTestCase<SellRatesDecimalsCollection>
	{
		public void TestGetDefaultSellRatesDecimals()
		{
			var systemNumberOfDecimalsAllowed = new SellRatesDecimalsCollection();
			var item = new SellRatesDecimalsRegistryItem("", null, null, null, RegistryStorageFlags.System | RegistryStorageFlags.Company, new SellRatesDecimalsCollection());
			item.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, systemNumberOfDecimalsAllowed);

			AssertEquals(4, item.GetDecimals(RatingConstants.RateCategory.WHS));

			SellRatesDecimals systemValue = systemNumberOfDecimalsAllowed.AddNew();
			systemValue.Code = RatingConstants.RateCategory.ALL;
			systemValue.Decimals = NumberOfDecimals.Two;
			item.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, systemNumberOfDecimalsAllowed);

			AssertEquals(2, item.GetDecimals(RatingConstants.RateCategory.WHS));

			SellRatesDecimals systemValueWHS = systemNumberOfDecimalsAllowed.AddNew();
			systemValueWHS.Code = RatingConstants.RateCategory.WHS;
			systemValueWHS.Decimals = NumberOfDecimals.Three;
			item.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, systemNumberOfDecimalsAllowed);

			AssertEquals(3, item.GetDecimals(RatingConstants.RateCategory.WHS));
			AssertEquals(2, item.GetDecimals(RatingConstants.RateCategory.ALL));
			AssertEquals(2, item.GetDecimals(RatingConstants.RateCategory.DST));
		}

		#region Implementation

		protected override StronglyTypedRegistryItem<SellRatesDecimalsCollection, SellRatesDecimalsCollection> GetNewRegistryItem()
		{
			return new SellRatesDecimalsRegistryItem("", null, null, null, RegistryStorageFlags.System, new SellRatesDecimalsCollection());
		}

		#endregion
	}

	[TestedType(typeof(SellRatesDecimalsRegistryDataType))]
	class SellRatesDecimalsRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<SellRatesDecimalsRegistryDataType>
	{
		#region Implementation

		protected override SellRatesDecimalsRegistryDataType GetNewDataType()
		{
			return new SellRatesDecimalsRegistryDataType();
		}

		protected override string ExpectedEditorName
		{
			get { return "SellRatesDecimalsRegistryItemEditor"; }
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			#region First

			var first = SellRatesDecimalsCollection.GetDefault();

			var firstBytes = new byte[]
			{
				60,0,63,0,120,0,109,0,108,0,32,0,118,0,101,0,114,0,115,0,105,0,111,0,110,0,61,0,34,0,49,0,46,0,48,0,34,0,32,0,101,0,110,0,99,0,111,0,100,0,105,0,110,0,103,0,61,0,34,0,117,0,116,0,102,
				0,45,0,49,0,54,0,34,0,63,0,62,0,60,0,65,0,114,0,114,0,97,0,121,0,79,0,102,0,83,0,101,0,108,0,108,0,82,0,97,0,116,0,101,0,115,0,68,0,101,0,99,0,105,0,109,0,97,0,108,0,115,0,32,0,120,
				0,109,0,108,0,110,0,115,0,58,0,120,0,115,0,105,0,61,0,34,0,104,0,116,0,116,0,112,0,58,0,47,0,47,0,119,0,119,0,119,0,46,0,119,0,51,0,46,0,111,0,114,0,103,0,47,0,50,0,48,0,48,0,49,0,47,
				0,88,0,77,0,76,0,83,0,99,0,104,0,101,0,109,0,97,0,45,0,105,0,110,0,115,0,116,0,97,0,110,0,99,0,101,0,34,0,32,0,120,0,109,0,108,0,110,0,115,0,58,0,120,0,115,0,100,0,61,0,34,0,104,0,116,
				0,116,0,112,0,58,0,47,0,47,0,119,0,119,0,119,0,46,0,119,0,51,0,46,0,111,0,114,0,103,0,47,0,50,0,48,0,48,0,49,0,47,0,88,0,77,0,76,0,83,0,99,0,104,0,101,0,109,0,97,0,34,0,62,0,60,
				0,83,0,101,0,108,0,108,0,82,0,97,0,116,0,101,0,115,0,68,0,101,0,99,0,105,0,109,0,97,0,108,0,115,0,62,0,60,0,67,0,111,0,100,0,101,0,62,0,65,0,76,0,76,0,60,0,47,0,67,0,111,0,100,0,101,
				0,62,0,60,0,68,0,101,0,99,0,105,0,109,0,97,0,108,0,115,0,62,0,52,0,60,0,47,0,68,0,101,0,99,0,105,0,109,0,97,0,108,0,115,0,62,0,60,0,47,0,83,0,101,0,108,0,108,0,82,0,97,0,116,0,101,
				0,115,0,68,0,101,0,99,0,105,0,109,0,97,0,108,0,115,0,62,0,60,0,47,0,65,0,114,0,114,0,97,0,121,0,79,0,102,0,83,0,101,0,108,0,108,0,82,0,97,0,116,0,101,0,115,0,68,0,101,0,99,0,105,0,109,
				0,97,0,108,0,115,0,62,0
			};

			#endregion

			#region Second

			var second = new SellRatesDecimalsCollection();

			var secondBytes = Array.Empty<byte>();

			#endregion

			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(first, firstBytes),
				new ValidSampleAndBinaryValueInDB(second, secondBytes)
			};
		}

		#endregion
	}
}
