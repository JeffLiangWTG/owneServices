using System.Collections.Generic;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using CargoWise.Types;
using Enterprise.DocumentEngine.GUI;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.Tracking.Web
{
	[ToolboxData("<{0}:ZDocumentsMenu runat=server></{0}:ZDocumentsMenu>")]
	public class ZDocumentsMenu : WebControl
	{
		public IDocumentsMenuProvider DocumentMenuProvider
		{
			get
			{
				return documentMenuProvider ?? ((ZPage)Page).DataSource as IDocumentsMenuProvider;
			}
			set
			{
				documentMenuProvider = value;
			}
		}
		IDocumentsMenuProvider documentMenuProvider;

		#region Overriden

		protected override void Render(HtmlTextWriter writer)
		{
			if (DesignMode)
			{
				writer.Write((NoResString)"<input type='button' value='Print Documents ...'>");
			}
			else
			{
				if (Helper != null)
				{
					StringBuilder sb = new StringBuilder();

					List<DocumentsMenuItem> items = Helper.GetAvailableDocuments();
					foreach (DocumentsMenuItem item in items)
					{
						sb.AppendFormat((NoResString)"<input type='button' onclick='document.location=\"{0}\";' value=\"{1}\"> ",
								GetDocumentUrl(item),
								item.Name);
					}

					writer.Write(sb.ToString());
				}
			}
		}

		#endregion

		#region Implementation

		DocumentsMenuHelper Helper
		{
			get { return DocumentMenuProvider != null ? DocumentMenuProvider.DocumentsMenuHelper : null; }
		}

		string GetDocumentUrl(DocumentsMenuItem item)
		{
			return DocumentRequestHandler.RequestHelper.GetHandlerUrl(Helper.GetType(), item.ContentType, new ZGuid[] { Helper.PKForBizOCreation, item.DocumentCommand.PK });
		}

		#endregion
	}
}
