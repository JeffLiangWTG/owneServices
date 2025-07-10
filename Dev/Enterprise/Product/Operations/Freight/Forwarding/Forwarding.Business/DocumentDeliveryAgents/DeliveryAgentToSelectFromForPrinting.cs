using System.ComponentModel;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Forwarding.Business
{
	public class DeliveryAgentToSelectFromForPrinting : DeliveryAgentOrgHeader
	{
		public abstract new class Schema : OrgHeader.Schema
		{
			public const string OH_Calc_PrintDocumentForDeliveryAgent = "OH_Calc_PrintDocumentForDeliveryAgent";
		}

		public DeliveryAgentToSelectFromForPrinting(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
			SetDefaultValueForPrintDocumentFlag();
		}

		#region OH_Calc_PrintDocumentForDeliveryAgent

		protected ZBool fOH_Calc_PrintDocumentForDeliveryAgent;

		public ZBool OH_Calc_PrintDocumentForDeliveryAgent
		{
			get { return fOH_Calc_PrintDocumentForDeliveryAgent; }
			set
			{
				SetNonPersistentPropertyValue(OH_Calc_PrintDocumentForDeliveryAgentInfo, ref fOH_Calc_PrintDocumentForDeliveryAgent, value);
			}
		}

		public ZPropertyInfo OH_Calc_PrintDocumentForDeliveryAgentInfo
		{
			get { return GetZPropertyInfo(Schema.OH_Calc_PrintDocumentForDeliveryAgent); }
		}

		#endregion

		protected void SetDefaultValueForPrintDocumentFlag()
		{
			OH_Calc_PrintDocumentForDeliveryAgent = ZBool.True;
		}

		[ReadOnly(true)]
		public override ZString OH_Code
		{
			get { return base.OH_Code; }
			set { base.OH_Code = value; }
		}

		[ReadOnly(true)]
		public override ZString OH_FullName
		{
			get { return base.OH_FullName; }
			set { base.OH_FullName = value; }
		}
	}
}
