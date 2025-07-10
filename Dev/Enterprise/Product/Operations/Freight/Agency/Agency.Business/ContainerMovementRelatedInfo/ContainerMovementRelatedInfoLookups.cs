using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Agency.Business
{
	public class ContainerMovementRelatedInfoLookups : ZLookups
	{
		public ContainerMovementRelatedInfoLookups(ContainerMovementRelatedInfo parent)
			: base(parent) { }

		public RefUNLOCOCollection Ports
		{
			get { return new RefUNLOCOCollection(Factory); }
		}
	}
}
