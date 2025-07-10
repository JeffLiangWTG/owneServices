using System;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Forwarding.GUI
{
	public partial class AllocateSelectionJobMawbForm : ZForm
	{
		public AllocateSelectionJobMawbForm(AllocateSelectionJobMawb businessObject)
			: base(businessObject)
		{
			InitializeComponent();
			ZFormPostingButtonsStrategy.SetupPosting(this, PostButtons);
		}

		#region Saving/Loading

		AllocateSelectionJobMawb JobMawbBizObj
		{
			get { return (AllocateSelectionJobMawb)BusinessEntity; }
		}

		protected override void Save(ITransactionParticipant[] factories)
		{
			JobMawbBizObj.Process();
			BusinessEntity.RunPreSaveValidation();
			base.Save(factories);
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			DisableNewAction();
		}

		public override string FormVerb
		{
			get { return ZString.Empty; }
		}

		#endregion
	}
}
