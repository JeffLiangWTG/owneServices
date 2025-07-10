using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(JobRequiredDocumentDependentCollection))]
	sealed class JobRequiredDocumentDependentCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestSetCollectionRelationships()
		{
			JobRequiredDocument doc = Factory.New<JobRequiredDocument>();
			Assert("Precondition - soft foreign key not set", doc.EQ_ParentID.IsEmpty);
			Assert("Precondition - parent table code not set", doc.EQ_ParentTableCode.IsEmpty);
			Docs.Add(doc);
			AssertEquals("Soft foreign key of collection item has been set", Shipment.RequiredDocumentsProvider.PK, doc.EQ_ParentID);
			AssertEquals("Parent table code of collection item has been set", JobDocsAndCartageSchema.Constants.Prefix, doc.EQ_ParentTableCode);
		}

		public void TestGetDocByType()
		{
			AssertEquals("Precondition - shipment doesn't have any documents", 0, Docs.Count);
			AssertNull("No JobRequiredDocument exists in the collection so null should be returned", Docs.GetDocByType(Core.Constants.RefDocTypes.MasterBill));
			JobRequiredDocument powerOfAttorney = Docs.AddNew();
			powerOfAttorney.EQ_DocType = Core.Constants.RefDocTypes.PowerOfAttorney;

			JobRequiredDocument oceanBillOfLadingExpired = Docs.AddNew();
			oceanBillOfLadingExpired.EQ_DocType = Core.Constants.RefDocTypes.MasterBill;
			oceanBillOfLadingExpired.EQ_RN_NKRelatedCountry = Core.Constants.CountryCodes.Australia;
			oceanBillOfLadingExpired.EQ_ValidToDate = ZDateTime.Today.AddMonths(-1);

			JobRequiredDocument oceanBillOfLading1 = Docs.AddNew();
			oceanBillOfLading1.EQ_DocType = Core.Constants.RefDocTypes.MasterBill;
			oceanBillOfLading1.EQ_RN_NKRelatedCountry = Core.Constants.CountryCodes.Australia;
			oceanBillOfLading1.EQ_ValidToDate = ZDateTime.Today.AddYears(1);
			JobRequiredDocAttrib attrib1 = oceanBillOfLading1.Attributes.AddNew();
			attrib1.D0_AttribName = "BOB CHECK";
			attrib1.D0_AttribValue = "PASS";
			AssertEquals("The same JobRequiredDocument row should be returned", oceanBillOfLading1, Docs.GetDocByType(Core.Constants.RefDocTypes.MasterBill));

			JobRequiredDocument oceanBillOfLading2 = Docs.AddNew();
			oceanBillOfLading2.EQ_DocType = Core.Constants.RefDocTypes.MasterBill;
			oceanBillOfLading2.EQ_RN_NKRelatedCountry = Core.Constants.CountryCodes.UnitedStates;
			oceanBillOfLading2.EQ_ValidToDate = ZDateTime.Today.AddYears(1);
			AssertEquals("The Australian JobRequiredDocument row should be returned", oceanBillOfLading1, Docs.GetDocByType(Core.Constants.RefDocTypes.MasterBill, Core.Constants.CountryCodes.Australia));
			AssertEquals("The US JobRequiredDocument row should be returned", oceanBillOfLading2, Docs.GetDocByType(Core.Constants.RefDocTypes.MasterBill, Core.Constants.CountryCodes.UnitedStates));

			AssertEquals("The US JobRequiredDocument row should be returned", oceanBillOfLading2, Docs.GetDocByType(Core.Constants.RefDocTypes.MasterBill, Core.Constants.CountryCodes.UnitedStates, new Predicate<JobRequiredDocument>(x => x.Attributes.Count == 0)));
			JobRequiredDocAttrib attrib2 = oceanBillOfLading2.Attributes.AddNew();
			attrib2.D0_AttribName = "WENDY CHECK";
			attrib2.D0_AttribValue = "FAIL";

			AssertNull("No JobRequiredDocument exists that has no attributes", Docs.GetDocByType(Core.Constants.RefDocTypes.MasterBill, Core.Constants.CountryCodes.UnitedStates, new Predicate<JobRequiredDocument>(x => x.Attributes.Count == 0)));
			AssertNull("No JobRequiredDocument matching attribute", Docs.GetDocByType(Core.Constants.RefDocTypes.MasterBill, Core.Constants.CountryCodes.Australia, new Predicate<JobRequiredDocument>(x => x.Attributes["BOB CHECK", "FAIL"] != null)));
			AssertEquals("The Australian JobRequiredDocument row should be returned", oceanBillOfLading1, Docs.GetDocByType(Core.Constants.RefDocTypes.MasterBill, Core.Constants.CountryCodes.Australia, new Predicate<JobRequiredDocument>(x => x.Attributes["BOB CHECK", "PASS"] != null)));
			AssertEquals("The US JobRequiredDocument row should be returned", oceanBillOfLading2, Docs.GetDocByType(Core.Constants.RefDocTypes.MasterBill, Core.Constants.CountryCodes.UnitedStates, new Predicate<JobRequiredDocument>(x => x.Attributes["WENDY CHECK", "FAIL"] != null)));
		}

		public void TestIsDocRequired()
		{
			Docs.Load();
			AssertEquals("Precondition - shipment doesn't have any documents", 0, Docs.Count);
			Assert("Expect original bill not to be required", !Docs.IsDocRequired(Core.Constants.RefDocTypes.MasterBill));

			JobRequiredDocument oceanBillOfLading = Docs.AddNew();
			oceanBillOfLading.EQ_DocType = Core.Constants.RefDocTypes.MasterBill;
			Assert("Expect original bill to be required", Docs.IsDocRequired(Core.Constants.RefDocTypes.MasterBill));
		}

		public void TestIsDocReceived()
		{
			Docs.Load();
			AssertEquals("Precondition - shipment doesn't have any documents", 0, Docs.Count);
			Assert("Expect original bill not to be received", !Docs.IsDocReceived(Core.Constants.RefDocTypes.MasterBill));

			JobRequiredDocument oceanBillOfLading = Docs.AddNew();
			oceanBillOfLading.EQ_DocType = Core.Constants.RefDocTypes.MasterBill;
			AssertEquals("Precondition - original bill received date empty", ZDateTimeOffset.Empty, oceanBillOfLading.EQ_DateReceived);
			Assert("Expect original bill not to be received", !Docs.IsDocReceived(Core.Constants.RefDocTypes.MasterBill));

			oceanBillOfLading.EQ_DateReceived = ZDateTimeOffset.Now;
			Assert("Expect original bill to be received", Docs.IsDocReceived(Core.Constants.RefDocTypes.MasterBill));
		}

		public void TestToggleDocReceived()
		{
			Assert("Precondition - shipment does not have received original bill", !Docs.IsDocReceived(Core.Constants.RefDocTypes.MasterBill));
			Docs.ToggleDocReceived(Core.Constants.RefDocTypes.MasterBill, true);
			Assert("Shipment now has received original bill", Docs.IsDocReceived(Core.Constants.RefDocTypes.MasterBill));
			Docs.ToggleDocReceived(Core.Constants.RefDocTypes.MasterBill, false);
			Assert("Shipment now does not have received original bill", !Docs.IsDocReceived(Core.Constants.RefDocTypes.MasterBill));
		}

		public void TestAddIfNotExists()
		{
			Assert("Precondition - no original bill on shipment", !Docs.IsDocRequired(Core.Constants.RefDocTypes.MasterBill));
			JobRequiredDocument oceanBillOfLading1 = Docs.AddIfNotExists(Core.Constants.RefDocTypes.MasterBill, JobRequiredDocument.DocUsage.Both);
			Assert("Shipment now has original bill", Docs.IsDocRequired(Core.Constants.RefDocTypes.MasterBill));
			JobRequiredDocument oceanBillOfLading2 = Docs.AddIfNotExists(Core.Constants.RefDocTypes.MasterBill, JobRequiredDocument.DocUsage.Both);
			AssertEquals("The original JobRequiredDocument object should be returned", oceanBillOfLading1, oceanBillOfLading2);
		}

		public void TestRemoveIfExists()
		{
			Assert("Precondition - shipment has no original bill", !Docs.IsDocRequired(Core.Constants.RefDocTypes.MasterBill));
			Docs.RemoveIfExists(Core.Constants.RefDocTypes.MasterBill); //Expect that this doesnt cause an exception
			JobRequiredDocument oceanBillOfLading = Docs.AddNew();
			oceanBillOfLading.EQ_DocType = Core.Constants.RefDocTypes.MasterBill;
			Assert("Shipment requires original bill", Docs.IsDocRequired(Core.Constants.RefDocTypes.MasterBill));
			Docs.RemoveIfExists(Core.Constants.RefDocTypes.MasterBill);
			Assert("Shipment doesn't require original bill", !Docs.IsDocRequired(Core.Constants.RefDocTypes.MasterBill));
		}

		public void TestAddNewWithDocType()
		{
			JobRequiredDocument addedDoc = Docs.AddNew(Core.Constants.RefDocTypes.PackingList);
			AssertEquals(Core.Constants.RefDocTypes.PackingList, addedDoc.EQ_DocType);
		}

		public void TestMSCCountryRequiredDocumentHasDescription()
		{
			RefCountryRequiredDocument requiredDocument = Factory.New<RefCountryRequiredDocument>();
			requiredDocument.RD_DocType = Core.Constants.RefDocTypes.MiscellaneousDocument;
			requiredDocument.RD_DocUsage = "ALL";
			requiredDocument.RD_TransportMode = "ALL";
			requiredDocument.RD_OnShipment = true;
			Factory.Save();

			BusinessObject shipment = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Enterprise.Integration.Forwarding.IForwardingShipment)));
			IHaveRequiredDocuments parent = shipment["DocsAndCartage"] as IHaveRequiredDocuments;
			parent.RequiredDocuments.SetAllDocumentsReceivedEventLogger();
			AssertEquals(0, parent.RequiredDocuments.Count);
			Factory.Save();

			AssertEquals(1, parent.RequiredDocuments.Count);
			AssertEquals(Core.Constants.RefDocTypes.MiscellaneousDocument, parent.RequiredDocuments[0].EQ_DocType);
			AssertEquals(Core.Constants.RefDocTypeDescriptions.MiscellaneousDocument.ToString(), parent.RequiredDocuments[0].EQ_DocDescription);
		}

		public void TestCopyJobRequiredDocumentDependentCollectionDoesntThrowNRE()
		{
			var order = (IHaveRequiredDocuments)Factory.New<Enterprise.Integration.Forwarding.IOrder>();
			var doc = order.RequiredDocuments.AddNew();
			doc.EQ_DocType = Core.Constants.RefDocTypes.MasterBill;
			doc.EQ_RN_NKRelatedCountry = Core.Constants.CountryCodes.Australia;
			doc.EQ_ValidToDate = ZDateTime.Today.AddMonths(-1);
			AssertEquals(1, order.RequiredDocuments.Count);

			var shipment = (IDocsAndCartageParent)Factory.New<Enterprise.Integration.Forwarding.IForwardingShipment>();
			AssertEquals(0, shipment.RequiredDocumentsProvider.RequiredDocuments.Count);

			order.RequiredDocuments.CopyToOtherCollection(shipment.RequiredDocumentsProvider.RequiredDocuments);
			AssertEquals(1, shipment.RequiredDocumentsProvider.RequiredDocuments.Count);
		}

		public void TestCopyJobRequiredDocumentDependentCollectionDoesntCreateDuplicateRecords()
		{
			var order = (IHaveRequiredDocuments)Factory.New<Enterprise.Integration.Forwarding.IOrder>();
			var doc = order.RequiredDocuments.AddNew();
			doc.EQ_DocType = Core.Constants.RefDocTypes.MasterBill;
			doc.EQ_DocCategory = Constants.ReferenceTypes.SupplyChainLogistics;
			doc.EQ_DocUsage = JobRequiredDocument.DocUsage.Export;
			doc.EQ_RN_NKRelatedCountry = Core.Constants.CountryCodes.Australia;
			doc.EQ_ValidToDate = ZDateTime.Today.AddMonths(-1);
			AssertEquals(1, order.RequiredDocuments.Count);

			var shipment = (IDocsAndCartageParent)Factory.New<Enterprise.Integration.Forwarding.IForwardingShipment>();
			doc = shipment.RequiredDocumentsProvider.RequiredDocuments.AddNew();
			doc.EQ_DocType = Core.Constants.RefDocTypes.MasterBill;
			doc.EQ_DocCategory = Constants.ReferenceTypes.SupplyChainLogistics;
			doc.EQ_DocUsage = JobRequiredDocument.DocUsage.Export;
			doc.EQ_RN_NKRelatedCountry = Core.Constants.CountryCodes.Australia;
			doc.EQ_ValidToDate = ZDateTime.Today.AddMonths(-1);
			AssertEquals(1, shipment.RequiredDocumentsProvider.RequiredDocuments.Count);

			order.RequiredDocuments.CopyToOtherCollection(shipment.RequiredDocumentsProvider.RequiredDocuments);
			AssertEquals("JobRequiredDocument will not be copied because target collection already has a record with same type", 1, shipment.RequiredDocumentsProvider.RequiredDocuments.Count);
		}

		public void TesDeleteRsbLog()
		{
			var expectedDeleteRecordReference = "Deleted RSB Document Tracking record for reporting period '30-Jun-21 00:00:00'";

			var org = Factory.NewWithValidTestData<OrgHeader>();
			Docs = org.RequiredDocuments;
			Docs.Load();
			AssertEquals("Precondition - shipment doesn't have any documents", 0, Docs.Count);

			var recordRsb = CreateRsbRecord();
			var recordNonRsb = CreateRsbRecord();
			recordNonRsb.EQ_DocType = "TST";

			Factory.Save();

			AssertEquals("Pre-condition", 0, org.Logs.GetAllLogs().Cast<StmALog>().Count(x => x.SL_SE_NKEvent == Events.EditedARecord.Code && x.SL_Reference.Contains(expectedDeleteRecordReference)));

			Docs.RemoveAndDelete(recordNonRsb);

			AssertEquals("Should NOT create log when delete non-RSB record", 0, org.Logs.GetAllLogs().Cast<StmALog>().Count(x => x.SL_SE_NKEvent == Events.EditedARecord.Code && x.SL_Reference.Contains(expectedDeleteRecordReference)));

			Docs.RemoveAndDelete(recordRsb);

			AssertEquals("Create log when delete RSB record", 1, org.Logs.GetAllLogs().Cast<StmALog>().Count(x => x.SL_SE_NKEvent == Events.EditedARecord.Code && x.SL_Reference.Contains(expectedDeleteRecordReference)));

			JobRequiredDocument CreateRsbRecord()
			{
				JobRequiredDocument record = Docs.AddNew();
				record.EQ_DocCategory = Constants.ReferenceTypes.ComplianceReport;
				record.EQ_DocType = AccountingMasterFilesConstants.ComplianceReportCodes.ReportableSmallBusines;
				record.EQ_DocUsage = JobRequiredDocument.DocUsage.Creditor;
				record.EQ_DocPeriod = Constants.JobRequiredDocuments.DocumentPeriods.Periodic;
				record.EQ_ValidToDate = new ZDateTime(2021, 6, 30);
				record.EQ_DateReceived = record.EQ_ValidToDate.ToDateTimeOffset(null);
				record.EQ_RN_NKRelatedCountry = CountryCodes.Australia;
				record.EQ_ParentID = org.PK;
				record.EQ_ParentTableCode = OrgHeaderSchema.Constants.Prefix;
				record.ParentType = typeof(OrgHeader);
				return record;
			}
		}

		#region TestAddRequiredDocuments

		public void TestAddCountryRequiredDocument()
		{
			RefCountry countryAU = RefCountry.LoadFromCountryCode(Factory, Constants.CountryCodes.Australia);
			ZString aUS = Constants.CountryCodes.Australia;

			RefCountryRequiredDocument requiredDoc1 = GetNewRequiredDoc(countryAU, Constants.RefDocTypes.AgentsInvoice, aUS, "NZ", JobRequiredDocument.DocUsage.All, Constants.TransportModes.All, true, true, true, true);//pass
			RefCountryRequiredDocument requiredDoc2 = GetNewRequiredDoc(countryAU, Constants.RefDocTypes.BeneficiaryCertificate, "", "NZ", JobRequiredDocument.DocUsage.All, Constants.TransportModes.All, true, true, true, true);//pass
			RefCountryRequiredDocument requiredDoc3 = GetNewRequiredDoc(countryAU, Constants.RefDocTypes.CartageAdvice, aUS, "", JobRequiredDocument.DocUsage.All, Constants.TransportModes.All, true, true, true, true);//pass
			RefCountryRequiredDocument requiredDoc4 = GetNewRequiredDoc(countryAU, Constants.RefDocTypes.DelayAlert, "", "", JobRequiredDocument.DocUsage.All, Constants.TransportModes.All, true, true, true, true);//pass

			RefCountryRequiredDocument requiredDoc5 = GetNewRequiredDoc(countryAU, Constants.RefDocTypes.ExportCartageAdvice, aUS, aUS, JobRequiredDocument.DocUsage.All, Constants.TransportModes.All, true, true, true, true);//fail
			RefCountryRequiredDocument requiredDoc6 = GetNewRequiredDoc(countryAU, Constants.RefDocTypes.FoodControlCertificate, aUS, "US", JobRequiredDocument.DocUsage.All, Constants.TransportModes.All, true, true, true, true);//fail
			RefCountryRequiredDocument requiredDoc7 = GetNewRequiredDoc(countryAU, Constants.RefDocTypes.HouseBill, "US", "NZ", JobRequiredDocument.DocUsage.All, Constants.TransportModes.All, true, true, true, true);//fail

			RefCountryRequiredDocument requiredDoc8 = GetNewRequiredDoc(countryAU, Constants.RefDocTypes.ImportCartageAdvice, aUS, "NZ", JobRequiredDocument.DocUsage.Import, Constants.TransportModes.All, true, true, true, true);//different Doc Usages
			RefCountryRequiredDocument requiredDoc9 = GetNewRequiredDoc(countryAU, Constants.RefDocTypes.Label, aUS, "NZ", JobRequiredDocument.DocUsage.Export, Constants.TransportModes.All, true, true, true, true);//different Doc Usages
			RefCountryRequiredDocument requiredDoc10 = GetNewRequiredDoc(countryAU, Constants.RefDocTypes.Manifest, aUS, "NZ", JobRequiredDocument.DocUsage.Both, Constants.TransportModes.All, true, true, true, true);//different Doc Usages
			RefCountryRequiredDocument requiredDoc11 = GetNewRequiredDoc(countryAU, Constants.RefDocTypes.ShipmentNotes, aUS, "NZ", JobRequiredDocument.DocUsage.Domestic, Constants.TransportModes.All, true, true, true, true);//different Doc Usages

			RefCountryRequiredDocument requiredDoc12 = GetNewRequiredDoc(countryAU, Constants.RefDocTypes.MasterBill, aUS, "NZ", JobRequiredDocument.DocUsage.All, Constants.TransportModes.All, true, true, true, true);//diiferent trnasport
			RefCountryRequiredDocument requiredDoc13 = GetNewRequiredDoc(countryAU, Constants.RefDocTypes.PreAlert, aUS, "NZ", JobRequiredDocument.DocUsage.All, Constants.TransportModes.Air, true, true, true, true);//diiferent trnasport
			RefCountryRequiredDocument requiredDoc14 = GetNewRequiredDoc(countryAU, Constants.RefDocTypes.QuarantineCertificate, aUS, "NZ", JobRequiredDocument.DocUsage.All, Constants.TransportModes.Rail, true, true, true, true);//diiferent trnasport
			RefCountryRequiredDocument requiredDoc15 = GetNewRequiredDoc(countryAU, Constants.RefDocTypes.RequestDocument, aUS, "NZ", JobRequiredDocument.DocUsage.All, Constants.TransportModes.Sea, true, true, true, true);//diiferent trnasport

			RefCountryRequiredDocument requiredDoc16 = GetNewRequiredDoc(countryAU, Constants.RefDocTypes.SanitaryCertificate, aUS, "NZ", JobRequiredDocument.DocUsage.All, Constants.TransportModes.All, false, false, false, true);//only on one
			RefCountryRequiredDocument requiredDoc17 = GetNewRequiredDoc(countryAU, Constants.RefDocTypes.VetinaryCertificate, aUS, "NZ", JobRequiredDocument.DocUsage.All, Constants.TransportModes.All, false, false, true, false);//only on one
			RefCountryRequiredDocument requiredDoc18 = GetNewRequiredDoc(countryAU, Constants.RefDocTypes.Worksheet, aUS, "NZ", JobRequiredDocument.DocUsage.All, Constants.TransportModes.All, false, true, false, false);//only on one
			RefCountryRequiredDocument requiredDoc19 = GetNewRequiredDoc(countryAU, Constants.RefDocTypes.HeXiaoDan, aUS, "NZ", JobRequiredDocument.DocUsage.All, Constants.TransportModes.All, true, false, false, false);//only on one

			RefCountryRequiredDocument requiredDoc20 = GetNewRequiredDoc(countryAU, Constants.RefDocTypes.DangerousGoodsForm, aUS, "NZ", JobRequiredDocument.DocUsage.Import, Constants.TransportModes.All, true, true, true, true);//Should Be Set To BOTH
			RefCountryRequiredDocument requiredDoc21 = GetNewRequiredDoc(countryAU, Constants.RefDocTypes.DangerousGoodsForm, aUS, "NZ", JobRequiredDocument.DocUsage.Export, Constants.TransportModes.All, true, true, true, true);//Should Be Set To BOTH

			#region Test Shipment
			IDocsAndCartageParent shipment = (IDocsAndCartageParent)Factory.New<Enterprise.Integration.Forwarding.IForwardingShipment>();
			shipment.RequiredDocumentsProvider.RequiredDocuments.AddCountryRequiredDocuments(RefCountryRequiredDocumentSchema.RD_OnShipment, Constants.TransportModes.Sea, Constants.ContainerModes.FCL, JobRequiredDocumentDependentCollection.DirectionFilterType.Both, "AUSYD", "NZAKL");

			// ERROR
			AssertNull("Doesn't match, shouldn't be in the list", shipment.RequiredDocumentsProvider.RequiredDocuments.GetDocByType(Constants.RefDocTypes.ExportCartageAdvice));
			AssertNull(shipment.RequiredDocumentsProvider.RequiredDocuments.GetDocByType(Constants.RefDocTypes.FoodControlCertificate));
			AssertNull(shipment.RequiredDocumentsProvider.RequiredDocuments.GetDocByType(Constants.RefDocTypes.HouseBill));
			AssertNull(shipment.RequiredDocumentsProvider.RequiredDocuments.GetDocByType(Constants.RefDocTypes.ShipmentNotes));
			AssertNull(shipment.RequiredDocumentsProvider.RequiredDocuments.GetDocByType(Constants.RefDocTypes.PreAlert));
			AssertNull(shipment.RequiredDocumentsProvider.RequiredDocuments.GetDocByType(Constants.RefDocTypes.QuarantineCertificate));
			AssertNull(shipment.RequiredDocumentsProvider.RequiredDocuments.GetDocByType(Constants.RefDocTypes.SanitaryCertificate));
			AssertNull(shipment.RequiredDocumentsProvider.RequiredDocuments.GetDocByType(Constants.RefDocTypes.VetinaryCertificate));
			AssertNull(shipment.RequiredDocumentsProvider.RequiredDocuments.GetDocByType(Constants.RefDocTypes.HeXiaoDan));
			//NO ERROR
			JobRequiredDocument shipmentAgentInvoice = shipment.RequiredDocumentsProvider.RequiredDocuments.GetDocByType(Constants.RefDocTypes.AgentsInvoice);
			AssertNotNull("AgentInvoice: Should be in the list", shipment.RequiredDocumentsProvider.RequiredDocuments.GetDocByType(Constants.RefDocTypes.AgentsInvoice));
			AssertEquals("AgentInvoice: DocUsage should be BTH", "BTH", shipmentAgentInvoice.EQ_DocUsage);

			JobRequiredDocument shipmentImportCartageAdvice = shipment.RequiredDocumentsProvider.RequiredDocuments.GetDocByType(Constants.RefDocTypes.ImportCartageAdvice);
			AssertNotNull("ImportCartageAdvice: Should be in the list", shipment.RequiredDocumentsProvider.RequiredDocuments.GetDocByType(Constants.RefDocTypes.ImportCartageAdvice));
			AssertEquals("ImportCartageAdvice: DocUsage should be IMP", "IMP", shipmentImportCartageAdvice.EQ_DocUsage);

			JobRequiredDocument shipmentLabel = shipment.RequiredDocumentsProvider.RequiredDocuments.GetDocByType(Constants.RefDocTypes.Label);
			AssertNotNull("Label: Should be in the list", shipment.RequiredDocumentsProvider.RequiredDocuments.GetDocByType(Constants.RefDocTypes.Label));
			AssertEquals("Label: DocUsage should be EXP", "EXP", shipmentLabel.EQ_DocUsage);

			AssertNotNull("Should be in the list", shipment.RequiredDocumentsProvider.RequiredDocuments.GetDocByType(Constants.RefDocTypes.BeneficiaryCertificate));
			AssertNotNull("Should be in the list", shipment.RequiredDocumentsProvider.RequiredDocuments.GetDocByType(Constants.RefDocTypes.CartageAdvice));
			AssertNotNull("Should be in the list", shipment.RequiredDocumentsProvider.RequiredDocuments.GetDocByType(Constants.RefDocTypes.DelayAlert));
			AssertNotNull("Should be in the list", shipment.RequiredDocumentsProvider.RequiredDocuments.GetDocByType(Constants.RefDocTypes.Manifest));
			AssertNotNull("Should be in the list", shipment.RequiredDocumentsProvider.RequiredDocuments.GetDocByType(Constants.RefDocTypes.MasterBill));
			AssertNotNull("Should be in the list", shipment.RequiredDocumentsProvider.RequiredDocuments.GetDocByType(Constants.RefDocTypes.RequestDocument));
			AssertNotNull("Should be in the list", shipment.RequiredDocumentsProvider.RequiredDocuments.GetDocByType(Constants.RefDocTypes.Worksheet));
			AssertNotNull("Should be in the list", shipment.RequiredDocumentsProvider.RequiredDocuments.GetDocByType(Constants.RefDocTypes.DangerousGoodsForm));
			#endregion

			#region Test Declaration
			IDocsAndCartageParent declaration = (IDocsAndCartageParent)Factory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();
			declaration.RequiredDocumentsProvider.RequiredDocuments.AddCountryRequiredDocuments(RefCountryRequiredDocumentSchema.RD_OnBrokerage, Constants.TransportModes.Sea, Constants.ContainerModes.FCL, JobRequiredDocumentDependentCollection.DirectionFilterType.Export, "AUSYD", "NZAKL");

			// ERROR
			AssertNull("Doesn't match, shouldn't be in the list", declaration.RequiredDocumentsProvider.RequiredDocuments.GetDocByType(Constants.RefDocTypes.ExportCartageAdvice));
			AssertNull(declaration.RequiredDocumentsProvider.RequiredDocuments.GetDocByType(Constants.RefDocTypes.FoodControlCertificate));
			AssertNull(declaration.RequiredDocumentsProvider.RequiredDocuments.GetDocByType(Constants.RefDocTypes.HouseBill));
			AssertNull(declaration.RequiredDocumentsProvider.RequiredDocuments.GetDocByType(Constants.RefDocTypes.ShipmentNotes));
			AssertNull(declaration.RequiredDocumentsProvider.RequiredDocuments.GetDocByType(Constants.RefDocTypes.PreAlert));
			AssertNull(declaration.RequiredDocumentsProvider.RequiredDocuments.GetDocByType(Constants.RefDocTypes.QuarantineCertificate));
			AssertNull(declaration.RequiredDocumentsProvider.RequiredDocuments.GetDocByType(Constants.RefDocTypes.Worksheet));
			AssertNull(declaration.RequiredDocumentsProvider.RequiredDocuments.GetDocByType(Constants.RefDocTypes.SanitaryCertificate));
			AssertNull(declaration.RequiredDocumentsProvider.RequiredDocuments.GetDocByType(Constants.RefDocTypes.HeXiaoDan));
			AssertNull(declaration.RequiredDocumentsProvider.RequiredDocuments.GetDocByType(Constants.RefDocTypes.ImportCartageAdvice));
			//NO ERROR
			JobRequiredDocument declarationAgentInvoice = declaration.RequiredDocumentsProvider.RequiredDocuments.GetDocByType(Constants.RefDocTypes.AgentsInvoice);
			AssertNotNull("AgentInvoice: Should be in the list", declaration.RequiredDocumentsProvider.RequiredDocuments.GetDocByType(Constants.RefDocTypes.AgentsInvoice));
			AssertNotEquals("AgentInvoice: DocUsage should not be BTH", "BTH", declarationAgentInvoice.EQ_DocUsage);
			AssertEquals("AgentInvoice: DocUsage should be EXP", "EXP", declarationAgentInvoice.EQ_DocUsage);

			JobRequiredDocument declarationLabel = declaration.RequiredDocumentsProvider.RequiredDocuments.GetDocByType(Constants.RefDocTypes.Label);
			AssertNotNull("Label: Should be in the list", declaration.RequiredDocumentsProvider.RequiredDocuments.GetDocByType(Constants.RefDocTypes.Label));
			AssertEquals("Label: DocUsage should be EXP", "EXP", declarationLabel.EQ_DocUsage);

			AssertNotNull("Should be in the list", declaration.RequiredDocumentsProvider.RequiredDocuments.GetDocByType(Constants.RefDocTypes.BeneficiaryCertificate));
			AssertNotNull("Should be in the list", declaration.RequiredDocumentsProvider.RequiredDocuments.GetDocByType(Constants.RefDocTypes.CartageAdvice));
			AssertNotNull("Should be in the list", declaration.RequiredDocumentsProvider.RequiredDocuments.GetDocByType(Constants.RefDocTypes.DelayAlert));
			AssertNotNull("Should be in the list", declaration.RequiredDocumentsProvider.RequiredDocuments.GetDocByType(Constants.RefDocTypes.Manifest));
			AssertNotNull("Should be in the list", declaration.RequiredDocumentsProvider.RequiredDocuments.GetDocByType(Constants.RefDocTypes.MasterBill));
			AssertNotNull("Should be in the list", declaration.RequiredDocumentsProvider.RequiredDocuments.GetDocByType(Constants.RefDocTypes.RequestDocument));
			AssertNotNull("Should be in the list", declaration.RequiredDocumentsProvider.RequiredDocuments.GetDocByType(Constants.RefDocTypes.VetinaryCertificate));
			AssertNotNull("Should be in the list", declaration.RequiredDocumentsProvider.RequiredDocuments.GetDocByType(Constants.RefDocTypes.DangerousGoodsForm));

			#endregion

			#region Test Order
			IHaveRequiredDocuments order = (IHaveRequiredDocuments)Factory.New<Enterprise.Integration.Forwarding.IOrder>();
			order.RequiredDocuments.AddCountryRequiredDocuments(RefCountryRequiredDocumentSchema.RD_OnOrder, Constants.TransportModes.Sea, Constants.ContainerModes.FCL, JobRequiredDocumentDependentCollection.DirectionFilterType.Both, "AUSYD", "NZAKL");

			// ERROR
			AssertNull("Doesn't match, shouldn't be in the list", order.RequiredDocuments.GetDocByType(Constants.RefDocTypes.ExportCartageAdvice));
			AssertNull(order.RequiredDocuments.GetDocByType(Constants.RefDocTypes.FoodControlCertificate));
			AssertNull(order.RequiredDocuments.GetDocByType(Constants.RefDocTypes.HouseBill));
			AssertNull(order.RequiredDocuments.GetDocByType(Constants.RefDocTypes.ShipmentNotes));
			AssertNull(order.RequiredDocuments.GetDocByType(Constants.RefDocTypes.PreAlert));
			AssertNull(order.RequiredDocuments.GetDocByType(Constants.RefDocTypes.QuarantineCertificate));
			AssertNull(order.RequiredDocuments.GetDocByType(Constants.RefDocTypes.Worksheet));
			AssertNull(order.RequiredDocuments.GetDocByType(Constants.RefDocTypes.VetinaryCertificate));
			AssertNull(order.RequiredDocuments.GetDocByType(Constants.RefDocTypes.HeXiaoDan));
			//NO ERROR
			JobRequiredDocument orderAgentInvoice = order.RequiredDocuments.GetDocByType(Constants.RefDocTypes.AgentsInvoice);
			AssertNotNull("AgentInvoice: Should be in the list", order.RequiredDocuments.GetDocByType(Constants.RefDocTypes.AgentsInvoice));
			AssertEquals("AgentInvoice: DocUsage should be BTH", "BTH", orderAgentInvoice.EQ_DocUsage);

			JobRequiredDocument orderImportCartageAdvice = order.RequiredDocuments.GetDocByType(Constants.RefDocTypes.ImportCartageAdvice);
			AssertNotNull("ImportCartageAdvice: Should be in the list", order.RequiredDocuments.GetDocByType(Constants.RefDocTypes.ImportCartageAdvice));
			AssertEquals("ImportCartageAdvice: DocUsage should be IMP", "IMP", orderImportCartageAdvice.EQ_DocUsage);

			JobRequiredDocument orderLabel = order.RequiredDocuments.GetDocByType(Constants.RefDocTypes.Label);
			AssertNotNull("Label: Should be in the list", order.RequiredDocuments.GetDocByType(Constants.RefDocTypes.Label));
			AssertEquals("Label: DocUsage should be EXP", "EXP", orderLabel.EQ_DocUsage);

			AssertNotNull("Should be in the list", order.RequiredDocuments.GetDocByType(Constants.RefDocTypes.BeneficiaryCertificate));
			AssertNotNull("Should be in the list", order.RequiredDocuments.GetDocByType(Constants.RefDocTypes.CartageAdvice));
			AssertNotNull("Should be in the list", order.RequiredDocuments.GetDocByType(Constants.RefDocTypes.DelayAlert));
			AssertNotNull("Should be in the list", order.RequiredDocuments.GetDocByType(Constants.RefDocTypes.Manifest));
			AssertNotNull("Should be in the list", order.RequiredDocuments.GetDocByType(Constants.RefDocTypes.MasterBill));
			AssertNotNull("Should be in the list", order.RequiredDocuments.GetDocByType(Constants.RefDocTypes.RequestDocument));
			AssertNotNull("Should be in the list", order.RequiredDocuments.GetDocByType(Constants.RefDocTypes.SanitaryCertificate));
			AssertNotNull("Should be in the list", order.RequiredDocuments.GetDocByType(Constants.RefDocTypes.DangerousGoodsForm));
			#endregion

			#region Test Consol
			IHaveRequiredDocuments consol = (IHaveRequiredDocuments)Factory.New<Enterprise.Integration.Forwarding.IForwardingConsol>();
			consol.RequiredDocuments.AddCountryRequiredDocuments(RefCountryRequiredDocumentSchema.RD_OnConsol, Constants.TransportModes.Sea, Constants.ContainerModes.FCL, JobRequiredDocumentDependentCollection.DirectionFilterType.Both, "AUSYD", "NZAKL");

			// ERROR
			AssertNull("Doesn't match, shouldn't be in the list", consol.RequiredDocuments.GetDocByType(Constants.RefDocTypes.ExportCartageAdvice));
			AssertNull(consol.RequiredDocuments.GetDocByType(Constants.RefDocTypes.FoodControlCertificate));
			AssertNull(consol.RequiredDocuments.GetDocByType(Constants.RefDocTypes.HouseBill));
			AssertNull(consol.RequiredDocuments.GetDocByType(Constants.RefDocTypes.ShipmentNotes));
			AssertNull(consol.RequiredDocuments.GetDocByType(Constants.RefDocTypes.PreAlert));
			AssertNull(consol.RequiredDocuments.GetDocByType(Constants.RefDocTypes.QuarantineCertificate));
			AssertNull(consol.RequiredDocuments.GetDocByType(Constants.RefDocTypes.Worksheet));
			AssertNull(consol.RequiredDocuments.GetDocByType(Constants.RefDocTypes.VetinaryCertificate));
			AssertNull(consol.RequiredDocuments.GetDocByType(Constants.RefDocTypes.SanitaryCertificate));
			//NO ERROR
			JobRequiredDocument consolAgentInvoice = consol.RequiredDocuments.GetDocByType(Constants.RefDocTypes.AgentsInvoice);
			AssertNotNull("AgentInvoice: Should be in the list", consol.RequiredDocuments.GetDocByType(Constants.RefDocTypes.AgentsInvoice));
			AssertEquals("AgentInvoice: DocUsage should be BTH", "BTH", consolAgentInvoice.EQ_DocUsage);

			JobRequiredDocument consolImportCartageAdvice = consol.RequiredDocuments.GetDocByType(Constants.RefDocTypes.ImportCartageAdvice);
			AssertNotNull("ImportCartageAdvice: Should be in the list", consol.RequiredDocuments.GetDocByType(Constants.RefDocTypes.ImportCartageAdvice));
			AssertEquals("ImportCartageAdvice: DocUsage should be IMP", "IMP", consolImportCartageAdvice.EQ_DocUsage);

			JobRequiredDocument consolLabel = consol.RequiredDocuments.GetDocByType(Constants.RefDocTypes.Label);
			AssertNotNull("Label: Should be in the list", consol.RequiredDocuments.GetDocByType(Constants.RefDocTypes.Label));
			AssertEquals("Label: DocUsage should be EXP", "EXP", consolLabel.EQ_DocUsage);

			AssertNotNull("Should be in the list", consol.RequiredDocuments.GetDocByType(Constants.RefDocTypes.BeneficiaryCertificate));
			AssertNotNull("Should be in the list", consol.RequiredDocuments.GetDocByType(Constants.RefDocTypes.CartageAdvice));
			AssertNotNull("Should be in the list", consol.RequiredDocuments.GetDocByType(Constants.RefDocTypes.DelayAlert));
			AssertNotNull("Should be in the list", consol.RequiredDocuments.GetDocByType(Constants.RefDocTypes.Manifest));
			AssertNotNull("Should be in the list", consol.RequiredDocuments.GetDocByType(Constants.RefDocTypes.MasterBill));
			AssertNotNull("Should be in the list", consol.RequiredDocuments.GetDocByType(Constants.RefDocTypes.RequestDocument));
			AssertNotNull("Should be in the list", consol.RequiredDocuments.GetDocByType(Constants.RefDocTypes.HeXiaoDan));
			AssertNotNull("Should be in the list", consol.RequiredDocuments.GetDocByType(Constants.RefDocTypes.DangerousGoodsForm));
			#endregion
		}

		public void TestAddOrganisationRequiredDocumentsWithDateNotExpired()
		{
			var docType = Factory.NewWithValidTestData<RefDocType>();
			docType.RT_AllowMultiplePeriodicDocs = true;

			var documentOwner = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();

			var organisation = Factory.New<OrgHeader>();
			var validToDate = ZDate.Today.AddDays(10);
			var requiredDoc1 = GetNewJobRequiredDoc(organisation, docType.RT_DocType, JobRequiredDocument.DocUsage.Both, Constants.JobRequiredDocuments.DocumentPeriods.Periodic, ZDate.Today.AddDays(-2), validToDate, "0001");
			requiredDoc1.EQ_OH_DocumentOwner = documentOwner.PK;
			var requiredDoc2 = GetNewJobRequiredDoc(organisation, docType.RT_DocType, JobRequiredDocument.DocUsage.Both, Constants.JobRequiredDocuments.DocumentPeriods.Periodic, ZDate.Today.AddDays(-2), ZDate.Today.AddDays(-1), "0002");
			organisation.RequiredDocuments.Add(requiredDoc1);
			organisation.RequiredDocuments.Add(requiredDoc2);

			var shipment = (IDocsAndCartageParent)Factory.New<Enterprise.Integration.Forwarding.IForwardingShipment>();
			shipment.RequiredDocumentsProvider.RequiredDocuments.AddAndAcquitRequiredDocuments(organisation, null, null, null, ZDate.Today.AddDays(4));

			AssertEquals("Only 1 required document was added.", 1, shipment.RequiredDocumentsProvider.RequiredDocuments.Count);
			AssertEquals(validToDate, shipment.RequiredDocumentsProvider.RequiredDocuments[0].EQ_ValidToDate);
			AssertEquals("document number should be added by default.", "0001", shipment.RequiredDocumentsProvider.RequiredDocuments[0].EQ_DocNumber);
			AssertEquals("document owner should be added by default.", documentOwner.PK, shipment.RequiredDocumentsProvider.RequiredDocuments[0].EQ_OH_DocumentOwner);
		}

		public void TestAddOrganisationRequiredDocumentsWithDocumentNotes()
		{
			var docType = Factory.NewWithValidTestData<RefDocType>();
			docType.RT_AllowMultiplePeriodicDocs = true;

			var documentOwner = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();

			var organisation = Factory.New<OrgHeader>();
			var validToDate = ZDate.Today.AddDays(10);
			var requiredDoc1 = GetNewJobRequiredDoc(organisation, docType.RT_DocType, JobRequiredDocument.DocUsage.Both, Constants.JobRequiredDocuments.DocumentPeriods.Periodic, ZDate.Today.AddDays(-2), validToDate, "0001");
			var documentNotes = "Test Document Notes";
			requiredDoc1.EQ_OH_DocumentOwner = documentOwner.PK;
			requiredDoc1.EQ_DocumentNotes = documentNotes;
			organisation.RequiredDocuments.Add(requiredDoc1);

			var shipment = (IDocsAndCartageParent)Factory.New<Enterprise.Integration.Forwarding.IForwardingShipment>();
			shipment.RequiredDocumentsProvider.RequiredDocuments.AddAndAcquitRequiredDocuments(organisation, null, null, null, ZDate.Today.AddDays(4));

			AssertEquals("document notes should be copied", documentNotes, shipment.RequiredDocumentsProvider.RequiredDocuments[0].EQ_DocumentNotes);
		}

		public void TestAddRequiredDocumentWithDuplicateDocType()
		{
			var docType = Factory.NewWithValidTestData<RefDocType>();
			docType.RT_DocType = "DC1";
			docType.RT_AllowMultiplePeriodicDocs = true;
			docType.RT_ReferenceType = Constants.ReferenceTypes.SupplyChainLogistics;

			var organisation = Factory.NewWithValidTestData<OrgHeader>();
			var validToDate = ZDate.Today.AddDays(10);
			GetNewJobRequiredDoc(organisation, docType.RT_DocType, JobRequiredDocument.DocUsage.Both, Constants.JobRequiredDocuments.DocumentPeriods.Periodic, ZDate.Today.AddDays(-2), validToDate, "0001");
			GetNewJobRequiredDoc(organisation, docType.RT_DocType, JobRequiredDocument.DocUsage.Both, Constants.JobRequiredDocuments.DocumentPeriods.Periodic, ZDate.Today.AddDays(-2), validToDate, "0001");
			Factory.Save();

			docType.RT_AllowMultiplePeriodicDocs = false;
			Factory.Save();

			var shipment = (IDocsAndCartageParent)Factory.New<Enterprise.Integration.Forwarding.IForwardingShipment>();
			shipment.RequiredDocumentsProvider.RequiredDocuments.AddAndAcquitRequiredDocuments(organisation, null, null, null, ZDate.Today.AddDays(4));
			AssertNoExceptionThrown(() =>
			{
				JobRequiredDocumentConcurrencyChecker.Register(Factory);
				Factory.Save();
				AssertEquals("1 required documents with duplicate doc type 'DC1' was added.", 1, shipment.RequiredDocumentsProvider.RequiredDocuments.OfType<JobRequiredDocument>().Count(doc => doc.EQ_DocType == docType.RT_DocType));
			});
		}

		public void TestAddRequiredDocumentMultipleTimesWithDuplicateDocType()
		{
			var docType = Factory.NewWithValidTestData<RefDocType>();
			docType.RT_DocType = "DC1";
			docType.RT_AllowMultiplePeriodicDocs = true;

			var documentOwner = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();

			var organisation = Factory.NewWithValidTestData<OrgHeader>();
			var validToDate = ZDate.Today.AddDays(10);
			var requiredDoc1 = GetNewJobRequiredDoc(organisation, docType.RT_DocType, JobRequiredDocument.DocUsage.Both, Constants.JobRequiredDocuments.DocumentPeriods.Periodic, ZDate.Today.AddDays(-2), validToDate, "0001");
			requiredDoc1.EQ_OH_DocumentOwner = documentOwner.PK;
			var requiredDoc2 = GetNewJobRequiredDoc(organisation, docType.RT_DocType, JobRequiredDocument.DocUsage.Both, Constants.JobRequiredDocuments.DocumentPeriods.Periodic, ZDate.Today.AddDays(-2), validToDate, "0002");
			organisation.RequiredDocuments.Add(requiredDoc1);
			organisation.RequiredDocuments.Add(requiredDoc2);

			var shipment = (IDocsAndCartageParent)Factory.New<Enterprise.Integration.Forwarding.IForwardingShipment>();
			shipment.RequiredDocumentsProvider.RequiredDocuments.AddAndAcquitRequiredDocuments(organisation, null, null, null, ZDate.Today.AddDays(4));
			Factory.Save();

			AssertEquals("2 required documents with duplicate doc type 'DC1' was added.", 2, shipment.RequiredDocumentsProvider.RequiredDocuments.OfType<JobRequiredDocument>().Count(doc => doc.EQ_DocType == docType.RT_DocType));

			var secondDocType = Factory.NewWithValidTestData<RefDocType>();
			secondDocType.RT_DocType = "DC2";
			secondDocType.RT_AllowMultiplePeriodicDocs = true;
			var secondOrganisation = Factory.NewWithValidTestData<OrgHeader>();
			var requiredDoc3 = GetNewJobRequiredDoc(secondOrganisation, secondDocType.RT_DocType, JobRequiredDocument.DocUsage.Both, Constants.JobRequiredDocuments.DocumentPeriods.Periodic, ZDate.Today.AddDays(-2), validToDate, "0003");
			requiredDoc1.EQ_OH_DocumentOwner = documentOwner.PK;
			var requiredDoc4 = GetNewJobRequiredDoc(secondOrganisation, secondDocType.RT_DocType, JobRequiredDocument.DocUsage.Both, Constants.JobRequiredDocuments.DocumentPeriods.Periodic, ZDate.Today.AddDays(-2), validToDate, "0004");
			var requiredDoc5 = GetNewJobRequiredDoc(secondOrganisation, docType.RT_DocType, JobRequiredDocument.DocUsage.Both, Constants.JobRequiredDocuments.DocumentPeriods.Periodic, ZDate.Today.AddDays(-2), validToDate, "0005");
			secondOrganisation.RequiredDocuments.Add(requiredDoc3);
			secondOrganisation.RequiredDocuments.Add(requiredDoc4);
			secondOrganisation.RequiredDocuments.Add(requiredDoc5);

			shipment.RequiredDocumentsProvider.RequiredDocuments.AddAndAcquitRequiredDocuments(secondOrganisation, null, null, null, ZDate.Today.AddDays(4));
			Factory.Save();

			AssertEquals("2 required documents with duplicate doc type 'DC2' was added.", 2, shipment.RequiredDocumentsProvider.RequiredDocuments.OfType<JobRequiredDocument>().Count(doc => doc.EQ_DocType == secondDocType.RT_DocType));
			AssertEquals("no required documents with doc type 'DC1' added as it already exists.", 2, shipment.RequiredDocumentsProvider.RequiredDocuments.OfType<JobRequiredDocument>().Count(doc => doc.EQ_DocType == docType.RT_DocType));
		}

		[TestDate(2007, 3, 15, 10, 15, 21)]
		public void TestAddOrganisationRequiredDocuments()
		{
			RefCountry countryAU = RefCountry.LoadFromCountryCode(Factory, Constants.CountryCodes.Australia);
			ZString aUS = Constants.CountryCodes.Australia;

			RefCountryRequiredDocument requiredDoc1 = GetNewRequiredDoc(countryAU, Constants.RefDocTypes.AgentsInvoice, aUS, "NZ", JobRequiredDocument.DocUsage.All, Constants.TransportModes.All, true, true, true, true);//pass
			RefCountryRequiredDocument requiredDoc2 = GetNewRequiredDoc(countryAU, Constants.RefDocTypes.BeneficiaryCertificate, "", "NZ", JobRequiredDocument.DocUsage.All, Constants.TransportModes.All, true, true, true, true);//pass
			RefCountryRequiredDocument testRequiredDoc6 = GetNewRequiredDoc(countryAU, Constants.RefDocTypes.VetinaryCertificate, "", "", JobRequiredDocument.DocUsage.All, Constants.TransportModes.All, true, true, true, true);//pass

			OrgHeader organisation = Factory.New<OrgHeader>();

			JobRequiredDocument requiredDoc3 = GetNewJobRequiredDoc(organisation, Constants.RefDocTypes.DangerousGoodsForm, JobRequiredDocument.DocUsage.Both, Constants.JobRequiredDocuments.DocumentPeriods.OncePerShipment, ZDate.Today.AddDays(-2), ZDate.Today.AddDays(2), "0001");
			JobRequiredDocument requiredDoc4 = GetNewJobRequiredDoc(organisation, Constants.RefDocTypes.AgentsInvoice, JobRequiredDocument.DocUsage.Both, Constants.JobRequiredDocuments.DocumentPeriods.Periodic, ZDate.Today.AddDays(-2), ZDate.Today.AddDays(2), "0002");
			JobRequiredDocument requiredDoc5 = GetNewJobRequiredDoc(organisation, Constants.RefDocTypes.BeneficiaryCertificate, JobRequiredDocument.DocUsage.Both, Constants.JobRequiredDocuments.DocumentPeriods.OncePerShipment, ZDate.Today.AddDays(-2), ZDate.Today.AddDays(2), "0003");
			JobRequiredDocument requiredDoc6 = GetNewJobRequiredDoc(organisation, Constants.RefDocTypes.VetinaryCertificate, JobRequiredDocument.DocUsage.Both, Constants.JobRequiredDocuments.DocumentPeriods.Periodic, ZDate.Today.AddDays(-2), ZDate.Today.AddDays(2), "0004");
			requiredDoc6.EQ_DateReceived = ZDateTimeOffset.Today.AddDays(-2);
			JobRequiredDocument requiredDoc7 = GetNewJobRequiredDoc(organisation, Constants.RefDocTypes.SanitaryCertificate, JobRequiredDocument.DocUsage.Both, Constants.JobRequiredDocuments.DocumentPeriods.Periodic, ZDate.Today.AddDays(-2), ZDate.Today.AddDays(2), "0005");
			JobRequiredDocument requiredDoc8 = GetNewJobRequiredDoc(organisation, Constants.RefDocTypes.MiscellaneousDocument, JobRequiredDocument.DocUsage.Both, Constants.JobRequiredDocuments.DocumentPeriods.Periodic, ZDate.Today.AddDays(-2), ZDate.Today.AddDays(2), "0005");
			requiredDoc8.EQ_DocDescription = "test";

			IDocsAndCartageParent shipment = (IDocsAndCartageParent)Factory.New<Enterprise.Integration.Forwarding.IForwardingShipment>();
			//set organinisation to shipment consignor or consignee
			shipment.RequiredDocumentsProvider.RequiredDocuments.AddCountryRequiredDocuments(RefCountryRequiredDocumentSchema.RD_OnShipment, Constants.TransportModes.Sea, Constants.ContainerModes.FCL, JobRequiredDocumentDependentCollection.DirectionFilterType.Both, "AUSYD", "NZAKL");
			shipment.RequiredDocumentsProvider.RequiredDocuments.AddAndAcquitRequiredDocuments(organisation, null, null, null, ZDate.Today.AddDays(1));

			JobRequiredDocument rdAgentInvoice = shipment.RequiredDocumentsProvider.RequiredDocuments.GetDocByType(Constants.RefDocTypes.AgentsInvoice);
			AssertNotNull("AgentInvoice: Should be in the list", shipment.RequiredDocumentsProvider.RequiredDocuments.GetDocByType(Constants.RefDocTypes.AgentsInvoice));
			AssertEquals("AgentInvoice: Period should be Once Per Shipment", Constants.JobRequiredDocuments.DocumentPeriods.OncePerShipment, rdAgentInvoice.EQ_DocPeriod);

			JobRequiredDocument rdVetinaryCertificate = shipment.RequiredDocumentsProvider.RequiredDocuments.GetDocByType(Constants.RefDocTypes.VetinaryCertificate);
			AssertNotNull("VetinaryCertificate: Should be in the list", shipment.RequiredDocumentsProvider.RequiredDocuments.GetDocByType(Constants.RefDocTypes.VetinaryCertificate));
			AssertEquals("VetinaryCertificate: Period should be Once Per Shipment", Constants.JobRequiredDocuments.DocumentPeriods.OncePerShipment, rdVetinaryCertificate.EQ_DocPeriod);

			AssertNotNull(shipment.RequiredDocumentsProvider.RequiredDocuments.GetDocByType(Constants.RefDocTypes.AgentsInvoice));
			AssertNotNull(shipment.RequiredDocumentsProvider.RequiredDocuments.GetDocByType(Constants.RefDocTypes.BeneficiaryCertificate));
			AssertNotNull(shipment.RequiredDocumentsProvider.RequiredDocuments.GetDocByType(Constants.RefDocTypes.DangerousGoodsForm));
			AssertNotNull(shipment.RequiredDocumentsProvider.RequiredDocuments.GetDocByType(Constants.RefDocTypes.MiscellaneousDocument));
			AssertEquals("test", shipment.RequiredDocumentsProvider.RequiredDocuments.GetDocByType(Constants.RefDocTypes.MiscellaneousDocument).EQ_DocDescription);

			//Testing Additional Country Column Required Documents			
			OrgHeader countryOrganisation = Factory.New<OrgHeader>();
			countryOrganisation.OH_RL_NKClosestPort = "AUSYD";

			JobRequiredDocument reqDoc1 = GetNewJobRequiredDoc(countryOrganisation, Constants.RefDocTypes.AgentsInvoice, JobRequiredDocument.DocUsage.Domestic, Constants.JobRequiredDocuments.DocumentPeriods.OncePerShipment, ZDate.Today.AddDays(-2), ZDate.Today.AddDays(2), "0001");
			reqDoc1.EQ_RN_NKRelatedCountry = countryOrganisation.CountryCode;
			JobRequiredDocument reqDoc2 = GetNewJobRequiredDoc(countryOrganisation, Constants.RefDocTypes.BeneficiaryCertificate, JobRequiredDocument.DocUsage.Both, Constants.JobRequiredDocuments.DocumentPeriods.OncePerShipment, ZDate.Today.AddDays(-2), ZDate.Today.AddDays(2), "0002");
			reqDoc2.EQ_RN_NKRelatedCountry = countryOrganisation.CountryCode;
			JobRequiredDocument reqDoc3 = GetNewJobRequiredDoc(countryOrganisation, Constants.RefDocTypes.DangerousGoodsForm, JobRequiredDocument.DocUsage.Import, Constants.JobRequiredDocuments.DocumentPeriods.OncePerShipment, ZDate.Today.AddDays(-2), ZDate.Today.AddDays(2), "0003");
			reqDoc3.EQ_RN_NKRelatedCountry = countryOrganisation.CountryCode;
			JobRequiredDocument reqDoc4 = GetNewJobRequiredDoc(countryOrganisation, Constants.RefDocTypes.VetinaryCertificate, JobRequiredDocument.DocUsage.Export, Constants.JobRequiredDocuments.DocumentPeriods.OncePerShipment, ZDate.Today.AddDays(-2), ZDate.Today.AddDays(2), "0004");
			reqDoc4.EQ_RN_NKRelatedCountry = countryOrganisation.CountryCode;
			JobRequiredDocument reqDoc5 = GetNewJobRequiredDoc(countryOrganisation, Constants.RefDocTypes.ShipmentNotes, JobRequiredDocument.DocUsage.Domestic, Constants.JobRequiredDocuments.DocumentPeriods.OncePerShipment, ZDate.Today.AddDays(-2), ZDate.Today.AddDays(2), "0005");
			reqDoc5.EQ_RN_NKRelatedCountry = Constants.CountryCodes.Austria;
			JobRequiredDocument reqDoc6 = GetNewJobRequiredDoc(countryOrganisation, Constants.RefDocTypes.MasterBill, JobRequiredDocument.DocUsage.Both, Constants.JobRequiredDocuments.DocumentPeriods.OncePerShipment, ZDate.Today.AddDays(-2), ZDate.Today.AddDays(2), "0006");
			reqDoc6.EQ_RN_NKRelatedCountry = Constants.CountryCodes.Austria;
			JobRequiredDocument reqDoc7 = GetNewJobRequiredDoc(countryOrganisation, Constants.RefDocTypes.OutturnReport, JobRequiredDocument.DocUsage.Import, Constants.JobRequiredDocuments.DocumentPeriods.OncePerShipment, ZDate.Today.AddDays(-2), ZDate.Today.AddDays(2), "0007");
			reqDoc7.EQ_RN_NKRelatedCountry = Constants.CountryCodes.Austria;
			JobRequiredDocument reqDoc8 = GetNewJobRequiredDoc(countryOrganisation, Constants.RefDocTypes.PreAlert, JobRequiredDocument.DocUsage.Export, Constants.JobRequiredDocuments.DocumentPeriods.OncePerShipment, ZDate.Today.AddDays(-2), ZDate.Today.AddDays(2), "0008");
			reqDoc8.EQ_RN_NKRelatedCountry = Constants.CountryCodes.Austria;

			IDocsAndCartageParent countryTestShipment = (IDocsAndCartageParent)Factory.New<Enterprise.Integration.Forwarding.IForwardingShipment>();
			countryTestShipment.RequiredDocumentsProvider.RequiredDocuments.AddCountryRequiredDocuments(RefCountryRequiredDocumentSchema.RD_OnShipment, Constants.TransportModes.Sea, Constants.ContainerModes.FCL, JobRequiredDocumentDependentCollection.DirectionFilterType.Both, "AUSYD", "ATANT");
			countryTestShipment.RequiredDocumentsProvider.RequiredDocuments.AddAndAcquitRequiredDocuments(countryOrganisation, countryOrganisation.CountryCode, "ATANT", countryOrganisation, ZDateTime.Now.AddYears(1));

			AssertNull("Should not be in the list", countryTestShipment.RequiredDocumentsProvider.RequiredDocuments.GetDocByType(Constants.RefDocTypes.AgentsInvoice));
			AssertNull("Should not be in the list", countryTestShipment.RequiredDocumentsProvider.RequiredDocuments.GetDocByType(Constants.RefDocTypes.DangerousGoodsForm));
			AssertNull("Should not be in the list", countryTestShipment.RequiredDocumentsProvider.RequiredDocuments.GetDocByType(Constants.RefDocTypes.ShipmentNotes));
			AssertNull("Should not be in the list", countryTestShipment.RequiredDocumentsProvider.RequiredDocuments.GetDocByType(Constants.RefDocTypes.PreAlert));
			AssertNull("Should not be in the list", countryTestShipment.RequiredDocumentsProvider.RequiredDocuments.GetDocByType(Constants.RefDocTypes.OutturnReport));

			AssertNotNull("BeneficiaryCertificate: Should be in the list", countryTestShipment.RequiredDocumentsProvider.RequiredDocuments.GetDocByType(Constants.RefDocTypes.BeneficiaryCertificate));
			AssertNotNull("VetinaryCertificate: Should be in the list", countryTestShipment.RequiredDocumentsProvider.RequiredDocuments.GetDocByType(Constants.RefDocTypes.VetinaryCertificate));
			AssertNotNull("MasterBill: Should be in the list", countryTestShipment.RequiredDocumentsProvider.RequiredDocuments.GetDocByType(Constants.RefDocTypes.MasterBill));
		}

		[TestDate(2007, 4, 16, 11, 16, 22, 0)]
		public void TestAddBuyerSupplierRequiredDocuments()
		{
			RefCountry countryAU = RefCountry.LoadFromCountryCode(Factory, Constants.CountryCodes.Australia);

			RefCountryRequiredDocument requiredDoc2 = GetNewRequiredDoc(countryAU, Constants.RefDocTypes.BeneficiaryCertificate, "", "NZ", JobRequiredDocument.DocUsage.All, Constants.TransportModes.All, true, true, true, true);//pass

			OrgHeader organisation = Factory.New<OrgHeader>();
			OrgHeader organisation2 = Factory.New<OrgHeader>();

			JobRequiredDocument requiredDoc3 = GetNewJobRequiredDoc(organisation, Constants.RefDocTypes.DangerousGoodsForm, JobRequiredDocument.DocUsage.Both, Constants.JobRequiredDocuments.DocumentPeriods.OncePerShipment, ZDate.Today.AddDays(-2), ZDate.Today.AddDays(2), "0001");

			OrgSupplierBuyerLink buyerSupplierLink = Factory.New<OrgSupplierBuyerLink>();
			buyerSupplierLink.OL_OH_Buyer = organisation2.PK;
			buyerSupplierLink.OL_OH_Supplier = organisation.PK;

			JobRequiredDocument requiredDoc4 = GetNewBuyerSupplierRequiredDoc(buyerSupplierLink, Constants.RefDocTypes.DangerousGoodsForm, JobRequiredDocument.DocUsage.Both, Constants.JobRequiredDocuments.DocumentPeriods.Periodic, ZDate.Today.AddDays(-2), ZDate.Today.AddDays(2), "0002");
			JobRequiredDocument requiredDoc5 = GetNewBuyerSupplierRequiredDoc(buyerSupplierLink, Constants.RefDocTypes.BeneficiaryCertificate, JobRequiredDocument.DocUsage.Both, Constants.JobRequiredDocuments.DocumentPeriods.Periodic, ZDate.Today.AddDays(-2), ZDate.Today.AddDays(2), "0003");
			JobRequiredDocument requiredDoc6 = GetNewBuyerSupplierRequiredDoc(buyerSupplierLink, Constants.RefDocTypes.AgentsInvoice, JobRequiredDocument.DocUsage.Both, Constants.JobRequiredDocuments.DocumentPeriods.Periodic, ZDate.Today.AddDays(-2), ZDate.Today.AddDays(2), "0004");
			requiredDoc6.EQ_DateReceived = ZDateTimeOffset.Today.AddDays(-2);
			JobRequiredDocument requiredDoc7 = GetNewBuyerSupplierRequiredDoc(buyerSupplierLink, Constants.RefDocTypes.SanitaryCertificate, JobRequiredDocument.DocUsage.Both, Constants.JobRequiredDocuments.DocumentPeriods.OncePerShipment, ZDate.Today.AddDays(-2), ZDate.Today.AddDays(2), "0005");
			JobRequiredDocument requiredDoc8 = GetNewBuyerSupplierRequiredDoc(buyerSupplierLink, Constants.RefDocTypes.VetinaryCertificate, JobRequiredDocument.DocUsage.Both, Constants.JobRequiredDocuments.DocumentPeriods.Periodic, ZDate.Today.AddDays(-2), ZDate.Today.AddDays(2), "0006");

			IDocsAndCartageParent shipment = (IDocsAndCartageParent)Factory.New<Enterprise.Integration.Forwarding.IForwardingShipment>();
			shipment.RequiredDocumentsProvider.RequiredDocuments.AddCountryRequiredDocuments(RefCountryRequiredDocumentSchema.RD_OnShipment, Constants.TransportModes.Sea, Constants.ContainerModes.FCL, JobRequiredDocumentDependentCollection.DirectionFilterType.Both, "AUSYD", "NZAKL");
			shipment.RequiredDocumentsProvider.RequiredDocuments.AddAndAcquitRequiredDocuments(organisation, null, null, null, ZDateTime.Now.AddDays(1));
			shipment.RequiredDocumentsProvider.RequiredDocuments.AddAndAcquitRequiredDocuments(buyerSupplierLink, null, null, null, ZDateTime.Now.AddDays(1));

			JobRequiredDocument rdVetinaryCertificate = shipment.RequiredDocumentsProvider.RequiredDocuments.GetDocByType(Constants.RefDocTypes.VetinaryCertificate);
			AssertNotNull("VetinaryCertificate: Should be in the list", shipment.RequiredDocumentsProvider.RequiredDocuments.GetDocByType(Constants.RefDocTypes.VetinaryCertificate));
			AssertEquals("VetinaryCertificate: Period should be periodic", Constants.JobRequiredDocuments.DocumentPeriods.Periodic, rdVetinaryCertificate.EQ_DocPeriod);
			AssertEquals(JobRequiredDocument.JRDOrigin.FromRequirements, rdVetinaryCertificate.Origin);

			JobRequiredDocument rdBeneficiaryCertificate = shipment.RequiredDocumentsProvider.RequiredDocuments.GetDocByType(Constants.RefDocTypes.BeneficiaryCertificate);
			AssertNotNull("BeneficiaryCertificate: Should be in the list", shipment.RequiredDocumentsProvider.RequiredDocuments.GetDocByType(Constants.RefDocTypes.BeneficiaryCertificate));
			AssertEquals("BeneficiaryCertificate: Period should be OnceShipment", Constants.JobRequiredDocuments.DocumentPeriods.OncePerShipment, rdBeneficiaryCertificate.EQ_DocPeriod);
			AssertEquals(JobRequiredDocument.JRDOrigin.FromRequirements, rdBeneficiaryCertificate.Origin);

			JobRequiredDocument rdAgentsInvoice = shipment.RequiredDocumentsProvider.RequiredDocuments.GetDocByType(Constants.RefDocTypes.AgentsInvoice);
			AssertNotNull("AgentInvoice: Should be in the list", shipment.RequiredDocumentsProvider.RequiredDocuments.GetDocByType(Constants.RefDocTypes.AgentsInvoice));
			AssertEquals("AgentInvoice: Date Received should be two days ago date", ZDateTimeOffset.Today.AddDays(-2), rdAgentsInvoice.EQ_DateReceived);
			AssertEquals("AgentsInvoice: Period should be periodic", Constants.JobRequiredDocuments.DocumentPeriods.Periodic, rdAgentsInvoice.EQ_DocPeriod);
			AssertEquals(JobRequiredDocument.JRDOrigin.FromRequirements, rdAgentsInvoice.Origin);

			AssertNotNull(shipment.RequiredDocumentsProvider.RequiredDocuments.GetDocByType(Constants.RefDocTypes.SanitaryCertificate));
			AssertNotNull(shipment.RequiredDocumentsProvider.RequiredDocuments.GetDocByType(Constants.RefDocTypes.DangerousGoodsForm));
		}

		public void TestAddAndAcquitRequiredDocumentCopiesOriginalDocRequiredAndCreditControlDoc()
		{
			OrgHeader organisation = Factory.New<OrgHeader>();
			OrgHeader organisation2 = Factory.New<OrgHeader>();

			JobRequiredDocument requiredDoc = GetNewJobRequiredDoc(organisation, Constants.RefDocTypes.DangerousGoodsForm, JobRequiredDocument.DocUsage.Both, Constants.JobRequiredDocuments.DocumentPeriods.OncePerShipment, ZDate.Today.AddDays(-2), ZDate.Today.AddDays(2), "0001");
			requiredDoc.EQ_CreditControlDoc = true;
			requiredDoc.EQ_OriginalDocRequired = true;

			OrgSupplierBuyerLink buyerSupplierLink = Factory.New<OrgSupplierBuyerLink>();
			buyerSupplierLink.OL_OH_Buyer = organisation2.PK;
			buyerSupplierLink.OL_OH_Supplier = organisation.PK;

			JobRequiredDocument requiredDoc2 = GetNewBuyerSupplierRequiredDoc(buyerSupplierLink, Constants.RefDocTypes.SanitaryCertificate, JobRequiredDocument.DocUsage.Both, Constants.JobRequiredDocuments.DocumentPeriods.OncePerShipment, ZDate.Today.AddDays(-2), ZDate.Today.AddDays(2), "0005");
			requiredDoc2.EQ_CreditControlDoc = true;
			requiredDoc2.EQ_OriginalDocRequired = true;

			IDocsAndCartageParent shipment = (IDocsAndCartageParent)Factory.New<Enterprise.Integration.Forwarding.IForwardingShipment>();
			AssertEquals(0, shipment.RequiredDocumentsProvider.RequiredDocuments.Count);

			shipment.RequiredDocumentsProvider.RequiredDocuments.AddAndAcquitRequiredDocuments(organisation, null, null, null, ZDateTime.Now.AddDays(1));
			AssertEquals(1, shipment.RequiredDocumentsProvider.RequiredDocuments.Count);
			AssertEquals(requiredDoc.EQ_DocType, shipment.RequiredDocumentsProvider.RequiredDocuments[0].EQ_DocType);
			AssertEquals(requiredDoc.EQ_DocUsage, shipment.RequiredDocumentsProvider.RequiredDocuments[0].EQ_DocUsage);
			AssertEquals(requiredDoc.EQ_CreditControlDoc, shipment.RequiredDocumentsProvider.RequiredDocuments[0].EQ_CreditControlDoc);
			AssertEquals(requiredDoc.EQ_OriginalDocRequired, shipment.RequiredDocumentsProvider.RequiredDocuments[0].EQ_OriginalDocRequired);
			AssertEquals(JobRequiredDocument.JRDOrigin.FromRequirements, shipment.RequiredDocumentsProvider.RequiredDocuments[0].Origin);

			shipment.RequiredDocumentsProvider.RequiredDocuments.AddAndAcquitRequiredDocuments(buyerSupplierLink, null, null, null, ZDateTime.Now.AddDays(1));
			AssertEquals(2, shipment.RequiredDocumentsProvider.RequiredDocuments.Count);
			AssertEquals(requiredDoc2.EQ_DocType, shipment.RequiredDocumentsProvider.RequiredDocuments[1].EQ_DocType);
			AssertEquals(requiredDoc2.EQ_DocUsage, shipment.RequiredDocumentsProvider.RequiredDocuments[1].EQ_DocUsage);
			AssertEquals(requiredDoc2.EQ_CreditControlDoc, shipment.RequiredDocumentsProvider.RequiredDocuments[1].EQ_CreditControlDoc);
			AssertEquals(requiredDoc2.EQ_OriginalDocRequired, shipment.RequiredDocumentsProvider.RequiredDocuments[1].EQ_OriginalDocRequired);
			AssertEquals(JobRequiredDocument.JRDOrigin.FromRequirements, shipment.RequiredDocumentsProvider.RequiredDocuments[1].Origin);
		}

		JobRequiredDocument GetNewBuyerSupplierRequiredDoc(OrgSupplierBuyerLink buyerSupplierLink, ZString docType, ZString docUsage, ZString period, ZDate recvDate, ZDate date, ZString docNumber)
		{
			JobRequiredDocument result = buyerSupplierLink.RequiredDocuments.AddNew();
			result.EQ_DocCategory = Constants.ReferenceTypes.SupplyChainLogistics;
			result.EQ_DocType = docType;
			result.EQ_DocUsage = docUsage;
			result.EQ_DocPeriod = period;
			result.EQ_DateReceived = recvDate.ToZDateTime().ToDateTimeOffset(null);
			result.EQ_ValidToDate = date;
			result.EQ_DocNumber = docNumber;
			return result;
		}

		RefCountryRequiredDocument GetNewRequiredDoc(RefCountry country, ZString docType, ZString orig, ZString dest, ZString usage, ZString transport, ZBool isConsol, ZBool isShipment, ZBool isBrokerage, ZBool isOrder)
		{
			RefCountryRequiredDocument result = country.RequiredDocuments.AddNew();
			result.RD_DocType = docType;
			result.RD_RN_NKOrigin = orig;
			result.RD_RN_NKDestination = dest;
			result.RD_DocUsage = usage;
			result.RD_TransportMode = transport;
			result.RD_OnConsol = isConsol;
			result.RD_OnShipment = isShipment;
			result.RD_OnBrokerage = isBrokerage;
			result.RD_OnOrder = isOrder;
			return result;
		}

		JobRequiredDocument GetNewJobRequiredDoc(OrgHeader org, ZString docType, ZString docUsage, ZString period, ZDate recvDate, ZDate date, ZString docNumber)
		{
			JobRequiredDocument result = org.RequiredDocuments.AddNew();
			result.EQ_DocCategory = Constants.ReferenceTypes.SupplyChainLogistics;
			result.EQ_DocType = docType;
			result.EQ_DocUsage = docUsage;
			result.EQ_DocPeriod = period;
			result.EQ_DateReceived = recvDate.ToZDateTime().ToDateTimeOffset(null);
			result.EQ_ValidToDate = date;
			result.EQ_DocNumber = docNumber;
			return result;
		}

		#endregion

		#region Implementation

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			IDocsAndCartageParent shipment = (IDocsAndCartageParent)Factory.New<Enterprise.Integration.Forwarding.IForwardingShipment>();
			return shipment.RequiredDocumentsProvider.RequiredDocuments;
		}

		IDocsAndCartageParent Shipment;
		JobRequiredDocumentDependentCollection Docs;

		protected override void SetUp()
		{
			Shipment = (IDocsAndCartageParent)Factory.New<Enterprise.Integration.Forwarding.IForwardingShipment>();
			Docs = Shipment.RequiredDocumentsProvider.RequiredDocuments;
		}

		#endregion
	}
}
