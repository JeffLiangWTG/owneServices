using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Tracking.Web.ServerServices;
using Enterprise.ZArchitecture.Web.GUI.WebControls;
using Enterprise.ZArchitecture.Web.ServerServices;

namespace Enterprise.Tracking.Web
{
	public class WarehouseOrderLineGridAddOn : WarehouseDocketLineGridAddOn
	{
		public string ShortfallBindTo { get; set; }

		protected override List<IWebServiceMethod> GetServiceMethods() => new List<IWebServiceMethod> { new WarehouseOrderLineUpdateWSMethod() };

		protected override void AddScripts(ZGuid docketRef, string lineRef, IEnumerable<ISelfBindingWebControl> cellControls)
		{
			var productControl = GetControl(cellControls, ProductBindTo)?.Controls.OfType<ZTextBox>().FirstOrDefault();
			var descriptionControl = GetControl(cellControls, DescriptionBindTo);
			var packsControl = GetControl(cellControls, PacksBindTo);
			var packsUQControl = GetControl(cellControls, PacksUQBindTo);
			var quantityControl = GetControl(cellControls, QuantityBindTo);
			var productUQControl = GetControl(cellControls, ProductUQBindTo);
			var shortfallControl = GetControl(cellControls, ShortfallBindTo);
			var attribute1Control = GetControl(cellControls, Attribute1BindTo);
			var attribute2Control = GetControl(cellControls, Attribute2BindTo);
			var attribute3Control = GetControl(cellControls, Attribute3BindTo);
			var serialNumberControl = GetControl(cellControls, SerialNumberBindTo);

			var updateScript = $"WarehouseOrderLineUpdate(this, " +
				$"'{docketRef}', " +
				$"'{lineRef}', " +
				$"'{GetClientID(productControl)}', " +
				$"'{GetClientID(descriptionControl)}', " +
				$"'{GetClientID(packsControl)}', " +
				$"'{GetClientID(packsUQControl)}', " +
				$"'{GetClientID(quantityControl)}', " +
				$"'{GetClientID(productUQControl)}', " +
				$"'{GetClientID(shortfallControl)}', " +
				$"'{GetClientID(attribute1Control)}', " +
				$"'{GetClientID(attribute2Control)}', " +
				$"'{GetClientID(attribute3Control)}', " +
				$"'{GetClientID(serialNumberControl)}');";

			AddUpdateScript(productControl, updateScript);
			AddUpdateScript(packsControl, updateScript);
			AddUpdateScript(packsUQControl, updateScript);
			AddUpdateScript(quantityControl, updateScript);
		}
	}
}
