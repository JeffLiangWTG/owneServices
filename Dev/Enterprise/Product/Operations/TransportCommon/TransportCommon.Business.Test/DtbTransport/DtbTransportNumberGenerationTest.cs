using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.TransportCommon.Business.Testing
{
	public abstract class DtbTransportNumberGenerationTest : TestCase
	{
		#region TestTransportNumberGeneration

		[UseSnapshotProtection]
		public void TestTransportNumberGeneration()
		{
			var customisation1 = CreateCustomisations(clientCodedPrefix: "AA", includeService: true, removeFountainPrefix: true);
			AddCustomisationToRegistry(customisation1);
			AssertGeneratedJobIDFromFountain("", "AA000001");
			AssertGeneratedJobIDFromFountain("STD", "AASTD000002");
			AssertGeneratedJobIDFromFountain("D2D", "AAD2D000003");

			var customisation2 = CreateCustomisations(clientCodedPrefix: "BB", includeService: false, removeFountainPrefix: true);
			AddCustomisationToRegistry(customisation2);
			AssertGeneratedJobIDFromFountain("", "BB000001");
			AssertGeneratedJobIDFromFountain("STD", "BB000002");
			AssertGeneratedJobIDFromFountain("D2D", "BB000003");

			var customisation3 = CreateCustomisations(clientCodedPrefix: "", includeService: false, removeFountainPrefix: false);
			AddCustomisationToRegistry(customisation3);
			AssertGeneratedJobIDFromFountain("", "000001", usePrefix: true);
			AssertGeneratedJobIDFromFountain("STD", "000002", usePrefix: true);
			AssertGeneratedJobIDFromFountain("D2D", "000003", usePrefix: true);

			var customisation4 = CreateCustomisations(clientCodedPrefix: "TEST", includeService: false, removeFountainPrefix: true);
			AddCustomisationToRegistry(customisation4);

			Db.Connection.BeginTransaction();
			try
			{
				AssertEquals("00000001", Env.NumberFountains.GetDtbTransportGeneratorFountain("TEST").GetNextFormatted(Factory));
				AssertGeneratedJobIDFromFountain("", "TEST000002");
			}
			finally
			{
				Db.Connection.RollbackTransaction();
			}
		}

		#endregion

		#region TestTransportNumberGeneration_HandlesUnexpectedCollisions

		[UseSnapshotProtection]
		public void TestTransportNumberGeneration_HandlesUnexpectedCollisions()
		{
			var customisation1 = CreateCustomisations(clientCodedPrefix: "AA", includeService: false, removeFountainPrefix: true);
			AddCustomisationToRegistry(customisation1);
			AssertGeneratedJobIDFromFountain("", "AA000001");

			var transport1 = GetSaveableTransportJob();
			transport1.KM_JobID = "AA000003"; // Hack to make job ID same as what the number fountain will hit next

			/* transport2 */
			AssertGeneratedJobIDFromFountain("", "AA000002");

			var transport3 = GetSaveableTransportJob();
			try
			{
				Factory.Save();
				Fail("Index exception should happen");
			}
			catch (ZSaveException ex)
			{
				ZExceptionReporting.HandleSaveException(ex);
				ErrorReporter.Clear();
			}

			Factory.Save();
			AssertEquals("Should bump up number fountain to fix collision.", transport3.KM_JobID, "AA000004");
		}

		#endregion

		#region TestTransportNumberGeneration_HandlesUnexpectedCollisions_NoCustomisation

		[UseSnapshotProtection]
		public void TestTransportNumberGeneration_HandlesUnexpectedCollisions_NoCustomisation()
		{
			AssertGeneratedJobIDFromFountain("", "00000001", true);

			var transport1 = GetSaveableTransportJob();
			transport1.KM_JobID = string.Format("{0}00000003", FountainPrefixForTest); // Hack to make job ID same as what the number fountain will hit next

			/* transport2 */
			AssertGeneratedJobIDFromFountain("", "00000002", true);

			var transport3 = GetSaveableTransportJob();
			try
			{
				Factory.Save();
				Fail("Index exception should happen");
			}
			catch (ZSaveException ex)
			{
				ZExceptionReporting.HandleSaveException(ex);
				ErrorReporter.Clear();
			}

			Factory.Save();
			AssertEquals("Should bump up number fountain to fix collision.", transport3.KM_JobID, string.Format("{0}00000004", FountainPrefixForTest));
		}

		#endregion

		#region Implementation

		void AssertGeneratedJobIDFromFountain(string serviceLevel, string expect, bool usePrefix = false)
		{
			AssertGeneratedJobIDFromFountain("", serviceLevel, expect, usePrefix);
		}

		void AssertGeneratedJobIDFromFountain(string assertionMessage, string serviceLevel, string expect, bool usePrefix = false)
		{
			var transport = GetSaveableTransportJob();
			var prefix = usePrefix ? FountainPrefixForTest : ZString.Empty;
			transport.KM_RS_NKServiceLevel = serviceLevel;
			Factory.Save();
			AssertEquals(assertionMessage, prefix + expect, transport.KM_JobID);
		}

		BillOfLadingNumberCustomisationsByServiceLevel CreateCustomisations(string clientCodedPrefix, bool includeService, bool removeFountainPrefix)
		{
			var serviceCustomisations = new BillOfLadingNumberCustomisationsByServiceLevel();
			foreach (BillOfLadingNumberCustomisation customisation in serviceCustomisations.BillOfLadingNumberCustomisations)
			{
				customisation.RemoveFountainPrefix = removeFountainPrefix;
				foreach (BillOfLadingNumberCustomisationElement element in customisation.Elements)
				{
					switch (element.Key)
					{
						case BillOfLadingNumberCustomisationElement.Keys.SequenceNumber:
							element.Include = true;
							element.Detail = "6";
							element.Order = 50;
							break;

						case BillOfLadingNumberCustomisationElement.Keys.ClientCoded1:
							element.Include = !string.IsNullOrEmpty(clientCodedPrefix);
							element.Detail = clientCodedPrefix;
							element.Fountain = true;
							element.Order = 1;
							break;

						case BillOfLadingNumberCustomisationElement.Keys.ServiceLevel:
							element.Include = includeService;
							element.Order = 2;
							break;

						default:
							element.Include = false;
							break;
					}
				}
			}
			return serviceCustomisations;
		}

		protected BusinessObjectFactory Factory
		{
			get { return factory ?? (factory = new BusinessObjectFactory()); }
		}
		BusinessObjectFactory factory;

		protected abstract DtbTransport GetSaveableTransportJob();
		protected abstract ZString FountainPrefixForTest { get; }
		protected abstract void AddCustomisationToRegistry(BillOfLadingNumberCustomisationsByServiceLevel customisation);

		#endregion
	}
}
