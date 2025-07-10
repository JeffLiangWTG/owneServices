using System.Collections.Generic;
using System.Web.UI.WebControls;
using Enterprise.Tracking.Web.ServerServices;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Web.GUI.WebControls;
using Enterprise.ZArchitecture.Web.ServerServices;

namespace Enterprise.Tracking.Web
{
	public class AWBHandlingCodeGridAddOn : ZDataGridAddOn, IWebServiceMethodsCaller
	{
		#region Constructors

		public AWBHandlingCodeGridAddOn()
			: base()
		{
		}

		public AWBHandlingCodeGridAddOn(string codeBindTo, string descriptionBindTo)
			: base()
		{
			this.codeBindTo = codeBindTo;
			this.descriptionBindTo = descriptionBindTo;
		}

		#endregion

		#region Overrides

		protected override void AddScripts(DataGridItem item)
		{
			base.AddScripts(item);
			if (Grid != null)
			{
				if (!Grid.ReadOnly && Grid.AllowEdit)
				{
					string codeControlID = string.Empty;
					string descriptionControlID = string.Empty;
					string getHandlingCodeDescriptionScript = string.Empty;

					foreach (TableCell cell in item.Controls)
					{
						foreach (WebControl control in cell.Controls)
						{
							ISelfBindingWebControl selfBindingControl = control as ISelfBindingWebControl;
							if (selfBindingControl != null)
							{
								if (selfBindingControl.BindTo == CodeBindTo)
								{
									codeControlID = control.ClientID;
								}
								if (selfBindingControl.BindTo == DescriptionBindTo)
								{
									descriptionControlID = control.ClientID;
								}
							}
						}
					}
					if (!string.IsNullOrEmpty(codeControlID) &&
						!string.IsNullOrEmpty(descriptionControlID))
					{
						getHandlingCodeDescriptionScript = "LookupAWBHandlingCode('" + codeControlID + "', '" + descriptionControlID + "');";
					}
					if (!string.IsNullOrEmpty(getHandlingCodeDescriptionScript))
					{
						foreach (TableCell cell in item.Controls)
						{
							foreach (WebControl control in cell.Controls)
							{
								ISelfBindingWebControl selfBindingControl = control as ISelfBindingWebControl;
								if (selfBindingControl != null)
								{
									if (selfBindingControl.BindTo == CodeBindTo)
									{
										var existingOnChangeCode = control.Attributes["onchange"] ?? string.Empty;
										control.Attributes.Add((NoResString)"onchange", getHandlingCodeDescriptionScript + " " + existingOnChangeCode);
									}
								}
							}
						}
					}
				}
			}
		}

		#endregion

		#region Implementation

		protected string CodeBindTo
		{
			get
			{
				return codeBindTo;
			}
		}

		protected string DescriptionBindTo
		{
			get
			{
				return descriptionBindTo;
			}
		}

		readonly string codeBindTo;
		readonly string descriptionBindTo;

		#endregion

		#region IWebServiceMethodsCaller

		public List<IWebServiceMethod> WebServiceMethods
		{
			get
			{
				if (webServiceMethods == null)
				{
					webServiceMethods = GetServiceMethods();
				}
				return webServiceMethods;
			}
		}

		List<IWebServiceMethod> webServiceMethods;

		protected virtual List<IWebServiceMethod> GetServiceMethods()
		{
			List<IWebServiceMethod> result = new List<IWebServiceMethod>();
			result.Add(new AWBHandlingCodeLookupWSMethod());
			return result;
		}

		#endregion
	}
}
