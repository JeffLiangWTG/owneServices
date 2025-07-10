using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business
{
	public class OrgSupplierPartCollection : Customs.Business.OrgSupplierPartCollection
	{
		public OrgSupplierPartCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public OrgSupplierPartCollection(BusinessObjectFactory factory, JobComInvoiceLine invoiceLine, bool isExport)
			: base(factory, invoiceLine, isExport)
		{
		}

		public OrgSupplierPartCollection(BusinessObjectFactory factory, OrgHeader supplier, OrgHeader owner, bool isExport)
			: base(factory, supplier, owner, isExport)
		{
		}

		public new OrgSupplierPart AddNew()
		{
			return (OrgSupplierPart)base.AddNew();
		}

		public new OrgSupplierPart this[int index]
		{
			get { return (OrgSupplierPart)Elements[index]; }
		}

		protected override void AddPivotWithAdditionalLineDetailsCore(Customs.Business.OrgSupplierPart part1, BaseJobComInvoiceLine invoiceLine1)
		{
			var part = (OrgSupplierPart)part1;
			var invoiceLine = (JobComInvoiceLine)invoiceLine1;
			if (part != null && invoiceLine != null && (invoiceLine.JI_CC.IsValid || !invoiceLine.JI_Tariff.IsEmpty || !invoiceLine.US_SupTariff.IsEmpty))
			{
				bool isExport = invoiceLine.IsExport;
				var cusClassPK = ZGuid.Empty;
				var cusClass = invoiceLine.Classification;
				if (cusClass != null)
				{
					cusClassPK = cusClass.PK;
				}

				CusClassPartPivot pivot = part.PivotsForBinding.AddNew();
				pivot.CI_ChildType = invoiceLine.GetPartPivotType();
				if (cusClassPK.IsValid)
				{
					pivot.CI_CC = cusClassPK;
				}
				else
				{
					pivot.CI_TariffNum = invoiceLine.JI_Tariff.Left(15);
				}

				if (isExport)
				{
					AddExportAdditionalDetails(invoiceLine, pivot);
				}
				else
				{
					pivot.CI_SupplementalTariff = invoiceLine.US_SupTariff.Left(15);
					AddImportAdditionalDetails(invoiceLine, pivot);
				}
			}
		}

		void AddExportAdditionalDetails(JobComInvoiceLine invoiceLine, CusClassPartPivot pivot)
		{
			invoiceLine.AddExportAdditionalDetailsToProducts(pivot);

			CopyExportAMS(invoiceLine, pivot);
			CopyExportEPA(invoiceLine, pivot);
			CopyExportNMFS(invoiceLine, pivot);
			CopyExportATF(invoiceLine, pivot);
			CopyExportFWS(invoiceLine, pivot);
			CopyExportDEA(invoiceLine, pivot);
			CopyExportTTB(invoiceLine, pivot);
		}

		void CopyExportAMS(JobComInvoiceLine invoiceLine, CusClassPartPivot pivot)
		{
			pivot.CD_ExportCertificateNo = invoiceLine.US_ExportCertificateNo;
		}

		void CopyExportEPA(JobComInvoiceLine invoiceLine, CusClassPartPivot pivot)
		{
			pivot.CD_EPAConsentNumber = invoiceLine.US_EPAConsentNumber;
			pivot.CD_HazWasteTrackingNo = invoiceLine.US_HazWasteTrackingNo;
		}

		void CopyExportNMFS(JobComInvoiceLine invoiceLine, CusClassPartPivot pivot)
		{
			foreach (NMFSLine invoiceLineNMFS in invoiceLine.NMFSLines)
			{
				var productNMFS = pivot.NMFSLines.AddNew();
				invoiceLineNMFS.Data.UpdateRelatedPropertyInfo();
				productNMFS.B7_AddInfoData = invoiceLineNMFS.B7_AddInfoData;
				productNMFS.US_Quantity = ZDecimal.Zero;
				productNMFS.US_UnitOfMeasure = ZString.Empty;
				productNMFS.US_DISDocumentID = ZString.Empty;
				productNMFS.Data.UpdateRelatedPropertyInfo();

				foreach (NMFSHarvestingDetail invoiceLineHarvesting in invoiceLineNMFS.HarvestingDetails)
				{
					var productHarvesting = productNMFS.HarvestingDetails.AddNew();
					invoiceLineHarvesting.Data.UpdateRelatedPropertyInfo();
					productHarvesting.B7_AddInfoData = invoiceLineHarvesting.B7_AddInfoData;
					productHarvesting.Data.UpdateRelatedPropertyInfo();
				}
			}
		}

		void CopyExportATF(JobComInvoiceLine invoiceLine, CusClassPartPivot pivot)
		{
			if (invoiceLine.ExportATF != null && pivot.ExportATF != null)
			{
				invoiceLine.ExportATF.UpdateAddInfoProperties();
				pivot.ExportATF.B7_AddInfoData = invoiceLine.ExportATF.B7_AddInfoData;
				pivot.ExportATF.US_Quantity = ZDecimal.Zero;
				pivot.ExportATF.UpdateAddInfoProperties();
			}
		}

		void CopyExportFWS(JobComInvoiceLine invoiceLine, CusClassPartPivot pivot)
		{
			if (invoiceLine.ExportFWS != null && pivot.ExportFWS != null)
			{
				invoiceLine.ExportFWS.UpdateAddInfoProperties();
				pivot.ExportFWS.B7_AddInfoData = invoiceLine.ExportFWS.B7_AddInfoData;
				pivot.ExportFWS.UpdateAddInfoProperties();
			}
		}

		void CopyExportDEA(JobComInvoiceLine invoiceLine, CusClassPartPivot pivot)
		{
			foreach (DEAHeader invoiceLineDEA in invoiceLine.DEAHeaders)
			{
				var productDEA = pivot.DEAHeaders.AddNew();
				invoiceLineDEA.UpdateAddInfoProperties();
				productDEA.B7_AddInfoData = invoiceLineDEA.B7_AddInfoData;
				productDEA.UpdateAddInfoProperties();

				foreach (DEAConstituent invoiceLineConstituent in invoiceLineDEA.Constituents)
				{
					var productConstituent = productDEA.Constituents.AddNew();
					invoiceLineConstituent.UpdateAddInfoProperties();
					productConstituent.B7_AddInfoData = invoiceLineConstituent.B7_AddInfoData;
					productConstituent.US_Weight = ZDecimal.Zero;
					productConstituent.US_WeightUQ = ZString.Empty;
					productConstituent.UpdateAddInfoProperties();
				}
			}
		}

		void CopyExportTTB(JobComInvoiceLine invoiceLine, CusClassPartPivot pivot)
		{
			foreach (TTBLine invoiceLineTTB in invoiceLine.TTBLines)
			{
				var productTTB = pivot.TTBLines.AddNew();
				invoiceLineTTB.UpdateAddInfoProperties();
				productTTB.B7_AddInfoData = invoiceLineTTB.B7_AddInfoData;
				productTTB.UpdateAddInfoProperties();
			}
		}

		void AddImportAdditionalDetails(JobComInvoiceLine invoiceLine, CusClassPartPivot pivot)
		{
			invoiceLine.AddImportAdditionalDetailsToProducts(pivot);

			CopyLacey(invoiceLine, pivot);
			CopyPGAFDA(invoiceLine, pivot);
			CopyNHTSA(invoiceLine, pivot);
			CopyATF(invoiceLine, pivot);
			CopyPST(invoiceLine, pivot);
			CopyVNE(invoiceLine, pivot);
			CopyOMC(invoiceLine, pivot);
			CopyODSTSCA(invoiceLine, pivot);
			CopyTTB(invoiceLine, pivot);
			CopyCPSC(invoiceLine, pivot);
			CopyDEA(invoiceLine, pivot);
			CopyAPHIS(invoiceLine, pivot);
			CopyAMS(invoiceLine, pivot);
			CopyDDTC(invoiceLine, pivot);
			CopyHFC(invoiceLine, pivot);

			CopyNMFS(invoiceLine, pivot);
			CopyFWS(invoiceLine, pivot);

			pivot.CD_RX_NK9802ValuePerUnitCurr = invoiceLine.JI_RX_NKLinePriceCurr;
			pivot.SupFormattedAdditionalTariff1 = invoiceLine.SupFormattedAdditionalTariff1.Left(15);
			pivot.SupFormattedAdditionalTariff2 = invoiceLine.SupFormattedAdditionalTariff2.Left(15);
			pivot.SupFormattedAdditionalTariff3 = invoiceLine.SupFormattedAdditionalTariff3.Left(15);
			pivot.SupFormattedAdditionalTariff4 = invoiceLine.SupFormattedAdditionalTariff4.Left(15);
			pivot.SupFormattedAdditionalTariff5 = invoiceLine.SupFormattedAdditionalTariff5.Left(15);

			if (!invoiceLine.IsChildLine)
			{
				if (!invoiceLine.JI_PartAttrib1.IsEmpty)
				{
					var attrib = pivot.Attributes1.AddNew();
					attrib.BG_AttributeValue1 = invoiceLine.JI_PartAttrib1;
				}
				if (!invoiceLine.JI_PartAttrib2.IsEmpty)
				{
					var attrib = pivot.Attributes2.AddNew();
					attrib.BG_AttributeValue1 = invoiceLine.JI_PartAttrib2;
				}
				if (!invoiceLine.JI_PartAttrib3.IsEmpty)
				{
					var attrib = pivot.Attributes3.AddNew();
					attrib.BG_AttributeValue1 = invoiceLine.JI_PartAttrib3;
				}

				var childLines = new List<JobComInvoiceLine>(invoiceLine.ChildLines);
				childLines.AddRange(invoiceLine.ProductRelatedLines);
				if (childLines.Count > 0)
				{
					childLines.Sort(new InvoiceLineParentChildComparer());

					foreach (JobComInvoiceLine childLine in childLines)
					{
						CusClassPartPivot newChildPivot = pivot.Children.AddNew();
						newChildPivot.CI_ChildType = childLine.US_JI_ParentProduct.IsValid ? ClassificationChildTypeList.Codes.Related : ClassificationChildTypeList.Codes.COMPONENT;
						newChildPivot.CI_TariffNum = childLine.JI_Tariff.Left(15);
						newChildPivot.CI_SupplementalTariff = childLine.US_SupTariff.Left(15);
						AddImportAdditionalDetails(childLine, newChildPivot);
						newChildPivot.CD_WeightUQ = childLine.JI_WeightUQ;
						if (childLine.JI_InvoiceQuantity > 0)
						{
							newChildPivot.CD_GrossWeight = childLine.JI_Weight / childLine.JI_InvoiceQuantity;
							newChildPivot.CD_NetWeight = Core.Constants.Weight.Convert(childLine.JI_NetWeight / childLine.JI_InvoiceQuantity, childLine.JI_NetWeightUQ, newChildPivot.CD_WeightUQ);
						}
					}
				}
			}
		}

		void CopyOMC(JobComInvoiceLine invoiceLine, CusClassPartPivot pivot)
		{
			foreach (OMCHeader invoiceLineOMC in invoiceLine.OMCHeaders)
			{
				OMCHeader productOMC = pivot.OMCHeaders.AddNew();
				invoiceLineOMC.UpdateAddInfoProperties();
				productOMC.B7_AddInfoData = invoiceLineOMC.B7_AddInfoData;
				productOMC.UpdateAddInfoProperties();
				productOMC.US_TrackingStatus = ZString.Empty;
				productOMC.US_LineNo = ZInt.Zero;
				productOMC.US_ElectronicImageSubmitted = ZBool.False;
				productOMC.US_DepartureDate = ZDateTime.Empty;
				foreach (USOMCAquacultureFacility aquacultureFacility in invoiceLineOMC.AquacultureFacilities)
				{
					USOMCAquacultureFacility productAquacultureFacility = productOMC.AquacultureFacilities.AddNew();
					aquacultureFacility.updateAddInfoProperties();
					productAquacultureFacility.B7_AddInfoData = aquacultureFacility.B7_AddInfoData;
					productAquacultureFacility.updateAddInfoProperties();
				}
			}
		}

		void CopyLacey(JobComInvoiceLine invoiceLine, CusClassPartPivot pivot)
		{
			foreach (PGA invoiceLinePGA in invoiceLine.LaceyActLines)
			{
				PGA newPivotPGA = pivot.PGAs.AddNew();
				invoiceLinePGA.UpdateAddInfoProperties();
				newPivotPGA.B7_AddInfoData = invoiceLinePGA.B7_AddInfoData;
				newPivotPGA.US_InvCurrPGAValue = ZDecimal.Zero;
				newPivotPGA.US_PGALineValue = ZDecimal.Zero;
				newPivotPGA.UpdateAddInfoProperties();
				newPivotPGA.US_TrackingStatus = ZString.Empty;

				foreach (ConstituentElement element in invoiceLinePGA.PG04ConstituentElements)
				{
					var newElement = newPivotPGA.PG04ConstituentElements.AddNew();
					element.UpdateAddInfoProperties();
					newElement.B7_AddInfoData = element.B7_AddInfoData;
					foreach (ScientificData data in element.ScientificDataCollection)
					{
						var newData = newElement.ScientificDataCollection.AddNew();
						data.Data.UpdateRelatedPropertyInfo();
						newData.B7_AddInfoData = data.B7_AddInfoData;
					}
				}

				if (!invoiceLine.IsACE)
				{
					newPivotPGA.TransformData(JobApplicationCodeList.Codes.ACE);
				}
			}
		}

		void CopyPGAFDA(JobComInvoiceLine invoiceLine, CusClassPartPivot pivot)
		{
			foreach (ACEFDA invoiceLineFDA in invoiceLine.ACE_FDALines)
			{
				var newPivotFDA = pivot.ACEFDAs.AddNew();
				invoiceLineFDA.UpdateAddInfoProperties();
				newPivotFDA.B7_AddInfoData = invoiceLineFDA.B7_AddInfoData;
				newPivotFDA.UpdateAddInfoProperties();
				newPivotFDA.US_InvCurrValue = ZDecimal.Zero;
				newPivotFDA.US_TotalValue = ZDecimal.Zero;
				newPivotFDA.US_TrackingStatus = ZString.Empty;

				if (newPivotFDA.EnableDocAddressForFDA)
				{
					foreach (ACEFDAJobDocAddress docAddress in invoiceLineFDA.DocAddresses)
					{
						newPivotFDA.DocAddresses.AddNew().CopyPersistentValuesFrom(docAddress
							, new BusinessObjectCloneArgs(new[] { ACEFDAJobDocAddress.Schema.E2_ParentID, ACEFDAJobDocAddress.Schema.E2_ParentTableCode }));
					}
				}

				foreach (Lot lot in invoiceLineFDA.Lots)
				{
					var newLot = newPivotFDA.Lots.AddNew();
					newLot.UpdateAddInfoProperties();
					newLot.B7_AddInfoData = lot.B7_AddInfoData;
				}

				foreach (ACEAffirmationCode affirmationCode in invoiceLineFDA.AffirmationCodes)
				{
					newPivotFDA.AffirmationCodes.AddNew().CopyPersistentValuesFrom(affirmationCode
						, new BusinessObjectCloneArgs(new[] { CusCodeData.Schema.CY_ParentID, CusCodeData.Schema.CY_ParentTableCode }));
				}

				foreach (ConstituentElement constituentElement in invoiceLineFDA.ProductConstituentElements)
				{
					var newData = newPivotFDA.ProductConstituentElements.AddNew();
					constituentElement.UpdateAddInfoProperties();
					newData.B7_AddInfoData = constituentElement.B7_AddInfoData;
				}
			}
		}

		void CopyAMS(JobComInvoiceLine invoiceLine, CusClassPartPivot pivot)
		{
			foreach (AMS invoiceLineAMS in invoiceLine.AMSLines)
			{
				var productAMS = pivot.AMSLines.AddNew();
				invoiceLineAMS.UpdateAddInfoProperties();
				productAMS.B7_AddInfoData = invoiceLineAMS.B7_AddInfoData;
				productAMS.UpdateAddInfoProperties();
				productAMS.US_TrackingStatus = ZString.Empty;

				foreach (AMSLine amsLine in invoiceLineAMS.AMSLines)
				{
					amsLine.UpdateAddInfoProperties();
					var newAMSLine = productAMS.AMSLines.AddNew();
					newAMSLine.Data.UpdateRelatedPropertyInfo();
					newAMSLine.B7_AddInfoData = amsLine.B7_AddInfoData;
					ClearUnnecessaryFields(productAMS.US_Program, newAMSLine);
				}

				foreach (var lotCode in invoiceLineAMS.LotCodes)
				{
					var newLotCode = productAMS.LotCodes.AddNew();
					newLotCode.CY_Code = lotCode.CY_Code;
					newLotCode.CY_Data = lotCode.CY_Data;
				}
			}
		}

		void CopyHFC(JobComInvoiceLine invoiceLine, CusClassPartPivot pivot)
		{
			foreach (USHFCHeader invoiceLineHFC in invoiceLine.USHFCHeaders)
			{
				var productHFC = pivot.HFCHeaders.AddNew();
				invoiceLineHFC.Data.UpdateRelatedPropertyInfo();
				productHFC.B7_AddInfoData = invoiceLineHFC.B7_AddInfoData;
				productHFC.Data.UpdateRelatedPropertyInfo();
				productHFC.US_TrackingStatus = ZString.Empty;
				productHFC.US_HFCImageSent = ZBool.False;
				foreach (USHFCDetail hfcDetail in invoiceLineHFC.USHFCDetails)
				{
					var newHFCDetail = productHFC.USHFCDetails.AddNew();
					hfcDetail.Data.UpdateRelatedPropertyInfo();
					newHFCDetail.B7_AddInfoData = hfcDetail.B7_AddInfoData;
					newHFCDetail.Data.UpdateRelatedPropertyInfo();
				}
			}
		}

		void ClearUnnecessaryFields(ZString programCode, AMSLine newLine)
		{
			IEnumerable<string> fieldsInColumnToClearOut = ColumnsWithAMSTypeHelper.GetElementsToBeHide(programCode);
			ColumnsWithAMSTypeHelper.ClearIrrelevantData(newLine, fieldsInColumnToClearOut);
			newLine.LotCodes.RemoveAndDeleteAll();
		}

		void CopyNMFS(JobComInvoiceLine invoiceLine, CusClassPartPivot pivot)
		{
			foreach (NMFSLine invoiceLineNMFS in invoiceLine.NMFSLines)
			{
				NMFSLine productNMFS = pivot.NMFSLines.AddNew();
				productNMFS.HarvestingDetails.RemoveAndDeleteAll();
				invoiceLineNMFS.UpdateAddInfoProperties();
				productNMFS.B7_AddInfoData = invoiceLineNMFS.B7_AddInfoData;
				productNMFS.UpdateAddInfoProperties();
				productNMFS.US_Quantity = ZDecimal.Zero;
				productNMFS.US_TrackingStatus = ZString.Empty;
				productNMFS.US_DISDocumentID = ZString.Empty;

				foreach (NMFSHarvestingDetail harvest in invoiceLineNMFS.HarvestingDetails)
				{
					NMFSHarvestingDetail newData = productNMFS.HarvestingDetails.AddNew();
					harvest.UpdateAddInfoProperties();
					newData.B7_AddInfoData = harvest.B7_AddInfoData;
					newData.UpdateAddInfoProperties();

					foreach (NMFSVessels harvestVessel in harvest.HarvestingVessles)
					{
						NMFSVessels newVessel = newData.HarvestingVessles.AddNew();
						harvestVessel.UpdateAddInfoProperties();
						newVessel.B7_AddInfoData = harvestVessel.B7_AddInfoData;
						newVessel.UpdateAddInfoProperties();
					}
				}
			}
		}

		void CopyFWS(JobComInvoiceLine invoiceLine, CusClassPartPivot pivot)
		{
			foreach (FWSHeader invoiceLineFWS in invoiceLine.FWSHeaders)
			{
				FWSHeader productFWS = pivot.FWSLines.AddNew();
				invoiceLineFWS.UpdateAddInfoProperties();
				productFWS.B7_AddInfoData = invoiceLineFWS.B7_AddInfoData;
				productFWS.UpdateAddInfoProperties();
				productFWS.US_TrackingStatus = ZString.Empty;

				foreach (FWSLicense license in invoiceLineFWS.Licenses)
				{
					FWSLicense newData = productFWS.Licenses.AddNew();
					license.UpdateAddInfoProperties();
					newData.B7_AddInfoData = license.B7_AddInfoData;
					newData.UpdateAddInfoProperties();
				}
			}
		}

		void CopyTTB(JobComInvoiceLine invoiceLine, CusClassPartPivot pivot)
		{
			foreach (TTBLine invoiceLineTTB in invoiceLine.TTBLines)
			{
				TTBLine productTTB = pivot.TTBLines.AddNew();
				invoiceLineTTB.UpdateAddInfoProperties();
				productTTB.B7_AddInfoData = invoiceLineTTB.B7_AddInfoData;
				productTTB.UpdateAddInfoProperties();
				productTTB.US_QuantityInPCS = ZDecimal.Zero;
				productTTB.US_TrackingStatus = ZString.Empty;

				foreach (TTBCigar cigar in invoiceLineTTB.Cigars)
				{
					TTBCigar newData = productTTB.Cigars.AddNew();
					cigar.UpdateAddInfoProperties();
					newData.B7_AddInfoData = cigar.B7_AddInfoData;
					newData.US_Quantity = ZInt.Zero;
					newData.US_UnitPrice = ZDecimal.Zero;
					newData.UpdateAddInfoProperties();
				}

				foreach (TTBCOLAAndCertificate permit in invoiceLineTTB.COLAAndCertificates)
				{
					TTBCOLAAndCertificate newData = productTTB.COLAAndCertificates.AddNew();
					permit.UpdateAddInfoProperties();
					newData.B7_AddInfoData = permit.B7_AddInfoData;
					newData.UpdateAddInfoProperties();
				}
			}
		}

		void CopyDDTC(JobComInvoiceLine invoiceLine, CusClassPartPivot pivot)
		{
			pivot.Details.CD_ITARExemptionNo = invoiceLine.US_DDTCExemptionCode;
			pivot.Details.CD_DDTCLicenceNo = invoiceLine.US_DDTCLicenseNo;
			pivot.Details.CD_DDTCLicenceType = invoiceLine.US_DDTCLicenseType;
			pivot.Details.CD_DDTCRegoNo = invoiceLine.US_DDTCRegistrationNo;
		}

		void CopyNHTSA(JobComInvoiceLine invoiceLine, CusClassPartPivot pivot)
		{
			foreach (NHTSAHeader invoiceLineNHTSA in invoiceLine.NHTSALines)
			{
				NHTSAHeader productNHTSA = pivot.NHTSALines.AddNew();
				invoiceLineNHTSA.UpdateAddInfoProperties();
				productNHTSA.NHTSADocuments.RemoveAndDeleteAll();
				productNHTSA.B7_AddInfoData = invoiceLineNHTSA.B7_AddInfoData;
				productNHTSA.US_NHTElectronicImage = false;
				productNHTSA.US_NHTEmbassyNationality = ZString.Empty;
				productNHTSA.US_NHTTravelDocNationality = ZString.Empty;
				productNHTSA.US_NHTTravelDocType = ZString.Empty;
				productNHTSA.US_NHTTravelDocNumber = ZString.Empty;
				productNHTSA.US_TrackingStatus = ZString.Empty;

				foreach (NHTSADetails invoiceLineNHTSADetails in invoiceLineNHTSA.NHTSADetails)
				{
					NHTSADetails productNHTSADetails = productNHTSA.NHTSADetails.AddNew();
					invoiceLineNHTSADetails.UpdateAddInfoProperties();
					productNHTSADetails.AdditionalNumbers.RemoveAndDeleteAll();
					productNHTSADetails.B7_AddInfoData = invoiceLineNHTSADetails.B7_AddInfoData;

					foreach (NHTSAAdditionalNum invoiceLineAdditionalNum in invoiceLineNHTSADetails.AdditionalNumbers)
					{
						NHTSAAdditionalNum productAdditionalNum = productNHTSADetails.AdditionalNumbers.AddNew();
						invoiceLineAdditionalNum.UpdateAddInfoProperties();
						productAdditionalNum.B7_AddInfoData = invoiceLineAdditionalNum.B7_AddInfoData;
					}

					foreach (NHTSAPermitAndLicenses invoiceLinePermitAndLicense in invoiceLineNHTSADetails.PermitAndLicenses)
					{
						NHTSAPermitAndLicenses productPermitAndLicense = productNHTSADetails.PermitAndLicenses.AddNew();
						invoiceLinePermitAndLicense.UpdateAddInfoProperties();
						productPermitAndLicense.B7_AddInfoData = invoiceLinePermitAndLicense.B7_AddInfoData;
					}
				}

				foreach (NHTSADocument invoiceLineNHTSADocument in invoiceLineNHTSA.NHTSADocuments)
				{
					NHTSADocument productDocument = productNHTSA.NHTSADocuments.AddNew();
					productDocument.B7_AddInfoData = invoiceLineNHTSADocument.B7_AddInfoData;
				}
			}
		}

		void CopyATF(JobComInvoiceLine invoiceLine, CusClassPartPivot pivot)
		{
			foreach (ATF invoiceLineATF in invoiceLine.ATFLines)
			{
				ATF productATF = pivot.ATFLines.AddNew();
				invoiceLineATF.UpdateAddInfoProperties();
				productATF.B7_AddInfoData = invoiceLineATF.B7_AddInfoData;
				productATF.UpdateAddInfoProperties();
				productATF.US_Quantity = ZDecimal.Zero;
				productATF.US_TrackingStatus = ZString.Empty;
			}
		}

		void CopyPST(JobComInvoiceLine invoiceLine, CusClassPartPivot pivot)
		{
			foreach (Pesticide invoiceLinePST in invoiceLine.PSTLines)
			{
				Pesticide productPST = pivot.PSTLines.AddNew();
				invoiceLinePST.updateAddInfoProperties();
				productPST.B7_AddInfoData = invoiceLinePST.B7_AddInfoData;
				productPST.updateAddInfoProperties();
				productPST.US_LineNo = ZInt.Zero;
				productPST.US_PSTLabelsSent = ZBool.False;
				productPST.US_TrackingStatus = ZString.Empty;
			}
		}

		void CopyCPSC(JobComInvoiceLine invoiceLine, CusClassPartPivot pivot)
		{
			var isDisclaimedAndDisclaimReasonIsA = pivot.CD_CPSCIndicator == OGAIndicatorList.Codes.Disclaimed && pivot.CD_CPSCDisclaimReason == PGADisclaimReasonList.Codes.A;

			foreach (CPSCHeader invoiceLineCPSC in invoiceLine.CPSCHeaders)
			{
				var productCPSC = isDisclaimedAndDisclaimReasonIsA && pivot.HasCPSCLines ? pivot.CPSCLines[0] : pivot.CPSCLines.AddNew();
				invoiceLineCPSC.UpdateAddInfoProperties();
				productCPSC.B7_AddInfoData = invoiceLineCPSC.B7_AddInfoData;
				productCPSC.US_LineNo = ZInt.Zero;
				productCPSC.UpdateAddInfoProperties();
				productCPSC.US_TrackingStatus = ZString.Empty;
				productCPSC.US_LineNo = ZInt.Zero;

				foreach (CPSCRule rule in invoiceLineCPSC.RuleAndLabs)
				{
					var newRule = productCPSC.RuleAndLabs.AddNew();
					rule.UpdateAddInfoProperties();
					newRule.B7_AddInfoData = rule.B7_AddInfoData;
					newRule.UpdateAddInfoProperties();

					foreach (CPSCReport report in rule.ReportAndLabs)
					{
						var newReport = newRule.ReportAndLabs.AddNew();
						report.UpdateAddInfoProperties();
						newReport.B7_AddInfoData = report.B7_AddInfoData;
						newReport.UpdateAddInfoProperties();
					}
				}
			}
		}

		void CopyVNE(JobComInvoiceLine invoiceLine, CusClassPartPivot pivot)
		{
			foreach (Vehicle invoiceLineVNE in invoiceLine.VehicleLines)
			{
				Vehicle productVNE = pivot.VehicleLines.AddNew();
				invoiceLineVNE.updateAddInfoProperties();
				productVNE.B7_AddInfoData = invoiceLineVNE.B7_AddInfoData;
				productVNE.updateAddInfoProperties();
				productVNE.US_LineNo = ZInt.Zero;
				productVNE.US_VNEElectronicImage = ZBool.False;
				productVNE.US_TrackingStatus = ZString.Empty;
			}
		}

		void CopyODSTSCA(JobComInvoiceLine invoiceLine, CusClassPartPivot pivot)
		{
			pivot.CD_TSCAIndicator = invoiceLine.US_TSCACertification;
			pivot.CD_TSCAODSCertIndividual = invoiceLine.US_TSCAODSCertIndividual;
		}

		void CopyDEA(JobComInvoiceLine invoiceLine, CusClassPartPivot pivot)
		{
			foreach (DEAHeader invoiceLineDEA in invoiceLine.DEAHeaders)
			{
				DEAHeader productDEA = pivot.DEAHeaders.AddNew();
				invoiceLineDEA.UpdateAddInfoProperties();
				productDEA.B7_AddInfoData = invoiceLineDEA.B7_AddInfoData;
				productDEA.UpdateAddInfoProperties();
				productDEA.US_LineNo = ZInt.Zero;
				productDEA.US_TrackingStatus = ZString.Empty;
				productDEA.US_PermitNumber = ZString.Empty;

				foreach (DEAConstituent rule in invoiceLineDEA.Constituents)
				{
					var newData = productDEA.Constituents.AddNew();
					rule.UpdateAddInfoProperties();
					newData.B7_AddInfoData = rule.B7_AddInfoData;
					newData.UpdateAddInfoProperties();
				}
			}
		}

		void CopyAPHIS(JobComInvoiceLine invoiceLine, CusClassPartPivot pivot)
		{
			foreach (APHISHeader invoiceLineAPHIS in invoiceLine.APHISHeaders)
			{
				var productAPHIS = pivot.APHISHeaders.AddNew();
				invoiceLineAPHIS.Data.UpdateRelatedPropertyInfo();
				productAPHIS.B7_AddInfoData = invoiceLineAPHIS.B7_AddInfoData;
				productAPHIS.US_LineNo = ZInt.Zero;
				productAPHIS.US_TrackingStatus = ZString.Empty;

				foreach (APHISRouting invoiceLineRouting in invoiceLineAPHIS.Routings)
				{
					var productRouting = productAPHIS.Routings.AddNew();
					invoiceLineRouting.Data.UpdateRelatedPropertyInfo();
					productRouting.B7_AddInfoData = invoiceLineRouting.B7_AddInfoData;
				}

				foreach (APHISSource invoiceLineSource in invoiceLineAPHIS.Sources)
				{
					var productSource = productAPHIS.Sources.AddNew();
					invoiceLineSource.Data.UpdateRelatedPropertyInfo();
					productSource.B7_AddInfoData = invoiceLineSource.B7_AddInfoData;
				}

				foreach (APHISProduct invoiceLineProduct in invoiceLineAPHIS.Products)
				{
					var productProduct = productAPHIS.Products.AddNew();
					invoiceLineProduct.Data.UpdateRelatedPropertyInfo();
					productProduct.B7_AddInfoData = invoiceLineProduct.B7_AddInfoData;

					foreach (APHISIdentity invoiceLineIdentity in invoiceLineProduct.Identities)
					{
						var productIdentity = productProduct.Identities.AddNew();
						productIdentity.CY_Code = invoiceLineIdentity.CY_Code;
						productIdentity.CY_Data = invoiceLineIdentity.CY_Data;

						foreach (APHISIdentityNumberRange invoiceLineNumberRange in invoiceLineIdentity.NumberRanges)
						{
							var productNumberRange = productIdentity.NumberRanges.AddNew();
							invoiceLineNumberRange.Data.UpdateRelatedPropertyInfo();
							productNumberRange.B7_AddInfoData = invoiceLineNumberRange.B7_AddInfoData;
						}
					}
				}

				foreach (APHISInspection invoiceLineInspection in invoiceLineAPHIS.Inspections)
				{
					var productInspection = productAPHIS.Inspections.AddNew();
					invoiceLineInspection.Data.UpdateRelatedPropertyInfo();
					productInspection.B7_AddInfoData = invoiceLineInspection.B7_AddInfoData;
				}

				foreach (APHISLicense invoiceLineLicense in invoiceLineAPHIS.Licenses)
				{
					var productLicense = productAPHIS.Licenses.AddNew();
					invoiceLineLicense.Data.UpdateRelatedPropertyInfo();
					productLicense.B7_AddInfoData = invoiceLineLicense.B7_AddInfoData;
				}
			}
		}
	}
}
