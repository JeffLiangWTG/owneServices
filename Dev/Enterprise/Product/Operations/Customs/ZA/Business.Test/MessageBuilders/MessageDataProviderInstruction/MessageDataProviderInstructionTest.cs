using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Customs;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Core.Constants.TransportModes;
using static Enterprise.Customs.Common.ZA.ZAJobMessageTypeList.Codes;
using static Enterprise.Customs.ZA.Business.UniversalReferenceConstants.ProcedureCategoryCodes;
using static Enterprise.Customs.ZA.Business.UniversalReferenceConstants.ProcedureCodes;
using Inst = Enterprise.Customs.ZA.Business.MessageDataProviderInstruction;

namespace Enterprise.Customs.ZA.Business.MessageBuilders.Testing
{
	sealed class MessageDataProviderInstructionTest : TestCaseWithFactory
	{
		public void TestShouldOutputDutiesAndFees()
		{
			foreach (CodeDescriptionPair declartionTypePair in new DeclarationTypeList())
			{
				Factor.DeclarationType = declartionTypePair.Code;
				switch (declartionTypePair.Code)
				{
					case DeclarationTypeList.Codes.RegularIncompleteDeclaration:
					case DeclarationTypeList.Codes.RegularProvisionalDeclaration:
						Assert(!Inst.ShouldOutputDutiesAndFees(Factor));
						break;
					case DeclarationTypeList.Codes.RegularCompleteDeclarationDefault:
					case DeclarationTypeList.Codes.RegularSupplementaryDeclaration:
						Assert(Inst.ShouldOutputDutiesAndFees(Factor));
						break;
				}
			}

			Factor.DeclarationType = ZString.Empty;
			Assert(Inst.ShouldOutputDutiesAndFees(Factor));
		}

		public void TestIsOrdinaryLevyItem()
		{
			Factor.CPC = _10;
			Factor.PPC = _40;
			Assert("Is Ordinary Levy Item", MessageDataProviderInstructionHelper.IsOrdinaryLevyItem(Factor));

			Factor.PPC = _20;
			Assert("Is Ordinary Levy Item", MessageDataProviderInstructionHelper.IsOrdinaryLevyItem(Factor));

			Factor.CPC = _11;
			Assert("Is Ordinary Levy Item", !MessageDataProviderInstructionHelper.IsOrdinaryLevyItem(Factor));

			Factor.CPC = _60;
			Assert("Is Ordinary Levy Item", !MessageDataProviderInstructionHelper.IsOrdinaryLevyItem(Factor));
		}

		public void TestShouldOutputCreditTerms()
		{
			Factor.ShipmentType = Import;
			Assert("CreditTerms must not output for Imports", !Inst.ShouldOutputCreditTerms(Factor));
			Factor.ShipmentType = Export;
			Assert("CreditTerms must output for Exports", Inst.ShouldOutputCreditTerms(Factor));
			Factor.ShipmentType = ExBond;
			Assert("CreditTerms must not output for ExBonds", !Inst.ShouldOutputCreditTerms(Factor));
		}

		public void TestShouldOutputTransactionBankCode()
		{
			Factor.ShipmentType = Import;
			Assert("TransactionBankCode must not output for Imports", !Inst.ShouldOutputTransactionBankCode(Factor));
			Factor.ShipmentType = Export;
			Assert("TransactionBankCode must output for Exports", Inst.ShouldOutputTransactionBankCode(Factor));
			Factor.ShipmentType = ExBond;
			Assert("TransactionBankCode must not output for ExBonds", !Inst.ShouldOutputTransactionBankCode(Factor));
		}

		public void TestShouldOutputContainers()
		{
			Factor.ShipmentType = Import;
			Factor.TransportMode = Sea;
			Assert("IMP, SEA, Containers must output", Inst.ShouldOutputContainers(Factor));
			Factor.TransportMode = Road;
			Assert("IMP, ROA, Containers must output", Inst.ShouldOutputContainers(Factor));
			Factor.TransportMode = Rail;
			Assert("IMP, RAI, Containers must output", Inst.ShouldOutputContainers(Factor));
			Factor.TransportMode = Other;
			Assert("IMP, OTH, Containers must output", Inst.ShouldOutputContainers(Factor));
			Factor.TransportMode = "";
			Assert("IMP, '', Containers must output", Inst.ShouldOutputContainers(Factor));
			Factor.TransportMode = Air;
			Assert("IMP, AIR, Containers must not output", !Inst.ShouldOutputContainers(Factor));

			Factor.ShipmentType = Export;
			Factor.TransportMode = Sea;
			Assert("EXP, SEA, Containers must output", Inst.ShouldOutputContainers(Factor));
			Factor.TransportMode = Road;
			Assert("EXP, ROA, Containers must output", Inst.ShouldOutputContainers(Factor));
			Factor.TransportMode = Rail;
			Assert("EXP, RAI, Containers must output", Inst.ShouldOutputContainers(Factor));
			Factor.TransportMode = Other;
			Assert("EXP, OTH, Containers must output", Inst.ShouldOutputContainers(Factor));
			Factor.TransportMode = "";
			Assert("EXP, '', Containers must output", Inst.ShouldOutputContainers(Factor));
			Factor.TransportMode = Air;
			Assert("EXP, AIR, Containers must not output", !Inst.ShouldOutputContainers(Factor));

			Factor.ShipmentType = ExBond;
			Assert("EXW, Containers must not output", !Inst.ShouldOutputContainers(Factor));
		}

		public void TestShouldOutputImporter()
		{
			Factor.ShipmentType = Import;
			Assert("Importer must output for Imports", Inst.ShouldOutputImporter(Factor));
			Factor.ShipmentType = Export;
			Assert("Importer must not output for Exports", !Inst.ShouldOutputImporter(Factor));
			Factor.ShipmentType = ExBond;
			Assert("Importer must output for ExBonds", Inst.ShouldOutputImporter(Factor));
		}

		public void TestShouldOutputImporterForExportJob()
		{
			Factor.ShipmentType = Import;
			Assert("Importer must output for Imports", !Inst.ShouldOutputImporterForExportJob(Factor));
			Factor.ShipmentType = Export;
			Assert("Importer must not output for Exports", Inst.ShouldOutputImporterForExportJob(Factor));
			Factor.ShipmentType = ExBond;
			Assert("Importer must output for ExBonds", !Inst.ShouldOutputImporterForExportJob(Factor));
		}

		public void TestShouldOutputExporter()
		{
			Factor.ShipmentType = Import;
			Assert("Exporter must not output for Imports", !Inst.ShouldOutputExporter(Factor));
			Factor.ShipmentType = Export;
			Assert("Exporter must output for Exports", Inst.ShouldOutputExporter(Factor));
			Factor.ShipmentType = ExBond;
			Assert("Exporter must not output for ExBonds", !Inst.ShouldOutputExporter(Factor));
		}

		public void TestShouldOutputTradeStatisticsIndicator()
		{
			Factor.ShipmentType = Import;
			Assert("TradeStatisticsIndicator must not output for Imports", !Inst.ShouldOutputTradeStatisticsIndicator(Factor));
			Factor.ShipmentType = Export;
			Assert("TradeStatisticsIndicator must output for Exports", Inst.ShouldOutputTradeStatisticsIndicator(Factor));
			Factor.ShipmentType = ExBond;
			Assert("TradeStatisticsIndicator must not output for ExBonds", !Inst.ShouldOutputTradeStatisticsIndicator(Factor));
		}

		public void TestShouldOutputCountryOfOrigin()
		{
			Factor.ShipmentType = Import;
			Assert("CountryOfOrigin must output for Imports", Inst.ShouldOutputCountryOfOrigin(Factor));
			Factor.ShipmentType = Export;
			Assert("CountryOfOrigin must output for Exports", Inst.ShouldOutputCountryOfOrigin(Factor));
			Factor.ShipmentType = ExBond;
			Assert("CountryOfOrigin must output for ExBonds", Inst.ShouldOutputCountryOfOrigin(Factor));
		}

		public void TestShouldOutputActualPrice()
		{
			Factor.ShipmentType = Import;
			Assert("ActualPrice must output for Imports", Inst.ShouldOutputActualPrice(Factor));
			Factor.ShipmentType = Export;
			Assert("ActualPrice must not output for Exports", !Inst.ShouldOutputActualPrice(Factor));
			Factor.ShipmentType = ExBond;
			Assert("ActualPrice must not output for ExBonds", Inst.ShouldOutputActualPrice(Factor));
		}

