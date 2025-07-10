using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class AccChargeCodeCarrierIataMappingValidation : AutoAccChargeCodeCarrierIataMappingValidation
	{
		public AccChargeCodeCarrierIataMappingValidation(AutoAccChargeCodeCarrierIataMapping parent) : base(parent)
		{
		}

		#region ACI_IATAChargeCodeMap

		protected override void CheckACI_IATAChargeCodeMap()
		{
			base.CheckACI_IATAChargeCodeMap();

			MandatoryValidation.CheckEntered(Parent.ACI_IATAChargeCodeMapInfo);

			if (!Parent.ACI_IATAChargeCodeMapInfo.HasErrors())
			{
				ListValidation.ErrorIfInvalidCode(Parent.ACI_IATAChargeCodeMapInfo, Lookups.ACI_IATAChargeCodeMap_List);
			}
		}

		#endregion

		#region ACI_OH_Carrier

		protected override void CheckACI_OH_Carrier()
		{
			base.CheckACI_OH_Carrier();

			if (Parent.ChargeCode != null && Parent.ChargeCode.AccChargeCodeCarrierIataMappings.Count(a => a.ACI_OH_Carrier == Parent.ACI_OH_Carrier) > 1)
			{
				Parent.ACI_OH_CarrierInfo.AddError(Res.GetString("33eca8a1-dfe0-43eb-ac75-78265b9330fb", "More than one IATA codes exist for Airline organization '{0}'.", Parent.Carrier.OH_Code));
			}
		}

		#endregion

		#region Implementation

		protected new AccChargeCodeCarrierIataMapping Parent => base.Parent as AccChargeCodeCarrierIataMapping;

		AccChargeCodeCarrierIataMappingLookups Lookups
		{
			get
			{
				return lookups ?? (lookups = new AccChargeCodeCarrierIataMappingLookups(Parent));
			}
		}
		AccChargeCodeCarrierIataMappingLookups lookups;

		#endregion
	}
}
