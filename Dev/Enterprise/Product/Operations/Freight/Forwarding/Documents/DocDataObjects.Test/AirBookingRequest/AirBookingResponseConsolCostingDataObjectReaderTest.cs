using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Integration;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;
using Moq;

namespace Enterprise.Freight.Forwarding.Documents.Testing
{
	public sealed class AirBookingResponseConsolCostingDataObjectReaderTest : TestCaseWithFactory
	{
		public void TestIsRegisteredWithObjectFactory()
		{
			var reader = ObjectFactory.Get<IAirBookingResponseConsolCostingDataObjectReader>();
			AssertNotNull("expected reader can be accessed via ObjectFactory", reader);
		}

		public void TestImportConsolCosting_NullConsolCostingDataObject()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.JK_AgentType = Core.Constants.AgentType.Agent;
			consol.JK_UniqueConsignRef = "COBA0000690080";

			Factory.Save();

			var logger = new Mock<IXmlImportLogger>();
			var reader = ObjectFactory.Get<IAirBookingResponseConsolCostingDataObjectReader>();
			AssertNoExceptionThrown(() => reader.Import(logger.Object, null, consol));
		}

		public void TestImportConsolCosting_NullConsolCostingLineCollection()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.JK_AgentType = Core.Constants.AgentType.Agent;
			consol.JK_UniqueConsignRef = "COBA0000690080";

			Factory.Save();

			var consolCosts = new ConsolCosts(DefaultDataObjectWriterStrategy.TestInstance);

