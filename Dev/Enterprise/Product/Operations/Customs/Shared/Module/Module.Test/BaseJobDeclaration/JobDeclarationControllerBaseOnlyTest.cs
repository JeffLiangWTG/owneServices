using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.GUI;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.GUI;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.Module.Testing
{
	sealed class JobDeclarationControllerBaseOnlyTest : TestCaseWithFactory
	{
		public void TestShowEditFormForDecBelongsToDifferentCompany()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_GC = company.PK;
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_GB = branch.PK;
			Factory.Save();
			var controller = new JobDeclarationController();
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			AssertNull(controller.ShowEditForm(declaration));
			AssertNull(controller.ShowViewForm(declaration));
			var message = UnitTestUserNotification.Instance.LastMessage;
			AssertContains("You are trying to view a declaration that belongs to a different company. Please log into the company", message.Text);
		}

		public void TestSkipRecentItems()
		{
			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			Factory.Save();

			var controller = new JobDeclarationController();
			try
			{
				var form = (BaseJobDeclarationForm)controller.ShowEditForm(declaration);
				AssertNull(form.SkipRecentItems);

				OpenedFormCache.GetInstance().CloseAllCachedForms();
				form = (BaseJobDeclarationForm)controller.ShowEditForm(declaration, true);
				AssertNotNull(form.SkipRecentItems);
				Assert(form.SkipRecentItems.GetValueOrDefault());
			}
			finally
			{
				UserIdleWorker.Flush();

				if (controller.LastShownForm is IZForm lastShownForm)
				{
					lastShownForm.Dispose();
				}
			}
		}

		public void TestOpenDeclarationFromRecentItemHasSameDisplayModeWhenOpenFromDeclarationGrid()
		{
			var testUser = Factory.NewWithValidTestData<GlbStaff>();
			testUser.GS_LoginName = "Test.User";
			testUser.GS_IsController = false;
			var operationsSecurity = Factory.New<GlbSecurity>();
			operationsSecurity.GU_GS = testUser.PK;
			operationsSecurity.GU_SecurityRight = Env.Security.Operations.Code;
			operationsSecurity.GU_SecurityItemIsAllowed = true;
			var declarationSecurity = Factory.New<GlbSecurity>();
			declarationSecurity.GU_GS = testUser.PK;
			declarationSecurity.GU_SecurityRight = Env.Security.CustomsDeclarationEnquiryEdit.Code;
			declarationSecurity.GU_SecurityItemIsAllowed = true;
			var shipmentSecurity = Factory.New<GlbSecurity>();
			shipmentSecurity.GU_GS = testUser.PK;
			shipmentSecurity.GU_SecurityRight = Env.Security.MaintainShipmentEdit.Code;
			shipmentSecurity.GU_SecurityItemIsAllowed = false;
			var forwardingShipment = Factory.New<ForwardingShipment>();
			forwardingShipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			forwardingShipment.JS_HouseBill = "HB27092101";
			Factory.Save();
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				var newFactory = new BusinessObjectFactory()
				{ RefreshEnabled = false };
				var companyAU = newFactory.New<GlbCompany>();
				companyAU.GC_Code = "A!#";
				companyAU.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
				companyAU.GC_OH_OrgProxy = GlbCompany.CurrentCompany.GC_OH_OrgProxy;
				var branchAU = companyAU.Branches.AddNew();
				branchAU.GB_Code = "A$#";
				branchAU.GB_OH_OrgProxy = GlbBranch.CurrentBranch.GB_OH_OrgProxy;
				var declarationAU = newFactory.New<BaseJobDeclaration>();
				declarationAU.JE_JS = forwardingShipment.PK;
				declarationAU.JE_MessageType = JobMessageTypeList.Codes.Import;
				declarationAU.JE_GB = branchAU.PK;
				newFactory.Save();
			}

			using (FreightDataRegistry.Instance.FreightEnableComplianceWise.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty,
				ComplianceWiseRegistryHelper.SetValue(false)))
			using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedStates))
			{
				var declaration = Factory.New<BaseJobDeclaration>();
				declaration.JE_JS = forwardingShipment.PK;
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				Factory.Save();
				try
				{
					using (Env.SetTemporaryUserContext(testUser.GS_LoginName, Env.CurrentBranchPK, Env.CurrentDepartmentPK))
					{
						var factories = PersistentFactoryCacheManager.Instance.GetBusinessObjectFactories().Where(x => x.NameForDebugging == "UrlHandler Factory").ToHashSet();
						using (var declarationModule = new JobDeclarationModule())
						{
							declarationModule.ModuleDecisionProvider.HandleFindBoxOKButton(new BusinessObject[] { declaration });
							using (var lastFormCreated = Application.OpenForms.OfType<ShipmentForm>().FirstOrDefault())
							{
								CombineAssertions("Open from declaration grid", () =>
								{
									AssertNotNull("Should have shown form", lastFormCreated);
									AssertEquals("lastFormCreated.Text", $"View Shipment {forwardingShipment.JS_UniqueConsignRef}", lastFormCreated.Text);
									AssertEquals("lastFormCreated.DisplayMode", ODisplayMode.ReadOnly, lastFormCreated.DisplayMode);
								});
							}
						}

						var queryStringFromRecentItem = new QueryString($"Command=ShowEditForm&ControllerID={ControllerIDs.Customs.JobDeclarationPluggedIntoShipment.Name}&BusinessEntityPK={forwardingShipment.PK}");
						ShowEditFormUrlHandler.Instance.Handle(queryStringFromRecentItem);
						using (var lastFormCreated = Application.OpenForms.OfType<ShipmentForm>().FirstOrDefault())
						{
							CombineAssertions("Open from recent item", () =>
							{
								AssertNotNull("Should have shown form", lastFormCreated);
								AssertEquals("lastFormCreated.Text", $"View Shipment {forwardingShipment.JS_UniqueConsignRef}", lastFormCreated.Text);
								AssertEquals("lastFormCreated.DisplayMode", ODisplayMode.ReadOnly, lastFormCreated.DisplayMode);
								var controllerFactory = PersistentFactoryCacheManager.Instance.GetBusinessObjectFactories().Where(x => x.NameForDebugging == "ZController_GetNewFactory" && !factories.Contains(x)).Last();
								AssertEquals("Should load current company declaration", 1, ((IBusinessObjectFactoryInternals)controllerFactory).AllBusinessObjects.Count(x => x.PK == declaration.PK));
							});
						}
					}
				}
				finally
				{
					foreach (var form in Application.OpenForms.OfType<ShipmentForm>().ToArray())
					{
						form.Dispose();
					}
				}
			}
		}
	}
}
