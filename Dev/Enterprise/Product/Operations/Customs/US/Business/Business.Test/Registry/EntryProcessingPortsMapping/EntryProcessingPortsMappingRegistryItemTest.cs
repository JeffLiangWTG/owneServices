using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.DataRegistry.Business.Testing
{
	[TestedType(typeof(EntryProcessingPortsMappingRegistryItem))]
	sealed class EntryProcessingPortsMappingRegistryItemTest : StronglyTypedRegistryItemTestCaseWithFactory<EntryProcessingPortsMappingCollection>
	{
		protected override StronglyTypedRegistryItem<EntryProcessingPortsMappingCollection, EntryProcessingPortsMappingCollection> GetNewRegistryItem() => new EntryProcessingPortsMappingRegistryItem("", null, null, null);

		protected override EntryProcessingPortsMappingCollection ValidValue
		{
			get
			{
				var newFactory = new BusinessObjectFactory();
				var helper = new UniversalReferenceTestDataHelper(newFactory);
				helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CUSOF");
				helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "3901", "Test Name", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6));
				helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "3902", "Test Name", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6));
				newFactory.Save();

				EntryProcessingPortsMappingCollection collection = new EntryProcessingPortsMappingCollection();
				EntryProcessingPortsMapping mapping = collection.AddNew();
				mapping.EntryPort = "3901";
				mapping.ProcessingPort = "3902";

				return collection;
			}
		}
	}

	[TestedType(typeof(EntryProcessingPortsMappingCollectionRegistryDataType))]
	sealed class EntryProcessingPortsMappingCollectionRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<EntryProcessingPortsMappingCollectionRegistryDataType>
	{
		protected override string ExpectedEditorName => "EntryProcessingPortsMappingRegistryItemEditor";

		protected override EntryProcessingPortsMappingCollectionRegistryDataType GetNewDataType() => new EntryProcessingPortsMappingCollectionRegistryDataType();

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var newFactory = new BusinessObjectFactory();
			var helper = new UniversalReferenceTestDataHelper(newFactory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CUSOF");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "3901", "Test Name", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6));
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "3902", "Test Name", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6));
			newFactory.Save();

			EntryProcessingPortsMappingCollection collection = new EntryProcessingPortsMappingCollection();
			EntryProcessingPortsMapping mapping = collection.AddNew();
			mapping.EntryPort = "3901";
			mapping.ProcessingPort = "3902";

			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(collection, new EntryProcessingPortsMappingCollectionRegistryDataType().Serialise(collection))
			};
		}
	}
}
