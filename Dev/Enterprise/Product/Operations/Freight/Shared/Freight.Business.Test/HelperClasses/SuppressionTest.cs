using System;
using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Freight.Business.Testing
{
	public class SuppressionTest : TestCaseWithFactory
	{
		public void TestEnabledForAnyOfFieldsWithCurrentBranchAndCompanyHasNoCountryCode()
		{
			using (new DocumentEngine.TemporaryValueSetter<string>(value => GlbBranch.CurrentBranch.GB_RL_NKHomePort = value, GlbBranch.CurrentBranch.GB_RL_NKHomePort, ""))
			using (new DocumentEngine.TemporaryValueSetter<string>(value => GlbCompany.CurrentCompany.GC_RN_NKCountryCode = value, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, ""))
			{
				var bizO = new DummySuppressionBizO { IsAir = true, JobDirection = Directions.Export };
				var testList = new List<SuppressFields> { SuppressFields.MasterBill, SuppressFields.Carrier };
				AssertNoExceptionThrown(() => Suppression.EnabledForAnyOfFields(bizO, testList.ToArray(), ContactType.Consignor));
			}
		}

		public void TestBizOAndTypeAreCached()
		{
			SuppressionForTest.CacheObjectClear();
			SetSuppressingFields(DocumentsDataRegistry.Instance.SuppressPrintingOfFlightDate, new List<SuppressFields>());

			DummySuppressionBizO bizO = new DummySuppressionBizO { IsAir = true, JobDirection = Directions.Export };
			List<SuppressFields> testList = new List<SuppressFields> { SuppressFields.MasterBill, SuppressFields.Carrier };
			AssertEquals(0, SuppressionForTest.CacheObjectCount());
			Globals.IsWeb = true;
			Suppression.EnabledForAnyOfFields(bizO, testList.ToArray(), ContactType.Consignor);
			AssertEquals(2, SuppressionForTest.CacheObjectCount());
			Globals.IsWeb = false;
		}

		public void TestEnabledByTypeAndRegistry()
		{
			SetSuppressingFields(DocumentsDataRegistry.Instance.SuppressPrintingOfFlightDate, new List<SuppressFields>());

			DummySuppressionBizO bizO = new DummySuppressionBizO { IsAir = true, JobDirection = Directions.Export };
			List<SuppressFields> testList = new List<SuppressFields> { SuppressFields.MasterBill, SuppressFields.Carrier };
			Assert(!Suppression.EnabledForAnyOfFields(bizO, testList.ToArray(), ContactType.Consignor));

			SetSuppressingFields(DocumentsDataRegistry.Instance.SuppressPrintingOfFlightDate, new List<SuppressFields> { SuppressFields.TransportInfo, SuppressFields.ETA, SuppressFields.ETD });
			Assert(!Suppression.EnabledForAnyOfFields(bizO, testList.ToArray(), ContactType.Consignor));

			SetSuppressingFields(DocumentsDataRegistry.Instance.SuppressPrintingOfFlightDate, new List<SuppressFields> { SuppressFields.MasterBill });
			Assert(Suppression.EnabledForAnyOfFields(bizO, testList.ToArray(), ContactType.Consignor));

			bizO.JobDirection = Directions.Unknown;
			Assert(!Suppression.EnabledForAnyOfFields(bizO, testList.ToArray(), ContactType.Consignor));

			Globals.IsWeb = true;
			try
			{
				Assert(!Suppression.EnabledForAnyOfFieldsWeb(bizO, testList.ToArray()));

				SetSuppressingFields(WebDataRegistry.Instance.SuppressFlightDetailsForDomestic, new List<SuppressFields>());
				SetSuppressingFields(WebDataRegistry.Instance.SuppressFlightDetailsForExport, new List<SuppressFields> { SuppressFields.Carrier, SuppressFields.ETA, SuppressFields.ETD, SuppressFields.MasterBill });
				SetSuppressingFields(WebDataRegistry.Instance.SuppressFlightDetailsForForeign, new List<SuppressFields>());
				SetSuppressingFields(WebDataRegistry.Instance.SuppressFlightDetailsForImport, new List<SuppressFields> { SuppressFields.Carrier, SuppressFields.ETA, SuppressFields.ETD, SuppressFields.MasterBill });

				bizO.JobDirection = Directions.Domestic;
				Assert(!Suppression.EnabledForAnyOfFieldsWeb(bizO, testList.ToArray()));

				bizO.JobDirection = Directions.Export;
				Assert(Suppression.EnabledForAnyOfFieldsWeb(bizO, testList.ToArray()));

				bizO.JobDirection = Directions.CrossTrade;
				Assert(!Suppression.EnabledForAnyOfFieldsWeb(bizO, testList.ToArray()));

				bizO.JobDirection = Directions.Import;
				Assert(Suppression.EnabledForAnyOfFieldsWeb(bizO, testList.ToArray()));

				SetSuppressingFields(WebDataRegistry.Instance.SuppressFlightDetailsForDomestic, new List<SuppressFields> { SuppressFields.Carrier, SuppressFields.ETA, SuppressFields.ETD, SuppressFields.MasterBill });
				SetSuppressingFields(WebDataRegistry.Instance.SuppressFlightDetailsForExport, new List<SuppressFields>());
				SetSuppressingFields(WebDataRegistry.Instance.SuppressFlightDetailsForForeign, new List<SuppressFields> { SuppressFields.Carrier, SuppressFields.ETA, SuppressFields.ETD, SuppressFields.MasterBill });
				SetSuppressingFields(WebDataRegistry.Instance.SuppressFlightDetailsForImport, new List<SuppressFields>());
				SuppressionForTest.CacheObjectClear();
				Assert(!Suppression.EnabledForAnyOfFieldsWeb(bizO, testList.ToArray()));

				bizO.JobDirection = Directions.Domestic;
				Assert(Suppression.EnabledForAnyOfFieldsWeb(bizO, testList.ToArray()));

				bizO.JobDirection = Directions.Export;
				Assert(!Suppression.EnabledForAnyOfFieldsWeb(bizO, testList.ToArray()));

				bizO.JobDirection = Directions.CrossTrade;
				Assert(Suppression.EnabledForAnyOfFieldsWeb(bizO, testList.ToArray()));
			}
			finally
			{
				Globals.IsWeb = false;
			}
		}

		public void TestNullHandlings()
		{
			AssertEquals("'GetValue' should handle null", "text", Suppression.GetValue("text", null, SuppressFields.Carrier, "*", ContactType.Consignor));
			AssertEquals("'GetValue<T>' should handle null", new ZDateTime(2001, 9, 11), Suppression.GetValue(new ZDateTime(2001, 9, 11), null, SuppressFields.Carrier, ContactType.Consignor));
			AssertEquals("'GetObject' should handle null", new DateTime(2001, 9, 11), Suppression.GetObject(new DateTime(2001, 9, 11), null, SuppressFields.Carrier, new DateTime(2009, 9, 16), ContactType.Consignor));
			Assert("'EnabledForAnyOfFields' should handle null", !Suppression.EnabledForAnyOfFields(null, new[] { SuppressFields.MasterBill }, ContactType.Consignor));
		}

		public void TestEnabledByCountrySpecifics()
		{
			List<SuppressFields> testList = new List<SuppressFields> { SuppressFields.MasterBill };
			SetSuppressingFields(DocumentsDataRegistry.Instance.SuppressPrintingOfFlightDate, testList);
			DummySuppressionBizO bizO = new DummySuppressionBizO { JobDirection = Directions.Export };
			ZString originalBranchPort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;

			GlbBranch.CurrentBranch.GB_RL_NKHomePort = "AUSYD";
			try
			{
				Assert("BizO is not Air -- should not be suppressed", !Suppression.EnabledForAnyOfFields(bizO, testList.ToArray(), ContactType.Consignor));

				bizO.IsAir = true;
				Assert(Suppression.EnabledForAnyOfFields(bizO, testList.ToArray(), ContactType.Consignor));

				bizO.HasActualRCVPassed = true;
				Assert(!Suppression.EnabledForAnyOfFields(bizO, testList.ToArray(), ContactType.Consignor));

				bizO.HasActualRCVPassed = false;
				bizO.HasETDPassed = true;
				Assert(!Suppression.EnabledForAnyOfFields(bizO, testList.ToArray(), ContactType.Consignor));

				bizO.HasActualRCVPassed = true;
				Assert(!Suppression.EnabledForAnyOfFields(bizO, testList.ToArray(), ContactType.Consignor));

				bizO.HasActualRCVPassed = false;
				bizO.HasETDPassed = false;
				GlbBranch.CurrentBranch.GB_RL_NKHomePort = "USNYC";
				Assert("It is not a passenger flight and this is US -- should not be suppressed", !Suppression.EnabledForAnyOfFields(bizO, testList.ToArray(), ContactType.Consignor));

				bizO.IsPassengerFlight = true;
				bizO.IsAir = false;
				Assert(!Suppression.EnabledForAnyOfFields(bizO, testList.ToArray(), ContactType.Consignor));

				bizO.IsAir = true;
				Assert(Suppression.EnabledForAnyOfFields(bizO, testList.ToArray(), ContactType.Consignor));

				bizO.HasFinalRoutingLegATDPassed = true;
				Assert(!Suppression.EnabledForAnyOfFields(bizO, testList.ToArray(), ContactType.Consignor));

				bizO.HasFinalRoutingLegATDPassed = false;
				bizO.IsPassengerFlight = false;
				bizO.IsAir = false;
				GlbBranch.CurrentBranch.GB_RL_NKHomePort = "NZAKL";
				Assert("BizO is not Air -- should not be suppressed", !Suppression.EnabledForAnyOfFields(bizO, testList.ToArray(), ContactType.Consignor));

				bizO.IsAir = true;
				Assert(Suppression.EnabledForAnyOfFields(bizO, testList.ToArray(), ContactType.Consignor));

				bizO.HasActualRCVPassed = true;
				Assert(!Suppression.EnabledForAnyOfFields(bizO, testList.ToArray(), ContactType.Consignor));

				bizO.HasActualRCVPassed = false;
				bizO.HasETDPassed = true;
				Assert(!Suppression.EnabledForAnyOfFields(bizO, testList.ToArray(), ContactType.Consignor));

				bizO.HasActualRCVPassed = true;
				Assert(!Suppression.EnabledForAnyOfFields(bizO, testList.ToArray(), ContactType.Consignor));
			}
			finally
			{
				GlbBranch.CurrentBranch.GB_RL_NKHomePort = originalBranchPort;
			}
		}

		public void TestSuppressionOnlyWorksForConsignors()
		{
			DummySuppressionBizO bizO = new DummySuppressionBizO { IsAir = true, JobDirection = Directions.Export };
			SuppressFields[] testArray = new[] { SuppressFields.MasterBill, SuppressFields.Carrier };
			SetSuppressingFields(DocumentsDataRegistry.Instance.SuppressPrintingOfFlightDate, new List<SuppressFields> { SuppressFields.MasterBill });

			Assert(!Suppression.EnabledForAnyOfFields(bizO, testArray, null));
			Assert(!Suppression.EnabledForAnyOfFields(bizO, testArray, ContactType.Consignee));
			Assert(!Suppression.EnabledForAnyOfFields(bizO, testArray, ContactType.CustomerService));
			Assert(!Suppression.EnabledForAnyOfFields(bizO, testArray, ContactType.Sales));
			Assert(!Suppression.EnabledForAnyOfFields(bizO, testArray, ContactType.Receivables));
			Assert(!Suppression.EnabledForAnyOfFields(bizO, testArray, ContactType.ShippingLine));
			Assert(!Suppression.EnabledForAnyOfFields(bizO, testArray, ContactType.Marketing));
			Assert(!Suppression.EnabledForAnyOfFields(bizO, testArray, ContactType.Warehouse));

			Assert(Suppression.EnabledForAnyOfFields(bizO, testArray, ContactType.Consignor));
		}

		#region Implementation

		public static void SetSuppressingFields(CodeDescriptionBoolRegistryItem item, List<SuppressFields> yesTypes)
		{
			item.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, RegistrySuppressionHelper.GetCollection(yesTypes));
		}

		public static void SetSuppressingFields(CodeDescriptionBoolRegistryItem item, bool allTo)
		{
			var suppressFields = new List<SuppressFields>();

			if (allTo)
			{
				foreach (CodeDescriptionBool field in item.DefaultValue)
				{
					SuppressFields type;
					if (RegistrySuppressionHelper.TryGetType(field.Code, out type))
					{
						suppressFields.Add(type);
					}
				}
			}

			SetSuppressingFields(item, allTo ? suppressFields : null);
		}

		public class DummySuppressionBizO : IFlightDetailsSuppression
		{
			public bool IsAir { get; set; }
			public ZBool HasActualRCVPassed { get; set; }
			public ZBool HasETDPassed { get; set; }
			public ZBool IsPassengerFlight { get; set; }
			public ZBool HasFinalRoutingLegATDPassed { get; set; }

			public Directions JobDirection
			{
				get { return jobDirection; }
				set
				{
					SuppressionForTest.CacheObjectClear();
					jobDirection = value;
				}
			}
			Directions jobDirection;
		}

		protected override void TearDown()
		{
			SuppressionForTest.CacheObjectClear();
			base.TearDown();
		}

		#endregion
	}
}
