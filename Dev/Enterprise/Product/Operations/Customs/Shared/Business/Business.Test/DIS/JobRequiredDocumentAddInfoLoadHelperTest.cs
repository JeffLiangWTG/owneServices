using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.DIS;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business.Testing
{
	sealed class JobRequiredDocumentAddInfoLoadHelperTest : TestCaseWithFactory
	{
		public void TestGetJobRequiredDocumentAddInfoFilter()
		{
			var cACompany = Factory.New<GlbCompany>();
			cACompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Canada;
			cACompany.GC_Code = "CA1";
			var branch1 = cACompany.Branches.AddNew();
			branch1.GB_Code = "GB1";
			OrgHeader orgheader = Factory.New<OrgHeader>();
			orgheader.OH_RL_NKClosestPort = "CALAX";
			orgheader.OH_Code = "OH1";

			var permit = GetCusPermitHeader();
			permit[CusPermitHeaderSchema.CPH_OH_PermitHolder] = orgheader.PK;
			var disHost = (IDISHost)permit;
			var requiredDocument = disHost.RequiredDocumentsProvider.RequiredDocuments.AddNew();
			requiredDocument.EQ_DocNumber = "DN21";
			requiredDocument.EQ_DocType = Core.Constants.RefDocTypes.CommercialInvoice;

			JobRequiredDocumentAddInfo addInfo = requiredDocument.AddInfos.AddNew();
			addInfo.EX_ReferenceNumber = "REF1";
			addInfo.EX_GC_Company = cACompany.PK;
			addInfo.EX_ApplicationCode = Core.Constants.Customs.DocumentImageSystemIDs.CA_DIF;
			var declaration = GetJobDeclaration();
			var requiredDocument1 = ((ICADIFHost)declaration).RequiredDocumentsProvider.RequiredDocuments.AddNew();
			JobRequiredDocumentAddInfo addInfo2 = requiredDocument1.AddInfos.AddNew();
			addInfo2.EX_ReferenceNumber = "REF2";
			addInfo2.EX_GC_Company = cACompany.PK;
			addInfo2.EX_ApplicationCode = Core.Constants.Customs.DocumentImageSystemIDs.CA_DIF;
			addInfo2.EX_AddInfo = @"<DIFDocument xmlns=""http://www.cargowise.com/Schemas/DIFDocument""><PGA>12</PGA><DocumentNumber>1498498498</DocumentNumber></DIFDocument>";
			Factory.Save();

			var holderList = new List<ZGuid>() { orgheader.PK };
			var query = JobRequiredDocumentAddInfoLoadHelper.GetJobRequiredDocumentAddInfoFilter(declaration.PK, holderList, cACompany.PK, Core.Constants.Customs.DocumentImageSystemIDs.CA_DIF, ZString.Empty);
			AssertEquals(2, Factory.Load<JobRequiredDocumentAddInfo>(query).Length);
			var query2 = JobRequiredDocumentAddInfoLoadHelper.GetJobRequiredDocumentAddInfoFilter(declaration.PK, holderList, cACompany.PK, Core.Constants.Customs.DocumentImageSystemIDs.CA_DIF, "12");
			AssertEquals(1, Factory.Load<JobRequiredDocumentAddInfo>(query2).Length);
			var query3 = JobRequiredDocumentAddInfoLoadHelper.GetJobRequiredDocumentAddInfoFilter(declaration.PK, holderList, cACompany.PK, Core.Constants.Customs.DocumentImageSystemIDs.CA_DIF, "1");
			AssertEquals(0, Factory.Load<JobRequiredDocumentAddInfo>(query3).Length);
		}

		public void TestGetJobRequiredDocumentAddInfoFilterForCusPermitHeader()
		{
			var cACompany = Factory.New<GlbCompany>();
			cACompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Canada;
			cACompany.GC_Code = "CA1";
			var uSCompany = Factory.New<GlbCompany>();
			uSCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
			uSCompany.GC_Code = "US1";

			var branch1 = cACompany.Branches.AddNew();
			branch1.GB_Code = "CA1";
			var branch2 = uSCompany.Branches.AddNew();
			branch2.GB_Code = "US1";
			OrgHeader orgheader = Factory.New<OrgHeader>();
			orgheader.OH_RL_NKClosestPort = "CALAX";
			orgheader.OH_Code = "OH1";

			var permit = GetCusPermitHeader();
			permit[CusPermitHeaderSchema.CPH_OH_PermitHolder] = orgheader.PK;
			var disHost = (IDISHost)permit;
			var requiredDocument = disHost.RequiredDocumentsProvider.RequiredDocuments.AddNew();
			requiredDocument.EQ_DocNumber = "DN21";
			requiredDocument.EQ_DocType = Core.Constants.RefDocTypes.CommercialInvoice;

			var addInfo = requiredDocument.AddInfos.AddNew();
			addInfo.EX_ReferenceNumber = "REF1";
			addInfo.EX_GC_Company = cACompany.PK;
			addInfo.EX_ApplicationCode = Core.Constants.Customs.DocumentImageSystemIDs.CA_DIF;
			addInfo.EX_AddInfo = @"<DIFDocument xmlns=""http://www.cargowise.com/Schemas/DIFDocument""><PGA>12</PGA><DocumentNumber>1498498498</DocumentNumber></DIFDocument>";

			var addInfo2 = requiredDocument.AddInfos.AddNew();
			addInfo2.EX_ReferenceNumber = "REF2";
			addInfo2.EX_GC_Company = uSCompany.PK;
			addInfo2.EX_ApplicationCode = Core.Constants.Customs.DocumentImageSystemIDs.US_DIS;
			addInfo2.EX_AddInfo = @"<DIFDocument xmlns=""http://www.cargowise.com/Schemas/DIFDocument""><PGA>2</PGA><DocumentNumber>1498498498</DocumentNumber></DIFDocument>";
			Factory.Save();

			var holderList = new List<ZGuid>() { orgheader.PK };
			var query1 = JobRequiredDocumentAddInfoLoadHelper.GetJobRequiredDocumentAddInfoFilterForCusPermitHeader(holderList, cACompany.PK, Core.Constants.Customs.DocumentImageSystemIDs.CA_DIF, "12");
			var query2 = JobRequiredDocumentAddInfoLoadHelper.GetJobRequiredDocumentAddInfoFilterForCusPermitHeader(holderList, cACompany.PK, Core.Constants.Customs.DocumentImageSystemIDs.US_DIS, ZString.Empty);
			var query3 = JobRequiredDocumentAddInfoLoadHelper.GetJobRequiredDocumentAddInfoFilterForCusPermitHeader(holderList, uSCompany.PK, Core.Constants.Customs.DocumentImageSystemIDs.US_DIS, "2");
			AssertEquals(addInfo, Factory.LoadTop1<JobRequiredDocumentAddInfo>(query1));
			AssertEquals(null, Factory.LoadTop1<JobRequiredDocumentAddInfo>(query2));
			AssertEquals(addInfo2, Factory.LoadTop1<JobRequiredDocumentAddInfo>(query3));
		}

		public void TestGetJobRequiredDocumentAddInfoFilterForJobDocsAndCartage()
		{
			var cACompany = Factory.New<GlbCompany>();
			cACompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Canada;
			cACompany.GC_Code = "CA1";
			var branch1 = cACompany.Branches.AddNew();
			branch1.GB_Code = "GB1";
			OrgHeader orgheader = Factory.New<OrgHeader>();
			orgheader.OH_RL_NKClosestPort = "CALAX";
			orgheader.OH_Code = "OH1";

			var uSCompany = Factory.New<GlbCompany>();
			uSCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
			uSCompany.GC_Code = "US1";
			var branch2 = uSCompany.Branches.AddNew();
			branch2.GB_Code = "US1";

			var declaration = GetJobDeclaration();
			var requiredDocument1 = ((IDISHost)declaration).RequiredDocumentsProvider.RequiredDocuments.AddNew();
			var addInfo1 = requiredDocument1.AddInfos.AddNew();
			addInfo1.EX_ReferenceNumber = "REF3";
			addInfo1.EX_GC_Company = cACompany.PK;
			addInfo1.EX_ApplicationCode = Core.Constants.Customs.DocumentImageSystemIDs.CA_DIF;
			addInfo1.EX_AddInfo = @"<DIFDocument xmlns=""http://www.cargowise.com/Schemas/DIFDocument""><PGA>12</PGA><DocumentNumber>1498498498</DocumentNumber></DIFDocument>";

			var addInfo2 = requiredDocument1.AddInfos.AddNew();
			addInfo2.EX_ReferenceNumber = "REF3";
			addInfo2.EX_GC_Company = uSCompany.PK;
			addInfo2.EX_ApplicationCode = Core.Constants.Customs.DocumentImageSystemIDs.US_DIS;
			addInfo2.EX_AddInfo = @"<DIFDocument xmlns=""http://www.cargowise.com/Schemas/DIFDocument""><PGA>12</PGA><DocumentNumber>1498498498</DocumentNumber></DIFDocument>";
			Factory.Save();

			var query1 = JobRequiredDocumentAddInfoLoadHelper.GetJobRequiredDocumentAddInfoFilterForJobDocsAndCartage(declaration.PK, cACompany.PK, Core.Constants.Customs.DocumentImageSystemIDs.CA_DIF, "12");
			var query2 = JobRequiredDocumentAddInfoLoadHelper.GetJobRequiredDocumentAddInfoFilterForJobDocsAndCartage(declaration.PK, uSCompany.PK, Core.Constants.Customs.DocumentImageSystemIDs.CA_DIF, ZString.Empty);
			var query3 = JobRequiredDocumentAddInfoLoadHelper.GetJobRequiredDocumentAddInfoFilterForJobDocsAndCartage(declaration.PK, uSCompany.PK, Core.Constants.Customs.DocumentImageSystemIDs.US_DIS, "12");
			AssertEquals(addInfo1, Factory.LoadTop1<JobRequiredDocumentAddInfo>(query1));
			AssertEquals(null, Factory.LoadTop1<JobRequiredDocumentAddInfo>(query2));
			AssertEquals(addInfo2, Factory.LoadTop1<JobRequiredDocumentAddInfo>(query3));

			var query4 = JobRequiredDocumentAddInfoLoadHelper.GetJobRequiredDocumentAddInfoFilterForJobDocsAndCartage(declaration.PK, cACompany.PK, Core.Constants.Customs.DocumentImageSystemIDs.CA_DIF, "12", "REF3");
			AssertEquals(addInfo1, Factory.LoadTop1<JobRequiredDocumentAddInfo>(query4));
			var query5 = JobRequiredDocumentAddInfoLoadHelper.GetJobRequiredDocumentAddInfoFilterForJobDocsAndCartage(declaration.PK, cACompany.PK, Core.Constants.Customs.DocumentImageSystemIDs.CA_DIF, "12", "REF2");
			AssertEquals(null, Factory.LoadTop1<JobRequiredDocumentAddInfo>(query5));
			var query6 = JobRequiredDocumentAddInfoLoadHelper.GetJobRequiredDocumentAddInfoFilterForJobDocsAndCartage(declaration.PK, cACompany.PK, Core.Constants.Customs.DocumentImageSystemIDs.CA_DIF, "", "REF3");
			AssertEquals(addInfo1, Factory.LoadTop1<JobRequiredDocumentAddInfo>(query6));
		}
		public BusinessObject GetCusPermitHeader()
		{
			var result = (BusinessObject)Factory.New<Integration.Customs.CA.ICusPermitHeader>();
			result[CusPermitHeaderSchema.CPH_Number.Name] = "Per123";
			result[CusPermitHeaderSchema.CPH_RN_NKCountryCode] = "CA";
			result[CusPermitHeaderSchema.CPH_StartDate] = ZDate.BrettsBirthday;
			result[CusPermitHeaderSchema.CPH_EndDate] = new ZDate(2017, 1, 1);
			result[CusPermitHeaderSchema.CPH_Type] = "PMT";
			result[CusPermitHeaderSchema.CPH_QtyValIndicator] = "BTH";
			return result;
		}

		public BusinessObject GetJobDeclaration()
		{
			var importer = Factory.NewWithValidTestData<OrgHeader>();
			importer.OH_FullName = "IMPORTER";
			var cusCode = importer.CustomsCodes.AddNew();
			cusCode.OK_CodeType = OrgCusCode.CACodeTypes.BusinessNumberForImportExport;
			cusCode.OK_CustomsRegNo = "BRM001";

			var result = (BusinessObject)Factory.New<Integration.Customs.CA.IJobDeclaration>();
			result[JobDeclarationSchema.JE_MessageType.Name] = "IMP";
			result[JobDeclarationSchema.JE_DeclarationReference] = "B00001000";
			result[JobDeclarationSchema.JE_OH_Importer] = importer.PK;

			return result;
		}
	}
}
