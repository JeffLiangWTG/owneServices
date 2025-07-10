using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.NZ.TradeSingleWindow;

namespace Enterprise.Customs.NZ.Business.TradeSingleWindow
{
	public abstract class Consignment
	{
		protected Consignment(WriteOffResponse response, string id, string status, string movementStatus)
		{
			Response = Argument.NotNull(response, "response");
			ID = Argument.NotNull(id, "id");
			Status = Argument.NotNull(status, "status");
			ConsignmentMovementStatus = movementStatus;
		}

		public readonly string ID;

		public readonly string Status;

		public readonly string ConsignmentMovementStatus;

		public ZString CustomsStatus
		{
			get { return CustomsStatusCore; }
			set
			{
				CustomsStatusCore = value;
			}
		}

		public ZString MessageStatus
		{
			get { return MessageStatusCore; }
			set
			{
				MessageStatusCore = value;
			}
		}

		public string EnterpriseStatus
		{
			get { return enterpriseStatus ?? (enterpriseStatus = GetEnterpriseStatus()); }
		}

		public string EnterpriseStatusDescription
		{
			get { return EnterpriseStatus + "-" + Response.LowValueConsignmentStatusList.GetDescriptionFromCode(EnterpriseStatus); }
		}

		public string HouseBill
		{
			get { return HouseBillCore; }
		}

		public string JobNumber
		{
			get { return JobNumberCore; }
		}

		public bool IsFormalDeclarationRequired
		{
			get { return EnterpriseStatus == LowValueConsignmentStatusList.Codes.FormalDeclarationRequired; }
		}

		public bool IsHeld
		{
			get { return EnterpriseStatus == LowValueConsignmentStatusList.Codes.ConsignmentHeld; }
		}

		public bool IsInError
		{
			get { return EnterpriseStatus == LowValueConsignmentStatusList.Codes.ConsignmentInError; }
		}

		public bool IsWrittenOff
		{
			get { return EnterpriseStatus == LowValueConsignmentStatusList.Codes.ConsignmentWrittenOff; }
		}

		#region Implementation

		protected readonly WriteOffResponse Response;
		string enterpriseStatus;

		protected abstract ZString CustomsStatusCore { get; set; }

		protected abstract ZString MessageStatusCore { get; set; }

		protected abstract ZString HouseBillCore { get; }

		protected virtual string JobNumberCore
		{
			get { return "UNKNOWN CONSIGNMENT NUMBER"; }
		}

		public virtual string MovementStatusDescription
		{
			get
			{
				var result = ZString.Empty;
				if (!ConsignmentMovementStatus.IsNullOrEmpty())
				{
					result = ConsignmentMovementStatus + "-" + MovementStatusList.GetDescriptionFromCode(ConsignmentMovementStatus);
				}

				return result;
			}
		}

		MovementStatus MovementStatusList => new BusinessObjectFactory().GetCachedValue<MovementStatus>();

		protected virtual void LogCustomsImpediment()
		{
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]   // case statement
		string GetEnterpriseStatus()
		{
			if (Response.IsCancellation)
			{
				return LowValueConsignmentStatusList.Codes.ConsignmentCancelled;
			}
			else if (string.IsNullOrEmpty(Status))
			{
				switch (CustomsStatus)
				{
					case LowValueConsignmentStatusList.Codes.NoStatusReported:
					case LowValueConsignmentStatusList.Codes.ConsignmentInError:
					case LowValueConsignmentStatusList.Codes.FormalDeclarationRequired:
						return LowValueConsignmentStatusList.Codes.FormalDeclarationRequired;
					case LowValueConsignmentStatusList.Codes.ConsignmentHeld:
						return LowValueConsignmentStatusList.Codes.ConsignmentHeld;
					case LowValueConsignmentStatusList.Codes.ConsignmentWrittenOff:
						return LowValueConsignmentStatusList.Codes.ConsignmentWrittenOff;
					default:
						return LowValueConsignmentStatusList.Codes.NoStatusReported;
				}
			}
			else
			{
				switch (Status)
				{
					case ConsignmentGoodsStatusList.Codes.Error:
						return LowValueConsignmentStatusList.Codes.ConsignmentInError;
					case ConsignmentGoodsStatusList.Codes.ExportDeclarationRequired:
						return LowValueConsignmentStatusList.Codes.ExportDeclarationRequired;
					case ConsignmentGoodsStatusList.Codes.ImportDeclarationRequired:
						return LowValueConsignmentStatusList.Codes.ImportDeclarationRequired;
					case ConsignmentGoodsStatusList.Codes.MpiImportDeclarationRequired:
						return LowValueConsignmentStatusList.Codes.MpiImportDecRequired;
					case ConsignmentGoodsStatusList.Codes.Held:
						return LowValueConsignmentStatusList.Codes.ConsignmentHeld;
					case ConsignmentGoodsStatusList.Codes.InternationalTranshipmentApproved:
						return LowValueConsignmentStatusList.Codes.InternationalTranshipmentApproved;
					case ConsignmentGoodsStatusList.Codes.DomesticTranshipmentApproved:
						return LowValueConsignmentStatusList.Codes.DomesticTranshipmentApproved;
					case ConsignmentGoodsStatusList.Codes.InternationalTranshipmentDeclined:
						return LowValueConsignmentStatusList.Codes.InternationalTranshipmentDeclined;
					case ConsignmentGoodsStatusList.Codes.DomesticTranshipmentDeclined:
						return LowValueConsignmentStatusList.Codes.DomesticTranshipmentDeclined;
					case ConsignmentGoodsStatusList.Codes.RescindPreviousStatusNotification:
						return LowValueConsignmentStatusList.Codes.RescindPreviousStatusNotification;
					case ConsignmentGoodsStatusList.Codes.WrittenOffCleared:
						return LowValueConsignmentStatusList.Codes.ConsignmentWrittenOff;
					case ConsignmentGoodsStatusList.Codes.ConsolidationIcrRequired:
						return LowValueConsignmentStatusList.Codes.ConsolidationIcrRequired;
				}
			}

			return LowValueConsignmentStatusList.Codes.NoStatusReported;
		}

		#endregion // Implementation
	}
}

// Tested in
// - CREMessageProcessorMAWBTest
// - CREMessageProcessorDeclarationTest
// - CREMessageProcessorManifestingTest
// - ICRMessageProcessorConsolTest
// - ICRMessageProcessorMAWBTest
