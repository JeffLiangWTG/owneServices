using System.Collections.Generic;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class PartAttributeUpdateHelperTest : WhsTestCaseWithFactory
	{
		#region TestSetPartAttributeValue

		public void TestSetPartAttributeValue()
		{
			var adjustmentLine = Factory.New<WhsAdjustmentLine>();
			AssertEquals("Precondition", "", adjustmentLine.WE_PartAttrib1);
			AssertEquals("Precondition", "", adjustmentLine.WE_PartAttrib2);
			AssertEquals("Precondition", "", adjustmentLine.WE_PartAttrib3);

			PartAttributeUpdateHelper.SetPartAttributeValue(adjustmentLine, PartAttributeNumber.One, "A1");
			AssertEquals("A1", adjustmentLine.WE_PartAttrib1);

			PartAttributeUpdateHelper.SetPartAttributeValue(adjustmentLine, PartAttributeNumber.Two, "A2");
			AssertEquals("A2", adjustmentLine.WE_PartAttrib2);

			PartAttributeUpdateHelper.SetPartAttributeValue(adjustmentLine, PartAttributeNumber.Three, "A3");
			AssertEquals("A3", adjustmentLine.WE_PartAttrib3);
		}

		#endregion

		#region TestSetPartAttributeUsage

		public void TestSetPartAttributeUsage()
		{
			var relation = Factory.New<OrgPartRelation>();
			AssertEquals("Precondition", false, relation.OU_UsePartAttrib1);
			AssertEquals("Precondition", false, relation.OU_UsePartAttrib2);
			AssertEquals("Precondition", false, relation.OU_UsePartAttrib3);

			PartAttributeUpdateHelper.SetPartAttributeUsage(relation, PartAttributeNumber.One, true);
			AssertEquals(true, relation.OU_UsePartAttrib1);

			PartAttributeUpdateHelper.SetPartAttributeUsage(relation, PartAttributeNumber.Two, true);
			AssertEquals(true, relation.OU_UsePartAttrib2);

			PartAttributeUpdateHelper.SetPartAttributeUsage(relation, PartAttributeNumber.Three, true);
			AssertEquals(true, relation.OU_UsePartAttrib3);
		}

		#endregion

		#region TestGetAttributeWithMatchingType

		public void TestGetAttributeWithMatchingTypeWithTypeDuplicatesListBased()
		{
			var matchingPartAttributes = new List<PartAttributeNumber>();
			var orgMiscServ = Factory.New<OrgMiscServ>();

			orgMiscServ.OM_IMPartAttrib1Type = PartAttributeTypeList.Codes.Mandatory;
			orgMiscServ.OM_IMPartAttrib1Name = "Name1";

			orgMiscServ.OM_IMPartAttrib2Type = PartAttributeTypeList.Codes.Mandatory;
			orgMiscServ.OM_IMPartAttrib2Name = "Name2";

			orgMiscServ.OM_IMPartAttrib3Type = PartAttributeTypeList.Codes.Mandatory;
			orgMiscServ.OM_IMPartAttrib3Name = "Name3";

			AssertEquals(PartAttributeNumber.None, PartAttributeUpdateHelper.GetAttributeWithMatchingTypeAndNameWhenRequired(PartAttributeTypeList.Codes.NonMandatory, "Name1", orgMiscServ, matchingPartAttributes));
			AssertEquals(PartAttributeNumber.None, PartAttributeUpdateHelper.GetAttributeWithMatchingTypeAndNameWhenRequired(PartAttributeTypeList.Codes.Mandatory, "newName1", orgMiscServ, matchingPartAttributes));
			AssertEquals(PartAttributeNumber.One, PartAttributeUpdateHelper.GetAttributeWithMatchingTypeAndNameWhenRequired(PartAttributeTypeList.Codes.Mandatory, "Name1", orgMiscServ, matchingPartAttributes));

			AssertEquals(PartAttributeNumber.None, PartAttributeUpdateHelper.GetAttributeWithMatchingTypeAndNameWhenRequired(PartAttributeTypeList.Codes.NonMandatory, "Name2", orgMiscServ, matchingPartAttributes));
			AssertEquals(PartAttributeNumber.None, PartAttributeUpdateHelper.GetAttributeWithMatchingTypeAndNameWhenRequired(PartAttributeTypeList.Codes.Mandatory, "newName2", orgMiscServ, matchingPartAttributes));
			AssertEquals(PartAttributeNumber.Two, PartAttributeUpdateHelper.GetAttributeWithMatchingTypeAndNameWhenRequired(PartAttributeTypeList.Codes.Mandatory, "Name2", orgMiscServ, matchingPartAttributes));

			AssertEquals(PartAttributeNumber.None, PartAttributeUpdateHelper.GetAttributeWithMatchingTypeAndNameWhenRequired(PartAttributeTypeList.Codes.NonMandatory, "Name3", orgMiscServ, matchingPartAttributes));
			AssertEquals(PartAttributeNumber.None, PartAttributeUpdateHelper.GetAttributeWithMatchingTypeAndNameWhenRequired(PartAttributeTypeList.Codes.Mandatory, "newName3", orgMiscServ, matchingPartAttributes));
			AssertEquals(PartAttributeNumber.Three, PartAttributeUpdateHelper.GetAttributeWithMatchingTypeAndNameWhenRequired(PartAttributeTypeList.Codes.Mandatory, "Name3", orgMiscServ, matchingPartAttributes));

			AssertEquals("Should not return PartAttributeNumber.One as it has been previously matched.", PartAttributeNumber.None, PartAttributeUpdateHelper.GetAttributeWithMatchingTypeAndNameWhenRequired(PartAttributeTypeList.Codes.Mandatory, "Name1", orgMiscServ, matchingPartAttributes));
			AssertEquals("Should not return PartAttributeNumber.Two as it has been previously matched.", PartAttributeNumber.None, PartAttributeUpdateHelper.GetAttributeWithMatchingTypeAndNameWhenRequired(PartAttributeTypeList.Codes.Mandatory, "Name2", orgMiscServ, matchingPartAttributes));
			AssertEquals("Should not return PartAttributeNumber.Three as it has been previously matched.", PartAttributeNumber.None, PartAttributeUpdateHelper.GetAttributeWithMatchingTypeAndNameWhenRequired(PartAttributeTypeList.Codes.Mandatory, "Name3", orgMiscServ, matchingPartAttributes));
		}

		public void TestGetAttributeWithMatchingTypeWithTypeDuplicatesListBasedCaseInsensitive()
		{
			var matchingPartAttributes = new List<PartAttributeNumber>();
			var orgMiscServ = Factory.New<OrgMiscServ>();

			orgMiscServ.OM_IMPartAttrib1Type = PartAttributeTypeList.Codes.Mandatory;
			orgMiscServ.OM_IMPartAttrib1Name = "name1";

			orgMiscServ.OM_IMPartAttrib2Type = PartAttributeTypeList.Codes.Mandatory;
			orgMiscServ.OM_IMPartAttrib2Name = "name2";

			orgMiscServ.OM_IMPartAttrib3Type = PartAttributeTypeList.Codes.Mandatory;
			orgMiscServ.OM_IMPartAttrib3Name = "name3";

			AssertEquals(PartAttributeNumber.None, PartAttributeUpdateHelper.GetAttributeWithMatchingTypeAndNameWhenRequired(PartAttributeTypeList.Codes.NonMandatory, "Name1", orgMiscServ, matchingPartAttributes));
			AssertEquals(PartAttributeNumber.None, PartAttributeUpdateHelper.GetAttributeWithMatchingTypeAndNameWhenRequired(PartAttributeTypeList.Codes.Mandatory, "newName1", orgMiscServ, matchingPartAttributes));
			AssertEquals(PartAttributeNumber.One, PartAttributeUpdateHelper.GetAttributeWithMatchingTypeAndNameWhenRequired(PartAttributeTypeList.Codes.Mandatory, "Name1", orgMiscServ, matchingPartAttributes));

			AssertEquals(PartAttributeNumber.None, PartAttributeUpdateHelper.GetAttributeWithMatchingTypeAndNameWhenRequired(PartAttributeTypeList.Codes.NonMandatory, "Name2", orgMiscServ, matchingPartAttributes));
			AssertEquals(PartAttributeNumber.None, PartAttributeUpdateHelper.GetAttributeWithMatchingTypeAndNameWhenRequired(PartAttributeTypeList.Codes.Mandatory, "newName2", orgMiscServ, matchingPartAttributes));
			AssertEquals(PartAttributeNumber.Two, PartAttributeUpdateHelper.GetAttributeWithMatchingTypeAndNameWhenRequired(PartAttributeTypeList.Codes.Mandatory, "Name2", orgMiscServ, matchingPartAttributes));

			AssertEquals(PartAttributeNumber.None, PartAttributeUpdateHelper.GetAttributeWithMatchingTypeAndNameWhenRequired(PartAttributeTypeList.Codes.NonMandatory, "Name3", orgMiscServ, matchingPartAttributes));
			AssertEquals(PartAttributeNumber.None, PartAttributeUpdateHelper.GetAttributeWithMatchingTypeAndNameWhenRequired(PartAttributeTypeList.Codes.Mandatory, "newName3", orgMiscServ, matchingPartAttributes));
			AssertEquals(PartAttributeNumber.Three, PartAttributeUpdateHelper.GetAttributeWithMatchingTypeAndNameWhenRequired(PartAttributeTypeList.Codes.Mandatory, "Name3", orgMiscServ, matchingPartAttributes));

			AssertEquals("Should not return PartAttributeNumber.One as it has been previously matched.", PartAttributeNumber.None, PartAttributeUpdateHelper.GetAttributeWithMatchingTypeAndNameWhenRequired(PartAttributeTypeList.Codes.Mandatory, "Name1", orgMiscServ, matchingPartAttributes));
			AssertEquals("Should not return PartAttributeNumber.Two as it has been previously matched.", PartAttributeNumber.None, PartAttributeUpdateHelper.GetAttributeWithMatchingTypeAndNameWhenRequired(PartAttributeTypeList.Codes.Mandatory, "Name2", orgMiscServ, matchingPartAttributes));
			AssertEquals("Should not return PartAttributeNumber.Three as it has been previously matched.", PartAttributeNumber.None, PartAttributeUpdateHelper.GetAttributeWithMatchingTypeAndNameWhenRequired(PartAttributeTypeList.Codes.Mandatory, "Name3", orgMiscServ, matchingPartAttributes));
		}

		public void TestGetAttributeWithMatchingTypeWithTypeDuplicatesDictionaryBased()
		{
			var matchingPartAttributes = new Dictionary<PartAttributeNumber, PartAttributeNumber>();
			var orgMiscServ = Factory.New<OrgMiscServ>();

			orgMiscServ.OM_IMPartAttrib1Type = PartAttributeTypeList.Codes.Mandatory;
			orgMiscServ.OM_IMPartAttrib1Name = "Name1";

			orgMiscServ.OM_IMPartAttrib2Type = PartAttributeTypeList.Codes.Mandatory;
			orgMiscServ.OM_IMPartAttrib2Name = "Name2";

			orgMiscServ.OM_IMPartAttrib3Type = PartAttributeTypeList.Codes.Mandatory;
			orgMiscServ.OM_IMPartAttrib3Name = "Name3";

			AssertEquals(PartAttributeNumber.None, PartAttributeUpdateHelper.GetAttributeWithMatchingTypeAndNameWhenRequired(PartAttributeTypeList.Codes.NonMandatory, "Name1", orgMiscServ, PartAttributeNumber.One, matchingPartAttributes));
			AssertEquals(PartAttributeNumber.None, PartAttributeUpdateHelper.GetAttributeWithMatchingTypeAndNameWhenRequired(PartAttributeTypeList.Codes.Mandatory, "newName1", orgMiscServ, PartAttributeNumber.One, matchingPartAttributes));
			AssertEquals(PartAttributeNumber.One, PartAttributeUpdateHelper.GetAttributeWithMatchingTypeAndNameWhenRequired(PartAttributeTypeList.Codes.Mandatory, "Name1", orgMiscServ, PartAttributeNumber.One, matchingPartAttributes));

			AssertEquals(PartAttributeNumber.None, PartAttributeUpdateHelper.GetAttributeWithMatchingTypeAndNameWhenRequired(PartAttributeTypeList.Codes.NonMandatory, "Name2", orgMiscServ, PartAttributeNumber.Two, matchingPartAttributes));
			AssertEquals(PartAttributeNumber.None, PartAttributeUpdateHelper.GetAttributeWithMatchingTypeAndNameWhenRequired(PartAttributeTypeList.Codes.Mandatory, "newName2", orgMiscServ, PartAttributeNumber.Two, matchingPartAttributes));
			AssertEquals(PartAttributeNumber.Two, PartAttributeUpdateHelper.GetAttributeWithMatchingTypeAndNameWhenRequired(PartAttributeTypeList.Codes.Mandatory, "Name2", orgMiscServ, PartAttributeNumber.Two, matchingPartAttributes));

			AssertEquals(PartAttributeNumber.None, PartAttributeUpdateHelper.GetAttributeWithMatchingTypeAndNameWhenRequired(PartAttributeTypeList.Codes.NonMandatory, "Name3", orgMiscServ, PartAttributeNumber.Three, matchingPartAttributes));
			AssertEquals(PartAttributeNumber.None, PartAttributeUpdateHelper.GetAttributeWithMatchingTypeAndNameWhenRequired(PartAttributeTypeList.Codes.Mandatory, "newName3", orgMiscServ, PartAttributeNumber.Three, matchingPartAttributes));
			AssertEquals(PartAttributeNumber.Three, PartAttributeUpdateHelper.GetAttributeWithMatchingTypeAndNameWhenRequired(PartAttributeTypeList.Codes.Mandatory, "Name3", orgMiscServ, PartAttributeNumber.Three, matchingPartAttributes));

			AssertEquals("Should not return PartAttributeNumber.One as it has been previously matched.", PartAttributeNumber.None, PartAttributeUpdateHelper.GetAttributeWithMatchingTypeAndNameWhenRequired(PartAttributeTypeList.Codes.Mandatory, "Name1", orgMiscServ, PartAttributeNumber.One, matchingPartAttributes));
			AssertEquals("Should not return PartAttributeNumber.Two as it has been previously matched.", PartAttributeNumber.None, PartAttributeUpdateHelper.GetAttributeWithMatchingTypeAndNameWhenRequired(PartAttributeTypeList.Codes.Mandatory, "Name2", orgMiscServ, PartAttributeNumber.Two, matchingPartAttributes));
			AssertEquals("Should not return PartAttributeNumber.Three as it has been previously matched.", PartAttributeNumber.None, PartAttributeUpdateHelper.GetAttributeWithMatchingTypeAndNameWhenRequired(PartAttributeTypeList.Codes.Mandatory, "Name3", orgMiscServ, PartAttributeNumber.Three, matchingPartAttributes));
		}

		public void TestGetAttributeWithMatchingTypeWithTypeDuplicatesDictionaryBasedCaseInsensitive()
		{
			var matchingPartAttributes = new Dictionary<PartAttributeNumber, PartAttributeNumber>();
			var orgMiscServ = Factory.New<OrgMiscServ>();

			orgMiscServ.OM_IMPartAttrib1Type = PartAttributeTypeList.Codes.Mandatory;
			orgMiscServ.OM_IMPartAttrib1Name = "name1";

			orgMiscServ.OM_IMPartAttrib2Type = PartAttributeTypeList.Codes.Mandatory;
			orgMiscServ.OM_IMPartAttrib2Name = "name2";

			orgMiscServ.OM_IMPartAttrib3Type = PartAttributeTypeList.Codes.Mandatory;
			orgMiscServ.OM_IMPartAttrib3Name = "name3";

			AssertEquals(PartAttributeNumber.None, PartAttributeUpdateHelper.GetAttributeWithMatchingTypeAndNameWhenRequired(PartAttributeTypeList.Codes.NonMandatory, "Name1", orgMiscServ, PartAttributeNumber.One, matchingPartAttributes));
			AssertEquals(PartAttributeNumber.None, PartAttributeUpdateHelper.GetAttributeWithMatchingTypeAndNameWhenRequired(PartAttributeTypeList.Codes.Mandatory, "newName1", orgMiscServ, PartAttributeNumber.One, matchingPartAttributes));
			AssertEquals(PartAttributeNumber.One, PartAttributeUpdateHelper.GetAttributeWithMatchingTypeAndNameWhenRequired(PartAttributeTypeList.Codes.Mandatory, "Name1", orgMiscServ, PartAttributeNumber.One, matchingPartAttributes));

			AssertEquals(PartAttributeNumber.None, PartAttributeUpdateHelper.GetAttributeWithMatchingTypeAndNameWhenRequired(PartAttributeTypeList.Codes.NonMandatory, "Name2", orgMiscServ, PartAttributeNumber.Two, matchingPartAttributes));
			AssertEquals(PartAttributeNumber.None, PartAttributeUpdateHelper.GetAttributeWithMatchingTypeAndNameWhenRequired(PartAttributeTypeList.Codes.Mandatory, "newName2", orgMiscServ, PartAttributeNumber.Two, matchingPartAttributes));
			AssertEquals(PartAttributeNumber.Two, PartAttributeUpdateHelper.GetAttributeWithMatchingTypeAndNameWhenRequired(PartAttributeTypeList.Codes.Mandatory, "Name2", orgMiscServ, PartAttributeNumber.Two, matchingPartAttributes));

			AssertEquals(PartAttributeNumber.None, PartAttributeUpdateHelper.GetAttributeWithMatchingTypeAndNameWhenRequired(PartAttributeTypeList.Codes.NonMandatory, "Name3", orgMiscServ, PartAttributeNumber.Three, matchingPartAttributes));
			AssertEquals(PartAttributeNumber.None, PartAttributeUpdateHelper.GetAttributeWithMatchingTypeAndNameWhenRequired(PartAttributeTypeList.Codes.Mandatory, "newName3", orgMiscServ, PartAttributeNumber.Three, matchingPartAttributes));
			AssertEquals(PartAttributeNumber.Three, PartAttributeUpdateHelper.GetAttributeWithMatchingTypeAndNameWhenRequired(PartAttributeTypeList.Codes.Mandatory, "Name3", orgMiscServ, PartAttributeNumber.Three, matchingPartAttributes));

			AssertEquals("Should not return PartAttributeNumber.One as it has been previously matched.", PartAttributeNumber.None, PartAttributeUpdateHelper.GetAttributeWithMatchingTypeAndNameWhenRequired(PartAttributeTypeList.Codes.Mandatory, "Name1", orgMiscServ, PartAttributeNumber.One, matchingPartAttributes));
			AssertEquals("Should not return PartAttributeNumber.Two as it has been previously matched.", PartAttributeNumber.None, PartAttributeUpdateHelper.GetAttributeWithMatchingTypeAndNameWhenRequired(PartAttributeTypeList.Codes.Mandatory, "Name2", orgMiscServ, PartAttributeNumber.Two, matchingPartAttributes));
			AssertEquals("Should not return PartAttributeNumber.Three as it has been previously matched.", PartAttributeNumber.None, PartAttributeUpdateHelper.GetAttributeWithMatchingTypeAndNameWhenRequired(PartAttributeTypeList.Codes.Mandatory, "Name3", orgMiscServ, PartAttributeNumber.Three, matchingPartAttributes));
		}

		public void TestGetAttributeWithMatchingTypeWithoutTypeDuplicatesListBased()
		{
			var matchingPartAttributes = new List<PartAttributeNumber>();
			var orgMiscServ = Factory.New<OrgMiscServ>();

			orgMiscServ.OM_IMPartAttrib1Type = PartAttributeTypeList.Codes.Mandatory;
			orgMiscServ.OM_IMPartAttrib1Name = "Name1";

			orgMiscServ.OM_IMPartAttrib2Type = PartAttributeTypeList.Codes.NonMandatory;
			orgMiscServ.OM_IMPartAttrib2Name = "Name2";

			orgMiscServ.OM_IMPartAttrib3Type = PartAttributeTypeList.Codes.BatchNumber;
			orgMiscServ.OM_IMPartAttrib3Name = "Name3";

			AssertEquals(PartAttributeNumber.None, PartAttributeUpdateHelper.GetAttributeWithMatchingTypeAndNameWhenRequired(PartAttributeTypeList.Codes.JulianBatchNumber, "newName1", orgMiscServ, matchingPartAttributes));
			AssertEquals(PartAttributeNumber.None, PartAttributeUpdateHelper.GetAttributeWithMatchingTypeAndNameWhenRequired(PartAttributeTypeList.Codes.JulianBatchNumber, "Name1", orgMiscServ, matchingPartAttributes));

			AssertEquals("Should return PartAttributeNumber.One as there are no duplicated types.", PartAttributeNumber.One, PartAttributeUpdateHelper.GetAttributeWithMatchingTypeAndNameWhenRequired(PartAttributeTypeList.Codes.Mandatory, "newName1", orgMiscServ, matchingPartAttributes));
			AssertEquals(PartAttributeNumber.None, PartAttributeUpdateHelper.GetAttributeWithMatchingTypeAndNameWhenRequired(PartAttributeTypeList.Codes.Mandatory, "newName1", orgMiscServ, matchingPartAttributes));
			AssertEquals(PartAttributeNumber.None, PartAttributeUpdateHelper.GetAttributeWithMatchingTypeAndNameWhenRequired(PartAttributeTypeList.Codes.Mandatory, "Name1", orgMiscServ, matchingPartAttributes));

			AssertEquals("Should return PartAttributeNumber.Two as there are no duplicated types.", PartAttributeNumber.Two, PartAttributeUpdateHelper.GetAttributeWithMatchingTypeAndNameWhenRequired(PartAttributeTypeList.Codes.NonMandatory, "newName2", orgMiscServ, matchingPartAttributes));
			AssertEquals(PartAttributeNumber.None, PartAttributeUpdateHelper.GetAttributeWithMatchingTypeAndNameWhenRequired(PartAttributeTypeList.Codes.NonMandatory, "newName2", orgMiscServ, matchingPartAttributes));
			AssertEquals(PartAttributeNumber.None, PartAttributeUpdateHelper.GetAttributeWithMatchingTypeAndNameWhenRequired(PartAttributeTypeList.Codes.NonMandatory, "Name2", orgMiscServ, matchingPartAttributes));

			AssertEquals("Should return PartAttributeNumber.Three as there are no duplicated types.", PartAttributeNumber.Three, PartAttributeUpdateHelper.GetAttributeWithMatchingTypeAndNameWhenRequired(PartAttributeTypeList.Codes.BatchNumber, "newName3", orgMiscServ, matchingPartAttributes));
			AssertEquals(PartAttributeNumber.None, PartAttributeUpdateHelper.GetAttributeWithMatchingTypeAndNameWhenRequired(PartAttributeTypeList.Codes.BatchNumber, "newName3", orgMiscServ, matchingPartAttributes));
			AssertEquals(PartAttributeNumber.None, PartAttributeUpdateHelper.GetAttributeWithMatchingTypeAndNameWhenRequired(PartAttributeTypeList.Codes.BatchNumber, "Name3", orgMiscServ, matchingPartAttributes));
		}

		public void TestGetAttributeWithMatchingTypeWithoutTypeDuplicatesDictionaryBased()
		{
			var matchingPartAttributes = new Dictionary<PartAttributeNumber, PartAttributeNumber>();
			var orgMiscServ = Factory.New<OrgMiscServ>();

			orgMiscServ.OM_IMPartAttrib1Type = PartAttributeTypeList.Codes.Mandatory;
			orgMiscServ.OM_IMPartAttrib1Name = "Name1";

			orgMiscServ.OM_IMPartAttrib2Type = PartAttributeTypeList.Codes.NonMandatory;
			orgMiscServ.OM_IMPartAttrib2Name = "Name2";

			orgMiscServ.OM_IMPartAttrib3Type = PartAttributeTypeList.Codes.BatchNumber;
			orgMiscServ.OM_IMPartAttrib3Name = "Name3";

			AssertEquals(PartAttributeNumber.None, PartAttributeUpdateHelper.GetAttributeWithMatchingTypeAndNameWhenRequired(PartAttributeTypeList.Codes.JulianBatchNumber, "newName1", orgMiscServ, PartAttributeNumber.One, matchingPartAttributes));
			AssertEquals(PartAttributeNumber.None, PartAttributeUpdateHelper.GetAttributeWithMatchingTypeAndNameWhenRequired(PartAttributeTypeList.Codes.JulianBatchNumber, "Name1", orgMiscServ, PartAttributeNumber.One, matchingPartAttributes));

			AssertEquals("Should return PartAttributeNumber.One as there are no duplicated types.", PartAttributeNumber.One, PartAttributeUpdateHelper.GetAttributeWithMatchingTypeAndNameWhenRequired(PartAttributeTypeList.Codes.Mandatory, "newName1", orgMiscServ, PartAttributeNumber.One, matchingPartAttributes));
			AssertEquals(PartAttributeNumber.None, PartAttributeUpdateHelper.GetAttributeWithMatchingTypeAndNameWhenRequired(PartAttributeTypeList.Codes.Mandatory, "newName1", orgMiscServ, PartAttributeNumber.One, matchingPartAttributes));
			AssertEquals(PartAttributeNumber.None, PartAttributeUpdateHelper.GetAttributeWithMatchingTypeAndNameWhenRequired(PartAttributeTypeList.Codes.Mandatory, "Name1", orgMiscServ, PartAttributeNumber.One, matchingPartAttributes));

			AssertEquals("Should return PartAttributeNumber.Two as there are no duplicated types.", PartAttributeNumber.Two, PartAttributeUpdateHelper.GetAttributeWithMatchingTypeAndNameWhenRequired(PartAttributeTypeList.Codes.NonMandatory, "newName2", orgMiscServ, PartAttributeNumber.Two, matchingPartAttributes));
			AssertEquals(PartAttributeNumber.None, PartAttributeUpdateHelper.GetAttributeWithMatchingTypeAndNameWhenRequired(PartAttributeTypeList.Codes.NonMandatory, "newName2", orgMiscServ, PartAttributeNumber.Two, matchingPartAttributes));
			AssertEquals(PartAttributeNumber.None, PartAttributeUpdateHelper.GetAttributeWithMatchingTypeAndNameWhenRequired(PartAttributeTypeList.Codes.NonMandatory, "Name2", orgMiscServ, PartAttributeNumber.Two, matchingPartAttributes));

			AssertEquals("Should return PartAttributeNumber.Three as there are no duplicated types.", PartAttributeNumber.Three, PartAttributeUpdateHelper.GetAttributeWithMatchingTypeAndNameWhenRequired(PartAttributeTypeList.Codes.BatchNumber, "newName3", orgMiscServ, PartAttributeNumber.Three, matchingPartAttributes));
			AssertEquals(PartAttributeNumber.None, PartAttributeUpdateHelper.GetAttributeWithMatchingTypeAndNameWhenRequired(PartAttributeTypeList.Codes.BatchNumber, "newName3", orgMiscServ, PartAttributeNumber.Three, matchingPartAttributes));
			AssertEquals(PartAttributeNumber.None, PartAttributeUpdateHelper.GetAttributeWithMatchingTypeAndNameWhenRequired(PartAttributeTypeList.Codes.BatchNumber, "Name3", orgMiscServ, PartAttributeNumber.Three, matchingPartAttributes));
		}

		#endregion
	}
}
