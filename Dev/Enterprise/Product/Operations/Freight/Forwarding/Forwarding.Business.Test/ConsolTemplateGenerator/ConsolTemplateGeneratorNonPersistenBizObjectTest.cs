using System.Collections.Generic;
using System.Linq;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	[TestedType(typeof(ConsolTemplateGenerator))]
	sealed class ConsolTemplateGeneratorNonPersistenBizObjectTest : NonPersistentBusinessObjectTestCase
	{
		public void TestCustomField()
		{
			SetupTemplateRecord();

			var sailings = new JobSailingCollection(Factory);
			var sailing = CreateSailing();
			sailings.Add(sailing);

			var multiDaysSelection = new MultiDaysSelection(sailings, Factory);
			var generator = new ConsolTemplateGenerator(multiDaysSelection);

			var template = generator.ConsolTemplates.AddNew();
			template.ConsolTemplateReferenceId = "TR00001000";
			template.ConsolsPerFlight = 1;

			generator.GenerateConsols(new List<JobSailingCollection> { sailings });
			AssertEquals(1, multiDaysSelection.CreatedConsols.Count);

			var consol = multiDaysSelection.CreatedConsols[0];
			var customFieldValue = consol.GetUserDefinedValue<ZString>("Test Field");
			AssertEquals("Test Value 123", customFieldValue);
		}

		public void TestNeutralAirWaybillServiceLevelList()
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_IsShippingLine = true;
			carrier.OH_IsAirLine = true;
			carrier.MiscServ.OM_RM_Airline = RefAirline.LoadFromAirline2LetterCode(Factory, "SQ").PK;

			var serviceLevel1 = carrier.MiscServ.CarrierServiceLevels.AddNew();
			serviceLevel1.PL_Code = "XXX";
			serviceLevel1.PL_CarrierServiceLevelDescription = "XXX Description";
			var serviceLevel2 = carrier.MiscServ.CarrierServiceLevels.AddNew();
			serviceLevel2.PL_Code = "EXP";
			serviceLevel2.PL_CarrierServiceLevelDescription = "EXP Description";

			Factory.Save();

			var sailings = new JobSailingCollection(Factory);
			var sailing = CreateSailing();
			sailings.Add(sailing);

			var multiDaysSelection = new MultiDaysSelection(sailings, Factory);
			var generator = new ConsolTemplateGenerator(multiDaysSelection);

			AssertEquals("XXX, EXP and STD", 3, generator.NeutralAirWaybillServiceLevelList.Count);
			var serviceLevels = generator.NeutralAirWaybillServiceLevelList.Cast<OrgCarrierServiceLevel>();
			AssertNotNull(serviceLevels.FirstOrDefault(s => s.PL_Code == "XXX"));
			AssertNotNull(serviceLevels.FirstOrDefault(s => s.PL_Code == "EXP"));
			AssertNotNull(serviceLevels.FirstOrDefault(s => s.PL_Code == OrgCarrierServiceLevel.StandardCode));
		}

		public void TestConsolServiceLevel()
		{
			SetupTemplateRecord();

			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_IsShippingLine = true;
			carrier.OH_IsAirLine = true;
			carrier.MiscServ.OM_RM_Airline = RefAirline.LoadFromAirline2LetterCode(Factory, "MU").PK;

			var serviceLevel1 = carrier.MiscServ.CarrierServiceLevels.AddNew();
			serviceLevel1.PL_Code = "XXX";
			serviceLevel1.PL_CarrierServiceLevelDescription = "XXX Description";
			var serviceLevel2 = carrier.MiscServ.CarrierServiceLevels.AddNew();
			serviceLevel2.PL_Code = "EXP";
			serviceLevel2.PL_CarrierServiceLevelDescription = "EXP Description";

			Factory.Save();

			var sailings = new JobSailingCollection(Factory);
			var sailing = CreateSailing();
			sailings.Add(sailing);

			var multiDaysSelection = new MultiDaysSelection(sailings, Factory);
			var generator = new ConsolTemplateGenerator(multiDaysSelection);
			generator.ServiceLevel = "EXP";

			var template = generator.ConsolTemplates.AddNew();
			template.ConsolTemplateReferenceId = "TR00001000";
			template.ConsolsPerFlight = 1;

			generator.GenerateConsols(new List<JobSailingCollection> { sailings });

			AssertEquals(1, multiDaysSelection.CreatedConsols.Count);
			var consol = multiDaysSelection.CreatedConsols[0];
			AssertEquals("EXP", consol.JK_AWBServiceLevel);
		}

		public void TestConsolCopyDatesAndPorts()
		{
			SetupTemplateRecord();

			var sailings = new JobSailingCollection(Factory);
			var sailing = CreateSailing();
			sailings.Add(sailing);

			var multiDaysSelection = new MultiDaysSelection(sailings, Factory);
			var generator = new ConsolTemplateGenerator(multiDaysSelection);

			var template = generator.ConsolTemplates.AddNew();
			template.ConsolTemplateReferenceId = "TR00001000";
			template.ConsolsPerFlight = 1;

			generator.GenerateConsols(new List<JobSailingCollection> { sailings });
			AssertEquals(1, multiDaysSelection.CreatedConsols.Count);

			var consol = multiDaysSelection.CreatedConsols[0];
			AssertEquals(new ZDateTime(2022, 4, 13), consol.JK_ConsolCutOffDate);
			AssertEquals(new ZDateTime(2022, 4, 14), consol.JK_DateFirstForeignPort);
			AssertEquals("NZAKL", consol.JK_RL_NKLastForeignPort);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return new ConsolTemplateGenerator(MultiDaysSelectionForTest);
		}

		MultiDaysSelection MultiDaysSelectionForTest
		{
			get
			{
				var sailings = new JobSailingCollection(Factory);
				sailings.Add(CreateSailing());

				return new MultiDaysSelection(sailings, Factory);
			}
		}

		JobSailing CreateSailing()
		{
			var voyage = Factory.New<JobVoyage>();
			voyage.JV_AirSeaRoad = Core.Constants.TransportModes.Air;
			voyage.JV_VoyageFlight = "SQ22";

			var origin = voyage.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = "AUSYD";
			origin.JA_E_DEP = ZDateTime.Today;

			var destination = voyage.Destinations.AddNew();
			destination.JB_RL_NKPortOfDischarge = "HKHKG";
			destination.JB_E_ARV = ZDate.Today.AddDays(1);

			voyage.GenerateSailings();

			var sailing = voyage.Sailings[0];
			sailing.JX_IsPublished = true;

			return sailing;
		}

		void SetupTemplateRecord()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "NZAKL";
			consol.JK_ConsolCutOffDate = new ZDateTime(2022, 4, 13);
			consol.JK_DateFirstForeignPort = new ZDateTime(2022, 4, 14);
			consol.JK_RL_NKLastForeignPort = "NZAKL";
			consol.SetUserDefinedValue("Test Field", new ZString("Test Value 123"));

			var templateRecord = Factory.New<StmTemplateRecord>();
			templateRecord.STR_TemplateName = "A";
			templateRecord.STR_ModuleID = ModuleIDs.JobConsol.Name;

			var templateRecordProvider = consol as ITemplateRecordProvider;
			templateRecordProvider.IsTemplateRecord = true;
			templateRecordProvider.TemplateRecord = templateRecord;

			using (((IDbConnected)Factory).Connection.BeginTransactionWithManager())
			{
				templateRecordProvider.SaveToTemplateRecord();
			}
		}

		#endregion
	}
}
