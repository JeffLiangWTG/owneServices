using System.Globalization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.SG.V4.Business
{
	public class JobDeclarationValidation : Customs.Business.BaseJobDeclarationValidation
	{
		public JobDeclarationValidation(JobDeclaration parent)
			: base(parent)
		{
		}

		#region Implementation

		public JobDeclaration Declaration
		{
			get { return Parent; }
		}

		protected new JobDeclaration Parent
		{
			get { return (JobDeclaration)base.Parent; }
		}

		protected ValidationHelper ValidationHelper
		{
			get { return validationHelper ?? (validationHelper = new ValidationHelper()); }
		}
		ValidationHelper validationHelper;

		#endregion

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateJE_Calc_InvoicesCount();
		}

		public void ValidateJE_Calc_InvoicesCount()
		{
			ValidateCalculatedProperty(Parent.JE_Calc_InvoicesCountInfo);
		}

		protected void CheckJE_Calc_InvoicesCount()
		{
			if (Parent.JE_Calc_InvoicesCount > 20 && Parent.IsImportOnly)
			{
				Parent.JE_Calc_InvoicesCountInfo.AddError(Only20InvoicesAreAllowed);
			}
		}

		internal const string Only20InvoicesAreAllowed = "For Inward declarations (INP / IPT), a maximum of 20 invoices only are allowed to be sent in any 1 TradeNet message.\r\nTo declare all the consigned goods, you will either need to combine invoices together or split the invoices into separate declarations.";

		protected override void CheckJE_ApplicationCode()
		{
			base.CheckJE_ApplicationCode();
			ListValidation.ErrorIfInvalidCode((NoResString)"Invalid TradeNet version entered. Choose from the drop down list.", Parent.JE_ApplicationCodeInfo);
			if (Parent.JE_ApplicationCode == SGConstants.TradeNetVersion.Four)
			{
				Parent.JE_ApplicationCodeInfo.AddMessageError("TradeNet version 4.0 is no longer available.");
			}
			ValidateJE_ContainerMode();
		}

		protected override void CheckJE_PaymentMethod()
		{
			ListValidation.MessageErrorIfInvalidCode(Parent.JE_PaymentMethodInfo, Parent.Lookups.PaymentPartyList, (NoResString)"Please enter a valid BG Indicator");
		}

		protected override void CheckJE_OH_ShippingLine()
		{
			ListValidation.ErrorIfInvalidPK(Parent.JE_OH_ShippingLineInfo, Parent.Lookups.Organisations);
		}

		protected override void CheckJE_OH_Importer()
		{
			base.CheckJE_OH_Importer();

			ListValidation.ErrorIfInvalidPK(Parent.JE_OH_ImporterInfo, Parent.Lookups.ImportersList);
			if (!Parent.JE_OH_Importer.IsEmpty)
			{
				if (!ValidationHelper.HasUEN(Parent.Importer))
				{
					Parent.JE_OH_ImporterInfo.AddMessageError("Importer " + ValidationHelper.UENDoesNotExist);
				}
			}
		}

		protected override void CheckJE_OH_Forwarder()
		{
			ListValidation.ErrorIfInvalidPK(Parent.JE_OH_ForwarderInfo, Parent.Lookups.ForwarderList);

			if (!Parent.JE_OH_Forwarder.IsEmpty)
			{
				if (!ValidationHelper.HasUEN(Parent.Forwarder))
				{
					Parent.JE_OH_ForwarderInfo.AddMessageError("Forwarder " + ValidationHelper.UENDoesNotExist);
				}
			}
		}

		protected override void CheckJE_VesselName()
		{
			ListValidation.MessageErrorIfInvalidCode(Parent.JE_VesselNameInfo, Parent.Lookups.Vessels);
			if (Parent.JE_TransportMode == TransportModeCodeList.Codes.TransportMode_1_SEA)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.JE_VesselNameInfo, "Inward Vessel");
			}

			if (Parent.Vessel != null && Parent.VesselHasDuplicates)
			{
				// This validation only becomes relevant (active) when the unique key constraint on RV_Code is removed
				Parent.JE_VesselNameInfo.AddMessageError("Inward Vessel entered has duplicate entries in the Vessel Reference file.\r\nPlease use the <F4> module search functionality to select the appropriate vessel.");
			}
		}

		protected override void CheckJE_VoyageFlightNo()
		{
			base.CheckJE_VoyageFlightNo();
			if (!Declaration.IsOutwardTransportOnly)
			{
				if (Parent.IsTradeNet4Point1)
				{
					if (Parent.IsSea)
					{
						MandatoryValidation.MessageErrorIfNotEntered(Parent.JE_VoyageFlightNoInfo, "Inward Voyage");
					}
					else if (Parent.IsAir)
					{
						if (Parent.JE_VoyageFlightNo.IsEmpty && Parent.JE_Folio.IsEmpty)
						{
							ZString errorMsg = "You have not entered an Inward Flight Number, (and/or Aircraft Registration Number for chartered flight).";
							Parent.JE_VoyageFlightNoInfo.AddMessageError(errorMsg);
						}
					}
					else if (Parent.IsRoad)
					{
						MandatoryValidation.MessageErrorIfNotEntered(Parent.JE_VoyageFlightNoInfo, "Vehicle Licence Registration Number");
					}
				}
				else
				{
					if (Parent.IsSea || Parent.IsAir)
					{
						MandatoryValidation.MessageErrorIfNotEntered(Parent.JE_VoyageFlightNoInfo, "Inward Voyage/Flight No.");
					}
				}
			}
		}

		protected override void CheckJE_Folio()
		{
			base.CheckJE_Folio();

			if (!Declaration.IsOutwardTransportOnly)
			{
				if (Parent.IsTradeNet4Point1)
				{
					if (Parent.IsAir)
					{
						if (Parent.JE_Folio.IsEmpty && Parent.JE_VoyageFlightNo.IsEmpty)
						{
							Parent.JE_FolioInfo.AddMessageError("For chartered flights, enter the Aircraft Registration number.");
						}

						ValidateJE_VoyageFlightNo();
					}
				}
			}
		}

		protected override void CheckJE_TransportMode()
		{
			ListValidation.MessageErrorIfInvalidCode(Parent.JE_TransportModeInfo, Parent.Lookups.TransportTypeList, (NoResString)"Please enter a valid Inward Transport Mode");
		}

		protected override void CheckJE_ContainerMode()
		{
			ListValidation.MessageErrorIfInvalidCode(Parent.JE_ContainerModeInfo, Parent.Lookups.CargoIdTypeList, (NoResString)"Please enter a valid Cargo Packing Type");
		}

		protected override void CheckJE_ContainerCount()
		{
		}

		protected override void CheckJE_DateOfArrival()
		{
			if (!Declaration.JE_TransportMode.IsEmpty)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.JE_DateOfArrivalInfo, "Date of Arrival");
			}
		}

		protected override void CheckJE_RL_NKPortOfLoading()
		{
			ListValidation.MessageErrorIfInvalidCode(Parent.JE_RL_NKPortOfLoadingInfo);

			if (!Declaration.HasBeenCleared)
			{
				if (Parent.JE_RL_NKPortOfLoading.StartsWith(Core.Constants.CountryCodes.KoreaNorth) ||
					Parent.JE_RL_NKPortOfLoading.StartsWith(Core.Constants.CountryCodes.Iran))
				{
					Parent.JE_RL_NKPortOfLoadingInfo.AddWarning(Circular18_2010);
				}
			}
		}
		internal const string Circular18_2010 = "Traders are reminded that all goods which are imported from, exported or re-exported to, the DPRK or Iran require a TradeNet® permit to be declared at least 3 working days before the intended date of shipment.\r\n(Refer Customs Circular No. 18/2010)";

		protected override void CheckJE_RL_NKPortOfArrival()
		{
			ListValidation.MessageErrorIfInvalidCode(Parent.JE_RL_NKPortOfArrivalInfo);
			if (!Parent.SG_OutwardTransportMode.IsEmpty && !Parent.IsSeaStore)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.JE_RL_NKPortOfArrivalInfo, "Port of Discharge");
			}

			if (!Declaration.HasBeenCleared)
			{
				if (Parent.JE_RL_NKPortOfArrival.StartsWith(Core.Constants.CountryCodes.KoreaNorth) ||
					Parent.JE_RL_NKPortOfArrival.StartsWith(Core.Constants.CountryCodes.Iran))
				{
					Parent.JE_RL_NKPortOfArrivalInfo.AddWarning(Circular18_2010);
				}
			}
		}

		protected override void CheckJE_MasterBill()
		{
			if (IsMasterBillMandatory && Parent.JE_MasterBill.IsEmpty)
			{
				Parent.JE_MasterBillInfo.AddMessageError("Inward Masterbill/OBL Number is required");
			}

			if (Declaration.JE_TransportMode == TransportModeCodeList.Codes.TransportMode_4_Air)
			{
				if (Declaration != null && !Declaration.IsExWarehouse)
				{
					if (Declaration.JE_VoyageFlightNo.Length >= 2)
					{
						RefAirline airline = RefAirline.LoadFromAirline2LetterCode(Parent.Factory, Declaration.JE_VoyageFlightNo.Left(2));

						if (airline != null && !Declaration.SG_IsInwardHandCarried && airline.RM_EagleAddedAirlinePrefixOrAccountingCode != Parent.JE_MasterBill.Left(3))
						{
							Declaration.JE_MasterBillInfo.AddWarning("The Airline Prefix does not match the Airline 2 Letter Code in the Flight Number.");
						}
					}
				}
			}
		}

		protected override void CheckJE_TotalWeight()
		{
			base.CheckJE_TotalWeight();

			if (!Parent.IsStandAloneCertificateOfOrigin)
			{
				ZDecimal declarationWeightInKG = GetDeclarationWeightInKG;
				ZDecimal invoiceLineWeightInKG = GetInvoiceLinesWeightInKG;

				if (invoiceLineWeightInKG > declarationWeightInKG)
				{
					int divisor = ReportInTonnes ? 1000 : 1;
					string reportedUnit = ReportInTonnes ? Core.Constants.Weight.Tonnes : Core.Constants.Weight.Kilograms;

					string weightErrorMsg =
						"Total Weight declared is less than the Customs Qty entered on the invoice line(s).\r\n" +
						"(Total Weight, in reportable unit of qty: " + (declarationWeightInKG / divisor) + " " + reportedUnit +
						" - Line weight, in equivalent unit of qty: " + (invoiceLineWeightInKG / divisor) + " " + reportedUnit + ")";

					Parent.JE_TotalWeightInfo.AddWarning(weightErrorMsg);
				}
			}
		}

		#region Weight Comparison

		ZDecimal GetDeclarationWeightInKG
		{
			get
			{
				ZDecimal result = 0;

				try
				{
					result = Core.Constants.Weight.Convert(Declaration.JE_TotalWeight, Declaration.JE_TotalWeightUnit, Core.Constants.Weight.Kilograms);
				}
				catch (System.ArgumentException)
				{
				}

				return result;
			}
		}

		ZDecimal GetInvoiceLinesWeightInKG
		{
			get
			{
				ZDecimal result = 0;

				foreach (JobComInvoiceLine invoiceLine in Declaration.InvoiceLines)
				{
					if (invoiceLine.JI_CustomsUnitQty == UnitOfQuantityCodeList.Codes.KGM)
					{
						result += invoiceLine.JI_CustomsQuantity;
					}
					else if (invoiceLine.JI_CustomsUnitQty == UnitOfQuantityCodeList.Codes.TNE)
					{
						result += invoiceLine.JI_CustomsQuantity * 1000m;
					}
				}

				return result;
			}
		}

		/// <summary>
		/// SG Customs requires declarations to be submitted in only 2 weight measurements:
		///     TNE when transport by sea
		///     KGM when transport is other than sea
		/// </summary>
		bool ReportInTonnes
		{
			get
			{
				return
					(Declaration.IsSea && (Declaration.JE_MessageType == MessageTypeCodeList.Codes.INP || Declaration.JE_MessageType == MessageTypeCodeList.Codes.IPT || Declaration.JE_MessageType == MessageTypeCodeList.Codes.TNP))
					|| Declaration.SG_OutwardTransportMode == Core.Constants.TransportModes.Sea && Declaration.IsOUTDEC;
			}
		}

		#endregion

		#region Overridden Check Validations

		protected override void CheckJE_RL_NKFinalDestination()
		{
			//not used in SG Customs
		}

		protected override void CheckJE_HouseBill()
		{
		}

		protected override void CheckJE_RL_NKOrigin()
		{
			//not used in SG Customs
		}

		public override bool IsMasterBillMandatory
		{
			get { return !Declaration.SG_IsInwardHandCarried && (Declaration.JE_TransportMode == TransportModeCodeList.Codes.TransportMode_1_SEA || Declaration.JE_TransportMode == TransportModeCodeList.Codes.TransportMode_4_Air); }
		}

		protected override void CheckJE_TotalNoOfPacksPackTypeIsAValidCode()
		{
			ListValidation.MessageErrorIfInvalidCode(Parent.JE_TotalNoOfPacksPackTypeInfo, Parent.Lookups.JE_TotalNoOfPacksPackType_List);
		}

		protected override void CheckJE_ExportDate()
		{
			if (!Parent.SG_OutwardTransportMode.IsEmpty && Parent.PlaceOfStorage == null)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.JE_ExportDateInfo, "Date of Departure");
			}
		}

		protected override void CheckJE_DateOfFirstArrival()
		{
		}

		protected override void CheckJE_RL_NKPortOfFirstArrival()
		{
		}

		protected override void CheckJE_OH_Consignee()
		{
			base.CheckJE_OH_Consignee();
			if (Parent.Consignee != null)
			{
				ListValidation.ErrorIfInvalidPK(Parent.JE_OH_ConsigneeInfo, Parent.Lookups.ImportersList);
				if (!Parent.HasCofO)
				{
					WarnOnPostCodeLengthGreaterThanAllowed(Parent.Consignee, Parent.JE_OH_ConsigneeInfo);
				}
			}

			foreach (JobComInvoiceHeader invoiceHeader in Declaration.Invoices)
			{
				foreach (JobComInvoiceLine invoiceLine in invoiceHeader.JobComInvoiceLines)
				{
					invoiceLine.MarkAsNeedingValidation();
				}
			}
		}
		protected override void CheckJE_OH_Exporter()
		{
			base.CheckJE_OH_Exporter();

			if (Parent.Exporter != null)
			{
				ListValidation.ErrorIfInvalidPK(Parent.JE_OH_ExporterInfo, Parent.Lookups.SuppliersList);
				if (!Parent.HasCofO)
				{
					WarnOnPostCodeLengthGreaterThanAllowed(Parent.Exporter, Parent.JE_OH_ExporterInfo);
				}
			}

			if (!ValidationHelper.HasUEN(Parent.Exporter))
			{
				Parent.JE_OH_ExporterInfo.AddMessageError("Exporter " + ValidationHelper.UENDoesNotExist);
			}
		}

		protected override void CheckJE_OH_Manufacturer()
		{
			base.CheckJE_OH_Manufacturer();
			if (!Parent.JE_OH_Manufacturer.IsEmpty)
			{
				var manufacturer = Parent.Factory.Load<OrgHeader>(Parent.JE_OH_Manufacturer);
				if (manufacturer != null)
				{
					WarnOnPostCodeLengthGreaterThanAllowed(manufacturer, Parent.JE_OH_ManufacturerInfo);
				}
			}
		}

		protected override void CheckJE_OH_Buyer()
		{
			base.CheckJE_OH_Buyer();
			if (Parent.Buyer != null)
			{
				ListValidation.ErrorIfInvalidPK(Parent.JE_OH_BuyerInfo, Parent.Lookups.ImportersList);
				WarnOnPostCodeLengthGreaterThanAllowed(Parent.Buyer, Parent.JE_OH_BuyerInfo);
			}
		}
		#endregion
		protected void WarnOnPostCodeLengthGreaterThanAllowed(OrgHeader orgToCheckPostCode, ZPropertyInfo fieldInfo)
		{
			if (orgToCheckPostCode != null)
			{
				var orgPostCode = orgToCheckPostCode.MainAddress.OA_PostCode;
				if (orgPostCode.Length > 9)
				{
					var adjustedPostCode = orgPostCode.KeepAlphanumericCharacters().SubstringSafe(0, 9);
					fieldInfo.AddWarning(string.Format(CultureInfo.CurrentCulture, "This Organization has a Post Code that is too long for Singapore Customs to accept, ({0}).\r\nUnless altered on the Organization record, it will be adjusted/truncated to {1} when sent in the TradeNet message.", orgPostCode, adjustedPostCode));
				}
			}
		}

		#region JE_OH_Claimant

		public virtual void ValidateClaimantAddress(JobDocAddressValidation validation)
		{
			var jobDocAddress = validation.Parent;
			ValidateJobDocAddress(jobDocAddress, Parent.JE_OH_ClaimantInfo, "Claimant");

			if (!jobDocAddress.OrganisationPK.IsValid && !Parent.SG_ClaimantCode.IsEmpty)
			{
				jobDocAddress.OrganisationPKInfo.AddMessageError(Res.GetString("87B618A3-30F7-4B98-AA83-C898203B4199", "Claimant is required when a Claimant Code is entered"));
			}
		}

		#endregion

		#region JE_OH_HandlingAgent

		public virtual void ValidateCarrierHandlingAgent(JobDocAddressValidation validation)
		{
			ValidateJobDocAddress(validation.Parent, Parent.JE_OH_HandlingAgentInfo, "Handling Agent");
		}

		#endregion

		#region JE_OH_InwardCarrierAgent

		public virtual void ValidateInwardCarrierAgent(JobDocAddressValidation validation)
		{
			var jobDocAddress = validation.Parent;
			ValidateJobDocAddress(jobDocAddress, Parent.JE_OH_InwardCarrierAgentInfo, "Inward Carrier Agent");

			if (!jobDocAddress.OrganisationPK.IsValid && IsInwardCarrierAgentMandatory)
			{
				MandatoryValidation.MessageErrorIfNotEntered(jobDocAddress.OrganisationPKInfo, "Inward Carrier Agent");
			}
		}

		protected virtual bool IsInwardCarrierAgentMandatory => false;

		#endregion

		#region OutwardShippingLineForwarder

		public virtual void ValidateOutwardShippingLineForwarder(JobDocAddressValidation validation)
		{
			var jobDocAddress = validation.Parent;
			ValidateJobDocAddress(jobDocAddress, Parent.OutwardShippingLineForwarderPKInfo, "Outward Carrier Agent");

			if (!jobDocAddress.OrganisationPK.IsValid && IsOutwardShippingLineForwarderMandatory)
			{
				MandatoryValidation.MessageErrorIfNotEntered(jobDocAddress.OrganisationPKInfo, "Outward Carrier Agent");
			}
		}

		protected virtual bool IsOutwardShippingLineForwarderMandatory => false;

		#endregion

		void ValidateJobDocAddress(JobDocAddress jobDocAddress, ZPropertyInfo listInfo, ZString title)
		{
			if (jobDocAddress.E2_AddressOverride)
			{
				jobDocAddress.OrganisationPKInfo.AddMessageError(Res.GetString("378C7D72-9053-4F61-8950-F3680F093B39", "The {0} Address has been overridden. Set this Organization again, or create a New Organization using the Address Details on the Addresses Tab.", title));
			}
			else if (jobDocAddress.OrganisationPK.IsValid)
			{
				ListValidation.ErrorIfInvalidPK(listInfo);
				if (!listInfo.HasErrors() && !ValidationHelper.HasUEN(jobDocAddress.Organisation))
				{
					jobDocAddress.OrganisationPKInfo.AddMessageError(Res.GetString("F6160711-9BE3-429A-9962-E48C5D734CD6", "{0} {1}", title, ValidationHelper.UENDoesNotExist));
				}
			}
		}
	}
}
