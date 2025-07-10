using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.CarbonEmissions.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using Moq;

namespace Enterprise.Freight.Forwarding.GUI.Testing
{
	public class CO2ePluginTest : TestCaseWithFactory
	{
		public void TestWithSupportedBizo()
		{
			var shipment = Factory.New<ForwardingShipment>();
			using (var form = new ZForm(shipment))
			{
				form.PlugIns.Add(ControllerIDs.CO2ePlugin);
				form.Show();

				AssertEquals("Should be no errors thrown.", 0, ErrorReporter.TotalErrorCount);
				AssertNotNull("Form actions menu should contain 'Calculate Greenhouse Gas Emissions (CO2e)' item.", GetCalculateCO2eEmissionMenuItem(form));
			}
		}

		public void TestWithUnsupportedBizo()
		{
			var dummy = Factory.New<DummyBusinessObject>();
			using (var form = new ZTestForm(dummy))
			{
				form.PlugIns.Add(ControllerIDs.CO2ePlugin);
				form.Show();

				AssertEquals("Should throw 1 error.", 1, ErrorReporter.TotalErrorCount);
				AssertNull("Form actions menu should contain 'Calculate Greenhouse Gas Emissions' (CO2e) item.", GetCalculateCO2eEmissionMenuItem(form));
				ErrorReporter.Clear();
			}
		}

		public void TestCalculateCO2eEmission_HasChanges()
		{
			// Arrange
			var hostSupporter = new Mock<ICO2eCalculationSupporter>();
			hostSupporter.As<IBusiness>();
			hostSupporter.As<IBusiness>().Setup(x => x.HasChanges).Returns(true);

			using (var form = new ZForm(hostSupporter.Object))
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.ClearUserResponses();
				form.PlugIns.Add(ControllerIDs.CO2ePlugin);
				form.Show();

				// Act
				GetCalculateCO2eEmissionMenuItem(form).PerformClick();

				// Assert
				var lastMessage = UnitTestUserNotification.Instance.LastMessage;
				CombineAssertions(() =>
				{
					AssertEquals("Check message caption", "Request failed", lastMessage.Caption);
					AssertEquals("Check message text", "Please save before calculating greenhouse gas emissions.", lastMessage.Text);
				});
			}
		}

		public void TestCalculateCO2eEmission_MultipleSupportersValidateInputs()
		{
			var hostSupporter = new Mock<ICO2eCalculationSupporter>();
			hostSupporter.As<IBusiness>().Setup(x => x.HasChanges).Returns(false);
			hostSupporter.As<IBusiness>().Setup(x => x.HumanReadableName).Returns("Host");
			hostSupporter.Setup(x => x.ValidateInputs()).Returns(new List<string> { "not good", "bad" });

			var childSupporter1 = new Mock<ICO2eCalculationSupporter>();
			childSupporter1.As<IBusiness>().Setup(x => x.HumanReadableName).Returns("First Child");
			childSupporter1.Setup(x => x.ValidateInputs()).Returns(new List<string> { "worse", "worst" });

			var childSupporter2 = new Mock<ICO2eCalculationSupporter>();
			childSupporter2.As<IBusiness>().Setup(x => x.HumanReadableName).Returns("Second Child");
			childSupporter2.Setup(x => x.ValidateInputs()).Returns(new List<string> { "hello", "world" });

			hostSupporter.Setup(x => x.AdditionalCalculationSupporters)
				.Returns(new[]
				{
					new AdditionalCalculationSupporter(childSupporter1.Object, () => 0m),
					new AdditionalCalculationSupporter(childSupporter2.Object, () => 0m)
				});

			using (var form = new ZForm(hostSupporter.Object))
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.ClearUserResponses();
				form.PlugIns.Add(ControllerIDs.CO2ePlugin);
				form.Show();

				// Act
				GetCalculateCO2eEmissionMenuItem(form).PerformClick();

				// Assert
				var lastMessage = UnitTestUserNotification.Instance.LastMessage;
				CombineAssertions(() =>
				{
					AssertEquals("Check message caption", "Request failed", lastMessage.Caption);
					AssertMultilineASCIIEquals("Check message text", @"The greenhouse gas emissions calculation cannot be requested because following mandatory input is missing or invalid:
Host: not good
Host: bad
First Child: worse
First Child: worst
Second Child: hello
Second Child: world"
						, lastMessage.Text);
				});
			}
		}

		MenuItem GetCalculateCO2eEmissionMenuItem(ZForm form)
		{
			return form.Menu.MenuItems[ZFormMenuStrategy.ActionsMenuItemName].MenuItems.FindByText("Calculate Greenhouse Gas Emissions (CO2e)");
		}

		public void TestRegisterCO2eRecalculationChecker()
		{
			var shipment = Factory.New<ForwardingShipment>();
			using (var form = new ZForm(shipment))
			{
				form.PlugIns.Add(ControllerIDs.CO2ePlugin);
				form.Show();

				AssertEquals(true, shipment.Factory.GetValue<ICO2eRecalculationChecker>() is CO2eRecalculationGUIChecker);
			}
		}
	}
}
