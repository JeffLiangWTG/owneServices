using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.DataTransfer;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Business.MessageBuilders;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Customs.US.DataTransfer
{
	class USDeclarationValueObjectDataAdapter : DeclarationValueObjectDataAdapter, Integration.Customs.US.IUSDeclarationValueObjectDataAdapter
	{
		public USDeclarationValueObjectDataAdapter()
		{
		}

		public USDeclarationValueObjectDataAdapter(EventsWithSourceType triggeredByEvents)
			: base(triggeredByEvents)
		{
		}

		#region Overrides

		protected override string AddInfoPrefix
		{
			get { return "US_"; }
		}

		protected override InvoiceValueObjectDataAdapter GetNewInvoiceAdapter(BaseJobDeclaration jobDec)
		{
			return new USInvoiceDataAdapter((JobDeclaration)jobDec);
		}

		protected override InvoicesGeneratorFromXSD GetNewInvoicesGenerator(BaseJobDeclaration jobDec)
		{
			return new USInvoicesGeneratorFromXSD(jobDec);
		}

		protected override BaseJobDeclaration NewBusinessObject(Xsd.ConsolAndShipment value, IValueObjectImportContext context)
		{
			return context.Factory.New<JobDeclaration>();
		}

		#endregion

		#region Import

		protected override void ImportDeclarationDetails(BaseJobDeclaration jobDec, Xsd.Declaration declarationXsd, ZString masterBillOnConsol, IValueObjectImportContext context)
		{
			base.ImportDeclarationDetails(jobDec, declarationXsd, masterBillOnConsol, context);
			JobDeclaration declaration = jobDec as JobDeclaration;
			Xsd.USDeclaration xmlDeclaration = null;
			if (declaration != null)
			{
				xmlDeclaration = declarationXsd.CountryPayload.USDeclaration;
				ImportUSDeclarationPayloadData(declaration, xmlDeclaration, context);
				declaration.ResumeApportionment();
				if (declaration.IsImport)
				{
					declaration.JE_MessageSubType = ZString.Empty;
				}

				ImportBillsDetails(declaration, masterBillOnConsol, xmlDeclaration.BillsOfLading);
			}
		}

		void ImportBillsDetails(JobDeclaration declaration, ZString masterBillNo, Xsd.USDeclarationBillOfLadingCollection billsOfLading)
		{
			if (billsOfLading.IsSpecified)
			{
				foreach (Xsd.USDeclarationBillOfLading xmlBill in billsOfLading)
				{
					Business.Bill bill = null;
					if (!xmlBill.ParentBillNumber.IsEmpty)
					{
						bill = declaration.Bills.FindAnyBillWithHouseBillMasterBillCombination(xmlBill.BillNumber, xmlBill.ParentBillNumber);
					}
					else
					{
						bill = declaration.Bills.FindByBillNumberAndType(xmlBill.BillNumber, xmlBill.BillType);
					}

					if (bill != null)
					{
						if (xmlBill.AMSCarrierSpecified)
						{
							bill.US_AMSCarrierIndicator = xmlBill.AMSCarrier;
						}

						if (xmlBill.IssuerSCACSpecified)
						{
							bill.US_UI_NKBillIssuerSCAC = xmlBill.IssuerSCAC;
						}

						if (xmlBill.ITNumberSpecified)
						{
							var itNos = bill.ITAndSplitDetails.FindByItNumber(xmlBill.ITNumber);

							if (itNos.Length == 0)
							{
								var itNo = bill.ITAndSplitDetails.AddNew();
								itNo.US_ITNumber = xmlBill.ITNumber;
								if (xmlBill.ManifestQty.IsSpecified)
								{
									itNo.US_NoOfPacks = xmlBill.ManifestQty.Value.ToZInt();
									bill.CU_PackType = xmlBill.ManifestQty.DimensionType;
									bill.CU_NoOfPacks = (ZDecimal)bill.ITAndSplitDetails.TotalNoOfPacks;
								}
							}
						}
						else if (xmlBill.ManifestQty.IsSpecified)
						{
							bill.CU_NoOfPacks = xmlBill.ManifestQty.Value;
							bill.CU_PackType = xmlBill.ManifestQty.DimensionType;
						}
					}
				}
			}
		}

		void ImportUSDeclarationPayloadData(JobDeclaration declaration, Xsd.USDeclaration xmlDeclaration, IValueObjectImportContext context)
		{
			if (xmlDeclaration.DateOfArrivalSpecified)
			{
				declaration.US_EntryDate = xmlDeclaration.DateOfArrival;
			}

			#region Import (IMX, MSC) Shipment Type Specific Details

			if (!declaration.IsExport)
			{
				ImportBondDetails(declaration, xmlDeclaration.Bond, context);

				if (xmlDeclaration.CarrierSCACSpecified)
				{
					declaration.US_UI_NKCarrierSCAC = xmlDeclaration.CarrierSCAC.Left(AutoUSAddInfo.Schema.US_UI_NKCarrierSCACMaxLength);
				}

				if (xmlDeclaration.ConsolidatedInformalSpecified)
				{
					context.SetPropertyInfoValue(declaration.US_ConsolidatedInformalIndicatorInfo, ConsolidatedInformalToXmlCodeMappings.Instance.GetEnterpriseCode(xmlDeclaration.ConsolidatedInformal.ToString(), "", context), true, "Declaration Consolidated Informal");
				}

				if (xmlDeclaration.CentralizedExamSiteSpecified)
				{
					declaration.US_US_NKCentralizedExamSite = xmlDeclaration.CentralizedExamSite.Left(AutoUSAddInfo.Schema.US_US_NKCentralizedExamSiteMaxLength);
				}

				if (xmlDeclaration.EntryDateElectionCodeSpecified)
				{
					context.SetPropertyInfoValue(declaration.US_EntryDateElectionCodeInfo, EntryDateElectionCodeToXmlCodeMapping.Instance.GetEnterpriseCode(xmlDeclaration.EntryDateElectionCode.ToString(), "", context), true, "Declaration Entry Date Election Code");
				}

				ImportEntryTypeDetails(declaration, xmlDeclaration.EntryType, context);

				if (xmlDeclaration.EstimatedEntryDateSpecified)
				{
					declaration.US_EstimatedEntryDate = xmlDeclaration.EstimatedEntryDate;
				}

				if (xmlDeclaration.EntryFilerCodeSpecified)
				{
					declaration.US_EntryFilerCode = xmlDeclaration.EntryFilerCode.Left(AutoUSAddInfo.Schema.US_EntryFilerCodeMaxLength);
				}

				if (xmlDeclaration.GeneralOrderNumberSpecified)
				{
					declaration.US_GeneralOrderNo = xmlDeclaration.GeneralOrderNumber.Left(AutoUSAddInfo.Schema.US_GeneralOrderNoMaxLength);
				}

				if (xmlDeclaration.HMFApplicableSpecified)
				{
					declaration.US_IsHMFApplicable = (xmlDeclaration.HMFApplicable == Xsd.TrueFalse.@true) ? YesNoDefaultList.Codes.Yes : YesNoDefaultList.Codes.No;
				}

				if (xmlDeclaration.LiveEntrySpecified)
				{
					declaration.US_LiveEntryIndicator = (xmlDeclaration.LiveEntry == Xsd.TrueFalse.@true) ? YesNoDefaultList.Codes.Yes : YesNoDefaultList.Codes.No;
				}

				if (xmlDeclaration.ImporterOfRecordDetails.PurchasedSpecified)
				{
					declaration.US_7501Purchased = (xmlDeclaration.ImporterOfRecordDetails.Purchased == Xsd.TrueFalse.@true) ? YesNoDefaultList.Codes.Yes : YesNoDefaultList.Codes.No;
				}

				if (xmlDeclaration.LocationOfGoodsSpecified)
				{
					declaration.US_US_NKLocationOfGoods = xmlDeclaration.LocationOfGoods.Left(AutoUSAddInfo.Schema.US_US_NKLocationOfGoodsMaxLength);
				}

				ImportWarehouseDetails(declaration, xmlDeclaration.Warehouse);

				if (xmlDeclaration.MissingDocument1Specified)
				{
					declaration.US_MissingDocument1 = xmlDeclaration.MissingDocument1.Left(AutoUSAddInfo.Schema.US_MissingDocument1MaxLength);
				}

				if (xmlDeclaration.MissingDocument2Specified)
				{
					declaration.US_MissingDocument2 = xmlDeclaration.MissingDocument2.Left(AutoUSAddInfo.Schema.US_MissingDocument2MaxLength);
				}

				if (xmlDeclaration.OGALineReleaseIndicatorSpecified)
				{
					context.SetPropertyInfoValue(declaration.US_OGALineReleaseIndicatorInfo, OGALineReleaseIndicatorToXmlCodeMappings.Instance.GetEnterpriseCode(xmlDeclaration.OGALineReleaseIndicator.ToString(), "", context), true, "Declaration OGA Line Release Indicator");
				}

				ImportAIIOrganizationsDetails(declaration, xmlDeclaration.Organisations, context);

				if (xmlDeclaration.PresentationDateSpecified)
				{
					declaration.US_PresentationDate = xmlDeclaration.PresentationDate;
				}

				if (xmlDeclaration.ITDateSpecified)
				{
					declaration.US_ITDate = xmlDeclaration.ITDate;
				}

				ImportPaymentDetails(declaration, xmlDeclaration.ImporterOfRecordDetails.PaymentTypeSpecified, xmlDeclaration.ImporterOfRecordDetails.PaymentType, xmlDeclaration.Payment, context);

				if (xmlDeclaration.PreparerOfficeCodeSpecified)
				{
					declaration.US_PreparerOfficeCode = xmlDeclaration.PreparerOfficeCode.Left(declaration.US_PreparerOfficeCodeInfo.MaxLength);
				}

				ImportReconDetails(declaration, xmlDeclaration.Recon, context);

				if (xmlDeclaration.ImporterOfRecordDetails.TaxDeferredIndSpecified)
				{
					context.SetPropertyInfoValue(declaration.US_TaxDeferIndicatorInfo, TaxDefferableToXmlCodeMappings.Instance.GetEnterpriseCode(xmlDeclaration.ImporterOfRecordDetails.TaxDeferredInd.ToString(), "", context), true, "Declaration Tax Defferable");
				}

				if (xmlDeclaration.TeamNoSpecified)
				{
					declaration.US_TeamNo = xmlDeclaration.TeamNo.Left(declaration.US_TeamNoInfo.MaxLength);
				}

				ImportPriorNoticeDetails(declaration, xmlDeclaration.PriorNotice, context);
			}

			#endregion

			ImportPortsDetails(declaration, xmlDeclaration.Ports, context);

			#region Export Shipment Type Specific Properties

			declaration.US_SchDExport = xmlDeclaration.Ports.Export.Left(declaration.US_SchDExportInfo.MaxLength);
			declaration.US_DateOfExport = xmlDeclaration.ExportDate;
			declaration.US_TransportReference = xmlDeclaration.TransportReference.Left(declaration.US_TransportReferenceInfo.MaxLength);
			declaration.US_RN_NKCountryOfDestination = xmlDeclaration.CountryOfUltimateDestination.Left(declaration.US_RN_NKCountryOfDestinationInfo.MaxLength);
			declaration.US_UC_NKCountryOfExport = xmlDeclaration.CountryOfExport.Left(declaration.US_UC_NKCountryOfExportInfo.MaxLength);
			if (xmlDeclaration.FilingOptionSpecified)
			{
				context.SetPropertyInfoValue(declaration.US_CommodityFilingOptionInfo, FilingOptionToXmlCodeMappings.Instance.GetEnterpriseCode(xmlDeclaration.FilingOption.ToString(), "", context), true, "Export Filing Option");
			}
			declaration.US_SoldEnRouteIndicator = xmlDeclaration.SoldEnRoute.Left(declaration.US_SoldEnRouteIndicatorInfo.MaxLength);
			if (declaration.US_SoldEnRouteIndicator == YesNoDefaultList.Codes.Yes)
			{
				declaration.US_RN_NKFirstPortOfCallCountry = xmlDeclaration.FirstCountry;
				declaration.US_FirstPortOfCallCity = xmlDeclaration.FirstCity;
			}

			#endregion
		}

		void ImportReconDetails(JobDeclaration declaration, Xsd.USReconciliation xmlRecon, IValueObjectImportContext context)
		{
			if (xmlRecon.IssueSpecified)
			{
				context.SetPropertyInfoValue(declaration.US_OtherReconIndicatorInfo, ReconciliationIssueToXmlCodeMappings.Instance.GetEnterpriseCode(xmlRecon.Issue.ToString(), "", context), true, "Declaration Reconciliation Issue");
			}

			if (xmlRecon.NAFTASpecified)
			{
				declaration.US_NAFTAReconIndicator = xmlRecon.NAFTA == Xsd.TrueFalse.@true;
			}
		}

		void ImportPortsDetails(JobDeclaration declaration, Xsd.USDeclarationPorts xmlPorts, IValueObjectImportContext context)
		{
			if (xmlPorts.IsSpecified)
			{
				if (xmlPorts.DesignatedExamPortSpecified)
				{
					context.SetPropertyInfoValue(declaration.US_SchDExamInfo, xmlPorts.DesignatedExamPort);
				}

				if (xmlPorts.DischargeSpecified)
				{
					context.SetPropertyInfoValue(declaration.US_SchDArrivalInfo, xmlPorts.Discharge);
				}

				if (xmlPorts.LoadingSpecified)
				{
					context.SetPropertyInfoValue(declaration.US_SchDLoadingInfo, xmlPorts.Loading);
				}

				if (xmlPorts.PortOfEntrySpecified)
				{
					context.SetPropertyInfoValue(declaration.US_SchDEntryInfo, xmlPorts.PortOfEntry);
				}

				if (xmlPorts.PreparerDistrictPortSpecified)
				{
					context.SetPropertyInfoValue(declaration.US_PreparerDistrictPortInfo, xmlPorts.PreparerDistrictPort);
				}
			}
		}

		void ImportPaymentDetails(JobDeclaration declaration, bool paymentTypeSpecified, Xsd.USImporterOfRecordDetailsPaymentType paymentType, Xsd.USDeclarationPayment xmlPayment, IValueObjectImportContext context)
		{
			if (paymentTypeSpecified)
			{
				context.SetPropertyInfoValue(declaration.US_PaymentTypeInfo, PaymentTypeToXmlCodeMappings.Instance.GetEnterpriseCode(paymentType.ToString(), "", context), true, "Declaration Payment Type");
			}

			if (xmlPayment.IsSpecified)
			{
				if (xmlPayment.ClientBranchDesignationSpecified)
				{
					declaration.US_ClientBranchDesignation = xmlPayment.ClientBranchDesignation.Left(declaration.US_ClientBranchDesignationInfo.MaxLength);
				}

				if (xmlPayment.PreliminaryStatementPrintDateSpecified)
				{
					declaration.US_PreliminaryStatementPrintDate = xmlPayment.PreliminaryStatementPrintDate;
				}

				if (xmlPayment.PreliminaryStatementMonthSpecified)
				{
					context.SetPropertyInfoValue(declaration.US_PeriodicStatementMMInfo, PreliminaryStatementMonthToXmlCodeMappings.Instance.GetEnterpriseCode(xmlPayment.PreliminaryStatementMonth.ToString(), "", context), true, "Declaration Preliminary Statement Month");
				}

				if (xmlPayment.CheckNoSpecified)
				{
					declaration.US_CheckNo = xmlPayment.CheckNo.Left(declaration.US_CheckNoInfo.MaxLength);
				}
			}
		}

		void ImportWarehouseDetails(JobDeclaration declaration, Xsd.USDeclarationWarehouse xmlWarehouse)
		{
			if (xmlWarehouse.IsSpecified)
			{
				if (xmlWarehouse.EntryFilerCodeSpecified)
				{
					declaration.US_WHSEntryFilerCode = xmlWarehouse.EntryFilerCode.Left(AutoUSAddInfo.Schema.US_WHSEntryFilerCodeMaxLength);
				}

				if (xmlWarehouse.EntryNumberSpecified)
				{
					declaration.US_WHSEntryNumber = xmlWarehouse.EntryNumber.Left(JobDeclaration.Schema.US_WHSEntryNumberMaxLength);
				}

				if (xmlWarehouse.FinalWHSSpecified)
				{
					declaration.US_IsFinalWHS = xmlWarehouse.FinalWHS;
				}

				if (xmlWarehouse.PortSpecified)
				{
					declaration.US_WHSDistrictPortCode = xmlWarehouse.Port.Left(AutoUSAddInfo.Schema.US_WHSDistrictPortCodeMaxLength);
				}
			}
		}

		void ImportBondDetails(JobDeclaration declaration, Xsd.USDeclarationBond xmlBond, IValueObjectImportContext context)
		{
			if (xmlBond.IsSpecified)
			{
				if (xmlBond.TypeSpecified)
				{
					context.SetPropertyInfoValue(declaration.US_BondTypeInfo, BondTypeToXmlCodeMappings.Instance.GetEnterpriseCode(xmlBond.Type.ToString(), "", context), true, "Declaration Bond Type");
				}

				if (xmlBond.AccountNumberSpecified)
				{
					declaration.US_BondProducerAccNo = xmlBond.AccountNumber.Left(AutoUSAddInfo.Schema.US_BondProducerAccNoMaxLength);
				}

				if (xmlBond.ADDCVDSuretyCodeSpecified)
				{
					declaration.US_ADDCVDSuretyCode = xmlBond.ADDCVDSuretyCode.Left(AutoUSAddInfo.Schema.US_ADDCVDSuretyCodeMaxLength);
				}

				if (xmlBond.AmountSpecified)
				{
					declaration.US_BondAmount = xmlBond.Amount;
				}

				if (xmlBond.SuretyCodeSpecified)
				{
					declaration.US_SuretyCode = xmlBond.SuretyCode.Left(AutoUSAddInfo.Schema.US_SuretyCodeMaxLength);
				}
			}
		}

		void ImportEntryTypeDetails(JobDeclaration declaration, Xsd.USDeclarationEntryType xmlEntryType, IValueObjectImportContext context)
		{
			if (xmlEntryType.IsSpecified)
			{
				if (xmlEntryType.CertifyCargoReleaseSpecified)
				{
					declaration.US_CertifyCargoRelease = xmlEntryType.CertifyCargoRelease == Xsd.TrueFalse.@true;
				}

				if (xmlEntryType.EnableCargoReleaseSpecified)
				{
					declaration.US_EnableCRL = xmlEntryType.EnableCargoRelease == Xsd.TrueFalse.@true;
				}

				if (xmlEntryType.EnableEntrySummarySpecified)
				{
					declaration.US_EnableENS = xmlEntryType.EnableEntrySummary == Xsd.TrueFalse.@true;
				}

				if (xmlEntryType.EnableElectronicInvoiceSpecified)
				{
					declaration.US_EnableAII = xmlEntryType.EnableElectronicInvoice == Xsd.TrueFalse.@true;
				}

				if (xmlEntryType.EnableInBondSpecified)
				{
					declaration.US_EnableINB = xmlEntryType.EnableInBond == Xsd.TrueFalse.@true;
				}

				if (xmlEntryType.ModeSpecified)
				{
					context.SetPropertyInfoValue(declaration.US_EntryModeInfo, EntryTypeModeToXmlCodeMappings.Instance.GetEnterpriseCode(xmlEntryType.Mode, "", context), xmlEntryType.ModeSpecified, "Job Declaration Entry Mode");
				}

				if (xmlEntryType.EntryTypeSpecified)
				{
					declaration.US_EntryType = xmlEntryType.EntryType.Left(AutoUSAddInfo.Schema.US_EntryTypeMaxLength);
				}
			}
		}

		void ImportPriorNoticeDetails(JobDeclaration declaration, Xsd.USDeclarationPriorNotice xmlPriorNotice, IValueObjectImportContext context)
		{
			if (xmlPriorNotice.IsSpecified)
			{
				if (xmlPriorNotice.ActualTimeOfArrival.IsValid)
				{
					declaration.US_FDAADTA = xmlPriorNotice.ActualTimeOfArrival;
				}

				if (xmlPriorNotice.PortOfCrossingSpecified)
				{
					declaration.US_FDAAPC = xmlPriorNotice.PortOfCrossing.Left(declaration.US_FDAAPCInfo.MaxLength);
				}

				if (xmlPriorNotice.ContactNameSpecified)
				{
					declaration.US_FDAContactName = xmlPriorNotice.ContactName.Left(declaration.US_FDAContactNameInfo.MaxLength);
				}

				if (xmlPriorNotice.ContactPhoneNoSpecified)
				{
					declaration.US_FDAContactPhoneNo = xmlPriorNotice.ContactPhoneNo.Left(declaration.US_FDAContactPhoneNoInfo.MaxLength);
				}

				if (xmlPriorNotice.Submitter.IsSpecified)
				{
					declaration.JE_OH_FDASubmitter = GetMatchedOrganisation(xmlPriorNotice.Submitter, context, declaration, OrganisationTypes.None);
				}

				if (xmlPriorNotice.Carrier.Item is Xsd.USPriorNoticeCarrierType)
				{
					Xsd.USPriorNoticeCarrierType carrier = (Xsd.USPriorNoticeCarrierType)xmlPriorNotice.Carrier.Item;
					if (carrier.IsSpecified)
					{
						declaration.US_FDACANType = FDACarrierTypeList.Codes.Carrier;
						if (carrier.CountrySpecified)
						{
							declaration.US_FDACCN = carrier.Country;
						}

						if (carrier.NameSpecified)
						{
							declaration.US_FDACAN = carrier.Name;
						}
					}
				}
				else if (xmlPriorNotice.Carrier.Item is Xsd.USPriorNoticePrivatelyOwnFNVehicleType)
				{
					Xsd.USPriorNoticePrivatelyOwnFNVehicleType fNVehicle = (Xsd.USPriorNoticePrivatelyOwnFNVehicleType)xmlPriorNotice.Carrier.Item;
					if (fNVehicle.IsSpecified)
					{
						declaration.US_FDACANType = FDACarrierTypeList.Codes.PrivatelyOwnedFNVehicle;
						if (fNVehicle.CountrySpecified)
						{
							declaration.US_FDACCN = fNVehicle.Country;
						}

						if (fNVehicle.ProvinceSpecified)
						{
							declaration.US_FDACAN = fNVehicle.Province;
						}
					}
				}
				else if (xmlPriorNotice.Carrier.Item is Xsd.USPriorNoticePrivatelyOwnVehicleType)
				{
					Xsd.USPriorNoticePrivatelyOwnVehicleType uSVehicle = (Xsd.USPriorNoticePrivatelyOwnVehicleType)xmlPriorNotice.Carrier.Item;
					if (uSVehicle.IsSpecified)
					{
						declaration.US_FDACANType = FDACarrierTypeList.Codes.PrivatelyOwnedUSVehicle;
						if (uSVehicle.StateSpecified)
						{
							declaration.US_FDACCN = uSVehicle.State;
						}

						if (uSVehicle.LicenseSpecified)
						{
							declaration.US_FDACAN = uSVehicle.License;
						}
					}
				}
			}
		}

		void ImportAIIOrganizationsDetails(JobDeclaration declaration, Xsd.USDeclarationOrganisations organisations, IValueObjectImportContext context)
		{
			if (organisations.Buyer.IsSpecified)
			{
				declaration.JE_OH_Buyer = GetMatchedOrganisation(organisations.Buyer, context, declaration, OrganisationTypes.None);
			}

			if (organisations.BuyingAgent.IsSpecified)
			{
				declaration.JE_OH_BuyingAgent = GetMatchedOrganisation(organisations.BuyingAgent, context, declaration, OrganisationTypes.None);
			}

			if (organisations.CBPBroker.IsSpecified)
			{
				declaration.JE_OH_CBPBroker = GetMatchedOrganisation(organisations.CBPBroker, context, declaration, OrganisationTypes.Broker);
			}

			if (organisations.Exporter.IsSpecified)
			{
				declaration.JE_OH_Exporter = GetMatchedOrganisation(organisations.Exporter, context, declaration, OrganisationTypes.None);
			}

			if (organisations.ImporterOfRecord.IsSpecified)
			{
				AdaptersHelper.SetOrgDetailsToBusinessObjectOrganization(organisations.ImporterOfRecord.Item, context, declaration.IOROrgPKInfo);
			}

			if (organisations.Invoicer.IsSpecified)
			{
				AdaptersHelper.SetOrgAddressDetails(organisations.Invoicer, context, declaration.JE_OA_InvoicerAddressInfo);
			}

			if (organisations.Manufacturer.IsSpecified)
			{
				AdaptersHelper.FindOrCreateOrgFromMID(organisations.Manufacturer.Item, context, declaration.JE_OA_ManufacturerAddressInfo);
			}

			if (organisations.Seller.IsSpecified)
			{
				AdaptersHelper.SetOrgAddressDetails(organisations.Seller, context, declaration.JE_OA_SellerAddressInfo);
			}

			if (organisations.SellingAgent.IsSpecified)
			{
				declaration.JE_OH_SellingAgent = GetMatchedOrganisation(organisations.SellingAgent, context, declaration, OrganisationTypes.None);
			}

			if (organisations.UltimateConsignee.IsSpecified)
			{
				AdaptersHelper.SetOrgDetailsToBusinessObjectOrganization(organisations.UltimateConsignee, context, declaration.JE_OA_ConsigneeAddressInfo);
			}

			if (organisations.NotifyParty.IsSpecified)
			{
				AdaptersHelper.SetOrgDetailsToBusinessObjectOrganization(organisations.NotifyParty.Item, context, declaration.JE_OH_NotifyPartyInfo);
			}
		}

		#endregion

		#region Export

		protected override void ExportToValueObjectCore(BaseJobDeclaration jobDec, Xsd.ConsolAndShipment result, IValueObjectExportContext context)
		{
			base.ExportToValueObjectCore(jobDec, result, context);

			JobDeclaration declaration = (JobDeclaration)jobDec;

			Xsd.USDeclaration xmlDeclaration = new Xsd.USDeclaration();
			xmlDeclaration.IsSpecified = true;
			ExportUSDeclarationPayloadData(declaration, xmlDeclaration, context);

			result.Shipment.Declaration.CountryPayload.USDeclaration = xmlDeclaration;
			if (declaration.IsImport)
			{
				result.Shipment.ShipmentDetails.DeclarationStyle = ZString.Empty;
			}
		}

		void ExportUSDeclarationPayloadData(JobDeclaration declaration, Xsd.USDeclaration xmlDeclaration, IValueObjectExportContext context)
		{
			ExportBills(declaration.Bills, xmlDeclaration);
			xmlDeclaration.DateOfArrival = declaration.US_EntryDate.Date;

			if (!declaration.IsExport)
			{
				#region Import (IMX, MSC) Shipment Type related Properties

				#region Bond

				xmlDeclaration.Bond.AccountNumber = declaration.US_BondProducerAccNo;
				xmlDeclaration.Bond.ADDCVDSuretyCode = declaration.US_ADDCVDSuretyCode;
				xmlDeclaration.Bond.Amount = declaration.US_BondAmount;
				xmlDeclaration.Bond.AmountSpecified = true;
				xmlDeclaration.Bond.SuretyCode = declaration.US_SuretyCode;
				if (!declaration.US_BondType.IsEmpty)
				{
					xmlDeclaration.Bond.Type = BondTypeToXmlCodeMappings.Instance.GetExternalCode(declaration.US_BondType, "", context);
					xmlDeclaration.Bond.TypeSpecified = true;
				}

				#endregion

				xmlDeclaration.CarrierSCAC = declaration.CarrierCodeForEntrySummary;
				xmlDeclaration.CentralizedExamSite = declaration.US_US_NKCentralizedExamSite;
				if (!declaration.US_ConsolidatedInformalIndicator.IsEmpty)
				{
					xmlDeclaration.ConsolidatedInformal = ConsolidatedInformalToXmlCodeMappings.Instance.GetExternalCode(declaration.US_ConsolidatedInformalIndicator, "", context);
					xmlDeclaration.ConsolidatedInformalSpecified = true;
				}

				if (!declaration.US_EntryDateElectionCode.IsEmpty)
				{
					xmlDeclaration.EntryDateElectionCode = EntryDateElectionCodeToXmlCodeMapping.Instance.GetExternalCode(declaration.US_EntryDateElectionCode, "", context);
					xmlDeclaration.EntryDateElectionCodeSpecified = true;
				}

				#region Entry Type

				xmlDeclaration.EntryType.EntryType = declaration.US_EntryType;
				xmlDeclaration.EntryType.CertifyCargoRelease = declaration.US_CertifyCargoRelease ? Xsd.TrueFalse.@true : Xsd.TrueFalse.@false;
				xmlDeclaration.EntryType.CertifyCargoReleaseSpecified = true;
				xmlDeclaration.EntryType.EnableCargoRelease = declaration.US_EnableCRL ? Xsd.TrueFalse.@true : Xsd.TrueFalse.@false;
				xmlDeclaration.EntryType.EnableCargoReleaseSpecified = true;
				xmlDeclaration.EntryType.EnableEntrySummary = declaration.US_EnableENS ? Xsd.TrueFalse.@true : Xsd.TrueFalse.@false;
				xmlDeclaration.EntryType.EnableEntrySummarySpecified = true;
				xmlDeclaration.EntryType.EnableElectronicInvoice = declaration.US_EnableAII ? Xsd.TrueFalse.@true : Xsd.TrueFalse.@false;
				xmlDeclaration.EntryType.EnableElectronicInvoiceSpecified = true;
				xmlDeclaration.EntryType.EnableInBond = declaration.US_EnableINB ? Xsd.TrueFalse.@true : Xsd.TrueFalse.@false;
				xmlDeclaration.EntryType.EnableInBondSpecified = true;
				if (!declaration.US_EntryMode.IsEmpty)
				{
					xmlDeclaration.EntryType.Mode = EntryTypeModeToXmlCodeMappings.Instance.GetExternalCode(declaration.US_EntryMode, "", context);
					xmlDeclaration.EntryType.ModeSpecified = true;
				}

				#endregion

				xmlDeclaration.EntryFilerCode = declaration.US_EntryFilerCode;
				xmlDeclaration.EstimatedEntryDate = declaration.US_EstimatedEntryDate.Date;
				xmlDeclaration.GeneralOrderNumber = declaration.US_GeneralOrderNo;

				if (!declaration.US_IsHMFApplicable.IsEmpty)
				{
					xmlDeclaration.HMFApplicable = declaration.US_IsHMFApplicable == YesNoDefaultList.Codes.Yes ? Xsd.TrueFalse.@true : Xsd.TrueFalse.@false;
					xmlDeclaration.HMFApplicableSpecified = true;
				}

				if (!declaration.US_LiveEntryIndicator.IsEmpty)
				{
					xmlDeclaration.LiveEntry = declaration.US_LiveEntryIndicator == YesNoDefaultList.Codes.Yes ? Xsd.TrueFalse.@true : Xsd.TrueFalse.@false;
					xmlDeclaration.LiveEntrySpecified = true;
				}

				if (!declaration.US_7501Purchased.IsEmpty)
				{
					xmlDeclaration.ImporterOfRecordDetails.Purchased = declaration.US_7501Purchased == YesNoDefaultList.Codes.Yes ? Xsd.TrueFalse.@true : Xsd.TrueFalse.@false;
					xmlDeclaration.ImporterOfRecordDetails.PurchasedSpecified = true;
				}

				#region Warehouse

				xmlDeclaration.Warehouse.IsSpecified = false;
				if (!declaration.US_WHSEntryFilerCode.IsEmpty || !declaration.US_WHSEntryNumber.IsEmpty || !declaration.US_WHSDistrictPortCode.IsEmpty || declaration.US_IsFinalWHS)
				{
					xmlDeclaration.Warehouse.IsSpecified = true;
					xmlDeclaration.Warehouse.EntryFilerCode = declaration.US_WHSEntryFilerCode;
					xmlDeclaration.Warehouse.EntryNumber = declaration.US_WHSEntryNumber;
					xmlDeclaration.Warehouse.FinalWHS = declaration.US_IsFinalWHS;
					xmlDeclaration.Warehouse.FinalWHSSpecified = true;
					xmlDeclaration.Warehouse.Port = declaration.US_WHSDistrictPortCode;
				}

				#endregion

				xmlDeclaration.LocationOfGoods = declaration.LocationOfGoods != null ? declaration.LocationOfGoods.ZZD_Code : ZString.Empty;
				xmlDeclaration.MissingDocument1 = declaration.US_MissingDocument1;
				xmlDeclaration.MissingDocument2 = declaration.US_MissingDocument2;
				if (!declaration.US_OGALineReleaseIndicator.IsEmpty)
				{
					xmlDeclaration.OGALineReleaseIndicator = OGALineReleaseIndicatorToXmlCodeMappings.Instance.GetExternalCode(declaration.US_OGALineReleaseIndicator, "", context);
					xmlDeclaration.OGALineReleaseIndicatorSpecified = true;
				}

				ExportAIIOrganizationsDetails(declaration, xmlDeclaration.Organisations, context);
				ExportPriorNoticeDetails(declaration, xmlDeclaration, context);
				ExportStatusDetails(declaration, xmlDeclaration);

				#region Payment

				xmlDeclaration.Payment.ClientBranchDesignation = declaration.US_ClientBranchDesignation;
				xmlDeclaration.Payment.CheckNo = declaration.US_CheckNo;
				if (!declaration.US_PeriodicStatementMM.IsEmpty)
				{
					xmlDeclaration.Payment.PreliminaryStatementMonth = PreliminaryStatementMonthToXmlCodeMappings.Instance.GetExternalCode(declaration.US_PeriodicStatementMM, "", context);
					xmlDeclaration.Payment.PreliminaryStatementMonthSpecified = true;
				}
				xmlDeclaration.Payment.PreliminaryStatementPrintDate = declaration.US_PreliminaryStatementPrintDate.Date;
				if (!declaration.US_PaymentType.IsEmpty)
				{
					xmlDeclaration.ImporterOfRecordDetails.PaymentType = PaymentTypeToXmlCodeMappings.Instance.GetExternalCode(declaration.US_PaymentType, "", context);
					xmlDeclaration.ImporterOfRecordDetails.PaymentTypeSpecified = true;
				}

				#endregion

				#region Reconciliation

				if (!declaration.US_OtherReconIndicator.IsEmpty)
				{
					xmlDeclaration.Recon.Issue = ReconciliationIssueToXmlCodeMappings.Instance.GetExternalCode(declaration.US_OtherReconIndicator, "", context);
				}
				if (declaration.US_NAFTAReconIndicator)
				{
					xmlDeclaration.Recon.NAFTA = declaration.US_NAFTAReconIndicator ? Xsd.TrueFalse.@true : Xsd.TrueFalse.@false;
					xmlDeclaration.Recon.NAFTASpecified = true;
				}

				#endregion

				xmlDeclaration.PreparerOfficeCode = declaration.US_PreparerOfficeCode;
				xmlDeclaration.PresentationDate = declaration.US_PresentationDate.Date;
				xmlDeclaration.ITDate = declaration.US_ITDate.Date;

				if (!declaration.US_TaxDeferIndicator.IsEmpty)
				{
					xmlDeclaration.ImporterOfRecordDetails.TaxDeferredInd = TaxDefferableToXmlCodeMappings.Instance.GetExternalCode(declaration.US_TaxDeferIndicator, "", context);
					xmlDeclaration.ImporterOfRecordDetails.TaxDeferredIndSpecified = true;
				}
				xmlDeclaration.TeamNo = declaration.US_TeamNo;

				xmlDeclaration.ExportDateSpecified = false;
				xmlDeclaration.FilingOptionSpecified = false;

				#endregion
			}
			else
			{
				#region Export Shipment Type related Properties

				xmlDeclaration.Ports.Export = declaration.US_SchDExport;
				xmlDeclaration.ExportDate = declaration.US_DateOfExport.Date;
				xmlDeclaration.TransportReference = declaration.US_TransportReference;
				xmlDeclaration.CountryOfUltimateDestination = declaration.US_RN_NKCountryOfDestination;
				xmlDeclaration.CountryOfExport = declaration.US_UC_NKCountryOfExport;
				xmlDeclaration.FilingOption = FilingOptionToXmlCodeMappings.Instance.GetExternalCode(declaration.US_CommodityFilingOption, "", context);
				xmlDeclaration.SoldEnRoute = declaration.US_SoldEnRouteIndicator;
				if (declaration.US_SoldEnRouteIndicator == YesNoDefaultList.Codes.Yes)
				{
					xmlDeclaration.FirstCountry = declaration.US_RN_NKFirstPortOfCallCountry;
					xmlDeclaration.FirstCity = declaration.US_FirstPortOfCallCity;
				}

				#endregion

				xmlDeclaration.Organisations.IsSpecified = false;
			}

			xmlDeclaration.ImporterOfRecordDetails.IsSpecified = xmlDeclaration.ImporterOfRecordDetails.PaymentTypeSpecified ||
				xmlDeclaration.ImporterOfRecordDetails.PurchasedSpecified ||
				xmlDeclaration.ImporterOfRecordDetails.TaxDeferredIndSpecified;

			#region Ports

			xmlDeclaration.Ports.DesignatedExamPort = declaration.US_SchDExam;
			xmlDeclaration.Ports.Discharge = declaration.US_SchDArrival;
			xmlDeclaration.Ports.Loading = declaration.US_SchDLoading;
			xmlDeclaration.Ports.PortOfEntry = declaration.US_SchDEntry;
			xmlDeclaration.Ports.PreparerDistrictPort = declaration.US_PreparerDistrictPort;

			#endregion
		}

		void ExportBills(BillCollection bills, Xsd.USDeclaration xmlDeclaration)
		{
			foreach (Business.Bill bill in bills)
			{
				if (bill.NoITNumbersExist)
				{
					var xmlBill = new Xsd.USDeclarationBillOfLading();
					xmlBill.AMSCarrier = bill.US_AMSCarrierIndicator;
					xmlBill.BillNumber = bill.CU_BillNum;
					xmlBill.BillType = bill.CU_BillType;
					xmlBill.IssuerSCAC = bill.US_UI_NKBillIssuerSCAC;
					xmlBill.ManifestQty.DimensionType = bill.CU_PackType;
					xmlBill.ManifestQty.Value = bill.CU_NoOfPacks;
					xmlBill.ManifestQty.IsSpecified = bill.CU_NoOfPacks > 0;

					if (bill.ParentBill != null)
					{
						xmlBill.ParentBillNumber = bill.ParentBill.CU_BillNum;
					}
					xmlBill.ShouldCreateElementForEmptyValue = false;

					xmlDeclaration.BillsOfLading.Add(xmlBill);
				}
				else
				{
					foreach (IBillDetails billDetails in bill.ITAndSplitDetails)
					{
						var xmlBill = new Xsd.USDeclarationBillOfLading();
						xmlBill.AMSCarrier = bill.US_AMSCarrierIndicator;
						xmlBill.BillNumber = bill.CU_BillNum;
						xmlBill.BillType = bill.CU_BillType;
						xmlBill.IssuerSCAC = bill.US_UI_NKBillIssuerSCAC;
						xmlBill.ITNumber = billDetails.ITNumber;
						xmlBill.ManifestQty.DimensionType = billDetails.PackageType;
						xmlBill.ManifestQty.Value = (ZDecimal)billDetails.PackageQuantity;
						xmlBill.ManifestQty.IsSpecified = billDetails.PackageQuantity > 0;

						if (bill.ParentBill != null)
						{
							xmlBill.ParentBillNumber = bill.ParentBill.CU_BillNum;
						}
						xmlBill.ShouldCreateElementForEmptyValue = false;

						xmlDeclaration.BillsOfLading.Add(xmlBill);
					}
				}
			}
		}

		void ExportPriorNoticeDetails(JobDeclaration declaration, Xsd.USDeclaration xmlDeclaration, IValueObjectExportContext context)
		{
			if (declaration.US_FDAADTA.IsValid)
			{
				xmlDeclaration.PriorNotice.ActualTimeOfArrival = declaration.US_FDAADTA;
			}
			xmlDeclaration.PriorNotice.PortOfCrossing = declaration.US_FDAAPC;
			xmlDeclaration.PriorNotice.Submitter = OrganisationDataAdapter.ExportToValueObject(declaration.FDASubmitter, context);
			xmlDeclaration.PriorNotice.Submitter.IsSpecified = true;

			xmlDeclaration.PriorNotice.ContactName = declaration.US_FDAContactName;
			xmlDeclaration.PriorNotice.ContactPhoneNo = declaration.US_FDAContactPhoneNo;

			switch (declaration.US_FDACANType)
			{
				case FDACarrierTypeList.Codes.Carrier:
					xmlDeclaration.PriorNotice.Carrier.Item = new Xsd.USPriorNoticeCarrierType();
					Xsd.USPriorNoticeCarrierType carrier = (Xsd.USPriorNoticeCarrierType)xmlDeclaration.PriorNotice.Carrier.Item;
					carrier.Country = declaration.US_FDACCN;
					carrier.Name = declaration.US_FDACAN;
					break;
				case FDACarrierTypeList.Codes.PrivatelyOwnedFNVehicle:
					xmlDeclaration.PriorNotice.Carrier.Item = new Xsd.USPriorNoticePrivatelyOwnFNVehicleType();
					Xsd.USPriorNoticePrivatelyOwnFNVehicleType fNVehicle = (Xsd.USPriorNoticePrivatelyOwnFNVehicleType)xmlDeclaration.PriorNotice.Carrier.Item;
					fNVehicle.Country = declaration.US_FDACCN;
					fNVehicle.Province = declaration.US_FDACAN;
					break;
				case FDACarrierTypeList.Codes.PrivatelyOwnedUSVehicle:
					xmlDeclaration.PriorNotice.Carrier.Item = new Xsd.USPriorNoticePrivatelyOwnVehicleType();
					Xsd.USPriorNoticePrivatelyOwnVehicleType uSVehicle = (Xsd.USPriorNoticePrivatelyOwnVehicleType)xmlDeclaration.PriorNotice.Carrier.Item;
					uSVehicle.State = declaration.US_FDACCN;
					uSVehicle.License = declaration.US_FDACAN;
					break;
			}
		}

		void ExportStatusDetails(JobDeclaration declaration, Xsd.USDeclaration xmlDeclaration)
		{
			xmlDeclaration.Status.DutyDueDate = declaration.US_PaymentDueDate.Date;
			xmlDeclaration.Status.EntryDate = declaration.EntryDate;

			CusLiquidation liquidation = declaration.Liquidations.GetMostRecentLiquidation();
			if (liquidation != null)
			{
				xmlDeclaration.Status.LiquidationDate = liquidation.B8_LiquidationDate.Date;
				if (!liquidation.B8_LiquidationType.IsEmpty)
				{
					xmlDeclaration.Status.LiquidationType = liquidation.B8_LiquidationType;
				}
			}

			foreach (DispositionData disposition in declaration.DispositionCodes)
			{
				Xsd.USDeclarationStatusDisposition xmlDisposition = new Xsd.USDeclarationStatusDisposition();
				xmlDisposition.ActionCode = disposition.US_Code;
				xmlDisposition.DateTime = disposition.US_DispositionDate;
				xmlDisposition.Narrative = disposition.DispositionCodeDesc;
				xmlDisposition.ReleaseDateTime = disposition.US_ReleaseDate;
				xmlDisposition.ReleaseOrigin = disposition.US_ReleaseOrigin;

				xmlDeclaration.Status.Dispositions.Add(xmlDisposition);
			}

			foreach (OGADispositionData ogaDisposition in declaration.OGADispositionCodes)
			{
				Xsd.USDeclarationStatusOGADisposition xmlOGADisposition = new Xsd.USDeclarationStatusOGADisposition();
				xmlOGADisposition.BeginCBPLine = ogaDisposition.US_OGADispositionBeginningCBPLine;
				xmlOGADisposition.BeginOGALine = ogaDisposition.US_OGADispositionBeginningOGALine;
				xmlOGADisposition.DateTime = ogaDisposition.US_DispositionDate;
				xmlOGADisposition.EndCBPLine = ogaDisposition.US_OGADispositionEndCBPLine;
				xmlOGADisposition.EndOGALine = ogaDisposition.US_OGADispositionEndOGALine;
				xmlOGADisposition.EntryLevelCode = ogaDisposition.US_OGADispositionStatusCode;
				xmlOGADisposition.LineLevelCode = ogaDisposition.US_Code;
				xmlOGADisposition.Message = ogaDisposition.US_OGADispositionStatusMessage;
				xmlOGADisposition.Qualifier = ogaDisposition.US_OGAIdentifier;
				xmlOGADisposition.RangeIndicator = ogaDisposition.US_OGADispositionRangeIndicator;

				xmlDeclaration.Status.OGADispositions.Add(xmlOGADisposition);
			}
		}

		void ExportAIIOrganizationsDetails(JobDeclaration declaration, Xsd.USDeclarationOrganisations organizations, IValueObjectExportContext context)
		{
			organizations.Buyer = OrganisationDataAdapter.ExportToValueObject(declaration.Buyer, context);
			organizations.Buyer.IsSpecified = true;
			organizations.BuyingAgent = OrganisationDataAdapter.ExportToValueObject(declaration.BuyingAgent, context);
			organizations.BuyingAgent.IsSpecified = true;
			organizations.CBPBroker = OrganisationDataAdapter.ExportToValueObject(declaration.CBPBroker, context);
			organizations.CBPBroker.IsSpecified = true;
			organizations.Exporter = OrganisationDataAdapter.ExportToValueObject(declaration.Exporter, context);
			organizations.Exporter.IsSpecified = true;

			Xsd.Organisation org;
			if (declaration.IOR != null)
			{
				org = OrganisationDataAdapter.ExportToValueObject(declaration.IOR, context);
				organizations.ImporterOfRecord.Item = org;
				organizations.ImporterOfRecord.IsSpecified = true;
			}

			if (declaration.InvoicerAddress != null)
			{
				organizations.Invoicer = OrganisationDataAdapter.ExportToValueObject(declaration.InvoicerAddress.Header, context);
				AdaptersHelper.SetAddressSequence(organizations.Invoicer.OrganisationDetails.Addresses, declaration.InvoicerAddress.OA_Address1, declaration.InvoicerAddress.OA_Address2);
				organizations.Invoicer.IsSpecified = true;
			}

			if (declaration.ManufacturerAddress != null)
			{
				org = OrganisationDataAdapter.ExportToValueObject(declaration.ManufacturerAddress.Header, context);
				organizations.Manufacturer.Item = org;
				AdaptersHelper.SetAddressSequence(((Xsd.Organisation)organizations.Manufacturer.Item).OrganisationDetails.Addresses, declaration.ManufacturerAddress.OA_Address1, declaration.ManufacturerAddress.OA_Address2);
				organizations.Manufacturer.IsSpecified = true;
			}

			organizations.Seller = OrganisationDataAdapter.ExportToValueObject(declaration.Seller, context);
			organizations.Seller.IsSpecified = true;
			organizations.SellingAgent = OrganisationDataAdapter.ExportToValueObject(declaration.SellingAgent, context);
			organizations.SellingAgent.IsSpecified = true;

			if (declaration.ConsigneeOrgAddress != null)
			{
				org = OrganisationDataAdapter.ExportToValueObject(declaration.ConsigneeOrgAddress, context);
				organizations.UltimateConsignee.Item = org;
				organizations.UltimateConsignee.IsSpecified = true;
			}

			if (declaration.NotifyParty != null)
			{
				org = OrganisationDataAdapter.ExportToValueObject(declaration.NotifyParty, context);
				organizations.NotifyParty.Item = org;
				organizations.NotifyParty.IsSpecified = true;
			}
		}

		#endregion

		#region Adapters and Related Objects

		OrganisationValueObjectDataAdapter OrganisationDataAdapter
		{
			get { return organisationDataAdapter ?? (organisationDataAdapter = new OrganisationValueObjectDataAdapter()); }
		}
		OrganisationValueObjectDataAdapter organisationDataAdapter;

		public AdaptersHelper AdaptersHelper
		{
			get { return adaptersHelper ?? (adaptersHelper = new AdaptersHelper()); }
		}
		AdaptersHelper adaptersHelper;

		#endregion
	}
}

#region Declaration Export/Import
#region Related Objects
#endregion
#endregion
#region Implementation
#endregion
