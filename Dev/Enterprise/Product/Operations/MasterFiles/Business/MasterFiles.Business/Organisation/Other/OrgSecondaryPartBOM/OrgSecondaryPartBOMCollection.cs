using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class OrgSecondaryPartBOMCollection : ActiveBusinessObjectCollection<OrgSecondaryPartBOM>
	{
		public OrgSecondaryPartBOMCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public OrgSecondaryPartBOMCollection(OrgSupplierPart part)
			: base(Argument.NotNull(part, nameof(part)).Factory, part, null, OrgSecondaryPartBOMSchema.OSB_OP_MainProduct)
		{
		}
	}
}
