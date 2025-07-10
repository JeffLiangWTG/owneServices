using System;
using CargoWise.Common;
using CargoWise.Data;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	internal static class SendingarnumerNumberFountainTestHelper
	{
		public static void SetNextForNumberFountain(INumberFountainProxy numFountain, int nextValue)
		{
			Db.Connection.BeginTransaction();       // Testing the number fountain

			try
			{
				numFountain.SetNext(Db.Connection, nextValue);
				Db.Connection.CommitTransaction();      // Testing the number fountain
			}
			catch (Exception e) when (!e.IsCriticalException())
			{
				Db.Connection.RollbackTransaction();        // Testing the number fountain
				throw;
			}
		}
	}
}
