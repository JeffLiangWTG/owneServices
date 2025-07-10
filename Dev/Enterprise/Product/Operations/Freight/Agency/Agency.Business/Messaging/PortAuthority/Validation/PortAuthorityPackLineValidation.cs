using CargoWise.EntityFramework;
using Enterprise.Core;
using Enterprise.Freight.Common.Business;

namespace Enterprise.Freight.Agency.Business
{
	public class PortAuthorityPackLineValidation : JobPackLinesValidation
	{
		public PortAuthorityPackLineValidation(BillOfLadingPackLine packline)
			: base(packline) { }

		public void ValidateJL_JC()
		{
			ValidateCalculatedProperty(Parent.JL_JCInfo);
		}

		protected void CheckJL_JC()
		{
			if (Parent.JL_JC.IsEmpty && IsContainerised && Parent.JL_MarksAndNumbers.IsEmpty)
			{
				Parent.JL_JCInfo.AddMessageError(Res.GetString("bee68822-aad9-49bf-be29-ec1a532b2552", "All packlines need to either be packed or have marks and numbers entered."));
			}
		}

		protected override void CheckJL_MarksAndNumbers()
		{
			base.CheckJL_MarksAndNumbers();

			if (Parent.JL_JC.IsEmpty && Parent.JL_MarksAndNumbers.IsEmpty)
			{
				if (IsContainerised)
				{
					Parent.JL_MarksAndNumbersInfo.AddMessageError(Res.GetString("bee68822-aad9-49bf-be29-ec1a532b2552", "All packlines need to either be packed or have marks and numbers entered."));
				}
				else
				{
					Parent.JL_MarksAndNumbersInfo.AddMessageError(Res.GetString("a746352c-7be3-42b0-9e16-876dae15cc82", "Marks and numbers are needed for port authority messaging."));
				}
			}
		}

		protected override void CheckJL_DetailedDescription()
		{
			base.CheckJL_DetailedDescription();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.JL_DetailedDescriptionInfo);
		}

		#region Implementation

		new BillOfLadingPackLine Parent
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return (BillOfLadingPackLine)base.Parent; }
		}

		bool IsContainerised
		{
			get { return Parent.Shipment != null && Parent.Shipment.JS_PackingMode == Constants.ContainerModes.FCL; }
		}

		#endregion
	}
}


