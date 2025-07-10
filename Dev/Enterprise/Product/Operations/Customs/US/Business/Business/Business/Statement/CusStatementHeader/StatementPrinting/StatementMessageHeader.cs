using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.DocumentEngineIntegration;
using Enterprise.MasterFiles.Business;
using Constants = Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Constants;

namespace Enterprise.Customs.US.Business
{
	abstract class StatementMessageHeader : NonPersistentBusinessObject, IObsoleteValidation, IStatementForProvider, ISourceIdentifierProvider
	{
		protected StatementMessageHeader(CusStatementHeader header, MQEDIMessage message)
			: base(message.Factory)
		{
			this.message = message;
			this.header = header;
			ProcessMessageBlocks(out PreliminaryFees, out FinalFees);
		}
		protected readonly CusStatementHeader header;
		protected readonly MQEDIMessage message;
		protected IABIControlMessageBlockB BlockB
		{
			get { return message.MessageBlock.B as IABIControlMessageBlockB; }
		}
		protected readonly IEnumerable<KeyValuePair<ZString, ZDecimal>> PreliminaryFees;
		protected readonly IEnumerable<KeyValuePair<ZString, ZDecimal>> FinalFees;

		protected bool IsFinal
		{
			get
			{
				if (!fIsFinal.HasValue)
				{
					var blockB = BlockB;
					fIsFinal = blockB != null && blockB.StatementStatus == "F";
				}
				return fIsFinal.Value;
			}
		}
		bool? fIsFinal;

		#region Preliminary Figures

		public ZDecimal TotalDuty
		{
			get { return GetPreliminaryFee(Core.Constants.USCustoms.FeeCodes.Duty); }
		}

		public ZDecimal TotalPayableTax
		{
			get { return GetPreliminaryFee(Core.Constants.USCustoms.FeeCodes.ExciseTaxPayable); }
		}

		public ZDecimal TotalADD
		{
			get { return GetPreliminaryFee(Core.Constants.USCustoms.FeeCodes.AntidumpingDuty); }
		}

		public ZDecimal TotalCVD
		{
			get { return GetPreliminaryFee(Core.Constants.USCustoms.FeeCodes.CountervailingDuty); }
		}

		public ZDecimal TotalInterestAmountForReconciliationSummary
		{
			get { return GetPreliminaryFee(Core.Constants.USCustoms.FeeCodes.ReconciliationInterest); }
		}

		public ZDecimal TotalCottonFee
		{
			get { return GetPreliminaryFee(Core.Constants.USCustoms.FeeCodes.Cotton); }
		}

		public ZDecimal TotalSugarFee
		{
			get { return GetPreliminaryFee(Core.Constants.USCustoms.FeeCodes.Sugar); }
		}

		public ZDecimal TotalMailFee
		{
			get { return GetPreliminaryFee(Core.Constants.USCustoms.FeeCodes.DutiableMail); }
		}

		public ZDecimal TotalPorkFee
		{
			get { return GetPreliminaryFee(Core.Constants.USCustoms.FeeCodes.Pork); }
		}

		public ZDecimal TotalBeefFee
		{
			get { return GetPreliminaryFee(Core.Constants.USCustoms.FeeCodes.Beef); }
		}

		public ZDecimal TotalMerchandiseProcessingFee
		{
			get { return GetPreliminaryFee(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing); }
		}

		public ZDecimal TotalInformalMerchandiseProcessingFee
		{
			get { return GetPreliminaryFee(Core.Constants.USCustoms.FeeCodes.MerchandiseInformal); }
		}

		public ZDecimal TotalManualSurcharge
		{
			get { return GetPreliminaryFee(Core.Constants.USCustoms.FeeCodes.MerchandiseSurcharge); }
		}

		public ZDecimal TotalHoneyFee
		{
			get { return GetPreliminaryFee(Core.Constants.USCustoms.FeeCodes.Honey); }
		}

		public ZDecimal TotalHarborMaintenanceFeeWaterways
		{
			get { return GetPreliminaryFee(Core.Constants.USCustoms.FeeCodes.HMF); }
		}

		public ZDecimal TotalRaspberryFee
		{
			get { return GetPreliminaryFee(Core.Constants.USCustoms.FeeCodes.Raspberry); }
		}

		public ZDecimal TotalPotatoFee
		{
			get { return GetPreliminaryFee(Core.Constants.USCustoms.FeeCodes.Potato); }
		}

		public ZDecimal TotalLimeFee
		{
			get { return GetPreliminaryFee(Core.Constants.USCustoms.FeeCodes.FreshLimes); }
		}

		public ZDecimal TotalMushroomFee
		{
			get { return GetPreliminaryFee(Core.Constants.USCustoms.FeeCodes.Mushroom); }
		}

		public ZDecimal TotalWatermelonFee
		{
			get { return GetPreliminaryFee(Core.Constants.USCustoms.FeeCodes.Watermelon); }
		}

		public ZDecimal TotalSoftwoodLumberFee
		{
			get { return GetPreliminaryFee(Core.Constants.USCustoms.FeeCodes.SoftwoodLumber); }
		}

