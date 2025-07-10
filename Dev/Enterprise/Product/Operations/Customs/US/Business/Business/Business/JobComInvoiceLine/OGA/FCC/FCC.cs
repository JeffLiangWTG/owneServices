using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Business.MessageBuilders;

namespace Enterprise.Customs.US.Business
{
	public class FCC : AutoFCC,
		IFCC
	{
		public FCC(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Related Objects

		public JobComInvoiceLine InvoiceLine
		{
			get { return Factory.Load<JobComInvoiceLine>(B7_ParentID); }
		}

		#endregion

		#region Overrides

		protected override BusinessObject CloneInternal(BusinessObjectCloneArgs args)
		{
			updateAddInfoProperties();
			FCC result = (FCC)base.CloneInternal(args);
			return result;
		}

		[DecimalPlaces(0)]
		public override ZDecimal US_FCCQty
		{
			get { return base.US_FCCQty; }
			set { base.US_FCCQty = value; }
		}

		#endregion

		#region IFCC Members

		ZString IFCC.ImportConditionNumber
		{
			get { return US_FCCImpCondNo; }
		}

		ZBool IFCC.ImportConditionNumberQuantityApproval
		{
			get { return US_FCCImpCondNoQtyAppr; }
		}

		ZInt IFCC.FCCLineNumber
		{
			get { return US_FCCLineNo; }
			set { US_FCCLineNo = value; }
		}

		ZString IFCC.FCCIdentifier
		{
			get { return US_FCCID; }
		}

		ZString IFCC.TradeName
		{
			get { return US_FCCTradeName; }
		}

		ZString IFCC.ModelTypeNumber
		{
			get { return US_FCCModel; }
		}

		ZDecimal IFCC.FCCQuantity
		{
			get { return US_FCCQty; }
		}

		ZBool IFCC.WithholdFromPublicInspectionRequested
		{
			get { return US_FCCWithhold; }
		}

		ZString IFCC.CommercialDescription
		{
			get { return US_FCCCommercialDesc; }
		}

		ZString IOGALine.CommercialDesc
		{
			get { return US_FCCCommercialDesc; }
			set { US_FCCCommercialDesc = value; }
		}

		#endregion

		public void UpdateAddInfoProperties()
		{
			updateAddInfoProperties();
		}
	}
}
