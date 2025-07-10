using System.Threading.Tasks;
using CargoWise.Application;
using CargoWise.Async;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;

namespace Enterprise.MasterFiles.Module.Testing
{
	sealed class RefPremisesGateCodeModule_Test : TestCaseWithFactory
	{
		public void TestGridCollection()
		{
			AssertEquals("RefPremisesGateCode", Module.GetNewGridCollectionForTest().TypeOfElements.Name);
		}

		public void TestFilterControl()
		{
			AssertEquals("Enterprise.MasterFiles.Module.RefPremisesGateCodeFilterBusinessObject", Module.GetNewFilterBusinessObjectForTest().ToString());
		}

		public void TestFindOrganisationEdit()
		{
			RefPremisesGateCode premisesGateCode = Factory.New<RefPremisesGateCode>();
			premisesGateCode.R5_PremisesGateCode = "1ST";
			premisesGateCode.R5_OrgRegCodeType = "1ST";
			OrgHeader organisation = Factory.New<OrgHeader>();
			OrgCusCode registrationNumber = Factory.New<OrgCusCode>();
			registrationNumber.OK_CustomsRegNo = "1ST";
			registrationNumber.OK_CodeType = "1ST";
			organisation.ConfigOrg.CustomsCodes.Add(registrationNumber);
			organisation.OH_FullName = "Very Test Organization";
			organisation.OH_Code = "VTO";
			Factory.Save();
			Module.ShowRelatedOrganisation(premisesGateCode);
			using (ZOrganisationsForm organisationForm = (ZOrganisationsForm)Module.OrganisationController.LastShownForm)
			{
				AssertEquals(typeof(ZOrganisationsForm), organisationForm.GetType());
				AssertEquals(organisation.PK, organisationForm.BusinessEntity.Identifier);
				AssertEquals("Organization - VTO / Very Test Organization", organisationForm.FormCaption);
			}
		}

		public void TestFindOrganisationNew()
		{
			ObjectFactory.Substitute<TaskScheduler>(new SynchronousTaskSchedulerForTest());

			RefPremisesGateCode premisesGateCode = Factory.New<RefPremisesGateCode>();
			premisesGateCode.R5_PremisesGateCode = "1ST";
			premisesGateCode.R5_OrgRegCodeType = "1ST";
			OrgHeader organisation = Factory.New<OrgHeader>();
			OrgCusCode registrationNumber = Factory.New<OrgCusCode>();
			registrationNumber.OK_CustomsRegNo = "1ST";
			registrationNumber.OK_CodeType = "";
			organisation.ConfigOrg.CustomsCodes.Add(registrationNumber);
			organisation.OH_FullName = "New Very Test Organization";
			organisation.OH_Code = "VTO";
			Factory.Save();
			Module.ShowRelatedOrganisation(premisesGateCode);
			using (ZOrganisationsForm organisationForm = (ZOrganisationsForm)Module.OrganisationController.LastShownForm)
			{
				AssertEquals(typeof(ZOrganisationsForm), organisationForm.GetType());
				AssertNotEquals(organisation.PK, organisationForm.BusinessEntity.Identifier);
				AssertEquals("Organization", organisationForm.FormCaption);
			}
		}

		RefPremisesGateCodeModuleForTest Module
		{
			get
			{
				if (fModule == null)
				{
					fModule = new RefPremisesGateCodeModuleForTest();
				}
				return fModule;
			}
		}

		RefPremisesGateCodeModuleForTest fModule;

		protected override void TearDown()
		{
			base.TearDown();
			if (fModule != null)
			{
				fModule.Dispose();
			}
		}
	}
}
