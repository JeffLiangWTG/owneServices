using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(InvoiceHeaderActiveCollection))]
	class MultipleDeclarationsInvoiceHeaderCollectionTest : ActiveBusinessObjectCollectionTestCase<InvoiceHeaderActiveCollection>
	{
		public void TestInvoiceHeaderActiveCollction()
		{
			var declaration1 = Factory.New<BaseJobDeclaration>();
			var declaration2 = Factory.New<BaseJobDeclaration>();
			var invoice1 = Factory.New<BaseJobComInvoiceHeader>();
			var invoice2 = Factory.New<BaseJobComInvoiceHeader>();
			var invoice3 = Factory.New<BaseJobComInvoiceHeader>();
			var invoice4 = Factory.New<BaseJobComInvoiceHeader>();
			invoice4.JZ_JE = declaration1.PK;
			var pivot11 = Factory.New<InvoiceRelatedDeclarationGenPivot>();
			pivot11.XX_Relation2ID = declaration1.PK;
			pivot11.XX_Relation1ID = invoice1.PK;
			var pivot12 = Factory.New<InvoiceRelatedDeclarationGenPivot>();
			pivot12.XX_Relation2ID = declaration1.PK;
			pivot12.XX_Relation1ID = invoice2.PK;
			var pivot22 = Factory.New<InvoiceRelatedDeclarationGenPivot>();
			pivot22.XX_Relation2ID = declaration2.PK;
			pivot22.XX_Relation1ID = invoice2.PK;
			var pivot23 = Factory.New<InvoiceRelatedDeclarationGenPivot>();
			pivot23.XX_Relation2ID = declaration2.PK;
			pivot23.XX_Relation1ID = invoice3.PK;
			var pivot0 = Factory.New<GenPivot>();
			pivot0.XX_RelationType = GenPivotTypeDecider.Types.FDARelatedBillsGenPivot;
			pivot0.XX_Relation2ID = declaration1.PK;
			pivot0.XX_Relation1ID = invoice3.PK;
			pivot0.XX_Relation2TableCode = JobDeclarationSchema.Constants.Prefix;
			pivot0.XX_Relation1TableCode = JobComInvoiceHeaderSchema.Constants.Prefix;
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			declaration1 = newFactory.Load<JobDeclarationSupportAdditionalInvoices>(declaration1.PK);
			declaration2 = newFactory.Load<JobDeclarationSupportAdditionalInvoices>(declaration2.PK);
			var invoices1 = new InvoiceHeaderActiveCollection(declaration1, GenPivotTypeDecider.Types.InvoiceRelatedDeclarationGenPivot);
			AssertEquals("InvoiceHeaderActiveCollection should has 3 elements", 3, invoices1.Count);
			AssertNotNull("InvoiceHeaderActiveCollection should contains invoice1", invoices1.FindByPK(invoice1.PK));
			AssertNotNull("InvoiceHeaderActiveCollection should contains invoice2", invoices1.FindByPK(invoice2.PK));
			AssertNull("InvoiceHeaderActiveCollection should not contains invoice3", invoices1.FindByPK(invoice3.PK));
			AssertNotNull("InvoiceHeaderActiveCollection should contains invoice4", invoices1.FindByPK(invoice4.PK));

			var invoices2 = new InvoiceHeaderActiveCollection(declaration2, GenPivotTypeDecider.Types.InvoiceRelatedDeclarationGenPivot);
			AssertEquals("InvoiceHeaderActiveCollection should has 2 elements", 2, invoices2.Count);
			AssertNotNull("InvoiceHeaderActiveCollection should contains invoice2", invoices2.FindByPK(invoice2.PK));
			AssertNotNull("InvoiceHeaderActiveCollection should contains invoice3", invoices2.FindByPK(invoice3.PK));
		}

		public void TestAddAndRemove()
		{
			var declaration1 = Factory.New<BaseJobDeclaration>();
			var declaration2 = Factory.New<BaseJobDeclaration>();
			var invoice1 = Factory.New<BaseJobComInvoiceHeader>();
			invoice1.JZ_JE = declaration2.PK;
			var invoice2 = Factory.New<BaseJobComInvoiceHeader>();
			invoice2.JZ_JE = declaration1.PK;
			var pivot = Factory.New<InvoiceRelatedDeclarationGenPivot>();
			pivot.XX_Relation2ID = declaration1.PK;
			pivot.XX_Relation1ID = invoice1.PK;
			var invoice3 = Factory.New<BaseJobComInvoiceHeader>();
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			declaration1 = newFactory.Load<JobDeclarationSupportAdditionalInvoices>(declaration1.PK);
			var invoices = declaration1.Invoices;
			AssertNotNull("InvoiceHeaderActiveCollection should contains invoice", invoices.FindByPK(invoice1.PK));
			AssertNotNull("InvoiceHeaderActiveCollection should contains invoice", invoices.FindByPK(invoice2.PK));
			invoice1 = (BaseJobComInvoiceHeader)invoices.FindByPK(invoice1.PK);
			invoice2 = (BaseJobComInvoiceHeader)invoices.FindByPK(invoice2.PK);
			var line = invoice1.InvoiceLines.AddNew();
			Assert("new line should be added to declaration1.InvoiceLines", declaration1.InvoiceLines.Contains(line));
			invoices.RemoveFromRelationship(invoice1);
			Assert("invoice1 should not be deleted", !invoice1.IsDeleted);
			AssertEquals("invoice1's declaration should not be changed", declaration2.PK, invoice1.JZ_JE);
			AssertEquals(1, invoices.Count);
			Assert("invoice line should be removed from declaration1.InvoiceLines", !declaration1.InvoiceLines.Contains(line));
			line = invoice1.InvoiceLines.AddNew();
			Assert("new line should not be added from declaration1.InvoiceLines", !declaration1.InvoiceLines.Contains(line));
			invoices.RemoveFromRelationship(invoice2);
			Assert("invoice2 should not be deleted", !invoice2.IsDeleted);
			AssertEquals(0, invoices.Count);
			AssertEquals("invoice2 should be removed from the declaration", ZGuid.Empty, invoice2.JZ_JE);
			invoices.Add(invoice3);
			AssertEquals("invoice3 should belong to the declaration", declaration1.PK, invoice3.JZ_JE);
			var invoice4 = invoices.AddNew();
			AssertEquals("invoice4 should belong to the declaration", declaration1.PK, invoice4.JZ_JE);
			newFactory.Save();

			pivot = Factory.Load<InvoiceRelatedDeclarationGenPivot>(pivot.PK);
			AssertNull("InvoiceRelatedDeclarationGenPivot should be deleted", pivot);

			AssertNull("No InvoiceRelatedDeclarationGenPivot should be added", GenPivot.LoadRelation2Pivot(declaration1, GenPivotTypeDecider.Types.InvoiceRelatedDeclarationGenPivot));
		}

		public void TestSupporAdditionalInvoices()
		{
			var otherFactory = new BusinessObjectFactory();
			var declaration = otherFactory.New<BaseJobDeclaration>();
			var invoice1 = otherFactory.New<BaseJobComInvoiceHeader>();
			var invoice2 = otherFactory.New<BaseJobComInvoiceHeader>();
			var pivot1 = otherFactory.New<InvoiceRelatedDeclarationGenPivot>();
			pivot1.XX_Relation2ID = declaration.PK;
			pivot1.XX_Relation1ID = invoice1.PK;
			var pivot2 = otherFactory.New<InvoiceRelatedDeclarationGenPivot>();
			pivot2.XX_Relation2ID = declaration.PK;
			pivot2.XX_Relation1ID = invoice2.PK;
			var invoice3 = otherFactory.New<BaseJobComInvoiceHeader>();
			invoice3.JZ_JE = declaration.PK;
			otherFactory.Save();

			declaration = Factory.Load<JobDeclarationSupportAdditionalInvoices>(declaration.PK);
			AssertEquals(3, declaration.Invoices.Count);
			invoice1 = (BaseJobComInvoiceHeader)declaration.Invoices.FindByPK(invoice1.PK);
			invoice2 = (BaseJobComInvoiceHeader)declaration.Invoices.FindByPK(invoice2.PK);
			invoice3 = (BaseJobComInvoiceHeader)declaration.Invoices.FindByPK(invoice3.PK);
			AssertNotNull("invoice1 should be in Invoices", invoice1);
			AssertNotNull("invoice2 should be in Invoices", invoice2);
			AssertNotNull("invoice3 should be in Invoices", invoice3);
			pivot1 = Factory.Load<InvoiceRelatedDeclarationGenPivot>(pivot1.PK);
			pivot2 = Factory.Load<InvoiceRelatedDeclarationGenPivot>(pivot2.PK);

			declaration.Delete();
			Assert("invoice1 should not be deleted", !invoice1.IsDeleted);
			Assert("invoice2 should not be deleted", !invoice2.IsDeleted);
			Assert("invoice3 should be deleted", invoice3.IsDeleted);
			Assert("pivot1 should not be deleted", pivot1.IsDeleted);
			Assert("pivot2 should not be deleted", pivot2.IsDeleted);
		}

		public void TestContainsAdditionalInvoices()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			Assert("Does not contain additional invoices", !new InvoiceHeaderActiveCollection(declaration).SupportAdditionalInvoices);
			Assert("Contains additional invoices", new InvoiceHeaderActiveCollection(declaration, GenPivotTypeDecider.Types.InvoiceRelatedDeclarationGenPivot).SupportAdditionalInvoices);
		}

		public void TestIsAdditionalInvoice()
		{
			var otherFactory = new BusinessObjectFactory();
			var declaration = otherFactory.New<BaseJobDeclaration>();
			var invoice1 = otherFactory.New<BaseJobComInvoiceHeader>();
			var pivot1 = otherFactory.New<InvoiceRelatedDeclarationGenPivot>();
			pivot1.XX_Relation2ID = declaration.PK;
			pivot1.XX_Relation1ID = invoice1.PK;
			var invoice2 = otherFactory.New<BaseJobComInvoiceHeader>();
			invoice2.JZ_JE = declaration.PK;
			otherFactory.Save();

			declaration = Factory.Load<JobDeclarationSupportAdditionalInvoices>(declaration.PK);
			Assert("invoice1 is an additional invoice", declaration.Invoices.IsAdditionalInvoice(invoice1));
			Assert("invoice2 is not an additional invoice", !declaration.Invoices.IsAdditionalInvoice(invoice2));
		}

		public void TestSetOverrideDeclaration()
		{
			var otherFactory = new BusinessObjectFactory();
			var declaration1 = otherFactory.New<BaseJobDeclaration>();
			var declaration2 = otherFactory.New<BaseJobDeclaration>();
			var invoice1 = declaration2.Invoices.AddNew();
			var pivot1 = otherFactory.New<InvoiceRelatedDeclarationGenPivot>();
			pivot1.XX_Relation2ID = declaration1.PK;
			pivot1.XX_Relation1ID = invoice1.PK;
			var invoice2 = declaration2.Invoices.AddNew();
			var pivot2 = otherFactory.New<InvoiceRelatedDeclarationGenPivot>();
			pivot2.XX_Relation2ID = declaration1.PK;
			pivot2.XX_Relation1ID = invoice2.PK;
			otherFactory.Save();

			declaration1 = Factory.Load<JobDeclarationSupportAdditionalInvoices>(declaration1.PK);
			AssertEquals("JobDeclaration", declaration2.PK, declaration1.Invoices[0].JobDeclaration.PK);
			AssertEquals("JobDeclaration", declaration2.PK, declaration1.Invoices[1].JobDeclaration.PK);
			declaration1.Invoices.SetOverrideDeclaration(declaration1);
			AssertEquals("JobDeclaration", declaration1.PK, declaration1.Invoices[0].JobDeclaration.PK);
			AssertEquals("JobDeclaration", declaration1.PK, declaration1.Invoices[1].JobDeclaration.PK);
			declaration1.Invoices.SetOverrideDeclaration(null);
			AssertEquals("JobDeclaration", declaration2.PK, declaration1.Invoices[0].JobDeclaration.PK);
			AssertEquals("JobDeclaration", declaration2.PK, declaration1.Invoices[1].JobDeclaration.PK);
		}

		#region Implementation

		BaseJobDeclaration Declaration
		{
			get
			{
				if (fDeclaration == null)
				{
					fDeclaration = GetNewJobDeclaration();
				}
				return fDeclaration;
			}
		}
		BaseJobDeclaration fDeclaration;

		protected virtual BaseJobDeclaration GetNewJobDeclaration()
		{
			return Factory.New<JobDeclarationSupportAdditionalInvoices>();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			BaseJobComInvoiceHeader invoice = Factory.New<BaseJobComInvoiceHeader>();
			return invoice;
		}

		protected override InvoiceHeaderActiveCollection GetCollectionToTest()
		{
			return Declaration.Invoices;
		}
		#endregion
	}
}
