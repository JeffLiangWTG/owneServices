using System;
using Enterprise.ZArchitecture.Web.ServerServices;

namespace Enterprise.Tracking.Web.ServerServices
{
	public class PackLineUpdateParameters : WebServiceParameters
	{
		public string ModifiedControlID { get; set; }
		public string NewValue { get; set; }
		public string ShipmentRef { get; set; }
		public string LineRef { get; set; }
		public string QuantityControlID { get; set; }
		public string LengthControlID { get; set; }
		public string WidthControlID { get; set; }
		public string HeightControlID { get; set; }
		public string PackUDControlID { get; set; }
		public string VolumeControlID { get; set; }
		public string VolumeUQControlID { get; set; }

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