		public void TestShouldOutputTotalCIFCAmount()
		{
			Factor.ShipmentType = Import;
			foreach (CodeDescriptionPair declarationTypePair in new DeclarationTypeList())
			{
				Factor.DeclarationType = declarationTypePair.Code;
				switch (declarationTypePair.Code)
				{
					case DeclarationTypeList.Codes.RegularIncompleteDeclaration:
					case DeclarationTypeList.Codes.RegularProvisionalDeclaration:
						Assert(!Inst.ShouldOutputTotalCIFCAmount(Factor));
						break;
					case DeclarationTypeList.Codes.RegularCompleteDeclarationDefault:
					case DeclarationTypeList.Codes.RegularSupplementaryDeclaration:
						Assert(Inst.ShouldOutputTotalCIFCAmount(Factor));
						break;
				}
			}
			Factor.ShipmentType = Export;
			Assert("TotalCIFCAmount must not output for Exports", !Inst.ShouldOutputTotalCIFCAmount(Factor));
			Factor.ShipmentType = ExBond;
			Assert("TotalCIFCAmount must not output for ExBonds", !Inst.ShouldOutputTotalCIFCAmount(Factor));

			Factor.PPC = _40;
			Assert("PPC = 40, ActualPrice must not output", !Inst.ShouldOutputTotalCIFCAmount(Factor));
		}

		public void TestShouldOutputTotalTransactionValueAndCurrency()
		{
			Factor.ShipmentType = Import;
			Assert("TotalTransactionValueAndCurrency must output for Imports", Inst.ShouldOutputTotalTransactionValueAndCurrency(Factor));
			Factor.ShipmentType = Export;
			Assert("TotalTransactionValueAndCurrency must output for Exports", Inst.ShouldOutputTotalTransactionValueAndCurrency(Factor));
			Factor.ShipmentType = ExBond;
			Assert("TotalTransactionValueAndCurrency must not output for ExBonds", !Inst.ShouldOutputTotalTransactionValueAndCurrency(Factor));
		}

		public void TestShouldOutputPartClearanceQuantity()
		{
			Factor.ShipmentType = Import;
			Assert("PartClearanceQuantity must output for Imports", Inst.ShouldOutputPartClearanceQuantity(Factor));
			Factor.ShipmentType = Export;
			Assert("PartClearanceQuantity must not output for Exports", !Inst.ShouldOutputPartClearanceQuantity(Factor));
			Factor.ShipmentType = ExBond;
			Assert("PartClearanceQuantity must output for ExBonds", Inst.ShouldOutputPartClearanceQuantity(Factor));
		}

		public void TestShouldOutputOriginalMRN()
		{
			Factor.MessageType = MessageSubTypeCodes.Codes.Cancellation;
			Assert("OriginalMRN must output for Cancellation entries", Inst.ShouldOutputOriginalMRN(Factor));
			Factor.MessageType = MessageSubTypeCodes.Codes.Change;
			Assert("OriginalMRN must output for Change/Amendment entries", Inst.ShouldOutputOriginalMRN(Factor));
			Factor.MessageType = MessageSubTypeCodes.Codes.Replace;
			Assert("OriginalMRN must NOT output for Replace entries", !Inst.ShouldOutputOriginalMRN(Factor));
			Factor.MessageType = MessageSubTypeCodes.Codes.Original;
			Assert("OriginalMRN must NOT output for Original entries", !Inst.ShouldOutputOriginalMRN(Factor));
		}

		public void TestShouldOutputMRNToBeReplaced()
		{
			Factor.MessageType = MessageSubTypeCodes.Codes.Cancellation;
			Assert("MRNToBeReplaced must output for Cancellation entries", Inst.ShouldOutputMRNToBeReplaced(Factor));
			Factor.MessageType = MessageSubTypeCodes.Codes.Change;
			Assert("MRNToBeReplaced must output for Change/Amendment entries", Inst.ShouldOutputMRNToBeReplaced(Factor));
			Factor.MessageType = MessageSubTypeCodes.Codes.Replace;
			Assert("MRNToBeReplaced must output for Replace entries", Inst.ShouldOutputMRNToBeReplaced(Factor));
			Factor.MessageType = MessageSubTypeCodes.Codes.Original;
			Assert("MRNToBeReplaced must NOT output for Original entries", !Inst.ShouldOutputMRNToBeReplaced(Factor));
		}

		public void TestShouldOutputTotalDutiesDueWhenZero()
		{
			foreach (var shipmentType in new[] { Import, ExBond })
			{
				CombineAssertions(shipmentType, () =>
				{
					Factor.ShipmentType = shipmentType;

					foreach (CodeDescriptionPair messageTypePair in new MessageSubTypeCodes())
					{
						Factor.MessageType = messageTypePair.Code;
						switch (messageTypePair.Code)
						{
							case MessageSubTypeCodes.Codes.Cancellation:
							case MessageSubTypeCodes.Codes.Change:
							case MessageSubTypeCodes.Codes.Replace:
								foreach (CodeDescriptionPair declarationTypePair in new DeclarationTypeList())
								{
									Factor.DeclarationType = declarationTypePair.Code;
									switch (declarationTypePair.Code)
									{
										case DeclarationTypeList.Codes.RegularIncompleteDeclaration:
										case DeclarationTypeList.Codes.RegularProvisionalDeclaration:
											Assert(!Inst.ShouldOutputTotalDutiesDueWhenZero(Factor));
											break;
										case DeclarationTypeList.Codes.RegularCompleteDeclarationDefault:
										case DeclarationTypeList.Codes.RegularSupplementaryDeclaration:
											Assert(Inst.ShouldOutputTotalDutiesDueWhenZero(Factor));
											break;
									}
								}
								Factor.DeclarationType = ZString.Empty;
								Assert(Inst.ShouldOutputTotalDutiesDueWhenZero(Factor));
								break;
							case MessageSubTypeCodes.Codes.Original:
								Assert(!Inst.ShouldOutputTotalDutiesDueWhenZero(Factor));
								break;
						}
					}
				});
			}

			CombineAssertions("Export", () =>
			{
				Factor.ShipmentType = Export;
				Factor.MessageType = MessageSubTypeCodes.Codes.Cancellation;
				Assert(!Inst.ShouldOutputTotalDutiesDueWhenZero(Factor));
				Factor.MessageType = MessageSubTypeCodes.Codes.Change;
				Assert(!Inst.ShouldOutputTotalDutiesDueWhenZero(Factor));
				Factor.MessageType = MessageSubTypeCodes.Codes.Original;
				Assert(!Inst.ShouldOutputTotalDutiesDueWhenZero(Factor));
				Factor.MessageType = MessageSubTypeCodes.Codes.Replace;
				Assert(!Inst.ShouldOutputTotalDutiesDueWhenZero(Factor));
			});
		}

		public void TestShouldOutputTotalVATDueWhenZero()
		{
			foreach (var shipmentType in new[] { Import, ExBond })
			{
				CombineAssertions(shipmentType, () =>
				{
					Factor.ShipmentType = shipmentType;

					foreach (CodeDescriptionPair messageTypePair in new MessageSubTypeCodes())
					{
						Factor.MessageType = messageTypePair.Code;
						switch (messageTypePair.Code)
						{
							case MessageSubTypeCodes.Codes.Cancellation:
							case MessageSubTypeCodes.Codes.Change:
							case MessageSubTypeCodes.Codes.Replace:
								foreach (CodeDescriptionPair declarationTypePair in new DeclarationTypeList())
								{
									Factor.DeclarationType = declarationTypePair.Code;
									switch (declarationTypePair.Code)
									{
										case DeclarationTypeList.Codes.RegularIncompleteDeclaration:
										case DeclarationTypeList.Codes.RegularProvisionalDeclaration:
											Assert(!Inst.ShouldOutputTotalVATDueWhenZero(Factor));
											break;
										case DeclarationTypeList.Codes.RegularCompleteDeclarationDefault:
										case DeclarationTypeList.Codes.RegularSupplementaryDeclaration:
											Assert(Inst.ShouldOutputTotalVATDueWhenZero(Factor));
											break;
									}
								}
								Factor.DeclarationType = ZString.Empty;
								Assert(Inst.ShouldOutputTotalVATDueWhenZero(Factor));
								break;
							case MessageSubTypeCodes.Codes.Original:
								Assert(!Inst.ShouldOutputTotalVATDueWhenZero(Factor));
								break;
						}
					}
				});
			}

			CombineAssertions("Export", () =>
			{
				Factor.ShipmentType = Export;
				Factor.MessageType = MessageSubTypeCodes.Codes.Cancellation;
				Assert(!Inst.ShouldOutputTotalVATDueWhenZero(Factor));
				Factor.MessageType = MessageSubTypeCodes.Codes.Change;
				Assert(!Inst.ShouldOutputTotalVATDueWhenZero(Factor));
				Factor.MessageType = MessageSubTypeCodes.Codes.Original;
				Assert(!Inst.ShouldOutputTotalVATDueWhenZero(Factor));
				Factor.MessageType = MessageSubTypeCodes.Codes.Replace;
				Assert(!Inst.ShouldOutputTotalVATDueWhenZero(Factor));
			});
		}

