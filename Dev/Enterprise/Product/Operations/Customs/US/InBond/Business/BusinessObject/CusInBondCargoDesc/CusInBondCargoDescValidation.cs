using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.InBond.Business
{
	public class CusInBondCargoDescValidation : Customs.Business.CusInBondCargoDescValidation
	{
		public CusInBondCargoDescValidation(CusInBondCargoDesc parent)
			: base(parent)
		{
		}

		protected SendingMessageValidationHelper Helper => new SendingMessageValidationHelper(Header, Parent);

		public override void ValidateAll()
		{
			var helper = Helper;
			Parent.ClearRowNotifications();
			if (!helper.IsArrivalValidationMode && !helper.IsExportationValidationMode)
			{
				base.ValidateAll();
			}
		}

		protected override void CheckBY_HarmonisedTariff()
		{
			if (Parent.IsDetailedInBond && !Parent.HasChildCommodities)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.BY_HarmonisedTariffInfo);
				if (!Parent.BY_HarmonisedTariff.IsEmpty)
				{
					if (Parent.BY_HarmonisedTariff.Length < 6)
					{
						Parent.BY_HarmonisedTariffInfo.AddMessageError(ValidationConstants.Commodity.TariffNumShouldBeAtLeast6Digits);
					}
					else
					{
						var dateFilter = new ZQuery(USCTariffSchema.UE_DateFrom, SQLComparisonOperator.LessThanOrEqualToDatePartOnly, Parent.EffectiveDateForDutyRate);
						dateFilter.AddToFilter(USCTariffSchema.UE_DateTo, SQLComparisonOperator.GreaterThanOrEqualToDatePartOnly, Parent.EffectiveDateForDutyRate);
						var query = new ZQuery(USCTariffSchema.UE_Tariff, SQLComparisonOperator.StartsWith, Parent.BY_HarmonisedTariff);
						query.AddToFilter(dateFilter);

						USCTariff tariff = Parent.Factory.LoadTop1<USCTariff>(query);
						if (tariff == null)
						{
							query = new ZQuery(USCTariffSchema.UE_Tariff, SQLComparisonOperator.StartsWith, Parent.BY_HarmonisedTariff);
							tariff = Parent.Factory.LoadTop1<USCTariff>(query);
							if (tariff == null)
							{
								Parent.BY_HarmonisedTariffInfo.AddMessageError(ValidationConstants.Commodity.GetTariffNotFound(Parent.BY_HarmonisedTariff));
							}
							else
							{
								Parent.BY_HarmonisedTariffInfo.AddMessageError(ValidationConstants.Commodity.TariffWasFoundButNotOnFile(Parent.EffectiveDateForDutyRate.ToShortDateString()));
							}
						}
					}
				}
			}
		}

		protected override void CheckBY_MonetaryValue()
		{
			base.CheckBY_MonetaryValue();
			if (Parent.BY_MonetaryValue <= 0m && Parent.IsDetailedInBond && !Parent.HasChildCommodities)
			{
				Parent.BY_MonetaryValueInfo.AddMessageError(ValidationConstants.Commodity.MonetaryValueIsRequired);
			}
		}

		protected override void CheckBY_GrossWeight()
		{
			base.CheckBY_GrossWeight();
			if (Parent.BY_GrossWeight <= 0m && Parent.IsDetailedInBond && !Parent.HasChildCommodities)
			{
				Parent.BY_GrossWeightInfo.AddMessageError(ValidationConstants.Commodity.WeightMustBeGreaterThanZero);
			}
		}

		protected override void CheckBY_GrossWeightUnit()
		{
			base.CheckBY_GrossWeightUnit();
			if (!Parent.HasChildCommodities)
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.BY_GrossWeightUnitInfo);
				if (Parent.IsDetailedInBond)
				{
					MandatoryValidation.MessageErrorIfNotEntered(Parent.BY_GrossWeightUnitInfo);
				}
			}
		}

		protected override void CheckBY_PieceCount()
		{
			base.CheckBY_PieceCount();

			if (Parent.HasPieceCountInContainer)
			{
				if (Parent.BY_PieceCount > ZInt.Zero)
				{
					Parent.BY_PieceCountInfo.AddWarning(ValidationConstants.Commodity.PieceCountIsNotRequired);
				}
			}
			else if (Parent.BY_PieceCount <= ZInt.Zero && Parent.IsDetailedInBond && Parent.IsTopLevelCommodity)
			{
				Parent.BY_PieceCountInfo.AddMessageError(ValidationConstants.Commodity.PieceCountMustBeGreaterThanZero);
			}
		}

		protected override void CheckBY_Description()
		{
			base.CheckBY_Description();
			if (Parent.IsDetailedInBond && Parent.IsTopLevelCommodity)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.BY_DescriptionInfo);
			}
		}

		protected override void CheckBY_MarksAndNumbers()
		{
			base.CheckBY_MarksAndNumbers();

			var header = Parent.Header;
			var isFTZMove = header != null && header.BH_FTZMove;
			if ((isFTZMove || Parent.IsDetailedInBond) && Parent.IsTopLevelCommodity)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.BY_MarksAndNumbersInfo);
			}
		}

		#region Part Related

		protected override void CheckBY_OH_Supplier()
		{
			base.CheckBY_OH_Supplier();
			ValidateBY_PartNumber();
		}

		protected override void CheckBY_InvoiceQuantity()
		{
			base.CheckBY_InvoiceQuantity();
			if (Parent.BY_InvoiceQuantity.IsEmpty && Parent.BY_PartNumber_CanBeSetByCustomer && !Parent.BY_PartNumber.IsEmpty && IsExBondAutomationEnabledAndNotDisabled)
			{
				Parent.BY_InvoiceQuantityInfo.AddMessageError(ValidationConstants.Commodity.ABondedWarehousingCommodityRequiresAnInvoiceQuantity);
			}
		}

		protected override void CheckBY_WarehouseEntryNumber()
		{
			base.CheckBY_WarehouseEntryNumber();
			if (Parent.BY_WarehouseEntryNumber.IsEmpty && Parent.BY_PartNumber_CanBeSetByCustomer && !Parent.BY_PartNumber.IsEmpty && IsExBondAutomationEnabledAndNotDisabled)
			{
				Parent.BY_WarehouseEntryNumberInfo.AddMessageError(ValidationConstants.Commodity.ABondedWarehousingCommodityRequiresEntryDetails);
			}
		}

		protected override void CheckBY_WarehouseEntryLineNo()
		{
			base.CheckBY_WarehouseEntryLineNo();
			if (Parent.BY_WarehouseEntryLineNo.IsEmpty && Parent.BY_PartNumber_CanBeSetByCustomer && !Parent.BY_PartNumber.IsEmpty && IsExBondAutomationEnabledAndNotDisabled)
			{
				Parent.BY_WarehouseEntryLineNoInfo.AddMessageError(ValidationConstants.Commodity.ABondedWarehousingCommodityRequiresEntryDetails);
			}
		}

		protected override void CheckBY_PartNumber()
		{
			if (Parent.BY_PartNumber_CanBeSetByCustomer)
			{
				base.CheckBY_PartNumber();
				var notificationType = IsExBondAutomationEnabledAndNotDisabled ? NotificationType.MessageError : NotificationType.Warning;
				var part = Parent.Part;
				if (!Parent.BY_PartNumber.IsEmpty)
				{
					if (part == null)
					{
						if (Parent.Supplier == null && Parent.Importer == null)
						{
							Parent.BY_PartNumberInfo.AddNotification(notificationType, ValidationConstants.Commodity.WarningPartCannotBeFoundBeforeEnteringASupplierOrAnImporter);
						}
						else if (Parent.PartSyncManager.TotalNumberOfPartsCount == 1)
						{
							Parent.BY_PartNumberInfo.AddNotification(notificationType, ValidationConstants.Commodity.WarningPartCodeFoundButNotRelatedToSupplierImporterCombination);
						}
						else if (Parent.PartSyncManager.TotalNumberOfPartsCount > 1)
						{
							Parent.BY_PartNumberInfo.AddNotification(notificationType, ValidationConstants.Commodity.WarningPartCodesFoundButNotRelatedToSupplierImporterCombination);
						}
						else
						{
							Parent.BY_PartNumberInfo.AddNotification(notificationType, ValidationConstants.Commodity.WarningPartCodeNotFoundAtAll);
						}
					}
					else
					{
						if (Parent.PartSyncManager.TotalMatchCount > 1)
						{
							Parent.BY_PartNumberInfo.AddNotification(notificationType, ValidationConstants.Commodity.WarningMoreThanOneProductMatchFound);
						}
						else if (Parent.Pivot == null)
						{
							var importer = Parent.Importer;
							var supplier = Parent.Supplier;
							ZString partAttrib1 = ZString.Empty;
							ZString partAttrib2 = ZString.Empty;
							ZString partAttrib3 = ZString.Empty;
							if (importer != null)
							{
								partAttrib1 = importer.PartAttributeManager.PartAttributeName1;
								partAttrib2 = importer.PartAttributeManager.PartAttributeName2;
								partAttrib3 = importer.PartAttributeManager.PartAttributeName3;
							}
							Parent.BY_PartNumberInfo.AddNotification(notificationType, Enterprise.Customs.US.Business.ValidationConstants.InvoiceLine.ImportTariff.CannotMatchClassificationForPart(part.OP_PartNum, importer == null ? ZString.Empty : importer.OH_Code, supplier == null ? ZString.Empty : supplier.OH_Code, partAttrib1, Parent.BY_PartAttrib1, partAttrib2, Parent.BY_PartAttrib2, partAttrib3, Parent.BY_PartAttrib3, Parent.BY_SerialNumber, Parent.EffectiveDateForDutyRate));
						}
					}
				}
			}
			ValidateBY_PartAttrib1();
			ValidateBY_PartAttrib3();
			ValidateBY_PartAttrib2();
			ValidateBY_SerialNumber();
		}

		protected override void CheckBY_PartAttrib1()
		{
			base.CheckBY_PartAttrib1();
			CheckPartAttribute(Parent.BY_PartAttrib1Info, 1);
		}

		protected override void CheckBY_PartAttrib2()
		{
			base.CheckBY_PartAttrib2();
			CheckPartAttribute(Parent.BY_PartAttrib2Info, 2);
		}

		protected override void CheckBY_PartAttrib3()
		{
			base.CheckBY_PartAttrib3();
			CheckPartAttribute(Parent.BY_PartAttrib3Info, 3);
		}

		void CheckPartAttribute(ZPropertyInfo info, int attribNumber)
		{
			if (Parent.BY_PartNumber_CanBeSetByCustomer)
			{
				var header = Parent.Header;
				if (header != null)
				{
					new PartAttributeValidation().CheckAttribute(header.ImporterOrg, Parent.Part, info, attribNumber);
				}
				if (!((CargoWise.ComponentModel.INotificationProvider)info).HasNotifications(CargoWise.ComponentModel.NotificationType.Error))
				{
					ListValidation.WarnIfInvalidCode(info);
				}
			}
			ValidateBY_PartNumber();
		}

		protected override void CheckBY_SerialNumber()
		{
			base.CheckBY_SerialNumber();
			CheckSerialNumber();
		}

		void CheckSerialNumber()
		{
			if (Parent.BY_PartNumber_CanBeSetByCustomer)
			{
				var header = Parent.Header;
				if (header != null)
				{
					new PartAttributeValidation().CheckSerialNumber(header.ImporterOrg, Parent.Part, Parent.BY_SerialNumberInfo);
				}
			}

			ValidateBY_PartNumber();
		}

		#endregion

		bool IsExBondAutomationEnabledAndNotDisabled
		{
			get
			{
				var moveHeader = Parent.MoveHeader;
				return moveHeader != null && moveHeader.IsExBondAutomationEnabledAndNotDisabled;
			}
		}

		protected new CusInBondCargoDesc Parent
		{
			get { return (CusInBondCargoDesc)base.Parent; }
		}

		protected CusInBondHeader Header => Parent.Header;
	}
}
