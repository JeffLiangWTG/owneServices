using CargoWise.EntityFramework;

namespace Enterprise.Customs.TR.Manifest.Business
{
	public class AsycudaContainerValidation : ASYCUDA.Business.AsycudaContainerValidation
	{
		public AsycudaContainerValidation(AsycudaContainer parent)
			: base(parent)
		{
		}

		protected new AsycudaContainer Parent => (AsycudaContainer)base.Parent;

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateRelation();
		}

		public void ValidateRelation()
		{
			ValidateCalculatedProperty(Parent.RelationInfo);
		}

		protected void CheckRelation()
		{
			ListValidation.MessageErrorIfInvalidCode(Parent.RelationInfo);
		}

		protected override void MandatoryValidationOfACN_RC_ContainerTypeCore()
		{
		}
	}
}
