using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Tracking.Web.ServerServices;
using Enterprise.ZArchitecture.Web.GUI.WebControls;
using Enterprise.ZArchitecture.Web.ServerServices;

namespace Enterprise.Tracking.Web
{
	public class BookingPackLineGridAddOn : TrackingGridAddOn
	{
		public string QuantityBindTo { get; set; }
		public string LengthBindTo { get; set; }
		public string WidthBindTo { get; set; }
		public string HeightBindTo { get; set; }
		public string PackUDBindTo { get; set; }
		public string VolumeBindTo { get; set; }
		public string VolumeUQBindTo { get; set; }

		protected override List<IWebServiceMethod> GetServiceMethods() => new List<IWebServiceMethod> { new PackLineUpdateWSMethod() };

		protected override void AddScripts(ZGuid shipmentRef, string lineRef, IEnumerable<ISelfBindingWebControl> cellControls)
		{
			var quantityControl = GetControl(cellControls, QuantityBindTo);
			var lengthControl = GetControl(cellControls, LengthBindTo);
			var widthControl = GetControl(cellControls, WidthBindTo);
			var heightControl = GetControl(cellControls, HeightBindTo);
			var packUDControl = GetControl(cellControls, PackUDBindTo);
			var volumeControl = GetControl(cellControls, VolumeBindTo);
			var volumeUQControl = GetControl(cellControls, VolumeUQBindTo);

			var updateScript = $"PackLineUpdate(this, " +
				$"'{shipmentRef}', " +
				$"'{lineRef}', " +
				$"'{GetClientID(quantityControl)}', " +
				$"'{GetClientID(lengthControl)}', " +
				$"'{GetClientID(widthControl)}', " +
				$"'{GetClientID(heightControl)}', " +
				$"'{GetClientID(packUDControl)}', " +
				$"'{GetClientID(volumeControl)}', " +
				$"'{GetClientID(volumeUQControl)}');";

			AddUpdateScript(quantityControl, updateScript);
			AddUpdateScript(lengthControl, updateScript);
			AddUpdateScript(widthControl, updateScript);
			AddUpdateScript(heightControl, updateScript);
			AddUpdateScript(packUDControl, updateScript);
			AddUpdateScript(volumeUQControl, updateScript);
		}
	}
}
