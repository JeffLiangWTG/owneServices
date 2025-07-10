//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoNZAddInfoValidation
//
//    This class should be used for overriding validation in AutoNZAddInfoValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using System;
using System.Globalization;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.NZ.Business.Declaration;
using Enterprise.Customs.NZ.Business.EDITariff_ReferenceFiles_NZ;
using Enterprise.Customs.NZ.Business.MasterFiles;
using Enterprise.Customs.NZ.Business.TariffValidation;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.NZ.Business
{
	public class NZAddInfoValidation : AutoNZAddInfoValidation
	{
		public NZAddInfoValidation(AutoNZAddInfo parent)
			: base(parent)
		{
		}

		protected new NZAddInfo Parent
		{
			get { return (NZAddInfo)base.Parent; }
		}

		protected JobComInvoiceHeader ParentInvoiceHeader
		{
			get { return Parent.ParentObject as JobComInvoiceHeader; }
		}

		protected JobDeclaration ParentDeclaration
		{
			get { return Parent.ParentObject as JobDeclaration; }
		}

		protected CusContainer ParentContainer
		{
			get { return Parent.ParentObject as CusContainer; }
		}

		protected CusClassPartPivot ParentPartPivot
		{
			get { return Parent.ParentObject as CusClassPartPivot; }
		}

		protected override void CheckZN_PartsOfClassification()
		{
			base.CheckZN_PartsOfClassification();

			if (Parent.ParentObject is ITariffValidationData validationData)
			{
				new TariffValidator(validationData).CheckPartsOfTariff();
			}
		}

		protected override void CheckZN_ConcessionCode()
		{
			base.CheckZN_ConcessionCode();
			if (!Parent.ZN_ConcessionCode.IsEmpty)
			{
				if (UniversalTariffHelper.UseRefDatabaseData)
				{
					ICodeDescriptionPairList concessionList = null;
					if (ParentPartPivot != null)
					{
						concessionList = ParentPartPivot.Lookups.ConcessionList as ICodeDescriptionPairList;
					}

					if (concessionList != null)
					{
						ListValidation.WarnIfInvalidCode(Parent.ZN_ConcessionCodeInfo, concessionList, UniversalTariffHelper.GetInvalidConcessionCodeWarningMessage(Parent.ZN_ConcessionCode));
					}
				}
				else
				{
					ZDateTime dateForDutyRate = ZDateTime.Now;
					if (ParentDeclaration != null)
					{
						dateForDutyRate = ParentDeclaration.DateForDutyRate;
					}
					ZString concessionCodeWarning = IsConcessionCodeValid(Parent.ZN_ConcessionCode, dateForDutyRate);
					if (!concessionCodeWarning.IsEmpty)
					{
						Parent.ZN_ConcessionCodeInfo.AddWarning(concessionCodeWarning.Replace("~", Parent.ZN_ConcessionCode) + ConcessionCodeWarningMessage);
					}
				}
			}
		}

		public const string ConcessionCodeWarningMessage = " - This either means you're using an invalid concession code, the concession code is unpublished, or your Tariff Data is out of date.";
		const string ConcessionCodeWarningNotRecognisedMessage = "Concession Code [~] not recognised";
		const string ConcessionCodeWarningNotActiveMessage = "Concession Code [~] not active yet";
		const string ConcessionCodeErrorExpired = "Concession Code [~] has expired";

		ZString IsConcessionCodeValid(ZString concessionCode, ZDateTime validDate)
		{
			ZString result = ZString.Empty;
			ZQuery concessionFilter = new ZQuery(NZCConcessionSchema.U2_Code, concessionCode);
			NonDependentNZCConcessionCollection concessions = new NonDependentNZCConcessionCollection(Parent.Factory);
			concessions.Load(concessionFilter);
			if (concessions.Count == 0)
			{
				result = ConcessionCodeWarningNotRecognisedMessage;
			}

			foreach (NZCConcession concession in concessions)
			{
				if (concession.U2_DateActiveFrom > validDate)
				{
					result = ConcessionCodeWarningNotActiveMessage;
					break;
				}
				else if (concession.U2_DateActiveTo.IsValid && concession.U2_DateActiveTo < validDate)
				{
					result = ConcessionCodeErrorExpired;
					break;
				}
			}
			return result;
		}

		protected override void CheckZN_SoldOrConsigned()
		{
			base.CheckZN_SoldOrConsigned();
			if (ParentDeclaration != null && !ParentDeclaration.IsTSWDeclaration)
			{
				if (ParentDeclaration.IsFormalEntry && ParentDeclaration.IsExport)
				{
					TermsOfSaleList list = new TermsOfSaleList();
					if (!list.ContainsCode(Parent.ZN_SoldOrConsigned))
					{
						Parent.ZN_SoldOrConsignedInfo.AddMessageError("You must enter the Terms of Sale for Export Customs Entries.");
					}
				}
			}
		}

		protected override void CheckZN_RL_NKProcessingPort()
		{
			base.CheckZN_RL_NKProcessingPort();
			JobDeclaration declaration = ParentDeclaration;
			if (declaration != null && declaration.IsFormalEntry && declaration.IsExcise && Parent.ZN_RL_NKProcessingPort.IsEmpty)
			{
				Parent.ZN_RL_NKProcessingPortInfo.AddMessageError(ExciseEntryMissingProcessingPort);
			}
		}
		public const string ExciseEntryMissingProcessingPort = "This is an excise entry and you need to enter a processing port.";

		protected override void CheckZN_IsZeroRatedAll()
		{
			base.CheckZN_IsZeroRatedAll();
			if (ParentDeclaration != null && !ParentDeclaration.IsExport)
			{
				if (!ParentDeclaration.JE_IsZeroRatedAll.IsEmpty && !Parent.Lookups.YesNoList.ContainsCode(ParentDeclaration.JE_IsZeroRatedAll))
				{
					Parent.ZN_IsZeroRatedAllInfo.AddMessageError(MessageErrorMustHaveValidZeroRatedAllFlag);
				}

				if (Parent.ZN_IsZeroRatedAll.IsEmpty && ParentDeclaration.IsBond)
				{
					Parent.ZN_IsZeroRatedAllInfo.AddMessageError(MessageErrorMustHaveZeroRatedAllFlagWhenBondedWarehouseSet);
				}
			}
		}

		protected override void CheckZN_GoodsLocatedAt()
		{
			base.CheckZN_GoodsLocatedAt();

			var declaration = ParentDeclaration;

			if (declaration != null && declaration.IsTSWDeclaration)
			{
				if (declaration.JE_GoodsLocatedAtVisible)
				{
					if (declaration.JE_GoodsLocatedAt.IsEmpty)
					{
						if (declaration.IsExport)
						{
							if (declaration.IsAir)
							{
								declaration.JE_GoodsLocatedAtInfo.AddMessageError(LocationRequiredForAirExport);
							}
							else if (declaration.IsPost)
							{
								declaration.JE_GoodsLocatedAtInfo.AddMessageError(LocationRequiredForPostExport);
							}
							else if (declaration.IsSea)
							{
								declaration.JE_GoodsLocatedAtInfo.AddWarning(LocationMayBeRequiredForSea);
							}
						}
						else
						{
							if (declaration.IsAir)
							{
								declaration.JE_GoodsLocatedAtInfo.AddMessageError(LocationRequiredForAir);
							}
							else if (declaration.IsPost)
							{
								declaration.JE_GoodsLocatedAtInfo.AddMessageError(LocationRequiredForPost);
							}
							else if (declaration.IsSea)
							{
								declaration.JE_GoodsLocatedAtInfo.AddWarning(LocationMayBeRequiredForSea);
							}
						}
					}

					ListValidation.MessageErrorIfInvalidCode(declaration.JE_GoodsLocatedAtInfo, declaration.Lookups.LocationList);

					var codes = new GoodsLocatedAtList();

					if (codes.ContainsCode(declaration.JE_GoodsLocatedAt))
					{
						ValidateGoodsLocationEntered(declaration, codes);
					}

					if (declaration.JE_GoodsLocatedAt == GoodsLocatedAtListForSeaImport.Codes.DES)
					{
						if (declaration.JE_RL_NKFinalDestination.IsEmpty)
						{
							declaration.JE_GoodsLocatedAtInfo.AddMessageError(FinalDestinatonRequiredForGoodsLocation);
						}
						else if (!declaration.JE_RL_NKFinalDestination.StartsWith(Core.Constants.CountryCodes.NewZealand, StringComparison.OrdinalIgnoreCase))
						{
							declaration.JE_GoodsLocatedAtInfo.AddMessageError(FinalDestinatonMustBeNZPort);
						}

						declaration.Validation.ValidateJE_RL_NKFinalDestination();
					}
				}
			}
		}

		void ValidateGoodsLocationEntered(JobDeclaration declaration, GoodsLocatedAtList codes)
		{
			var orgToCheck = declaration.GetGoodsLocationOrg();

			if (orgToCheck == null)
			{
				var locationHint = GetLocationHint(declaration);
				var extContent = codes.GetDescriptionFromCode(declaration.JE_GoodsLocatedAt);
				var message = string.Format(CultureInfo.InvariantCulture, LocationOrgNotEntered + locationHint, extContent);

				declaration.JE_GoodsLocatedAtInfo.AddMessageError(message);
			}

			if (orgToCheck != null)
			{
				var premiseId = GetCustomsCode(orgToCheck, OrgCusCode.CodeTypes.ControlledPremisesID);

				if (premiseId.IsEmpty)
				{
					declaration.JE_GoodsLocatedAtInfo.AddMessageError(PremiseIDNotEnteredOnOrg);
				}

				var addressToCheck = declaration.GetGoodsLocationAddressPK();

				if (!addressToCheck.IsEmpty && addressToCheck.IsValid)
				{
					premiseId = declaration.GetCustomsCodeForAddress(orgToCheck, addressToCheck, OrgCusCode.CodeTypes.ControlledPremisesID);

					if (premiseId.IsEmpty)
					{
						declaration.JE_GoodsLocatedAtInfo.AddMessageError(LocationAddressNotLinkedToCode);
					}
				}
			}
		}

		ZString GetLocationHint(JobDeclaration declaration)
		{
			var locationHint = ZString.Empty;

			if (declaration.IsExport)
			{
				if (declaration.IsAir)
				{
					locationHint = LocationExportAirHint;
				}
				else if (declaration.IsSea)
				{
					locationHint = LocationExportSeaHint;
				}
			}
			else
			{
				if (declaration.IsAir)
				{
					locationHint = LocationAirHint;
				}
				else if (declaration.IsPost)
				{
					locationHint = LocationPostHint;
				}
				else if (declaration.IsSea)
				{
					locationHint = LocationSeaHint;
				}
			}

			return locationHint;
		}

		ZString GetCustomsCode(OrgHeader organisation, params ZString[] codeTypes)
		{
			return organisation == null ? ZString.Empty : organisation.CustomsCodes.GetCustomsRegNoMatchingCountryAndCodes(Core.Constants.CountryCodes.NewZealand, codeTypes);
		}

		protected override void CheckZN_TransactionNature()
		{
			base.CheckZN_TransactionNature();
			if (ParentDeclaration != null && ParentDeclaration.IsTSWDeclaration)
			{
				if (ParentDeclaration.JE_TransactionNatureVisible)
				{
					if (ParentDeclaration.JE_TransactionNature.IsEmpty)
					{
						Parent.ZN_TransactionNatureInfo.AddMessageError(NatureOfTransactionRequired);
					}
					else if (!ParentDeclaration.Lookups.TransactionNatureList.ContainsCode(ParentDeclaration.JE_TransactionNature))
					{
						Parent.ZN_TransactionNatureInfo.AddMessageError(NatureOfTransactionInvalid);
					}
				}
			}
		}

		protected override void CheckZN_RN_NKCountryOfOrigin()
		{
			base.CheckZN_RN_NKCountryOfOrigin();

			if (ParentPartPivot != null)
			{
				if (!ParentPartPivot.CI_RN_NKCountryOfOrigin.IsEmpty)
				{
					ListValidation.ErrorIfInvalidCode(Parent.ZN_RN_NKCountryOfOriginInfo);
				}
			}
		}

		public const string MessageErrorMustHaveValidZeroRatedAllFlag = "'Zero Rated Charges' must be either blank, 'Yes' or 'No'.";
		public const string MessageErrorMustHaveZeroRatedAllFlagWhenBondedWarehouseSet = "When you select a Bonded Warehouse, you need to decide if you want to Zero Rate all Customs Charges on your Declaration to Customs. Please select a valid value.";
		public const string NatureOfTransactionRequired = "Nature of Transaction must be entered to specify the terms of sale or transfer of ownership of the shipment.";
		public const string NatureOfTransactionInvalid = "Nature of Transaction must be specified from values in the drop down list.";
		public const string LocationRequiredForAir = "For Air, must be transmitted to state the cargo terminal operator / consolidator / freight forwarder premises where the goods are located.";
		public const string LocationRequiredForPost = "For Post, must be transmitted to state the cargo terminal operator / consolidator / freight forwarder premises where the goods are located.";
		public const string LocationRequiredForSea = "For Sea, must be transmitted to state the place at which the goods are located if different to the port of discharge. Not required when goods are cleared directly from the Port.";
		public const string LocationRequiredForAirExport = "For Air, must be transmitted to state the Cargo Terminal Operator / consolidator / freight forwarder responsible for export loading.";
		public const string LocationRequiredForPostExport = "For Post, must be transmitted to state the Cargo Terminal Operator / consolidator / freight forwarder responsible for export loading.";
		public const string LocationMayBeRequiredForSea = "For Sea, must be transmitted to state any consolidator / freight forwarder responsible for export loading. Not required when goods are delivered direct to the Port company.";
		public const string LocationOrgNotEntered = "{0} selected for Goods Location has not been entered.";
		public const string LocationAddressNotLinkedToCode = "The address selected for the Goods Location organisation does not have a Controlled Premise ID code (CCP) associated with it.\r\nPlease update the Organisation > Config grid to add the relevant CCP code for this address.";
		public const string LocationExportAirHint = "\r\nFor Air - Must be entered to state the cargo terminal operator / consolidator / freight forwarder responsible for export loading.";
		public const string LocationExportSeaHint = "\r\nFor Sea - Must be entered to identify where the goods are to be delivered for export loading by a consolidator/freight forwarder. Not required when goods are delivered direct to the Port.";
		public const string LocationAirHint = "\r\nFor Air - Must be entered to state the cargo terminal operator / consolidator / freight forwarder where the goods are located.";
		public const string LocationPostHint = "\r\nFor Post - Must be entered to state the cargo terminal operator / consolidator / freight forwarder where the goods are located.";
		public const string LocationSeaHint = "\r\nFor Sea - enter to state the current location of goods where different to place of discharge i.e. goods have been moved via a domestic transhipment request.";
		public const string PremiseIDNotEnteredOnOrg = "The Customs Premise ID has not been entered on the organisation chosen for Goods Location.";
		public const string FinalDestinatonRequiredForGoodsLocation = "Final Destination selected for the Goods Location has not been entered.";
		public const string FinalDestinatonMustBeNZPort = "Final Destination can only be selected as Goods Location when the destination is a NZ port.";
	}
}
