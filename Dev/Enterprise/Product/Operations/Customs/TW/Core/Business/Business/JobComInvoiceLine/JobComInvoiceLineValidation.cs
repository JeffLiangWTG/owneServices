using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Customs.TW.Business.Constants;

namespace Enterprise.Customs.TW.Business
{
	public partial class JobComInvoiceLineValidation : AutoTWJobComInvoiceLineValidation
	{
		public JobComInvoiceLineValidation(JobComInvoiceLine parent)
			: base(parent)
		{
		}

		public new JobComInvoiceLine Parent => (JobComInvoiceLine)base.Parent;

		public new JobDeclaration Declaration => (JobDeclaration)base.Declaration;

		protected override void CheckJI_InnerPackDescription()
		{
			base.CheckJI_InnerPackDescription();
			var invoiceLine = Parent;
			if (invoiceLine.IsLinkedNX101WithCertificate15)
			{
				MandatoryValidation.MessageErrorIfNotEntered(invoiceLine.JI_InnerPackDescriptionInfo);
			}
		}

		IEnumerable<ZString> PackingHouseTariffs
		{
			get
			{
				var factory = Parent.Factory;
				return factory.GetCachedValue("Enterprise.Customs.TW.Business.PackingHouseValidation|PackingHouseTariffs", () =>
				{
					return TWRefCusCodeListTypes.GetPackingHouseTariffs(factory);
				});
			}
		}

		public void ValidateJI_PHValueNumeric() => ValidateCalculatedProperty(Parent.JI_PHValueNumericInfo);
		protected void CheckJI_PHValueNumeric()
		{
			ValidateJI_PHValue();
			Parent.JI_PHValueNumericInfo.AddAllNotificationsFrom(Parent.JI_PHValueInfo);
		}

		protected sealed override void CheckJI_PHValue()
		{
			CheckJI_PHValueCore();
			ValidateJI_PHValueNumeric();
		}
		protected virtual void CheckJI_PHValueCore() { }

		public void ValidateJI_SterilizationValueNumeric() => ValidateCalculatedProperty(Parent.JI_SterilizationValueNumericInfo);
		protected void CheckJI_SterilizationValueNumeric()
		{
			ValidateJI_SterilizationValueNumeric();
			Parent.JI_SterilizationValueNumericInfo.AddAllNotificationsFrom(Parent.JI_SterilizationValueNumericInfo);
		}

		protected sealed override void CheckJI_SterilizationValue()
		{
			CheckJI_SterilizationValueCore();
			ValidateJI_SterilizationValueNumeric();
		}
		protected virtual void CheckJI_SterilizationValueCore() { }

		public void ValidateJI_DeclarationGoodsDescription()
		{
			ValidateCalculatedProperty(Parent.JI_DeclarationGoodsDescriptionInfo);
		}

		protected virtual void CheckJI_DeclarationGoodsDescription()
		{
			var invoiceLine = Parent;
			var declarationGoodsDescriptionInfo = invoiceLine.JI_DeclarationGoodsDescriptionInfo;
			var declarationGoodsDescription = invoiceLine.JI_DeclarationGoodsDescription;
			if (declarationGoodsDescription.IsEmpty)
			{
				if (invoiceLine.OverrideDeclarationGoodsDescription)
				{
					MandatoryValidation.CheckEntered(declarationGoodsDescriptionInfo);
				}
				else
				{
					MandatoryValidation.MessageErrorIfNotEntered(declarationGoodsDescriptionInfo);
				}
			}
			ValidationHelper.CheckGroupingPlusDeclarationGoodsDescriptionLength(invoiceLine, declarationGoodsDescriptionInfo);

			if (ValidationHelper.IsDifferentInvoiceLineValueLinkedToSameControllingMsgHeader(invoiceLine, ControllingMessageTypeList.Codes.NX301_DN, i => i.JI_DeclarationGoodsDescription))
			{
				declarationGoodsDescriptionInfo.AddWarning(Res.GetString("B189195B-2868-47C9-9A79-813E8D48CD9F", "Invoice lines of different Declaration Goods Description cannot be linked to the same NX301_DN message."));
			}
		}

		public void ValidateNX101ShippingMarks()
		{
			ValidateCalculatedProperty(Parent.NX101ShippingMarksInfo);
		}

