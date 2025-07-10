using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EventReference;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.CN
{
	public class ETerminalReleaseMessageConsol : NonPersistentBusinessObject, IObsoleteValidation
	{
		public ETerminalReleaseMessageConsol(ForwardingConsol consol)
			: base(consol.Factory)
		{
			this.consol = Argument.NotNull(consol, nameof(consol));
		}

		readonly ForwardingConsol consol;

		public ForwardingConsol Consol => consol;

		[List("Consol.RefUNLOCO_List")]
		public ZString LoadPort => Consol.JK_RL_NKLoadPort;

		#region Send

		public ZBool Send
		{
			get => send;
			set
			{
				if (SetNonPersistentPropertyValue(SendInfo, ref send, value))
				{
				}
			}
		}
		ZBool send;

		public ZPropertyInfo SendInfo => GetZPropertyInfo(nameof(Send));

		#endregion

		#region Status

		public ZString Status
		{
			get
			{
				if (!status.HasValue)
				{
					status = GetStatus();
				}

				return status.Value;
			}
		}
		ZString? status;

		public ZPropertyInfo StatusInfo => GetZPropertyInfo(nameof(Status));

		public void RefreshStatus()
		{
			status = null;
			StatusInfo.RefreshBinding();
		}

		string GetStatus()
		{
			var status = NotSentStatus;
			var latestLog = GetEventLogsInDescendingOrder().FirstOrDefault();

			if (latestLog != null && latestLog.SL_SE_NKEvent != Events.StatusUpdatedCode)
			{
				status = latestLog.Event?.SE_DescMultilingual;
			}

			return status;
		}

		public string NotSentStatus = (NoResString)"Not Sent";  // non-translatable status

		IEnumerable<StmALog> GetEventLogsInDescendingOrder()
		{
			foreach (var log in consol.Logs.GetAllLogs().OfType<StmALog>().OrderByDescending(log => log.SL_PostedTimeUtc))
			{
				var messageType = log.Parameters.GetValueSafe(Constants.EventReferenceParameters.Codes.MessageType);

				if (string.Compare(messageType, ConsolDocumentNames.ETerminalReleaseManifest, StringComparison.OrdinalIgnoreCase) == 0)
				{
					yield return log;
				}
			}
		}

		#endregion
	}
}
