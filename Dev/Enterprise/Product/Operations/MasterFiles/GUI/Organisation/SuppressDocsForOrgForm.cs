using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public partial class SuppressDocsForOrgForm : ZChildForm
	{
		public SuppressDocsForOrgForm() : base()
		{
		}

		public SuppressDocsForOrgForm(OrgHeader orgHeader) : base(orgHeader)
		{
			this.Header = orgHeader;
		}

		readonly OrgHeader Header;

		#region Form Heading

		public override string FormHeading
		{
			get
			{
				string caption = Res.GetString("SuppressDocsForOrgForm|FormHeading1", "Documents to Never Deliver");
				if (Header != null)
				{
					caption = Res.GetString("SuppressDocsForOrgForm|FormHeading2", "Documents to Never Deliver for") + " " + Header.OH_Code;
				}
				return caption;
			}
		}

		#endregion
	}
}