		public void TestShouldOutputLineLevelInformation()
		{
			CombineAssertions("Import and Export", () =>
			{
				Factor.MessageType = MessageSubTypeCodes.Codes.Original;
				Assert("LineLevelInformation must output for Original entries", Inst.ShouldOutputLineLevelInformation(Factor));
				Factor.MessageType = MessageSubTypeCodes.Codes.Cancellation;
				Assert("LineLevelInformation must not output for Cancellation entries", !Inst.ShouldOutputLineLevelInformation(Factor));
			});
		}

		public void TestShouldOutputVoyageFlightNo()
		{
			CombineAssertions("Import and Export", () =>
			{
				ZString[] shipmentTypes = new ZString[] { Import, Export };
				ZString[] transportModes = new ZString[] { Sea, Air };
				foreach (ZString shipmentType in shipmentTypes)
				{
					Factor.ShipmentType = shipmentType;
					foreach (ZString transportMode in transportModes)
					{
						Factor.TransportMode = transportMode;
						Assert(transportMode + " " + shipmentType + " must output VoyageFlightVehicle", Inst.ShouldOutputVoyageFlightNo(Factor));
					}
				}
			});

			CombineAssertions("ExBond", () =>
			{
				Factor.ShipmentType = ExBond;
				Factor.TransportMode = Road;
				Assert("Vehicle must not output for ExBond if TransportCode is Road", !Inst.ShouldOutputVoyageFlightNo(Factor));

				Factor.TransportMode = Air;
				Assert("Flight must not output for ExBond if TransportCode is Air", !Inst.ShouldOutputVoyageFlightNo(Factor));

				Factor.TransportMode = Sea;
				Assert("Voyage must not output for ExBond if TransportCode is Sea", !Inst.ShouldOutputVoyageFlightNo(Factor));
			});
		}

		public void TestShouldOutputTransportMode()
		{
			Factor.ShipmentType = Import;
			Factor.CountryOfDestination = RefCountry.LoadFromCountryCode(Factory, Core.Constants.CountryCodes.China);
			Assert(Inst.ShouldOutputTransportMode(Factor));

			Factor.CountryOfDestination = RefCountry.LoadFromCountryCode(Factory, Core.Constants.CountryCodes.Botswana);
			Assert(Inst.ShouldOutputTransportMode(Factor));

			Factor.ShipmentType = Export;
			Factor.CountryOfDestination = RefCountry.LoadFromCountryCode(Factory, Core.Constants.CountryCodes.China);
			Assert(Inst.ShouldOutputTransportMode(Factor));
			Factor.CountryOfDestination = RefCountry.LoadFromCountryCode(Factory, Core.Constants.CountryCodes.Botswana);
			Assert(Inst.ShouldOutputTransportMode(Factor));

			Factor.ShipmentType = ExBond;
			Assert(Inst.ShouldOutputTransportMode(Factor));

			Factor.CountryOfDestination = RefCountry.LoadFromCountryCode(Factory, Core.Constants.CountryCodes.China);
			Assert(!Inst.ShouldOutputTransportMode(Factor));
		}

		public void TestShouldOutputUnknownRemovalTransportMode()
		{
			CombineAssertions(() =>
			{
				Factor.PPC = ZString.Empty;
				Factor.CPC = _10;
				Assert("Must not output Unknown Removal Transporter Mode for CPC 10", !Inst.ShouldOutputUnknownRemovalTransportMode(Factor));
				Factor.CPC = _41;
				Assert("Must output Unknown Removal Transporter Mode for CPC 41", Inst.ShouldOutputUnknownRemovalTransportMode(Factor));
				Factor.CPC = _48;
				Assert("Must not output Unknown Removal Transporter Mode for CPC 48", !Inst.ShouldOutputUnknownRemovalTransportMode(Factor));
				Factor.PPC = _42;
				Assert("Must output Unknown Removal Transporter Mode for CPC 48 and PPC 42", Inst.ShouldOutputUnknownRemovalTransportMode(Factor));
			});
		}

		public void TestShouldOutputRemovalTransportMode()
		{
			CombineAssertions(() =>
			{
				var blnsCountry = Factory.NewWithValidTestData<RefCountry>();
				blnsCountry.RN_Code = "XX";
				blnsCountry.RN_EconomicGrouping = EconomicGroupList.Codes.BLNS;

				var nonBLNSCountry = Factory.NewWithValidTestData<RefCountry>();
				nonBLNSCountry.RN_Code = "YY";
				nonBLNSCountry.RN_EconomicGrouping = EconomicGroupList.Codes.EuropeanUnion;

				Factor.CountryOfDestination = blnsCountry;
				Factor.ShipmentType = "IMP";
				Factor.PPC = ZString.Empty;
				Factor.CPC = _10;
				Assert("Must not output Removal Transporter Mode for CPC 10", !Inst.ShouldOutputRemovalTransportMode(Factor));

				Factor.ProcedureCategory = _B;
				Assert("Must output Removal Transporter Mode for Category B", Inst.ShouldOutputRemovalTransportMode(Factor));

				Factor.ShipmentType = "EXP";
				Assert("Must not output Removal Transporter Mode when shipment type is not IMP or EXW", !Inst.ShouldOutputRemovalTransportMode(Factor));

				Factor.ShipmentType = "IMP";
				Factor.ProcedureCategory = _E;
				Factor.CPC = _41;
				Assert(!Inst.ShouldOutputRemovalTransportMode(Factor));
				Factor.CPC = ZString.Empty;
				Assert(Inst.ShouldOutputRemovalTransportMode(Factor));

				Factor.ProcedureCategory = ZString.Empty;
				var cpcs = new ZString[] { _52, _53, _67, _68 };
				foreach (ZString cpc in cpcs)
				{
					Factor.CPC = cpc;
					Assert(Inst.ShouldOutputRemovalTransportMode(Factor));
				}

				Factor.CPC = _11;
				Assert("Must output Removal Transporter Mode for CPC 11 and final destination is a BLNS country/region", Inst.ShouldOutputRemovalTransportMode(Factor));

				Factor.RemovalTransportMode = Core.Constants.TransportModes.Road;
				Assert("If removal Mode is ROAD, then Remover must output irrespective of whether CPC is Bonded or not, or whether the Final Destination is BLNS or not", Inst.ShouldOutputRemovalTransportMode(Factor));

				Factor.RemovalTransportMode = ZString.Empty;
				Factor.CountryOfDestination = nonBLNSCountry;
				Assert("Must not output Removal Transporter Mode for CPC 11 and final destination is not a BLNS country/region", !Inst.ShouldOutputRemovalTransportMode(Factor));

				Factor.RemovalTransportMode = Core.Constants.TransportModes.Road;
				Assert("If removal Mode is ROAD, then Remover must output irrespective of whether CPC is Bonded or not, or whether the Final Destination is BLNS or not", Inst.ShouldOutputRemovalTransportMode(Factor));
			});
		}

