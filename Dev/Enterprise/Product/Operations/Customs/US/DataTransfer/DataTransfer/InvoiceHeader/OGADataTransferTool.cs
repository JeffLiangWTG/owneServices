using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Business;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Customs.US.DataTransfer
{
	public class OGADataTransferTool
	{
		public OGADataTransferTool() { }

		#region Import OGA Details

		public void ImportDOTDetails(Xsd.USDOTCollection xmlDOTs, DOTCollection dOTs)
		{
			foreach (Xsd.USDOT xmlDOT in xmlDOTs)
			{
				DOT dot = dOTs.Factory.New<DOT>();
				dOTs.Add(dot);
				ImportDOTsDetails(xmlDOT, dot);
			}
		}

		void ImportDOTsDetails(Xsd.USDOT xmlDOT, DOT dot)
		{
			NotificationBuffer notify = new NotificationBuffer();
			ValueObjectImportContext context = new ValueObjectImportContext(dot.Factory, notify);

			context.SetPropertyInfoValue(dot.US_DOTBoxNoInfo, xmlDOT.BoxNumber);
			context.SetPropertyInfoValue(dot.US_DOTClarCodeInfo, xmlDOT.Clarification);
			context.SetPropertyInfoValue(dot.US_DOTCommercialDescInfo, xmlDOT.CommercialDesc);
			context.SetPropertyInfoValue(dot.US_DOTCountryOfOriginInfo, xmlDOT.CountryOfOrigin);
			dot.US_DOTImpSubstStatement = xmlDOT.ImporterStatement;
			context.SetPropertyInfoValue(dot.US_DOTPassportInfo, xmlDOT.PassportNo);
			dot.US_DOTPriorApproval = xmlDOT.PriorApproved;
			context.SetPropertyInfoValue(dot.US_DOTBondSuretyCodeInfo, xmlDOT.SuretyCode);
			context.SetPropertyInfoValue(dot.US_DOTTireBrandNameInfo, xmlDOT.Tire.BrandName);
			context.SetPropertyInfoValue(dot.US_DOTTireIDInfo, xmlDOT.Tire.ID);

			Xsd.USDOTVehicleDetailCollection vehicleDetails = new Xsd.USDOTVehicleDetailCollection();

			foreach (Xsd.USDOTVehicleDetail xmlDOTVIN in xmlDOT.VehicleDetails)
			{
				DOTVIN dotVIN = dot.DOTVINs.AddNew();
				context.SetPropertyInfoValue(dotVIN.US_DOTVENInfo, xmlDOTVIN.EligibilityNo);
				context.SetPropertyInfoValue(dotVIN.US_DOTMakeInfo, xmlDOTVIN.Make);
				context.SetPropertyInfoValue(dotVIN.US_DOTModelInfo, xmlDOTVIN.Model);
				context.SetPropertyInfoValue(dotVIN.US_DOTRINoInfo, xmlDOTVIN.NHTSANumber);
				context.SetPropertyInfoValue(dotVIN.US_DOTVINInfo, xmlDOTVIN.VIN);
				dotVIN.US_DOTYear = xmlDOTVIN.Year;
			}
		}

		public void ImportFCCDetails(Xsd.USFCCCollection xmlFCCs, FCCCollection fCCs)
		{
			foreach (Xsd.USFCC xmlFCC in xmlFCCs)
			{
				FCC fcc = fCCs.Factory.New<FCC>();
				fCCs.Add(fcc);
				ImportFCCsDetails(xmlFCC, fcc);
			}
		}

		void ImportFCCsDetails(Xsd.USFCC xmlFCC, FCC fcc)
		{
			fcc.US_FCCCommercialDesc = xmlFCC.CommercialDesc.Left(FCCAddInfo.Schema.US_FCCCommercialDescMaxLength);
			fcc.US_FCCID = xmlFCC.ID.Left(FCCAddInfo.Schema.US_FCCIDMaxLength);
			fcc.US_FCCImpCondNo = xmlFCC.ImportConditionNo.Left(FCCAddInfo.Schema.US_FCCImpCondNoMaxLength);
			fcc.US_FCCImpCondNoQtyAppr = xmlFCC.ImportConditionQtyApproved;
			fcc.US_FCCModel = xmlFCC.Model.Left(FCCAddInfo.Schema.US_FCCModelMaxLength);
			fcc.US_FCCQty = xmlFCC.Qty;
			fcc.US_FCCTradeName = xmlFCC.TradeName.Left(FCCAddInfo.Schema.US_FCCTradeNameMaxLength);
			fcc.US_FCCWithhold = xmlFCC.Withhold;
		}

		#region Import FDA

		public void ImportFDADetails(Xsd.USFDACollection xmlFDAs, FDACollection fDAs, IValueObjectImportContext context)
		{
			foreach (Xsd.USFDA xmlFDA in xmlFDAs)
			{
				FDA fda = fDAs.Factory.New<FDA>();
				fDAs.Add(fda);
				ImportFDAsDetails(xmlFDA, fda, context);
			}
		}

		void ImportFDAsDetails(Xsd.USFDA xmlFDA, FDA fda, IValueObjectImportContext context)
		{
			foreach (Xsd.USAffirmationCode xmlCode in xmlFDA.AffirmationCodes)
			{
				AffirmationCode code = fda.Factory.New<AffirmationCode>();
				fda.AffirmationCodes.Add(code);
				code.CY_Code = xmlCode.Code.Left(AffirmationCode.Schema.CY_CodeMaxLength);
				code.CY_Data = xmlCode.Value.Left(code.CY_DataInfo.MaxLength);
			}

			fda.US_TradeBrandName = xmlFDA.BrandName.Left(USFDAAddInfo.Schema.US_TradeBrandNameMaxLength);
			fda.US_FDACargoStorageCode = xmlFDA.CargoStorageCode.Left(fda.US_FDACargoStorageCodeInfo.MaxLength);
			if (!xmlFDA.Value.IsEmpty)
			{
				context.AddMessageError(ValueFieldIsObseleteWarning);
			}
			fda.US_InvCurrFDAValue = xmlFDA.InvValue;
			fda.US_FDACommercialDesc = xmlFDA.CommercialDesc.Left(USFDAAddInfo.Schema.US_FDACommercialDescMaxLength);
			fda.US_UC_NKFDAProduction = xmlFDA.CountryOfProduction.Left(USFDAAddInfo.Schema.US_UC_NKFDAProductionMaxLength);

			fda.US_DimUQ = ContainerDimensionUQToXmlCodeMappings.Instance.GetEnterpriseCode(xmlFDA.InnermostContainer.DimensionUQ, "Container Dimension UQ", context);
			fda.US_ContainerDim1 = xmlFDA.InnermostContainer.Dimension1;
			fda.US_ContainerDim2 = xmlFDA.InnermostContainer.Dimension2;
			fda.US_ContainerDim3 = xmlFDA.InnermostContainer.Dimension3;
			fda.US_FDAContainerDimType = xmlFDA.InnermostContainer.Shape.Left(fda.US_FDAContainerDimTypeInfo.MaxLength);

			ImportOrganizationsDetails(xmlFDA, fda, context);

			ImportPriorNoticeDetails(xmlFDA, fda, context);

			fda.US_FDAProductCode = xmlFDA.ProductCode.Left(fda.US_FDAProductCodeInfo.MaxLength);
			fda.US_FDAQty1 = xmlFDA.Quantities.Quantity1.Value;
			fda.US_FDAMeasure1 = xmlFDA.Quantities.Quantity1.DimensionType.Left(fda.US_FDAMeasure1Info.MaxLength);
			fda.US_FDAQty2 = xmlFDA.Quantities.Quantity2.Value;
			fda.US_FDAMeasure2 = xmlFDA.Quantities.Quantity2.DimensionType.Left(fda.US_FDAMeasure2Info.MaxLength);
			fda.US_FDAQty3 = xmlFDA.Quantities.Quantity3.Value;
			fda.US_FDAMeasure3 = xmlFDA.Quantities.Quantity3.DimensionType.Left(fda.US_FDAMeasure3Info.MaxLength);
			fda.US_FDAQty4 = xmlFDA.Quantities.Quantity4.Value;
			fda.US_FDAMeasure4 = xmlFDA.Quantities.Quantity4.DimensionType.Left(fda.US_FDAMeasure4Info.MaxLength);
			fda.US_FDAQty5 = xmlFDA.Quantities.Quantity5.Value;
			fda.US_FDAMeasure5 = xmlFDA.Quantities.Quantity5.DimensionType.Left(fda.US_FDAMeasure5Info.MaxLength);
			fda.US_FDAQty6 = xmlFDA.Quantities.Quantity6.Value;
			fda.US_FDAMeasure6 = xmlFDA.Quantities.Quantity6.DimensionType.Left(fda.US_FDAMeasure6Info.MaxLength);
		}

		internal const string ValueFieldIsObseleteWarning = "Warning: Field USInvoiceLine/FDAs/Value is obsolete. Use InvValue instead.";

		void ImportOrganizationsDetails(Xsd.USFDA xmlFDA, FDA fda, IValueObjectImportContext context)
		{
			if (xmlFDA.Manufacturer.IsSpecified)
			{
				AdaptersHelper.FindOrCreateOrgFromMID(xmlFDA.Manufacturer.Item, context, fda.US_FDAManufacturerAddressInfo);
			}

			if (xmlFDA.EstablismentID.IsSpecified)
			{
				AdaptersHelper.FindOrCreateOrgFromRegistrationTypeIfNeeded(xmlFDA.EstablismentID.Item, context, fda.US_OA_FDAFEIInfo, OrgCusCode.CodeTypes.FDAEstablishmentIdentifier, null);
			}

			if (xmlFDA.Shipper.IsSpecified)
			{
				AdaptersHelper.FindOrCreateOrgFromMID(xmlFDA.Shipper.Item, context, fda.US_FDAShipperAddressInfo);
			}
		}

		void ImportPriorNoticeDetails(Xsd.USFDA xmlFDA, FDA fda, IValueObjectImportContext context)
		{
			if (xmlFDA.OwnerFirmTypeSpecified)
			{
				context.SetPropertyInfoValue(fda.US_OFTInfo, FDAOwnerFirmTypeToXmlCodeMappings.Instance.GetEnterpriseCode(xmlFDA.OwnerFirmType.ToString(), "", context), true, "FDA Owner Firm Type");
			}

			fda.US_CSH = xmlFDA.CountryOfShipping.Left(fda.US_CSHInfo.MaxLength);
			fda.US_SFR = xmlFDA.ShipperRegistrationNumber.Left(fda.US_SFRInfo.MaxLength);
			fda.US_PFR = xmlFDA.ManufacturerProducerData.FoodFacilityRegistrationNumber.Left(fda.US_PFRInfo.MaxLength);

			if (xmlFDA.ManufacturerProducerData.FoodFacilityRegistrationExemptionSpecified)
			{
				context.SetPropertyInfoValue(fda.US_FMEInfo, FDAFoodFacilityRegToXmlCodeMappings.Instance.GetEnterpriseCode(xmlFDA.ManufacturerProducerData.FoodFacilityRegistrationExemption.ToString(), "", context), true, "FDA Food Facility Registration Exemption");
			}

			if (xmlFDA.ManufacturerProducerData.ProducerFirmTypeSpecified)
			{
				context.SetPropertyInfoValue(fda.US_PFTInfo, FDAProducerFirmTypeToXmlCodeMappings.Instance.GetEnterpriseCode(xmlFDA.ManufacturerProducerData.ProducerFirmType.ToString(), "", context), true, "FDA Producer Firm Type");
			}

			foreach (Xsd.USFDABill xmlBill in xmlFDA.Bills)
			{
				SetBillForFDALine(xmlBill.HouseBill, fda.BillsAvailable);
				SetBillForFDALine(xmlBill.MasterBill, fda.BillsAvailable);
			}

			foreach (Xsd.USOGAContainer xmlContainer in xmlFDA.Containers)
			{
				FDARelatedContainer container = fda.ContainersForInvoiceLine.FindByContainerNumber(xmlContainer.ContainerNumber);
				if (container != null)
				{
					container.IsForFDALine = true;
				}
			}
		}

		void SetBillForFDALine(ZString billNo, FDARelatedBillsCollection billsAvailable)
		{
			FDARelatedBill relatedBill = billsAvailable.FindByBillNumber(billNo);
			if (relatedBill != null)
			{
				relatedBill.IsForFDALine = true;
			}
		}

		#endregion

		#endregion

		#region Import Lacey Act Data

		public PGACollection ImportLaceyActDetails(Xsd.USLaceyActCollection xmlPGAs, PGACollection pGAs, BusinessObjectFactory factory, bool shouldImportValueAndContainers, NonPersistentCusContainerCollection containersForInvoiceLines)
		{
			foreach (Xsd.USLaceyAct xmlLaceyAct in xmlPGAs)
			{
				PGA pGA = pGAs.AddNew();

				pGA.US_PGACommercialDescription = xmlLaceyAct.CommercialDescription.Left(pGA.US_PGACommercialDescriptionInfo.MaxLength);

				if (shouldImportValueAndContainers)
				{
					foreach (Xsd.USOGAContainer xmlContainer in xmlLaceyAct.Containers)
					{
						NonPersistentCusContainer container = (NonPersistentCusContainer)containersForInvoiceLines.FindByContainerNumber(xmlContainer.ContainerNumber);
						if (container != null)
						{
							RelatedContainer relatedContainer = new RelatedContainer(pGA);
							relatedContainer.SetContainerInvoiceLinePivot(container.Pivot);
							relatedContainer.IsForPGALine = true;
						}
					}

					pGA.US_PGALineValue = (ZDecimal)xmlLaceyAct.Value;
				}

				foreach (Xsd.USConstituentElementDetail xmlElement in xmlLaceyAct.ConstituentElements)
				{
					ConstituentElement element = factory.New<ConstituentElement>();

					element.US_PGANameOfTheConstituentElement = xmlElement.Name.Left(element.US_PGANameOfTheConstituentElementInfo.MaxLength);
					element.US_PGAPercentOfConstituentElement = xmlElement.Percent;
					element.US_PGAQuantityOfConstituentElement = xmlElement.Quantity;
					element.US_PGAUnitOfMeasure = xmlElement.UOM.Left(element.US_PGAUnitOfMeasureInfo.MaxLength);

					foreach (Xsd.USScientificData xmlScientificData in xmlElement.ScientificData)
					{
						ScientificData scientificData = factory.New<ScientificData>();
						scientificData.US_PGACountryCode = xmlScientificData.Country.Left(scientificData.US_PGACountryCodeInfo.MaxLength);
						scientificData.US_PGAScientificGenusName = xmlScientificData.GenusName.Left(scientificData.US_PGAScientificGenusNameInfo.MaxLength);
						scientificData.US_PGAScientificSpeciesName = xmlScientificData.SpeciesName.Left(scientificData.US_PGAScientificSpeciesNameInfo.MaxLength);

						element.ScientificDataCollection.Add(scientificData);
					}

					pGA.PG04ConstituentElements.Add(element);
				}
			}

			return pGAs;
		}

		#endregion

		#region Export OGA Data

		#region Export DOTs Details

		public Xsd.USDOTCollection ExportDOTsDetails(DOTCollection dOTs)
		{
			Xsd.USDOTCollection xmlDOTs = new Xsd.USDOTCollection();

			foreach (DOT dot in dOTs)
			{
				Xsd.USDOT xmlDOT = xmlDOTs.AddNew();
				ExportDOTDetails(xmlDOT, dot);
			}

			return xmlDOTs;
		}

		void ExportDOTDetails(Xsd.USDOT xmlDOT, DOT dot)
		{
			xmlDOT.BoxNumber = dot.US_DOTBoxNo;
			xmlDOT.Clarification = dot.US_DOTClarCode;
			xmlDOT.CommercialDesc = dot.US_DOTCommercialDesc;
			xmlDOT.CountryOfOrigin = dot.US_DOTCountryOfOrigin;
			xmlDOT.ImporterStatement = dot.US_DOTImpSubstStatement;
			xmlDOT.ImporterStatementSpecified = true;
			xmlDOT.PassportNo = dot.US_DOTPassport;
			xmlDOT.PriorApproved = dot.US_DOTPriorApproval;
			xmlDOT.PriorApprovedSpecified = true;
			xmlDOT.SuretyCode = dot.US_DOTBondSuretyCode.ToString();
			xmlDOT.Tire.BrandName = dot.US_DOTTireBrandName;
			xmlDOT.Tire.ID = dot.US_DOTTireID;

			Xsd.USDOTVehicleDetailCollection vehicleDetails = new Xsd.USDOTVehicleDetailCollection();

			foreach (DOTVIN dotVIN in dot.DOTVINs)
			{
				Xsd.USDOTVehicleDetail vin = vehicleDetails.AddNew();

				vin.EligibilityNo = dotVIN.US_DOTVEN;
				vin.Make = dotVIN.US_DOTMake;
				vin.Model = dotVIN.US_DOTModel;
				vin.NHTSANumber = dotVIN.US_DOTRINo;
				vin.VIN = dotVIN.US_DOTVIN;
				vin.Year = dotVIN.US_DOTYear;
				vin.YearSpecified = true;
			}

			xmlDOT.VehicleDetails = vehicleDetails;
		}

		#endregion

		#region Export FCCs Details

		public Xsd.USFCCCollection ExportFCCsDetails(FCCCollection fCCs)
		{
			Xsd.USFCCCollection xmlFCCs = new Xsd.USFCCCollection();

			foreach (FCC fcc in fCCs)
			{
				Xsd.USFCC xmlFCC = xmlFCCs.AddNew();
				ExportFCCDetails(xmlFCC, fcc);
			}

			return xmlFCCs;
		}

		void ExportFCCDetails(Xsd.USFCC xmlFCC, FCC fcc)
		{
			xmlFCC.CommercialDesc = fcc.US_FCCCommercialDesc;
			xmlFCC.ID = fcc.US_FCCID;
			xmlFCC.ImportConditionNo = fcc.US_FCCImpCondNo;
			xmlFCC.ImportConditionQtyApproved = fcc.US_FCCImpCondNoQtyAppr;
			xmlFCC.ImportConditionQtyApprovedSpecified = true;
			xmlFCC.Model = fcc.US_FCCModel;
			xmlFCC.Qty = fcc.US_FCCQty;
			xmlFCC.QtySpecified = true;
			xmlFCC.TradeName = fcc.US_FCCTradeName;
			xmlFCC.Withhold = fcc.US_FCCWithhold;
			xmlFCC.WithholdSpecified = true;
		}

		#endregion

		#region Export FDAs Details

		public Xsd.USFDACollection ExportFDAsDetails(FDACollection fDAs, IValueObjectExportContext context)
		{
			Xsd.USFDACollection xmlFDAs = new Xsd.USFDACollection();

			foreach (FDA fda in fDAs)
			{
				Xsd.USFDA xmlFDA = xmlFDAs.AddNew();
				ExportFDADetails(xmlFDA, fda, context);
			}

			return xmlFDAs;
		}

		void ExportFDADetails(Xsd.USFDA xmlFDA, FDA fda, IValueObjectExportContext context)
		{
			#region Affirmation Codes

			Xsd.USAffirmationCodeCollection xmlAffirmationCodes = new Xsd.USAffirmationCodeCollection();

			foreach (AffirmationCode afCode in fda.AffirmationCodes)
			{
				Xsd.USAffirmationCode xmlAfCode = xmlAffirmationCodes.AddNew();

				xmlAfCode.Code = afCode.CY_Code;
				xmlAfCode.Value = afCode.CY_Data;
			}

			xmlFDA.AffirmationCodes = xmlAffirmationCodes;

			#endregion

			xmlFDA.BrandName = fda.US_TradeBrandName;
			xmlFDA.CargoStorageCode = fda.US_FDACargoStorageCode;
			xmlFDA.CommercialDesc = fda.US_FDACommercialDesc;
			xmlFDA.CountryOfProduction = fda.US_UC_NKFDAProduction;

			#region Innermost Container

			if (!fda.US_DimUQ.IsEmpty)
			{
				xmlFDA.InnermostContainer.DimensionUQ = ContainerDimensionUQToXmlCodeMappings.Instance.GetExternalCode(fda.US_DimUQ, "Container Dimension UQ", context);
			}
			xmlFDA.InnermostContainer.Dimension1 = fda.US_ContainerDim1;
			xmlFDA.InnermostContainer.Dimension2 = fda.US_ContainerDim2;
			xmlFDA.InnermostContainer.Dimension3 = fda.US_ContainerDim3;
			xmlFDA.InnermostContainer.Shape = fda.US_FDAContainerDimType;

			#endregion

			#region Organization

			ExportOrganisation(xmlFDA.Manufacturer, fda.ManufacturerAddress, context);
			ExportOrganisation(xmlFDA.EstablismentID, fda.FDAFEIAddress, context);
			ExportOrganisation(xmlFDA.Shipper, fda.ShipperAddress, context);

			#endregion

			ExportPriorNoticeDetails(xmlFDA, fda, context);

			#region Quantities

			xmlFDA.Quantities.Quantity1.Value = fda.US_FDAQty1;
			xmlFDA.Quantities.Quantity1.DimensionType = fda.US_FDAMeasure1;
			xmlFDA.Quantities.Quantity2.Value = fda.US_FDAQty2;
			xmlFDA.Quantities.Quantity2.DimensionType = fda.US_FDAMeasure2;
			xmlFDA.Quantities.Quantity3.Value = fda.US_FDAQty3;
			xmlFDA.Quantities.Quantity3.DimensionType = fda.US_FDAMeasure3;
			xmlFDA.Quantities.Quantity4.Value = fda.US_FDAQty4;
			xmlFDA.Quantities.Quantity4.DimensionType = fda.US_FDAMeasure4;
			xmlFDA.Quantities.Quantity5.Value = fda.US_FDAQty5;
			xmlFDA.Quantities.Quantity5.DimensionType = fda.US_FDAMeasure5;
			xmlFDA.Quantities.Quantity6.Value = fda.US_FDAQty6;
			xmlFDA.Quantities.Quantity6.DimensionType = fda.US_FDAMeasure6;

			#endregion

			xmlFDA.ProductCode = fda.US_FDAProductCode;
			xmlFDA.InvValue = fda.US_InvCurrFDAValue;
			xmlFDA.InvValueSpecified = true;
		}

		void ExportPriorNoticeDetails(Xsd.USFDA xmlFDA, FDA fda, IValueObjectExportContext context)
		{
			if (!fda.US_OFT.IsEmpty)
			{
				xmlFDA.OwnerFirmType = FDAOwnerFirmTypeToXmlCodeMappings.Instance.GetExternalCode(fda.US_OFT, "", context);
				xmlFDA.OwnerFirmTypeSpecified = true;
			}

			xmlFDA.CountryOfShipping = fda.US_CSH;
			xmlFDA.ShipperRegistrationNumber = fda.US_SFR;
			xmlFDA.ManufacturerProducerData.FoodFacilityRegistrationNumber = fda.US_PFR;

			if (!fda.US_FME.IsEmpty)
			{
				xmlFDA.ManufacturerProducerData.FoodFacilityRegistrationExemption = FDAFoodFacilityRegToXmlCodeMappings.Instance.GetExternalCode(fda.US_FME, "", context);
				xmlFDA.ManufacturerProducerData.FoodFacilityRegistrationExemptionSpecified = true;
			}
			if (!fda.US_PFT.IsEmpty)
			{
				xmlFDA.ManufacturerProducerData.ProducerFirmType = FDAProducerFirmTypeToXmlCodeMappings.Instance.GetExternalCode(fda.US_PFT, "", context);
				xmlFDA.ManufacturerProducerData.ProducerFirmTypeSpecified = true;
			}

			foreach (IMasterHouse bill in fda.BillsForFDALine)
			{
				Xsd.USFDABill xmlBill = new Xsd.USFDABill();
				xmlBill.HouseBill = bill.HouseBill;
				xmlBill.MasterBill = bill.MasterBill;
				if (!xmlBill.HouseBill.IsEmpty || !xmlBill.MasterBill.IsEmpty)
				{
					xmlFDA.Bills.Add(xmlBill);
				}
			}

			foreach (IUSContainer container in fda.ContainersForFDALine)
			{
				if (!container.ContainerEquipmentID.IsEmpty)
				{
					Xsd.USOGAContainer xmlcontainer = new Xsd.USOGAContainer();
					xmlcontainer.ContainerNumber = container.ContainerEquipmentID;
					xmlFDA.Containers.Add(xmlcontainer);
				}
			}
		}

		#endregion

		#endregion

		#region Export Lacey Act Data

		public Xsd.USLaceyActCollection ExportLaceyActData(bool shouldExportValueAndContainers, PGACollection pGAs)
		{
			Xsd.USLaceyActCollection xmlLaceyActDetails = new Xsd.USLaceyActCollection();

			foreach (PGA pGA in pGAs)
			{
				Xsd.USLaceyAct lacey = new Xsd.USLaceyAct();
				lacey.CommercialDescription = pGA.US_PGACommercialDescription;

				if (shouldExportValueAndContainers)
				{
					if (!pGA.US_PGALineValue.IsEmpty)
					{
						lacey.Value = pGA.US_PGALineValue.ToZInt();
						lacey.ValueSpecified = true;
					}

					foreach (IContainerNumber container in pGA.ContainersForPGALine)
					{
						Xsd.USOGAContainer xmlcontainer = new Xsd.USOGAContainer();
						xmlcontainer.ContainerNumber = container.ContainerEquipmentID;

						lacey.Containers.Add(xmlcontainer);
					}
				}

				foreach (ConstituentElement element in pGA.PG04ConstituentElements)
				{
					Xsd.USConstituentElementDetail xmlElement = new Xsd.USConstituentElementDetail();

					xmlElement.Name = element.US_PGANameOfTheConstituentElement;
					if (!element.US_PGAPercentOfConstituentElement.IsEmpty)
					{
						xmlElement.Percent = element.US_PGAPercentOfConstituentElement;
						xmlElement.PercentSpecified = true;
					}

					if (!element.US_PGAQuantityOfConstituentElement.IsEmpty)
					{
						xmlElement.Quantity = element.US_PGAQuantityOfConstituentElement;
						xmlElement.QuantitySpecified = true;
					}
					xmlElement.UOM = element.US_PGAUnitOfMeasure;

					foreach (ScientificData scientificData in element.ScientificDataCollection)
					{
						Xsd.USScientificData xmlScientificData = new Xsd.USScientificData();
						xmlScientificData.Country = scientificData.US_PGACountryCode;
						xmlScientificData.GenusName = scientificData.US_PGAScientificGenusName;
						xmlScientificData.SpeciesName = scientificData.US_PGAScientificSpeciesName;

						xmlElement.ScientificData.Add(xmlScientificData);
					}

					lacey.ConstituentElements.Add(xmlElement);
				}

				xmlLaceyActDetails.Add(lacey);
			}

			return xmlLaceyActDetails;
		}

		#endregion

		#region Export Organisation

		public void ExportOrganisation(Xsd.IOrganisationOrIDSupporterValueObject valueObjec, OrgAddress address, IValueObjectExportContext context)
		{
			if (valueObjec != null && address != null)
			{
				var org = address.Header;
				if (org != null)
				{
					var orgValueObject = OrganisationDataAdapter.ExportToValueObject(org, context);
					AdaptersHelper.SetAddressSequence(orgValueObject.OrganisationDetails.Addresses, address.OA_Address1, address.OA_Address2);
					valueObjec.Item = orgValueObject;
					valueObjec.IsSpecified = true;
				}
			}
		}

		#endregion

		#region Related Objects

		OrganisationValueObjectDataAdapter OrganisationDataAdapter
		{
			get { return organisationDataAdapter ?? (organisationDataAdapter = new OrganisationValueObjectDataAdapter()); }
		}
		OrganisationValueObjectDataAdapter organisationDataAdapter;

		AdaptersHelper AdaptersHelper
		{
			get { return adaptersHelper ?? (adaptersHelper = new AdaptersHelper()); }
		}
		AdaptersHelper adaptersHelper;

		#endregion
	}
}
