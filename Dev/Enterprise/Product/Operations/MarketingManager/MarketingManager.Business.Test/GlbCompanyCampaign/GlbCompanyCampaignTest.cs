using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MarketingManager.Business.Testing
{
	[TestedType(typeof(GlbCompanyCampaign))]
	public class GlbCompanyCampaignTest : EnterpriseBusinessObjectTestCase
	{
		public void TestTranslatableDataFieldAttributeHasColumn_G0_CampaignName()
		{
			var property = typeof(GlbCompanyCampaign).GetProperty(GlbCompanyCampaign.Schema.G0_CampaignName);
			AssertNotNull(property);

			var attribute = property.GetCustomAttribute(typeof(TranslatableDataFieldAttribute)) as TranslatableDataFieldAttribute;
			AssertNotNull(attribute);

			AssertEquals(GlbCompanyCampaign.Schema.G0_CampaignName, attribute.ContextColumnName);
		}

		public void TestTranslatableDataFieldAttributeHasColumn_G0_CampaignComment()
		{
			var property = typeof(GlbCompanyCampaign).GetProperty(GlbCompanyCampaign.Schema.G0_CampaignComment);
			AssertNotNull(property);

			var attribute = property.GetCustomAttribute(typeof(TranslatableDataFieldAttribute)) as TranslatableDataFieldAttribute;
			AssertNotNull(attribute);

			AssertEquals(GlbCompanyCampaign.Schema.G0_CampaignComment, attribute.ContextColumnName);
		}

		public void TestImportChildInfoOnAttach()
		{
			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			var opportunity = Factory.NewWithValidTestData<OrgOpportunity>();
			Factory.Save();

			AssertEquals(ZGuid.Empty, opportunity.P8_G0);

			((IImportChildRelatedActivityInfoOnAttach)campaign).ImportChildInfo(opportunity, new ImportRelatedActivityNoDecisionFactory());
			AssertEquals(campaign.PK, opportunity.P8_G0);
		}

		public void TestImportChildInfoOnDetach()
		{
			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			var opportunity = Factory.NewWithValidTestData<OrgOpportunity>();
			opportunity.P8_G0 = campaign.PK;
			Factory.Save();

			AssertEquals(campaign.PK, opportunity.P8_G0);

			((IImportChildRelatedActivityInfoOnDetach)campaign).ImportChildInfo(opportunity, new ImportRelatedActivityNoDecisionFactory());
			AssertEquals(ZGuid.Empty, opportunity.P8_G0);
		}

		public void TestG0_CampaignID()
		{
			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			campaign.G0_CampaignID = "";

			var expected = Env.NumberFountains.GlbCompanyCampaignID.PeekPreliminaryFormatted(Factory);
			Factory.Save();

			AssertEquals("Campaign ID should come from the peek of the NumberFountain", expected, campaign.G0_CampaignID);
		}

		public void TestG0_EmailSenderOptionClearsSenderLocation()
		{
			var campaign = Factory.New<GlbCompanyCampaign>();
			campaign.G0_EmailSenderOption = "EML";
			campaign.G0_RL_NKEmailSenderUNLOCO = "AUSYD";
			AssertEquals("AUSYD", campaign.G0_RL_NKEmailSenderUNLOCO);

			campaign.G0_EmailSenderOption = "COR";
			AssertEquals("", campaign.G0_RL_NKEmailSenderUNLOCO);

			campaign.G0_RL_NKEmailSenderUNLOCO = "AUSYD";
			campaign.G0_EmailSenderOption = "SPS";
			AssertEquals("", campaign.G0_RL_NKEmailSenderUNLOCO);

			campaign.G0_RL_NKEmailSenderUNLOCO = "AUSYD";
			campaign.G0_EmailSenderOption = "ORG";
			AssertEquals("", campaign.G0_RL_NKEmailSenderUNLOCO);

			campaign.G0_RL_NKEmailSenderUNLOCO = "AUSYD";
			campaign.G0_EmailSenderOption = "EML";
			AssertEquals("AUSYD", campaign.G0_RL_NKEmailSenderUNLOCO);
		}

		public void TestG0_CampaignName_Translatable()
		{
			var bizO = Factory.New<GlbCompanyCampaign>();
			bizO.G0_CampaignName = "Boom";
			string resKey = bizO.G0_CampaignNameInfo.CustomizableDataResourceStrings.GetMultilingualString(bizO, "Boom").ResourceKey;
			AssertEquals("Boom", bizO.G0_CampaignNameMultilingual);
			using (Res.TemporarilySwitchLanguage(Core.SharedConstants.Languages.Russian))
			using (var mockRes = Res.UseMockData())
			{
				mockRes.Put(resKey, new ResourceStringData(resKey, "222"));
				AssertEquals("222", bizO.G0_CampaignNameMultilingual);
				AssertEquals("222", bizO.G0_CampaignNameLocalized);
			}
		}

		public void TestG0_CampaignComment_Translatable()
		{
			var bizO = Factory.New<GlbCompanyCampaign>();
			bizO.G0_CampaignComment = "Boom";
			string resKey = bizO.G0_CampaignCommentInfo.CustomizableDataResourceStrings.GetMultilingualString(bizO, "Boom").ResourceKey;
			AssertEquals("Boom", bizO.G0_CampaignCommentMultilingual);
			using (Res.TemporarilySwitchLanguage(Core.SharedConstants.Languages.Russian))
			using (var mockRes = Res.UseMockData())
			{
				mockRes.Put(resKey, new ResourceStringData(resKey, "222"));
				AssertEquals("222", bizO.G0_CampaignCommentMultilingual);
				AssertEquals("222", bizO.G0_CampaignCommentLocalized);
			}
		}

		#region Drip Marketing

		public void TestTransferDoesNotLoadItemsCollection()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "ABCD";

			var contact1 = org.Contacts.AddNew();
			contact1.OC_Email = "AA@gmail.com";
			contact1.OC_ContactName = "AA";

			var contact2 = org.Contacts.AddNew();
			contact2.OC_Email = "bb@gmail.com";
			contact2.OC_ContactName = "bb";

			var contact3 = org.Contacts.AddNew();
			contact3.OC_Email = "cc@gmail.com";
			contact3.OC_ContactName = "cc";

			var contact4 = org.Contacts.AddNew();
			contact4.OC_Email = "dd@gmail.com";
			contact4.OC_ContactName = "dd";

			GlbCompanyCampaign master = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			master.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.DripMarketing;
			master.G0_CampaignName = "master";

			var touch1 = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			touch1.G0_CampaignName = "Test Campaign";
			touch1.HtmlDocumentBlob = ZBlob.FromAscii("gday gday");
			touch1.G0_HorizontalId = 1;
			touch1.G0_VerticalId = "A";
			touch1.G0_G0_Master = master.PK;
			master.AllTouches.Add(touch1);

			var touch2a = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			touch2a.G0_CampaignName = "Test Campaign2a";
			touch2a.HtmlDocumentBlob = ZBlob.FromAscii("gday gday");
			touch2a.G0_HorizontalId = 2;
			touch2a.G0_VerticalId = "B";
			touch2a.G0_G0_Master = master.PK;
			master.AllTouches.Add(touch2a);

			var touch2b = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			touch2b.G0_CampaignName = "Test Campaign2b";
			touch2b.HtmlDocumentBlob = ZBlob.FromAscii("gday gday");
			touch2b.G0_HorizontalId = 2;
			touch2b.G0_VerticalId = "A";
			touch2b.G0_G0_Master = master.PK;
			master.AllTouches.Add(touch2b);

			var campaignItem1 = touch1.CampaignsItemsSent.AddNew();
			campaignItem1.G8_TrackingStatus = TrackingStatusCodes.Codes.VER;
			campaignItem1.G8_ScheduleTimeUtc = ZDateTime.UtcNow;
			campaignItem1.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem1.G8_RecipientID = contact1.PK;

			var campaignItem2 = touch2a.CampaignsItemsSent.AddNew();
			campaignItem2.G8_TrackingStatus = TrackingStatusCodes.Codes.QUE;
			campaignItem2.G8_ScheduleTimeUtc = ZDateTime.UtcNow;
			campaignItem2.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem2.G8_RecipientID = contact1.PK;

			//not to be loaded
			var campaignItem3 = touch1.CampaignsItemsSent.AddNew();
			campaignItem3.G8_TrackingStatus = TrackingStatusCodes.Codes.VER;
			campaignItem3.G8_ScheduleTimeUtc = ZDateTime.UtcNow;
			campaignItem3.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem3.G8_RecipientID = contact2.PK;

			var campaignItem4 = touch2a.CampaignsItemsSent.AddNew();
			campaignItem4.G8_TrackingStatus = TrackingStatusCodes.Codes.VER;
			campaignItem4.G8_ScheduleTimeUtc = ZDateTime.UtcNow;
			campaignItem4.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem4.G8_RecipientID = contact3.PK;

			var campaignItem5 = touch2b.CampaignsItemsSent.AddNew();
			campaignItem5.G8_TrackingStatus = TrackingStatusCodes.Codes.VER;
			campaignItem5.G8_ScheduleTimeUtc = ZDateTime.UtcNow;
			campaignItem5.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem5.G8_RecipientID = contact4.PK;

			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var newMaster = newFactory.Load<GlbCompanyCampaign>(master.PK);
			var newTouch1 = newFactory.Load<GlbCompanyCampaign>(touch1.PK);
			var newTouch2a = newFactory.Load<GlbCompanyCampaign>(touch2a.PK);
			var newTouch2b = newFactory.Load<GlbCompanyCampaign>(touch2b.PK);

			newTouch1.TransitionAndSchedule(new ZGuid[] { campaignItem1.PK });

			AssertEquals(false, ((IBusinessObjectFactoryInternals)newFactory).AllBusinessObjects.Any(x => x.PK == campaignItem3.PK));
			AssertEquals(false, ((IBusinessObjectFactoryInternals)newFactory).AllBusinessObjects.Any(x => x.PK == campaignItem4.PK));
			AssertEquals(false, ((IBusinessObjectFactoryInternals)newFactory).AllBusinessObjects.Any(x => x.PK == campaignItem5.PK));
		}

		public void TestDripMarketingModuleID()
		{
			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			AssertEquals(ExpectedModuleID, campaign.DripMarketingFilterRuleModule);
		}

		protected virtual ModuleIdentifier ExpectedModuleID
		{
			get
			{
				return ModuleIDs.DripMarketingFilterRule;
			}
		}

		public void TestDefibrillate()
		{
			SetupDripCampaign();

			var item1a1 = AddCampaignItem(touch1a, "UNV");
			var item1a2 = AddCampaignItem(touch1a, "VER");
			var item1a3 = AddCampaignItem(touch1a, "NDR");
			var item1a4 = AddCampaignItem(touch1a, "QUE");
			var item1a5 = AddCampaignItem(touch1a, "UNV");

			var item1b1 = AddCampaignItem(touch1b, "UNV");
			var item1b2 = AddCampaignItem(touch1b, "VER");
			var item1b3 = AddCampaignItem(touch1b, "NDR");
			var item1b4 = AddCampaignItem(touch1b, "QUE");
			var item1b5 = AddCampaignItem(touch1b, "VER");

			var contact1 = item1a1.G8_RecipientID;
			var contact2 = item1a2.G8_RecipientID;
			var contact3 = item1a3.G8_RecipientID;
			var contact4 = item1a4.G8_RecipientID;
			var contact5 = item1a5.G8_RecipientID;
			var contact6 = item1b1.G8_RecipientID;
			var contact7 = item1b2.G8_RecipientID;
			var contact8 = item1b3.G8_RecipientID;
			var contact9 = item1b4.G8_RecipientID;
			var contact10 = item1b5.G8_RecipientID;

			var item2b1 = AddCampaignItem(touch2b, "VER", contact1);
			var item2a1 = AddCampaignItem(touch2a, "UNV", contact6);

			Factory.Save();

			touch2a.DefibrillateTransitions();
			AssertEquals(false, item1a1.G8_IsCheckTransitionRequired);
			AssertEquals(true, item1a2.G8_IsCheckTransitionRequired);
			AssertEquals(false, item1a3.G8_IsCheckTransitionRequired);
			AssertEquals(false, item1a4.G8_IsCheckTransitionRequired);
			AssertEquals(true, item1a5.G8_IsCheckTransitionRequired);
			AssertEquals(false, item1b1.G8_IsCheckTransitionRequired);
			AssertEquals(false, item1b2.G8_IsCheckTransitionRequired);
			AssertEquals(false, item1b3.G8_IsCheckTransitionRequired);
			AssertEquals(false, item1b4.G8_IsCheckTransitionRequired);
			AssertEquals(false, item1b5.G8_IsCheckTransitionRequired);
			AssertEquals(false, item2b1.G8_IsCheckTransitionRequired);
			AssertEquals(false, item2a1.G8_IsCheckTransitionRequired);

			touch3a.DefibrillateTransitions();
			AssertEquals(false, item1a1.G8_IsCheckTransitionRequired);
			AssertEquals(true, item1a2.G8_IsCheckTransitionRequired);
			AssertEquals(false, item1a3.G8_IsCheckTransitionRequired);
			AssertEquals(false, item1a4.G8_IsCheckTransitionRequired);
			AssertEquals(true, item1a5.G8_IsCheckTransitionRequired);
			AssertEquals(false, item1b1.G8_IsCheckTransitionRequired);
			AssertEquals(true, item1b2.G8_IsCheckTransitionRequired);
			AssertEquals(false, item1b3.G8_IsCheckTransitionRequired);
			AssertEquals(false, item1b4.G8_IsCheckTransitionRequired);
			AssertEquals(true, item1b5.G8_IsCheckTransitionRequired);
			AssertEquals(true, item2b1.G8_IsCheckTransitionRequired);
			AssertEquals(false, item2a1.G8_IsCheckTransitionRequired);
		}

		public void TestPromoteToGroup()
		{
			SetupDripCampaign();

			var touch2c = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			touch2c.G0_HorizontalId = 2;
			touch2c.G0_VerticalId = "B";
			touch2c.G0_G0_Master = master.PK;
			master.AllTouches.Add(touch2c);
			touch2c.TransitionRulesToThisCampaign[0].GCD_G0_ParentTouch = ZGuid.Empty;
			touch2c.TransitionRulesToThisCampaign[0].GCD_ParentHorizontalId = 1;
			touch2c.TransitionRulesToThisCampaign.AddNew();
			touch2c.TransitionRulesToThisCampaign[1].GCD_G0_ParentTouch = touch1a.PK;
			touch2c.TransitionRulesToThisCampaign[1].GCD_ParentHorizontalId = 1;

			Factory.Save();

			touch2a.CurrentGroupColor = 1;
			touch2b.CurrentGroupColor = 2;
			touch2c.CurrentGroupColor = 1;

			Factory.Save();

			AssertEquals(1, touch2a.TransitionRulesToThisCampaign.Count);
			AssertEquals(2, touch2b.TransitionRulesToThisCampaign.Count);
			AssertEquals(1, touch2c.TransitionRulesToThisCampaign.Count);

			AssertEquals(touch2c.TransitionRulesToThisCampaign[0].PK, touch2a.TransitionRulesToThisCampaign[0].PK);
			AssertNotEquals(touch2b.TransitionRulesToThisCampaign[0].PK, touch2a.TransitionRulesToThisCampaign[0].PK);
			AssertNotEquals(touch2b.TransitionRulesToThisCampaign[1].PK, touch2a.TransitionRulesToThisCampaign[0].PK);
			AssertEquals(touch2a.SendSettings, touch2c.SendSettings);
			AssertNotEquals(touch2a.SendSettings, touch2b.SendSettings);

			AssertEquals(ZGuid.Empty, touch2a.TransitionRulesToThisCampaign[0].GCD_G0_NextTouch);
			AssertNotEquals(ZGuid.Empty, touch2a.TransitionRulesToThisCampaign[0].GCD_GCG_Group);

			AssertEquals(ZGuid.Empty, touch2b.TransitionRulesToThisCampaign[0].GCD_G0_NextTouch);
			AssertNotEquals(ZGuid.Empty, touch2b.TransitionRulesToThisCampaign[0].GCD_GCG_Group);

			AssertEquals(ZGuid.Empty, touch2c.TransitionRulesToThisCampaign[0].GCD_G0_NextTouch);
			AssertNotEquals(ZGuid.Empty, touch2c.TransitionRulesToThisCampaign[0].GCD_GCG_Group);

			AssertEquals(ZGuid.Empty, touch2a.SendSettings.GSC_G0_Campaign);
			AssertNotEquals(ZGuid.Empty, touch2a.SendSettings.GSC_GCG_Group);

			AssertEquals(ZGuid.Empty, touch2b.SendSettings.GSC_G0_Campaign);
			AssertNotEquals(ZGuid.Empty, touch2b.SendSettings.GSC_GCG_Group);

			AssertEquals(ZGuid.Empty, touch2c.SendSettings.GSC_G0_Campaign);
			AssertNotEquals(ZGuid.Empty, touch2c.SendSettings.GSC_GCG_Group);

			AssertEquals(touch2c.TransitionRulesToThisCampaign[0].GCD_GCG_Group, touch2a.TransitionRulesToThisCampaign[0].GCD_GCG_Group);
			AssertNotEquals(touch2b.TransitionRulesToThisCampaign[0].GCD_GCG_Group, touch2a.TransitionRulesToThisCampaign[0].GCD_GCG_Group);
		}

		public void TestChangeGroup()
		{
			SetupDripCampaign();

			var touch2c = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			touch2c.G0_HorizontalId = 2;
			touch2c.G0_VerticalId = "B";
			touch2c.G0_G0_Master = master.PK;
			master.AllTouches.Add(touch2c);
			touch2c.TransitionRulesToThisCampaign[0].GCD_G0_ParentTouch = ZGuid.Empty;
			touch2c.TransitionRulesToThisCampaign[0].GCD_ParentHorizontalId = 1;
			touch2c.TransitionRulesToThisCampaign.AddNew();
			touch2c.TransitionRulesToThisCampaign[1].GCD_G0_ParentTouch = touch1a.PK;
			touch2c.TransitionRulesToThisCampaign[1].GCD_ParentHorizontalId = 1;

			Factory.Save();

			touch2a.CurrentGroupColor = 1;
			touch2b.CurrentGroupColor = 2;
			touch2c.CurrentGroupColor = 1;

			Factory.Save();

			AssertEquals(1, touch2a.TransitionRulesToThisCampaign.Count);
			AssertEquals(2, touch2b.TransitionRulesToThisCampaign.Count);
			AssertEquals(1, touch2c.TransitionRulesToThisCampaign.Count);

			AssertEquals(touch2c.TransitionRulesToThisCampaign[0].PK, touch2a.TransitionRulesToThisCampaign[0].PK);
			AssertNotEquals(touch2b.TransitionRulesToThisCampaign[0].PK, touch2a.TransitionRulesToThisCampaign[0].PK);
			AssertNotEquals(touch2b.TransitionRulesToThisCampaign[1].PK, touch2a.TransitionRulesToThisCampaign[0].PK);
			AssertEquals(touch2a.SendSettings, touch2c.SendSettings);
			AssertNotEquals(touch2a.SendSettings, touch2b.SendSettings);

			touch2c.CurrentGroupColor = 2;

			Factory.Save();

			AssertEquals(1, touch2a.TransitionRulesToThisCampaign.Count);
			AssertEquals(2, touch2b.TransitionRulesToThisCampaign.Count);
			AssertEquals(2, touch2c.TransitionRulesToThisCampaign.Count);

			AssertNotEquals(touch2c.TransitionRulesToThisCampaign[0].PK, touch2a.TransitionRulesToThisCampaign[0].PK);
			AssertNotEquals(touch2b.TransitionRulesToThisCampaign[0].PK, touch2a.TransitionRulesToThisCampaign[0].PK);
			AssertNotEquals(touch2b.TransitionRulesToThisCampaign[1].PK, touch2a.TransitionRulesToThisCampaign[0].PK);
			AssertEquals(touch2b.TransitionRulesToThisCampaign[0].PK, touch2c.TransitionRulesToThisCampaign[0].PK);
			AssertEquals(touch2b.TransitionRulesToThisCampaign[1].PK, touch2c.TransitionRulesToThisCampaign[1].PK);
			AssertEquals(touch2b.SendSettings, touch2c.SendSettings);
			AssertNotEquals(touch2a.SendSettings, touch2b.SendSettings);
		}

		public void TestLeaveGroup()
		{
			SetupDripCampaign();

			var touch2c = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			touch2c.G0_HorizontalId = 2;
			touch2c.G0_VerticalId = "B";
			touch2c.G0_G0_Master = master.PK;
			master.AllTouches.Add(touch2c);
			touch2c.TransitionRulesToThisCampaign[0].GCD_G0_ParentTouch = ZGuid.Empty;
			touch2c.TransitionRulesToThisCampaign[0].GCD_ParentHorizontalId = 1;
			touch2c.TransitionRulesToThisCampaign.AddNew();
			touch2c.TransitionRulesToThisCampaign[1].GCD_G0_ParentTouch = touch1a.PK;
			touch2c.TransitionRulesToThisCampaign[1].GCD_ParentHorizontalId = 1;

			Factory.Save();

			touch2a.CurrentGroupColor = 1;
			touch2b.CurrentGroupColor = 2;
			touch2c.CurrentGroupColor = 1;

			Factory.Save();

			touch2c.CurrentGroupColor = 0;
			Factory.Save();

			AssertEquals(1, touch2a.TransitionRulesToThisCampaign.Count);
			AssertEquals(2, touch2b.TransitionRulesToThisCampaign.Count);
			AssertEquals(1, touch2c.TransitionRulesToThisCampaign.Count);

			AssertEquals(ZGuid.Empty, touch2a.TransitionRulesToThisCampaign[0].GCD_G0_NextTouch);
			AssertNotEquals(ZGuid.Empty, touch2a.TransitionRulesToThisCampaign[0].GCD_GCG_Group);

			AssertEquals(ZGuid.Empty, touch2b.TransitionRulesToThisCampaign[0].GCD_G0_NextTouch);
			AssertNotEquals(ZGuid.Empty, touch2b.TransitionRulesToThisCampaign[0].GCD_GCG_Group);

			AssertNotEquals(ZGuid.Empty, touch2c.TransitionRulesToThisCampaign[0].GCD_G0_NextTouch);
			AssertEquals(ZGuid.Empty, touch2c.TransitionRulesToThisCampaign[0].GCD_GCG_Group);

			AssertEquals(ZGuid.Empty, touch2a.SendSettings.GSC_G0_Campaign);
			AssertNotEquals(ZGuid.Empty, touch2a.SendSettings.GSC_GCG_Group);

			AssertEquals(ZGuid.Empty, touch2b.SendSettings.GSC_G0_Campaign);
			AssertNotEquals(ZGuid.Empty, touch2b.SendSettings.GSC_GCG_Group);

			AssertNotEquals(ZGuid.Empty, touch2c.SendSettings.GSC_G0_Campaign);
			AssertEquals(ZGuid.Empty, touch2c.SendSettings.GSC_GCG_Group);
		}

		public void TestGroupDistribution()
		{
			SetupDripCampaign();

			var touch2c = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			touch2c.G0_HorizontalId = 2;
			touch2c.G0_VerticalId = "C";
			touch2c.G0_G0_Master = master.PK;
			master.AllTouches.Add(touch2c);
			touch2c.TransitionRulesToThisCampaign[0].GCD_G0_ParentTouch = ZGuid.Empty;
			touch2c.TransitionRulesToThisCampaign[0].GCD_ParentHorizontalId = 1;
			touch2c.TransitionRulesToThisCampaign.AddNew();
			touch2c.TransitionRulesToThisCampaign[1].GCD_G0_ParentTouch = touch1a.PK;
			touch2c.TransitionRulesToThisCampaign[1].GCD_ParentHorizontalId = 1;

			GlbCompanyCampaignTestHelper.PopulateCampaign(touch2c, Factory);

			Factory.Save();

			touch2a.CurrentGroupColor = 1;
			touch2b.CurrentGroupColor = 2;
			touch2c.CurrentGroupColor = 1;
			touch2a.G0_GroupRatio = 75;
			touch2c.G0_GroupRatio = 25;
			touch2b.G0_GroupRatio = 100;

			Factory.Save();

			var item1a1 = AddCampaignItem(touch1a, "UNV");
			var item1a2 = AddCampaignItem(touch1a, "UNV");
			var item1a3 = AddCampaignItem(touch1a, "UNV");
			var item1a4 = AddCampaignItem(touch1a, "UNV");

			var item1b1 = AddCampaignItem(touch1b, "UNV");
			var item1b2 = AddCampaignItem(touch1b, "UNV");
			var item1b3 = AddCampaignItem(touch1b, "UNV");
			var item1b4 = AddCampaignItem(touch1b, "UNV");

			Factory.Save();

			touch1a.TransitionAndSchedule();

			touch1b = new BusinessObjectFactory() { RefreshEnabled = false }.Load<GlbCompanyCampaign>(touch1b.PK);

			touch1b.TransitionAndSchedule();

			AssertEquals(3, touch2a.CampaignsItemsSent.Count);
			AssertEquals(1, touch2c.CampaignsItemsSent.Count);

			int count2b = new BusinessObjectFactory() { RefreshEnabled = false }.Load<GlbCompanyCampaign>(touch2b.PK).CampaignsItemsSent.Count;
			AssertEquals(4, count2b);
		}

		public void TestNextTouches()
		{
			var master = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			master.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.DripMarketing;

			var touch1a = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			touch1a.G0_HorizontalId = 1;
			touch1a.G0_VerticalId = "A";
			master.AllTouches.Add(touch1a);

			var touch1b = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			touch1b.G0_HorizontalId = 1;
			touch1b.G0_VerticalId = "B";
			master.AllTouches.Add(touch1b);

			var touch1c = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			touch1c.G0_HorizontalId = 1;
			touch1c.G0_VerticalId = "C";
			master.AllTouches.Add(touch1c);

			var touch2a = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			touch2a.G0_HorizontalId = 2;
			touch2a.G0_VerticalId = "A";
			master.AllTouches.Add(touch2a);
			touch2a.TransitionRulesToThisCampaign[0].GCD_G0_ParentTouch = ZGuid.Empty;
			touch2a.TransitionRulesToThisCampaign[0].GCD_ParentHorizontalId = 1;
			touch2a.CurrentGroupColor = 1;

			var touch2b = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			touch2b.G0_HorizontalId = 2;
			touch2b.G0_VerticalId = "A";
			master.AllTouches.Add(touch2b);
			touch2b.CurrentGroupColor = 1;

			var touch2c = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			touch2c.G0_HorizontalId = 2;
			touch2c.G0_VerticalId = "A";
			master.AllTouches.Add(touch2c);
			touch2c.TransitionRulesToThisCampaign[0].GCD_G0_ParentTouch = touch1c.PK;
			touch2c.TransitionRulesToThisCampaign[0].GCD_ParentHorizontalId = 1;
			touch2c.CurrentGroupColor = 2;

			var touch2d = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			touch2d.G0_HorizontalId = 2;
			touch2d.G0_VerticalId = "A";
			master.AllTouches.Add(touch2d);
			touch2d.TransitionRulesToThisCampaign[0].GCD_G0_ParentTouch = touch1c.PK;
			touch2d.TransitionRulesToThisCampaign[0].GCD_ParentHorizontalId = 1;

			var touch3a = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			touch3a.G0_HorizontalId = 3;
			touch3a.G0_VerticalId = "A";
			master.AllTouches.Add(touch3a);
			touch3a.TransitionRulesToThisCampaign[0].GCD_G0_ParentTouch = ZGuid.Empty;
			touch3a.TransitionRulesToThisCampaign[0].GCD_ParentHorizontalId = 2;
			touch3a.CurrentGroupColor = 1;

			var touch3b = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			touch3b.G0_HorizontalId = 3;
			touch3b.G0_VerticalId = "B";
			master.AllTouches.Add(touch3b);
			touch3b.CurrentGroupColor = 1;

			var touch3c = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			touch3c.G0_HorizontalId = 3;
			touch3c.G0_VerticalId = "C";
			master.AllTouches.Add(touch3c);
			touch3c.TransitionRulesToThisCampaign[0].GCD_G0_ParentTouch = touch2c.PK;
			touch3c.TransitionRulesToThisCampaign[0].GCD_ParentHorizontalId = 2;
			touch3c.CurrentGroupColor = 2;

			var touch3d = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			touch3d.G0_HorizontalId = 3;
			touch3d.G0_VerticalId = "C";
			master.AllTouches.Add(touch3d);
			touch3d.TransitionRulesToThisCampaign[0].GCD_G0_ParentTouch = touch2c.PK;
			touch3d.TransitionRulesToThisCampaign[0].GCD_ParentHorizontalId = 2;

			GlbCompanyCampaignTestHelper.PopulateCampaign(master, Factory);
			GlbCompanyCampaignTestHelper.PopulateCampaign(touch1a, Factory);
			GlbCompanyCampaignTestHelper.PopulateCampaign(touch1b, Factory);
			GlbCompanyCampaignTestHelper.PopulateCampaign(touch1c, Factory);
			GlbCompanyCampaignTestHelper.PopulateCampaign(touch2a, Factory);
			GlbCompanyCampaignTestHelper.PopulateCampaign(touch2b, Factory);
			GlbCompanyCampaignTestHelper.PopulateCampaign(touch2c, Factory);
			GlbCompanyCampaignTestHelper.PopulateCampaign(touch2d, Factory);
			GlbCompanyCampaignTestHelper.PopulateCampaign(touch3a, Factory);
			GlbCompanyCampaignTestHelper.PopulateCampaign(touch3b, Factory);
			GlbCompanyCampaignTestHelper.PopulateCampaign(touch3c, Factory);
			GlbCompanyCampaignTestHelper.PopulateCampaign(touch3d, Factory);

			Factory.Save();

			AssertContainsExactElementsInAnyOrder(new[] { touch2a.PK, touch2b.PK }, touch1a.NextTouches.Select(t => t.PK));
			AssertContainsExactElementsInAnyOrder(new[] { touch2a.PK, touch2b.PK }, touch1b.NextTouches.Select(t => t.PK));
			AssertContainsExactElementsInAnyOrder(new[] { touch2a.PK, touch2b.PK, touch2c.PK, touch2d.PK }, touch1c.NextTouches.Select(t => t.PK));
			AssertContainsExactElementsInAnyOrder(new[] { touch3a.PK, touch3b.PK }, touch2a.NextTouches.Select(t => t.PK));
			AssertContainsExactElementsInAnyOrder(new[] { touch3a.PK, touch3b.PK }, touch2b.NextTouches.Select(t => t.PK));
			AssertContainsExactElementsInAnyOrder(new[] { touch3a.PK, touch3b.PK, touch3c.PK, touch3d.PK }, touch2c.NextTouches.Select(t => t.PK));
			AssertContainsExactElementsInAnyOrder(new[] { touch3a.PK, touch3b.PK }, touch2d.NextTouches.Select(t => t.PK));
		}

		public void TestCheckTransitionWhenRulesChange()
		{
			SetupDripCampaign();

			var item1a = AddCampaignItem(touch1a, "UNV");
			var item1b1 = AddCampaignItem(touch1b, "VER");
			var item1b2 = AddCampaignItem(touch1b, "QUE");

			var item2a1 = AddCampaignItem(touch2a, "UNV");
			var item2a2 = AddCampaignItem(touch2a, "QUE");
			var item2a3 = AddCampaignItem(touch2a, "VER");

			var item2b1 = AddCampaignItem(touch2b, "UNV");
			var item2b2 = AddCampaignItem(touch2b, "QUE");
			var item2b3 = AddCampaignItem(touch2b, "VER");

			var item3a1 = AddCampaignItem(touch3a, "UNV");
			var item3a2 = AddCampaignItem(touch3a, "QUE");
			var item3a3 = AddCampaignItem(touch3a, "VER");

			Factory.Save();

			AssertEquals(false, item1a.G8_IsCheckTransitionRequired);
			AssertEquals(false, item1b1.G8_IsCheckTransitionRequired);
			AssertEquals(false, item1b2.G8_IsCheckTransitionRequired);
			AssertEquals(false, item2a1.G8_IsCheckTransitionRequired);
			AssertEquals(false, item2a2.G8_IsCheckTransitionRequired);
			AssertEquals(false, item2a3.G8_IsCheckTransitionRequired);
			AssertEquals(false, item2b1.G8_IsCheckTransitionRequired);
			AssertEquals(false, item2b2.G8_IsCheckTransitionRequired);
			AssertEquals(false, item2b3.G8_IsCheckTransitionRequired);
			AssertEquals(false, item3a1.G8_IsCheckTransitionRequired);
			AssertEquals(false, item3a2.G8_IsCheckTransitionRequired);
			AssertEquals(false, item3a3.G8_IsCheckTransitionRequired);

			touch2b.TransitionRulesToThisCampaign[0].HasChanges = true;
			Factory.Save();

			AssertEquals(true, item1a.G8_IsCheckTransitionRequired);
			AssertEquals(true, item1b1.G8_IsCheckTransitionRequired);
			AssertEquals(false, item1b2.G8_IsCheckTransitionRequired);
			AssertEquals(false, item2a1.G8_IsCheckTransitionRequired);
			AssertEquals(true, item2a2.G8_IsCheckTransitionRequired);
			AssertEquals(false, item2a3.G8_IsCheckTransitionRequired);
			AssertEquals(false, item2b1.G8_IsCheckTransitionRequired);
			AssertEquals(true, item2b2.G8_IsCheckTransitionRequired);
			AssertEquals(false, item2b3.G8_IsCheckTransitionRequired);
			AssertEquals(false, item3a1.G8_IsCheckTransitionRequired);
			AssertEquals(true, item3a2.G8_IsCheckTransitionRequired);
			AssertEquals(false, item3a3.G8_IsCheckTransitionRequired);
		}

		GlbCompanyCampaignItem AddCampaignItem(GlbCompanyCampaign campaign, string status)
		{
			return AddCampaignItem(campaign, status, ZGuid.Empty);
		}

		GlbCompanyCampaignItem AddCampaignItem(GlbCompanyCampaign campaign, string status, ZGuid contactPK)
		{
			if (contactPK == ZGuid.Empty)
			{
				var org = Factory.NewWithValidTestData<OrgHeader>();
				var contact = org.Contacts.AddNew();
				contact.OC_Email = ZGuid.NewZGuid().ToGuid().ToString("N") + "@example.com";
				contactPK = contact.PK;
			}

			var item = Factory.NewWithValidTestData<GlbCompanyCampaignItem>();
			item.G8_G0 = campaign.PK;
			item.G8_TrackingStatus = status;
			item.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			item.G8_RecipientID = contactPK;

			return item;
		}

		public void TestSendSettings()
		{
			var master = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			master.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.DripMarketing;

			AssertNull(master.SendSettings);

			var touch = master.AllTouches.AddNew();
			AssertNotNull(touch.SendSettings);
			Assert(touch.IsRegisteredEditableChildObject(touch.SendSettings));

			var normalCampaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			AssertNull(normalCampaign.SendSettings);
		}

		public void TestAddTouches()
		{
			var master = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			master.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.DripMarketing;

			var touch1 = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			touch1.G0_HorizontalId = 1;
			touch1.G0_VerticalId = "A";
			master.AllTouches.Add(touch1);

			AssertNotNull(touch1.TransitionRulesToThisCampaign);

			AssertEquals(1, touch1.TransitionRulesToThisCampaign.Count);
			AssertEquals(master.PK, touch1.G0_G0_Master);
			AssertEquals(master, touch1.TransitionRulesToThisCampaign[0].ParentTouch);
			AssertEquals(master.PK, touch1.MasterCampaign.PK);
			AssertEquals(touch1.PK, touch1.TransitionRulesToThisCampaign[0].NextTouches[0].PK);
			AssertEquals(master.PK, touch1.G0_G0_Master);
			AssertEquals(master.PK, touch1.TransitionRulesToThisCampaign[0].GCD_G0_ParentTouch);
			AssertEquals(touch1.PK, touch1.TransitionRulesToThisCampaign[0].GCD_G0_NextTouch);
			AssertEquals((byte)0, touch1.TransitionRulesToThisCampaign[0].GCD_ParentHorizontalId);

			var touch2 = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			touch2.G0_HorizontalId = 2;
			touch2.G0_VerticalId = "A";
			master.AllTouches.Add(touch2);

			AssertNotNull(touch2.TransitionRulesToThisCampaign);
			AssertEquals(1, touch2.TransitionRulesToThisCampaign.Count);
			AssertEquals(master.PK, touch2.G0_G0_Master);
			AssertNotNull(touch2.TransitionRulesToThisCampaign[0].ParentTouch);
			AssertEquals(master.PK, touch2.MasterCampaign.PK);
			AssertEquals(touch2.PK, touch2.TransitionRulesToThisCampaign[0].NextTouches[0].PK);
			AssertEquals(master.PK, touch2.G0_G0_Master);
			AssertEquals(touch1.PK, touch2.TransitionRulesToThisCampaign[0].GCD_G0_ParentTouch);
			AssertEquals(touch2.PK, touch2.TransitionRulesToThisCampaign[0].GCD_G0_NextTouch);
			AssertEquals((byte)1, touch2.TransitionRulesToThisCampaign[0].GCD_ParentHorizontalId);
		}

		public void TestMasterCampaign()
		{
			var master = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			master.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.DripMarketing;

			var touch = master.AllTouches.AddNew();
			AssertEquals(master, touch.MasterCampaign);
		}

		public void TestIsMaster()
		{
			var master = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			AssertEquals(false, master.IsMasterCampaign);

			master.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.DripMarketing;
			AssertEquals(true, master.IsMasterCampaign);

			master.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.Broadcast;
			AssertEquals(false, master.IsMasterCampaign);

			master.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.InsideSales;
			AssertEquals(true, master.IsMasterCampaign);
		}

		public void TestTouchId()
		{
			var touch1a = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			touch1a.G0_CampaignName = "name";
			AssertEquals("", touch1a.TouchId);

			touch1a.G0_HorizontalId = 1;
			AssertEquals("1", touch1a.TouchId);

			touch1a.G0_VerticalId = "A";
			AssertEquals("1A", touch1a.TouchId);
		}

		public void TestTouchFullName()
		{
			var touch1a = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			touch1a.G0_CampaignName = "name";

			AssertEquals("Touch [] - name", touch1a.TouchFullName);

			touch1a.G0_HorizontalId = 1;
			AssertEquals("Touch [1] - name", touch1a.TouchFullName);

			touch1a.G0_VerticalId = "A";
			AssertEquals("Touch [1A] - name", touch1a.TouchFullName);
		}

		public void TestHorizontals()
		{
			SetupDripCampaign();

			var horizontals = master.Horizontals.ToArray();

			AssertEquals(3, horizontals.Length);

			AssertEquals((ZByte)1, horizontals[0].Id);
			AssertEquals((ZByte)2, horizontals[1].Id);
			AssertEquals((ZByte)3, horizontals[2].Id);

			AssertContainsExactElementsInAnyOrder(new[] { touch1a, touch1b }, horizontals[0].Campaigns);
			AssertContainsExactElementsInAnyOrder(new[] { touch2a, touch2b }, horizontals[1].Campaigns);
			AssertContainsExactElementsInAnyOrder(new[] { touch3a }, horizontals[2].Campaigns);
		}

		public void TestG0_HorizontalId()
		{
			SetupDripCampaign();
			touch3a.TransitionRulesToThisCampaign[1].GCD_G0_ParentTouch = touch1a.PK;
			touch3a.TransitionRulesToThisCampaign[1].GCD_ParentHorizontalId = 1;

			Factory.Save();

			touch1a.G0_HorizontalId = 2;

			AssertEquals((ZByte)1, touch3a.TransitionRulesToThisCampaign[0].GCD_ParentHorizontalId);
			AssertEquals((ZByte)2, touch3a.TransitionRulesToThisCampaign[1].GCD_ParentHorizontalId);
			AssertEquals(touch1a.PK, touch3a.TransitionRulesToThisCampaign[1].GCD_G0_ParentTouch);
		}

		public void TestRulesFromThisCampaign()
		{
			SetupDripCampaign();

			Factory.Save();

			AssertEquals(2, master.TransitionRulesFromThisCampaign.Count);
			AssertEquals(4, touch1a.TransitionRulesFromThisCampaign.Count);
			AssertEquals(2, touch1b.TransitionRulesFromThisCampaign.Count);
			AssertEquals(0, touch2a.TransitionRulesFromThisCampaign.Count);
			AssertEquals(1, touch2b.TransitionRulesFromThisCampaign.Count);
			AssertEquals(0, touch3a.TransitionRulesFromThisCampaign.Count);
		}

		public void TestRulesToThisCampaign()
		{
			SetupDripCampaign();

			AssertEquals(0, master.TransitionRulesToThisCampaign.Count);
			AssertEquals(1, touch1a.TransitionRulesToThisCampaign.Count);
			AssertEquals(1, touch1b.TransitionRulesToThisCampaign.Count);
			AssertEquals(1, touch2a.TransitionRulesToThisCampaign.Count);
			AssertEquals(2, touch2b.TransitionRulesToThisCampaign.Count);
			AssertEquals(2, touch3a.TransitionRulesToThisCampaign.Count);
		}

		public void TestAddHorizontal()
		{
			SetupDripCampaign();

			AssertEquals(3, master.Horizontals.Count());

			master.AddHorizontal();
			AssertEquals(4, master.Horizontals.Count());
			AssertEquals((byte)4, master.Horizontals.Last().Id);
			AssertEquals(0, master.Horizontals.Last().Campaigns.Count);
		}

		public void TestSummaryStats()
		{
			SetupDripCampaign();

			var deliverySummaryItems = new List<CampaignDeliverySummaryItem>();
			var transitionResults = new List<CampaignTransitionResults>();

			deliverySummaryItems.Add(new CampaignDeliverySummaryItem()
			{
				CampaignId = touch1a.PK,
				TrackingStatus = "QUE",
				StatusCount = 5
			});

			deliverySummaryItems.Add(new CampaignDeliverySummaryItem()
			{
				CampaignId = touch2a.PK,
				TrackingStatus = "VER",
				StatusCount = 3
			});

			master.summaryStats = new CampaignSummaryStatsForTest(deliverySummaryItems, transitionResults, master.Horizontals.Max(h => h.Id));

			AssertEquals(5, touch1a.SummaryStats.QueuedCount);
			AssertEquals(0, touch1a.SummaryStats.SentCount);

			AssertEquals(0, touch2a.SummaryStats.QueuedCount);
			AssertEquals(3, touch2a.SummaryStats.SentCount);

			AssertEquals(0, touch3a.SummaryStats.QueuedCount);
			AssertEquals(0, touch3a.SummaryStats.SentCount);

			var campaignItem1 = Factory.NewWithValidTestData<GlbCompanyCampaignItem>();
			var campaignItem2 = Factory.NewWithValidTestData<GlbCompanyCampaignItem>();
			var campaignItem3 = Factory.NewWithValidTestData<GlbCompanyCampaignItem>();

			campaignItem1.G8_TrackingStatus = "NDR";
			campaignItem1.G8_G0 = touch1a.PK;

			campaignItem2.G8_TrackingStatus = "QUE";
			campaignItem2.G8_G0 = touch2a.PK;

			campaignItem3.G8_TrackingStatus = "UNV";
			campaignItem3.G8_G0 = touch3a.PK;

			master.SummaryStats.AddCampaignItem(campaignItem1);
			master.SummaryStats.AddCampaignItem(campaignItem2);
			master.SummaryStats.AddCampaignItem(campaignItem3);

			AssertEquals(5, touch1a.SummaryStats.QueuedCount);
			AssertEquals(1, touch1a.SummaryStats.SentCount);

			AssertEquals(1, touch2a.SummaryStats.QueuedCount);
			AssertEquals(3, touch2a.SummaryStats.SentCount);

			AssertEquals(0, touch3a.SummaryStats.QueuedCount);
			AssertEquals(1, touch3a.SummaryStats.SentCount);
		}

		public void TestTransitionAndSchedule_WithTouchGroup()
		{
			var helper = new GlbCompanyCampaignTestHelper(Factory);
			helper.SetupDripCampaign();
			helper.Touch1A.CampaignsItemsSent.DeleteAll();
			helper.Touch1B.CampaignsItemsSent.DeleteAll();

			helper.Touch1A.CurrentGroupColor = 10;
			helper.Touch1B.CurrentGroupColor = 10;

			Factory.Save();

			helper.Touch1A.SendSettings.IsBatchSchedule = true;
			helper.Touch1A.SendSettings.GSC_ContactLimitEachBatch = 30;
			helper.Touch1A.SendSettings.IsContactLimitEachBatchUsed = true;

			var org = Factory.NewWithValidTestData<OrgHeader>();
			TestTransitionAndSchedule(helper, org, 3, 47);

			AssertEquals(25, helper.Touch1A.CampaignsItemsSent.Count);
			AssertEquals(25, helper.Touch1B.CampaignsItemsSent.Count);
		}

		public void TestTransitionAndSchedule_RelatedPartiesFilter()
		{
			var helper = new GlbCompanyCampaignTestHelper(Factory);
			helper.SetupDripCampaign(true);

			var otherOrg = Factory.NewWithValidTestData<OrgHeader>();
			otherOrg.OH_Code = "OTHORG";

			var localContact1 = otherOrg.Contacts.AddNew();
			localContact1.OC_Email = "bob@test.com";
			localContact1.OC_ContactName = "Bob";

			var localContact2 = otherOrg.Contacts.AddNew();
			localContact2.OC_Email = "bill@test.com";
			localContact2.OC_ContactName = "Bill";

			var localContact3 = otherOrg.Contacts.AddNew();
			localContact3.OC_Email = "steve@test.com";
			localContact3.OC_ContactName = "Steve";

			var item1 = helper.Master.CampaignsItemsSent.AddNew();
			item1.G8_TrackingStatus = "UNV";
			item1.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			item1.G8_RecipientID = localContact1.PK;

			var item2 = helper.Master.CampaignsItemsSent.AddNew();
			item2.G8_TrackingStatus = "UNV";
			item2.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			item2.G8_RecipientID = localContact2.PK;

			var item3 = helper.Master.CampaignsItemsSent.AddNew();
			item3.G8_TrackingStatus = "UNV";
			item3.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			item3.G8_RecipientID = localContact3.PK;

			helper.Master.AllTouches.Delete(helper.Touch2B);
			helper.Master.AllTouches.Delete(helper.Touch3A);

			helper.Touch1A.CampaignsItemsSent.DeleteAll();
			helper.Touch1B.CampaignsItemsSent.DeleteAll();
			helper.Touch2A.CampaignsItemsSent.DeleteAll();

			helper.Touch1A.CurrentGroupColor = 10;
			helper.Touch1B.CurrentGroupColor = 10;
			helper.Touch2A.CurrentGroupColor = 14;

			var relatedOrg = Factory.NewWithValidTestData<OrgHeader>();
			relatedOrg.OH_Code = "TSTORG";

			helper.Org.AddRelatedParty(relatedOrg.PK, "WRP", ZString.Empty, ZString.Empty, ZString.Empty, GlbCompany.CurrentCompany);

			Factory.Save();

			var groupPK = Factory.Load<GlbCompanyCampaignGroup>(new ZQuery(GlbCompanyCampaignGroupSchema.GCG_GroupColor, helper.Touch1A.CurrentGroupColor)).First().PK;
			var dripMarketingQuery = new ZQuery(GlbCompanyCampaignDripMarketingSchema.GCD_G0_ParentTouch, helper.Master.PK);
			dripMarketingQuery.AddToFilter(new ZQuery(GlbCompanyCampaignDripMarketingSchema.GCD_GCG_Group, groupPK), JoinCondition.And);
			var dripMarketingBizOPK = Factory.Load<GlbCompanyCampaignDripMarketing>(dripMarketingQuery).First().PK;
			var filter = Factory.Load<StmModuleFilter>(new ZQuery(StmModuleFilterSchema.S9_ParentID, dripMarketingBizOPK)).First();

			var userDataPK = ZGuid.NewZGuid();
			var sql = @"UPDATE dbo.StmModuleFilter SET S9_FilterData = dbo.CLRCompressStringAsBytes('{0}') WHERE S9_PK = '{1}'

							INSERT INTO dbo.StmModuleFilterUserData(S0_PK, S0_FilterDataValues, S0_S9, S0_RelatedEntityID) VALUES ('{2}', dbo.CLRCompressStringAsBytes('{3}'), '{1}', '{4}')";

			using (var command = Db.Connection.Command(string.Format(sql, FilterLayoutXMLForCampaign, filter.PK, userDataPK, string.Format(FilterLayoutValuesXMLForInquiryCampaign, relatedOrg.PK), ZGuid.Empty)))
			{
				command.ExecuteNonQuery();
			}

			filter.Reload();

			helper.Master.TransitionAndSchedule();

			AssertEquals(1, helper.Touch1A.CampaignsItemsSent.Count);
			AssertEquals(2, helper.Touch1B.CampaignsItemsSent.Count);
			AssertEquals(3, helper.Touch2A.CampaignsItemsSent.Count);
		}

		const string FilterLayoutXMLForCampaign = @"<FilterLayoutSerializer>
  <FilterStrips>
    <FilterStrip>
      <FilterDescription>SubscriptionStatus</FilterDescription>
      <OrCategory>None</OrCategory>
      <GroupOrCategory>None</GroupOrCategory>
      <GroupName />
      <AdditionalColourName />
      <AdditionalGroupColourName />
      <FilterPropertyLockStatus>False</FilterPropertyLockStatus>
    </FilterStrip>
    <FilterStrip>
      <FilterDescription>Related Parties_55a594ef-d39f-4d99-81ae-e575ad4b13c4</FilterDescription>
      <OrCategory>None</OrCategory>
      <GroupOrCategory>None</GroupOrCategory>
      <GroupName />
      <AdditionalColourName />
      <AdditionalGroupColourName />
      <FilterPropertyLockStatus>False</FilterPropertyLockStatus>
    </FilterStrip>
  </FilterStrips>
</FilterLayoutSerializer>";

		const string FilterLayoutValuesXMLForInquiryCampaign = @"<FilterLayoutValuesSerializer>
  <ModuleFilters>
    <ModuleFilter>
      <Comparer>not equal</Comparer>
      <Property>UNS</Property>
    </ModuleFilter>
    <ModuleFilter>
      <Comparer>starts with</Comparer>
      <Property />
      <PartyType>WRP</PartyType>
      <Direction />
      <TransportMode />
      <ContainerMode />
      <RelatedParty>{0}</RelatedParty>
    </ModuleFilter>
  </ModuleFilters>
</FilterLayoutValuesSerializer>";

		public void TestTransitionAndSchedule_WhenManyContacts()
		{
			var helper = new GlbCompanyCampaignTestHelper(Factory);
			helper.SetupDripCampaign();
			helper.Touch1A.CampaignsItemsSent.DeleteAll();
			helper.Touch1B.CampaignsItemsSent.DeleteAll();
			var org = Factory.NewWithValidTestData<OrgHeader>();

			TestTransitionAndSchedule(helper, org, 3, 150);
			TestTransitionAndSchedule(helper, org, 153, 100);
		}

		public void TestTransitionAndSchedule_WithInactiveContacts()
		{
			var helper = new GlbCompanyCampaignTestHelper(Factory);
			helper.SetupDripCampaign();
			helper.Touch1A.CampaignsItemsSent.DeleteAll();
			helper.Touch1B.CampaignsItemsSent.DeleteAll();
			var org = Factory.NewWithValidTestData<OrgHeader>();

			TestTransitionAndSchedule(helper, org, 3, 5, false);
		}

		[TestDate(2019, 2, 25)]
		public void TestTransitionAndSchedule_Logging()
		{
			var errorList = new List<String>();

			var helper = new GlbCompanyCampaignTestHelper(Factory);
			helper.SetupDripCampaign();
			helper.Master.G0_CampaignID = "TST00001000";
			helper.Touch1A.CampaignsItemsSent.DeleteAll();
			helper.Touch1B.CampaignsItemsSent.DeleteAll();

			helper.Touch1A.SendSettings.GSC_ScheduleType = GlbCompanyCampaignSendSettingsLookups.Codes.BAT;
			helper.Touch1A.SendSettings.GSC_ContactLimitEachBatch = 0;
			AssertEquals("Pre-condition", true, helper.Touch1A.SendSettings.GSC_ContactLimitEachBatchInfo.HasErrors());

			var org = Factory.NewWithValidTestData<OrgHeader>();
			var contacts = Enumerable.Range(1, 3).Select(i =>
			{
				var contact = org.Contacts.AddNew();
				contact.OC_ContactName = $"contact{i:D4}";
				contact.OC_Email = $"{contact.OC_ContactName}@ema.il";
				return contact;
			}).ToArray();

			helper.Factory.Save();

			var campaignContactCollection = Testing.GlbCompanyCampaignSenderTest.ContactCollection(new List<BusinessObject>(contacts), helper.Master);
			var campaignSender = new GlbCompanyCampaignSender(helper.Master, campaignContactCollection.Cast<CampaignContact>().ToList());
			campaignSender.ShouldContinueWithSending += (num, campaignItem) => true;
			helper.Master.TransitionAndSchedule(errorList);

			var campaignItems = helper.Master.CampaignsItemsSent;

			helper.Touch1A.TransitionAndSchedule(new ZGuid[] { campaignItems[0].PK }, errorList);
			helper.Touch1B.TransitionAndSchedule(new ZGuid[] { campaignItems[1].PK }, errorList);

			AssertEquals("Should have error", 1, errorList.Count);

			var errorMessage = errorList.First();
			AssertEquals("Message should contain campaign ID", $@"190225_***_***_Touch 1A_1A: Cannot Send Campaigns All errors on this campaign must be corrected before campaigns can be sent.
- Please set at least one batch limit or change your Schedule Type.
This touch campaign can be accessed through its master campaign with ID {helper.Master.G0_CampaignID}.", errorMessage);
		}

		static void TestTransitionAndSchedule(GlbCompanyCampaignTestHelper helper, OrgHeader org, int initialContactCount, int contactCount, bool isActive = true)
		{
			var contacts = Enumerable.Range(initialContactCount, contactCount).Select(i =>
			{
				var contact = org.Contacts.AddNew();
				contact.OC_ContactName = $"contact{i:D4}";
				contact.OC_Email = $"{contact.OC_ContactName}@ema.il";
				contact.OC_IsActive = isActive;
				return contact;
			}).ToArray();
			AssertEquals("Created contacts count", contactCount, contacts.Length);
			helper.Factory.Save();

			var campaignContactCollection = Testing.GlbCompanyCampaignSenderTest.ContactCollection(new List<BusinessObject>(contacts), helper.Master);
			var campaignSender = new GlbCompanyCampaignSender(helper.Master, campaignContactCollection.Cast<CampaignContact>().ToList());
			campaignSender.ShouldContinueWithSending += (num, campaignItem) => true;
			AssertEquals("Should send correctly", isActive, campaignSender.CheckAndSendCampaigns());

			var totalSent = initialContactCount + contacts.Length;
			if (isActive)
			{
				AssertEquals("Sent count", totalSent, helper.Master.CampaignsItemsSent.Count);
			}
			else
			{
				AssertEquals("Sent count", initialContactCount, helper.Master.CampaignsItemsSent.Count);
			}

			helper.Master.TransitionAndSchedule();

			if (isActive)
			{
				AssertEquals("Transition count", totalSent, helper.Touch1A.CampaignsItemsSent.Count + helper.Touch1B.CampaignsItemsSent.Count);
			}
			else
			{
				AssertEquals("Transition count", initialContactCount, helper.Touch1A.CampaignsItemsSent.Count + helper.Touch1B.CampaignsItemsSent.Count);
			}
		}

		#region Huge transition count stress test

		[SnailTest]
		[StressTest]
		public void TestHugeTransition()
		{
			const int contactsCount = 33000;
			var masterCampaignPk = PrepareHugeData(contactsCount);

			var masterCampaign = new BusinessObjectFactory().Load<GlbCompanyCampaign>(masterCampaignPk);
			var list = masterCampaign.GetTransfers(Array.Empty<ZGuid>());
			AssertEquals("Transition count", contactsCount, list.Count);
		}

		ZGuid PrepareHugeData(int contactsCount)
		{
			var helper = new GlbCompanyCampaignTestHelper(Factory);
			helper.SetupDripCampaign();
			helper.Master.CampaignsItemsSent.DeleteAll();
			helper.Touch1A.CampaignsItemsSent.DeleteAll();
			helper.Touch1B.CampaignsItemsSent.DeleteAll();

			var org = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();
			var sqlCreateContacts = $@"
INSERT INTO dbo.orgContact (OC_PK, OC_IsActive, OC_ContactName, OC_Email, OC_OH, OC_Language, OC_NotifyMode, OC_AttachmentType, OC_SystemCreateTimeUtc, OC_SystemCreateUser, OC_SystemLastEditTimeUtc, OC_SystemLastEditUser)
SELECT newid(), 1, newid(), 'asdf' + CONVERT(varchar(255), NEWID()) + '@ema.il', '{org.PK}', 'ENG', 'EML', 'PDF', GetUtcDate(), '~BP', GetUtcDate(), '~BP' FROM dbo.StmNumberSequence WHERE SNS_Number<{contactsCount}
";
			using (var cmd = Db.Connection.Command(sqlCreateContacts))
			{
				cmd.ExecuteNonQuery();
			}

			using (var cmd = Db.Connection.Command($"select count(*) from dbo.OrgContact where OC_OH = '{org.PK}'"))
			{
				AssertEquals("Contacts Count", contactsCount, cmd.ExecuteScalar());
			}

			var sqlCreateCampaignItems = $@"
insert into dbo.GlbCompanyCampaignItem (G8_PK, G8_IsValid, G8_G0, G8_DeliveryMethod, G8_TrackingStatus, G8_RecipientTableCode, G8_RecipientID, G8_SystemCreateTimeUtc, G8_SystemCreateUser, G8_SystemLastEditTimeUtc, G8_SystemLastEditUser)
select newid(), 1, '{helper.Master.PK}', 'EML', '{TrackingStatusCodes.Codes.VER}', 'OC', OC_PK, GetUtcDate(), '~BP', GetUtcDate(), '~BP' FROM dbo.OrgContact WHERE OC_OH='{org.PK}'
";
			using (var cmd = Db.Connection.Command(sqlCreateCampaignItems))
			{
				AssertEquals("Campaign Items Count", contactsCount, cmd.ExecuteNonQuery());
			}
			return helper.Master.PK;
		}

		#endregion

		class CampaignSummaryStatsForTest : CampaignSummaryStats
		{
			public CampaignSummaryStatsForTest(List<CampaignDeliverySummaryItem> deliverySummaryItems, List<CampaignTransitionResults> transitionResults, int maxHorizontal)
				: base(deliverySummaryItems, transitionResults, maxHorizontal)
			{
			}
		}

		public void TestTouchSourceCampaignPKs()
		{
			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			campaign.SourceCampaignPK = campaign.PK;
			AssertEquals(campaign.PK, campaign.SourceCampaignPK);
			AssertEquals(1, campaign.TouchSourceCampaignPKs.Length);
			AssertEquals(campaign.PK, campaign.TouchSourceCampaignPKs[0]);

			var guid1 = ZGuid.NewZGuid();
			var guid2 = ZGuid.NewZGuid();

			campaign.TouchSourceCampaignPKs = new[] { guid1, guid2 };
			AssertContainsExactElementsInAnyOrder(new[] { guid1, guid2 }, campaign.TouchSourceCampaignPKs);
			AssertEquals(campaign.PK, campaign.SourceCampaignPK);
		}

		GlbCompanyCampaign master;
		GlbCompanyCampaign touch1a;
		GlbCompanyCampaign touch1b;
		GlbCompanyCampaign touch2a;
		GlbCompanyCampaign touch2b;
		GlbCompanyCampaign touch3a;

		void SetupDripCampaign()
		{
			master = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			master.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.DripMarketing;

			touch1a = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			touch1a.G0_HorizontalId = 1;
			touch1a.G0_VerticalId = "A";
			touch1a.G0_G0_Master = master.PK;
			master.AllTouches.Add(touch1a);

			touch1b = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			touch1b.G0_HorizontalId = 1;
			touch1b.G0_VerticalId = "B";
			touch1b.G0_G0_Master = master.PK;
			master.AllTouches.Add(touch1b);

			touch2a = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			touch2a.G0_HorizontalId = 2;
			touch2a.G0_VerticalId = "A";
			touch2a.G0_G0_Master = master.PK;
			master.AllTouches.Add(touch2a);
			touch2a.TransitionRulesToThisCampaign[0].GCD_ParentHorizontalId = 1;
			touch2a.TransitionRulesToThisCampaign[0].GCD_G0_ParentTouch = touch1a.PK;

			touch2b = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			touch2b.G0_HorizontalId = 2;
			touch2b.G0_VerticalId = "B";
			touch2b.G0_G0_Master = master.PK;
			master.AllTouches.Add(touch2b);
			touch2b.TransitionRulesToThisCampaign[0].GCD_G0_ParentTouch = ZGuid.Empty;
			touch2b.TransitionRulesToThisCampaign[0].GCD_ParentHorizontalId = 1;
			touch2b.TransitionRulesToThisCampaign.AddNew();
			touch2b.TransitionRulesToThisCampaign[1].GCD_G0_ParentTouch = touch1a.PK;
			touch2b.TransitionRulesToThisCampaign[1].GCD_ParentHorizontalId = 1;

			touch3a = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			touch3a.G0_HorizontalId = 3;
			touch3a.G0_VerticalId = "A";
			touch3a.G0_G0_Master = master.PK;
			master.AllTouches.Add(touch3a);
			touch3a.TransitionRulesToThisCampaign[0].GCD_G0_ParentTouch = ZGuid.Empty;
			touch3a.TransitionRulesToThisCampaign[0].GCD_ParentHorizontalId = 1;
			touch3a.TransitionRulesToThisCampaign.AddNew();
			touch3a.TransitionRulesToThisCampaign[1].GCD_G0_ParentTouch = touch2b.PK;
			touch3a.TransitionRulesToThisCampaign[1].GCD_ParentHorizontalId = 2;

			GlbCompanyCampaignTestHelper.PopulateCampaign(master, Factory);
			GlbCompanyCampaignTestHelper.PopulateCampaign(touch1a, Factory);
			GlbCompanyCampaignTestHelper.PopulateCampaign(touch1b, Factory);
			GlbCompanyCampaignTestHelper.PopulateCampaign(touch2a, Factory);
			GlbCompanyCampaignTestHelper.PopulateCampaign(touch2b, Factory);
			GlbCompanyCampaignTestHelper.PopulateCampaign(touch3a, Factory);
		}

		#endregion

		#region BusinessObject Overrides

		public void TestSetDefaultValues()
		{
			var campaign = Factory.New<GlbCompanyCampaign>();
			AssertEquals("Campaign company should be current company", GlbCompany.CurrentCompany.PK, campaign.G0_GC);
			AssertEquals("Campaign currency should be current company's currency", GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, campaign.G0_RX_NKCampaignCurrency);
			AssertEquals("Default batch count should be", 100, campaign.G0_BatchCountDefault);
			AssertEquals("Default questions per webpage should be", 20, campaign.G0_QuestionsPerWebPage);
			AssertEquals("Default reply to should be true", true, campaign.UseEmailSenderAddressAsReplyTo);
			AssertEquals("Default Sales And Marketing should be true", true, campaign.G0_IsSalesAndMarketing);
		}

		#endregion

		#region Campaign Items Sent

		public void TestCampaignsItemsSent()
		{
			var contact = Factory.NewWithValidTestData<OrgContact>();
			GlbCompanyCampaign campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			Factory.Save();

			var item = Factory.New<GlbCompanyCampaignItem>();
			AssertEquals("Item should NOT be in list", 0, campaign.CampaignsItemsSent.Count);
			item.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			item.G8_RecipientID = contact.PK;
			item.G8_G0 = campaign.PK;
			Factory.Save();

			BusinessObjectFactory newFactory = new BusinessObjectFactory();

			var campaignReloaded = newFactory.Load<GlbCompanyCampaign>(campaign.PK);
			AssertEquals("Item should be in list", 1, campaignReloaded.CampaignsItemsSent.Count);
			AssertEquals("Item should be in list", item.PK, campaignReloaded.CampaignsItemsSent[0].PK);
		}

		public void TestCampaignsItemsSentForDisplayOnly()
		{
			var contact = Factory.NewWithValidTestData<OrgContact>();
			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			Factory.Save();

			var item = Factory.New<GlbCompanyCampaignItem>();
			AssertEquals("Item should NOT be in list", 0, campaign.CampaignsItemsSentForDisplayOnly.Count);
			item.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			item.G8_RecipientID = contact.PK;
			item.G8_G0 = campaign.PK;
			Factory.Save();

			var newFactory = new BusinessObjectFactory();

			var campaignReloaded = newFactory.Load<GlbCompanyCampaign>(campaign.PK);
			AssertEquals("Should not load automatically", 0, campaignReloaded.CampaignsItemsSentForDisplayOnly.Count);
			campaignReloaded.CampaignsItemsSentForDisplayOnly.Load();
			AssertEquals("Item should be in list", 1, campaignReloaded.CampaignsItemsSentForDisplayOnly.Count);
			AssertEquals("Item should be in list", item.PK, campaignReloaded.CampaignsItemsSentForDisplayOnly[0].PK);
		}

		public void TestCampaignsItemsSent_SetCampaignHasChangesOnSalesRelationsModelChange()
		{
			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			var campaignItem = campaign.CampaignsItemsSent.AddNew();
			campaignItem.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem.G8_RecipientID = ZGuid.NewZGuid();
			Factory.Save();

			AssertEquals("Precondition: campaign.HasChanges", false, campaign.HasChanges);

			var model = campaignItem.SalesRelationModel as SalesRelationModel;
			model.MasterNode.AddNewChild(Factory.New<OrgSalesCall>());
			AssertEquals("Precondition: campaignItem.SalesRelationModel.HasChanges", true, campaignItem.SalesRelationModel.HasChanges);

			AssertEquals("campaign.HasChanges", true, campaign.HasChanges);
		}

		#endregion

		#region Filtering

		public void TestSubQueryWithNullableColumn()
		{
			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			var excludeDuplicateQuery = campaign.ExcludeDuplicateQuery(new ZQuery()).LiteralTextADO;

			var expectedQuery = $"SELECT G8_RecipientID FROM dbo.GlbCompanyCampaignItem WHERE G8_G0 = CONVERT('{campaign.PK}', 'System.Guid') AND G8_RecipientID is not null";
			AssertContains(expectedQuery, excludeDuplicateQuery);
		}

		public void TestDuplicateQueryWithEmptyInner()
		{
			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();

			var result = campaign.ExcludeDuplicateQuery(new ZQuery()).LiteralTextADO;
			result = result.Replace("\r\n", " ");
			result = result.Replace("\t", " ");
			result = result.Trim();

			AssertEquals(true, !result.ToUpperInvariant().Contains("AND GROUP"));
			AssertEquals(true, result.ToUpperInvariant().Contains("AND 1=1 GROUP"));
		}

		public void TestContactQuery()
		{
			GlbCompanyCampaign campaign = Factory.New<GlbCompanyCampaign>();
			AssertEquals("", campaign.FormattedFilterText);

			AddFilterPartsToCampaignForTest(campaign);
			var expected = @"
(
	(
		(
			VCC_ContactSource like 'Source1%' 
			OR
			VCC_ContactSource = 'Source2'
		)
		AND
		(
			VCC_Email like '%z@z.com%' 
			OR
			VCC_Email like '%r@r.com'
		)
		OR
		(
			VCC_JobCategory = 'CEO' 
			AND
			VCC_JobCategory = 'CFO'
		)
	)
	AND
	(
		(
			VCC_TableCode = 'OC' 
			AND
			VCC_OH IN 
			(
				SELECT OH_PK FROM dbo.OrgHeader WHERE OH_RL_NKClosestPort like '%LA%'
			)
		)
		OR
		(
			VCC_TableCode = 'OC' 
			AND
			VCC_OH IN 
			(
				SELECT OH_PK FROM dbo.OrgHeader WHERE OH_RL_NKClosestPort IN 
				(
					SELECT RL_Code FROM dbo.RefUNLOCO WHERE RL_RN_NKCountryCode like 'A%'
				)
			)
		)
	)
)
AND
(
	VCC_TableCode = 'OC' 
	AND
	VCC_PK IN 
	(
		SELECT PC_OC FROM dbo.OrgContactAttribute WHERE PC_Type = 'FOT'
	)
)
";
			AssertMultilineASCIIEquals(expected.Replace("\t", ""), campaign.FormattedFilterText.Replace("\t", ""));
		}

		public void TestContactQuery_WithOrganisationsSearchRestriction()
		{
			Env.Security.OrganisationAllowSearchOutsideLoginCountry.IsAllowed = false;

			GlbCompanyCampaign campaign = Factory.New<GlbCompanyCampaign>();
			GlbCampaignContactCollection campaignContactCollection = Testing.GlbCompanyCampaignSenderTest.ContactCollection(new List<BusinessObject>(), campaign);
			AddFilterPartsToCampaignForTest(campaign);

			campaign.LoadFilteredContacts(campaignContactCollection);
			PropertyInfo lastLoadedAdditionalFilterProperty = campaign.FilteredContacts.GetType().GetProperty("LastLoadedAdditionalFilter", BindingFlags.FlattenHierarchy | BindingFlags.NonPublic | BindingFlags.Instance);
			ZQuery contactQuery = (ZQuery)lastLoadedAdditionalFilterProperty.GetValue(campaign.FilteredContacts, null);
			string lastLoadedAdditionalFilterADOText = contactQuery.LiteralTextADOFormatted.Replace("\t", "");
			AssertContains(expectedContactQueryADOText.Replace("\t", ""), lastLoadedAdditionalFilterADOText);

			const string expectedOrgRestrictionQuery =
@"VCC_OH IN 
(
	SELECT OH_PK FROM dbo.OrgHeader WHERE 
	(
		OH_RL_NKClosestPort like 'AU%' 
		AND
		OH_RL_NKClosestPort >= 'AU' 
		AND
		OH_RL_NKClosestPort <= 'Aþ'
	)
)";
			AssertContains(expectedOrgRestrictionQuery.Replace("\t", ""), lastLoadedAdditionalFilterADOText);
		}

		#region expectedContactQueryADOText

		readonly ZString expectedContactQueryADOText =
@"(
	(
		(
			(
				VCC_ContactSource like 'Source1%' 
				AND
				VCC_ContactSource >= 'Source1' 
				AND
				VCC_ContactSource <= 'Sourceþ'
			)
			OR
			VCC_ContactSource = 'Source2'
		)
		AND
		(
			VCC_Email like '%z@z.com%' 
			OR
			VCC_Email like '%r@r.com'
		)
		OR
		(
			VCC_JobCategory = 'CEO' 
			AND
			VCC_JobCategory = 'CFO'
		)
	)
	AND
	(
		(
			VCC_TableCode = 'OC' 
			AND
			VCC_OH IN 
			(
				SELECT OH_PK FROM dbo.OrgHeader WHERE OH_RL_NKClosestPort like '%LA%'
			)
		)
		OR
		(
			VCC_TableCode = 'OC' 
			AND
			VCC_OH IN 
			(
				SELECT OH_PK FROM dbo.OrgHeader WHERE OH_RL_NKClosestPort IN 
				(
					SELECT RL_Code FROM dbo.RefUNLOCO WHERE 
					(
						RL_RN_NKCountryCode like 'A%' 
						AND
						RL_RN_NKCountryCode >= 'A' 
						AND
						RL_RN_NKCountryCode <= 'þ'
					)
				)
			)
		)
	)
)
AND
(
	VCC_TableCode = 'OC' 
	AND
	VCC_PK IN 
	(
		SELECT PC_OC FROM dbo.OrgContactAttribute WHERE PC_Type = 'FOT'
	)
)
";

		#endregion

		void AddFilterPartsToCampaignForTest(GlbCompanyCampaign campaign)
		{
			ZDBOnlyQuery result = new ZDBOnlyQuery(typeof(CampaignContact));
			ZDBOnlyQuery result1 = new ZDBOnlyQuery(typeof(CampaignContact));
			ZDBOnlyQuery result2 = new ZDBOnlyQuery(typeof(CampaignContact));
			ZDBOnlyQuery query1 = new ZDBOnlyQuery(typeof(CampaignContact));
			ZDBOnlyQuery query2 = new ZDBOnlyQuery(typeof(CampaignContact));
			ZDBOnlyQuery query3 = new ZDBOnlyQuery(typeof(CampaignContact));

			query1.AddToFilter(JoinCondition.And, ViewCampaignContactSchema.VCC_ContactSource, SQLComparisonOperator.StartsWith, "Source1");
			query1.AddToFilter(JoinCondition.Or, ViewCampaignContactSchema.VCC_ContactSource, SQLComparisonOperator.Equal, "Source2");
			query2.AddToFilter(JoinCondition.And, ViewCampaignContactSchema.VCC_Email, SQLComparisonOperator.Contains, "z@z.com");
			query2.AddToFilter(JoinCondition.Or, ViewCampaignContactSchema.VCC_Email, SQLComparisonOperator.EndsWith, "r@r.com");
			query3.AddToFilter(JoinCondition.And, ViewCampaignContactSchema.VCC_JobCategory, SQLComparisonOperator.Equal, "CEO");
			query3.AddToFilter(JoinCondition.And, ViewCampaignContactSchema.VCC_JobCategory, SQLComparisonOperator.Equal, "CFO");
			result1.AddToFilter(query1);
			result1.AddToFilter(query2);
			result1.AddToFilter(query3, JoinCondition.Or);

			result2.AddToFilter(GetUNLOCO(SQLComparisonOperator.Contains, "LA"));
			result2.AddToFilter(GetCountry(SQLComparisonOperator.StartsWith, "A"), JoinCondition.Or);

			result.AddToFilter(result1, JoinCondition.And);
			result.AddToFilter(result2, JoinCondition.And);
			result.AddToFilter(GetContactAttributes(SQLComparisonOperator.Equal, "FOT"));

			campaign.AdditionalFilter = result;
		}

		ZQuery GetContactAttributes(SQLComparisonOperator comparisonOperator, ZString value)
		{
			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(CampaignContact));
			query.AddToFilter(ViewCampaignContactSchema.VCC_TableCode, "OC");

			if (comparisonOperator == SQLComparisonOperator.NotEqual)
			{
				ZDBOnlySubQuery notInQuery = new ZDBOnlySubQuery(typeof(OrgContactAttribute), OrgContactAttributeSchema.PC_OC, true);

				ZDBOnlySubQuery attributeSubQuery = new ZDBOnlySubQuery(typeof(OrgContactAttribute), OrgContactAttributeSchema.PC_OC);
				attributeSubQuery.AddToFilter(OrgContactAttributeSchema.PC_Type, comparisonOperator, value);

				query.AddSubQuery(notInQuery, JoinCondition.Or);
				query.AddSubQuery(attributeSubQuery, JoinCondition.Or);
			}
			else
			{
				ZDBOnlySubQuery attributeSubQuery = new ZDBOnlySubQuery(typeof(OrgContactAttribute), OrgContactAttributeSchema.PC_OC);
				attributeSubQuery.AddToFilter(OrgContactAttributeSchema.PC_Type, comparisonOperator, value);
				query.AddSubQuery(attributeSubQuery, JoinCondition.And);
			}

			return query;
		}

		ZQuery GetUNLOCO(SQLComparisonOperator comparisonOperator, ZString nk)
		{
			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(CampaignContact));
			query.AddToFilter(ViewCampaignContactSchema.VCC_TableCode, "OC");

			ZDBOnlySubQuery orgSubQuery = new ZDBOnlySubQuery(typeof(OrgHeader), OrgHeaderSchema.PK);
			orgSubQuery.AddToFilter(OrgHeaderSchema.OH_RL_NKClosestPort, comparisonOperator, nk);

			query.AddSubQuery(ViewCampaignContactSchema.VCC_OH, orgSubQuery, JoinCondition.And);
			return query;
		}

		ZQuery GetCountry(SQLComparisonOperator comparisonOperator, ZString nk)
		{
			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(CampaignContact));
			query.AddToFilter(ViewCampaignContactSchema.VCC_TableCode, "OC");

			ZDBOnlySubQuery orgSubQuery = new ZDBOnlySubQuery(typeof(OrgHeader), OrgHeaderSchema.PK);
			ZDBOnlySubQuery uNLOCOSubQuery = new ZDBOnlySubQuery(typeof(RefUNLOCO), OrgHeaderSchema.OH_RL_NKClosestPort);
			uNLOCOSubQuery.AddToFilter(RefUNLOCOSchema.RL_RN_NKCountryCode, comparisonOperator, nk);

			orgSubQuery.AddSubQuery(OrgHeaderSchema.OH_RL_NKClosestPort, RefUNLOCOSchema.RL_Code, uNLOCOSubQuery, JoinCondition.And);

			query.AddSubQuery(ViewCampaignContactSchema.VCC_OH, orgSubQuery, JoinCondition.And);
			return query;
		}

		public void TestFilterAlreadyAddedReset()
		{
			GlbStaff someStaff = Factory.NewWithValidTestData<GlbStaff>();
			someStaff.GS_Code = "ZW";
			Factory.Save();

			GlbCompanyCampaign campaign = Factory.New<GlbCompanyCampaign>();
			AssertEquals("", campaign.FormattedFilterText);

			ZQuery query = new ZQuery();
			query.AddToFilter(GetStaffAssignmentPersonAndRoleFilter(SQLComparisonOperator.Equal, "ZW", ""), JoinCondition.And);
			query.AddToFilter(GetStaffAssignmentPersonAndRoleFilter(SQLComparisonOperator.Equal, "", "SAL"));
			campaign.AdditionalFilter = query;

			string queryFirstRun = campaign.FormattedFilterText;
			AssertEquals("2nd time should be the same - FilterAlreadyAdded should be reset", queryFirstRun, campaign.FormattedFilterText);
		}

		ZQuery GetStaffAssignmentPersonAndRoleFilter(SQLComparisonOperator comparisonOperator, ZString staffAssignmentPerson, ZString staffAssignmentRole)
		{
			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(CampaignContact));
			query.AddToFilter(ViewCampaignContactSchema.VCC_TableCode, "OC");
			ZDBOnlySubQuery orgSubQuery = new ZDBOnlySubQuery(typeof(OrgHeader), OrgHeaderSchema.PK);

			bool allPersonItemsNotIn = false;
			ZDBOnlySubQuery staffAssignmentsSubQuery = new ZDBOnlySubQuery(typeof(OrgStaffAssignments), OrgStaffAssignmentsSchema.O8_OH, comparisonOperator.IsNegativeSQLOperator());
			if (!staffAssignmentPerson.IsEmpty)
			{
				staffAssignmentsSubQuery.AddToFilter(OrgStaffAssignmentsSchema.O8_GS_NKPersonResponsible, comparisonOperator.GetNegatingSQLOperatorIfNotInSubquery(), staffAssignmentPerson);
			}

			if (!staffAssignmentRole.IsEmpty)
			{
				staffAssignmentsSubQuery.AddToFilter(OrgStaffAssignmentsSchema.O8_Role, comparisonOperator.GetNegatingSQLOperatorIfNotInSubquery(), staffAssignmentRole);
			}

			if (comparisonOperator != SQLComparisonOperator.NotEqual)
			{
				allPersonItemsNotIn = false;
			}
			orgSubQuery.AddSubQuery(staffAssignmentsSubQuery, JoinCondition.And);

			if (allPersonItemsNotIn)
			{
				ZDBOnlySubQuery notInStaffAssignmentsQuery = new ZDBOnlySubQuery(typeof(OrgStaffAssignments), OrgStaffAssignmentsSchema.O8_OH, true);
				orgSubQuery.AddSubQuery(notInStaffAssignmentsQuery, JoinCondition.Or);
			}

			query.AddSubQuery(ViewCampaignContactSchema.VCC_OH, orgSubQuery, JoinCondition.And);

			return query;
		}

		public void TestDeDuplicateContacts()
		{
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();

			OrgContact contact1 = org.Contacts.AddNew();
			contact1.OC_ContactName = "Bell";
			contact1.OC_Phone = GlbCompanyCampaignForTest.PhoneFilterForLoadingLessObjects;
			contact1.OC_Email = "test@example.com";

			OrgContact contact2 = org.Contacts.AddNew();
			contact2.OC_ContactName = "Ding";
			contact2.OC_Phone = GlbCompanyCampaignForTest.PhoneFilterForLoadingLessObjects;
			contact2.OC_Email = "test@example.com";

			OrgContact contact3 = org.Contacts.AddNew();
			contact3.OC_ContactName = "Witch";
			contact3.OC_Phone = GlbCompanyCampaignForTest.PhoneFilterForLoadingLessObjects;
			contact3.OC_Email = "test@example.com";

			OrgContact contact4 = org.Contacts.AddNew();
			contact4.OC_ContactName = "Dead";
			contact4.OC_Phone = GlbCompanyCampaignForTest.PhoneFilterForLoadingLessObjects;
			contact4.OC_Email = "noone@example.com";

			OrgContact contact5 = org.Contacts.AddNew();
			contact5.OC_ContactName = "Bang";
			contact5.OC_Phone = GlbCompanyCampaignForTest.PhoneFilterForLoadingLessObjects;
			contact5.OC_Email = "";

			OrgContact contact6 = org.Contacts.AddNew();
			contact6.OC_ContactName = "Boom";
			contact6.OC_Phone = GlbCompanyCampaignForTest.PhoneFilterForLoadingLessObjects;
			contact6.OC_Email = "";

			Factory.Save();

			GlbCompanyCampaignForTest campaign = Factory.NewWithValidTestData<GlbCompanyCampaignForTest>();

			GlbCampaignContactCollection campaignContactCollection = Testing.GlbCompanyCampaignSenderTest.ContactCollection(new List<BusinessObject>() { contact1, contact2, contact3, contact4, contact5, contact6 }, campaign);

			campaign.G0_DeDuplicateContacts = false;
			campaign.LoadFilteredContacts(campaignContactCollection);
			AssertEquals("All contacts found", 6, campaignContactCollection.Count);

			campaign.G0_DeDuplicateContacts = true;
			campaign.LoadFilteredContacts(campaignContactCollection);
			AssertEquals(4, campaignContactCollection.Count);

			bool containsContact1 = campaignContactCollection.GetPKs().Contains(contact1.PK);
			bool containsContact2 = campaignContactCollection.GetPKs().Contains(contact2.PK);
			bool containsContact3 = campaignContactCollection.GetPKs().Contains(contact3.PK);
			AssertEquals("Last contact with unique address", true, containsContact1 || containsContact2 || containsContact3);
			AssertCollectionContains("Only contact with this address", contact4.PK, campaignContactCollection.GetPKs());
			AssertCollectionContains("No email - so included", contact5.PK, campaignContactCollection.GetPKs());
			AssertCollectionContains("No email - so included", contact6.PK, campaignContactCollection.GetPKs());

			GlbCompanyCampaignItem campaignSentToContact3 = campaign.CampaignsItemsSent.AddNew();
			campaignSentToContact3.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignSentToContact3.G8_RecipientID = contact3.PK;

			GlbCompanyCampaignItem campaignSentToContact4 = campaign.CampaignsItemsSent.AddNew();
			campaignSentToContact4.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignSentToContact4.G8_RecipientID = contact4.PK;

			GlbCompanyCampaignItem campaignSentToContact5 = campaign.CampaignsItemsSent.AddNew();
			campaignSentToContact5.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignSentToContact5.G8_RecipientID = contact5.PK;

			Factory.Save();

			AssertEquals(3, campaign.CampaignsItemsSent.Count);

			campaignContactCollection = Testing.GlbCompanyCampaignSenderTest.ContactCollection(new List<BusinessObject>() { contact1, contact2, contact3, contact4, contact5, contact6 }, campaign);
			campaign.LoadFilteredContacts(campaignContactCollection);
			AssertEquals("3, 4 and 5 have been sent to - only 6 is left - not a duplicate as blank email", 1, campaignContactCollection.Count);
			AssertEquals("Only contact 6 - blank email", contact6.PK, campaignContactCollection[0].PK);

			campaign.G0_DeDuplicateContacts = false;
			campaignContactCollection = Testing.GlbCompanyCampaignSenderTest.ContactCollection(new List<BusinessObject>() { contact1, contact2, contact3, contact4, contact5, contact6 }, campaign);
			campaign.LoadFilteredContacts(campaignContactCollection);
			AssertEquals(3, campaignContactCollection.Count);
			AssertCollectionContains("Duplicates are included - so 1 is included", contact1.PK, campaignContactCollection.GetPKs());
			AssertCollectionContains("Duplicates are included - so 2 is included", contact2.PK, campaignContactCollection.GetPKs());
			AssertCollectionContains("No email on 6 so it's still included", contact6.PK, campaignContactCollection.GetPKs());
		}

		public void TestIgnoreInactiveContacts()
		{
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();

			OrgContact contact1 = org.Contacts.AddNew();
			contact1.OC_IsActive = false;
			contact1.OC_ContactName = "Johnny";
			contact1.OC_Phone = GlbCompanyCampaignForTest.PhoneFilterForLoadingLessObjects;

			OrgContact contact2 = org.Contacts.AddNew();
			contact2.OC_ContactName = "Knockers";
			contact2.OC_Phone = GlbCompanyCampaignForTest.PhoneFilterForLoadingLessObjects;

			Factory.Save();

			GlbCompanyCampaignForTest campaign = Factory.NewWithValidTestData<GlbCompanyCampaignForTest>();
			FakeFormForTest testForm = new FakeFormForTest(campaign);

			GlbCampaignContactCollection campaignContactCollection = Testing.GlbCompanyCampaignSenderTest.ContactCollection(new List<BusinessObject>() { contact1, contact2 }, campaign);

			testForm.ErrorMessage = ZString.Empty;
			campaign.LoadFilteredContacts(campaignContactCollection);
			AssertEquals("Only 1 contact should be loaded", 1, campaignContactCollection.Count);
			AssertEquals(contact2.PK, campaignContactCollection[0].PK);
		}

		public void TestIgnoreInactiveOrgs()
		{
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_IsActive = false;

			OrgContact contact1 = org.Contacts.AddNew();
			contact1.OC_ContactName = "Johnny";
			contact1.OC_Phone = GlbCompanyCampaignForTest.PhoneFilterForLoadingLessObjects;

			OrgHeader org2 = Factory.NewWithValidTestData<OrgHeader>();

			OrgContact contact2 = org2.Contacts.AddNew();
			contact2.OC_ContactName = "Active person";
			contact2.OC_Phone = GlbCompanyCampaignForTest.PhoneFilterForLoadingLessObjects;

			Factory.Save();

			GlbCompanyCampaignForTest campaign = Factory.NewWithValidTestData<GlbCompanyCampaignForTest>();
			FakeFormForTest testForm = new FakeFormForTest(campaign);

			GlbCampaignContactCollection campaignContactCollection = Testing.GlbCompanyCampaignSenderTest.ContactCollection(new List<BusinessObject>() { contact1, contact2 }, campaign);

			testForm.ErrorMessage = ZString.Empty;
			campaign.LoadFilteredContacts(campaignContactCollection);
			AssertEquals("Only 1 contact should be loaded as org 1 is inactive", 1, campaignContactCollection.Count);
			AssertEquals(contact2.PK, campaignContactCollection[0].PK);
		}

		public void TestLoadFilteredContacts()
		{
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			OrgContact contact1 = org.Contacts.AddNew();
			OrgContact contact2 = org.Contacts.AddNew();
			OrgContact contact3 = org.Contacts.AddNew();

			contact1.OC_ContactName = "A";
			contact2.OC_ContactName = "B";
			contact3.OC_ContactName = "Kot Matroskin";
			contact3.OC_IsActive = false;

			contact1.OC_Phone = GlbCompanyCampaignForTest.PhoneFilterForLoadingLessObjects;
			contact2.OC_Phone = GlbCompanyCampaignForTest.PhoneFilterForLoadingLessObjects;
			contact3.OC_Phone = GlbCompanyCampaignForTest.PhoneFilterForLoadingLessObjects;

			Factory.Save();

			GlbCompanyCampaignForTest campaign = Factory.NewWithValidTestData<GlbCompanyCampaignForTest>();
			campaign.G0_BatchCountDefault = 1;
			FakeFormForTest testForm = new FakeFormForTest(campaign);

			GlbCampaignContactCollection campaignContactCollection = Testing.GlbCompanyCampaignSenderTest.ContactCollection(new List<BusinessObject>() { contact1, contact2, contact3 }, campaign);

			testForm.ErrorMessage = ZString.Empty;
			campaign.LoadFilteredContacts(campaignContactCollection);
			AssertEquals("Too many rows but error reporting off, Event should NOT have been fired", ZString.Empty, testForm.ErrorMessage);

			campaign = Factory.NewWithValidTestData<GlbCompanyCampaignForTest>();
			campaign.G0_BatchCountDefault = 3;

			testForm.ErrorMessage = ZString.Empty;
			campaign.LoadFilteredContacts(campaignContactCollection);
			AssertEquals("Rows OK, Event should NOT have been fired", ZString.Empty, testForm.ErrorMessage);
			AssertEquals("Two contacts should be loaded", 2, campaignContactCollection.Count);

			testForm.ErrorMessage = ZString.Empty;
			campaign.LoadFilteredContacts(campaignContactCollection);
			AssertEquals("Rows OK, Event should NOT have been fired", ZString.Empty, testForm.ErrorMessage);
			AssertEquals("Two contacts should be loaded", 2, campaignContactCollection.Count);
		}

		public void TestLoadFilteredContactsOrganisationFilter()
		{
			OrgHeader org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_Code = "HHH";
			OrgContact contact1 = org1.Contacts.AddNew();
			OrgContact contact2 = org1.Contacts.AddNew();

			OrgHeader org2 = Factory.NewWithValidTestData<OrgHeader>();
			org2.OH_Code = "GGG";
			OrgContact contact3 = org2.Contacts.AddNew();

			contact1.OC_ContactName = "Aaa";
			contact1.OC_Email = "aaa@some.com";
			contact1.OC_IsActive = true;
			contact2.OC_ContactName = "Bbb";
			contact2.OC_Email = "bbb@some.com";
			contact2.OC_IsActive = true;
			contact3.OC_ContactName = "Ccc";
			contact3.OC_Email = "ccc@some.com";
			contact3.OC_IsActive = true;
			Factory.Save();

			GlbCompanyCampaign campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			campaign.G0_BatchCountDefault = 5;

			GlbCampaignContactCollection campaignContactCollection = Testing.GlbCompanyCampaignSenderTest.ContactCollection(new List<BusinessObject>() { contact1, contact2, contact3 }, campaign);

			campaign.AdditionalFilter = new ZQuery(ViewCampaignContactSchema.VCC_OrgCode, SQLComparisonOperator.Equal, "HHH");
			campaign.LoadFilteredContacts(campaignContactCollection);
			AssertEquals("Two contacts should be loaded", 2, campaignContactCollection.Count);

			campaign.AdditionalFilter = new ZQuery(ViewCampaignContactSchema.VCC_OrgCode, SQLComparisonOperator.Equal, "GGG");
			campaign.LoadFilteredContacts(campaignContactCollection);
			AssertEquals("One contact should be loaded", 1, campaignContactCollection.Count);
		}

		public void TestContactsNotSentToQuery()
		{
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			OrgContact contact1 = org.Contacts.AddNew();
			OrgContact contact2 = org.Contacts.AddNew();

			contact1.OC_ContactName = "A";
			contact2.OC_ContactName = "B";
			contact1.OC_Phone = GlbCompanyCampaignForTest.PhoneFilterForLoadingLessObjects;
			contact2.OC_Phone = GlbCompanyCampaignForTest.PhoneFilterForLoadingLessObjects;

			Factory.Save();

			GlbCompanyCampaignForTest campaign = Factory.NewWithValidTestData<GlbCompanyCampaignForTest>();
			GlbCampaignContactCollection campaignContactCollection = Testing.GlbCompanyCampaignSenderTest.ContactCollection(new List<BusinessObject>() { contact1, contact2 }, campaign);
			campaign.LoadFilteredContacts(campaignContactCollection);

			AssertCollectionContains("Collection should contain contact Contact1", contact1.PK, campaignContactCollection.GetPKs());
			AssertCollectionContains("Collection should contain contact Contact2", contact2.PK, campaignContactCollection.GetPKs());

			GlbCompanyCampaignItem campaignSentToContact1 = campaign.CampaignsItemsSent.AddNew();
			campaignSentToContact1.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignSentToContact1.G8_RecipientID = contact1.PK;
			Factory.Save();

			campaign.LoadFilteredContacts(campaignContactCollection);

			AssertCollectionNotContains("Campaign sent to Contact1, Collection should NOT contain contact Contact1", contact1.PK, campaignContactCollection.GetPKs());
			AssertCollectionContains("Collection should contain contact Contact2", contact2.PK, campaignContactCollection.GetPKs());
		}

		public void TestDoNotLoadIfNoFilterAndNoBatchCount()
		{
			SystemDataRegistry.Instance.MaxNumberOfRecordsToShowInDisplayGrids.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 2);
			int contactCount = Factory.GetDatabaseCount(typeof(OrgContact), new ZQuery());
			Assert("Pre-condition:", contactCount > 2);

			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			OrgContact contact1 = org.Contacts.AddNew();
			contact1.OC_ContactName = "Tester";
			contact1.OC_Email = "Tester@abc.com";

			OrgContact contact2 = org.Contacts.AddNew();
			contact2.OC_ContactName = "Demo User";
			contact2.OC_Email = "Tester@demo.com";

			OrgContact contact3 = org.Contacts.AddNew();
			contact3.OC_ContactName = "Demo User 2";
			contact3.OC_Email = "Tester@DDD.com";

			Factory.Save();

			GlbCompanyCampaignForTest campaign = Factory.NewWithValidTestData<GlbCompanyCampaignForTest>();
			campaign.SendToAll = true;

			GlbCampaignContactCollection campaignContactCollection = Testing.GlbCompanyCampaignSenderTest.ContactCollection(new List<BusinessObject>(), Campaign);

			campaign.LoadFilteredContacts(campaignContactCollection);
			AssertEquals(true, campaign.NoFilterDefinedAndNoBatchCount);
			AssertEquals("Nothing should be loaded", 0, campaignContactCollection.Count);

			contact1.OC_Phone = GlbCompanyCampaignForTest.PhoneFilterForLoadingLessObjects;
			contact2.OC_Phone = GlbCompanyCampaignForTest.PhoneFilterForLoadingLessObjects;
			contact3.OC_Phone = GlbCompanyCampaignForTest.PhoneFilterForLoadingLessObjects;
			Factory.Save();

			campaignContactCollection = Testing.GlbCompanyCampaignSenderTest.ContactCollection(new List<BusinessObject>() { contact1, contact2, contact3 }, Campaign);
			campaign.AdditionalFilter = new ZQuery(ViewCampaignContactSchema.VCC_Email, SQLComparisonOperator.StartsWith, "Tester");
			campaign.LoadFilteredContacts(campaignContactCollection);
			AssertEquals("Custom filter is defined", 3, campaignContactCollection.Count);
		}

		#endregion

		#region Properties

		public void TestUseLastSenderEmailValidatesContactDataSource()
		{
			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			campaign.ContactDataSource = ContactDataSourceList.Codes.ClientIntelligence;
			campaign.G0_UseLastEmailSenderAddress = false;

			AssertNoWarnings(campaign.ContactDataSourceInfo);

			campaign.G0_UseLastEmailSenderAddress = true;

			AssertHasWarnings(campaign.ContactDataSourceInfo);
		}

		public void TestContactDataSourceValidatesUseLastSenderEmail()
		{
			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			campaign.ContactDataSource = ContactDataSourceList.Codes.CampaignTracking;
			campaign.G0_UseLastEmailSenderAddress = true;

			AssertNoWarnings(campaign.ContactDataSourceInfo);

			campaign.ContactDataSource = ContactDataSourceList.Codes.ClientIntelligence;

			AssertHasWarnings(campaign.G0_UseLastEmailSenderAddressInfo);
		}

		public void TestIsBroadcastCampaign()
		{
			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			campaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.DripMarketing;

			Assert(!campaign.IsBroadcastCampaign);

			campaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.Broadcast;

			Assert(campaign.IsBroadcastCampaign);
		}

		public void TestSendToAll()
		{
			GlbCompanyCampaign campaign = Factory.New<GlbCompanyCampaign>();
			AssertEquals("By default, 100 batch count", 100, campaign.G0_BatchCountDefault);
			AssertEquals("By default, send to all is false", false, campaign.SendToAll);
			AssertEquals("By default, displayed batch count also 100", 100, campaign.BatchCountForDisplay);
			AssertEquals("By default, displayed batch count not readonly", false, campaign.BatchCountForDisplayInfo.ReadOnly);

			campaign.SendToAll = true;
			AssertEquals("MAX batch count", campaign.MaxDisplayRecords, campaign.G0_BatchCountDefault);
			AssertEquals("Send to all is true", true, campaign.SendToAll);
			AssertEquals("Displayed batch count 0", 0, campaign.BatchCountForDisplay);
			AssertEquals("Displayed batch count readonly", true, campaign.BatchCountForDisplayInfo.ReadOnly);

			campaign.SendToAll = false;
			AssertEquals("100 batch count", 100, campaign.G0_BatchCountDefault);
			AssertEquals("Send to all is false", false, campaign.SendToAll);
			AssertEquals("Displayed batch count also 100", 100, campaign.BatchCountForDisplay);
			AssertEquals("Displayed batch count not readonly", false, campaign.BatchCountForDisplayInfo.ReadOnly);
		}

		public void TestCampaignID()
		{
			ZDateTime referenceTime = new ZDateTime(2004, 2, 2);
			GlbCompanyCampaign campaign = Factory.New<GlbCompanyCampaign>();
			AssertEquals("CampaignID should be", "___", campaign.CampaignID);

			campaign.G0_EstimatedStartedDate = referenceTime;
			AssertEquals("Campaign ID should be", referenceTime.ToString("yyMMdd") + "___", campaign.CampaignID);

			campaign.G0_Category = "Doc";
			AssertEquals("Campaign ID should be", referenceTime.ToString("yyMMdd") + "_Doc__", campaign.CampaignID);

			campaign.G0_Type = "Jam";
			AssertEquals("Campaign ID should be", referenceTime.ToString("yyMMdd") + "_Doc_Jam_", campaign.CampaignID);

			campaign.G0_CampaignName = "Name";
			AssertEquals("Campaign ID should be", referenceTime.ToString("yyMMdd") + "_Doc_Jam_Name", campaign.CampaignID);
		}

		public void TestCampaignID_MaxLength()
		{
			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			var expectedMaxLength = 6 + 1 + GlbCompanyCampaign.Schema.G0_CategoryMaxLength + 1 + GlbCompanyCampaign.Schema.G0_TypeMaxLength + 1 + GlbCompanyCampaign.Schema.G0_CampaignNameMaxLength + 1 + 3 + 1;
			AssertEquals(expectedMaxLength, campaign.CampaignIDInfo.MaxLength);
		}

		[ExpectNoExceptions]
		public void TestGetInfoFromCampaignID()
		{
			ZDateTime referenceTime = new ZDateTime(2004, 2, 2);
			Campaign.G0_EstimatedStartedDate = referenceTime;
			Campaign.G0_Category = "Doc";
			Campaign.G0_Type = "Jam";
			Campaign.G0_CampaignName = "Name";
			AssertEquals("040202_Doc_Jam_Name", Campaign.CampaignID);

			ZDateTime extractedTime;
			ZString extractedCategory;
			ZString extractedType;
			ZString extractedCampaignName;
			GlbCompanyCampaign.GetInfoFromCampaignID(Campaign.CampaignID, out extractedTime, out extractedCategory, out extractedType, out extractedCampaignName);
			AssertEquals(referenceTime, extractedTime);
			AssertEquals("Doc", extractedCategory);
			AssertEquals("Jam", extractedType);
			AssertEquals("Name", extractedCampaignName);

			GlbCompanyCampaign.GetInfoFromCampaignID(ZString.Empty, out extractedTime, out extractedCategory, out extractedType, out extractedCampaignName);
			AssertEquals(ZDateTime.Empty, extractedTime);
			AssertEquals(ZString.Empty, extractedCategory);
			AssertEquals(ZString.Empty, extractedType);
			AssertEquals(ZString.Empty, extractedCampaignName);

			GlbCompanyCampaign.GetInfoFromCampaignID("99UI10_???_AS@_AAA", out extractedTime, out extractedCategory, out extractedType, out extractedCampaignName);
			AssertEquals(ZDateTime.Invalid, extractedTime);
			AssertEquals("???", extractedCategory);
			AssertEquals("AS@", extractedType);
			AssertEquals("AAA", extractedCampaignName);
		}

		public void TestTotalCost()
		{
			GlbCompanyCampaign campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			AssertEquals("Total cost should be 0", 0m, campaign.TotalCost);

			GlbCompanyCampaignBudgetItem item1 = campaign.BudgetItems.AddNew();
			item1.G9_FlatAmount = 100m;

			GlbCompanyCampaignBudgetItem item2 = campaign.BudgetItems.AddNew();
			item2.G9_FlatAmount = 33m;

			GlbCompanyCampaignBudgetItem item3 = campaign.BudgetItems.AddNew();
			item3.G9_FlatAmount = 71m;

			GlbCompanyCampaignBudgetItem item4 = campaign.BudgetItems.AddNew();
			item4.G9_PerUnitAmount = 9m;
			item4.G9_FlatAmount = 300m;

			AssertEquals("Total cost should be", 504m, campaign.TotalCost);

			campaign.BudgetItems.Remove(item3);

			AssertEquals("Total cost should be", 433m, campaign.TotalCost);
		}

		public void TestCostPerUnit()
		{
			GlbCompanyCampaign campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			GlbCompanyCampaignBudgetItem item = campaign.BudgetItems.AddNew();
			item.G9_PerUnitAmount = 25m;
			item.G9_FlatAmount = 300m;

			AssertEquals("Total cost should be", 300m, campaign.TotalCost);
			AssertEquals("No Campaigns should be sent", 0, campaign.CampaignsItemsSent.Count);
			AssertEquals("Cost per unit should be ", 300m, campaign.CostPerUnit);

			campaign.CampaignsItemsSent.AddNew();
			AssertEquals("1 Campaign should be sent", 1, campaign.CampaignsItemsSent.Count);
			AssertEquals("Cost per unit should be ", 325m, campaign.CostPerUnit);

			campaign.CampaignsItemsSent.AddNew();
			AssertEquals("2 Campaigns should be sent", 2, campaign.CampaignsItemsSent.Count);
			AssertEquals("Cost per unit should be ", 175m, campaign.CostPerUnit);
		}

		public void TestCampaignUnitsSent()
		{
			GlbCompanyCampaign campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			AssertEquals("Campaign units sent should be ", 0, campaign.CampaignUnitsSent);

			campaign.CampaignsItemsSent.AddNew();
			AssertEquals("Campaign units sent should be ", 1, campaign.CampaignUnitsSent);

			campaign.CampaignsItemsSent.AddNew();
			AssertEquals("Campaign units sent should be ", 2, campaign.CampaignUnitsSent);
		}

		public void TestDocumentAttachedMessage()
		{
			GlbCompanyCampaign campaign = Factory.New<GlbCompanyCampaign>();
			AssertEquals("No document should be attached", ZBool.False, campaign.IsDocumentAttached);

			campaign.HtmlDocumentBlob = ZBlob.FromAscii("blah blah");
			AssertEquals("Document should be attached", ZBool.True, campaign.IsDocumentAttached);
		}

		public void TestEmailContentIsNotSetButRequired()
		{
			var campaign = Factory.New<GlbCompanyCampaign>();
			Assert("The default HtmlDocumentBlob is empty.", campaign.HtmlDocumentBlob.IsEmpty);
			Assert("Default campaign type BRD is EmailContentIsNotSetButRequired", campaign.EmailContentIsNotSetButRequired);

			campaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.TargetList;
			Assert("IsTargetList type does not need EmailContent", !campaign.EmailContentIsNotSetButRequired);

			campaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.DripMarketing;
			Assert("IsMasterCampaign type does not need EmailContent", !campaign.EmailContentIsNotSetButRequired);

			campaign.G0_BroadcastVoteSurveyExam = InsideSalesTouchTypeList.Codes.OpportunityCreation;
			Assert("IsOpportunityCreationCampaign type does not need EmailContent", !campaign.EmailContentIsNotSetButRequired);
		}

		[StressTest]
		public void TestSearchRecordsFoundMessage_BatchCountLessThanTotal()
		{
			OrgContactCollection allContacts = new OrgContactCollection(Factory);
			allContacts.Load();

			OrgColdCallRegisterCollection allInquiryContacts = new OrgColdCallRegisterCollection(Factory);
			allInquiryContacts.Load();

			GlbCompanyCampaign campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			campaign.G0_BatchCountDefault = 100;
			campaign.G0_DeDuplicateContacts = false;

			GlbCampaignContactCollection campaignContactCollection = new GlbCampaignContactCollection(campaign);

			campaign.LoadFilteredContacts(campaignContactCollection);
			AssertEquals("Filtered contacts should not be loaded", 100, campaignContactCollection.Count);
		}

		[StressTest]
		public void TestSearchRecordsFoundMessage_BatchCountMoreThanTotal()
		{
			OrgContactCollection allContacts = new OrgContactCollection(Factory);
			allContacts.Load();
			GlbCompanyCampaign campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			campaign.G0_BatchCountDefault = allContacts.Count + 100;
			GlbCampaignContactCollection campaignContactCollection = campaign.FilteredContacts;
			AssertEquals("Filtered contacts should not be loaded", 0, campaignContactCollection.Count);

			int uniqueEmails = (int)Db.Connection.ExecuteScalar("select count(distinct OC_Email) from dbo.OrgContact where OC_Email != ''");
			int blankEmails = (int)Db.Connection.ExecuteScalar("select count(*) from dbo.OrgContact where OC_Email = ''");

			campaign.G0_DeDuplicateContacts = true;
			campaignContactCollection = Testing.GlbCompanyCampaignSenderTest.ContactCollection(new List<BusinessObject>(), campaign);
			campaign.LoadFilteredContacts(campaignContactCollection);

			campaign.G0_DeDuplicateContacts = false;
			campaign.LoadFilteredContacts(campaignContactCollection);
		}

		public void TestSearchRecordsFoundMessage_TooManyRecords()
		{
			GlbCompanyCampaign campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			campaign.SendToAll = true;
			SystemDataRegistry.Instance.MaxNumberOfRecordsToShowInDisplayGrids.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 10);
			GlbCampaignContactCollection campaignContactCollection = Testing.GlbCompanyCampaignSenderTest.ContactCollection(new List<BusinessObject>(), campaign);
			campaign.LoadFilteredContacts(campaignContactCollection);
			AssertEquals("Filtered contacts should not be loaded", 0, campaignContactCollection.Count);
		}

		public void TestCampaignName()
		{
			GlbCompanyCampaign campaign = Factory.New<GlbCompanyCampaign>();
			campaign.G0_EmailSubject = ZString.Empty;
			campaign.G0_CampaignName = "Blobs";
			AssertEquals("Email subject was empty, so should default to campaign name", "Blobs", campaign.G0_EmailSubject);
			Assert("should be true by default", campaign.UseCampaignName);
			Assert("should be read only", campaign.G0_EmailSubjectInfo.ReadOnly);

			campaign.G0_CampaignName = "lalal";
			AssertEquals("Campaign name changed, email subject should change", "lalal", campaign.G0_EmailSubject);

			campaign.UseCampaignName = false;
			Assert("should be writable", !campaign.G0_EmailSubjectInfo.ReadOnly);
			campaign.G0_EmailSubject = "PPP";
			campaign.G0_CampaignName = "Blobs";
			AssertEquals("Email subject should  be", "PPP", campaign.G0_EmailSubject);

			GlbCompanyCampaign campaign2 = Factory.New<GlbCompanyCampaign>();
			campaign2.UseCampaignName = false;
			campaign2.G0_EmailSubject = ZString.Empty;
			campaign2.G0_CampaignName = "Blobs";
			AssertEquals("if UseCampaignName is false, don't default from campaign name", ZString.Empty, campaign2.G0_EmailSubject);
			Assert("should be writable", !campaign2.G0_EmailSubjectInfo.ReadOnly);
		}

		public void TestCampaignNameInfo()
		{
			Assert("Default should be read-write", !Campaign.G0_CampaignNameInfo.ReadOnly);

			Campaign.CampaignsItemsSent.AddNew();
			Assert("Should still be read-write as Campaign Type is a default type (Broadcast)", !Campaign.G0_CampaignNameInfo.ReadOnly);

			Campaign.CampaignsItemsSent.RemoveAll();
			Campaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.Survey;
			Campaign.CampaignsItemsSent.AddNew();
			Assert("Should be read-only if Campaign Items sent and Campaign Type is Survey", Campaign.G0_CampaignNameInfo.ReadOnly);

			Campaign.CampaignsItemsSent.RemoveAll();
			Campaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.Voting;
			Campaign.CampaignsItemsSent.AddNew();
			Assert("Should be read-only if Campaign Items sent and Campaign Type is Voting", Campaign.G0_CampaignNameInfo.ReadOnly);

			Campaign.CampaignsItemsSent.RemoveAll();
			Assert("Should be read-write if Campaign Items not sent", !Campaign.G0_CampaignNameInfo.ReadOnly);
		}

		public void TestCampaignHumanReadableName()
		{
			var campaign = Factory.New<GlbCompanyCampaign>();
			campaign.G0_CampaignID = "TST00001000";

			AssertEquals("Human readable name should display both campaign ID and campaign name", "Campaign (TST00001000)", campaign.HumanReadableName);

			campaign.Delete();
			AssertEquals("Human readable name is Campaign when the BO is deleted.", "Campaign", campaign.HumanReadableName);
		}

		public void TestCampaignHumanReadableShortcutName()
		{
			var campaign = Factory.New<GlbCompanyCampaign>();
			campaign.G0_CampaignID = "TST00001000";
			campaign.G0_CampaignName = "tst campaign";

			AssertEquals("Human readable name should display both campaign ID and campaign name", "TST00001000 - tst campaign", campaign.HumanReadableShortcutName);

			campaign.Delete();
			AssertEquals("Human readable name is Campaign when the BO is deleted.", "Campaign", campaign.HumanReadableShortcutName);
		}

		public void TestEmailSubjectInfo()
		{
			Campaign.UseCampaignName = false;

			Assert("Default should be read-write if UseCampaignName is false", !Campaign.G0_EmailSubjectInfo.ReadOnly);

			Campaign.CampaignsItemsSent.AddNew();
			Assert("Should still be read-write as Campaign Type is a default type (Broadcast)", !Campaign.G0_EmailSubjectInfo.ReadOnly);

			Campaign.CampaignsItemsSent.RemoveAll();
			Campaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.Survey;
			Campaign.CampaignsItemsSent.AddNew();
			Assert("Should be read-only if Campaign Items sent and Campaign Type is Survey", Campaign.G0_EmailSubjectInfo.ReadOnly);

			Campaign.CampaignsItemsSent.RemoveAll();
			Campaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.Voting;
			Campaign.CampaignsItemsSent.AddNew();
			Assert("Should be read-only if Campaign Items sent and Campaign Type is Voting", Campaign.G0_EmailSubjectInfo.ReadOnly);

			Campaign.CampaignsItemsSent.RemoveAll();
			Assert("Should be read-write if Campaign Items not sent", !Campaign.G0_EmailSubjectInfo.ReadOnly);
		}

		public void TestG0_EmailSenderOption()
		{
			GlbStaff staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_EmailAddress = "rxm@cargowise.com";
			staff.GS_Code = "NOE";
			staff.GS_FullName = "Edward Onwodi";

			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			campaign.G0_EmailSenderOption = EmailSenderOptionCodeDescriptionList.Codes.COR;
			Assert("G0_EmailSenderRole is empty when G0_EmailSenderOption is equal to 'COR'", campaign.G0_EmailSenderRole == ZString.Empty);

			campaign.G0_GS_NKCampaignCoordinator = "NOE";
			campaign.G0_EmailSenderOption = EmailSenderOptionCodeDescriptionList.Codes.COR;
			campaign.UseEmailSenderAddressAsReplyTo = false;
			AssertEquals("Reply to email address is set", "rxm@cargowise.com", campaign.G0_ReplyToEmail);

			AssertEquals("Free text sender email address should not be empty", "", campaign.G0_EmailSenderName);
			AssertEquals("Free text sender name should not be empty", "", campaign.G0_SenderEmail);

			campaign.G0_GS_NKCampaignCoordinator = "";
			campaign.G0_EmailSenderOption = EmailSenderOptionCodeDescriptionList.Codes.EML;
			campaign.G0_EmailSenderName = "John Smizz";
			campaign.UseEmailSenderAddressAsReplyTo = false;
			Assert("G0_EmailSenderRole is empty when G0_EmailSenderOption is equal to 'EML'", campaign.G0_EmailSenderRole == ZString.Empty);
			AssertEquals("G0_SenderEmail is set to empty when G0_EmailSenderOption is equal to 'EML'", "", campaign.G0_SenderEmail);
			AssertEquals(campaign.G0_ReplyToEmail, campaign.G0_SenderEmail);

			campaign.G0_SenderEmail = "default@cargowise.com";
			campaign.G0_EmailSenderOption = EmailSenderOptionCodeDescriptionList.Codes.EML;
			AssertEquals("Setting the same sender option again does not change G0_SenderEmail.", "default@cargowise.com", campaign.G0_SenderEmail);

			campaign.G0_SenderEmail = "";
			campaign.G0_GS_NKCampaignCoordinator = "NOE";
			Assert("G0_EmailSenderRole is empty when G0_EmailSenderOption is equal to 'EML'", campaign.G0_EmailSenderRole == ZString.Empty);
			Assert(campaign.G0_EmailSenderName == "John Smizz");
			AssertEquals("G0_SenderEmail is not affected by campaign coordinator.", "", campaign.G0_SenderEmail);

			campaign.G0_EmailSenderOption = EmailSenderOptionCodeDescriptionList.Codes.ORG;
			Assert(campaign.G0_ReplyToEmail == "");
			Assert(campaign.G0_SenderEmail == "");
			Assert(campaign.G0_EmailSenderName == "");

			campaign.G0_ReplyToEmail = "789";
			campaign.G0_SenderEmail = "456";
			campaign.G0_EmailSenderName = "123";

			campaign.G0_EmailSenderOption = EmailSenderOptionCodeDescriptionList.Codes.SPS;
			AssertEquals(ZString.Empty, campaign.G0_ReplyToEmail);
			AssertEquals(ZString.Empty, campaign.G0_SenderEmail);
			AssertEquals(ZString.Empty, campaign.G0_EmailSenderName);
		}

		public void TestG0_GS_NKCampaignCoordinator()
		{
			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			staff1.GS_EmailAddress = "rxm@cargowise.com";
			staff1.GS_Code = "NOE";
			staff1.GS_FullName = "Edward Onwodi";

			var staff2 = Factory.NewWithValidTestData<GlbStaff>();
			staff2.GS_EmailAddress = "tst@cargowise.com";
			staff2.GS_Code = "TST";
			staff2.GS_FullName = "Test Name";

			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			campaign.G0_EmailSenderOption = EmailSenderOptionCodeDescriptionList.Codes.COR;
			campaign.G0_GS_NKCampaignCoordinator = "NOE";
			AssertEquals("Precondition: G0_Sender Email is Empty", "", campaign.G0_SenderEmail);

			campaign.G0_SenderEmail = "NotCoordinatorEmail@cargowise.com";
			campaign.Validation.ValidateCoordinatorEmailAddress();
			Assert("Should error when email is not set to one of coordinator's emails", campaign.CoordinatorEmailAddressInfo.HasError("Invalid Coordinator Email address."));

			campaign.G0_GS_NKCampaignCoordinator = "TST";
			AssertEquals("G0_SenderEmail is set to empty when coordinator changes and email sender option is COR", "", campaign.G0_SenderEmail);
			Assert("CoordinatorEmailAddress gets validated when coordinator changes.", !campaign.CoordinatorEmailAddressInfo.HasErrors());

			campaign.G0_EmailSenderOption = EmailSenderOptionCodeDescriptionList.Codes.EML;
			campaign.G0_SenderEmail = "randomEmail@cargowise.com";

			campaign.G0_GS_NKCampaignCoordinator = "NOE";
			AssertEquals("G0_SenderEmail is not deleted when the email sender option is not COR", "randomEmail@cargowise.com", campaign.G0_SenderEmail);
		}

		public void TestEmailSenderOptionTransition()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "TST";
			staff.GS_EmailAddress = "main@gmail.com";
			staff.GS_FullName = "My full name";

			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			campaign.G0_GS_NKCampaignCoordinator = "TST";

			var emailAddress1 = staff.EmailAddresses.AddNew();
			emailAddress1.GSE_EmailAddress = "secondary@gmail.com";
			emailAddress1.GSE_Type = "FIR";

			AssertEquals("Precondition: Default sender Email option is COR", EmailSenderOptionCodeDescriptionList.Codes.COR, campaign.G0_EmailSenderOption);
			AssertEquals("Precondition: G0_SenderEmail is emtpy", "", campaign.G0_SenderEmail);

			CombineAssertions("Transition: COR -> EML", () =>
			{
				campaign.G0_EmailSenderOption = EmailSenderOptionCodeDescriptionList.Codes.EML;
				AssertEquals("When transitioning to EML, G0_SenderEmail should be set to empty string.", "", campaign.G0_SenderEmail);
				AssertEquals("When transitioning to EML, SenderEmailAddress should return empty string.", "", campaign.SenderEmailAddress);
			});

			CombineAssertions("Transition: EML -> COR", () =>
			{
				campaign.G0_EmailSenderOption = EmailSenderOptionCodeDescriptionList.Codes.COR;
				AssertEquals("When transitioning to COR, G0_SenderEmail should be set to empty string.", "", campaign.G0_SenderEmail);
				AssertEquals("When transitioning to COR, main email address is shown by default.", "main@gmail.com", campaign.CoordinatorEmailAddress);
			});

			CombineAssertions("Transition: ORG -> EML & ORG -> COR", () =>
			{
				campaign.G0_EmailSenderOption = EmailSenderOptionCodeDescriptionList.Codes.ORG;
				campaign.G0_EmailSenderOption = EmailSenderOptionCodeDescriptionList.Codes.EML;
				AssertEquals("When transitioning to EML, G0_SenderEmail should be set to empty string.", "", campaign.G0_SenderEmail);
				AssertEquals("When transitioning to EML, SenderEmailAddress should return empty string", "", campaign.SenderEmailAddress);

				campaign.G0_EmailSenderOption = EmailSenderOptionCodeDescriptionList.Codes.ORG;
				campaign.G0_EmailSenderOption = EmailSenderOptionCodeDescriptionList.Codes.COR;
				AssertEquals("When transitioning to COR, G0_SenderEmail should be set to empty string.", "", campaign.G0_SenderEmail);
				AssertEquals("When transitioning to COR, main email address is shown by default", "main@gmail.com", campaign.CoordinatorEmailAddress);
			});

			CombineAssertions("Transition: SPS -> EML & SPS -> COR", () =>
			{
				campaign.G0_EmailSenderOption = EmailSenderOptionCodeDescriptionList.Codes.SPS;
				campaign.G0_EmailSenderOption = EmailSenderOptionCodeDescriptionList.Codes.EML;
				AssertEquals("When transitioning to EML, G0_SenderEmail should be set to empty string.", "", campaign.G0_SenderEmail);
				AssertEquals("When transitioning to EML, main email address is shown by default", "", campaign.SenderEmailAddress);

				campaign.G0_EmailSenderOption = EmailSenderOptionCodeDescriptionList.Codes.SPS;
				campaign.G0_EmailSenderOption = EmailSenderOptionCodeDescriptionList.Codes.COR;
				AssertEquals("When transitioning to COR, G0_SenderEmail should be set to empty string.", "", campaign.G0_SenderEmail);
				AssertEquals("When transitioning to COR, main email address is shown by default", "main@gmail.com", campaign.CoordinatorEmailAddress);
			});
		}

		public void TestG0_ReplyToEmailInfo()
		{
			Campaign.UseEmailSenderAddressAsReplyTo = true;
			Assert("When reply to check box is true should set Reply to email to readonly", Campaign.G0_ReplyToEmailInfo.ReadOnly);

			Campaign.UseEmailSenderAddressAsReplyTo = false;
			Assert("When reply to check box is false Reply to email should not readonly", !Campaign.G0_ReplyToEmailInfo.ReadOnly);
		}

		public void TestUseEmailSenderAddressAsReplyTo()
		{
			Campaign.G0_ReplyToEmail = "john@gmail.com";
			Campaign.G0_SenderEmail = "default@cargowise.com";

			Campaign.UseEmailSenderAddressAsReplyTo = true;
			AssertEquals(true, Campaign.UseEmailSenderAddressAsReplyTo);
			AssertEquals("", Campaign.G0_ReplyToEmail);

			Campaign.G0_ReplyToEmail = "john@gmail.com";
			Campaign.UseEmailSenderAddressAsReplyTo = false;
			AssertEquals(false, Campaign.UseEmailSenderAddressAsReplyTo);
			AssertEquals("default@cargowise.com", Campaign.G0_ReplyToEmail);
			Campaign.G0_EmailSenderOption = EmailSenderOptionCodeDescriptionList.Codes.EML;
			Campaign.UseEmailSenderAddressAsReplyTo = true;
			AssertEquals("", Campaign.G0_ReplyToEmail);

			Campaign.G0_EmailSenderOption = EmailSenderOptionCodeDescriptionList.Codes.ORG;
			Campaign.UseEmailSenderAddressAsReplyTo = true;
			AssertEquals("", Campaign.G0_ReplyToEmail);
		}

		public void TestReplyToEmailAddress()
		{
			GlbCompanyCampaign campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();

			campaign.G0_ReplyToEmail = "replyto@cargowise.com";

			campaign.UseEmailSenderAddressAsReplyTo = false;
			AssertEquals("", campaign.ReplyToEmailAddress);

			campaign.UseEmailSenderAddressAsReplyTo = true;
			AssertEquals("", campaign.SenderEmailAddress);
		}

		public void TestSenderEmailAddress()
		{
			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();

			GlbStaff staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "TST";
			staff.GS_EmailAddress = "john@gmail.com";

			Assert(campaign.SenderEmailAddress == "");
			campaign.G0_GS_NKCampaignCoordinator = "TST";
			Assert(campaign.G0_EmailSenderOption == EmailSenderOptionCodeDescriptionList.Codes.COR);
			Assert("SenderEmailAddress should still not be empty", campaign.SenderEmailAddress == "john@gmail.com");

			campaign.G0_EmailSenderOption = EmailSenderOptionCodeDescriptionList.Codes.COR;
			AssertEquals("john@gmail.com", campaign.SenderEmailAddress);
			AssertEquals(true, campaign.SenderEmailAddress_ReadOnly);

			campaign.G0_EmailSenderOption = EmailSenderOptionCodeDescriptionList.Codes.EML;
			AssertEquals(ZString.Empty, campaign.SenderEmailAddress);
			AssertEquals(false, campaign.SenderEmailAddress_ReadOnly);

			campaign.G0_SenderEmail = "xrm@cargowise.com";
			AssertEquals("xrm@cargowise.com", campaign.SenderEmailAddress);

			campaign.G0_EmailSenderOption = EmailSenderOptionCodeDescriptionList.Codes.ORG;
			AssertEquals(ZString.Empty, campaign.G0_SenderEmail);
			AssertEquals(staff.GS_EmailAddress, campaign.SenderEmailAddress);
			AssertEquals(true, campaign.SenderEmailAddress_ReadOnly);

			campaign.G0_SenderEmail = "xrm@cargowise.com";
			campaign.G0_EmailSenderOption = EmailSenderOptionCodeDescriptionList.Codes.SPS;
			AssertEquals(ZString.Empty, campaign.G0_SenderEmail);
			AssertEquals("", campaign.SenderEmailAddress);
			AssertEquals(true, campaign.SenderEmailAddress_ReadOnly);
		}

		public void TestSenderName()
		{
			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();

			GlbStaff staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "AAE";
			staff.GS_EmailAddress = "john@gmail.com";
			staff.GS_FullName = "My full name";

			Assert(campaign.SenderName == "");
			campaign.G0_GS_NKCampaignCoordinator = "AAE";
			Assert("SenderEmailAddress should not be empty", campaign.SenderName == "My full name");
			campaign.G0_EmailSenderOption = EmailSenderOptionCodeDescriptionList.Codes.COR;
			AssertEquals("My full name", campaign.SenderName);
			AssertEquals(true, campaign.SenderName_ReadOnly);

			campaign.G0_EmailSenderName = "Funny name";
			campaign.G0_EmailSenderOption = EmailSenderOptionCodeDescriptionList.Codes.EML;
			AssertEquals("Funny name", campaign.SenderName);
			AssertEquals(false, campaign.SenderName_ReadOnly);

			campaign.G0_EmailSenderName = "Funny name";
			campaign.G0_EmailSenderOption = EmailSenderOptionCodeDescriptionList.Codes.ORG;
			AssertEquals(staff.GS_FullName, campaign.SenderName);
			AssertEquals(true, campaign.SenderName_ReadOnly);

			campaign.G0_EmailSenderName = "Funny name";
			campaign.G0_EmailSenderOption = EmailSenderOptionCodeDescriptionList.Codes.SPS;
			AssertEquals(staff.GS_FullName, campaign.SenderName);
			AssertEquals(true, campaign.SenderName_ReadOnly);
		}

		public void TestFilterLayoutHasChanges()
		{
			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			Factory.Save();
			campaign.FilterLayoutHasChanges = true;
			Assert(campaign.FilterLayoutHasChanges);
			Assert(campaign.HasChanges);

			campaign.HasChanges = true;
			Assert(campaign.HasChanges);
			Factory.Save();

			campaign.FilterLayoutHasChanges = true;
			Assert(campaign.HasChanges);
		}

		public void TestSimulationCampaignItem()
		{
			var company = Factory.New<GlbCompany>();
			var campaign = Factory.New<GlbCompanyCampaign>();
			campaign.G0_GC = company.PK;

			var simulationCampaign = campaign.SimulationCampaignItem.CompanyCampaign;
			AssertEquals("CampaignsItemsSent Count", 1, simulationCampaign.CampaignsItemsSent.Count);
			AssertEquals("Company PK", company.PK, simulationCampaign.G0_GC);
		}

		public void TestCoordinatorEmailAddressList()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "TST";
			staff.GS_EmailAddress = "john@gmail.com";
			staff.GS_FullName = "My full name";

			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			campaign.G0_GS_NKCampaignCoordinator = "TST";

			var emailAddress1 = staff.EmailAddresses.AddNew();
			emailAddress1.GSE_EmailAddress = "test@gmail.com";
			emailAddress1.GSE_Type = "FIR";

			var emailAddress2 = staff.EmailAddresses.AddNew();
			emailAddress2.GSE_EmailAddress = "test2@gmail.com";
			emailAddress2.GSE_Type = "SEC";

			AssertEquals("MAI", campaign.CoordinatorEmailAddressList[0].Code);
			AssertEquals("FIR", campaign.CoordinatorEmailAddressList[1].Code);
			AssertEquals("SEC", campaign.CoordinatorEmailAddressList[2].Code);

			AssertEquals("john@gmail.com", campaign.CoordinatorEmailAddressList[0].Description);
			AssertEquals("test@gmail.com", campaign.CoordinatorEmailAddressList[1].Description);
			AssertEquals("test2@gmail.com", campaign.CoordinatorEmailAddressList[2].Description);
		}

		public void TestCoordinatorEmailAddress()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "TST";
			staff.GS_EmailAddress = "main@gmail.com";
			staff.GS_FullName = "My full name";

			var emailAddress1 = staff.EmailAddresses.AddNew();
			emailAddress1.GSE_EmailAddress = "secondary@gmail.com";
			emailAddress1.GSE_Type = "FIR";

			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			campaign.G0_GS_NKCampaignCoordinator = "TST";

			campaign.CoordinatorEmailAddress = "secondary@gmail.com";
			AssertEquals("G0_SenderEmail is set to secondary email address", "secondary@gmail.com", campaign.G0_SenderEmail);

			campaign.CoordinatorEmailAddress = "main@gmail.com";
			AssertEquals("G0_SenderEmail should be empty when CoordinatorEmailAddress is Coordinator's main email address", "", campaign.G0_SenderEmail);

			var staff2 = Factory.NewWithValidTestData<GlbStaff>();
			staff2.GS_Code = "ZZZ";
			staff2.GS_EmailAddress = "test2@gmail.com";
			staff2.GS_FullName = "My full name2";

			campaign.CoordinatorEmailAddress = "secondary@gmail.com";
			campaign.G0_GS_NKCampaignCoordinator = "ZZZ";

			AssertEquals("Precondition", EmailSenderOptionCodeDescriptionList.Codes.COR, campaign.G0_EmailSenderOption);
			AssertEquals("G0_SenderEmail is set to empty when campaign coordinator changes and email sender option is COR", "", campaign.G0_SenderEmail);
			AssertEquals("Coordinator Email Address should be updated.", "test2@gmail.com", campaign.CoordinatorEmailAddress);
		}

		#endregion

		#region Doc Manager

		public void TestImplementsIDocManagerSupport()
		{
			GlbCompanyCampaign campaign = Factory.New<GlbCompanyCampaign>();
			Assert("it is an IDocManagerSupport", typeof(IDocManagerSupport).IsAssignableFrom(typeof(GlbCompanyCampaign)));
			AssertEquals("HRJobApplicant.DocManagerCode = GCC", Core.Constants.DocManagerCodes.CompanyCampaign, campaign.DocManagerInfo.DocManagerCode);
		}

		public void TestDocManagerInfo_UseBusinessEntityAsFactoryAsInternal()
		{
			var campaign = Factory.New<GlbCompanyCampaign>();
			AssertEquals("eDocs Factory should be using campaign factory.", true, campaign.DocManagerInfo.UseBusinessEntityFactoryAsInternal);
		}

		#endregion

		#region Logging

		public void TestIsAutologged()
		{
			GlbCompanyCampaign campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			Factory.Save();

			AssertEquals("Should have one log", 1, campaign.Logs.GetAllLogs().Count);
			campaign.G0_EmailSubject = "Blah";
			Factory.Save();
			AssertEquals("Should have two logs", 2, campaign.Logs.GetAllLogs().Count);
		}

		public void TestLogSource()
		{
			GlbCompanyCampaign campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			Factory.Save();

			AssertEquals("Should have one log", 1, campaign.Logs.GetAllLogs().Count);
			AssertStartsWith("Log Source should start with: This Campaign", "This Campaign", campaign.Logs.GetAllLogs()[0].SL_TableFriendlyName);
		}

		#endregion

		#region Document Field Definition Collection

		public void TestDocumentFieldDefinitionCollection()
		{
			GlbCompanyCampaign campaign = Factory.New<GlbCompanyCampaign>();
			AssertNotNull("Document Field Definition collection should not be null", campaign.DocumentFieldDefinitions);

			Type docCompanyCampaignType = ObjectFactory.GetType<DocumentWrappers.IDocCompanyCampaignItem>();

			PropertyInfo[] infos = docCompanyCampaignType.GetProperties();
			Assert("Precondition: DocCompanyCampaign should contain at least one property", infos.Length > 0);

			Assert("Should contain attribute 1 in document field definitions", campaign.DocumentFieldDefinitions.ContainsField(infos[0].Name));
			Assert("Description should NOT be found in document field definitions", !campaign.DocumentFieldDefinitions.ContainsField("3423423423423423423"));
			Assert("Should contain CampaignURL field", campaign.DocumentFieldDefinitions.ContainsField(GlbCompanyCampaign.CampaignURLDocFieldName));
		}

		#endregion

		#region Template Copy

		public void TestTemplateCopy()
		{
			var campaign = Factory.New<GlbCompanyCampaign>();
			campaign.G0_CampaignName = "Test Campaign";
			campaign.G0_EmailSubject = "Test Campaign Subject";
			campaign.G0_ActualStartedDate = ZDateTime.Now;
			campaign.G0_ActualCompletedDate = ZDateTime.Now;
			campaign.G0_EstimatedStartedDate = ZDateTime.Now;
			campaign.G0_EstimatedCompletedDate = ZDateTime.Now;
			campaign.G0_CampaignID = "CRT000001";

			campaign.BudgetItems.AddNew();
			Factory.Save();

			var newCampaign = (GlbCompanyCampaign)((ITemplateCopyable)campaign).TemplateCopy();
			AssertEquals("Test Campaign", newCampaign.G0_CampaignName);
			AssertEquals(ZDateTime.Empty, newCampaign.G0_ActualStartedDate);
			AssertEquals(ZDateTime.Empty, newCampaign.G0_ActualCompletedDate);
			AssertEquals(ZDateTime.Empty, newCampaign.G0_EstimatedStartedDate);
			AssertEquals(ZDateTime.Empty, newCampaign.G0_EstimatedCompletedDate);
			AssertEquals(ZString.Empty, newCampaign.G0_CampaignID);

			AssertEquals(1, newCampaign.BudgetItems.Count);
			AssertEquals(false, newCampaign.UseCampaignName);
			Assert(newCampaign.IsUsingCloneFactory);
		}

		public void TestTemplateCopy_Filters()
		{
			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();

			var filter1 = Factory.New<StmModuleFilter>();
			filter1.S9_FilterName = ZString.Empty;
			filter1.S9_ModuleID = "GlbCompanyCampaignContact";
			filter1.S9_GC = ZGuid.Empty;
			filter1.S9_RelatedEntityID = campaign.PK;

			var userData1 = Factory.New<StmModuleFilterUserData>();
			userData1.S0_RelatedEntityID = campaign.PK;
			userData1.S0_S9 = filter1.PK;

			Factory.Save();

			var newCampaign = (GlbCompanyCampaign)((ITemplateCopyable)campaign).TemplateCopy();

			var oldFilterList = Factory.Load<StmModuleFilter>(new ZQuery(StmModuleFilterSchema.S9_RelatedEntityID, campaign.PK));
			var newFilterList = Factory.Load<StmModuleFilter>(new ZQuery(StmModuleFilterSchema.S9_RelatedEntityID, newCampaign.PK));
			CombineAssertions(() =>
			{
				AssertEquals("Filter count for existing campaign should remain the same", 1, oldFilterList.Length);
				AssertEquals("Filter count for new campaign", 1, newFilterList.Length);
			});

			var newFilter = newFilterList[0];
			var oldFilterUserDataList = Factory.Load<StmModuleFilterUserData>(new ZQuery(StmModuleFilterUserDataSchema.S0_S9, filter1.PK));
			var newFilterUserDataList = Factory.Load<StmModuleFilterUserData>(new ZQuery(StmModuleFilterUserDataSchema.S0_S9, newFilter.PK));
			CombineAssertions(() =>
			{
				AssertEquals("Filter user data count for existing filter should remain the same", 1, oldFilterUserDataList.Length);
				AssertEquals("Filter user data count for new filter", 1, newFilterUserDataList.Length);
			});

			var newFilterUserData = newFilterUserDataList[0];
			CombineAssertions(() =>
			{
				AssertEquals("New filter name", ZString.Empty, newFilter.S9_FilterName);
				AssertEquals("New filter module", "GlbCompanyCampaignContact", newFilter.S9_ModuleID);
				AssertEquals("New filter company", ZGuid.Empty, newFilter.S9_GC);
				AssertEquals("New user data record should be related to new campaign", newCampaign.PK, newFilterUserData.S0_RelatedEntityID);
			});
		}

		public void TestTemplateCopy_QueryPerformance()
		{
			GlbCompanyCampaign campaign = Factory.New<GlbCompanyCampaign>();

			using (Db.Connection.TrackExecutedCommands())
			{
				((ITemplateCopyable)campaign).TemplateCopy();
				var matchingCommands = Db.Connection.ExecutedCommands.Where(x => x.Contains("FROM dbo.StmModuleFilter")).ToArray();
				AssertEquals(1, matchingCommands.Length);
				AssertContains("It's important to add the filter type clause so that the correct index is used. SAD!", "S9_FilterType <> 'FRU'", matchingCommands.Single());
			}
		}

		#endregion

		#region Vote / Exam / Survey

		public void TestG0_BroadcastVoteSurveyExam_DefaultValue()
		{
			AssertEquals("Default should be Broadcast", CampaignTypeList.Codes.Broadcast, Campaign.G0_BroadcastVoteSurveyExam);
		}

		public void TestG0_BroadcastVoteSurveyExam_ShouldReportDeveloperErrorIfChangedWhenCampaignItemsAlreadySent()
		{
			Campaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.Voting;
			AssertEquals("There should be no errors reported", "", ErrorReporter.LastMessageReported);

			Campaign.CampaignsItemsSent.AddNew();
			Campaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.Broadcast;
			AssertEquals("Campaign type should not be changed once the campaign emails are sent", ErrorReporter.LastMessageReported);
			AssertEquals("GlbCompanyCampaign_ShouldNotChangeTypeOnceSent", ErrorReporter.LastKeyReported);

			ErrorReporter.Clear();
		}

		public void TestG0_BroadcastVoteSurveyExam_ShouldResetQuestions()
		{
			Campaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.Survey;
			VoteExamSurveyQuestion surveyQuestion1 = Campaign.Questions.AddNew();
			VoteExamSurveyQuestion surveyQuestion2 = Campaign.Questions.AddNew();
			AssertEquals("Pre-condition", 2, Campaign.Questions.Count);
			AssertNull("Pre-condition. Survey should not have VoteHeader", Campaign.VoteHeader);

			Campaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.Voting;
			AssertEquals("Questions should be cleared and replaced by VoteHeader", 1, Campaign.Questions.Count);
			AssertNotEquals(surveyQuestion1, Campaign.Questions[0]);
			AssertNotEquals(surveyQuestion2, Campaign.Questions[0]);
			AssertEquals(Campaign.VoteHeader, Campaign.Questions[0]);

			Campaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.Survey;
			AssertEquals("Questions should be cleared", 0, Campaign.Questions.Count);
		}

		public void TestG0_BroadcastVoteSurveyExam_SetDefaultAnswerType()
		{
			AssertEquals("Pre-condition", "", Campaign.G0_DefaultAnswerType);

			Campaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.Survey;
			AssertEquals(VoteExamSurveyAnswerTypeList.Codes.YesNo, Campaign.G0_DefaultAnswerType);

			Campaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.Voting;
			AssertEquals("", Campaign.G0_DefaultAnswerType);

			Campaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.Broadcast;
			AssertEquals("", Campaign.G0_DefaultAnswerType);
		}

		public void TestG0_BroadcastVoteSurveyExam_ReadOnlyIfCampaignItemsAlreadySent()
		{
			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			AssertEquals("Not read only because no campaign items are sent yet", false, campaign.G0_BroadcastVoteSurveyExamInfo.ReadOnly);
			AssertNull("Should not load campaign item collection", campaign.campaignsItemsSent);

			var item = campaign.CampaignsItemsSent.AddNew();
			item.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			item.G8_RecipientID = ZGuid.NewZGuid();

			AssertEquals("Is read only because there is sent campaign item", true, campaign.G0_BroadcastVoteSurveyExamInfo.ReadOnly);

			Factory.Save();

			var loaadedCampaign = new BusinessObjectFactory().Load<GlbCompanyCampaign>(campaign.PK);
			AssertEquals("Is read only because there is sent campaign item", true, loaadedCampaign.G0_BroadcastVoteSurveyExamInfo.ReadOnly);
			AssertNull("Should not load campaign item collection", loaadedCampaign.campaignsItemsSent);
		}

		public void TestG0_BroadcastVoteSurveyExam_CampaignTypeChangingEventFired()
		{
			object shouldBeCancelled = false;
			bool campaignTypeChangingFired = false;
			Campaign.CampaignTypeChanging += (object sender, CancelEventArgs args) =>
			 {
				 campaignTypeChangingFired = true;
				 args.Cancel = (bool)shouldBeCancelled;
			 };

			Assert("Pre-condition", !campaignTypeChangingFired);

			Campaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.Survey;
			Assert(campaignTypeChangingFired);

			campaignTypeChangingFired = false;
			shouldBeCancelled = true;
			Campaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.Voting;
			Assert(campaignTypeChangingFired);
			AssertEquals("Should be cancelled", CampaignTypeList.Codes.Survey, Campaign.G0_BroadcastVoteSurveyExam);
		}

		public void TestG0_BroadcastVoteSurveyExam_IsRandomisableWhenChangedToVotingCampaign()
		{
			AssertEquals("Pre-condition", CampaignTypeList.Codes.Broadcast, Campaign.G0_BroadcastVoteSurveyExam);
			AssertEquals("Pre-condition", false, Campaign.G0_RandomizeWithinHeader);

			Campaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.Voting;
			Assert("When campaign type is changed to Voting, questions are randomised by default.", Campaign.G0_RandomizeWithinHeader);

			Campaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.Survey;
			AssertEquals("G0_RandomizeWithinHeader should only be applicable to Voting.", false, Campaign.G0_RandomizeWithinHeader);
		}

		public void TestCampaignTypeCaption()
		{
			AssertEquals("Default type", "Broadcast Campaign", Campaign.CampaignTypeCaption);

			Campaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.Voting;
			AssertEquals("Voting Campaign", Campaign.CampaignTypeCaption);

			Campaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.Survey;
			AssertEquals("Survey Campaign", Campaign.CampaignTypeCaption);
		}

		public void TestIsSurveyCampaign()
		{
			Assert("Pre-condition", !Campaign.IsSurveyCampaign);

			Campaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.Voting;
			Assert(!Campaign.IsSurveyCampaign);

			Campaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.Survey;
			Assert(Campaign.IsSurveyCampaign);
		}

		public void TestIsVoteCampaign()
		{
			Assert("Pre-condition", !Campaign.IsVoteCampaign);

			Campaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.Voting;
			Assert(Campaign.IsVoteCampaign);

			Campaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.Survey;
			Assert(!Campaign.IsVoteCampaign);
		}

		public void TestIsExamCampaign()
		{
			Assert("Pre-condition", !Campaign.IsExamCampaign);

			Campaign.G0_BroadcastVoteSurveyExam = Core.Constants.Recruiter.LearningCentreCampaignType;
			Assert(Campaign.IsExamCampaign);

			Campaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.Survey;
			Assert(!Campaign.IsExamCampaign);
		}

		public void TestRandomiseQuestionAndMultipleChoiceOrder()
		{
			Assert("Pre-condition", !Campaign.IsVoteCampaign);
			Assert("Pre-condition", !Campaign.RandomiseQuestionAndMultipleChoiceOrder);

			Campaign.G0_RandomizeWithinHeader = true;
			Campaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.Voting;
			Assert(Campaign.RandomiseQuestionAndMultipleChoiceOrder);

			Campaign.G0_RandomizeWithinHeader = false;
			AssertEquals(false, Campaign.RandomiseQuestionAndMultipleChoiceOrder);

			Campaign.G0_RandomizeWithinHeader = true;
			Campaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.Survey;
			Assert(!Campaign.RandomiseQuestionAndMultipleChoiceOrder);
		}

		public void TestRequiresCampaignURL()
		{
			Assert("Default campaign type does not require CampaignURL", !Campaign.RequiresCampaignURL);

			Campaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.Survey;
			Assert(Campaign.RequiresCampaignURL);

			Campaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.Voting;
			Assert(Campaign.RequiresCampaignURL);
		}

		public void TestIsCampaignURLSettingsValid()
		{
			Assert("Pre-condition", !Campaign.IsCampaignURLSettingsValid);

			WebDataRegistry.Instance.WebCampaignUrl.SetValue(Campaign.G0_GC.ToGuid(), Guid.Empty, Guid.Empty, "http://meh");
			Assert(Campaign.IsCampaignURLSettingsValid);
		}

		public void TestQuestions()
		{
			Campaign.FillWithValidTestData();
			Campaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.Survey;
			Assert("Should be registered as EditableChildObject", Campaign.IsRegisteredEditableChildObject(Campaign.Questions));

			VoteExamSurveyQuestion surveyQuestion = Campaign.Questions.AddNew();
			surveyQuestion.FillWithValidTestData();
			Factory.Save();

			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			GlbCompanyCampaign campaignFromNewFactory = newFactory.Load<GlbCompanyCampaign>(Campaign.PK);
			AssertEquals(1, campaignFromNewFactory.Questions.Count);
			AssertEquals(surveyQuestion.PK, campaignFromNewFactory.Questions[0].PK);
		}

		public void TestQuestions_SortedWhenLazyLoaded()
		{
			GlbCompanyCampaign campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			VoteExamSurveyQuestion header1 = campaign.Questions.AddNew();
			header1.FillWithValidTestData();
			header1.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.Header;
			VoteExamSurveyQuestion question1 = campaign.Questions.AddNew();
			question1.FillWithValidTestData();
			VoteExamSurveyQuestion question2 = campaign.Questions.AddNew();
			question2.FillWithValidTestData();
			VoteExamSurveyQuestion header2 = campaign.Questions.AddNew();
			header2.FillWithValidTestData();
			header2.HY_QuestionOrder = question2.HY_QuestionOrder;
			header2.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.Header;
			Factory.Save();

			GlbCompanyCampaign campaignInNewFactory = new BusinessObjectFactory().Load<GlbCompanyCampaign>(campaign.PK);
			AssertEquals(4, campaignInNewFactory.Questions.Count);
			AssertEquals(header1.PK, campaignInNewFactory.Questions[0].PK);
			AssertEquals(question1.PK, campaignInNewFactory.Questions[1].PK);
			AssertEquals(header2.PK, campaignInNewFactory.Questions[2].PK);
			AssertEquals(question2.PK, campaignInNewFactory.Questions[3].PK);
		}

		public void TestInactiveQuestions()
		{
			Assert(Campaign.IsRegisteredEditableChildObject(Campaign.InactiveQuestions));

			AssertEquals("Precondition", 0, Campaign.Questions.Count);
			AssertEquals("Precondition", 0, Campaign.InactiveQuestions.Count);

			VoteExamSurveyQuestion question1 = Campaign.Questions.AddNew();
			VoteExamSurveyQuestion question2 = Campaign.Questions.AddNew();
			AssertEquals(2, Campaign.Questions.Count);
			AssertEquals(0, Campaign.InactiveQuestions.Count);

			question1.HY_IsActive = false;
			AssertEquals(1, Campaign.Questions.Count);
			AssertCollectionContains(question2, Campaign.Questions);
			AssertEquals(1, Campaign.InactiveQuestions.Count);
			AssertCollectionContains(question1, Campaign.InactiveQuestions);

			question2.HY_IsActive = false;
			AssertEquals(0, Campaign.Questions.Count);
			AssertEquals(2, Campaign.InactiveQuestions.Count);
			AssertCollectionContains(question1, Campaign.InactiveQuestions);
			AssertCollectionContains(question2, Campaign.InactiveQuestions);

			question1.HY_IsActive = true;
			AssertEquals(1, Campaign.Questions.Count);
			AssertCollectionContains(question1, Campaign.Questions);
			AssertEquals(1, Campaign.InactiveQuestions.Count);
			AssertCollectionContains(question2, Campaign.InactiveQuestions);
		}

		public void TestVoteHeader()
		{
			AssertNull("Pre-condition", Campaign.VoteHeader);

			Campaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.Voting;
			AssertEquals(Campaign.Questions[0], Campaign.VoteHeader);

			Campaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.Survey;
			AssertNull(Campaign.VoteHeader);
		}

		public void TestGetActualQuestions()
		{
			Campaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.Survey;
			VoteExamSurveyQuestion surveyQuestion1 = Campaign.Questions.AddNew();
			surveyQuestion1.HY_Question = "survey1";
			VoteExamSurveyQuestion surveyQuestion2 = Campaign.Questions.AddNew();
			surveyQuestion2.HY_Question = "survey2";

			VoteExamSurveyQuestion[] actualQuestions = Campaign.GetActualQuestions();
			AssertEquals(2, actualQuestions.Length);
			AssertEquals("survey1", actualQuestions[0].HY_Question);
			AssertEquals("survey2", actualQuestions[1].HY_Question);

			actualQuestions = Campaign.GetActualQuestions(new ZQuery(VoteExamSurveyQuestionSchema.HY_Question, SQLComparisonOperator.EndsWith, "1"));
			AssertEquals(1, actualQuestions.Length);
			AssertEquals("survey1", actualQuestions[0].HY_Question);

			Campaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.Voting;
			Campaign.VoteHeader.HY_Question = "voteheader";
			VoteExamSurveyQuestion votingItem1 = Campaign.VoteHeader.SubQuestions.AddNew();
			votingItem1.HY_Question = "candidate1";
			VoteExamSurveyQuestion votingItem2 = Campaign.VoteHeader.SubQuestions.AddNew();
			votingItem2.HY_Question = "candidate2";
			actualQuestions = Campaign.GetActualQuestions();
			AssertEquals(2, actualQuestions.Length);
			AssertEquals("candidate1", actualQuestions[0].HY_Question);
			AssertEquals("candidate2", actualQuestions[1].HY_Question);

			actualQuestions = Campaign.GetActualQuestions(new ZQuery(VoteExamSurveyQuestionSchema.HY_Question, SQLComparisonOperator.EndsWith, "2"));
			AssertEquals(1, actualQuestions.Length);
			AssertEquals("candidate2", actualQuestions[0].HY_Question);
		}

		public void TestGetActualQuestions_CountrySpecific()
		{
			Campaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.Survey;
			VoteExamSurveyQuestion surveyQuestion1 = Campaign.Questions.AddNew();
			surveyQuestion1.HY_Question = "survey1";
			surveyQuestion1.HY_RN_NKCountryCode = "AU";
			VoteExamSurveyQuestion surveyQuestion2 = Campaign.Questions.AddNew();
			surveyQuestion2.HY_Question = "survey2";
			surveyQuestion2.HY_RN_NKCountryCode = "NZ";
			VoteExamSurveyQuestion surveyQuestion3 = Campaign.Questions.AddNew();
			surveyQuestion3.HY_Question = "survey3";
			surveyQuestion3.HY_RN_NKCountryCode = "";

			VoteExamSurveyQuestion[] questions = Campaign.GetActualQuestions("AU");
			AssertEquals(2, questions.Length);
			AssertCollectionContains(surveyQuestion1, questions);
			AssertCollectionContains(surveyQuestion3, questions);

			questions = Campaign.GetActualQuestions("NZ");
			AssertEquals(2, questions.Length);
			AssertCollectionContains(surveyQuestion2, questions);
			AssertCollectionContains(surveyQuestion3, questions);

			questions = Campaign.GetActualQuestions((string)null);
			AssertEquals(3, questions.Length);
			AssertCollectionContains(surveyQuestion1, questions);
			AssertCollectionContains(surveyQuestion2, questions);
			AssertCollectionContains(surveyQuestion3, questions);
		}

		public void TestActualQuestionsForBinding()
		{
			Campaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.Survey;
			VoteExamSurveyQuestion surveyQuestion1 = Campaign.Questions.AddNew();
			surveyQuestion1.HY_Question = "exam1";
			VoteExamSurveyQuestion surveyQuestion2 = Campaign.Questions.AddNew();
			surveyQuestion2.HY_Question = "exam2";
			AssertEquals(2, Campaign.ActualQuestionsForBinding.Count);
			AssertEquals("exam1", Campaign.ActualQuestionsForBinding[0].HY_Question);
			AssertEquals("exam2", Campaign.ActualQuestionsForBinding[1].HY_Question);

			Campaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.Voting;
			Campaign.VoteHeader.HY_Question = "voteheader";
			VoteExamSurveyQuestion votingItem1 = Campaign.VoteHeader.SubQuestions.AddNew();
			votingItem1.HY_Question = "candidate1";
			VoteExamSurveyQuestion votingItem2 = Campaign.VoteHeader.SubQuestions.AddNew();
			votingItem2.HY_Question = "candidate2";
			AssertEquals(2, Campaign.ActualQuestionsForBinding.Count);
			AssertEquals("candidate1", Campaign.ActualQuestionsForBinding[0].HY_Question);
			AssertEquals("candidate2", Campaign.ActualQuestionsForBinding[1].HY_Question);
		}

		public void TestSurveySummaries()
		{
			Campaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.Survey;
			VoteExamSurveyQuestion surveyQuestion1 = Campaign.Questions.AddNew();
			surveyQuestion1.HY_Question = "question1";
			VoteExamSurveyQuestion surveyQuestion2 = Campaign.Questions.AddNew();
			surveyQuestion2.HY_Question = "question2";

			VoteExamSurveySummaryCollection summaryCollection = new VoteExamSurveySummaryCollection(Campaign.ActualQuestionsForBinding);
			summaryCollection.Load();
			AssertEquals(2, summaryCollection.SurveyQuestions.Count);
			AssertEquals(2, summaryCollection.Count);
		}

		public void TestDelete_ShouldDeleteQuestions()
		{
			var campaign = Factory.New<GlbCompanyCampaign>();
			campaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.Survey;
			campaign.G0_CampaignName = ZGuid.NewZGuid().ToString();
			campaign.G0_CampaignID = "12345";
			var question1 = campaign.Questions.AddNew();
			question1.HY_Question = "question1";
			question1.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.MultipleChoice;
			question1.SubQuestions.AddNew();
			question1.SubQuestions.AddNew();
			var question2 = campaign.Questions.AddNew();
			question2.HY_Question = "question2";
			question2.HY_IsActive = false;
			Factory.Save();

			campaign.Delete();
			Assert(campaign.IsDeleted);
			Assert(question1.IsDeleted);
			Assert(question2.IsDeleted);
			Factory.Save();
		}

		[TestDate(2006, 2, 5)]
		public void TestHasEnded()
		{
			Campaign.G0_ActualCompletedDate = new ZDateTime(2006, 1, 1);
			AssertEquals(true, Campaign.HasEnded);

			Campaign.G0_ActualCompletedDate = new ZDateTime(2006, 2, 5);
			AssertEquals(true, Campaign.HasEnded);

			Campaign.G0_ActualCompletedDate = new ZDateTime(2006, 3, 5);
			AssertEquals(false, Campaign.HasEnded);
		}

		public void TestVoteExamSurveyNameAndDescription()
		{
			Campaign.G0_CampaignName = "MEH MEH";
			Campaign.G0_CampaignComment = "Some exam description";
			AssertEquals("MEH MEH", Campaign.VoteExamSurveyName);
			AssertEquals("Some exam description", Campaign.VoteExamSurveyDescription);
		}

		public void TestVoteDetails()
		{
			Campaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.Voting;
			Campaign.G0_CampaignName = "Vote 123";
			Campaign.G0_CampaignComment = "A very important voting round";
			Campaign.VoteHeader.HY_Min = 1;
			Campaign.VoteHeader.HY_Max = 20;
			string actual = GetVoteExamSurveyDetailsAsString(Campaign);
			string expected = @"
Vote Name: Vote 123
Description: A very important voting round
No. of Votes Required: 1 (up to 20)
".Trim();
			AssertEquals(expected, actual);

			Campaign.VoteHeader.HY_Min = 2;
			Campaign.VoteHeader.HY_Max = 2;
			expected = @"
Vote Name: Vote 123
Description: A very important voting round
No. of Votes Required: 2
".Trim();
			actual = GetVoteExamSurveyDetailsAsString(Campaign);
			AssertEquals(expected, actual);
		}

		public void TestSurveyDetails()
		{
			Campaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.Survey;
			Campaign.G0_CampaignName = "Survey 123";
			Campaign.G0_CampaignComment = "A very important survey";
			VoteExamSurveyQuestion header = Campaign.Questions.AddNew();
			header.HY_RN_NKCountryCode = "AU";
			header.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.Header;
			VoteExamSurveyQuestion auQuestion1 = Campaign.Questions.AddNew();
			auQuestion1.HY_RN_NKCountryCode = "AU";
			VoteExamSurveyQuestion auQuestion2 = Campaign.Questions.AddNew();
			auQuestion2.HY_RN_NKCountryCode = "AU";
			VoteExamSurveyQuestion nzQuestion1 = Campaign.Questions.AddNew();
			nzQuestion1.HY_RN_NKCountryCode = "NZ";

			string actual = GetVoteExamSurveyDetailsAsString(Campaign, "AU");
			string expected = @"
Survey Name: Survey 123
Description: A very important survey
No. of Questions: 2
".Trim();
			AssertEquals(expected, actual);

			actual = GetVoteExamSurveyDetailsAsString(Campaign, "NZ");
			expected = @"
Survey Name: Survey 123
Description: A very important survey
No. of Questions: 1
".Trim();
			AssertEquals(expected, actual);
		}

		string GetVoteExamSurveyDetailsAsString(GlbCompanyCampaign campaign)
		{
			return GetVoteExamSurveyDetailsAsString(campaign, "");
		}

		string GetVoteExamSurveyDetailsAsString(GlbCompanyCampaign campaign, string countryCode)
		{
			return string.Join("\r\n", campaign.GetVoteExamSurveyDetails(countryCode).Select(x => x.Key + ": " + x.Value).ToArray());
		}

		public void TestG0_DefaultAnswerTypeList()
		{
			IEnumerable list = CargoWise.ComponentModel.MetaData.GetListDataSource(Campaign, Campaign.G0_DefaultAnswerTypeInfo.PropertyDescriptor);
			AssertEquals(Campaign.Lookups.DefaultAnswerTypes.ElementsAsString, ((CodeDescriptionPairList)list).ElementsAsString);
		}

		#endregion

		#region Custom Fields

		[TestedType(typeof(GlbCompanyCampaign))]
		class CustomFieldsTest : TestICustomFieldProvider
		{
		}

		public void TestGetTemplateSelectionCriteria()
		{
			var campaign = Factory.New<GlbCompanyCampaign>();
			campaign.G0_Category = "PRINT";
			campaign.G0_Type = "MEDIA";
			IWorkflowProviderCore provider = campaign;
			var criteria = (ColumnValueRanker)provider.GetTemplateSelectionCriteria();
			AssertEquals("P0_SubType1 is G0_Category", "PRINT", criteria.GetValues(ProcessTaskTemplateSchema.P0_SubType1)[0].ToString());
			AssertEquals("P0_SubType2 is G0_Type", "MEDIA", criteria.GetValues(ProcessTaskTemplateSchema.P0_SubType2)[0].ToString());
		}

		#endregion

		#region IImportParentRelatedActivityInfoOnNew

		public void TestImportParentRelatedActivityInfoOnNew()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "TESTAA";
			var contact = org.Contacts.AddNew();
			contact.OC_ContactName = "Test Test";
			contact.OC_Email = "test@test.com";
			var inquiry = Factory.New<SalesEnquiry>();
			inquiry.O1_OH_ConvertedToQualifiedLead = org.PK;
			inquiry.O1_OC_LinkedContact = contact.PK;

			var campaign = Factory.New<GlbCompanyCampaign>();
			((IImportParentRelatedActivityInfoOnNew)campaign).ImportParentInfo(inquiry, new ImportRelatedActivityNoDecisionFactory());

			AssertEquals("TESTAA", campaign.ImportOrgCode);
			AssertEquals("Test Test", campaign.ImportContactName);
			AssertEquals(ZGuid.Empty, campaign.ImportInquiryPK);

			var inquiryWithNoContactLink = Factory.New<SalesEnquiry>();
			inquiryWithNoContactLink.O1_Email = "eddie@hotmail.com";
			inquiryWithNoContactLink.O1_ContactName = "Eddie Tan";
			var campaign1 = Factory.New<GlbCompanyCampaign>();
			((IImportParentRelatedActivityInfoOnNew)campaign1).ImportParentInfo(inquiryWithNoContactLink, new ImportRelatedActivityNoDecisionFactory());
			AssertEquals("", campaign1.ImportOrgCode);
			AssertEquals("Eddie Tan", campaign1.ImportContactName);
			AssertEquals(inquiryWithNoContactLink.PK, campaign1.ImportInquiryPK);
			Assert(campaign1.ContactDataSource == ContactDataSourceList.Codes.Inquiries);

			var previousCampaign = Factory.New<GlbCompanyCampaign>();

			var campaign2 = Factory.New<GlbCompanyCampaign>();
			var campaignItem = campaign.CampaignsItemsSent.AddNew();
			campaignItem.G8_RecipientID = inquiryWithNoContactLink.PK;
			campaignItem.G8_RecipientTableCode = OrgColdCallRegisterSchema.Constants.Prefix;
			((IImportParentRelatedActivityInfoOnNew)campaign2).ImportParentInfo(campaignItem, new ImportRelatedActivityNoDecisionFactory());
			AssertEquals("", campaign2.ImportOrgCode);
			AssertEquals("Eddie Tan", campaign2.ImportContactName);
			AssertEquals(ZGuid.Empty, campaign2.ImportInquiryPK);
			Assert(campaign2.ContactDataSource == ContactDataSourceList.Codes.CampaignTracking);
			AssertEquals(campaign2.SourceCampaignPK, campaignItem.CompanyCampaign.PK);

			var campaign3 = Factory.New<GlbCompanyCampaign>();
			((IImportParentRelatedActivityInfoOnNew)campaign3).ImportParentInfo(campaign, new ImportRelatedActivityNoDecisionFactory());
			Assert(campaign3.ContactDataSource == ContactDataSourceList.Codes.CampaignTracking);
			AssertEquals(campaign3.SourceCampaignPK, campaign.PK);
		}

		#endregion

		#region Tracked Links

		public void TestNeedsTrackedLinks()
		{
			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			campaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.Broadcast;
			campaign.TemplateEditor.TrackedLinks.AddNew();

			AssertEquals(0, campaign.TrackedLinks.Count);
			AssertEquals(true, campaign.NeedsTrackedLinks);

			campaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.DripMarketing;
			AssertEquals(false, campaign.NeedsTrackedLinks);

			campaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.TargetList;
			AssertEquals(false, campaign.NeedsTrackedLinks);

			campaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.Broadcast;
			campaign.TrackedLinks.AddNew();
			AssertEquals(false, campaign.NeedsTrackedLinks);

			campaign.TrackedLinks.DeleteAll();
			AssertEquals(true, campaign.NeedsTrackedLinks);

			campaign.CampaignsItemsSent.AddNew();
			AssertEquals(false, campaign.NeedsTrackedLinks);
		}

		public void TestNeedsTrackedLinks2()
		{
			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			campaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.Broadcast;
			var link = campaign.TemplateEditor.TrackedLinks.AddNew();

			campaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.Broadcast;
			AssertEquals(true, campaign.NeedsTrackedLinks);

			campaign.TemplateEditor.TrackedLinks.DeleteAll();
			AssertEquals(false, campaign.NeedsTrackedLinks);
		}

		public void TestTrackedLinks()
		{
			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			AssertEquals(0, campaign.TrackedLinks.Count);
			var link = campaign.TrackedLinks.AddNew();
			AssertEquals(campaign.PK, link.GCL_G0_Campaign);
			AssertEquals(1, campaign.TrackedLinks.Count);

			var link2 = Factory.NewWithValidTestData<GlbCompanyCampaignLink>();
			link2.GCL_G0_Campaign = campaign.PK;
			AssertEquals(2, campaign.TrackedLinks.Count);
		}

		#endregion

		#region UnsubscribedCollection

		public void TestUnsubscribedCollection()
		{
			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			var campaignOther = Factory.NewWithValidTestData<GlbCompanyCampaign>();

			var unsubscribe1 = Factory.NewWithValidTestData<GlbCompanyCampaignSubscription>();
			unsubscribe1.GCS_G0 = campaign.PK;
			var unsubscribe2 = Factory.NewWithValidTestData<GlbCompanyCampaignSubscription>();
			unsubscribe2.GCS_G0 = campaign.PK;
			var unsubscribe3 = Factory.NewWithValidTestData<GlbCompanyCampaignSubscription>();
			unsubscribe3.GCS_G0 = campaignOther.PK;

			AssertEquals(2, campaign.UnsubscribedCollection.Count);
			AssertCollectionContains(unsubscribe1, campaign.UnsubscribedCollection);
			AssertCollectionContains(unsubscribe2, campaign.UnsubscribedCollection);

			AssertEquals(1, campaignOther.UnsubscribedCollection.Count);
			AssertCollectionContains(unsubscribe3, campaignOther.UnsubscribedCollection);
		}

		#endregion

		#region Default values on constructor

		public void TestConstructorSetsDefaultContactDataSource()
		{
			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			AssertEquals("Default data source (ClientIntelligence) is set on new campaign", ContactDataSourceList.Codes.ClientIntelligence, campaign.ContactDataSource);
			campaign.ContactDataSource = ContactDataSourceList.Codes.CampaignTracking;
			Factory.Save();
			AssertEquals(ContactDataSourceList.Codes.CampaignTracking, campaign.ContactDataSource);

			var reloadedCampaign = new BusinessObjectFactory().Load<GlbCompanyCampaign>(campaign.PK);
			AssertEquals("Default data source (ClientIntelligence) is set on existing campaign", ContactDataSourceList.Codes.ClientIntelligence, reloadedCampaign.ContactDataSource);
		}

		#endregion

		public void TestContactDataSource()
		{
			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			campaign.ContactDataSource = "AAA";
			AssertEquals("Invalid value will not be accepted", ContactDataSourceList.Codes.ClientIntelligence, campaign.ContactDataSource);
			campaign.ContactDataSource = ContactDataSourceList.Codes.CampaignTracking;
			AssertEquals("Valid value will be accepted", ContactDataSourceList.Codes.CampaignTracking, campaign.ContactDataSource);
			campaign.ContactDataSource = "";
			AssertEquals("Invalid value will not be accepted", ContactDataSourceList.Codes.ClientIntelligence, campaign.ContactDataSource);
			campaign.ContactDataSource = ContactDataSourceList.Codes.Inquiries;
			AssertEquals("Valid value will be accepted", ContactDataSourceList.Codes.Inquiries, campaign.ContactDataSource);
			campaign.ContactDataSource = ContactDataSourceList.Codes.ClientIntelligence;
			AssertEquals("Valid value will be accepted", ContactDataSourceList.Codes.ClientIntelligence, campaign.ContactDataSource);
		}

		public void TestSourceCampaignPK()
		{
			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			campaign.SourceCampaignPK = campaign.PK;
			AssertEquals(campaign.PK, campaign.SourceCampaignPK);
		}

		public void TestContactDataSourceForTouch()
		{
			SetupDripCampaign();

			AssertEquals(ContactDataSourceList.Codes.CampaignTracking, touch1a.ContactDataSource);

			touch1a.ContactDataSource = ContactDataSourceList.Codes.ClientIntelligence;
			AssertEquals(ContactDataSourceList.Codes.CampaignTracking, touch1a.ContactDataSource);

			touch1a.ContactDataSource = ContactDataSourceList.Codes.Inquiries;
			AssertEquals(ContactDataSourceList.Codes.CampaignTracking, touch1a.ContactDataSource);

			touch1a.ContactDataSource = "";
			AssertEquals(ContactDataSourceList.Codes.CampaignTracking, touch1a.ContactDataSource);
		}

		public void TestG0_GS_NKCampaignManager_ReadOnly()
		{
			Env.Security.CampaignManagementEditModifyStaffAssignment.IsAllowed = true;
			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			AssertEquals(false, campaign.G0_GS_NKCampaignManager_ReadOnly);

			Env.Security.CampaignManagementEditModifyStaffAssignment.IsAllowed = false;
			AssertEquals(false, campaign.G0_GS_NKCampaignManager_ReadOnly);

			Factory.Save();

			Env.Security.CampaignManagementEditModifyStaffAssignment.IsAllowed = true;
			AssertEquals(false, campaign.G0_GS_NKCampaignManager_ReadOnly);

			Env.Security.CampaignManagementEditModifyStaffAssignment.IsAllowed = false;
			AssertEquals(true, campaign.G0_GS_NKCampaignManager_ReadOnly);
		}

		public void TestG0_GS_NKCampaignCoordinator_ReadOnly()
		{
			Env.Security.CampaignManagementEditModifyStaffAssignment.IsAllowed = true;
			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			AssertEquals(false, campaign.G0_GS_NKCampaignCoordinator_ReadOnly);

			Env.Security.CampaignManagementEditModifyStaffAssignment.IsAllowed = false;
			AssertEquals(false, campaign.G0_GS_NKCampaignCoordinator_ReadOnly);

			Factory.Save();

			Env.Security.CampaignManagementEditModifyStaffAssignment.IsAllowed = true;
			AssertEquals(false, campaign.G0_GS_NKCampaignCoordinator_ReadOnly);

			Env.Security.CampaignManagementEditModifyStaffAssignment.IsAllowed = false;
			AssertEquals(true, campaign.G0_GS_NKCampaignCoordinator_ReadOnly);
		}

		public void TestG0_IsSalesAndMarketing_ReadOnly()
		{
			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			AssertEquals(true, campaign.G0_IsSalesAndMarketing_ReadOnly);
		}

		public void TestRelatedLayoutsShouldBeDeletedWhenDeletingCampaign()
		{
			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			var campaignPk = campaign.PK;
			var filter = Factory.New<StmModuleFilter>();
			filter.S9_FilterName = "";
			filter.S9_ModuleID = "GlbCompanyCampaignContact";
			filter.S9_GC = Env.CurrentCompanyPK;
			filter.S9_RelatedEntityID = campaign.PK;
			Factory.Save();

			AssertEquals(1, Factory.Load<StmModuleFilter>(new ZQuery(StmModuleFilterSchema.S9_RelatedEntityID, campaignPk)).Length);

			campaign.Delete();
			Factory.Save();

			AssertEquals(0, Factory.Load<StmModuleFilter>(new ZQuery(StmModuleFilterSchema.S9_RelatedEntityID, campaignPk)).Length);
		}

		public void TestHasPreviousTouches()
		{
			var masterCampaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			masterCampaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.InsideSales;

			var touch1A = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			touch1A.G0_HorizontalId = 1;
			touch1A.G0_VerticalId = "A";
			touch1A.G0_G0_Master = masterCampaign.PK;
			touch1A.G0_BroadcastVoteSurveyExam = InsideSalesTouchTypeList.Codes.PreApproachEmail;
			masterCampaign.AllTouches.Add(touch1A);

			var touch1B = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			touch1B.G0_HorizontalId = 1;
			touch1B.G0_VerticalId = "B";
			touch1B.G0_G0_Master = masterCampaign.PK;
			touch1B.G0_BroadcastVoteSurveyExam = InsideSalesTouchTypeList.Codes.PreApproachEmail;
			masterCampaign.AllTouches.Add(touch1B);

			var touch2A = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			touch2A.G0_HorizontalId = 2;
			touch2A.G0_VerticalId = "A";
			touch2A.G0_G0_Master = masterCampaign.PK;
			touch2A.G0_BroadcastVoteSurveyExam = InsideSalesTouchTypeList.Codes.OpportunityCreation;
			masterCampaign.AllTouches.Add(touch2A);

			var touch2B = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			touch2B.G0_HorizontalId = 2;
			touch2B.G0_VerticalId = "B";
			touch2B.G0_G0_Master = masterCampaign.PK;
			touch2B.G0_BroadcastVoteSurveyExam = InsideSalesTouchTypeList.Codes.OpportunityCreation;
			masterCampaign.AllTouches.Add(touch2B);

			var touch2C = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			touch2C.G0_HorizontalId = 2;
			touch2C.G0_VerticalId = "C";
			touch2C.G0_G0_Master = masterCampaign.PK;
			touch2C.G0_BroadcastVoteSurveyExam = InsideSalesTouchTypeList.Codes.OpportunityCreation;
			masterCampaign.AllTouches.Add(touch2C);

			var touch3A = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			touch3A.G0_HorizontalId = 3;
			touch3A.G0_VerticalId = "A";
			touch3A.G0_G0_Master = masterCampaign.PK;
			touch3A.G0_BroadcastVoteSurveyExam = InsideSalesTouchTypeList.Codes.OpportunityCreation;
			masterCampaign.AllTouches.Add(touch3A);
			Factory.Save();

			Assert(!masterCampaign.HasPreviousTouches);
			Assert(!touch1A.HasPreviousTouches);
			Assert(!touch1B.HasPreviousTouches);
			Assert(touch2A.HasPreviousTouches);
			Assert(touch2B.HasPreviousTouches);
			Assert(touch2C.HasPreviousTouches);
			Assert(touch3A.HasPreviousTouches);
		}

		public void TestRecalculateItemsStandAloneCampaign()
		{
			var coordinator = Factory.NewWithValidTestData<GlbStaff>();
			var unverifiedSender = Factory.NewWithValidTestData<GlbStaff>();

			var standAloneCampaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			standAloneCampaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.Broadcast;
			standAloneCampaign.G0_GS_NKCampaignCoordinator = coordinator.GS_Code;
			standAloneCampaign.G0_EmailSenderOption = EmailSenderOptionCodeDescriptionList.Codes.SPS;

			var poolItem1 = GetGlbCompanyCampaignSenderPoolItem(standAloneCampaign);
			var poolItem2 = GetGlbCompanyCampaignSenderPoolItem(standAloneCampaign);

			var queuedItem1 = CreateCampaignItem(standAloneCampaign, TrackingStatusCodes.Codes.QUE);
			var queuedItem2 = CreateCampaignItem(standAloneCampaign, TrackingStatusCodes.Codes.QUE);
			var queuedItem3 = CreateCampaignItem(standAloneCampaign, TrackingStatusCodes.Codes.QUE);
			var queuedItem4 = CreateCampaignItem(standAloneCampaign, TrackingStatusCodes.Codes.QUE);
			var unverifiedItem1 = CreateCampaignItem(standAloneCampaign, TrackingStatusCodes.Codes.UNV);
			unverifiedItem1.G8_GS_NKSender = unverifiedSender.GS_Code;

			standAloneCampaign.RecalculateItemsStandAloneCampaign();
			var campaignItems = standAloneCampaign.CampaignsItemsSent.Cast<GlbCompanyCampaignItem>().ToList();

			AssertEquals(poolItem1.GCP_GS_NKSender, campaignItems.First(i => i.PK == queuedItem1.PK).G8_GS_NKSender);
			AssertEquals(poolItem2.GCP_GS_NKSender, campaignItems.First(i => i.PK == queuedItem2.PK).G8_GS_NKSender);
			AssertEquals(poolItem1.GCP_GS_NKSender, campaignItems.First(i => i.PK == queuedItem3.PK).G8_GS_NKSender);
			AssertEquals(poolItem2.GCP_GS_NKSender, campaignItems.First(i => i.PK == queuedItem4.PK).G8_GS_NKSender);
			AssertEquals(unverifiedItem1.G8_GS_NKSender, campaignItems.First(i => i.PK == unverifiedItem1.PK).G8_GS_NKSender);

			standAloneCampaign.G0_EmailSenderOption = EmailSenderOptionCodeDescriptionList.Codes.COR;
			standAloneCampaign.RecalculateItemsStandAloneCampaign();
			campaignItems = standAloneCampaign.CampaignsItemsSent.Cast<GlbCompanyCampaignItem>().ToList();

			AssertEquals(coordinator.GS_Code, campaignItems.First(i => i.PK == queuedItem1.PK).G8_GS_NKSender);
			AssertEquals(coordinator.GS_Code, campaignItems.First(i => i.PK == queuedItem2.PK).G8_GS_NKSender);
			AssertEquals(coordinator.GS_Code, campaignItems.First(i => i.PK == queuedItem3.PK).G8_GS_NKSender);
			AssertEquals(coordinator.GS_Code, campaignItems.First(i => i.PK == queuedItem4.PK).G8_GS_NKSender);
			AssertEquals(unverifiedItem1.G8_GS_NKSender, campaignItems.First(i => i.PK == unverifiedItem1.PK).G8_GS_NKSender);

			GlbCompanyCampaignItem CreateCampaignItem(GlbCompanyCampaign campaign, string trackingStatus)
			{
				var item = campaign.CampaignsItemsSent.AddNew();
				item.G8_TrackingStatus = trackingStatus;
				item.G8_ScheduleTimeUtc = ZDateTime.UtcNow;

				return item;
			}

			GlbCompanyCampaignSenderPoolItem GetGlbCompanyCampaignSenderPoolItem(GlbCompanyCampaign campaign)
			{
				var poolItem = campaign.SenderPool.AddNew();
				var glbStaff = Factory.NewWithValidTestData<GlbStaff>();
				glbStaff.GS_EmailAddress = $"{nameof(glbStaff)}@ema.il";
				poolItem.GCP_GS_NKSender = glbStaff.GS_Code;

				return poolItem;
			}
		}

		public void TestPublishedListCodeDescription()
		{
			var rules = new SubscriptionRuleCollection();
			rules.AddNewRule("CD1", (NoResString)"Description 1", false, false, new string[] { "PRINT;SLT30;DESC1;SUM1" });
			rules.AddNewRule("CD2", (NoResString)"Description 2", false, false, new string[] { "TELEV;EXIST;DESC1;SUM1" });
			rules.AddNewRule("CDD", (NoResString)"Default Description", true, false, new string[] { "PRINT;EXIST;DESC2;SUM2" });

			OrganisationsDataRegistry.Instance.SubscriptionRules.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, rules);
			var campaign = Factory.New<GlbCompanyCampaign>();

			campaign.G0_PublishedListCode = "";
			AssertEquals("Should retrieve default rule description if no code is set", "Default Description", campaign.PublishedListCodeDescription);
			AssertEquals("Code should be set to default when accessing PublishedListCodeDescription if code was unset", "CDD", campaign.G0_PublishedListCode);

			campaign.G0_PublishedListCode = "CD2";
			AssertEquals("Should retrieve correct description from code", "Description 2", campaign.PublishedListCodeDescription);
			AssertEquals("Code should be unchanged if it was set", "CD2", campaign.G0_PublishedListCode);

			campaign.G0_PublishedListCode = "INV";
			AssertEquals("Should retrieve default rule description if invalid code is set", "Default Description", campaign.PublishedListCodeDescription);
			AssertEquals("Code should be set to default when accessing PublishedListCodeDescription if code was invalid", "CDD", campaign.G0_PublishedListCode);

			campaign.PublishedListCodeDescription = "Description 1";
			AssertEquals("Should set associated code when setting description", "CD1", campaign.G0_PublishedListCode);

			OrganisationsDataRegistry.Instance.SubscriptionRules.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new SubscriptionRuleCollection());
			campaign.G0_PublishedListCode = "";
			AssertEquals("Should return empty description if code was unset and default could not be retrieved", "", campaign.PublishedListCodeDescription);
			AssertEquals("Code should be unchanged if default could not be retrieved", "", campaign.G0_PublishedListCode);
		}

		protected virtual Type ExpectedGlbCompanyCampaignType
		{
			get { return typeof(GlbCompanyCampaign); }
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			GlbCompanyCampaign campaign = (GlbCompanyCampaign)base.GetNewBusinessObjectForDeleteTest(factory);
			campaign.Questions.DeleteAll();
			return campaign;
		}

		GlbCompanyCampaign Campaign
		{
			get { return (GlbCompanyCampaign)CachedBusinessObject; }
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return Factory.NewWithValidTestData(GetExpectedBusinessObjectType());
		}

		[SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Testing")]
		OrgContact CreateContact(ZString name, OrgHeader org)
		{
			OrgContact contact = org.Contacts.AddNew();
			contact.OC_OH = org.PK;
			contact.OC_ContactName = name;
			contact.OC_Email = "default@cargowise.com";
			contact.OC_Phone = GlbCompanyCampaignForTest.PhoneFilterForLoadingLessObjects;
			return contact;
		}

		[SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Testing")]
		void AssertJobTitleFilter(GlbCompanyCampaignForTest campaign, OrgContact contact1, OrgContact contact2,
			OrgContact contact3, SchemaStringColumn jobTitleColumn1, SchemaStringColumn jobTitleColumn2)
		{
			GlbCampaignContactCollection campaignContactCollection = Testing.GlbCompanyCampaignSenderTest.ContactCollection(new List<BusinessObject>() { contact1, contact2, contact3 }, campaign);

			campaign.LoadFilteredContacts(campaignContactCollection);

			AssertCollectionContains("Filter for all job title", contact1.PK, campaignContactCollection.GetPKs());
			AssertCollectionContains("Filter for all job title", contact2.PK, campaignContactCollection.GetPKs());
			AssertCollectionContains("Filter for all job title", contact3.PK, campaignContactCollection.GetPKs());

			campaign[jobTitleColumn1.Name] = "***";
			campaign.LoadFilteredContacts(campaignContactCollection);
			AssertCollectionContains("Filter for *** job title", contact1.PK, campaignContactCollection.GetPKs());
			AssertCollectionNotContains("Filter for *** job title", contact2.PK, campaignContactCollection.GetPKs());
			AssertCollectionNotContains("Filter for *** job title", contact3.PK, campaignContactCollection.GetPKs());

			campaign[jobTitleColumn1.Name] = ";;";
			campaign.LoadFilteredContacts(campaignContactCollection);
			AssertCollectionNotContains("Filter for ;; job title", contact1.PK, campaignContactCollection.GetPKs());
			AssertCollectionContains("Filter for ;; job title", contact2.PK, campaignContactCollection.GetPKs());
			AssertCollectionNotContains("Filter for ;; job title", contact3.PK, campaignContactCollection.GetPKs());

			campaign[jobTitleColumn1.Name] = ";;";
			campaign[jobTitleColumn2.Name] = "***";
			campaign.LoadFilteredContacts(campaignContactCollection);
			AssertCollectionContains("Filter for ;; or *** job title", contact1.PK, campaignContactCollection.GetPKs());
			AssertCollectionContains("Filter for ;; or ***  job title", contact2.PK, campaignContactCollection.GetPKs());
			AssertCollectionNotContains("Filter for ;; or *** job title", contact3.PK, campaignContactCollection.GetPKs());

			campaign[jobTitleColumn1.Name] = ZString.Empty; //Clear filter for next test
			campaign[jobTitleColumn2.Name] = ZString.Empty;
		}

		[SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Testing")]
		void AssertUNLOCOFilter(GlbCompanyCampaignForTest campaign, OrgContact aUContact1, OrgContact uSContact2,
			SchemaStringColumn locationColumn1, SchemaStringColumn locationColumn2)
		{
			GlbCampaignContactCollection campaignContactCollection = Testing.GlbCompanyCampaignSenderTest.ContactCollection(new List<BusinessObject>() { aUContact1, uSContact2 }, campaign);
			campaign.LoadFilteredContacts(campaignContactCollection);

			AssertCollectionContains("No filter", aUContact1.PK, campaignContactCollection.GetPKs());
			AssertCollectionContains("No filter", uSContact2.PK, campaignContactCollection.GetPKs());

			campaign[locationColumn1.Name] = "AUEC";
			campaign.LoadFilteredContacts(campaignContactCollection);

			AssertCollectionContains("Filter for AUEC - Contact should be found", aUContact1.PK, campaignContactCollection.GetPKs());
			AssertCollectionNotContains("Filter for AUEC - Contact2 should NOT be found", uSContact2.PK, campaignContactCollection.GetPKs());

			campaign[locationColumn1.Name] = "USLAX";
			campaign.LoadFilteredContacts(campaignContactCollection);

			AssertCollectionNotContains("Filter for USLAX - Contact should NOT be found", aUContact1.PK, campaignContactCollection.GetPKs());
			AssertCollectionContains("Filter for USLAX - Contact2 should be found", uSContact2.PK, campaignContactCollection.GetPKs());

			campaign[locationColumn1.Name] = "AU";
			campaign.LoadFilteredContacts(campaignContactCollection);

			AssertCollectionContains("Filter for AU - Contact should be found", aUContact1.PK, campaignContactCollection.GetPKs());
			AssertCollectionNotContains("Filter for AU - Contact2 should NOT be found", uSContact2.PK, campaignContactCollection.GetPKs());

			campaign[locationColumn1.Name] = "AU";
			campaign[locationColumn2.Name] = "USLAX";
			campaign.LoadFilteredContacts(campaignContactCollection);

			AssertCollectionContains("Filter for AU or USLAX - Contact should be found", aUContact1.PK, campaignContactCollection.GetPKs());
			AssertCollectionContains("Filter for AU or USLAX - Contact2 should be found", uSContact2.PK, campaignContactCollection.GetPKs());

			campaign[locationColumn1.Name] = ZString.Empty;     //Clear filter for next test
			campaign[locationColumn2.Name] = ZString.Empty;
		}

		#region Test Objects

		public class GlbCompanyCampaignForTest : GlbCompanyCampaign
		{
			public const string PhoneFilterForLoadingLessObjects = "000000000000000";
			public GlbCompanyCampaignForTest(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
				FilterToStopLoadingAllContacts = new ZQuery(ViewCampaignContactSchema.VCC_Phone, PhoneFilterForLoadingLessObjects);
			}

			readonly ZQuery FilterToStopLoadingAllContacts;

			protected override ZQuery ContactFilter
			{
				get
				{
					ZQuery filter = base.ContactFilter;
					filter.AddToFilter(FilterToStopLoadingAllContacts);
					return filter;
				}
			}
		}

		public class FakeFormForTest
		{
			public FakeFormForTest(GlbCompanyCampaignForTest campaign)
			{
			}

			#region TooManyResultsInFilter

			public void Campaign_TooManyResultsInFilter(ZString errorMessage)
			{
				this.ErrorMessage = errorMessage;
			}

			public ZString ErrorMessage;

			#endregion

			#region MessageOnCampaignSending

			public void Campaign_MessageOnCampaignSending(object sender, GlbCompanyCampaignSender.MessageOnCampaignSendingEventArgs e)
			{
				MessageOnCampaignSendingArgs = e;
			}

			public GlbCompanyCampaignSender.MessageOnCampaignSendingEventArgs MessageOnCampaignSendingArgs;

			#endregion

			#region ShouldContinueWithSending

			public bool UserChoiceContinueWithSending;
			public bool Campaign_ShouldContinueWithSending(int numCampaignsToSend, GlbCompanyCampaignItem[] campaignToResend)
			{
				return UserChoiceContinueWithSending;
			}

			#endregion

			#region ContactsNotSentCampaign

			public void Campaign_ContactsNotSentCampaign(int numContactsSent, OrgContactCollection contactsNotSentTo)
			{
				this.NumContactsSent = numContactsSent;
				this.ContactsNotSentTo = contactsNotSentTo;
			}

			public int NumContactsSent;
			public OrgContactCollection ContactsNotSentTo;

			#endregion

		}

		#endregion

		#endregion
	}
}
