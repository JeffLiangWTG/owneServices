using System;
using System.Data;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core.Testing;

namespace Enterprise.Freight.Business.Testing
{
	sealed class ThrowIfNoParentTransportStrategyTest : TestCaseWithFactory
	{
		public void TestValueSet()
		{
			Transport transport = Factory.New<Transport>();
			try
			{
				AssertExceptionThrown(typeof(InvalidOperationException), delegate
				{ Strategy.ValueSet(transport.JW_ParentBillOfLadingInfo, new ZString()); });
				AssertEquals(true, ErrorReporter.LastKeyReported.Contains("WI00033391 International Logistics"));
			}
			finally
			{
				ExceptionReporterTestListener.Instance.Clear();
			}

			transport.ParentType = typeof(CommonConsol);
			AssertNoExceptionThrown(delegate
			{ Strategy.ValueSet(transport.JW_ParentBillOfLadingInfo, new ZString()); });
		}

		public void TestValueSet_AuditColumns()
		{
			var transport = Factory.New<DummyTransport>();
			Strategy.ValueSet(transport.JW_SystemLastEditTimeUtcInfo, new ZDateTime(2024, 11, 6));

			AssertNoExceptionThrown(() => Strategy.ValueSet(transport.JW_SystemLastEditTimeUtcInfo, new ZDateTime(2024, 11, 6)));
		}

		#region Implementation

		IValueSetStrategy Strategy
		{
			get { return strategy ?? (strategy = new ThrowIfNoParentTransportStrategy()); }
		}
		IValueSetStrategy strategy;

		#endregion

		#region DummyTransport
		public class DummyTransport : Transport
		{
			public DummyTransport(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			public override bool IsInDatabase => true;

			public override bool HasChanges => false;
		}
		#endregion
	}
}
