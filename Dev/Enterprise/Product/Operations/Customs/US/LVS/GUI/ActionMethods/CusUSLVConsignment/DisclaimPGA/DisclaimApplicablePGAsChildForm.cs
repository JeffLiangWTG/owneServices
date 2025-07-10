using System;
using CargoWise.Common;
using Enterprise.Customs.US.LVS.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Env = System.Environment;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.LVS.GUI
{
	public partial class DisclaimApplicablePGAsChildForm : ZChildForm
	{
		public DisclaimApplicablePGAsChildForm(DisclaimApplicablePGAsApplicator applicator, CusUSLVConsignment[] applicableConsignments)
			: base(applicator)
		{
			consignments = Argument.NotNull(applicableConsignments, "applicableConsignments");
			applicator.PopulateConsignments(consignments);
		}
		readonly CusUSLVConsignment[] consignments;

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
			((IPostingButtonsProvider)this).AssignButtonsInternal(null, cancelButton, applyButton);
		}

		public override string FormVerb => string.Empty;

		protected override void OnApplyButtonClick(object sender, EventArgs e)
		{
			if (BusinessEntity is DisclaimApplicablePGAsApplicator applicator)
			{
				if (applicator.DisclaimOptions.Count == 0)
				{
					Globals.Message.ShowInformation(Res.GetString("0d2830a3-6d84-4296-bd69-f1c64fa9dcb0", "There is no applicable consignment to update."));
				}
				else
				{
					var sectionLog = new DisclaimApplicablePGAsSectionLog();
					applicator.Apply(sectionLog, consignments);

					if (!sectionLog.Logs.IsEmpty)
					{
						Globals.Message.ShowInformation(Res.GetString("1c25d196-2595-41ff-b5c2-0660b5d8c969", "System is unable to update PGA disclaim reason for following consignments:") + Env.NewLine + Env.NewLine + sectionLog.Logs);
					}
					else
					{
						Globals.Message.ShowInformation(Res.GetString("c3619161-8ce3-4a07-b7e2-ab0f07a0698f", "System has updated PGA disclaim reason for all applicable consignments."));
					}
				}
			}

			Close();
		}
	}
}
