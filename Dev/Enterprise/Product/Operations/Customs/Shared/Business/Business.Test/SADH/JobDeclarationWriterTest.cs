using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Business.SADH.Testing
{
	public class JobDeclarationWriterTest : TestCaseWithFactory
	{
		public void TestWriteFromSavesAllFieldsBackToDeclaration()
		{
			SADHFormData formData = FormData;

			OrgHeader consignor = Factory.New<OrgHeader>();
			consignor.OH_Code = "BLAH";
			consignor.MainAddress.OA_Code = "BST";
			formData.D1_OH_Consignor = consignor.PK;

			OrgHeader consignee = Factory.New<OrgHeader>();
			consignee.OH_Code = "FOOBAR";
			consignee.MainAddress.OA_Code = "FST";
			formData.D1_OH_Consignee = consignee.PK;

			RefCurrency currency = Factory.NewWithValidTestData<RefCurrency>();
			GlbBranch branch = GlbCompany.CurrentCompany.Branches.AddNew();

			SetupFormDataFields(currency, branch);

			Writer.WriteFrom(FormData);

			BaseJobDeclaration declaration = Declaration;
			BaseJobComInvoiceHeader firstInvoice = declaration.Invoices[0];
			BaseJobComInvoiceLine firstLine = firstInvoice.JobComInvoiceLines.GetByLineNo(1);

			AssertWriteFromSavesAllFieldsBackToDeclaration(currency, branch, firstInvoice, firstLine);
		}

		public void TestInvalidValuesAreNotBeingWrittenBack()
		{
			BaseJobDeclaration declaration = Declaration;
			SADHFormData formData = FormData;

			formData.D1_FlightDate = new ZDateTime(2008, 1, 1);
			GlbBranch branch = GlbCompany.CurrentCompany.Branches.AddNew();
			OrgHeader consignee = Factory.New<OrgHeader>();
			consignee.OH_Code = "FOOBAR";
			formData.D1_OH_Consignee = consignee.PK;
			OrgHeader consignor = Factory.New<OrgHeader>();
			consignor.OH_Code = "BLAH";
			formData.D1_OH_Consignor = consignor.PK;
			RefCurrency currency = Factory.NewWithValidTestData<RefCurrency>();
			formData.D1_RX_InvoiceCurrency = currency.PK;

			Writer.WriteFrom(formData);
			BaseJobComInvoiceHeader firstInvoice = declaration.Invoices[0];

			AssertEquals("Precondition: declaration.JE_ExportDate", new ZDateTime(2008, 1, 1), declaration.JE_ExportDate);
			AssertEquals("Precondition: declaration.JE_OH_Supplier", "BLAH", declaration.Supplier.OH_Code);
			AssertEquals("Precondition: declaration.JE_OH_Importer", "FOOBAR", declaration.Importer.OH_Code);
			AssertEquals("Precondition: invoice.JZ_RX_NKInvoice_Currency", currency.RX_Code, firstInvoice.JZ_RX_NKInvoice_Currency);

			formData.D1_FlightDate = ZDateTime.Invalid;
			formData.D1_OH_Consignee = ZGuid.Invalid;
			formData.D1_OH_Consignor = ZGuid.Invalid;
			formData.D1_RX_InvoiceCurrency = ZGuid.Invalid;

			Writer.WriteFrom(formData);

			AssertEquals("declaration.JE_ExportDate", new ZDateTime(2008, 1, 1), declaration.JE_ExportDate);
			AssertEquals("declaration.JE_OH_Supplier", "BLAH", declaration.Supplier.OH_Code);
			AssertEquals("declaration.JE_OH_Importer", "FOOBAR", declaration.Importer.OH_Code);
			AssertEquals("invoice.JZ_RX_NKInvoice_Currency", currency.RX_Code, firstInvoice.JZ_RX_NKInvoice_Currency);
		}

		#region Implementation
		#region Declaration
		protected BaseJobDeclaration Declaration
		{
			get { return fDeclaration ?? (fDeclaration = GetNewJobDeclaration()); }
		}
		BaseJobDeclaration fDeclaration;

		protected virtual BaseJobDeclaration GetNewJobDeclaration()
		{
			return Factory.New<BaseJobDeclaration>();
		}
		#endregion

		#region Writer
		protected JobDeclarationWriter Writer
		{
			get { return fWriter ?? (fWriter = GetNewJobDeclarationWriter()); }
		}
		JobDeclarationWriter fWriter;

		protected virtual JobDeclarationWriter GetNewJobDeclarationWriter()
		{
			return new JobDeclarationWriter(Declaration);
		}
		#endregion

		#region FormData
		protected SADHFormData FormData
		{
			get { return fFormData ?? (fFormData = GetNewSADHFormData()); }
		}
		SADHFormData fFormData;

		protected virtual SADHFormData GetNewSADHFormData()
		{
			return new SADHFormData(Factory, Declaration);
		}
		#endregion

		protected virtual void SetupFormDataFields(RefCurrency currency, GlbBranch branch)
		{
			SADHFormData formData = FormData;
			formData.D1_GrossMassUQ = "OZ";
			formData.D1_GrossMass = 500m;

			formData.D1_TotalPackages = 27;
			formData.D1_TotalPackagesPackType = Constants.PkgUnit.Box;

			formData.D1_RL_NKCountryOfOrigin = "NZAKL";
			formData.D1_RL_NKCountryOfDestination = "GBLON";

			formData.D1_InvoiceTotalAmount = 399m;

			formData.D1_RX_InvoiceCurrency = currency.PK;

			formData.D1_DescriptionOfGoods = "Something longer than the 280 characters limit. Blah Blah Blah Blah Blah Blah Blah Blah Blah Blah Blah Blah Blah Blah Blah Blah Blah Blah Blah Blah Blah Blah Blah Blah Blah Blah Blah Blah Blah Blah Blah Blah Blah Blah Blah Blah Blah Blah Blah Blah Blah Blah Blah Blah Blah Blah Bl";
			formData.D1_MarksAndNumbers = "MARK II MACH V";
			formData.D1_ItemQty = 4231.3409m;
			formData.D1_ItemUQ = "KG";

			formData.D1_MessageType = "Z_Z";

			formData.D1_ModeOfTransportAtTheBorder = "AIR";

			formData.D1_RN_NKItemCountryOfOrigin = "AU";

			formData.D1_RL_NKCountryOfDispatchOrExport = "GBLON";
			formData.D1_RL_NKPlaceOfUnloading = "NZAKL";

			formData.D1_DeliveryTerms = Constants.IncoTerms.FreeOnBoard;

			formData.D1_ItemPrice = 12m;

			formData.D1_SupplementaryQty = 3.45m;
			formData.D1_SupplementaryUQ = "KG";

			formData.D1_FlightNo = "ABER3541";
			formData.D1_FlightDate = new ZDateTime(2008, 1, 1);
			formData.D1_VesselCode = "USS ENTERPRISE";
		}

		protected virtual void AssertWriteFromSavesAllFieldsBackToDeclaration(RefCurrency currency, GlbBranch branch, BaseJobComInvoiceHeader firstInvoice, BaseJobComInvoiceLine firstLine)
		{
			BaseJobDeclaration declaration = Declaration;
			AssertEquals("declaration.JE_OH_Supplier", "BLAH", declaration.Supplier.OH_Code);
			AssertEquals("declaration.JE_OH_Importer", "FOOBAR", declaration.Importer.OH_Code);
			AssertEquals("declaration.SupplierDocumentaryAddress", "BST", declaration.SupplierDocumentaryAddress.Address.OA_Code);
			AssertEquals("declaration.ImporterDocumentaryAddress", "FST", declaration.ImporterDocumentaryAddress.Address.OA_Code);
			AssertEquals("declaration.JE_RL_NKOrigin", "NZAKL", declaration.JE_RL_NKOrigin);
			AssertEquals("latestLine.JI_Description", "Something longer than the 280 characters limit. Blah Blah Blah Blah Blah Blah Blah Blah Blah Blah Blah Blah Blah Blah Blah Blah Blah Blah Blah Blah Blah Blah Blah Blah Blah Blah Blah Blah Blah Blah Blah Blah Blah Blah Blah Blah Blah Blah Blah Blah Blah Blah Blah Blah Blah Blah Bl", firstLine.JI_Description);
			AssertEquals("declaration.JE_MarksAndNumbers", "MARK II MACH V", declaration.JE_MarksAndNumbers);
			AssertEquals("declaration.JE_MessageType", "Z_Z", declaration.JE_MessageType);
			AssertEquals("declaration.JE_TransportMode", "AIR", declaration.JE_TransportMode);
			AssertEquals("latestLine.JI_CountryOfOrigin", "AU", firstLine.JI_CountryOfOrigin);
			AssertEquals("declaration.JE_RL_NKPortOfLoading", "GBLON", declaration.JE_RL_NKPortOfLoading);
			AssertEquals("declaration.JE_RL_NKPortOfArrival", "NZAKL", declaration.JE_RL_NKPortOfArrival);
			AssertEquals("latestInvoice.JZ_InvoiceAmount", 399m, firstInvoice.JZ_InvoiceAmount);
			AssertEquals("invoice.JZ_RX_NKInvoice_Currency", currency.RX_Code, firstInvoice.JZ_RX_NKInvoice_Currency);
			AssertEquals("declaration.JE_TotalNoOfPieces", 27, declaration.JE_TotalNoOfPacks);
			AssertEquals("declaration.JE_TotalNoOfPacksPackType", Constants.PkgUnit.Box, declaration.JE_TotalNoOfPacksPackType);
			AssertEquals("declaration.JE_RL_NKFinalDestination", "GBLON", declaration.JE_RL_NKFinalDestination);
			AssertEquals("declaration.JE_ShipmentIncoTerm", Constants.IncoTerms.FreeOnBoard, declaration.JE_ShipmentIncoTerm);
			AssertEquals("firstInvoice.JZ_IncoTerm", Constants.IncoTerms.FreeOnBoard, firstInvoice.JZ_IncoTerm);
			AssertEquals("firstLine.JI_LinePrice", 12m, firstLine.JI_LinePrice);
			AssertEquals("firstLine.JI_CustomsQuantity", 3.45m, firstLine.JI_CustomsQuantity);
			AssertEquals("firstLine.JI_CustomsUnitQty", "KG", firstLine.JI_CustomsUnitQty);
			AssertEquals("declaration.JE_VoyageFlightNo", "ABER3541", declaration.JE_VoyageFlightNo);
			AssertEquals("declaration.JE_ExportDate", new ZDateTime(2008, 1, 1), declaration.JE_ExportDate);
			AssertEquals("declaration.JE_VesselName", "USS ENTERPRISE", declaration.JE_VesselName);
			AssertEquals("firstLine.JI_InvoiceQuantity", 4231.3409m, firstLine.JI_InvoiceQuantity);
			AssertEquals("firstLine.JI_InvoiceUQ", "KG", firstLine.JI_InvoiceUQ);
			AssertEquals("firstLine.JI_WeightUQ", "OZ", firstLine.JI_WeightUQ);
			AssertEquals("firstLine.WeightExposedToUsers", 500m, firstLine.JI_Weight);
		}
		#endregion
	}
}
