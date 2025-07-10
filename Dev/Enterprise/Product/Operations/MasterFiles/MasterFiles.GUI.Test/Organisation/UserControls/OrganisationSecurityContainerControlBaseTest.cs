using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using NUnit.Framework.TestHelper;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestsSubclassesOf(typeof(OrganisationSecurityContainerControl))]
	public abstract class OrganisationSecurityContainerControlBaseTest : TestCaseWithFactory
	{
		protected virtual void SetDefaultsOnNewControl(OrganisationSecurityContainerControl control)
		{
		}

		protected abstract OrganisationSecurityContainerControl GetNewControlForTesting();

		protected Type ControlTypeForTesting => TestedTypeHelper.GetTestedType(GetType());

		protected abstract string[] SecurityContainerPropertiesEnabledForThisControl { get; }

		readonly string[] alwaysAppliedSecurityItems = new[] { "IsModifyBranchProxies", "IsModifyCompanyProxies" };

		#region Control Has Correct Security Settings

		/// <summary>
		/// This checks that the list of security items from the test are all enabled on the control
		/// </summary>
		[RequiresSTA]
		public void TestControlHasSpecifiedSecuritySettingsSet()
		{
			foreach (string securityControlSecurityItem in SecurityContainerPropertiesEnabledForThisControl)
			{
				PropertyInfo info = ControlTypeForTesting.GetProperty(securityControlSecurityItem);
				AssertNotNull("PRE: Security Item " + securityControlSecurityItem + " should be a valid property on OrganisationSecurityContainerControl", info);
				using (OrganisationSecurityContainerControl testControl = GetNewControlForTesting())
				{
					bool value = (bool)info.GetValue(testControl, null);
					Assert("The security settings on control " + ControlTypeForTesting.Name + " SHOULD have " + securityControlSecurityItem + " set to true", value);
				}
			}
		}

		[RequiresSTA]
		/// <summary>
		/// This checks that there are no security items enabled on the control that ARENT in the test-specified list
		/// </summary>
		public void TestAllSecurityItemsEnabledOnControlAreAllowed()
		{
			var controlSecurityItems = ControlTypeForTesting.GetProperties(BindingFlags.Instance | BindingFlags.Public);
			foreach (var info in controlSecurityItems)
			{
				if (info.PropertyType == typeof(bool) && info.Name.StartsWith("IsModify"))
				{
					using (var testControl = GetNewControlForTesting())
					{
						if (!alwaysAppliedSecurityItems.Contains(info.Name))
						{
							var value = (bool)info.GetValue(testControl, null);
							if (value)
							{
								var itemErroneouslyActive = !Array.Exists(SecurityContainerPropertiesEnabledForThisControl, delegate(string currentSecurityItem)
								{
									return currentSecurityItem == info.Name;
								}
								);

								Assert("The security item " + info.Name + " was enabled for the control " + ControlTypeForTesting.Name + " BUT was not specified in the test list of expected security items", !itemErroneouslyActive);
							}
						}
					}
				}
			}
		}

		#endregion

		#region Test All Security Item Properties Bound To Panel Are Displayed

		[RequiresSTA]
		public void TestAllSecurityItemPropertiesBoundToPanelAreDisplayed()
		{
			PropertyInfo[] controlSecurityItems = ControlTypeForTesting.GetProperties(BindingFlags.Instance | BindingFlags.Public);
			foreach (PropertyInfo info in controlSecurityItems)
			{
				if (info.PropertyType == typeof(bool) && info.Name.StartsWith("IsModify"))
				{
					using (OrganisationSecurityContainerControl testControl = ControlForTesting)
					{
						bool value = (bool)info.GetValue(testControl, null);
						if (value)
						{
							string checkPointName = GetCheckpointNameFromSecurityControlItem(info.Name);
							SecurityCheckpoint cp = Env.Security.FindCheckPoint(new CheckpointLookupKey(checkPointName)); // Cant use FindCheckPoint because we want to reflect out property names from security class
							AssertNotNull("PRE: The checkpoint " + checkPointName + " should have been found in Env.Security (info was: " + info.Name + ")", cp);
							if (!alwaysAppliedSecurityItems.Contains(info.Name))
							{
								AssertSecurityItemCanTogglePanel(info, cp);
							}
						}
					}
				}
			}
		}

		/// <summary>
		/// There is (currently) one Org checkpoint in the system which actually maps to a different module. 
		/// EG: The opportunity Management filter maps to the Opportunity Management security items
		/// </summary>
		Dictionary<string, string> CheckPointMappingTable
		{
			get
			{
				if (fCheckPointMappingTable == null)
				{
					fCheckPointMappingTable = new Dictionary<string, string>();
					fCheckPointMappingTable.Add("OrgSalesModifyOpportunityManagement", "OpportunityManagementEdit");
					fCheckPointMappingTable.Add("OrgBranchModifyProxies", "OrgBranchProxiesEdit");
					fCheckPointMappingTable.Add("OrgCompanyModifyProxies", "OrgCompanyProxiesEdit");
					fCheckPointMappingTable.Add("OrgAddressModifyCapabilities", "OrgAddressCapabilitiesModify");
					fCheckPointMappingTable.Add("OrgAddressModifyCapabilitiesARAP", "OrgAddressCapabilitiesARAP");
					fCheckPointMappingTable.Add("OrgAddressModifyCapabilitiesNonARAP", "OrgAddressCapabilitiesNonARAP");
				}

				return fCheckPointMappingTable;
			}
		}
		Dictionary<string, string> fCheckPointMappingTable;

		public virtual string GetCheckpointNameFromSecurityControlItem(string itemName)
		{
			Regex nameDelimiter = new Regex("([A-Z1-9])", RegexOptions.Compiled);
			string[] delimitedNameParts = nameDelimiter.Replace(itemName, " $1").Trim().Split(' ');
			string result = "Org" + delimitedNameParts[2] + "Modify";

			if (delimitedNameParts.Length > 3)
			{
				for (int i = 3; i < delimitedNameParts.Length; i++)
				{
					result += delimitedNameParts[i];
				}
			}

			string mappedName;
			CheckPointMappingTable.TryGetValue(result, out mappedName);

			return mappedName ?? result;
		}

		void AssertSecurityItemCanTogglePanel(PropertyInfo securityContainerPropertyToTest, SecurityCheckpoint checkpointForTest)
		{
			//				ITEM TOGGLED			SECURITY ALLOWED			PANEL SHOWS 
			//						true								false								true			
			//						true								true								false			
			//						false								true								false			
			//						false								false								false			

			AssertPanelVisibilityForSecurityItem(securityContainerPropertyToTest, checkpointForTest, true, false, true);
			AssertPanelVisibilityForSecurityItem(securityContainerPropertyToTest, checkpointForTest, true, true, false);
			AssertPanelVisibilityForSecurityItem(securityContainerPropertyToTest, checkpointForTest, false, true, false);
			AssertPanelVisibilityForSecurityItem(securityContainerPropertyToTest, checkpointForTest, false, false, false);
		}

		void AssertPanelVisibilityForSecurityItem(PropertyInfo securityContainerPropertyToTest, SecurityCheckpoint checkpointForTest, bool controlSecurityEnabled, bool checkPointSecurityAllowed, bool expectSecurityPanelToBeDisplayed)
		{
			bool oldSecurityValue = checkpointForTest.IsAllowed;
			try
			{
				checkpointForTest.IsAllowed = checkPointSecurityAllowed;
				OrgHeader testHeader = LoadAndSetupDEMOrgHeaderForTest();

				using (ZForm testForm = new ZForm(testHeader))
				{
					using (OrganisationSecurityContainerControl testControl = ControlForTesting)
					{
						testForm.Controls.Add(testControl);
						testForm.Width = testControl.Width + 1;
						testForm.Height = testControl.Height;
						securityContainerPropertyToTest.SetValue(testControl, controlSecurityEnabled, null);

						testForm.Show();
						AssertEquals("Visiblity of security panel for security item " + securityContainerPropertyToTest.Name, expectSecurityPanelToBeDisplayed, testControl.SecurityPanel.Visible);
					}
				}
			}
			finally
			{
				checkpointForTest.IsAllowed = oldSecurityValue;
			}
		}

		protected virtual OrgHeader LoadAndSetupDEMOrgHeaderForTest()
		{
			return Factory.LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, "DEMORG");
		}

		#endregion

		#region TestHeader

		protected OrgHeader TestHeader
		{
			get
			{
				if (fTestHeader == null)
				{
					fTestHeader = OrgHeader.New(Factory);
					fTestHeader.FillWithValidTestData();
				}

				return fTestHeader;
			}
		}
		OrgHeader fTestHeader;

		#endregion

		#region ControlForTesting

		protected OrganisationSecurityContainerControl ControlForTesting
		{
			get
			{
				OrganisationSecurityContainerControl result = GetNewControlForTesting();
				SetDefaultsOnNewControl(result);
				return result;
			}
		}

		#endregion

		#region Test always applied security items

		[RequiresSTA]
		public void TestAlwaysAppliedSecurityItems()
		{
			var controlSecurityItems = ControlTypeForTesting.GetProperties(BindingFlags.Instance | BindingFlags.Public);
			foreach (var info in controlSecurityItems)
			{
				if (info.PropertyType == typeof(bool) && alwaysAppliedSecurityItems.Contains(info.Name))
				{
					using (var testControl = GetNewControlForTesting())
					{
						AssertEquals(true, (bool)info.GetValue(testControl, null));
					}
				}
			}
		}

		#endregion
	}
}
