using System;
using Enterprise.ZArchitecture.Web.ServerServices;

namespace Enterprise.Tracking.Web.ServerServices
{
	public abstract class WarehouseDocketLineUpdateParameters : WebServiceParameters
	{
		public string ModifiedControlID { get; set; }
		public string NewValue { get; set; }
		public string DocketRef { get; set; }
		public string LineRef { get; set; }
		public string ProductControlID { get; set; }
		public string DescriptionControlID { get; set; }
		public string PacksControlID { get; set; }
		public string PacksUQControlID { get; set; }
		public string QuantityControlID { get; set; }
		public string ProductUQControlID { get; set; }
		public string Attribute1ControlID { get; set; }
		public string Attribute2ControlID { get; set; }
		public string Attribute3ControlID { get; set; }
		public string SerialNumberControlID { get; set; }

		protected override void ValidateCore()
		{
			base.ValidateCore();
			if (string.IsNullOrEmpty(ModifiedControlID))
			{
				throw new ArgumentNullException("ModifiedControlID");
			}
		}
	}
}
