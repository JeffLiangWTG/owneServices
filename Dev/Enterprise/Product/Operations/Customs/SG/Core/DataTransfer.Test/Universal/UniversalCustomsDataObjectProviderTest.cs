using CargoWise.EntityFramework;
using Enterprise.Customs.Common.SG;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.Customs.SG.V4.Business;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.SG.V4.DataTransfer.Universal.Testing
{
	partial class UniversalCustomsDataObjectProviderTest : UniversalDataBuss.Management.Testing.TestCaseWithFactoryAndMessagingHelpers
	{
		public void TestGetNewUniversalDataObjectReaderHelper()
		{
			AssertType<UniversalDataObjectReaderHelper>(new UniversalCustomsDataObjectProvider().GetNewUniversalDataObjectReaderHelper(new UniversalObjectFactory(new BusinessObjectFactory()), Core.Constants.CountryCodes.Australia, string.Empty));
		}

		public void TestTableSpecificCusAddInfoTypeListEndToEnd()
		{
			var sql = @"
IF (OBJECT_ID('Constraint_B7_Type') IS NOT NULL)
BEGIN
    ALTER TABLE dbo.CusAddInfo NOCHECK CONSTRAINT Constraint_B7_Type
END";
			TestConnection.ExecuteNonQuery(sql);

			var declaration = Factory.New<JobDeclaration>();
			var cpc = declaration.CPCs.AddNew();
			cpc.SG_CPCCode = "1";
			var dummyAddInfo = declaration.CPCs.AddNew();
			dummyAddInfo.B7_Type = "!@#";
			dummyAddInfo.SG_CPCCode = "1";
			Factory.SaveForTesting();
			var newFactory = new BusinessObjectFactory();
			declaration = newFactory.Load<JobDeclaration>(declaration.PK);
			var dataObject = (Shipment)Enterprise.MasterFiles.DataTransfer.Universal.Workflow.UniversalXmlWriter.GetDataObject(RecipientRoleType.ORP, declaration);
			AssertEquals("Invalid types for declaration should not be included", 1, dataObject.AddInfoGroupCollection.Count);
			var addInfoGroup = dataObject.AddInfoGroupCollection[0];
			AssertEquals("addInfoGroup.Type.Code", Enterprise.Customs.Business.MultiLineAddInfos.CusAddInfoTypeAttribute.Codes.SGCustomsProcedureCode, addInfoGroup.Type.GetCodeAsUpperCase());
			AssertEquals("addInfoGroup.Type.Description", "Customs Procedure Code", addInfoGroup.Type.Description.GetValueOrDefault());
		}

		public void TestTableSpecificCusCodeDataTypeListEndToEnd()
		{
			var declaration = Factory.New<JobDeclaration>();
			var licence = declaration.CALicences.AddNew();
			licence.CY_Type = CusCodeDataTypeList.Codes.CALicenceNumber;
			licence.CY_Data = "1";
			var permit2 = declaration.CALicences.AddNew();
			permit2.CY_Type = CusCodeDataTypeList.Codes.CMD;
			permit2.CY_Data = "2";
			Factory.SaveForTesting();

			var newFactory = new BusinessObjectFactory();
			declaration = newFactory.Load<JobDeclaration>(declaration.PK);
			var dataObject = (Shipment)Enterprise.MasterFiles.DataTransfer.Universal.Workflow.UniversalXmlWriter.GetDataObject(RecipientRoleType.ORP, declaration);
			AssertEquals(1, dataObject.CustomsReferenceCollection.Count);
			var customsReference = dataObject.CustomsReferenceCollection[0];
			AssertEquals("customsReference.Type.Code", CusCodeDataTypeList.Codes.CALicenceNumber, customsReference.Type.GetCodeAsUpperCase());
			AssertEquals("customsReference.Type.Description", CusCodeDataTypeList.Descriptions.CALicenceNumber, customsReference.Type.Description);
		}

		public void TestGetNewJobDeclarationDataObjectReader()
		{
			AssertType(typeof(JobDeclarationDataObjectReader), new UniversalCustomsDataObjectProvider().GetNewJobDeclarationDataObjectReader(new Shipment(DefaultDataObjectWriterStrategy.TestInstance), new TestErrorLogger(), Factory, null));
		}

		public void TestTableSpecificAddInfoGroupTypesNeedInsertedToOtherTableList()
		{
			AssertNull(new UniversalCustomsDataObjectProvider().TableSpecificAddInfoGroupTypesNeedInsertedToOtherTableList("JE", ""));
		}

		public void TestTableSpecificCusReferenceTypeList()
		{
			AssertNull(new UniversalCustomsDataObjectProvider().TableSpecificCusReferenceTypeList(CusEntryInstructionSchema.Constants.Prefix, string.Empty));
		}
	}
}
