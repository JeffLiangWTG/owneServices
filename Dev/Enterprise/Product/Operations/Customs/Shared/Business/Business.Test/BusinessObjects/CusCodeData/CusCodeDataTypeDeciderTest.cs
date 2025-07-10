using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Integration.Customs;

namespace Enterprise.Customs.Business.Testing
{
	sealed class CusCodeDataTypeDeciderTest : TestCaseWithFactory
	{
		public void TestGetTypeForBinding()
		{
			var typeDecider = new CusCodeDataTypeDecider();
			AssertNull(typeDecider.GetTypeForBinding());
		}

		public void TestGetTypeForNew()
		{
			var typeDecider = new CusCodeDataTypeDecider();
			AssertNull(typeDecider.GetTypeForNew());
		}

		public void TestGetTypeForLoad()
		{
			var usCompany = Factory.New<GlbCompany>();
			usCompany.GC_Code = "U!@";
			usCompany.GC_Name = "US COMPANY";
			usCompany.GC_OH_OrgProxy = GlbCompany.CurrentCompany.GC_OH_OrgProxy;
			usCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
			var usBranch = usCompany.Branches.AddNew();
			usBranch.GB_Code = "U!@";
			usBranch.GB_BranchName = "US BRANCH";
			usBranch.GB_OH_OrgProxy = GlbBranch.CurrentBranch.GB_OH_OrgProxy;
			var usDeclaration = Factory.New<US.IJobDeclaration>();
			usDeclaration.JE_GB = usBranch.PK;
			var supporter = (ICusCodeDataTypeSupporter)usDeclaration;
			Type contractNumberType = null;
			supporter.GetCusCodeDataTypes().TryGetValue("CNN", out contractNumberType);
			var cusCodeData = (CusCodeData)Factory.New(contractNumberType);
			cusCodeData.CY_ParentID = usDeclaration.PK;
			cusCodeData.CY_ParentTableCode = JobDeclarationSchema.Constants.Prefix;

			var noneUSDeclaration = Factory.New<BaseJobDeclaration>();

			var typeDecider = new CusCodeDataTypeDecider();
			var message = string.Format("{0} does not support CY_Type 'CNN'", noneUSDeclaration.GetType().FullName);
			AssertNull(typeDecider.GetTypeForLoad("CNN", JobDeclarationSchema.Constants.Prefix, noneUSDeclaration.PK, Factory));
			AssertEquals(message, ErrorReporter.LastKeyReported);
			ErrorReporter.Clear();
			AssertEquals(contractNumberType, typeDecider.GetTypeForLoad("CNN", JobDeclarationSchema.Constants.Prefix, usDeclaration.PK, Factory));
			AssertEquals(true, string.IsNullOrEmpty(ErrorReporter.LastKeyReported));
			AssertEquals(contractNumberType, typeDecider.GetTypeForLoad(cusCodeData, Factory));
			AssertEquals(true, string.IsNullOrEmpty(ErrorReporter.LastKeyReported));
			AssertEquals(contractNumberType, typeDecider.GetTypeForLoad(((IBusinessObjectInternals)cusCodeData).Row, Factory));
			AssertEquals(true, string.IsNullOrEmpty(ErrorReporter.LastKeyReported));

			cusCodeData.CY_Type = "Z!D";
			message = string.Format("{0} does not support CY_Type 'Z!D'", usDeclaration.GetType().FullName);
			AssertNull(typeDecider.GetTypeForLoad("Z!D", JobDeclarationSchema.Constants.Prefix, usDeclaration.PK, Factory));
			AssertEquals(message, ErrorReporter.LastKeyReported);
			ErrorReporter.Clear();
			AssertNull(typeDecider.GetTypeForLoad(cusCodeData, Factory));
			AssertEquals(message, ErrorReporter.LastKeyReported);
			ErrorReporter.Clear();
			AssertNull(typeDecider.GetTypeForLoad(((IBusinessObjectInternals)cusCodeData).Row, Factory));
			AssertEquals(message, ErrorReporter.LastKeyReported);
			ErrorReporter.Clear();
		}
	}
}
