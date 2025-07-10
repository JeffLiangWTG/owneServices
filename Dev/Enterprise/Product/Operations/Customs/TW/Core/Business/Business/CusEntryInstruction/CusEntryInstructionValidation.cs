using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.TW.Business
{
	public class CusEntryInstructionValidation : AutoTWCusEntryInstructionValidation
	{
		public CusEntryInstructionValidation(CusEntryInstruction parent)
			: base(parent)
		{
		}

		protected new CusEntryInstruction Parent => (CusEntryInstruction)base.Parent;

		JobDeclaration Declaration => Parent.JobDeclaration;
		CusEntryInstructionLookups Lookups => Parent.Lookups;

		public override void ValidateAll()
		{
			base.ValidateAll();
			using (((ISingleElementListInternal)Parent).SuspendListChanged())
			{
				ValidateTW_TradersRemarks();
				ValidateTW_ICIExamLocation();
				ValidateTW_ICIExamTime();
				ValidateUCRNumber();
			}
		}

		protected override void CheckCEI_Style()
		{
			base.CheckCEI_Style();
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.CEI_StyleInfo, Lookups.StyleList);
		}

		bool AddressDoesNotHaveControlledPremisesIDAndWarehouseControlledPremisesID(OrgAddress address)
		{
			return address?.GetCustomsRegNo(new string[] { OrgCusCode.CodeTypes.ControlledPremisesID, OrgCusCode.CodeTypes.WarehouseControlledPremisesID }).IsEmpty ?? false;
		}

		#region CEI_OA_Warehouse
		protected override void CheckCEI_OA_Warehouse()
		{
			base.CheckCEI_OA_Warehouse();
			var declarationType = Parent.CEI_Style;
			var targetInfo = Parent.CEI_OA_WarehouseInfo;
			var warehouse = Parent.Warehouse;
			if (!targetInfo.Value.IsEmpty && WarehouseShouldnotEnterByCEI_StyleList.Contains(declarationType))
			{
				targetInfo.AddMessageError(Res.GetString("13B9065D-15DC-4875-B14F-6B310C81C9F9", "From Bonded Warehouse Bonded ID is not required for Declaration Type {0}.", declarationType));
			}
			ValidationHelper.CheckOrganisationExistVAT(warehouse?.Header, () => targetInfo.AddMessageError(Res.GetString("0B5F8480-FC07-4C67-A0F9-8D89C52C2012", "A valid TW-VAT number is required for From Bonded Warehouse. To create a valid TW-VAT, visit Organization > Details > Config > Registration.")));

			if (AddressDoesNotHaveControlledPremisesIDAndWarehouseControlledPremisesID(warehouse))
			{
				targetInfo.AddMessageError(Res.GetString("19b83da4-1992-486e-87d5-2564065a94bb", "From Bonded Warehouse should be of Organization Registration Code CCP or CPW. To create a valid TW-CCP or TW-CPW, visit Organization > Details > Config > Registration."));
			}
		}

		IEnumerable<ZString> fWarehouseShouldnotEnterByCEI_StyleList;

		IEnumerable<ZString> WarehouseShouldnotEnterByCEI_StyleList
		{
			get
			{
				if (fWarehouseShouldnotEnterByCEI_StyleList == null)
				{
					fWarehouseShouldnotEnterByCEI_StyleList = new ZString[] {
						Constants.DeclarationTypes.Export.B1,
						Constants.DeclarationTypes.Export.B2,
						Constants.DeclarationTypes.Import.B6,
						Constants.DeclarationTypes.Export.B8,
						Constants.DeclarationTypes.Export.B9,
						Constants.DeclarationTypes.Export.D1,
						Constants.DeclarationTypes.Import.D8,
						Constants.DeclarationTypes.Import.L1,
						Constants.DeclarationTypes.Import.F1,
						Constants.DeclarationTypes.Import.F2,
						Constants.DeclarationTypes.Import.F3,
						Constants.DeclarationTypes.Export.F4,
						Constants.DeclarationTypes.Export.F5,
						Constants.DeclarationTypes.Import.G1,
						Constants.DeclarationTypes.Import.G2,
						Constants.DeclarationTypes.Import.G7,
					};
				}
				return fWarehouseShouldnotEnterByCEI_StyleList;
			}
		}

		protected override void CheckCEI_OA_WarehouseIsValidZGuid()
		{
			base.CheckCEI_OA_WarehouseIsValidZGuid();
			var targetInfo = Parent.CEI_OA_WarehouseInfo;
			var declarationType = Parent.CEI_Style;
			if (targetInfo.Value.IsEmpty && WarehouseShouldEnterByCEI_StyleList.Contains(declarationType))
			{
				targetInfo.AddMessageError(Res.GetString("6B6B70DB-89A7-4D87-9A9F-9EC358A903D5", "From Bonded Warehouse is required for Declaration Type {0}.", declarationType));
			}
		}

		IEnumerable<ZString> fWarehouseShouldEnterByCEI_StyleList;

		IEnumerable<ZString> WarehouseShouldEnterByCEI_StyleList
		{
			get
			{
				if (fWarehouseShouldEnterByCEI_StyleList == null)
				{
					fWarehouseShouldEnterByCEI_StyleList = new ZString[] {
						Constants.DeclarationTypes.Import.D2,
						Constants.DeclarationTypes.Export.D5,
						Constants.DeclarationTypes.Import.D7,
					};
				}
				return fWarehouseShouldEnterByCEI_StyleList;
			}
		}
		#endregion;

		#region CEI_OA_Warehouse2
		protected override void CheckCEI_OA_Warehouse2()
		{
			base.CheckCEI_OA_Warehouse2();
			var declarationType = Parent.CEI_Style;
			var targetInfo = Parent.CEI_OA_Warehouse2Info;
			var warehouse = Parent.Warehouse2;

			if (targetInfo.Value.IsEmpty)
			{
				if (declarationType == Constants.DeclarationTypes.Import.L1)
				{
					targetInfo.AddMessageError(Res.GetString("D7C700D3-3E92-4F00-B631-2FAEB902F4FB", "To Bonded Warehouse is required for Declaration Type L1."));
				}
			}
			else if (Warehouse2ShouldNotEnterByCEI_StyleList.Contains(declarationType))
			{
				targetInfo.AddMessageError(Res.GetString("3E0B6B1D-ACFE-42AA-969B-C06C73147840", "To Bonded Warehouse Bonded ID is not required for Declaration Type {0}.", declarationType));
			}

			ValidationHelper.CheckOrganisationExistVAT(warehouse?.Header, () => targetInfo.AddMessageError(Res.GetString("1A4D9941-2063-4FF3-AABD-6FC929C9CC20", "A valid TW-VAT number is required for To Bonded Warehouse. To create a valid TW-VAT, visit Organization > Details > Config > Registration.")));
			CheckWarehouse2CanNotCoExistOrEmptyWithImporterBondedId(targetInfo);

			if (AddressDoesNotHaveControlledPremisesIDAndWarehouseControlledPremisesID(warehouse))
			{
				targetInfo.AddMessageError(Res.GetString("71c12c92-1935-4083-ab5a-64e36cfd7c63", "To Bonded Warehouse should be of Organization Registration Code CCP or CPW. To create a valid TW-CCP or TW-CPW, visit Organization > Details > Config > Registration."));
			}
		}

		void CheckWarehouse2CanNotCoExistOrEmptyWithImporterBondedId(ZPropertyInfo targetInfo)
		{
			var jobDeclaration = Parent.JobDeclaration;
			if (jobDeclaration != null && ImporterAddressRequirement.ImporterBondedIdCanNotCoExistOrEmptyWithToBondedWarehouseList.Contains(jobDeclaration.CusEntryInstruction.CEI_Style))
			{
				var importerBondedId = jobDeclaration.ImporterDocumentaryAddress.CBPCode;
				if ((importerBondedId.IsEmpty && targetInfo.Value.IsEmpty) || (!importerBondedId.IsEmpty && !targetInfo.Value.IsEmpty))
				{
					targetInfo.AddMessageError(ValidationConstants.TWJobDocAddress.ImporterBondedIdCanNotCoExistOrEmptyWithToBondedWarehouse);
				}
			}
		}

		IEnumerable<ZString> fWarehouse2ShouldnotEnterByCEI_StyleList;

		IEnumerable<ZString> Warehouse2ShouldNotEnterByCEI_StyleList
		{
			get
			{
				if (fWarehouse2ShouldnotEnterByCEI_StyleList == null)
				{
					fWarehouse2ShouldnotEnterByCEI_StyleList = new ZString[] {
						Constants.DeclarationTypes.Export.B1,
						Constants.DeclarationTypes.Import.B6,
						Constants.DeclarationTypes.Export.B8,
						Constants.DeclarationTypes.Export.B9,
						Constants.DeclarationTypes.Import.D2,
						Constants.DeclarationTypes.Export.D5,
						Constants.DeclarationTypes.Import.F1,
						Constants.DeclarationTypes.Import.F2,
						Constants.DeclarationTypes.Import.F3,
						Constants.DeclarationTypes.Export.F4,
						Constants.DeclarationTypes.Export.F5,
						Constants.DeclarationTypes.Import.G1,
						Constants.DeclarationTypes.Import.G2,
						Constants.DeclarationTypes.Import.G7,
					};
				}
				return fWarehouse2ShouldnotEnterByCEI_StyleList;
			}
		}

		protected override void CheckCEI_OA_Warehouse2IsValidZGuid()
		{
			base.CheckCEI_OA_Warehouse2IsValidZGuid();
			var targetInfo = Parent.CEI_OA_Warehouse2Info;
			var declarationType = Parent.CEI_Style;
			if (Warehouse2ShouldEnterByCEI_StyleList.Contains(declarationType) && targetInfo.Value.IsEmpty)
			{
				targetInfo.AddMessageError(Res.GetString("9B9E0159-7D09-45A5-BF9F-3E92FD0847BB", "To Bonded Warehouse is required for Declaration Type {0}.", declarationType));
			}
		}

		IEnumerable<ZString> fWarehouse2ShouldEnterByCEI_StyleList;

		IEnumerable<ZString> Warehouse2ShouldEnterByCEI_StyleList
		{
			get
			{
				if (fWarehouse2ShouldEnterByCEI_StyleList == null)
				{
					fWarehouse2ShouldEnterByCEI_StyleList = new ZString[] {
						Constants.DeclarationTypes.Export.D1,
						Constants.DeclarationTypes.Import.D8,
					};
				}
				return fWarehouse2ShouldEnterByCEI_StyleList;
			}
		}
		#endregion

		#region TW_TradersRemarks
		public void ValidateTW_TradersRemarks()
		{
			ValidateCalculatedProperty(Parent.TW_TradersRemarksInfo);
		}

		protected void CheckTW_TradersRemarks()
		{
			var tradersRemarksStringLength = Parent.TW_TradersRemarks.Length;
			var targetInfo = Parent.TW_TradersRemarksInfo;
			if (Parent.TW_OverrideTradersRemarks)
			{
				MandatoryValidation.CheckEntered(targetInfo);
			}

			var maxlength = 512;
			if (tradersRemarksStringLength > maxlength)
			{
				targetInfo.AddWarning(ValidationConstants.TWJobDocAddressValidationMessages.MaxCharactersLength(maxlength));
			}
		}
		#endregion

		#region TW_ICIExamLocation
		public void ValidateTW_ICIExamLocation()
		{
			ValidateCalculatedProperty(Parent.TW_ICIExamLocationInfo);
		}

		protected void CheckTW_ICIExamLocation()
		{
			if (!Parent.TW_ICIExamLocation.IsEmpty)
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.TW_ICIExamLocationInfo, Lookups.ExaminationZoneList);
			}
			else if (!Parent.TW_ICIExamTime.IsEmpty)
			{
				Parent.TW_ICIExamLocationInfo.AddMessageError(Res.GetString("81EDFA58-3A4B-476E-91A5-D717EDD98B25", "Examination Zone is expected when Time is set."));
			}
		}
		#endregion

		#region TW_ICIExamTime
		public void ValidateTW_ICIExamTime()
		{
			ValidateCalculatedProperty(Parent.TW_ICIExamTimeInfo);
		}

		protected void CheckTW_ICIExamTime()
		{
			TypeValidation.CheckValidSmallDateTime(Parent.TW_ICIExamTimeInfo);
			TypeValidation.CheckValidZDateTimeRange(Parent.TW_ICIExamTimeInfo);
			var examTime = Parent.TW_ICIExamTime;
			if (!examTime.IsEmpty)
			{
				if (examTime < Parent.EntryHeader.DeclarationDate)
				{
					Parent.TW_ICIExamTimeInfo.AddMessageError(Res.GetString("AA5EB907-85C8-45BF-9FAE-D8CB7A328889", "Examination Time should be after the Declaration Date."));
				}
			}
			else if (!Parent.TW_ICIExamLocation.IsEmpty)
			{
				Parent.TW_ICIExamTimeInfo.AddMessageError(Res.GetString("0482BB97-AB90-4267-8900-50925C8AF938", "Examination Time is expected when Zone is set."));
			}
		}
		#endregion

		#region UCRNumber
		public void ValidateUCRNumber()
		{
			ValidateCalculatedProperty(Parent.UCRNumberInfo);
		}

		protected void CheckUCRNumber()
		{
			EnglishCharactersValidation.ErrorIfNotWesternEuropean(Parent.UCRNumberInfo);
		}
		#endregion

		protected override void CheckCEI_DateForDuty()
		{
			var dateForDuty = Parent.CEI_DateForDuty;
			var entryHeader = Parent.EntryHeader as CusEntryHeader;
			if (!(entryHeader?.IsWaitingForResponseOrHasBeenLodgedAtCustoms ?? false) && (!dateForDuty.IsValid || dateForDuty < ZDateTime.Today))
			{
				Parent.CEI_DateForDutyInfo.AddMessageError(Res.GetString("97D2ACEE-8551-4153-B716-E75683DA56D0", "Please enter a 'Declaration Date' that is today."));
			}
		}

		protected override void CheckCEI_CustomsOffice()
		{
			base.CheckCEI_CustomsOffice();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.CEI_CustomsOfficeInfo);
			if (!Parent.CEI_CustomsOffice.IsEmpty)
			{
				if (Parent.CustomsOffice == null)
				{
					Parent.CEI_CustomsOfficeInfo.AddMessageError(ListValidation.InvalidCodeMessageError.ToString());
				}
			}
		}

		protected override void CheckCEI_GoodsLocation()
		{
			base.CheckCEI_GoodsLocation();
			var declaration = Declaration;
			var parent = Parent;
			var goodsLocationInfo = parent.CEI_GoodsLocationInfo;
			if ((declaration?.IsExport ?? false) || (declaration?.IsImport ?? false) || declaration?.JE_CustomsOffice != parent?.CEI_CustomsOffice)
			{
				MandatoryValidation.MessageErrorIfNotEntered(goodsLocationInfo);
			}

			if (!parent.CEI_GoodsLocation.IsEmpty)
			{
				var locationOfGoods = Parent.GoodsLocation;
				if (locationOfGoods == null)
				{
					goodsLocationInfo.AddMessageError(ListValidation.InvalidCodeMessageError.ToString());
				}
				else if (!locationOfGoods.Attributes.HasAttribute(RefCusCodeListAttributeTypes.Codes.CustomsOffice, parent.CEI_CustomsOffice))
				{
					goodsLocationInfo.AddWarning(ValidationConstants.Declaration.GoodsLocationDoesNotBelongToCustomsOffice(Parent, parent.CEI_GoodsLocation, parent.CEI_CustomsOffice));
				}
			}
		}

		protected override void CheckCEI_ExamMode()
		{
			base.CheckCEI_ExamMode();
			var targetInfo = Parent.CEI_ExamModeInfo;
			var examMode = Parent.CEI_ExamMode;
			ListValidation.MessageErrorIfInvalidCode(targetInfo, Lookups.ExamModeList);

			var declaration = Declaration;
			if (declaration != null && examMode != ExamModeList.Codes.WrittenReview)
			{
				var invoiceLines = declaration.InvoiceLines.OfType<JobComInvoiceLine>();
				if (examMode == ExamModeList.Codes.ShallBe && invoiceLines.Any(x => x.IsTariffQuota))
				{
					targetInfo.AddMessageError(Res.GetString("2c3ae640-b61b-4dcb-aa04-190f37e09ddd", "Examination Mode cannot be '9' because the declaration job contains an invoice line applying for a tariff quota."));
				}

				if (invoiceLines.Any(x => x.JI_DeclarationGoodsDescription.Length > 512))
				{
					targetInfo.AddWarning(Res.GetString("e66ac6b5-653b-4a0f-8ca5-a7d1353c9ece", "Examination Mode must be 8 when the declaration has an invoice line where the goods description is longer than 512 bytes."));
				}

				if (Parent.CEI_Style == Constants.DeclarationTypes.Export.G3)
				{
					if (invoiceLines.Any(x => x.JI_Procedure == TW.Business.Constants.ProcedureCodes._92))
					{
						targetInfo.AddWarning(Res.GetString("09525c87-59f0-4de2-b8c1-11e53b9201d6", "Exam. Mode should be 8 when re-exporting unused foreign goods that are applicable for duty refunds."));
					}
				}

				if (declaration.IsImport)
				{
					var entryHeader = declaration.EntryHeader;
					var examModeCaption = targetInfo.HumanReadableName;
					if (entryHeader != null)
					{
						var chargeTypesForWrittenReview = new HashSet<ZString> { DutyTaxFeeCodeList.Codes.A20, DutyTaxFeeCodeList.Codes.A30, DutyTaxFeeCodeList.Codes.A40, DutyTaxFeeCodeList.Codes.A50 };
						if (entryHeader.HasSpecifiedDutyTaxFeeCharges(chargeTypesForWrittenReview))
						{
							targetInfo.AddWarning(Res.GetString("78ae02f2-17e5-4820-97ff-c70a5b4ddd39", "{0} might need to be set to '8' when the entry is subject to Anti-Dumping Duty, Countervailing Duty, Additional Duty, or Retaliatory Duty.", examModeCaption));
						}

						if (examMode != ExamModeList.Codes.CommodityTax && entryHeader.HasB10OrB19Charges)
						{
							targetInfo.AddMessageError(Res.GetString("06353b33-74fd-434f-b0cc-6c5d31220fb9", "{0} must be '8' or 'A' when at least one of the entry lines is subject to Commodity Tax.", examModeCaption));
						}

						if (entryHeader.HasB31OrB69Charges)
						{
							targetInfo.AddMessageError(Res.GetString("55b26b77-0509-4d50-bc2b-865ad2958e52", "{0} must be '8' when at least one of the entry lines is subject to Tobacco and Alcohol Tax.", examModeCaption));
						}

						if (entryHeader.HasB60OrB89Charges)
						{
							targetInfo.AddMessageError(Res.GetString("614c8463-c117-4591-8f48-b5942b7e8365", "{0} must be '8' when at least one of the entry lines is subject to Specifically Selected Goods and Services Tax.", examModeCaption));
						}
					}

					if (invoiceLines.Any(x => x.JI_Tariff.StartsWith("98")))
					{
						targetInfo.AddWarning(Res.GetString("b1439aea-048d-45a0-8907-04894d1f43d3", "{0} might need to be set to '8' when the entry is subject to Quota Duty.", examModeCaption));
					}

					if (invoiceLines.Any(x => !x.HighTechLicense.IsEmpty))
					{
						targetInfo.AddMessageError(Res.GetString("7284e485-1e4d-4931-848b-72b701a81587", "{0} must be '8' when at least one of the Invoice Line is subject to SHTC import goods.", examModeCaption));
					}
				}
			}
		}

		protected override void CheckCEI_ReasonForDuty()
		{
			base.CheckCEI_ReasonForDuty();
			var targetInfo = Parent.CEI_ReasonForDutyInfo;
			ListValidation.MessageErrorIfInvalidCode(targetInfo, Lookups.ReasonforDutyList);

			var entryInstruction = Parent;
			var baseInvoiceLines = entryInstruction?.InvoiceLines;
			var tw_ReasonForDuty = Parent.CEI_ReasonForDuty;
			if (baseInvoiceLines != null && !tw_ReasonForDuty.IsEmpty)
			{
				var invoiceLines = baseInvoiceLines.Cast<JobComInvoiceLine>();
				var je_MessageType = Declaration?.JE_MessageType ?? ZString.Empty;
				var cei_Stype = entryInstruction.CEI_Style;

				if (cei_Stype == Constants.DeclarationTypes.Import.G2 && tw_ReasonForDuty == ReasonforDutyList.Codes.PoorDiscSupplementary && invoiceLines.Any(x => x.JI_Procedure == Constants.ProcedureCodes._35) && invoiceLines.Any(x => x.JI_Procedure != Constants.ProcedureCodes._35))
				{
					Parent.CEI_ReasonForDutyInfo.AddMessageError(ValidationConstants.EntryInstruction.AllInvoiceLineDutyTreatmentMustBe35ForShortInventory);
				}
				if (cei_Stype == Constants.DeclarationTypes.Import.G2 && je_MessageType == Customs.Business.JobMessageTypeList.Codes.Import && invoiceLines.Any(x => x.IsShippingFromFactoryToDutyLevyingArea) && !entryInstruction.IsShippingFromFactoryToDutyLevyingArea)
				{
					Parent.CEI_ReasonForDutyInfo.AddMessageError(ValidationConstants.EntryInstruction.ReasonForDutyMustBe01or13or99);
				}
				if (cei_Stype == Constants.DeclarationTypes.Import.F2 && invoiceLines.Any(x => x.JI_Procedure == Constants.ProcedureCodes._35) && !entryInstruction.IsShippedFromFreeTradeZoneToDutyLevyingArea)
				{
					Parent.CEI_ReasonForDutyInfo.AddMessageError(ValidationConstants.EntryInstruction.ReasonForDutyMustBe04or05or06or07or13);
				}
			}
		}

		protected override void CheckCEI_WHSMonth()
		{
			base.CheckCEI_WHSMonth();
			var monthString = Parent.CEI_WHSMonth;
			if (!monthString.IsEmpty)
			{
				bool isError = !monthString.IsNumbersOnlyOrEmpty;
				if (!isError)
				{
					var intValue = int.Parse(Parent.CEI_WHSMonth, CultureInfo.InvariantCulture);
					if (intValue < 1 || intValue > 12)
					{
						isError = true;
					}
				}

				if (isError)
				{
					Parent.CEI_WHSMonthInfo.AddMessageError(ValidationConstants.EntryInstruction.WHSMonthAllowedValue);
				}
			}
		}

		protected override void CheckCEI_DaysOfDelayedDeclaration()
		{
			base.CheckCEI_DaysOfDelayedDeclaration();
			var daysOfDelayed = Parent.CEI_DaysOfDelayedDeclaration;
			if (daysOfDelayed < 0)
			{
				Parent.CEI_DaysOfDelayedDeclarationInfo.AddMessageError(ValidationConstants.EntryInstruction.DaysOfDelayedDeclarationAllowedValue);
			}
			else if (daysOfDelayed > 20)
			{
				Parent.CEI_DaysOfDelayedDeclarationInfo.AddMessageError(ValidationConstants.EntryInstruction.DaysOfDelayedDeclarationAllowedValue);
			}
		}

		protected override void CheckCEI_DutyRefund()
		{
			base.CheckCEI_DutyRefund();
			var parent = Parent;
			var entryInstruction = Parent;
			var declarationType = entryInstruction.CEI_Style;
			var supplierBondedIDType = Declaration?.SupplierDocumentaryAddress?.CBPCodeType ?? ZString.Empty;
			if (declarationType == Constants.DeclarationTypes.Export.F4)
			{
				var targetInfo = parent.CEI_DutyRefundInfo;
				if (parent.CEI_DutyRefund)
				{
					if (supplierBondedIDType == OrgCusCode.TaiwanCodeTypes.FTZ)
					{
						targetInfo.AddMessageError(Res.GetString("000295D7-6DDF-4FDC-ABA4-6DC21D9B7821", "When Goods is imported from Free Trade Zone, Request Duty Refund shouldn't be ticked."));
					}
				}
				else
				{
					if (supplierBondedIDType != OrgCusCode.TaiwanCodeTypes.FTZ)
					{
						targetInfo.AddMessageError(Res.GetString("6344536C-E905-48A6-9C12-E92E3D9B6B59", "When Goods is Not imported from Free Trade Zone, Request Duty Refund should be ticked."));
					}
				}
			}
		}

		protected override void CheckCEI_PackageDescription()
		{
			base.CheckCEI_PackageDescription();
			if (Parent.CEI_IsCoPackaged)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.CEI_PackageDescriptionInfo);
			}
		}

		protected override void CheckCEI_BoxNumber()
		{
			base.CheckCEI_BoxNumber();
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.CEI_BoxNumberInfo, Parent.Lookups.BoxNumberList);
		}

		protected override void CheckCEI_RORPaymentMethod()
		{
			base.CheckCEI_RORPaymentMethod();
			var parent = Parent;
			var targetInfo = parent.CEI_RORPaymentMethodInfo;
			if (parent.JobDeclaration is JobDeclaration declaration && declaration.InvoiceLines.Cast<JobComInvoiceLine>().Any(x => x.IsROR))
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(targetInfo);
			}
			else
			{
				ListValidation.MessageErrorIfInvalidCode(targetInfo);
			}
		}
	}
}
