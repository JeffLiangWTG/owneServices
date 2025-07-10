using System.Collections.Generic;
using System.Linq;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class AccTransactionLinesCriticalValidationNonTransactionedTest : TestCase
	{
		[UseSnapshotProtection]
		public void TestTransactionLineCannotBeDeletedForSpecificTypesDueToDatabaseTrigger()
		{
			var lineTypes = new List<string> { TransactionLineTypes.WIP, TransactionLineTypes.Accrual, TransactionLineTypes.Revenue, TransactionLineTypes.Cost };
			var factory = new BusinessObjectFactory();

			foreach (var lineType in lineTypes)
			{
				var line = factory.NewWithValidTestData<AccTransactionLines>();
				line.AL_AG = factory.NewWithValidTestData<AccGLHeader>().PK;
				var header = factory.NewWithValidTestData<AccTransactionHeader>();
				var newFactory = new BusinessObjectFactory();
				if (new[] { TransactionLineTypes.Cost, TransactionLineTypes.Revenue }.Contains(lineType))
				{
					header.AH_InvoiceDate = new ZDateTime(2007, 6, 22);
					header.AH_Ledger = lineType == TransactionLineTypes.Cost ? LedgerTypes.AccountsPayable : LedgerTypes.AccountsReceivable;
					header.AH_TransactionType = TransactionTypes.Invoice;
					line.AL_AH = header.PK;
				}

				line.AL_LineType = lineType;
				factory.Save();
				line.AL_OSAmount = line.AL_OSAmount + 1;
				factory.Save(); // Lines are edited after save to prove that trigger does not affect updating records.

				var lineInNewFactory = newFactory.Load<AccTransactionLines>(line.PK);
				try
				{
					lineInNewFactory.Delete();
					newFactory.Save();
				}
				catch (ZSaveException e)
				{
					AssertEquals("Exception message", $"Attempting to delete transaction line of type {line.AL_LineType}. Transaction lines of type WIP, ACR, CST &  REV cannot be deleted once saved in database.", e.FriendlyMessage);
					continue;
				}
				Fail("Should not get executed");
			}
		}
	}
}
