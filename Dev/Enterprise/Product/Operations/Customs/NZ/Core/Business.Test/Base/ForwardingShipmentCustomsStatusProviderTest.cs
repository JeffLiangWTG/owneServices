using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.NZ.Business.Declaration;
using Enterprise.Customs.NZ.Business.Express;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.NZ.Business.Testing
{
	using System.Linq;
	using Enterprise.Customs.Common.Shared;
	using Enterprise.Customs.NZ.Business;
	using Enterprise.ZArchitecture.Schema;

	public class ForwardingShipmentCustomsStatusProviderTest : TestCaseWithFactory
	{
		public void TestStatusEmptyWhenNoDeclarationOrAirCargoECIExists()
		{
			SetupStatusProvider();
			AssertEquals("statusProvider.CustomsCargoStatus()", "", statusProvider.CustomsCargoStatus());
			AssertEquals("statusProvider.CustomsMessageStatus()", "", statusProvider.CustomsMessageStatus());
		}

		public void TestStatusFromFormalDeclaration()
		{
			JobDeclaration declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			declaration.JE_JS = Shipment.PK;

			AssertEquals("Precondition:IsECIWriteOff", false, declaration.IsECIWriteoff);
			AssertEquals("Precondition: declaration.JE_EntryStatus", LowValueConsignmentStatusList.Codes.NotSentToCustoms, declaration.JE_EntryStatus);

			SetupStatusProvider();
			AssertEquals("statusProvider.CustomsCargoStatus()", FormalEntryStatusList.Descriptions.NotSentToCustoms, statusProvider.CustomsCargoStatus());
			AssertEquals("statusProvider.CustomsMessageStatus()", FormalEntryStatusList.Descriptions.NotSentToCustoms, statusProvider.CustomsMessageStatus());

			statusProvider.SetCustomsCargoStatusCode(ConsolidatedEntryStatusList.Codes.ReadyForConsolidation);
			SetupStatusProvider();
			AssertEquals("Consolidation status stored in JE_EntryStatus should not affect existing functionality - RFC is still a not sent status", FormalEntryStatusList.Descriptions.NotSentToCustoms, statusProvider.CustomsCargoStatus());
			AssertEquals("statusProvider.CustomsMessageStatus()", FormalEntryStatusList.Descriptions.NotSentToCustoms, statusProvider.CustomsMessageStatus());

			statusProvider.SetCustomsCargoStatusCode(ConsolidatedEntryStatusList.Codes.AppliedToConsolidation);
			SetupStatusProvider();
			AssertEquals("Consolidation status stored in JE_EntryStatus should not affect existing functionality - ATC is still a not sent status", FormalEntryStatusList.Descriptions.NotSentToCustoms, statusProvider.CustomsCargoStatus());
			AssertEquals("statusProvider.CustomsMessageStatus()", FormalEntryStatusList.Descriptions.NotSentToCustoms, statusProvider.CustomsMessageStatus());

			statusProvider.SetCustomsCargoStatusCode(FormalEntryStatusList.Codes.InspectionsAuditRequirements);
			AssertEquals(FormalEntryStatusList.Codes.InspectionsAuditRequirements, declaration.JE_EntryStatus);
			AssertEquals(FormalEntryStatusList.Codes.InspectionsAuditRequirements, statusProvider.GetCustomsCargoStatusCode());
			SetupStatusProvider();
			AssertEquals("statusProvider.CustomsCargoStatus()", FormalEntryStatusList.Descriptions.InspectionsAuditRequirements, statusProvider.CustomsCargoStatus());
			AssertEquals("statusProvider.CustomsMessageStatus()", FormalEntryStatusList.Descriptions.InspectionsAuditRequirements, statusProvider.CustomsMessageStatus());

			declaration.JE_EntryStatus = FormalEntryStatusList.Codes.ManualEntryCannotBeSentToCustoms;
			SetupStatusProvider();
			AssertEquals("statusProvider.CustomsCargoStatus()", FormalEntryStatusList.Descriptions.ManualEntryCannotBeSentToCustoms, statusProvider.CustomsCargoStatus());
			AssertEquals("statusProvider.CustomsMessageStatus()", FormalEntryStatusList.Descriptions.ManualEntryCannotBeSentToCustoms, statusProvider.CustomsMessageStatus());

			declaration.JE_EntryStatus = FormalEntryStatusList.Codes.QueuedForSending;
			SetupStatusProvider();
			AssertEquals("statusProvider.CustomsCargoStatus()", FormalEntryStatusList.Descriptions.QueuedForSending, statusProvider.CustomsCargoStatus());
			AssertEquals("statusProvider.CustomsMessageStatus()", FormalEntryStatusList.Descriptions.QueuedForSending, statusProvider.CustomsMessageStatus());
		}

		public void TestStatusFromECIDeclaration()
		{
			JobDeclaration declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
			declaration.JE_JS = Shipment.PK;

			AssertEquals("Precondition: IsECIWriteOff", true, declaration.IsECIWriteoff);
			AssertEquals("Precondition: declaration.CusEntryHeader.CH_EntryStatus", LowValueConsignmentStatusList.Codes.NotSentToCustoms, declaration.CusEntryHeader.CH_EntryStatus);
			AssertEquals("Precondition: declaration.JE_EntryStatus", LowValueManifestStatusList.Codes.NotSentToCustoms, declaration.JE_EntryStatus);

			SetupStatusProvider();
			AssertEquals("statusProvider.CustomsMessageStatus()", LowValueManifestStatusList.Descriptions.NotSentToCustoms, statusProvider.CustomsMessageStatus());
			AssertEquals("statusProvider.CustomsCargoStatus()", LowValueConsignmentStatusList.Descriptions.NotSentToCustoms, statusProvider.CustomsCargoStatus());

			statusProvider.SetCustomsCargoStatusCode(ConsolidatedEntryStatusList.Codes.ReadyForConsolidation);
			SetupStatusProvider();
			AssertEquals("Consolidation status stored in JE_EntryStatus should not affect existing functionality - RFC is still a 'not sent' status", LowValueManifestStatusList.Descriptions.NotSentToCustoms, statusProvider.CustomsMessageStatus());
			AssertEquals("statusProvider.CustomsCargoStatus()", LowValueConsignmentStatusList.Descriptions.NotSentToCustoms, statusProvider.CustomsCargoStatus());

			statusProvider.SetCustomsCargoStatusCode(ConsolidatedEntryStatusList.Codes.AppliedToConsolidation);
			SetupStatusProvider();
			AssertEquals("Consolidation status stored in JE_EntryStatus should not affect existing functionality - ATC is still a 'not sent' status", LowValueManifestStatusList.Descriptions.NotSentToCustoms, statusProvider.CustomsMessageStatus());
			AssertEquals("statusProvider.CustomsCargoStatus()", LowValueConsignmentStatusList.Descriptions.NotSentToCustoms, statusProvider.CustomsCargoStatus());

			statusProvider.SetCustomsCargoStatusCode(FormalEntryStatusList.Codes.SentToCustoms);
			AssertEquals(FormalEntryStatusList.Codes.SentToCustoms, declaration.JE_EntryStatus);
			AssertEquals(FormalEntryStatusList.Codes.SentToCustoms, statusProvider.GetCustomsCargoStatusCode());
			declaration.CusEntryHeader.CH_EntryStatus = LowValueManifestStatusList.Codes.SentToCustoms;
			SetupStatusProvider();
			AssertEquals("statusProvider.CustomsMessageStatus()", LowValueManifestStatusList.Descriptions.SentToCustoms, statusProvider.CustomsMessageStatus());
			AssertEquals("statusProvider.CustomsCargoStatus()", LowValueConsignmentStatusList.Descriptions.SentToCustoms, statusProvider.CustomsCargoStatus());

			declaration.JE_EntryStatus = LowValueConsignmentStatusList.Codes.ConsignmentHeld;
			declaration.CusEntryHeader.CH_EntryStatus = LowValueManifestStatusList.Codes.InspectionsAuditRequirements;
			SetupStatusProvider();
			AssertEquals("statusProvider.CustomsMessageStatus()", LowValueManifestStatusList.Descriptions.InspectionsAuditRequirements, statusProvider.CustomsMessageStatus());
			AssertEquals("statusProvider.CustomsCargoStatus()", LowValueConsignmentStatusList.Descriptions.ConsignmentHeld, statusProvider.CustomsCargoStatus());

			declaration.JE_EntryStatus = LowValueConsignmentStatusList.Codes.ConsignmentInError;
			declaration.CusEntryHeader.CH_EntryStatus = LowValueManifestStatusList.Codes.ManifestRejected;
			SetupStatusProvider();
			AssertEquals("statusProvider.CustomsMessageStatus()", LowValueManifestStatusList.Descriptions.ManifestRejected, statusProvider.CustomsMessageStatus());
			AssertEquals("statusProvider.CustomsCargoStatus()", LowValueConsignmentStatusList.Descriptions.ConsignmentInError, statusProvider.CustomsCargoStatus());
		}

		public void TestStatusFromDeclarationsAndCusMAWBsFromBranchInOtherCompanyDontAffectStatus()
		{
			CusMAWB cusMAWBOtherCountry = Factory.NewWithValidTestData<CusMAWB>();
			CusHAWB cusHAWBOtherCountry = cusMAWBOtherCountry.ChildBills.AddNew();
			cusHAWBOtherCountry.CS_JS = Shipment.PK;
			cusHAWBOtherCountry.CS_CustomsStatus = LowValueConsignmentStatusList.Codes.SentToCustoms;
			cusMAWBOtherCountry.CM_CustomsStatus = LowValueManifestStatusList.Codes.SentToCustoms;
			cusMAWBOtherCountry.CM_ApplicationCode = ZString.Empty;   // reset to blank as default is NZE

			JobDeclaration declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
			declaration.JE_JS = Shipment.PK;
			declaration.JE_GB = Factory.Load<GlbBranch>(new ZQuery(GlbBranchSchema.GB_Code, "DEM")).FirstOrDefault().PK;
			declaration.JE_EntryStatus = LowValueManifestStatusList.Codes.SentToCustoms;

			AssertEquals("Precondition:IsECIWriteOff", true, declaration.IsECIWriteoff);

			SetupStatusProvider();
			AssertEquals("statusProvider.GetCustomsStatus()", "", statusProvider.GetCustomsCargoStatusCode());
			AssertEquals("statusProvider.CustomsMessageStatus()", "", statusProvider.CustomsMessageStatus());
			AssertEquals("statusProvider.CustomsCargoStatus()", "", statusProvider.CustomsCargoStatus());
		}

		public void TestStatusFromAirCargoECIWhenNoDeclarationExists()
		{
			CusMAWB cusMAWB = Factory.NewWithValidTestData<CusMAWB>();
			CusHAWB cusHAWB = cusMAWB.ChildBills.AddNew();
			cusHAWB.CS_JS = Shipment.PK;

			SetupStatusProvider();
			AssertEquals("statusProvider.CustomsMessageStatus()", LowValueManifestStatusList.Descriptions.NotSentToCustoms, statusProvider.CustomsMessageStatus());
			AssertEquals("statusProvider.CustomsCargoStatus()", LowValueConsignmentStatusList.Descriptions.NotSentToCustoms, statusProvider.CustomsCargoStatus());

			cusMAWB.CM_CustomsStatus = LowValueManifestStatusList.Codes.SentToCustoms;

			statusProvider.SetCustomsCargoStatusCode(LowValueConsignmentStatusList.Codes.SentToCustoms);
			AssertEquals(LowValueConsignmentStatusList.Codes.SentToCustoms, cusHAWB.CS_CustomsStatus);
			AssertEquals(LowValueConsignmentStatusList.Codes.SentToCustoms, statusProvider.GetCustomsCargoStatusCode());
			SetupStatusProvider();
			AssertEquals("statusProvider.CustomsMessageStatus()", LowValueManifestStatusList.Descriptions.SentToCustoms, statusProvider.CustomsMessageStatus());
			AssertEquals("statusProvider.CustomsCargoStatus()", LowValueConsignmentStatusList.Descriptions.SentToCustoms, statusProvider.CustomsCargoStatus());

			cusMAWB.CM_CustomsStatus = LowValueManifestStatusList.Codes.ManifestAccepted;
			cusHAWB.CS_CustomsStatus = LowValueConsignmentStatusList.Codes.ConsignmentWrittenOff;
			SetupStatusProvider();
			AssertEquals("statusProvider.CustomsMessageStatus()", LowValueManifestStatusList.Descriptions.ManifestAccepted, statusProvider.CustomsMessageStatus());
			AssertEquals("statusProvider.CustomsCargoStatus()", LowValueConsignmentStatusList.Descriptions.ConsignmentWrittenOff, statusProvider.CustomsCargoStatus());

			cusMAWB.CM_CustomsStatus = LowValueManifestStatusList.Codes.ManifestAccepted;
			cusHAWB.CS_CustomsStatus = LowValueConsignmentStatusList.Codes.FormalDeclarationRequired;
			SetupStatusProvider();
			AssertEquals("statusProvider.CustomsMessageStatus()", LowValueManifestStatusList.Descriptions.ManifestAccepted, statusProvider.CustomsMessageStatus());
			AssertEquals("statusProvider.CustomsCargoStatus()", LowValueConsignmentStatusList.Descriptions.FormalDeclarationRequired, statusProvider.CustomsCargoStatus());

			cusMAWB.CM_CustomsStatus = LowValueManifestStatusList.Codes.ManifestInError;
			cusHAWB.CS_CustomsStatus = LowValueConsignmentStatusList.Codes.ConsignmentWrittenOff;
			SetupStatusProvider();
			AssertEquals("statusProvider.CustomsMessageStatus()", LowValueManifestStatusList.Descriptions.ManifestInError, statusProvider.CustomsMessageStatus());
			AssertEquals("statusProvider.CustomsCargoStatus()", LowValueConsignmentStatusList.Descriptions.ConsignmentWrittenOff, statusProvider.CustomsCargoStatus());

			cusMAWB.CM_CustomsStatus = LowValueManifestStatusList.Codes.ManifestRejected;
			cusHAWB.CS_CustomsStatus = LowValueConsignmentStatusList.Codes.ConsignmentInError;
			SetupStatusProvider();
			AssertEquals("statusProvider.CustomsMessageStatus()", LowValueManifestStatusList.Descriptions.ManifestRejected, statusProvider.CustomsMessageStatus());
			AssertEquals("statusProvider.CustomsCargoStatus()", LowValueConsignmentStatusList.Descriptions.ConsignmentInError, statusProvider.CustomsCargoStatus());

			cusMAWB.CM_CustomsStatus = LowValueManifestStatusList.Codes.ManifestAccepted;
			cusHAWB.CS_CustomsStatus = LowValueConsignmentStatusList.Codes.ConsignmentInError;
			SetupStatusProvider();
			AssertEquals("statusProvider.CustomsMessageStatus()", LowValueManifestStatusList.Descriptions.ManifestAccepted, statusProvider.CustomsMessageStatus());
			AssertEquals("statusProvider.CustomsCargoStatus()", LowValueConsignmentStatusList.Descriptions.ConsignmentInError, statusProvider.CustomsCargoStatus());
		}

		public void TestStatusFromTSWAirCargo()
		{
			var cusMAWB = Factory.NewWithValidTestData<CusMAWB>();
			cusMAWB.CM_ApplicationCode = Core.Constants.Customs.ExpressApplicationCodes.NZ.TSWWriteOff;
			var cusHAWB = cusMAWB.ChildBills.AddNew();
			cusHAWB.CS_ApplicationCode = Core.Constants.Customs.ExpressApplicationCodes.NZ.TSWWriteOff;
			cusHAWB.CS_JS = Shipment.PK;

			SetupStatusProvider();
			AssertEquals("statusProvider.CustomsMessageStatus()", LowValueManifestStatusList.Descriptions.NotSentToCustoms, statusProvider.CustomsMessageStatus());
			AssertEquals("statusProvider.CustomsCargoStatus()", LowValueConsignmentStatusList.Descriptions.NotSentToCustoms, statusProvider.CustomsCargoStatus());

			cusMAWB.CM_CustomsStatus = LowValueManifestStatusList.Codes.SentToCustoms;

			statusProvider.SetCustomsCargoStatusCode(LowValueConsignmentStatusList.Codes.SentToCustoms);
			AssertEquals(LowValueConsignmentStatusList.Codes.SentToCustoms, cusHAWB.CS_CustomsStatus);
			AssertEquals(LowValueConsignmentStatusList.Codes.SentToCustoms, statusProvider.GetCustomsCargoStatusCode());
			SetupStatusProvider();
			AssertEquals("statusProvider.CustomsMessageStatus()", LowValueManifestStatusList.Descriptions.SentToCustoms, statusProvider.CustomsMessageStatus());
			AssertEquals("statusProvider.CustomsCargoStatus()", LowValueConsignmentStatusList.Descriptions.SentToCustoms, statusProvider.CustomsCargoStatus());

			cusMAWB.CM_CustomsStatus = LowValueManifestStatusList.Codes.ManifestAccepted;
			cusHAWB.CS_CustomsStatus = LowValueConsignmentStatusList.Codes.ConsignmentWrittenOff;
			SetupStatusProvider();
			AssertEquals("statusProvider.CustomsMessageStatus()", LowValueManifestStatusList.Descriptions.ManifestAccepted, statusProvider.CustomsMessageStatus());
			AssertEquals("statusProvider.CustomsCargoStatus()", LowValueConsignmentStatusList.Descriptions.ConsignmentWrittenOff, statusProvider.CustomsCargoStatus());

			cusMAWB.CM_CustomsStatus = LowValueManifestStatusList.Codes.ManifestAccepted;
			cusHAWB.CS_CustomsStatus = LowValueConsignmentStatusList.Codes.FormalDeclarationRequired;
			SetupStatusProvider();
			AssertEquals("statusProvider.CustomsMessageStatus()", LowValueManifestStatusList.Descriptions.ManifestAccepted, statusProvider.CustomsMessageStatus());
			AssertEquals("statusProvider.CustomsCargoStatus()", LowValueConsignmentStatusList.Descriptions.FormalDeclarationRequired, statusProvider.CustomsCargoStatus());

			cusMAWB.CM_CustomsStatus = LowValueManifestStatusList.Codes.ManifestInError;
			cusHAWB.CS_CustomsStatus = LowValueConsignmentStatusList.Codes.ConsignmentWrittenOff;
			SetupStatusProvider();
			AssertEquals("statusProvider.CustomsMessageStatus()", LowValueManifestStatusList.Descriptions.ManifestInError, statusProvider.CustomsMessageStatus());
			AssertEquals("statusProvider.CustomsCargoStatus()", LowValueConsignmentStatusList.Descriptions.ConsignmentWrittenOff, statusProvider.CustomsCargoStatus());

			cusMAWB.CM_CustomsStatus = LowValueManifestStatusList.Codes.ManifestRejected;
			cusHAWB.CS_CustomsStatus = LowValueConsignmentStatusList.Codes.ConsignmentInError;
			SetupStatusProvider();
			AssertEquals("statusProvider.CustomsMessageStatus()", LowValueManifestStatusList.Descriptions.ManifestRejected, statusProvider.CustomsMessageStatus());
			AssertEquals("statusProvider.CustomsCargoStatus()", LowValueConsignmentStatusList.Descriptions.ConsignmentInError, statusProvider.CustomsCargoStatus());

			cusMAWB.CM_CustomsStatus = LowValueManifestStatusList.Codes.ManifestAccepted;
			cusHAWB.CS_CustomsStatus = LowValueConsignmentStatusList.Codes.ConsignmentInError;
			SetupStatusProvider();
			AssertEquals("statusProvider.CustomsMessageStatus()", LowValueManifestStatusList.Descriptions.ManifestAccepted, statusProvider.CustomsMessageStatus());
			AssertEquals("statusProvider.CustomsCargoStatus()", LowValueConsignmentStatusList.Descriptions.ConsignmentInError, statusProvider.CustomsCargoStatus());
		}

		public void TestStatusFromTSWSeaCargo()
		{
			var oceanBill = Factory.NewWithValidTestData<CusSCAOceanBill>();
			oceanBill.CB_ApplicationCode = Core.Constants.Customs.ExpressApplicationCodes.NZ.TSWWriteOff;
			var houseBill = oceanBill.HouseBills.AddNew();
			houseBill.CA_JS = Shipment.PK;

			SetupStatusProvider();
			AssertEquals("statusProvider.CustomsMessageStatus()", LowValueConsignmentStatusList.Descriptions.NotSentToCustoms, statusProvider.CustomsMessageStatus());
			AssertEquals("statusProvider.CustomsCargoStatus()", LowValueConsignmentStatusList.Descriptions.NotSentToCustoms, statusProvider.CustomsCargoStatus());

			oceanBill.CB_CustomsStatus = LowValueManifestStatusList.Codes.ManifestAccepted;
			houseBill.CA_ShipmentStatus = LowValueConsignmentStatusList.Codes.ConsignmentWrittenOff;
			houseBill.CA_MessageStatus = LowValueConsignmentStatusList.Codes.Acknowledgement;
			SetupStatusProvider();
			AssertEquals("statusProvider.CustomsMessageStatus()", LowValueManifestStatusList.Descriptions.ManifestAccepted, statusProvider.CustomsMessageStatus());
			AssertEquals("statusProvider.CustomsCargoStatus()", LowValueConsignmentStatusList.Descriptions.ConsignmentWrittenOff, statusProvider.CustomsCargoStatus());

			oceanBill.CB_CustomsStatus = LowValueManifestStatusList.Codes.ManifestAccepted;
			houseBill.CA_ShipmentStatus = LowValueConsignmentStatusList.Codes.FormalDeclarationRequired;
			SetupStatusProvider();
			AssertEquals("statusProvider.CustomsMessageStatus()", LowValueManifestStatusList.Descriptions.ManifestAccepted, statusProvider.CustomsMessageStatus());
			AssertEquals("statusProvider.CustomsCargoStatus()", LowValueConsignmentStatusList.Descriptions.FormalDeclarationRequired, statusProvider.CustomsCargoStatus());

			oceanBill.CB_CustomsStatus = LowValueManifestStatusList.Codes.ManifestAccepted;
			houseBill.CA_ShipmentStatus = LowValueConsignmentStatusList.Codes.ConsignmentWrittenOff;
			SetupStatusProvider();
			AssertEquals("statusProvider.CustomsMessageStatus()", LowValueManifestStatusList.Descriptions.ManifestAccepted, statusProvider.CustomsMessageStatus());
			AssertEquals("statusProvider.CustomsCargoStatus()", LowValueConsignmentStatusList.Descriptions.ConsignmentWrittenOff, statusProvider.CustomsCargoStatus());

			oceanBill.CB_CustomsStatus = LowValueManifestStatusList.Codes.ManifestAccepted;
			houseBill.CA_ShipmentStatus = LowValueConsignmentStatusList.Codes.ConsignmentInError;
			SetupStatusProvider();
			AssertEquals("statusProvider.CustomsMessageStatus()", LowValueManifestStatusList.Descriptions.ManifestAccepted, statusProvider.CustomsMessageStatus());
			AssertEquals("statusProvider.CustomsCargoStatus()", LowValueConsignmentStatusList.Descriptions.ConsignmentInError, statusProvider.CustomsCargoStatus());

			oceanBill.CB_CustomsStatus = LowValueManifestStatusList.Codes.ManifestAccepted;
			houseBill.CA_ShipmentStatus = LowValueConsignmentStatusList.Codes.ConsignmentInError;
			SetupStatusProvider();
			AssertEquals("statusProvider.CustomsMessageStatus()", LowValueManifestStatusList.Descriptions.ManifestAccepted, statusProvider.CustomsMessageStatus());
			AssertEquals("statusProvider.CustomsCargoStatus()", LowValueConsignmentStatusList.Descriptions.ConsignmentInError, statusProvider.CustomsCargoStatus());
		}

		public void TestStatusFromDeclarationWhenAirCargoECIAndDeclarationExist()
		{
			TestStatusFromAirCargoECIWhenNoDeclarationExists();
			TestStatusFromECIDeclaration();
		}

		#region Implementation
		void SetupStatusProvider()
		{
			Factory.Save();
			statusProvider = new ForwardingShipmentCustomsStatusProvider(Shipment);
		}
		ForwardingShipmentCustomsStatusProvider statusProvider;

		ForwardingShipment Shipment
		{
			get { return fShipment ?? (fShipment = Factory.NewWithValidTestData<ForwardingShipment>()); }
		}
		ForwardingShipment fShipment;
		#endregion
	}
}
