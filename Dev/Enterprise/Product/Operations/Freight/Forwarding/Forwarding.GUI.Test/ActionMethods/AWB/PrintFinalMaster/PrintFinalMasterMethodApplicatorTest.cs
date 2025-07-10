using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.AWB.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Registry;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Services.OperationalActions.Support.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.GUI.Testing
{
	[TestedType(typeof(PrintFinalMasterMethodApplicator))]
	public class PrintFinalMasterMethodApplicatorTest : OperationalActionMethodApplicatorTest
	{
		public void TestOnCancel()
		{
			const string expectedLog = "ERROR: Operation was canceled.";

			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Cancel;
			ApplyApplicator(new BusinessObject[] { Factory.New<ForwardingConsol>() }, expectedLog);
		}

		public void TestNoConsols()
		{
			const string expectedLog = "ERROR: No consols selected.";

			ApplyApplicator(Array.Empty<BusinessObject>(), expectedLog);
		}

		[TestDate(2012, 12, 20)]
		public void TestSkip()
		{
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
			Settings.OnErrorMessageError = AWBPrintSettings.Codes.Skip;
			Applicator.PrintMasterAirWaybill = ZBool.True;
			Applicator.MAWBPrinter = CreatePrinterPK();
			Applicator.AllowPrintWithMessageErrors = ZBool.True;
			Applicator.PrintConsignmentSecurityDeclaration = ZBool.False;

			var consolWithoutErrors = CreateConsolWithoutErrors();
			consolWithoutErrors.JK_UniqueConsignRef = "GOODCONSOL";

			var consolWithErrors = Factory.New<ForwardingConsol>();
			consolWithErrors.JK_UniqueConsignRef = "BADCONSOL";
			consolWithErrors.JK_TransportMode = Core.Constants.TransportModes.Sea;

			Factory.Save();
			string expectedLog =
				"WARNING: [HL BADCONSOL] is not an air consol and was skipped.\n" +
				"INFO: [HL GOODCONSOL] processed successfully.";
			ApplyApplicator(new BusinessObject[] { consolWithoutErrors, consolWithErrors }, expectedLog);

			Applicator.AllowPrintWithMessageErrors = ZBool.False;

			consolWithErrors.JK_TransportMode = Core.Constants.TransportModes.Air;
			RemoveMessageErrorsFromAWB(consolWithErrors);
			var accountingInfo = consolWithErrors.AWBHeader.AWBAccountingInformations.AddNew();
			accountingInfo.EA_InformationID = "D";

			Factory.Save();
			expectedLog =
				"WARNING: [HL GOODCONSOL] has errors/message errors and was skipped.\n" +
				"WARNING: [HL BADCONSOL] has errors/message errors and was skipped.\n" +
				"ERROR: All selected consols were skipped. See above for details.";
			ApplyApplicator(new BusinessObject[] { consolWithoutErrors, consolWithErrors }, expectedLog);

			Applicator.AllowPrintWithMessageErrors = ZBool.True;
			Applicator.SendFWB = ZBool.True;

			Factory.Save();
			expectedLog =
				"WARNING: FWB Message Cannot Be Sent Without a Complete MAWB Number.\n" +
				"WARNING: FWB Message Cannot Be Sent while there are Message Errors on the AWB.\n" +
				"WARNING: [HL GOODCONSOL] has skipped electronic messaging for the above reasons.\n" +
				"INFO: [HL GOODCONSOL] processed successfully.";
			ApplyApplicator(new BusinessObject[] { consolWithoutErrors }, expectedLog);

			var unsecuredConsol = CreateConsolWithoutErrors();
			unsecuredConsol.JK_UniqueConsignRef = "UNSECUREDCONSOL";
			unsecuredConsol.AWBHeader.AWBSpecialHandlingItems.RemoveAndDeleteAll();
			var securityStatus = unsecuredConsol.AWBHeader.AWBSpecialHandlingItems.AddNew();
			securityStatus.EP_SpecialHandling = AWBSpecialHandlingCodeDescriptionPairList.Codes.CargoHasNotBeenSecuredYetForPassengerOrAllCargoAircraft;
			Applicator.SendFWB = ZBool.False;

			Factory.Save();
			expectedLog =
				"INFO: [HL UNSECUREDCONSOL] processed successfully.";
			ApplyApplicator(new BusinessObject[] { unsecuredConsol }, expectedLog);

			Applicator.PrintConsignmentSecurityDeclaration = ZBool.True;
			Factory.Save();
			expectedLog = "WARNING: The eCSD can only be issued for secured Consolidations – those with Security Status \"SPX\", \"SCO\" or \"SHR\".\n" +
				"WARNING: [HL UNSECUREDCONSOL] has skipped electronic messaging for the above reasons.\n" +
				"INFO: [HL UNSECUREDCONSOL] processed successfully.";
			ApplyApplicator(new BusinessObject[] { unsecuredConsol }, expectedLog);

			Applicator.AllowPrintWithMessageErrors = ZBool.False;
			securityStatus = unsecuredConsol.AWBHeader.AWBSpecialHandlingItems.AddNew();
			securityStatus.EP_SpecialHandling = AWBSpecialHandlingCodeDescriptionPairList.Codes.CargoHasNotBeenSecuredYetForPassengerOrAllCargoAircraft;
			Factory.Save();
			expectedLog =
				"WARNING: The eCSD can only be issued for secured Consolidations – those with Security Status \"SPX\", \"SCO\" or \"SHR\".\n" +
				"WARNING: [HL UNSECUREDCONSOL] was skipped for the above reasons.\n" +
				"ERROR: All selected consols were skipped. See above for details.";
			ApplyApplicator(new BusinessObject[] { unsecuredConsol }, expectedLog);

			securityStatus.EP_SpecialHandling = AWBSpecialHandlingCodeDescriptionPairList.Codes.CargoSecureForPassengerAndAllCargoAircraft;
			Factory.Save();
			expectedLog =
				"WARNING: The eCSD can only be issued for secured Consolidations – those with Security Status \"SPX\", \"SCO\" or \"SHR\".\n" +
				"WARNING: [HL UNSECUREDCONSOL] was skipped for the above reasons.\n" +
				"ERROR: All selected consols were skipped. See above for details.";
			ApplyApplicator(new BusinessObject[] { unsecuredConsol }, expectedLog);
		}

		[TestDate(2012, 12, 20)]
		public void TestAbortSendFHLForAWBHeaderErrors()
		{
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
			Settings.OnErrorMessageError = AWBPrintSettings.Codes.Abort;
			Applicator.PrintMasterAirWaybill = ZBool.True;
			Applicator.MAWBPrinter = CreatePrinterPK();
			Applicator.SendFHL = true;

			ForwardingConsol consolWithoutErrors = CreateConsolWithoutErrors();
			consolWithoutErrors.JK_UniqueConsignRef = "GOODCONSOL";
			RemoveMessageErrorsFromAWB(consolWithoutErrors);

			var shipment = consolWithoutErrors.Shipments.AddNew();

			Factory.Save();

			var expectedLog = "ERROR: [HL GOODCONSOL] has errors and/or message errors.\n" +
				"ERROR: Operation failed. See errors above.";
			ApplyApplicator(new BusinessObject[] { consolWithoutErrors }, expectedLog);
		}

		[TestDate(2012, 12, 20)]
		public void TestNoFHLValidationWithOnlyFWBTicked()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("JM"))
			{
				GlbBranch.CurrentBranch.HomePort.RL_IATA = "XXX";
				ForwardingConfigurationRegistry.Instance.SenderPIMAAddress.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "4645135");
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				Settings.OnErrorMessageError = AWBPrintSettings.Codes.Abort;
				Applicator.PrintMasterAirWaybill = ZBool.True;
				Applicator.MAWBPrinter = CreatePrinterPK();
				Applicator.AllowPrintWithMessageErrors = ZBool.True;

				var consolWithoutErrors = CreateConsolWithoutErrors();
				consolWithoutErrors.JK_UniqueConsignRef = "GOODCONSOL";
				consolWithoutErrors.JK_MasterBillNum = "12345678901";
				RemoveMessageErrorsFromAWB(consolWithoutErrors);
				var shipment = consolWithoutErrors.Shipments.AddNew();
				shipment.JS_InspectionTypeCode = BaseJobShipmentLookups.InspectionType_Approved;
				Factory.Save();

				Applicator.SendFWB = true;
				Applicator.SendFHL = true;
				var expectedLog = "ERROR: FHL Message Cannot Be Sent while there are Message Errors on the AWB.\n" +
					"ERROR: [HL GOODCONSOL] has errors and/or message errors.\n" +
					"ERROR: Operation failed. See errors above.";
				ApplyApplicator(new BusinessObject[] { consolWithoutErrors }, expectedLog);

				Applicator.SendFHL = false;
				Factory.Save();
				expectedLog = "INFO: [HL GOODCONSOL] processed successfully.";
				ApplyApplicator(new BusinessObject[] { consolWithoutErrors }, expectedLog);
			}
		}

		[TestDate(2012, 12, 20)]
		public void TestAbort()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("JM"))
			{
				GlbBranch.CurrentBranch.HomePort.RL_IATA = "XXX";
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				Settings.OnErrorMessageError = AWBPrintSettings.Codes.Abort;
				Applicator.PrintMasterAirWaybill = ZBool.True;
				Applicator.MAWBPrinter = CreatePrinterPK();
				Applicator.AllowPrintWithMessageErrors = ZBool.True;

				ForwardingConsol consolWithoutErrors = CreateConsolWithoutErrors();
				consolWithoutErrors.JK_UniqueConsignRef = "GOODCONSOL";
				RemoveMessageErrorsFromAWB(consolWithoutErrors);

				Factory.Save();
				string expectedLog = "INFO: [HL GOODCONSOL] processed successfully.";
				ApplyApplicator(new BusinessObject[] { consolWithoutErrors }, expectedLog);

				ForwardingConsol consolWithErrors = Factory.New<ForwardingConsol>();
				consolWithErrors.JK_UniqueConsignRef = "BADCONSOL";
				consolWithErrors.JK_TransportMode = Core.Constants.TransportModes.Sea;

				Factory.Save();
				expectedLog = "ERROR: [HL BADCONSOL] is not an air consol.";
				ApplyApplicator(new BusinessObject[] { consolWithoutErrors, consolWithErrors }, expectedLog);

				consolWithErrors.JK_TransportMode = Core.Constants.TransportModes.Air;

				Factory.Save();
				expectedLog = "ERROR: [HL BADCONSOL] has errors and/or message errors.\n" +
							"ERROR: Operation failed. See errors above.";
				ApplyApplicator(new BusinessObject[] { consolWithoutErrors, consolWithErrors }, expectedLog);

				Applicator.SendFWB = ZBool.True;
				expectedLog =
					"ERROR: FWB Message Cannot Be Sent Without a Complete MAWB Number.\n" +
					"ERROR: [HL GOODCONSOL] has errors and/or message errors.\n" +
					"ERROR: Operation failed. See errors above.";
				ApplyApplicator(new BusinessObject[] { consolWithoutErrors }, expectedLog);

				ForwardingConfigurationRegistry.Instance.SenderPIMAAddress.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "4645135");
				consolWithoutErrors.JK_MasterBillNum = "12345678901";
				Factory.Save();
				expectedLog = "INFO: [HL GOODCONSOL] processed successfully.";
				ApplyApplicator(new BusinessObject[] { consolWithoutErrors }, expectedLog);
			}
		}

		[TestDate(2012, 12, 20)]
		public void TestConsolAction_PrintConsignmentSecurityDeclaration_OnBulkAction()
		{
			var printer = Factory.New<StmPrintQueue>();
			printer.SQ_DisplayName = "DisplayName";
			printer.SQ_QueueName = "QueueName";
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
			Settings.OnErrorMessageError = AWBPrintSettings.Codes.Abort;
			Applicator.PrintMasterAirWaybill = ZBool.True;
			Applicator.MAWBPrinter = printer.PK;
			Applicator.PrintConsignmentSecurityDeclaration = false;
			var consolWithoutErrors = CreateConsolWithoutErrors();
			var consolWithoutErrors1 = CreateConsolWithoutErrors();
			var consolWithoutErrors2 = CreateConsolWithoutErrors();
			RemoveMessageErrorsFromAWB(consolWithoutErrors);
			RemoveMessageErrorsFromAWB(consolWithoutErrors1);
			RemoveMessageErrorsFromAWB(consolWithoutErrors2);
			Factory.Save();
			consolWithoutErrors.Shipments.AddNew();
			consolWithoutErrors.Shipments[0].JS_InspectionTypeCode = BaseJobShipmentLookups.InspectionType_Approved;

			var expectedLog = @"INFO: [HL 26FYS0PM3GCY3VHFAX26] processed successfully.
INFO: [HL LH368W538T2QSF50OJ3K] processed successfully.
INFO: [HL 3STEPRLMD6TIHXVN354Y] processed successfully.";
			ApplyApplicator(new BusinessObject[] { consolWithoutErrors, consolWithoutErrors1, consolWithoutErrors2 }, expectedLog);

			var filter = new ZQuery(StmPrintJobSchema.SP_SQ, printer.PK);
			var printJobs = Factory.Load<StmPrintJob>(filter);
			AssertEquals("6 MAWB and 0 Consignment Security Declaration print job is created", 6, printJobs.Length);

			printer = Factory.New<StmPrintQueue>();
			printer.SQ_DisplayName = "DisplayName1";
			printer.SQ_QueueName = "QueueName1";
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
			Settings.OnErrorMessageError = AWBPrintSettings.Codes.Abort;
			Applicator.PrintMasterAirWaybill = ZBool.True;
			Applicator.MAWBPrinter = printer.PK;
			Applicator.PrintConsignmentSecurityDeclaration = true;
			consolWithoutErrors = CreateConsolWithoutErrors();
			consolWithoutErrors1 = CreateConsolWithoutErrors();
			consolWithoutErrors2 = CreateConsolWithoutErrors();
			RemoveMessageErrorsFromAWB(consolWithoutErrors);
			RemoveMessageErrorsFromAWB(consolWithoutErrors1);
			RemoveMessageErrorsFromAWB(consolWithoutErrors2);
			Factory.Save();
			consolWithoutErrors.Shipments.AddNew();
			consolWithoutErrors.Shipments[0].JS_InspectionTypeCode = BaseJobShipmentLookups.InspectionType_Approved;

			expectedLog = @"INFO: [HL N2IM4N14HKJB5GKAJS5C] processed successfully.
INFO: [HL 5D7TLIIMMXA2UYAWXF6Q] processed successfully.
INFO: [HL ONW11EX4QB0UJIYJD073] processed successfully.";
			ApplyApplicator(new BusinessObject[] { consolWithoutErrors, consolWithoutErrors1, consolWithoutErrors2 }, expectedLog);

			filter = new ZQuery(StmPrintJobSchema.SP_SQ, printer.PK);
			printJobs = Factory.Load<StmPrintJob>(filter);
			AssertEquals("6 MAWB and 3 Consignment Security Declaration print job is created", 9, printJobs.Length);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return new PrintFinalMasterMethodApplicator(Settings, Factory);
		}

		AWBPrintSettings Settings
		{
			get { return settings ?? (settings = new AWBPrintSettings()); }
		}
		AWBPrintSettings settings;

		ForwardingConsol CreateConsolWithoutErrors()
		{
			ForwardingConsol consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_ConsolMode = Core.Constants.ContainerModes.Loose;
			consol.JK_RL_NKDischargePort = "JMKIN";
			consol.JK_RL_NKLoadPort = "NZAKL";
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.JK_MasterBillNum = "081";
			var securityStatus = consol.AWBHeader.AWBSpecialHandlingItems.AddNew();
			securityStatus.EP_SpecialHandling = AWBSpecialHandlingCodeDescriptionPairList.Codes.CargoSecureForPassengerAndAllCargoAircraft;
			return consol;
		}

		ZGuid CreatePrinterPK()
		{
			StmPrintQueue printer = Factory.New<StmPrintQueue>();
			printer.SQ_DisplayName = "DisplayName";
			printer.SQ_QueueName = "QueueName";
			return printer.PK;
		}

		void RemoveMessageErrorsFromAWB(ForwardingConsol consol)
		{
			consol.JK_OverrideWaybillDefaults = ZBool.True;

			ExportAWBHeader awbHeader = consol.AWBHeader;
			awbHeader.AWBRateLines[0].ER_GrossWeight = 5;
			awbHeader.AWBRateLines[0].ER_NoOfPiecesOrRCP = "2";
			awbHeader.AWBRateLines[0].ER_WeightInLBsOrKGs = Core.Constants.AWB.RateLineUQ.Kilos;
			awbHeader.EH_By1st = "AU";
			awbHeader.EH_ChargesCode = ExportAWBHeader.Constants.ChargeCodes.AllChargesCollect;
			awbHeader.EH_ConsigneeAddress = "742 Evergreen Terrace";
			awbHeader.EH_ConsigneeCountryCode = "US";
			awbHeader.EH_ConsigneeName = "Captain Planet";
			awbHeader.EH_ConsigneePlace = "Earth";
			awbHeader.EH_ShipperAddress = "72 ORiordan St";
			awbHeader.EH_ShipperCountryCode = "AU";
			awbHeader.EH_ShipperName = "CargoWise";
			awbHeader.EH_ShipperPlace = "Alexandria";
			awbHeader.EH_AirportOfDestinationCode = "USA";
			awbHeader.EH_AWBIssueDate = new ZDateTime(2012, 6, 6);
			awbHeader.EH_AWBIssuePlace = "Ogdenville";
			awbHeader.EH_AWBOriginCode = "AUS";
			awbHeader.EH_Currency = "AUD";

			Env.Registry.Freight.AirWaybill.IssuingCarrierAgentIATACode = "1234567";
			Env.Registry.Freight.AirWaybill.IssuingCarrierAgentName = "Agent Smith";
			Env.Registry.Freight.AirWaybill.IssuingCarrierAgentCity = "The Matrix";
		}

		#region TestFetchHintsForConsols

		public void TestFetchHintsForConsolsOnPrintFinalMaster()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();

			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
			Settings.OnErrorMessageError = AWBPrintSettings.Codes.Skip;
			Applicator.SendFWB = true;
			Applicator.SendFHL = true;
			Applicator.MAWBPrinter = CreatePrinterPK();
			Applicator.AllowPrintWithMessageErrors = ZBool.True;

			factory.Save();

			factory.ResetDatabaseLoadCount();
			ForwardingConsol[] loadedConsols = factory.Load<ForwardingModuleConsol>(new ZQuery(JobConsolSchema.PK, CreateConsols()));

			foreach (ForwardingConsol loadedConsol in loadedConsols)
			{
				var count = loadedConsol.Shipments.Count;
			}

			factory.ResetDatabaseLoadCount();
			SimulateRun(loadedConsols, true);

			var refCountryDbHits = factory.TableSelects.Single(x => x.TableName == RefCountrySchema.Constants.TableName).Value;
			Assert("Uber factory may or may not share user context data with our factory depending on the state of the user context factory prior to this test run.", refCountryDbHits <= 2);

			var expected = new Dictionary<string, int>
			{
				{ StmNoteSchema.Constants.TableName, 2 },
				{ JobConsolCostSchema.Constants.TableName, 6 },
				{ JobMawbSchema.Constants.TableName, 5 },
				{ JobHeaderSchema.Constants.TableName, 2 },
				{ ExportAWBHeaderSchema.Constants.TableName, 6 },
				{ JobDeclarationSchema.Constants.TableName, 3 },
				{ JobDocAddressSchema.Constants.TableName, 3 },
				{ RefAirlineSchema.Constants.TableName, 1 },
				{ JobCartageSchema.Constants.TableName, 2 },
				{ JobOrderHeaderSchema.Constants.TableName, 2 },
				{ RefCountrySchema.Constants.TableName, refCountryDbHits },
				{ StmMenuItemSchema.Constants.TableName, 2 },
				{ EDIMessageSchema.Constants.TableName, 1 },
				{ ExportAWBAccountingInformationSchema.Constants.TableName, 3 },
				{ ExportAWBOtherChargesSchema.Constants.TableName, 3 },
				{ ExportAWBRateLineSchema.Constants.TableName, 3 },
				{ ExportAWBSecurityStatusLineSchema.Constants.TableName, 3 },
				{ ExportAWBSpecialHandlingSchema.Constants.TableName, 3 },
				{ CusEntryNumSchema.Constants.TableName, 3 },
				{ CusHAWBSchema.Constants.TableName, 2 },
				{ CusInBondHeaderSchema.Constants.TableName, 1 },
				{ CusSCAHouseSchema.Constants.TableName, 2 },
				{ JobConShipLinkSchema.Constants.TableName, 1 },
				{ JobConsolTransportSchema.Constants.TableName, 3 },
				{ JobContainerSchema.Constants.TableName, 1 },
				{ JobPackLinesSchema.Constants.TableName, 1 },
				{ OrgMiscServSchema.Constants.TableName, 1 },
				{ RefUNLOCOSchema.Constants.TableName, 1 },
				{ JobDocsAndCartageSchema.Constants.TableName, 1 },
				{ GlbStaffSchema.Constants.TableName, 0 },
				{ JobDocumentDataSchema.Constants.TableName, 4 },
				{ JobConsolAWBSpecialHandlingSchema.Constants.TableName, 4 },
				{ RefDocOrgCusCodeSchema.Constants.TableName, 3 },
				{ StmMenuDeliveryRestrictionSchema.Constants.TableName, 2 },
				{ JobCO2eSchema.Constants.TableName, 1 }
			};

			AssertDbHits(expected, factory);
		}

		List<ZGuid> CreateConsols()
		{
			List<ZGuid> result = new List<ZGuid>();
			BusinessObjectFactory factory = new BusinessObjectFactory();

			FreightDataRegistry.Instance.MAWBAccountingInfoExtraText.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new CodeDescriptionPairList
																													{
																														new CodeDescriptionPair("ABC", "FIRST DESCRIPTION"),
																														new CodeDescriptionPair("BBC", "<DocConsol.DirectShipment.DetailedDescriptionOfGoods>"),
																														new CodeDescriptionPair("CNN", "LAST DESCRIPTION"),
																													});
			for (int i = 1; i <= 4; i++)
			{
				var consol = factory.NewWithValidTestData<ForwardingConsol>();
				consol.JK_UniqueConsignRef = "0811234511" + i;
				consol.JK_ConsolMode = Core.Constants.ContainerModes.ULD;
				consol.JK_TransportMode = Core.Constants.TransportModes.Air;
				consol.JK_MasterBillNum = "0811234511" + i;
				consol.JK_PrepaidCollect = "PP";

				if (i == 1 || i == 2)
				{
					var cusEntryNum = consol.CusEntryNums.AddNew();
					cusEntryNum.CE_EntryNum = "CX34555" + i;
					cusEntryNum.CE_ParentTable = consol.TableName;
				}

				if (i == 3 || i == 4)
				{
					consol.JK_OverrideWaybillDefaults = true;
					consol.JK_OverrideSecurityDeclarationDefaults = true;
					var header = consol.AWBHeader;

					var shipment = consol.Shipments.AddNew();
					shipment.JS_RL_NKOrigin = "AUSYD";
					shipment.JS_RL_NKDestination = "NZAKL";
					shipment.JS_OverrideWaybillDefaults = true;
				}

				result.Add(consol.PK);
			}

			factory.Save();
			return result;
		}

		#endregion

		#endregion

		new PrintFinalMasterMethodApplicator Applicator => (PrintFinalMasterMethodApplicator)base.Applicator;
	}
}
