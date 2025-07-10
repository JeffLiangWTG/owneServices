using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business.Testing
{
	class CustomLabelPropertyValidationTest : TestCaseWithFactory
	{
		public void TestValidate()
		{
			CustomLabelPropertyValidation validation = new CustomLabelPropertyValidation(Factory);
			OrgHeader bizOWithCustomLabels = Factory.New<OrgHeader>();

			using (bizOWithCustomLabels.SuspendValidationTesting())
			using (bizOWithCustomLabels.MainAddress.SuspendValidationTesting())
			{
				bizOWithCustomLabels.MainAddress.OA_Email = "";
				bizOWithCustomLabels.MainAddress.OA_Fax = "";
				OrgCustomLabels label1 = bizOWithCustomLabels.CustomLabels.AddNew();
				label1.OT_FieldName = OrgAddressSchema.OA_Email.Name;
				label1.OT_IsMandatory = true;
				OrgCustomLabels label2 = bizOWithCustomLabels.CustomLabels.AddNew();
				label2.OT_FieldName = OrgAddressSchema.OA_Fax.Name;
				label2.OT_IsMandatory = false;
				validation.Validate(new MockOrgCustomLabelsProvider(new MockCustomLabelsConfigOrgProvider(bizOWithCustomLabels)), bizOWithCustomLabels.MainAddress.OA_EmailInfo);
				validation.Validate(new MockOrgCustomLabelsProvider(new MockCustomLabelsConfigOrgProvider(bizOWithCustomLabels)), bizOWithCustomLabels.MainAddress.OA_FaxInfo);

				AssertEquals("MainAddress.OA_Email should have errors as it IS mandatory", true, bizOWithCustomLabels.MainAddress.OA_EmailInfo.HasErrors());
				AssertEquals("MainAddress.OA_Fax should NOT have errors as it is not mandatory", false, bizOWithCustomLabels.MainAddress.OA_FaxInfo.HasErrors());
			}
		}

		#region Implementation

		protected class MockCustomLabelsConfigOrgProvider : ICustomLabelsConfigOrgProvider
		{
			public MockCustomLabelsConfigOrgProvider(OrgHeader configOrg)
			{
				this.fConfigOrg = configOrg;
			}

			readonly OrgHeader fConfigOrg;
			public OrgHeader ConfigOrg
			{
				get { return fConfigOrg; }
			}

			public event EventHandler ConfigOrgChanged
			{
				add { }
				remove { }
			}

			public BusinessObjectFactory Factory
			{
				get { return ConfigOrg.Factory; }
			}
		}

		protected class MockOrgCustomLabelsProvider : ICustomLabelsProvider
		{
			public MockOrgCustomLabelsProvider(ICustomLabelsConfigOrgProvider configOrgProvider)
			{
				this.fConfigOrgProvider = configOrgProvider;
			}

			public ICustomLabelsConfigOrgProvider ConfigOrgProvider
			{
				get { return fConfigOrgProvider; }
			}

			public CustomLabelInfoList GetCustomFields(OrgHeader configOrg, BusinessObjectFactory factory)
			{
				var result = new CustomLabelInfoList(typeof(OrgAddress), configOrg, (NoResString)"", factory);
				result.Add(OrgAddressSchema.OA_Email.Name, OrgAddressSchema.OA_Email.Name, (NoResString)"Some Custom Label", (NoResString)"Hint");
				result.Add(OrgAddressSchema.OA_Fax.Name, OrgAddressSchema.OA_Fax.Name, (NoResString)"Some Custom Label", (NoResString)"Hint");
				return result;
			}

			protected ICustomLabelsConfigOrgProvider fConfigOrgProvider;
		}

		#endregion
	}
}
