
namespace Enterprise.Freight.Agency.Business
{
	using CargoWise.EntityFramework;

	public class PortMessageValidation : AutoPortMessageValidation
	{
		public PortMessageValidation(AutoPortMessage parent)
			: base(parent) { }

		protected override void CheckDirection()
		{
			base.CheckDirection();
			MandatoryValidation.CheckEntered(Parent.DirectionInfo);
			ListValidation.ErrorIfInvalidCode(Parent.DirectionInfo);
		}

		protected override void CheckPort()
		{
			base.CheckPort();
			MandatoryValidation.CheckEntered(Parent.PortInfo);
			ListValidation.ErrorIfInvalidCode(Parent.PortInfo);
		}

		#region Implementation

		public new PortMessage Parent
		{
			get { return (PortMessage)base.Parent; }
		}

		#endregion
	}
}
