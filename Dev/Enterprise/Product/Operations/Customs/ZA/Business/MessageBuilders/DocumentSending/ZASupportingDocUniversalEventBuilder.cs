using System.Collections.Generic;
using System.Linq;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Customs.Business.MessageBuilders;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Customs.ZA.Business.MessageBuilders.DocumentSending
{
	public class ZASupportingDocUniversalEventBuilder : SupportingDocUniversalEventBuilder
	{
		public ZASupportingDocUniversalEventBuilder(ISupportingDocumentMessageDataProvider dataWrapper) : base(dataWrapper)
		{
		}

		public override IEnumerable<UniversalEvent> BuildUniversalEvent(ISupportingDocumentMessageDataProvider[] sendingObjects)
		{
			var eDoc = sendingObjects?.FirstOrDefault()?.Document;
			if (eDoc != null)
			{
				var eventParameters = new EventParameters();
				eventParameters.ReferenceNumber = DataWrapper.LocalReferenceNumber;
				eventParameters.RequestNumber = DataWrapper.CaseNumber;
				eventParameters.ExternalDocumentType = DataWrapper.DocumentType;

				var parameters = new Dictionary<string, string>();
				parameters.Add(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.ReferenceNumber, DataWrapper.LocalReferenceNumber);
				parameters.Add(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.RequestNumber, DataWrapper.CaseNumber);
				parameters.Add(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.ExternalDocumentType, DataWrapper.DocumentType);

				var universalEvent = new UniversalEvent();
				universalEvent.EventTime = ZDateTimeOffset.Now;
				universalEvent.EventType = Events.DocumentSentCode;
				universalEvent.EventReference = StmALog.GenerateEventReference(ZString.Empty, parameters);
				universalEvent.EventParameters = eventParameters;

				var dataContext = DataContextFactory.New(UniversalXmlInfo.Namespace_2011_11);
				dataContext.AddDataSource(DataWrapper.ContextType, DataWrapper.ContextReference);
				dataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
				universalEvent.DataContext = dataContext;

				PopulateContextCollection(eDoc, universalEvent);

				var type = new DocumentType { Code = DataWrapper.DocumentType, Description = CustomsAuthority };
				var attachedDocument = new AttachedDocument();
				attachedDocument.Type = type;

				attachedDocument.FileName = eDoc.FileName;
				attachedDocument.ImageData = eDoc.GetImageDataReader().CopyToSubStreamableStreamAndCloseStream();

				universalEvent.AttachedDocumentCollection = new List<AttachedDocument>();
				universalEvent.AttachedDocumentCollection.Add(attachedDocument);

				yield return universalEvent;
			}
		}

		protected virtual void PopulateContextCollection(MasterFiles.Integration.IeDoc eDoc, UniversalEvent universalEvent)
		{
			universalEvent.ContextCollection = new List<Context>
				{
					new Context { Type = TradingPartyID, Value = ((IZASupportingDocumentMessageDataProvider)DataWrapper).TradingPartyID },
					new Context { Type = DualProfileCode, Value = ((IZASupportingDocumentMessageDataProvider)DataWrapper).DualProfileCode },
					new Context { Type = FileName, Value = eDoc.FileName }
				};
		}

		const string TradingPartyID = "TradingPartyID";
		const string DualProfileCode = "DualProfileCode";
		const string CustomsAuthority = "CustomsAuthority";
		const string FileName = "FileName";
	}
}
