using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(USAMSLineProductNumberCollection))]
	public class USAMSLineProductNumberCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override Type GetExpectedCollectionType()
		{
			return typeof(USAMSLineProductNumberCollection);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new USAMSLineProductNumberCollection(Factory, "", true);
		}

		public void TestIsList()
		{
			var collection = new USAMSLineProductNumberCollection(Factory, "", true);
			collection.Load();
			AssertEquals(5, collection.Count);
			AssertEquals(0, collection.FilterBusinessObjectDefaults.Count);
			collection = new USAMSLineProductNumberCollection(Factory, AMSProgramList.Codes.PN1, true);
			collection.Load();
			AssertEquals(2, collection.Count);
			AssertEquals(0, collection.FilterBusinessObjectDefaults.Count);
			collection = new USAMSLineProductNumberCollection(Factory, AMSProgramList.Codes.PN1, false);
			collection.Load();
			AssertEquals(5, collection.Count);
			AssertEquals(2, collection.FilterBusinessObjectDefaults.Count);
			AssertEquals("Attribute Name:Property", (ZString)Core.Constants.Customs.Universal.RefCusCodeList.Attributes.USDAAMSProgram, collection.FilterBusinessObjectDefaults["Attribute Name:Property"].Value);
			AssertEquals("Attribute Value:Property", (ZString)Core.Constants.Customs.Universal.RefCusCodeList.AttributeValues.PN1, collection.FilterBusinessObjectDefaults["Attribute Value:Property"].Value);
		}

		public void TestInitialiseFilterDefaults()
		{
			var collection = new USAMSLineProductNumberCollection(Factory, "", true);
			AssertEquals(0, collection.FilterBusinessObjectDefaults.Count);
			collection = new USAMSLineProductNumberCollection(Factory, AMSProgramList.Codes.PN1, true);
			AssertEquals(0, collection.FilterBusinessObjectDefaults.Count);
			collection = new USAMSLineProductNumberCollection(Factory, AMSProgramList.Codes.PN1, false);
			AssertEquals(2, collection.FilterBusinessObjectDefaults.Count);
			AssertEquals("Attribute Name:Property", (ZString)Core.Constants.Customs.Universal.RefCusCodeList.Attributes.USDAAMSProgram, collection.FilterBusinessObjectDefaults["Attribute Name:Property"].Value);
			AssertEquals("Attribute Value:Property", (ZString)Core.Constants.Customs.Universal.RefCusCodeList.AttributeValues.PN1, collection.FilterBusinessObjectDefaults["Attribute Value:Property"].Value);
			collection = new USAMSLineProductNumberCollection(Factory, USAMSLineProductNumberCollection.All, false);
			AssertEquals(2, collection.FilterBusinessObjectDefaults.Count);
			AssertEquals("Attribute Name:Property", (ZString)Core.Constants.Customs.Universal.RefCusCodeList.Attributes.USDAAMSProgram, collection.FilterBusinessObjectDefaults["Attribute Name:Property"].Value);
			AssertEquals("Attribute Value:Property", ZString.Empty, collection.FilterBusinessObjectDefaults["Attribute Value:Property"].Value);
		}

		void CreateAMSProductNumber()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsStandardProductAndServiceCodes, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsStandardProductAndServiceCodes);

			CreateNewOrGetExistingCusCodeListAndAddAttribute(helper, "50102501", new ZString[] {
				Core.Constants.Customs.Universal.RefCusCodeList.AttributeValues.EG1,
				Core.Constants.Customs.Universal.RefCusCodeList.AttributeValues.PN1,
				Core.Constants.Customs.Universal.RefCusCodeList.AttributeValues.OR1,
				Core.Constants.Customs.Universal.RefCusCodeList.AttributeValues.MO6,
				Core.Constants.Customs.Universal.RefCusCodeList.AttributeValues.OTH });
			CreateNewOrGetExistingCusCodeListAndAddAttribute(helper, "50102502", new ZString[] {
				Core.Constants.Customs.Universal.RefCusCodeList.AttributeValues.PN1,
				Core.Constants.Customs.Universal.RefCusCodeList.AttributeValues.OR1,
				Core.Constants.Customs.Universal.RefCusCodeList.AttributeValues.MO6,
				Core.Constants.Customs.Universal.RefCusCodeList.AttributeValues.OTH });
			CreateNewOrGetExistingCusCodeListAndAddAttribute(helper, "50102503", new ZString[] {
				Core.Constants.Customs.Universal.RefCusCodeList.AttributeValues.OR1,
				Core.Constants.Customs.Universal.RefCusCodeList.AttributeValues.MO6,
				Core.Constants.Customs.Universal.RefCusCodeList.AttributeValues.OTH });
			CreateNewOrGetExistingCusCodeListAndAddAttribute(helper, "50102504", new ZString[] {
				Core.Constants.Customs.Universal.RefCusCodeList.AttributeValues.MO6,
				Core.Constants.Customs.Universal.RefCusCodeList.AttributeValues.OTH });
			CreateNewOrGetExistingCusCodeListAndAddAttribute(helper, "50102505", new ZString[] {
				Core.Constants.Customs.Universal.RefCusCodeList.AttributeValues.OTH });
			Factory.Save();
		}

		void CreateNewOrGetExistingCusCodeListAndAddAttribute(UniversalReferenceTestDataHelper helper, ZString code, ZString[] attributeValues)
		{
			var refCusCode = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsStandardProductAndServiceCodes, code, code, new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6));
			var refCusCodePK = refCusCode.PK;
			foreach (var value in attributeValues)
			{
				helper.CreateNewOrGetExistingCusCodeListAttribute(refCusCodePK, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.USDAAMSProgram, value);
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			CreateAMSProductNumber();
		}
	}
}
