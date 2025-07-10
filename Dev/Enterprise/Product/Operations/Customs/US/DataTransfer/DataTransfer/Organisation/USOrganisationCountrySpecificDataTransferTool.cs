using Enterprise.Customs.US.Business;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Customs.US.DataTransfer
{
	public class USOrganisationCountrySpecificDataTransferTool : OrganisationCountrySpecificDataTransferTool, Integration.Customs.US.IUSOrganisationCountrySpecificDataTransferTool
	{
		public USOrganisationCountrySpecificDataTransferTool()
		{
		}

		public override void ImportData(OrgHeader bizObj, Xsd.OrganisationDetail xsdOrgDetails, IValueObjectImportContext context)
		{
			base.ImportData(bizObj, xsdOrgDetails, context);

			OrgHeaderWrapper wrapper = OrgHeaderWrapper.New(bizObj);
			Xsd.USOrganisationSpecificDetails usOrgDetails = xsdOrgDetails.CountrySpecificDetails.USOrganisationSpecificDetails;
			if (usOrgDetails.FDA.ProducerFirmTypeSpecified)
			{
				context.SetPropertyInfoValue(wrapper.ZO_ProducerFirmTypeInfo, FDAProducerFirmTypeToXmlCodeMappings.Instance.GetEnterpriseCode(usOrgDetails.FDA.ProducerFirmType.ToString(), "", context), true, "FDA Producer Firm Type");
			}
			if (usOrgDetails.FDA.FoodFacilityRegistrationExemptionSpecified)
			{
				context.SetPropertyInfoValue(wrapper.ZO_MFRRegExemptInfo, FDAFoodFacilityRegToXmlCodeMappings.Instance.GetEnterpriseCode(usOrgDetails.FDA.FoodFacilityRegistrationExemption.ToString(), "", context), true, "FDA Food Facility Registration Exemption");
			}
			if (usOrgDetails.FDA.SubmitterFirmTypeSpecified)
			{
				context.SetPropertyInfoValue(wrapper.ZO_SubmitterFirmTypeInfo, SubmitterFirmTypeToXmlCodeMappings.Instance.GetEnterpriseCode(usOrgDetails.FDA.SubmitterFirmType.ToString(), "", context), true, "Submitter Firm Type");
			}

			wrapper.ZO_ENSPrintCustomAttrib1 = (usOrgDetails.EntryDocumentPrinting.CustomAttribute1 == Xsd.TrueFalse.@true);
			wrapper.ZO_ENSPrintCustomAttrib2 = (usOrgDetails.EntryDocumentPrinting.CustomAttribute2 == Xsd.TrueFalse.@true);
			wrapper.ZO_ENSPrintCustomAttrib3 = (usOrgDetails.EntryDocumentPrinting.CustomAttribute3 == Xsd.TrueFalse.@true);
			wrapper.ZO_ENSPrintProduct = (usOrgDetails.EntryDocumentPrinting.ProductCode == Xsd.TrueFalse.@true);
			wrapper.ZO_DoNotAutoGenerateSDCR = (usOrgDetails.ImporterOfRecordDetails.DoNotAutoGenerateStmDayChangeRequest == Xsd.TrueFalse.@true);
			wrapper.ZO_AccountNo = usOrgDetails.ImporterOfRecordDetails.PayersUnitNo.Left(wrapper.ZO_AccountNoInfo.MaxLength);
			if (usOrgDetails.ImporterOfRecordDetails.PaymentTypeSpecified)
			{
				context.SetPropertyInfoValue(wrapper.ZO_PaymentTypeInfo, PaymentTypeToXmlCodeMappings.Instance.GetEnterpriseCode(usOrgDetails.ImporterOfRecordDetails.PaymentType.ToString(), "", context), true, "Payment Type");
			}
			if (usOrgDetails.ImporterOfRecordDetails.PurchasedSpecified)
			{
				wrapper.ZO_Purchased = (usOrgDetails.ImporterOfRecordDetails.Purchased == Xsd.TrueFalse.@true) ? YesNoDefaultList.Codes.Yes : YesNoDefaultList.Codes.No;
			}
			if (usOrgDetails.ImporterOfRecordDetails.StatementPrintDateWorkingDaysSpecified)
			{
				wrapper.ZO_SPDNumberOfDays = usOrgDetails.ImporterOfRecordDetails.StatementPrintDateWorkingDays;
			}
			if (usOrgDetails.ImporterOfRecordDetails.TaxDeferredIndSpecified)
			{
				context.SetPropertyInfoValue(wrapper.ZO_TaxDeferredIndInfo, TaxDefferableToXmlCodeMappings.Instance.GetEnterpriseCode(usOrgDetails.ImporterOfRecordDetails.TaxDeferredInd.ToString(), "", context), true, "Tax Defferable");
			}
			if (usOrgDetails.ImporterOfRecordDetails.NotifyParty.IsSpecified)
			{
				var item = usOrgDetails.ImporterOfRecordDetails.NotifyParty.Item;
				if (item is Xsd.Organisation)
				{
					wrapper.ZO_OH_NPInfo.Value = context.FindOrCreateTempOrganisationPK((Xsd.Organisation)item, null, MasterFiles.Integration.OrganisationTypes.None);
				}
				else if (item is string)
				{
					wrapper.ZO_NPIDInfo.Value = (CargoWise.Types.ZString)item;
				}
			}
			wrapper.ZO_FileTheirOwnRecon = (usOrgDetails.Reconciliation.FileTheirOwnRecon == Xsd.TrueFalse.@true);
			if (usOrgDetails.Reconciliation.IssueSpecified)
			{
				context.SetPropertyInfoValue(wrapper.ZO_OtherReconIndicatorInfo, ReconciliationIssueToXmlCodeMappings.Instance.GetEnterpriseCode(usOrgDetails.Reconciliation.Issue.ToString(), "", context), true, "Reconciliation Issue");
			}
			wrapper.ZO_NAFTAReconIndicator = (usOrgDetails.Reconciliation.NAFTA == Xsd.TrueFalse.@true);

			if (!usOrgDetails.Misc.BIRDDefaultBranch.IsEmpty)
			{
				GlbBranch branch = (GlbBranch)context.Factory.LoadFromUniqueKey(typeof(GlbBranch), GlbBranchSchema.GB_Code, usOrgDetails.Misc.BIRDDefaultBranch);
				if (branch != null)
				{
					wrapper.ZO_GB = branch.PK;
				}
				else
				{
					WarningNotification notification = new WarningNotification(string.Format("Branch Code not found: {0}", usOrgDetails.Misc.BIRDDefaultBranch));
					context.Add(notification);
				}
			}
		}

		public override void ExportData(OrgHeader bizObj, Xsd.OrganisationDetail xsdOrgDetails, IValueObjectExportContext context)
		{
			base.ExportData(bizObj, xsdOrgDetails, context);

			OrgHeaderWrapper wrapper = OrgHeaderWrapper.New(bizObj);
			Xsd.USOrganisationSpecificDetails usOrgDetails = xsdOrgDetails.CountrySpecificDetails.USOrganisationSpecificDetails;

			#region FDA

			if (!wrapper.ZO_ProducerFirmType.IsEmpty)
			{
				usOrgDetails.FDA.ProducerFirmType = FDAProducerFirmTypeToXmlCodeMappings.Instance.GetExternalCode(wrapper.ZO_ProducerFirmType, "", context);
				usOrgDetails.FDA.ProducerFirmTypeSpecified = true;
			}
			if (!wrapper.ZO_MFRRegExempt.IsEmpty)
			{
				usOrgDetails.FDA.FoodFacilityRegistrationExemption = FDAFoodFacilityRegToXmlCodeMappings.Instance.GetExternalCode(wrapper.ZO_MFRRegExempt, "", context);
				usOrgDetails.FDA.FoodFacilityRegistrationExemptionSpecified = true;
			}
			if (!wrapper.ZO_SubmitterFirmType.IsEmpty)
			{
				usOrgDetails.FDA.SubmitterFirmType = SubmitterFirmTypeToXmlCodeMappings.Instance.GetExternalCode(wrapper.ZO_SubmitterFirmType, "", context);
				usOrgDetails.FDA.SubmitterFirmTypeSpecified = true;
			}

			#endregion

			#region EntryDocumentPrinting

			if (wrapper.ZO_ENSPrintCustomAttrib1)
			{
				usOrgDetails.EntryDocumentPrinting.CustomAttribute1 = wrapper.ZO_ENSPrintCustomAttrib1 ? Xsd.TrueFalse.@true : Xsd.TrueFalse.@false;
				usOrgDetails.EntryDocumentPrinting.CustomAttribute1Specified = true;
			}
			if (wrapper.ZO_ENSPrintCustomAttrib2)
			{
				usOrgDetails.EntryDocumentPrinting.CustomAttribute2 = wrapper.ZO_ENSPrintCustomAttrib2 ? Xsd.TrueFalse.@true : Xsd.TrueFalse.@false;
				usOrgDetails.EntryDocumentPrinting.CustomAttribute2Specified = true;
			}
			if (wrapper.ZO_ENSPrintCustomAttrib3)
			{
				usOrgDetails.EntryDocumentPrinting.CustomAttribute3 = wrapper.ZO_ENSPrintCustomAttrib3 ? Xsd.TrueFalse.@true : Xsd.TrueFalse.@false;
				usOrgDetails.EntryDocumentPrinting.CustomAttribute3Specified = true;
			}
			if (wrapper.ZO_ENSPrintProduct)
			{
				usOrgDetails.EntryDocumentPrinting.ProductCode = wrapper.ZO_ENSPrintProduct ? Xsd.TrueFalse.@true : Xsd.TrueFalse.@false;
				usOrgDetails.EntryDocumentPrinting.ProductCodeSpecified = true;
			}

			#endregion

			#region ImporterOfRecordDetails

			if (wrapper.ZO_DoNotAutoGenerateSDCR)
			{
				usOrgDetails.ImporterOfRecordDetails.DoNotAutoGenerateStmDayChangeRequest = wrapper.ZO_DoNotAutoGenerateSDCR ? Xsd.TrueFalse.@true : Xsd.TrueFalse.@false;
				usOrgDetails.ImporterOfRecordDetails.DoNotAutoGenerateStmDayChangeRequestSpecified = true;
			}
			usOrgDetails.ImporterOfRecordDetails.PayersUnitNo = wrapper.ZO_AccountNo;
			if (!wrapper.ZO_PaymentType.IsEmpty)
			{
				usOrgDetails.ImporterOfRecordDetails.PaymentType = PaymentTypeToXmlCodeMappings.Instance.GetExternalCode(wrapper.ZO_PaymentType, "", context);
				usOrgDetails.ImporterOfRecordDetails.PaymentTypeSpecified = true;
			}
			if (!wrapper.ZO_Purchased.IsEmpty)
			{
				usOrgDetails.ImporterOfRecordDetails.Purchased = wrapper.ZO_Purchased == YesNoDefaultList.Codes.Yes ? Xsd.TrueFalse.@true : Xsd.TrueFalse.@false;
				usOrgDetails.ImporterOfRecordDetails.PurchasedSpecified = true;
			}
			if (wrapper.ZO_SPDNumberOfDays > 0)
			{
				usOrgDetails.ImporterOfRecordDetails.StatementPrintDateWorkingDays = wrapper.ZO_SPDNumberOfDays;
				usOrgDetails.ImporterOfRecordDetails.StatementPrintDateWorkingDaysSpecified = true;
			}
			if (!wrapper.ZO_TaxDeferredInd.IsEmpty)
			{
				usOrgDetails.ImporterOfRecordDetails.TaxDeferredInd = TaxDefferableToXmlCodeMappings.Instance.GetExternalCode(wrapper.ZO_TaxDeferredInd, "", context);
				usOrgDetails.ImporterOfRecordDetails.TaxDeferredIndSpecified = true;
			}
			if (wrapper.NotifyParty != null)
			{
				if (wrapper.NotifyParty.PK == bizObj.PK)
				{
					usOrgDetails.ImporterOfRecordDetails.NotifyParty.Item = new Xsd.Organisation() { EDICode = wrapper.NotifyParty.OH_Code };
					usOrgDetails.ImporterOfRecordDetails.NotifyParty.IsSpecified = true;
				}
				else
				{
					usOrgDetails.ImporterOfRecordDetails.NotifyParty.Item = new OrganisationValueObjectDataAdapter().ExportToValueObject(wrapper.NotifyParty, context);
					usOrgDetails.ImporterOfRecordDetails.NotifyParty.IsSpecified = true;
				}
			}
			else if (!wrapper.ZO_NPID.IsEmpty)
			{
				usOrgDetails.ImporterOfRecordDetails.NotifyParty.Item = wrapper.ZO_NPID;
				usOrgDetails.ImporterOfRecordDetails.NotifyParty.IsSpecified = true;
			}

			#endregion

			if (wrapper.BIRDDefaultBranch != null)
			{
				usOrgDetails.Misc.BIRDDefaultBranch = wrapper.BIRDDefaultBranch.GB_Code;
			}

			#region Reconciliation

			if (wrapper.ZO_FileTheirOwnRecon)
			{
				usOrgDetails.Reconciliation.FileTheirOwnRecon = wrapper.ZO_FileTheirOwnRecon ? Xsd.TrueFalse.@true : Xsd.TrueFalse.@false;
				usOrgDetails.Reconciliation.FileTheirOwnReconSpecified = true;
			}
			if (!wrapper.ZO_OtherReconIndicator.IsEmpty)
			{
				usOrgDetails.Reconciliation.Issue = ReconciliationIssueToXmlCodeMappings.Instance.GetExternalCode(wrapper.ZO_OtherReconIndicator, "", context);
				usOrgDetails.Reconciliation.IssueSpecified = true;
			}
			if (wrapper.ZO_NAFTAReconIndicator)
			{
				usOrgDetails.Reconciliation.NAFTA = wrapper.ZO_NAFTAReconIndicator ? Xsd.TrueFalse.@true : Xsd.TrueFalse.@false;
				usOrgDetails.Reconciliation.NAFTASpecified = true;
			}

			#endregion
		}
	}
}
