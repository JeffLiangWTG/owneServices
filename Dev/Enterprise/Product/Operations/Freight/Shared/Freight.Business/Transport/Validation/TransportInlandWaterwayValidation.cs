using CargoWise.ComponentModel;

namespace Enterprise.Freight.Business
{
	public class TransportInlandWaterwayValidation : TransportValidation
	{
		public TransportInlandWaterwayValidation(Transport transport) : base(transport)
		{
		}

		string IWTLegCannotBeLinkedErrorMessage
		{
			get
			{
				return Res.GetString("c6d3ce2f-d775-45f7-ba34-836701b366a3",
					"Leg cannot be linked when transport mode is Inland Waterway.");
			}
		}

		protected override void CheckJW_IsLinked()
		{
			base.CheckJW_IsLinked();

			if (!Parent.JW_IsLinkedInfo.HasErrors() && Parent.JW_IsLinked && Parent.JW_TransportMode == Core.Constants.TransportModes.InlandWaterwayTransport)
			{
				Parent.JW_IsLinkedInfo.AddError(IWTLegCannotBeLinkedErrorMessage);
			}
		}

		protected override void CheckJW_TransportMode()
		{
			base.CheckJW_TransportMode();

			if (!Parent.JW_IsLinkedInfo.HasErrors() && Parent.JW_TransportMode == Core.Constants.TransportModes.InlandWaterwayTransport && Parent.IsMainLeg)
			{
				Parent.JW_TransportModeInfo.AddError(Res.GetString("7ea37b8b-7cca-4e91-a744-4263fb3b5520",
					"This transport mode is not applicable to Main legs."));
			}

			if (!Parent.JW_IsLinkedInfo.HasErrors() && Parent.JW_IsLinked && Parent.JW_TransportMode == Core.Constants.TransportModes.InlandWaterwayTransport)
			{
				Parent.JW_TransportModeInfo.AddError(IWTLegCannotBeLinkedErrorMessage);
			}
		}
	}
}
