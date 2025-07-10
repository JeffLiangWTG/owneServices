using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;

namespace Enterprise.Warehouse.Transactions.DataTransfer
{
	public class WDFEDIInterchange : EDIInterchange
	{
		public WDFEDIInterchange(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override ZString GetInterchangeNumber()
		{
			if (this.EI_InterchangeNum.IsEmpty)
			{
				throw new ArgumentException("Interchange number is mandatory.");
			}
			return this.EI_InterchangeNum;
		}

		protected override EDIInterchangeEDIMessageCollection GetNewContainedMessagesCollection() => new WDFEDIInterchangeWDFEDIMessageCollection(this, Factory);

		protected override Type GetMessageTypeToCreate(ZString messageText) => typeof(WDFEDIMessage);

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			EI_ApplicationCode = EDIInterchange.ApplicationCodes.WarehouseDocket;
			EI_InterchangeType = EI_ApplicationCode;
			EI_FooterText = ZString.Empty;
			EI_GB = GlbBranch.CurrentBranch.PK;
		}
	}
}
