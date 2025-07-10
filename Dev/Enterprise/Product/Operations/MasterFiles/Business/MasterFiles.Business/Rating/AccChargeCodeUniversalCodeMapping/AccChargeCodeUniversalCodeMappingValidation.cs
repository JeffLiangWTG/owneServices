//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoAccChargeCodeUniversalCodeMappingValidation
//
//    This class should be used for overriding validation in AutoAccChargeCodeUniversalCodeMappingValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.MasterFiles.Business.Rating
{
	using System.Linq;
	using CargoWise.EntityFramework;
	using Enterprise.ZArchitecture.Schema;
	using WiseRates.Api.Model;

	public class AccChargeCodeUniversalCodeMappingValidation : AutoAccChargeCodeUniversalCodeMappingValidation
	{
		public AccChargeCodeUniversalCodeMappingValidation(AutoAccChargeCodeUniversalCodeMapping parent) : base(parent)
		{ }

		protected override void CheckAUP_Code()
		{
			base.CheckAUP_Code();

			if (string.IsNullOrWhiteSpace(Parent.AUP_Code))
			{
				MandatoryValidation.CheckEntered(Parent.AUP_CodeInfo);
				return;
			}

			var chargeCodeDescription = Parent.AUP_Type == AccChargeCodeUniversalCodeMapping.Constants.ChargeCodeMappingTypes.Carrier
				? AccChargeCodeUniversalCodeMapping.Constants.ChargeCodeMappingTypeDescriptions.Carrier
				: AccChargeCodeUniversalCodeMapping.Constants.ChargeCodeMappingTypeDescriptions.Universal;

			var universalChargeCodeList = AccChargeCodeUniversalCodeMappingLookups.GetAllUniversalChargeCodesWithMappingInfo(Parent.Factory);

			if (!TryGetCode(universalChargeCodeList))
			{
				Parent.AUP_CodeInfo.AddError($"{chargeCodeDescription} is not valid.");
				return;
			}

			if (Parent.ChargeCode == null)
			{
				Parent.AUP_CodeInfo.AddError(Res.GetString("891fefa0-68dd-4ec8-85b4-741792c3766b", "Parent charge code is not valid"));
				return;
			}

			if (TryGetExistingChargeCodesWithTheSameCurrentCode(out var chargeCode))
			{
				Parent.AUP_CodeInfo.AddError(Res.GetString("7b1d7eb0-0699-4eb9-b2ec-7853652f491a", "\"{0}\" cannot be selected as it has been mapped as the \"{1}\" for Charge Code \"{2}\".", Parent.AUP_Code, chargeCodeDescription, chargeCode.AC_Code));
			}
		}

		protected override void CheckAUP_OH_Carrier()
		{
			base.CheckAUP_OH_Carrier();

			if (Parent.AUP_Type == AccChargeCodeUniversalCodeMapping.Constants.ChargeCodeMappingTypes.Universal)
			{
				if (Parent.Carrier != null)
				{
					Parent.AUP_OH_CarrierInfo.AddError(Res.GetString("9c46e58c-234f-4094-9Bcc-805341b51fa8", "Carrier must be empty for mapping of type UCC."));
				}

				return;
			}

			if (!AccChargeCodeUniversalCodeMappingLookups.IsCarrierUnrestrictedForChargeCode(Parent.Factory, Parent.AUP_Code, Parent.AUP_TransportMode))
			{
				if (!string.IsNullOrWhiteSpace(Parent.AUP_Code) && Parent.Carrier == null)
				{
					Parent.AUP_OH_CarrierInfo.AddError(Res.GetString("bfb507c6-1fe8-4155-99c5-ced0ceb52a5e", "No Universal Charge Code mapping has been set up for charge code \"{0}\" for carrier \"{1}\". Please raise an eRequest if a mapping must be added to Rates Service.", Parent.AUP_Code, Parent.Carrier?.OH_Code));
					return;
				}

				var allOrganisationsLinkedToCode = AccChargeCodeUniversalCodeMappingLookups.TryGetAllCarriersFromCarrierChargeCode(Parent.Factory, Parent.AUP_Code, Parent.AUP_TransportMode);
				if (allOrganisationsLinkedToCode == null || !allOrganisationsLinkedToCode.Contains(Parent.Carrier))
				{
					var transportLine = Parent.AUP_TransportMode == Core.Constants.TransportModes.Air
						? Res.GetString("b75a03d5-84f1-4b36-8078-fb313da86ca1", "air")
						: Res.GetString("e7f8b55e-3f91-4b67-9bc9-af6097453c92", "shipping");
					Parent.AUP_OH_CarrierInfo.AddError(Res.GetString("928d4562-de8d-42f6-a9f1-405d296e1a07", "Organization \"{0}\" cannot be selected as it is not linked to a {1} line connected to the Carrier Charge Code \"{2}\".", Parent.Carrier?.OH_Code, transportLine, Parent.AUP_Code));
					return;
				}
			}
		}

		protected override void CheckAUP_TransportMode()
		{
			base.CheckAUP_TransportMode();

			if (Parent.AUP_Type == AccChargeCodeUniversalCodeMapping.Constants.ChargeCodeMappingTypes.Universal)
			{
				if (!string.IsNullOrEmpty(Parent.AUP_TransportMode))
				{
					Parent.AUP_TransportModeInfo.AddError(Res.GetString("d34b8036-fd32-4183-9418-c8cecbadefe8", "The transport mode must be empty for type UCC"));
				}

				return;
			}

			if (Parent.AUP_TransportMode != Core.Constants.TransportModes.Sea && Parent.AUP_TransportMode != Core.Constants.TransportModes.Air)
			{
				Parent.AUP_TransportModeInfo.AddError(Res.GetString("96ec1d9d-ada2-4cc8-a4d1-c455676e7071", "Only AIR and SEA transport modes are supported"));
				return;
			}
		}

		protected override void CheckAUP_Type()
		{
			base.CheckAUP_Type();
		}

		#region Helpers

		bool TryGetExistingChargeCodesWithTheSameCurrentCode(out AccChargeCode chargeCode)
		{
			var chargeCodeFilter = new ZDBOnlyQuery(typeof(AccChargeCode));
			if (Parent.ChargeCode.IsGlobal)
			{
				chargeCodeFilter.AddToFilter(JoinCondition.And, AccChargeCodeSchema.AC_GC, SQLComparisonOperator.Equal, null);
			}
			else
			{
				chargeCodeFilter.AddToFilter(JoinCondition.And, AccChargeCodeSchema.AC_GC, SQLComparisonOperator.Equal, Parent.ChargeCode.AC_GC);
			}

			var mappingFilter = new ZDBOnlySubQuery(typeof(AccChargeCodeUniversalCodeMapping), AccChargeCodeUniversalCodeMappingSchema.AUP_AC);
			mappingFilter.AddToFilter(AccChargeCodeUniversalCodeMappingSchema.AUP_Code, Parent.AUP_Code);
			mappingFilter.AddToFilter(AccChargeCodeUniversalCodeMappingSchema.AUP_Type, Parent.AUP_Type);
			mappingFilter.AddToFilter(JoinCondition.And, AccChargeCodeUniversalCodeMappingSchema.AUP_AC, SQLComparisonOperator.NotEqual, Parent.AUP_AC);

			chargeCodeFilter.AddSubQuery(AccChargeCodeSchema.PK, mappingFilter, JoinCondition.And);

			chargeCode = Parent.Factory.LoadTop1<AccChargeCode>(chargeCodeFilter);

			return chargeCode != null;
		}

		bool TryGetCode(ChargeCodeWithMappingInfo[] universalChargeCodeList) =>
			Parent.AUP_Type == AccChargeCodeUniversalCodeMapping.Constants.ChargeCodeMappingTypes.Universal
			? universalChargeCodeList.Any(x => x.Code == Parent.AUP_Code)
			: universalChargeCodeList.Any(x => x.ForeignCode == Parent.AUP_Code);

		#endregion
	}
}
