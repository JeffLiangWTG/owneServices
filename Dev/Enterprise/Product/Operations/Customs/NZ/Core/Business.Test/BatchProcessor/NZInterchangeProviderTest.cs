using System;
using Enterprise.BatchProcessor;
using Enterprise.Customs.NZ.Registry;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.InterchangeProviders;
using Enterprise.Messaging.InterchangeProviders.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NZ.Business.BatchProcessor.Testing
{
	public class ExceptionTest : SenderReceiverIDExceptionTest
	{
		protected override InterchangeProviderBase GetInterchangeProvider(NonDependentEDIMessageCollection messages)
		{
			return new NZInterchangeProvider(new LoggingInformation(), messages);
		}
	}

	public class NZInterchangeProviderTest : InterchangeProviderTestCase
	{
		[TestDate(2007, 06, 13, 10, 36, 0)]
		public override void TestMessagesPopulateNewInterchange()
		{
			NonDependentEDIMessageCollection messages = new NonDependentEDIMessageCollection(Factory);
			EDIMessage message = messages.AddNew();
			message.EM_MessageText = @"UNH+2437+CUSRES:D:96B:UN+111145'
	BGM+963+36693381:05'
	GIS+837:120:143'
	ERP+001::80'
	ERC+676::143'
	UNT+6+2437'
	UNH+2438+CUSRES:D:96B:UN+111145'
	BGM+963+36693381:06'
	GIS+837:120:143'
	ERP+001::80'
	ERC+676::143'
	UNT+6+2438'
	UNH+2439+CUSRES:D:96B:UN+111146'
	BGM+963+46489765:05'
	GIS+837:120:143'
	ERP+001::95'
	ERC+110::143'
	UNT+6+2439'
	UNH+2440+CUSRES:D:96B:UN+111146'
	BGM+965+46489765:06'
	GIS+830:120:143'
	UNT+4+2440'";
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.NewZealandCustoms;
			message.EM_IsTestMessage = true;
			message.EM_MessageOwner = "";

			EDIMessage message2 = messages.AddNew();
			message2.EM_MessageText = message.EM_MessageText;
			message2.EM_ApplicationCode = EDIMessage.ApplicationCodes.NewZealandCustoms;
			message2.EM_IsTestMessage = true;
			message2.EM_MessageOwner = "";

			NZInterchangeProvider provider = new NZInterchangeProvider(new LoggingInformation(), messages);

			EDIInterchangeCollection interchanges = new EDIInterchangeCollection(Factory);
			interchanges.AddRange(provider.Interchanges);

			AssertEquals("NumberOfInterchanges - legacy interchanges should not be generated any more", 0, interchanges.Count);
		}

		#region Implementation

		protected override InterchangeProviderBase GetInterchangeProvider(NonDependentEDIMessageCollection collection)
		{
			return new NZInterchangeProvider(new LoggingInformation(), collection);
		}

		protected override void SetUp()
		{
			base.SetUp();
			NZCustomsDataRegistry.Instance.NZBrokerageID.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "00009917B");
		}

		#endregion
	}
}
