using System;
using System.Windows.Forms;
using Enterprise.ComplianceRisk.Business;
using Enterprise.Freight.Business;
using Enterprise.Freight.Business.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Forwarding.GUI.Testing
{
	public class ComplianceRiskConsolFormTest : BaseFreightTest
	{
		public void TestComplianceMessageBannerWhenValidRegisrtySecurityShowVisibleON_PotentialRisk()
		{
			AssertRiskBannerVisible("PSK");
		}

		public void TestComplianceMessageBannerWhenValidRegisrtySecurityShowVisibleON_Blocked()
		{
			AssertRiskBannerVisible("BLK");
		}

		public void TestComplianceMessageBannerWhenValidRegisrtySecurityShowVisibleON_Held()
		{
			AssertRiskBannerVisible("HLD");
		}

		void AssertRiskBannerVisible(string status)
		{
			var consol = Factory.New<ForwardingConsol>();
			var complianceRisk = Factory.NewWithValidTestData<ComplianceRiskStatus>();
			complianceRisk.COR_ParentTableCode = consol.TablePrefix;
			complianceRisk.COR_ParentID = consol.PK;

			Factory.Save();

			ChildEditableService.SetState(Factory, ChildEditableServiceStates.Shipment);

			using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (OrganisationsDataRegistry.Instance.EnableComplianceWarningMessage.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (FreightDataRegistry.Instance.FreightEnableComplianceWise.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty,
					   ComplianceWiseRegistryHelper.SetValue(true)
				   ))
			using (var form = new ConsolForm(consol))
			{
				form.Show();
				complianceRisk.COR_OverallRisk = status;
				Application.DoEvents();

				var riskBanner = form.MainStatusBar.FindSingle<ZLabel>(x => x.Name == "CompliancePotentialRiskMessageBanner");
				AssertEquals("Job Compliance status is not Clear. View the Compliance Risk tab.", riskBanner.Text);
				AssertEquals(true, riskBanner.Visible);
			}
		}

		public void TestCompliancePotentialRiskMessageBannerWhenInvalidRegisrtySecurityShowVisibleOFf()
		{
			var consol = Factory.New<ForwardingConsol>();
			var complianceRisk = Factory.NewWithValidTestData<ComplianceRiskStatus>();
			complianceRisk.COR_ParentTableCode = consol.TablePrefix;
			complianceRisk.COR_ParentID = consol.PK;
			complianceRisk.COR_OverallRisk = ComplianceRisk.Integration.ComplianceRiskStatusCodeList.Codes.OverrideClear;

			Factory.Save();

			ChildEditableService.SetState(Factory, ChildEditableServiceStates.Shipment);

			using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (OrganisationsDataRegistry.Instance.EnableComplianceWarningMessage.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (FreightDataRegistry.Instance.FreightEnableComplianceWise.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty,
				ComplianceWiseRegistryHelper.SetValue(true)))
			using (var form = new ConsolForm(consol))
			{
				form.Show();
				Application.DoEvents();

				var riskBanner = form.MainStatusBar.FindSingleOrDefault<ZLabel>(x => x.Name == "CompliancePotentialRiskMessageBanner");
				AssertNull(riskBanner);
			}
		}
	}
}
