using System;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Rating.Business.Testing
{
	[TestedType(typeof(DefaultRoundingsRegistryItem))]
	class DefaultRoundingsRegistryItemTest : StronglyTypedRegistryItemTestCase<DefaultRoundingsCollection>
	{
		public void TestGetDefaultRounding()
		{
			var systemRoundings = new DefaultRoundingsCollection();
			DefaultRoundings systemRounding1 = systemRoundings.AddNew();
			systemRounding1.Code = RatingConstants.RateCategory.ALL;
			systemRounding1.RoundingType = RatingRoundingTypes.Bankers;

			var systemRounding2 = systemRoundings.AddNew();
			systemRounding2.Code = RatingConstants.RateCategory.WHS;
			systemRounding2.RoundingType = RatingRoundingTypes.Custom;
			systemRounding2.RoundingFactor = 2m;

			var companyRoundings = new DefaultRoundingsCollection();
			var companyRounding1 = companyRoundings.AddNew();
			companyRounding1.Code = RatingConstants.RateCategory.WHS;
			companyRounding1.RoundingType = RatingRoundingTypes.Custom;
			companyRounding1.RoundingFactor = 5m;

			var item = new DefaultRoundingsRegistryItem("", null, null, null, RegistryStorageFlags.System | RegistryStorageFlags.Company, new DefaultRoundingsCollection());
			item.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, systemRoundings);

			var defaultRounding = item.GetDefaultRounding(RatingConstants.RateCategory.ALL);
			AssertEquals("rounding for rate type 'ALL'", RatingRoundingTypes.Bankers, defaultRounding.RoundingType);

			defaultRounding = item.GetDefaultRounding(RatingConstants.RateCategory.WHS);
			AssertEquals("rounding for rate type 'WHS'", RatingRoundingTypes.Custom, defaultRounding.RoundingType);
			AssertEquals(2m, defaultRounding.RoundingFactor);

			defaultRounding = item.GetDefaultRounding(RatingConstants.RateCategory.LCL);
			AssertEquals("there is no specific rounding for 'LCL', but there is arounding for rate type 'ALL'", RatingRoundingTypes.Bankers, defaultRounding.RoundingType);

			item.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, companyRoundings);

			defaultRounding = item.GetDefaultRounding(RatingConstants.RateCategory.WHS);
			AssertEquals("use company settings if present", RatingRoundingTypes.Custom, defaultRounding.RoundingType);
			AssertEquals(5m, defaultRounding.RoundingFactor);

			defaultRounding = item.GetDefaultRounding(RatingConstants.RateCategory.FCL);
			AssertEquals("There is no default rounding either for FCL or ALL so falls back to No Rounding", RatingRoundingTypes.NoRounding, defaultRounding.RoundingType);
		}

		#region Implementation

		protected override StronglyTypedRegistryItem<DefaultRoundingsCollection, DefaultRoundingsCollection> GetNewRegistryItem()
		{
			return new DefaultRoundingsRegistryItem("", null, null, null, RegistryStorageFlags.System, new DefaultRoundingsCollection());
		}

		#endregion
	}

	[TestedType(typeof(DefaultRoundingsRegistryDataType))]
	class DefaultRoundingsRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<DefaultRoundingsRegistryDataType>
	{
		#region Implementation

		protected override DefaultRoundingsRegistryDataType GetNewDataType()
		{
			return new DefaultRoundingsRegistryDataType();
		}

		protected override string ExpectedEditorName
		{
			get { return "RoundingsRegistryItemEditor"; }
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			#region First

			var first = DefaultRoundingsCollection.GetDefault();

			var firstBytes = new byte[]
			{
				255,254,60,0,63,0,120,0,109,0,108,0,32,0,118,0,101,0,114,0,115,0,105,0,111,0,110,0,61,0,34,0,49,0,46,0,48,0,34,0,32,0,101,0,110,
				0,99,0,111,0,100,0,105,0,110,0,103,0,61,0,34,0,117,0,116,0,102,0,45,0,49,0,54,0,34,0,63,0,62,0,60,0,65,0,114,0,114,0,97,0,121,0,
				79,0,102,0,68,0,101,0,102,0,97,0,117,0,108,0,116,0,82,0,111,0,117,0,110,0,100,0,105,0,110,0,103,0,115,0,32,0,120,0,109,0,108,0,
				110,0,115,0,58,0,120,0,115,0,105,0,61,0,34,0,104,0,116,0,116,0,112,0,58,0,47,0,47,0,119,0,119,0,119,0,46,0,119,0,51,0,46,0,111,
				0,114,0,103,0,47,0,50,0,48,0,48,0,49,0,47,0,88,0,77,0,76,0,83,0,99,0,104,0,101,0,109,0,97,0,45,0,105,0,110,0,115,0,116,0,97,0,110,
				0,99,0,101,0,34,0,32,0,120,0,109,0,108,0,110,0,115,0,58,0,120,0,115,0,100,0,61,0,34,0,104,0,116,0,116,0,112,0,58,0,47,0,47,0,119,
				0,119,0,119,0,46,0,119,0,51,0,46,0,111,0,114,0,103,0,47,0,50,0,48,0,48,0,49,0,47,0,88,0,77,0,76,0,83,0,99,0,104,0,101,0,109,0,97,
				0,34,0,62,0,60,0,68,0,101,0,102,0,97,0,117,0,108,0,116,0,82,0,111,0,117,0,110,0,100,0,105,0,110,0,103,0,115,0,62,0,60,0,67,0,111,
				0,100,0,101,0,62,0,65,0,76,0,76,0,60,0,47,0,67,0,111,0,100,0,101,0,62,0,60,0,68,0,101,0,115,0,99,0,114,0,105,0,112,0,116,0,105,0,
				111,0,110,0,62,0,68,0,101,0,102,0,97,0,117,0,108,0,116,0,32,0,114,0,111,0,117,0,110,0,100,0,105,0,110,0,103,0,32,0,116,0,121,0,112,
				0,101,0,60,0,47,0,68,0,101,0,115,0,99,0,114,0,105,0,112,0,116,0,105,0,111,0,110,0,62,0,60,0,82,0,111,0,117,0,110,0,100,0,105,0,110,
				0,103,0,84,0,121,0,112,0,101,0,62,0,78,0,79,0,82,0,60,0,47,0,82,0,111,0,117,0,110,0,100,0,105,0,110,0,103,0,84,0,121,0,112,0,101,0,
				62,0,60,0,47,0,68,0,101,0,102,0,97,0,117,0,108,0,116,0,82,0,111,0,117,0,110,0,100,0,105,0,110,0,103,0,115,0,62,0,60,0,68,0,101,0,
				102,0,97,0,117,0,108,0,116,0,82,0,111,0,117,0,110,0,100,0,105,0,110,0,103,0,115,0,62,0,60,0,67,0,111,0,100,0,101,0,62,0,87,0,72,0,
				83,0,60,0,47,0,67,0,111,0,100,0,101,0,62,0,60,0,68,0,101,0,115,0,99,0,114,0,105,0,112,0,116,0,105,0,111,0,110,0,62,0,68,0,101,0,102,
				0,97,0,117,0,108,0,116,0,32,0,114,0,111,0,117,0,110,0,100,0,105,0,110,0,103,0,32,0,116,0,121,0,112,0,101,0,32,0,102,0,111,0,114,0,
				32,0,87,0,97,0,114,0,101,0,104,0,111,0,117,0,115,0,101,0,32,0,99,0,104,0,97,0,114,0,103,0,101,0,115,0,60,0,47,0,68,0,101,0,115,0,99,
				0,114,0,105,0,112,0,116,0,105,0,111,0,110,0,62,0,60,0,82,0,111,0,117,0,110,0,100,0,105,0,110,0,103,0,84,0,121,0,112,0,101,0,62,0,85,
				0,80,0,49,0,60,0,47,0,82,0,111,0,117,0,110,0,100,0,105,0,110,0,103,0,84,0,121,0,112,0,101,0,62,0,60,0,47,0,68,0,101,0,102,0,97,0,117,
				0,108,0,116,0,82,0,111,0,117,0,110,0,100,0,105,0,110,0,103,0,115,0,62,0,60,0,47,0,65,0,114,0,114,0,97,0,121,0,79,0,102,0,68,0,101,0,
				102,0,97,0,117,0,108,0,116,0,82,0,111,0,117,0,110,0,100,0,105,0,110,0,103,0,115,0,62,0
			};

			#endregion

			#region Second

			var second = new DefaultRoundingsCollection();

			var secondBytes = Array.Empty<byte>();

			#endregion

			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(first, firstBytes),
				new ValidSampleAndBinaryValueInDB(second, secondBytes),
			};
		}

		#endregion
	}
}
