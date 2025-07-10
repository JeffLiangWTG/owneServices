using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;

namespace Enterprise.Freight.Forwarding.Business
{
	public class AllocationAdjustmentsSecurity : SecurityOverridenLogin
	{
		public AllocationAdjustmentsSecurity()
			: this(Enumerable.Empty<ForwardingConsol>())
		{
		}

		[SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors", Justification = "Virtual methods are reviewed. All possible values are expected and handled.")]
		public AllocationAdjustmentsSecurity(IEnumerable<ForwardingConsol> consols)
			: base()
		{
			if (consols != null)
			{
				foreach (ForwardingConsol consol in consols)
				{
					Adjustments.Add(new AllocationAdjustment(consol));
				}
			}
		}

		#region Adjustments

		public AllocationAdjustmentCollection Adjustments
		{
			get
			{
				if (adjustments == null)
				{
					adjustments = new AllocationAdjustmentCollection();
					RegisterEditableChildObject(adjustments);
				}
				return adjustments;
			}
		}
		AllocationAdjustmentCollection adjustments;

		#endregion

		#region Adjust All

		public virtual void AdjustAll(GlbStaff authorizer)
		{
			foreach (AllocationAdjustment allocationAdjustment in Adjustments)
			{
				allocationAdjustment.Adjust(authorizer);
			}
		}

		#endregion
	}
}
