using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Customs.US.Business.MessageBuilders;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.GUI
{
	public partial class StatementAndACHRerouteForm : ZChildForm
	{
		public StatementAndACHRerouteForm()
		{
		}

		public StatementAndACHRerouteForm(StatementAndACHPaymentReroute businessEntity)
			: base(businessEntity)
		{
			UpdateVisibilityChange();
			businessEntity.Z9_MessageTypeInfo.ValueChanged += new EventHandler(Z9_MessageTypeInfo_ValueChanged);
			businessEntity.Z9_RerouteTypeInfo.ValueChanged += new EventHandler(Z9_RerouteTypeInfo_ValueChanged);
		}

		void Z9_MessageTypeInfo_ValueChanged(object sender, EventArgs e)
		{
			UpdateVisibilityChange();
		}

		void Z9_RerouteTypeInfo_ValueChanged(object sender, EventArgs e)
		{
			UpdateVisibilityChange();
		}

		void UpdateVisibilityChange()
		{
			var isVisible = !BusinessEntity.IsPeriodicMonthlyStatement && !BusinessEntity.IsACE;

			PeriodicStatementPaymentAuthorizationCheckBox.Visible = isVisible;
			ACHPaymentCheckBox.Visible = isVisible;
		}

		public override string FormVerb
		{
			get { return ""; }
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}

		protected new StatementAndACHPaymentReroute BusinessEntity
		{
			get { return (StatementAndACHPaymentReroute)base.BusinessEntity; }
		}

		void SendButton_Click(object sender, EventArgs e)
		{
			if (BusinessEntity != null)
			{
				if (ValidateAndSave() == ContinueWithSave.Yes)
				{
					try
					{
						BusinessEntity.SendRequest();
					}
					catch (Exception e1) when (!e1.IsCriticalException())
					{
						HandleSaveException(e1);
					}
					Close();
				}
			}
		}
	}
}
