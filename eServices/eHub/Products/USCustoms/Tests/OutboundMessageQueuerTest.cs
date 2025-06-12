using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Text;
using CargoWise.eServices.USCustoms.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eServices.USCustoms.Tests
{
	[TestClass]
	public class OutboundMessageQueuerTest
	{
		#if DEBUG
 
		//[TestMethod]
		public void ProcessInboundMessage()
		{
			StoredProcedures.ProcessInboundMessage();
			Assert.Fail();
		}

		//[TestMethod]
		public void TestEnqueueMessage()
		{
			var con = new SqlConnection(ConfigurationManager.ConnectionStrings["eHubTransactions"].ConnectionString);
			con.Open();
			var trn = con.BeginTransaction();
			var que = new OutboundMessageQueuer(false);
			var msg = new MemoryStream(Encoding.Default.GetBytes("H4sIAAAAAAAEAOy9B2AcSZYlJi9tynt/SvVK1+B0oQiAYBMk2JBAEOzBiM3mkuwdaUcjKasqgcplVmVdZhZAzO2dvPfee++999577733ujudTif33/8/XGZkAWz2zkrayZ4hgKrIHz9+fB8/Ih6fLSfVejn7Im+a7CI/+o2TNH387Tyb5fXR8b17Ow8efvrtlJ+dT3d3d/d2dtP3e3bM83D38V0FzJ08qWbXR092drWT07MbQYXPwxgmuzuC9O42Ad59+JD7db/32h/Y3zBOxnP3gAZ5AOgnX6Z7/VfMs7+DnnY+3Xmwu/eEX9zZfeg32Nk/eJDK99/effgpf7a3F4L49OHDe3ufPnxwP/3u732wd//+vWfffrrvNzH023v5ewUf63NvJ/Zp+Ox0/o7R7f7vLT/3MY7f+0vt084dPYJ/5zFf7u7EvqXn/k56cP/ewcOdA/ov3uT3jn/snpNjIuKnu3svTo7TT3duam2e3/vLsydfnBy/uHd/Z+eL41cDrTDevTff1VHcv+14dw5i36ad8cpzf48+9J7fO/6me+x46fdveryfevy3zyIBBHcGWt/8HDw0UIiO9yMC9p6PgdWlmv+7/3hd63MvwOL3IREx+oW+3L03AP+2z+O7rLZYgT2rqpaU2U9941pSAf/GyeO7He38/wQAAP//ceCWga4FAAA="));
			que.EnqueueMessage(trn, "GEOGSCGUS", Guid.NewGuid(), "USI", "EI", msg);
			trn.Commit();
		}
		 
		//[TestMethod]
		public void TestEnqueueMessage_MessageBodyLengthLessThanStartIndex()
		{
			var con = new SqlConnection(ConfigurationManager.ConnectionStrings["eHubTransactions"].ConnectionString);
			con.Open();
			var trn = con.BeginTransaction();
			var que = new OutboundMessageQueuer(false);
			var msg = new MemoryStream(Encoding.Default.GetBytes("H4sIAAAAAAAEALPxzEvKL81L8U0tLk5MT7Xj5VJQsPFITUxJLbJzNDY2MLc081AAAwMzQ0NDIwNDBdKAAQxYGtroQw0GW+KUn1Jp52RgaeZpow9mg0Xd8vNLgCqiqG411GBeLht9NC8DAIjal4YDAQAA"));
			
			TestBase.AssertException<ApplicationException>(() => que.EnqueueMessage(trn, "GEOGSCGUS", Guid.NewGuid(), "USI", "EI", msg),
				null,
				"ABI message has empty Entry Filer Code will be rejected by CBP, please check the \"Admin->System->Registry->Customs->United States of America->CBP->Import->ABI->Entry Filer\"setting in Enterprise");

			trn.Commit();
		}

		#endif

		[TestMethod]
		public void MatchCachedReferenceFile_Success()
		{
			var mockConnection = MockRepository.GenerateStub<SqlConnection>();
			var mockTransaction = MockRepository.GenerateStub<SqlTransaction>();
			var mockCommand = MockRepository.GenerateStub<SqlCommand>();
			var mockParameters = MockRepository.GenerateStub<SqlParameterCollection>();
			var mockReader = MockRepository.GenerateStub<SqlDataReader>();
			var queuerTarget = new OutboundMessageQueuer(true);
			string clientID = "ALBNOHTRN";
			Guid internalTrackingID = new Guid("5bb33e11-ee27-49b2-892e-f6287297bfa4");
			string applicationCode = "USI";
			string messageType = "FI";
			string aText = "A0411851      11261401";
			string body = "B010411851FI                                               ALBNOHTRN_1650       F1101406                                                                        Y  0411851FI00001";
			string zText = "Z0411851      11261401";
			Guid rfpk = new Guid("744a4afd-93d5-429e-aaf8-5949689b7433");
			mockConnection.Stub(x => x.CreateCommand()).Do(new Func<SqlCommand>(() => { mockCommand.Transaction = null; return mockCommand; }));
			mockCommand.Stub(x => x.Parameters).Return(mockParameters);
			mockCommand.Expect(x => x.ExecuteReader(CommandBehavior.SingleRow)).Callback((CommandBehavior b) => mockCommand.CommandText == "SelectReferenceFileQuery" && mockCommand.Transaction == mockTransaction).Return(mockReader).Repeat.Once();
			mockReader.Stub(x => x.Read()).Return(true);
			mockReader.Stub(x => x["RF_PK"]).Return(rfpk);
			mockCommand.Expect(x => x.ExecuteNonQuery()).Callback(() => mockCommand.CommandText == "InsertOutboxFromReferenceFileCache" && mockCommand.Transaction == mockTransaction).Return(1).Repeat.Once();

			var result = queuerTarget.MatchCachedReferenceFile(mockConnection, mockTransaction, clientID, internalTrackingID, applicationCode, messageType, aText, body, zText);

			Assert.IsTrue(result);
			mockCommand.VerifyAllExpectations();
			mockParameters.AssertWasCalled(x => x.AddWithValue("@applicationCode", applicationCode), x => x.Repeat.Once());
			mockParameters.AssertWasCalled(x => x.AddWithValue("@type", messageType), x => x.Repeat.Once());
			mockParameters.AssertWasCalled(x => x.AddWithValue("@query", "F1101406"), x => x.Repeat.Once());
			mockParameters.AssertWasCalled(x => x.AddWithValue("@inboxPK", internalTrackingID), x => x.Repeat.Once());
			mockParameters.AssertWasCalled(x => x.AddWithValue("@rfpk", rfpk), x => x.Repeat.Once());
			mockParameters.AssertWasCalled(x => x.AddWithValue("@messageType", "USCustoms Import"), x => x.Repeat.Once());
			mockParameters.AssertWasCalled(x => x.AddWithValue("@header", "<InboundMessage><Header><![CDATA[" + aText + "]]></Header><Body><![CDATA["), x => x.Repeat.Once());
			mockParameters.AssertWasCalled(x => x.AddWithValue("@footer", "]]></Body><Footer><![CDATA[" + zText + "]]></Footer></InboundMessage>"), x => x.Repeat.Once());
		}

		[TestMethod]
		public void MatchCachedReferenceFile_NoMatch()
		{
			var mockConnection = MockRepository.GenerateStub<SqlConnection>();
			var mockTransaction = MockRepository.GenerateStub<SqlTransaction>();
			var mockCommand = MockRepository.GenerateStub<SqlCommand>();
			var mockParameters = MockRepository.GenerateStub<SqlParameterCollection>();
			var mockReader = MockRepository.GenerateStub<SqlDataReader>();
			var queuerTarget = new OutboundMessageQueuer(true);
			string clientID = "ALBNOHTRN";
			Guid internalTrackingID = new Guid("5bb33e11-ee27-49b2-892e-f6287297bfa4");
			string applicationCode = "USI";
			string messageType = "FI";
			string aText = "A0411851      11261401";
			string body = "B010411851FI                                               ALBNOHTRN_1650       F1101406                                                                        Y  0411851FI00001";
			string zText = "Z0411851      11261401";
			Guid rfpk = new Guid("744a4afd-93d5-429e-aaf8-5949689b7433");
			mockConnection.Stub(x => x.CreateCommand()).Do(new Func<SqlCommand>(() => { mockCommand.Transaction = null; return mockCommand; }));
			mockCommand.Stub(x => x.Parameters).Return(mockParameters);
			mockCommand.Expect(x => x.ExecuteReader(CommandBehavior.SingleRow)).Callback((CommandBehavior b) => mockCommand.CommandText == "SelectReferenceFileQuery" && mockCommand.Transaction == mockTransaction).Return(mockReader).Repeat.Once();
			mockReader.Stub(x => x.Read()).Return(false);
			mockReader.Stub(x => x["RF_PK"]).Return(rfpk);

			var result = queuerTarget.MatchCachedReferenceFile(mockConnection, mockTransaction, clientID, internalTrackingID, applicationCode, messageType, aText, body, zText);

			Assert.IsFalse(result);
			mockCommand.VerifyAllExpectations();
			mockCommand.AssertWasNotCalled(x => x.ExecuteNonQuery());
			mockParameters.AssertWasCalled(x => x.AddWithValue("@applicationCode", applicationCode), x => x.Repeat.Once());
			mockParameters.AssertWasCalled(x => x.AddWithValue("@type", messageType), x => x.Repeat.Once());
			mockParameters.AssertWasCalled(x => x.AddWithValue("@query", "F1101406"), x => x.Repeat.Once());
		}

		[TestMethod]
		public void MatchCachedReferenceFile_ExcludedApplication()
		{
			var mockConnection = MockRepository.GenerateStub<SqlConnection>();
			var mockTransaction = MockRepository.GenerateStub<SqlTransaction>();
			var queuerTarget = new OutboundMessageQueuer(true);
			string clientID = "ALBNOHTRN";
			Guid internalTrackingID = new Guid("5bb33e11-ee27-49b2-892e-f6287297bfa4");
			string applicationCode = "MAN";
			string messageType = "PTR";
			string aText = "UNB+UNOA:4+5DS9:ZZ+CBP-ACE:ZZ+20141021:1606+14++ACE'UNG+CUSREP+5DS9:ZZ+CBP-ACE:ZZ+20141021:1606+14+UN+D:03B'";
			string body = "UNH+14+CUSREP:D:03B:UN'BGM+336:::STANDARD+RDWYMAN0000003+3'DTM+132:201410221219:203'RFF+ABO:PTR14'UNT+5+14'";
			string zText = "UNE+1+14'UNZ+1+14'";

			var result = queuerTarget.MatchCachedReferenceFile(mockConnection, mockTransaction, clientID, internalTrackingID, applicationCode, messageType, aText, body, zText);

			Assert.IsFalse(result);
			mockConnection.AssertWasNotCalled(x => x.CreateCommand());
		}
	}
}
