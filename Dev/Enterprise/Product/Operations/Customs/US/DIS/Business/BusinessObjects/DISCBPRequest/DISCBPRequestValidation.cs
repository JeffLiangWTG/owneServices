using CargoWise.EntityFramework;

namespace Enterprise.Customs.US.DIS.Business
{
	public class DISCBPRequestValidation : AutoDISCBPRequestValidation
	{
		public DISCBPRequestValidation(AutoDISCBPRequest bizObj)
			: base(bizObj)
		{
		}

		new DISCBPRequest Parent
		{
			get { return (DISCBPRequest)base.Parent; }
		}

		protected override void CheckID()
		{
			base.CheckID();

			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.IDInfo);
		}

		protected override void CheckType()
		{
			base.CheckType();

			ListValidation.MessageErrorIfInvalidCode(Parent.TypeInfo);
		}
	}
}
