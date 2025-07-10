using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;

namespace Enterprise.Freight.Agency.Business
{
	public class ContainerMovementDefaultFilterProvider : DefaultFilterProvider
	{
		public ZString MovementType { get; set; }
		public bool? DetentionInvoiced { get; set; }
		public bool? Detentionable { get; set; }
		public ZString TriggeringDetentionType { get; set; }
		public ZGuid Principal { get; set; }
		public ZGuid Client { get; set; }

		protected override void SetDefaultFiltersCore(IFilterBusinessObjectDefaultsProvider collection)
		{
			AddFilterDefaults(collection, "Movement Type", MovementType); // filter name
			AddFilterDefaults(collection, "Principal", Principal); // filter name
			AddFilterDefaults(collection, "Client", Client); // filter name
			AddFilterDefaults(collection, "Triggering Detentions", TriggeringDetentionType); // filter name

			if (DetentionInvoiced.HasValue)
			{
				AddFilterDefaults(collection, "Detention Invoiced", DetentionInvoiced.Value ? "INV" : "NIV"); // filter name
			}

			if (Detentionable.HasValue)
			{
				AddFilterDefaults(collection, "Detentionable", Detentionable.Value ? "DET" : "NDT"); // filter name
			}
		}
	}
}


