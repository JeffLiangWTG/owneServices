using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.ZArchitecture.Web.Business.Utilities;

namespace Enterprise.Tracking.Web
{
	public class QuoteClientReplyRequestHelper : DataRequestHelper
	{
		public override string BaseUrl
		{
			get { return "QuoteClientReplyRequestHandler.axd"; }
		}

		public override bool EnableCache
		{
			get { return false; }
		}

		public override bool UseSecureQueryString
		{
			get { return true; }
		}

		public string GetHandlerUrlForClientAcceptance(ZGuid pk)
		{
			requestedReply = QuoteClientReplyRequestHandler.ClientReply.AcceptQuotation;
			return base.GetHandlerUrl(pk);
		}

		public string GetHandlerUrlForClientNonAcceptance(ZGuid pk)
		{
			requestedReply = QuoteClientReplyRequestHandler.ClientReply.RequestFurtherDiscussion;
			return base.GetHandlerUrl(pk);
		}

		protected override IEnumerable<KeyValuePair<string, string>> InsertAdditionalParameters(IEnumerable<KeyValuePair<string, string>> queryStringValues)
		{
			var key = nameof(QuoteClientReplyRequestHandler.ClientReply);
			string value;

			switch (requestedReply)
			{
				case QuoteClientReplyRequestHandler.ClientReply.AcceptQuotation:
					value = nameof(QuoteClientReplyRequestHandler.ClientReply.AcceptQuotation);
					return queryStringValues.Union(new KeyValuePair<string, string>[] { new KeyValuePair<string, string>(key, value) });

				case QuoteClientReplyRequestHandler.ClientReply.RequestFurtherDiscussion:
					value = nameof(QuoteClientReplyRequestHandler.ClientReply.RequestFurtherDiscussion);
					return queryStringValues.Union(new KeyValuePair<string, string>[] { new KeyValuePair<string, string>(key, value) });

				default:
					throw new InvalidOperationException("Expected either 'AcceptQuotation' or 'RequestFurtherDiscussion'. Got neither!");
			}
		}

		QuoteClientReplyRequestHandler.ClientReply requestedReply = QuoteClientReplyRequestHandler.ClientReply.None;
	}
}
