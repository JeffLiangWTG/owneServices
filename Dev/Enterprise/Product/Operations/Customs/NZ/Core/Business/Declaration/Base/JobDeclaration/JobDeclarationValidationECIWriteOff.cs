using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.NZ.Business.Declaration.CustomsChargeCalculators;

namespace Enterprise.Customs.NZ.Business.Declaration
{
	public class JobDeclarationValidationECIWriteOff : JobDeclarationValidation
	{
		public JobDeclarationValidationECIWriteOff(JobDeclaration jobDeclaration)
			: base(jobDeclaration)
		{
		}

		protected new JobDeclaration Declaration
		{
			get { return base.Declaration; }
		}

		protected override void CheckJE_TransportMode()
		{
			base.CheckJE_TransportMode();
			ListValidation.ErrorIfInvalidCode(Declaration.JE_TransportModeInfo, Declaration.Lookups.TransportTypeList);
			if (Declaration.JE_TransportMode.IsEmpty)
			{
				Declaration.JE_TransportModeInfo.AddMessageError("Please enter a transport mode.");
			}
		}

		protected override void CheckJE_OH_ShippingLine()
		{
			base.CheckJE_OH_ShippingLine();
			if (Declaration.JE_OH_ShippingLine.IsEmpty || Declaration.ShippingLine == null)
			{
				Declaration.JE_OH_ShippingLineInfo.AddMessageError("Please enter a shipping line.");
			}
		}

		protected override void CheckJE_VoyageFlightNo()
		{
			base.CheckJE_VoyageFlightNo();
			if (Declaration.IsAir || Declaration.IsSea)
			{
				if (Declaration.JE_VoyageFlightNo.IsEmpty)
				{
					Declaration.JE_VoyageFlightNoInfo.AddMessageError("Please enter a voyage/flight no.");
				}
			}
		}

		protected override void CheckJE_RL_NKPortOfArrival()
		{
			base.CheckJE_RL_NKPortOfArrival();
			if (Declaration.PortOfArrival == null)
			{
				Declaration.JE_RL_NKPortOfArrivalInfo.AddMessageError(MessageErrorMustHaveDischargePort);
			}
		}
		public const string MessageErrorMustHaveDischargePort = "Please enter a valid discharge port code.";

		protected override void CheckJE_RL_NKPortOfLoading()
		{
			base.CheckJE_RL_NKPortOfLoading();
			if (Declaration.PortOfLoading == null)
			{
				Declaration.JE_RL_NKPortOfLoadingInfo.AddMessageError("Please enter a valid loading port code.");
			}
		}

		protected override void CheckJE_RL_NKFinalDestination()
		{
			if (Declaration.JE_MessageType != JobMessageTypeList.Codes.Import)
			{
				base.CheckJE_RL_NKFinalDestination();
			}
			if (Declaration.FinalDestination == null)
			{
				Declaration.JE_RL_NKFinalDestinationInfo.AddMessageError(MessageErrorMustHaveDestinationPortCode);
			}
		}
		public const string MessageErrorMustHaveDestinationPortCode = "Please enter a valid destination port code.";

		protected override void CheckJE_RL_NKOrigin()
		{
			base.CheckJE_RL_NKOrigin();
			if (Declaration.Origin == null)
			{
				Declaration.JE_RL_NKOriginInfo.AddMessageError(MessageErrorMissingOriginPortCode);
			}
		}
		public const string MessageErrorMissingOriginPortCode = "Please enter a valid origin port code.";

		protected override void CheckJE_OH_Supplier()
		{
			base.CheckJE_OH_Supplier();
			if (Declaration.Supplier == null)
			{
				Declaration.JE_OH_SupplierInfo.AddMessageError("Please enter a valid supplier.");
			}
		}

		protected override void CheckJE_OH_Importer()
		{
			base.CheckJE_OH_Importer();
			if (Declaration.Importer == null)
			{
				Declaration.JE_OH_ImporterInfo.AddMessageError("Please enter a valid importer");
			}
		}

		protected override void CheckJE_GoodsDescription()
		{
			base.CheckJE_GoodsDescription();
			if (Declaration.JE_GoodsDescription.IsEmpty)
			{
				Declaration.JE_GoodsDescriptionInfo.AddMessageError("Please enter a goods description.");
			}
		}

		protected override void CheckJE_TotalNoOfPacks()
		{
			base.CheckJE_TotalNoOfPacks();
			if (!Declaration.HasContainersAndTheyreAllEmpty)
			{
				if (Declaration.JE_TotalNoOfPacks.IsDefault)
				{
					Declaration.JE_TotalNoOfPacksInfo.AddMessageError("Please enter a number of outer packs.");
				}
			}
		}

		protected override void CheckJE_TotalNoOfPacksPackType()
		{
			base.CheckJE_TotalNoOfPacksPackType();
			if (Declaration.JE_TotalNoOfPacks > 1 && UniversalReferenceHelper.UNEPackageTypeIsBulk(Declaration.Factory, Declaration.JE_TotalNoOfPacksPackType))
			{
				Declaration.JE_TotalNoOfPacksPackTypeInfo.AddMessageError(Declaration.JE_TotalNoOfPacksPackType + " is a bulk type and the number of outer packs should be 1.");
			}
		}

