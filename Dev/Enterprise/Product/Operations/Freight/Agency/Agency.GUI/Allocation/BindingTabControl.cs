using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Agency.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Agency.GUI
{
	public sealed class BindingTabControl : ZTemplateTabControl
	{
		#region DataCollection

		public BusinessObjectCollection DataCollection
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return dataCollection; }
			set
			{
				if (dataCollection != value)
				{
					if (dataCollection != null)
					{
						UnHookCollection(dataCollection);

						foreach (BusinessObject bizObj in dataCollection)
						{
							RemoveTab(bizObj);
						}
					}

					dataCollection = value;

					if (dataCollection != null)
					{
						foreach (BusinessObject bizObj in dataCollection)
						{
							AddTab(bizObj);
						}

						HookCollection(dataCollection);
					}
				}
			}
		}
		BusinessObjectCollection dataCollection;

		#endregion

		#region Implementation

		void HookCollection(BusinessObjectCollection collection)
		{
			collection.CountChanged += new CollectionCountChangedEventHandler(Collection_CountChanged);
		}

		void UnHookCollection(BusinessObjectCollection collection)
		{
			collection.CountChanged -= new CollectionCountChangedEventHandler(Collection_CountChanged);
		}

		void AddTab(BusinessObject bizObj)
		{
			if (!tabLookup.ContainsKey(bizObj.PK))
			{
				AgencyPrincipal principal = (AgencyPrincipal)bizObj;

				BindingTab tab = new BindingTab(principal);
				tab.Text = principal.Code;
				tab.Controls.Add(CreateNewControl(principal));

				TabPages.Add(tab);
				tabLookup.Add(bizObj.PK, tab);
			}
		}

		void RemoveTab(BusinessObject bizObj)
		{
			BindingTab tab;

			if (tabLookup.TryGetValue(bizObj.PK, out tab))
			{
				tabLookup.Remove(bizObj.PK);
				tab.TabVisible = false;
				tab.Dispose();
			}
		}

		void Collection_CountChanged(object sender, CollectionCountChangedEventArgs e)
		{
			if (e.ItemAdded)
			{
				AddTab(e.BizObject);
			}
			else if (e.ItemRemoved)
			{
				RemoveTab(e.BizObject);
			}
		}

		static Control CreateNewControl(AgencyPrincipal principal)
		{
			Control result;

			if (ShipsAgencyPrincipalCollectionWithSecurityCheck.AllowedAccessTo(principal.Principal))
			{
				result = new InnerVoyageAllocationControl();
			}
			else
			{
				Label label = new Label();
				label.Text = Res.GetString("af94dfd3-d6c0-49b4-8178-71f23d2b7e1f", "You are not authorized to view details for this principal");
				label.TextAlign = ContentAlignment.MiddleCenter;
				result = label;
			}

			result.Dock = DockStyle.Fill;
			return result;
		}

		readonly Dictionary<ZGuid, BindingTab> tabLookup = new Dictionary<ZGuid, BindingTab>();

		#endregion
	}
}


