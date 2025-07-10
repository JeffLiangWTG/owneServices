using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Freight.Forwarding.Business.AWB
{
	[DependentBusinessObject(typeof(ShipmentExportAWBHeader), "AWBOtherCharges")]
	public class ShipmentExportAWBOtherCharges : ExportAWBOtherCharges
	{
		public ShipmentExportAWBOtherCharges(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Default Values

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			EO_PPDCLT = ExportAWBHeader.Constants.PrepaidCollect3CharCodes.Collect;
		}

		#endregion

		#region Properties

		public override ZString EO_PPDCLT
		{
			get { return base.EO_PPDCLT; }
			set
			{
				base.EO_PPDCLT = value;
				RefreshMaster();
			}
		}

		#endregion

		#region Validation

		protected override Forwarding.AWB.Business.ExportAWBOtherChargesValidation GetNewValidation()
		{
			return new ShipmentExportAWBOtherChargesValidation(this);
		}

		#endregion

		#region Human Readable Name

		protected override ZString HumanReadableNameCore
		{
			get { return Res.GetString("89193ed9-dc9e-446a-bc59-4c1d87c47cc8", "House Air Waybill Other Charge"); }
		}

		#endregion
	}
}