		public ZDecimal TotalBlueberryFee
		{
			get { return GetPreliminaryFee(Core.Constants.USCustoms.FeeCodes.Blueberry); }
		}

		public ZDecimal TotalHassAvocadoFee
		{
			get { return GetPreliminaryFee(Core.Constants.USCustoms.FeeCodes.Avocado); }
		}

		public ZDecimal TotalMangoFee
		{
			get { return GetPreliminaryFee(Core.Constants.USCustoms.FeeCodes.Mango); }
		}

		public ZDecimal TotalSorghumFee
		{
			get { return GetPreliminaryFee(Core.Constants.USCustoms.FeeCodes.Sorghum); }
		}

		public ZDecimal TotalDairyFee
		{
			get { return GetPreliminaryFee(Core.Constants.USCustoms.FeeCodes.DairyFee); }
		}

		public ZDecimal TotalDeferredTax
		{
			get { return GetPreliminaryFee(Core.Constants.USCustoms.FeeCodes.ExciseTaxDeferred); }
		}

		public ZDecimal TotalAmountDue
		{
			get { return GetPreliminaryFee(Constants.TotalAmountDue); }
		}

		protected ZDecimal GetPreliminaryFee(ZString feeCode)
		{
			return (from fee in PreliminaryFees
					where fee.Key == feeCode
					select fee.Value).FirstOrDefault();
		}

		#endregion

		#region Final Figures

		public ZDecimal FinalTotalDuty
		{
			get { return IsFinal ? GetFinalFee(Core.Constants.USCustoms.FeeCodes.Duty) : TotalDuty; }
		}

		public ZDecimal FinalTotalPayableTax
		{
			get { return IsFinal ? GetFinalFee(Core.Constants.USCustoms.FeeCodes.ExciseTaxPayable) : TotalPayableTax; }
		}

		public ZDecimal FinalTotalADD
		{
			get { return IsFinal ? GetFinalFee(Core.Constants.USCustoms.FeeCodes.AntidumpingDuty) : TotalADD; }
		}

		public ZDecimal FinalTotalCVD
		{
			get { return IsFinal ? GetFinalFee(Core.Constants.USCustoms.FeeCodes.CountervailingDuty) : TotalCVD; }
		}

		public ZDecimal FinalTotalInterestAmountForReconciliationSummary
		{
			get { return IsFinal ? GetFinalFee(Core.Constants.USCustoms.FeeCodes.ReconciliationInterest) : TotalInterestAmountForReconciliationSummary; }
		}

		public ZDecimal FinalTotalCottonFee
		{
			get { return IsFinal ? GetFinalFee(Core.Constants.USCustoms.FeeCodes.Cotton) : TotalCottonFee; }
		}

		public ZDecimal FinalTotalSugarFee
		{
			get { return IsFinal ? GetFinalFee(Core.Constants.USCustoms.FeeCodes.Sugar) : TotalSugarFee; }
		}

		public ZDecimal FinalTotalMailFee
		{
			get { return IsFinal ? GetFinalFee(Core.Constants.USCustoms.FeeCodes.DutiableMail) : TotalMailFee; }
		}

		public ZDecimal FinalTotalPorkFee
		{
			get { return IsFinal ? GetFinalFee(Core.Constants.USCustoms.FeeCodes.Pork) : TotalPorkFee; }
		}

		public ZDecimal FinalTotalBeefFee
		{
			get { return IsFinal ? GetFinalFee(Core.Constants.USCustoms.FeeCodes.Beef) : TotalBeefFee; }
		}

		public ZDecimal FinalTotalMerchandiseProcessingFee
		{
			get { return IsFinal ? GetFinalFee(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing) : TotalMerchandiseProcessingFee; }
		}

		public ZDecimal FinalTotalInformalMerchandiseProcessingFee
		{
			get { return IsFinal ? GetFinalFee(Core.Constants.USCustoms.FeeCodes.MerchandiseInformal) : TotalInformalMerchandiseProcessingFee; }
		}

		public ZDecimal FinalTotalManualSurcharge
		{
			get { return IsFinal ? GetFinalFee(Core.Constants.USCustoms.FeeCodes.MerchandiseSurcharge) : TotalManualSurcharge; }
		}

		public ZDecimal FinalTotalHoneyFee
		{
			get { return IsFinal ? GetFinalFee(Core.Constants.USCustoms.FeeCodes.Honey) : TotalHoneyFee; }
		}

		public ZDecimal FinalTotalHarborMaintenanceFeeWaterways
		{
			get { return IsFinal ? GetFinalFee(Core.Constants.USCustoms.FeeCodes.HMF) : TotalHarborMaintenanceFeeWaterways; }
		}

		public ZDecimal FinalTotalRaspberryFee
		{
			get { return IsFinal ? GetFinalFee(Core.Constants.USCustoms.FeeCodes.Raspberry) : TotalRaspberryFee; }
		}

		public ZDecimal FinalTotalPotatoFee
		{
			get { return IsFinal ? GetFinalFee(Core.Constants.USCustoms.FeeCodes.Potato) : TotalPotatoFee; }
		}

