using System.Collections.Generic;
using System.Web.UI;
using System.Web.UI.WebControls;
using CargoWise.EntityFramework;
using Enterprise.Freight.Forwarding.AWB.Business;
using Enterprise.Tracking.Web.ServerServices;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Web.GUI.WebControls;
using Enterprise.ZArchitecture.Web.ServerServices;

namespace Enterprise.Tracking.Web
{
	public class AWBOtherChargeCodeGridAddOn : ZDataGridAddOn, IWebServiceMethodsCaller
	{
		#region Constructors

		public AWBOtherChargeCodeGridAddOn()
			: base()
		{
		}

		public AWBOtherChargeCodeGridAddOn(string codeBindTo, string descriptionBindTo, string entitlementCodeBindTo, string prepaidCollectFlagBindTo)
			: base()
		{
			this.codeBindTo = codeBindTo;
			this.descriptionBindTo = descriptionBindTo;
			this.entitlementCodeBindTo = entitlementCodeBindTo;
			this.prepaidCollectFlagBindTo = prepaidCollectFlagBindTo;
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
					string entitlementCodeID = string.Empty;
					string prepaidCollectFlagControlID = string.Empty;
					string getOtherChargeCodeDescriptionScript = string.Empty;

					string otherChargePK = item.DataItem is ExportAWBOtherCharges ? ((ExportAWBOtherCharges)item.DataItem).PK.ToString() : string.Empty;
					if (string.IsNullOrEmpty(otherChargePK))
					{
						IBusinessObjectCollection dataSource = Grid.DataSource as IBusinessObjectCollection;
						if (item.DataSetIndex > -1 && item.DataSetIndex < dataSource.Count)
						{
							if (dataSource[item.DataSetIndex] is BusinessObject)
							{
								otherChargePK = ((BusinessObject)dataSource[item.DataSetIndex]).PK.ToString();
							}
						}
					}

					if (Page != null)
					{
						Control[] prepaidCollectFlagControls = Page.FindControls(PrepaidCollectFlagBindTo);
						foreach (Control prepaidCollectFlagControl in prepaidCollectFlagControls)
						{
							if (prepaidCollectFlagControl is ZDropEditList)
							{
								prepaidCollectFlagControlID = ((ZDropEditList)prepaidCollectFlagControl).TextBoxControl.ClientID;
							}
						}
					}

					string dataSourceIndex = Page != null ? Page.DataSourceIndexer.ToString() : string.Empty;
					if (!string.IsNullOrEmpty(dataSourceIndex) && !string.IsNullOrEmpty(otherChargePK))
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
										codeControlID = control.ClientID;
									}
									if (selfBindingControl.BindTo == DescriptionBindTo)
									{
										descriptionControlID = control.ClientID;
									}
									if (selfBindingControl.BindTo == EntitlementCodeBindTo)
									{
										entitlementCodeID = control.ClientID;
									}
								}
							}
						}
						if (!string.IsNullOrEmpty(codeControlID) &&
							(!string.IsNullOrEmpty(descriptionControlID) ||
							!string.IsNullOrEmpty(entitlementCodeID)))
						{
							getOtherChargeCodeDescriptionScript = "LookupAWBOtherChargeCode('" + codeControlID + "', '" + descriptionControlID + "', '" + entitlementCodeID + "', '" + dataSourceIndex + "', '" + otherChargePK + "', '" + prepaidCollectFlagControlID + "');";
						}
						if (!string.IsNullOrEmpty(getOtherChargeCodeDescriptionScript))
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
											control.Attributes.Add((NoResString)"onchange", getOtherChargeCodeDescriptionScript + " " + existingOnChangeCode);
										}
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

		protected new MAWBDetails Page
		{
			get
			{
				return base.Page as MAWBDetails;
			}
		}

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

		protected string EntitlementCodeBindTo
		{
			get
			{
				return entitlementCodeBindTo;
			}
		}

		protected string PrepaidCollectFlagBindTo
		{
			get
			{
				return prepaidCollectFlagBindTo;
			}
		}

		readonly string codeBindTo;
		readonly string descriptionBindTo;
		readonly string entitlementCodeBindTo;
		readonly string prepaidCollectFlagBindTo;

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
			result.Add(new AWBOtherChargeCodeLookupWSMethod());
			return result;
		}

		#endregion
	}
}