		public void TestRemoverTransporterCodeRequired()
		{
			CombineAssertions(() =>
			{
				var removalTransportModes = new ZString[] { Air, Sea, Rail, Mail, FixedTransportInstallations, Other, "" };
				foreach (var removalTransportMode in removalTransportModes)
				{
					Factor.RemovalTransportMode = removalTransportMode;
					Factor.CPC = _10;
					Factor.ProcedureCategory = _A;
					Assert("Must not output Removal Transporter Mode for CPC 10 and " + removalTransportMode, !Inst.RemoverTransporterCodeRequired(Factor));

					Factor.CPC = _20;
					Factor.ProcedureCategory = _B;
					Assert("Must not output Removal Transporter Mode for Category B and " + removalTransportMode, !Inst.RemoverTransporterCodeRequired(Factor));
				}

				Factor.RemovalTransportMode = Road;
				Factor.CPC = _10;
				Factor.ProcedureCategory = _A;
				Assert("Must not output Removal Transporter Mode for CPC 10 and Road", !Inst.RemoverTransporterCodeRequired(Factor));

				Factor.CPC = _20;
				Factor.ProcedureCategory = _B;
				Assert("Must output Removal Transporter Mode for Category B and Road", Inst.RemoverTransporterCodeRequired(Factor));

				Factor.CPC = _41;
				Factor.ProcedureCategory = _E;
				Assert("Must not output Removal Transporter Mode for CPC 41 and Road", !Inst.RemoverTransporterCodeRequired(Factor));

				Factor.CPC = _53;
				Factor.ProcedureCategory = _F;
				Assert("Must output Removal Transporter Mode for CPC 53 and Road", Inst.RemoverTransporterCodeRequired(Factor));

				Factor.CPC = _68;
				Factor.ProcedureCategory = _H;
				Assert("Must output Removal Transporter Mode for CPC 68 and Road", Inst.RemoverTransporterCodeRequired(Factor));
			});
		}

		public void TestRemovalTransportModeRequiredForPreviousWarehouseExport()
		{
			CombineAssertions(() =>
			{
				var removalTransportModes = new ZString[] { Air, Sea, Rail, Mail, FixedTransportInstallations, Other, "" };
				foreach (var removalTransportMode in removalTransportModes)
				{
					Factor.RemovalTransportMode = removalTransportMode;
					Factor.CPC = "48";
					Factor.PPC = "42";
					Assert("Must not output Removal Transporter Mode for CPC 48 with PPC 42 and " + removalTransportMode, !Inst.RemoverTransporterCodeRequiredForPreviousWarehouseExport(Factor));

					Factor.CPC = "48";
					Factor.PPC = "48";
					Assert("Must not output Removal Transporter Mode for CPC 48 with PPC 48 and " + removalTransportMode, !Inst.RemoverTransporterCodeRequiredForPreviousWarehouseExport(Factor));

					Factor.CPC = "48";
					Factor.PPC = "48";
					Assert("Must not output Removal Transporter Mode for CPC 48 with PPC 49 and " + removalTransportMode, !Inst.RemoverTransporterCodeRequiredForPreviousWarehouseExport(Factor));
				}

				Factor.RemovalTransportMode = Road;
				Factor.CPC = "48";
				Factor.PPC = "42";
				Assert("Must not output Removal Transporter Mode for CPC 48 with PPC 42 and Road", !Inst.RemoverTransporterCodeRequiredForPreviousWarehouseExport(Factor));

				Factor.CPC = "48";
				Factor.PPC = "48";
				Assert("Must output Removal Transporter Mode for CPC 48 with PPC 48 and Road", Inst.RemoverTransporterCodeRequiredForPreviousWarehouseExport(Factor));

				Factor.CPC = "48";
				Factor.PPC = "48";
				Assert("Must output Removal Transporter Mode for CPC 48 with PPC 49 and Road", Inst.RemoverTransporterCodeRequiredForPreviousWarehouseExport(Factor));
			});
		}

		public void TestShouldOutputRemoverTransporterCode()
		{
			var testRemover = Factory.NewWithValidTestData<OrgHeader>();
			testRemover.MainAddress.Address1 = "REMOVERADDR1";
			testRemover.CustomsCodes.AddNew(OrgCusCode.SouthAfricaCodeTypes.RemoverUserCode, "00111110", "ZA");
			testRemover.OH_RL_NKClosestPort = "ZACPT";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Export;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Road;
			var entryInstruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_Style = _11;
			entryInstruction.CEI_OH_Carrier = testRemover.PK;

			CombineAssertions(() =>
			{
				Factor.Remover = ZString.Empty;
				Factor.RemovalTransportMode = Sea;
				Assert("Must not output Remover Transporter Code for removal Sea", !Inst.ShouldOutputRemoverTransporterCode(Factor));
				Factor.RemovalTransportMode = Road;
				Factor.Remover = ZString.Empty;
				Factor.CPC = _10; //Must output Remover Transporter Code for all as long as the Removal Transport Mode is 'ROAD'
				Assert("Must not output Remover Transporter Code Because Remover is blank '", !Inst.ShouldOutputRemoverTransporterCode(Factor));
				Factor.RemovalTransportMode = Road;
				Factor.Remover = "00111110";
				Assert("Must output Remover Transporter Code as long as the Removal Transport Mode is 'ROAD' and Remover is not blank", Inst.ShouldOutputRemoverTransporterCode(Factor));
				Factor.RemovalTransportMode = Rail;
				Factor.ProcedureCategory = _E;
				Factor.CPC = _47;
				Assert("Must not output Remover Transporter Code for removal 'Rail'", !Inst.ShouldOutputRemoverTransporterCode(Factor));
				Factor.RemovalTransportMode = Air;
				Factor.CPC = _48;
				Assert("Must not output Remover Transporter Code for removal 'Air'", !Inst.ShouldOutputRemoverTransporterCode(Factor));
			});
		}

		public void TestShouldOutputSupplier()
		{
			Factor.ShipmentType = Import;
			Factor.VDN = "112233";
			Assert("Must output Supplier for Import, with VDN", Inst.ShouldOutputSupplier(Factor));

			Factor.ShipmentType = Import;
			Factor.VDN = "";
			Assert("Must output Supplier for Import, empty VDN", Inst.ShouldOutputSupplier(Factor));

			Factor.ShipmentType = Export;
			Assert("Must not output Supplier for Export", !Inst.ShouldOutputSupplier(Factor));

			Factor.ShipmentType = ExBond;
			Assert("Must not output Supplier for ExBond", !Inst.ShouldOutputSupplier(Factor));
		}

		public void TestShouldOutputSupplierCode()
		{
			Factor.ShipmentType = Import;
			Factor.VDN = "112233";
			Assert("Must output SupplierCode for Import, with VDN", Inst.ShouldOutputSupplierCode(Factor));
			Factor.VDN = "";
			Assert("Must not output SupplierCode for Import, empty VDN", !Inst.ShouldOutputSupplierCode(Factor));
		}

		#region RelationshipIndicator and ValuationCode:

		class RelationshipIndicator_and_ValuationCode_TestCase
		{
			public RelationshipIndicator_and_ValuationCode_TestCase(string shipmentType, string cpc, string ppc, string relationshipIndicator, bool shouldOutput)
			{
				ApplySanityCheck_BeforeTesting_ShouldOutput_RelatedIndicator_and_ValuationCode_in_EdiMessage(shipmentType, cpc, shouldOutput);

				factor = new MessageDataProviderKeyFactor()
				{
					ShipmentType = shipmentType,
					CPC = cpc,
					PPC = ppc,
					RelationshipIndicator = relationshipIndicator
				};
				this.shouldOutput = shouldOutput;
			}

			public void Check_ShouldOutput_RelationshipIndicator_and_ValuationCode()
			{
				#region Prepare Assert Message:

				var negate = shouldOutput ? "" : " not";
				var direction = (factor.ShipmentType == Export) ? "Export" : "Import";
				var related = "";

				switch (factor.RelationshipIndicator)
				{
					case RelatedIndicatorList.Codes.Yes:
						related = ", Related";
						break;
					case RelatedIndicatorList.Codes.No:
						related = ", Not Related";
						break;
					case RelatedIndicatorList.Codes.Exempt:
						related = ", Exempt";
						break;
				}
				var assertMsg = "Must" + negate + " output Related Indicator and Valuation Code for " + direction + " (CPC=" + factor.CPC + ")" + related;

				#endregion Prepare Assert Message.

				AssertEquals(assertMsg, shouldOutput, Inst.ShouldOutputRelatedIndicatorAndValuationCode(factor));
			}

			readonly MessageDataProviderKeyFactor factor;
			readonly bool shouldOutput;
		}

