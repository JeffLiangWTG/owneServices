using System.Data;

using CargoWise.EntityFramework;

namespace Enterprise.Freight.Forwarding.Business.AWB
{
	[DependentBusinessObject(typeof(ConsolExportAWBHeader), "AWBAccountingInformations")]
	public class ConsolExportAWBAccountingInformation : ExportAWBAccountingInformation
	{
		public ConsolExportAWBAccountingInformation(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Validation

		protected override Forwarding.AWB.Business.ExportAWBAccountingInformationValidation GetNewValidation()
		{
			return new ConsolExportAWBAccountingInformationValidation(this);
		}

		#endregion
	}
}
