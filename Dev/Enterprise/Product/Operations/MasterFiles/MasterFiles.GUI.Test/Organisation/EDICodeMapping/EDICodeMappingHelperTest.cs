using Enterprise.Core;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Test
{
	public class EDICodeMappingHelperTest : TestCase
	{
		public void TestControlTypeAsPerRelationship()
		{
			AssertControlTypeAsPerRelationship(Constants.OrgPatternMatchOverrideRelationships.ChargeCodes, false, true, false);

			AssertControlTypeAsPerRelationship(Constants.OrgPatternMatchOverrideRelationships.Organisation, true, false, false);
			AssertControlTypeAsPerRelationship(Constants.OrgPatternMatchOverrideRelationships.Port, true, false, false);
			AssertControlTypeAsPerRelationship(Constants.OrgPatternMatchOverrideRelationships.Currency, true, false, false);
			AssertControlTypeAsPerRelationship(Constants.OrgPatternMatchOverrideRelationships.Country, true, false, false);
			AssertControlTypeAsPerRelationship(Constants.OrgPatternMatchOverrideRelationships.Commodities, true, false, false);
			AssertControlTypeAsPerRelationship(Constants.OrgPatternMatchOverrideRelationships.Equipment, true, false, false);
			AssertControlTypeAsPerRelationship(Constants.OrgPatternMatchOverrideRelationships.ContainerType, true, false, false);
			AssertControlTypeAsPerRelationship(Constants.OrgPatternMatchOverrideRelationships.Warehouse, true, false, false);
			AssertControlTypeAsPerRelationship(Constants.OrgPatternMatchOverrideRelationships.ServiceLevel, true, false, false);
			AssertControlTypeAsPerRelationship(Constants.OrgPatternMatchOverrideRelationships.IntZone, true, false, false);
			AssertControlTypeAsPerRelationship(Constants.OrgPatternMatchOverrideRelationships.DocumentType, true, false, false);

			AssertControlTypeAsPerRelationship(Constants.OrgPatternMatchOverrideRelationships.DropMode, false, false, true);
			AssertControlTypeAsPerRelationship(Constants.OrgPatternMatchOverrideRelationships.IncoTerm, false, false, true);
			AssertControlTypeAsPerRelationship(Constants.OrgPatternMatchOverrideRelationships.PackageType, false, false, true);
			AssertControlTypeAsPerRelationship(Constants.OrgPatternMatchOverrideRelationships.EventCode, false, false, true);
			AssertControlTypeAsPerRelationship(Constants.OrgPatternMatchOverrideRelationships.CarrierServiceLevel, false, false, true);
			AssertControlTypeAsPerRelationship("EHC", false, false, true); // EHubClientID
		}

		#region Implementation

		void AssertControlTypeAsPerRelationship(string relationship, bool isGuidFindBox, bool isCodeFindBox, bool isDropDownFindBox)
		{
			AssertEquals(isGuidFindBox, EDICodeMappingHelper.IsGuidFindBox(relationship));
			AssertEquals(isCodeFindBox, EDICodeMappingHelper.IsCodeFindBox(relationship));
			AssertEquals(isDropDownFindBox, EDICodeMappingHelper.IsDropDownFindBox(relationship));
		}

		#endregion
	}
}
