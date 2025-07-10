using System.Data;
using System.Diagnostics;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Integration.AWB;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.AWB.Business
{
	[DebuggerDisplay("EO_ChargeCode: {EO_ChargeCode}, EO_EntitlementCode: {EO_EntitlementCode}, EO_PPDCLT: {EO_PPDCLT}, EO_Amount: {EO_Amount}")]
	[DependentBusinessObject(typeof(ExportAWBHeader), "AWBOtherCharges")]
	public class ExportAWBOtherCharges : AutoExportAWBOtherCharges, IAWBOtherChargesMessageDetailsProvider
	{
		public ExportAWBOtherCharges(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		[List("PrepayCollectList")]
		public override ZString EO_PPDCLT
		{
			get { return base.EO_PPDCLT; }
			set { base.EO_PPDCLT = value; }
		}

		[DecimalPlaces(2)]
		public override ZDecimal EO_Amount
		{
			get { return base.EO_Amount; }
			set
			{
				base.EO_Amount = value;
				RefreshMaster();
			}
		}

		[List("EntitlementCodesList")]
		public override ZString EO_EntitlementCode
		{
			get { return base.EO_EntitlementCode; }
			set
			{
				base.EO_EntitlementCode = value;
				RefreshMaster();
			}
		}

		[List("IATAChargeCodesList")]
		public override ZString EO_ChargeCode
		{
			get { return base.EO_ChargeCode; }
			set
			{
				if (base.EO_ChargeCode != value && value != "")
				{
					SetEO_ChargeDescriptionSafely(IATAChargeCodesList.GetDescriptionFromCode(value));
				}

				base.EO_ChargeCode = value;

				ZString pPDCOL = GetChargePPDCOL();

				if (Master != null)
				{
					var displayOption = Master.GetDisplayOption(EO_ChargeCode, pPDCOL);
					EO_EntitlementCode = displayOption.Entitlement;
				}

				RefreshMaster();
			}
		}

		public ZString Currency { get; set; }

		public void SetEO_ChargeDescriptionSafely(ZString value)
		{
			EO_ChargeDescription = value.SubstringSafe(0, GetZPropertyInfo(ExportAWBOtherChargesSchema.Constants.EO_ChargeDescription).MaxLength);
		}

		protected virtual ZString GetChargePPDCOL()
		{
			return EO_PPDCLT;
		}

		public CodeDescriptionPairList IATAChargeCodesList
		{
			get { return iataChargeCodesList ?? (iataChargeCodesList = Factory.GetCachedCodeDescriptionPairList(OLookUpEditType.AWBChargeCodes)); }
		}
		CodeDescriptionPairList iataChargeCodesList;

		public CodeDescriptionPairList EntitlementCodesList
		{
			get { return new CodeDescriptionPairList(OLookUpEditType.AWBEntitlementCodes); }
		}

		public CodeDescriptionPairList PrepayCollectList
		{
			get
			{
				CodeDescriptionPairList list = new CodeDescriptionPairList();
				list.AddPair(ExportAWBHeader.Constants.PrepaidCollect3CharCodes.Prepaid, Res.GetString("af39fded-ec05-410b-bfae-7a4c6b4ecfc5", "Prepaid"));
				list.AddPair(ExportAWBHeader.Constants.PrepaidCollect3CharCodes.Collect, Res.GetString("7c6633bc-1d0c-4f9b-9e02-7af398c14b34", "Collect"));
				return list;
			}
		}

		void RefreshMaster()
		{
			if (Master != null)
			{
				Master.RefreshOtherChargesData();
			}
		}

		public ExportAWBHeader Master
		{
			get { return Factory.Load<ExportAWBHeader>(EO_EH); }
		}

		#region IAWBOtherChargesMessageDetailsProvider Members

		public ZString ChargeCode => EO_ChargeCode;

		public ZString EntitlementCode => EO_EntitlementCode;

		public ZDecimal Amount => EO_Amount;

		#endregion
	}
}
