using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using System.Xml.Schema;
using System.Reflection;
using System.Xml;
using System.Xml.Serialization;
using Microsoft.XLANGs.Core;
using System.Collections;
using CargoWise.eHub.Core.Logging;

namespace CargoWise.eHub.Products.SGCustoms.MHAccess.Orchestrations.Helpers
{
	public static class ExceptionHandler
	{
		public static void HandleEdifactLoopbackException(Exception exception, string senderID, string recipientID, string messageTrackingID, ref ArrayList outboxMessageTrackingIDArrayList)
		{
			var isExceptionHandled = false;
			var error = exception.ToString();

			if (exception is XlangSoapException)
			{
				isExceptionHandled = true;
				var errorMessageTrackingIDArrayList = new ArrayList();
				errorMessageTrackingIDArrayList.Add(messageTrackingID);
				FailMessages(senderID, recipientID, error, errorMessageTrackingIDArrayList);
				outboxMessageTrackingIDArrayList.Remove(messageTrackingID);
			}

			if (!isExceptionHandled)
			{
				throw new Exception(string.Format("Unhandled Edifact Loopback Exception(TrackingID: '{0}')", messageTrackingID), exception);
			}
		}

		public static Action<string, string, string, ArrayList> FailMessages = (string senderID, string recipientID, string error, ArrayList errorMessageTrackingIDArrayList) =>
		{
			eHubTransactionsContextAccessor.FailMessages(senderID, recipientID, errorMessageTrackingIDArrayList, error);
		};
	}
}