using System;
using System.IO;
using CargoWise.Application;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.IO;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using ServiceManager.Integration.Abstractions;

namespace Enterprise.MasterFiles.DataTransfer.Universal.Testing
{
	class UniversalInterchangeRequeueRequestHandlerTest : TestCase
	{
		[UseSnapshotProtection]
		public void TestProcess()
		{
			var requestXml =
				@"<UniversalInterchangeRequeueRequest xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"">
					<InterchangeRequeueRequest>
						<FilterCollection>
							<Filter>
								<Type>CreateDateUTCFrom</Type>
								<Value>{0}</Value>
							</Filter>
							<Filter>		
								<Type>CreateDateUTCTo</Type>
								<Value>{1}</Value>
							</Filter>
							<Filter>
								<Type>BranchCode</Type>
								<Value>{2}</Value>
							</Filter>
							<Filter>
								<Type>BranchCode</Type>
								<Value>{3}</Value>
							</Filter>
					</FilterCollection>
				  </InterchangeRequeueRequest>
				</UniversalInterchangeRequeueRequest>";

			var badRequestXML =
								@"<UniversalInterchangeRequeueRequest xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"">
					<InterchangeRequeueRequest>
						<FilterCollection>
							<Filter>
								<Type>CreateDateUTCFrom</Type>
								<Value>2017-01-01 00:00:30</Value>
							</Filter>
					</FilterCollection>
				  </InterchangeRequeueRequest>
				</UniversalInterchangeRequeueRequest>";

			//preparing test data
			var factory = new BusinessObjectFactory();

			var branch1 = factory.LoadTop1<IGlbBranch>(new ZQuery(GlbBranchSchema.GB_Code, "SIN"));
			var branch2 = factory.LoadTop1<IGlbBranch>(new ZQuery(GlbBranchSchema.GB_Code, "BNE"));

			var ediInterchange1 = factory.New<IEDIInterchange>();
			ediInterchange1.EI_GB = branch1.PK;
			ediInterchange1.EI_From = "senderCode1";
			ediInterchange1.EI_To = "recipientCode1";
			ediInterchange1.EI_Status = "SNT";

			var ediInterchange2 = factory.New<IEDIInterchange>();
			ediInterchange2.EI_GB = branch2.PK;
			ediInterchange2.EI_From = "senderCode2";
			ediInterchange2.EI_To = "recipientCode2";
			ediInterchange2.EI_Status = "SNT";

			var stmALog1 = factory.New<StmALog>();
			using (stmALog1.LockForUpdatingKeyFieldsForTesting())
			{
				stmALog1.SL_Parent = ediInterchange1.PK;
				stmALog1.SL_Table = ediInterchange1.TableName;
				stmALog1.SL_Reference = "blah blah AQU";
			}

			factory.Save();

			var xmlSessionTracker = new XmlSessionTracker(new SimpleLogger());
			var handler = new UniversalInterchangeRequeueRequestHandler(xmlSessionTracker);

			var request = handler.CreateRequestMessage();

			//send the bad request first
			using (var stream = (SubStreamableStream)new MemoryStream())
			{
				new StreamWriter(stream) { AutoFlush = true }.Write(badRequestXML);
				request.SetMessageTextSource(stream);
				request.Save();
			}

			using (((BusinessObject)request).Factory.AddDisposableService())
			{
				var processingResult = handler.Process(request);
				request.Save();
				AssertNotNull(processingResult);
				AssertEquals("ERR", processingResult.Status);
			}
			AssertContains("When CreateDateUTCTo is not specified the value of CreateDateUTCFrom must be less than 24 hours in the past", xmlSessionTracker.ToString());

			//now send a good request
			requestXml = string.Format(requestXml, DateTime.UtcNow.AddHours(-1), DateTime.UtcNow.AddHours(1), "BNE", "SIN");
			using (var stream = (SubStreamableStream)new MemoryStream())
			{
				new StreamWriter(stream) { AutoFlush = true }.Write(requestXml);
				request.SetMessageTextSource(stream);
				request.Save();
			}

			using (((BusinessObject)request).Factory.AddDisposableService())
			using (ObjectFactory.Substitute(Mock.Of<INudgingController>()))
			{
				var processingResult = handler.Process(request);
				request.Save();
				AssertNotNull(processingResult);
				AssertEquals("PRS", processingResult.Status);
				processingResult.ResponseMessageText.Position = 0;
				string responseMessageText;
				using (var reader = new StreamReader(processingResult.ResponseMessageText))
				{
					responseMessageText = reader.ReadToEnd();
				}

				var query = new ZQuery();
				query.AddToFilter(StmALogSchema.SL_Parent, ediInterchange1.PK);
				query.OrderBy = $"{StmALogSchema.SL_EventTime.Name} DESC";
				var logRecord = factory.LoadTop1<StmALog>(query);

			Assert(logRecord != null);
			Assert(logRecord.SL_Reference == "|NEW=AQU|OLD=SNT");

				factory.ClearQueryCache();
				ediInterchange1 = (new BusinessObjectFactory()).Load<IEDIInterchange>(ediInterchange1.PK);
				AssertEquals(EDIInterchangeStatusList.Codes.eAdaptorQueued, ediInterchange1.EI_Status);
				AssertEquals(EDIInterchangeTransportTypeList.Codes.eAdaptor, ediInterchange1.EI_TransportType);

				AssertContains(
	@"      <Context>
        <Type>InterchangeRequeueCount</Type>
        <Value>1</Value>
      </Context>", responseMessageText);
			}
		}

		[UseSnapshotProtection]
		public void TestDoesNotAllowUnknownFilterType()
		{
			var badRequestXML =
				@"<UniversalInterchangeRequeueRequest xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"">
					<InterchangeRequeueRequest>
						<FilterCollection>
							<Filter>
								<Type>CreateDateUTCFrom</Type>
								<Value>{0}</Value>
							</Filter>
							<Filter>		
								<Type>CreateDateUTCTo</Type>
								<Value>{1}</Value>
							</Filter>
							<Filter>
								<Type>ThisIsAnInvalidFilterType</Type>
								<Value>AValue</Value>
							</Filter>
					</FilterCollection>
				  </InterchangeRequeueRequest>
				</UniversalInterchangeRequeueRequest>";

			var xmlSessionTracker = new XmlSessionTracker(new SimpleLogger());
			var handler = new UniversalInterchangeRequeueRequestHandler(xmlSessionTracker);

			var request = handler.CreateRequestMessage();
			badRequestXML = string.Format(badRequestXML, DateTime.UtcNow.AddHours(-1), DateTime.UtcNow.AddHours(1));

			using (var stream = (SubStreamableStream)new MemoryStream())
			{
				new StreamWriter(stream) { AutoFlush = true }.Write(badRequestXML);
				request.SetMessageTextSource(stream);
				request.Save();
			}

			var processingResult = handler.Process(request);
			request.Save();
			AssertNotNull(processingResult);
			AssertEquals("ERR", processingResult.Status);
			AssertContains("Invalid Filter Type [ThisIsAnInvalidFilterType] - Valid types are CreateDateUTCFrom, CreateDateUTCTo, ApplicationCode, InterchangeNumberFrom, InterchangeNumberTo, SenderCode, RecipientCode and BranchCode.", xmlSessionTracker.ToString());
		}

		[UseSnapshotProtection]
		public void TestDoesNotAllowMissingFilterCollection()
		{
			var badRequestXML =
				@"<UniversalInterchangeRequeueRequest xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"">
					<InterchangeRequeueRequest>
				  </InterchangeRequeueRequest>
				</UniversalInterchangeRequeueRequest>";

			var xmlSessionTracker = new XmlSessionTracker(new SimpleLogger());
			var handler = new UniversalInterchangeRequeueRequestHandler(xmlSessionTracker);

			var request = handler.CreateRequestMessage();

			using (var stream = (SubStreamableStream)new MemoryStream())
			{
				new StreamWriter(stream) { AutoFlush = true }.Write(badRequestXML);
				request.SetMessageTextSource(stream);
				request.Save();
			}

			var processingResult = handler.Process(request);
			request.Save();
			AssertNotNull(processingResult);
			AssertEquals("ERR", processingResult.Status);
			AssertContains("Please specify FilterCollection.", xmlSessionTracker.ToString());
		}
	}
}
