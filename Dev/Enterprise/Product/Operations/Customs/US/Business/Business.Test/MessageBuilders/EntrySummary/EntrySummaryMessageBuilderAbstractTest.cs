using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Customs.US.Business.Testing;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using Moq;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Customs.US.Business.MessageBuilders.Testing
{
	abstract class EntrySummaryMessageBuilderAbstractTest : MessageBuilderTestCase
	{
		protected virtual void SetDeclarationDataApplicationSpecific(JobDeclaration declaration)
		{
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_SchDEntry = "8888";
			declaration.US_PaymentType = PaymentTypeList.Codes.IndividualBasis;
			declaration.BrokerToPayIndicator = YesNoDefaultList.Codes.No;
		}

		protected void MergeAndSend(string testReference)
		{
			MergeAndSend(testReference, false, false);
		}

		protected void MergeAndSend(ZString testReference, bool certifyCargoRelease, bool expectedMessageErrors)
		{
			foreach (Bill bill in declaration.Bills)
			{
				if (bill.IsMasterBill && bill.CU_MasterBill.IsEmpty)
				{
					bill.CU_MasterBill = "OBL" + testReference.Left(CusDecHouseBillSchema.CU_BillNum.MaxLength - 3);
				}
			}

			foreach (JobComInvoiceHeader header in declaration.Invoices)
			{
				ZDecimal amount = 0m;
				foreach (JobComInvoiceLine line in header.JobComInvoiceLines)
				{
					if (!line.IsSetXLine)
					{
						amount += line.JI_LinePrice;
					}

					if (certifyCargoRelease && line.RequiresPriorNoticeReporting())
					{
						foreach (FDA fda in line.FDAs)
						{
							fda.US_FDAValue = 1;
							var fdaBillsAvailable = fda.BillsAvailable;
							if (fdaBillsAvailable.Count > 0)
							{
								fdaBillsAvailable[0].IsForFDALine = true;
							}

							fda.US_PFR = "12345678901";
							fda.US_PFT = ProducerFirmTypeList.Codes.G;
						}
					}

					if (line.JI_CustomsUnitQty == line.JI_WeightUQ && line.JI_Weight < line.JI_CustomsQuantity)
					{
						line.JI_Weight = line.JI_CustomsQuantity;
					}

					if ((line.JI_LinePrice > 0 || line.JI_CustomsValue > 0) && line.JI_Weight.IsEmpty)
					{
						line.JI_Weight = 100m;
						line.JI_WeightUQ = "KG";
					}

					var spiList = line.AddInfoLookups.SPIList;
					if (spiList.Count == 0 || (spiList.Count == 1 && spiList[0].Code == SPICompleteList.MoreCodes.NotApplicable))
					{
						line.US_SPI = ZString.Empty;
					}
				}

				header.JZ_InvoiceAmount = amount;
				header.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

				if (header.US_UC_NKCountryOfOrigin.IsEmpty)
				{
					header.US_UC_NKCountryOfOrigin = declaration.JE_RL_NKOrigin.Left(2);
				}
			}

			declaration.JE_OwnerRef = testReference.Left(JobDeclarationSchema.JE_OwnerRef.MaxLength);
			if (declaration.IsENSFormalImport && EntryTypeList.IsValidForRecon(declaration.US_EntryType))
			{
				if (declaration.US_OtherReconIndicator.IsEmpty)
				{
					declaration.US_OtherReconIndicator = ReconIssueCodeList.Codes.NotApplicable;
				}
			}
			else
			{
				declaration.US_OtherReconIndicator = ZString.Empty;
			}

			if (declaration.InvoiceLines[0].ImportTariff != null && declaration.InvoiceLines[0].ImportTariff.UE_DutyComputationCode == ComputationCodeList.Codes.Derived)
			{
				declaration.JE_GoodsDescription = "Derived Computation Duty";
			}

			declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();
			declaration.RunPreSaveValidation();

			var messageErrors = new Customs.Business.CustomsNotificationCollector(declaration, true, false, Enterprise.Customs.Business.CustomsNotificationCollector.PropertyDescriptionType.HumanReadableName).GetMessageErrors().ToUniqueMessageListString();
			if (expectedMessageErrors)
			{
				AssertNotEquals("Messages", "", messageErrors);
			}
			else
			{
				ZString messageError = GetFinalMessageErrorsToCheck(messageErrors);

				if (!messageError.IsEmpty)
				{
					AssertEquals("Messages", "", messageError);
				}
			}

			var errors = new Customs.Business.CustomsNotificationCollector(declaration, true, false, Enterprise.Customs.Business.CustomsNotificationCollector.PropertyDescriptionType.HumanReadableName).GetErrors();
			AssertEquals("Errors", "", errors.ToUniqueMessageListString());

			PopulateMessages(declaration, testReference, certifyCargoRelease);
			Factory.Save();
		}

		protected virtual ZString GetFinalMessageErrorsToCheck(ZString messageError)
		{
			ZString result = messageError;

			if (sendTestMessagesToCustoms)
			{
				result = result.Replace("Message Error - JE_OH_Importer: There is a Power of Attorney Document on the organization (eDocs > Document Tracking), but it is not valid for 'US' Country, 'IMP' Direction and '8888' Port Of Entry.", "");
			}

			result = result.Replace("Message: Message Mode Must be ACE for this entry type.", "");

			return result;
		}

		protected virtual void PopulateMessages(JobDeclaration declaration, ZString testReference, bool certifyCargoRelease)
		{
			foreach (CusEntryHeader entryHeader in declaration.CustomsEntryHeaders)
			{
				entryHeader.CH_BGMReference = entryHeader.CH_MessageType + testReference.Left(CusEntryHeaderSchema.CH_BGMReference.MaxLength - 3);
				PopulateMessage(entryHeader, certifyCargoRelease);
			}
		}

		protected virtual void PopulateMessage(CusEntryHeader entryHeader, bool certifyCargoRelease)
		{
			EntrySummaryMessageBuilder messageBuilder = new EntrySummaryMessageBuilder(entryHeader, UpdateActionCode.Add,
				certifyCargoRelease);
			MQEDIMessage message = messageBuilder.PopulateMessage();
			entryHeader.Messages.Add(message);
		}

		protected override void SetUp()
		{
			base.SetUp();
			SetUpData();
		}

		protected void SetUpData()
		{
			var refHelper = new UniversalReferenceTestDataHelper(Factory);
			refHelper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, "PORT");
			var portList = new ZString[] { "60267", "50810", "51100", "55900", "21199", "71425", "21500", "71403", "33776", "55976", "58886", "68209", "53800", "93501" };
			foreach (var port in portList)
			{
				var foreignPort = refHelper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, port, "Test", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
				refHelper.CreateNewOrGetExistingCusCodeListAttribute(foreignPort.PK, RefCusCodeListAttributeTypes.Codes.PortValidType, ForeignPortTypeList.Codes.Common);
			}

			refHelper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CUSOF");
			var port8888 = refHelper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "8888", "Test Name", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6));
			refHelper.CreateTransportModeForCusCodeList(port8888.PK, TransportTypeList.Codes.Sea);
			refHelper.CreateTransportModeForCusCodeList(port8888.PK, TransportTypeList.Codes.Air);
			var attributeNameUnlading = refHelper.CreateNewOrGetExistingRefCusCodeListAttributeName(RefCusCodeListAttributeTypes.Codes.Unlading, "Desc", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, Core.Constants.CountryCodes.UnitedStates);
			refHelper.CreateNewOrGetExistingCusCodeListAttribute(port8888.PK, attributeNameUnlading.ZXE_Name, "Y");

			refHelper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.FIRMSTypeCode, "FIRMS");
			refHelper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.FIRMSTypeCode, "J123", "Test Name", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
			refHelper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.FIRMSTypeCode, "F629", "Test Name", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
			refHelper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.FIRMSTypeCode, "Z104", "Test Name", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
			refHelper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.FIRMSTypeCode, "S002", "Test Name", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);

			refHelper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.UnitedStates);
			var tradeGroup = refHelper.CreateTradeGroup(Core.Constants.CountryCodes.UnitedStates, Core.Constants.CountryCodes.Russia, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			var tradeGroupCountry = refHelper.AddCountry(tradeGroup, Core.Constants.CountryCodes.Russia);
			var tariffType = refHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.UnitedStates, Universal.Constants.TariffTypes.HarmonizedSystem);
			var conditionType1 = refHelper.CreateOrGetExistingRefCusConditionType(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusConditionTypes.ConditionClass.Control, UniversalReferenceConstants.TariffConditionTypes.Codes.Fishing);
			var tariff1 = refHelper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.UnitedStates, tariffType.PK, "4421909720", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			var condition1 = refHelper.CreateOrGetExistingRefCusCondition(Core.Constants.CountryCodes.UnitedStates, conditionType1.PK, tariff1.PK, "Fishing Info", true, false, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			refHelper.CreateCusApplicability(condition1, tradeGroup, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			Factory.Save();
			USCustomsDataRegistry.Instance.DoDefaultShipTo.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			helper = new DeclarationTestHelper(Factory);

			SetupTariffs("1902194000", "D AUBHCACLILJ+JOMAMXSG");
			SetupTariffs("4113903000", "D AUBHCACLILJ+JOMAMXSG");
			SetupTariffs("0712311000", "D AUBHCACLILJ+JOMAMXSG");
			SetupTariffs("2002908020", "D AUBHCACLILJ+JOMAMXSG");

			SetupImporterData();

			usCarrier = Factory.LoadTop1<USCarrierCombined>(new ZQuery(USCarrierCombinedSchema.UI_Code, "AAAA"));
			if (usCarrier == null)
			{
				usCarrier = Factory.New<USCarrierCombined>();
				usCarrier.UI_Code = "AAAA";
			}
			usCarrier.UI_ModeOfTransportation = TransportModeCodes.Codes.VesselContainer;

			SetupShippingLineData();

			mockDeclaration = Factory.NewMoq<JobDeclaration>();
			declaration = mockDeclaration.Object;
			declaration.JE_OH_Importer = importer.PK;
			declaration.IOROrgPK = importer.PK;

			declaration.JE_TotalNoOfPacksPackType = "";
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;

			declaration.US_CertifyCargoRelease = false;
			declaration.JE_TransportMode = declaration.TransportModeSeaCodeForTesting;
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.NonContainerised;
			declaration.US_UI_NKCarrierSCAC = usCarrier.UI_Code;

			declaration.JE_VesselName = "APL EMERALD";
			declaration.JE_VoyageFlightNo = "V123W";
			declaration.US_BondType = BondTypeList.Codes.ContinuousBond;
			declaration.US_BondType2 = BondTypeList.Codes.SingleTransactionBond;
			declaration.US_BondAmount2 = 10m;
			declaration.US_BondProducerAccNo2 = "1234";
			declaration.US_SchDLoading = "60267";
			declaration.JE_RL_NKPortOfLoading = "AUSYD";
			declaration.JE_RL_NKOrigin = "AUSYD";
			declaration.JE_RL_NKPortOfArrival = "USSFO";
			declaration.US_SchDArrival = "8888";
			declaration.US_SuretyCode = "891";
			declaration.US_7501Purchased = "Y";
			declaration.JE_ExportDate = ZDateTime.Today.AddDays(-7);
			declaration.JE_MasterBillIssuerSCAC = usCarrier.UI_Code;
			declaration.US_IsHMFApplicable = YesNoDefaultList.Codes.No;

			if (!sendTestMessagesToCustoms)
			{
				// This sets a registry item and SendTestMessagesToCustoms mode is not transactionedtestcase
				var poaDocument = declaration.DocsAndCartage.RequiredDocuments.AddNew();
				poaDocument.EQ_DocType = "POA";
				poaDocument.EQ_DocDescription = "Power of Attorney";
				poaDocument.EQ_DocPeriod = Core.Constants.JobRequiredDocuments.DocumentPeriods.OncePerShipment;
				poaDocument.EQ_DateReceived = ZDateTimeOffset.Today.AddMonths(-2);
			}

			declaration.JE_OH_ShippingLine = shippingLine.PK;

			declaration.PrimaryMasterBill.CU_NoOfPacks = 1;
			declaration.PrimaryMasterBill.CU_PackType = DeclarationTestHelper.BillUS_ManifestUQForTest;
			SetDeclarationDataApplicationSpecific(declaration);

			invoiceHeader = declaration.Invoices.AddNew();
			SetupManufacturerData();
			invoiceHeader.JZ_OA_SupplierAddress = manufacturer.MainAddress.PK;

			invoiceHeader.US_TransactionsRelated = "N";
			invoiceHeader.JZ_InvoiceNumber = "INV1232";
			invoiceHeader.JZ_IncoTerm = Constants.IncoTerms.FreeOnBoard;
			invoiceHeader.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoiceHeader.Charges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.OverseasFreight, 50m, "USD");

			invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "4421909720";
			invoiceLine.JI_Description = "COMMERCIAL DESCRIPTION";

			invoiceLine.JI_Weight = 9000m;
			invoiceLine.JI_LinePrice = 10000m;
			invoiceLine.JI_InvoiceQuantity = 10000m;
			invoiceLine.JI_InvoiceUQ = "NO";
			invoiceLine.JI_CustomsQuantity = 70m;
			invoiceLine.US_UC_NKCountryOfExport = "AU";
			invoiceLine.US_UC_NKCountryOfOrigin = "AU";
			invoiceLine.US_SPI = SPICompleteList.MoreCodes.NotApplicable;
			invoiceHeader.JZ_OA_ManufacturerAddress = manufacturer.MainAddress.PK;

			declaration.US_DestinationState = "IL";

			new FeeCalculationHelperTest().PrepareFeeAndTexData();
		}

		void SetupTariffs(string tariffNumber, string spiCode)
		{
			var tariff = Factory.LoadTop1<USCTariff>(new ZQuery(USCTariffSchema.UE_Tariff, tariffNumber));
			if (tariff == null)
			{
				tariff = Factory.New<USCTariff>();
				tariff.UE_Tariff = tariffNumber;
				tariff.UE_DateFrom = ZDateTime.BrettsBirthday;
				tariff.UE_DateTo = ZDateTime.MaxSmallDateTime;
			}
			tariff.UE_SPICode = spiCode;
		}

		void SetupManufacturerData()
		{
			manufacturer = Factory.Load<OrgHeader>(manufacturerPK);
			if (manufacturer == null)
			{
				manufacturer = Factory.New<OrgHeader>();
				manufacturer.OH_Code = "MAN" + new Random().Next(1000000).ToString();
				manufacturer.OH_FullName = "Mr manufacturer";
				manufacturer.OH_IsConsignor = true;
				manufacturer.OH_RL_NKClosestPort = "AUSYD";

				manufacturerCode = manufacturer.MainAddress.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.ManufacturerID, "AUSOUPAC195PAD", GlbCompany.CurrentCompany.Country);
			}
			else
			{
				if (manufacturer.Contacts.Count > 0)
				{
					contact = manufacturer.Contacts[0];
				}

				manufacturerCode = manufacturer.MainAddress.CustomsCodes.GetOrgCusCodeObjectForCodeTypeAndCountry(OrgCusCode.USACodeTypes.ManufacturerID, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			}
		}

		void SetupShippingLineData()
		{
			shippingLine = Factory.Load<OrgHeader>(shippingLinePK);
			if (shippingLine == null)
			{
				shippingLine = Factory.New<OrgHeader>();
				shippingLine.OH_Code = "SHP" + new Random().Next(1000000).ToString();
				shippingLine.OH_IsShippingLine = true;
				shippingLine.OH_FullName = "Mr Shipping Line";

				scacCode = shippingLine.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CarrierCode, "AAAA", GlbCompany.CurrentCompany.Country);
			}
			else
			{
				scacCode = shippingLine.CustomsCodes.GetOrgCusCode(OrgCusCode.CodeTypes.CarrierCode, GlbCompany.CurrentCompany.Country);
			}
		}

		void SetupImporterData()
		{
			importer = Factory.Load<OrgHeader>(importerPK);
			if (importer == null)
			{
				importer = Factory.New<OrgHeader>();
				importer.OH_FullName = "Mr Importer";
				importer.OH_Code = "IMP" + new Random().Next(1000000).ToString();
				importer.OH_IsConsignee = true;

				importerCustomsCode = importer.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "91-013199000", GlbCompany.CurrentCompany.Country);
				OrgCountryData countryData = importer.CountryData;
				OrgImpAddInfo addInfo = new OrgImpAddInfo((ZPropertyInfoString)countryData.OV_ImportCustomsDefaultAddInfoInfo);
				addInfo.ZO_IsEINNumberVerifiedIndicator = YesNoDefaultList.Codes.Yes;
			}
			else
			{
				importerCustomsCode = importer.CustomsCodes.GetOrgCusCode(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, GlbCompany.CurrentCompany.Country);
			}
		}

		protected Mock<JobDeclaration> mockDeclaration;
		protected DeclarationTestHelper helper;
		protected JobDeclaration declaration;
		protected JobComInvoiceHeader invoiceHeader;
		protected JobComInvoiceLine invoiceLine;
		protected OrgCusCode scacCode;
		protected OrgHeader importer;
		protected OrgHeader shippingLine;
		protected OrgHeader manufacturer;
		protected OrgContact contact;
		protected OrgCusCode manufacturerCode;
		protected OrgCusCode importerCustomsCode;
		protected USCarrierCombined usCarrier;
	}
}
