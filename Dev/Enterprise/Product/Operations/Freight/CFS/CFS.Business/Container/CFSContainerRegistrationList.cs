
using CargoWise.EntityFramework;

namespace Enterprise.Freight.CFS.Business
{
	public class CFSContainerRegistrationList : BusinessObjectCollection<CFSContainer>
	{
		public CFSContainerRegistrationList(BusinessObjectFactory factory) : base(factory)
		{
		}

		public CFSContainerRegistrationList(BusinessObjectFactory factory, ZQuery filter) : base(factory, filter)
		{
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}
	}
}
