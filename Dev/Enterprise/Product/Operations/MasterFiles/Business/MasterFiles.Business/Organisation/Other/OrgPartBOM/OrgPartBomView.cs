using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	public class OrgPartBomView : NonPersistentBusinessObject, IObsoleteValidation
	{
		public OrgPartBomView(OrgPartBOM parentBom, ZInt bomLevel)
			: base(parentBom.Factory)
		{
			this.parentBom = parentBom;
			this.bomLevel = bomLevel;
		}

		readonly OrgPartBOM parentBom;

		public override bool ReadOnly
		{
			get { return true; }
		}

		#region Properties

		#region ComponentCode

		public ZString ComponentCode
		{
			get { return new string(' ', 4 * (BOMLevel - 1)) + parentBom.Component.OP_PartNum; }
		}

		public ZPropertyInfo ComponentCodeInfo
		{
			get { return GetZPropertyInfo(nameof(ComponentCode)); }
		}

		#endregion

		#region ComponentDescription

		public ZString ComponentDescription
		{
			get { return parentBom.ComponentDescription; }
		}

		public ZPropertyInfo ComponentDescriptionInfo
		{
			get { return parentBom == null ? null : parentBom.ComponentDescriptionInfo; }
		}

		#endregion

		#region ComponentLevel

		public ZInt BOMLevel
		{
			get { return bomLevel; }
		}
		readonly ZInt bomLevel;

		public ZPropertyInfo BOMLevelInfo
		{
			get { return GetZPropertyInfo(nameof(BOMLevel)); }
		}

		#endregion

		#region ComponentPack

		public ZString ComponentPack
		{
			get { return parentBom.OE_F3_NKPackType; }
		}

		public ZPropertyInfo ComponentPackInfo
		{
			get { return parentBom == null ? null : parentBom.OE_F3_NKPackTypeInfo; }
		}

		#endregion

		#region ComponentQty

		public ZDecimal ComponentQty
		{
			get { return parentBom.OE_ComponentQty; }
		}

		public ZPropertyInfo ComponentQtyInfo
		{
			get { return parentBom == null ? null : parentBom.OE_ComponentQtyInfo; }
		}

		#endregion

		#endregion

	}
}
