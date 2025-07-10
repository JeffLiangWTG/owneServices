using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WTG.CreditCheck;
using WTG.ROPE.Model;

namespace Enterprise.MasterFiles.GUI.Tests
{
	class CreditCheckServiceForTest : ICreditCheckService
	{
		event EventHandler ICreditCheckService.OnCertificateMismatched { add { } remove { } }

		public Task<CompanyLookupResponse> CompanyLookupAsync(CompanyLookupRequest request)
		{
			throw new NotImplementedException();
		}

		public Task<CompanyLookupResponse> CompanyLookupAsync(CompanyLookupRequest request, CancellationToken cancellationToken)
		{
			if (request.CallingMode == CallingMode.Background)
			{
				if (request.Identifiers.Single().ID == "123")
				{
					return Task.FromResult(new CompanyLookupResponse() { ResultCode = ResultCode.Successful, CompanyItems = new ResponseCompanyItem[] { new ResponseCompanyItem() { Identifiers = new Identifier[] { new Identifier() { ID = "123456789", Type = IdentifierType.DUNS } } } } });
				}
			}

			return null;
		}

		public Task<CreditReportResponse> GetReportAsync(CreditReportRequest request)
		{
			throw new NotImplementedException();
		}

		public Task<CreditReportResponse> GetReportAsync(CreditReportRequest request, CancellationToken cancellationToken)
		{
			throw new NotImplementedException();
		}

		public Task<RetrospectiveMonitorResponse> GetRetrospectiveMonitorAsync(RetrospectiveMonitorRequest request)
		{
			RetrospectiveMonitorResponse result;

			if (request.Name == "Throw Exception")
			{
				throw new Exception("Error occur when try to get retrospective monitor");
			}

			if (string.IsNullOrEmpty(request.Name))
			{
				result = new RetrospectiveMonitorResponse() { Events = new List<CreditEvent>() };
			}
			else
			{
				result = new RetrospectiveMonitorResponse()
				{
					Events = DummyEvents
				};
			}

			return Task.FromResult(result);
		}

		public Task<RetrospectiveMonitorResponse> GetRetrospectiveMonitorAsync(RetrospectiveMonitorRequest request, CancellationToken cancellationToken)
		{
			throw new NotImplementedException();
		}

		public Task<TradeInformationResponse> SaveTradeInformationAsync(TradeInformationRequest request)
		{
			throw new NotImplementedException();
		}

		public Task<TradeInformationResponse> SaveTradeInformationAsync(TradeInformationRequest request, CancellationToken cancellationToken)
		{
			throw new NotImplementedException();
		}

		public Task<SubscriptionResponse> SubscribeAsync(SubscriptionRequest request)
		{
			throw new NotImplementedException();
		}

		public Task<SubscriptionResponse> SubscribeAsync(SubscriptionRequest request, CancellationToken cancellationToken)
		{
			throw new NotImplementedException();
		}

		public Task<SubscriptionResponse> UnsubscribeAsync(SubscriptionRequest request)
		{
			throw new NotImplementedException();
		}

		public Task<SubscriptionResponse> UnsubscribeAsync(SubscriptionRequest request, CancellationToken cancellationToken)
		{
			throw new NotImplementedException();
		}

		public IEnumerable<CreditEvent> DummyEvents { get; set; } = new List<CreditEvent>()
		{
			new CreditEvent() { EventDate = DateTime.Today.AddMonths(-3), Type = CreditEventType.CollectionChange },
			new CreditEvent() { EventDate = DateTime.Today.AddMonths(-5), Type = CreditEventType.StatusChange }
		};
	}
}
