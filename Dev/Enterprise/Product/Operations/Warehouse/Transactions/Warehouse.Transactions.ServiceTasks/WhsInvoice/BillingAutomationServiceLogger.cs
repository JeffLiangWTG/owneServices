using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Warehouse.Transactions.Invoicing;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Warehouse.Transactions.ServiceTasks
{
	public class BillingAutomationServiceLogger : IAutoRatingServiceLogger
	{
		public BillingAutomationServiceLogger(ILogger logger)
		{
			Logger = logger;
			NoteForInvoice = new ZStringBuilder();
		}

		#region IAutoRatingServiceLogger

		public void Error(string message) => LogAndAddToNote(LogType.Error, message);

		public void Information(string message) => LogAndAddToNote(LogType.Information, message);

		public void Log(LogType type, string message)
		{
			LogAndAddToNote(type, message);
		}

		public void Log(LogType type, string message, Exception ex)
		{
			LogAndAddToNote(type, message);
		}

		void LogAndAddToNote(LogType error, string message)
		{
			Logger.Log(error, message);
			AddToNote(error, message);
		}

		void AddToNote(LogType type, string message)
		{
			if (IsLoggingInvoiceInProgress)
			{
				NoteForInvoice.Append(type.ToString() + "|" + message);
			}
		}

		#endregion

		#region EnableAddingNoteWhileLogging

		public IDisposable EnableAddingNoteWhileLogging(ZGuid invoicePK)
		{
			InvoicePK = invoicePK;
			return new DisposableAction(StartLog, EndLog);
		}

		void EndLog()
		{
			SaveNoteToInvoice();
			IsLoggingInvoiceInProgress = false;
		}

		void StartLog()
		{
			IsLoggingInvoiceInProgress = true;
			NoteForInvoice.Clear();
		}

		#endregion

		#region ToString

		public override string ToString() => Logger.ToString();

		#endregion

		#region SaveNoteToInvoice

		void SaveNoteToInvoice()
		{
			var closingPart = EndingNoteMessage();
			NoteForInvoice.AppendLine(closingPart);
			var factory = new BusinessObjectFactory();
			var invoice = factory.Load<WhsInvoice>(InvoicePK);

			using (WhsInvoiceHelper.SetUserContextForInvoice(invoice))
			{
				var note = GetAutoRatingLogNoteAndPurgeUnnecessaryNotes(invoice.Notes);
				note.ST_NoteDataAsText = NoteForInvoice.ToStringWithNewLineBetweenAppends();
				factory.Save();
			}

			string EndingNoteMessage()
			{
				var endingNoteMsg = new ZStringBuilder();
				endingNoteMsg.AppendLine(Res.GetString("BC465F5E-9F59-4F2E-A5C2-D0708F0F0570", "Service task log"));
				endingNoteMsg.AppendLine(Res.GetString("C2EACBB6-2590-4089-97E7-A2AB40D1E4A8", "Time: {0}", ZDateTime.Now.ToLongTimeString()));
				return endingNoteMsg.ToStringWithNewLineBetweenAppends();
			}
		}

		#endregion

		#region Fields

		readonly ILogger Logger;

		protected ZStringBuilder NoteForInvoice;

		ZGuid InvoicePK;

		bool IsLoggingInvoiceInProgress;

		#endregion

		#region GetAutoRatingLogNoteAndPurgeUnnecessaryNotes

		// this is copied from /Enterprise/Product/Operations/Accounting/Business/Rating/AutoRatingRunner.cs need to be fixed
		StmNote GetAutoRatingLogNoteAndPurgeUnnecessaryNotes(Notes notes)
		{
			StmNote note = null;
			notes.FindByDescription(PredefinedNoteTypes.Instance.AutoRatingAuditLog.Description)
				.Where(x => x.IsBelongingToCurrentLoginCompany)
				.ForEach(x =>
				{
					if (note == null && x.ST_GC_RelatedCompany != ZGuid.Empty)
					{
						note = x;
					}
					else
					{
						x.Delete();
					}
				});

			if (note == null)
			{
				note = notes.AddNew();
				using (note.GetValidationSuspender())
				{
					note.ST_Description = PredefinedNoteTypes.Instance.AutoRatingAuditLog.Description;
					note.ST_GC_RelatedCompany = Env.CurrentCompany.PK;
					note.ST_IsCustomDescription = false;
				}
			}

			return note;
		}

		#endregion
	}
}
