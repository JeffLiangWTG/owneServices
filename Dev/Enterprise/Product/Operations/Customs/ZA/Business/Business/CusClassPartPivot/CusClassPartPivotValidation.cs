using CargoWise.EntityFramework;

namespace Enterprise.Customs.ZA.Business
{
	public partial class CusClassPartPivotValidation : AutoZACusClassPartPivotValidation
	{
		public CusClassPartPivotValidation(CusClassPartPivot parent) : base(parent)
		{
		}

		public new CusClassPartPivot Parent
		{
			get { return (CusClassPartPivot)base.Parent; }
		}

		#region CheckCI_TariffNum

		protected override void CheckCI_TariffNum()
		{
			base.CheckCI_TariffNum();
			var tariff = Parent.CI_TariffNum;
			if (!tariff.IsEmpty)
			{
				var cusTariff = Parent.UniversalTariff;
				if (cusTariff == null)
				{
					Parent.CI_TariffNumInfo.AddMessageError(ValidationConstants.InvoiceLine.Schedule1Part1TariffDoesNotExists(tariff));
				}
			}
		}

		#endregion

		protected override void CheckCI_PrimaryPreference()
		{
			base.CheckCI_PrimaryPreference();
			ListValidation.MessageErrorIfInvalidCode(Parent.CI_PrimaryPreferenceInfo);
		}

		protected override void CheckCI_VehicleFormat()
		{
			base.CheckCI_VehicleFormat();
			ListValidation.MessageErrorIfInvalidCode(Parent.CI_VehicleFormatInfo);
		}

		protected override void CheckCI_VehicleType()
		{
			base.CheckCI_VehicleType();
			ListValidation.MessageErrorIfInvalidCode(Parent.CI_VehicleTypeInfo);
		}

		protected override void CheckCI_NewUsed()
		{
			base.CheckCI_NewUsed();
			ListValidation.MessageErrorIfInvalidCode(Parent.CI_NewUsedInfo);
		}
	}
}
