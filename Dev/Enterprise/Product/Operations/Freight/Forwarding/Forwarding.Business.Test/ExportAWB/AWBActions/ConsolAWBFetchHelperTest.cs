using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.LocalCartage.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.Business.AWB.Testing
{
	sealed class ConsolAWBFetchHelperTest : TestCaseWithFactory
	{
		public void TestFetchHintsForShipmentsToSendOnConsolAWBActions()
		{
			Factory.ResetDatabaseLoadCount();

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.JK_MasterBillNum = "08112345678";

			for (int i = 1; i <= 2; i++)
			{
				var shipment = consol.Shipments.AddNew();
				shipment.JS_UniqueConsignRef = "S00001" + i;

				var transport = shipment.Transports.AddNew();

				var consignorCode = "CNR" + i;
				var consigneeCode = "CNE" + i;

				var consignor = GetOrganization(consignorCode);
				var consignorNotes = consignor.Notes.AddNew();

				var consignee = GetOrganization(consigneeCode);
				var consigneeNotes = consignee.Notes.AddNew();

				var consignorAddr = consignor.Addresses.AddNew();
				consignorAddr.OA_Address1 = "aaa";

				var consigneeAddr = consignor.Addresses.AddNew();
				consigneeAddr.OA_Address1 = "bbb";

				shipment.ConsigneePK = consignee.PK;
				shipment.ConsignorPK = consignor.PK;

				if (i == 1)
				{
					consignorAddr.OA_RL_NKRelatedPortCode = "TCGDT";
					consigneeAddr.OA_RL_NKRelatedPortCode = "REANN";

					consignor.OH_RL_NKClosestPort = "JPTYO";
					consignee.OH_RL_NKClosestPort = "CAAAB";

					shipment.JS_RL_NKOrigin = "DEMIO";
					shipment.JS_RL_NKDestination = "CAAAB";
				}

				if (i == 2)
				{
					shipment.JS_RL_NKOrigin = "AUMEL";
					shipment.JS_RL_NKDestination = "USMIA";

					consignor.OH_RL_NKClosestPort = "INABG";
					consignee.OH_RL_NKClosestPort = "PAABA";

					consignorAddr.OA_RL_NKRelatedPortCode = "DE222";
					consigneeAddr.OA_RL_NKRelatedPortCode = "REANN";
				}

				var jobCartage = (BusinessObject)Factory.New<ICommonCartage>();
				jobCartage[JobCartageSchema.JJ_ParentID] = shipment.PK;
				jobCartage[JobCartageSchema.JJ_ParentTableCode] = JobShipmentSchema.Constants.Prefix;
				jobCartage[JobCartageSchema.JJ_ConsignmentID] = "D0000" + i;

				var job = new JobHeader.Loader(shipment).TryCreate();
				job.JH_GE = GlbDepartment.CurrentDepartment.PK;
				job.JH_GB = GlbBranch.CurrentBranch.PK;

				var declaration = Factory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();
				declaration.JE_JS = shipment.PK;
			}

			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			AssertEquals("Precondition: Should be zero db hit count before loading", 0, newFactory.DatabaseLoadCount);

			var loadedConsol = newFactory.Load<ForwardingConsol>(consol.PK);
			var shipments = loadedConsol.Shipments.Count;

			var actions = new ConsolAWBActions(loadedConsol, AWBActions.ActionsModeType.All);

			newFactory.ResetDatabaseLoadCount();
			actions.ValidateShipmentsToSend();

			AssertMaxDbHits(38, newFactory);

			//JobComInvoiceHeader: 2
			//JobDocumentData: 2
			//JobHeader: 4
			//JobOrderHeader: 2
			//RefCountry: 1
			//RefDatabase_RefDocOrgCusCode: 2
			//ExportAWBHeader: 1
			//GlbCompany: 5
			//JobCartage: 1
			//JobCharge: 1
			//JobCO2e: 1
			//JobConShipLink: 1
			//JobConsolTransport: 1
			//JobDocsAndCartage: 1
			//OrgAddress: 1
			//OrgAddressCapability: 1
			//OrgContact: 1
			//OrgDocument: 1
			//OrgMiscServ: 1
			//RefAirline: 1
			//StmData: 1
			//StmNote: 1
			//CusEntryHeader 1
			//CusEntryNum  1
			//CusHAWB 1
			//CusInBondHeader  1
			//CusSCAHouse  1
			//GlbBranch: 0
			//GlbDepartment: 0
			//GlbStaff: 0
			//RefCountryRequiredDocument: 0
			//RefCountryRules: 0
			//RefCountryStates: 0
			//RefCurrency: 0
			//RefDocType: 0
			//RefLocalLanguage: 0
			//RefServiceLevel: 0
			//RefTimeZone: 0
			//RefTimeZoneSet: 0
			//RefUNLOCO: 0
		}

		OrgHeader GetOrganization(ZString code)
		{
			var organization = Factory.NewWithValidTestData<OrgHeader>();
			organization.OH_Code = code;
			organization.MainAddress.OA_Code = "ADD1";

			var miscServ = organization.MiscServ;

			var contact = organization.Contacts.AddNew();
			var document = contact.Documents.AddNew();

			var orgCountryData = Factory.NewWithValidTestData<OrgCountryData>();
			orgCountryData.OV_OH_OrgHeader = organization.PK;

			return organization;
		}
	}
}