		class RelationshipIndicator_and_ValuationCode_TestCase_Collection
		{
			public void Add(string shipmentType, string cpc, string ppc, string relationshipIndicator, bool shouldOutput)
			{
				list.Add(new RelationshipIndicator_and_ValuationCode_TestCase(shipmentType, cpc, ppc, relationshipIndicator, shouldOutput));
			}

			public void TestAllTestCases() => list.ForEach(x => x.Check_ShouldOutput_RelationshipIndicator_and_ValuationCode());

			readonly List<RelationshipIndicator_and_ValuationCode_TestCase> list = new List<RelationshipIndicator_and_ValuationCode_TestCase>();
		}

		public void TestShouldOutputRelatedIndicatorAndValuationCode()
		{
			#region Constants:

			const string Related = RelatedIndicatorList.Codes.Yes;
			const string NotRelated = RelatedIndicatorList.Codes.No;
			const string Exempt = RelatedIndicatorList.Codes.Exempt;

			#endregion Constants.

			var list = new RelationshipIndicator_and_ValuationCode_TestCase_Collection();

			list.Add(Import, "14", "00", Exempt, true);
			list.Add(Import, "11", "00", NotRelated, true);
			list.Add(Import, "12", "00", NotRelated, false);
			list.Add(Import, "20", "00", NotRelated, false);
			list.Add(Import, "21", "00", NotRelated, false);
			list.Add(Import, "22", "00", NotRelated, false);
			list.Add(Import, "37", "00", NotRelated, false);
			list.Add(Import, "78", "00", NotRelated, false);
			list.Add(Export, "60", "00", Related, false);

			list.TestAllTestCases();
		}

		public static void ApplySanityCheck_BeforeTesting_ShouldOutput_RelatedIndicator_and_ValuationCode_in_EdiMessage(string shipmentType, string cpc, bool specifiedDesiredOutput)
		{
			var cpcList = new List<string>() { "12", "20", "21", "22", "37", "78" };
			bool calculatedOutput = false;

			if (shipmentType == Import)
			{
				if (!cpcList.Contains(cpc))
				{
					calculatedOutput = true;
				}
			}

			if (specifiedDesiredOutput != calculatedOutput)
			{
				var errorMsg = "Invalid input parameters specified for unit testing. The following combination does not make sense:";
				var assertMsg = $"{errorMsg} (ShipmentType = {shipmentType}; CPC = {cpc}; ShouldOutputRelatedIndicator = {specifiedDesiredOutput})";
				Assert(assertMsg, false);
			}
		}

		#endregion RelationshipIndicator and ValuationCode.

		public void TestShouldOutputFromWarehouse()
		{
			Factor.ShipmentType = ZAJobMessageTypeList.Codes.Import;
			Factor.PPC = _10;
			Assert("FromWarehouse must not output if Import or PPC 10", !Inst.ShouldOutputFromWarehouse(Factor));
			Factor.ShipmentType = ZAJobMessageTypeList.Codes.ExBond;
			Assert("FromWarehouse must output if Exbond", Inst.ShouldOutputFromWarehouse(Factor));
			Factor.ShipmentType = ZAJobMessageTypeList.Codes.Import;
			Factor.PPC = _40;
			Assert("FromWarehouse must output if PPC 40", Inst.ShouldOutputFromWarehouse(Factor));
			Factor.CPC = _52;
			Assert(Inst.ShouldOutputFromWarehouse(Factor));
			Factor.CPC = _53;
			Assert(Inst.ShouldOutputFromWarehouse(Factor));
			Factor.CPC = _67;
			Assert(Inst.ShouldOutputFromWarehouse(Factor));
			Factor.CPC = _68;
			Assert(Inst.ShouldOutputFromWarehouse(Factor));
		}

		public void TestIsFromWarehouseRequired()
		{
			Factor.CPC = _52;
			Assert(Inst.IsFromWarehouseRequired(Factor));
			Factor.CPC = _53;
			Assert(Inst.IsFromWarehouseRequired(Factor));
			Factor.CPC = _67;
			Assert(Inst.IsFromWarehouseRequired(Factor));
			Factor.CPC = _68;
			Assert(Inst.IsFromWarehouseRequired(Factor));
		}

		public void TestShouldOutputToWarehouse()
		{
			Factor.TransportMode = string.Empty;
			Factor.ProcedureCategory = string.Empty;
			Factor.CPC = _20;
			Assert("ToWarehouse must not output if CPC is 20 and Destination is not BLNS", !Inst.ShouldOutputToWarehouse(Factor));
			Factor.CountryOfDestination = Factory.Load<RefCountry>(Core.Constants.CountryGuids.Botswana);
			Assert("ToWarehouse must output if CPC is 20 and Destination is BLNS", Inst.ShouldOutputToWarehouse(Factor));
			Factor.CPC = _21;
			Assert("ToWarehouse must not output if CPC is 21", !Inst.ShouldOutputToWarehouse(Factor));
			Factor.CPC = _40;
			Assert("ToWarehouse must output if CPC is 40", Inst.ShouldOutputToWarehouse(Factor));
		}

		public void TestShouldOutputVessel()
		{
			Factor.TransportMode = Sea;
			Factor.ShipmentType = Import;
			Assert("Vessel must output for Import if TransportCode is Sea", Inst.ShouldOutputVessel(Factor));

			Factor.ShipmentType = Export;
			Assert("Vessel must output for Export if TransportCode is Sea", Inst.ShouldOutputVessel(Factor));

			Factor.ShipmentType = ExBond;
			Assert("Vessel must not output for ExBond if TransportCode is Sea", !Inst.ShouldOutputVessel(Factor));
		}

		public void TestShouldOutputRoadTransport()
		{
			Factor.TransportMode = Road;
			Factor.ShipmentType = Import;
			Assert("Vehicle and Trailer Registration Numbers must output if Road Import", Inst.ShouldOutputRoadVehicle(Factor));

			Factor.ShipmentType = Export;
			Assert("Vehicle and Trailer Registration Numbers must output if Road Export", Inst.ShouldOutputRoadVehicle(Factor));
		}

		public void TestShouldOutputInvoiceInformations()
		{
			Factor.ShipmentType = Import;
			Factor.MessageType = MessageSubTypeCodes.Codes.Cancellation;
			Assert("Invoice Information must output if Import and Cancellation", Inst.ShouldOutputInvoiceInformations(Factor));
			Factor.MessageType = MessageSubTypeCodes.Codes.Change;
			Assert("Invoice Information must output if Import and Change", Inst.ShouldOutputInvoiceInformations(Factor));
			Factor.MessageType = MessageSubTypeCodes.Codes.Original;
			Assert("Invoice Information must output if Import and Original", Inst.ShouldOutputInvoiceInformations(Factor));
			Factor.MessageType = MessageSubTypeCodes.Codes.Replace;
			Assert("Invoice Information must output if Import and Replace", Inst.ShouldOutputInvoiceInformations(Factor));

			Factor.ShipmentType = Export;
			AssertEquals("Invoice Information must output if Export and Effective", true, Inst.ShouldOutputInvoiceInformations(Factor));
			Factor.MessageType = MessageSubTypeCodes.Codes.Cancellation;
			Assert("Invoice Information must output if Export and Cancellation", Inst.ShouldOutputInvoiceInformations(Factor));
			Factor.MessageType = MessageSubTypeCodes.Codes.Change;
			Assert("Invoice Information must output if Export and Change", Inst.ShouldOutputInvoiceInformations(Factor));
			Factor.MessageType = MessageSubTypeCodes.Codes.Original;
			Assert("Invoice Information must output if Export and Original", Inst.ShouldOutputInvoiceInformations(Factor));
			Factor.MessageType = MessageSubTypeCodes.Codes.Replace;
			Assert("Invoice Information must output if Export and Replace", Inst.ShouldOutputInvoiceInformations(Factor));
		}

		#region PortOfExit/Departure

