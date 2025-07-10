using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(TraceSourceSettingsCollection))]
	public class TraceSourceSettingsCollectionTest : NonPersistentBusinessObjectCollectionTestCase<TraceSourceSettingsCollection>
	{
		protected override TraceSourceSettingsCollection GetCollectionToTest()
		{
			return new TraceSourceSettingsCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new TraceSourceSettings();
		}

		public void TestAllowNew()
		{
			var collection = GetCollectionToTest();
			AssertEquals(false, collection.AllowNew);
		}

		public void TestAllowRemove()
		{
			var collection = GetCollectionToTest();
			AssertEquals(false, collection.AllowRemove);
		}

		public void TestGetTraceSources()
		{
			var collection = TraceSourceSettingsCollection.GetTraceSources();
			AssertEquals("Source setting collection must contain the correct number of trace sources", 8, collection.Count);

			//Accounting
			AssertEquals("APA trace source", AccountingTraceSourceCodes.APA, collection[0].TraceSourceName);
			AssertEquals("CASS trace source", AccountingTraceSourceCodes.CASS, collection[1].TraceSourceName);
			AssertEquals("Consol Cost trace source", AccountingTraceSourceCodes.ConsolCost, collection[2].TraceSourceName);
			AssertEquals("eInvoicing trace source", AccountingTraceSourceCodes.eInvoicing, collection[3].TraceSourceName);
			AssertEquals("FPOS trace source", AccountingTraceSourceCodes.FPOS, collection[4].TraceSourceName);
			AssertEquals("HTTP trace source", AccountingTraceSourceCodes.Http, collection[5].TraceSourceName);
			AssertEquals("CLC trace source", AccountingTraceSourceCodes.CLC, collection[6].TraceSourceName);

			//Core
			AssertEquals("Registry trace source", CoreTraceSourceCodes.Registry, collection[7].TraceSourceName);
		}

		public void TestGetAllCodes()
		{
			var codes = TraceSourceSettingsCollection.GetTraceSources().GetAllCodes();

			AssertEquals("Source setting codes collection must contain correct number of codes", 8, codes.Length);

			//Accounting
			AssertEquals("APA trace source", AccountingTraceSourceCodes.APA, codes[0]);
			AssertEquals("CASS trace source", AccountingTraceSourceCodes.CASS, codes[1]);
			AssertEquals("Consol Cost trace source", AccountingTraceSourceCodes.ConsolCost, codes[2]);
			AssertEquals("eInvoicing trace source", AccountingTraceSourceCodes.eInvoicing, codes[3]);
			AssertEquals("FPOS trace source", AccountingTraceSourceCodes.FPOS, codes[4]);
			AssertEquals("HTTP trace source", AccountingTraceSourceCodes.Http, codes[5]);
			AssertEquals("CLC trace source", AccountingTraceSourceCodes.CLC, codes[6]);

			//Core
			AssertEquals("Registry trace source", CoreTraceSourceCodes.Registry, codes[7]);
		}
	}
}
