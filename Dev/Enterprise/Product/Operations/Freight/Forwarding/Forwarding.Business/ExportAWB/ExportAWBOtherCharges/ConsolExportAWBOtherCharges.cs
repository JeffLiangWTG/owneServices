using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Freight.Forwarding.Business.AWB
{
	[DependentBusinessObject(typeof(ConsolExportAWBHeader), "AWBOtherCharges")]
	public class ConsolExportAWBOtherCharges : ExportAWBOtherCharges
	{
		public ConsolExportAWBOtherCharges(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		#region Default Values

		protected override ZString HumanReadableNameCore
		{
			get { return Res.GetString("c56fc105-a3a5-4f04-99c1-dba486de6aaa", "Master Air Waybill Other Charge"); }
		}

		protected override ZString GetChargePPDCOL()
		{
			if (Master != null && Master.Consol != null)
			{
				return Master.Consol.JK_PrepaidCollect;
			}
			else
			{
				return "";
			}
		}

		#endregion

		#region Validation

		protected override Forwarding.AWB.Business.ExportAWBOtherChargesValidation GetNewValidation()
		{
			return new ConsolExportAWBOtherChargesValidation(this);
		}

		#endregion
	}
}
