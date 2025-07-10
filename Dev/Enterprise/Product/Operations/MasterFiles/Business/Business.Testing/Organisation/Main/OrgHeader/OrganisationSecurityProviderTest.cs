using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.Registry.Business;
using Enterprise.Security.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	public class OrganisationSecurityProviderTest : TestCaseWithFactory
	{
		#region Overrides

		protected virtual OrganisationSecurityProvider GetNewSecurityProvider()
		{
			org = Factory.New<OrgHeader>();
			org.OH_Code = "ABC";
			return new OrganisationSecurityProvider(org);
		}

		protected virtual Type TypeOfSecurityProvider
		{
			get { return typeof(OrganisationSecurityProvider); }
		}
		OrgHeader org;
		OrgHeader testOrg;
		OrganisationSecurityProvider testSecurityProvider;
		OrganisationSecurityProvider testUnsavedSecurityProvider;

		protected override void SetUp()
		{
			base.SetUp();
			testOrg = Factory.LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, "DEMORG");
			testSecurityProvider = new OrganisationSecurityProvider(testOrg);
			testUnsavedSecurityProvider = GetNewSecurityProvider();
		}

		#endregion

		#region Test All CheckPoints Tested

		public void TestAllCheckPointsAreTested()
		{
			PropertyInfo[] providerFields = TypeOfSecurityProvider.GetProperties(BindingFlags.Instance | BindingFlags.Public);
			MethodInfo[] testcaseMethods = this.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public);
			List<string> propertiesUntested = new List<string>();

			Assert("providerFields has members", providerFields.Length > 0);
			foreach (PropertyInfo field in providerFields)
			{
				if (field.PropertyType == typeof(bool) && field.Name.StartsWith("Has"))
				{
					bool testWasFound = Array.Exists(testcaseMethods, delegate(MethodInfo testcaseMethod)
					{
						return testcaseMethod.Name == "Test" + field.Name;
					}
					);

					if (!testWasFound)
					{
						propertiesUntested.Add(field.Name);
					}
				}
			}

			if (propertiesUntested.Count > 0)
			{
				Fail("A test for the following properties should have been created, but has not. Please create a test in this class testing your new Organisation Security Items.\r\n" + String.Join("\r\n", propertiesUntested.ToArray()));
			}
		}

		#endregion

		#region TestHasSecurityByName

		public void TestHasSecurityByName_ItemWithValidFormat()
		{
			Env.Security.OrgDetailsModify.IsAllowed = false;
			AssertEquals("Searching for Modify Details with perfect format", false, testSecurityProvider.HasSecurityByName("IsModifyDetails"));
			AssertEquals("Searching for Modify Details without the leading Is", false, testSecurityProvider.HasSecurityByName("ModifyDetails"));
			AssertEquals("Searching for Modify Details with trailing security", false, testSecurityProvider.HasSecurityByName("ModifyDetailsSecurity"));
		}

		[ExpectException(typeof(ArgumentException))]
		public void TestHasSecurityByName_ItemWithInvalidFormat()
		{
			bool isAllowed = testSecurityProvider.HasSecurityByName("SomeCrapNameThatWontExist");
		}

		#endregion

		#region TestHasNewDetailsIsNationalOrgSecurity

		public void TestHasNewDetailsIsNationalOrgSecurity()
		{
			Env.Security.OrgDetailsNewIsNationalOrg.IsAllowed = false;
			AssertEquals("HasNewDetailsIsNationalOrgSecurity should NOT be allowed", false, testSecurityProvider.HasNewDetailsIsNationalOrgSecurity);

			Env.Security.OrgDetailsNewIsNationalOrg.IsAllowed = true;
			AssertEquals("HasNewDetailsIsNationalOrgSecurity should be allowed", true, testSecurityProvider.HasNewDetailsIsNationalOrgSecurity);
		}

		#endregion

		#region TestHasNewDetailsIsGlobalOrgSecurity

		public void TestHasNewDetailsIsGlobalOrgSecurity()
		{
			Env.Security.OrgDetailsNewIsGlobalOrg.IsAllowed = false;
			AssertEquals("HasNewDetailsIsGlobalOrgSecurity should NOT be allowed", false, testSecurityProvider.HasNewDetailsIsGlobalOrgSecurity);

			Env.Security.OrgDetailsNewIsGlobalOrg.IsAllowed = true;
			AssertEquals("HasNewDetailsIsGlobalOrgSecurity should be allowed", true, testSecurityProvider.HasNewDetailsIsGlobalOrgSecurity);
		}

		#endregion

		#region TestHasNewDetailsIsTemporaryOrgSecurity

		public void TestHasNewDetailsIsTemporaryOrgSecurity()
		{
			Env.Security.OrgDetailsNewIsTemporaryOrg.IsAllowed = false;
			AssertEquals("HasNewDetailsIsTemporaryOrgSecurity should NOT be allowed", false, testSecurityProvider.HasNewDetailsIsTemporaryOrgSecurity);

			Env.Security.OrgDetailsNewIsTemporaryOrg.IsAllowed = true;
			AssertEquals("HasNewDetailsIsTemporaryOrgSecurity should be allowed", true, testSecurityProvider.HasNewDetailsIsTemporaryOrgSecurity);
		}

		#endregion

		#region TestHasNewDetailsOrgTypeFlagAR

		public void TestHasNewDetailsOrgTypeFlagAR()
		{
			Env.Security.OrgDetailsNewOrgTypeFlagAR.IsAllowed = false;
			AssertEquals("HasNewDetailsOrgTypeFlagAR should NOT be allowed", false, testSecurityProvider.HasNewDetailsOrgTypeFlagAR);

			Env.Security.OrgDetailsNewOrgTypeFlagAR.IsAllowed = true;
			AssertEquals("HasNewDetailsOrgTypeFlagAR should be allowed", true, testSecurityProvider.HasNewDetailsOrgTypeFlagAR);
		}

		#endregion

		#region TestHasNewDetailsOrgTypeFlagAP

		public void TestHasNewDetailsOrgTypeFlagAP()
		{
			Env.Security.OrgDetailsNewOrgTypeFlagAP.IsAllowed = false;
			AssertEquals("HasNewDetailsOrgTypeFlagAP should NOT be allowed", false, testSecurityProvider.HasNewDetailsOrgTypeFlagAP);

			Env.Security.OrgDetailsNewOrgTypeFlagAP.IsAllowed = true;
			AssertEquals("HasNewDetailsOrgTypeFlagAP should be allowed", true, testSecurityProvider.HasNewDetailsOrgTypeFlagAP);
		}

		#endregion

		#region TestHasNewDetailsOrgTypeTempARFlag

		public void TestHasNewDetailsOrgTypeTempARFlag()
		{
			Env.Security.OrgDetailsNewOrgTypeTempARFlag.IsAllowed = false;
			AssertEquals("HasNewDetailsOrgTypeTempARFlag should NOT be allowed", false, testSecurityProvider.HasNewDetailsOrgTypeTempARFlag);

			Env.Security.OrgDetailsNewOrgTypeTempARFlag.IsAllowed = true;
			AssertEquals("HasNewDetailsOrgTypeTempARFlag should be allowed", true, testSecurityProvider.HasNewDetailsOrgTypeTempARFlag);
		}

		#endregion

		#region TestHasNewDetailsOrgTypeTempAPFlag

		public void TestHasNewDetailsOrgTypeTempAPFlag()
		{
			Env.Security.OrgDetailsNewOrgTypeTempAPFlag.IsAllowed = false;
			AssertEquals("HasNewDetailsOrgTypeTempAPFlag should NOT be allowed", false, testSecurityProvider.HasNewDetailsOrgTypeTempAPFlag);

			Env.Security.OrgDetailsNewOrgTypeTempAPFlag.IsAllowed = true;
			AssertEquals("HasNewDetailsOrgTypeTempAPFlag should be allowed", true, testSecurityProvider.HasNewDetailsOrgTypeTempAPFlag);
		}

		#endregion

		#region TestHasNewDetailsOrgTypeFlagSP

		public void TestHasNewDetailsOrgTypeFlagSP()
		{
			Env.Security.OrgDetailsNewOrgTypeFlagSP.IsAllowed = false;
			AssertEquals("HasNewDetailsOrgTypeFlagSP should NOT be allowed", false, testSecurityProvider.HasNewDetailsOrgTypeFlagSP);

			Env.Security.OrgDetailsNewOrgTypeFlagSP.IsAllowed = true;
			AssertEquals("HasNewDetailsOrgTypeFlagSP should be allowed", true, testSecurityProvider.HasNewDetailsOrgTypeFlagSP);
		}

		#endregion

		#region TestHasNewDetailsOrgTypeTempSPFlag

		public void TestHasNewDetailsOrgTypeTempSPFlag()
		{
			Env.Security.OrgDetailsNewOrgTypeTempSPFlag.IsAllowed = false;
			AssertEquals("HasNewDetailsOrgTypeTempSPFlag should NOT be allowed", false, testSecurityProvider.HasNewDetailsOrgTypeTempSPFlag);

			Env.Security.OrgDetailsNewOrgTypeTempSPFlag.IsAllowed = true;
			AssertEquals("HasNewDetailsOrgTypeTempSPFlag should be allowed", true, testSecurityProvider.HasNewDetailsOrgTypeTempSPFlag);
		}

		#endregion

		#region TestHasNewDetailsOrgTypeFlagCon

		public void TestHasNewDetailsOrgTypeFlagCon()
		{
			Env.Security.OrgDetailsNewOrgTypeFlagCon.IsAllowed = false;
			AssertEquals("HasNewDetailsOrgTypeFlagCon should NOT be allowed", false, testSecurityProvider.HasNewDetailsOrgTypeFlagCon);

			Env.Security.OrgDetailsNewOrgTypeFlagCon.IsAllowed = true;
			AssertEquals("HasNewDetailsOrgTypeFlagCon should be allowed", true, testSecurityProvider.HasNewDetailsOrgTypeFlagCon);
		}

		#endregion

		#region TestHasNewDetailsOrgTypeTempConFlag

		public void TestHasNewDetailsOrgTypeTempConFlag()
		{
			Env.Security.OrgDetailsNewOrgTypeTempConFlag.IsAllowed = false;
			AssertEquals("HasNewDetailsOrgTypeTempConFlag should NOT be allowed", false, testSecurityProvider.HasNewDetailsOrgTypeTempConFlag);

			Env.Security.OrgDetailsNewOrgTypeTempConFlag.IsAllowed = true;
			AssertEquals("HasNewDetailsOrgTypeTempConFlag should be allowed", true, testSecurityProvider.HasNewDetailsOrgTypeTempConFlag);
		}

		#endregion

		#region TestHasNewDetailsOrgTypeFlagTC

		public void TestHasNewDetailsOrgTypeFlagTC()
		{
			Env.Security.OrgDetailsNewOrgTypeFlagTC.IsAllowed = false;
			AssertEquals("HasNewDetailsOrgTypeFlagTC should NOT be allowed", false, testSecurityProvider.HasNewDetailsOrgTypeFlagTC);

			Env.Security.OrgDetailsNewOrgTypeFlagTC.IsAllowed = true;
			AssertEquals("HasNewDetailsOrgTypeFlagTC should be allowed", true, testSecurityProvider.HasNewDetailsOrgTypeFlagTC);
		}

		#endregion

		#region TestHasNewDetailsOrgTypeTempTCFlag

		public void TestHasNewDetailsOrgTypeTempTCFlag()
		{
			Env.Security.OrgDetailsNewOrgTypeTempTCFlag.IsAllowed = false;
			AssertEquals("HasNewDetailsOrgTypeTempTCFlag should NOT be allowed", false, testSecurityProvider.HasNewDetailsOrgTypeTempTCFlag);

			Env.Security.OrgDetailsNewOrgTypeTempTCFlag.IsAllowed = true;
			AssertEquals("HasNewDetailsOrgTypeTempTCFlag should be allowed", true, testSecurityProvider.HasNewDetailsOrgTypeTempTCFlag);
		}

		#endregion

		#region TestHasNewDetailsOrgTypeFlagWH

		public void TestHasNewDetailsOrgTypeFlagWH()
		{
			Env.Security.OrgDetailsNewOrgTypeFlagWH.IsAllowed = false;
			AssertEquals("HasNewDetailsOrgTypeFlagWH should NOT be allowed", false, testSecurityProvider.HasNewDetailsOrgTypeFlagWH);

			Env.Security.OrgDetailsNewOrgTypeFlagWH.IsAllowed = true;
			AssertEquals("HasNewDetailsOrgTypeFlagWH should be allowed", true, testSecurityProvider.HasNewDetailsOrgTypeFlagWH);
		}

		#endregion

		#region TestHasNewDetailsOrgTypeTempWHFlag

		public void TestHasNewDetailsOrgTypeTempWHFlag()
		{
			Env.Security.OrgDetailsNewOrgTypeTempWHFlag.IsAllowed = false;
			AssertEquals("HasNewDetailsOrgTypeTempWHFlag should NOT be allowed", false, testSecurityProvider.HasNewDetailsOrgTypeTempWHFlag);

			Env.Security.OrgDetailsNewOrgTypeTempWHFlag.IsAllowed = true;
			AssertEquals("HasNewDetailsOrgTypeTempWHFlag should be allowed", true, testSecurityProvider.HasNewDetailsOrgTypeTempWHFlag);
		}

		#endregion

		#region TestHasNewDetailsOrgTypeFlagCrr

		public void TestHasNewDetailsOrgTypeFlagCrr()
		{
			Env.Security.OrgDetailsNewOrgTypeFlagCrr.IsAllowed = false;
			AssertEquals("HasNewDetailsOrgTypeFlagCrr should NOT be allowed", false, testSecurityProvider.HasNewDetailsOrgTypeFlagCrr);

			Env.Security.OrgDetailsNewOrgTypeFlagCrr.IsAllowed = true;
			AssertEquals("HasNewDetailsOrgTypeFlagCrr should be allowed", true, testSecurityProvider.HasNewDetailsOrgTypeFlagCrr);
		}

		#endregion

		#region TestHasNewDetailsOrgTypeFlagFA

		public void TestHasNewDetailsOrgTypeFlagFA()
		{
			Env.Security.OrgDetailsNewOrgTypeFlagFA.IsAllowed = false;
			AssertEquals("HasNewDetailsOrgTypeFlagFA should NOT be allowed", false, testSecurityProvider.HasNewDetailsOrgTypeFlagFA);

			Env.Security.OrgDetailsNewOrgTypeFlagFA.IsAllowed = true;
			AssertEquals("HasNewDetailsOrgTypeFlagFA should be allowed", true, testSecurityProvider.HasNewDetailsOrgTypeFlagFA);
		}

		#endregion

		#region TestHasNewDetailsOrgTypeTempFAFlag

		public void TestHasNewDetailsOrgTypeTempFAFlag()
		{
			Env.Security.OrgDetailsNewOrgTypeTempFAFlag.IsAllowed = false;
			AssertEquals("HasNewDetailsOrgTypeTempFAFlag should NOT be allowed", false, testSecurityProvider.HasNewDetailsOrgTypeTempFAFlag);

			Env.Security.OrgDetailsNewOrgTypeTempFAFlag.IsAllowed = true;
			AssertEquals("HasNewDetailsOrgTypeTempFAFlag should be allowed", true, testSecurityProvider.HasNewDetailsOrgTypeTempFAFlag);
		}

		#endregion

		#region TestHasNewDetailsOrgTypeFlagBR

		public void TestHasNewDetailsOrgTypeFlagBR()
		{
			Env.Security.OrgDetailsNewOrgTypeFlagBR.IsAllowed = false;
			AssertEquals("HasNewDetailsOrgTypeFlagBR should NOT be allowed", false, testSecurityProvider.HasNewDetailsOrgTypeFlagBR);

			Env.Security.OrgDetailsNewOrgTypeFlagBR.IsAllowed = true;
			AssertEquals("HasNewDetailsOrgTypeFlagBR should be allowed", true, testSecurityProvider.HasNewDetailsOrgTypeFlagBR);
		}

		#endregion

		#region TestHasNewDetailsOrgTypeTempBRFlag

		public void TestHasNewDetailsOrgTypeTempBRFlag()
		{
			Env.Security.OrgDetailsNewOrgTypeTempBRFlag.IsAllowed = false;
			AssertEquals("HasNewDetailsOrgTypeTempBRFlag should NOT be allowed", false, testSecurityProvider.HasNewDetailsOrgTypeTempBRFlag);

			Env.Security.OrgDetailsNewOrgTypeTempBRFlag.IsAllowed = true;
			AssertEquals("HasNewDetailsOrgTypeTempBRFlag should be allowed", true, testSecurityProvider.HasNewDetailsOrgTypeTempBRFlag);
		}

		#endregion

		#region TestHasNewDetailsOrgTypeFlagSV

		public void TestHasNewDetailsOrgTypeFlagSV()
		{
			Env.Security.OrgDetailsNewOrgTypeFlagSV.IsAllowed = false;
			AssertEquals("HasNewDetailsOrgTypeFlagSV should NOT be allowed", false, testSecurityProvider.HasNewDetailsOrgTypeFlagSV);

			Env.Security.OrgDetailsNewOrgTypeFlagSV.IsAllowed = true;
			AssertEquals("HasNewDetailsOrgTypeFlagSV should be allowed", true, testSecurityProvider.HasNewDetailsOrgTypeFlagSV);
		}

		#endregion

		#region TestHasNewDetailsOrgTypeTempSVFlag

		public void TestHasNewDetailsOrgTypeTempSVFlag()
		{
			Env.Security.OrgDetailsNewOrgTypeTempSVFlag.IsAllowed = false;
			AssertEquals("HasNewDetailsOrgTypeTempSVFlag should NOT be allowed", false, testSecurityProvider.HasNewDetailsOrgTypeTempSVFlag);

			Env.Security.OrgDetailsNewOrgTypeTempSVFlag.IsAllowed = true;
			AssertEquals("HasNewDetailsOrgTypeTempSVFlag should be allowed", true, testSecurityProvider.HasNewDetailsOrgTypeTempSVFlag);
		}

		#endregion

		#region TestHasNewDetailsOrgTypeFlagCM

		public void TestHasNewDetailsOrgTypeFlagCM()
		{
			Env.Security.OrgDetailsNewOrgTypeFlagCM.IsAllowed = false;
			AssertEquals("HasNewDetailsOrgTypeFlagCM should NOT be allowed", false, testSecurityProvider.HasNewDetailsOrgTypeFlagCM);

			Env.Security.OrgDetailsNewOrgTypeFlagCM.IsAllowed = true;
			AssertEquals("HasNewDetailsOrgTypeFlagCM should be allowed", true, testSecurityProvider.HasNewDetailsOrgTypeFlagCM);
		}

		#endregion

		#region TestHasNewDetailsOrgTypeTempCMFlag

		public void TestHasNewDetailsOrgTypeTempCMFlag()
		{
			Env.Security.OrgDetailsNewOrgTypeTempCMFlag.IsAllowed = false;
			AssertEquals("HasNewDetailsOrgTypeTempCMFlag should NOT be allowed", false, testSecurityProvider.HasNewDetailsOrgTypeTempCMFlag);

			Env.Security.OrgDetailsNewOrgTypeTempCMFlag.IsAllowed = true;
			AssertEquals("HasNewDetailsOrgTypeTempCMFlag should be allowed", true, testSecurityProvider.HasNewDetailsOrgTypeTempCMFlag);
		}

		#endregion

		#region TestHasNewDetailsOrgTypeFlagSal

		public void TestHasNewDetailsOrgTypeFlagSal()
		{
			Env.Security.OrgDetailsNewOrgTypeFlagSal.IsAllowed = false;
			AssertEquals("HasNewDetailsOrgTypeFlagSal should NOT be allowed", false, testSecurityProvider.HasNewDetailsOrgTypeFlagSal);

			Env.Security.OrgDetailsNewOrgTypeFlagSal.IsAllowed = true;
			AssertEquals("HasNewDetailsOrgTypeFlagSal should be allowed", true, testSecurityProvider.HasNewDetailsOrgTypeFlagSal);
		}

		#endregion

		#region TestHasNewDetailsOrgTypeTempSalFlag

		public void TestHasNewDetailsOrgTypeTempSalFlag()
		{
			Env.Security.OrgDetailsNewOrgTypeTempSalFlag.IsAllowed = false;
			AssertEquals("HasNewDetailsOrgTypeTempSalFlag should NOT be allowed", false, testSecurityProvider.HasNewDetailsOrgTypeTempSalFlag);

			Env.Security.OrgDetailsNewOrgTypeTempSalFlag.IsAllowed = true;
			AssertEquals("HasNewDetailsOrgTypeTempSalFlag should be allowed", true, testSecurityProvider.HasNewDetailsOrgTypeTempSalFlag);
		}

		#endregion

		#region TestHasNewDetailsOrgTypeFlagCtrlAgent

		public void TestHasNewDetailsOrgTypeFlagCtrlAgent()
		{
			Env.Security.OrgDetailsNewOrgTypeFlagCtrlAgent.IsAllowed = false;
			AssertEquals("HasNewDetailsOrgTypeFlagCtrlAgent should NOT be allowed", false, testSecurityProvider.HasNewDetailsOrgTypeFlagCtrlAgent);

			Env.Security.OrgDetailsNewOrgTypeFlagCtrlAgent.IsAllowed = true;
			AssertEquals("HasNewDetailsOrgTypeFlagCtrlAgent should be allowed", true, testSecurityProvider.HasNewDetailsOrgTypeFlagCtrlAgent);
		}

		#endregion

		#region TestHasNewDetailsOrgTypeFlagCtrlCustomer

		public void TestHasNewDetailsOrgTypeFlagCtrlCustomer()
		{
			Env.Security.OrgDetailsNewOrgTypeFlagCtrlCustomer.IsAllowed = false;
			AssertEquals("HasNewDetailsOrgTypeFlagCtrlCustomer should NOT be allowed", false, testSecurityProvider.HasNewDetailsOrgTypeFlagCtrlCustomer);

			Env.Security.OrgDetailsNewOrgTypeFlagCtrlCustomer.IsAllowed = true;
			AssertEquals("HasNewDetailsOrgTypeFlagCtrlCustomer should be allowed", true, testSecurityProvider.HasNewDetailsOrgTypeFlagCtrlCustomer);
		}

		#endregion

		#region TestHasNewDetailsOrgTypeFlagCtrlCustomerWithoutCtrlAgent

		public void TestHasNewDetailsOrgTypeFlagCtrlCustomerWithoutCtrlAgent()
		{
			Env.Security.OrgDetailsNewOrgTypeFlagCtrlCustomerWithoutAgt.IsAllowed = false;
			AssertEquals("HasNewDetailsOrgTypeFlagCtrlCustomerWithoutCtrlAgent should NOT be allowed", false, testSecurityProvider.HasNewDetailsOrgTypeFlagCtrlCustomerWithoutCtrlAgent);

			Env.Security.OrgDetailsNewOrgTypeFlagCtrlCustomerWithoutAgt.IsAllowed = true;
			AssertEquals("HasNewDetailsOrgTypeFlagCtrlCustomerWithoutCtrlAgent should be allowed", true, testSecurityProvider.HasNewDetailsOrgTypeFlagCtrlCustomerWithoutCtrlAgent);
		}

		#endregion

		#region TestHasModifyDetailsCodeSecurity

		public void TestHasModifyDetailsCodeSecurity()
		{
			Env.Security.OrgDetailsModifyCode.IsAllowed = false;
			AssertEquals("HasModifyDetailsCodeSecurity should NOT be allowed", false, testSecurityProvider.HasModifyDetailsCodeSecurity);

			Env.Security.OrgDetailsModifyCode.IsAllowed = true;
			AssertEquals("HasModifyDetailsCodeSecurity should be allowed", true, testSecurityProvider.HasModifyDetailsCodeSecurity);
		}

		#endregion

		#region TestHasModifyDetailsIsActiveOrgSecurity

		public void TestHasModifyDetailsIsActiveOrgSecurity()
		{
			Env.Security.OrgDetailsModifyIsActiveOrg.IsAllowed = false;
			AssertEquals("HasModifyDetailsIsActiveOrgSecurity should NOT be allowed", false, testSecurityProvider.HasModifyDetailsIsActiveOrgSecurity);
			AssertEquals("HasModifyDetailsIsActiveOrgSecurity should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifyDetailsIsActiveOrgSecurity);

			Env.Security.OrgDetailsModifyIsActiveOrg.IsAllowed = true;
			AssertEquals("HasModifyDetailsIsActiveOrgSecurity should be allowed", true, testSecurityProvider.HasModifyDetailsIsActiveOrgSecurity);
			AssertEquals("HasModifyDetailsIsActiveOrgSecurity should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifyDetailsIsActiveOrgSecurity);
		}

		#endregion

		#region TestHasModifyDetailsIsNationalOrgSecurity

		public void TestHasModifyDetailsIsNationalOrgSecurity()
		{
			Env.Security.OrgDetailsModifyIsNationalOrg.IsAllowed = false;
			AssertEquals("HasModifyDetailsIsNationalOrgSecurity should NOT be allowed", false, testSecurityProvider.HasModifyDetailsIsNationalOrgSecurity);
			AssertEquals("HasModifyDetailsIsNationalOrgSecurity should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifyDetailsIsNationalOrgSecurity);

			Env.Security.OrgDetailsModifyIsNationalOrg.IsAllowed = true;
			AssertEquals("HasModifyDetailsIsNationalOrgSecurity should be allowed", true, testSecurityProvider.HasModifyDetailsIsNationalOrgSecurity);
			AssertEquals("HasModifyDetailsIsNationalOrgSecurity should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifyDetailsIsNationalOrgSecurity);
		}

		#endregion

		#region TestHasModifyDetailsIsGlobalOrgSecurity

		public void TestHasModifyDetailsIsGlobalOrgSecurity()
		{
			Env.Security.OrgDetailsModifyIsGlobalOrg.IsAllowed = false;
			AssertEquals("HasModifyDetailsIsGlobalOrgSecurity should NOT be allowed", false, testSecurityProvider.HasModifyDetailsIsGlobalOrgSecurity);
			AssertEquals("HasModifyDetailsIsGlobalOrgSecurity should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifyDetailsIsGlobalOrgSecurity);

			Env.Security.OrgDetailsModifyIsGlobalOrg.IsAllowed = true;
			AssertEquals("HasModifyDetailsIsGlobalOrgSecurity should be allowed", true, testSecurityProvider.HasModifyDetailsIsGlobalOrgSecurity);
			AssertEquals("HasModifyDetailsIsGlobalOrgSecurity should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifyDetailsIsGlobalOrgSecurity);
		}

		#endregion

		#region TestHasModifyDetailsIsTemporaryOrgSecurity

		public void TestHasModifyDetailsIsTemporaryOrgSecurity()
		{
			Env.Security.OrgDetailsModifyIsTemporaryOrg.IsAllowed = false;
			AssertEquals("HasModifyDetailsIsTemporaryOrgSecurity should NOT be allowed", false, testSecurityProvider.HasModifyDetailsIsTemporaryOrgSecurity);
			AssertEquals("HasModifyDetailsIsTemporaryOrgSecurity should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifyDetailsIsTemporaryOrgSecurity);

			Env.Security.OrgDetailsModifyIsTemporaryOrg.IsAllowed = true;
			AssertEquals("HasModifyDetailsIsTemporaryOrgSecurity should be allowed", true, testSecurityProvider.HasModifyDetailsIsTemporaryOrgSecurity);
			AssertEquals("HasModifyDetailsIsTemporaryOrgSecurity should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifyDetailsIsTemporaryOrgSecurity);
		}

		#endregion

		#region TestHasModifyDetailsOrgTypeFlagAR

		public void TestHasModifyDetailsOrgTypeFlagAR()
		{
			Env.Security.OrgDetailsModifyOrgTypeFlagAR.IsAllowed = false;
			AssertEquals("HasModifyDetailsOrgTypeFlagAR should NOT be allowed", false, testSecurityProvider.HasModifyDetailsOrgTypeFlagAR);
			AssertEquals("HasModifyDetailsOrgTypeFlagAR should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifyDetailsOrgTypeFlagAR);

			Env.Security.OrgDetailsModifyOrgTypeFlagAR.IsAllowed = true;
			AssertEquals("HasModifyDetailsOrgTypeFlagAR should be allowed", true, testSecurityProvider.HasModifyDetailsOrgTypeFlagAR);
			AssertEquals("HasModifyDetailsOrgTypeFlagAR should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifyDetailsOrgTypeFlagAR);
		}

		#endregion

		#region TestHasModifyDetailsOrgTypeFlagAP

		public void TestHasModifyDetailsOrgTypeFlagAP()
		{
			Env.Security.OrgDetailsModifyOrgTypeFlagAP.IsAllowed = false;
			AssertEquals("HasModifyDetailsOrgTypeFlagAP should NOT be allowed", false, testSecurityProvider.HasModifyDetailsOrgTypeFlagAP);
			AssertEquals("HasModifyDetailsOrgTypeFlagAP should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifyDetailsOrgTypeFlagAP);

			Env.Security.OrgDetailsModifyOrgTypeFlagAP.IsAllowed = true;
			AssertEquals("HasModifyDetailsOrgTypeFlagAP should be allowed", true, testSecurityProvider.HasModifyDetailsOrgTypeFlagAP);
			AssertEquals("HasModifyDetailsOrgTypeFlagAP should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifyDetailsOrgTypeFlagAP);
		}

		#endregion

		#region TestHasModifyDetailsOrgTypeTempARFlag

		public void TestHasModifyDetailsOrgTypeTempARFlag()
		{
			Env.Security.OrgDetailsModifyOrgTypeTempARFlag.IsAllowed = false;
			AssertEquals("HasModifyDetailsOrgTypeTempARFlag should NOT be allowed", false, testSecurityProvider.HasModifyDetailsOrgTypeTempARFlag);
			AssertEquals("HasModifyDetailsOrgTypeTempARFlag should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifyDetailsOrgTypeTempARFlag);

			Env.Security.OrgDetailsModifyOrgTypeTempARFlag.IsAllowed = true;
			AssertEquals("HasModifyDetailsOrgTypeTempARFlag should be allowed", true, testSecurityProvider.HasModifyDetailsOrgTypeTempAPFlag);
			AssertEquals("HasModifyDetailsOrgTypeTempARFlag should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifyDetailsOrgTypeTempARFlag);
		}

		#endregion

		#region TestHasModifyDetailsOrgTypeTempAPFlag

		public void TestHasModifyDetailsOrgTypeTempAPFlag()
		{
			Env.Security.OrgDetailsModifyOrgTypeTempAPFlag.IsAllowed = false;
			AssertEquals("HasModifyDetailsOrgTypeTempAPFlag should NOT be allowed", false, testSecurityProvider.HasModifyDetailsOrgTypeTempAPFlag);
			AssertEquals("HasModifyDetailsOrgTypeTempAPFlag should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifyDetailsOrgTypeTempAPFlag);

			Env.Security.OrgDetailsModifyOrgTypeTempAPFlag.IsAllowed = true;
			AssertEquals("HasModifyDetailsOrgTypeTempAPFlag should be allowed", true, testSecurityProvider.HasModifyDetailsOrgTypeTempAPFlag);
			AssertEquals("HasModifyDetailsOrgTypeTempAPFlag should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifyDetailsOrgTypeTempAPFlag);
		}

		#endregion

		#region TestHasModifyDetailsOrgTypeFlagSP

		public void TestHasModifyDetailsOrgTypeFlagSP()
		{
			Env.Security.OrgDetailsModifyOrgTypeFlagSP.IsAllowed = false;
			AssertEquals("HasModifyDetailsOrgTypeFlagSP should NOT be allowed", false, testSecurityProvider.HasModifyDetailsOrgTypeFlagSP);
			AssertEquals("HasModifyDetailsOrgTypeFlagSP should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifyDetailsOrgTypeFlagSP);

			Env.Security.OrgDetailsModifyOrgTypeFlagSP.IsAllowed = true;
			AssertEquals("HasModifyDetailsOrgTypeFlagSP should be allowed", true, testSecurityProvider.HasModifyDetailsOrgTypeFlagSP);
			AssertEquals("HasModifyDetailsOrgTypeFlagSP should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifyDetailsOrgTypeFlagSP);
		}

		#endregion

		#region TestHasModifyDetailsOrgTypeTempSPFlag

		public void TestHasModifyDetailsOrgTypeTempSPFlag()
		{
			Env.Security.OrgDetailsModifyOrgTypeTempSPFlag.IsAllowed = false;
			AssertEquals("HasModifyDetailsOrgTypeTempSPFlag should NOT be allowed", false, testSecurityProvider.HasModifyDetailsOrgTypeTempSPFlag);
			AssertEquals("HasModifyDetailsOrgTypeTempSPFlag should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifyDetailsOrgTypeTempSPFlag);

			Env.Security.OrgDetailsModifyOrgTypeTempSPFlag.IsAllowed = true;
			AssertEquals("HasModifyDetailsOrgTypeTempSPFlag should be allowed", true, testSecurityProvider.HasModifyDetailsOrgTypeTempSPFlag);
			AssertEquals("HasModifyDetailsOrgTypeTempSPFlag should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifyDetailsOrgTypeTempSPFlag);
		}

		#endregion

		#region TestHasModifyDetailsOrgTypeFlagCon

		public void TestHasModifyDetailsOrgTypeFlagCon()
		{
			Env.Security.OrgDetailsModifyOrgTypeFlagCon.IsAllowed = false;
			AssertEquals("HasModifyDetailsOrgTypeFlagCon should NOT be allowed", false, testSecurityProvider.HasModifyDetailsOrgTypeFlagCon);
			AssertEquals("HasModifyDetailsOrgTypeFlagCon should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifyDetailsOrgTypeFlagCon);

			Env.Security.OrgDetailsModifyOrgTypeFlagCon.IsAllowed = true;
			AssertEquals("HasModifyDetailsOrgTypeFlagCon should be allowed", true, testSecurityProvider.HasModifyDetailsOrgTypeFlagCon);
			AssertEquals("HasModifyDetailsOrgTypeFlagCon should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifyDetailsOrgTypeFlagCon);
		}

		#endregion

		#region TestHasModifyDetailsOrgTypeTempConFlag

		public void TestHasModifyDetailsOrgTypeTempConFlag()
		{
			Env.Security.OrgDetailsModifyOrgTypeTempConFlag.IsAllowed = false;
			AssertEquals("HasModifyDetailsOrgTypeTempConFlag should NOT be allowed", false, testSecurityProvider.HasModifyDetailsOrgTypeTempConFlag);
			AssertEquals("HasModifyDetailsOrgTypeTempConFlag should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifyDetailsOrgTypeTempConFlag);

			Env.Security.OrgDetailsModifyOrgTypeTempConFlag.IsAllowed = true;
			AssertEquals("HasModifyDetailsOrgTypeTempConFlag should be allowed", true, testSecurityProvider.HasModifyDetailsOrgTypeTempConFlag);
			AssertEquals("HasModifyDetailsOrgTypeTempConFlag should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifyDetailsOrgTypeTempConFlag);
		}

		#endregion

		#region TestHasModifyDetailsOrgTypeFlagTC

		public void TestHasModifyDetailsOrgTypeFlagTC()
		{
			Env.Security.OrgDetailsModifyOrgTypeFlagTC.IsAllowed = false;
			AssertEquals("HasModifyDetailsOrgTypeFlagTC should NOT be allowed", false, testSecurityProvider.HasModifyDetailsOrgTypeFlagTC);
			AssertEquals("HasModifyDetailsOrgTypeFlagTC should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifyDetailsOrgTypeFlagTC);

			Env.Security.OrgDetailsModifyOrgTypeFlagTC.IsAllowed = true;
			AssertEquals("HasModifyDetailsOrgTypeFlagTC should be allowed", true, testSecurityProvider.HasModifyDetailsOrgTypeFlagTC);
			AssertEquals("HasModifyDetailsOrgTypeFlagTC should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifyDetailsOrgTypeFlagTC);
		}

		#endregion

		#region TestHasModifyDetailsOrgTypeTempTCFlag

		public void TestHasModifyDetailsOrgTypeTempTCFlag()
		{
			Env.Security.OrgDetailsModifyOrgTypeTemlTCFlag.IsAllowed = false;
			AssertEquals("HasModifyDetailsOrgTypeTempTCFlag should NOT be allowed", false, testSecurityProvider.HasModifyDetailsOrgTypeTempTCFlag);
			AssertEquals("HasModifyDetailsOrgTypeTempTCFlag should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifyDetailsOrgTypeTempTCFlag);

			Env.Security.OrgDetailsModifyOrgTypeTemlTCFlag.IsAllowed = true;
			AssertEquals("HasModifyDetailsOrgTypeTempTCFlag should be allowed", true, testSecurityProvider.HasModifyDetailsOrgTypeTempTCFlag);
			AssertEquals("HasModifyDetailsOrgTypeTempTCFlag should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifyDetailsOrgTypeTempTCFlag);
		}

		#endregion

		#region TestHasModifyDetailsOrgTypeFlagWH

		public void TestHasModifyDetailsOrgTypeFlagWH()
		{
			Env.Security.OrgDetailsModifyOrgTypeFlagWH.IsAllowed = false;
			AssertEquals("HasModifyDetailsOrgTypeFlagWH should NOT be allowed", false, testSecurityProvider.HasModifyDetailsOrgTypeFlagWH);
			AssertEquals("HasModifyDetailsOrgTypeFlagWH should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifyDetailsOrgTypeFlagWH);

			Env.Security.OrgDetailsModifyOrgTypeFlagWH.IsAllowed = true;
			AssertEquals("HasModifyDetailsOrgTypeFlagWH should be allowed", true, testSecurityProvider.HasModifyDetailsOrgTypeFlagWH);
			AssertEquals("HasModifyDetailsOrgTypeFlagWH should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifyDetailsOrgTypeFlagWH);
		}

		#endregion

		#region TestHasModifyDetailsOrgTypeTempWHFlag

		public void TestHasModifyDetailsOrgTypeTempWHFlag()
		{
			Env.Security.OrgDetailsModifyOrgTypeTempWHFlag.IsAllowed = false;
			AssertEquals("HasModifyDetailsOrgTypeTempWHFlag should NOT be allowed", false, testSecurityProvider.HasModifyDetailsOrgTypeTempWHFlag);
			AssertEquals("HasModifyDetailsOrgTypeTempWHFlag should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifyDetailsOrgTypeTempWHFlag);

			Env.Security.OrgDetailsModifyOrgTypeTempWHFlag.IsAllowed = true;
			AssertEquals("HasModifyDetailsOrgTypeTempWHFlag should be allowed", true, testSecurityProvider.HasModifyDetailsOrgTypeTempWHFlag);
			AssertEquals("HasModifyDetailsOrgTypeTempWHFlag should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifyDetailsOrgTypeTempWHFlag);
		}

		#endregion

		#region TestHasModifyDetailsOrgTypeFlagCrr

		public void TestHasModifyDetailsOrgTypeFlagCrr()
		{
			Env.Security.OrgDetailsModifyOrgTypeFlagCrr.IsAllowed = false;
			AssertEquals("HasModifyDetailsOrgTypeFlagCrr should NOT be allowed", false, testSecurityProvider.HasModifyDetailsOrgTypeFlagCrr);
			AssertEquals("HasModifyDetailsOrgTypeFlagCrr should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifyDetailsOrgTypeFlagCrr);

			Env.Security.OrgDetailsModifyOrgTypeFlagCrr.IsAllowed = true;
			AssertEquals("HasModifyDetailsOrgTypeFlagCrr should be allowed", true, testSecurityProvider.HasModifyDetailsOrgTypeFlagCrr);
			AssertEquals("HasModifyDetailsOrgTypeFlagCrr should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifyDetailsOrgTypeFlagCrr);
		}

		#endregion

		#region TestHasModifyDetailsOrgTypeFlagFA

		public void TestHasModifyDetailsOrgTypeFlagFA()
		{
			Env.Security.OrgDetailsModifyOrgTypeFlagFA.IsAllowed = false;
			AssertEquals("HasModifyDetailsOrgTypeFlagFA should NOT be allowed", false, testSecurityProvider.HasModifyDetailsOrgTypeFlagFA);
			AssertEquals("HasModifyDetailsOrgTypeFlagFA should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifyDetailsOrgTypeFlagFA);

			Env.Security.OrgDetailsModifyOrgTypeFlagFA.IsAllowed = true;
			AssertEquals("HasModifyDetailsOrgTypeFlagFA should be allowed", true, testSecurityProvider.HasModifyDetailsOrgTypeFlagFA);
			AssertEquals("HasModifyDetailsOrgTypeFlagFA should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifyDetailsOrgTypeFlagFA);
		}

		#endregion

		#region TestHasModifyDetailsOrgTypeTempFAFlag

		public void TestHasModifyDetailsOrgTypeTempFAFlag()
		{
			Env.Security.OrgDetailsModifyOrgTypeTempFAFlag.IsAllowed = false;
			AssertEquals("HasModifyDetailsOrgTypeTempFAFlag should NOT be allowed", false, testSecurityProvider.HasModifyDetailsOrgTypeTempFAFlag);
			AssertEquals("HasModifyDetailsOrgTypeTempFAFlag should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifyDetailsOrgTypeTempFAFlag);

			Env.Security.OrgDetailsModifyOrgTypeTempFAFlag.IsAllowed = true;
			AssertEquals("HasModifyDetailsOrgTypeTempFAFlag should be allowed", true, testSecurityProvider.HasModifyDetailsOrgTypeTempFAFlag);
			AssertEquals("HasModifyDetailsOrgTypeTempFAFlag should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifyDetailsOrgTypeTempFAFlag);
		}

		#endregion

		#region TestHasModifyDetailsOrgTypeFlagBR

		public void TestHasModifyDetailsOrgTypeFlagBR()
		{
			Env.Security.OrgDetailsModifyOrgTypeFlagBR.IsAllowed = false;
			AssertEquals("HasModifyDetailsOrgTypeFlagBR should NOT be allowed", false, testSecurityProvider.HasModifyDetailsOrgTypeFlagBR);
			AssertEquals("HasModifyDetailsOrgTypeFlagBR should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifyDetailsOrgTypeFlagBR);

			Env.Security.OrgDetailsModifyOrgTypeFlagBR.IsAllowed = true;
			AssertEquals("HasModifyDetailsOrgTypeFlagBR should be allowed", true, testSecurityProvider.HasModifyDetailsOrgTypeFlagBR);
			AssertEquals("HasModifyDetailsOrgTypeFlagBR should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifyDetailsOrgTypeFlagBR);
		}

		#endregion

		#region TestHasModifyDetailsOrgTypeTempBRFlag

		public void TestHasModifyDetailsOrgTypeTempBRFlag()
		{
			Env.Security.OrgDetailsModifyOrgTypeTempBRFlag.IsAllowed = false;
			AssertEquals("HasModifyDetailsOrgTypeTempBRFlag should NOT be allowed", false, testSecurityProvider.HasModifyDetailsOrgTypeTempBRFlag);
			AssertEquals("HasModifyDetailsOrgTypeTempBRFlag should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifyDetailsOrgTypeTempBRFlag);

			Env.Security.OrgDetailsModifyOrgTypeTempBRFlag.IsAllowed = true;
			AssertEquals("HasModifyDetailsOrgTypeTempBRFlag should be allowed", true, testSecurityProvider.HasModifyDetailsOrgTypeTempBRFlag);
			AssertEquals("HasModifyDetailsOrgTypeTempBRFlag should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifyDetailsOrgTypeTempBRFlag);
		}

		#endregion

		#region TestHasModifyDetailsOrgTypeFlagSV

		public void TestHasModifyDetailsOrgTypeFlagSV()
		{
			Env.Security.OrgDetailsModifyOrgTypeFlagSV.IsAllowed = false;
			AssertEquals("HasModifyDetailsOrgTypeFlagSV should NOT be allowed", false, testSecurityProvider.HasModifyDetailsOrgTypeFlagSV);
			AssertEquals("HasModifyDetailsOrgTypeFlagSV should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifyDetailsOrgTypeFlagSV);

			Env.Security.OrgDetailsModifyOrgTypeFlagSV.IsAllowed = true;
			AssertEquals("HasModifyDetailsOrgTypeFlagSV should be allowed", true, testSecurityProvider.HasModifyDetailsOrgTypeFlagSV);
			AssertEquals("HasModifyDetailsOrgTypeFlagSV should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifyDetailsOrgTypeFlagSV);
		}

		#endregion

		#region TestHasModifyDetailsOrgTypeTempSVFlag

		public void TestHasModifyDetailsOrgTypeTempSVFlag()
		{
			Env.Security.OrgDetailsModifyOrgTypeTempSVFlag.IsAllowed = false;
			AssertEquals("HasModifyDetailsOrgTypeTempSVFlag should NOT be allowed", false, testSecurityProvider.HasModifyDetailsOrgTypeTempSVFlag);
			AssertEquals("HasModifyDetailsOrgTypeTempSVFlag should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifyDetailsOrgTypeTempSVFlag);

			Env.Security.OrgDetailsModifyOrgTypeTempSVFlag.IsAllowed = true;
			AssertEquals("HasModifyDetailsOrgTypeTempSVFlag should be allowed", true, testSecurityProvider.HasModifyDetailsOrgTypeTempSVFlag);
			AssertEquals("HasModifyDetailsOrgTypeTempSVFlag should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifyDetailsOrgTypeTempSVFlag);
		}

		#endregion

		#region TestHasModifyDetailsOrgTypeFlagCM

		public void TestHasModifyDetailsOrgTypeFlagCM()
		{
			Env.Security.OrgDetailsModifyOrgTypeFlagCM.IsAllowed = false;
			AssertEquals("HasModifyDetailsOrgTypeFlagCM should NOT be allowed", false, testSecurityProvider.HasModifyDetailsOrgTypeFlagCM);
			AssertEquals("HasModifyDetailsOrgTypeFlagCM should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifyDetailsOrgTypeFlagCM);

			Env.Security.OrgDetailsModifyOrgTypeFlagCM.IsAllowed = true;
			AssertEquals("HasModifyDetailsOrgTypeFlagCM should be allowed", true, testSecurityProvider.HasModifyDetailsOrgTypeFlagCM);
			AssertEquals("HasModifyDetailsOrgTypeFlagCM should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifyDetailsOrgTypeFlagCM);
		}

		#endregion

		#region TestHasModifyDetailsOrgTypeTempCMFlag

		public void TestHasModifyDetailsOrgTypeTempCMFlag()
		{
			Env.Security.OrgDetailsModifyOrgTypeTempCMFlag.IsAllowed = false;
			AssertEquals("HasModifyDetailsOrgTypeTempCMFlag should NOT be allowed", false, testSecurityProvider.HasModifyDetailsOrgTypeTempCMFlag);
			AssertEquals("HasModifyDetailsOrgTypeTempCMFlag should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifyDetailsOrgTypeTempCMFlag);

			Env.Security.OrgDetailsModifyOrgTypeTempCMFlag.IsAllowed = true;
			AssertEquals("HasModifyDetailsOrgTypeTempCMFlag should be allowed", true, testSecurityProvider.HasModifyDetailsOrgTypeTempCMFlag);
			AssertEquals("HasModifyDetailsOrgTypeTempCMFlag should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifyDetailsOrgTypeTempCMFlag);
		}

		#endregion

		#region TestHasModifyDetailsOrgTypeFlagSal

		public void TestHasModifyDetailsOrgTypeFlagSal()
		{
			Env.Security.OrgDetailsModifyOrgTypeFlagSal.IsAllowed = false;
			AssertEquals("HasModifyDetailsOrgTypeFlagSal should NOT be allowed", false, testSecurityProvider.HasModifyDetailsOrgTypeFlagSal);
			AssertEquals("HasModifyDetailsOrgTypeFlagSal should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifyDetailsOrgTypeFlagSal);

			Env.Security.OrgDetailsModifyOrgTypeFlagSal.IsAllowed = true;
			AssertEquals("HasModifyDetailsOrgTypeFlagSal should be allowed", true, testSecurityProvider.HasModifyDetailsOrgTypeFlagSal);
			AssertEquals("HasModifyDetailsOrgTypeFlagSal should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifyDetailsOrgTypeFlagSal);
		}

		#endregion

		#region TestHasModifyDetailsOrgTypeTempSalFlag

		public void TestHasModifyDetailsOrgTypeTempSalFlag()
		{
			Env.Security.OrgDetailsModifyOrgTypeTempSalFlag.IsAllowed = false;
			AssertEquals("HasModifyDetailsOrgTypeTempSalFlag should NOT be allowed", false, testSecurityProvider.HasModifyDetailsOrgTypeTempSalFlag);
			AssertEquals("HasModifyDetailsOrgTypeTempSalFlag should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifyDetailsOrgTypeTempSalFlag);

			Env.Security.OrgDetailsModifyOrgTypeTempSalFlag.IsAllowed = true;
			AssertEquals("HasModifyDetailsOrgTypeTempSalFlag should be allowed", true, testSecurityProvider.HasModifyDetailsOrgTypeTempSalFlag);
			AssertEquals("HasModifyDetailsOrgTypeTempSalFlag should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifyDetailsOrgTypeTempSalFlag);
		}

		#endregion

		#region TestHasModifyDetailsOrgTypeFlagCtrlAgent

		public void TestHasModifyDetailsOrgTypeFlagCtrlAgent()
		{
			Env.Security.OrgDetailsModifyOrgTypeFlagCtrlAgent.IsAllowed = false;
			AssertEquals("HasModifyDetailsOrgTypeFlagCtrlAgent should NOT be allowed", false, testSecurityProvider.HasModifyDetailsOrgTypeFlagCtrlAgent);
			AssertEquals("HasModifyDetailsOrgTypeFlagCtrlAgent should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifyDetailsOrgTypeFlagCtrlAgent);

			Env.Security.OrgDetailsModifyOrgTypeFlagCtrlAgent.IsAllowed = true;
			AssertEquals("HasModifyDetailsOrgTypeFlagCtrlAgent should be allowed", true, testSecurityProvider.HasModifyDetailsOrgTypeFlagCtrlAgent);
			AssertEquals("HasModifyDetailsOrgTypeFlagCtrlAgent should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifyDetailsOrgTypeFlagCtrlAgent);
		}

		#endregion

		#region TestHasModifyDetailsOrgTypeFlagCtrlCustomer

		public void TestHasModifyDetailsOrgTypeFlagCtrlCustomer()
		{
			Env.Security.OrgDetailsModifyOrgTypeFlagCtrlCustomer.IsAllowed = false;
			AssertEquals("HasModifyDetailsOrgTypeFlagCtrlCustomer should NOT be allowed", false, testSecurityProvider.HasModifyDetailsOrgTypeFlagCtrlCustomer);
			AssertEquals("HasModifyDetailsOrgTypeFlagCtrlCustomer should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifyDetailsOrgTypeFlagCtrlCustomer);

			Env.Security.OrgDetailsModifyOrgTypeFlagCtrlCustomer.IsAllowed = true;
			AssertEquals("HasModifyDetailsOrgTypeFlagCtrlCustomer should be allowed", true, testSecurityProvider.HasModifyDetailsOrgTypeFlagCtrlCustomer);
			AssertEquals("HasModifyDetailsOrgTypeFlagCtrlCustomer should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifyDetailsOrgTypeFlagCtrlCustomer);
		}

		#endregion

		#region TestHasModifyDetailsOrgTypeFlagCtrlCustomerWithoutCtrlAgent

		public void TestHasModifyDetailsOrgTypeFlagCtrlCustomerWithoutCtrlAgent()
		{
			Env.Security.OrgDetailsModifyOrgTypeFlagCtrlCustomerWithoutAgt.IsAllowed = false;
			AssertEquals("HasModifyDetailsOrgTypeFlagCtrlCustomerWithoutCtrlAgent should NOT be allowed", false, testSecurityProvider.HasModifyDetailsOrgTypeFlagCtrlCustomerWithoutCtrlAgent);
			AssertEquals("HasModifyDetailsOrgTypeFlagCtrlCustomerWithoutCtrlAgent should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifyDetailsOrgTypeFlagCtrlCustomerWithoutCtrlAgent);

			Env.Security.OrgDetailsModifyOrgTypeFlagCtrlCustomerWithoutAgt.IsAllowed = true;
			AssertEquals("HasModifyDetailsOrgTypeFlagCtrlCustomerWithoutCtrlAgent should be allowed", true, testSecurityProvider.HasModifyDetailsOrgTypeFlagCtrlCustomerWithoutCtrlAgent);
			AssertEquals("HasModifyDetailsOrgTypeFlagCtrlCustomerWithoutCtrlAgent should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifyDetailsOrgTypeFlagCtrlCustomerWithoutCtrlAgent);
		}

		#endregion

		#region TestHasModifyDetailsSecurity

		public void TestHasModifyDetailsSecurity()
		{
			Env.Security.OrgDetailsModify.IsAllowed = false;
			AssertEquals("HasModifyDetailsSecurity should NOT be allowed", false, testSecurityProvider.HasModifyDetailsSecurity);
			AssertEquals("HasModifyDetailsSecurity should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifyDetailsSecurity);

			Env.Security.OrgDetailsModify.IsAllowed = true;
			AssertEquals("HasModifyDetailsSecurity should be allowed", true, testSecurityProvider.HasModifyDetailsSecurity);
			AssertEquals("HasModifyDetailsSecurity should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifyDetailsSecurity);
		}

		#endregion

		#region TestHasModifyDetailsRelatedPartiesSecurity

		public void TestHasModifyDetailsRelatedPartiesSecurity()
		{
			Env.Security.OrgDetailsModifyRelatedParties.IsAllowed = false;
			AssertEquals("HasModifyDetailsRelatedPartiesSecurity should NOT be allowed", false, testSecurityProvider.HasModifyDetailsRelatedPartiesSecurity);
			AssertEquals("HasModifyDetailsRelatedPartiesSecurity should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifyDetailsRelatedPartiesSecurity);

			Env.Security.OrgDetailsModifyRelatedParties.IsAllowed = true;
			AssertEquals("HasModifyDetailsRelatedPartiesSecurity should be allowed", true, testSecurityProvider.HasModifyDetailsRelatedPartiesSecurity);
			AssertEquals("HasModifyDetailsRelatedPartiesSecurity should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifyDetailsRelatedPartiesSecurity);
		}

		#endregion

		#region TestHasModifyDetailsFinancialRelatedPartiesSecurity

		public void TestHasModifyDetailsFinancialRelatedPartiesSecurity()
		{
			Env.Security.OrgDetailsModifyFinancialRelatedParties.IsAllowed = false;
			AssertEquals("HasModifyDetailsFinancialRelatedPartiesSecurity should NOT be allowed", false, testSecurityProvider.HasModifyDetailsFinancialRelatedPartiesSecurity);
			AssertEquals("HasModifyDetailsFinancialRelatedPartiesSecurity should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifyDetailsFinancialRelatedPartiesSecurity);

			Env.Security.OrgDetailsModifyFinancialRelatedParties.IsAllowed = true;
			AssertEquals("HasModifyDetailsFinancialRelatedPartiesSecurity should be allowed", true, testSecurityProvider.HasModifyDetailsFinancialRelatedPartiesSecurity);
			AssertEquals("HasModifyDetailsFinancialRelatedPartiesSecurity should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifyDetailsFinancialRelatedPartiesSecurity);
		}

		#endregion

		#region TestHasModifyDetailsNonFinancialRelatedPartiesSecurity

		public void TestHasModifyDetailsNonFinancialRelatedPartiesSecurity()
		{
			Env.Security.OrgDetailsModifyNonFinancialRelatedParties.IsAllowed = false;
			AssertEquals("HasModifyDetailsNonFinancialRelatedPartiesSecurity should NOT be allowed", false, testSecurityProvider.HasModifyDetailsNonFinancialRelatedPartiesSecurity);
			AssertEquals("HasModifyDetailsNonFinancialRelatedPartiesSecurity should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifyDetailsNonFinancialRelatedPartiesSecurity);

			Env.Security.OrgDetailsModifyNonFinancialRelatedParties.IsAllowed = true;
			AssertEquals("HasModifyDetailsNonFinancialRelatedPartiesSecurity should be allowed", true, testSecurityProvider.HasModifyDetailsNonFinancialRelatedPartiesSecurity);
			AssertEquals("HasModifyDetailsNonFinancialRelatedPartiesSecurity should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifyDetailsNonFinancialRelatedPartiesSecurity);
		}

		#endregion

		#region TestHasModifyDetailsControllingAgentRelatedPartyToAnyOrgSecurity

		public void TestHasModifyDetailsControllingAgentRelatedPartyToAnyOrgSecurity()
		{
			OrganisationsDataRegistry.Instance.EnableControllingAgentFunctionalityAndValidations.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			Env.Security.OrgDetailsNewModifyCtrlAgentRelatedPartyToAnyOrg.IsAllowed = false;
			Env.Security.OrgDetailsModifyCtrlAgentRelatedPartyToAnyOrg.IsAllowed = false;
			AssertEquals("HasModifyDetailsControllingAgentRelatedPartyToAnyOrgSecurity should NOT be allowed (not saved)", false, testUnsavedSecurityProvider.HasModifyDetailsControllingAgentRelatedPartyToAnyOrgSecurity);
			AssertEquals("HasModifyDetailsControllingAgentRelatedPartyToAnyOrgSecurity should NOT be allowed", false, testSecurityProvider.HasModifyDetailsControllingAgentRelatedPartyToAnyOrgSecurity);

			Env.Security.OrgDetailsNewModifyCtrlAgentRelatedPartyToAnyOrg.IsAllowed = false;
			Env.Security.OrgDetailsModifyCtrlAgentRelatedPartyToAnyOrg.IsAllowed = true;
			AssertEquals("HasModifyDetailsControllingAgentRelatedPartyToAnyOrgSecurity should NOT be allowed (not saved)", false, testUnsavedSecurityProvider.HasModifyDetailsControllingAgentRelatedPartyToAnyOrgSecurity);
			AssertEquals("HasModifyDetailsControllingAgentRelatedPartyToAnyOrgSecurity should be allowed", true, testSecurityProvider.HasModifyDetailsControllingAgentRelatedPartyToAnyOrgSecurity);

			Env.Security.OrgDetailsNewModifyCtrlAgentRelatedPartyToAnyOrg.IsAllowed = true;
			Env.Security.OrgDetailsModifyCtrlAgentRelatedPartyToAnyOrg.IsAllowed = false;
			AssertEquals("HasModifyDetailsControllingAgentRelatedPartyToAnyOrgSecurity should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifyDetailsControllingAgentRelatedPartyToAnyOrgSecurity);
			AssertEquals("HasModifyDetailsControllingAgentRelatedPartyToAnyOrgSecurity should NOT be allowed", false, testSecurityProvider.HasModifyDetailsControllingAgentRelatedPartyToAnyOrgSecurity);

			Env.Security.OrgDetailsNewModifyCtrlAgentRelatedPartyToAnyOrg.IsAllowed = true;
			Env.Security.OrgDetailsModifyCtrlAgentRelatedPartyToAnyOrg.IsAllowed = true;
			AssertEquals("HasModifyDetailsControllingAgentRelatedPartyToAnyOrgSecurity should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifyDetailsControllingAgentRelatedPartyToAnyOrgSecurity);
			AssertEquals("HasModifyDetailsControllingAgentRelatedPartyToAnyOrgSecurity should be allowed", true, testSecurityProvider.HasModifyDetailsControllingAgentRelatedPartyToAnyOrgSecurity);

			OrganisationsDataRegistry.Instance.EnableControllingAgentFunctionalityAndValidations.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			Env.Security.OrgDetailsNewModifyCtrlAgentRelatedPartyToAnyOrg.IsAllowed = false;
			Env.Security.OrgDetailsModifyCtrlAgentRelatedPartyToAnyOrg.IsAllowed = false;
			AssertEquals("HasModifyDetailsControllingAgentRelatedPartyToAnyOrgSecurity should be allowed (not saved, registry setting for Controlling Agent validation is OFF)", true, testUnsavedSecurityProvider.HasModifyDetailsControllingAgentRelatedPartyToAnyOrgSecurity);
			AssertEquals("HasModifyDetailsControllingAgentRelatedPartyToAnyOrgSecurity should be allowed (registry setting for Controlling Agent validation is OFF)", true, testSecurityProvider.HasModifyDetailsControllingAgentRelatedPartyToAnyOrgSecurity);

			Env.Security.OrgDetailsNewModifyCtrlAgentRelatedPartyToAnyOrg.IsAllowed = false;
			Env.Security.OrgDetailsModifyCtrlAgentRelatedPartyToAnyOrg.IsAllowed = true;
			AssertEquals("HasModifyDetailsControllingAgentRelatedPartyToAnyOrgSecurity should be allowed (not saved, registry setting for Controlling Agent validation is OFF)", true, testUnsavedSecurityProvider.HasModifyDetailsControllingAgentRelatedPartyToAnyOrgSecurity);
			AssertEquals("HasModifyDetailsControllingAgentRelatedPartyToAnyOrgSecurity should be allowed (registry setting for Controlling Agent validation is OFF)", true, testSecurityProvider.HasModifyDetailsControllingAgentRelatedPartyToAnyOrgSecurity);

			Env.Security.OrgDetailsNewModifyCtrlAgentRelatedPartyToAnyOrg.IsAllowed = true;
			Env.Security.OrgDetailsModifyCtrlAgentRelatedPartyToAnyOrg.IsAllowed = false;
			AssertEquals("HasModifyDetailsControllingAgentRelatedPartyToAnyOrgSecurity should be allowed (not saved, registry setting for Controlling Agent validation is OFF)", true, testUnsavedSecurityProvider.HasModifyDetailsControllingAgentRelatedPartyToAnyOrgSecurity);
			AssertEquals("HasModifyDetailsControllingAgentRelatedPartyToAnyOrgSecurity should be allowed (registry setting for Controlling Agent validation is OFF)", true, testSecurityProvider.HasModifyDetailsControllingAgentRelatedPartyToAnyOrgSecurity);

			Env.Security.OrgDetailsNewModifyCtrlAgentRelatedPartyToAnyOrg.IsAllowed = true;
			Env.Security.OrgDetailsModifyCtrlAgentRelatedPartyToAnyOrg.IsAllowed = true;
			AssertEquals("HasModifyDetailsControllingAgentRelatedPartyToAnyOrgSecurity should be allowed (not saved, registry setting for Controlling Agent validation is OFF)", true, testUnsavedSecurityProvider.HasModifyDetailsControllingAgentRelatedPartyToAnyOrgSecurity);
			AssertEquals("HasModifyDetailsControllingAgentRelatedPartyToAnyOrgSecurity should be allowed (registry setting for Controlling Agent validation is OFF)", true, testSecurityProvider.HasModifyDetailsControllingAgentRelatedPartyToAnyOrgSecurity);
		}

		#endregion

		#region TestHasModifyDetailsControllingAgentRelatedPartyToOwnOrgSecurity

		public void TestHasModifyDetailsControllingAgentRelatedPartyToOwnOrgSecurity()
		{
			OrganisationsDataRegistry.Instance.EnableControllingAgentFunctionalityAndValidations.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			Env.Security.OrgDetailsNewModifyCtrlAgentRelatedPartyToOwnOrg.IsAllowed = false;
			Env.Security.OrgDetailsModifyCtrlAgentRelatedPartyToOwnOrg.IsAllowed = false;
			AssertEquals("HasModifyDetailsControllingAgentRelatedPartyToOwnOrgSecurity should NOT be allowed (not saved)", false, testUnsavedSecurityProvider.HasModifyDetailsControllingAgentRelatedPartyToOwnOrgSecurity);
			AssertEquals("HasModifyDetailsControllingAgentRelatedPartyToOwnOrgSecurity should NOT be allowed", false, testSecurityProvider.HasModifyDetailsControllingAgentRelatedPartyToOwnOrgSecurity);

			Env.Security.OrgDetailsNewModifyCtrlAgentRelatedPartyToOwnOrg.IsAllowed = false;
			Env.Security.OrgDetailsModifyCtrlAgentRelatedPartyToOwnOrg.IsAllowed = true;
			AssertEquals("HasModifyDetailsControllingAgentRelatedPartyToOwnOrgSecurity should NOT be allowed (not saved)", false, testUnsavedSecurityProvider.HasModifyDetailsControllingAgentRelatedPartyToOwnOrgSecurity);
			AssertEquals("HasModifyDetailsControllingAgentRelatedPartyToOwnOrgSecurity should be allowed", true, testSecurityProvider.HasModifyDetailsControllingAgentRelatedPartyToOwnOrgSecurity);

			Env.Security.OrgDetailsNewModifyCtrlAgentRelatedPartyToOwnOrg.IsAllowed = true;
			Env.Security.OrgDetailsModifyCtrlAgentRelatedPartyToOwnOrg.IsAllowed = false;
			AssertEquals("HasModifyDetailsControllingAgentRelatedPartyToOwnOrgSecurity should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifyDetailsControllingAgentRelatedPartyToOwnOrgSecurity);
			AssertEquals("HasModifyDetailsControllingAgentRelatedPartyToOwnOrgSecurity should NOT be allowed", false, testSecurityProvider.HasModifyDetailsControllingAgentRelatedPartyToOwnOrgSecurity);

			Env.Security.OrgDetailsNewModifyCtrlAgentRelatedPartyToOwnOrg.IsAllowed = true;
			Env.Security.OrgDetailsModifyCtrlAgentRelatedPartyToOwnOrg.IsAllowed = true;
			AssertEquals("HasModifyDetailsControllingAgentRelatedPartyToOwnOrgSecurity should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifyDetailsControllingAgentRelatedPartyToOwnOrgSecurity);
			AssertEquals("HasModifyDetailsControllingAgentRelatedPartyToOwnOrgSecurity should be allowed", true, testSecurityProvider.HasModifyDetailsControllingAgentRelatedPartyToOwnOrgSecurity);

			OrganisationsDataRegistry.Instance.EnableControllingAgentFunctionalityAndValidations.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			Env.Security.OrgDetailsNewModifyCtrlAgentRelatedPartyToOwnOrg.IsAllowed = false;
			Env.Security.OrgDetailsModifyCtrlAgentRelatedPartyToOwnOrg.IsAllowed = false;
			AssertEquals("HasModifyDetailsControllingAgentRelatedPartyToOwnOrgSecurity should be allowed (not saved, registry setting for Controlling Agent validation is OFF)", true, testUnsavedSecurityProvider.HasModifyDetailsControllingAgentRelatedPartyToOwnOrgSecurity);
			AssertEquals("HasModifyDetailsControllingAgentRelatedPartyToOwnOrgSecurity should be allowed (registry setting for Controlling Agent validation is OFF)", true, testSecurityProvider.HasModifyDetailsControllingAgentRelatedPartyToOwnOrgSecurity);

			Env.Security.OrgDetailsNewModifyCtrlAgentRelatedPartyToOwnOrg.IsAllowed = false;
			Env.Security.OrgDetailsModifyCtrlAgentRelatedPartyToOwnOrg.IsAllowed = true;
			AssertEquals("HasModifyDetailsControllingAgentRelatedPartyToOwnOrgSecurity should be allowed (not saved, registry setting for Controlling Agent validation is OFF)", true, testUnsavedSecurityProvider.HasModifyDetailsControllingAgentRelatedPartyToOwnOrgSecurity);
			AssertEquals("HasModifyDetailsControllingAgentRelatedPartyToOwnOrgSecurity should be allowed (registry setting for Controlling Agent validation is OFF)", true, testSecurityProvider.HasModifyDetailsControllingAgentRelatedPartyToOwnOrgSecurity);

			Env.Security.OrgDetailsNewModifyCtrlAgentRelatedPartyToOwnOrg.IsAllowed = true;
			Env.Security.OrgDetailsModifyCtrlAgentRelatedPartyToOwnOrg.IsAllowed = false;
			AssertEquals("HasModifyDetailsControllingAgentRelatedPartyToOwnOrgSecurity should be allowed (not saved, registry setting for Controlling Agent validation is OFF)", true, testUnsavedSecurityProvider.HasModifyDetailsControllingAgentRelatedPartyToOwnOrgSecurity);
			AssertEquals("HasModifyDetailsControllingAgentRelatedPartyToOwnOrgSecurity should be allowed (registry setting for Controlling Agent validation is OFF)", true, testSecurityProvider.HasModifyDetailsControllingAgentRelatedPartyToOwnOrgSecurity);

			Env.Security.OrgDetailsNewModifyCtrlAgentRelatedPartyToOwnOrg.IsAllowed = true;
			Env.Security.OrgDetailsModifyCtrlAgentRelatedPartyToOwnOrg.IsAllowed = true;
			AssertEquals("HasModifyDetailsControllingAgentRelatedPartyToOwnOrgSecurity should be allowed (not saved, registry setting for Controlling Agent validation is OFF)", true, testUnsavedSecurityProvider.HasModifyDetailsControllingAgentRelatedPartyToOwnOrgSecurity);
			AssertEquals("HasModifyDetailsControllingAgentRelatedPartyToOwnOrgSecurity should be allowed (registry setting for Controlling Agent validation is OFF)", true, testSecurityProvider.HasModifyDetailsControllingAgentRelatedPartyToOwnOrgSecurity);
		}

		#endregion

		#region TestHasModifyDetailsCategorySecurity

		public void TestHasModifyDetailsCategorySecurity()
		{
			Env.Security.OrgDetailsNewModifyCategory.IsAllowed = false;
			Env.Security.OrgDetailsModifyCategory.IsAllowed = false;
			AssertEquals("HasModifyDetailsNameAndAddressSecurity should NOT be allowed", false, testSecurityProvider.HasModifyDetailsCategorySecurity);
			AssertEquals("HasModifyDetailsNameAndAddressSecurity should be allowed (not saved)", false, testUnsavedSecurityProvider.HasModifyDetailsCategorySecurity);

			Env.Security.OrgDetailsNewModifyCategory.IsAllowed = true;
			Env.Security.OrgDetailsModifyCategory.IsAllowed = true;
			AssertEquals("HasModifyDetailsNameAndAddressSecurity should be allowed", true, testSecurityProvider.HasModifyDetailsCategorySecurity);
			AssertEquals("HasModifyDetailsNameAndAddressSecurity should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifyDetailsCategorySecurity);
		}

		#endregion

		#region TestHasModifyDetailsNameAndAddressSecurity

		public void TestHasModifyDetailsNameAndAddressSecurity()
		{
			Env.Security.OrgDetailsModifyNameAndAddress.IsAllowed = false;
			AssertEquals("HasModifyDetailsNameAndAddressSecurity should NOT be allowed", false, testSecurityProvider.HasModifyDetailsNameAndAddressSecurity);
			AssertEquals("HasModifyDetailsNameAndAddressSecurity should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifyDetailsNameAndAddressSecurity);

			Env.Security.OrgDetailsModifyNameAndAddress.IsAllowed = true;
			AssertEquals("HasModifyDetailsNameAndAddressSecurity should be allowed", true, testSecurityProvider.HasModifyDetailsNameAndAddressSecurity);
			AssertEquals("HasModifyDetailsNameAndAddressSecurity should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifyDetailsNameAndAddressSecurity);
		}

		#endregion

		#region TestHasModifyDetailsAddressShortCodeSecurity

		public void TestHasModifyDetailsAddressShortCodeSecurity()
		{
			Env.Security.OrgDetailsModifyAddressShortCode.IsAllowed = false;
			AssertEquals("HasModifyDetailsAddressShortCodeSecurity should NOT be allowed", false, testSecurityProvider.HasModifyDetailsAddressShortCodeSecurity);
			AssertEquals("HasModifyDetailsAddressShortCodeSecurity should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifyDetailsAddressShortCodeSecurity);

			Env.Security.OrgDetailsModifyAddressShortCode.IsAllowed = true;
			AssertEquals("HasModifyDetailsAddressShortCodeSecurity should be allowed", true, testSecurityProvider.HasModifyDetailsAddressShortCodeSecurity);
			AssertEquals("HasModifyDetailsAddressShortCodeSecurity should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifyDetailsAddressShortCodeSecurity);
		}

		#endregion

		#region TestHasModifyDetailsPhFaxWebDetailsSecurity

		public void TestHasModifyDetailsPhFaxWebDetailsSecurity()
		{
			Env.Security.OrgDetailsModifyPhFaxWebDetails.IsAllowed = false;
			AssertEquals("HasModifyDetailsPhFaxWebDetailsSecurity should NOT be allowed", false, testSecurityProvider.HasModifyDetailsPhFaxWebDetailsSecurity);
			AssertEquals("HasModifyDetailsPhFaxWebDetailsSecurity should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifyDetailsPhFaxWebDetailsSecurity);

			Env.Security.OrgDetailsModifyPhFaxWebDetails.IsAllowed = true;
			AssertEquals("HasModifyDetailsPhFaxWebDetailsSecurity should be allowed", true, testSecurityProvider.HasModifyDetailsPhFaxWebDetailsSecurity);
			AssertEquals("HasModifyDetailsPhFaxWebDetailsSecurity should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifyDetailsPhFaxWebDetailsSecurity);
		}

		#endregion

		#region TestHasModifyDetailsStaffAssignmentsSecurity

		public void TestHasModifyDetailsStaffAssignmentsSecurity()
		{
			Env.Security.OrgDetailsModifyStaffAssignments.IsAllowed = false;
			AssertEquals("HasModifyDetailsStaffAssignmentsSecurity should NOT be allowed", false, testSecurityProvider.HasModifyDetailsStaffAssignmentsSecurity);
			AssertEquals("HasModifyDetailsStaffAssignmentsSecurity should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifyDetailsStaffAssignmentsSecurity);

			Env.Security.OrgDetailsModifyStaffAssignments.IsAllowed = true;
			AssertEquals("HasModifyDetailsStaffAssignmentsSecurity should be allowed", true, testSecurityProvider.HasModifyDetailsStaffAssignmentsSecurity);
			AssertEquals("HasModifyDetailsStaffAssignmentsSecurity should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifyDetailsStaffAssignmentsSecurity);
		}

		#endregion

		#region TestHasModifyDetailsOtherCompanysStaffAssignmentsSecurity

		public void TestHasModifyDetailsOtherCompanysStaffAssignmentsSecurity()
		{
			Env.Security.OrgDetailsModifyOtherCompanysStaffAssignments.IsAllowed = false;
			AssertEquals("HasModifyDetailsOtherCompanysStaffAssignmentsSecurity should NOT be allowed", false, testSecurityProvider.HasModifyDetailsOtherCompanysStaffAssignmentsSecurity);
			AssertEquals("HasModifyDetailsOtherCompanysStaffAssignmentsSecurity should be allowed (not saved)", false, testUnsavedSecurityProvider.HasModifyDetailsOtherCompanysStaffAssignmentsSecurity);

			Env.Security.OrgDetailsModifyOtherCompanysStaffAssignments.IsAllowed = true;
			AssertEquals("HasModifyDetailsOtherCompanysStaffAssignmentsSecurity should be allowed", true, testSecurityProvider.HasModifyDetailsOtherCompanysStaffAssignmentsSecurity);
			AssertEquals("HasModifyDetailsOtherCompanysStaffAssignmentsSecurity should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifyDetailsOtherCompanysStaffAssignmentsSecurity);
		}

		#endregion

		#region TestHasModifyDetailsWebSecurity

		public void TestHasModifyDetailsWebSecurity()
		{
			Env.Security.OrgDetailsModifyWebSecurity.IsAllowed = false;
			AssertEquals("HasModifyDetailsWebSecurity should NOT be allowed", false, testSecurityProvider.HasModifyDetailsWebSecurity);
			AssertEquals("HasModifyDetailsWebSecurity should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifyDetailsWebSecurity);

			Env.Security.OrgDetailsModifyWebSecurity.IsAllowed = true;
			AssertEquals("HasModifyDetailsWebSecurity should be allowed", true, testSecurityProvider.HasModifyDetailsWebSecurity);
			AssertEquals("HasModifyDetailsWebSecurity should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifyDetailsWebSecurity);
		}

		#endregion

		#region TestHasModifyDetailsCustomFieldsSecurity

		public void TestHasModifyDetailsCustomFieldsSecurity()
		{
			Env.Security.OrgDetailsModifyCustomFields.IsAllowed = false;
			AssertEquals("HasModifyDetailsCustomFieldsSecurity should NOT be allowed", false, testSecurityProvider.HasModifyDetailsCustomFieldsSecurity);
			AssertEquals("HasModifyDetailsCustomFieldsSecurity should be allowed (not saved)", false, testUnsavedSecurityProvider.HasModifyDetailsCustomFieldsSecurity);

			Env.Security.OrgDetailsModifyCustomFields.IsAllowed = true;
			AssertEquals("HasModifyDetailsCustomSecurity should be allowed", true, testSecurityProvider.HasModifyDetailsCustomFieldsSecurity);
			AssertEquals("HasModifyDetailsCustomSecurity should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifyDetailsCustomFieldsSecurity);
		}

		#endregion

		#region TestHasNewDetailsWebSecurity

		public void TestHasNewDetailsWebSecurity()
		{
			Env.Security.OrgDetailsNewWebSecurity.IsAllowed = false;
			AssertEquals("HasNewDetailsWebSecurity should be allowed", true, testSecurityProvider.HasNewDetailsWebSecurity);
			AssertEquals("HasNewDetailsWebSecurity should NOT be allowed (not saved)", false, testUnsavedSecurityProvider.HasNewDetailsWebSecurity);

			Env.Security.OrgDetailsNewWebSecurity.IsAllowed = true;
			AssertEquals("HasNewDetailsWebSecurity should be allowed", true, testSecurityProvider.HasNewDetailsWebSecurity);
			AssertEquals("HasNewDetailsWebSecurity should be allowed (not saved)", true, testUnsavedSecurityProvider.HasNewDetailsWebSecurity);
		}

		#endregion

		#region TestHasNewDetailsAllowCreationOutsideLoginCountry

		public void TestHasNewDetailsAllowCreationOutsideLoginCountry()
		{
			Env.Security.OrgDetailsNewAllowCreationOutsideLoginCountry.IsAllowed = false;
			AssertEquals("HasNewDetailsAllowCreationOutsideLoginCountry should NOT be allowed", false, testSecurityProvider.HasNewDetailsAllowCreationOutsideLoginCountry);
			AssertEquals("HasNewDetailsAllowCreationOutsideLoginCountry should NOT be allowed (not saved)", false, testUnsavedSecurityProvider.HasNewDetailsAllowCreationOutsideLoginCountry);

			Env.Security.OrgDetailsNewAllowCreationOutsideLoginCountry.IsAllowed = true;
			AssertEquals("HasNewDetailsAllowCreationOutsideLoginCountry should be allowed", true, testSecurityProvider.HasNewDetailsAllowCreationOutsideLoginCountry);
			AssertEquals("HasNewDetailsAllowCreationOutsideLoginCountry should be allowed (not saved)", true, testUnsavedSecurityProvider.HasNewDetailsAllowCreationOutsideLoginCountry);
		}

		#endregion

		#region TestHasModifyDetailsRatingAndTariffsSecurity

		public void TestHasModifyDetailsRatingAndTariffsSecurity()
		{
			Env.Security.OrgDetailsModifyRatingAndTariffs.IsAllowed = false;
			AssertEquals("HasModifyDetailsRatingAndTariffsSecurity should NOT be allowed", false, testSecurityProvider.HasModifyDetailsRatingAndTariffsSecurity);
			AssertEquals("HasModifyDetailsRatingAndTariffsSecurity should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifyDetailsRatingAndTariffsSecurity);

			Env.Security.OrgDetailsModifyRatingAndTariffs.IsAllowed = true;
			AssertEquals("HasModifyDetailsRatingAndTariffsSecurity should be allowed", true, testSecurityProvider.HasModifyDetailsRatingAndTariffsSecurity);
			AssertEquals("HasModifyDetailsRatingAndTariffsSecurity should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifyDetailsRatingAndTariffsSecurity);
		}

		#endregion

		#region TestHasModifyDetailsOrganisationTypeSecurity

		public void TestHasModifyDetailsOrganisationTypeSecurity()
		{
			Env.Security.OrgDetailsModifyOrganisationType.IsAllowed = false;
			Env.Security.OrgDetailsNewOrganisationType.IsAllowed = false;
			AssertEquals("HasModifyDetailsOrganisationTypeSecurity should NOT be allowed", false, testSecurityProvider.HasModifyDetailsOrganisationTypeSecurity);
			AssertEquals("HasModifyDetailsOrganisationTypeSecurity should NOT be allowed (not saved)", false, testUnsavedSecurityProvider.HasModifyDetailsOrganisationTypeSecurity);

			Env.Security.OrgDetailsModifyOrganisationType.IsAllowed = true;
			Env.Security.OrgDetailsNewOrganisationType.IsAllowed = true;
			AssertEquals("HasModifyDetailsOrganisationTypeSecurity should be allowed", true, testSecurityProvider.HasModifyDetailsOrganisationTypeSecurity);
			AssertEquals("HasModifyDetailsOrganisationTypeSecurity should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifyDetailsOrganisationTypeSecurity);
		}

		#endregion

		#region TestHasModifyAddressSecurity

		public void TestHasModifyAddressSecurity()
		{
			Env.Security.OrgAddressModify.IsAllowed = false;
			AssertEquals("HasModifyAddressSecurity should NOT be allowed", false, testSecurityProvider.HasModifyAddressSecurity);
			AssertEquals("HasModifyAddressSecurity should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifyAddressSecurity);

			Env.Security.OrgAddressModify.IsAllowed = true;
			AssertEquals("HasModifyAddressSecurity should be allowed", true, testSecurityProvider.HasModifyAddressSecurity);
			AssertEquals("HasModifyAddressSecurity should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifyAddressSecurity);

			Env.Security.OrgAddressNew.IsAllowed = false;
			AssertEquals("HasModifyAddressSecurity should be allowed", true, testSecurityProvider.HasModifyAddressSecurity);
			AssertEquals("HasModifyAddressSecurity should NOT be allowed (not saved)", false, testUnsavedSecurityProvider.HasModifyAddressSecurity);

			Env.Security.OrgAddressNew.IsAllowed = true;
			AssertEquals("HasModifyAddressSecurity should be allowed", true, testSecurityProvider.HasModifyAddressSecurity);
			AssertEquals("HasModifyAddressSecurity should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifyAddressSecurity);
		}

		#endregion

		#region TestHasModifyAddressListSecurity

		public void TestHasModifyAddressListSecurity()
		{
			Env.Security.OrgAddressListModify.IsAllowed = false;
			AssertEquals("HasModifyAddressListSecurity should NOT be allowed", false, testSecurityProvider.HasModifyAddressListSecurity);
			AssertEquals("HasModifyAddressListSecurity should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifyAddressListSecurity);

			Env.Security.OrgAddressListModify.IsAllowed = true;
			AssertEquals("HasModifyAddressListSecurity should be allowed", true, testSecurityProvider.HasModifyAddressListSecurity);
			AssertEquals("HasModifyAddressListSecurity should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifyAddressListSecurity);

			Env.Security.OrgAddressListNew.IsAllowed = false;
			AssertEquals("HasModifyAddressListSecurity should be allowed", true, testSecurityProvider.HasModifyAddressListSecurity);
			AssertEquals("HasModifyAddressListSecurity should NOT be allowed (not saved)", false, testUnsavedSecurityProvider.HasModifyAddressListSecurity);

			Env.Security.OrgAddressListNew.IsAllowed = true;
			AssertEquals("HasModifyAddressListSecurity should be allowed", true, testSecurityProvider.HasModifyAddressListSecurity);
			AssertEquals("HasModifyAddressListSecurity should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifyAddressListSecurity);
		}

		#endregion

		#region TestHasModifyAddressDetailsSecurity

		public void TestHasModifyAddressDetailsSecurity()
		{
			Env.Security.OrgAddressDetailsModify.IsAllowed = false;
			AssertEquals("HasModifyAddressDetailsSecurity should NOT be allowed", false, testSecurityProvider.HasModifyAddressDetailsSecurity);
			AssertEquals("HasModifyAddressDetailsSecurity should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifyAddressDetailsSecurity);

			Env.Security.OrgAddressDetailsModify.IsAllowed = true;
			AssertEquals("HasModifyAddressDetailsSecurity should be allowed", true, testSecurityProvider.HasModifyAddressDetailsSecurity);
			AssertEquals("HasModifyAddressDetailsSecurity should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifyAddressDetailsSecurity);

			Env.Security.OrgAddressDetailsNew.IsAllowed = false;
			AssertEquals("HasModifyAddressDetailsSecurity should be allowed", true, testSecurityProvider.HasModifyAddressDetailsSecurity);
			AssertEquals("HasModifyAddressDetailsSecurity should NOT be allowed (not saved)", false, testUnsavedSecurityProvider.HasModifyAddressDetailsSecurity);

			Env.Security.OrgAddressDetailsNew.IsAllowed = true;
			AssertEquals("HasModifyAddressDetailsSecurity should be allowed", true, testSecurityProvider.HasModifyAddressDetailsSecurity);
			AssertEquals("HasModifyAddressDetailsSecurity should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifyAddressDetailsSecurity);
		}

		#endregion

		#region TestHasModifyAddressShortCodeSecurity

		public void TestHasModifyAddressShortCodeSecurity()
		{
			Env.Security.OrgAddressShortCodeModify.IsAllowed = false;
			AssertEquals("HasModifyAddressShortCodeSecurity should NOT be allowed", false, testSecurityProvider.HasModifyAddressShortCodeSecurity);
			AssertEquals("HasModifyAddressShortCodeSecurity should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifyAddressShortCodeSecurity);

			Env.Security.OrgAddressShortCodeModify.IsAllowed = true;
			AssertEquals("HasModifyAddressShortCodeSecurity should be allowed", true, testSecurityProvider.HasModifyAddressShortCodeSecurity);
			AssertEquals("HasModifyAddressShortCodeSecurity should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifyAddressShortCodeSecurity);

			Env.Security.OrgAddressShortCodeNew.IsAllowed = false;
			AssertEquals("HasModifyAddressShortCodeSecurity should be allowed", true, testSecurityProvider.HasModifyAddressShortCodeSecurity);
			AssertEquals("HasModifyAddressShortCodeSecurity should NOT be allowed (not saved)", false, testUnsavedSecurityProvider.HasModifyAddressShortCodeSecurity);

			Env.Security.OrgAddressShortCodeNew.IsAllowed = true;
			AssertEquals("HasModifyAddressShortCodeSecurity should be allowed", true, testSecurityProvider.HasModifyAddressShortCodeSecurity);
			AssertEquals("HasModifyAddressShortCodeSecurity should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifyAddressShortCodeSecurity);
		}

		#endregion

		#region TestHasModifyAddressCapabilitiesSecurity

		public void TestHasModifyAddressCapabilitiesSecurity()
		{
			Env.Security.OrgAddressCapabilitiesModify.IsAllowed = false;
			AssertEquals("HasModifyAddressCapabilitiesSecurity should NOT be allowed", false, testSecurityProvider.HasModifyAddressCapabilitiesSecurity);
			AssertEquals("HasModifyAddressCapabilitiesSecurity should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifyAddressCapabilitiesSecurity);

			Env.Security.OrgAddressCapabilitiesModify.IsAllowed = true;
			AssertEquals("HasModifyAddressCapabilitiesSecurity should be allowed", true, testSecurityProvider.HasModifyAddressCapabilitiesSecurity);
			AssertEquals("HasModifyAddressCapabilitiesSecurity should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifyAddressCapabilitiesSecurity);

			Env.Security.OrgAddressCapabilitiesNew.IsAllowed = false;
			AssertEquals("HasModifyAddressCapabilitiesSecurity should be allowed", true, testSecurityProvider.HasModifyAddressCapabilitiesSecurity);
			AssertEquals("HasModifyAddressCapabilitiesSecurity should NOT be allowed (not saved)", false, testUnsavedSecurityProvider.HasModifyAddressCapabilitiesSecurity);

			Env.Security.OrgAddressCapabilitiesNew.IsAllowed = true;
			AssertEquals("HasModifyAddressCapabilitiesSecurity should be allowed", true, testSecurityProvider.HasModifyAddressCapabilitiesSecurity);
			AssertEquals("HasModifyAddressCapabilitiesSecurity should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifyAddressCapabilitiesSecurity);
		}

		#endregion

		#region TestHasModifyAddressAdditionalDetailsSecurity

		public void TestHasModifyAddressAdditionalDetailsSecurity()
		{
			Env.Security.OrgAddressAdditionalDetailsModify.IsAllowed = false;
			AssertEquals("HasModifyAddressAdditionalDetailsSecurity should NOT be allowed", false, testSecurityProvider.HasModifyAddressAdditionalDetailsSecurity);
			AssertEquals("HasModifyAddressAdditionalDetailsSecurity should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifyAddressAdditionalDetailsSecurity);

			Env.Security.OrgAddressAdditionalDetailsModify.IsAllowed = true;
			AssertEquals("HasModifyAddressAdditionalDetailsSecurity should be allowed", true, testSecurityProvider.HasModifyAddressAdditionalDetailsSecurity);
			AssertEquals("HasModifyAddressAdditionalDetailsSecurity should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifyAddressAdditionalDetailsSecurity);

			Env.Security.OrgAddressAdditionalDetailsNew.IsAllowed = false;
			AssertEquals("HasModifyAddressAdditionalDetailsSecurity should be allowed", true, testSecurityProvider.HasModifyAddressAdditionalDetailsSecurity);
			AssertEquals("HasModifyAddressAdditionalDetailsSecurity should NOT be allowed (not saved)", false, testUnsavedSecurityProvider.HasModifyAddressAdditionalDetailsSecurity);

			Env.Security.OrgAddressAdditionalDetailsNew.IsAllowed = true;
			AssertEquals("HasModifyAddressAdditionalDetailsSecurity should be allowed", true, testSecurityProvider.HasModifyAddressAdditionalDetailsSecurity);
			AssertEquals("HasModifyAddressAdditionalDetailsSecurity should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifyAddressAdditionalDetailsSecurity);
		}

		#endregion

		#region TestHasModifyCustomsAddressSecurity

		public void TestHasModifyCustomsAddressSecurity()
		{
			Env.Security.OrgAddressCustomsAddressModify.IsAllowed = false;
			AssertEquals("HasModifyCustomsAddressSecurity should NOT be allowed", false, testSecurityProvider.HasModifyCustomsAddressSecurity);
			AssertEquals("HasModifyCustomsAddressSecurity should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifyCustomsAddressSecurity);

			Env.Security.OrgAddressCustomsAddressModify.IsAllowed = true;
			AssertEquals("HasModifyCustomsAddressSecurity should be allowed", true, testSecurityProvider.HasModifyCustomsAddressSecurity);
			AssertEquals("HasModifyCustomsAddressSecurity should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifyCustomsAddressSecurity);

			Env.Security.OrgAddressCustomsAddressNew.IsAllowed = false;
			AssertEquals("HasModifyCustomsAddressSecurity should be allowed", true, testSecurityProvider.HasModifyCustomsAddressSecurity);
			AssertEquals("HasModifyCustomsAddressSecurity should NOT be allowed (not saved)", false, testUnsavedSecurityProvider.HasModifyCustomsAddressSecurity);

			Env.Security.OrgAddressCustomsAddressNew.IsAllowed = true;
			AssertEquals("HasModifyCustomsAddressSecurity should be allowed", true, testSecurityProvider.HasModifyCustomsAddressSecurity);
			AssertEquals("HasModifyCustomsAddressSecurity should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifyCustomsAddressSecurity);
		}

		#endregion

		#region TestHasModifyEUCustomsAddressSecurity
		public void TestHasModifyEUCustomsAddressSecurity()
		{
			Env.Security.OrgAddressEUCustomsAddressModify.IsAllowed = false;
			AssertEquals("HasModifyCustomsAddressSecurity should NOT be allowed", false, testSecurityProvider.HasModifyEUCustomsAddressSecurity);
			AssertEquals("HasModifyCustomsAddressSecurity should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifyEUCustomsAddressSecurity);

			Env.Security.OrgAddressEUCustomsAddressModify.IsAllowed = true;
			AssertEquals("HasModifyCustomsAddressSecurity should be allowed", true, testSecurityProvider.HasModifyEUCustomsAddressSecurity);
			AssertEquals("HasModifyCustomsAddressSecurity should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifyEUCustomsAddressSecurity);

			Env.Security.OrgAddressEUCustomsAddressNew.IsAllowed = false;
			AssertEquals("HasModifyCustomsAddressSecurity should be allowed", true, testSecurityProvider.HasModifyEUCustomsAddressSecurity);
			AssertEquals("HasModifyCustomsAddressSecurity should NOT be allowed (not saved)", false, testUnsavedSecurityProvider.HasModifyEUCustomsAddressSecurity);

			Env.Security.OrgAddressEUCustomsAddressNew.IsAllowed = true;
			AssertEquals("HasModifyCustomsAddressSecurity should be allowed", true, testSecurityProvider.HasModifyEUCustomsAddressSecurity);
			AssertEquals("HasModifyCustomsAddressSecurity should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifyEUCustomsAddressSecurity);
		}
		#endregion

		#region TestHasModifyContactSecurity

		public void TestHasModifyContactSecurity()
		{
			Env.Security.OrgContactModify.IsAllowed = false;
			AssertEquals("HasModifyContactSecurity should NOT be allowed", false, testSecurityProvider.HasModifyContactSecurity);
			AssertEquals("HasModifyContactSecurity should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifyContactSecurity);

			Env.Security.OrgContactModify.IsAllowed = true;
			AssertEquals("HasModifyContactSecurity should be allowed", true, testSecurityProvider.HasModifyContactSecurity);
			AssertEquals("HasModifyContactSecurity should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifyContactSecurity);
		}

		#endregion

		#region TestHasModifyContactContactDetailsSecurity

		public void TestHasModifyContactContactDetailsSecurity()
		{
			Env.Security.OrgContactModifyContactDetails.IsAllowed = false;
			AssertEquals("HasModifyContactContactDetailsSecurity should NOT be allowed", false, testSecurityProvider.HasModifyContactContactDetailsSecurity);
			AssertEquals("HasModifyContactContactDetailsSecurity should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifyContactContactDetailsSecurity);

			Env.Security.OrgContactModifyContactDetails.IsAllowed = true;
			AssertEquals("HasModifyContactContactDetailsSecurity should be allowed", true, testSecurityProvider.HasModifyContactContactDetailsSecurity);
			AssertEquals("HasModifyContactContactDetailsSecurity should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifyContactContactDetailsSecurity);
		}

		#endregion

		#region TestHasModifyContactPersonalInformationSecurity

		public void TestHasModifyContactPersonalInformationSecurity()
		{
			Env.Security.OrgContactModifyPersonalInformation.IsAllowed = false;
			AssertEquals("HasModifyContactPersonalInformationSecurity should NOT be allowed", false, testSecurityProvider.HasModifyContactPersonalInformationSecurity);
			AssertEquals("HasModifyContactPersonalInformationSecurity should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifyContactPersonalInformationSecurity);

			Env.Security.OrgContactModifyPersonalInformation.IsAllowed = true;
			AssertEquals("HasModifyContactPersonalInformationSecurity should be allowed", true, testSecurityProvider.HasModifyContactPersonalInformationSecurity);
			AssertEquals("HasModifyContactPersonalInformationSecurity should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifyContactPersonalInformationSecurity);
		}

		#endregion

		#region TestHasModifyContactDocDeliveryDetailsSecurity

		public void TestHasModifyContactDocDeliveryDetailsSecurity()
		{
			Env.Security.OrgContactModifyDocDeliveryDetails.IsAllowed = false;
			AssertEquals("HasModifyContactDocDeliveryDetailsSecurity should NOT be allowed", false, testSecurityProvider.HasModifyContactDocDeliveryDetailsSecurity);
			AssertEquals("HasModifyContactDocDeliveryDetailsSecurity should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifyContactDocDeliveryDetailsSecurity);

			Env.Security.OrgContactModifyDocDeliveryDetails.IsAllowed = true;
			AssertEquals("HasModifyContactDocDeliveryDetailsSecurity should be allowed", true, testSecurityProvider.HasModifyContactDocDeliveryDetailsSecurity);
			AssertEquals("HasModifyContactDocDeliveryDetailsSecurity should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifyContactDocDeliveryDetailsSecurity);
		}

		#endregion

		#region TestHasModifyContactMobileNumberSecurity

		public void TestHasModifyContactMobileNumberSecurity()
		{
			Env.Security.OrgContactModifyMobileNumber.IsAllowed = false;
			AssertEquals("HasModifyContactMobileNumberSecurity should NOT be allowed", false, testSecurityProvider.HasModifyContactMobileNumberSecurity);
			AssertEquals("HasModifyContactMobileNumberSecurity should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifyContactMobileNumberSecurity);

			Env.Security.OrgContactModifyMobileNumber.IsAllowed = true;
			AssertEquals("HasModifyContactMobileNumberSecurity should be allowed", true, testSecurityProvider.HasModifyContactMobileNumberSecurity);
			AssertEquals("HasModifyContactMobileNumberSecurity should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifyContactMobileNumberSecurity);
		}

		#endregion

		#region TestHasModifyContactHomePhoneNumberSecurity

		public void TestHasModifyContactHomePhoneNumberSecurity()
		{
			Env.Security.OrgContactModifyHomePhoneNumber.IsAllowed = false;
			AssertEquals("HasModifyContactHomePhoneNumberSecurity should NOT be allowed", false, testSecurityProvider.HasModifyContactHomePhoneNumberSecurity);
			AssertEquals("HasModifyContactHomePhoneNumberSecurity should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifyContactHomePhoneNumberSecurity);

			Env.Security.OrgContactModifyHomePhoneNumber.IsAllowed = true;
			AssertEquals("HasModifyContactHomePhoneNumberSecurity should be allowed", true, testSecurityProvider.HasModifyContactHomePhoneNumberSecurity);
			AssertEquals("HasModifyContactHomePhoneNumberSecurity should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifyContactHomePhoneNumberSecurity);
		}

		#endregion

		#region TestHasModifyReceivablesSecurity

		public void TestHasModifyReceivablesSecurity()
		{
			Env.Security.OrgReceivablesModify.IsAllowed = false;
			AssertEquals("HasModifyReceivablesSecurity should NOT be allowed", false, testSecurityProvider.HasModifyReceivablesSecurity);
			AssertEquals("HasModifyReceivablesSecurity should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifyReceivablesSecurity);

			Env.Security.OrgReceivablesModify.IsAllowed = true;
			AssertEquals("HasModifyReceivablesSecurity should be allowed", true, testSecurityProvider.HasModifyReceivablesSecurity);
			AssertEquals("HasModifyReceivablesSecurity should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifyReceivablesSecurity);
		}

		#endregion

		#region TestHasModifyReceivablesCreditCardDetailsSecurity

		public void TestHasModifyReceivablesCreditCardDetailsSecurity()
		{
			Env.Security.OrgReceivablesModifyCreditCardDetails.IsAllowed = false;
			AssertEquals("HasModifyReceivablesCreditCardDetailsSecurity should NOT be allowed", false, testSecurityProvider.HasModifyReceivablesCreditCardDetailsSecurity);
			AssertEquals("HasModifyReceivablesCreditCardDetailsSecurity should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifyReceivablesCreditCardDetailsSecurity);

			Env.Security.OrgReceivablesModifyCreditCardDetails.IsAllowed = true;
			AssertEquals("HasModifyReceivablesCreditCardDetailsSecurity should be allowed", true, testSecurityProvider.HasModifyReceivablesCreditCardDetailsSecurity);
			AssertEquals("HasModifyReceivablesCreditCardDetailsSecurity should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifyReceivablesCreditCardDetailsSecurity);
		}

		#endregion

		#region TestHasModifyReceivablesConfigSecurity

		public void TestHasModifyReceivablesConfigSecurity()
		{
			Env.Security.OrgReceivablesModifyConfig.IsAllowed = false;
			AssertEquals("HasModifyReceivablesConfigSecurity should NOT be allowed", false, testSecurityProvider.HasModifyReceivablesConfigSecurity);
			AssertEquals("HasModifyReceivablesConfigSecurity should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifyReceivablesConfigSecurity);

			Env.Security.OrgReceivablesModifyConfig.IsAllowed = true;
			AssertEquals("HasModifyReceivablesConfigSecurity should be allowed", true, testSecurityProvider.HasModifyReceivablesConfigSecurity);
			AssertEquals("HasModifyReceivablesConfigSecurity should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifyReceivablesConfigSecurity);
		}

		#endregion

		#region TestHasModifyReceivablesAccountDetailsSecurity

		public void TestHasModifyReceivablesAccountDetailsSecurity()
		{
			Env.Security.OrgReceivablesModifyAccountDetails.IsAllowed = false;
			AssertEquals("HasModifyReceivablesAccountDetailsSecurity should NOT be allowed", false, testSecurityProvider.HasModifyReceivablesAccountDetailsSecurity);
			AssertEquals("HasModifyReceivablesAccountDetailsSecurity should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifyReceivablesAccountDetailsSecurity);

			Env.Security.OrgReceivablesModifyAccountDetails.IsAllowed = true;
			AssertEquals("HasModifyReceivablesAccountDetailsSecurity should be allowed", true, testSecurityProvider.HasModifyReceivablesAccountDetailsSecurity);
			AssertEquals("HasModifyReceivablesAccountDetailsSecurity should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifyReceivablesAccountDetailsSecurity);
		}

		#endregion

		#region TestHasModifyReceivablesInvoicingSecurity

		public void TestHasModifyReceivablesInvoicingSecurity()
		{
			Env.Security.OrgReceivablesModifyInvoicing.IsAllowed = false;
			AssertEquals("HasModifyReceivablesInvoicingSecurity should NOT be allowed", false, testSecurityProvider.HasModifyReceivablesInvoicingSecurity);
			AssertEquals("HasModifyReceivablesInvoicingSecurity should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifyReceivablesInvoicingSecurity);

			Env.Security.OrgReceivablesModifyInvoicing.IsAllowed = true;
			AssertEquals("HasModifyReceivablesInvoicingSecurity should be allowed", true, testSecurityProvider.HasModifyReceivablesInvoicingSecurity);
			AssertEquals("HasModifyReceivablesInvoicingSecurity should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifyReceivablesInvoicingSecurity);
		}

		#endregion

		#region TestHasModifyReceivablesCreditControlSecurity

		public void TestHasModifyReceivablesCreditControlSecurity()
		{
			Env.Security.OrgReceivablesModifyCreditControl.IsAllowed = false;
			AssertEquals("HasModifyReceivablesCreditControlSecurity should NOT be allowed", false, testSecurityProvider.HasModifyReceivablesCreditControlSecurity);
			AssertEquals("HasModifyReceivablesCreditControlSecurity should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifyReceivablesCreditControlSecurity);

			Env.Security.OrgReceivablesModifyCreditControl.IsAllowed = true;
			AssertEquals("HasModifyReceivablesCreditControlSecurity should be allowed", true, testSecurityProvider.HasModifyReceivablesCreditControlSecurity);
			AssertEquals("HasModifyReceivablesCreditControlSecurity should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifyReceivablesCreditControlSecurity);
		}

		#endregion

		#region TestHasReceivablesGlobalCreditControlSecurity

		public void TestHasReceivablesGlobalCreditControlSecurity()
		{
			Env.Security.OrgReceivablesGlobalCreditControl.IsAllowed = false;
			AssertEquals("HasReceivablesGlobalCreditControlSecurity should NOT be allowed", false, testSecurityProvider.HasReceivablesGlobalCreditControlSecurity);
			AssertEquals("HasReceivablesGlobalCreditControlSecurityshould be allowed (not saved)", true, testUnsavedSecurityProvider.HasReceivablesGlobalCreditControlSecurity);

			Env.Security.OrgReceivablesGlobalCreditControl.IsAllowed = true;
			AssertEquals("HasReceivablesGlobalCreditControlSecurity should be allowed", true, testSecurityProvider.HasReceivablesGlobalCreditControlSecurity);
			AssertEquals("HasReceivablesGlobalCreditControlSecurity should be allowed (not saved)", true, testUnsavedSecurityProvider.HasReceivablesGlobalCreditControlSecurity);
		}

		#endregion

		#region TestHasModifyReceivablesCurrencyUpliftSecurity

		public void TestHasModifyReceivablesCurrencyUpliftSecurity()
		{
			Env.Security.OrgReceivablesModifyCurrencyUplift.IsAllowed = false;
			AssertEquals("HasModifyReceivablesCurrencyUpliftSecurity should NOT be allowed", false, testSecurityProvider.HasModifyReceivablesCurrencyUpliftSecurity);
			AssertEquals("HasModifyReceivablesCurrencyUpliftSecurity should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifyReceivablesCurrencyUpliftSecurity);

			Env.Security.OrgReceivablesModifyCurrencyUplift.IsAllowed = true;
			AssertEquals("HasModifyReceivablesCurrencyUpliftSecurity should be allowed", true, testSecurityProvider.HasModifyReceivablesCurrencyUpliftSecurity);
			AssertEquals("HasModifyReceivablesCurrencyUpliftSecurity should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifyReceivablesCurrencyUpliftSecurity);
		}

		#endregion

		#region TestHasModifyReceivablesTaxDetailsSecurity

		public void TestHasModifyReceivablesTaxDetailsSecurity()
		{
			Env.Security.OrgReceivablesModifyTaxDetails.IsAllowed = false;
			AssertEquals("HasModifyReceivablesTaxDetailsSecurity should NOT be allowed", false, testSecurityProvider.HasModifyReceivablesTaxDetailsSecurity);
			AssertEquals("HasModifyReceivablesTaxDetailsSecurity should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifyReceivablesTaxDetailsSecurity);

			Env.Security.OrgReceivablesModifyTaxDetails.IsAllowed = true;
			AssertEquals("HasModifyReceivablesTaxDetailsSecurity should be allowed", true, testSecurityProvider.HasModifyReceivablesTaxDetailsSecurity);
			AssertEquals("HasModifyReceivablesTaxDetailsSecurity should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifyReceivablesTaxDetailsSecurity);
		}

		#endregion

		#region TestHasModifyReceivablesQualityAssuranceSecurity

		public void TestHasModifyReceivablesQualityAssuranceSecurity()
		{
			Env.Security.OrgReceivablesModifyQualityAssurance.IsAllowed = false;
			AssertEquals("HasModifyReceivablesQualityAssuranceSecurity should NOT be allowed", false, testSecurityProvider.HasModifyReceivablesQualityAssuranceSecurity);
			AssertEquals("HasModifyReceivablesQualityAssuranceSecurity should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifyReceivablesQualityAssuranceSecurity);

			Env.Security.OrgReceivablesModifyQualityAssurance.IsAllowed = true;
			AssertEquals("HasModifyReceivablesQualityAssuranceSecurity should be allowed", true, testSecurityProvider.HasModifyReceivablesQualityAssuranceSecurity);
			AssertEquals("HasModifyReceivablesQualityAssuranceSecurity should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifyReceivablesQualityAssuranceSecurity);
		}

		#endregion

		#region TestHasModifyReceivablesExternalDebtorSecurity

		public void TestHasModifyReceivablesExternalDebtorSecurity()
		{
			Env.Security.OrgReceivablesModifyExternalDebtor.IsAllowed = false;
			AssertEquals("HasModifyReceivablesExternalDebtorSecurity should NOT be allowed", false, testSecurityProvider.HasModifyReceivablesExternalDebtorSecurity);
			AssertEquals("HasModifyReceivablesExternalDebtorSecurity should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifyReceivablesExternalDebtorSecurity);

			Env.Security.OrgReceivablesModifyExternalDebtor.IsAllowed = true;
			AssertEquals("HasModifyReceivablesExternalDebtorSecurity should be allowed", true, testSecurityProvider.HasModifyReceivablesExternalDebtorSecurity);
			AssertEquals("HasModifyReceivablesExternalDebtorSecurity should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifyReceivablesExternalDebtorSecurity);
		}

		#endregion

		#region TestHasModifyReceivablesSettlementGroupSecurity

		public void TestHasModifyReceivablesSettlementGroupSecurity()
		{
			Env.Security.OrgReceivablesModifySettlementGroup.IsAllowed = false;
			AssertEquals("HasModifyReceivablesSettlementGroupSecurity should NOT be allowed", false, testSecurityProvider.HasModifyReceivablesSettlementGroupSecurity);
			AssertEquals("HasModifyReceivablesSettlementGroupSecurity should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifyReceivablesSettlementGroupSecurity);

			Env.Security.OrgReceivablesModifySettlementGroup.IsAllowed = true;
			AssertEquals("HasModifyReceivablesSettlementGroupSecurity should be allowed", true, testSecurityProvider.HasModifyReceivablesSettlementGroupSecurity);
			AssertEquals("HasModifyReceivablesSettlementGroupSecurity should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifyReceivablesSettlementGroupSecurity);
		}

		#endregion

		#region TestHasModifyReceivablesPaymentTermsSecurity

		public void TestHasModifyReceivablesPaymentTermsSecurity()
		{
			Env.Security.OrgReceivablesModifyPaymentTerms.IsAllowed = false;
			AssertEquals("HasModifyReceivablesPaymentTermsSecurity should NOT be allowed", false, testSecurityProvider.HasModifyReceivablesPaymentTermsSecurity);
			AssertEquals("HasModifyReceivablesPaymentTermsSecurity should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifyReceivablesPaymentTermsSecurity);

			Env.Security.OrgReceivablesModifyPaymentTerms.IsAllowed = true;
			AssertEquals("HasModifyReceivablesPaymentTermsSecurity should be allowed", true, testSecurityProvider.HasModifyReceivablesPaymentTermsSecurity);
			AssertEquals("HasModifyReceivablesPaymentTermsSecurity should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifyReceivablesPaymentTermsSecurity);
		}

		#endregion

		#region TestHasModifyReceivablesInvoiceDetailsSecurity

		public void TestHasModifyReceivablesInvoiceDetailsSecurity()
		{
			Env.Security.OrgReceivablesModifyInvoiceDetails.IsAllowed = false;
			AssertEquals("HasModifyReceivablesInvoiceDetailsSecurity should NOT be allowed", false, testSecurityProvider.HasModifyReceivablesInvoiceDetailsSecurity);
			AssertEquals("HasModifyReceivablesInvoiceDetailsSecurity should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifyReceivablesInvoiceDetailsSecurity);

			Env.Security.OrgReceivablesModifyInvoiceDetails.IsAllowed = true;
			AssertEquals("HasModifyReceivablesInvoiceDetailsSecurity should be allowed", true, testSecurityProvider.HasModifyReceivablesInvoiceDetailsSecurity);
			AssertEquals("HasModifyReceivablesInvoiceDetailsSecurity should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifyReceivablesInvoiceDetailsSecurity);
		}

		#endregion

		#region TestHasModifyReceivablesChargeGroupingSecurity

		public void TestHasModifyReceivablesChargeGroupingSecurity()
		{
			Env.Security.OrgReceivablesModifyChargeGrouping.IsAllowed = false;
			AssertEquals("HasModifyReceivablesChargeGroupingSecurity should NOT be allowed", false, testSecurityProvider.HasModifyReceivablesChargeGroupingSecurity);
			AssertEquals("HasModifyReceivablesChargeGroupingSecurity should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifyReceivablesChargeGroupingSecurity);

			Env.Security.OrgReceivablesModifyChargeGrouping.IsAllowed = true;
			AssertEquals("HasModifyReceivablesInvoiceDetailsSecurity should be allowed", true, testSecurityProvider.HasModifyReceivablesChargeGroupingSecurity);
			AssertEquals("HasModifyReceivablesInvoiceDetailsSecurity should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifyReceivablesChargeGroupingSecurity);
		}

		#endregion

		#region TestHasModifyReceivablesInvoiceBatchingSecurity

		public void TestHasModifyReceivablesInvoiceBatchingSecurity()
		{
			Env.Security.OrgReceivablesModifyInvoiceBatching.IsAllowed = false;
			AssertEquals("HasModifyReceivablesInvoiceBatchingSecurity should NOT be allowed", false, testSecurityProvider.HasModifyReceivablesInvoiceBatchingSecurity);
			AssertEquals("HasModifyReceivablesInvoiceBatchingSecurity should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifyReceivablesInvoiceBatchingSecurity);

			Env.Security.OrgReceivablesModifyInvoiceBatching.IsAllowed = true;
			AssertEquals("HasModifyReceivablesInvoiceBatchingSecurity should be allowed", true, testSecurityProvider.HasModifyReceivablesInvoiceBatchingSecurity);
			AssertEquals("HasModifyReceivablesInvoiceBatchingSecurity should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifyReceivablesInvoiceBatchingSecurity);
		}

		#endregion

		#region TestHasModifyReceivablesExchangeRatesSecurity

		public void TestHasModifyReceivablesExchangeRatesSecurity()
		{
			Env.Security.OrgReceivablesModifyExchangeRates.IsAllowed = false;
			AssertEquals("HasModifyReceivablesExchangeRatesSecurity should NOT be allowed", false, testSecurityProvider.HasModifyReceivablesExchangeRatesSecurity);
			AssertEquals("HasModifyReceivablesExchangeRatesSecurity should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifyReceivablesExchangeRatesSecurity);

			Env.Security.OrgReceivablesModifyExchangeRates.IsAllowed = true;
			AssertEquals("HasModifyReceivablesExchangeRatesSecurity should be allowed", true, testSecurityProvider.HasModifyReceivablesExchangeRatesSecurity);
			AssertEquals("HasModifyReceivablesExchangeRatesSecurity should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifyReceivablesExchangeRatesSecurity);
		}

		#endregion

		#region TestHasModifyReceivablesBuyersConsolInvoicingSecurity

		public void TestHasModifyReceivablesBuyersConsolInvoicingSecurity()
		{
			Env.Security.OrgReceivablesModifyBuyersConsolInvoicing.IsAllowed = false;
			AssertEquals("HasModifyReceivablesBuyersConsolInvoicingSecurity should NOT be allowed", false, testSecurityProvider.HasModifyReceivablesBuyersConsolInvoicingSecurity);
			AssertEquals("HasModifyReceivablesBuyersConsolInvoicingSecurity should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifyReceivablesBuyersConsolInvoicingSecurity);

			Env.Security.OrgReceivablesModifyBuyersConsolInvoicing.IsAllowed = true;
			AssertEquals("HasModifyReceivablesBuyersConsolInvoicingSecurity should be allowed", true, testSecurityProvider.HasModifyReceivablesBuyersConsolInvoicingSecurity);
			AssertEquals("HasModifyReceivablesBuyersConsolInvoicingSecurity should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifyReceivablesBuyersConsolInvoicingSecurity);
		}

		#endregion

		#region TestHasModifyReceivablesTaxConfigurationSecurity

		public void TestHasModifyReceivablesTaxConfigurationTemplateSecurity()
		{
			Env.Security.OrgReceivablesModifyTaxConfigurationTemplate.IsAllowed = false;
			AssertEquals("HasModifyReceivablesTaxConfigurationTemplateSecurity should NOT be allowed", false, testSecurityProvider.HasModifyReceivablesTaxConfigurationTemplateSecurity);
			AssertEquals("HasModifyReceivablesTaxConfigurationTemplateSecurity should NOT be allowed (not saved)", false, testUnsavedSecurityProvider.HasModifyReceivablesTaxConfigurationTemplateSecurity);

			Env.Security.OrgReceivablesModifyTaxConfigurationTemplate.IsAllowed = true;
			AssertEquals("HasModifyReceivablesTaxConfigurationSecurity should be allowed", true, testSecurityProvider.HasModifyReceivablesTaxConfigurationTemplateSecurity);
			AssertEquals("HasModifyReceivablesTaxConfigurationSecurity should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifyReceivablesTaxConfigurationTemplateSecurity);
		}

		public void TestHasModifyReceivablesTaxConfigurationGridSecurity()
		{
			Env.Security.OrgReceivablesModifyTaxConfigurationGrid.IsAllowed = false;
			AssertEquals("HasModifyReceivablesTaxConfigurationGridSecurity should NOT be allowed", false, testSecurityProvider.HasModifyReceivablesTaxConfigurationGridSecurity);
			AssertEquals("HasModifyReceivablesTaxConfigurationGridSecurity should NOT be allowed (not saved)", false, testUnsavedSecurityProvider.HasModifyReceivablesTaxConfigurationGridSecurity);

			Env.Security.OrgReceivablesModifyTaxConfigurationGrid.IsAllowed = true;
			AssertEquals("HasModifyReceivablesTaxConfigurationGridSecurity should be allowed", true, testSecurityProvider.HasModifyReceivablesTaxConfigurationGridSecurity);
			AssertEquals("HasModifyReceivablesTaxConfigurationGridSecurity should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifyReceivablesTaxConfigurationGridSecurity);
		}

		public void TestHasModifyReceivablesTaxConfigurationRatesSecurity()
		{
			Env.Security.OrgReceivablesModifyTaxConfigurationRates.IsAllowed = false;
			AssertEquals("HasModifyReceivablesTaxConfigurationRatesSecurity should NOT be allowed", false, testSecurityProvider.HasModifyReceivablesTaxConfigurationRatesSecurity);
			AssertEquals("HasModifyReceivablesTaxConfigurationRatesSecurity should NOT be allowed (not saved)", false, testUnsavedSecurityProvider.HasModifyReceivablesTaxConfigurationRatesSecurity);

			Env.Security.OrgReceivablesModifyTaxConfigurationRates.IsAllowed = true;
			AssertEquals("HasModifyReceivablesTaxConfigurationRatesSecurity should be allowed", true, testSecurityProvider.HasModifyReceivablesTaxConfigurationRatesSecurity);
			AssertEquals("HasModifyReceivablesTaxConfigurationRatesSecurity should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifyReceivablesTaxConfigurationRatesSecurity);
		}

		#endregion

		#region TestHasModifyPayablesSecurity

		public void TestHasModifyPayablesSecurity()
		{
			Env.Security.OrgPayablesModify.IsAllowed = false;
			AssertEquals("HasModifyPayablesSecurity should NOT be allowed", false, testSecurityProvider.HasModifyPayablesSecurity);
			AssertEquals("HasModifyPayablesSecurity should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifyPayablesSecurity);

			Env.Security.OrgPayablesModify.IsAllowed = true;
			AssertEquals("HasModifyPayablesSecurity should be allowed", true, testSecurityProvider.HasModifyPayablesSecurity);
			AssertEquals("HasModifyPayablesSecurity should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifyPayablesSecurity);
		}

		#endregion

		#region TestHasModifyConfigPayablesSecurity

		public void TestHasModifyConfigPayablesSecurity()
		{
			Env.Security.OrgPayablesModifyConfig.IsAllowed = false;
			AssertEquals("HasModifyConfigPayablesSecurity should NOT be allowed", false, testSecurityProvider.HasModifyConfigPayablesSecurity);
			AssertEquals("HasModifyConfigPayablesSecurity should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifyConfigPayablesSecurity);

			Env.Security.OrgPayablesModifyConfig.IsAllowed = true;
			AssertEquals("HasModifyConfigPayablesSecurity should be allowed", true, testSecurityProvider.HasModifyConfigPayablesSecurity);
			AssertEquals("HasModifyConfigPayablesSecurity should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifyConfigPayablesSecurity);
		}

		#endregion

		#region TestHasModifyPayablesCreditorDetailsSecurity

		public void TestHasModifyPayablesCreditorDetailsSecurity()
		{
			Env.Security.OrgPayablesCreditorDetailsModify.IsAllowed = false;
			AssertEquals("HasModifyPayablesCreditorDetailsSecurity should NOT be allowed", false, testSecurityProvider.HasModifyPayablesCreditorDetailsSecurity);
			AssertEquals("HasModifyPayablesCreditorDetailsSecurity should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifyPayablesCreditorDetailsSecurity);

			Env.Security.OrgPayablesCreditorDetailsModify.IsAllowed = true;
			AssertEquals("HasModifyPayablesCreditorDetailsSecurity should be allowed", true, testSecurityProvider.HasModifyPayablesCreditorDetailsSecurity);
			AssertEquals("HasModifyPayablesCreditorDetailsSecurity should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifyPayablesCreditorDetailsSecurity);
		}

		#endregion

		#region TestHasModifyPayablesDefaultsSecurity

		public void TestHasModifyPayablesDefaultsSecurity()
		{
			Env.Security.OrgPayablesDefaultsModify.IsAllowed = false;
			AssertEquals("HasModifyPayablesDefaultsSecurity should NOT be allowed", false, testSecurityProvider.HasModifyPayablesDefaultsSecurity);
			AssertEquals("HasModifyPayablesDefaultsSecurity should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifyPayablesDefaultsSecurity);

			Env.Security.OrgPayablesDefaultsModify.IsAllowed = true;
			AssertEquals("HasModifyPayablesDefaultsSecurity should be allowed", true, testSecurityProvider.HasModifyPayablesDefaultsSecurity);
			AssertEquals("HasModifyPayablesDefaultsSecurity should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifyPayablesDefaultsSecurity);
		}

		#endregion

		#region TestHasModifyPayablesAccountDetailsSecurity

		public void TestHasModifyPayablesAccountDetailsSecurity()
		{
			Env.Security.OrgPayablesAccountDetailsModify.IsAllowed = false;
			AssertEquals("HasModifyPayablesAccountDetailsSecurity should NOT be allowed", false, testSecurityProvider.HasModifyPayablesAccountDetailsSecurity);
			AssertEquals("HasModifyPayablesAccountDetailsSecurity should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifyPayablesAccountDetailsSecurity);

			Env.Security.OrgPayablesAccountDetailsModify.IsAllowed = true;
			AssertEquals("HasModifyPayablesAccountDetailsSecurity should be allowed", true, testSecurityProvider.HasModifyPayablesAccountDetailsSecurity);
			AssertEquals("HasModifyPayablesAccountDetailsSecurity should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifyPayablesAccountDetailsSecurity);
		}

		#endregion

		#region TestHasModifyPayablesAccountDetailsSecurity

		public void TestHasModifyPayablesAccountDetailsEPaymentSecurity()
		{
			Env.Security.OrgPayablesAccountDetailsEPaymentModify.IsAllowed = false;
			AssertEquals("HasModifyPayablesAccountDetailsEPaymentSecurity should NOT be allowed", false, testSecurityProvider.HasModifyPayablesAccountDetailsEPaymentSecurity);
			AssertEquals("HasModifyPayablesAccountDetailsEPaymentSecurity should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifyPayablesAccountDetailsEPaymentSecurity);

			Env.Security.OrgPayablesAccountDetailsEPaymentModify.IsAllowed = true;
			AssertEquals("HasModifyPayablesAccountDetailsEPaymentSecurity should be allowed", true, testSecurityProvider.HasModifyPayablesAccountDetailsEPaymentSecurity);
			AssertEquals("HasModifyPayablesAccountDetailsEPaymentSecurity should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifyPayablesAccountDetailsEPaymentSecurity);
		}

		#endregion

		#region TestHasModifyPayablesCreditDetailsSecurity

		public void TestHasModifyPayablesCreditDetailsSecurity()
		{
			Env.Security.OrgPayablesCreditDetailsModify.IsAllowed = false;
			AssertEquals("HasModifyPayablesCreditDetailsSecurity should NOT be allowed", false, testSecurityProvider.HasModifyPayablesCreditDetailsSecurity);
			AssertEquals("HasModifyPayablesCreditDetailsSecurity should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifyPayablesCreditDetailsSecurity);

			Env.Security.OrgPayablesCreditDetailsModify.IsAllowed = true;
			AssertEquals("HasModifyPayablesCreditDetailsSecurity should be allowed", true, testSecurityProvider.HasModifyPayablesCreditDetailsSecurity);
			AssertEquals("HasModifyPayablesCreditDetailsSecurity should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifyPayablesCreditDetailsSecurity);
		}

		#endregion

		#region TestHasModifyPayablesPaymentTermsSecurity

		public void TestHasModifyPayablesPaymentTermsSecurity()
		{
			Env.Security.OrgPayablesPaymentTermsModify.IsAllowed = false;
			AssertEquals("HasModifyPayablesPaymentTermsSecurity should NOT be allowed", false, testSecurityProvider.HasModifyPayablesPaymentTermsSecurity);
			AssertEquals("HasModifyPayablesPaymentTermsSecurity should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifyPayablesPaymentTermsSecurity);

			Env.Security.OrgPayablesPaymentTermsModify.IsAllowed = true;
			AssertEquals("HasModifyPayablesPaymentTermsSecurity should be allowed", true, testSecurityProvider.HasModifyPayablesPaymentTermsSecurity);
			AssertEquals("HasModifyPayablesPaymentTermsSecurity should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifyPayablesPaymentTermsSecurity);
		}

		#endregion

		#region TestHasModifyPayablesTaxDetailsSecurity

		public void TestHasModifyPayablesTaxDetailsSecurity()
		{
			Env.Security.OrgPayablesTaxDetailsModify.IsAllowed = false;
			AssertEquals("HasModifyPayablesTaxDetailsSecurity should NOT be allowed", false, testSecurityProvider.HasModifyPayablesTaxDetailsSecurity);
			AssertEquals("HasModifyPayablesTaxDetailsSecurity should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifyPayablesTaxDetailsSecurity);

			Env.Security.OrgPayablesTaxDetailsModify.IsAllowed = true;
			AssertEquals("HasModifyPayablesTaxDetailsSecurity should be allowed", true, testSecurityProvider.HasModifyPayablesTaxDetailsSecurity);
			AssertEquals("HasModifyPayablesTaxDetailsSecurity should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifyPayablesTaxDetailsSecurity);
		}

		#endregion

		#region TestHasModifyPayablesOtherDetailsSecurity

		public void TestHasModifyPayablesOtherDetailsSecurity()
		{
			Env.Security.OrgPayablesOtherDetailsModify.IsAllowed = false;
			AssertEquals("HasModifyPayablesOtherDetailsSecurity should NOT be allowed", false, testSecurityProvider.HasModifyPayablesOtherDetailsSecurity);
			AssertEquals("HasModifyPayablesOtherDetailsSecurity should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifyPayablesOtherDetailsSecurity);

			Env.Security.OrgPayablesOtherDetailsModify.IsAllowed = true;
			AssertEquals("HasModifyPayablesOtherDetailsSecurity should be allowed", true, testSecurityProvider.HasModifyPayablesOtherDetailsSecurity);
			AssertEquals("HasModifyPayablesOtherDetailsSecurity should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifyPayablesOtherDetailsSecurity);
		}

		#endregion

		#region TestHasModifyPayablesQualityAssuranceSecurity

		public void TestHasModifyPayablesQualityAssuranceSecurity()
		{
			Env.Security.OrgPayablesQualityAssuranceModify.IsAllowed = false;
			AssertEquals("HasModifyPayablesQualityAssuranceSecurity should NOT be allowed", false, testSecurityProvider.HasModifyPayablesQualityAssuranceSecurity);
			AssertEquals("HasModifyPayablesQualityAssuranceSecurity should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifyPayablesQualityAssuranceSecurity);

			Env.Security.OrgPayablesQualityAssuranceModify.IsAllowed = true;
			AssertEquals("HasModifyPayablesQualityAssuranceSecurity should be allowed", true, testSecurityProvider.HasModifyPayablesQualityAssuranceSecurity);
			AssertEquals("HasModifyPayablesQualityAssuranceSecurity should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifyPayablesQualityAssuranceSecurity);
		}

		#endregion

		#region TestHasModifyPayablesExternalCreditorSecurity

		public void TestHasModifyPayablesExternalCreditorSecurity()
		{
			Env.Security.OrgPayablesModifyExternalCreditor.IsAllowed = false;
			AssertEquals("HasModifyPayablesExternalCreditorSecurity should NOT be allowed", false, testSecurityProvider.HasModifyPayablesExternalCreditorSecurity);
			AssertEquals("HasModifyPayablesExternalCreditorSecurity should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifyPayablesExternalCreditorSecurity);

			Env.Security.OrgPayablesModifyExternalCreditor.IsAllowed = true;
			AssertEquals("HasModifyPayablesExternalCreditorSecurity should be allowed", true, testSecurityProvider.HasModifyPayablesExternalCreditorSecurity);
			AssertEquals("HasModifyPayablesExternalCreditorSecurity should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifyPayablesExternalCreditorSecurity);
		}

		#endregion

		#region TestHasModifyPayablesTaxConfigurationSecurity

		public void TestHasModifyPayablesTaxConfigurationTemplateSecurity()
		{
			Env.Security.OrgPayablesModifyTaxConfigurationTemplate.IsAllowed = false;
			AssertEquals("HasModifyPayablesTaxConfigurationTemplateSecurity should NOT be allowed", false, testSecurityProvider.HasModifyPayablesTaxConfigurationTemplateSecurity);
			AssertEquals("HasModifyPayablesTaxConfigurationTemplateSecurity should NOT be allowed (not saved)", false, testUnsavedSecurityProvider.HasModifyPayablesTaxConfigurationTemplateSecurity);

			Env.Security.OrgPayablesModifyTaxConfigurationTemplate.IsAllowed = true;
			AssertEquals("HasModifyPayablesTaxConfigurationSecurity should be allowed", true, testSecurityProvider.HasModifyPayablesTaxConfigurationTemplateSecurity);
			AssertEquals("HasModifyPayablesTaxConfigurationSecurity should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifyPayablesTaxConfigurationTemplateSecurity);
		}

		public void TestHasModifyPayablesTaxConfigurationGridSecurity()
		{
			Env.Security.OrgPayablesModifyTaxConfigurationGrid.IsAllowed = false;
			AssertEquals("HasModifyPayablesTaxConfigurationGridSecurity should NOT be allowed", false, testSecurityProvider.HasModifyPayablesTaxConfigurationGridSecurity);
			AssertEquals("HasModifyPayablesTaxConfigurationGridSecurity should NOT be allowed (not saved)", false, testUnsavedSecurityProvider.HasModifyPayablesTaxConfigurationGridSecurity);

			Env.Security.OrgPayablesModifyTaxConfigurationGrid.IsAllowed = true;
			AssertEquals("HasModifyPayablesTaxConfigurationGridSecurity should be allowed", true, testSecurityProvider.HasModifyPayablesTaxConfigurationGridSecurity);
			AssertEquals("HasModifyPayablesTaxConfigurationGridSecurity should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifyPayablesTaxConfigurationGridSecurity);
		}

		public void TestHasModifyPayablesTaxConfigurationRatesSecurity()
		{
			Env.Security.OrgPayablesModifyTaxConfigurationRates.IsAllowed = false;
			AssertEquals("HasModifyPayablesTaxConfigurationRatesSecurity should NOT be allowed", false, testSecurityProvider.HasModifyPayablesTaxConfigurationRatesSecurity);
			AssertEquals("HasModifyPayablesTaxConfigurationRatesSecurity should NOT be allowed (not saved)", false, testUnsavedSecurityProvider.HasModifyPayablesTaxConfigurationRatesSecurity);

			Env.Security.OrgPayablesModifyTaxConfigurationRates.IsAllowed = true;
			AssertEquals("HasModifyPayablesTaxConfigurationRatesSecurity should be allowed", true, testSecurityProvider.HasModifyPayablesTaxConfigurationRatesSecurity);
			AssertEquals("HasModifyPayablesTaxConfigurationRatesSecurity should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifyPayablesTaxConfigurationRatesSecurity);
		}

		#endregion

		#region TestHasOrgPayablesModifyConfigTransCreationRestrictionSecurity

		public void TestHasOrgPayablesModifyConfigTransCreationRestrictionSecurity()
		{
			Env.Security.OrgPayablesModifyConfigTransCreationRestriction.IsAllowed = false;
			AssertEquals("HasOrgPayablesModifyConfigTransCreationRestrictionSecurity should NOT be allowed", false, testSecurityProvider.HasOrgPayablesModifyConfigTransCreationRestrictionSecurity);
			AssertEquals("HasOrgPayablesModifyConfigTransCreationRestrictionSecurity should NOT be allowed (not saved)", false, testUnsavedSecurityProvider.HasOrgPayablesModifyConfigTransCreationRestrictionSecurity);

			Env.Security.OrgPayablesModifyConfigTransCreationRestriction.IsAllowed = true;
			AssertEquals("HasOrgPayablesModifyConfigTransCreationRestrictionSecurity should be allowed", true, testSecurityProvider.HasOrgPayablesModifyConfigTransCreationRestrictionSecurity);
			AssertEquals("HasOrgPayablesModifyConfigTransCreationRestrictionSecurity should be allowed (not saved)", true, testUnsavedSecurityProvider.HasOrgPayablesModifyConfigTransCreationRestrictionSecurity);
		}

		#endregion

		#region TestHasOrgReceivablesModifyConfigTransCreationRestrictionSecurity

		public void TestHasOrgReceivablesModifyConfigTransCreationRestrictionSecurity()
		{
			Env.Security.OrgReceivablesModifyConfigTransCreationRestriction.IsAllowed = false;
			AssertEquals("HasOrgReceivablesModifyConfigTransCreationRestrictionSecurity should NOT be allowed", false, testSecurityProvider.HasOrgReceivablesModifyConfigTransCreationRestrictionSecurity);
			AssertEquals("HasOrgReceivablesModifyConfigTransCreationRestrictionSecurity should NOT be allowed (not saved)", false, testUnsavedSecurityProvider.HasOrgReceivablesModifyConfigTransCreationRestrictionSecurity);

			Env.Security.OrgReceivablesModifyConfigTransCreationRestriction.IsAllowed = true;
			AssertEquals("HasOrgReceivablesModifyConfigTransCreationRestrictionSecurity should be allowed", true, testSecurityProvider.HasOrgReceivablesModifyConfigTransCreationRestrictionSecurity);
			AssertEquals("HasOrgReceivablesModifyConfigTransCreationRestrictionSecurity should be allowed (not saved)", true, testUnsavedSecurityProvider.HasOrgReceivablesModifyConfigTransCreationRestrictionSecurity);
		}

		#endregion

		#region TestHasOrgReceivablesModifySurchargeConfigurationSecurity

		public void TestHasOrgReceivablesModifySurchargeConfigurationSecurity()
		{
			var testSecurity = new SecurityForTest(null, Env.CurrentUser.PK, Env.CurrentBranch.PK, Env.CurrentDepartment.PK, Env.CurrentCompany.PK);
			testSecurity.OrgReceivablesModifySurchargeConfiguration.IsAllowed = false;
			Env.SetTemporarySecurityInstanceForTest(testSecurity);
			AssertEquals("HasOrgReceivablesModifySurchargeConfigurationSecurity should NOT be allowed", false, testSecurityProvider.HasOrgReceivablesModifySurchargeConfigurationSecurity);
			AssertEquals("HasOrgReceivablesModifySurchargeConfigurationSecurity should NOT be allowed (not saved)", false, testUnsavedSecurityProvider.HasOrgReceivablesModifySurchargeConfigurationSecurity);

			testSecurity.OrgReceivablesModifySurchargeConfiguration.IsAllowed = true;
			AssertEquals("HasOrgReceivablesModifySurchargeConfigurationSecurity should be allowed", true, testSecurityProvider.HasOrgReceivablesModifySurchargeConfigurationSecurity);
			AssertEquals("HasOrgReceivablesModifySurchargeConfigurationSecurity should be allowed (not saved)", true, testUnsavedSecurityProvider.HasOrgReceivablesModifySurchargeConfigurationSecurity);
		}
		#endregion

		#region TestHasModifyConsignorSecurity

		public void TestHasModifyConsignorSecurity()
		{
			Env.Security.OrgConsignorModify.IsAllowed = false;
			AssertEquals("HasModifyConsignorSecurity should NOT be allowed", false, testSecurityProvider.HasModifyConsignorSecurity);
			AssertEquals("HasModifyConsignorSecurity should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifyConsignorSecurity);

			Env.Security.OrgConsignorModify.IsAllowed = true;
			AssertEquals("HasModifyConsignorSecurity should be allowed", true, testSecurityProvider.HasModifyConsignorSecurity);
			AssertEquals("HasModifyConsignorSecurity should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifyConsignorSecurity);
		}

		#endregion

		#region TestHasModifyConsignorDetailsSecurity

		public void TestHasModifyConsignorDetailsSecurity()
		{
			Env.Security.OrgConsignorModifyDetails.IsAllowed = false;
			AssertEquals("HasModifyConsignorDetailsSecurity should NOT be allowed", false, testSecurityProvider.HasModifyConsignorDetailsSecurity);
			AssertEquals("HasModifyConsignorDetailsSecurity should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifyConsignorDetailsSecurity);

			Env.Security.OrgConsignorModifyDetails.IsAllowed = true;
			AssertEquals("HasModifyConsignorDetailsSecurity should be allowed", true, testSecurityProvider.HasModifyConsignorDetailsSecurity);
			AssertEquals("HasModifyConsignorDetailsSecurity should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifyConsignorDetailsSecurity);
		}

		#endregion

		#region TestHasModifyConsignorRelationshipsSecurity

		public void TestHasModifyConsignorRelationshipsSecurity()
		{
			Env.Security.OrgConsignorModifyRelationships.IsAllowed = false;
			AssertEquals("HasModifyConsignorRelationshipsSecurity should NOT be allowed", false, testSecurityProvider.HasModifyConsignorRelationshipsSecurity);
			AssertEquals("HasModifyConsignorRelationshipsSecurity should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifyConsignorRelationshipsSecurity);

			Env.Security.OrgConsignorModifyRelationships.IsAllowed = true;
			AssertEquals("HasModifyConsignorRelationshipsSecurity should be allowed", true, testSecurityProvider.HasModifyConsignorRelationshipsSecurity);
			AssertEquals("HasModifyConsignorRelationshipsSecurity should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifyConsignorRelationshipsSecurity);
		}

		#endregion

		#region TestHasModifyConsignorExporterSchemeSecurity

		public void TestHasModifyConsignorExporterSchemeSecurity()
		{
			Env.Security.OrgConsignorModifyExporterScheme.IsAllowed = false;
			AssertEquals("HasModifyConsignorExporterSchemeSecurity should NOT be allowed", false, testSecurityProvider.HasModifyConsignorExporterSchemeSecurity);
			AssertEquals("HasModifyConsignorExporterSchemeSecurity should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifyConsignorExporterSchemeSecurity);

			Env.Security.OrgConsignorModifyExporterScheme.IsAllowed = true;
			AssertEquals("HasModifyConsignorExporterSchemeSecurity should be allowed", true, testSecurityProvider.HasModifyConsignorExporterSchemeSecurity);
			AssertEquals("HasModifyConsignorExporterSchemeSecurity should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifyConsignorExporterSchemeSecurity);
		}

		#endregion

		#region TestHasModifyConsigneeSecurity

		public void TestHasModifyConsigneeSecurity()
		{
			Env.Security.OrgConsigneeModify.IsAllowed = false;
			AssertEquals("HasModifyConsigneeSecurity should NOT be allowed", false, testSecurityProvider.HasModifyConsigneeSecurity);
			AssertEquals("HasModifyConsigneeSecurity should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifyConsigneeSecurity);

			Env.Security.OrgConsigneeModify.IsAllowed = true;
			AssertEquals("HasModifyConsigneeSecurity should be allowed", true, testSecurityProvider.HasModifyConsigneeSecurity);
			AssertEquals("HasModifyConsigneeSecurity should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifyConsigneeSecurity);
		}

		#endregion

		#region TestHasModifyConsigneeDetailsSecurity

		public void TestHasModifyConsigneeDetailsSecurity()
		{
			Env.Security.OrgConsigneeModifyDetails.IsAllowed = false;
			AssertEquals("HasModifyConsigneeDetailsSecurity should NOT be allowed", false, testSecurityProvider.HasModifyConsigneeDetailsSecurity);
			AssertEquals("HasModifyConsigneeDetailsSecurity should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifyConsigneeDetailsSecurity);

			Env.Security.OrgConsigneeModifyDetails.IsAllowed = true;
			AssertEquals("HasModifyConsigneeDetailsSecurity should be allowed", true, testSecurityProvider.HasModifyConsigneeDetailsSecurity);
			AssertEquals("HasModifyConsigneeDetailsSecurity should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifyConsigneeDetailsSecurity);
		}

		#endregion

		#region TestHasModifyConsigneeRelationshipsSecurity

		public void TestHasModifyConsigneeRelationshipsSecurity()
		{
			Env.Security.OrgConsigneeModifyRelationships.IsAllowed = false;
			AssertEquals("HasModifyConsigneeRelationshipsSecurity should NOT be allowed", false, testSecurityProvider.HasModifyConsigneeRelationshipsSecurity);
			AssertEquals("HasModifyConsigneeRelationshipsSecurity should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifyConsigneeRelationshipsSecurity);

			Env.Security.OrgConsigneeModifyRelationships.IsAllowed = true;
			AssertEquals("HasModifyConsigneeRelationshipsSecurity should be allowed", true, testSecurityProvider.HasModifyConsigneeRelationshipsSecurity);
			AssertEquals("HasModifyConsigneeRelationshipsSecurity should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifyConsigneeRelationshipsSecurity);
		}

		#endregion

		#region TestHasModifyConsigneeLandedCostingSecurity

		public void TestHasModifyConsigneeLandedCostingSecurity()
		{
			Env.Security.OrgConsigneeModifyLandedCosting.IsAllowed = false;
			AssertEquals("HasModifyConsigneeLandedCostingSecurity should NOT be allowed", false, testSecurityProvider.HasModifyConsigneeLandedCostingSecurity);
			AssertEquals("HasModifyConsigneeLandedCostingSecurity should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifyConsigneeLandedCostingSecurity);

			Env.Security.OrgConsigneeModifyLandedCosting.IsAllowed = true;
			AssertEquals("HasModifyConsigneeLandedCostingSecurity should be allowed", true, testSecurityProvider.HasModifyConsigneeLandedCostingSecurity);
			AssertEquals("HasModifyConsigneeLandedCostingSecurity should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifyConsigneeLandedCostingSecurity);
		}

		#endregion

		#region TestHasModifyConsolidationCategory

		public void TestHasModifyConsolidationCategory()
		{
			Env.Security.OrgModifyConsolidationCategory.IsAllowed = false;
			AssertEquals("HasModifyConsolidationCategory should NOT be allowed", false, testSecurityProvider.HasModifyConsolidationCategory);
			AssertEquals("HasModifyConsolidationCategory should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifyConsolidationCategory);

			Env.Security.OrgModifyConsolidationCategory.IsAllowed = true;
			AssertEquals("HasModifyConsolidationCategory should be allowed", true, testSecurityProvider.HasModifyConsolidationCategory);
			AssertEquals("HasModifyConsolidationCategory should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifyConsolidationCategory);
		}

		#endregion

		#region TestHasModifyGlobalSupplierDetails

		public void TestHasModifyGlobalSupplierDetails()
		{
			Env.Security.OrgGlobalSupplierDetailsModify.IsAllowed = false;
			AssertEquals("HasModifyGlobalSupplierDetails should NOT be allowed", false, testSecurityProvider.HasModifyGlobalSupplierDetails);
			AssertEquals("HasModifyGlobalSupplierDetails should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifyGlobalSupplierDetails);

			Env.Security.OrgGlobalSupplierDetailsModify.IsAllowed = true;
			AssertEquals("HasModifyGlobalSupplierDetails should be allowed", true, testSecurityProvider.HasModifyGlobalSupplierDetails);
			AssertEquals("HasModifyGlobalSupplierDetails should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifyGlobalSupplierDetails);
		}

		#endregion

		#region TestCanEditGlobalSupplier

		public void TestCanEditGlobalSupplier()
		{
			var factory = new BusinessObjectFactory();
			var savedOrg = factory.New<OrgHeader>();
			savedOrg.OH_Code = "DEF";
			factory.Save();

			var unsavedOrg = Factory.New<OrgHeader>();
			unsavedOrg.OH_Code = "GHI";

			var anotherTestSavedSecturityProvider = new OrganisationSecurityProvider(savedOrg);
			var anotherTestUnsavedSecturityProvider = new OrganisationSecurityProvider(unsavedOrg);

			Env.Security.OrgGlobalSupplierDetailsModify.IsAllowed = false;
			savedOrg.OH_IsGlobalAccount = false;
			unsavedOrg.OH_IsGlobalAccount = false;
			AssertEquals("CanEditGlobalSupplier should be true (this is not a GlobalAccount)", true, anotherTestSavedSecturityProvider.CanEditGlobalSupplier);
			AssertEquals("CanEditGlobalSupplier should be true (not saved)", true, anotherTestUnsavedSecturityProvider.CanEditGlobalSupplier);

			savedOrg.OH_IsGlobalAccount = true;
			unsavedOrg.OH_IsGlobalAccount = true;
			AssertEquals("CanEditGlobalSupplier should be false (Don't have HasModifyGlobalSupplierDetails right)", false, anotherTestSavedSecturityProvider.CanEditGlobalSupplier);
			AssertEquals("CanEditGlobalSupplier should be true (not saved)", true, anotherTestUnsavedSecturityProvider.CanEditGlobalSupplier);

			Env.Security.OrgGlobalSupplierDetailsModify.IsAllowed = true;
			savedOrg.OH_IsGlobalAccount = false;
			unsavedOrg.OH_IsGlobalAccount = false;
			AssertEquals("CanEditGlobalSupplier should be true (this is not a GlobalAccount)", true, anotherTestSavedSecturityProvider.CanEditGlobalSupplier);
			AssertEquals("CanEditGlobalSupplier should be true (not saved & this is not a GlobalAccount)", true, anotherTestUnsavedSecturityProvider.CanEditGlobalSupplier);

			savedOrg.OH_IsGlobalAccount = true;
			unsavedOrg.OH_IsGlobalAccount = true;
			AssertEquals("CanEditGlobalSupplier should be true", true, anotherTestSavedSecturityProvider.CanEditGlobalSupplier);
			AssertEquals("CanEditGlobalSupplier should be true (not saved)", true, anotherTestUnsavedSecturityProvider.CanEditGlobalSupplier);
		}

		#endregion

		#region TestHasModifyWarehouseSecurity

		public void TestHasModifyWarehouseSecurity()
		{
			Env.Security.OrgWarehouseModify.IsAllowed = false;
			AssertEquals("HasModifyWarehouseSecurity should NOT be allowed", false, testSecurityProvider.HasModifyWarehouseSecurity);
			AssertEquals("HasModifyWarehouseSecurity should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifyWarehouseSecurity);

			Env.Security.OrgWarehouseModify.IsAllowed = true;
			AssertEquals("HasModifyConsigneeWarehouseSecurity should be allowed", true, testSecurityProvider.HasModifyWarehouseSecurity);
			AssertEquals("HasModifyConsigneeWarehouseSecurity should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifyWarehouseSecurity);
		}

		#endregion

		#region TestHasModifyForwarderSecurity

		public void TestHasModifyForwarderSecurity()
		{
			Env.Security.OrgForwarderModify.IsAllowed = false;
			AssertEquals("HasModifyForwarderSecurity should NOT be allowed", false, testSecurityProvider.HasModifyForwarderSecurity);
			AssertEquals("HasModifyForwarderSecurity should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifyForwarderSecurity);

			Env.Security.OrgForwarderModify.IsAllowed = true;
			AssertEquals("HasModifyForwarderSecurity should be allowed", true, testSecurityProvider.HasModifyForwarderSecurity);
			AssertEquals("HasModifyForwarderSecurity should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifyForwarderSecurity);
		}

		#endregion

		#region TestHasModifyForwarderDetailsSecurity

		public void TestHasModifyForwarderDetailsSecurity()
		{
			Env.Security.OrgForwarderModifyDetails.IsAllowed = false;
			AssertEquals("HasModifyForwarderDetailsSecurity should NOT be allowed", false, testSecurityProvider.HasModifyForwarderDetailsSecurity);
			AssertEquals("HasModifyForwarderDetailsSecurity should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifyForwarderDetailsSecurity);

			Env.Security.OrgForwarderModifyDetails.IsAllowed = true;
			AssertEquals("HasModifyForwarderDetailsSecurity should be allowed", true, testSecurityProvider.HasModifyForwarderDetailsSecurity);
			AssertEquals("HasModifyForwarderDetailsSecurity should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifyForwarderDetailsSecurity);
		}

		#endregion

		#region TestHasModifyForwarderProfitShareSecurity

		public void TestHasModifyForwarderProfitShareSecurity()
		{
			Env.Security.OrgForwarderModifyProfitShare.IsAllowed = false;
			AssertEquals("HasModifyForwarderProfitShareSecurity should NOT be allowed", false, testSecurityProvider.HasModifyForwarderProfitShareSecurity);
			AssertEquals("HasModifyForwarderProfitShareSecurity should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifyForwarderProfitShareSecurity);

			Env.Security.OrgForwarderModifyProfitShare.IsAllowed = true;
			AssertEquals("HasModifyForwarderProfitShareSecurity should be allowed", true, testSecurityProvider.HasModifyForwarderProfitShareSecurity);
			AssertEquals("HasModifyForwarderProfitShareSecurity should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifyForwarderProfitShareSecurity);
		}

		#endregion

		#region TestHasModifyCarrierSecurity

		public void TestHasModifyCarrierSecurity()
		{
			Env.Security.OrgCarrierModify.IsAllowed = false;
			AssertEquals("HasModifyCarrierSecurity should NOT be allowed", false, testSecurityProvider.HasModifyCarrierSecurity);
			AssertEquals("HasModifyCarrierSecurity should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifyCarrierSecurity);

			Env.Security.OrgCarrierModify.IsAllowed = true;
			AssertEquals("HasModifyCarrierSecurity should be allowed", true, testSecurityProvider.HasModifyCarrierSecurity);
			AssertEquals("HasModifyCarrierSecurity should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifyCarrierSecurity);
		}

		#endregion

		#region TestHasModifyCarrierSecurityAir

		public void TestHasModifyCarrierSecurityAir()
		{
			Env.Security.OrgCarrierModifyAir.IsAllowed = false;
			AssertEquals("HasModifyCarrierSecurityAir should NOT be allowed", false, testSecurityProvider.HasModifyCarrierSecurityAir);
			AssertEquals("HasModifyCarrierSecurityAir should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifyCarrierSecurityAir);

			Env.Security.OrgCarrierModifyAir.IsAllowed = true;
			AssertEquals("HasModifyCarrierSecurityAir should be allowed", true, testSecurityProvider.HasModifyCarrierSecurityAir);
			AssertEquals("HasModifyCarrierSecurity should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifyCarrierSecurityAir);
		}

		#endregion

		#region TestHasModifyCarrierSecuritySea

		public void TestHasModifyCarrierSecuritySea()
		{
			Env.Security.OrgCarrierModifySea.IsAllowed = false;
			AssertEquals("HasModifyCarrierSecuritySea should NOT be allowed", false, testSecurityProvider.HasModifyCarrierSecuritySea);
			AssertEquals("HasModifyCarrierSecuritySea should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifyCarrierSecuritySea);

			Env.Security.OrgCarrierModifySea.IsAllowed = true;
			AssertEquals("HasModifyCarrierSecuritySea should be allowed", true, testSecurityProvider.HasModifyCarrierSecuritySea);
			AssertEquals("HasModifyCarrierSecuritySea should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifyCarrierSecuritySea);
		}

		#endregion

		#region TestHasModifyCarrierSecurityLand

		public void TestHasModifyCarrierSecurityLand()
		{
			Env.Security.OrgCarrierModifyLand.IsAllowed = false;
			AssertEquals("HasModifyCarrierSecurityLand should NOT be allowed", false, testSecurityProvider.HasModifyCarrierSecurityLand);
			AssertEquals("HasModifyCarrierSecurityLand should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifyCarrierSecurityLand);

			Env.Security.OrgCarrierModifyLand.IsAllowed = true;
			AssertEquals("HasModifyCarrierSecurityLand should be allowed", true, testSecurityProvider.HasModifyCarrierSecurityLand);
			AssertEquals("HasModifyCarrierSecurityLand should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifyCarrierSecurityLand);
		}

		#endregion

		#region TestHasModifyServicesSecurity

		public void TestHasModifyServicesSecurity()
		{
			Env.Security.OrgServicesModify.IsAllowed = false;
			AssertEquals("HasModifyServicesSecurity should NOT be allowed", false, testSecurityProvider.HasModifyServicesSecurity);
			AssertEquals("HasModifyServicesSecurity should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifyServicesSecurity);

			Env.Security.OrgServicesModify.IsAllowed = true;
			AssertEquals("HasModifyServicesSecurity should be allowed", true, testSecurityProvider.HasModifyServicesSecurity);
			AssertEquals("HasModifyServicesSecurity should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifyServicesSecurity);
		}

		#endregion

		#region TestHasModifySalesSecurity

		public void TestHasModifySalesSecurity()
		{
			Env.Security.ClientIntelligenceModify.IsAllowed = false;
			AssertEquals("HasModifySalesSecurity should NOT be allowed", false, testSecurityProvider.HasModifySalesSecurity);
			AssertEquals("HasModifySalesSecurity should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifySalesSecurity);

			Env.Security.ClientIntelligenceModify.IsAllowed = true;
			AssertEquals("HasModifySalesSecurity should be allowed", true, testSecurityProvider.HasModifySalesSecurity);
			AssertEquals("HasModifySalesSecurity should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifySalesSecurity);
		}

		#endregion

		#region TestHasModifySalesClientSummarySecurity

		public void TestHasModifySalesClientSummarySecurity()
		{
			Env.Security.ClientIntelligenceModifyClientSummary.IsAllowed = false;
			AssertEquals("HasModifySalesClientSummarySecurity should NOT be allowed", false, testSecurityProvider.HasModifySalesClientSummarySecurity);
			AssertEquals("HasModifySalesClientSummarySecurity should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifySalesClientSummarySecurity);

			Env.Security.ClientIntelligenceModifyClientSummary.IsAllowed = true;
			AssertEquals("HasModifySalesClientSummarySecurity should be allowed", true, testSecurityProvider.HasModifySalesClientSummarySecurity);
			AssertEquals("HasModifySalesClientSummarySecurity should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifySalesClientSummarySecurity);
		}

		#endregion

		#region TestHasModifySalesOpportunityManagementSecurity

		public void TestHasModifySalesOpportunityManagementSecurity()
		{
			Env.Security.OpportunityManagementEdit.IsAllowed = false;
			AssertEquals("HasModifySalesOpportunityManagementSecurity should NOT be allowed", false, testSecurityProvider.HasModifySalesOpportunityManagementSecurity);
			AssertEquals("HasModifySalesOpportunityManagementSecurity should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifySalesOpportunityManagementSecurity);

			Env.Security.OpportunityManagementEdit.IsAllowed = true;
			AssertEquals("HasModifySalesOpportunityManagementSecurity should be allowed", true, testSecurityProvider.HasModifySalesOpportunityManagementSecurity);
			AssertEquals("HasModifySalesOpportunityManagementSecurity should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifySalesOpportunityManagementSecurity);
		}

		#endregion

		#region TestHasModifySalesTradeProfileSecurity

		public void TestHasModifySalesTradeProfileSecurity()
		{
			Env.Security.ClientIntelligenceModifyTradeProfile.IsAllowed = false;
			AssertEquals("HasModifySalesTradeProfileSecurity should NOT be allowed", false, testSecurityProvider.HasModifySalesTradeProfileSecurity);
			AssertEquals("HasModifySalesTradeProfileSecurity should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifySalesTradeProfileSecurity);

			Env.Security.ClientIntelligenceModifyTradeProfile.IsAllowed = true;
			AssertEquals("HasModifySalesTradeProfileSecurity should be allowed", true, testSecurityProvider.HasModifySalesTradeProfileSecurity);
			AssertEquals("HasModifySalesTradeProfileSecurity should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifySalesTradeProfileSecurity);
		}

		#endregion

		#region TestHasModifyOrgServiceLevelsSecurity

		public void TestHasModifyOrgServiceLevelsSecurity()
		{
			Env.Security.OrgConsignorModifyDetails.IsAllowed = false;
			AssertEquals("HasModifyOrgServiceLevelsSecurity should NOT be allowed", false, testSecurityProvider.HasModifyOrgServiceLevelsSecurity);
			AssertEquals("HasModifyOrgServiceLevelsSecurity should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifyOrgServiceLevelsSecurity);

			Env.Security.OrgConsignorModifyDetails.IsAllowed = true;
			AssertEquals("HasModifyOrgServiceLevelsSecurity should be allowed", true, testSecurityProvider.HasModifyOrgServiceLevelsSecurity);
			AssertEquals("HasModifyOrgServiceLevelsSecurity should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifyOrgServiceLevelsSecurity);
		}

		#endregion

		#region TestHasModifySalesClientRelationshipSecurity

		public void TestHasModifySalesClientRelationshipSecurity()
		{
			Env.Security.ClientIntelligenceModifyClientRelationship.IsAllowed = false;
			AssertEquals("HasModifySalesClientRelationshipSecurity should NOT be allowed", false, testSecurityProvider.HasModifySalesClientRelationshipSecurity);
			AssertEquals("HasModifySalesClientRelationshipSecurity should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifySalesClientRelationshipSecurity);

			Env.Security.ClientIntelligenceModifyClientRelationship.IsAllowed = true;
			AssertEquals("HasModifySalesClientRelationshipSecurity should be allowed", true, testSecurityProvider.HasModifySalesClientRelationshipSecurity);
			AssertEquals("HasModifySalesClientRelationshipSecurity should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifySalesClientRelationshipSecurity);
		}

		#endregion

		#region TestHasModifyCompetitorSecurity

		public void TestHasModifyCompetitorSecurity()
		{
			Env.Security.CompetitorIntelligenceModify.IsAllowed = false;
			AssertEquals("HasModifyCompetitorSecurity should NOT be allowed", false, testSecurityProvider.HasModifyCompetitorSecurity);
			AssertEquals("HasModifyCompetitorSecurity should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifyCompetitorSecurity);

			Env.Security.CompetitorIntelligenceModify.IsAllowed = true;
			AssertEquals("HasModifyCompetitorSecurity should be allowed", true, testSecurityProvider.HasModifyCompetitorSecurity);
			AssertEquals("HasModifyCompetitorSecurity should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifyCompetitorSecurity);
		}

		#endregion

		#region TestHasModifyCustomSecurity

		public void TestHasModifyCustomSecurity()
		{
			Env.Security.OrgCustomModify.IsAllowed = false;
			AssertEquals("HasModifyCustomSecurity should NOT be allowed", false, testSecurityProvider.HasModifyCustomSecurity);
			AssertEquals("HasModifyCustomSecurity should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifyCustomSecurity);

			Env.Security.OrgCustomModify.IsAllowed = true;
			AssertEquals("HasModifyCustomSecurity should be allowed", true, testSecurityProvider.HasModifyCustomSecurity);
			AssertEquals("HasModifyCustomSecurity should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifyCustomSecurity);
		}

		#endregion

		#region TestHasModifyConfigSecurity

		public void TestHasModifyConfigSecurity()
		{
			Env.Security.OrgConfigModify.IsAllowed = false;
			AssertEquals("HasModifyConfigSecurity should NOT be allowed", false, testSecurityProvider.HasModifyConfigSecurity);
			AssertEquals("HasModifyConfigSecurity should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifyConfigSecurity);

			Env.Security.OrgConfigModify.IsAllowed = true;
			AssertEquals("HasModifyConfigSecurity should be allowed", true, testSecurityProvider.HasModifyConfigSecurity);
			AssertEquals("HasModifyConfigSecurity should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifyConfigSecurity);
		}

		#endregion

		#region TestHasModifyConfigFinancialRegistrationNumbersSecurity

		public void TestHasModifyConfigFinancialRegistrationNumbersSecurity()
		{
			Env.Security.OrgConfigModifyFinancialRegistrationNumbers.IsAllowed = false;
			AssertEquals("HasModifyConfigFinancialRegistrationNumbersSecurity should NOT be allowed", false, testSecurityProvider.HasModifyConfigFinancialRegistrationNumbersSecurity);
			AssertEquals("HasModifyConfigFinancialRegistrationNumbersSecurity should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifyConfigFinancialRegistrationNumbersSecurity);

			Env.Security.OrgConfigModifyFinancialRegistrationNumbers.IsAllowed = true;
			AssertEquals("HasModifyConfigFinancialRegistrationNumbersSecurity should be allowed", true, testSecurityProvider.HasModifyConfigFinancialRegistrationNumbersSecurity);
			AssertEquals("HasModifyConfigFinancialRegistrationNumbersSecurity should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifyConfigFinancialRegistrationNumbersSecurity);
		}

		#endregion

		#region TestHasModifyConfigNonFinancialRegistrationNumbersSecurity

		public void TestHasModifyConfigNonFinancialRegistrationNumbersSecurity()
		{
			Env.Security.OrgConfigModifyNonFinancialRegistrationNumbers.IsAllowed = false;
			AssertEquals("HasModifyConfigNonFinancialRegistrationNumbersSecurity should NOT be allowed", false, testSecurityProvider.HasModifyConfigNonFinancialRegistrationNumbersSecurity);
			AssertEquals("HasModifyConfigNonFinancialRegistrationNumbersSecurity should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifyConfigNonFinancialRegistrationNumbersSecurity);

			Env.Security.OrgConfigModifyNonFinancialRegistrationNumbers.IsAllowed = true;
			AssertEquals("HasModifyConfigNonFinancialRegistrationNumbersSecurity should be allowed", true, testSecurityProvider.HasModifyConfigNonFinancialRegistrationNumbersSecurity);
			AssertEquals("HasModifyConfigNonFinancialRegistrationNumbersSecurity should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifyConfigNonFinancialRegistrationNumbersSecurity);
		}

		#endregion

		#region TestHasModifyConfigRegistrationNumbersSecurity

		public void TestHasModifyConfigRegistrationNumbersSecurity()
		{
			Env.Security.OrgConfigModifyRegistrationNumbers.IsAllowed = false;
			AssertEquals("HasModifyConfigRegistrationNumbersSecurity should NOT be allowed", false, testSecurityProvider.HasModifyConfigRegistrationNumbersSecurity);
			AssertEquals("HasModifyConfigRegistrationNumbersSecurity should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifyConfigRegistrationNumbersSecurity);

			Env.Security.OrgConfigModifyRegistrationNumbers.IsAllowed = true;
			AssertEquals("HasModifyConfigRegistrationNumbersSecurity should be allowed", true, testSecurityProvider.HasModifyConfigRegistrationNumbersSecurity);
			AssertEquals("HasModifyConfigRegistrationNumbersSecurity should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifyConfigRegistrationNumbersSecurity);
		}

		#endregion

		#region TestHasModifyConfigFinancialARAPRegistrationNumbersSecurity

		public void TestHasModifyConfigFinancialARAPRegistrationNumbersSecurity()
		{
			Env.Security.OrgConfigModifyFinancialARAPRegistrationNumbers.IsAllowed = false;
			AssertEquals("HasModifyConfigFinancialARAPRegistrationNumbersSecurity should NOT be allowed", false, testSecurityProvider.HasModifyConfigFinancialARAPRegistrationNumbersSecurity);
			AssertEquals("HasModifyConfigFinancialARAPRegistrationNumbersSecurity should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifyConfigFinancialARAPRegistrationNumbersSecurity);

			Env.Security.OrgConfigModifyFinancialARAPRegistrationNumbers.IsAllowed = true;
			AssertEquals("HasModifyConfigFinancialARAPRegistrationNumbersSecurity should be allowed", true, testSecurityProvider.HasModifyConfigFinancialARAPRegistrationNumbersSecurity);
			AssertEquals("HasModifyConfigFinancialARAPRegistrationNumbersSecurity should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifyConfigFinancialARAPRegistrationNumbersSecurity);
		}

		#endregion

		#region TestHasModifyConfigFinancialNonARAPRegistrationNumbersSecurity

		public void TestHasModifyConfigFinancialNonARAPRegistrationNumbersSecurity()
		{
			Env.Security.OrgConfigModifyFinancialNonARAPRegistrationNumbers.IsAllowed = false;
			AssertEquals("HasModifyConfigFinancialNonARAPRegistrationNumbersSecurity should NOT be allowed", false, testSecurityProvider.HasModifyConfigFinancialNonARAPRegistrationNumbersSecurity);
			AssertEquals("HasModifyConfigFinancialNonARAPRegistrationNumbersSecurity should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifyConfigFinancialNonARAPRegistrationNumbersSecurity);

			Env.Security.OrgConfigModifyFinancialNonARAPRegistrationNumbers.IsAllowed = true;
			AssertEquals("HasModifyConfigFinancialNonARAPRegistrationNumbersSecurity should be allowed", true, testSecurityProvider.HasModifyConfigFinancialNonARAPRegistrationNumbersSecurity);
			AssertEquals("HasModifyConfigFinancialNonARAPRegistrationNumbersSecurity should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifyConfigFinancialNonARAPRegistrationNumbersSecurity);
		}

		#endregion

		#region TestHasModifyConfigNonFinancialARAPRegistrationNumbersSecurity

		public void TestHasModifyConfigNonFinancialARAPRegistrationNumbersSecurity()
		{
			Env.Security.OrgConfigModifyNonFinancialARAPRegistrationNumbers.IsAllowed = false;
			AssertEquals("HasModifyConfigNonFinancialARAPRegistrationNumbersSecurity should NOT be allowed", false, testSecurityProvider.HasModifyConfigNonFinancialARAPRegistrationNumbersSecurity);
			AssertEquals("HasModifyConfigNonFinancialARAPRegistrationNumbersSecurity should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifyConfigNonFinancialARAPRegistrationNumbersSecurity);

			Env.Security.OrgConfigModifyNonFinancialARAPRegistrationNumbers.IsAllowed = true;
			AssertEquals("HasModifyConfigNonFinancialARAPRegistrationNumbersSecurity should be allowed", true, testSecurityProvider.HasModifyConfigNonFinancialARAPRegistrationNumbersSecurity);
			AssertEquals("HasModifyConfigNonFinancialARAPRegistrationNumbersSecurity should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifyConfigNonFinancialARAPRegistrationNumbersSecurity);
		}

		#endregion

		#region TestHasModifyConfigNonFinancialNonARAPRegistrationNumbersSecurity

		public void TestHasModifyConfigNonFinancialNonARAPRegistrationNumbersSecurity()
		{
			Env.Security.OrgConfigModifyNonFinancialNonARAPRegistrationNumbers.IsAllowed = false;
			AssertEquals("HasModifyConfigNonFinancialNonARAPRegistrationNumbersSecurity should NOT be allowed", false, testSecurityProvider.HasModifyConfigNonFinancialNonARAPRegistrationNumbersSecurity);
			AssertEquals("HasModifyConfigNonFinancialNonARAPRegistrationNumbersSecurity should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifyConfigNonFinancialNonARAPRegistrationNumbersSecurity);

			Env.Security.OrgConfigModifyNonFinancialNonARAPRegistrationNumbers.IsAllowed = true;
			AssertEquals("HasModifyConfigNonFinancialNonARAPRegistrationNumbersSecurity should be allowed", true, testSecurityProvider.HasModifyConfigNonFinancialNonARAPRegistrationNumbersSecurity);
			AssertEquals("HasModifyConfigNonFinancialNonARAPRegistrationNumbersSecurity should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifyConfigNonFinancialNonARAPRegistrationNumbersSecurity);
		}

		#endregion

		#region TestHasConfigNewSecurity

		public void TestHasConfigNewSecurity()
		{
			Env.Security.OrgConfigNew.IsAllowed = false;
			AssertEquals("HasConfigNewSecurity should NOT be allowed", false, testSecurityProvider.HasConfigNewSecurity);

			Env.Security.OrgConfigNew.IsAllowed = true;
			AssertEquals("HasConfigNewSecurity should be allowed", true, testSecurityProvider.HasConfigNewSecurity);
		}

		#endregion

		#region TestHasNewConfigModifyFinancialRegistrationNosSecurity

		public void TestHasNewConfigModifyFinancialRegistrationNosSecurity()
		{
			Env.Security.OrgConfigNewModifyFinancialRegistrationNos.IsAllowed = false;
			AssertEquals("HasNewConfigModifyFinancialRegistrationNosSecurity should NOT be allowed", false, testSecurityProvider.HasNewConfigModifyFinancialRegistrationNosSecurity);

			Env.Security.OrgConfigNewModifyFinancialRegistrationNos.IsAllowed = true;
			AssertEquals("HasNewConfigModifyFinancialRegistrationNosSecurity should be allowed", true, testSecurityProvider.HasNewConfigModifyFinancialRegistrationNosSecurity);
		}

		#endregion

		#region TestHasNewConfigModifyNonFinancialRegistrationNosSecurity

		public void TestHasNewConfigModifyNonFinancialRegistrationNosSecurity()
		{
			Env.Security.OrgConfigNewModifyNonFinancialRegistrationNos.IsAllowed = false;
			AssertEquals("HasNewConfigModifyNonFinancialRegistrationNosSecurity should NOT be allowed", false, testSecurityProvider.HasNewConfigModifyNonFinancialRegistrationNosSecurity);

			Env.Security.OrgConfigNewModifyNonFinancialRegistrationNos.IsAllowed = true;
			AssertEquals("HasNewConfigModifyNonFinancialRegistrationNosSecurity should be allowed", true, testSecurityProvider.HasNewConfigModifyNonFinancialRegistrationNosSecurity);
		}

		#endregion

		#region TestHasNewConfigRegistrationNumbersSecurity

		public void TestHasNewConfigRegistrationNumbersSecurity()
		{
			Env.Security.OrgConfigNewModifyRegistrationNumbers.IsAllowed = false;
			AssertEquals("HasNewConfigRegistrationNumbersSecurity should NOT be allowed", false, testSecurityProvider.HasNewConfigRegistrationNumbersSecurity);

			Env.Security.OrgConfigNewModifyRegistrationNumbers.IsAllowed = true;
			AssertEquals("HasNewConfigRegistrationNumbersSecurity should be allowed", true, testSecurityProvider.HasNewConfigRegistrationNumbersSecurity);
		}

		#endregion

		#region TestHasModifyConfigEDICodeMappingSecurity

		public void TestHasModifyConfigEDICodeMappingSecurity()
		{
			Env.Security.OrgConfigModifyEDICodeMapping.IsAllowed = false;
			AssertEquals("HasModifyConfigEDICodeMappingSecurity should NOT be allowed", false, testSecurityProvider.HasModifyConfigEDICodeMappingSecurity);
			AssertEquals("HasModifyConfigEDICodeMappingSecurity should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifyConfigEDICodeMappingSecurity);

			Env.Security.OrgConfigModifyEDICodeMapping.IsAllowed = true;
			AssertEquals("HasModifyConfigEDICodeMappingSecurity should be allowed", true, testSecurityProvider.HasModifyConfigEDICodeMappingSecurity);
			AssertEquals("HasModifyConfigEDICodeMappingSecurity should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifyConfigEDICodeMappingSecurity);
		}

		#endregion

		#region TestHasModifyConfigBrandsAndCompanyNamesSecurity

		public void TestHasModifyConfigBrandsAndCompanyNamesSecurity()
		{
			Env.Security.OrgConfigModifyBrandsAndCompanyNames.IsAllowed = false;
			AssertEquals("HasModifyConfigBrandsAndCompanyNamesSecurity should NOT be allowed", false, testSecurityProvider.HasModifyConfigBrandsAndCompanyNamesSecurity);
			AssertEquals("HasModifyConfigBrandsAndCompanyNamesSecurity should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifyConfigBrandsAndCompanyNamesSecurity);

			Env.Security.OrgConfigModifyBrandsAndCompanyNames.IsAllowed = true;
			AssertEquals("HasModifyConfigBrandsAndCompanyNamesSecurity should be allowed", true, testSecurityProvider.HasModifyConfigBrandsAndCompanyNamesSecurity);
			AssertEquals("HasModifyConfigBrandsAndCompanyNamesSecurity should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifyConfigBrandsAndCompanyNamesSecurity);
		}

		#endregion

		#region TestHasModifyConfigGeneralSecurity

		public void TestHasModifyConfigGeneralSecurity()
		{
			Env.Security.OrgConfigModifyGeneral.IsAllowed = false;
			AssertEquals("HasModifyConfigGeneralSecurity should NOT be allowed", false, testSecurityProvider.HasModifyConfigGeneralSecurity);
			AssertEquals("HasModifyConfigGeneralSecurity should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifyConfigGeneralSecurity);

			Env.Security.OrgConfigModifyGeneral.IsAllowed = true;
			AssertEquals("HasModifyConfigGeneralSecurity should be allowed", true, testSecurityProvider.HasModifyConfigGeneralSecurity);
			AssertEquals("HasModifyConfigGeneralSecurity should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifyConfigGeneralSecurity);
		}

		#endregion

		#region TestHasModifyAddressCapabilitiesARAP

		public void TestHasModifyAddressCapabilitiesARAP()
		{
			Env.Security.OrgAddressCapabilitiesARAP.IsAllowed = false;
			AssertEquals("HasModifyAddressCapabilitiesARAP should NOT be allowed", false, testSecurityProvider.HasModifyAddressCapabilitiesARAP);
			AssertEquals("HasModifyAddressCapabilitiesARAP should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifyAddressCapabilitiesARAP);

			Env.Security.OrgAddressCapabilitiesARAP.IsAllowed = true;
			AssertEquals("HasModifyAddressCapabilitiesARAP should be allowed", true, testSecurityProvider.HasModifyAddressCapabilitiesARAP);
			AssertEquals("HasModifyAddressCapabilitiesARAP should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifyAddressCapabilitiesARAP);

			Env.Security.OrgAddressCapabilitiesARAPNew.IsAllowed = false;
			AssertEquals("HasModifyAddressCapabilitiesARAP should be allowed", true, testSecurityProvider.HasModifyAddressCapabilitiesARAP);
			AssertEquals("HasModifyAddressCapabilitiesARAP should NOT be allowed (not saved)", false, testUnsavedSecurityProvider.HasModifyAddressCapabilitiesARAP);

			Env.Security.OrgAddressCapabilitiesARAPNew.IsAllowed = true;
			AssertEquals("HasModifyAddressCapabilitiesARAP should be allowed", true, testSecurityProvider.HasModifyAddressCapabilitiesARAP);
			AssertEquals("HasModifyAddressCapabilitiesARAP should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifyAddressCapabilitiesARAP);
		}

		#endregion

		#region TestHasModifyAddressCapabilitiesNonARAP

		public void TestHasModifyAddressCapabilitiesNonARAP()
		{
			Env.Security.OrgAddressCapabilitiesNonARAP.IsAllowed = false;
			AssertEquals("HasModifyAddressCapabilitiesNonARAP should NOT be allowed", false, testSecurityProvider.HasModifyAddressCapabilitiesNonARAP);
			AssertEquals("HasModifyAddressCapabilitiesNonARAP should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifyAddressCapabilitiesNonARAP);

			Env.Security.OrgAddressCapabilitiesNonARAP.IsAllowed = true;
			AssertEquals("HasModifyAddressCapabilitiesNonARAP should be allowed", true, testSecurityProvider.HasModifyAddressCapabilitiesNonARAP);
			AssertEquals("HasModifyAddressCapabilitiesNonARAP should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifyAddressCapabilitiesNonARAP);

			Env.Security.OrgAddressCapabilitiesNonARAPNew.IsAllowed = false;
			AssertEquals("HasModifyAddressCapabilitiesNonARAP should be allowed", true, testSecurityProvider.HasModifyAddressCapabilitiesNonARAP);
			AssertEquals("HasModifyAddressCapabilitiesNonARAP should NOT be allowed (not saved)", false, testUnsavedSecurityProvider.HasModifyAddressCapabilitiesNonARAP);

			Env.Security.OrgAddressCapabilitiesNonARAPNew.IsAllowed = true;
			AssertEquals("HasModifyAddressCapabilitiesNonARAP should be allowed", true, testSecurityProvider.HasModifyAddressCapabilitiesNonARAP);
			AssertEquals("HasModifyAddressCapabilitiesNonARAP should be allowed (not saved)", true, testUnsavedSecurityProvider.HasModifyAddressCapabilitiesNonARAP);
		}

		#endregion

		#region TestHasModifyBranchProxies

		public void TestHasModifyBranchProxies()
		{
			var org = Factory.LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, "DEMORG");

			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_OH_OrgProxy = org.PK;

			Env.Security.OrgBranchProxiesEdit.IsAllowed = true;
			AssertEquals("OrgBranchProxiesEdit should be allowed for companies", true, testSecurityProvider.HasModifyBranchProxies);

			var branch = Factory.Load<GlbBranch>(GlbCompany.CurrentCompany.Branches[0].PK); // this is changed
			branch.GB_OH_OrgProxy = org.PK;

			Env.Security.OrgBranchProxiesEdit.IsAllowed = false;
			AssertEquals("OrgBranchProxiesEdit should NOT be allowed", false, testSecurityProvider.HasModifyBranchProxies);

			Env.Security.OrgBranchProxiesEdit.IsAllowed = true;
			AssertEquals("OrgBranchProxiesEdit should be allowed", true, testSecurityProvider.HasModifyBranchProxies);

			Db.Connection.DatabaseUpgradedExceptionHasBeenThrown = true;

			Env.Security.OrgBranchProxiesEdit.IsAllowed = false;
			AssertEquals("OrgBranchProxiesEdit should be allowed", true, testSecurityProvider.HasModifyBranchProxies);

			Env.Security.OrgBranchProxiesEdit.IsAllowed = true;
			AssertEquals("OrgBranchProxiesEdit should be allowed", true, testSecurityProvider.HasModifyBranchProxies);

			Db.Connection.DatabaseUpgradedExceptionHasBeenThrown = false;

			testOrg.Factory.RelinquishThreadOwnership();

			Env.Security.OrgBranchProxiesEdit.IsAllowed = true;
			AssertEquals("OrgBranchProxiesEdit should be allowed", true, testSecurityProvider.HasModifyBranchProxies);

			Env.Security.OrgBranchProxiesEdit.IsAllowed = true;
			AssertEquals("OrgBranchProxiesEdit should be allowed", true, testSecurityProvider.HasModifyBranchProxies);
		}

		#endregion

		#region TestHasModifyCompanyProxies

		public void TestHasModifyCompanyProxies()
		{
			Env.Security.OrgCompanyProxiesEdit.IsAllowed = false;
			AssertEquals("OrgCompanyProxiesEdit should NOT be allowed", false, testSecurityProvider.HasModifyCompanyProxies);

			Env.Security.OrgCompanyProxiesEdit.IsAllowed = true;
			AssertEquals("OrgCompanyProxiesEdit should NOT be allowed", true, testSecurityProvider.HasModifyCompanyProxies);

			var org = Factory.LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, "DEMORG");

			var branch = Factory.Load<GlbBranch>(GlbCompany.CurrentCompany.Branches[0].PK); // this is changed
			branch.GB_OH_OrgProxy = org.PK;

			Env.Security.OrgCompanyProxiesEdit.IsAllowed = true;
			AssertEquals("OrgCompanyProxiesEdit should be allowed for branches", true, testSecurityProvider.HasModifyCompanyProxies);

			var newCompany = Factory.NewWithValidTestData<GlbCompany>();
			newCompany.GC_OH_OrgProxy = org.PK;

			Env.Security.OrgCompanyProxiesEdit.IsAllowed = false;
			AssertEquals("OrgCompanyProxiesEdit should NOT be allowed", false, testSecurityProvider.HasModifyCompanyProxies);

			Env.Security.OrgCompanyProxiesEdit.IsAllowed = true;
			AssertEquals("OrgCompanyProxiesEdit should be allowed", true, testSecurityProvider.HasModifyCompanyProxies);

			Db.Connection.DatabaseUpgradedExceptionHasBeenThrown = true;

			Env.Security.OrgCompanyProxiesEdit.IsAllowed = false;
			AssertEquals("OrgCompanyProxiesEdit should be allowed", true, testSecurityProvider.HasModifyCompanyProxies);

			Env.Security.OrgCompanyProxiesEdit.IsAllowed = true;
			AssertEquals("OrgCompanyProxiesEdit should be allowed", true, testSecurityProvider.HasModifyCompanyProxies);

			Db.Connection.DatabaseUpgradedExceptionHasBeenThrown = false;

			testOrg.Factory.RelinquishThreadOwnership();

			Env.Security.OrgCompanyProxiesEdit.IsAllowed = false;
			AssertEquals("OrgCompanyProxiesEdit should be allowed", true, testSecurityProvider.HasModifyCompanyProxies);

			Env.Security.OrgCompanyProxiesEdit.IsAllowed = true;
			AssertEquals("OrgCompanyProxiesEdit should be allowed", true, testSecurityProvider.HasModifyCompanyProxies);
		}

		#endregion

		#region CRM Security

		public void TestHasModifyDetailsStaffAssignmentsAnyRoleSecurity()
		{
			foreach (var pair in Env.Security.OrgDetailsModifyStaffAssignmentsLookup)
			{
				pair.Value.IsAllowed = false;
			}

			AssertEquals(false, testSecurityProvider.HasModifyDetailsStaffAssignmentsAnyRoleSecurity);

			Env.Security.OrgDetailsModifyStaffAssignmentsLookup.First().Value.IsAllowed = true;

			AssertEquals(true, testSecurityProvider.HasModifyDetailsStaffAssignmentsAnyRoleSecurity);
		}

		public void TestCanModifyDetailsStaffAssignment()
		{
			foreach (var pair in Env.Security.OrgDetailsModifyStaffAssignmentsLookup)
			{
				pair.Value.IsAllowed = false;
			}

			AssertEquals(false, testSecurityProvider.CanModifyDetailsStaffAssignment("SAL"));

			Env.Security.OrgDetailsModifyStaffAssignmentsLookup["SAL"].IsAllowed = true;

			AssertEquals(true, testSecurityProvider.CanModifyDetailsStaffAssignment("SAL"));
		}

		#endregion
	}
}