		public ZDecimal FinalTotalLimeFee
		{
			get { return IsFinal ? GetFinalFee(Core.Constants.USCustoms.FeeCodes.FreshLimes) : TotalLimeFee; }
		}

		public ZDecimal FinalTotalMushroomFee
		{
			get { return IsFinal ? GetFinalFee(Core.Constants.USCustoms.FeeCodes.Mushroom) : TotalMushroomFee; }
		}

		public ZDecimal FinalTotalWatermelonFee
		{
			get { return IsFinal ? GetFinalFee(Core.Constants.USCustoms.FeeCodes.Watermelon) : TotalWatermelonFee; }
		}

		public ZDecimal FinalTotalSoftwoodLumberFee
		{
			get { return IsFinal ? GetFinalFee(Core.Constants.USCustoms.FeeCodes.SoftwoodLumber) : TotalSoftwoodLumberFee; }
		}

		public ZDecimal FinalTotalBlueberryFee
		{
			get { return IsFinal ? GetFinalFee(Core.Constants.USCustoms.FeeCodes.Blueberry) : TotalBlueberryFee; }
		}

		public ZDecimal FinalTotalHassAvocadoFee
		{
			get { return IsFinal ? GetFinalFee(Core.Constants.USCustoms.FeeCodes.Avocado) : TotalHassAvocadoFee; }
		}

		public ZDecimal FinalTotalMangoFee
		{
			get { return IsFinal ? GetFinalFee(Core.Constants.USCustoms.FeeCodes.Mango) : TotalMangoFee; }
		}

		public ZDecimal FinalTotalSorghumFee
		{
			get { return IsFinal ? GetFinalFee(Core.Constants.USCustoms.FeeCodes.Sorghum) : TotalSorghumFee; }
		}

		public ZDecimal FinalTotalDairyFee
		{
			get { return IsFinal ? GetFinalFee(Core.Constants.USCustoms.FeeCodes.DairyFee) : TotalDairyFee; }
		}

		public ZDecimal FinalTotalDeferredTax
		{
			get { return IsFinal ? GetFinalFee(Core.Constants.USCustoms.FeeCodes.ExciseTaxDeferred) : TotalDeferredTax; }
		}

		public ZDecimal FinalTotalAmountDue
		{
			get { return IsFinal ? GetFinalFee(Constants.TotalAmountDue) : TotalAmountDue; }
		}

		protected ZDecimal GetFinalFee(ZString feeCode)
		{
			return (from fee in FinalFees
					where fee.Key == feeCode
					select fee.Value).FirstOrDefault();
		}

		#endregion

		#region Properties

		public OrgHeader Importer
		{
			get
			{
				var result = header?.Importer;
				if (result == null && BlockB != null)
				{
					var orgCusCode = BlockB.ImporterOfRecordNumber;
					if (!orgCusCode.IsEmpty)
					{
						result = new OrganizationLoader(Factory).LoadAllOrganisationByCusCode(orgCusCode).FirstOrDefault();
					}
				}

				return result;
			}
		}

		public ZString StatementFor
		{
			get { return this.GetStatementFor(); }
		}

		public ZDateTime B2_ProcessDate
		{
			get { return header != null ? header.B2_ProcessDate : ZDateTime.Empty; }
		}

		public ZDateTime B2_PrintDate => GetEffectiveValue((x) => x.B2_PrintDate, (y) => y.PreliminaryStatementPrintDate);

		public ZString B2_BranchDesignation => GetEffectiveValue((x) => x.B2_BranchDesignation, (y) => y.ClientBranchDesignation);

		public ZString B2_EntryFilerCode => GetEffectiveValue((x) => x.B2_EntryFilerCode, (y) => y.EntryFilerCode);

		public ZString B2_ProcessPort => GetEffectiveValue((x) => x.B2_ProcessPort, (y) => y.ProcessingDistrictPortCode);

		public ZString B2_ImporterCustomsID => GetEffectiveValue((x) => x.B2_ImporterCustomsID, (y) => y.ImporterOfRecordNumber);

		public T GetEffectiveValue<T>(Func<CusStatementHeader, T> getHeaderValue, Func<IABIControlMessageBlockB, T> getMessageValue) where T : IZType
		{
			var headerValue = header == null ? default(T) : getHeaderValue(header);
			var messageValue = BlockB == null ? default(T) : getMessageValue(BlockB);
			return (headerValue.IsDefault || headerValue.IsEmpty) ? messageValue : headerValue;
		}

		#endregion

		protected abstract void ProcessMessageBlocks(out IEnumerable<KeyValuePair<ZString, ZDecimal>> preliminaryFees,
			out IEnumerable<KeyValuePair<ZString, ZDecimal>> finalFees);

		#region IStatementForProvider Members

		public ZString B2_PaymentType => GetEffectiveValue((x) => x.B2_PaymentType, (y) => y.PaymentTypeIndicator);

		public GlbCompany Company
		{
			get { return message.Company; }
		}

		#endregion

		#region ISourceIdentifierProvider members

		ZGuid ISourceIdentifierProvider.SourceIdentifier => header?.PK ?? ZGuid.Empty;

		#endregion
	}
}