		protected void CheckNX101ShippingMarks()
		{
			var invoiceLine = Parent;
			var isCertificateType15 = invoiceLine.InvoiceLineLinkControllingMsgHeaders.Cast<InvoiceLineLinkControllingMsgHeader>().Any(x => x.IsLinkedCMHeader && x.CertificateType == CertificateTypeList.Codes.Code15);
			if (isCertificateType15)
			{
				MandatoryValidation.MessageErrorIfNotEntered(invoiceLine.NX101ShippingMarksInfo);
			}
		}

		public void ValidateNX101PermitGoodsDescription()
		{
			ValidateCalculatedProperty(Parent.NX101PermitGoodsDescriptionInfo);
		}

		protected void CheckNX101PermitGoodsDescription()
		{
			MandatoryValidation.MessageErrorIfNotEntered(Parent.NX101PermitGoodsDescriptionInfo);
		}

		public virtual void ValidatePreviousPermitNo() { }

		protected virtual void CheckPreviousPermitNo() { }

		protected override void CheckJI_NetWeightUQ()
		{
			base.CheckJI_NetWeightUQ();
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.JI_NetWeightUQInfo);
		}

		protected override void CheckJI_InvoiceUQ()
		{
			base.CheckJI_InvoiceUQ();
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.JI_InvoiceUQInfo, Parent.Lookups.InvoiceUQList);
		}

		protected override void CheckJI_PrimaryPreference()
		{
			base.CheckJI_PrimaryPreference();
			if (Parent.IsImport)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.JI_PrimaryPreferenceInfo);
			}
		}

		protected override void CheckJI_InvoiceQuantity()
		{
			base.CheckJI_InvoiceQuantity();

			var invoiceQuantity = Parent.JI_InvoiceQuantity;
			var chassisCount = Parent.ChassisJobComInvLineRefsCollection.Cast<ChassisJobComInvLineRefs>().Count(chassis => !chassis.JG_ReferenceNumber.IsEmpty);
			var isCarDataRelatedEmpty = Parent.IsCarRelatedDataEmpty;
			var targetInfo = Parent.JI_InvoiceQuantityInfo;
			if (!isCarDataRelatedEmpty && chassisCount != invoiceQuantity)
			{
				targetInfo.AddMessageError(Res.GetString("D8B96FFC-4333-40F7-89DA-C644160075D9", "The number of chassis number must be equal to invoice quantity."));
			}

			var totalQuantityForLinkToPackage = Parent.TotalQuantityForPackagesPivot;
			if (totalQuantityForLinkToPackage != invoiceQuantity && Parent.PackagesPivot.Any())
			{
				targetInfo.AddMessageError(ValidationConstants.InvoiceLine.TotalQuantityForPackagesPivotNotEqualToInvoiceQuantity(totalQuantityForLinkToPackage, invoiceQuantity));
			}

			CompareValidation.MessageErrorIfLessThanOrEqualToZero(targetInfo);
		}

		protected override void CheckJI_PreviousEntryNumber()
		{
			base.CheckJI_PreviousEntryNumber();
			var previousEntryNumber = Parent.JI_PreviousEntryNumber;
			var targetInfo = Parent.JI_PreviousEntryNumberInfo;
			if (!Parent.JI_PreviousEntryLineNumber.IsEmpty)
			{
				MandatoryValidation.MessageErrorIfNotEntered(targetInfo);
			}
			if (!previousEntryNumber.IsEmpty && previousEntryNumber.Length != 14)
			{
				targetInfo.AddMessageError(Res.GetString("17947028-3E33-40DA-A403-87491B20D14E", "{0} should be 14 characters long", targetInfo.HumanReadableName));
			}
		}

		protected override void CheckJI_PreviousEntryLineNumber()
		{
			base.CheckJI_PreviousEntryLineNumber();
			if (!Parent.JI_PreviousEntryNumber.IsEmpty)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.JI_PreviousEntryLineNumberInfo);
			}
		}

		protected override void CheckJI_CEI()
		{
			var targetInfo = Parent.JI_CEIInfo;
			if (Parent.Declaration?.IsPersistent ?? false)
			{
				MandatoryValidation.MessageErrorIfNotEntered(targetInfo);
			}
		}

		protected override void CheckJI_NetWeight()
		{
			base.CheckJI_NetWeight();
			CompareValidation.MessageErrorIfLessThanOrEqualToZero(Parent.JI_NetWeightInfo);
		}

		protected override void CheckJI_LinePrice()
		{
			base.CheckJI_LinePrice();
			var invoiceLine = Parent;
			var linePrice = invoiceLine.JI_LinePrice;
			var targetInfo = invoiceLine.JI_LinePriceInfo;
			if (linePrice <= 0m)
			{
				targetInfo.AddWarning(Res.GetString("c0f10c0c-d87c-4fb1-b083-ec003dcc08ef", "Line Price must be greater than or equal to zero."));
			}

			var invoiceQuantity = invoiceLine.JI_InvoiceQuantity;
			var unitPrice = invoiceLine.JI_EnteredUnitPrice;
			var invoice = invoiceLine.InvoiceHeader;
			var unitPriceDecimalPlaces = unitPrice.DecimalPlaces > 2 ? unitPrice.DecimalPlaces : 2;
			var linePriceDecimalPlaces = invoice?.Invoice_Currency?.Decimals ?? 2;
			var calculatedLinePrice = invoiceQuantity * unitPrice;

			if (Utilities.Round(calculatedLinePrice, 2) != linePrice
				&& Utilities.Round(calculatedLinePrice, linePriceDecimalPlaces) != linePrice
				&& (invoiceQuantity <= 0 || Utilities.Round(linePrice / invoiceQuantity, unitPriceDecimalPlaces) != unitPrice))
			{
				targetInfo.AddWarning(Res.GetString("817d9a76-2000-4dcc-877f-19f108f77cde", "Unit Price * Quantity must equal to Line Price."));
			}
		}

		protected override void CheckJI_Tariff()
		{
			Parent.ClearRowNotifications();
			base.CheckJI_Tariff();
			if (!Parent.JI_Tariff.IsEmpty)
			{
				CheckTariffIfInvalid();
				CheckJI_TariffHasImportRegulationOrExportRegulation();
				CheckJI_TariffHasPackingHouseIfRequired();
			}
		}

		void CheckTariffIfInvalid()
		{
			if (Parent.UniversalTariff == null)
			{
				Parent.JI_TariffInfo.AddMessageError(ValidationConstants.InvoiceLine.TariffDoesnotExist);
			}
		}

		void CheckJI_TariffHasImportRegulationOrExportRegulation()
		{
			var parent = Parent;
			var importOrExportRegulationCodes = GetImportExportRegulationCodesCore(parent);
			if (importOrExportRegulationCodes.Any())
			{
				if (importOrExportRegulationCodes.Count != (parent.PermitCusSupportingCollection.Count + parent.ExemptionOfControllingAgenciesCusSupportings.Count))
				{
					parent.AddRowWarning(GetTariffHasImportRegulationOrExportRegulationMessage(parent, importOrExportRegulationCodes));
				}
			}
			else if (parent.PermitCusSupportingCollection.Any() || parent.ExemptionOfControllingAgenciesCusSupportings.Any())
			{
				parent.AddRowWarning(GetTariffHasNoRegulationMessage(parent));
			}
		}

		protected virtual List<ZString> GetImportExportRegulationCodesCore(JobComInvoiceLine invoiceLine)
		{
			return invoiceLine.GetImportExportRegulationCodes().ToList();
		}

		string GetTariffHasImportRegulationOrExportRegulationMessage(JobComInvoiceLine line, IEnumerable<ZString> regulationCodes)
		{
			var codes = string.Join(", ", regulationCodes.ToArray());
			return line.IsImport ? Res.GetString("e1d5f922-5caf-4fd6-a912-0651648f79c6", "The selected Tariff has Import Regulation {0}. Permit Number might be required. 'NIL' will be declared if no permit number is entered.", codes)
				: Res.GetString("704a24a2-a0cb-430e-beea-8e6fda41da0e", "The selected Tariff has Export Regulation {0}. Permit Number might be required.", codes);
		}

		string GetTariffHasNoRegulationMessage(JobComInvoiceLine line) => line.IsImport ? Res.GetString("56228dae-8eb2-4f1c-8583-3fd7893a70a9", "The selected Tariff doesn't have Import Regulation. Permit Number might not be required.")
			: Res.GetString("4e8c346a-2d49-42a6-b706-ca54ec35c083", "The selected Tariff doesn't have Export Regulation. Permit Number might not be required.");

		void CheckJI_TariffHasPackingHouseIfRequired()
		{
			var parent = Parent;
			if (parent.IsForCMHeaderMessageTypeNX401)
			{
				var tariff = parent.JI_Tariff;
				if (!parent.PackingHouseCollection.Cast<PackingHouse>().Any(x => x.CY_Code == tariff) && PackingHouseTariffs.Contains(tariff))
				{
					parent.JI_TariffInfo.AddMessageError(Res.GetString("28846A2B-70BD-4113-92ED-CFA232D14CD0", "Please enter a Packing House for Tariff {0}.", tariff));
				}
			}
		}

		protected override void CheckJI_CustomsUnitQty()
		{
		}

		protected override void CheckJI_CustomsSecondUnitQty()
		{
			base.CheckJI_CustomsSecondUnitQty();
			ListValidation.MessageErrorIfInvalidCode(Parent.JI_CustomsSecondUnitQtyInfo);
		}

		protected override void CheckJI_ExtraInfoForClassification()
		{
			base.CheckJI_ExtraInfoForClassification();
			var extraInfo = Parent.JI_ExtraInfoForClassification;
			if (!extraInfo.IsEmpty)
			{
				var entryInstruction = Parent.EntryInstruction;
				if (entryInstruction != null && entryInstruction.CEI_ExamMode != ExamModeList.Codes.WrittenReview)
				{
					Parent.JI_ExtraInfoForClassificationInfo.AddMessageError(ValidationConstants.InvoiceLine.ExtraInfoErrorMessage);
				}
			}
		}

		protected override void CheckJI_CustomsQuantity()
		{
		}

		protected override void CheckJI_Procedure()
		{
			base.CheckJI_Procedure();
			var parent = Parent;
			var entryInstruction = parent.EntryInstruction;
			if (entryInstruction != null)
			{
				if (entryInstruction.Warehouse2 != null)
				{
					var d5List = new HashSet<ZString>() { "97", "98", "9U", "1A", "8A" };
					CheckDutyTreatmentTypes(Constants.DeclarationTypes.Export.D5, d5List, ValidationConstants.InvoiceLine.IncorrectDutyTreatmentTypesForD5);

					var b8List = new HashSet<ZString> { "81", "8A", "82", "92", "9M" };
					CheckDutyTreatmentTypes(Constants.DeclarationTypes.Export.B8, b8List, ValidationConstants.InvoiceLine.IncorrectDutyTreatmentTypesForB8);

					var f5List = new HashSet<ZString> { "01", "1A", "02", "03", "04", "05", "06", "08", "81", "82", "8A", "8B", "8C", "90", "91", "92", "94", "95", "9A", "9B", "9C", "9D", "9E", "9F", "9G", "9H", "9K", "9M", "9N", "9P", "9L", "9S" };
					CheckDutyTreatmentTypes(Constants.DeclarationTypes.Export.F5, f5List, ValidationConstants.InvoiceLine.IncorrectDutyTreatmentTypes);
				}

				if (entryInstruction.CEI_OA_Warehouse.IsEmpty)
				{
					var d8List = new HashSet<ZString> { Constants.ProcedureCodes._98, Constants.ProcedureCodes._92 };
					CheckDutyTreatmentTypes(Constants.DeclarationTypes.Import.D8, d8List, ValidationConstants.InvoiceLine.IncorrectDutyTreatmentTypesForD8);
				}

				var l1List = new HashSet<ZString> { Constants.ProcedureCodes._99 };
				CheckDutyTreatmentTypes(Constants.DeclarationTypes.Import.L1, l1List, ValidationConstants.InvoiceLine.IncorrectDutyTreatmentTypesForL1);

				var d1List = new HashSet<ZString> { ProcedureCodes._97, ProcedureCodes._98, ProcedureCodes._9T };
				CheckDutyTreatmentTypes(Constants.DeclarationTypes.Export.D1, d1List, ValidationConstants.InvoiceLine.IncorrectDutyTreatmentTypesForD1);

				var b1List = new HashSet<ZString> { ProcedureCodes._97, ProcedureCodes._9T, ProcedureCodes._9U };
				CheckDutyTreatmentTypes(Constants.DeclarationTypes.Export.B1, b1List, ValidationConstants.InvoiceLine.IncorrectDutyTreatmentTypesForB1);

				if (parent.JI_LineNo == 1 && parent.InvoiceHeader?.JZ_InvoiceDisplaySequence == 1)
				{
					var b6List = new HashSet<ZString> { ProcedureCodes._56, ProcedureCodes._5Y, ProcedureCodes._58, ProcedureCodes._5C, ProcedureCodes._99 };
					CheckDutyTreatmentTypes(Constants.DeclarationTypes.Import.B6, b6List, ValidationConstants.InvoiceLine.IncorrectDutyTreatmentTypesForB6);
				}

				if (parent.JI_Procedure != Constants.ProcedureCodes.EF && entryInstruction.CEI_Style == Constants.DeclarationTypes.Import.F2
					&& (Declaration?.FilteredInvoiceLines.Cast<JobComInvoiceLine>().Any(x => x.JI_Procedure == Constants.ProcedureCodes.EF) ?? false))
				{
					parent.JI_ProcedureInfo.AddMessageError(ValidationConstants.InvoiceLine.IncorrectDutyTreatmentTypesForF2);
				}
			}
		}

		void CheckDutyTreatmentTypes(string declarationType, HashSet<ZString> checkCodeList, string message)
		{
			var parent = Parent;
			if (parent.EntryInstruction.CEI_Style == declarationType && !checkCodeList.Contains(parent.JI_Procedure))
			{
				parent.JI_ProcedureInfo.AddMessageError(message);
			}
		}

		protected override void CheckJI_ConcessionOrder()
		{
			base.CheckJI_ConcessionOrder();
			ListValidation.MessageErrorIfInvalidCode(Parent.JI_ConcessionOrderInfo, Parent.Lookups.OrderNumbersList);
		}

		protected override void CheckJI_CustomsThirdQuantity()
		{
			base.CheckJI_CustomsThirdQuantity();

			var targetInfo = Parent.JI_CustomsThirdQuantityInfo;
			if (!targetInfo.ReadOnly)
			{
				MandatoryValidation.MessageErrorIfIsZero(targetInfo);
			}
		}

		protected override void CheckJI_CustomsThirdUnitQty()
		{
			base.CheckJI_CustomsThirdUnitQty();

			var targetInfo = Parent.JI_CustomsThirdUnitQtyInfo;
			if (!targetInfo.ReadOnly)
			{
				MandatoryValidation.MessageErrorIfNotEntered(targetInfo);
			}
		}

		protected override void CheckJI_BrandName()
		{
			base.CheckJI_BrandName();
			var parent = Parent;
			var targetInfo = parent.JI_BrandNameInfo;
			var brandName = parent.JI_BrandName;
			if (brandName.IsEmpty)
			{
				if (parent.HasRegulationsCode581Or541)
				{
					var description = MandatoryValidation.GetErrorFieldFromProperyInfo(targetInfo);
					targetInfo.AddMessageError(MandatoryValidation.YouHaveNotEnteredMessage(description));
				}

				if (parent.HasRegulationsCode602)
				{
					targetInfo.AddWarning(ValidationConstants.InvoiceLine.WarningForImportRegulation602(targetInfo.HumanReadableName));
				}
			}
			if (ValidationHelper.IsDifferentInvoiceLineValueLinkedToSameControllingMsgHeader(parent, ControllingMessageTypeList.Codes.NX301_AX, i => i.JI_BrandName))
			{
				targetInfo.AddWarning(Res.GetString("2F244E0D-1077-4F1D-B4D9-09B8C9DBA1FE", "Invoice lines of different Brand name cannot be linked to the same NX301_AX message."));
			}
			if (ValidationHelper.IsDifferentInvoiceLineValueLinkedToSameControllingMsgHeader(parent, ControllingMessageTypeList.Codes.NX601, i => i.JI_BrandName))
			{
				targetInfo.AddWarning(Res.GetString("42966AD9-6346-4EC0-A1C1-E5F6CB2CB8F9", "Invoice lines of different Brand name cannot be linked to the same NX601 message."));
			}
		}

		protected override void CheckJI_ZZF_NKTaxType()
		{
			//no validation, TaxType is not used in TW
		}

		protected override void CheckJI_HazMatCode()
		{
			base.CheckJI_HazMatCode();
			var info = Parent.JI_HazMatCodeInfo;
			ListValidation.MessageErrorIfInvalidCode(info, ResString.GetMultilingualString("861E008B-FAEE-4CDD-AC3F-0E4A4C09997C", "The entered {0} does not exist.", info.HumanReadableName));
		}

		internal void CheckAssignedJobComInvLineRefs()
		{
			var invoiceLine = Parent;
			var assignedCount = invoiceLine.AssignedJobComInvLineRefsCollection.Cast<AssignedJobComInvLineRefs>().Count(x => !x.JG_ReferenceNumber.IsEmpty);
			if (!invoiceLine.JI_Calc_EnvironmentalProtectionCode.IsEmpty)
			{
				assignedCount++;
			}
			invoiceLine.RemoveRowMessageError(ValidationConstants.InvoiceLine.MoreThan10AssignedNumbersPerInvoiceLine);
			if (assignedCount > 10)
			{
				invoiceLine.AddRowMessageError(ValidationConstants.InvoiceLine.MoreThan10AssignedNumbersPerInvoiceLine);
			}
		}

		protected void CheckJI_Calc_RAPRORUnitPrice()
		{
			var targetInfo = Parent.JI_Calc_RAPRORUnitPriceInfo;
			MandatoryValidation.MessageErrorIfIsNegative(targetInfo);
		}

		public void ValidateJI_Calc_RAPRORUnitPrice()
		{
			((IValidationInternals)this).Validate(Parent.JI_Calc_RAPRORUnitPriceInfo, () => { CheckJI_Calc_RAPRORUnitPrice(); });
		}

		public void ValidatePartyIdentifier()
		{
			((IValidationInternals)this).Validate(Parent.PartyIdentifierInfo, () => { CheckPartyIdentifier(); });
		}

		protected void CheckPartyIdentifier()
		{
			var parent = Parent;
			var targetInfo = parent.PartyIdentifierInfo;
			if (!targetInfo.ReadOnly)
			{
				MandatoryValidation.MessageErrorIfNotEntered(targetInfo);
				ListValidation.MessageErrorIfInvalidCode(targetInfo, parent.Lookups.PartyIdentifierCodeList);
			}
		}

		public void ValidateAuthorizedPerson()
		{
			((IValidationInternals)this).Validate(Parent.AuthorizedPersonInfo, () => { CheckAuthorizedPerson(); });
		}

		protected void CheckAuthorizedPerson()
		{
			var targetInfo = Parent.AuthorizedPersonInfo;
			if (!targetInfo.ReadOnly)
			{
				MandatoryValidation.MessageErrorIfNotEntered(targetInfo);
			}
		}

		public void ValidateCertificateNo()
		{
			((IValidationInternals)this).Validate(Parent.CertificateNoInfo, () => { CheckCertificateNo(); });
		}

		protected void CheckCertificateNo()
		{
			var parent = Parent;
			var targetInfo = parent.CertificateNoInfo;
			if (!targetInfo.ReadOnly)
			{
				var certificateNumber = parent.CertificateNo;
				if (!certificateNumber.IsEmpty && certificateNumber.Length != 14)
				{
					targetInfo.AddMessageError(ValidationConstants.MedicalInstrumentOrFood.CertificateNumberLength);
				}
			}
		}

		public void ValidateTrademarkStorageDocsGuid()
		{
			((IValidationInternals)this).Validate(Parent.TrademarkStorageDocsGuidInfo, () => { CheckTrademarkStorageDocsGuid(); });
		}

		protected void CheckTrademarkStorageDocsGuid()
		{
			var propertyInfo = Parent.TrademarkStorageDocsGuidInfo;
			ListValidation.ErrorIfInvalidPK(propertyInfo, Parent.Lookups.TrademarkEDocList);

			var edoc = Parent.TrademarkStorageDoc;
			if (edoc != null)
			{
				string[] imageFileExtentions = { ".GIF", ".BMP", ".TIF", ".JPEG", ".JPG", ".PNG" };
				if (!imageFileExtentions.Any(x => edoc.FileName.EndsWith(x, System.StringComparison.InvariantCultureIgnoreCase)))
				{
					propertyInfo.AddMessageError(Res.GetString("F4115AA0-B0D6-46A0-9B2D-D9F372D27C27", "Please select an image."));
				}
			}
		}

		public override void ValidateAll()
		{
			base.ValidateAll();
			using (((ISingleElementListInternal)Parent).SuspendListChanged())
			{
				ValidateJI_Calc_RAPRORUnitPrice();
				ValidateJI_DeclarationGoodsDescription();
				ValidateJI_PHValueNumeric();
				ValidateJI_SterilizationValueNumeric();
				ValidatePartyIdentifier();
				ValidateAuthorizedPerson();
				ValidateCertificateNo();
				ValidateTrademarkStorageDocsGuid();
				ValidateNX101ShippingMarks();
				ValidateNX101PermitGoodsDescription();
				ValidatePermitCusSupportingNo1();
			}
		}

		public void ValidatePermitCusSupportingNo1()
		{
			((IValidationInternals)this).Validate(Parent.PermitCusSupportingNo1Info, CheckPermitCusSupportingNo1);
		}

		protected virtual void CheckPermitCusSupportingNo1()
		{ }

		protected override void CheckJI_CountryOfOrigin()
		{
			base.CheckJI_CountryOfOrigin();
			var invoiceLine = Parent;
			var targetInfo = invoiceLine.JI_CountryOfOriginInfo;
			var countryOfOrigin = invoiceLine.JI_CountryOfOrigin;
			if (countryOfOrigin.IsEmpty && invoiceLine.IsLinkedNX101WithCertificateType(CertificateTypeList.Codes.Code8, CertificateTypeList.Codes.Code16, CertificateTypeList.Codes.Code17))
			{
				targetInfo.AddMessageError(MandatoryValidation.YouHaveNotEnteredMessage(Res.GetString("C6AC701C-1E31-4C7B-B345-49286A39CA68", "Goods Origin")));
			}
			if (ValidationHelper.IsDifferentInvoiceLineValueLinkedToSameControllingMsgHeader(invoiceLine, ControllingMessageTypeList.Codes.NX301_DN, i => i.JI_CountryOfOrigin))
			{
				targetInfo.AddWarning(Res.GetString("055BC631-4DD6-4D3F-A444-4CE38DDF7E9B", "Invoice lines of different Goods Origin cannot be linked to the same NX301_DN message."));
			}
		}

		protected override void CheckJI_Model()
		{
			base.CheckJI_Model();
			var parent = Parent;
			if (parent.JI_Model.IsEmpty)
			{
				var targetInfo = parent.JI_ModelInfo;
				if (parent.HasRegulationsCode581Or541)
				{
					var description = MandatoryValidation.GetErrorFieldFromProperyInfo(targetInfo);
					targetInfo.AddMessageError(MandatoryValidation.YouHaveNotEnteredMessage(description));
				}

				if (parent.HasRegulationsCode602)
				{
					targetInfo.AddWarning(ValidationConstants.InvoiceLine.WarningForImportRegulation602(targetInfo.HumanReadableName));
				}
			}
		}

		protected override void CheckJI_PackagingQTY()
		{
			base.CheckJI_PackagingQTY();
			if (InvoiceLine.IsForCMHeaderByMessageTypes(ControllingMessageTypeList.Codes.NX301_DN, ControllingMessageTypeList.Codes.NX401, ControllingMessageTypeList.Codes.NX603))
			{
				CompareValidation.MessageErrorIfLessThanOrEqualToZero(Parent.JI_PackagingQTYInfo);
			}
		}

		protected override void CheckJI_PackagingUQ()
		{
			base.CheckJI_PackagingUQ();
			var targetInfo = Parent.JI_PackagingUQInfo;
			ListValidation.MessageErrorIfInvalidCode(targetInfo);
			if (!Parent.JI_PackagingQTY.IsEmpty || InvoiceLine.IsForCMHeaderByMessageTypes(ControllingMessageTypeList.Codes.NX301_DN, ControllingMessageTypeList.Codes.NX401, ControllingMessageTypeList.Codes.NX603))
			{
				MandatoryValidation.MessageErrorIfNotEntered(targetInfo);
			}
		}
	}
}
