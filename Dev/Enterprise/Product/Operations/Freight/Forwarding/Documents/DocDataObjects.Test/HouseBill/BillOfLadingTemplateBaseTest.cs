using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.Documents.Testing.HouseBill
{
	abstract class BillOfLadingTemplateBaseTest : DocumentVisualizer.Testing.BillOfLadingContentTest
	{
		public abstract ForwardingShipment GetNewShipment();

		public abstract string TemplateName { get; }

		protected IHouseBillTemplate LoadBillOfLadingTemplate(string name)
		{
			var query = new ZQuery(StmTemplateSchema.SO_Name, name);
			query.AddToFilter(StmTemplateSchema.SO_TemplateType, StmTemplateTypes.Codes.Form);

			return LoadTemplate(query);
		}

		protected JobCharge CreateLineCharge(JobHeader header, ZGuid sellAccountPk, ZDecimal osSellAmount, ZString chargeCode, ZString currencyCode, ZDateTime? systemLastEditTimeUtc = null, ZShort displaySequence = default)
		{
			var query = new ZQuery(AccChargeCodeSchema.AC_Code, SQLComparisonOperator.Equal, chargeCode);
			query.AddToFilter(AccChargeCodeSchema.AC_GC, SQLComparisonOperator.Equal, header.JH_GC);

			var accChargeCode = Factory.LoadTop1<AccChargeCode>(query);

			AssertNotNull($"prerequisite: charge code '{chargeCode}' was found", accChargeCode);

			var lineCharge = Factory.New<JobCharge>();
			lineCharge.JR_JH = header.PK;
			lineCharge.JR_GE = header.JH_GE;
			lineCharge.JR_GB = header.JH_GB;
			lineCharge.JR_AC = accChargeCode.PK;
			lineCharge.JR_OH_SellAccount = sellAccountPk;
			lineCharge.JR_RX_NKSellCurrency = currencyCode;
			lineCharge.JR_OSSellAmt = osSellAmount;
			lineCharge.JR_Desc = accChargeCode.AC_Desc;
			lineCharge.JR_DisplaySequence = displaySequence;
			lineCharge.JR_SystemLastEditTimeUtc = systemLastEditTimeUtc ?? ZDateTime.Empty;

			return lineCharge;
		}
	}
}
