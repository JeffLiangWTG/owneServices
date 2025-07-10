using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Integration.AWB;

namespace Enterprise.Freight.Forwarding.AWB.Business
{
	[DependentBusinessObject(typeof(ExportAWBHeader), "AWBAccountingInformations")]
	public class ExportAWBAccountingInformation : AutoExportAWBAccountingInformation, IAWBAccountingInformationMessageDetailsProvider
	{
		public ExportAWBAccountingInformation(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		[List("Lookups.AccountingCodes")]
		public sealed override ZString EA_InformationID
		{
			get { return base.EA_InformationID; }
			set { base.EA_InformationID = value; }
		}

		public virtual bool IsSkippedOnMessaging
		{
			get { return false; }
		}

		#region IAWBAccountingInformationMessageDetailsProvider Members

		public ZString InformationID => EA_InformationID;

		public ZString Information => EA_Information;

		#endregion
	}
}
