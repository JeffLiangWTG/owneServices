using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgCustomLabels))]
	sealed class OrgCustomLabelsTest : EnterpriseBusinessObjectTestCase
	{
		#region ReadOnly Security

		public void TestReadOnlySecurity()
		{
			bool oldCustomValue = Env.Security.OrgCustomModify.IsAllowed;

			try
			{
				OrgCustomLabels testLabel = OrgInDB.CustomLabels.AddNew();

				Env.Security.OrgCustomModify.IsAllowed = true;
				Assert("Access Allowed - Not ReadOnly", !testLabel.OT_CaptionInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !testLabel.OT_ColumnSizeInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !testLabel.OT_FieldNameInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !testLabel.OT_HintInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !testLabel.OT_IsMandatoryInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !testLabel.OT_PositionInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !testLabel.OT_RuleInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !testLabel.OT_TypeInfo.ReadOnly);

				Env.Security.OrgCustomModify.IsAllowed = false;
				Assert("Access NOT Allowed - ReadOnly", testLabel.OT_CaptionInfo.ReadOnly);
				Assert("Access NOT Allowed - ReadOnly", testLabel.OT_ColumnSizeInfo.ReadOnly);
				Assert("Access NOT Allowed - ReadOnly", testLabel.OT_FieldNameInfo.ReadOnly);
				Assert("Access NOT Allowed - ReadOnly", testLabel.OT_HintInfo.ReadOnly);
				Assert("Access NOT Allowed - ReadOnly", testLabel.OT_IsMandatoryInfo.ReadOnly);
				Assert("Access NOT Allowed - ReadOnly", testLabel.OT_PositionInfo.ReadOnly);
				Assert("Access NOT Allowed - ReadOnly", testLabel.OT_RuleInfo.ReadOnly);
				Assert("Access NOT Allowed - ReadOnly", testLabel.OT_TypeInfo.ReadOnly);
			}
			finally
			{
				Env.Security.OrgCustomModify.IsAllowed = oldCustomValue;
			}
		}

		OrgHeader OrgInDB
		{
			get
			{
				if (fOrgInDB == null)
				{
					ResetOrgInDB();
				}

				return fOrgInDB;
			}
		}
		OrgHeader fOrgInDB;

		void ResetOrgInDB()
		{
			fOrgInDB = new BusinessObjectFactory().LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, "DEMORG");
		}

		#endregion

		public void TestLogging()
		{
			Label.Delete();
			LCGroup1InRegistry.Delete();

			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			OrgLandedCostingPrefs preference = org.LandedCostingPreferences.AddNew();
			OrgCustomLabels customLabel = org.CustomDocumentLabels.AddNew();

			customLabel.OT_FieldName = "PPP";
			customLabel.OT_Caption = "LLL";

			Factory.Save();

			AssertEquals("Logs count", 1, customLabel.Logs.GetAllLogs().Count);
			ZString expectedReference = "Label Field Name: PPP Caption: LLL";
			AssertEquals("Log should have reference", expectedReference, customLabel.Logs.MostRecentLogByEventTime(Events.AddedARecordToTheSystem).SL_Reference);

			customLabel.OT_FieldName = "OOO";
			customLabel.OT_Caption = "MMM";

			Factory.Save();

			AssertEquals("Logs count", 2, customLabel.Logs.GetAllLogs().Count);
			expectedReference = "Label Field Name: OOO(PPP) Caption: MMM(LLL)";
			AssertEquals("Log should have reference", expectedReference, customLabel.Logs.MostRecentLogByEventTime(Events.EditedARecord).SL_Reference);
		}

		public override void TestSaveAndDeleteBusinessObject()
		{
			Assert("This should be implemented if a client has an issue with deleting", true);
		}

		public void TestDefaultCaptionOnLandedCostGroupOnParentOrg()
		{
			OrgHeader org = Factory.New<OrgHeader>();
			OrgLandedCostingPrefs preference = org.LandedCostingPreferences.AddNew();
			preference.O9_LandedCostGroup = 1;
			preference.O9_LandedCostGroupName = "GROUP1";

			OrgCustomLabels customsLabel = org.CustomDocumentLabels.AddNew();
			customsLabel.OT_FieldName = LandedCostingCustomDocumentLabelsList.Codes.LandedCostGroup1;
			AssertEquals("Caption should be defaulted from Org Preference", preference.O9_LandedCostGroupName, customsLabel.OT_Caption);

			customsLabel.OT_FieldName = LandedCostingCustomDocumentLabelsList.Codes.LandedCostGroupMisc;
			AssertEquals("Caption should be defaulted", "Misc Charges", customsLabel.OT_Caption);
		}

		public void TestDefaultCaptionOnLandedCostGroupOnRegistry()
		{
			OrgHeader org = Factory.New<OrgHeader>();
			AssertEquals("Does not have any preference", 0, org.LandedCostingPreferences.Count);
			OrgCustomLabels customsLabel = org.CustomDocumentLabels.AddNew();
			customsLabel.OT_FieldName = LandedCostingCustomDocumentLabelsList.Codes.LandedCostGroup1;

			AssertEquals("Caption should be defaulted from Registry Preference", LCGroup1InRegistry.GroupName, customsLabel.OT_Caption);
		}

		public void TestDefaultCaption()
		{
			OrgHeader org = Factory.New<OrgHeader>();
			OrgCustomLabels customLabel = org.CustomDocumentLabels.AddNew();
			customLabel.OT_FieldName = LandedCostingCustomDocumentLabelsList.ExtraCodes.SpecialTax1;
			AssertEquals("Caption should be defaulted from Lookup List", customLabel.Lookups.OT_FieldName_List.GetDescriptionFromCode(LandedCostingCustomDocumentLabelsList.ExtraCodes.SpecialTax1), customLabel.OT_Caption);
		}

		public void TestTypeDecider()
		{
			AssertType<OrgCustomLabelsTypeDecider>(OrgCustomLabels.TypeDecider);
		}

		#region Validation
		public void TestValidateOT_Caption()
		{
			Label.OT_Type = OrgConstants.CustomLabelType.Form;
			Label.OT_Caption = "Hi";
			Assert("Invalid caption error expected", Label.OT_CaptionInfo.HasErrors());

			Label.OT_Type = OrgConstants.CustomLabelType.Form;
			Label.OT_Caption = "";
			Assert("Invalid caption error expected", Label.OT_CaptionInfo.HasErrors());

			Label.OT_Type = OrgConstants.CustomLabelType.Form;
			Label.OT_Caption = "Hello";
			Assert("Valid caption no error expected", !Label.OT_CaptionInfo.HasErrors());

			Label.OT_Type = OrgConstants.CustomLabelType.Document;
			Label.OT_Caption = "";
			Assert("Valid caption no error expected", !Label.OT_CaptionInfo.HasErrors());

			Label.OT_Type = OrgConstants.CustomLabelType.Report;
			Label.OT_Caption = "";
			Assert("Valid caption no error expected", !Label.OT_CaptionInfo.HasErrors());

			Label.OT_Type = OrgConstants.CustomLabelType.Report;
			Label.OT_Caption = "1";
			Assert("Invalid caption error expected", Label.OT_CaptionInfo.HasErrors());
		}
		#endregion

		#region Implementation
		OrgCustomLabels Label;

		LandedCostingGroup LCGroup1InRegistry;

		protected override void SetUp()
		{
			base.SetUp();
			Label = Factory.New<OrgCustomLabels>();

			LandedCostingGroupCollection collection = new LandedCostingGroupCollection();
			LCGroup1InRegistry = collection.AddNew();
			LCGroup1InRegistry.GroupID = 1;
			LCGroup1InRegistry.GroupName = "Group 1";
			LCGroup1InRegistry.CostDistributionCode = "VOL";
			FreightDataRegistry.Instance.LandedCostingPreferences.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, collection);
		}

		#endregion
	}
}
