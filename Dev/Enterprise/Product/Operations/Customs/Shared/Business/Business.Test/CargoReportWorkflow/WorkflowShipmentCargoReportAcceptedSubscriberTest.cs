using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.LogWalker.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(WorkflowShipmentCargoReportAcceptedSubscriber))]
	sealed class WorkflowShipmentCargoReportAcceptedSubscriberTest : LogSubscriberTest<WorkflowShipmentCargoReportAcceptedSubscriber>
	{
		public void TestWorkflowShipmentCargoReportAcceptedMilestoneCreatedOnAttachedToConsol()
		{
			TestWorkflowShipmentCargoReportAcceptedMilestoneCreatedOnEventRaised(Events.Attached, true);
		}

		public void TestWorkflowShipmentCargoReportAcceptedMilestoneCreatedOnAttachedToConsol_TYPParameterMatched()
		{
			TestWorkflowShipmentCargoReportAcceptedMilestoneCreatedOnEventRaised(Events.Attached, true, new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Type, Core.Constants.EventReferenceParameterTypes.Consol));
		}

		void TestWorkflowShipmentCargoReportAcceptedMilestoneCreatedOnEventRaised(Event ev, bool expectedResult, params KeyValuePair<string, string>[] parameters)
		{
			GlbGroup group = Factory.New<GlbGroup>();
			CustomsDataRegistry.Instance.GroupToSendLateSeaCargoReportAndWarningsTo.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, group.PK.ToGuid());
			Shipment.Logs.RemoveAndDeleteAll();
			Shipment.Logs.AddNew(ev, new ZDateTimeOffset(2005, 1, 2), parameters);
			Factory.Save();
			AssertNull("No Milestone on shipment", CargoReportAcceptedMilestone(Shipment));
			RunLogWalkerCycleForTest();
			AssertEquals("Milestone on shipment", expectedResult, CargoReportAcceptedMilestone(Shipment) != null);
		}

		public void TestWorkflowShipmentCargoReport1()
		{
			TestWorkflowShipmentCargoReportAcceptedMilestoneCreatedBytransportMode(Core.Constants.TransportModes.Sea, false, false, false);
		}

		public void TestWorkflowShipmentCargoReport2()
		{
			TestWorkflowShipmentCargoReportAcceptedMilestoneCreatedBytransportMode(Core.Constants.TransportModes.Sea, true, false, true);
		}

		public void TestWorkflowShipmentCargoReport3()
		{
			TestWorkflowShipmentCargoReportAcceptedMilestoneCreatedBytransportMode(Core.Constants.TransportModes.Sea, true, true, true);
		}

		public void TestWorkflowShipmentCargoReport4()
		{
			TestWorkflowShipmentCargoReportAcceptedMilestoneCreatedBytransportMode(Core.Constants.TransportModes.Sea, false, true, false);
		}

		public void TestWorkflowShipmentCargoReport5()
		{
			TestWorkflowShipmentCargoReportAcceptedMilestoneCreatedBytransportMode(Core.Constants.TransportModes.Air, false, false, false);
		}

		public void TestWorkflowShipmentCargoReport6()
		{
			TestWorkflowShipmentCargoReportAcceptedMilestoneCreatedBytransportMode(Core.Constants.TransportModes.Air, true, false, false);
		}

		public void TestWorkflowShipmentCargoReport7()
		{
			TestWorkflowShipmentCargoReportAcceptedMilestoneCreatedBytransportMode(Core.Constants.TransportModes.Air, true, true, true);
		}

		public void TestWorkflowShipmentCargoReport8()
		{
			TestWorkflowShipmentCargoReportAcceptedMilestoneCreatedBytransportMode(Core.Constants.TransportModes.Air, false, true, true);
		}

		void TestWorkflowShipmentCargoReportAcceptedMilestoneCreatedBytransportMode(ZString transportMode, bool seaActive, bool airActive, bool expectedResult)
		{
			GlbGroup group = Factory.New<GlbGroup>();
			CustomsDataRegistry.Instance.GroupToSendLateSeaCargoReportAndWarningsTo.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, seaActive ? group.PK.ToGuid() : Guid.Empty);
			CustomsDataRegistry.Instance.GroupToSendLateAirCargoReportAndWarningsTo.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, airActive ? group.PK.ToGuid() : Guid.Empty);
			Shipment.JS_TransportMode = transportMode;
			Shipment.Logs.RemoveAndDeleteAll();
			Shipment.Logs.AddNew(Events.Attached, new ZDateTimeOffset(2005, 1, 2));
			Factory.Save();
			AssertNull("No Milestone on shipment", CargoReportAcceptedMilestone(Shipment));
			RunLogWalkerCycleForTest();
			AssertEquals("Required result", expectedResult, CargoReportAcceptedMilestone(Shipment) != null);
		}

		public void TestWorkflowShipmentCargoReportAcceptedMilestoneCreatedWhenTaskInitiatedFromOtherCompany()
		{
			GlbGroup group = Factory.New<GlbGroup>();
			CustomsDataRegistry.Instance.GroupToSendLateSeaCargoReportAndWarningsTo.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, group.PK.ToGuid());

			GlbCompany otherCompany = Factory.New<GlbCompany>();
			otherCompany.GC_Code = "OTH";
			otherCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Canada;
			GlbBranch otherBranch = otherCompany.Branches.AddNew();
			otherBranch.GB_Code = "XYZ";
			otherBranch.GB_RL_NKHomePort = "CAWYZ";
			Factory.Save();

			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, otherBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				Shipment.Logs.RemoveAndDeleteAll();
				Shipment.Logs.AddNew(Events.Attached, "AUSYD", new ZDateTimeOffset(2005, 1, 2));
				Factory.Save();
				AssertNull("No Milestone on shipment", CargoReportAcceptedMilestone(Shipment));
				RunLogWalkerCycleForTest();
				AssertNotNull("Milestone on shipment", CargoReportAcceptedMilestone(Shipment));
			}
		}
		public void TestWhenCurrentCompanyHasNoActiveBranch()
		{
			GlbGroup group = Factory.New<GlbGroup>();
			CustomsDataRegistry.Instance.GroupToSendLateSeaCargoReportAndWarningsTo.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, group.PK.ToGuid());

			GlbCompany testCompany = Factory.New<GlbCompany>();
			testCompany.GC_Code = "OTH";
			testCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			GlbBranch testBranch = testCompany.Branches.AddNew();
			testBranch.GB_Code = "OTH";
			testBranch.GB_RL_NKHomePort = "AUSUD";
			testBranch.GB_IsActive = false;
			Factory.Save();

			using (testBranch.SetAsTemporaryContext())
			{
				Shipment.Logs.RemoveAndDeleteAll();
				Shipment.Logs.AddNew(Events.Attached, "AUMEL", new ZDateTimeOffset(2005, 1, 2));
				Factory.Save();
				AssertNull("No Milestone on shipment", CargoReportAcceptedMilestone(Shipment));
				RunLogWalkerCycleForTest();
				AssertNull("Still no Milestone on shipment, as no valid branch on current non-demo AU company", CargoReportAcceptedMilestone(Shipment));
			}
		}

		public void TestNoTemplateApplication()
		{
			GlbGroup group = Factory.New<GlbGroup>();
			CustomsDataRegistry.Instance.GroupToSendLateSeaCargoReportAndWarningsTo.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, group.PK.ToGuid());

			GlbCompany testCompany = Factory.New<GlbCompany>();
			testCompany.GC_Code = "OTH";
			testCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			GlbBranch testBranch = testCompany.Branches.AddNew();
			testBranch.GB_Code = "OTH";
			testBranch.GB_RL_NKHomePort = "AUSUD";
			testBranch.GB_IsActive = false;
			Factory.Save();

			using (testBranch.SetAsTemporaryContext())
			{
				Shipment.Logs.RemoveAndDeleteAll();
				Shipment.Logs.AddNew(Events.Attached, "AUMEL", new ZDateTimeOffset(2005, 1, 2));
				Factory.Save();

				var otherFactory = new BusinessObjectFactory();
				var template = otherFactory.NewWithValidTestData<ProcessTaskTemplate>();
				template.P0_ProcessType = "SHP";
				var task = template.WorkflowItems.Tasks.AddNew();
				task.P9_Description = "Tasky boi";
				otherFactory.Save();

				var query = new ZQuery(StmALogSchema.SL_Parent, Shipment.PK)
					.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.WorkflowTemplateAppliedCode);
				query.ReLoadExistingRows = true;
				var logs = Factory.Load<StmALog>(query);
				RunLogWalkerCycleForTest();
				var logs2 = Factory.Load<StmALog>(query);
				AssertEquals("No extra shipment edit log", logs.Length, logs2.Length);
			}
		}

		public void TestWhenDemoCompanyHasNoActiveBranch()
		{
			GlbGroup group = Factory.New<GlbGroup>();
			CustomsDataRegistry.Instance.GroupToSendLateSeaCargoReportAndWarningsTo.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, group.PK.ToGuid());

			GlbCompany testCompany = Factory.New<GlbCompany>();
			testCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			GlbBranch testBranch = testCompany.Branches.AddNew();
			testBranch.GB_Code = "OTH";
			testBranch.GB_RL_NKHomePort = "AUSUD";
			testBranch.GB_IsActive = false;
			GlbCompany demoCompany = Factory.LoadFromNaturalKey<GlbCompany>(GlbCompanySchema.GC_Code, "DEM");
			try
			{
				testCompany.GC_Code = "OTH";
				demoCompany.GC_Code = "XYZ";
				Factory.Save();
				testCompany.GC_Code = "DEM";
				Factory.Save();

				using (testBranch.SetAsTemporaryContext())
				{
					Shipment.Logs.RemoveAndDeleteAll();
					Shipment.Logs.AddNew(Events.Attached, "AUMEL", new ZDateTimeOffset(2005, 1, 2));
					Factory.Save();
					AssertNull("No Milestone on shipment", CargoReportAcceptedMilestone(Shipment));
					RunLogWalkerCycleForTest();
					AssertNotNull("Milestone on shipment, as finds valid non demo AU company", CargoReportAcceptedMilestone(Shipment));
				}
			}
			finally
			{
				testCompany.GC_Code = "OTH";
				Factory.Save();
				demoCompany.GC_Code = "DEM";
				Factory.Save();
			}
		}

		public void TestCargoAvailableSubscriberDoesNotApplyWorkflowInAUCompany()
		{
			using (ProcessTask.Loader.SuppressTemplateApplication())
			{
				var template = Factory.New<ProcessTaskTemplate>();
				template.P0_ProcessType = "SHP";
				template.P0_Name = "Banana";
				template.GlobalTemplate = true;
				var t1 = template.WorkflowItems.Triggers.AddNew();
				t1.TriggerConditions.TriggerEventCode = Events.CustomisableEvent00Code;
				t1.P9_Description = "Japan";

				var testCompany = Factory.New<GlbCompany>();
				testCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
				var testBranch = testCompany.Branches.AddNew();
				testBranch.GB_Code = "OTH";
				testBranch.GB_RL_NKHomePort = "AUSUD";
				testBranch.GB_IsActive = false;
				var demoCompany = Factory.LoadFromNaturalKey<GlbCompany>(GlbCompanySchema.GC_Code, "DEM");
				demoCompany.GC_Code = "XYZ";
				testCompany.GC_Code = "DEM";

				var group = Factory.New<GlbGroup>();
				CustomsDataRegistry.Instance.GroupToSendLateSeaCargoReportAndWarningsTo.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, group.PK.ToGuid());

				Shipment.Logs.RemoveAndDeleteAll();
				Shipment.Logs.AddNew(Events.Attached, "AUMEL", new ZDateTimeOffset(2005, 1, 2));
				Factory.Save();

				AssertNull("No Milestone on shipment", CargoReportAcceptedMilestone(Shipment));
			}

			RunLogWalkerCycleForTest();
			Shipment.WorkflowItems.Reload(true);
			AssertEquals(0, Shipment.WorkflowItems.Triggers.Count);
		}

		#region Implementation

		ForwardingShipment Shipment
		{
			get
			{
				if (shipment == null)
				{
					shipment = Factory.New<ForwardingShipment>();
					shipment.JS_RL_NKOrigin = "XXYYY";
					shipment.JS_RL_NKDestination = "AUSYD";
					shipment.JS_IsForwardRegistered = true;
					shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
				}
				return shipment;
			}
		}
		ForwardingShipment shipment;

		ProcessTask CargoReportAcceptedMilestone(ForwardingShipment shipment)
		{
			ProcessTask result = null;
			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			ForwardingShipment shipment2 = factory2.Load<ForwardingShipment>(shipment.PK);
			foreach (ProcessTask mileStone in shipment2.WorkflowItems.Milestones)
			{
				if (mileStone.P9_Description == "Cargo Report Accepted")
				{
					result = mileStone;
					break;
				}
			}
			return result;
		}

		#endregion
	}
}