		public void TestShouldOutputPortOfDestinationOrExit()
		{
			Factor.ShipmentType = Import;
			Factor.CPC = _20;
			Assert("Port of Destination Or Exit must output for import CPC 20", Inst.ShouldOutputPortOfExit(Factor));
			Factor.CPC = _21;
			Assert("Port of Destination Or Exit must output for import CPC 21", Inst.ShouldOutputPortOfExit(Factor));
			Factor.CPC = _22;
			Assert("Port of Destination Or Exit must output for import CPC 22", Inst.ShouldOutputPortOfExit(Factor));
			Factor.CPC = _40;
			Assert("Port of Destination Or Exit must output for import CPC 40", Inst.ShouldOutputPortOfExit(Factor));
			Factor.CPC = _42;
			Assert("Port of Destination Or Exit must output for import CPC 42", Inst.ShouldOutputPortOfExit(Factor));

			Factor.ShipmentType = Export;
			ZString[] transportModes = new ZString[] { Sea, Air, Road };
			foreach (var transportMode in transportModes)
			{
				Factor.TransportMode = transportMode;
				Assert("Port of Destination Or Exit must output for " + transportMode + " Exports", Inst.ShouldOutputPortOfExit(Factor));
			}

			Factor.ShipmentType = ExBond;
			Factor.CountryOfDestination = RefCountry.LoadFromCountryCode(Factory, Core.Constants.CountryCodes.Lesotho);
			Assert("Port of Destination Or Exit must output for ExBonds with CountryOfDestination.IsBLNS", Inst.ShouldOutputPortOfExit(Factor));

			Factor.ShipmentType = Import;
			Factor.CountryOfDestination = RefCountry.LoadFromCountryCode(Factory, Core.Constants.CountryCodes.Lesotho);
			Assert("Port of Destination Or Exit must output for Imports with CountryOfDestination.IsBLNS", Inst.ShouldOutputPortOfExit(Factor));
		}

		public void TestIsPortOfDestinationRequired()
		{
			Factor.ShipmentType = Import;
			Factor.CPC = _20;
			Assert("Port of Exit is required for import CPC 20", Inst.IsPortOfDestinationRequired(Factor));
			Factor.CPC = _21;
			Assert("Port of Exit is required for import CPC 21", Inst.IsPortOfDestinationRequired(Factor));
			Factor.CPC = _22;
			Assert("Port of Exit is required for import CPC 22", Inst.IsPortOfDestinationRequired(Factor));
			Factor.CPC = _10;
			Assert("Port of Exit is not required for import CPC 10", !Inst.IsPortOfDestinationRequired(Factor));
			Factor.CPC = _40;
			Assert("Port of Exit is required for import CPC 40", Inst.IsPortOfDestinationRequired(Factor));
			Factor.CPC = _42;
			Assert("Port of Exit is required for import CPC 42", Inst.IsPortOfDestinationRequired(Factor));
		}

		public void TestIsPortOfExitRequiredForExports()
		{
			Factor.ShipmentType = Export;
			ZString[] transportModes = new ZString[] { Sea, Air, Road, Rail };
			foreach (var transportMode in transportModes)
			{
				Factor.TransportMode = transportMode;
				Assert("Port of Exit is required for " + transportMode + " Exports", Inst.IsPortOfExitRequired(Factor));
			}
		}

		public void TestIsPortOfExitRequiredForExBonds()
		{
			Factor.ShipmentType = ExBond;
			Factor.CountryOfDestination = RefCountry.LoadFromCountryCode(Factory, Core.Constants.CountryCodes.Lesotho);
			Assert("Port of Exit is required for ExBonds with CountryOfDestination.IsBLNS", Inst.IsPortOfExitRequired(Factor));
		}

		public void TestIsPortOfExitRequiredForBLNSImports()
		{
			Factor.ShipmentType = Import;
			ZString[] blnsCountryCodes = new ZString[] { Core.Constants.CountryCodes.Botswana, Core.Constants.CountryCodes.Lesotho, Core.Constants.CountryCodes.Namibia, Core.Constants.CountryCodes.Swaziland };
			foreach (var countryCode in blnsCountryCodes)
			{
				Factor.CountryOfDestination = RefCountry.LoadFromCountryCode(Factory, countryCode);
				Factor.CPC = _11;
				Assert("Port of Exit is Optional for BLNS & required when captured for Imports with CountryOfDestination.IsBLNS", !Inst.IsPortOfExitRequired(Factor));
			}
		}

		#endregion

		public void TestShouldOutputCountryOfExport()
		{
			CombineAssertions("Import, Export", () =>
			{
				var shipmentTypes = new string[] { Import, Export };
				foreach (var shipmentType in shipmentTypes)
				{
					Factor.ShipmentType = shipmentType;
					Assert("Country/Region of Export must output for " + shipmentType, Inst.ShouldOutputCountryOfExport(Factor));
				}
			});

			CombineAssertions("Import, Export and ExBond", () =>
			{
				Factor.ShipmentType = ExBond;
				Assert("Country/Region of Export must output for EXW", !Inst.ShouldOutputCountryOfExport(Factor));
			});
		}

		public void TestShouldOutputLocationOfGoods()
		{
			CombineAssertions("Import and Export", () =>
			{
				var shipmentTypes = new string[] { Import, Export };
				var transportModes = new string[] { Air, Sea, Road };
				foreach (var shipmentType in shipmentTypes)
				{
					Factor.ShipmentType = shipmentType;
					foreach (var transportMode in transportModes)
					{
						Factor.TransportMode = transportMode;
						if (Factor.TransportMode == Road)
						{
							Assert("Location of Goods must not output for " + transportMode + " " + shipmentType, !Inst.ShouldOutputLocationOfGoods(Factor));
						}
						else
						{
							Assert("Location of Goods must output for " + transportMode + " " + shipmentType, Inst.ShouldOutputLocationOfGoods(Factor));
						}
					}
				}
			});

			CombineAssertions("Exbond", () =>
			{
				var transportModes = new string[] { Air, Sea, Road };
				Factor.ShipmentType = ExBond;
				foreach (var transportMode in transportModes)
				{
					Factor.TransportMode = transportMode;
					Assert("Location of Goods must not output for " + transportMode + " EXW", !Inst.ShouldOutputLocationOfGoods(Factor));
				}
			});
		}

		public void TestShouldOutputDateOfArrival()
		{
			CombineAssertions("Import", () =>
			{
				var transportModes = new string[] { Sea, Road };
				Factor.ShipmentType = Import;
				foreach (var transportMode in transportModes)
				{
					Factor.TransportMode = transportMode;
					Assert("Date of Arrival must output for " + transportMode + " Import", Inst.ShouldOutputDateOfArrival(Factor));
				}
			});
		}

		public void TestShouldOutputDateOfDepartureOrDateOfFlight()
		{
			CombineAssertions("Export and ExBond", () =>
			{
				var shipmentTypes = new string[] { Export, ExBond };
				var transportModes = new string[] { Air, Sea, Road };
				foreach (var shipmentType in shipmentTypes)
				{
					Factor.ShipmentType = shipmentType;
					foreach (var transportMode in transportModes)
					{
						Factor.TransportMode = transportMode;
						Assert("Date of Departure must output for " + transportMode + " " + shipmentType, Inst.ShouldOutputDateOfDepartureOrDateOfFlight(Factor));
					}
				}
			});

			Factor.ShipmentType = Import;
			Factor.TransportMode = Air;
			Assert("Date of Flight (arrival) must output for Air Imports", Inst.ShouldOutputDateOfDepartureOrDateOfFlight(Factor));
		}

		public void TestShouldOutputDateOfDeparture()
		{
			CombineAssertions("Export and ExBond", () =>
			{
				var shipmentTypes = new string[] { Export, ExBond };
				var transportModes = new string[] { Air, Sea, Road };
				foreach (var shipmentType in shipmentTypes)
				{
					Factor.ShipmentType = shipmentType;
					foreach (var transportMode in transportModes)
					{
						Factor.TransportMode = transportMode;
						Assert("Date of Departure must output for " + transportMode + " " + shipmentType, Inst.ShouldOutputDateOfDeparture(Factor));
					}
				}
			});
		}

		public void TestShouldOutputDateOfFlight()
		{
			Factor.ShipmentType = Import;
			Factor.TransportMode = Air;
			Assert("Date of Flight (arrival) must output for Air Imports", Inst.ShouldOutputDateOfFlight(Factor));
		}

