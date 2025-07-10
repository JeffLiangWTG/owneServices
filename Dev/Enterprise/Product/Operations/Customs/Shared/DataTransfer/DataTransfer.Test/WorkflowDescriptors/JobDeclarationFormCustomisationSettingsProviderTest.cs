using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Application;
using CargoWise.FeatureControl.Abstractions;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(JobDeclarationFormCustomisationSettingsProvider))]
	sealed class JobDeclarationFormCustomisationSettingsProviderTest : FormCustomisationSettingsProviderTest<JobDeclarationFormCustomisationSettingsProvider>
	{
		public override void TestPropertiesThatAffectWorkflow()
		{
			var expected = new string[]
			{
				BaseJobDeclaration.Schema.JE_TransportMode,
				BaseJobDeclaration.Schema.JE_MessageType,
				BaseJobDeclaration.Schema.JE_RL_NKPortOfLoading,
				BaseJobDeclaration.Schema.JE_RL_NKPortOfArrival,
				BaseJobDeclaration.Schema.JE_OH_Importer,
				BaseJobDeclaration.Schema.JE_OH_Supplier,
				BaseJobDeclaration.Schema.JE_GB
			};
			var provider = GetNewProvider();
			AssertContainsExactElementsInAnyOrder(expected, provider.PropertiesThatAffectWorkflow);
		}

		public override void TestDisplayTabs()
		{
			const string complianceRiskTabName = "ComplianceRiskTabPage";
			var expectedTabNames = new[]
			{
				"RoutingTabPage",
				"ContainerTabPage",
				"InvoiceGroupingTabPage",
				"PickupTabPage",
				"DeliveryTabPage",
				"WorkflowTabPage",
				"LandedCostingTabPage",
				"BillingTabPage",
				"AddressesTabPage",
				"DocDataTabPage",
				"eDocsTabPage",
				"BrokerageStmNoteTabPage",
				"EventTabPage",
				complianceRiskTabName
			};

			AssertDisplayTabs(enableComplianceWise: false);
			AssertDisplayTabs(enableComplianceWise: true);

			void AssertDisplayTabs(bool enableComplianceWise)
			{
				if (enableComplianceWise)
				{
					var complianceWiseFeatureRule = new ComplianceRiskFeatureControlRule { Enabled = true };
					var featureDataMock = new Mock<IFeatureData>();
					var featureControlMock = new Mock<IFeatureControlManager>();
					featureDataMock.Setup(x => x.TryDeserializeParameterAsJson(out complianceWiseFeatureRule)).Returns(true);
					featureControlMock.Setup(x => x.GetFeatureDataAsync(CargoWise.Definitions.LicenceFeatureCodeList.Codes.ComplianceWiseCustomsDeclarationModule, CancellationToken.None)).Returns(Task.FromResult(featureDataMock.Object));
					using (ObjectFactory.Substitute(featureControlMock.Object))
					{
						AssertTabResult(enableComplianceWise);
					}
				}
				else
				{
					AssertTabResult(enableComplianceWise);
				}
			}

			void AssertTabResult(bool enableComplianceWise)
			{
				var provider = GetNewProvider();
				var tabs = provider.DisplayTabs;
				AssertEquals($"Expected {expectedTabNames.Length} tabs", expectedTabNames.Length, tabs.Count);
				AssertContainsExactElementsInAnyOrder(expectedTabNames.Select(tabName =>
				{
					if (tabName == complianceRiskTabName)
					{
						return (complianceRiskTabName, enableComplianceWise);
					}
					else
					{
						return (tabName, true);
					}
				}), tabs.Cast<FormCustomisableElement>().Select(elem => ((string)elem.ElementName, (bool)elem.IsAvailable)).ToArray());
			}
		}

		public override void TestTabPlacementProhibitions()
		{
			var provider = GetNewProvider();
			AssertEquals(0, provider.TabPlacementProhibitions.Length);
		}

		public override JobDeclarationFormCustomisationSettingsProvider GetNewProvider() => new JobDeclarationFormCustomisationSettingsProvider();
	}
}
