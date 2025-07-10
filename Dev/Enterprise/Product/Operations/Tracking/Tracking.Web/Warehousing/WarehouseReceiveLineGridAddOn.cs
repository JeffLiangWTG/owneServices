using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Tracking.Web.ServerServices;
using Enterprise.ZArchitecture.Web.GUI.WebControls;
using Enterprise.ZArchitecture.Web.ServerServices;

namespace Enterprise.Tracking.Web
{
	public class WarehouseReceiveLineGridAddOn : WarehouseDocketLineGridAddOn
	{
		protected override List<IWebServiceMethod> GetServiceMethods() => new List<IWebServiceMethod> { new WarehouseReceiveLineUpdateWSMethod() };

		protected override void AddScripts(ZGuid docketRef, string lineRef, IEnumerable<ISelfBindingWebControl> cellControls)
		{
			var productControl = GetControl(cellControls, ProductBindTo)?.Controls.OfType<ZTextBox>().FirstOrDefault();
			var descriptionControl = GetControl(cellControls, DescriptionBindTo);
			var packsControl = GetControl(cellControls, PacksBindTo);
			var packsUQControl = GetControl(cellControls, PacksUQBindTo);
			var quantityControl = GetControl(cellControls, QuantityBindTo);
			var productUQControl = GetControl(cellControls, ProductUQBindTo);
			var expectedQuantityControl = GetControl(cellControls, ExpectedQuantityBindTo);
			var attribute1Control = GetControl(cellControls, Attribute1BindTo);
			var attribute2Control = GetControl(cellControls, Attribute2BindTo);
			var attribute3Control = GetControl(cellControls, Attribute3BindTo);
			var serialNumberControl = GetControl(cellControls, SerialNumberBindTo);
			var expiryDateControl = GetControl(cellControls, ExpiryDateBindTo);

			var updateScript = $"WarehouseReceiveLineUpdate(this, " +
				$"'{docketRef}', " +
				$"'{lineRef}', " +
				$"'{GetClientID(productControl)}', " +
				$"'{GetClientID(descriptionControl)}', " +
				$"'{GetClientID(packsControl)}', " +
				$"'{GetClientID(packsUQControl)}', " +
				$"'{GetClientID(quantityControl)}', " +
				$"'{GetClientID(productUQControl)}', " +
				$"'{GetClientID(expectedQuantityControl)}', " +
				$"'{GetClientID(attribute1Control)}', " +
				$"'{GetClientID(attribute2Control)}', " +
				$"'{GetClientID(attribute3Control)}', " +
				$"'{GetClientID(serialNumberControl)}', " +
				$"'{GetClientID(expiryDateControl)}');";

			AddUpdateScript(productControl, updateScript);
			AddUpdateScript(packsControl, updateScript);
			AddUpdateScript(packsUQControl, updateScript);
			AddUpdateScript(quantityControl, updateScript);
			AddUpdateScript(expectedQuantityControl, updateScript);
		}

		public string ExpectedQuantityBindTo { get; set; }
		public string ExpiryDateBindTo { get; set; }
	}
}
