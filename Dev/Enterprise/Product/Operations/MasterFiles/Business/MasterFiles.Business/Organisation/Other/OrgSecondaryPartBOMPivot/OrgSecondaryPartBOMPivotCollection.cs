using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class OrgSecondaryPartBOMPivotCollection : ActiveBusinessObjectCollection<OrgSecondaryPartBOMPivot>
	{
		public OrgSecondaryPartBOMPivotCollection(OrgSecondaryPartBOM secondaryProduct)
			: base(Argument.NotNull(secondaryProduct, nameof(secondaryProduct)).Factory, secondaryProduct, null, OrgSecondaryPartBOMPivotSchema.OPP_OSB_SecondaryPart)
		{
		}
	}
}
