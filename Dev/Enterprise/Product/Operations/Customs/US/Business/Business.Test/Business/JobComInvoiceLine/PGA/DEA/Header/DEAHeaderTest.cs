using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Business.MessageBuilders;
using Enterprise.ZArchitecture.Business.UniversalCopy;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(DEAHeader))]
	internal class DEAHeaderTest : Customs.Business.MultiLineAddInfos.Testing.CusAddInfoTest<DEAHeader>
	{
		public void TestCloneInNewFactory()
		{
			var originalBO = Factory.New<DEAHeader>();
			originalBO.Constituents.AddNew();

			var newBO = (DEAHeader)originalBO.Clone();

			AssertEquals(1, newBO.Constituents.Count);

			var fac = new BusinessObjectFactory();
			var newFacClone = (DEAHeader)originalBO.Clone(new BusinessObjectCloneArgs(fac, Array.Empty<string>(), typeof(DEAHeader), false));
			AssertEquals("Same Factory", fac.GetHashCode(), newFacClone.Factory.GetHashCode());
			AssertEquals("Same Factory", fac.GetHashCode(), newFacClone.Constituents[0].Factory.GetHashCode());
			AssertNotEquals("Different Factory", originalBO.Factory.GetHashCode(), newFacClone.Constituents[0].Factory.GetHashCode());
		}

		public void TestArrivalDate()
		{
			AssertEquals(ZDate.Empty, ((IDEAHeader)Header).ArrivalDate);
			Declaration.US_FDAADTA = new ZDateTime(2016, 8, 3);
			AssertEquals(new ZDateTime(2016, 8, 3), ((IDEAHeader)Header).ArrivalDate);
		}

		public void TestClone()
		{
			var dea = Factory.New<DEAHeader>();
			dea.US_PermitNumber = "123456";
			dea.US_CountryOfShipment = "US";
			dea.US_FormID = "DEA-35";
			dea.US_RegistrationNumber = "123456789";
			var constituent = dea.Constituents.AddNew();
			constituent.US_ProductCode = "4000";
			constituent.US_Weight = 100m;
			constituent.US_WeightUQ = "KG";

			var clonedDEA = (DEAHeader)dea.Clone();
			AssertEquals("US_CountryOfShipment", "US", clonedDEA.US_CountryOfShipment);
			AssertEquals("US_FormID", "DEA-35", clonedDEA.US_FormID);
			AssertEquals("US_RegistrationNumber", "123456789", clonedDEA.US_RegistrationNumber);
			AssertEquals("US_PermitNumber should NOT be cloned", ZString.Empty, clonedDEA.US_PermitNumber);

			AssertEquals(1, clonedDEA.Constituents.Count);

			var clonedConstituent = clonedDEA.Constituents[0];
			AssertEquals("US_ProductCode", "4000", clonedConstituent.US_ProductCode);
			AssertEquals("US_WeightUQ", "KG", clonedConstituent.US_WeightUQ);
			AssertEquals("US_Weight should NOT be cloned", 0m, clonedConstituent.US_Weight);
		}

		public void TestIAESDEAMembers()
		{
			Header.US_DrugCode = "DRUG";
			Header.US_Weight = 100m;
			Header.US_UnitOfMeasure = "G";
			Header.US_PermitNumber = "1111";
			Header.US_RegistrationNumber = "22222";
			Declaration.US_InbondType = InbondTypeList.Codes.IEWarehouseWithdrawal;

			var aesDEA = (IAESDEA)Header;
			AssertEquals("DRUG", aesDEA.DrugCode);
			AssertEquals(100m, aesDEA.Quantity);
			AssertEquals("G", aesDEA.UnitOfMeasure);
			AssertEquals("1111", aesDEA.PermitNumber);
			AssertEquals("22222", aesDEA.RegistrationNumber);
			AssertEquals("T", aesDEA.TransactionType);

			Declaration.US_InbondType = InbondTypeList.Codes.TAndEWarehouseWithdrawal;
			AssertEquals("T", aesDEA.TransactionType);

			Declaration.US_InbondType = InbondTypeList.Codes.IEForeignTradeZoneWithdrawal;
			AssertEquals("T", aesDEA.TransactionType);

			Declaration.US_InbondType = InbondTypeList.Codes.TAndEForeignTradeZoneWithdrawal;
			AssertEquals("T", aesDEA.TransactionType);

			Declaration.US_InbondType = InbondTypeList.Codes.MerchandiseNOTShippedInbond;
			AssertEquals("E", aesDEA.TransactionType);

			Declaration.US_InbondType = ZString.Empty;
			AssertEquals("E", aesDEA.TransactionType);
		}

		public void TestIsExport()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			var invoideLineDEA = invoiceLine.DEAHeaders.AddNew();
			AssertEquals(false, invoideLineDEA.IsExport);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertEquals(true, invoideLineDEA.IsExport);

			var pivot = Factory.New<CusClassPartPivot>();
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			var productDEA = pivot.DEAHeaders.AddNew();
			AssertEquals(false, productDEA.IsExport);
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTE;
			AssertEquals(true, productDEA.IsExport);
		}

		public void TestUniversalCopy()
		{
			var ignoreElementAttributes = (UniversalCopyIgnoreElementAttribute[])(typeof(DEAHeader).GetCustomAttributes(typeof(UniversalCopyIgnoreElementAttribute), false));
			AssertEquals(1, ignoreElementAttributes.Length);
			AssertCollectionContains(DEAHeader.Schema.US_DrugCode, ignoreElementAttributes[0].ElementNames);
			AssertCollectionContains(DEAHeader.Schema.US_Weight, ignoreElementAttributes[0].ElementNames);
			AssertCollectionContains(DEAHeader.Schema.US_UnitOfMeasure, ignoreElementAttributes[0].ElementNames);

			var constituentsPropertyInfo = typeof(DEAHeader).GetProperty("Constituents");
			Assert(Attribute.IsDefined(constituentsPropertyInfo, typeof(UniversalCopyCollectionEntityAttribute)));
		}

		public void TestUS_PermitNumberMaxLength()
		{
			var pivot = Factory.New<CusClassPartPivot>();
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			var productDEA = pivot.DEAHeaders.AddNew();
			AssertEquals(DEAHeaderAddInfo.Schema.US_PermitNumberMaxLength, productDEA.US_PermitNumberInfo.MaxLength);
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTE;
			AssertEquals(7, productDEA.US_PermitNumberInfo.MaxLength);
		}

		public void TestUS_RegistrationNumberMaxLength()
		{
			var pivot = Factory.New<CusClassPartPivot>();
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			var productDEA = pivot.DEAHeaders.AddNew();
			AssertEquals(DEAHeaderAddInfo.Schema.US_RegistrationNumberMaxLength, productDEA.US_RegistrationNumberInfo.MaxLength);
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTE;
			AssertEquals(9, productDEA.US_RegistrationNumberInfo.MaxLength);
		}

		JobDeclaration Declaration
		{
			get
			{
				if (fDeclaration == null)
				{
					fDeclaration = Factory.New<JobDeclaration>();
				}
				fDeclaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
				fDeclaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
				return fDeclaration;
			}
		}
		JobDeclaration fDeclaration;

		DEAHeader Header
		{
			get
			{
				if (header == null)
				{
					var invoice = Declaration.Invoices.AddNew();
					var invoiceLine = invoice.JobComInvoiceLines.AddNew();
					header = invoiceLine.DEAHeaders.AddNew();
				}
				return header;
			}
		}
		DEAHeader header;

		protected override IEnumerable<DEAHeader> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			yield return (DEAHeader)GetNewBusinessObjectForDeleteTest(factory);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var declaration = factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			return invoiceLine.DEAHeaders.AddNew();
		}
	}
}
