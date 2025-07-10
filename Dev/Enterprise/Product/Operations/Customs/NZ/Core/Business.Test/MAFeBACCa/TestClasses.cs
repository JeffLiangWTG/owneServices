using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Core;
using Enterprise.Customs.NZ.Business.Declaration;
using Enterprise.Customs.NZ.Business.MAFeBACCa.CodedLists;
using Enterprise.Customs.NZ.Business.MAFeBACCa.MessageBuilders;
using Enterprise.Customs.NZ.Registry;
using Enterprise.Customs.NZ.TradeSingleWindow;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.NZ.Business.MAFeBACCa.Testing
{
	public class TestDataBuilder
	{
		public TestDataBuilder(BusinessObjectFactory factory)
		{
			this.factory = factory;
		}

		public TestDataBuilder(JobDeclaration declaration)
		{
			this.declaration = declaration;
			factory = declaration.Factory;
		}
		internal readonly JobDeclaration declaration;
		readonly BusinessObjectFactory factory;

		public void PopulateDeclarationForSea()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			declaration.JE_TransportMode = JobTransportModeList.Codes.Sea;
			declaration.JE_VesselName = "BUNGA DELIMA";
			declaration.JE_VoyageFlightNo = "3599";
			declaration.JE_DateOfArrival = new DateTime(2009, 2, 1);
			declaration.JE_MasterBill = "BR298032";
			declaration.JE_HouseBill = "BOL01010101";
			PopulateDeclaration(false);

			CusContainer container = declaration.CusContainers.AddNew();
			container.CO_ContainerNumber = "OOCL0000023";
			container.CO_FCL_LCL_AIR = ContainerModeList.Codes.FCL;
		}

		public void PopulateDeclarationThatPassesValidation()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			declaration.JE_TransportMode = JobTransportModeList.Codes.Sea;
			declaration.JE_VesselName = "BUNGA DELIMA";
			declaration.JE_VoyageFlightNo = "3599";
			declaration.JE_DateOfArrival = new DateTime(2009, 2, 1);
			declaration.JE_MasterBill = "BR298032";
			declaration.JE_HouseBill = "BOL01010101";
			PopulateDeclaration(false);

			declaration.JE_OH_ShippingLine = declaration.JE_OH_Importer;

			CusContainer container = declaration.CusContainers.AddNew();
			container.CO_ContainerNumber = "OOCL0000023";
			container.CO_MAF_ContainerType = ContainerTypeList.Codes.MoveableCaseL615m;
		}

		public void PopulateConsolThatPassesValidation(ForwardingConsol consol)
		{
			var depot = GetDepot();
			factory.Save();

			SetupCurrentUser();
			GlbBranch.CurrentBranch.GB_RL_NKHomePort = "NZAKL";
			GlbBranch.CurrentBranch.OrgProxy.CustomsCodes.AddNew(OrgCusCode.NZCodeTypes.MAFCoverSheetQE, "AB123");
			consol.JK_TransportMode = JobTransportModeList.Codes.Sea;
			consol.JK_ConsolMode = Constants.ContainerModes.Groupage;
			consol.JK_RL_NKLoadPort = "AUMEL";
			consol.JK_RL_NKDischargePort = "NZCHC";
			consol.JK_MasterBillNum = "BR298032";
			var container = consol.Containers.AddNew();
			container.JC_ContainerNum = "OOCL0000023";
			container.JC_ContainerMode = ContainerModeList.Codes.FCL;

			consol.JK_OA_ReceivingForwarderAddress = GetImporter().MainAddress.PK;
			consol.JK_OA_SendingForwarderAddress = GetExporter().MainAddress.PK;
			consol.JK_OA_UnpackDepotAddress = depot.MainAddress.PK;
			consol.JK_OA_ShippingLineAddress = consol.JK_OA_ReceivingForwarderAddress;

			var transport = consol.Transports.MostInterestingTransport;
			transport.JW_IsLinked = false;
			transport.JW_Vessel = "BUNGA DELIMA";
			transport.JW_VoyageFlight = "3599";
			transport.JW_ATA = new DateTime(2009, 2, 1);

			transport = consol.Transports.AddNew();
			transport.JW_IsLinked = false;
			transport.JW_Vessel = "OTHER VESSEL";
			transport.JW_VoyageFlight = "1234";
			transport.JW_ATA = new DateTime(2010, 2, 1);
			transport.JW_RL_NKLoadPort = "NZCHC";
			transport.JW_RL_NKDiscPort = "NZNPE";

			var shipment = consol.Shipments.AddNew();
			shipment.JS_HouseBill = "BOL01010101";
			shipment.JS_OuterPacks = 12;
			shipment.JS_F3_NKPackType = "BX";
			shipment.JS_RL_NKDestination = "NZNPE";
			shipment.JS_ActualWeight = 10;
			shipment.JS_UnitOfWeight = Constants.Weight.Kilograms;

			shipment = consol.Shipments.AddNew();
			shipment.JS_HouseBill = "BOL01010102";
			shipment.JS_OuterPacks = 12;
			shipment.JS_F3_NKPackType = "BX";
			shipment.JS_RL_NKDestination = "NZNPE";
			shipment.JS_ActualWeight = 12;
			shipment.JS_UnitOfWeight = Constants.Weight.Kilograms;

			consol.DocManagerInfo.AddFileOrDocument(Encoding.ASCII.GetBytes("%PDF <FlatChat> %EOF\n"), "FlatChat.pdf", "FCT");
			var file = GetMAFMessaging(consol).Files.AddNew().Data;
			file.ZF_DocumentType = DocumentTypeList.Codes.ExporterDeclaration;
			file.ZF_EDocsUniqueID = (ZGuid)file.Lookups.AvailableEDocs[0].PK;
			file.ZF_FileName = "FlatChat.pdf";
		}

		public void PopulateAirConsolThatPassesValidation(ForwardingConsol consol)
		{
			var depot = GetDepot();
			factory.Save();

			SetupCurrentUser();
			GlbBranch.CurrentBranch.GB_RL_NKHomePort = "NZAKL";
			GlbBranch.CurrentBranch.OrgProxy.CustomsCodes.AddNew(OrgCusCode.NZCodeTypes.MAFCoverSheetQE, "AB123");
			consol.JK_TransportMode = JobTransportModeList.Codes.Air;
			consol.JK_ConsolMode = Constants.ContainerModes.AIR;
			consol.JK_RL_NKLoadPort = "AUMEL";
			consol.JK_RL_NKDischargePort = "NZCHC";
			consol.JK_MasterBillNum = "08100239487";

			consol.JK_OA_ReceivingForwarderAddress = GetImporter().MainAddress.PK;
			consol.JK_OA_SendingForwarderAddress = GetExporter().MainAddress.PK;
			consol.JK_OA_UnpackDepotAddress = depot.MainAddress.PK;
			consol.JK_OA_ShippingLineAddress = consol.JK_OA_ReceivingForwarderAddress;

			var transport = consol.Transports.MostInterestingTransport;
			transport.JW_IsLinked = false;
			transport.JW_VoyageFlight = "QF18";
			transport.JW_ATA = new DateTime(2017, 05, 29);

			var shipment = consol.Shipments.AddNew();
			shipment.JS_HouseBill = "BOL01010101";
			shipment.JS_OuterPacks = 12;
			shipment.JS_F3_NKPackType = "BX";
			shipment.JS_RL_NKDestination = "NZNPE";
			shipment.JS_ActualWeight = 10;
			shipment.JS_UnitOfWeight = Constants.Weight.Kilograms;

			shipment = consol.Shipments.AddNew();
			shipment.JS_HouseBill = "BOL01010102";
			shipment.JS_OuterPacks = 12;
			shipment.JS_F3_NKPackType = "BX";
			shipment.JS_RL_NKDestination = "NZNPE";
			shipment.JS_ActualWeight = 12;
			shipment.JS_UnitOfWeight = Constants.Weight.Kilograms;

			consol.DocManagerInfo.AddFileOrDocument(Encoding.ASCII.GetBytes("%PDF <FlatChat> %EOF\n"), "FlatChat.pdf", "FCT");
			var file = GetMAFMessaging(consol).Files.AddNew().Data;
			file.ZF_DocumentType = DocumentTypeList.Codes.ExporterDeclaration;
			file.ZF_EDocsUniqueID = (ZGuid)file.Lookups.AvailableEDocs[0].PK;
			file.ZF_FileName = "FlatChat.pdf";
		}

		public void PopulateDeclarationForAirWithTwoInvoiceLines()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			declaration.JE_TransportMode = JobTransportModeList.Codes.Air;
			declaration.JE_VoyageFlightNo = "QF253";
			declaration.JE_DateOfArrival = new DateTime(2009, 2, 2);
			declaration.JE_MasterBill = "081-11111111";
			declaration.JE_HouseBill = "HB01010101";
			PopulateDeclaration(true);
		}

		public void PopulateDeclarationForAir()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			declaration.JE_TransportMode = JobTransportModeList.Codes.Air;
			declaration.JE_VoyageFlightNo = "QF253";
			declaration.JE_DateOfArrival = new DateTime(2009, 2, 2);
			declaration.JE_MasterBill = "081-11111111";
			declaration.JE_HouseBill = "HB01010101";
			PopulateDeclaration(false);
		}

		void PopulateDeclaration(bool includeSecondInvoiceLine)
		{
			SetupCurrentUser();

			var branch = factory.New<GlbBranch>();
			branch.GB_GC = GlbCompany.CurrentCompany.PK;
			branch.GB_BranchName = "CARGOWISE BROKERS ZZZ";
			branch.GB_Address1 = "72 BROKER STREET";
			branch.GB_Address2 = "BROKERS TOWER";
			branch.GB_City = "BROKERVILLE";
			branch.GB_RL_NKHomePort = "NZAKL";
			branch.GB_PostCode = "2015";
			declaration.JE_GB = branch.PK;

			declaration.JE_OH_Importer = GetImporter().PK;
			declaration.JE_OH_Supplier = GetExporter().PK;
			declaration.JE_RL_NKOrigin = "AUMEL";
			declaration.JE_RL_NKPortOfArrival = "NZCHC";
			declaration.JE_RL_NKFinalDestination = "NZNPE";
			declaration.JE_GoodsDescription = "UNQUALIFIED GROMMETS";
			declaration.JE_TotalNoOfPacks = 12;
			declaration.JE_TotalNoOfPacksPackType = "BX";
			declaration.OtherInfos.AddNew(HeaderOtherInfoList.Codes.PrivateImportTransactionFee, "");

			CusEntryHeader entryHeader = declaration.CusEntryHeader;
			entryHeader.EntryNumber = "72334537";

			declaration.DepotDocAddress.E2_OA_Address = GetDepot().MainAddress.PK;

			OrgHeader treatmentProvider = GetNewOrganisation(
				"TREATMENT PROVIDER",
				"15 TREATMENT ROAD",
				"BACK OF TREATMENT",
				"TREATMENT",
				"2323", "NZ$X$");
			declaration.TreatmentProviderDocAddress.E2_OA_Address = treatmentProvider.MainAddress.PK;

			JobComInvoiceHeader invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			invoiceHeader.JZ_InvoiceNumber = "INV1230984";
			invoiceHeader.JZ_InvoiceAmount = 1000.00m;
			invoiceHeader.JZ_RX_NKInvoice_Currency = "NZD";
			invoiceHeader.JZ_IncoTerm = IncoTermList.Codes.FreeOnBoard;

			JobComInvoiceLine invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "4011.10.09.11E";
			invoiceLine.JI_Description = "STEEL BELTED RADIAL TYRES";
			invoiceLine.JI_CountryOfOrigin = "AU";
			invoiceLine.JI_QualifiesForPreferentialDuty = QualifiesForPreferentialDutyList.Codes.Qualifies;
			invoiceLine.JI_RN_NKCountryOfExport = "AU";
			invoiceLine.JI_LinePrice = 1000.00m;
			invoiceLine.JI_Volume = 12;
			invoiceLine.JI_VolumeUQ = Constants.Volume.CubicMetres;
			invoiceLine.JI_Weight = 120;
			invoiceLine.JI_WeightUQ = Constants.Weight.Kilograms;

			if (includeSecondInvoiceLine)
			{
				JobComInvoiceLine invoiceLine2 = invoiceHeader.JobComInvoiceLines.AddNew();
				invoiceLine2.JI_Tariff = "6905.10.00.00F";
				invoiceLine2.JI_Description = "CERAMIC ROOFING TILES";
				invoiceLine2.JI_CountryOfOrigin = "AU";
				invoiceLine2.JI_QualifiesForPreferentialDuty = QualifiesForPreferentialDutyList.Codes.NonQualifying;
				invoiceLine2.JI_RN_NKCountryOfExport = "AU";
				invoiceLine2.JI_LinePrice = 0;
				invoiceLine2.JI_Volume = 5;
				invoiceLine2.JI_VolumeUQ = Constants.Volume.TeaChest;
			}

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var eDoc = declaration.DocManagerInfo.AddFileOrDocument(Encoding.ASCII.GetBytes("%PDF <FlatChat> %EOF\n"), "FlatChat.pdf", "FCT");
			MAFFile file = GetMAFMessaging(declaration).Files.AddNew().Data;
			file.ZF_DocumentType = DocumentTypeList.Codes.ExporterDeclaration;
			file.ZF_EDocsUniqueID = (ZGuid)file.Lookups.AvailableEDocs[0].PK;
			file.ZF_FileName = "FlatChat.pdf";
		}

		internal void SetupCurrentUser()
		{
			var currentCompany = GlbCompany.CurrentCompany;
			currentCompany.GC_Name = "CARGOWISE BROKERS";

			var branch = GlbBranch.CurrentBranch;
			branch.GB_GC = currentCompany.PK;
			branch.GB_BranchName = "CARGOWISE BROKERS ZZZ";
			branch.GB_Address1 = "72 BROKER STREET";
			branch.GB_Address2 = "BROKERS TOWER";
			branch.GB_City = "BROKERVILLE";
			branch.GB_RL_NKHomePort = "NZAKL";
			branch.GB_PostCode = "2015";

			NZCustomsDataRegistry.Instance.NZBrokerageID.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "00009915B");
			Env.Registry.MailboxEmailAddress = "cargowiseone@brokers.cargowise.com";

			var currentUser = GlbStaff.CurrentUser;
			currentUser.GS_FullName = "Sidney allaballah Broker";
			currentUser.GS_EmailAddress = "broker@brokers.cargowise.com";
			currentUser.GS_FaxNum = "99887766";
			currentUser.GS_WorkPhone = "44556677";
		}

		OrgHeader GetDepot()
		{
			return GetNewOrganisation(
				"TRANSITIONAL FACILITY",
				"45 TRANSITIONAL ROAD",
				"FACILITY TOPS",
				"TRANSITIONAL",
				"4389", "NZWPW",
				OrgCusCode.NZCodeTypes.ApprovedTransitionalFacility, "TF1");
		}

		OrgHeader GetExporter()
		{
			return GetNewOrganisation(
				"SUPPLIER PTY LTD",
				"98 SUPPLIER CIRCUIT",
				"SUPPLIERHAVEN",
				"SUPPLIER HILL",
				"2234", "AUMEL",
				OrgCusCode.CodeTypes.SupplierCode, "00998877Z",
				"Major", "Exporter", "major.exporter@exporter.com.au", "12345678", "876543211");
		}

		OrgHeader GetImporter()
		{
			var importer = GetNewOrganisation(
				"IMPORTER INCORPORATED",
				"77 IMPORTER AVENUE",
				"IMPORTER SPIRE",
				"IMPORTERVILLE",
				"0232", "NZNPE",
				OrgCusCode.CodeTypes.CustomsClientCode, "00112233F",
				"Test", "Importer", "importer@importer.co.nz", "11111111", "22222222");
			importer.CustomsCodes.AddNew(OrgCusCode.NZCodeTypes.MAFCoverSheetQE, "AB123");
			return importer;
		}

		OrgHeader GetNewOrganisation(string organisationName, string addressLine1, string addressLine2, string city, string postalCode, string unloco)
		{
			return GetNewOrganisation(organisationName, addressLine1, addressLine2, city, postalCode, unloco, null, null);
		}

		OrgHeader GetNewOrganisation(string organisationName, string addressLine1, string addressLine2, string city, string postalCode, string unloco, string customsCodeType, string customsCode)
		{
			return GetNewOrganisation(organisationName, addressLine1, addressLine2, city, postalCode, unloco, customsCodeType, customsCode, null, null, null, null, null);
		}

		internal OrgHeader GetNewOrganisation(string organisationName, string addressLine1, string addressLine2, string city, string postalCode, string unloco, string customsCodeType, string customsCode, string contactFirstName, string contactLastName, string email, string fax, string phone)
		{
			OrgHeader organisation = factory.NewWithValidTestData<OrgHeader>();
			organisation.OH_FullName = organisationName;
			OrgAddress address = organisation.MainAddress;
			address.OA_Address1 = addressLine1;
			address.OA_Address2 = addressLine2;
			address.OA_City = city;
			address.OA_RL_NKRelatedPortCode = unloco;
			address.OA_PostCode = postalCode;

			if (contactFirstName != null)
			{
				OrgContact contact = organisation.Contacts.AddNew();
				contact.OC_ContactName = contactFirstName + " ABDULLAH " + contactLastName;
				contact.OC_Email = email;
				contact.OC_Fax = fax;
				contact.OC_Phone = phone;
				OrgDocument document = contact.Documents.AddNew();
				document.OD_DocumentGroup = "ALL";
				document.OD_DefaultContact = true;
			}

			if (customsCodeType != null)
			{
				organisation.CustomsCodes.AddNew(customsCodeType, customsCode);
			}

			if (organisation.OH_Code.IsEmpty)
			{
				organisation.OH_Code = organisation.OH_FullName.Left(5).Trim() + address.OA_RL_NKRelatedPortCode;
			}
			return organisation;
		}

		internal static void SetupAddressContactDetails(OrgAddress orgAddress, string phone, string fax, string email)
		{
			orgAddress.OA_Phone = phone;
			orgAddress.OA_Fax = fax;
			orgAddress.OA_Email = email;
		}

		internal static void AddContact(OrgHeader ogranisation, ContactType contactType, string contactName, string contactPhone, string contactFax, string contactEmail)
		{
			var contact = ogranisation.Contacts.AddNew();
			contact.OC_ContactName = contactName;
			contact.OC_Phone = contactPhone;
			contact.OC_Fax = contactFax;
			contact.OC_Email = contactEmail;

			var contactDocument = contact.Documents.AddNew();
			contactDocument.OD_DocumentGroup = contactType.Code;
			contactDocument.OD_DefaultContact = true;
		}

		public static MAFMessagingBO GetMAFMessaging(JobDeclaration declaration)
		{
			return new MAFMessagingBO(new MAFPlugInSupportDeclarationWrapper(declaration));
		}

		public static MAFMessagingBO GetMAFMessaging(ForwardingConsol consol)
		{
			return new MAFMessagingBO(new MAFPlugInSupportConsolWrapper(consol));
		}
	}

	public class eDocsCollectionForTesting : IStorageDocsBaseCollection
	{
		public eDocsCollectionForTesting(params IeDoc[] elements)
		{
			docs = new List<IeDoc>();
			if (elements != null)
			{
				docs.AddRange(elements);
			}
		}

		readonly List<IeDoc> docs;

		public void Add(IeDoc elementToAdd)
		{
			docs.Add(elementToAdd);
		}

		public bool Contains(IeDoc element)
		{
			return docs.Contains(element);
		}

		public int Count
		{
			get { return docs.Count; }
		}

		public IeDoc GetFromUniqueKey(Guid uniqueKey)
		{
			return docs.Find(doc => doc.UniqueKey == uniqueKey);
		}

		public IeDoc GetMostRecentEDoc(string docType)
		{
			return null;
		}

		public void Remove(IeDoc elementToRemove)
		{
		}

		public bool ContainsDocType(ZString docType)
		{
			return false;
		}

		public IeDoc this[int index]
		{
			get { return docs[index]; }
		}

		public IEnumerator GetEnumerator()
		{
			return docs.GetEnumerator();
		}
	}

	sealed public class eDocForTesting : IeDoc
	{
		public eDocForTesting(ZString docType, ZString fileName, ZString description, ZDateTime dateAdded)
		{
			UniqueKey = ZGuid.NewZGuid();
			DocType = docType;
			FileName = fileName;
			Description = description;
			DateAdded = dateAdded;
		}

		public ZDateTime DateAdded
		{
			get;
			set;
		}

		public ZString Description
		{
			get;
			set;
		}

		public ZString DocType
		{
			get;
			set;
		}

		public ZString DocSourceDescription
		{
			get { return "Description"; }
			set { }
		}

		public ZString DocSource
		{
			get;
			set;
		}

		public CodeDescriptionPairList DocType_List
		{
			get { return new CodeDescriptionPairList(); }
		}

		public ZString FileName
		{
			get;
			private set;
		}

		public ZBlob ImageData
		{
			get;
			set;
		}

		public ZBool IsDeleted
		{
			get;
			set;
		}

		public ZBool IsPublished
		{
			get;
			set;
		}

		public ZString VisibleCompanyCode
		{
			get { return string.Empty; }
		}

		public ZString VisibleBranchCode
		{
			get { return string.Empty; }
		}

		public ZString VisibleDepartmentCode
		{
			get { return string.Empty; }
		}

		public ZBool IsSystemGenerated
		{
			get { return false; }
		}

		public ZDateTime LastEdited
		{
			get { return DateAdded.AddDays(1); }
		}

		public ZString LastEditedUser
		{
			get { return GlbStaff.CurrentUser.GS_Code; }
		}

		public void NotifyReadByUser()
		{
		}

		public ZBool IsCustomisableDocTypes
		{
			get { return false; }
		}

		public ZGuid UniqueKey
		{
			get;
			private set;
		}

		public BusinessObject ParentMain
		{
			get
			{
				return null;
			}
		}

		public void SetValuesForTest(ZDateTime dateTime, ZString dataType)
		{
		}

		public ZString DataType
		{
			get
			{
				return null;
			}
		}

		public ZString FileNameOnly
		{
			get
			{
				return null;
			}
		}

		public ZDecimal FileSizeInMB
		{
			get
			{
				return ZDecimal.Zero;
			}
		}

		public IDisposable OpenForEdit()
		{
			throw new NotImplementedException();
		}

		public Stream GetImageDataReader() => new CargoWise.IO.Shim.SubStreamableStream();

		public void SetImageDataStream(Stream stream) { }

		public string CreateReference()
		{
			throw new NotImplementedException();
		}

		public void Delete()
		{
			throw new NotImplementedException();
		}
	}

	public class LoggerForTesting : ILogger
	{
		readonly ZStringBuilder result = new ZStringBuilder();

		public void Log(LogType type, string message, Exception ex)
		{
			Log(type, message);
			result.Append("    Exception:- " + ex.Message);
		}

		public void Log(LogType type, string message)
		{
			result.Append(type.ToString() + ":- " + message);
		}

		public override string ToString()
		{
			return result.ToStringWithNewLineBetweenAppends();
		}

		public LoggingInformation OldLogger
		{
			get { return oldLogger ?? (oldLogger = GetNewLoggingInformation()); }
		}
		LoggingInformation oldLogger;

		LoggingInformation GetNewLoggingInformation()
		{
			LoggingInformation logger = new LoggingInformation();
			logger.OnLogInfoAdded += new LoggingInformation.LogInfoAdded(OldLogger_OnLogInfoAdded);
			return logger;
		}

		void OldLogger_OnLogInfoAdded(string log, LogType logType)
		{
			Log(logType, log);
		}
	}

	public static class SentMessages
	{
		public const string OriginalAirMessage = @"
<?xml version=""1.0"" encoding=""utf-8""?>
<maf:MessagingRequest xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:maf=""http://www.maf.govt.nz/Messaging/Request/2008/03"">
	<maf:Header>
		<maf:ApplicationName>EBACCA</maf:ApplicationName>
		<maf:ApplicationVersion>1.0.0</maf:ApplicationVersion>
		<maf:DocumentType>EBACCA</maf:DocumentType>
		<maf:Sender>
			<maf:Name>CARGOWISE BROKERS</maf:Name>
			<maf:EndPointType>Email</maf:EndPointType>
			<maf:Address>cargowiseone@brokers.cargowise.com</maf:Address>
		</maf:Sender>
		<maf:CallerRefID>1</maf:CallerRefID>
	</maf:Header>
	<maf:Body>
		<maf:MetaData>
			<ebacca:EBACCARequest xmlns:ebacca=""http://www.maf.govt.nz/EBACCA/Messaging/Request/2008/03/"">
				<ebacca:Broker>
					<ebacca:OrganisationCode>00009915B</ebacca:OrganisationCode>
					<ebacca:OrganisationName>CARGOWISE BROKERS</ebacca:OrganisationName>
					<ebacca:Address>
						<ebacca:AddressLine1>72 BROKER STREET</ebacca:AddressLine1>
						<ebacca:AddressLine2>BROKERS TOWER</ebacca:AddressLine2>
						<ebacca:City>BROKERVILLE</ebacca:City>
						<ebacca:PostalCode>2015</ebacca:PostalCode>
						<ebacca:Country>NZ</ebacca:Country>
					</ebacca:Address>
					<ebacca:ContactMethods>
						<ebacca:ContactMethod>
							<ebacca:ContactMethodType>Phone</ebacca:ContactMethodType>
							<ebacca:ContactMethodValue>44556677</ebacca:ContactMethodValue>
						</ebacca:ContactMethod>
						<ebacca:ContactMethod>
							<ebacca:ContactMethodType>Fax</ebacca:ContactMethodType>
							<ebacca:ContactMethodValue>99887766</ebacca:ContactMethodValue>
						</ebacca:ContactMethod>
						<ebacca:ContactMethod>
							<ebacca:ContactMethodType>EMail</ebacca:ContactMethodType>
							<ebacca:ContactMethodValue>broker@brokers.cargowise.com</ebacca:ContactMethodValue>
						</ebacca:ContactMethod>
					</ebacca:ContactMethods>
					<ebacca:ContactPerson>
						<ebacca:FirstName>Sidney</ebacca:FirstName>
						<ebacca:LastName>Broker</ebacca:LastName>
					</ebacca:ContactPerson>
				</ebacca:Broker>
				<ebacca:Importer>
					<ebacca:OrganisationCode>00112233F</ebacca:OrganisationCode>
					<ebacca:OrganisationName>IMPORTER INCORPORATED</ebacca:OrganisationName>
					<ebacca:Address>
						<ebacca:AddressLine1>77 IMPORTER AVENUE</ebacca:AddressLine1>
						<ebacca:AddressLine2>IMPORTER SPIRE</ebacca:AddressLine2>
						<ebacca:City>IMPORTERVILLE</ebacca:City>
						<ebacca:PostalCode>0232</ebacca:PostalCode>
						<ebacca:Country>NZ</ebacca:Country>
					</ebacca:Address>
					<ebacca:ContactMethods>
						<ebacca:ContactMethod>
							<ebacca:ContactMethodType>Phone</ebacca:ContactMethodType>
							<ebacca:ContactMethodValue>22222222</ebacca:ContactMethodValue>
						</ebacca:ContactMethod>
						<ebacca:ContactMethod>
							<ebacca:ContactMethodType>Fax</ebacca:ContactMethodType>
							<ebacca:ContactMethodValue>11111111</ebacca:ContactMethodValue>
						</ebacca:ContactMethod>
						<ebacca:ContactMethod>
							<ebacca:ContactMethodType>EMail</ebacca:ContactMethodType>
							<ebacca:ContactMethodValue>importer@importer.co.nz</ebacca:ContactMethodValue>
						</ebacca:ContactMethod>
					</ebacca:ContactMethods>
					<ebacca:ContactPerson>
						<ebacca:FirstName>Test</ebacca:FirstName>
						<ebacca:LastName>Importer</ebacca:LastName>
					</ebacca:ContactPerson>
				</ebacca:Importer>
				<ebacca:Exporter>
					<ebacca:OrganisationCode>00998877Z</ebacca:OrganisationCode>
					<ebacca:OrganisationName>SUPPLIER PTY LTD</ebacca:OrganisationName>
					<ebacca:Address>
						<ebacca:AddressLine1>98 SUPPLIER CIRCUIT</ebacca:AddressLine1>
						<ebacca:AddressLine2>SUPPLIERHAVEN</ebacca:AddressLine2>
						<ebacca:City>SUPPLIER HILL</ebacca:City>
						<ebacca:PostalCode>2234</ebacca:PostalCode>
						<ebacca:Country>AU</ebacca:Country>
					</ebacca:Address>
					<ebacca:ContactMethods>
						<ebacca:ContactMethod>
							<ebacca:ContactMethodType>Phone</ebacca:ContactMethodType>
							<ebacca:ContactMethodValue>876543211</ebacca:ContactMethodValue>
						</ebacca:ContactMethod>
						<ebacca:ContactMethod>
							<ebacca:ContactMethodType>Fax</ebacca:ContactMethodType>
							<ebacca:ContactMethodValue>12345678</ebacca:ContactMethodValue>
						</ebacca:ContactMethod>
						<ebacca:ContactMethod>
							<ebacca:ContactMethodType>EMail</ebacca:ContactMethodType>
							<ebacca:ContactMethodValue>major.exporter@exporter.com.au</ebacca:ContactMethodValue>
						</ebacca:ContactMethod>
					</ebacca:ContactMethods>
					<ebacca:ContactPerson>
						<ebacca:FirstName>Major</ebacca:FirstName>
						<ebacca:LastName>Exporter</ebacca:LastName>
					</ebacca:ContactPerson>
				</ebacca:Exporter>
				<ebacca:Details>
					<ebacca:ConsignmentType>PrivateCargo</ebacca:ConsignmentType>
					<ebacca:Shipment>
						<ebacca:OriginCountry>AU</ebacca:OriginCountry>
						<ebacca:DischargePorts>
							<ebacca:DischargePort>NZCHC</ebacca:DischargePort>
						</ebacca:DischargePorts>
						<ebacca:Destinations>
							<ebacca:Destination>NZNPE</ebacca:Destination>
						</ebacca:Destinations>
						<ebacca:Flight>
							<ebacca:FlightNumber>QF253</ebacca:FlightNumber>
							<ebacca:FlightArrivalDate>2009-02-02</ebacca:FlightArrivalDate>
						</ebacca:Flight>
						<ebacca:Identifiers>
							<ebacca:BillOfLadings>
								<ebacca:BillOfLadingNumber>08111111111</ebacca:BillOfLadingNumber>
							</ebacca:BillOfLadings>
							<ebacca:SubBillOfLadings>
								<ebacca:SubBillOfLadingNumber>HB01010101</ebacca:SubBillOfLadingNumber>
							</ebacca:SubBillOfLadings>
						</ebacca:Identifiers>
					</ebacca:Shipment>
					<ebacca:ConsignmentDescription>UNQUALIFIED GROMMETS</ebacca:ConsignmentDescription>
					<ebacca:ConsignmentMeasurement>
						<ebacca:MeasurementUnitQualifer>box</ebacca:MeasurementUnitQualifer>
						<ebacca:MeasurementValue>12</ebacca:MeasurementValue>
					</ebacca:ConsignmentMeasurement>
					<ebacca:Commodities>
						<ebacca:Commodity>
							<ebacca:GoodsType>TYR</ebacca:GoodsType>
							<ebacca:GoodsDescription>STEEL BELTED RADIAL TYRES</ebacca:GoodsDescription>
							<ebacca:GoodsMeasurements>
								<ebacca:GoodsMeasurement>
									<ebacca:MeasurementUnitQualifer>kilograms</ebacca:MeasurementUnitQualifer>
									<ebacca:MeasurementValue>120</ebacca:MeasurementValue>
								</ebacca:GoodsMeasurement>
							</ebacca:GoodsMeasurements>
							<ebacca:IsNew>true</ebacca:IsNew>
							<ebacca:TariffCodes>
								<ebacca:TariffCode>4011100911E</ebacca:TariffCode>
							</ebacca:TariffCodes>
						</ebacca:Commodity>
					</ebacca:Commodities>
					<ebacca:MAFProcessingOffice>Christchurch</ebacca:MAFProcessingOffice>
				</ebacca:Details>
				<ebacca:References>
					<ebacca:Customs>
						<ebacca:EntryNumber>72334537</ebacca:EntryNumber>
						<ebacca:EntryType>IE</ebacca:EntryType>
					</ebacca:Customs>
					<ebacca:ClientReferenceNumber>B00001000</ebacca:ClientReferenceNumber>
				</ebacca:References>
				<ebacca:PaymentDetails>
					<ebacca:Account>
						<ebacca:AccountHolderName>IMPORTER INCORPORATED</ebacca:AccountHolderName>
						<ebacca:AccountNumber>AB123</ebacca:AccountNumber>
					</ebacca:Account>
				</ebacca:PaymentDetails>
				<ebacca:TransitionalFacility>
					<ebacca:OrganisationCode>TF1</ebacca:OrganisationCode>
					<ebacca:OrganisationName>TRANSITIONAL FACILITY</ebacca:OrganisationName>
					<ebacca:Address>
						<ebacca:AddressLine1>45 TRANSITIONAL ROAD</ebacca:AddressLine1>
						<ebacca:AddressLine2>FACILITY TOPS</ebacca:AddressLine2>
						<ebacca:City>TRANSITIONAL</ebacca:City>
						<ebacca:PostalCode>4389</ebacca:PostalCode>
						<ebacca:Country>NZ</ebacca:Country>
					</ebacca:Address>
				</ebacca:TransitionalFacility>
				<ebacca:TreatmentProvider>
					<ebacca:OrganisationName>TREATMENT PROVIDER</ebacca:OrganisationName>
					<ebacca:Address>
						<ebacca:AddressLine1>15 TREATMENT ROAD</ebacca:AddressLine1>
						<ebacca:AddressLine2>BACK OF TREATMENT</ebacca:AddressLine2>
						<ebacca:City>TREATMENT</ebacca:City>
						<ebacca:PostalCode>2323</ebacca:PostalCode>
						<ebacca:Country>NZ</ebacca:Country>
					</ebacca:Address>
				</ebacca:TreatmentProvider>
			</ebacca:EBACCARequest>
		</maf:MetaData>
		<maf:Files>
			<maf:File>
				<maf:FileName>FlatChat.pdf</maf:FileName>
				<DocumentType>ExporterDeclaration</DocumentType>
				<maf:ContentType>PDF</maf:ContentType>
				<maf:Data>
					<maf:DataEncoding>Base64</maf:DataEncoding>
					<maf:DataContent>JVBERiA8RmxhdENoYXQ+ICVFT0YK</maf:DataContent>
				</maf:Data>
			</maf:File>
		</maf:Files>
	</maf:Body>
</maf:MessagingRequest>
";
	}

	public static class ResponseMessages
	{
		public const string AcknowledgementASCII = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<EBACCANotification
    xmlns=""http://www.maf.govt.nz/EBACCA/Messaging/Notification/2008/03/""
    xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
    <MessageType>Error</MessageType>
    <References>
        <CallerRefID>54</CallerRefID>
        <ReceiptNumber/>
        <ConsignmentNumber/>
        <OrganisationCode>00009917B</OrganisationCode>
    </References>
    <Description xmlns=""""> The 'http://www.maf.govt.nz/EBACCA/Messaging/Request/2008/03/:BillOfLadingNumber' element is invalid - The value '' is invalid according to its datatype 'String' - The actual length is less than the MinLength value. ---&gt; System.Xml.Schema.XmlSchemaException: The actual length is less than the MinLength value.
   --- End of inner exception stack trace ---
   </Description>
</EBACCANotification>";

		public const string AcknowledgementXML = @"<?xml version=""1.0"" encoding=""utf-16""?>
<MessagingResponse xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns=""http://www.maf.govt.nz/Messaging/Response/2008/03"">
	<CallerRefID>1</CallerRefID>
	<ReceiptNumber>QGKJCF66Q6</ReceiptNumber>
	<MetaData>
		<ebacca:EBACCAResponse xmlns:ebacca=""http://www.maf.govt.nz/EBACCA/Messaging/Response/2008/03/"">
			<ebacca:OrganisationCode>00009917B</ebacca:OrganisationCode>
		</ebacca:EBACCAResponse>
	</MetaData>
</MessagingResponse>";
		public const string AcknowledgementXMLNo2 = @"<?xml version=""1.0"" encoding=""utf-16""?>
<MessagingResponse xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns=""http://www.maf.govt.nz/Messaging/Response/2008/03"">
	<CallerRefID>2</CallerRefID>
	<ReceiptNumber>QGKJCF66Q6</ReceiptNumber>
	<MetaData>
		<ebacca:EBACCAResponse xmlns:ebacca=""http://www.maf.govt.nz/EBACCA/Messaging/Response/2008/03/"">
			<ebacca:OrganisationCode>00009917B</ebacca:OrganisationCode>
		</ebacca:EBACCAResponse>
	</MetaData>
</MessagingResponse>";

		public const string ErrorDuplicateXML = @"<?xml version=""1.0"" encoding=""utf-16""?>
<EBACCANotificationType xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns=""http://www.maf.govt.nz/EBACCA/Messaging/Notification/2008/03/"">
	<ExtensionData />
	<MessageType>Error</MessageType>
	<References>
		<ExtensionData />
		<CallerRefID>1</CallerRefID>
		<ReceiptNumber>E95HBQPPOJ</ReceiptNumber>
		<OrganisationCode>00009917B</OrganisationCode>
	</References>
	<Description xmlns="""">Duplicate application detected. Original application receipt number: LX9BBGSF7O</Description>
</EBACCANotificationType>";

		public const string ErrorResupplyXML = @"<?xml version=""1.0"" encoding=""utf-16""?>
<EBACCANotificationType xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns=""http://www.maf.govt.nz/EBACCA/Messaging/Notification/2008/03/"">
	<ExtensionData/>
	<MessageType>Error</MessageType>
	<References>
		<ExtensionData/>
		<CallerRefID>1</CallerRefID>
		<ReceiptNumber>3KSKDXS92D</ReceiptNumber>
		<OrganisationCode>00009917B</OrganisationCode>
	</References>
	<Description xmlns="""">The Receipt number included in this eBACCa MessageRequest TL21SK2MED, refers to an errored version on this application. Please resupply this as a new application.</Description>
</EBACCANotificationType>";

		public const string ErrorResupplyXMLWithTypeSuffixAndReferencesElementRemoved = @"<?xml version=""1.0"" encoding=""utf-16""?>
<EBACCANotification xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns=""http://www.maf.govt.nz/EBACCA/Messaging/Notification/2008/03/"">
	<MessageType>Error</MessageType>
	<References>
		<CallerRefID>1</CallerRefID>
		<ReceiptNumber>3KSKDXS92D</ReceiptNumber>
		<OrganisationCode>00009917B</OrganisationCode>
	</References>
	<Description xmlns="""">The Receipt number included in this eBACCa MessageRequest TL21SK2MED, refers to an errored version on this application. Please resupply this as a new application.</Description>
</EBACCANotification>";

		public const string RequestMoreInfoXML = @"<?xml version=""1.0"" encoding=""utf-16""?>
<EBACCANotificationType xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns=""http://www.maf.govt.nz/EBACCA/Messaging/Notification/2008/03/"">
	<ExtensionData/>
	<MessageType>RequestMoreInfo</MessageType>
	<References>
		<ExtensionData/>
		<CallerRefID>1</CallerRefID>
		<ReceiptNumber>EB4P9WXXL9</ReceiptNumber>
		<ConsignmentNumber>B2008/278199</ConsignmentNumber>
		<OrganisationCode>00009917B</OrganisationCode>
	</References>
	<Description xmlns="""">Please provide more info on exactly what is in the parcel. What is the colon cleanser?</Description>
</EBACCANotificationType>";

		public const string RequestMoreInfoXMLNo2 = @"<?xml version=""1.0"" encoding=""utf-16""?>
<EBACCANotificationType xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns=""http://www.maf.govt.nz/EBACCA/Messaging/Notification/2008/03/"">
	<ExtensionData/>
	<MessageType>RequestMoreInfo</MessageType>
	<References>
		<ExtensionData/>
		<CallerRefID>2</CallerRefID>
		<ReceiptNumber>EB4P9WXXL9</ReceiptNumber>
		<ConsignmentNumber>B2008/278199</ConsignmentNumber>
		<OrganisationCode>00009917B</OrganisationCode>
	</References>
	<Description xmlns="""">Please provide more info on exactly what is in the parcel. What is the colon cleanser?</Description>
</EBACCANotificationType>";

		public const string NotifyCRNMessageXML = @"<?xml version=""1.0"" encoding=""utf-16""?>
<EBACCANotificationType xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns=""http://www.maf.govt.nz/EBACCA/Messaging/Notification/2008/03/"">
	<ExtensionData/>
	<MessageType>NotifyCRN</MessageType>
	<References>
		<ExtensionData/>
		<CallerRefID>121</CallerRefID>
		<ReceiptNumber>GPEFGSBQDB</ReceiptNumber>
		<ConsignmentNumber>B2008/279885</ConsignmentNumber>
		<OrganisationCode>00009917B</OrganisationCode>
	</References>
	<Description xmlns=""""/>
</EBACCANotificationType>";

		public const string CancellationXML = @"<?xml version=""1.0"" encoding=""utf-16""?>
<EBACCANotificationType xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns=""http://www.maf.govt.nz/EBACCA/Messaging/Notification/2008/03/"">
	<ExtensionData/>
	<MessageType>Cancellation</MessageType>
	<References>
		<ExtensionData/>
		<CallerRefID>1</CallerRefID>
		<ReceiptNumber>DJ34IKXS9F</ReceiptNumber>
		<OrganisationCode>00009917B</OrganisationCode>
	</References>
</EBACCANotificationType>";

		public const string ErrorDuplicateXMLAllOnOneLine = @"<?xml version=""1.0"" encoding=""utf-16""?><EBACCANotificationType xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns=""http://www.maf.govt.nz/EBACCA/Messaging/Notification/2008/03/""><ExtensionData /><MessageType>Error</MessageType><References><ExtensionData /><CallerRefID>1</CallerRefID><ReceiptNumber>E95HBQPPOJ</ReceiptNumber><OrganisationCode>00009917B</OrganisationCode></References><Description xmlns="""">Duplicate application detected. Original application receipt number: LX9BBGSF7O</Description></EBACCANotificationType>";

		public const string InvalidFormatMessage1 = @"<?xml version=""1.0"" encoding=""utf-16""?>??<MessagingResponse xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns=""http://www.maf.govt.nz/Messaging/Response/2008/03"">??  <CallerRefID>1</CallerRefID>??  <ReceiptNumber>QUEGT7TU9B</ReceiptNumber>??  <MetaData>??    <ebacca:EBACCAResponse xmlns:ebacca=""http://www.maf.govt.nz/EBACCA/Messaging/Response/2008/03/"">??      <ebacca:OrganisationCode>40238120F</ebacca:OrganisationCode>??    </ebacca:EBACCAResponse>??  </MetaData>??</MessagingResponse>?";
		public const string InvalidFormatMessage2 = @"<?xml version=""1.0"" encoding=""utf-16""?><EBACCANotification xmlns:i=""http://www.w3.org/2001/XMLSchema-instance"" xmlns=""http://www.maf.govt.nz/EBACCA/Messaging/Notification/2008/03/""><MessageType>NotifyCRN</MessageType><References><CallerRefID>1</CallerRefID><ReceiptNumber>QUEGT7TU9B</ReceiptNumber><ConsignmentNumber>C2012/25451</ConsignmentNumber><OrganisationCode>40238120F</OrganisationCode></References></EBACCANotification>?";
	}

	public abstract class TestCaseForMessageTesting : TestCaseWithFactory
	{
		protected NZMMessage GetReceivedMessage(string messageText, string messageNo = "")
		{
			NZMMessage message = Factory.New<NZMMessage>();
			message.EM_IsTestMessage = true;
			message.EM_MessageNum = messageNo;
			message.EM_ReceiveTransmit = NZMMessage.Direction.Receive;
			message.EM_Status = NZMMessage.Status.Queued;
			message.EM_MessageType = ZString.Empty;
			message.EM_MessageSubType = ZString.Empty;
			message.EM_MessageText = messageText;
			return message;
		}

		protected JobDeclaration GetDeclarationWithSentMessage()
		{
			return GetDeclarationWithSentMessage("1", "");
		}

		protected JobDeclaration GetDeclarationWithSentMessage(string messageNumber, string userCode = "")
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_GB = Branch.PK;
			var mafMessaging = TestDataBuilder.GetMAFMessaging(declaration);
			mafMessaging.ZX_MessagingStatus = MessagingStatusList.Codes.SentAndAcknowledged;
			mafMessaging.ZX_ReceiptNumber = "QGKJCF66Q6";

			var sentMessage = CreateSendMessage(mafMessaging, messageNumber, userCode);

			// This little "Double Save" necessary to stop the Message Number getting blown away on the first save.
			sentMessage.EM_ReceiveTransmit = NZMMessage.Direction.Transmit;
			Factory.Save();

			return declaration;
		}

		protected NZMMessage CreateSendMessage(MAFMessagingBO mafMessaging, ZString messageNumber, string userCode = "")
		{
			NZMMessage sentMessage = mafMessaging.Messages.AddNew();
			sentMessage.EM_IsTestMessage = true;
			sentMessage.EM_MessageNum = messageNumber;
			sentMessage.EM_MessageType = NZMMessage.MessageTypes.Transmit.MessagingRequest;
			sentMessage.EM_MessageSubType = NZMMessage.MessageTypes.Transmit.MessageSubTypes.Original;
			sentMessage.EM_ReceiveTransmit = NZMMessage.Direction.Receive;
			sentMessage.EM_Status = NZMMessage.Status.Acknowledged;
			sentMessage.EM_MessageText = "We sent something. Yay.";
			sentMessage.EM_GB = Branch.PK;
			sentMessage.EM_SystemCreateUser = userCode;
			sentMessage.EM_SystemLastEditUser = userCode;

			Factory.Save();
			return sentMessage;
		}

		protected GlbBranch Branch
		{
			get { return branch ?? (branch = GetNewBranch()); }
		}
		GlbBranch branch;

		GlbBranch GetNewBranch()
		{
			GlbBranch branch = new BusinessObjectFactory().NewWithValidTestData<GlbBranch>();
			branch.GB_RL_NKHomePort = "NZAKL";
			branch.Company.GC_RN_NKCountryCode = Constants.CountryCodes.NewZealand;
			branch.Factory.Save();
			NZCustomsDataRegistry.Instance.NZBrokerageID.SetValue(branch.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, "00009917B");
			NZCustomsDataRegistry.Instance.NZBrokerageID.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, "00009917C"); // So that EDIMessage Gets a Sender for the Reference on Save.
			return branch;
		}

		protected GlbCompany Company
		{
			get { return branch.Company; }
		}
	}
}
