using System;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Web.WebService.Common;

namespace Enterprise.Warehouse.Web.WebService.Business
{
#if DEBUG
	[Serializable]
#endif
	public class WhsCartonGroupInfo : DataObjectInfo
	{
		#region Constructors

		public WhsCartonGroupInfo(WhsCartonGroup cartonGroup)
			: this()
		{
			if (cartonGroup == null)
			{
				throw new ArgumentNullException(nameof(cartonGroup), "CartonGroup is required");
			}
			Code = cartonGroup.WCG_Code;
			cartonSizes = new WhsCartonSizeInfoCollection(cartonGroup.CartonSizes);
		}

		public WhsCartonGroupInfo()
		{
			Code = "";
		}

		#endregion;

		#region Properties

		public string Code { get; set; }

		public WhsCartonSizeInfoCollection CartonSizes
		{
			get { return cartonSizes ?? (cartonSizes = new WhsCartonSizeInfoCollection()); }
			set { cartonSizes = value; }
		}

		WhsCartonSizeInfoCollection cartonSizes;

		#endregion
	}
}
