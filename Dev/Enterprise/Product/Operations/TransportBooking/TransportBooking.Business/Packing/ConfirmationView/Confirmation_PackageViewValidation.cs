using System;
using CargoWise.EntityFramework;

namespace Enterprise.TransportBookings.Business
{
	public class Confirmation_PackageViewValidation : ZValidation
	{
		public Confirmation_PackageViewValidation(Confirmation_PackageView parent)
			: base(parent)
		{
			Parent = parent;
		}

		readonly Confirmation_PackageView Parent;

		public override void ValidateAll()
		{
			ValidateInstructionDivotPK();
		}

		public void ValidateInstructionDivotPK()
		{
			ValidateCalculatedProperty(Parent.InstructionDivotPKInfo);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "It is to address the unit test: Enterprise.TransportBookings.Business.Testing.Confirmation_PackageViewValidationTest.TestValidateParentID_InstructionDivot()")]
		void CheckInstructionDivotPK()
		{
			MandatoryValidation.CheckEntered(Parent.InstructionDivotPKInfo);
			ListValidation.ErrorIfInvalidPK(Parent.InstructionDivotPKInfo);
		}

		public override Type AutoValidationType
		{
			get { return typeof(Confirmation_PackageViewValidation); }
		}
	}
}
