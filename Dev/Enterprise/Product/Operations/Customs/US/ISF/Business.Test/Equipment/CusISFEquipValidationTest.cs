using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.ISF.DataRegistry.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.ISF.Business.Testing
{
	sealed class CusISFEquipValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckBE_ContainerNum()
		{
			CusISFEquip container2 = header.Equipments.AddNew();
			container2.BE_ContainerNum = "TURE3923424";
			container.BE_ContainerNum = "!";
			AssertHasMessageError(container.BE_ContainerNumInfo, CusISFEquipValidation.MustOnlyContainAlphaNumerics);
			container.BE_ContainerNum = "";
			AssertNoMessageError(container.BE_ContainerNumInfo, CusISFEquipValidation.MustOnlyContainAlphaNumerics);
			AssertHasWarning(container.BE_ContainerNumInfo, "You have not entered a Container Number.");
			container.BE_ContainerNum = "X";
			AssertNoMessageError(container.BE_ContainerNumInfo, CusISFEquipValidation.MustOnlyContainAlphaNumerics);
			AssertHasWarning(container.BE_ContainerNumInfo, "Container number does not conform to ISO standard of 4 letters followed by 6 digits and a check digit.");
			container.BE_ContainerNum = "TURE3923422";
			string invalidCheckDigit = "Container number does not have a valid check (last) digit. The check digit should be 4.";
			AssertNoWarning(container.BE_ContainerNumInfo, "Container number does not conform to ISO standard of 4 letters followed by 6 digits and a check digit.");
			AssertHasWarning(container.BE_ContainerNumInfo, invalidCheckDigit);
			AssertNoError(container.BE_ContainerNumInfo, CusISFEquipValidation.ContainerNumberAlreadyExists);
			container.BE_ContainerNum = "TURE3923424";
			AssertNoWarning(container.BE_ContainerNumInfo, invalidCheckDigit);
			AssertHasError(container.BE_ContainerNumInfo, CusISFEquipValidation.ContainerNumberAlreadyExists);
			header.BF_SendEquipment = Business.YesNoDefaultList.Codes.No;
			container.BE_ContainerNum = "TURE 3923422";
			container2.BE_ContainerNum = "TURE 3923422";
			AssertHasWarning("Validation Error should now be treated only as a warning", container.BE_ContainerNumInfo, CusISFEquipValidation.MustOnlyContainAlphaNumerics);
			AssertHasWarning("Validation Error should now be treated only as a warning", container2.BE_ContainerNumInfo, CusISFEquipValidation.ContainerNumberAlreadyExists);
			header.BF_SendEquipment = Business.YesNoDefaultList.Codes.Yes;
			container.BE_ContainerNum = "TURE 3923422";
			container2.BE_ContainerNum = "TURE 3923422";
			AssertHasMessageError("Errors should now be message errors again", container.BE_ContainerNumInfo, CusISFEquipValidation.MustOnlyContainAlphaNumerics);
			AssertHasError("Duplicate Error should now be error again", container2.BE_ContainerNumInfo, CusISFEquipValidation.ContainerNumberAlreadyExists);
		}

		public void TestContainerErrorsOrWarningsWhenDefaultSet()
		{
			var company1 = Factory.New<GlbCompany>();
			company1.GC_Code = "Z!1";
			company1.GC_Name = "DUMMY COMPANY";
			company1.GC_OH_OrgProxy = GlbCompany.CurrentCompany.GC_OH_OrgProxy;
			var branch1 = company1.Branches.AddNew();
			branch1.GB_Code = "Z!1";
			branch1.GB_BranchName = "DUMMY BRANCH";
			branch1.GB_OH_OrgProxy = GlbBranch.CurrentBranch.GB_OH_OrgProxy;
			Factory.Save();
			var branch1PK = branch1.PK.ToGuid();
			ISFRegistry.Instance.ImporterSecurityFilingShouldReportContainerToCustoms.SetValue(Guid.Empty, branch1PK, Guid.Empty, false);
			ISFRegistry.Instance.ImporterSecurityFilingShouldReportContainerToCustoms.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, false);
			header.BF_SendEquipment = Business.YesNoDefaultList.Codes.Default;
			container.BE_ContainerNum = "TURE 3923422";
			CusISFEquip container2 = header.Equipments.AddNew();
			container2.BE_ContainerNum = "TURE 3923422";
			AssertHasWarning("Validation Error should now be treated only as a warning", container.BE_ContainerNumInfo, CusISFEquipValidation.MustOnlyContainAlphaNumerics);
			AssertHasWarning("Validation Error should now be treated only as a warning", container2.BE_ContainerNumInfo, CusISFEquipValidation.ContainerNumberAlreadyExists);
			ISFRegistry.Instance.ImporterSecurityFilingShouldReportContainerToCustoms.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, true);
			container.BE_ContainerNum = "TURE 3923422";
			container2.BE_ContainerNum = "TURE 3923422";
			AssertHasMessageError("Errors should now be message errors again", container.BE_ContainerNumInfo, CusISFEquipValidation.MustOnlyContainAlphaNumerics);
			AssertHasError("Duplicate Error should now be error again", container2.BE_ContainerNumInfo, CusISFEquipValidation.ContainerNumberAlreadyExists);
			header.BF_GB = branch1.PK;
			container.BE_ContainerNum = "TURE 3923422";
			container2.BE_ContainerNum = "TURE 3923422";
			AssertHasWarning("Validation Error should now be treated only as a warning based on Branch1 setting", container.BE_ContainerNumInfo, CusISFEquipValidation.MustOnlyContainAlphaNumerics);
			AssertHasWarning("Validation Error should now be treated only as a warning based on Branch1 setting", container2.BE_ContainerNumInfo, CusISFEquipValidation.ContainerNumberAlreadyExists);
		}

		public void TestCheckBE_EquipCode()
		{
			container.BE_EquipCode = ZString.Empty;
			AssertHasMessageErrorContaining(container.BE_EquipCodeInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(container.BE_EquipCodeInfo, ListValidation.InvalidCodeMessageError);
			container.BE_EquipCode = "Z!";
			AssertNoMessageErrorContaining(container.BE_EquipCodeInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(container.BE_EquipCodeInfo, ListValidation.InvalidCodeMessageError);
			foreach (ICodeDescription pair in USContainerCodeList.GetEquipmentDescriptionCodeList())
			{
				container.BE_EquipCode = pair.Code;
				AssertNoMessageErrorContaining(container.BE_EquipCodeInfo, ListValidation.InvalidCodeMessageError);
			}
		}

		CusISFHeader header;
		CusISFEquip container;
		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.New<CusISFHeader>();
			container = header.Equipments.AddNew();
		}
	}
}
