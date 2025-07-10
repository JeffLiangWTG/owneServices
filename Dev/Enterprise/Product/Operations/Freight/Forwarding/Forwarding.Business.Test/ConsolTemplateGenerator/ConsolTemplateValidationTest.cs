using System;
using CargoWise.Data;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	sealed class ConsolTemplateValidationTest : BusinessObjectValidationTestCase
	{
		public void TestConsolTemplateReferenceId()
		{
			var templateName = $"Template-Name-{new Random().Next(999999)}";
			var templateRefId = SetupTemplateRecord(templateName);

			// Simulate WI00706479 - Consolidation template doesn't exist when importing flight schedule
			// Because it calls validate template by name but actually input value is reference id so the error message
			Template.ConsolTemplateName = templateRefId;
			AssertHasError(Template.ConsolTemplateNameInfo, "Consolidation template doesn't exist.");

			// No error for valid reference id
			Template.ConsolTemplateReferenceId = templateRefId;
			AssertNoNotifications(Template.ConsolTemplateReferenceIdInfo);

			// Reference id does not allow empty
			Template.ConsolTemplateReferenceId = ZString.Empty;
			AssertHasError(Template.ConsolTemplateReferenceIdInfo, "Please enter a value.");

			// Non-exist reference id
			Template.ConsolTemplateReferenceId = new Random().Next(999999).ToString();
			AssertHasError(Template.ConsolTemplateReferenceIdInfo, "Consolidation template doesn't exist.");

			// Create inactive template then validate
			templateName = $"Template-Name-{new Random().Next(999999)}";
			templateRefId = SetupTemplateRecord(templateName, false);
			Template.ConsolTemplateReferenceId = templateRefId;
			AssertHasError(Template.ConsolTemplateReferenceIdInfo, "Consolidation template should be active.");
		}

		public void TestConsolTemplateName()
		{
			SetupTemplateRecord("A");
			SetupTemplateRecord("C", isActive: false);

			Template.ConsolTemplateName = ZString.Empty;
			AssertNoNotifications("Please enter a value.", Template.ConsolTemplateNameInfo);

			Template.ConsolTemplateName = "B";
			AssertHasError(Template.ConsolTemplateNameInfo, "Consolidation template doesn't exist.");

			Template.ConsolTemplateName = "C";
			AssertHasError(Template.ConsolTemplateNameInfo, "Consolidation template should be active.");

			Template.ConsolTemplateName = "A";
			AssertNoNotifications(Template.ConsolTemplateNameInfo);

			Template.ConsolTemplateName = "a";
			AssertNoNotifications("Template name is case-insensitve", Template.ConsolTemplateNameInfo);
		}

		public void TestConsolsPerFlight()
		{
			Template.ConsolsPerFlight = -1;
			AssertHasError(Template.ConsolsPerFlightInfo, "value cannot be negative.");

			Template.ConsolsPerFlight = 0;
			AssertHasError(Template.ConsolsPerFlightInfo, "Please enter a value.");

			Template.ConsolsPerFlight = 10;
			AssertNoNotifications(Template.ConsolsPerFlightInfo);
		}

		#region Implementation

		ZString SetupTemplateRecord(ZString templateName, bool isActive = true)
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "AUBNE";

			var templateRecord = Factory.New<StmTemplateRecord>();
			templateRecord.STR_TemplateName = templateName;
			templateRecord.STR_ModuleID = ModuleIDs.JobConsol.Name;
			templateRecord.STR_IsActive = isActive;

			var templateRecordProvider = consol as ITemplateRecordProvider;
			templateRecordProvider.IsTemplateRecord = true;
			templateRecordProvider.TemplateRecord = templateRecord;

			using (((IDbConnected)Factory).Connection.BeginTransactionWithManager())
			{
				templateRecordProvider.SaveToTemplateRecord();
			}

			return templateRecord.STR_ReferenceId;
		}

		ConsolTemplate Template
		{
			get
			{
				return template ?? (template = new ConsolTemplate(MultiDaysSelectionForTest));
			}
		}
		ConsolTemplate template;

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

		#endregion
	}
}
