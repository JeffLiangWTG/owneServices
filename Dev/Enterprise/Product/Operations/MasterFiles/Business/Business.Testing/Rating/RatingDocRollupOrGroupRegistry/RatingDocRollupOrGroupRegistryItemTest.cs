using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(RatingDocRollupOrGroupRegistryItem))]
	class RatingDocRollupOrGroupRegistryItemTest : StronglyTypedRegistryItemTestCase<RatingDocRollupOrGroupRegistryCollection>
	{
		protected override StronglyTypedRegistryItem<RatingDocRollupOrGroupRegistryCollection, RatingDocRollupOrGroupRegistryCollection> GetNewRegistryItem()
			=> new RatingDocRollupOrGroupRegistryItem("", null, null, null, RegistryStorageFlags.System, RegistryOptions.Default, new RatingDocRollupOrGroupRegistryCollection());

		public void TestRegistryItemDefaultValues()
		{
			var defaultRegistryItemValue = OrganisationRegistry.Instance.RatingDocRollupOrGroup.Value;
			AssertEquals("There is only one default data.", 1, defaultRegistryItemValue.Count);
			AssertEquals("The default value of the Module code is 'All' and its description value is 'All'", DocRollupOrSortModuleList.Codes.All, defaultRegistryItemValue[0].Module);
			AssertEquals("The default value of the Job Type code is 'All' and its description value is 'All'", DocRollupOrSortJobTypeList.Codes.All, defaultRegistryItemValue[0].JobType);
			AssertEquals("The default value of the Mode code is 'All' and its description value is 'All'", DocRollupOrSortTransportModeList.Codes.All, defaultRegistryItemValue[0].TransportMode);
			AssertEquals("The default value of the Display code is 'ROL' and its description value is 'ROL'", DocRollupOrSortDisplayList.Codes.RollUpCharges, defaultRegistryItemValue[0].Display);
			AssertEquals("The default value of the Style code is 'NOG' and its description value is 'NOG'", DocRollupOrSortStyleList.Codes.NoGrouping, defaultRegistryItemValue[0].Style);
		}
	}

	[TestedType(typeof(RatingDocRollupOrGroupDataType))]
	class RatingDocumentsChargeGroupingAndRollUpDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<RatingDocRollupOrGroupDataType>
	{
		protected override RatingDocRollupOrGroupDataType GetNewDataType() => new RatingDocRollupOrGroupDataType();

		protected override string ExpectedEditorName => "RatingDocumentsChargeGroupingAndRollUpRegistryItemEditor";

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			var collection1 = new RatingDocRollupOrGroupRegistryCollection();
			var ratingDocumentsChargeGroupingAndRollUp1 = collection1.AddNew();

			ratingDocumentsChargeGroupingAndRollUp1.Module = DocRollupOrSortModuleList.Codes.All;
			ratingDocumentsChargeGroupingAndRollUp1.JobType = DocRollupOrSortJobTypeList.Codes.All;
			ratingDocumentsChargeGroupingAndRollUp1.TransportMode = DocRollupOrSortTransportModeList.Codes.All;
			ratingDocumentsChargeGroupingAndRollUp1.Display = DocRollupOrSortDisplayList.Codes.RollUpCharges;
			ratingDocumentsChargeGroupingAndRollUp1.Style = DocRollupOrSortStyleList.Codes.NoGrouping;

			var collection2 = new RatingDocRollupOrGroupRegistryCollection();
			var ratingDocumentsChargeGroupingAndRollUp2 = collection2.AddNew();

			ratingDocumentsChargeGroupingAndRollUp2.Module = DocRollupOrSortModuleList.Codes.All;
			ratingDocumentsChargeGroupingAndRollUp2.JobType = DocRollupOrSortJobTypeList.Codes.All;
			ratingDocumentsChargeGroupingAndRollUp2.TransportMode = DocRollupOrSortTransportModeList.Codes.All;
			ratingDocumentsChargeGroupingAndRollUp2.Display = DocRollupOrSortDisplayList.Codes.Alphabetical;
			ratingDocumentsChargeGroupingAndRollUp2.Style = DocRollupOrSortStyleList.Codes.NoGrouping;

			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(collection1, DataType.Serialise(collection1)),
				new ValidSampleAndBinaryValueInDB(collection2, DataType.Serialise(collection2))
			};
		}
	}
}
