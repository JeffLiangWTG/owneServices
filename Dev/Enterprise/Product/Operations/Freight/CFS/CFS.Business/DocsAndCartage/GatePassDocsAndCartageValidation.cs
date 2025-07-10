using Enterprise.Freight.Business;

namespace Enterprise.Freight.CFS.Business
{
	public class GatePassDocsAndCartageValidation : CFSDocsAndCartageValidation
	{
		public GatePassDocsAndCartageValidation(AutoJobDocsAndCartage parent)
			: base(parent)
		{
		}

		#region Parents

		public new GatePassDocsAndCartage Parent
		{
			get { return (GatePassDocsAndCartage)base.Parent; }
		}

		#endregion

		#region Override

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateReasonForContingencyRelease();
		}

		#endregion

		#region ReasonForContingencyRelease

		public void ValidateReasonForContingencyRelease()
		{
			ValidateCalculatedProperty(Parent.ReasonForContingencyReleaseInfo);
		}

		protected void CheckReasonForContingencyRelease()
		{
			if (Parent.JP_IsContingencyRelease && Parent.ReasonForContingencyRelease.IsEmpty)
			{
				string message = Res.GetString("82c77882-909b-4c40-a056-dcaa6cc6ebf6", "If you choose to manually change the Sea Cargo status, you must enter a reason.");
				Parent.ReasonForContingencyReleaseInfo.AddError(message);
			}
		}

		#endregion
	}
}
