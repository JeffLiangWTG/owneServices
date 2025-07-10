using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business.Testing
{
	internal class USAMSLineAddInfoLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestInspectionLocationCodeList()
		{
			var amsLine = amsHeader.AMSLines.AddNew();
			amsLine.US_Party = FoodInspectionAgencyList.Codes.CA;
			AssertNotNull(amsLine.AddInfoLookups.InspectionLocationCodeList);
			AssertEquals(amsLine.AddInfoLookups.InspectionLocationCodeList.Count, new CanadaStatesList().Count);
		}

		public void TestCertTypeCodeList()
		{
			var amsLine = amsHeader.AMSLines.AddNew();
			AssertNotNull(amsLine.AddInfoLookups.CertTypeCodeList);

			Assert(amsLine.AddInfoLookups.CertTypeCodeList.ContainsCode(LPCOTypeList.Codes.AM2));
			Assert(amsLine.AddInfoLookups.CertTypeCodeList.ContainsCode(LPCOTypeList.Codes.AM6));
			Assert(amsLine.AddInfoLookups.CertTypeCodeList.ContainsCode(LPCOTypeList.Codes.AM7));
			Assert(amsLine.AddInfoLookups.CertTypeCodeList.ContainsCode(LPCOTypeList.Codes.AM9));

			amsHeader.US_Program = AMSProgramList.Codes.OR2;
			Assert(!amsLine.AddInfoLookups.CertTypeCodeList.ContainsCode(LPCOTypeList.Codes.AM2));
			Assert(!amsLine.AddInfoLookups.CertTypeCodeList.ContainsCode(LPCOTypeList.Codes.AM6));
			Assert(!amsLine.AddInfoLookups.CertTypeCodeList.ContainsCode(LPCOTypeList.Codes.AM7));
			Assert(!amsLine.AddInfoLookups.CertTypeCodeList.ContainsCode(LPCOTypeList.Codes.AM9));
			Assert(amsLine.AddInfoLookups.CertTypeCodeList.ContainsCode(LPCOTransactionTypeList.Codes.SingleUse));
			Assert(amsLine.AddInfoLookups.CertTypeCodeList.ContainsCode(LPCOTransactionTypeList.Codes.Continuous));
		}

		public void TestOrganizations()
		{
			var amsLine = amsHeader.AMSLines.AddNew();
			AssertNotNull(amsLine.AddInfoLookups.Organizations);
		}

		public void TestProductNumberCodes()
		{
			CreateAMSProductNumber();

			amsHeader.US_Program = AMSProgramList.Codes.PN1;
			var amsLine = amsHeader.AMSLines.AddNew();
			var productNumberList = (CodeDescriptionPairList)amsLine.AddInfoLookups.ProductNumberCodes;
			AssertEquals("PN1", 2, productNumberList.Count);

			amsHeader.US_Program = AMSProgramList.Codes.EG1;
			productNumberList = (CodeDescriptionPairList)amsLine.AddInfoLookups.ProductNumberCodes;
			AssertEquals("EG1", 1, productNumberList.Count);

			amsHeader.US_Program = AMSProgramList.Codes.EG2;
			productNumberList = (CodeDescriptionPairList)amsLine.AddInfoLookups.ProductNumberCodes;
			AssertEquals("EG1", 1, productNumberList.Count);

			amsHeader.US_Program = AMSProgramList.Codes.OR1;
			var productNumberCollection = (USAMSLineProductNumberCollection)amsLine.AddInfoLookups.ProductNumberCodes;
			AssertEquals("Attribute Name:Property", (ZString)Core.Constants.Customs.Universal.RefCusCodeList.Attributes.USDAAMSProgram, productNumberCollection.FilterBusinessObjectDefaults["Attribute Name:Property"].Value);
			AssertEquals("Attribute Value:Property", (ZString)Core.Constants.Customs.Universal.RefCusCodeList.AttributeValues.OR1, productNumberCollection.FilterBusinessObjectDefaults["Attribute Value:Property"].Value);

			amsHeader.US_Program = AMSProgramList.Codes.MO1;
			productNumberCollection = (USAMSLineProductNumberCollection)amsLine.AddInfoLookups.ProductNumberCodes;
			AssertEquals("Attribute Name:Property", (ZString)Core.Constants.Customs.Universal.RefCusCodeList.Attributes.USDAAMSProgram, productNumberCollection.FilterBusinessObjectDefaults["Attribute Name:Property"].Value);
			AssertEquals("Attribute Value:Property", (ZString)Core.Constants.Customs.Universal.RefCusCodeList.AttributeValues.OTH, productNumberCollection.FilterBusinessObjectDefaults["Attribute Value:Property"].Value);

			amsHeader.US_Program = AMSProgramList.Codes.MO4;
			productNumberCollection = (USAMSLineProductNumberCollection)amsLine.AddInfoLookups.ProductNumberCodes;
			AssertEquals(2, productNumberCollection.FilterBusinessObjectDefaults.Count);
			AssertEquals("Attribute Name:Property", (ZString)Core.Constants.Customs.Universal.RefCusCodeList.Attributes.USDAAMSProgram, productNumberCollection.FilterBusinessObjectDefaults["Attribute Name:Property"].Value);
			AssertEquals("Attribute Value:Property", ZString.Empty, productNumberCollection.FilterBusinessObjectDefaults["Attribute Value:Property"].Value);

			amsHeader.US_Program = AMSProgramList.Codes.MO6;
			productNumberCollection = (USAMSLineProductNumberCollection)amsLine.AddInfoLookups.ProductNumberCodes;
			AssertEquals("Attribute Name:Property", (ZString)Core.Constants.Customs.Universal.RefCusCodeList.Attributes.USDAAMSProgram, productNumberCollection.FilterBusinessObjectDefaults["Attribute Name:Property"].Value);
			AssertEquals("Attribute Value:Property", (ZString)Core.Constants.Customs.Universal.RefCusCodeList.AttributeValues.MO6, productNumberCollection.FilterBusinessObjectDefaults["Attribute Value:Property"].Value);
		}

		public void TestPackingTypes()
		{
			var amsLine = amsHeader.AMSLines.AddNew();
			AssertNotNull(amsLine.AddInfoLookups.UnitOfMeasureList);
		}

		public void TestWeightUQList()
		{
			var amsLine = amsHeader.AMSLines.AddNew();
			AssertNotNull(amsLine.AddInfoLookups.UnitOfMeasureList);
		}

		public void TestLotEntity()
		{
			var amsLine = amsHeader.AMSLines.AddNew();
			AssertNotNull(amsLine.AddInfoLookups.LotEntityList);
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

		AMS amsHeader;
		protected override void SetUp()
		{
			base.SetUp();
			var fDeclaration = Factory.New<JobDeclaration>();
			fDeclaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			fDeclaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			var invoiceLine = fDeclaration.Invoices.AddNew().InvoiceLines.AddNew();
			amsHeader = invoiceLine.AMSLines.AddNew();
		}
	}
}
