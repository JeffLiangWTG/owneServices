using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class TWMessageTestCase : TestCase
	{
		[UseSnapshotProtection]
		[ExpectNoExceptions]
		public void TestSavingSetsEM_MessageNum_NoDuplicateReferenceException()
		{
			var factory = new BusinessObjectFactory();
			var message = factory.New<TWMessageForTest>();
			message.IsTransmitMessage = false;
			var messageNum = ZString.Empty;
			var dbConnection = ((CargoWise.Data.IDbConnected)factory).Connection;
			try
			{
				dbConnection.BeginTransaction();
				message.PopulateMessageNumberIfNeededExposed();
				messageNum = message.EM_MessageNum;
			}
			finally
			{
				dbConnection.RollbackTransaction();
			}

			var newFactory = new BusinessObjectFactory();
			var message2 = newFactory.New<TWMessage>();
			message2.IsTransmitMessage = false;
			newFactory.Save();
			NUnit.Framework.Assert.That(message2.EM_MessageNum, NUnit.Framework.Is.EqualTo(messageNum));
			NUnit.Framework.Assert.That(message.EM_MessageNum, NUnit.Framework.Is.EqualTo(messageNum));
			factory.Save();
			NUnit.Framework.Assert.That(message.EM_MessageNum, NUnit.Framework.Is.Not.EqualTo(messageNum));
		}

		[ExpectNoExceptions]
		public void TestPopulateMessageNumberIfNeeded()
		{
			var factory = new BusinessObjectFactory();
			var dbConnection = ((CargoWise.Data.IDbConnected)factory).Connection;
			dbConnection.BeginTransaction();
			try
			{
				var message = factory.New<TWMessageForTest>();
				NUnit.Framework.Assert.That(message.EM_MessageNum, NUnit.Framework.Is.EqualTo(ZString.Empty));
				var messageNum = Env.NumberFountains.GetIncomingTWCustomsMessageNumber().PeekPreliminaryFormatted(factory);
				message.EM_MessageNum = "BSZSSD23423";
				message.PopulateMessageNumberIfNeededExposed();
				NUnit.Framework.Assert.That(message.EM_MessageNum, NUnit.Framework.Is.EqualTo("BSZSSD23423").Using(CustomComparers.TypeComparison));
				message.EM_MessageNum = ZString.Empty;
				message.IsTransmitMessage = true;
				message.PopulateMessageNumberIfNeededExposed();
				NUnit.Framework.Assert.That(message.EM_MessageNum.ToString(), NUnit.Framework.Is.Null.Or.Empty);
				message.IsTransmitMessage = false;
				message.PopulateMessageNumberIfNeededExposed();
				NUnit.Framework.Assert.That(message.EM_MessageNum, NUnit.Framework.Is.EqualTo(messageNum).Using(CustomComparers.TypeComparison));
			}
			finally
			{
				dbConnection.RollbackTransaction();
			}
		}
	}
}
