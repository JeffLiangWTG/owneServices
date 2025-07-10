using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class CustomLabelInfoTest : TestCaseWithFactory
	{
		public class CustomLabelInfoForTesting : CustomLabelInfo
		{
			public CustomLabelInfoForTesting(string labelName, OrgHeader org)
				: base(labelName, "PropertyName", typeof(ZString), (NoResString)"DefaultCaption", (NoResString)"DefaultHint", CustomLabelStyles.None, org, new BusinessObjectFactory())
			{
			}

			public new OrgCustomLabels Label
			{
				get { return base.Label; }
				set
				{
					fLabel = value;
					hasCalculatedLabel = false;
				}
			}
		}

		public void TestCaptionReturnsDefaultWhenOrgCusLabelDeleted()
		{
			OrgHeader org = Factory.New<OrgHeader>();
			OrgCustomLabels label = org.CustomLabels.AddNew();
			label.OT_FieldName = "label";
			label.OT_Caption = "labelcaption";
			label.OT_Hint = "labelhint";
			CustomLabelInfo info = new CustomLabelInfo("label", "prop", typeof(ZString), (NoResString)"defaultcaption", (NoResString)"defaulthint", CustomLabelStyles.None, org, Factory);

			AssertEquals("labelcaption", info.Caption);
			AssertEquals("labelhint", info.Hint);

			label.Delete();
			AssertEquals("defaultcaption", info.Caption);
			AssertEquals("defaulthint", info.Hint);
		}

		public void TestLabelFromOrgProxy_OrgProxyIsNull()
		{
			var orgProxy = Factory.NewWithValidTestData<OrgHeader>();
			GlbCompany.GetCurrentCompany(Factory).GC_OH_OrgProxy = ZGuid.Empty;
			Factory.Save();
			var labelInfo = new CustomLabelInfoForTesting("TestLabel", null);
			AssertNull("Label not assigned to Org or OrgProxy", labelInfo.Label);
		}

		public void TestLabelFromOrgProxy()
		{
			OrgHeader orgProxy = Factory.NewWithValidTestData<OrgHeader>();
			GlbCompany.GetCurrentCompany(Factory).GC_OH_OrgProxy = orgProxy.PK;
			Factory.Save();
			AssertEquals("CurrentCompany has correct OrgProxy", orgProxy.PK, GlbCompany.CurrentCompany.OrgProxy.PK);
			AssertEquals("CurrentCompany has correct OrgProxy", orgProxy.OH_Code, GlbCompany.CurrentCompany.OrgProxy.OH_Code);

			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();

			CustomLabelInfoForTesting labelInfo = new CustomLabelInfoForTesting("TestLabel", org);
			AssertNull("Label not assigned to Org or OrgProxy", labelInfo.Label);

			OrgCustomLabels proxyLabel = orgProxy.CustomLabels.AddNew();
			proxyLabel.OT_FieldName = "TestLabel";
			Factory.Save();
			labelInfo.Label = null;
			AssertEquals("Label comes from OrgProxy", proxyLabel.PK, labelInfo.Label.PK);

			OrgCustomLabels orgLabel = org.CustomLabels.AddNew();
			orgLabel.OT_FieldName = "TestLabel";
			labelInfo.Label = null;
			AssertEquals("Label comes from Org", orgLabel.PK, labelInfo.Label.PK);

			orgLabel.OT_FieldName = "LabelChange";
			labelInfo.Label = null;
			AssertNull("Label should not fall back to OrgProxy as there is one configuration against Organisation", labelInfo.Label);

			org.CustomLabels.RemoveAndDeleteAll();
			labelInfo.Label = null;
			AssertEquals("Label comes from OrgProxy", proxyLabel.PK, labelInfo.Label.PK);
		}
	}
}
