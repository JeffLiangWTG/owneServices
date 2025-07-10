using CargoWise.EntityFramework;

namespace Enterprise.Freight.Agency.Business
{
	public class DangerousGoodsManifestMessageValidation : AutoDangerousGoodsManifestMessageValidation
	{
		public DangerousGoodsManifestMessageValidation(AutoDangerousGoodsManifestMessage parent) : base(parent)
		{
		}

		protected override void CheckPort()
		{
			base.CheckPort();

			MandatoryValidation.CheckEntered(Parent.PortInfo);
			ListValidation.ErrorIfInvalidCode(Parent.PortInfo);
		}

		protected override void CheckPrincipalPK()
		{
			base.CheckPrincipalPK();

			MandatoryValidation.CheckEntered(Parent.PrincipalPKInfo);
			ListValidation.ErrorIfInvalidPK(Parent.PrincipalPKInfo);
		}

		protected override void CheckDirection()
		{
			base.CheckDirection();

			MandatoryValidation.CheckEntered(Parent.DirectionInfo);
			ListValidation.ErrorIfInvalidCode(Parent.DirectionInfo);
		}

		protected override void CheckMessageType()
		{
			base.CheckMessageType();

			MandatoryValidation.CheckEntered(Parent.MessageTypeInfo);
			ListValidation.ErrorIfInvalidCode(Parent.MessageTypeInfo);
		}

		#region Implementation

		public new DangerousGoodsManifestMessage Parent
		{
			get { return (DangerousGoodsManifestMessage)base.Parent; }
		}

		#endregion
	}
}
