using System;
using CargoWise.Data;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Registry;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.ServiceTasks.CargoIMPPhase2.Test
{
	public class TraxonCargoIMPPhase2InterchangeProviderTest : TestCaseWithFactory
	{
		[TestDate(2007, 1, 1, 10, 11, 12)]
		public void TestMessagesPopulateNewInterchange()
		{
			ForwardingConfigurationRegistry.Instance.CargoIMPPhase2TraxonSenderPIMAAddress.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "PIMAADDR");
			NonDependentEDIMessageCollection messages = new NonDependentEDIMessageCollection(Factory);
			EDIMessage message1 = messages.AddNew();
			message1.EM_MessageText = "MESSAGE1" + EDIMessage.MessageNumberPlaceHolder;
			message1.EM_ApplicationCode = ApplicationCodeList.Codes.CargoIMPPhase2;
			message1.EM_MessageType = "RMI";
			message1.EM_MessageOwner = string.Empty;
			message1.MessageNumberStrategy = new TestMessageNumberStrategy();
			EDIMessage message2 = messages.AddNew();
			message2.EM_MessageText = "MESSAGE2" + EDIMessage.MessageNumberPlaceHolder;
			message2.EM_ApplicationCode = ApplicationCodeList.Codes.CargoIMPPhase2;
			message2.EM_MessageType = "RMX";
			message2.EM_MessageOwner = string.Empty;
			message2.MessageNumberStrategy = new TestMessageNumberStrategy();
			Factory.Save();
			TraxonCargoIMPPhase2InterchangeProvider provider = new TraxonCargoIMPPhase2InterchangeProvider(messages);
			EDIInterchangeCollection interchanges = new EDIInterchangeCollection(Factory);
			interchanges.AddRange(provider.Interchanges);
			AssertEquals("NumberOfInterchanges", 2, interchanges.Count);
			EDIInterchange interchange1 = interchanges[0];
			AssertEquals("Header", "UNB+TRXA:1+PIMAADDR:PIMA+REUAGT82YAS:PIMA+070101:1011+" + EDIInterchange.InterchangeNumberPlaceHolder + "+0'UNH+1+TRXRMI:3+1'", interchange1.EI_HeaderText);
			AssertEquals("Footer", "'UNT+3+1'UNZ+1+" + EDIInterchange.InterchangeNumberPlaceHolder + "'", interchange1.EI_FooterText);
			AssertEquals(TraxonCargoIMPPhase2InterchangeProvider.TraxonPIMAAddress, interchange1.EI_To);
			AssertEquals("PIMAADDR", interchange1.EI_From);
			EDIInterchange interchange2 = interchanges[1];
			AssertEquals("Header", "UNB+TRXA:1+PIMAADDR:PIMA+REUAGT82YAS:PIMA+070101:1011+" + EDIInterchange.InterchangeNumberPlaceHolder + "+0'UNH+1+TRXRMX:2+1'", interchange2.EI_HeaderText);
			AssertEquals("Footer", "'UNT+3+1'UNZ+1+" + EDIInterchange.InterchangeNumberPlaceHolder + "'", interchange2.EI_FooterText);
			AssertEquals(TraxonCargoIMPPhase2InterchangeProvider.TraxonPIMAAddress, interchange2.EI_To);
			AssertEquals("PIMAADDR", interchange2.EI_From);
			long nextMessageNumber = Env.NumberFountains.EDIFACTNumberFountain("I", "PIMAADDR", TraxonCargoIMPPhase2InterchangeProvider.TraxonPIMAAddress).PeekPreliminary(Db.Connection);
			Factory.Save();
			AssertEquals(nextMessageNumber.ToString(), interchange1.EI_InterchangeNum);
			AssertEquals((nextMessageNumber + 1).ToString(), interchange2.EI_InterchangeNum);
		}
	}
}