		public void TestShouldOutputTransportDocumentNumber()
		{
			CombineAssertions("Import and Export", () =>
			{
				ZString[] shipmentTypes = new ZString[] { Import, Export };
				ZString[] transportModes = new ZString[] { Sea, Air, Road, Rail, Mail, FixedTransportInstallations, Other, "" };
				foreach (ZString shipmentType in shipmentTypes)
				{
					Factor.ShipmentType = shipmentType;
					foreach (ZString transportMode in transportModes)
					{
						Factor.TransportMode = transportMode;
						Assert(transportMode + " " + shipmentType + " must output TransportDocumentNumber", Inst.ShouldOutputTransportDocumentNumber(Factor));
					}
				}
			});

			CombineAssertions("ExBond", () =>
			{
				Factor.ShipmentType = ExBond;
				Factor.TransportMode = Road;
				Factor.RemovalTransportMode = Road;
				Assert("TransportDocumentNumber must output for ExBond if RemovalTransportCode is Road", Inst.ShouldOutputTransportDocumentNumber(Factor));

				ZString[] transportModes = new ZString[] { Sea, Air, Rail, Mail, FixedTransportInstallations, Other, "" };
				foreach (ZString transportMode in transportModes)
				{
					Factor.TransportMode = transportMode;
					Factor.RemovalTransportMode = ZString.Empty;
					Assert("TransportDocumentNumber must not output for ExBond if RemovalTransportCode is not Road", !Inst.ShouldOutputTransportDocumentNumber(Factor));
				}
			});
		}

		public void TestShouldOutputTransportDocumentIssuedAt()
		{
			CombineAssertions("Import and Export", () =>
			{
				ZString[] shipmentTypes = new ZString[] { Import, Export };
				ZString[] transportModes = new ZString[] { Sea, Air, Road, Rail, Mail, FixedTransportInstallations, Other, "" };
				foreach (ZString shipmentType in shipmentTypes)
				{
					Factor.ShipmentType = shipmentType;
					foreach (ZString transportMode in transportModes)
					{
						Factor.TransportMode = transportMode;
						Assert(transportMode + " " + shipmentType + " must output TransportDocumentIssuedAt", Inst.ShouldOutputTransportDocumentIssuedAt(Factor));
					}
				}
			});

			CombineAssertions("ExBond", () =>
			{
				Factor.ShipmentType = ExBond;
				Factor.TransportMode = Road;
				Factor.CountryOfDestination = RefCountry.LoadFromCountryCode(Factory, Core.Constants.CountryCodes.Lesotho);
				Assert("TransportDocumentIssuedAt must output for ExBond if CountryOfDestination.IsBLNS", Inst.ShouldOutputTransportDocumentIssuedAt(Factor));

				Factor.CountryOfDestination = RefCountry.LoadFromCountryCode(Factory, Core.Constants.CountryCodes.Germany);
				Assert("TransportDocumentIssuedAt must not output for ExBond if !CountryOfDestination.IsBLNS", !Inst.ShouldOutputTransportDocumentIssuedAt(Factor));
			});
		}

		public void TestShouldOutputDateOfAssessment()
		{
			Factor.MessageType = MessageSubTypeCodes.Codes.Cancellation;
			Assert("Date Of Assessment should not output for Cancellation", Inst.ShouldOutputDateOfAssessment(Factor));
			Factor.MessageType = MessageSubTypeCodes.Codes.Change;
			Assert("Date Of Assessment should output for Change", Inst.ShouldOutputDateOfAssessment(Factor));
			Factor.MessageType = MessageSubTypeCodes.Codes.Original;
			Assert("Date Of Assessment should not output for Original", !Inst.ShouldOutputDateOfAssessment(Factor));
			Factor.MessageType = MessageSubTypeCodes.Codes.Replace;
			Assert("Date Of Assessment should not output for Replace", Inst.ShouldOutputDateOfAssessment(Factor));
		}

		public void TestShouldOutputTransportDocumentDate()
		{
			CombineAssertions("Import and Export", () =>
			{
				ZString[] shipmentTypes = new ZString[] { Import, Export };
				ZString[] transportModes = new ZString[] { Sea, Air, Road, Rail, Mail, FixedTransportInstallations, Other, "" };
				foreach (ZString shipmentType in shipmentTypes)
				{
					Factor.ShipmentType = shipmentType;
					foreach (ZString transportMode in transportModes)
					{
						Factor.TransportMode = transportMode;
						Assert(transportMode + " " + shipmentType + " must output TransportDocumentDate", Inst.ShouldOutputTransportDocumentDate(Factor));
					}
				}
			});

			CombineAssertions("ExBond", () =>
			{
				Factor.ShipmentType = ExBond;
				Factor.TransportMode = Road;
				Factor.RemovalTransportMode = Road;
				Assert("TransportDocumentDate must output for ExBond if RemovalTransportCode is Road", Inst.ShouldOutputTransportDocumentDate(Factor));
				ZString[] transportModes = new ZString[] { Sea, Air, Rail, Mail, FixedTransportInstallations, Other, "" };
				foreach (ZString transportMode in transportModes)
				{
					Factor.TransportMode = transportMode;
					Factor.RemovalTransportMode = ZString.Empty;
					Assert("TransportDocumentDate must not output for ExBond if RemovalTransportCode is not Road", !Inst.ShouldOutputTransportDocumentDate(Factor));
				}
			});
		}

		public void TestShouldOutputPreviousProcedureMRN()
		{
			CombineAssertions("PreviousProcedureMRN", () =>
			{
				Factor.PPC = _00;
				Assert("Must not output PreviousProcedureMRN for PPC 00", !Inst.ShouldOutputPreviousMRN(Factor));
				Factor.PPC = _10;
				Assert("Must output PreviousProcedureMRN for PPC 10", Inst.ShouldOutputPreviousMRN(Factor));
				Factor.PPC = _42;
				Assert("Must output PreviousProcedureMRN for PPC 42", Inst.ShouldOutputPreviousMRN(Factor));
			});
		}

		public void TestShouldOutputPreviousMRNLineNumber()
		{
			CombineAssertions("PreviousMRNLineNumber", () =>
			{
				Factor.PPC = _00;
				Assert("Must not output PreviousMRNLineNumber for PPC 00", !Inst.ShouldOutputPreviousMRNLineNumber(Factor));
				Factor.PPC = _10;
				Assert("Must not output PreviousMRNLineNumber for PPC 10", !Inst.ShouldOutputPreviousMRNLineNumber(Factor));
				Factor.PPC = _43;
				Assert("Must output PreviousMRNLineNumber for PPC 43", Inst.ShouldOutputPreviousMRNLineNumber(Factor));
				Factor.PPC = _42;
				Assert("Must output PreviousMRNLineNumber for PPC 42", Inst.ShouldOutputPreviousMRNLineNumber(Factor));
				Factor.PPC = _45;
				Assert("Must output PreviousMRNLineNumber for PPC 45", Inst.ShouldOutputPreviousMRNLineNumber(Factor));
				Factor.PPC = _40;
				Assert("Must output PreviousMRNLineNumber for PPC 42", Inst.ShouldOutputPreviousMRNLineNumber(Factor));
				Factor.PPC = _44;
				Assert("Must output PreviousMRNLineNumber for PPC 42", Inst.ShouldOutputPreviousMRNLineNumber(Factor));
				Factor.PPC = _48;
				Assert("Must output PreviousMRNLineNumber for PPC 42", Inst.ShouldOutputPreviousMRNLineNumber(Factor));
				Factor.CPC = _36;
				Assert("Must output PreviousMRNLineNumber for CPC 36", Inst.ShouldOutputPreviousMRNLineNumber(Factor));
				Factor.CPC = _38;
				Assert("Must output PreviousMRNLineNumber for CPC 38", Inst.ShouldOutputPreviousMRNLineNumber(Factor));
				Factor.CPC = _62;
				Assert("Must output PreviousMRNLineNumber for CPC 62", Inst.ShouldOutputPreviousMRNLineNumber(Factor));
				Factor.CPC = _65;
				Assert("Must output PreviousMRNLineNumber for CPC 65", Inst.ShouldOutputPreviousMRNLineNumber(Factor));
				Factor.CPC = _66;
				Assert("Must output PreviousMRNLineNumber for CPC 66", Inst.ShouldOutputPreviousMRNLineNumber(Factor));
				Factor.CPC = _83;
				Assert("Must output PreviousMRNLineNumber for CPC 83", Inst.ShouldOutputPreviousMRNLineNumber(Factor));
			});
		}

