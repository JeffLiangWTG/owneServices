using System;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business.Testing
{
	sealed class CommonJobComInvoiceHeaderTest : TestCaseWithFactory
	{
		public void TestGetTopBusinessObject()
		{
			var header = Factory.NewWithValidTestData<BaseJobComInvoiceHeader>();
			var controllerFactory = System.Reflection.Assembly.Load("Enterprise.ZArchitecture.GUI").GetType("Enterprise.ZArchitecture.Modules.ZControllerFactory").GetField("Instance").GetValue(null);
			var controller = controllerFactory.GetType().GetMethod("GetControllerForBizo").Invoke(controllerFactory, new[] { header.GetTopBusinessObject() });
			AssertNotNull("We got the controller, so we got the Form", controller);
			AssertEquals("Controller type", "Enterprise.Customs.Module.JobDeclarationController", controller.GetType().FullName);
		}

		#region AdditionalDeclarationTest

		public void TestAdditionalDeclarations()
		{
			var declaration1 = Factory.New<JobDeclarationSupportAdditionalInvoices>();
			var declaration2 = Factory.New<JobDeclarationSupportAdditionalInvoices>();
			var invoice = Factory.New<JobComInvoiceHeaderSupportAdditionalDeclarations>();
			var groupHeader = Factory.New<JobComInvoiceGroupHeaderSupportAdditionalDeclarations>();

			invoice.AttachToAdditionalDeclaration(declaration1);
			invoice.AttachToAdditionalDeclaration(declaration2);
			groupHeader.AttachToAdditionalDeclaration(declaration1);
			groupHeader.AttachToAdditionalDeclaration(declaration2);

			AssertEquals("invoice.JZ_JE should not be changed", ZGuid.Empty, invoice.JZ_JE);
			AssertEquals("groupHeader.JZ_JE should not be changed", ZGuid.Empty, groupHeader.JZ_JE);

			Assert("invoice should be attached to declaration1", invoice.AdditionalDeclarations.Contains(declaration1));
			Assert("invoice should be attached to declaration2", invoice.AdditionalDeclarations.Contains(declaration2));
			Assert("invoice should be attached to declaration1", declaration1.Invoices.Contains(invoice));
			Assert("invoice should be attached to declaration2", declaration2.Invoices.Contains(invoice));
			Assert("groupHeader should be attached to declaration1", groupHeader.AdditionalDeclarations.Contains(declaration1));
			Assert("groupHeader should be attached to declaration2", groupHeader.AdditionalDeclarations.Contains(declaration2));
			Assert("groupHeader should be attached to declaration1", declaration1.JobComInvoiceGroupHeaders.Contains(groupHeader));
			Assert("groupHeader should be attached to declaration2", declaration2.JobComInvoiceGroupHeaders.Contains(groupHeader));

			ZQuery query1 = new ZQuery();
			query1.AddToFilter(GenPivotSchema.XX_Relation1ID, new ZGuid[] { invoice.PK, groupHeader.PK });
			query1.AddToFilter(GenPivotSchema.XX_Relation1TableCode, JobComInvoiceHeaderSchema.Constants.Prefix);
			query1.AddToFilter(GenPivotSchema.XX_Relation2TableCode, JobDeclarationSchema.Constants.Prefix);
			query1.AddToFilter(GenPivotSchema.XX_RelationType, GenPivotTypeDecider.Types.InvoiceRelatedDeclarationGenPivot);
			AssertEquals("2 InvoiceRelatedDeclarationGenPivots should be created", 2, Factory.Load<GenPivot>(query1).Length);

			ZQuery query2 = new ZQuery();
			query2.AddToFilter(GenPivotSchema.XX_Relation1ID, new ZGuid[] { invoice.PK, groupHeader.PK });
			query2.AddToFilter(GenPivotSchema.XX_Relation1TableCode, JobComInvoiceHeaderSchema.Constants.Prefix);
			query2.AddToFilter(GenPivotSchema.XX_Relation2TableCode, JobDeclarationSchema.Constants.Prefix);
			query2.AddToFilter(GenPivotSchema.XX_RelationType, GenPivotTypeDecider.Types.GroupRelatedDeclarationGenPivot);
			AssertEquals("2 GroupRelatedDeclarationGenPivots should be created", 2, Factory.Load<GenPivot>(query2).Length);

			invoice.DetachFromAdditionalDeclaration(declaration1);
			Assert("declaration1 should just be removed, not be deleted", !declaration1.IsDeleted);
			Assert("invoice should be detached from declaration1", !invoice.AdditionalDeclarations.Contains(declaration1));
			Assert("invoice should not be detached from declaration2", invoice.AdditionalDeclarations.Contains(declaration2));
			Assert("invoice should be detached from declaration1", !declaration1.Invoices.Contains(invoice));
			Assert("invoice should not be detached from declaration2", declaration2.Invoices.Contains(invoice));

			groupHeader.DetachFromAdditionalDeclaration(declaration1);
			Assert("declaration1 should just be removed, not be deleted", !declaration1.IsDeleted);
			Assert("groupHeader should be detached from declaration1", !groupHeader.AdditionalDeclarations.Contains(declaration1));
			Assert("groupHeader should not be detached from declaration2", groupHeader.AdditionalDeclarations.Contains(declaration2));
			Assert("groupHeader should be detached from declaration1", !declaration1.JobComInvoiceGroupHeaders.Contains(groupHeader));
			Assert("groupHeader should not be detached from declaration2", declaration2.JobComInvoiceGroupHeaders.Contains(groupHeader));

			AssertEquals("invoice.HiddenOriginalParentGuid should not be changed", ZGuid.Empty, invoice.HiddenOriginalParentGuid);
			AssertEquals("groupHeader.HiddenOriginalParentGuid should not be changed", ZGuid.Empty, groupHeader.HiddenOriginalParentGuid);

			invoice.Delete();
			Assert("declaration2 should just be removed, not be deleted", !declaration2.IsDeleted);
			groupHeader.Delete();
			Assert("declaration2 should just be removed, not be deleted", !declaration2.IsDeleted);

			AssertEquals("GenPivots should be deleted", 0, Factory.Load<GenPivot>(query1).Length);
			AssertEquals("GenPivots should be deleted", 0, Factory.Load<GenPivot>(query2).Length);
		}

		public void TestFirstAdditionalDeclaration()
		{
			var declaration = Factory.New<JobDeclarationSupportAdditionalInvoices>();
			var invoice = Factory.New<JobComInvoiceHeaderSupportAdditionalDeclarations>();
			var groupHeader = Factory.New<JobComInvoiceGroupHeaderSupportAdditionalDeclarations>();
			groupHeader.JZ_JE = declaration.PK;

			invoice.AttachToAdditionalDeclaration(declaration);
			groupHeader.AttachToAdditionalDeclaration(declaration);
			AssertEquals(declaration, invoice.FirstAdditionalDeclaration);
			AssertEquals(declaration, groupHeader.FirstAdditionalDeclaration);
			Factory.Save();

			var factory = Factory.CreateNewFactory();
			var invoice1 = factory.Load<BaseJobComInvoiceHeader>(invoice.PK);
			Assert("Precondition: SupportAdditionalDeclarations should be false", !invoice1.SupportAdditionalDeclarations);
			AssertNull("FirstAdditionalDeclaration should be null", invoice1.FirstAdditionalDeclaration);

			var groupHeader1 = factory.Load<BaseJobComInvoiceGroupHeader>(groupHeader.PK);
			Assert("Precondition: SupportAdditionalDeclarations should be false", !groupHeader1.SupportAdditionalDeclarations);
			AssertNull("FirstAdditionalDeclaration should be null", groupHeader1.FirstAdditionalDeclaration);
		}

		#endregion
	}
}
