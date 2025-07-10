using System;
using CargoWise.IO;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Moq.Protected;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Customs.DataTransfer.Testing
{
	[TestedType(typeof(DeclarationValueObjectDataAdapter))]
	sealed class DeclarationValueObjectDataAdapterBaseOnlyTest : DeclarationValueObjectDataAdapterAbstractTest
	{
		public void TestNew()
		{
			Assert(DeclarationValueObjectDataAdapter.New("AU", EventsWithSourceType.Empty) is Integration.Customs.AU.IAUDeclarationValueObjectDataAdapter);
			Assert(DeclarationValueObjectDataAdapter.New("NZ", EventsWithSourceType.Empty) is Integration.Customs.NZ.INZDeclarationValueObjectDataAdapter);
			Assert(DeclarationValueObjectDataAdapter.New("US", EventsWithSourceType.Empty) is Integration.Customs.US.IUSDeclarationValueObjectDataAdapter);
			Assert(DeclarationValueObjectDataAdapter.New("ZA", EventsWithSourceType.Empty) is Integration.Customs.ZA.IZADeclarationValueObjectDataAdapter);

			GlbCompany newNZCompany = Factory.NewWithValidTestData<GlbCompany>();
			newNZCompany.GC_RN_NKCountryCode = "NZ";
			newNZCompany.Branches.AddNew().FillWithValidTestData();
			Factory.Save();

			using (DisposableEnvironment.ForBranch(newNZCompany.FirstActiveBranch.PK.ToGuid()))
			{
				Assert(DeclarationValueObjectDataAdapter.New() is Integration.Customs.NZ.INZDeclarationValueObjectDataAdapter);
				Assert(DeclarationValueObjectDataAdapter.New("AU", EventsWithSourceType.Empty) is Integration.Customs.AU.IAUDeclarationValueObjectDataAdapter);
			}
		}

		public void TestExportingAndImportingOrderReferences()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var orderItems = declaration.DocsAndCartage.OrderItems;
			var order1 = orderItems.AddNew();
			order1.JT_OrderReference = "ORDER1";
			var order2 = orderItems.AddNew();
			order2.JT_OrderReference = "ORDER2";
			var order3 = orderItems.AddNew();
			order3.JT_OrderReference = "ORDER3";

			var adapter = new TestDeclarationValueObjectDataAdapter();
			var xmlDec = adapter.ExportToValueObject(declaration, new ValueObjectExportContext(new NotificationBuffer()));
			AssertEquals(true, xmlDec.Shipment.IsSpecified);
			AssertEquals(true, xmlDec.Shipment.ShipmentDetails.IsSpecified);
			AssertEquals(3, xmlDec.Shipment.ShipmentDetails.OrderReferences.Length);
			AssertCollectionContains("ORDER1", xmlDec.Shipment.ShipmentDetails.OrderReferences);
			AssertCollectionContains("ORDER2", xmlDec.Shipment.ShipmentDetails.OrderReferences);
			AssertCollectionContains("ORDER3", xmlDec.Shipment.ShipmentDetails.OrderReferences);

			xmlDec.Shipment.ShipmentDetails.OrderReferences = new[] { "ORDER4", "ORDER3", "ORDER5" };

			adapter.ImportFromValueObject(declaration, xmlDec, new ValueObjectImportContext(Factory, new NotificationBuffer()));
			AssertEquals(5, orderItems.Count);
			AssertEquals(order1, orderItems["ORDER1"]);
			AssertEquals(order2, orderItems["ORDER2"]);
			AssertEquals(order3, orderItems["ORDER3"]);
			AssertNotNull(orderItems["ORDER4"]);
			AssertNotNull(orderItems["ORDER5"]);
		}

		public void TestImportingPaymentMethodFuctionality()
		{
			var xmlDec = new Xsd.ConsolAndShipment();
			xmlDec.Shipment.Declaration.IsSpecified = true;
			xmlDec.Shipment.Declaration.PaymentTerms = "B!D";
			xmlDec.Shipment.Declaration.PaymentTermsSpecified = true;
			var context = new ValueObjectImportContext(Factory, new NotificationBuffer());

			var mock = Factory.NewMoq<BaseJobDeclaration>();
			BaseJobDeclaration declaration = mock.Object;
			mock.Protected().SetupSequence<bool>("SupportJE_PaymentMethodUsageCore").Returns(false).Returns(true);
			var adapter = new TestDeclarationValueObjectDataAdapter();
			adapter.ImportFromValueObject(declaration, xmlDec, context);
			AssertNotEquals("B!D", declaration.JE_PaymentMethod);
			adapter.ImportFromValueObject(declaration, xmlDec, context);
			AssertEquals("B!D", declaration.JE_PaymentMethod);
			mock.VerifyAll();
		}

		public void TestImportingUnmatchedOrganisation()
		{
			var org = new UnmatchedOrganisation(Factory);
			org.IsEnabled = true;
			OrganisationsDataRegistry.Instance.UseUnmatchedOrganisationForMatching.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, org);

			var xmlDec = new Xsd.ConsolAndShipment();

			var xmlConsignor = new Xsd.Organisation();
			xmlConsignor.OwnerCode = "100463";
			xmlConsignor.OrganisationDetails.Name = "LIBERTY MILLS LIMITED";

			xmlDec.Shipment.ShipmentDetails.Consignor = xmlConsignor;

			var context = new ValueObjectImportContext(Factory, new NotificationBuffer());

			var declaration = Factory.New<BaseJobDeclaration>();

			var adapter = new TestDeclarationValueObjectDataAdapter();
			adapter.ImportFromValueObject(declaration, xmlDec, context);

			StmNote[] unmatchedOrganisationNotes = declaration.Notes.FindByDescription(PredefinedNoteTypes.Instance.UnmatchedOrgDetails.Description);
			AssertEquals("Unmatched organisation note is created", 1, unmatchedOrganisationNotes.Length);
			AssertMultilineASCIIEquals("Note has been added to the Declaration", @"Organisation Type: Consignor
Owner Code: 100463
EDI Code: 
Organisation Name: LIBERTY MILLS LIMITED
Address Line 1: 
Address Line 2: 
City: 
Post Code: 
State or Province: 
Country: 
Doc Address Type:", unmatchedOrganisationNotes[0].ST_NoteText.TrimEnd());
		}

		public void TestExportingPaymentMethodFuctionality()
		{
			var mock = Factory.NewMoq<BaseJobDeclaration>();
			var declaration = mock.Object;
			declaration.JE_PaymentMethod = "B!D";
			Factory.Save();

			mock.Protected().SetupSequence<bool>("SupportJE_PaymentMethodUsageCore").Returns(false).Returns(true);
			var adapter = new TestDeclarationValueObjectDataAdapter();
			var xmlDec = adapter.ExportToValueObject(declaration, new ValueObjectExportContext(new NotificationBuffer()));
			AssertNotEquals("B!D", xmlDec.Shipment.Declaration.PaymentTerms);
			xmlDec = adapter.ExportToValueObject(declaration, new ValueObjectExportContext(new NotificationBuffer()));
			AssertEquals("B!D", xmlDec.Shipment.Declaration.PaymentTerms);
			mock.VerifyAll();
		}

		protected override string TestingCountry => null;

		protected override void MergeAndSetupCustomsCharges(BaseJobDeclaration declaration)
		{
			base.MergeAndSetupCustomsCharges(declaration);
			var entryHeader = declaration.CustomsEntryHeaders[0];
			entryHeader.Charges.AddNew().C1_ChargeAmount = 123.45m;
			entryHeader.MergedLines[0].Fees.AddNew().CF_ChargeAmount = 12.35m;
			entryHeader.MergedLines[1].Fees.AddNew().CF_ChargeAmount = 67.89m;
		}

		protected override BusinessObjectAndExpectedOutputFileName GetEmptyBizObjSample()
		{
			return new BusinessObjectAndExpectedOutputFileName(GetEmptyJobDeclaration(), FileReader.ExtractEmbeddedResourceToFile(BaseEmbeddedResourcePath, TempDir.DirectoryName, EmptyDeclarationFileName), ValidationKind.None, "Empty Declaration");
		}

		protected override BusinessObjectAndExpectedOutputFileName GetFullyPopulatedBizObjSample(BaseJobDeclaration declaration)
		{
			return new BusinessObjectAndExpectedOutputFileName(declaration, FileReader.ExtractEmbeddedResourceToFile(BaseEmbeddedResourcePath, TempDir.DirectoryName, "PopulatedDeclaration.xml"), ValidationKind.Xsd | ValidationKind.FactorySave, "Fully Populated Declaration");
		}

		protected override void TearDown()
		{
			base.TearDown();

			tempDir?.Dispose();
			tempDir = null;
		}

		TempDirectory TempDir => tempDir ??= new ();
		TempDirectory tempDir;

		TestFileReader FileReader => fileReader ??= new (typeof(DeclarationValueObjectDataAdapterBaseOnlyTest));
		TestFileReader fileReader;

		const string BaseEmbeddedResourcePath = "Enterprise.Customs.DataTransfer.Testing.Testing";
	}
}
