using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Environment;

namespace Enterprise.Freight.Forwarding.Business.AWB
{
	public class ShipmentExportAWBOtherChargesValidation : ExportAWBOtherChargesValidation
	{
		public ShipmentExportAWBOtherChargesValidation(ShipmentExportAWBOtherCharges parent) : base(parent)
		{
		}

		public new ShipmentExportAWBOtherCharges Parent
		{
			get { return (ShipmentExportAWBOtherCharges)base.Parent; }
		}

		protected override void CheckEO_PPDCLT()
		{
			base.CheckEO_PPDCLT();
			if (!Parent.EO_PPDCLTInfo.HasErrors() && !Parent.EO_PPDCLT.IsEmpty)
			{
				ListValidation.ErrorIfInvalidCode(Parent.EO_PPDCLTInfo, Parent.PrepayCollectList);
			}
		}

		protected override bool ShouldValidateMandatoryChargeCode
		{
			get { return Env.Registry.Freight.AirWaybill.ShowChargeCodeForOtherChargesInHAWBScreen; }
		}

		protected override void CheckValueRequiredInElectronicTransmissionIsNotEmpty(ZPropertyInfo propertyInfo)
		{
			MandatoryValidation.WarnIfNotEntered(propertyInfo);
		}

		protected override void CheckValueRequiredInElectronicTransmissionIsValid(ZPropertyInfo propertyInfo)
		{
			ListValidation.WarnIfInvalidCode(propertyInfo);
		}
	}
}
