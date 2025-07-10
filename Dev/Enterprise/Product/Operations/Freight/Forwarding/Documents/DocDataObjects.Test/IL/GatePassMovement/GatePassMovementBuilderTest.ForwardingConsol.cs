using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Documents.DataObjects;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.IL;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Forwarding.Documents.Testing.IL
{
	sealed partial class GatePassMovementBuilderTest : TestCaseWithFactory
	{
		public void TestSourceType_ForwardingConsol()
		{
			var consol = CreateForwardingConsol();
			var dataObject = new GatePassMovementBuilder(new ForwardingConsolGatePassMovementProvider(consol)).Build();
			AssertEquals("SourceType", "ForwardingConsol", ((IDataSourceProvider)dataObject).SourceType);
		}

		public void TestSourceID_ForwardingConsol()
		{
			var consol = CreateForwardingConsol();
			var dataObject = new GatePassMovementBuilder(new ForwardingConsolGatePassMovementProvider(consol)).Build();
			AssertEquals("SourceID", "UNIQ123", ((IDataSourceProvider)dataObject).SourceID);
		}

		public void TestProcessType_ForwardingConsol()
		{
			var consol = CreateForwardingConsol();
			var dataObject = new GatePassMovementBuilder(new ForwardingConsolGatePassMovementProvider(consol)).Build();
			AssertEquals("ProcessType", "1", dataObject.ProcessType);
		}

		public void TestGatePassMovementNumber_ForwardingConsol()
		{
			var consol = CreateForwardingConsol();
			var dataObject = new GatePassMovementBuilder(new ForwardingConsolGatePassMovementProvider(consol)).Build();
			AssertEquals("GatePassMovementNumber", "100000007", dataObject.GatePassMovementNumber);
		}

		public void TestOriginSiteCode_ForwardingConsol()
		{
			var consol = CreateForwardingConsol();
			var dataObject = new GatePassMovementBuilder(new ForwardingConsolGatePassMovementProvider(consol)).Build();
			AssertEquals("OriginSiteCode", "ILASH", dataObject.OriginSite.Code);
		}

		public void TestDestinationSiteCode_ForwardingConsol()
		{
			var consol = CreateForwardingConsol();
			var dataObject = new GatePassMovementBuilder(new ForwardingConsolGatePassMovementProvider(consol)).Build();
			AssertNullOrEmpty("DestinationSiteCode", dataObject.DestinationSite.Code);
		}

		public void TestCargoTypeCode_ForwardingConsol()
		{
			var consol = CreateForwardingConsol();
			var dataObject = new GatePassMovementBuilder(new ForwardingConsolGatePassMovementProvider(consol)).Build();
			AssertEquals("CargoTypeCode", "FCL", dataObject.CargoType.Code);
		}

		public void TestCargoIdentifierTypeCode_ForwardingConsol()
		{
			var consol = CreateForwardingConsol();
			var dataObject = new GatePassMovementBuilder(new ForwardingConsolGatePassMovementProvider(consol)).Build();
			AssertEquals("CargoIdentifierTypeCode", "11", dataObject.CargoIdentifierType.Code);
		}

		public void TestCargoIdentifierKey1_ForwardingConsol()
		{
			var consol = CreateForwardingConsol();
			var transport = consol.Transports.AddNew();
			transport.JW_RL_NKDiscPort = "ILASH";
			transport.JW_ArrivalPortRouteId = "ARR123";
			var dataObject = new GatePassMovementBuilder(new ForwardingConsolGatePassMovementProvider(consol)).Build();
			AssertEquals("CargoIdentifierKey1", "ARR123", dataObject.CargoIdentifierKey1);
		}

		public void TestCargoIdentifierKey2_ForwardingConsol()
		{
			var consol = CreateForwardingConsol();
			var pdnconsol = consol.Numbers.AddNew();
			pdnconsol.CE_EntryType = IsraelConsolAdditionalReferenceNumberTypes.Codes.ParentDealNumber;
			pdnconsol.CE_EntryNum = "PDN456";
			var dataObject = new GatePassMovementBuilder(new ForwardingConsolGatePassMovementProvider(consol)).Build();
			AssertEquals("CargoIdentifierKey2", "PDN456", dataObject.CargoIdentifierKey2);
		}

		public void TestCargoIdentifierKey3_ForwardingConsol()
		{
			var consol = CreateForwardingConsol();
			var dataObject = new GatePassMovementBuilder(new ForwardingConsolGatePassMovementProvider(consol)).Build();
			AssertNullOrEmpty("CargoIdentifierKey3", dataObject.CargoIdentifierKey3);
			AssertEquals("CargoIdentifierKey3IsVisible", false, dataObject.CargoIdentifierKey3IsVisible);
		}

		public void TestTransportMode_ForwardingConsol()
		{
			var consol = CreateForwardingConsol();
			var dataObject = new GatePassMovementBuilder(new ForwardingConsolGatePassMovementProvider(consol)).Build();
			AssertEquals("TransportMode", "", dataObject.TransportMethod.Code);
		}

		public void TestCargoType_InList_ForwardingConsol()
		{
			const string messageError = "Cargo Type should be selected from the list";
			var consol = CreateForwardingConsol();
			var dataObject = new GatePassMovementBuilder(new ForwardingConsolGatePassMovementProvider(consol)).Build();
			var codeInfo = ((DocumentVisualizer.DocDataObjects.CodeDescription)dataObject.CargoType).CodeInfo;
			AssertNoMessageError(codeInfo, messageError);

			dataObject.CargoType.Code = "NotInList";
			dataObject.ValidateAll();
			AssertHasMessageError(codeInfo, messageError);
		}

		public void TestOriginSite_Mandatory_ForwardingConsol()
		{
			const string messageError = "Origin Site must be provided";
			var consol = CreateForwardingConsol();
			var dataObject = new GatePassMovementBuilder(new ForwardingConsolGatePassMovementProvider(consol)).Build();
			var codeInfo = ((DocumentVisualizer.DocDataObjects.CodeDescription)dataObject.OriginSite).CodeInfo;
			AssertNoMessageError(codeInfo, messageError);

			dataObject.OriginSite.Code = ZString.Empty;
			dataObject.ValidateAll();
			AssertHasMessageError(codeInfo, messageError);
		}

		public void TestOriginSite_InList_ForwardingConsol()
		{
			const string messageError = "Origin Site should be selected from the list";
			var consol = CreateForwardingConsol();
			CreateFacilities();

			var dataObject = new GatePassMovementBuilder(new ForwardingConsolGatePassMovementProvider(consol)).Build();
			var codeInfo = ((DocumentVisualizer.DocDataObjects.CodeDescription)dataObject.OriginSite).CodeInfo;
			AssertNoMessageError(codeInfo, messageError);

			dataObject.OriginSite.Code = "IL000001";
			dataObject.ValidateAll();
			AssertHasMessageError(codeInfo, messageError);
		}

		public void TestDestinationSite_Mandatory()
		{
			const string messageError = "Destination Site must be provided";
			var consol = CreateForwardingConsol();
			var dataObject = new GatePassMovementBuilder(new ForwardingConsolGatePassMovementProvider(consol)).Build();
			dataObject.DestinationSite.Code = "ILHFA";
			var codeInfo = ((DocumentVisualizer.DocDataObjects.CodeDescription)dataObject.DestinationSite).CodeInfo;
			AssertNoMessageError(codeInfo, messageError);

			dataObject.DestinationSite.Code = ZString.Empty;
			dataObject.ValidateAll();
			AssertHasMessageError(codeInfo, messageError);
		}

		public void TestDestinationSite_InList_ForwardingConsol()
		{
			const string messageError = "Destination Site should be selected from the list";
			var consol = CreateForwardingConsol();
			CreateFacilities();

			var dataObject = new GatePassMovementBuilder(new ForwardingConsolGatePassMovementProvider(consol)).Build();
			dataObject.DestinationSite.Code = "ILASH";
			var codeInfo = ((DocumentVisualizer.DocDataObjects.CodeDescription)dataObject.DestinationSite).CodeInfo;
			AssertNoMessageError(codeInfo, messageError);

			dataObject.DestinationSite.Code = "IL000001";
			dataObject.ValidateAll();
			AssertHasMessageError(codeInfo, messageError);
		}

		public void TestTransportMethod_Mandatory_ForwardingConsol()
		{
			const string messageError = "Transport Method must be provided";
			var consol = CreateForwardingConsol();
			var dataObject = new GatePassMovementBuilder(new ForwardingConsolGatePassMovementProvider(consol)).Build();

			var codeInfo = ((DocumentVisualizer.DocDataObjects.CodeDescription)dataObject.TransportMethod).CodeInfo;
			AssertHasMessageError(codeInfo, messageError);

			dataObject.TransportMethod.Code = "1";
			dataObject.ValidateAll();
			AssertNoMessageError(codeInfo, messageError);
		}

		public void TestTransportMethod_InList_ForwardingConsol()
		{
			const string messageError = "Transport Method should be selected from the list";
			CreateILTransportMethod();
			var consol = CreateForwardingConsol();
			var dataObject = new GatePassMovementBuilder(new ForwardingConsolGatePassMovementProvider(consol)).Build();
			var codeInfo = ((DocumentVisualizer.DocDataObjects.CodeDescription)dataObject.TransportMethod).CodeInfo;
			dataObject.TransportMethod.Code = "93";
			dataObject.ValidateAll();
			AssertNoMessageError(codeInfo, messageError);

			dataObject.TransportMethod.Code = "NotInList";
			dataObject.ValidateAll();
			AssertHasMessageError(codeInfo, messageError);
		}

		public void TestCargoIdentifierType_Mandatory_ForwardingConsol()
		{
			const string messageError = "Cargo Identifier Type must be provided";
			var consol = CreateForwardingConsol();
			var dataObject = new GatePassMovementBuilder(new ForwardingConsolGatePassMovementProvider(consol)).Build();
			var codeInfo = ((DocumentVisualizer.DocDataObjects.CodeDescription)dataObject.CargoIdentifierType).CodeInfo;
			AssertNoMessageError(codeInfo, messageError);

			dataObject.CargoIdentifierType.Code = ZString.Empty;
			dataObject.ValidateAll();
			AssertHasMessageError(codeInfo, messageError);
		}

		public void TestCargoIdentifierType_InList_ForwardingConsol()
		{
			const string messageError = "Cargo Identifier Type should be selected from the list";
			CreateILCargoIdentifierType();
			var consol = CreateForwardingConsol();
			var dataObject = new GatePassMovementBuilder(new ForwardingConsolGatePassMovementProvider(consol)).Build();
			var codeInfo = ((DocumentVisualizer.DocDataObjects.CodeDescription)dataObject.CargoIdentifierType).CodeInfo;
			dataObject.TransportMethod.Code = "11";
			dataObject.ValidateAll();
			AssertNoMessageError(codeInfo, messageError);

			dataObject.CargoIdentifierType.Code = "NotInList";
			dataObject.ValidateAll();
			AssertHasMessageError(codeInfo, messageError);
		}

		public void TestCargoIdentifierKey1_Mandatory_ForwardingConsol()
		{
			const string messageError = "Cargo Identifier Key 1 must be provided";
			var consol = CreateForwardingConsol();
			var transport = consol.Transports.AddNew();
			transport.JW_RL_NKDiscPort = "ILASH";
			transport.JW_ArrivalPortRouteId = "ARR123";

			var dataObject = new GatePassMovementBuilder(new ForwardingConsolGatePassMovementProvider(consol)).Build();
			var codeInfo = dataObject.CargoIdentifierKey1Info;
			AssertNoMessageError(codeInfo, messageError);

			dataObject.CargoIdentifierKey1 = ZString.Empty;
			dataObject.ValidateAll();
			AssertHasMessageError(codeInfo, messageError);
		}

		public void TestCargoIdentifierKey2_Mandatory_ForwardingConsol()
		{
			const string messageError = "Cargo Identifier Key 2 must be provided";
			var consol = CreateForwardingConsol();
			CombineAssertions("When TransportMode!=Road", () =>
			{
				var dataObject = new GatePassMovementBuilder(new ForwardingConsolGatePassMovementProvider(consol)).Build();
				dataObject.CargoIdentifierKey2 = "KEY2";
				var codeInfo = dataObject.CargoIdentifierKey2Info;
				AssertNoMessageError(codeInfo, messageError);

				dataObject.CargoIdentifierKey2 = ZString.Empty;
				dataObject.ValidateAll();
				AssertHasMessageError(codeInfo, messageError);
			});

			CombineAssertions("When TransportMode==Road", () =>
			{
				consol.JK_TransportMode = Core.Constants.TransportModes.Road;
				var dataObject = new GatePassMovementBuilder(new ForwardingConsolGatePassMovementProvider(consol)).Build();
				dataObject.ValidateAll();
				AssertNoMessageError(dataObject.CargoIdentifierKey2Info, messageError);
			});
		}

		ForwardingConsol CreateForwardingConsol()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_UniqueConsignRef = "UNIQ123";
			consol.JK_RL_NKDischargePort = "ILASH";
			consol.JK_ConsolMode = "FCL";
			consol.JK_GMN = "100000007";
			consol.JK_RL_NKDischargePort = "ILASH";
			return consol;
		}

		void CreateFacilities()
		{
			var factory = Factory;
			var helper = new Customs.Universal.Testing.UniversalReferenceTestDataHelper(factory);
			var eunZZZ = helper.CreateNewOrGetExistingDataGrouping(EconomicGroupList.Codes.EuropeanUnion);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Israel, parent: eunZZZ);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, "FAC");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Israel, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, "ILASH", "Ashdod", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Israel, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, "ILTLV", "Tel Aviv", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			factory.Save();
		}

		void CreateILTransportMethod()
		{
			var factory = Factory;
			var helper = new Customs.Universal.Testing.UniversalReferenceTestDataHelper(factory);
			var eunZZZ = helper.CreateNewOrGetExistingDataGrouping(EconomicGroupList.Codes.EuropeanUnion);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Israel, parent: eunZZZ);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ILTransportMethod, "C00042");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Israel, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ILTransportMethod, "93", "Self", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			factory.Save();
		}

		void CreateILCargoIdentifierType()
		{
			var factory = Factory;
			var helper = new Customs.Universal.Testing.UniversalReferenceTestDataHelper(factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Israel);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ILCargoIdentifierType, "C1259");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Israel, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ILCargoIdentifierType, "11", "SeaDealImport", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			factory.Save();
		}
	}
}
