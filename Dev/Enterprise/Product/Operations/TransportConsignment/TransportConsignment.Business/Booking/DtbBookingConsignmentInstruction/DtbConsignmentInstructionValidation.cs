using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.TransportCommon.Business;

namespace Enterprise.TransportConsignment.Business
{
	public class DtbConsignmentInstructionValidation : DtbTransportInstructionValidation
	{
		public DtbConsignmentInstructionValidation(DtbConsignmentInstruction parent)
			: base(parent)
		{
		}

		// non-persistent

		#region ValidateReqFrom

		public void ValidateReqFrom()
		{
			ValidateCalculatedProperty(Parent.ReqFromInfo);
		}

		protected void CheckReqFrom()
		{
			CheckAddressExists(Parent.ReqFromInfo);
		}

		#endregion

		#region ValidateReqTo

		public void ValidateReqTo()
		{
			ValidateCalculatedProperty(Parent.ReqToInfo);
		}

		protected void CheckReqTo()
		{
			CheckAddressExists(Parent.ReqToInfo);
		}

		#endregion

		// persistent

		#region CheckKN_DropMode

		protected override void CheckKN_DropMode()
		{
			base.CheckKN_DropMode();
			CheckAddressExists(Parent.KN_DropModeInfo);
		}

		#endregion

		#region CheckKN_IsAuthorisedToLeave

		protected override void CheckKN_IsAuthorisedToLeave()
		{
			base.CheckKN_IsAuthorisedToLeave();
			CheckAddressExists(Parent.KN_IsAuthorisedToLeaveInfo);
		}

		#endregion

		#region CheckKN_ServiceInstruction

		protected override void CheckKN_ServiceInstruction()
		{
			base.CheckKN_ServiceInstruction();
			CheckAddressExists(Parent.KN_ServiceInstructionInfo);
		}

		void CheckAddressExists(ZPropertyInfo info)
		{
			if (Parent.IsDelivery && !info.HasErrors() && !info.Value.IsDefault && !Parent.Address.IsValidAddress)
			{
				info.AddError(Res.GetString("a5954502-354d-4604-869d-e2ba8aa08a5b", "There is no Address entered for Delivery."));
			}
		}

		#endregion

		new DtbConsignmentInstruction Parent
		{
			get { return (DtbConsignmentInstruction)base.Parent; }
		}
	}
}