			var logger = new Mock<IXmlImportLogger>();
			var reader = ObjectFactory.Get<IAirBookingResponseConsolCostingDataObjectReader>();
			AssertNoExceptionThrown(() => reader.Import(logger.Object, consolCosts, consol));
		}

		public void TestImportConsolCosting_NullConsol()
		{
			var consolCosts = new ConsolCosts(DefaultDataObjectWriterStrategy.TestInstance);
			consolCosts.SetConsolCostLineCollection(() => new List<ConsolCostLine>
			{
				new ConsolCostLine
				{
					ChargeCode = new ChargeCode { Code = "FRT", Description = "International Freight" },
					SupplierReference = "93c45e64-0467-4fd1-8ef3-9e2b7b45fb55",
					CostOSAmount = 1251.9m,
					CostOSCurrency = new Currency { Code = "AUD" },
					RatingBehaviour = new RatingBehaviour { Code = "SPT" },
					ApportionmentMethod = "CHG",
					ImportMetaData = new ImportMetaData { Instruction = InstructionType.Insert }
				}
			});

			var logger = new Mock<IXmlImportLogger>();
			var reader = ObjectFactory.Get<IAirBookingResponseConsolCostingDataObjectReader>();
			AssertNoExceptionThrown(() => reader.Import(logger.Object, consolCosts, null));
		}

		public void TestImportConsolCosting_ChargeCodeIsInvalid()
		{
			AssertShouldNotAddConsolCostingForInvalidChargeCode(new ChargeCode { Code = "MM7", Description = "International Freight" });
		}

		public void TestImportConsolCosting_ChargeCodeIsEmpty()
		{
			AssertShouldNotAddConsolCostingForInvalidChargeCode(new ChargeCode { Code = "", Description = "International Freight" });
		}

		public void TestImportConsolCosting_ChargeCodeDoesNotExist()
		{
			AssertShouldNotAddConsolCostingForInvalidChargeCode(new ChargeCode { Description = "International Freight" });
		}

		public void TestImportConsolCosting()
		{
			var consol = CreateConsol();

			var costs = new JobConsolCostCollection(Factory, consol);
			AssertEquals("no consol costs have been created", 0, costs.Count);

			Factory.Save();

			var consolCosts = new ConsolCosts(DefaultDataObjectWriterStrategy.TestInstance);
			consolCosts.SetConsolCostLineCollection(() => new List<ConsolCostLine>
			{
				new ConsolCostLine
				{
					ChargeCode = new ChargeCode { Code = "FRT", Description = "International Freight" },
					SupplierReference = "93c45e64-0467-4fd1-8ef3-9e2b7b45fb55",
					CostOSAmount = 1251.9m,
					CostOSCurrency = new Currency { Code = "AUD" },
					RatingBehaviour = new RatingBehaviour { Code = "SPT" },
					ApportionmentMethod = "CHG",
					ImportMetaData = new ImportMetaData { Instruction = InstructionType.Insert }
				}
			});

			var logger = new Mock<IXmlImportLogger>();
			var reader = ObjectFactory.Get<IAirBookingResponseConsolCostingDataObjectReader>();
			reader.Import(logger.Object, consolCosts, consol);

			costs.Reload(true);
			AssertEquals("created cost from ABE response have been created", 1, costs.Count);

			AssertEquals("Consol Cost charge code", "FRT", costs[0].ChargeCode.AC_Code);
			AssertEquals("Consol Cost charge code", "93c45e64-0467-4fd1-8ef3-9e2b7b45fb55", costs[0].E6_CostReference);
			AssertEquals("Consol Cost charge code", 1251.9m, costs[0].E6_LocalCostAmount);
			AssertEquals("Consol Cost charge code", "AUD", costs[0].E6_RX_NKCurrency);
			AssertEquals("Consol Cost charge code", "CHG", costs[0].E6_ApportionmentMethod);
			AssertEquals("Consol Cost charge code", "SPT", costs[0].E6_RatingBehaviour);
		}

		ForwardingConsol CreateConsol()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.JK_AgentType = Core.Constants.AgentType.Agent;
			consol.JK_UniqueConsignRef = "COBA0000690080";

			consol.JK_RL_NKLoadPort = "FRCDG";
			consol.JK_RL_NKLoadPort = "USJFK";

			var transport = consol.Transports[0];
			transport.JW_TransportMode = Core.Constants.TransportModes.Air;
			transport.JW_Status = "PLN";
			transport.JW_TransportType = "FL1";
			transport.JW_VoyageFlightForBinding = "EY345";
			transport.JW_RL_NKLoadPort = "FRCDG";
			transport.JW_RL_NKDiscPort = "USORD";
			transport.JW_IsLinked = false;

			AssertEquals("prerequisite: consol has 1 transport", 1, consol.Transports.Count);

			var shipment = consol.Shipments.AddNew();
			shipment.JS_TransportMode = "AIR";
			shipment.JS_RL_NKLoadPort = "FRCDG";
			shipment.JS_RL_NKDischargePort = "USORD";

			var forwardingExportAirDept = Factory.LoadTop1<GlbDepartment>(new ZQuery(GlbDepartmentSchema.GE_Code, "FEA"));
			var shipmentJob = new JobHeader.Loader(Factory, shipment).TryLoadOrCreate();
			shipmentJob.JH_GE = forwardingExportAirDept.PK;
			return consol;
		}

		void AssertShouldNotAddConsolCostingForInvalidChargeCode(ChargeCode chargeCode)
		{
			var consol = CreateConsol();

			var costs = new JobConsolCostCollection(Factory, consol);
			AssertEquals("no consol costs have been created", 0, costs.Count);

			Factory.Save();

			var consolCosts = new ConsolCosts(DefaultDataObjectWriterStrategy.TestInstance);
			consolCosts.SetConsolCostLineCollection(() => new List<ConsolCostLine>
			{
				new ConsolCostLine
				{
					ChargeCode = chargeCode,
					SupplierReference = "93c45e64-0467-4fd1-8ef3-9e2b7b45fb55",
					CostOSAmount = 1251.9m,
					CostOSCurrency = new Currency { Code = "AUD" },
					RatingBehaviour = new RatingBehaviour { Code = "SPT" },
					ApportionmentMethod = "CHG",
					ImportMetaData = new ImportMetaData { Instruction = InstructionType.Insert }
				}
			});

			var logger = new TestLogger();

			var reader = ObjectFactory.Get<IAirBookingResponseConsolCostingDataObjectReader>();
			reader.Import(logger, consolCosts, consol);

			AssertNoExceptionThrown(() => Factory.Save());
			AssertEquals("Should not add a cost without charge code", 0, costs.Count);
			AssertEquals($"Could not find matched charge code of: {chargeCode?.Code}.", logger.Logs.LastOrDefault().Message);
		}

		class TestLogger : IXmlImportLogger
		{
			readonly List<TestLog> logs = new List<TestLog>();

			public bool IsUpdatingConsol { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
			public bool HasIgnoredModule { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
			public ITopLevelDataObject TopLevelDataObject => throw new System.NotImplementedException();
			public IDataContextDataObject TopLevelDataContext => throw new System.NotImplementedException();
			public bool OrgMatchingDisabled => throw new System.NotImplementedException();

			public IEnumerable<ISimpleLog> Logs => logs;

			public void Log(LogType type, string message)
			{
				logs.Add(new TestLog(type, message));
			}

			public void FireDataImportedToBusinessObject(BusinessObject targetBO) { throw new System.NotImplementedException(); }
			public void LogBoth(LogType type, string message) { throw new System.NotImplementedException(); }
			public void LogErrorToServiceTaskOnly(string message) { throw new System.NotImplementedException(); }
			public void LogTopLevelDataContextKey(GetDataContextKey getDataContextKey) { throw new System.NotImplementedException(); }
			public IEnumerable<IValidationRule> ValidationRuleCollection { get; set; }
		}

		class TestLog : ISimpleLog
		{
			internal TestLog(LogType type, string message)
			{
				Type = type;
				Message = message;
			}

			public LogType Type { get; }

			public string Message { get; }

			LogType ISimpleLog.Type => Type;
		}
	}
}
