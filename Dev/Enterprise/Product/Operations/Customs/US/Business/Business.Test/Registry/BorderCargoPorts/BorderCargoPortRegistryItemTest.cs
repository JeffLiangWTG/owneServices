using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Customs.US.Business;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.DataRegistry.Business.Testing
{
	[TestedType(typeof(BorderCargoPortRegistryItem))]
	sealed class BorderCargoPortRegistryItemTest : StronglyTypedRegistryItemTestCaseWithFactory<BorderCargoPortCollection>
	{
		protected override StronglyTypedRegistryItem<BorderCargoPortCollection, BorderCargoPortCollection> GetNewRegistryItem() => new BorderCargoPortRegistryItem("", null, null, null, RegistryStorageFlags.Branch);
	}

	[TestedType(typeof(BorderCargoPortCollectionRegistryDataType))]
	sealed class BorderCargoPortCollectionRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<BorderCargoPortCollectionRegistryDataType>
	{
		protected override string ExpectedEditorName => "BorderCargoPortRegistryItemEditor";

		protected override BorderCargoPortCollectionRegistryDataType GetNewDataType() => new BorderCargoPortCollectionRegistryDataType();

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var newFactory = new BusinessObjectFactory();
			var startDate = ZDateTime.UtcToday.Date.AddMonths(-1);
			var endDate = ZDateTime.UtcToday.Date.AddMonths(1);
			var helper = new UniversalReferenceTestDataHelper(newFactory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CUSOF");
			var code = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "3901", "CHICAGO, IL", startDate, endDate);
			var attributeNameState = helper.CreateNewOrGetExistingRefCusCodeListAttributeName(RefCusCodeListAttributeTypes.Codes.State, "Desc", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, Core.Constants.CountryCodes.UnitedStates);
			var attributeState = helper.CreateNewOrGetExistingCusCodeListAttribute(code.PK, attributeNameState.ZXE_Name, "IL");
			var attributeNameAddress1 = helper.CreateNewOrGetExistingRefCusCodeListAttributeName(RefCusCodeListAttributeTypes.Codes.Address1, "Desc", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, Core.Constants.CountryCodes.UnitedStates);
			var attributeAddress1 = helper.CreateNewOrGetExistingCusCodeListAttribute(code.PK, attributeNameState.ZXE_Name, "PORT DIRECTOR");

			newFactory.Save();

			var coll = new BorderCargoPortCollection(new FallbackLevel(Guid.Empty, Env.CurrentBranch.PK, Guid.Empty), newFactory);
			var element = coll.AddNew();
			element.PortCode = "3901";
			element.CRProcess = CRProcessList.Codes.OneStep;
			element.Location = LocationList.Codes.South;

			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(coll, new BorderCargoPortCollectionRegistryDataType().Serialise(coll))
			};
		}
	}
}
