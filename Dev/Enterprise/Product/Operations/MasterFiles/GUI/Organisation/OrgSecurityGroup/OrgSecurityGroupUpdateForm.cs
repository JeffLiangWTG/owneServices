using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public partial class OrgSecurityGroupUpdateForm : ZChildForm
	{
		[Obsolete("Use the constructor that takes a business object, this constructor is just for the designer", true)]
		protected OrgSecurityGroupUpdateForm()
		{
		}

		public OrgSecurityGroupUpdateForm(IEnumerable<OrgMiscServ> orgMiscs) : base(orgMiscs.FirstOrDefault())
		{
			Master = orgMiscs.FirstOrDefault();
			OrgMiscs = orgMiscs;
			PostingButtonsUserControl.SaveButton.Visible = false;
			ZFormPostingButtonsStrategy.SetupPosting(this, PostingButtonsUserControl, true);
		}

		public override string FormVerb
		{
			get
			{
				return string.Empty;
			}
		}

		protected override void Save(ITransactionParticipant[] factories)
		{
			foreach (var misc in OrgMiscs.Where(x => x.OM_GG_OrgSecurityGroup != Master.OM_GG_OrgSecurityGroup))
			{
				misc.OM_GG_OrgSecurityGroup = Master.OM_GG_OrgSecurityGroup;
			}

			base.Save(factories);
		}

		readonly OrgMiscServ Master;
		readonly IEnumerable<OrgMiscServ> OrgMiscs;
	}
}
