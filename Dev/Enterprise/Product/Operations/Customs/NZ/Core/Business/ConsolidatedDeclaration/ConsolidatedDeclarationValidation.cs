using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.NZ.Business
{
	public sealed class ConsolidatedDeclarationValidation : Customs.Business.ConsolidatedDeclarationValidation
	{
		public ConsolidatedDeclarationValidation(ConsolidatedDeclaration parent) : base(parent)
		{
		}

		new ConsolidatedDeclaration Parent => (ConsolidatedDeclaration)base.Parent;

		protected override void CheckCongruence()
		{
			var hasPeriodicDeclaration = Parent.JobDeclarations.Any(x => x.JE_MessageSubType == JobMessageSubTypeList.Codes.Periodic);
			var message = hasPeriodicDeclaration
				? Res.GetString("A05493CF-6CB5-43BE-B2E5-9390BC8B623D", "Periodic Consolidated Entries must be of the same Importer, Transport Mode, Entry Style, Vessel and Flight/Voyage.")
				: Res.GetString("23936657-F7B1-4D39-8E79-6CFE411D176F", "Consolidated Entries must be of the same Importer, Transport Mode, Entry Style, Discharge ETA, Vessel and Flight/Voyage.");

			if (!Parent.JobDeclarations.IsCongruentOn(
					dec => dec.JE_TransportMode,
					dec => dec.JE_OH_Importer,
					dec => dec.JE_MessageSubType,
					dec => dec.JE_VesselName,
					dec => dec.JE_VoyageFlightNo,
					dec => hasPeriodicDeclaration ? ZDateTime.Empty : dec.JE_DateOfArrival))
			{
				Parent.AddRowError(message);
			}
		}

		protected override void CheckCRD_PeriodTo()
		{
			base.CheckCRD_PeriodTo();

			if (Parent.EntryStyle == JobMessageSubTypeList.Codes.Periodic)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.CRD_PeriodToInfo);
			}
		}
	}
}
