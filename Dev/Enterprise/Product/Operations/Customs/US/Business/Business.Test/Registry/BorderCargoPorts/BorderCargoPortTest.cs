using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Customs.US.Business;
using Enterprise.Environment;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.US.DataRegistry.Business.Testing
{
	[TestedType(typeof(BorderCargoPort))]
	sealed class BorderCargoPortTest : RegistryBusinessObjectTemplateTestCase<BorderCargoPort>
	{
		public void TestBorderCargoPort()
		{
			var newFactory = new BusinessObjectFactory();
			var startDate = ZDateTime.UtcToday.Date.AddMonths(-1);
			var endDate = ZDateTime.UtcToday.Date.AddMonths(1);
			var helper = new UniversalReferenceTestDataHelper(newFactory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CUSOF");
			var code = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "2704", "CHICAGO, IL1", startDate, endDate);
			var attributeNameState = helper.CreateNewOrGetExistingRefCusCodeListAttributeName(RefCusCodeListAttributeTypes.Codes.State, "Desc", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, Core.Constants.CountryCodes.UnitedStates);
			var attributeState = helper.CreateNewOrGetExistingCusCodeListAttribute(code.PK, attributeNameState.ZXE_Name, "IL");
			var attributeNameAddress1 = helper.CreateNewOrGetExistingRefCusCodeListAttributeName(RefCusCodeListAttributeTypes.Codes.Address1, "Desc", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, Core.Constants.CountryCodes.UnitedStates);
			var attributeAddress1 = helper.CreateNewOrGetExistingCusCodeListAttribute(code.PK, attributeNameState.ZXE_Name, "PORT DIRECTOR1");
			newFactory.Save();

			var mustBeEntered = "Please enter a value.";
			var enterValidSelection = "Enter a valid selection.";

			var coll = new BorderCargoPortCollection(new FallbackLevel(Guid.Empty, Env.CurrentBranch.PK, Guid.Empty), Factory);
			var bcPort = coll.AddNew();
			AssertEquals(4, bcPort.PortCodeInfo.MaxLength);
			AssertEquals(1, bcPort.LocationInfo.MaxLength);
			AssertEquals(typeof(ZZRefCusCodeListCombinedCollection), bcPort.Ports.GetType());
			AssertEquals(typeof(CRProcessList), bcPort.ProcessList.GetType());
			Assert(bcPort.ProcessList.ContainsCode("1-Step"));
			Assert(bcPort.ProcessList.ContainsCode("2-Step"));
			AssertEquals(typeof(LocationList), bcPort.Locations.GetType());
			Assert(bcPort.Locations.ContainsCode("N"));
			Assert(bcPort.Locations.ContainsCode("S"));
			var bcPort2 = coll.AddNew();
			Assert("List Cached", object.ReferenceEquals(bcPort.ProcessList, bcPort2.ProcessList));
			Assert("List Cached", object.ReferenceEquals(bcPort.Locations, bcPort2.Locations));
			bcPort.RunPreSaveValidation();
			AssertHasError(bcPort.PortCodeInfo, mustBeEntered);
			bcPort.PortCode = "----";
			bcPort.RunPreSaveValidation();
			AssertHasError(bcPort.PortCodeInfo, enterValidSelection);
			AssertHasError(bcPort.CRProcessInfo, mustBeEntered);
			bcPort.CRProcess = "1-Step1";
			bcPort.RunPreSaveValidation();
			AssertHasError(bcPort.CRProcessInfo, enterValidSelection);
			bcPort.Location = "X";
			bcPort.RunPreSaveValidation();
			AssertHasError(bcPort.LocationInfo, enterValidSelection);
			bcPort.PortCode = "2704";
			bcPort.CRProcess = "1-Step";
			bcPort.Location = "N";
			bcPort.RunPreSaveValidation();
			AssertNoError(bcPort.PortCodeInfo, mustBeEntered);
			AssertNoError(bcPort.PortCodeInfo, enterValidSelection);
			AssertNoError(bcPort.CRProcessInfo, mustBeEntered);
			AssertNoError(bcPort.CRProcessInfo, enterValidSelection);
			AssertNoError(bcPort.LocationInfo, enterValidSelection);
		}

		protected override bool RequiresFactory => true;

		protected override bool RequiresFallbackLevel => true;

		protected override BusinessObject GetNewBusinessObject()
		{
			var coll = new BorderCargoPortCollection(new FallbackLevel(Guid.Empty, Env.CurrentBranch.PK, Guid.Empty), Factory);
			return coll.AddNew();
		}

		protected override BorderCargoPort GetBusinessObjectToClone() => (BorderCargoPort)GetNewBusinessObject();

		protected override BorderCargoPort GetBusinessObjectToSerialise() => GetBusinessObjectToClone();
	}
}
