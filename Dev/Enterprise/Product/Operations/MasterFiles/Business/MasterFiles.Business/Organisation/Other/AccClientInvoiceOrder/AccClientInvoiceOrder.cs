using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class AccClientInvoiceOrder : AutoAccClientInvoiceOrder
	{
		public AccClientInvoiceOrder(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new class Schema : AutoAccClientInvoiceOrder.Schema
		{
			public const string InvoiceTypeDescription = "InvoiceTypeDescription";
		}

		#region ChargeCode

		public override AccChargeCode ChargeCode
		{
			get
			{
				if (fChargeCode == null || (!AI_AC.IsEmpty && fChargeCode.PK != AI_AC))
				{
					fChargeCode = Factory.Load<AccChargeCode>(AI_AC);
					if (fChargeCode != null &&
						!fChargeCode.MatchesFilter(((IBusinessObjectCollection)Lookups.ChargeCodes).CompleteFilter))
					{
						fChargeCode = null;
					}
				}
				return fChargeCode;
			}
		}
		AccChargeCode fChargeCode;

		#endregion

		#region Charge Description

		public ZString AC_Desc
		{
			get { return ChargeCode != null ? ChargeCode.AC_Desc : ZString.Empty; }
		}

		public virtual ZPropertyInfo AC_DescInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get
			{
				return GetZPropertyInfo(AccChargeCodeSchema.Constants.AC_Desc);
			}
		}

		#endregion

		#region Charge Base Sequence

		public ZShort AC_PrintSequence
		{
			get { return ChargeCode != null ? ChargeCode.AC_PrintSequence : ZShort.Zero; }
		}

		#endregion

		#region AI_AC
		[List("Lookups.ChargeCodes")]
		public override ZGuid AI_AC
		{
			get
			{
				return base.AI_AC;
			}
			set
			{
				if (base.AI_AC != value)
				{
					base.AI_AC = value;
					AC_DescInfo.RefreshBinding();
				}
			}
		}

		#endregion

		#region AI_InvoiceType

		[List("Lookups.InvoiceTypeList")]
		public override ZString AI_InvoiceType
		{
			get
			{
				return base.AI_InvoiceType;
			}
			set
			{
				base.AI_InvoiceType = value;
			}
		}
		#endregion

		#region InvoiceTypes

		[MaxLength(255)]
		public ZString InvoiceTypeDescription
		{
			get { return Lookups.InvoiceTypeList.GetDescriptionFromCode(AI_InvoiceType); }
		}

		public ZPropertyInfo InvoiceTypeDescriptionInfo
		{
			get { return GetZPropertyInfo(Schema.InvoiceTypeDescription); }
		}

		#endregion
	}
}
