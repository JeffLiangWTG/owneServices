using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class OrgPartBOMCollection : ActiveBusinessObjectCollection<OrgPartBOM>
	{
		public OrgPartBOMCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public OrgPartBOMCollection(OrgSupplierPart parentPart)
			: this(parentPart, new ZQuery())
		{
		}

		public OrgPartBOMCollection(OrgSupplierPart parentPart, ZQuery filter)
			: base(parentPart.Factory, parentPart, filter, OrgPartBOMSchema.OE_OP_MainProduct)
		{
		}

		#region IsReusableComponent

		public bool IsReusableComponent(OrgSupplierPart childProduct, ZString bomStockKeepingUnit)
		{
			Argument.NotNull(childProduct, "childProduct");
			Argument.NotNullOrEmpty(bomStockKeepingUnit, "bomStockKeepingUnit");

			return this.Single(bom => bom.OE_OP_Component == childProduct.PK && bom.OE_F3_NKPackType == bomStockKeepingUnit).OE_CanReuse;
		}

		#endregion

		#region FindByComponentPKandPackType

		// Retrive a bom product given it's pk and packType
		public OrgPartBOM FindByComponentPKandPackType(ZGuid bomProductPK, ZString productPackType)
		{
			Argument.NotNullOrEmpty(productPackType, "productPackType");
			return this.FirstOrDefault(part => part.OE_OP_Component == bomProductPK && part.OE_F3_NKPackType == productPackType);
		}

		#endregion
	}
}
