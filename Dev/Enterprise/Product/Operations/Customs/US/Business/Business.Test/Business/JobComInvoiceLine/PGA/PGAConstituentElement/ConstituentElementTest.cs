using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.Integration.Testing;
using NUnit.Framework;
using static Enterprise.Integration.Customs;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(ConstituentElement))]
	public class ConstituentElementTest : Customs.Business.MultiLineAddInfos.Testing.CusAddInfoTest<ConstituentElement>
	{
		public void TestCloneInNewFactory()
		{
			var originalBO = Factory.New<ConstituentElement>();
			originalBO.ScientificDataCollection.AddNew();

			var newBO = (ConstituentElement)originalBO.Clone();

			AssertEquals(1, newBO.ScientificDataCollection.Count);

			var fac = new BusinessObjectFactory();
			var newFacClone = (ConstituentElement)originalBO.Clone(new BusinessObjectCloneArgs(fac, System.Array.Empty<string>(), typeof(ConstituentElement), false));
			AssertEquals("Same Factory", fac.GetHashCode(), newFacClone.Factory.GetHashCode());
			AssertEquals("Same Factory", fac.GetHashCode(), newFacClone.ScientificDataCollection[0].Factory.GetHashCode());
			AssertNotEquals("Different Factory", originalBO.Factory.GetHashCode(), newFacClone.ScientificDataCollection[0].Factory.GetHashCode());
		}

		public void TestIConstituentElement()
		{
			var iConstituentElement = (IConstituentElement)ConstituentElement;
			ConstituentElement.US_PGANameOfTheConstituentElement = "name";
			AssertEquals(ConstituentElement.US_PGANameOfTheConstituentElement, iConstituentElement.Name);

			ConstituentElement.US_PGAPercentOfConstituentElement = 5.6m;
			AssertEquals(ConstituentElement.US_PGAPercentOfConstituentElement, iConstituentElement.Percent);

			ConstituentElement.US_PGAQuantityOfConstituentElement = 156.8m;
			AssertEquals(ConstituentElement.US_PGAQuantityOfConstituentElement, iConstituentElement.Quantity);

			ConstituentElement.US_PGAUnitOfMeasure = "KG";
			AssertEquals(ConstituentElement.US_PGAUnitOfMeasure, iConstituentElement.UnitOfMeasure);
		}

		public void TestICusAddInfoTypeSupporter()
		{
			ICusAddInfoTypeSupporter supporter = ConstituentElement;
			supporter.AssertType(typeof(ScientificData), CusAddInfoTypeAttribute.Codes.USSCI);
			supporter.AssertType(null, "ZZ!");

			var scientificData = ConstituentElement.ScientificDataCollection.AddNew();
			scientificData.US_PGACountryCode = "1";
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var addInfo = newFactory.Load<CusAddInfo>(scientificData.PK);
			AssertEquals(typeof(ScientificData), addInfo.GetType());
		}

		public void TestClone()
		{
			ConstituentElement.US_PGAPercentOfConstituentElement = 21.5m;
			ConstituentElement.ScientificDataCollection.AddNew();
			ConstituentElement.ScientificDataCollection.AddNew();

			var newObj = (ConstituentElement)ConstituentElement.Clone();
			AssertEquals(ConstituentElement.US_PGAPercentOfConstituentElement, newObj.US_PGAPercentOfConstituentElement);
			AssertEquals(2, newObj.ScientificDataCollection.Count);
		}

		public void TestDelete()
		{
			var scientificData = ConstituentElement.ScientificDataCollection.AddNew();

			ConstituentElement.Delete();

			Assert(scientificData.IsDeleted);
		}

		public void TestRelatedObjects()
		{
			var parent = ConstituentElement.Parent;
			AssertEquals("Parent PGA", typeof(PGA), parent.GetType());
			AssertEquals("Parent for PGA - Invoice Line", ((PGA)parent).InvoiceLine, ConstituentElement.InvoiceLine);

			var lookup = Factory.New<CusClassification>();
			lookup.CC_LookupCode = "LOOK434";
			var product = Factory.New<OrgSupplierPart>();
			product.OP_PartNum = "PART434";

			var pivot = Factory.New<CusClassPartPivot>();
			pivot.CI_CC = lookup.PK;
			pivot.CI_OP = product.PK;

			var pga = pivot.PGAs.AddNew();
			var constituentElementForProduct = pga.PG04ConstituentElements.AddNew();

			AssertEquals("Parent PGA", pga, constituentElementForProduct.Parent);
			AssertEquals("Parent for PGA - Invoice Line should be null, if pga data entered for product", null, constituentElementForProduct.InvoiceLine);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			var fda = invoiceLine.ACE_FDALines.AddNew();
			var fdaConstElement = fda.ProductConstituentElements.AddNew();
			AssertEquals("Parent ACE FDA", fda, fdaConstElement.Parent);
			AssertEquals("Invoice Line", invoiceLine, fdaConstElement.InvoiceLine);
		}

		public void TestUnknownBreakTotalRelatedProperties()
		{
			ConstituentElement.LaceyAct.US_UnknownBreakdownTotal = true;
			AssertEquals(true, ConstituentElement.US_PGAUnitOfMeasureInfo.ReadOnly);
			AssertEquals(true, ConstituentElement.US_PGANameOfTheConstituentElementInfo.ReadOnly);
			AssertEquals(true, ConstituentElement.US_PGAQuantityOfConstituentElementInfo.ReadOnly);
			AssertEquals(true, ConstituentElement.US_UnknownBreakdownCountryCodeInfo.ReadOnly);

			ConstituentElement.LaceyAct.US_UnknownBreakdownTotal = false;
			AssertEquals(false, ConstituentElement.US_PGAUnitOfMeasureInfo.ReadOnly);
			AssertEquals(false, ConstituentElement.US_PGANameOfTheConstituentElementInfo.ReadOnly);
			AssertEquals(false, ConstituentElement.US_PGAQuantityOfConstituentElementInfo.ReadOnly);
			AssertEquals(false, ConstituentElement.US_UnknownBreakdownCountryCodeInfo.ReadOnly);
		}

		#region Overrides

		protected override BusinessObject GetNewBusinessObject()
		{
			return ConstituentElement;
		}

		protected override IEnumerable<ConstituentElement> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			yield return (ConstituentElement)GetNewBusinessObjectForDeleteTest(factory);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var declaration = factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			var pga = invoiceLine.LaceyActLines.AddNew();
			return pga.PG04ConstituentElements.AddNew();
		}

		#endregion

		ConstituentElement ConstituentElement
		{
			get
			{
				if (constituentElement == null)
				{
					declaration = Factory.New<JobDeclaration>();
					declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
					var invoiceHeader = declaration.Invoices.AddNew();
					var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
					var pga = invoiceLine.LaceyActLines.AddNew();
					constituentElement = pga.PG04ConstituentElements.AddNew();
				}
				return constituentElement;
			}
		}
		ConstituentElement constituentElement;
		JobDeclaration declaration;
	}
}
