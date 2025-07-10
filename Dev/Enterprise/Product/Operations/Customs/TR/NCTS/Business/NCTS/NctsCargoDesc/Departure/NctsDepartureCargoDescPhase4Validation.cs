using CargoWise.EntityFramework;

namespace Enterprise.Customs.TR.NCTS.Business
{
	public class NctsDepartureCargoDescPhase4Validation : EU.NCTS.Business.NctsDepartureCargoDescPhase4Validation, INctsDepartureCargoDescValidation
	{
		public NctsDepartureCargoDescPhase4Validation(NctsDepartureCargoDesc parent) : base(parent)
		{
		}

		protected new NctsDepartureCargoDesc Parent => (NctsDepartureCargoDesc)base.Parent;

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateExportDeclarationType();
		}

		public void ValidateExportDeclarationType()
		{
			ValidateCalculatedProperty(Parent.ExportDeclarationTypeInfo);
		}

		protected void CheckExportDeclarationType()
		{
			if (!Parent.ExportDeclarationNumber.IsEmpty)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.ExportDeclarationTypeInfo);
			}
		}
	}
}
