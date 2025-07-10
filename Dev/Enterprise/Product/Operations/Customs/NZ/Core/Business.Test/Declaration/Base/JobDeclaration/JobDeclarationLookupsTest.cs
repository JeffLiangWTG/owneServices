using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Customs.NZ.Business.MAFeBACCa.CodedLists;
using Enterprise.Customs.NZ.TradeSingleWindow;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.NZ.Business.Declaration.Testing
{
	public class JobDeclarationLookupsTest : TestCaseWithFactory
	{
		public void TestGoodsLocationList()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			declaration.JE_RL_NKFinalDestination = "NZWLG";
			declaration.JE_RL_NKPortOfArrival = "NZAKL";

			var forwardingHeader = Factory.NewWithValidTestData<OrgHeader>();
			forwardingHeader.OH_Code = "FWCODE";
			declaration.JE_OH_Forwarder = forwardingHeader.PK;

			var shippingLineHeader = Factory.NewWithValidTestData<OrgHeader>();
			shippingLineHeader.OH_Code = "SLCODE";
			declaration.JE_OH_ShippingLine = shippingLineHeader.PK;

			var warehouseHeader = Factory.NewWithValidTestData<OrgHeader>();
			warehouseHeader.OH_Code = "WHCODE";
			declaration.WarehouseDocAddress.E2_OA_Address = warehouseHeader.MainAddress.PK;

			var containerYardHeader = Factory.NewWithValidTestData<OrgHeader>();
			containerYardHeader.OH_Code = "CYCODE";
			declaration.ContainerYardDocAddress.E2_OA_Address = containerYardHeader.MainAddress.PK;

			var containerTerminalOperatorHeader = Factory.NewWithValidTestData<OrgHeader>();
			containerTerminalOperatorHeader.OH_Code = "CTCODE";
			declaration.ContainerTerminalOperatorDocAddress.E2_OA_Address = containerTerminalOperatorHeader.MainAddress.PK;

			var depotHeader = Factory.NewWithValidTestData<OrgHeader>();
			depotHeader.OH_Code = "DECODE";
			declaration.DepotDocAddress.E2_OA_Address = depotHeader.MainAddress.PK;

			forwardingHeader.CustomsCodes.AddNew(OrgCusCode.CodeTypes.ControlledPremisesID, "FW-OH-000", Core.Constants.CountryCodes.NewZealand);
			shippingLineHeader.CustomsCodes.AddNew(OrgCusCode.CodeTypes.ControlledPremisesID, "C0-OH-000", Core.Constants.CountryCodes.NewZealand);
			warehouseHeader.CustomsCodes.AddNew(OrgCusCode.CodeTypes.ControlledPremisesID, "BW-OH-000", Core.Constants.CountryCodes.NewZealand);
			containerYardHeader.CustomsCodes.AddNew(OrgCusCode.CodeTypes.ControlledPremisesID, "CY-OH-000", Core.Constants.CountryCodes.NewZealand);
			containerTerminalOperatorHeader.CustomsCodes.AddNew(OrgCusCode.CodeTypes.ControlledPremisesID, "CTO-OH-00", Core.Constants.CountryCodes.NewZealand);
			depotHeader.CustomsCodes.AddNew(OrgCusCode.CodeTypes.ControlledPremisesID, "D0-OH-000", Core.Constants.CountryCodes.NewZealand);
			depotHeader.CustomsCodes.AddNew(OrgCusCode.NZCodeTypes.ApprovedTransitionalFacility, "D1-OH-000", Core.Constants.CountryCodes.NewZealand);
			depotHeader.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CustomsClientCode, "D2-OH-000", Core.Constants.CountryCodes.NewZealand);

			forwardingHeader.MainAddress.CustomsCodes.AddNew(OrgCusCode.CodeTypes.ControlledPremisesID, "FW-OA-000", Core.Constants.CountryCodes.NewZealand);
			shippingLineHeader.MainAddress.CustomsCodes.AddNew(OrgCusCode.CodeTypes.ControlledPremisesID, "C0-OA-000", Core.Constants.CountryCodes.NewZealand);
			warehouseHeader.MainAddress.CustomsCodes.AddNew(OrgCusCode.CodeTypes.ControlledPremisesID, "BW-OA-000", Core.Constants.CountryCodes.NewZealand);
			containerYardHeader.MainAddress.CustomsCodes.AddNew(OrgCusCode.CodeTypes.ControlledPremisesID, "CY-OA-000", Core.Constants.CountryCodes.NewZealand);
			containerTerminalOperatorHeader.MainAddress.CustomsCodes.AddNew(OrgCusCode.CodeTypes.ControlledPremisesID, "CTO-OA-00", Core.Constants.CountryCodes.NewZealand);
			depotHeader.MainAddress.CustomsCodes.AddNew(OrgCusCode.CodeTypes.ControlledPremisesID, "D0-OA-000", Core.Constants.CountryCodes.NewZealand);
			depotHeader.MainAddress.CustomsCodes.AddNew(OrgCusCode.NZCodeTypes.ApprovedTransitionalFacility, "D1-OA-000", Core.Constants.CountryCodes.NewZealand);
			depotHeader.MainAddress.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CustomsClientCode, "D2-OA-000", Core.Constants.CountryCodes.NewZealand);

			void AssertGoodsLocationList(string goodsLocatedAt, string[] expectedList)
			{
				declaration.JE_GoodsLocatedAt = goodsLocatedAt;

				var actualList = declaration.Lookups
					.GoodsLocationList
					.Cast<ICodeDescription>()
					.Select(c => $"{c.Code}|{c.Description}")
					.ToArray();

				AssertArrayEqualsByElements($"The list should be {string.Join(", ", expectedList)} when JE_GoodsLocatedAt is {goodsLocatedAt}.", expectedList, actualList);
			}

			CombineAssertions(() =>
			{
				AssertGoodsLocationList(GoodsLocatedAtListForSeaImport.Codes.DES, new[] { "NZWLG|NZWLG" });
				AssertGoodsLocationList(GoodsLocatedAtListForSeaImport.Codes.DIS, new[] { "NZAKL|NZAKL" });

				AssertGoodsLocationList(GoodsLocatedAtList.Codes.BW, new[] { $"BW-OH-000|CCP - {warehouseHeader.HumanReadableName}", $"BW-OA-000|CCP - {warehouseHeader.MainAddress.HumanReadableName}" });
				AssertGoodsLocationList(GoodsLocatedAtList.Codes.C, new[] { $"C0-OH-000|CCP - {shippingLineHeader.HumanReadableName}", $"C0-OA-000|CCP - {shippingLineHeader.MainAddress.HumanReadableName}" });
				AssertGoodsLocationList(GoodsLocatedAtList.Codes.FW, new[] { $"FW-OH-000|CCP - {forwardingHeader.HumanReadableName}", $"FW-OA-000|CCP - {forwardingHeader.MainAddress.HumanReadableName}" });
				AssertGoodsLocationList(GoodsLocatedAtList.Codes.CTO, new[] { $"CTO-OH-00|CCP - {containerTerminalOperatorHeader.HumanReadableName}", $"CTO-OA-00|CCP - {containerTerminalOperatorHeader.MainAddress.HumanReadableName}" });
				AssertGoodsLocationList(GoodsLocatedAtList.Codes.CY, new[] { $"CY-OH-000|CCP - {containerYardHeader.HumanReadableName}", $"CY-OA-000|CCP - {containerYardHeader.MainAddress.HumanReadableName}" });
				AssertGoodsLocationList
				(
					GoodsLocatedAtList.Codes.D,
					new[]
					{
						$"D0-OH-000|CCP - {depotHeader.HumanReadableName}",
						$"D0-OA-000|CCP - {depotHeader.MainAddress.HumanReadableName}",
						$"D1-OH-000|ATF - {depotHeader.HumanReadableName}",
						$"D1-OA-000|ATF - {depotHeader.MainAddress.HumanReadableName}",
						$"D2-OH-000|CCD - {depotHeader.HumanReadableName}",
						$"D2-OA-000|CCD - {depotHeader.MainAddress.HumanReadableName}",
					});
			});
		}

		public void TestYesNoList()
		{
			AssertEquals(typeof(YesNoList), Declaration.Lookups.YesNoList.GetType());
		}

		public void TestLocationList()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			Declaration.JE_TransportMode = JobTransportModeList.Codes.Air;
			AssertEquals("LocationList", typeof(GoodsLocatedAtList), Declaration.Lookups.LocationList.GetType());

			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			Declaration.JE_TransportMode = JobTransportModeList.Codes.Sea;
			AssertEquals("LocationList", typeof(GoodsLocatedAtListForSeaImport), Declaration.Lookups.LocationList.GetType());

			Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			Declaration.JE_TransportMode = JobTransportModeList.Codes.Air;
			AssertEquals("LocationList", typeof(GoodsLocatedAtList), Declaration.Lookups.LocationList.GetType());

			Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			Declaration.JE_TransportMode = JobTransportModeList.Codes.Sea;
			AssertEquals("LocationList", typeof(GoodsLocatedAtListForSeaExport), Declaration.Lookups.LocationList.GetType());

			Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
			Declaration.JE_TransportMode = JobTransportModeList.Codes.Air;
			AssertEquals("LocationList", typeof(GoodsLocatedAtList), Declaration.Lookups.LocationList.GetType());

			Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
			Declaration.JE_TransportMode = JobTransportModeList.Codes.Sea;
			AssertEquals("LocationList", typeof(GoodsLocatedAtListForSeaExport), Declaration.Lookups.LocationList.GetType());
		}

		public void TestAcceptableCurrencyListContainsEUR()
		{
			AssertEquals("AcceptableNZCustomCurrencyList.Contains('EUR')", true, Declaration.Lookups.AcceptableNZCustomCurrencyList.ContainsCode("EUR"));
		}

		public void TestAcceptableCurrencyListContainsNZD()
		{
			AssertEquals("AcceptableNZCustomCurrencyList.Contains('NZD')", true, Declaration.Lookups.AcceptableNZCustomCurrencyList.ContainsCode("NZD"));
		}

		public void TestOrigins()
		{
			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertAdditionaFilterSetCorrectlyForOrigins(Declaration.Lookups.Origins, localPortGetsAnError: true, osPortGetsAnError: false);
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertAdditionaFilterSetCorrectlyForOrigins(Declaration.Lookups.Origins, localPortGetsAnError: false, osPortGetsAnError: true);

			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertAdditionaFilterSetCorrectlyForOrigins(Declaration.Lookups.Origins, localPortGetsAnError: true, osPortGetsAnError: false);
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertAdditionaFilterSetCorrectlyForOrigins(Declaration.Lookups.Origins, localPortGetsAnError: false, osPortGetsAnError: false);
		}

		void AssertAdditionaFilterSetCorrectlyForOrigins(RefUNLOCOCollection list, bool localPortGetsAnError, bool osPortGetsAnError)
		{
			ZQuery additionalFilter = list.Relationship.RelationshipFilter;
			ZQuery localFilter = new ZQuery(additionalFilter);
			localFilter.AddToFilter(RefUNLOCOSchema.RL_Code, "NZAKL");
			AssertEquals("Checking AdditionalValidation on 'NZAKL' - HasError", localPortGetsAnError, Factory.LoadTop1<RefUNLOCO>(localFilter) == null);
			ZQuery osFilter = new ZQuery(additionalFilter);
			osFilter.AddToFilter(RefUNLOCOSchema.RL_Code, "GBTIL");
			AssertEquals("Checking AdditionalValidation on 'GBTIL' - HasError", osPortGetsAnError, Factory.LoadTop1<RefUNLOCO>(osFilter) == null);
		}

		public void TestJobMessageSubTypeList()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals("Lookups.MessageSubTypeList.GetType()", typeof(JobMessageSubTypeForImportList), Declaration.Lookups.MessageSubTypeList.GetType());
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertEquals("Lookups.MessageSubTypeList.GetType()", typeof(JobMessageSubTypeForExportList), Declaration.Lookups.MessageSubTypeList.GetType());
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Excise;
			AssertEquals("Lookups.MessageSubTypeList.GetType()", typeof(JobMessageSubTypeForExciseList), Declaration.Lookups.MessageSubTypeList.GetType());
			Declaration.JE_MessageType = ZString.Empty;
			AssertEquals("Lookups.MessageSubTypeList.GetType()", typeof(JobMessageSubTypeList), Declaration.Lookups.MessageSubTypeList.GetType());
		}

		public void TestStatusList()
		{
			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
			AssertNoExceptionThrown("It should not thrown any exception.", () =>
			{
				var testInstruction = "Some delivery instruction " + Declaration.Lookups.StatusList.GetDescriptionFromCode(LowValueConsignmentStatusList.Codes.ConsignmentWrittenOff);
			});
			AssertEquals("Lookups.StatusList.GetType()", typeof(LowValueConsignmentStatusList), Declaration.Lookups.StatusList.GetType());
			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			AssertEquals("Lookups.StatusList.GetType()", typeof(StatusList), Declaration.Lookups.StatusList.GetType());
		}

		public void TestEntryStatusList()
		{
			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
			AssertEquals("Lookups.EntryStatusList.GetType()", typeof(LowValueConsignmentStatusList), Declaration.Lookups.EntryStatusList.GetType());

			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			AssertEquals("Lookups.EntryStatusList.GetType()", typeof(CodeDescriptionPairList), Declaration.Lookups.EntryStatusList.GetType());
			Assert("EntryStatus List contains Consolidation status ReadyForConsolidation", Declaration.Lookups.EntryStatusList.ContainsCode("RFC"));
			Assert("EntryStatus List contains Consolidation status AppliedToConsolidation", Declaration.Lookups.EntryStatusList.ContainsCode("ATC"));
		}

		public void TestOriginalEntryTypeList()
		{
			AssertEquals("OriginalEntryTypeList.Contains('SIT')", true, Declaration.Lookups.OriginalEntryTypeList.ContainsCode(JobMessageSubTypeList.Codes.Sight));
			AssertEquals("OriginalEntryTypeList.Contains('TMP')", true, Declaration.Lookups.OriginalEntryTypeList.ContainsCode(JobMessageSubTypeList.Codes.Temporary));
			AssertEquals("OriginalEntryTypeList should not contain any other type code", false, Declaration.Lookups.OriginalEntryTypeList.ContainsCode(JobMessageSubTypeList.Codes.Normal));
		}

		public void TestTSWEntryStatusList()
		{
			AssertEquals("Lookups.TSWEntryStatusList.GetType()", typeof(TSWEntryStatusList), Declaration.Lookups.TSWEntryStatusList.GetType());
		}

		public void TestContainerModeList()
		{
			AssertEquals(typeof(ContainerModeList), Declaration.Lookups.ContainerModeList.GetType());
		}

		public void TestMessagingStatuses()
		{
			AssertEquals(typeof(MessagingStatusList), Declaration.Lookups.MessagingStatuses.GetType());
		}

		public void TestTransactionNatureList()
		{
			AssertNotNull("Lookups.TransactionNatureList", Declaration.Lookups.TransactionNatureList);
			AssertEquals(typeof(NatureOfTransactionList), Declaration.Lookups.TransactionNatureList.GetType());
			AssertEquals("10 = Goods for final purchase/sale", true, Declaration.Lookups.TransactionNatureList.ContainsCode("10"));
			AssertEquals("41 = Goods for repair and maintenance against payment", true, Declaration.Lookups.TransactionNatureList.ContainsCode("41"));
		}

		public void TestPaymentPartyList()
		{
			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			AssertEquals("Lookups.PaymentPartyList.GetType()", typeof(PaymentMethodList), Declaration.Lookups.PaymentPartyList.GetType());

			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			AssertEquals("Lookups.PaymentPartyList.GetType() should now return TSW Freight Payment code list", typeof(FreightPaymentMethodList), Declaration.Lookups.PaymentPartyList.GetType());
		}

		public void TestNotifyPartyOrganisations()
		{
			AssertEquals(typeof(OrgHeaderCollection), Declaration.Lookups.NotifyPartyOrganisations.GetType());
		}

		public void TestDeliveryDestinationPartyOrganisations()
		{
			AssertEquals(typeof(OrgHeaderCollection), Declaration.Lookups.DeliveryDestinationPartyOrganisations.GetType());
		}

		public void TestMPIPaymentMethods()
		{
			AssertNotNull("Lookups.MPIPaymentMethods", Declaration.Lookups.MPIPaymentMethods);
			AssertEquals(typeof(CodeDescriptionPairList), Declaration.Lookups.MPIPaymentMethods.GetType());
			AssertEquals("ACC", true, Declaration.Lookups.MPIPaymentMethods.ContainsCode(MAFPaymentMethodList.Codes.Account));
			AssertEquals("CSH", true, Declaration.Lookups.MPIPaymentMethods.ContainsCode(MAFPaymentMethodList.Codes.Cash));
			AssertEquals("OTH code should have been removed from the list", false, Declaration.Lookups.MPIPaymentMethods.ContainsCode(MAFPaymentMethodList.Codes.Other));
		}

		public void TestIncoTermList()
		{
			AssertEquals(typeof(IncoTermList), Declaration.Lookups.IncoTermList.GetType());
			Assert(Declaration.Lookups.IncoTermList.ContainsCode(IncoTermList.Codes.CarriageAndInsurancePaidTo));
			Assert(Declaration.Lookups.IncoTermList.ContainsCode(IncoTermList.Codes.CarriagePaidTo));
			Assert(Declaration.Lookups.IncoTermList.ContainsCode(IncoTermList.Codes.CostAndFreight));
			Assert(Declaration.Lookups.IncoTermList.ContainsCode(IncoTermList.Codes.CostInsuranceAndFreight));
			Assert(Declaration.Lookups.IncoTermList.ContainsCode(IncoTermList.Codes.DeliveredAtPlace));
			Assert(Declaration.Lookups.IncoTermList.ContainsCode(IncoTermList.Codes.DeliveredAtTerminal));
			Assert(Declaration.Lookups.IncoTermList.ContainsCode(IncoTermList.Codes.DeliveredAtPlaceUnloaded));
			Assert(Declaration.Lookups.IncoTermList.ContainsCode(IncoTermList.Codes.DeliveredDutyPaid));
			Assert(Declaration.Lookups.IncoTermList.ContainsCode(IncoTermList.Codes.ExWorks));
			Assert(Declaration.Lookups.IncoTermList.ContainsCode(IncoTermList.Codes.FreeAlongsideShip));
			Assert(Declaration.Lookups.IncoTermList.ContainsCode(IncoTermList.Codes.FreeCarrier));
			Assert(Declaration.Lookups.IncoTermList.ContainsCode(IncoTermList.Codes.FreeOnBoard));
		}

		public void TestNotifyParties()
		{
			AssertType<OrgHeaderCollection>("NotifyParties", Declaration.Lookups.NotifyParties);
		}

		public void TestDeliveryNotificationCCPATFOrganisations()
		{
			AssertType<OrgHeaderCollection>("DeliveryNotificationCCPATFOrganisations", Declaration.Lookups.DeliveryNotificationCCPATFOrganisations);
		}

		public void TestDeliveryNotificationPortList()
		{
			Assert("Should be cached.", ReferenceEquals(Declaration.Lookups.DeliveryNotifyPortList, Declaration.Lookups.DeliveryNotifyPortList));
			AssertNotNull("We do have this port in DB.", Factory.Load<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, "NZAKL")));
			Assert("Should contain NZ ports.", Declaration.Lookups.DeliveryNotifyPortList.Cast<RefUNLOCO>().Any(x => x.RL_Code == "NZAKL"));
			AssertNotNull("We do have this port in DB.", Factory.Load<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, "GBITIL")));
			Assert("Should NOT contain non-NZ ports.", Declaration.Lookups.DeliveryNotifyPortList.Cast<RefUNLOCO>().All(x => x.RL_Code != "GBTIL"));
		}

		public void TestApplicationCodeList()
		{
			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			var customsInterface = new LocalCountryCustomsInterface { RecipientID = "RecipientID" };

			customsInterface.SubmissionType = DeclarationApplicationCodeListForRegistry.Codes.Builtin;
			using (CustomsDataRegistry.Instance.LocalCountryCustomsInterface.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, customsInterface))
			{
				AssertEquals("SubmissionType: Builtin", "CUS, TSW", Declaration.Lookups.ApplicationCodeList.CodesAsString);
			}

			customsInterface.SubmissionType = DeclarationApplicationCodeListForRegistry.Codes.Interfaced;
			using (CustomsDataRegistry.Instance.LocalCountryCustomsInterface.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, customsInterface))
			{
				AssertEquals("SubmissionType: Interfaced", "ITF, TSW", Declaration.Lookups.ApplicationCodeList.CodesAsString);
			}

			customsInterface.SubmissionType = DeclarationApplicationCodeListForRegistry.Codes.BothBuiltInDefaulted;
			using (CustomsDataRegistry.Instance.LocalCountryCustomsInterface.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, customsInterface))
			{
				AssertEquals("SubmissionType: BothBuiltInDefaulted", "ITF, TSW", Declaration.Lookups.ApplicationCodeList.CodesAsString);
			}

			customsInterface.SubmissionType = DeclarationApplicationCodeListForRegistry.Codes.BothInterfaceDefaulted;
			using (CustomsDataRegistry.Instance.LocalCountryCustomsInterface.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, customsInterface))
			{
				AssertEquals("SubmissionType: BothInterfaceDefaulted", "ITF, TSW", Declaration.Lookups.ApplicationCodeList.CodesAsString);
			}
		}

		#region Implementation
		protected JobDeclaration Declaration
		{
			get
			{
				if (fDeclaration == null)
				{
					fDeclaration = JobDeclaration.New(Factory);
				}
				return fDeclaration;
			}
		}
		JobDeclaration fDeclaration;
		#endregion
	}
}
