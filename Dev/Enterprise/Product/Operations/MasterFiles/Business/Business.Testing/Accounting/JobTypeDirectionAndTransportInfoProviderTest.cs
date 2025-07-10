using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business.Testing.Accounting
{
	sealed class JobTypeDirectionAndTransportInfoProviderTest : TestCaseWithFactory
	{
		public void TestJobTypeList()
		{
			var bizO = new DummyInfoProviderConsumer(Factory);
			var provider = new JobTypeDirectionAndTransportInfoProvider(() => bizO.JobTypeInfo, () => bizO.DirectionInfo, () => bizO.TransportModeInfo);

			var expectedJobTypes = new string[]
							{
								"ALL",
								"SHP", "QSH",
								"FCN", "GCN", "BRK", "PCB",
								"AWB", "CSH", "CLL",
								"CST", "TRN", "TBM", "ATB", "ABK",
								"TCW", "LTC", "WIN", "WOU", "WSJ", "WVO",
								"WST", "WSC",
								"ACR", "UBR", "CTO",
								"AHW", "AHE", "AGS",
								"AGB", "ACD", "AVA",
								"ASC", "ISF",
								"MAN", "CAE",
								"WKI", "WKP", "WKR",
								"YRA", "YRE", "YTU", "MWO", "YAO", "YPI",
								"TRC", "TDC", "TRU", "TDL", "TDU",
								"NCT", "LPC", "STO"
							};

			AssertEquals("The JobTypeList should contain 'ALL' as the first element", "ALL", provider.JobTypeList[0].Code);
			Assert("The JobType 'ALL' should be JobInvoicingConsumerType", provider.JobTypeList[0] is JobInvoicingConsumerType);
			AssertEquals("JobType: No unexpected item should be found", false, provider.JobTypeList.Cast<CodeDescriptionPair>().Select(x => x.Code).Except(expectedJobTypes).Any());
			AssertEquals("JobType: No expected item should be missing", false, expectedJobTypes.Except(provider.JobTypeList.Cast<CodeDescriptionPair>().Select(x => x.Code)).Any());
		}

		public void TestDirectionList()
		{
			var bizO = new DummyInfoProviderConsumer(Factory);
			var provider = new JobTypeDirectionAndTransportInfoProvider(() => bizO.JobTypeInfo, () => bizO.DirectionInfo, () => bizO.TransportModeInfo);

			var expectedDirections = new string[]
				{
					"ALL", "EXP", "IMP", "DOM", "OTH"
				};

			foreach (CodeDescriptionPair jobType in provider.JobTypeList)
			{
				bizO.JobType = jobType.Code;

				AssertEquals($"JobType: {bizO.JobType}, Direction: No unexpected item should be found", false, provider.DirectionList.Cast<CodeDescriptionPair>().Select(x => x.Code).Except(expectedDirections).Any());
				AssertEquals($"JobType: {bizO.JobType}, Direction: No expected item should be missing", false, expectedDirections.Except(provider.DirectionList.Cast<CodeDescriptionPair>().Select(x => x.Code)).Any());
			}
		}

		public void TestTransportModeList()
		{
			var bizO = new DummyInfoProviderConsumer(Factory);
			var provider = new JobTypeDirectionAndTransportInfoProvider(() => bizO.JobTypeInfo, () => bizO.DirectionInfo, () => bizO.TransportModeInfo);
			var transportModeList = provider.TransportModeList;

			var expectedTransportModes = new string[]
				{
					"ALL", "AIR", "SEA", "FSA", "FAS", "ROA", "RAI", "COU"
				};

			foreach (var jobTypeCode in provider.JobTypeList.GetAllCodes().Where(x => x != JobInvoicingConsumerTypes.Brokerage.Code))
			{
				bizO.JobType = jobTypeCode;

				AssertEquals($"JobType: {bizO.JobType}, TransportMode: No unexpected item should be found", false, provider.TransportModeList.Cast<CodeDescriptionPair>().Select(x => x.Code).Except(expectedTransportModes).Any());
				AssertEquals($"JobType: {bizO.JobType}, TransportMode: No expected item should be missing", false, expectedTransportModes.Except(provider.TransportModeList.Cast<CodeDescriptionPair>().Select(x => x.Code)).Any());
			}

			var expectedBrokerageTransportModes = new string[]
				{
					"ALL", "AIR", "FIX", "IWT", "OWN", "MAI", "RAI", "ROA", "SEA"
				};

			bizO.JobType = JobInvoicingConsumerTypes.Brokerage.Code;

			AssertEquals("JobType: BRK, TransportMode: No unexpected item should be found", false, provider.TransportModeList.Cast<CodeDescriptionPair>().Select(x => x.Code).Except(expectedBrokerageTransportModes).Any());
			AssertEquals("JobType: BRK, TransportMode: No expected item should be missing", false, expectedBrokerageTransportModes.Except(provider.TransportModeList.Cast<CodeDescriptionPair>().Select(x => x.Code)).Any());
		}

		public void TestInvoiceTypeList()
		{
			var bizO = new DummyInfoProviderConsumer(Factory);
			var provider = new JobTypeDirectionAndTransportInfoProvider(() => bizO.JobTypeInfo, () => bizO.DirectionInfo, () => bizO.TransportModeInfo);

			bizO.JobType = string.Empty;
			AssertEquals("InvoiceType: No Item Should be found", 0, provider.InvoiceTypeList.Count);

			bizO.JobType = JobInvoicingConsumerTypes.Shipment.Code;
			var expectedInvoiceTypes = new string[]
				{
					"DES", "DED", "DCU", "DCD",
					"DBT", "DBD", "FIN",
					"FID", "CUR", "CUD", "FRT",
					"FRD", "ITC", "ITD", "SBR",
					"SBD"
				};

			AssertEquals("InvoiceType: No unexpected item should be found", false, provider.InvoiceTypeList.Cast<CodeDescriptionPair>().Select(x => x.Code).Except(expectedInvoiceTypes).Any());
			AssertEquals("InvoiceType: No expected item should be missing", false, expectedInvoiceTypes.Except(provider.InvoiceTypeList.Cast<CodeDescriptionPair>().Select(x => x.Code)).Any());

			bizO.JobType = JobInvoicingConsumerTypes.AgencyBooking.Code;
			expectedInvoiceTypes = new string[]
				{
					"FCO", "FCD",
					"FPP", "FPD", "LCO",
					"LCD", "LPP", "LPD",
					"MSC", "MSD"
				};

			AssertEquals("InvoiceType: No unexpected item should be found", false, provider.InvoiceTypeList.Cast<CodeDescriptionPair>().Select(x => x.Code).Except(expectedInvoiceTypes).Any());
			AssertEquals("InvoiceType: No expected item should be missing", false, expectedInvoiceTypes.Except(provider.InvoiceTypeList.Cast<CodeDescriptionPair>().Select(x => x.Code)).Any());

			bizO.JobType = JobInvoicingConsumerTypes.AgencyVoyageAccounting.Code;
			expectedInvoiceTypes = new string[]
				{
					"DES", "DCU", "DCD",
					"DBT", "DBD", "FIN",
					"FID", "CUR", "CUD", "FRT",
					"ITC", "ITD", "SBR",
					"SBD"
				};

			AssertEquals("InvoiceType: No unexpected item should be found", false, provider.InvoiceTypeList.Cast<CodeDescriptionPair>().Select(x => x.Code).Except(expectedInvoiceTypes).Any());
			AssertEquals("InvoiceType: No expected item should be missing", false, expectedInvoiceTypes.Except(provider.InvoiceTypeList.Cast<CodeDescriptionPair>().Select(x => x.Code)).Any());
		}

		public void TestJobTypeValidation()
		{
			var bizO = new DummyInfoProviderConsumer(Factory);
			var provider = new JobTypeDirectionAndTransportInfoProvider(() => bizO.JobTypeInfo, () => bizO.DirectionInfo, () => bizO.TransportModeInfo);

			bizO.JobType = string.Empty;
			provider.ValidateJobType(false);
			AssertNoError("No Error", bizO.JobTypeInfo, "Please enter a value.");

			provider.ValidateJobType();
			AssertHasError("Has Error", bizO.JobTypeInfo, "Please enter a value.");

			bizO.JobTypeInfo.ClearAllNotifications();
			bizO.JobType = "ZZZ";
			provider.ValidateJobType();
			AssertHasError("Has Error", bizO.JobTypeInfo, "Enter a valid selection.");

			bizO.JobTypeInfo.ClearAllNotifications();
			bizO.JobType = JobInvoicingConsumerTypes.Shipment.Code;
			provider.ValidateJobType();
			AssertNoError("No Error", bizO.JobTypeInfo, "Enter a valid selection.");
		}

		public void TestDirectionValidation()
		{
			var bizO = new DummyInfoProviderConsumer(Factory);
			var provider = new JobTypeDirectionAndTransportInfoProvider(() => bizO.JobTypeInfo, () => bizO.DirectionInfo, () => bizO.TransportModeInfo);

			bizO.JobType = string.Empty;
			bizO.Direction = string.Empty;

			provider.ValidateDirection(false);
			AssertNoError("No Error", bizO.DirectionInfo, "Please enter a value.");

			provider.ValidateDirection(true);
			AssertNoError("No Error", bizO.DirectionInfo, "Please enter a value.");

			bizO.JobType = JobInvoicingConsumerTypes.Shipment.Code;
			provider.ValidateDirection(true);
			AssertHasError("Has Error", bizO.DirectionInfo, "Please enter a value.");

			bizO.DirectionInfo.ClearAllNotifications();
			bizO.Direction = "ZZZ";
			provider.ValidateDirection();
			AssertHasError("Has Error", bizO.DirectionInfo, "Enter a valid selection.");

			bizO.JobTypeInfo.ClearAllNotifications();
			bizO.JobType = "IMP";
			provider.ValidateDirection();
			AssertNoError("No Error", bizO.JobTypeInfo, "Enter a valid selection.");
		}

		public void TestTransportModeValidation()
		{
			var bizO = new DummyInfoProviderConsumer(Factory);
			var provider = new JobTypeDirectionAndTransportInfoProvider(() => bizO.JobTypeInfo, () => bizO.DirectionInfo, () => bizO.TransportModeInfo);

			bizO.JobType = string.Empty;
			bizO.TransportMode = string.Empty;

			provider.ValidateTransportMode(false);
			AssertNoError("No Error", bizO.TransportModeInfo, "Please enter a value.");

			provider.ValidateTransportMode(true);
			AssertNoError("No Error", bizO.TransportModeInfo, "Please enter a value.");

			bizO.JobType = JobInvoicingConsumerTypes.Shipment.Code;
			provider.ValidateTransportMode(true);
			AssertHasError("Has Error", bizO.TransportModeInfo, "Please enter a value.");

			bizO.TransportModeInfo.ClearAllNotifications();
			bizO.TransportMode = "ZZZ";
			provider.ValidateTransportMode();
			AssertHasError("Has Error", bizO.TransportModeInfo, "Enter a valid selection.");

			bizO.TransportModeInfo.ClearAllNotifications();
			bizO.TransportMode = "SEA";
			provider.ValidateTransportMode();
			AssertNoError("No Error", bizO.TransportModeInfo, "Enter a valid selection.");
		}

		public void TestReadOnly()
		{
			var bizO = new DummyInfoProviderConsumer(Factory);
			var provider = new JobTypeDirectionAndTransportInfoProvider(() => bizO.JobTypeInfo, () => bizO.DirectionInfo, () => bizO.TransportModeInfo);

			bizO.JobType = string.Empty;
			AssertEquals("Direction: Shouldn't be read only", false, provider.IsDirectionReadonly);
			AssertEquals("TransportMode: Shouldn't be read only", false, provider.IsTransportModeReadonly);

			bizO.JobType = JobInvoicingConsumerTypes.Brokerage.Code;
			AssertEquals("Direction: Shouldn't be read only", false, provider.IsDirectionReadonly);
			AssertEquals("TransportMode: Shouldn't be read only", false, provider.IsTransportModeReadonly);

			bizO.JobType = JobInvoicingConsumerTypes.Shipment.Code;
			AssertEquals("Direction: Shouldn't be read only", false, provider.IsDirectionReadonly);
			AssertEquals("TransportMode: Shouldn't be read only", false, provider.IsTransportModeReadonly);
		}

		sealed class DummyInfoProviderConsumer : NonPersistentBusinessObject, IObsoleteValidation
		{
			public DummyInfoProviderConsumer(BusinessObjectFactory factory)
				: base(factory)
			{
				provider = new JobTypeDirectionAndTransportInfoProvider(() => JobTypeInfo, () => DirectionInfo, () => TransportModeInfo);
			}
			readonly JobTypeDirectionAndTransportInfoProvider provider;

			#region JobType
			[List(nameof(JobTypeList))]
			public ZString JobType
			{
				get { return jobType; }
				set { SetNonPersistentPropertyValue(DirectionInfo, ref jobType, value); }
			}
			ZString jobType;

			public ZPropertyInfo JobTypeInfo
			{
				get { return GetZPropertyInfo(Schema.JobType); }
			}

			#endregion

			#region Direction
			[List(nameof(DirectionList))]
			public ZString Direction
			{
				get { return direction; }
				set { SetNonPersistentPropertyValue(DirectionInfo, ref direction, value); }
			}
			ZString direction;

			public ZPropertyInfo DirectionInfo
			{
				get { return GetZPropertyInfo(Schema.Direction); }
			}

			#endregion

			#region TransportMode
			[List(nameof(TransportModeList))]
			public ZString TransportMode
			{
				get { return transportMode; }
				set { SetNonPersistentPropertyValue(TransportModeInfo, ref transportMode, value); }
			}
			ZString transportMode;

			public ZPropertyInfo TransportModeInfo
			{
				get { return GetZPropertyInfo(Schema.TransportMode); }
			}

			#endregion

			CodeDescriptionPairList JobTypeList
			{
				get { return provider.JobTypeList; }
			}

			CodeDescriptionPairList TransportModeList
			{
				get { return provider.TransportModeList; }
			}

			CodeDescriptionPairList DirectionList
			{
				get { return provider.DirectionList; }
			}

			public abstract class Schema
			{
				public const string JobType = "JobType";
				public const string Direction = "Direction";
				public const string TransportMode = "TransportMode";
			}
		}
	}
}
