//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoJobContainerDetentionLookups
//
//    This class should be used for overriding collections in AutoJobContainerDetentionLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Agency.Business
{
	public class JobContainerDetentionLookups : AutoJobContainerDetentionLookups
	{
		public JobContainerDetentionLookups(AutoJobContainerDetention parent)
			: base(parent) { }

		public DetentionInvoiceType DetentionTypes
		{
			get { return Factory.GetCachedValue<DetentionInvoiceType>(); }
		}

		public DetentionInvoiceStatus DetentionStatus
		{
			get { return Factory.GetCachedValue<DetentionInvoiceStatus>(); }
		}

		public ContainerMovementCollection Movements
		{
			get
			{
				ContainerMovementDefaultFilterProvider provider = new ContainerMovementDefaultFilterProvider();
				provider.DetentionInvoiced = false;
				provider.Detentionable = true;
				provider.Principal = Parent.NC_OH_Principal;
				provider.Client = Parent.NC_OH_Client;

				ZQuery filter = new ZQuery();

				switch (Parent.NC_DetentionType)
				{
					case DetentionInvoiceType.Codes.Import:
					case DetentionInvoiceType.Codes.Export:
						provider.TriggeringDetentionType = Parent.NC_DetentionType;
						filter.AddToFilter(JobContainerMoveSchema.E9_MovementType, ContainerMovementTypes.GetMovementCodesForDetention(Parent.NC_DetentionType));
						break;

					default:
						filter.AddToFilter(ZQuery.NoResultQuery);
						break;
				}

				ContainerMovementCollection result = new ContainerMovementCollection(Factory, false);
				result.AdditionalFilter = filter;
				provider.SetDefaultFilters(result);
				return result;
			}
		}

		public override OrgHeaderCollection Principals
		{
			get { return new ShipsAgencyPrincipalCollection(Factory); }
		}

		new ContainerDetention Parent
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return (ContainerDetention)base.Parent; }
		}
	}
}


