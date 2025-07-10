using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Accounting.CountryCompliance;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Moq;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(GlbCompanyModule))]
	sealed class GlbCompanyModuleTest : ZArchitecture.Modules.Testing.ZModuleBasherTest
	{
		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.GlbCompany;
		}

		public void TestActionMenuItem_AuthorizeForEInvoicingIsHiddenWhenNotSupported()
		{
			using (companyModule = new GlbCompanyModuleForTest())
			{
				var menuItem = companyModule.GetNewActionMenuItemsForTest().Where(x => x.Text == "Authorize for E-Invoicing").FirstOrDefault();

				AssertNull(menuItem);
			}
		}

		public void TestActionMenuItem_AuthorizeForEInvoicingIsVisibleButDisabled()
		{
			WebUrlLauncher.ClearLastUrlLaunched();
			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

			Factory.Save();

			var mockCountryComplianceFactory = new Mock<ICountryComplianceFactory>();
			var mockCountryComplianceInfo = new Mock<ICountryComplianceInfoBase>();

			mockCountryComplianceInfo.As<IEInvoiceCredentialsProvider>()
				.Setup(x => x.GetAuthorizationURL(It.IsAny<ICompany>()))
				.Returns("https://example.com");
			mockCountryComplianceInfo.As<IEInvoiceCredentialsProvider>()
				.Setup(x => x.ShouldShowCredentialsTab(It.IsAny<ICompany>()))
				.Returns(false);

			mockCountryComplianceFactory.Setup(x => x.GetICountryComplianceInfoBase(It.Is<ZString>(x => x == GlbCompany.CurrentCompany.GC_RN_NKCountryCode)))
				.Returns(mockCountryComplianceInfo.Object);

			using (ObjectFactory.Substitute(mockCountryComplianceFactory.Object))
			using (companyModule = new GlbCompanyModuleForTest())
			{
				var menuItem = companyModule.GetNewActionMenuItemsForTest().Where(x => x.Text == "Authorize for E-Invoicing").FirstOrDefault();

				companyModule.SetSelectedBusinessObjects(new BusinessObject[] { company });
				menuItem.PerformClick();

				AssertEquals("Action is unavailable for this company.", GetAndClearLastMessage());
				AssertNullOrEmpty(WebUrlLauncher.LastUrlLaunched);
			}
		}

		public void TestActionMenuItem_AuthorizeForEInvoicingIsVisible()
		{
			var company1 = Factory.NewWithValidTestData<GlbCompany>();
			company1.GC_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			var company2 = Factory.NewWithValidTestData<GlbCompany>();
			company2.GC_RN_NKCountryCode = "@@";

			Factory.Save();

			var mockCountryComplianceFactory = new Mock<ICountryComplianceFactory>();
			var mockCountryComplianceInfo = new Mock<ICountryComplianceInfoBase>();

			mockCountryComplianceInfo.As<IEInvoiceCredentialsProvider>()
				.Setup(x => x.GetAuthorizationURL(It.IsAny<ICompany>()))
				.Returns("https://example.com");
			mockCountryComplianceInfo.As<IEInvoiceCredentialsProvider>()
				.Setup(x => x.ShouldShowCredentialsTab(It.IsAny<ICompany>()))
				.Returns(true);

			mockCountryComplianceFactory.Setup(x => x.GetICountryComplianceInfoBase(It.Is<ZString>(x => x == GlbCompany.CurrentCompany.GC_RN_NKCountryCode)))
				.Returns(mockCountryComplianceInfo.Object);

			using (ObjectFactory.Substitute(mockCountryComplianceFactory.Object))
			using (companyModule = new GlbCompanyModuleForTest())
			{
				var menuItem = companyModule.GetNewActionMenuItemsForTest().Where(x => x.Text == "Authorize for E-Invoicing").FirstOrDefault();

				AssertNotNull(menuItem);

				var selectedBusinessObjects = new List<BusinessObject> { company1 };
				companyModule.SetSelectedBusinessObjects(selectedBusinessObjects.ToArray());
				menuItem.PerformClick();

				AssertNullOrEmpty(GetAndClearLastMessage());
				AssertEquals("https://example.com", WebUrlLauncher.LastUrlLaunched);

				mockCountryComplianceInfo.As<IEInvoiceCredentialsProvider>()
					.Verify(x => x.CreateOrUpdateEInvoicingCredential(It.IsAny<ICompany>()), Times.Once);

				selectedBusinessObjects.Add(company2);
				companyModule.SetSelectedBusinessObjects(selectedBusinessObjects.ToArray());
				menuItem.PerformClick();
				AssertEquals("You can only authorize one company at a time.", GetAndClearLastMessage());

				selectedBusinessObjects.Remove(company1);
				companyModule.SetSelectedBusinessObjects(selectedBusinessObjects.ToArray());
				menuItem.PerformClick();
				AssertEquals("Action is unavailable for this company.", GetAndClearLastMessage());
			}
		}

		public void TestActionMenuItem_AuthorizeForEInvoicingIsVisible_DifferentCrendentialProviders()
		{
			var company1 = Factory.NewWithValidTestData<GlbCompany>();
			company1.GC_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			var company2 = Factory.NewWithValidTestData<GlbCompany>();
			company2.GC_RN_NKCountryCode = "AA";

			Factory.Save();

			var mockCountryComplianceFactory = new Mock<ICountryComplianceFactory>();
			var mockCountryComplianceInfo1 = new Mock<ICountryComplianceInfoBase>();
			var mockCountryComplianceInfo2 = new Mock<ICountryComplianceInfoBase>();

			mockCountryComplianceInfo1.As<IEInvoiceCredentialsProvider>()
				.Setup(x => x.GetAuthorizationURL(It.IsAny<ICompany>()))
				.Returns("https://example1.com");
			mockCountryComplianceInfo1.As<IEInvoiceCredentialsProvider>()
				.Setup(x => x.ShouldShowCredentialsTab(It.IsAny<ICompany>()))
				.Returns(true);

			mockCountryComplianceInfo2.As<IEInvoiceCredentialsProvider>()
				.Setup(x => x.GetAuthorizationURL(It.IsAny<ICompany>()))
				.Returns("https://example2.com");
			mockCountryComplianceInfo2.As<IEInvoiceCredentialsProvider>()
				.Setup(x => x.ShouldShowCredentialsTab(It.IsAny<ICompany>()))
				.Returns(true);

			mockCountryComplianceFactory.Setup(x => x.GetICountryComplianceInfoBase(It.Is<ZString>(x => x == GlbCompany.CurrentCompany.GC_RN_NKCountryCode)))
				.Returns(mockCountryComplianceInfo1.Object);
			mockCountryComplianceFactory.Setup(x => x.GetICountryComplianceInfoBase(It.Is<ZString>(x => x == "AA")))
				.Returns(mockCountryComplianceInfo2.Object);

			using (ObjectFactory.Substitute(mockCountryComplianceFactory.Object))
			using (companyModule = new GlbCompanyModuleForTest())
			{
				var menuItem = companyModule.GetNewActionMenuItemsForTest().Where(x => x.Text == "Authorize for E-Invoicing").FirstOrDefault();

				companyModule.SetSelectedBusinessObjects(new BusinessObject[] { company1 });
				menuItem.PerformClick();

				AssertNullOrEmpty(GetAndClearLastMessage());
				AssertEquals("https://example1.com", WebUrlLauncher.LastUrlLaunched);

				WebUrlLauncher.ClearLastUrlLaunched();

				companyModule.SetSelectedBusinessObjects(new BusinessObject[] { company2 });
				menuItem.PerformClick();

				AssertEquals("Action is unavailable for this company.", GetAndClearLastMessage());
				AssertNullOrEmpty(WebUrlLauncher.LastUrlLaunched);
			}
		}

		[RequiresSTA]
		public void TestFilterControl()
		{
			companyModule = new GlbCompanyModuleForTest();
			IFilterControl controlForTest = companyModule.GetNewFilterControlForTest();
			Assert(controlForTest is GlbCompanyFilterControl);
			controlForTest.Dispose();
			companyModule.Dispose();
		}

		public void TestGridCollection()
		{
			companyModule = new GlbCompanyModuleForTest();
			Assert(companyModule.GetNewGridCollectionForTest() is IBusinessObjectCollection);
			companyModule.Dispose();
		}

		public void TestFilterBusinessObject()
		{
			companyModule = new GlbCompanyModuleForTest();
			Assert(companyModule.GetNewFilterBusinessObjectForTest() is FilterBusinessObject);
			companyModule.Dispose();
		}

		[RequiresSTA]
		public void TestControlResourceString()
		{
			using (companyModule = new GlbCompanyModuleForTest())
			using (var filterControl = companyModule.GetNewFilterControlForTest() as GlbCompanyFilterControl)
			{
				AssertNotNull(filterControl);

				StringBuilder result = new StringBuilder();
				foreach (var columnStyle in filterControl.FilteredGrid.ColumnStyles)
				{
					var resCaptionedControl = columnStyle as IResCaptionedControl;
					if (resCaptionedControl != null)
					{
						var zGridColumn = resCaptionedControl as ZGridColumnInfo;
						if (zGridColumn != null)
						{
							var resourceStringData = new ResourceStringKeyCalculator(filterControl.FilteredGrid, zGridColumn.ColumnName).DataString;
							if (resourceStringData != null && !resourceStringData.IsEmpty())
							{
								var captions = resourceStringData.GetCaptions();
								var controlResoureString = resCaptionedControl.CaptionResourceString.Caption;
								var definedResourceString = string.Join(",", captions);
								if (!(definedResourceString + ",").Contains(controlResoureString + ","))
								{
									result.AppendLine(string.Format(@"Control has the resource string set to '{0}', but it has different caption(s) '{1}' defined in the resource xml.", controlResoureString, definedResourceString));
								}
							}
						}
					}
				}
				AssertEquals("", result.ToString());
			}
		}

		void AssertControlHasSameCaptionWithDefinedResourceString(Control parentControl)
		{
			foreach (Control control in parentControl.Controls)
			{
				var resCaptionedControl = control as IResCaptionedControl;
				if (resCaptionedControl != null)
				{
					var resourceStringData = new ResourceStringKeyCalculator(control).DataString;
					if (resourceStringData != null && !resourceStringData.IsEmpty())
					{
						var captions = resourceStringData.GetCaptions();
						Assert((string.Join(",", captions) + ",").Contains(resCaptionedControl.CaptionResourceString.Caption + ","));
					}
				}
				AssertControlHasSameCaptionWithDefinedResourceString(control);
			}
		}

		#region Implementation

		string GetAndClearLastMessage()
		{
			var notification = ((UnitTestUserNotification)Globals.Message);
			var message = notification.LastMessage.Text;
			notification.ClearMessages();

			return message;
		}

		GlbCompanyModuleForTest companyModule;

		#endregion
	}
}
