using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration.Customs.PermitService;
using Moq;

namespace Enterprise.Warehouse.Environment.Business.Testing
{
	public static class PermitServiceTestHelper
	{
		public static IDisposable MockServiceToGetPermits(SuccessOrFailure successOrFailure = SuccessOrFailure.Success)
		{
			return MockServiceToGetPermits(successOrFailure, successOrFailure);
		}

		public static IDisposable MockServiceToGetPermits(SuccessOrFailure successOrFailureForTryGetPermits, SuccessOrFailure successOrFailureForIsPermitAvailable)
		{
			return MockServiceToGetPermitsCore(null, successOrFailureForTryGetPermits, successOrFailureForIsPermitAvailable);
		}

		public static IDisposable MockServiceToGetPermits(IEnumerable<IPermitWithdrawRequestResponse> responsesReturned)
		{
			Argument.NotNull(responsesReturned, nameof(responsesReturned));
			return MockServiceToGetPermitsCore(responsesReturned, SuccessOrFailure.Success);
		}

		static IDisposable MockServiceToGetPermitsCore(IEnumerable<IPermitWithdrawRequestResponse> responsesReturned, SuccessOrFailure successOrFailureForBoth, SuccessOrFailure? successOrFailureForIsPermitAvailableOverride = null)
		{
			var successOrFailureForIsPermitAvailable = successOrFailureForIsPermitAvailableOverride ?? successOrFailureForBoth;

			IPermitWithdrawRequestResponseResult methodForIsPermitAvailable(IEnumerable<IPermitWithdrawRequest> args)
			{
				var permitRequests = args.ToArray();
				return new PermitWithdrawRequestResponseResultMock()
				{
					Success = successOrFailureForIsPermitAvailable == SuccessOrFailure.Success,
					Responses = responsesReturned?.ToArray() ?? GetResponsesBasedOnRequests(permitRequests, successOrFailureForIsPermitAvailable)
				};
			}

			IPermitWithdrawRequestResponseResult methodForTryGetPermits(IEnumerable<IPermitWithdrawRequest> args)
			{
				var permitRequests = args.ToArray();
				return new PermitWithdrawRequestResponseResultMock()
				{
					Success = successOrFailureForBoth == SuccessOrFailure.Success,
					Responses = responsesReturned?.ToArray() ?? GetResponsesBasedOnRequests(permitRequests, successOrFailureForBoth)
				};
			}

			var permitService = new Mock<IPermitService>();
			permitService.Setup(m => m.IsPermitAvailable(It.IsAny<IEnumerable<IPermitWithdrawRequest>>()))
				.Returns<IEnumerable<IPermitWithdrawRequest>>(methodForIsPermitAvailable);

			permitService.Setup(m => m.TryGetPermits(It.IsAny<IEnumerable<IPermitWithdrawRequest>>()))
				.Returns<IEnumerable<IPermitWithdrawRequest>>(methodForTryGetPermits);

			permitService.Setup(m => m.ConfirmPermitTransactions(It.IsAny<IEnumerable<IPermitWithdrawalRequestDetail>>()))
				.Returns(true);

			return ObjectFactory.Substitute(permitService.Object);
		}

		static PermitWithdrawRequestResponseMock[] GetResponsesBasedOnRequests(IPermitWithdrawRequest[] permitRequests, SuccessOrFailure successOrFailure)
		{
			return permitRequests.Select(r => new PermitWithdrawRequestResponseMock
			{
				Request = r,
				SuccessOrFailure = successOrFailure,
				FailureReason = successOrFailure == SuccessOrFailure.Success
					? ZString.Empty
					: r.DetailedTrackingEnabled ? new ZString($"Tariff: {r.Tariff}, Country of Origin: {r.CountryOfOrigin}, Zone Status: {r.CountryOfOrigin}, Manufacturer: {r.Manufacturer.OA_Code}") : new ZString($"Tariff: {r.Tariff}"),
				AvailableQty = 1m,
				OutwardEntryNumber = successOrFailure == SuccessOrFailure.Success ? r.PermitTransactionRefNumber : null,
			}).ToArray();
		}

		class PermitWithdrawRequestResponseMock : IPermitWithdrawRequestResponse
		{
			public IPermitWithdrawRequest Request { get; set; }

			public SuccessOrFailure SuccessOrFailure { get; set; }

			public ZString FailureReason { get; set; }

			public ZDecimal? AvailableQty { get; set; }

			public ZString? OutwardEntryNumber { get; set; }
		}

		class PermitWithdrawRequestResponseResultMock : IPermitWithdrawRequestResponseResult
		{
			public PermitWithdrawRequestResponseResultMock()
			{
				responses = new List<IPermitWithdrawRequestResponse>();
			}

			public bool Success { get; set; }

			public IEnumerable<IPermitWithdrawRequestResponse> Responses
			{
				get { return responses; }
				set
				{
					responses.Clear();
					if (value != null)
					{
						responses.AddRange(value);
					}
				}
			}
			readonly List<IPermitWithdrawRequestResponse> responses;
		}
	}
}
