using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.US.Business.MessageBuilders;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	class PGAGovernmentAgenciesWithPGACorrectionTest : TestCaseWithFactory
	{
		[TestDate(2016, 8, 1)]
		public void TestWhenUpdateLineByLineIsSupported()
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.PGADataCorrection2ndPhase, Core.Constants.CountryCodes.UnitedStates, ZDateTime.Today, true))
			{
				var dec = Factory.New<JobDeclaration>();
				dec.JE_MessageType = JobMessageTypeList.Codes.Import;
				dec.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
				dec.US_EntryFilerCode = "XJ5";
				dec.US_EnableENS = true;

				dec.Invoices.AddNew();
				var invoiceLine = dec.InvoiceLines.AddNew();
				var aphisLine1 = invoiceLine.APHISHeaders.AddNew();
				aphisLine1.US_TrackingStatus = PGATrackingStatusList.Codes.ToBeDeleted;

				var aphisLine1_1 = invoiceLine.APHISHeaders.AddNew();
				aphisLine1_1.US_TrackingStatus = PGATrackingStatusList.Codes.Deleted;

				var secondaryLine = invoiceLine.AddSecondaryInvoiceLine();
				var aphisLine2 = secondaryLine.APHISHeaders.AddNew();
				aphisLine2.US_TrackingStatus = PGATrackingStatusList.Codes.ToBeDeleted;
				var aphisLine3 = secondaryLine.APHISHeaders.AddNew();
				aphisLine3.US_TrackingStatus = PGATrackingStatusList.Codes.Added;

				var secondaryLine2 = invoiceLine.AddSecondaryInvoiceLine();
				var aphisLine4 = secondaryLine2.APHISHeaders.AddNew();
				aphisLine4.US_TrackingStatus = PGATrackingStatusList.Codes.ToBeUpdated;
				var aphisLine5 = secondaryLine2.APHISHeaders.AddNew();
				aphisLine5.US_TrackingStatus = PGATrackingStatusList.Codes.Added;

				dec.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

				var line = (IGovernmentAgenciesCommon)new PGAGovernmentAgenciesWithPGACorrection(invoiceLine.CusEntryLine, invoiceLine.CusEntryLine);
				AssertEquals(1, line.APHISHeaders.Count());
				AssertEquals(aphisLine1, line.APHISHeaders.ElementAt(0));

				var line2 = (IGovernmentAgenciesCommon)new PGAGovernmentAgenciesWithPGACorrection(invoiceLine.CusEntryLine, secondaryLine.CusEntryLine);
				AssertEquals(1, line2.APHISHeaders.Count());

				var line3 = (IGovernmentAgenciesCommon)new PGAGovernmentAgenciesWithPGACorrection(invoiceLine.CusEntryLine, secondaryLine2.CusEntryLine);
				AssertEquals(1, line3.APHISHeaders.Count());
			}
		}

		[TestDate(2016, 8, 1)]
		public void TestBeforeUpdateLineByLineIsSupported()
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.PGADataCorrection2ndPhase, Core.Constants.CountryCodes.UnitedStates, ZDateTime.Today, false))
			{
				var dec = Factory.New<JobDeclaration>();
				dec.JE_MessageType = JobMessageTypeList.Codes.Import;
				dec.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
				dec.US_EntryFilerCode = "XJ5";
				dec.US_EnableENS = true;

				dec.Invoices.AddNew();
				var invoiceLine = dec.InvoiceLines.AddNew();
				var aphisLine1 = invoiceLine.APHISHeaders.AddNew();
				aphisLine1.US_TrackingStatus = PGATrackingStatusList.Codes.ToBeDeleted;

				var aphisLine1_1 = invoiceLine.APHISHeaders.AddNew();
				aphisLine1_1.US_TrackingStatus = PGATrackingStatusList.Codes.Deleted;

				var secondaryLine = invoiceLine.AddSecondaryInvoiceLine();
				var aphisLine2 = secondaryLine.APHISHeaders.AddNew();
				aphisLine2.US_TrackingStatus = PGATrackingStatusList.Codes.ToBeDeleted;
				var aphisLine3 = secondaryLine.APHISHeaders.AddNew();
				aphisLine3.US_TrackingStatus = PGATrackingStatusList.Codes.Added;

				var secondaryLine2 = invoiceLine.AddSecondaryInvoiceLine();
				var aphisLine4 = secondaryLine2.APHISHeaders.AddNew();
				aphisLine4.US_TrackingStatus = PGATrackingStatusList.Codes.ToBeUpdated;
				var aphisLine5 = secondaryLine2.APHISHeaders.AddNew();
				aphisLine5.US_TrackingStatus = PGATrackingStatusList.Codes.Added;

				dec.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

				var line = (IGovernmentAgenciesCommon)new PGAGovernmentAgenciesWithPGACorrection(invoiceLine.CusEntryLine, invoiceLine.CusEntryLine);
				AssertEquals(1, line.APHISHeaders.Count());
				AssertEquals(aphisLine1, line.APHISHeaders.ElementAt(0));

				var line2 = (IGovernmentAgenciesCommon)new PGAGovernmentAgenciesWithPGACorrection(invoiceLine.CusEntryLine, secondaryLine.CusEntryLine);
				AssertEquals(2, line2.APHISHeaders.Count());

				var line3 = (IGovernmentAgenciesCommon)new PGAGovernmentAgenciesWithPGACorrection(invoiceLine.CusEntryLine, secondaryLine2.CusEntryLine);
				AssertEquals(2, line3.APHISHeaders.Count());
			}
		}

		[TestDate(2016, 8, 1)]
		public void TestAllEPALinesSentInFirstPhase()
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.PGADataCorrection2ndPhase, Core.Constants.CountryCodes.UnitedStates, ZDateTime.Today, false))
			{
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
				declaration.US_EntryFilerCode = "XJ5";
				declaration.US_EnableCRL = true;

				var invoice = declaration.Invoices.AddNew();
				var invoiceLine = invoice.InvoiceLines.AddNew();
				invoiceLine.JI_Description = "TEST ALL EPA LINES";
				declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

				invoiceLine.US_ODSInd = OGAIndicatorList.Codes.Declared;
				invoiceLine.US_ODSTrackingStatus = PGATrackingStatusList.Codes.Adding;
				invoiceLine.US_TSCAInd = OGAIndicatorList.Codes.Declared;
				invoiceLine.US_TSCAODSCertIndividual = PartyTypeList.Codes.Importer;
				invoiceLine.US_FDAContactName = "IAN TEST TSCA";
				invoiceLine.US_VNEInd = OGAIndicatorList.Codes.Declared;
				var vehicle = invoiceLine.VehicleLines.AddNew();
				vehicle.US_FormType = EPAVNEDocumentIdentifierList.Codes.EPA3520_1;
				invoiceLine.US_PSTIndicator = OGAIndicatorList.Codes.Declared;
				var pst = invoiceLine.PSTLines.AddNew();
				pst.US_ProductType = PSTProductTypeList.Codes.PS1;
				pst.US_TrackingStatus = PGATrackingStatusList.Codes.Updating;
				Factory.Save();

				var seEntry = declaration.ActiveEntryHeaders.SimplifiedEntry;
				var content = new PGACorrectionMessageBuilder(seEntry, ACEEntrySummaryMessageSendingOption.New(), false).GetSerialiseMessageContents();
				AssertContains(
	@"-----------------AENSOI-----------------
 Commercial Description Text (11-80) :TEST ALL EPA LINES

----------------AEPAPG01----------------
 P G A Line Number (5-7)                :1
 Government Agency Code (8-10)          :EPA
 Government Agency Program Code (11-13) :ODS

----------------AEPAPG01----------------
 P G A Line Number (5-7)                :2
 Government Agency Code (8-10)          :EPA
 Government Agency Program Code (11-13) :TS1

----------------AEPAPG02----------------
 Item Type (5-5) :P

----------------AEPAPG22----------------
 Entity Role Code (18-20)          :CI
 Declaration Certification (25-25) :Y
 Date Of Signature (26-33)         :01-Aug-16

----------------AEPAPG21----------------
 Individual Qualifier (5-7) :CI
 Individual Name (8-30)     :IAN TEST TSCA

----------------AEPAPG01----------------
 P G A Line Number (5-7)                :3
 Government Agency Code (8-10)          :EPA
 Government Agency Program Code (11-13) :VNE

----------------AEPAPG02----------------
 Item Type (5-5) :P

----------------AEPAPG22----------------
 Document Identifier (6-12)        :942
 Entity Role Code (18-20)          :CI
 Declaration Code (21-24)          :EP2
 Declaration Certification (25-25) :Y
 Date Of Signature (26-33)         :01-Aug-16

----------------AEPAPG01----------------
 P G A Line Number (5-7)                :4
 Government Agency Code (8-10)          :EPA
 Government Agency Program Code (11-13) :PS1

----------------AEPAPG02----------------
 Item Type (5-5) :P", content);
			}
		}

		[TestDate(2016, 11, 21)]
		public void TestHasODSOrTSCALinesToBeSent()
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.PGADataCorrection2ndPhase, Core.Constants.CountryCodes.UnitedStates, ZDateTime.Today, false))
			{
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
				declaration.US_EnableENS = true;
				declaration.US_EntryFilerCode = "XJ5";

				declaration.Invoices.AddNew();
				var invoiceLine = declaration.InvoiceLines.AddNew();
				declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
				IPGAGovernmentAgenciesCommon line = new PGAGovernmentAgenciesWithPGACorrection(invoiceLine.CusEntryLine, invoiceLine.CusEntryLine);
				AssertEquals(false, line.HasODSOrTSCALinesToBeSent());
				AssertEquals(false, line.HasNHTSALinesToBeSent());
				AssertEquals(false, line.HasPGARequiringDeclarationDate());

				invoiceLine.US_ODSInd = OGAIndicatorList.Codes.Declared;
				AssertEquals(true, line.HasODSOrTSCALinesToBeSent());
				AssertEquals(false, line.HasNHTSALinesToBeSent());
				AssertEquals(false, line.HasPGARequiringDeclarationDate());

				invoiceLine.US_ODSInd = ZString.Empty;
				invoiceLine.US_TSCAInd = OGAIndicatorList.Codes.Declared;
				AssertEquals(true, line.HasODSOrTSCALinesToBeSent());
				AssertEquals(false, line.HasNHTSALinesToBeSent());
				AssertEquals(false, line.HasPGARequiringDeclarationDate());

				invoiceLine.US_TSCAInd = ZString.Empty;
				invoiceLine.US_NHTSAIndicator = OGAIndicatorList.Codes.Declared;
				invoiceLine.NHTSALines.AddNew();
				AssertEquals(false, line.HasODSOrTSCALinesToBeSent());
				AssertEquals(true, line.HasNHTSALinesToBeSent());
				AssertEquals(true, line.HasPGARequiringDeclarationDate());

				invoiceLine.US_NHTSAIndicator = ZString.Empty;
				invoiceLine.NHTSALines.RemoveAndDeleteAll();
				invoiceLine.US_VNEInd = OGAIndicatorList.Codes.Declared;
				invoiceLine.VehicleLines.AddNew();
				AssertEquals(false, line.HasODSOrTSCALinesToBeSent());
				AssertEquals(false, line.HasNHTSALinesToBeSent());
				AssertEquals(true, line.HasPGARequiringDeclarationDate());

				invoiceLine.US_VNEInd = ZString.Empty;
				invoiceLine.VehicleLines.RemoveAndDeleteAll();
				invoiceLine.US_PSTIndicator = OGAIndicatorList.Codes.Declared;
				invoiceLine.PSTLines.AddNew();
				AssertEquals(false, line.HasODSOrTSCALinesToBeSent());
				AssertEquals(false, line.HasNHTSALinesToBeSent());
				AssertEquals(true, line.HasPGARequiringDeclarationDate());

				invoiceLine.US_PSTIndicator = ZString.Empty;
				invoiceLine.PSTLines.RemoveAndDeleteAll();
				invoiceLine.US_FSISInd = OGAIndicatorList.Codes.Declared;
				invoiceLine.FSISLines.AddNew();
				AssertEquals(false, line.HasODSOrTSCALinesToBeSent());
				AssertEquals(false, line.HasNHTSALinesToBeSent());
				AssertEquals(true, line.HasPGARequiringDeclarationDate());

				invoiceLine.US_FSISInd = ZString.Empty;
				invoiceLine.FSISLines.RemoveAndDeleteAll();
				invoiceLine.US_LaceyIndicator = OGAIndicatorList.Codes.Declared;
				invoiceLine.LaceyActLines.AddNew();
				AssertEquals(false, line.HasODSOrTSCALinesToBeSent());
				AssertEquals(false, line.HasNHTSALinesToBeSent());
				AssertEquals(true, line.HasPGARequiringDeclarationDate());

				invoiceLine.US_LaceyIndicator = ZString.Empty;
				invoiceLine.LaceyActLines.RemoveAndDeleteAll();
				invoiceLine.US_FWSInd = OGAIndicatorList.Codes.Declared;
				invoiceLine.FWSHeaders.AddNew();
				AssertEquals(false, line.HasODSOrTSCALinesToBeSent());
				AssertEquals(false, line.HasNHTSALinesToBeSent());
				AssertEquals(true, line.HasPGARequiringDeclarationDate());
			}
		}

		public void TestNMFSCOA()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = JobMessageTypeList.Codes.Import;
			dec.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			dec.US_EntryFilerCode = "XJ5";
			dec.US_EnableENS = true;

			dec.Invoices.AddNew();
			var invoiceLine = dec.InvoiceLines.AddNew();
			invoiceLine.US_NMFSCOAInd = OGAIndicatorList.Codes.Declared;
			var nmfsCOALine = invoiceLine.NMFSLines.AddNew();
			nmfsCOALine.US_ProgramType = NMFSProgramCodeList.Codes.COA;

			dec.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var line = (IGovernmentAgenciesCommon)new PGAGovernmentAgenciesWithPGACorrection(invoiceLine.CusEntryLine, invoiceLine.CusEntryLine);
			AssertEquals(1, line.NMFSCOALines.Count());
			AssertEquals("D", line.NMFSCOAIndicator);
		}

		public void TestNMFSCOAHasLinesToSend()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";

			declaration.Invoices.AddNew();
			var invoiceLine = declaration.InvoiceLines.AddNew();
			var invoiceLine1 = declaration.InvoiceLines.AddNew();
			var nmfsCOALine = invoiceLine.NMFSLines.AddNew();
			nmfsCOALine.US_ProgramType = NMFSProgramCodeList.Codes.COA;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			var line = new PGAGovernmentAgenciesWithPGACorrection(invoiceLine.CusEntryLine, invoiceLine.CusEntryLine);
			AssertEquals(true, line.HasLinesToSend());

			line = new PGAGovernmentAgenciesWithPGACorrection(invoiceLine1.CusEntryLine, invoiceLine1.CusEntryLine);
			AssertEquals(false, line.HasLinesToSend());
		}

		public void TestHFC()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = JobMessageTypeList.Codes.Import;
			dec.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			dec.US_EntryFilerCode = "XJ5";
			dec.US_EnableENS = true;

			dec.Invoices.AddNew();
			var invoiceLine = dec.InvoiceLines.AddNew();
			invoiceLine.US_HFCInd = OGAIndicatorList.Codes.Disclaimed;
			invoiceLine.US_HFCDisclaimReason = PGADisclaimReasonList.Codes.A;
			dec.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			var line = (IGovernmentAgenciesCommon)new PGAGovernmentAgenciesWithPGACorrection(invoiceLine.CusEntryLine, invoiceLine.CusEntryLine);
			AssertEquals(OGAIndicatorList.Codes.Disclaimed, line.HFCIndicator);
			AssertEquals(PGADisclaimReasonList.Codes.A, line.HFCDisclaimReason);

			invoiceLine.US_HFCInd = OGAIndicatorList.Codes.Declared;
			var hfcHeader = invoiceLine.USHFCHeaders.AddNew();
			dec.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			line = new PGAGovernmentAgenciesWithPGACorrection(invoiceLine.CusEntryLine, invoiceLine.CusEntryLine);
			AssertEquals(1, line.EPA_HFCHeaders.Count());
			AssertEquals(OGAIndicatorList.Codes.Declared, line.HFCIndicator);
		}

		public void TestHFCHasLinesToSend()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = JobMessageTypeList.Codes.Import;
			dec.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			dec.US_EntryFilerCode = "XJ5";
			dec.US_EnableENS = true;

			dec.Invoices.AddNew();
			var invoiceLine = dec.InvoiceLines.AddNew();
			var invoiceLine1 = dec.InvoiceLines.AddNew();
			invoiceLine.US_HFCInd = OGAIndicatorList.Codes.Declared;
			var hfcHeader = invoiceLine.USHFCHeaders.AddNew();

			dec.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			var line = new PGAGovernmentAgenciesWithPGACorrection(invoiceLine.CusEntryLine, invoiceLine.CusEntryLine);
			AssertEquals(true, line.HasLinesToSend());

			line = new PGAGovernmentAgenciesWithPGACorrection(invoiceLine1.CusEntryLine, invoiceLine1.CusEntryLine);
			AssertEquals(false, line.HasLinesToSend());
		}
	}
}
