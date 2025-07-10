using System;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Web.WebService.Common;

namespace Enterprise.Warehouse.Web.WebService.Business
{
#if DEBUG
	[Serializable]
#endif
	public class WhsInventoryHeldCodeInfo : DataObjectInfo
	{
		#region Constructors

		public WhsInventoryHeldCodeInfo()
			: base()
		{
			Code = "";
			Description = "";
			ClientCode = "";
			CodeAndDescription = "";
		}

		public WhsInventoryHeldCodeInfo(WhsInventoryHeldCode heldCode)
			: this()
		{
			Code = heldCode.WHC_Code.ToString();
			Description = heldCode.WHC_DescriptionMultilingual.ToString();
			ClientCode = heldCode.Client?.OH_Code.ToString() ?? "";
			CodeAndDescription = string.IsNullOrEmpty(heldCode.WHC_Code) ? string.Empty : heldCode.WHC_Code + " - " + heldCode.WHC_DescriptionMultilingual;
		}

		#endregion

		#region Properties

		public string Code { get; set; }

		public string Description { get; set; }

		public string ClientCode { get; set; }

		public string CodeAndDescription { get; set; }

		#endregion
	}
}
