using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Web.UI.WebControls;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Web.GUI.WebControls;
using Enterprise.ZArchitecture.Web.ServerServices;

namespace Enterprise.Tracking.Web
{
	public abstract class TrackingGridAddOn : ZDataGridAddOn, IWebServiceMethodsCaller
	{
		public List<IWebServiceMethod> WebServiceMethods => webServiceMethods ?? (webServiceMethods = GetServiceMethods());
		List<IWebServiceMethod> webServiceMethods;

		protected abstract List<IWebServiceMethod> GetServiceMethods();

		protected override void OnGridChange()
		{
			base.OnGridChange();

			if (Grid != null)
			{
				Grid.IncludeItemDataRefKey = true;
			}
		}

		protected new ZPage Page => base.Page as ZPage;

		[SuppressMessage("Microsoft.Globalization", "CA1305:Script.")]
		protected override void AddScripts(DataGridItem item)
		{
			base.AddScripts(item);
			if (Grid != null)
			{
				if (!Grid.ReadOnly && Grid.AllowEdit && Page != null)
				{
					var dataRef = Page.DataSourceIndexer;
					var lineRef = item.Attributes["ref"];

					if (!string.IsNullOrEmpty(lineRef))
					{
						var cellControls = item.Controls
							.OfType<TableCell>()
							.SelectMany(c => c.Controls
								.OfType<WebControl>()
								.OfType<ISelfBindingWebControl>());

						AddScripts(dataRef, lineRef, cellControls);
					}
				}
			}
		}

		protected abstract void AddScripts(ZGuid docketRef, string lineRef, IEnumerable<ISelfBindingWebControl> cellControls);

		protected void AddUpdateScript(WebControl control, string script)
		{
			if (control != null)
			{
				control.Attributes.Add((NoResString)"onchange", script);
			}
		}

		protected string GetClientID(WebControl control) => control?.ClientID ?? string.Empty;

		protected WebControl GetControl(IEnumerable<ISelfBindingWebControl> controls, string bindTo) => controls.Where(c => c.BindTo == bindTo).Cast<WebControl>().FirstOrDefault();
	}
}
