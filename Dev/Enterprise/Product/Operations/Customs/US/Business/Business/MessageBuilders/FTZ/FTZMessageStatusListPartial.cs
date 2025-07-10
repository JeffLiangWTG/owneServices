using System.Collections.Generic;
using Enterprise.Customs.Common.US;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business
{
	partial class FTZMessageStatusList : IStatusList
	{
		#region IStatusList Members

		public IReadOnlyList<string> AcceptedStatusToCancelRejectStatusInterested
		{
			get
			{
				return new string[]
				{
					Codes.ClearFTZAdmissionAdd,
					Codes.ClearFTZAdmissionAddWithWarnings,
					Codes.ClearFTZAdmissionAmend,
					Codes.ClearFTZAdmissionDelete,
					Codes.ClearConcurrence,
					Codes.ClearDeliveryOfGoods,
					Codes.ClearGoodsArrival,
					Codes.ClearPermitToTransfer,
					Codes.PermitToTransferArrived,
					Codes.ClearUnconcurrence,
					Codes.PermitToTransferCancelAccepted,
					Codes.PermitToTransferUnArrived
				};
			}
		}

		public IReadOnlyList<string> GetFirstClearStatusFor(ImportMessageStatusList.MessageType messageType)
		{
			return new string[]
			{
				Codes.ClearFTZAdmissionAdd,
				Codes.ClearFTZAdmissionAddWithWarnings
			};
		}

		public bool IsArrivalExportBTATransmissionStatus(string status)
		{
			return false;
		}

		public bool IsPartialStatus(string status)
		{
			return false;
		}

		public bool IsStatusClear(string status)
		{
			return status == Codes.ClearFTZAdmissionAdd ||
					status == Codes.ClearFTZAdmissionAddWithWarnings ||
					status == Codes.ClearFTZAdmissionAmend ||
					status == Codes.ClearFTZAdmissionDelete ||
					status == Codes.ClearConcurrence ||
					status == Codes.ClearDeliveryOfGoods ||
					status == Codes.ClearGoodsArrival ||
					status == Codes.ClearPermitToTransfer ||
					status == Codes.PermitToTransferArrived ||
					status == Codes.ClearUnconcurrence ||
					status == Codes.PermitToTransferCancelAccepted ||
					status == Codes.PermitToTransferUnArrived;
		}

		public bool IsWaitingForResponse(string status)
		{
			return
				status == Codes.AwaitingFTZAdmissionAdd ||
				status == Codes.AwaitingFTZAdmissionAmend ||
				status == Codes.AwaitingFTZAdmissionDelete ||
				status == Codes.AwaitingConcurrence ||
				status == Codes.AwaitingDeliveryOfGoods ||
				status == Codes.AwaitingGoodsArrival ||
				status == Codes.AwaitingPermitToTransfer ||
				status == Codes.AwaitingPermitToTransferArrival ||
				status == Codes.AwaitingUnconcurrence ||
				status == Codes.AwaitingCancelPermitToTransfer ||
				status == Codes.AwaitingPermitToTransferUnArrival;
		}

		public IReadOnlyList<string> RejectStatusInterested
		{
			get
			{
				return new string[]
				{
					Codes.ErrorFTZAdmissionAdd,
					Codes.ErrorFTZAdmissionAmend,
					Codes.ErrorFTZAdmissionDelete,
					Codes.ErrorConcurrence,
					Codes.ErrorDeliveryOfGoods,
					Codes.ErrorGoodsArrival,
					Codes.ErrorPermitToTransfer,
					Codes.ErrorPermitToTransferArrival,
					Codes.ErrorUnconcurrence,
					Codes.PermitToTransferCancelUnauthorized,
					Codes.PermitToTransferArrivalCancelUnauthorized
				};
			}
		}

		bool IStatusList.IsWithdrawnStatus(string status)
		{
			return status == Codes.ClearFTZAdmissionDelete;
		}

		#endregion
	}

	public class FTZPTTMessageStatusList : CodeDescriptionPairList, DocumentEngine.RuntimeOptions.ICodeDescriptionPairListProvider
	{
		public FTZPTTMessageStatusList()
		{
			AddPair(FTZMessageStatusList.Codes.AwaitingCancelPermitToTransfer, FTZMessageStatusList.Descriptions.AwaitingCancelPermitToTransfer);
			AddPair(FTZMessageStatusList.Codes.AwaitingPermitToTransfer, FTZMessageStatusList.Descriptions.AwaitingPermitToTransfer);
			AddPair(FTZMessageStatusList.Codes.ClearPermitToTransfer, FTZMessageStatusList.Descriptions.ClearPermitToTransfer);
			AddPair(FTZMessageStatusList.Codes.ErrorPermitToTransfer, FTZMessageStatusList.Descriptions.ErrorPermitToTransfer);
			AddPair(FTZMessageStatusList.Codes.PermitToTransferCancelAccepted, FTZMessageStatusList.Descriptions.PermitToTransferCancelAccepted);
			AddPair(FTZMessageStatusList.Codes.PermitToTransferCancelUnauthorized, FTZMessageStatusList.Descriptions.PermitToTransferCancelUnauthorized);

			if (ZZCustomsFunctionality.IsAMSHBREffective)
			{
				AddPair(FTZMessageStatusList.Codes.AwaitingPermitToTransferArrival, FTZMessageStatusList.Descriptions.AwaitingPermitToTransferArrival);
				AddPair(FTZMessageStatusList.Codes.AwaitingPermitToTransferUnArrival, FTZMessageStatusList.Descriptions.AwaitingPermitToTransferUnArrival);
				AddPair(FTZMessageStatusList.Codes.PermitToTransferArrived, FTZMessageStatusList.Descriptions.PermitToTransferArrived);
				AddPair(FTZMessageStatusList.Codes.ErrorPermitToTransferArrival, FTZMessageStatusList.Descriptions.ErrorPermitToTransferArrival);
				AddPair(FTZMessageStatusList.Codes.PermitToTransferUnArrived, FTZMessageStatusList.Descriptions.PermitToTransferUnArrived);
				AddPair(FTZMessageStatusList.Codes.PermitToTransferArrivalCancelUnauthorized, FTZMessageStatusList.Descriptions.PermitToTransferArrivalCancelUnauthorized);
			}
		}

		ReadOnlyCodeDescriptionPairList DocumentEngine.RuntimeOptions.ICodeDescriptionPairListProvider.GetCodeDescriptionPairList()
		{
			return this;
		}
	}

	public class FTZAdmissionMessageStatusList : CodeDescriptionPairList, DocumentEngine.RuntimeOptions.ICodeDescriptionPairListProvider
	{
		public FTZAdmissionMessageStatusList()
		{
			AddPair(FTZMessageStatusList.Codes.AwaitingFTZAdmissionAdd, FTZMessageStatusList.Descriptions.AwaitingFTZAdmissionAdd);
			AddPair(FTZMessageStatusList.Codes.AwaitingFTZAdmissionDelete, FTZMessageStatusList.Descriptions.AwaitingFTZAdmissionDelete);
			AddPair(FTZMessageStatusList.Codes.AwaitingFTZAdmissionAmend, FTZMessageStatusList.Descriptions.AwaitingFTZAdmissionAmend);
			AddPair(FTZMessageStatusList.Codes.ClearFTZAdmissionAdd, FTZMessageStatusList.Descriptions.ClearFTZAdmissionAdd);
			AddPair(FTZMessageStatusList.Codes.ClearFTZAdmissionAddWithWarnings, FTZMessageStatusList.Descriptions.ClearFTZAdmissionAddWithWarnings);
			AddPair(FTZMessageStatusList.Codes.ClearFTZAdmissionAmend, FTZMessageStatusList.Descriptions.ClearFTZAdmissionAmend);
			AddPair(FTZMessageStatusList.Codes.ClearFTZAdmissionDelete, FTZMessageStatusList.Descriptions.ClearFTZAdmissionDelete);
			AddPair(FTZMessageStatusList.Codes.ErrorFTZAdmissionAdd, FTZMessageStatusList.Descriptions.ErrorFTZAdmissionAdd);
			AddPair(FTZMessageStatusList.Codes.ErrorFTZAdmissionAmend, FTZMessageStatusList.Descriptions.ErrorFTZAdmissionAmend);
			AddPair(FTZMessageStatusList.Codes.ErrorFTZAdmissionDelete, FTZMessageStatusList.Descriptions.ErrorFTZAdmissionDelete);
		}

		ReadOnlyCodeDescriptionPairList DocumentEngine.RuntimeOptions.ICodeDescriptionPairListProvider.GetCodeDescriptionPairList()
		{
			return this;
		}
	}

	public class FTZConcurrenceMessageStatusList : CodeDescriptionPairList, DocumentEngine.RuntimeOptions.ICodeDescriptionPairListProvider
	{
		public FTZConcurrenceMessageStatusList()
		{
			AddPair(FTZMessageStatusList.Codes.AwaitingConcurrence, FTZMessageStatusList.Descriptions.AwaitingConcurrence);
			AddPair(FTZMessageStatusList.Codes.ClearConcurrence, FTZMessageStatusList.Descriptions.ClearConcurrence);
			AddPair(FTZMessageStatusList.Codes.ErrorConcurrence, FTZMessageStatusList.Descriptions.ErrorConcurrence);
		}

		ReadOnlyCodeDescriptionPairList DocumentEngine.RuntimeOptions.ICodeDescriptionPairListProvider.GetCodeDescriptionPairList()
		{
			return this;
		}
	}

	public class FTZGoodsArrivalMessageStatusList : CodeDescriptionPairList, DocumentEngine.RuntimeOptions.ICodeDescriptionPairListProvider
	{
		public FTZGoodsArrivalMessageStatusList()
		{
			AddPair(FTZMessageStatusList.Codes.AwaitingGoodsArrival, FTZMessageStatusList.Descriptions.AwaitingGoodsArrival);
			AddPair(FTZMessageStatusList.Codes.ClearGoodsArrival, FTZMessageStatusList.Descriptions.ClearGoodsArrival);
			AddPair(FTZMessageStatusList.Codes.ErrorGoodsArrival, FTZMessageStatusList.Descriptions.ErrorGoodsArrival);
		}

		ReadOnlyCodeDescriptionPairList DocumentEngine.RuntimeOptions.ICodeDescriptionPairListProvider.GetCodeDescriptionPairList()
		{
			return this;
		}
	}

	public class FTZDeliveryOfGoodsMessageStatusList : CodeDescriptionPairList, DocumentEngine.RuntimeOptions.ICodeDescriptionPairListProvider
	{
		public FTZDeliveryOfGoodsMessageStatusList()
		{
			AddPair(FTZMessageStatusList.Codes.AwaitingDeliveryOfGoods, FTZMessageStatusList.Descriptions.AwaitingDeliveryOfGoods);
			AddPair(FTZMessageStatusList.Codes.ClearDeliveryOfGoods, FTZMessageStatusList.Descriptions.ClearDeliveryOfGoods);
			AddPair(FTZMessageStatusList.Codes.ErrorDeliveryOfGoods, FTZMessageStatusList.Descriptions.ErrorDeliveryOfGoods);
		}

		ReadOnlyCodeDescriptionPairList DocumentEngine.RuntimeOptions.ICodeDescriptionPairListProvider.GetCodeDescriptionPairList()
		{
			return this;
		}
	}
}
