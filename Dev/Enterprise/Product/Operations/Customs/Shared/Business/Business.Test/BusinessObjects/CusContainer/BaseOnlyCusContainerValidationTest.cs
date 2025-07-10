using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business.Testing
{
	sealed class BaseOnlyCusContainerValidationTest : TestCaseWithFactory
	{
		const string DuplicateContainersWithOtherDec = " has the same container as this declaration.";

		public void TestValidateForDuplicateContainersAgainstOtherCountryDeclaration()
		{
			var nzCompany = Factory.New<GlbCompany>();
			nzCompany.FillWithValidTestData();
			var nzBranch = nzCompany.Branches.AddNew();
			nzBranch.GB_RL_NKHomePort = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, Core.Constants.CountryCodes.NewZealand)).RL_Code;

			GlbCompany.CurrentCompany.SetCountry("NZ");
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_GB = nzBranch.PK;
			declaration.JE_DateOfArrival = ZDateTime.Today.AddMonths(-1);//OK
			var container = declaration.CusContainers.AddNew();
			container.CO_ContainerNumber = "C1";
			AssertEquals("No warnings expected", false, container.CO_ContainerNumberInfo.GetWarnings().ContainsNotificationContaining(DuplicateContainersWithOtherDec));
			Factory.Save();

			GlbCompany.CurrentCompany.SetCountry("AU");
			BaseJobDeclaration declaration2 = Factory.New<BaseJobDeclaration>();
			declaration2.JE_DateOfArrival = ZDateTime.Today.AddMonths(-1);
			BaseCusContainer container2 = declaration2.CusContainers.AddNew();
			container2.CO_ContainerNumber = "C1";
			AssertEquals("No warnings expected", false, container2.CO_ContainerNumberInfo.GetWarnings().ContainsNotificationContaining(DuplicateContainersWithOtherDec));
		}

		public void TestValidateForDuplicateContainersAgainstDeclarationTwoMonthsAgo()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_DateOfArrival = ZDateTime.Today.AddMonths(-2);
			BaseCusContainer container = declaration.CusContainers.AddNew();
			container.CO_ContainerNumber = "C1";
			AssertEquals("No warnings expected", false, container.CO_ContainerNumberInfo.GetWarnings().ContainsNotificationContaining(DuplicateContainersWithOtherDec));
			Factory.Save();

			BaseJobDeclaration declaration2 = Factory.New<BaseJobDeclaration>();
			declaration2.JE_DateOfArrival = ZDateTime.Today.AddMonths(-1);
			BaseCusContainer container2 = declaration2.CusContainers.AddNew();
			container2.CO_ContainerNumber = "C1";
			AssertEquals("No warnings expected", false, container2.CO_ContainerNumberInfo.GetWarnings().ContainsNotificationContaining(DuplicateContainersWithOtherDec));
		}

		public void TestValidateForDuplicateContainersAgainstDeclarationCancelled()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_DateOfArrival = ZDateTime.Today.AddMonths(-1);
			declaration.JE_IsCancelled = true;
			BaseCusContainer container = declaration.CusContainers.AddNew();
			container.CO_ContainerNumber = "C1";
			AssertEquals("No warnings expected", false, container.CO_ContainerNumberInfo.GetWarnings().ContainsNotificationContaining(DuplicateContainersWithOtherDec));
			Factory.Save();

			BaseJobDeclaration declaration2 = Factory.New<BaseJobDeclaration>();
			declaration2.JE_DateOfArrival = ZDateTime.Today.AddMonths(-1);
			BaseCusContainer container2 = declaration2.CusContainers.AddNew();
			container2.CO_ContainerNumber = "C1";
			AssertEquals("No warnings expected", false, container2.CO_ContainerNumberInfo.GetWarnings().ContainsNotificationContaining(DuplicateContainersWithOtherDec));
		}

		public void TestValidateForDuplicateContainers()
		{
			((IBusinessObjectFactoryInternals)GlbCompany.CurrentCompany.Factory).CanSave = true;
			GlbCompany.CurrentCompany.Factory.Save();

			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_TransportMode = declaration.TransportModeSeaCodeForTesting;
			declaration.JE_DateOfArrival = ZDateTime.Today.AddMonths(-1);
			var container = declaration.CusContainers.AddNew();
			container.CO_ContainerNumber = "C1";

			AssertEquals("No warnings expected", false, container.CO_ContainerNumberInfo.GetWarnings().ContainsNotificationContaining(DuplicateContainersWithOtherDec));
			Factory.Save();

			var declaration2 = Factory.New<BaseJobDeclaration>();
			declaration2.JE_DateOfArrival = ZDateTime.Today.AddMonths(-1);
			var container2 = declaration2.CusContainers.AddNew();
			container2.CO_ContainerNumber = "C1";
			var msgText = string.Format("Another Declaration already contains the same container as this declaration (Declaration: '{0}', Company: '{1}', Branch: '{2}').", declaration.JE_DeclarationReference, declaration.Company.GC_Name, declaration.Branch.GB_BranchName);
			AssertHasWarningContaining(container2.CO_ContainerNumberInfo, msgText);

			declaration2.JE_DateOfArrival = ZDateTime.Today.AddMonths(-3);
			container2.CO_ContainerNumber = "C1";
			AssertNoWarningContaining(container2.CO_ContainerNumberInfo, msgText);

			declaration2.JE_DateOfArrival = ZDateTime.Today.AddMonths(-1);
			container2.CO_ContainerNumber = "C1";
			AssertHasWarningContaining(container2.CO_ContainerNumberInfo, msgText);

			container2.CO_ContainerNumber = "C2";
			AssertNoWarningContaining(container2.CO_ContainerNumberInfo, msgText);
		}

		public void TestCheckCO_ContainerNumber()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var container = declaration.CusContainers.AddNew();
			ValidationTestHelper.AssertInvalidCodeMessageError(container.CO_RN_NKOwnerCountryInfo, "XX", "AU");
		}

		public void TestCheckCO_RN_NKOwnerCountry()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			var container1 = declaration.CusContainers.AddNew();
			container1.CO_ContainerNumber = "container1";
			var container2 = declaration.CusContainers.AddNew();
			container2.CO_ContainerNumber = "container2";
			AssertHasWarning(container1.CO_ContainerNumberInfo, CusContainerValidation.ContainerShouldLinkToOneInvoiceLine);
			AssertHasWarning(container2.CO_ContainerNumberInfo, CusContainerValidation.ContainerShouldLinkToOneInvoiceLine);
			var pivot = invoiceLine.ContainersPivot.AddNew();
			pivot.C2_CO = container1.PK;
			container1.Validation.ValidateCO_ContainerNumber();
			AssertNoWarning(container1.CO_ContainerNumberInfo, CusContainerValidation.ContainerShouldLinkToOneInvoiceLine);
		}
	}
}