		protected override void CheckJE_TotalWeight()
		{
			ValidateJE_TotalWeightUnit();
			base.CheckJE_TotalWeight();
			if (!Declaration.HasContainersAndTheyreAllEmpty)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Declaration.JE_TotalWeightInfo, "Weight");
			}

			if (Declaration.CusContainers.Count > 0)
			{
				if (Declaration.PrimaryHouseBill == null || ((Bill)Declaration.PrimaryHouseBill).LoosePackageCount == 0)
				{
					if (Declaration.JE_DeclaredWeight != Declaration.CusContainers.TotalGoodsWeight)
					{
						Declaration.JE_TotalWeightInfo.AddMessageError(MessageErrorTotalWeightMustBeEqualToContainerWeight + " (" + Declaration.CusContainers.TotalGoodsWeight.ToString() + " Kg)");
					}
				}
				else
				{
					if (Declaration.JE_DeclaredWeight < Declaration.CusContainers.TotalGoodsWeight)
					{
						Declaration.JE_TotalWeightInfo.AddMessageError(MessageErrorTotalWeightMustBeMoreThanContainerWeight + " (" + Declaration.CusContainers.TotalGoodsWeight.ToString() + " Kg)");
					}
				}
			}
		}
		public const string MessageErrorTotalWeightMustBeEqualToContainerWeight = "When no loose packages are entered, Total Weight must equal the Total Weight Declared on all Containers.";
		public const string MessageErrorTotalWeightMustBeMoreThanContainerWeight = "Total Weight cannot be less than the Total Weight Declared on all Containers.";

		protected override void CheckJE_TotalWeightUnit()
		{
			ValidateJE_TotalWeight();
			base.CheckJE_TotalWeightUnit();
			MandatoryValidation.MessageErrorIfNotEntered(Declaration.JE_TotalWeightUnitInfo, "Unit of weight");
			ListValidation.MessageErrorIfInvalidCode(Declaration.JE_TotalWeightUnitInfo, Declaration.Lookups.WeightUnitList);
		}

		protected override void CheckJE_ECI_InvoiceAmount()
		{
			if (Declaration.JE_ECI_InvoiceAmount == 0m
				&& !Declaration.HasContainersAndTheyreAllEmpty
				&& !Declaration.JE_GoodsDescription.EqualsIgnoringCase("Docs")
				&& !Declaration.JE_GoodsDescription.EqualsIgnoringCase("Documents"))
			{
				Declaration.JE_ECI_InvoiceAmountInfo.AddMessageError(MessageErrorPleaseEnterInvoiceAmount);
			}

			if (Declaration.IsImport && Declaration.IsECIWriteoff)
			{
				var feeChargeCalculator = new EntryFeeCalculator.FeeChargeCalculator(ZDateTime.Now, Declaration.Factory);
				var lowValue = feeChargeCalculator.LowValue;
				if (!lowValue.IsEmpty && Declaration.JE_ECI_InvoiceAmountInLocalCurrency >= lowValue)
				{
					Declaration.JE_ECI_InvoiceAmountInfo.AddWarning(Res.GetString("7DACD46E-3715-4C92-99AC-D1CDCADBA952", "Tariff details are required for consignment values above NZD${0}.  Please enter invoice lines.", lowValue.Truncate(2)));
				}
			}
		}
		public const string MessageErrorPleaseEnterInvoiceAmount = "Please enter the Commercial Invoice Amount.";

		protected override void CheckJE_ECI_InvoiceCurrency()
		{
			if (Declaration.IsECIWriteoff)
			{
				if (Declaration.ECI_InvoiceCurrency == null)
				{
					Declaration.JE_ECI_InvoiceCurrencyInfo.AddMessageError(MessageErrorPleaseEnterAValidCurrencyCode);
				}
				else if (!Declaration.Lookups.AcceptableNZCustomCurrencyList.ContainsCode(Declaration.ECI_InvoiceCurrency.RX_Code))
				{
					Declaration.JE_ECI_InvoiceCurrencyInfo.AddMessageError(MessageErrorInvalidCurrencyForNZCustomsPurposes);
				}
			}
		}
		public const string MessageErrorPleaseEnterAValidCurrencyCode = "Please enter a valid currency code.";
		public const string MessageErrorInvalidCurrencyForNZCustomsPurposes = "Currency Code not allowable for NZ Customs purposes. Customs do not accept this currency.";

		protected override void CheckPackagesActualPackageCount()
		{
			if (Declaration.CusContainers.Count > 0)
			{
				base.CheckPackagesActualPackageCount();
			}
		}

		protected override void CheckJE_PaymentMethod()
		{
			// Don't call base validation as this field does not show for ECI Writeoffs.

			// It does for TSW
			if (Declaration.IsTSWDeclaration)
			{
				base.CheckJE_PaymentMethod();
			}
		}
	}
}
