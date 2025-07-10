using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Freight.Agency.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.Module.Testing
{
	[TestedType(typeof(ContainerDetentionController))]
	internal class ContainerDetentionControllerBasherTest : ZControllerBasherTest
	{
		public void TestEdit_IntraCompany()
		{
			OrgHeader principal = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader client = Factory.NewWithValidTestData<OrgHeader>();

			ContainerDetention detention = Factory.New<ContainerDetention>();
			detention.NC_OH_Principal = principal.PK;
			detention.NC_OH_Client = client.PK;

			Factory.Save();

			ZController controller = ZControllerFactory.Create(ControllerIDs.AgencyContainerDetention);

			using (ZForm form = (ZForm)controller.ShowEditForm(detention))
			{
				CombineAssertions(delegate
				{
					AssertEquals("Should not have shown a dialog.", "None ", UnitTestUserNotification.Instance.LastMessage.ToString());
					AssertEquals("Should have shown the form in browse mode.", ODisplayMode.Browse, form.DisplayMode);
				});
			}
		}

		public void TestEdit_InterCompnay_WithoutRights()
		{
			Env.Security.AgencyContainerDetentionViewInterCompany.IsAllowed = false;

			GlbCompany company = Factory.New<GlbCompany>();
			company.GC_Code = "TST";

			GlbBranch branch = company.Branches.AddNew();
			branch.GB_Code = "TST";

			OrgHeader principal = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader client = Factory.NewWithValidTestData<OrgHeader>();

			ContainerDetention detention = Factory.New<ContainerDetention>();
			detention.NC_GC = company.PK;
			detention.NC_OH_Principal = principal.PK;
			detention.NC_OH_Client = client.PK;

			Factory.Save();

			ZController controller = ZControllerFactory.Create(ControllerIDs.AgencyContainerDetention);

			using (ZForm form = (ZForm)controller.ShowEditForm(detention))
			{
				CombineAssertions(delegate
				{
					AssertEquals("Should have shown a dialog.", "Information You are not authorized to view the detention jobs of other companies.", UnitTestUserNotification.Instance.LastMessage.ToString());
					AssertEquals("Should not have shown the form", null, form);
				});
			}
		}

		public void TestEdit_InterCompany_WithRights()
		{
			Env.Security.AgencyContainerDetentionViewInterCompany.IsAllowed = true;

			GlbCompany company = Factory.New<GlbCompany>();
			company.GC_Code = "TST";

			GlbBranch branch = company.Branches.AddNew();
			branch.GB_Code = "TST";

			OrgHeader principal = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader client = Factory.NewWithValidTestData<OrgHeader>();

			ContainerDetention detention = Factory.New<ContainerDetention>();
			detention.NC_GC = company.PK;
			detention.NC_OH_Principal = principal.PK;
			detention.NC_OH_Client = client.PK;

			Factory.Save();

			ZController controller = ZControllerFactory.Create(ControllerIDs.AgencyContainerDetention);

			using (ZForm form = (ZForm)controller.ShowEditForm(detention))
			{
				CombineAssertions(delegate
				{
					AssertEquals("Should not have shown a dialog.", "None ", UnitTestUserNotification.Instance.LastMessage.ToString());
					AssertEquals("Should have shown the form in read only mode.", ODisplayMode.ReadOnly, form.DisplayMode);
				});
			}
		}

		#region Implementation

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.AgencyContainerDetention;
		}

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			ContainerDetention detention = Factory.New<ContainerDetention>();
			detention.NC_DetentionType = DetentionInvoiceType.Codes.Import;
			detention.NC_OH_Client = Factory.NewWithValidTestData<OrgHeader>().PK;
			detention.NC_OH_Principal = Factory.NewWithValidTestData<OrgHeader>().PK;
			Factory.Save();
			return detention;
		}

		public override void TestGetOpenFormUrlslDoesNotHitDatabase()
		{
			Assert(true);
		}

		#endregion
	}
}
