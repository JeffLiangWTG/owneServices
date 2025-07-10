using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.MasterFiles.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business
{
	public class AddInfoJobDeclarationValidation : USAddInfoValidation
	{
		public AddInfoJobDeclarationValidation(AddInfoJobDeclaration parent)
			: base(parent)
		{
		}

		public JobDeclaration Declaration
		{
			get { return Parent.Declaration; }
		}

		protected new AddInfoJobDeclaration Parent
		{
			get { return (AddInfoJobDeclaration)base.Parent; }
		}

		protected AddInfoJobDeclarationLookups Lookups
		{
			get { return Parent.Lookups; }
		}

		protected override void CheckUS_InspecFirms()
		{
			base.CheckUS_InspecFirms();
			ListValidation.MessageErrorIfInvalidCode(Parent.US_InspecFirmsInfo, Parent.Lookups.FIRMSList);
		}

		protected override void CheckUS_InspecPort()
		{
			base.CheckUS_InspecPort();
			ListValidation.MessageErrorIfInvalidCode(Parent.US_InspecPortInfo, Parent.Lookups.RegionDistrictPorts);
		}

		protected override void CheckUS_FSISInspec()
		{
			base.CheckUS_FSISInspec();
			ListValidation.MessageErrorIfInvalidCode(Parent.US_FSISInspecInfo, Parent.Lookups.ImportEstablishments);
		}

		protected override void CheckUS_EntryType()
		{
			base.CheckUS_EntryType();
			if (ShouldValidateUS_EntryType)
			{
				if (EntryTypeList.IsPaperBased(Parent.US_EntryType))
				{
					Parent.US_EntryTypeInfo.AddMessageError(PaperBasedEntryType);
				}
			}

			var declaration = Declaration;
			if (!declaration.IsExport)
			{
				if (!declaration.IsDrawback && EntryTypeList.IsInBondEntryType(Parent.US_EntryType) && !(declaration.IsACEStandalonePNWithoutENSAndCRL && declaration.IsStandalonePNTypeOfBLN))
				{
					Parent.US_EntryTypeInfo.AddMessageError(InBondEntryTypeShouldBeEnteredInInBondType);
				}

				ListValidation.MessageErrorIfInvalidCode(Parent.US_EntryTypeInfo, Parent.Lookups.US_EntryTypeList, GetEntryTypeShoulBeInList(Parent.US_EntryTypeInfo.HumanReadableName));
			}
		}

		internal static IMultilingualString GetEntryTypeShoulBeInList(string type)
		{
			return (NoResString)$"Please enter a valid {type} Code. The code you have selected is not in the {type} codes List.";
		}

		protected override void CheckUS_UI_NKCarrierSCAC()
		{
			base.CheckUS_UI_NKCarrierSCAC();

			var declaration = Declaration;
			if (!Parent.US_UI_NKCarrierSCAC.IsEmpty)
			{
				new IssuerCarrierSCACValidator(declaration.Factory).ValidateSCACCode(declaration.US_UI_NKCarrierSCACInfo, declaration.JE_TransportMode, "Carrier", !declaration.IsACEAutoRoadAndPedTransportMode && !declaration.IsDrawback, declaration.IsExport);

				var shippingLineSCAC = declaration.ShippingLineSCACCode;
				if (!shippingLineSCAC.IsEmpty && Parent.US_UI_NKCarrierSCAC != shippingLineSCAC)
				{
					Parent.US_UI_NKCarrierSCACInfo.AddWarning(string.Format(SCACMisMatchWarning, shippingLineSCAC));
				}
			}
			else
			{
				if (IsCarrierSCACRequired && declaration.CarrierCode.IsEmpty)
				{
					MandatoryValidation.MessageErrorIfNotEntered(Parent.US_UI_NKCarrierSCACInfo, "Carrier");
				}
			}
		}
		const string SCACMisMatchWarning = "The value entered for Carrier SCAC is not the same as that recorded for the Carrier Organization entered ('{0}')";

		protected override void CheckUS_SchDLoading()
		{
			base.CheckUS_SchDLoading();
			if (Parent.US_SchDLoading.IsEmpty && Parent.Declaration.PortOfLadingMappings.Count > 1)
			{
				if (Declaration.IsSchDPortOfLoadingRequired)
				{
					Parent.US_SchDLoadingInfo.AddMessageError(string.Format(MultipleMatches, Parent.IsExport ? Schedule.D : Schedule.K));
				}
			}
			else
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.US_SchDLoadingInfo, Lookups.LoadingSchDList,
					(NoResString)string.Format(ScheduleDPortCodeInvalid, Parent.IsExport ? Schedule.D : Schedule.K));
			}
			ValidateUS_SchDArrival();
		}
		internal const string ScheduleDPortCodeInvalid = "The code you have entered is not a valid Schedule {0} port code.";
		internal const string MultipleMatches = "Multiple Schedule {0} port code matches have been found for this UNLoco. Please select the appropriate code from list.";
		internal const string ExportMultipleMatches = "Multiple port code matches have been found for this UNLoco. Please select the appropriate code from list.";

		protected override void CheckUS_SchDArrival()
		{
			base.CheckUS_SchDArrival();
			if (Parent.US_SchDArrival.IsEmpty && IsArrivalLocalCodeRequired && Parent.Declaration.PortOfArrivalRefLocoMappings.Count > 1)
			{
				Parent.US_SchDArrivalInfo.AddMessageError(string.Format(MultipleMatches, Declaration.IsExport ? Schedule.K : Schedule.D));
			}
		}

		bool IsArrivalLocalCodeRequired
		{
			get { return !Declaration.IsExport || !Declaration.IsAir; }
		}

		protected override void CheckUS_7501Purchased()
		{
			base.CheckUS_7501Purchased();

			var declaration = Declaration;
			if (declaration.IsFormalImport && declaration.IsEntrySummaryValidationMode)
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.US_7501PurchasedInfo, Lookups.US_YesNoList, "Purchased declaration, which is required for Entry Summary printing");
			}
		}

		protected override void CheckUS_SchKINBFinalForeignDest()
		{
			base.CheckUS_SchKINBFinalForeignDest();
			ListValidation.MessageErrorIfInvalidCode(Parent.US_SchKINBFinalForeignDestInfo, Lookups.SchKList, (NoResString)ScheduleKCodeInvalid);
			ValidateUS_SpecialKINBFinalForeignDest();
		}
		internal const string ScheduleKCodeInvalid = "The code you have entered is not a valid Schedule K port code.";

		protected override void CheckUS_SpecialKINBFinalForeignDest()
		{
			base.CheckUS_SpecialKINBFinalForeignDest();
			ListValidation.MessageErrorIfInvalidCode(Parent.US_SpecialKINBFinalForeignDestInfo, Lookups.SpecialKList, (NoResString)SpecialKCodeInvalid);
			if (Parent.US_InbondType == EntryTypeList.Codes.ImmediateExportation || Parent.US_InbondType == EntryTypeList.Codes.TransportationExportation)
			{
				if (Parent.US_SpecialKINBFinalForeignDest.IsEmpty && Parent.US_SchKINBFinalForeignDest.IsEmpty)
				{
					Parent.US_SpecialKINBFinalForeignDestInfo.AddMessageError(FinalForeignDestinationRequired);
				}

				if (!Parent.US_SpecialKINBFinalForeignDest.IsEmpty && !Parent.US_SchKINBFinalForeignDest.IsEmpty)
				{
					Parent.US_SpecialKINBFinalForeignDestInfo.AddMessageError(FinalForeignDestinationOnlyOneRequired);
				}
			}
		}
		internal const string SpecialKCodeInvalid = "The code you have entered is not a valid Special K (for Canada & Mexico) port code.";
		internal const string FinalForeignDestinationRequired = "Final Foreign Destination is required. Enter either Schedule K port code or Special K (for Canada & Mexico) port code.";
		internal const string FinalForeignDestinationOnlyOneRequired = "Final Foreign Destination - Enter one of either Schedule K port code or Special K (for Canada & Mexico) port code, not both.";

		#region Common

		protected void CheckPortMatchesTransportMode(ZPropertyInfo portInfo)
		{
			var port = ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(Parent.Factory, portInfo.Value.ToString(), Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, ZDateTime.Today);

			if (port != null)
			{
				if (Parent.Declaration.IsAir)
				{
					if (!port.ZZD_IsAir)
					{
						portInfo.AddMessageError(ValidationConstants.Declaration.InvalidPortForTransportMode(TransportTypeList.Codes.Air));
					}
				}
				else if (Parent.Declaration.IsSea)
				{
					if (!port.ZZD_IsSea)
					{
						portInfo.AddMessageError(ValidationConstants.Declaration.InvalidPortForTransportMode(TransportTypeList.Codes.Sea));
					}
				}
			}
		}

		protected virtual bool IsCarrierSCACRequired
		{
			get
			{
				switch (Declaration.JE_Calc_USTransportMode)
				{
					case TransportModeCodes.Codes.VesselContainer:
					case TransportModeCodes.Codes.VesselNonContainer:
					case TransportModeCodes.Codes.RailContainer:
					case TransportModeCodes.Codes.RailNonContainer:
					case TransportModeCodes.Codes.AirContainer:
					case TransportModeCodes.Codes.AirNonContainer:
						return true;
					default:
						return false;
				}
			}
		}

		#endregion
	}
}
