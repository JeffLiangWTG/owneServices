using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using CargoWise.FeatureControl.Abstractions;
using Enterprise.ComplianceRisk.Business;
using Enterprise.ComplianceRisk.Integration;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Moq;

namespace Enterprise.Customs.GUI.Testing
{
	public class ComplianceRiskBaseJobDeclarationFormTest : TestCaseWithFactory
	{
		public void TestDeclarationComplianceMessageBannerWhenEnableCompliance()
		{
			AssertRiskBannerVisible(ComplianceRiskStatusCodeList.Codes.Blocked);
			AssertRiskBannerVisible(ComplianceRiskStatusCodeList.Codes.Clear);

			void AssertRiskBannerVisible(string status)
			{
				var declaration = Factory.New<BaseJobDeclaration>();
				var complianceRisk = Factory.NewWithValidTestData<ComplianceRiskStatus>();
				complianceRisk.COR_ParentTableCode = declaration.TablePrefix;
				complianceRisk.COR_ParentID = declaration.PK;

				Factory.Save();

				var complianceWiseFeatureRule = new ComplianceRiskFeatureControlRule { Enabled = true };
				var featureDataMock = new Mock<IFeatureData>();
				var featureControlMock = new Mock<IFeatureControlManager>();
				featureDataMock.Setup(x => x.TryDeserializeParameterAsJson(out complianceWiseFeatureRule)).Returns(true);
				featureControlMock.Setup(x => x.GetFeatureDataAsync(CargoWise.Definitions.LicenceFeatureCodeList.Codes.ComplianceWiseCustomsDeclarationModule, CancellationToken.None)).Returns(Task.FromResult(featureDataMock.Object));
				using (ObjectFactory.Substitute(featureControlMock.Object))
				using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				using (OrganisationsDataRegistry.Instance.EnableComplianceWarningMessage.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				using (var form = new BaseJobDeclarationForm(declaration))
				{
					form.Show();
					complianceRisk.COR_OverallRisk = status;
					Application.DoEvents();

					var riskBanner = form.MainStatusBar.FindSingle<ZLabel>(x => x.Name == "CompliancePotentialRiskMessageBanner");
					if (status == ComplianceRiskStatusCodeList.Codes.Clear)
					{
						AssertEquals(false, riskBanner.Visible);
					}
					else
					{
						AssertEquals("Job Compliance status is not Clear. View the Compliance Risk tab.", riskBanner.Text);
						AssertEquals(true, riskBanner.Visible);
					}
				}
			}
		}
	}
}
