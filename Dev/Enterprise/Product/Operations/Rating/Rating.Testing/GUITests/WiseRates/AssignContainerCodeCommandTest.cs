using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.RatingTests.Testing;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.Schema;
using DTO = WiseRates.Api.Model;

namespace Enterprise.Rating.GUI.Test
{
	public class AssignContainerCodeCommandTest : BaseRatingIntegrationTest
	{
		public void TestIsEnabled()
		{
			var command = new AssignContainerCodeCommand() as IWiseRatesCommand;

			var entryWithSeaContainerMapped = CreateWiseEntryView("SEA", "FCL", "22G0");
			entryWithSeaContainerMapped.Validation.ValidateAll();
			AssertEquals("Expected command to not be enabled for SEA container code 22G0.", false, command.IsEnabled(entryWithSeaContainerMapped));

			var entryWithSeaContainerNotMapped = CreateWiseEntryView("SEA", "FCL", "22R1");
			entryWithSeaContainerNotMapped.Validation.ValidateAll();
			AssertEquals("Expected command to be enabled for SEA container code 22R1.", true, command.IsEnabled(entryWithSeaContainerNotMapped));

			var entryWithEmptySeaContainer = CreateWiseEntryView("SEA", "LCL", "");
			entryWithEmptySeaContainer.Validation.ValidateAll();
			AssertEquals("Expected command to not be enabled for SEA container with empty code.", false, command.IsEnabled(entryWithEmptySeaContainer));

			var entryWithAirContainerMapped = CreateWiseEntryView("AIR", "ULD", "AAA");
			entryWithAirContainerMapped.Validation.ValidateAll();
			AssertEquals("Expected command to not be enabled for AIR container code AAA.", false, command.IsEnabled(entryWithAirContainerMapped));

			var entryWithAirContainerNotMapped = CreateWiseEntryView("AIR", "ULD", "AAB");
			entryWithAirContainerNotMapped.Validation.ValidateAll();
			AssertEquals("Expected command to be enabled for AIR container code AAB.", true, command.IsEnabled(entryWithAirContainerNotMapped));

			var entryWithEmptyAirContainer = CreateWiseEntryView("AIR", "ULD", "");
			entryWithEmptyAirContainer.Validation.ValidateAll();
			AssertEquals("Expected command to not be enabled for AIR container with empty code.", false, command.IsEnabled(entryWithEmptyAirContainer));
		}

		public void TestAssignForAir()
		{
			int firstContainerCount = 0, secondContainerCount = 0;

			ZFormModaliser.ShowDialogsInTest = true;
			ZFormModaliser.SetDelegateToCallOnFormShown(delegate(object dialog)
			{
				var embeddedPopup = (EmbeddedModulePopup)dialog;
				var module = embeddedPopup.Module_ForTest;
				var filter = module.FilterBusinessObject.Filter;
				var containers = Factory.Load<RefContainer>(filter);
				firstContainerCount = containers.Length;

				var newFactory = Factory.CreateNewFactory();
				var newAirContainer = newFactory.NewWithValidTestData<RefContainer>();
				newAirContainer.RC_Code = "AAB";
				newAirContainer.RC_ShippingMode = "AIR";
				newFactory.Save();

				containers = Factory.Load<RefContainer>(filter);
				secondContainerCount = containers.Length;

				var decisionProvider = embeddedPopup.Module_ForTest.ModuleDecisionProvider;
				decisionProvider.HandleDefaultAction(new BusinessObject[] { containers[0] });
			});

			var cmd = new AssignContainerCodeCommand();
			var wiseEntryView = CreateWiseEntryView("AIR", "ULD", "AAB");
			var assignmentResult = ((IWiseRatesCommand)cmd).Assign(wiseEntryView, null);

			AssertEquals("Assignment result should be true", true, assignmentResult);
			AssertEquals("First container count should be 0", 0, firstContainerCount);
			AssertEquals("Second container count should be 1", 1, secondContainerCount);
			var reloadedEntry = CreateWiseEntryView("AIR", "ULD", "AAB");
			AssertEquals("Reloaded entry TI_RC should not be empty", false, reloadedEntry.TI_RC.IsEmpty);
		}

		public void TestAssignForSea()
		{
			int firstContainerCount = 0, secondContainerCount = 0;

			ZFormModaliser.ShowDialogsInTest = true;
			ZFormModaliser.SetDelegateToCallOnFormShown(dialog =>
			{
				var embeddedPopup = (EmbeddedModulePopup)dialog;
				var module = embeddedPopup.Module_ForTest;
				var filter = module.FilterBusinessObject.Filter;
				var containers = Factory.Load<RefContainer>(filter);
				firstContainerCount = containers.Length;

				var newFactory = Factory.CreateNewFactory();
				var newContainer = newFactory.NewWithValidTestData<RefContainer>();
				newContainer.RC_Code = "AAB";
				newContainer.RC_ShippingMode = "SEA";
				newContainer.RC_ISOType = "22R1";
				newFactory.Save();

				containers = Factory.Load<RefContainer>(filter);
				secondContainerCount = containers.Length;

				var decisionProvider = embeddedPopup.Module_ForTest.ModuleDecisionProvider;
				decisionProvider.HandleDefaultAction(new BusinessObject[] { containers[0] });
			});

			var cmd = new AssignContainerCodeCommand();
			var wiseEntryView = CreateWiseEntryView("SEA", "FCL", "22R1");
			var assignmentResult = ((IWiseRatesCommand)cmd).Assign(wiseEntryView, null);

			AssertEquals("The assignment result should be true.", true, assignmentResult);
			AssertEquals("The initial container count should be zero.", 0, firstContainerCount);
			AssertEquals("The second container count should increment to one after addition.", 1, secondContainerCount);

			var reloadedEntry = CreateWiseEntryView("SEA", "FCL", "22R1");
			AssertEquals("The reloaded entry should not be empty.", false, reloadedEntry.TI_RC.IsEmpty);
		}

		WiseEntryView CreateWiseEntryView(string transportMode, string containerMode, string containerType)
		{
			RefContainer refContainer = null;
			var errorToAdd = string.Empty;

			if (!string.IsNullOrWhiteSpace(containerType))
			{
				refContainer = transportMode == "SEA"
					? Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_ISOType, containerType))
					: Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, containerType));
				errorToAdd = $"{containerType} container is not mapped";
			}
			var costing = CreateTestRate(transportMode, containerMode, "AUSYD", "USLAX", "", "", containerType, "REFCARRIER", "", "");
			costing.Container = new DTO.RefContainer
			{
				Code = containerType,
				ISOType = containerType
			};

			var entry = new WiseEntry(costing, Factory);
			// Mock rate mode and rate category settings. For real, see WiseRatesConverter.
			entry.TI_RateCategory = transportMode == "SEA" ? containerMode : "AIR";
			entry.TI_Mode = transportMode;
			if (refContainer != null)
			{
				entry.TI_RC = refContainer.PK;
			}
			else if (!string.IsNullOrWhiteSpace(errorToAdd))
			{
				// mock the container checking and error adding
				entry.Errors[RateEntrySchema.TI_RC] = errorToAdd;
			}

			var header = new WiseHeader(Factory);
			header.ChildRateEntries = new[] { entry };

			var response = new DTO.RatesSearchResponse
			{
				Containers = new[]
				{
					new DTO.RefContainer { Code = containerType, ISOType = containerType }
				}
			};

			return new WiseEntryView(entry, response);
		}
	}
}
