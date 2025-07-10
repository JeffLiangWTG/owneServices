using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class JobRequiredDocAttribLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestAttributeValueList()
		{
			var reqDoc = Factory.New<JobRequiredDocument>();
			var attrib = reqDoc.Attributes.AddNew();
			attrib.D0_AttribName = JobRequiredDocAttribTypeList.Codes.Direction;
			var list = attrib.Lookups.AttributeValueList;
			CombineAssertions("Attribute Name: DIRECTION", () =>
			{
				AssertEquals(4, list.Count);
				AssertEquals(ImportExportCodeList.Descriptions.Export, list.GetDescriptionFromCode(ImportExportCodeList.Codes.Export));
				AssertEquals(ImportExportCodeList.Descriptions.Import, list.GetDescriptionFromCode(ImportExportCodeList.Codes.Import));
				AssertEquals(ImportExportCodeList.Descriptions.ImportISF, list.GetDescriptionFromCode(ImportExportCodeList.Codes.ImportISF));
				AssertEquals(ImportExportCodeList.Descriptions.ImportOnly, list.GetDescriptionFromCode(ImportExportCodeList.Codes.ImportOnly));
			});

			attrib.D0_AttribName = JobRequiredDocAttribTypeList.Codes.PortOfEntry;
			list = attrib.Lookups.AttributeValueList;
			AssertEquals(0, list.Count);

			attrib.D0_AttribName = JobRequiredDocAttribTypeList.Codes.CostaRicaEXVDocumentType;
			list = attrib.Lookups.AttributeValueList;
			CombineAssertions("Attribute Name: COSTA RICA EXV DOCUMENT TYPE", () =>
			{
				AssertEquals(6, list.Count);
				AssertEquals(CostaRicaEXVDocumentTypeList.Descriptions.ComprasAutorizadas, list.GetDescriptionFromCode(CostaRicaEXVDocumentTypeList.Codes.ComprasAutorizadas));
				AssertEquals(CostaRicaEXVDocumentTypeList.Descriptions.VentasExentasDiplomticos, list.GetDescriptionFromCode(CostaRicaEXVDocumentTypeList.Codes.VentasExentasDiplomticos));
				AssertEquals(CostaRicaEXVDocumentTypeList.Descriptions.OrdenCompraPblicas, list.GetDescriptionFromCode(CostaRicaEXVDocumentTypeList.Codes.OrdenCompraPblicas));
				AssertEquals(CostaRicaEXVDocumentTypeList.Descriptions.ExencionesDireccinGeneralHacienda, list.GetDescriptionFromCode(CostaRicaEXVDocumentTypeList.Codes.ExencionesDireccinGeneralHacienda));
				AssertEquals(CostaRicaEXVDocumentTypeList.Descriptions.ZonasFrancas, list.GetDescriptionFromCode(CostaRicaEXVDocumentTypeList.Codes.ZonasFrancas));
				AssertEquals(CostaRicaEXVDocumentTypeList.Descriptions.OtrosExcenciones, list.GetDescriptionFromCode(CostaRicaEXVDocumentTypeList.Codes.OtrosExcenciones));
			});

			attrib.D0_AttribName = JobRequiredDocAttribTypeList.Codes.CustomsDistrict;
			list = attrib.Lookups.AttributeValueList;
			CombineAssertions("Attribute Name: CUSTOMS DISTRICT", () =>
			{
				AssertEquals(4, list.Count);
				AssertEquals((NoResString)"A", TaiwanCustomsDistrictList.Codes.A);
				AssertEquals((NoResString)"B", TaiwanCustomsDistrictList.Codes.B);
				AssertEquals((NoResString)"C", TaiwanCustomsDistrictList.Codes.C);
				AssertEquals((NoResString)"D", TaiwanCustomsDistrictList.Codes.D);
			});

			var helper = ObjectFactory.Get<Enterprise.Integration.Customs.Shared.Universal.IUniversalReferenceTestDataHelper>("Universal.IUniversalReferenceTestDataHelper", Factory);
			for (var i = 1; i <= 24; i++)
			{
				var code = i.ToString().PadLeft(2, '0');
				helper.CreatePreferenceForCountry(code, code, CountryCodes.Canada);
			}

			reqDoc.EQ_RN_NKRelatedCountry = Core.Constants.CountryCodes.Canada;
			attrib.D0_AttribName = JobRequiredDocAttribTypeList.Codes.TradePreferenceCode;
			list = attrib.Lookups.AttributeValueList;
			CombineAssertions("Attribute Name: TRADE PREFERENCE CODE, Contry: CA", () =>
			{
				AssertEquals(24, list.Count);
				AssertEquals(typeof(CodeDescriptionPairList), list.GetType());
			});

			reqDoc.EQ_RN_NKRelatedCountry = Core.Constants.CountryCodes.UnitedStates;
			list = attrib.Lookups.AttributeValueList;
			AssertEquals("Attribute Name: TRADE PREFERENCE CODE, Contry: US", 0, list.Count);
		}

		public void TestAttributeValueList_BondID()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var address = orgHeader.MainAddress;
			Factory.Save();
			var requiredDoc = Factory.New<JobRequiredDocument>();
			requiredDoc.ParentType = orgHeader.GetType();
			requiredDoc.EQ_ParentTableCode = OrgHeaderSchema.Constants.Prefix;
			requiredDoc.EQ_ParentID = orgHeader.PK;
			requiredDoc.EQ_DocUsage = JobRequiredDocument.DocUsage.Broker;
			requiredDoc.EQ_DocType = RefDocTypes.PowerOfAttorney;
			requiredDoc.EQ_RN_NKRelatedCountry = Core.Constants.CountryCodes.Taiwan;
			Factory.Save();

			var attrib = requiredDoc.Attributes.AddNew();
			attrib.D0_AttribName = JobRequiredDocAttribTypeList.Codes.BondedID;
			var list = attrib.Lookups.AttributeValueList;
			AssertEquals(0, list.Count);

			var country = RefCountry.LoadFromCountryCode(Factory, Core.Constants.CountryCodes.Taiwan);
			orgHeader.CustomsCodes.AddNew(OrgCusCode.CodeTypes.ControlledPremisesID, "11111", country);
			orgHeader.CustomsCodes.AddNew(OrgCusCode.CodeTypes.WarehouseControlledPremisesID, "11112", country);
			orgHeader.CustomsCodes.AddNew(OrgCusCode.TaiwanCodeTypes.SciencePark, "11113", country);
			orgHeader.CustomsCodes.AddNew(OrgCusCode.CodeTypes.VATCode, "11114", country);
			address.CustomsCodes.AddNew(OrgCusCode.TaiwanCodeTypes.CBF, "11115", country);
			address.CustomsCodes.AddNew(OrgCusCode.TaiwanCodeTypes.EPZ, "11116", country);
			address.CustomsCodes.AddNew(OrgCusCode.TaiwanCodeTypes.AgriculturalTechnologyPark, "11117", country);
			address.CustomsCodes.AddNew(OrgCusCode.TaiwanCodeTypes.FTZ, "11118", country);
			list = attrib.Lookups.AttributeValueList;
			CombineAssertions(() =>
			{
				AssertEquals(7, list.Count);
				Assert(list.ContainsCode("11111"));
				AssertEquals(OrgCusCode.CodeTypes.ControlledPremisesID, list.GetDescriptionFromCode("11111"));
				Assert(list.ContainsCode("11112"));
				AssertEquals(OrgCusCode.CodeTypes.WarehouseControlledPremisesID, list.GetDescriptionFromCode("11112"));
				Assert(list.ContainsCode("11113"));
				AssertEquals(OrgCusCode.TaiwanCodeTypes.SciencePark, list.GetDescriptionFromCode("11113"));
				Assert(list.ContainsCode("11115"));
				AssertEquals(OrgCusCode.TaiwanCodeTypes.CBF, list.GetDescriptionFromCode("11115"));
				Assert(list.ContainsCode("11116"));
				AssertEquals(OrgCusCode.TaiwanCodeTypes.EPZ, list.GetDescriptionFromCode("11116"));
				Assert(list.ContainsCode("11117"));
				AssertEquals(OrgCusCode.TaiwanCodeTypes.AgriculturalTechnologyPark, list.GetDescriptionFromCode("11117"));
				Assert(list.ContainsCode("11118"));
				AssertEquals(OrgCusCode.TaiwanCodeTypes.FTZ, list.GetDescriptionFromCode("11118"));
			});

			country = RefCountry.LoadFromCountryCode(Factory, Core.Constants.CountryCodes.Australia);
			orgHeader.CustomsCodes.RemoveAndDeleteAll();
			address.CustomsCodes.DeleteAll();
			orgHeader.CustomsCodes.AddNew(OrgCusCode.CodeTypes.ControlledPremisesID, "11111", country);
			orgHeader.CustomsCodes.AddNew(OrgCusCode.CodeTypes.WarehouseControlledPremisesID, "11112", country);
			orgHeader.CustomsCodes.AddNew(OrgCusCode.TaiwanCodeTypes.SciencePark, "11113", country);
			orgHeader.CustomsCodes.AddNew(OrgCusCode.CodeTypes.VATCode, "11114", country);
			address.CustomsCodes.AddNew(OrgCusCode.TaiwanCodeTypes.CBF, "11115", country);
			address.CustomsCodes.AddNew(OrgCusCode.TaiwanCodeTypes.EPZ, "11116", country);
			address.CustomsCodes.AddNew(OrgCusCode.TaiwanCodeTypes.AgriculturalTechnologyPark, "11117", country);
			address.CustomsCodes.AddNew(OrgCusCode.TaiwanCodeTypes.FTZ, "11118", country);
			list = attrib.Lookups.AttributeValueList;
			AssertEquals(0, list.Count);
		}

		public void TestAttributeNameList()
		{
			JobRequiredDocAttrib attrib1 = Factory.New<JobRequiredDocAttrib>();
			JobRequiredDocAttrib attrib2 = Factory.New<JobRequiredDocAttrib>();
			AssertEquals(Factory.GetCachedValue<JobRequiredDocAttribTypeList>(), attrib1.Lookups.AttributeNameList);
			AssertEquals(Factory.GetCachedValue<JobRequiredDocAttribTypeList>(), attrib2.Lookups.AttributeNameList);
		}

		public void TestBoxNumberList()
		{
			var attrib1 = Factory.New<JobRequiredDocAttribForTestBoxNumber>();
			attrib1.D0_AttribName = JobRequiredDocAttribTypeList.Codes.BoxNumber;
			AssertEquals(0, attrib1.Lookups.AttributeValueList.Count);

			var attrib2 = Factory.New<JobRequiredDocAttribForTestBoxNumber>();
			attrib2.D0_AttribName = JobRequiredDocAttribTypeList.Codes.CustomsDistrict;
			attrib2.D0_AttribDisplayValue = "A";

			var requiredDocument = Factory.New<JobRequiredDocumentForTestBoxNumber>();
			attrib1.D0_EQ = requiredDocument.PK;
			AssertEquals(0, attrib1.Lookups.AttributeValueList.Count);
			attrib2.D0_EQ = requiredDocument.PK;
			var attributeNameList = attrib1.Lookups.AttributeValueList;
			AssertEquals(2, attributeNameList.Count);
			AssertEquals(@"111 - 
222 -", attributeNameList.ElementsAsString);

			attrib2.D0_AttribDisplayValue = "B";
			attributeNameList = attrib1.Lookups.AttributeValueList;
			AssertEquals(1, attributeNameList.Count);
			AssertEquals("333 -", attributeNameList.ElementsAsString);

			attrib2.D0_AttribDisplayValue = "C";
			attributeNameList = attrib1.Lookups.AttributeValueList;
			AssertEquals(0, attributeNameList.Count);
		}
	}
}
