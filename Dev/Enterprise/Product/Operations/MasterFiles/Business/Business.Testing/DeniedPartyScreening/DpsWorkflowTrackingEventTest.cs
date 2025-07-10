using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.FeatureControl.Abstractions;
using Enterprise.ComplianceRisk.Integration;
using Enterprise.Freight.Integration;
using Enterprise.Freight.Integration.QuotedBooking;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq;
using static Enterprise.Core.Constants;
using static Enterprise.Integration.Customs;
using IForwardingConsol = Enterprise.Integration.Forwarding.IForwardingConsol;
using IForwardingShipment = Enterprise.Integration.Forwarding.IForwardingShipment;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class DpsWorkflowTrackingEventTest : TestCaseWithFactory
	{
		public void TestScreeningMatchConfidenceRating_EventReferenceShouldShowMediumAndHigh()
		{
			var organization = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();

			DpsWorkflowTrackingEvent.AddNew(organization, ScreeningStatusesList.Codes.NotScreened, ScreeningStatusesList.Codes.RequiresReview, null, ScreeningType.Silent, ScreeningMatchConfidenceRating.High);
			DpsWorkflowTrackingEvent.AddNew(organization, ScreeningStatusesList.Codes.NotScreened, ScreeningStatusesList.Codes.RequiresReview, null, ScreeningType.Silent, ScreeningMatchConfidenceRating.Medium);

			var query = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.DeniedPartyStatusUpdatedCode);
			var results = organization.GetLogs().Find(query);
			 
			CombineAssertions(() =>
			{
				AssertEquals(string.Format(CultureInfo.InvariantCulture, "New Status: Requires Review, Old Status: Not Screened, Score: High, Type: Silent Screen"), results.FirstOrDefault()?.DisplayEventReference);
				AssertContains(string.Format(CultureInfo.InvariantCulture, "|NEW=REQ|OLD=NOT|SCR=HIG|TYP=SIL"), results.FirstOrDefault()?.SL_Reference);
				AssertEquals(string.Format(CultureInfo.InvariantCulture, "New Status: Requires Review, Old Status: Not Screened, Score: Medium, Type: Silent Screen"), results.LastOrDefault()?.DisplayEventReference);
				AssertContains(string.Format(CultureInfo.InvariantCulture, "|NEW=REQ|OLD=NOT|SCR=MED|TYP=SIL"), results.LastOrDefault()?.SL_Reference);
			});
		}

		public void TestAddNew_WhenJobClearedExternally_ShouldShowEventReferenceTypeExternallySet()
		{
			var organization = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();

			DpsWorkflowTrackingEvent.AddNew(organization, ScreeningStatusesList.Codes.NotScreened, ScreeningStatusesList.Codes.JobClearedExternal, null, ScreeningType.ExternallySet, null);

			var query = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.DeniedPartyStatusUpdatedCode);
			var results = organization.GetLogs().Find(query);

			CombineAssertions(() =>
			{
				AssertEquals(string.Format(CultureInfo.InvariantCulture, "New Status: Job Cleared Ext, Old Status: Not Screened, Type: Externally Set"), results.FirstOrDefault()?.DisplayEventReference);
				AssertContains(string.Format(CultureInfo.InvariantCulture, "|NEW=JCE|OLD=NOT|TYP=EXT"), results.FirstOrDefault()?.SL_Reference);
			});
		}

		public void TestAddNew_WhenJobBlockedExternally_ShouldShowEventReferenceTypeExternallySet()
		{
			var organization = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();

			DpsWorkflowTrackingEvent.AddNew(organization, ScreeningStatusesList.Codes.NotScreened, ScreeningStatusesList.Codes.JobBlockedExternal, null, ScreeningType.ExternallySet, null);

			var query = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.DeniedPartyStatusUpdatedCode);
			var results = organization.GetLogs().Find(query);

			CombineAssertions(() =>
			{
				AssertEquals(string.Format(CultureInfo.InvariantCulture, "New Status: Job Blocked Ext, Old Status: Not Screened, Type: Externally Set"), results.FirstOrDefault()?.DisplayEventReference);
				AssertContains(string.Format(CultureInfo.InvariantCulture, "|NEW=JBE|OLD=NOT|TYP=EXT"), results.FirstOrDefault()?.SL_Reference);
			});
		}

		public void TestAddNew_WhenNewStatusIsSetMatched_ShouldOverwriteOldStatusJobClearedExternally()
		{
			var organization = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();

			DpsWorkflowTrackingEvent.AddNew(organization, ScreeningStatusesList.Codes.JobClearedExternal, ScreeningStatusesList.Codes.Matched, null, ScreeningType.Manual, null);

			var query = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.DeniedPartyStatusUpdatedCode);
			var results = organization.GetLogs().Find(query);

			CombineAssertions(() =>
			{
				AssertEquals(string.Format(CultureInfo.InvariantCulture, "New Status: Matched, Old Status: Job Cleared Ext, Type: Manual Screen"), results.FirstOrDefault()?.DisplayEventReference);
				AssertContains(string.Format(CultureInfo.InvariantCulture, "|NEW=MAT|OLD=JCE|TYP=MAN"), results.FirstOrDefault()?.SL_Reference);
			});
		}

		public void TestAddNew_WhenNewStatusIsSetMatched_ShouldOverwriteOldStatusJobBlockedExternally()
		{
			var organization = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();

			DpsWorkflowTrackingEvent.AddNew(organization, ScreeningStatusesList.Codes.JobBlockedExternal, ScreeningStatusesList.Codes.Matched, null, ScreeningType.Manual, null);

			var query = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.DeniedPartyStatusUpdatedCode);
			var results = organization.GetLogs().Find(query);

			CombineAssertions(() =>
			{
				AssertEquals(string.Format(CultureInfo.InvariantCulture, "New Status: Matched, Old Status: Job Blocked Ext, Type: Manual Screen"), results.FirstOrDefault()?.DisplayEventReference);
				AssertContains(string.Format(CultureInfo.InvariantCulture, "|NEW=MAT|OLD=JBE|TYP=MAN"), results.FirstOrDefault()?.SL_Reference);
			});
		}

		public void TestAddNew_WhenEnableComplianceRiskTrue_WithIComplianceRiskStatusProvider_DPEEventNotCreated()
		{
			AsserTrackingEventDPE((BusinessObject)Factory.New<IForwardingShipment>());
			AsserTrackingEventDPE((BusinessObject)Factory.New<IForwardingConsol>());
			AsserTrackingEventDPE((BusinessObject)ObjectFactory.Get<IQuotedBookingBuilder>().CreateNew(QuoteBookingType.QuickBooking, Factory));

			void AsserTrackingEventDPE(BusinessObject jobBizO)
			{
				using (FreightDataRegistry.Instance.FreightEnableComplianceWise.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty,
					ComplianceWiseRegistryHelper.SetValue(true)))
				using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				{
					AssertEquals("IComplianceRiskStatusProvider interface was implemented", true, jobBizO is IComplianceItemRiskStatusProvider);

					DpsWorkflowTrackingEvent.AddNew(jobBizO, ScreeningStatusesList.Codes.NotScreened, ScreeningStatusesList.Codes.RequiresReview, null, ScreeningType.Manual, ScreeningMatchConfidenceRating.High);

					var eventLog = (jobBizO as IStmALogParent).Logs.GetAllLogs().Where(x => x.SL_SE_NKEvent == Events.DeniedPartyStatusUpdatedCode);
					AssertEquals("Should not create DPE event log", 0, eventLog.Count());
				}
			}
		}

		public void TestAddNew_WhenEnableComplianceRiskTrue_WithDeclaration_DPEEvent()
		{
			var declaration = Factory.New<IBaseJobDeclaration>();

			AssertTrackingEventDPE(true, 0);
			AssertTrackingEventDPE(false, 1);

			void AssertTrackingEventDPE(bool enableCompliance, int expectedCount)
			{
				var jobBizO = declaration as BusinessObject;
				var complianceWiseFeatureRule = new ComplianceRiskFeatureControlRule { Enabled = true };
				var featureDataMock = new Mock<IFeatureData>();
				var featureControlMock = new Mock<IFeatureControlManager>();
				featureDataMock.Setup(x => x.TryDeserializeParameterAsJson(out complianceWiseFeatureRule)).Returns(enableCompliance);
				featureControlMock.Setup(x => x.GetFeatureDataAsync(CargoWise.Definitions.LicenceFeatureCodeList.Codes.ComplianceWiseCustomsDeclarationModule, CancellationToken.None)).Returns(Task.FromResult(featureDataMock.Object));
				using (ObjectFactory.Substitute(featureControlMock.Object))
				using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				{
					AssertEquals("IComplianceRiskStatusProvider interface was implemented", true, jobBizO is IComplianceItemRiskStatusProvider);

					DpsWorkflowTrackingEvent.AddNew(jobBizO, ScreeningStatusesList.Codes.NotScreened, ScreeningStatusesList.Codes.RequiresReview, null, ScreeningType.Manual, ScreeningMatchConfidenceRating.High);

					var eventLog = (jobBizO as IStmALogParent).Logs.GetAllLogs().Where(x => x.SL_SE_NKEvent == Events.DeniedPartyStatusUpdatedCode);
					AssertEquals("DPE event log count", expectedCount, eventLog.Count());
				}
			}
		}

		public void TestAddNew_WhenEnableComplianceRiskTrue_WithoutIComplianceRiskStatusProvider_DPEEventIsCreated()
		{
			AsserTrackingEventDPE((BusinessObject)Factory.New<IOrgHeader>());
			AsserTrackingEventDPE(Factory.New<RefVessel>());
			AsserTrackingEventDPE((BusinessObject)Factory.New<IWhsOrder>());
			AsserTrackingEventDPE((BusinessObject)Factory.New<IWhsReceive>());

			void AsserTrackingEventDPE(BusinessObject jobBizO)
			{
				using (FreightDataRegistry.Instance.FreightEnableComplianceWise.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty,
					ComplianceWiseRegistryHelper.SetValue(true)))
				using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				{
					AssertEquals("IComplianceRiskStatusProvider interface not implemented", false, jobBizO is IComplianceItemRiskStatusProvider);

					DpsWorkflowTrackingEvent.AddNew(jobBizO, ScreeningStatusesList.Codes.NotScreened, ScreeningStatusesList.Codes.RequiresReview, null, ScreeningType.Manual, ScreeningMatchConfidenceRating.High);

					var eventLog = (jobBizO as IStmALogParent).Logs.GetAllLogs().Where(x => x.SL_SE_NKEvent == Events.DeniedPartyStatusUpdatedCode);
					AssertEquals("Should create DPE event log", 1, eventLog.Count());
				}
			}
		}
	}
}
