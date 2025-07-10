using System.Globalization;
using System.IO;
using System.Threading;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DataTransfer.Xml;
using Enterprise.Freight.Business;
using Enterprise.Freight.LocalCartage.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Freight.LocalCartage.DataTransfer
{
	public class CommonCartageXmlExporter
	{
		public CommonCartageXmlExporter(CommonCartageValueObjectDataAdapter adapter)
		{
			this.Adapter = adapter;
		}

		public void Export(ICartageExporter cartageExporter, CommonCartage cartage, TextWriter writer, INotifications notify, CancellationToken token)
		{
			ExportCore(cartageExporter, cartage, writer, null, notify, token);
		}

		public void Export(ICartageExporter cartageExporter, CommonCartage cartage, CartageXmlMessageDeliver processor, INotifications notify, CancellationToken token)
		{
			ExportCore(cartageExporter, cartage, null, processor, notify, token);
		}

		protected virtual void ExportCore(ICartageExporter cartageExporter, CommonCartage cartage, TextWriter writer, CartageXmlMessageDeliver processor, INotifications notify, CancellationToken token)
		{
			var buffer = new NotificationBuffer(notify);

			if (!buffer.HasErrors)
			{
				LogStatusUpdateEventCartageAdviseAndNotes(cartageExporter, cartage);

				try
				{
					cartageExporter.ParentFactory.Save();
				}
				catch (ZSaveConcurrencyException ex)
				{
					ZExceptionReporting.HandleZSaveConcurrencyException(ex, NotificationHandler.Instance);
					buffer.Notify(new ErrorNotification(ErrorType.Error, Res.GetString("6238d94c-8fcd-472b-b15d-a09958379ddb", "Please resolve concurrency issues before exporting to XML")));
				}
			}

			if (!buffer.HasErrors)
			{
				if (processor != null)
				{
					processor.Process(buffer.Inner, token);
				}
				else
				{
					ExportToXml(cartage, writer, buffer);
				}
			}

			if (!buffer.HasErrors)
			{
				try
				{
					cartageExporter.ParentFactory.Save();
				}
				catch (ZSaveConcurrencyException ex)
				{
					ZExceptionReporting.HandleZSaveConcurrencyException(ex, NotificationHandler.Instance);
				}
			}
		}

		void ExportToXml(CommonCartage cartage, TextWriter writer, NotificationBuffer buffer)
		{
			var interchangeSerialiser = new XmlValueObjectSerializer(typeof(Xsd.XmlInterchange));
			var interchange = Adapter.GetXMLIntechangeWithTargetType(cartage, buffer);

			interchangeSerialiser.Serialize(writer, interchange);
		}

		void LogStatusUpdateEventCartageAdviseAndNotes(ICartageExporter cartageExporter, CommonCartage cartage)
		{
			CommonCartageBookingInformation bookingInfo = cartage.BookingInformation;

			var ediStatusDescription = CommonCartageValueObjectDataAdapter.GetStatusDescription(bookingInfo.CalculatedBookingStatus, true);
			var orgDescription = CommonCartageValueObjectDataAdapter.GetOrganisationDescription(cartageExporter.SendTo);
			var reference = string.Format(CultureInfo.InvariantCulture, (NoResString)"{0} sent to {1}.", ediStatusDescription, orgDescription);

			var logs = cartageExporter.ParentLogs;
			logs.AddNew(Events.DataExport, GetCartageXmlExportReference(cartageExporter));
			logs.AddNew(Events.StatusUpdated, reference);

			StmNote note = FindOrCreateLocalCartageNote(cartageExporter.ParentNotes);
			var noteBuilder = new ZStringBuilder();
			if (!note.ST_NoteDataAsText.IsEmpty)
			{
				noteBuilder.Append(note.ST_NoteDataAsText);
				noteBuilder.Append("---");
			}
			noteBuilder.Append(CommonCartageValueObjectDataAdapter.GetTimeDesciption(ZDateTime.Now) + " - " + reference);
			noteBuilder.AppendIfNotEmpty(CommonCartageValueObjectDataAdapter.GetCommentDescription(bookingInfo.BookingComment));
			note.ST_NoteDataAsText = noteBuilder.ToStringWithNewLineBetweenAppends();

			if (bookingInfo.CalculatedBookingStatus == FreightConstants.LocalCartageBookingStatus.Codes.FirmBookingRequest)
			{
				cartageExporter.CartageAdvised(cartageExporter.ParentFactory);
			}
		}

		StmNote FindOrCreateLocalCartageNote(Notes notesCollection)
		{
			StmNote[] notes = notesCollection.FindByDescription(FreightConstants.LocalCartageNote);
			StmNote result = null;
			if (notes.Length == 0)
			{
				result = notesCollection.AddNew();
				result.ST_IsCustomDescription = ZBool.True;
				result.ST_Description = FreightConstants.LocalCartageNote;
			}
			else
			{
				result = notes[0];
			}
			return result;
		}

		protected virtual ZString GetCartageXmlExportReference(ICartageExporter cartageType)
		{
			return FreightConstants.CargoWiseOnePortTransportXMLFile;
		}

		readonly CommonCartageValueObjectDataAdapter Adapter;
	}
}
