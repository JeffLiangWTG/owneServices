using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Business.Testing
{
	public abstract class PartAttribListTest<TInvoiceLine, TPart, TPartPivot> : TestCaseWithFactory
		where TInvoiceLine : BaseJobComInvoiceLine
		where TPart : OrgSupplierPart
		where TPartPivot : BaseCusClassPartPivot
	{
		protected PartAttribListTest() { }

		public void TestPartAttrib1List()
		{
			AssertPartAttribList(x => x.Lookups.PartAttrib1List, y => y.Attributes1.AddNew());
		}

		public void TestPartAttrib2List()
		{
			AssertPartAttribList(x => x.Lookups.PartAttrib2List, y => y.Attributes2.AddNew());
		}

		public void TestPartAttrib3List()
		{
			AssertPartAttribList(x => x.Lookups.PartAttrib3List, y => y.Attributes3.AddNew());
		}

		protected abstract string HTE { get; }
		protected abstract string HTI { get; }
		protected abstract string SHB { get; }

		void AssertPartAttribList(GetPartAttribListDelegate getPartAttribList, GetNewAttributeDelegate getNewAttribute)
		{
			var importer1 = Factory.New<OrgHeader>();
			importer1.OH_IsConsignee = true;
			var importer2 = Factory.New<OrgHeader>();
			importer2.OH_IsConsignee = true;

			var supplier1 = Factory.New<OrgHeader>();
			supplier1.OH_IsConsignor = true;
			var supplier2 = Factory.New<OrgHeader>();
			supplier2.OH_IsConsignor = true;
			var supplier3 = Factory.New<OrgHeader>();
			supplier3.OH_IsConsignor = true;

			var part = Factory.New<TPart>();
			part.OP_PartNum = "Z123Z456";
			var orgRelationImp1 = part.RelatedOrganisations.AddOrganisationIfNotExist(importer1.PK, OrgPartRelation.RelationshipTypes.Owner);
			var orgRelationImp2 = part.RelatedOrganisations.AddOrganisationIfNotExist(importer2.PK, OrgPartRelation.RelationshipTypes.Owner);
			var orgRelationSup1 = part.RelatedOrganisations.AddOrganisationIfNotExist(supplier1.PK, OrgPartRelation.RelationshipTypes.Supplier);
			var orgRelationSup2 = part.RelatedOrganisations.AddOrganisationIfNotExist(supplier2.PK, OrgPartRelation.RelationshipTypes.Supplier);
			var orgRelationSup3 = part.RelatedOrganisations.AddOrganisationIfNotExist(supplier3.PK, OrgPartRelation.RelationshipTypes.Supplier);
			var useSHB = !string.IsNullOrEmpty(SHB);
			var pivot9 = AddPivot(part, orgRelationSup2.OU_OH, HTE, "17", "18", getNewAttribute);
			var pivot8 = AddPivot(part, orgRelationSup1.OU_OH, HTE, "15", "16", getNewAttribute);
			var pivot7 = AddPivot(part, orgRelationImp2.OU_OH, HTE, "13", "14", getNewAttribute);
			var pivot6 = AddPivot(part, orgRelationImp1.OU_OH, HTE, "11", "12", getNewAttribute);
			var pivot5 = AddPivot(part, ZGuid.Empty, HTI, "9", "10", getNewAttribute);
			var pivot4 = AddPivot(part, orgRelationSup2.OU_OH, HTI, "7", "8", getNewAttribute);
			var pivot3 = AddPivot(part, orgRelationSup1.OU_OH, HTI, "5", "6", getNewAttribute);
			var pivot25 = AddPivot(part, ZGuid.Empty, HTE, "49", "50", getNewAttribute);
			var pivot24 = AddPivot(part, orgRelationSup2.OU_OH, HTE, "47", "48", getNewAttribute);
			var pivot23 = AddPivot(part, orgRelationSup1.OU_OH, HTE, "45", "46", getNewAttribute);
			var pivot22 = AddPivot(part, orgRelationImp2.OU_OH, HTE, "43", "44", getNewAttribute);
			var pivot21 = AddPivot(part, orgRelationImp1.OU_OH, HTE, "41", "42", getNewAttribute);
			var pivot20 = AddPivot(part, ZGuid.Empty, HTI, "39", "40", getNewAttribute);
			var pivot2 = AddPivot(part, orgRelationImp2.OU_OH, HTI, "3", "4", getNewAttribute);
			var pivot19 = AddPivot(part, orgRelationSup2.OU_OH, HTI, "37", "38", getNewAttribute);
			var pivot18 = AddPivot(part, orgRelationSup1.OU_OH, HTI, "35", "36", getNewAttribute);
			var pivot17 = AddPivot(part, orgRelationImp2.OU_OH, HTI, "33", "34", getNewAttribute);
			var pivot16 = AddPivot(part, orgRelationImp1.OU_OH, HTI, "31", "32", getNewAttribute);
			var pivot10 = AddPivot(part, ZGuid.Empty, HTE, "19", "20", getNewAttribute);
			var pivot1 = AddPivot(part, orgRelationImp1.OU_OH, HTI, "1", "2", getNewAttribute);
			if (useSHB)
			{
				var pivot30 = AddPivot(part, ZGuid.Empty, SHB, "59", "60", getNewAttribute);
				var pivot29 = AddPivot(part, orgRelationSup2.OU_OH, SHB, "57", "58", getNewAttribute);
				var pivot28 = AddPivot(part, orgRelationSup1.OU_OH, SHB, "55", "56", getNewAttribute);
				var pivot27 = AddPivot(part, orgRelationImp2.OU_OH, SHB, "53", "54", getNewAttribute);
				var pivot26 = AddPivot(part, orgRelationImp1.OU_OH, SHB, "51", "52", getNewAttribute);
				var pivot15 = AddPivot(part, ZGuid.Empty, SHB, "29", "30", getNewAttribute);
				var pivot14 = AddPivot(part, orgRelationSup2.OU_OH, SHB, "27", "28", getNewAttribute);
				var pivot13 = AddPivot(part, orgRelationSup1.OU_OH, SHB, "25", "26", getNewAttribute);
				var pivot12 = AddPivot(part, orgRelationImp2.OU_OH, SHB, "23", "24", getNewAttribute);
				var pivot11 = AddPivot(part, orgRelationImp1.OU_OH, SHB, "21", "22", getNewAttribute);
			}

			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_OH_Importer = importer1.PK;
			declaration.JE_OH_Supplier = supplier1.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = (TInvoiceLine)invoice.JobComInvoiceLines.AddNew();
			invoice.JZ_OH_Buyer = importer2.PK;
			invoice.JZ_OH_Supplier = supplier2.PK;
			invoiceLine.JI_PartNo = "Z123Z456";
			SetupData(invoiceLine);
			AssertPartAttribList(invoiceLine, new string[] { "3", "4", "33", "34" }, getPartAttribList);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			invoice.JZ_OH_Buyer = importer2.PK;
			invoice.JZ_OH_Supplier = supplier2.PK;
			SetHTI(invoiceLine);
			AssertPartAttribList(invoiceLine, new string[] { "13", "14", "43", "44" }, getPartAttribList);

			if (useSHB)
			{
				SetSHB(invoiceLine);
				AssertPartAttribList(invoiceLine, new string[] { "23", "24", "53", "54" }, getPartAttribList);

				invoice.JZ_OH_Buyer = ZGuid.Empty;
				AssertPartAttribList(invoiceLine, new string[] { "21", "22", "51", "52" }, getPartAttribList);
			}
			else
			{
				invoice.JZ_OH_Buyer = ZGuid.Empty;
			}

			SetHTI(invoiceLine);
			AssertPartAttribList(invoiceLine, new string[] { "11", "12", "41", "42" }, getPartAttribList);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			invoice.JZ_OH_Buyer = ZGuid.Empty;
			invoice.JZ_OH_Supplier = supplier2.PK;
			AssertPartAttribList(invoiceLine, new string[] { "1", "2", "31", "32" }, getPartAttribList);

			declaration.JE_OH_Importer = ZGuid.Empty;
			AssertPartAttribList(invoiceLine, new string[] { "7", "8", "37", "38" }, getPartAttribList);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			invoice.JZ_OH_Buyer = ZGuid.Empty;
			invoice.JZ_OH_Supplier = supplier2.PK;
			AssertPartAttribList(invoiceLine, new string[] { "17", "18", "47", "48" }, getPartAttribList);

			if (useSHB)
			{
				SetSHB(invoiceLine);
				AssertPartAttribList(invoiceLine, new string[] { "27", "28", "57", "58" }, getPartAttribList);

				invoice.JZ_OH_Supplier = supplier1.PK;
				AssertPartAttribList(invoiceLine, new string[] { "25", "26", "55", "56" }, getPartAttribList);
			}
			else
			{
				invoice.JZ_OH_Supplier = supplier1.PK;
			}

			SetHTI(invoiceLine);
			AssertPartAttribList(invoiceLine, new string[] { "15", "16", "45", "46" }, getPartAttribList);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			invoice.JZ_OH_Supplier = supplier1.PK;
			AssertPartAttribList(invoiceLine, new string[] { "5", "6", "35", "36" }, getPartAttribList);

			invoice.JZ_OH_Supplier = supplier3.PK;
			AssertPartAttribList(invoiceLine, new string[] { "9", "10", "39", "40" }, getPartAttribList);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			invoice.JZ_OH_Supplier = supplier3.PK;
			AssertPartAttribList(invoiceLine, new string[] { "19", "20", "49", "50" }, getPartAttribList);

			if (useSHB)
			{
				SetSHB(invoiceLine);
				AssertPartAttribList(invoiceLine, new string[] { "29", "30", "59", "60" }, getPartAttribList);
			}
		}

		protected abstract void SetSHB(TInvoiceLine invoiceLine);
		protected abstract void SetHTI(TInvoiceLine invoiceLine);

		protected virtual void SetupData(TInvoiceLine invoiceLine)
		{
		}

		void AssertPartAttribList(TInvoiceLine invoiceLine, string[] expectedList, GetPartAttribListDelegate getPartAttribList)
		{
			var sortedList = new List<string>(expectedList);
			sortedList.Sort();
			var list = getPartAttribList(invoiceLine);
			AssertEquals(sortedList.Count, list.Count);
			for (int i = 0; i < sortedList.Count; i++)
			{
				AssertEquals(sortedList[i], list[i].Description);
			}
		}

		delegate CodeDescriptionPairList GetPartAttribListDelegate(TInvoiceLine invoiceLine);
		delegate CusAttributeFilter GetNewAttributeDelegate(TPartPivot pivot);

		TPartPivot AddPivot(TPart part, ZGuid orgRelationPK, ZString childType, ZString value1, ZString value2, GetNewAttributeDelegate getNewAttribute)
		{
			var result = (TPartPivot)part.PivotsForBinding.AddNew();
			result.CI_OH = orgRelationPK;
			result.CI_ChildType = childType;
			var attrib1 = getNewAttribute(result);
			attrib1.BG_AttributeValue1 = value1;
			var attrib2 = getNewAttribute(result);
			attrib2.BG_AttributeValue1 = value2;
			return result;
		}
	}
}