		public void TestShouldOutputFreeForPaymentMethod()
		{
			Factor.MessageType = MessageSubTypeCodes.Codes.Original;
			Assert(!Inst.ShouldOutputFreeForPaymentMethod(Factor));
			Factor.MessageType = MessageSubTypeCodes.Codes.Change;
			Assert(!Inst.ShouldOutputFreeForPaymentMethod(Factor));
			Factor.MessageType = MessageSubTypeCodes.Codes.Cancellation;
			Assert(Inst.ShouldOutputFreeForPaymentMethod(Factor));
			Factor.MessageType = MessageSubTypeCodes.Codes.Replace;
			Assert(!Inst.ShouldOutputFreeForPaymentMethod(Factor));
			Factor.MessageType = MessageSubTypeCodes.Codes.Undefined;
			Assert(!Inst.ShouldOutputFreeForPaymentMethod(Factor));
			Factor.MessageType = ZString.Empty;
			Assert(!Inst.ShouldOutputFreeForPaymentMethod(Factor));
		}

		public void TestShouldOutputProvisionalPayments()
		{
			foreach (var shipmentType in new[] { ExBond, Import })
			{
				Factor.ShipmentType = shipmentType;
				foreach (CodeDescriptionPair declarationTypePair in new DeclarationTypeList())
				{
					Factor.DeclarationType = declarationTypePair.Code;
					var message = shipmentType + declarationTypePair.Code;
					switch (declarationTypePair.Code)
					{
						case DeclarationTypeList.Codes.RegularIncompleteDeclaration:
						case DeclarationTypeList.Codes.RegularProvisionalDeclaration:
							Assert(message, !Inst.ShouldOutputProvisionalPayments(Factor));
							break;
						case DeclarationTypeList.Codes.RegularCompleteDeclarationDefault:
						case DeclarationTypeList.Codes.RegularSupplementaryDeclaration:
							Assert(message, Inst.ShouldOutputProvisionalPayments(Factor));
							break;
					}
				}
				Factor.DeclarationType = ZString.Empty;
				Assert(Inst.ShouldOutputProvisionalPayments(Factor));
			}

			foreach (var shipmentType in new ZString[] { Export, Miscellaneous, "Whatever", ZString.Empty })
			{
				Factor.ShipmentType = shipmentType;
				AssertEquals(false, Inst.ShouldOutputProvisionalPayments(Factor));
			}
		}

		public void TestShouldOutputProvisionalPaymentsForDiamondLevy()
		{
			Factor.ShipmentType = Export;
			foreach (CodeDescriptionPair declarationTypePair in new DeclarationTypeList())
			{
				Factor.DeclarationType = declarationTypePair.Code;
				switch (declarationTypePair.Code)
				{
					case DeclarationTypeList.Codes.RegularIncompleteDeclaration:
					case DeclarationTypeList.Codes.RegularProvisionalDeclaration:
						Assert(declarationTypePair.Code, !Inst.ShouldOutputProvisionalPaymentsForDiamondLevy(Factor));
						break;
					case DeclarationTypeList.Codes.RegularCompleteDeclarationDefault:
					case DeclarationTypeList.Codes.RegularSupplementaryDeclaration:
						Assert(declarationTypePair.Code, Inst.ShouldOutputProvisionalPaymentsForDiamondLevy(Factor));
						break;
				}
			}
			Factor.DeclarationType = ZString.Empty;
			Assert(!Inst.ShouldOutputProvisionalPayments(Factor));

			foreach (var shipmentType in new ZString[] { ExBond, Import, Miscellaneous, "Whatever", ZString.Empty })
			{
				Factor.ShipmentType = shipmentType;
				AssertEquals(false, Inst.ShouldOutputProvisionalPaymentsForDiamondLevy(Factor));
			}
		}

		public void TestShouldOutputRebateUser()
		{
			CombineAssertions("Rebate User not depends on shipment Type, but depends on Concession Code", () =>
			{
				Factor.ShipmentType = Export;
				Assert("Do not output for exports", !Inst.ShouldOutputRebateUserCode(Factor));

				Factor.ShipmentType = Import;
				Assert("Do not output for imports", !Inst.ShouldOutputRebateUserCode(Factor));

				Factor.ShipmentType = ExBond;
				Assert("Do not output for exbond", !Inst.ShouldOutputRebateUserCode(Factor));
			});

			CombineAssertions("Rebate User not depends on shipment Type, but depends on Concession Code", () =>
			{
				Factor.FirstNonSpecificTariffTypeConcession = "X";
				Assert("Do not output for unknown concession", !Inst.ShouldOutputRebateUserCode(Factor));
				Assert("Do not output for unknown concession", !Inst.ShouldOutputImporterCustomsCodeAsRebateUserCode(Factor));
				Assert("Do not output for unknown concession", !Inst.ShouldOutputImporterRebateUserCodeAsRebateUserCode(Factor));

				Factor.FirstNonSpecificTariffTypeConcession = "6";
				Assert("Do not output for 6", !Inst.ShouldOutputRebateUserCode(Factor));
				Assert("Do not output for 6", !Inst.ShouldOutputImporterCustomsCodeAsRebateUserCode(Factor));
				Assert("Do not output for 6", !Inst.ShouldOutputImporterRebateUserCodeAsRebateUserCode(Factor));

				Factor.FirstNonSpecificTariffTypeConcession = "5";
				Assert("Do not output for 5", !Inst.ShouldOutputRebateUserCode(Factor));
				Assert("Do not output for 5", !Inst.ShouldOutputImporterCustomsCodeAsRebateUserCode(Factor));
				Assert("Do not output for 5", !Inst.ShouldOutputImporterRebateUserCodeAsRebateUserCode(Factor));

				Factor.FirstNonSpecificTariffTypeConcession = UniversalReferenceConstants.Schedule._4;
				Assert("Do not output for 4", Inst.ShouldOutputRebateUserCode(Factor));
				Assert("Do not output for 4", Inst.ShouldOutputImporterCustomsCodeAsRebateUserCode(Factor));
				Assert("Do not output for 4", !Inst.ShouldOutputImporterRebateUserCodeAsRebateUserCode(Factor));

				Factor.FirstNonSpecificTariffTypeConcession = UniversalReferenceConstants.Schedule._3;
				Assert("Do output for 3", Inst.ShouldOutputRebateUserCode(Factor));
				Assert("Do output for 3", !Inst.ShouldOutputImporterCustomsCodeAsRebateUserCode(Factor));
				Assert("Do output for 3", Inst.ShouldOutputImporterRebateUserCodeAsRebateUserCode(Factor));

				Factor.FirstNonSpecificTariffTypeConcession = ZString.Empty;
				Assert("Do output for Empty", !Inst.ShouldOutputRebateUserCode(Factor));
				Assert("Do output for Empty", !Inst.ShouldOutputImporterCustomsCodeAsRebateUserCode(Factor));
				Assert("Do output for Empty", !Inst.ShouldOutputImporterRebateUserCodeAsRebateUserCode(Factor));
			});
		}

		public void TestIsBlns()
		{
			var blnsCountry = Factory.NewWithValidTestData<RefCountry>();
			blnsCountry.RN_Code = "XX";
			blnsCountry.RN_EconomicGrouping = EconomicGroupList.Codes.BLNS;

			var nonBLNSCountry = Factory.NewWithValidTestData<RefCountry>();
			nonBLNSCountry.RN_Code = "YY";
			nonBLNSCountry.RN_EconomicGrouping = EconomicGroupList.Codes.EuropeanUnion;

			AssertEquals("BLNS Country -> True", true, Inst.IsBlns(blnsCountry));
			AssertEquals("NON BLNS Country/Region -> False", false, Inst.IsBlns(nonBLNSCountry));
		}

		public void TestIsShipmentImpOrExw()
		{
			var shipmentType = ZAJobMessageTypeList.Codes.Import;
			AssertEquals("IMP -> True", true, Inst.IsShipmentImpOrExw(shipmentType));

			shipmentType = ZAJobMessageTypeList.Codes.ExBond;
			AssertEquals("EXW -> True", true, Inst.IsShipmentImpOrExw(shipmentType));

			shipmentType = ZAJobMessageTypeList.Codes.Export;
			AssertEquals("EXP -> False", false, Inst.IsShipmentImpOrExw(shipmentType));
		}

		protected override void SetUp()
		{
			base.SetUp();

			Factor = new MessageDataProviderKeyFactor();
		}

		internal MessageDataProviderKeyFactor Factor;
	}
}
