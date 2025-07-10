using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.Customs.Common.NZ;
using Enterprise.Customs.NZ.Business.Declaration.CustomsChargeCalculators;
using Enterprise.Customs.NZ.Business.Declaration.OutwardReport;
using Enterprise.Customs.NZ.Registry;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.NZ.Business
{
	public class ConsolCustomsCharges : ICustomsCharges, Integration.Customs.NZ.IConsolCustomsCharges
	{
		public ConsolCustomsCharges(ForwardingConsol consol)
		{
			manifestStatus = new OutwardReportManifestStatus(consol);
		}
		readonly OutwardReportManifestStatus manifestStatus;

		#region ICustomsCharges Members

		CustomsCharge[] ICustomsCharges.GetCustomsCharges(ILogger logger)
		{
			var chargeType = new EntryChargeTypeList()[EntryChargeTypeList.Codes.EntryFee];
			if (chargeType != null)
			{
				var chargeTypeSetting = chargeType.GetChargeTypeSpecificRegistrySettings();
				AccChargeCode accChargeCode = null;
				if (chargeTypeSetting != null)
				{
					var accChargeCodePK = chargeTypeSetting.AC_ChargeCode;
					if (!accChargeCodePK.IsEmpty)
					{
						accChargeCode = manifestStatus.Factory.Load<AccChargeCode>(accChargeCodePK);
					}
				}

				return new CustomsCharge[]
				{
					new CustomsCharge(accChargeCode, chargeType.Description, GetFee(), GetGST(), true, Enterprise.Registry.Business.RatingDataRegistry.Instance.CustomsDisbursementCreditor.Value)
				};
			}
			return System.Array.Empty<CustomsCharge>();
		}

		ZBool ICustomsCharges.IsActive
		{
			get { return manifestStatus.E2_MessageStatus != OutwardReportStatusList.Descriptions.NotSent.ToString(); }
		}

		#endregion

		ZDecimal GetFee()
		{
			var result = ZDecimal.Zero;

			var feeChargeCalculator = new EntryFeeCalculator.FeeChargeCalculator(manifestStatus.EDITransmitDate, manifestStatus.Factory);

			if (manifestStatus.Consol.JK_TransportMode == Core.Constants.TransportModes.Air)
			{
				result = feeChargeCalculator.OutwardReportTransactionFeeAir;
			}
			else if (manifestStatus.Consol.JK_TransportMode == Core.Constants.TransportModes.Sea)
			{
				result = feeChargeCalculator.OutwardReportTransactionFeeSea;
			}

			return Utilities.Round(result, 2);
		}

		ZDecimal GetGST()
		{
			return GSTCalculator.GetGST(manifestStatus.Factory, GetFee(), manifestStatus.EDITransmitDate);
		}
	}
}
