using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Registry.Business;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.Testing
{
	sealed class HouseBillChargesTest : TestCaseWithFactory
	{
		#region TestChargesDisplay_NoCharges

		public void TestChargesDisplay_NoCharges()
		{
			var shipment = CreateShipmentWithCharges();

			AssertChargesDisplay(DocumentsDataRegistry.HBLChargesDisplayTypes.NoCharges,
				shipment,
				isOriginal: true,
				expectedHideCharges: true,
				expectedShowAsAgreed: false,
				expectedShowCollect: false,
				expectedShowPrepaid: false);

			AssertChargesDisplay(DocumentsDataRegistry.HBLChargesDisplayTypes.NoCharges,
				shipment,
				isOriginal: false,
				expectedHideCharges: true,
				expectedShowAsAgreed: false,
				expectedShowCollect: false,
				expectedShowPrepaid: false);
		}

		#endregion

		#region TestChargesDisplay_AsAgreed

		public void TestChargesDisplay_AsAgreed()
		{
			var shipment = CreateShipmentWithCharges();

			AssertChargesDisplay(DocumentsDataRegistry.HBLChargesDisplayTypes.AsAgreed,
				shipment,
				isOriginal: true,
				expectedHideCharges: false,
				expectedShowAsAgreed: true,
				expectedShowCollect: false,
				expectedShowPrepaid: false);

			AssertChargesDisplay(DocumentsDataRegistry.HBLChargesDisplayTypes.AsAgreed,
				shipment,
				isOriginal: false,
				expectedHideCharges: false,
				expectedShowAsAgreed: true,
				expectedShowCollect: false,
				expectedShowPrepaid: false);
		}

		#endregion

		#region TestChargesDisplay_CollectCharges

		public void TestChargesDisplay_CollectCharges()
		{
			var shipment = CreateShipmentWithCharges();

			AssertChargesDisplay(DocumentsDataRegistry.HBLChargesDisplayTypes.CollectCharges,
				shipment,
				isOriginal: true,
				expectedHideCharges: false,
				expectedShowAsAgreed: false,
				expectedShowCollect: true,
				expectedShowPrepaid: false);

			AssertChargesDisplay(DocumentsDataRegistry.HBLChargesDisplayTypes.CollectCharges,
				shipment,
				isOriginal: false,
				expectedHideCharges: false,
				expectedShowAsAgreed: false,
				expectedShowCollect: true,
				expectedShowPrepaid: false);
		}

		#endregion

		#region TestChargesDisplay_PrepaidCharges

		public void TestChargesDisplay_PrepaidCharges()
		{
			var shipment = CreateShipmentWithCharges();

			AssertChargesDisplay(DocumentsDataRegistry.HBLChargesDisplayTypes.PrepaidCharges,
				shipment,
				isOriginal: true,
				expectedHideCharges: false,
				expectedShowAsAgreed: false,
				expectedShowCollect: false,
				expectedShowPrepaid: true);

			AssertChargesDisplay(DocumentsDataRegistry.HBLChargesDisplayTypes.PrepaidCharges,
				shipment,
				isOriginal: false,
				expectedHideCharges: false,
				expectedShowAsAgreed: false,
				expectedShowCollect: false,
				expectedShowPrepaid: true);
		}

		#endregion

		#region TestChargesDisplay_PrepaidAndCollectCharges

		public void TestChargesDisplay_PrepaidAndCollectCharges()
		{
			var shipment = CreateShipmentWithCharges();

			AssertChargesDisplay(DocumentsDataRegistry.HBLChargesDisplayTypes.PrepaidAndCollectCharges,
				shipment,
				isOriginal: true,
				expectedHideCharges: false,
				expectedShowAsAgreed: false,
				expectedShowCollect: true,
				expectedShowPrepaid: true);

			AssertChargesDisplay(DocumentsDataRegistry.HBLChargesDisplayTypes.PrepaidAndCollectCharges,
				shipment,
				isOriginal: false,
				expectedHideCharges: false,
				expectedShowAsAgreed: false,
				expectedShowCollect: true,
				expectedShowPrepaid: true);
		}

		#endregion

		#region TestChargesDisplay_OriginalAsAgreedCopyWithCollectCharges

		public void TestChargesDisplay_OriginalAsAgreedCopyWithCollectCharges()
		{
			var shipment = CreateShipmentWithCharges();

			AssertChargesDisplay(DocumentsDataRegistry.HBLChargesDisplayTypes.OriginalAsAgreedCopyWithCollectCharges,
				shipment,
				isOriginal: true,
				expectedHideCharges: false,
				expectedShowAsAgreed: true,
				expectedShowCollect: false,
				expectedShowPrepaid: false);

			AssertChargesDisplay(DocumentsDataRegistry.HBLChargesDisplayTypes.OriginalAsAgreedCopyWithCollectCharges,
				shipment,
				isOriginal: false,
				expectedHideCharges: false,
				expectedShowAsAgreed: false,
				expectedShowCollect: true,
				expectedShowPrepaid: false);
		}

		#endregion

		#region TestChargesDisplay_OriginalAsAgreedCopyWithPrepaidCharges

		public void TestChargesDisplay_OriginalAsAgreedCopyWithPrepaidCharges()
		{
			var shipment = CreateShipmentWithCharges();

			AssertChargesDisplay(DocumentsDataRegistry.HBLChargesDisplayTypes.OriginalAsAgreedCopyWithPrepaidCharges,
				shipment,
				isOriginal: true,
				expectedHideCharges: false,
				expectedShowAsAgreed: true,
				expectedShowCollect: false,
				expectedShowPrepaid: false);

			AssertChargesDisplay(DocumentsDataRegistry.HBLChargesDisplayTypes.OriginalAsAgreedCopyWithPrepaidCharges,
				shipment,
				isOriginal: false,
				expectedHideCharges: false,
				expectedShowAsAgreed: false,
				expectedShowCollect: false,
				expectedShowPrepaid: true);
		}

		#endregion

		#region TestChargesDisplay_OriginalAsAgreedCopyWithPrepaidCharges

		public void TestChargesDisplay_OriginalAsAgreedCopyWithPrepaidAndCollectCharges()
		{
			var shipment = CreateShipmentWithCharges();

			AssertChargesDisplay(DocumentsDataRegistry.HBLChargesDisplayTypes.OriginalAsAgreedCopyWithPrepaidAndCollectCharges,
				shipment,
				isOriginal: true,
				expectedHideCharges: false,
				expectedShowAsAgreed: true,
				expectedShowCollect: false,
				expectedShowPrepaid: false);

			AssertChargesDisplay(DocumentsDataRegistry.HBLChargesDisplayTypes.OriginalAsAgreedCopyWithPrepaidAndCollectCharges,
				shipment,
				isOriginal: false,
				expectedHideCharges: false,
				expectedShowAsAgreed: false,
				expectedShowCollect: true,
				expectedShowPrepaid: true);
		}

		#endregion

		#region TestChargesDisplay_NoDebtor

		public void TestChargesDisplay_NoDebtor()
		{
			var collection = new PrintChargesBilledToLocalClientAtDestAsCollectCollection();

			using (FreightDataRegistry.Instance.PrintChargesBilledToLocalClientAtDestAsCollect.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, collection))
			{
				var shipment = CreateShipment();
				var header = shipment.JobHeader;

				CreateLineCharge(header, header.LocalChargesPK, 1000m, "FRT", "AUD");
				CreateLineCharge(header, new ZGuid(), 500m, "OLAB", "AUD");
				CreateLineCharge(header, header.LocalChargesPK, 750m, "DLAB", "USD");
				CreateLineCharge(header, header.AgentCollectPK, 800m, "FRT", "AUD");
				CreateLineCharge(header, new ZGuid(), 300m, "DDOC", "AUD");
				CreateLineCharge(header, header.AgentCollectPK, 150m, "ODOC", "USD");

				var lookups = new HouseBillLookups(Factory);
				var chargesCollection = new ChargesCollection(shipment, lookups, false);

				AssertEquals("6 Charges in total", 6, chargesCollection.All.Count);
				AssertEquals("4 Charges with Debtor in total", 4, chargesCollection.Count);
				AssertEquals("Charges with Debtor - Lump Sum", "FREIGHT LUMP SUM: 3060.00 USD", chargesCollection.LumpSum.ToString());

				var usd = shipment.Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, Core.Constants.CurrencyCodes.UnitedStates);
				AssertEquals(chargesCollection.LumpSum.Amount, chargesCollection.Sum(ch =>
				{
					var refCurrency = shipment.Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, ch.Sell.Currency.Code);
					var money = new MasterFiles.Business.Money(ch.Sell.Amount, refCurrency);
					return shipment.JobHeader.CurrencyConverter.ConvertRounded(money, usd).Amount;
				}));
			}
		}

		#endregion

		#region AssertChargesDisplay

		void AssertChargesDisplay(string chargesDisplay, ForwardingShipment shipment, bool isOriginal, bool expectedHideCharges, bool expectedShowAsAgreed, bool expectedShowCollect, bool expectedShowPrepaid)
		{
			shipment.JS_HBLAWBChargesDisplay = chargesDisplay;

			var lookups = new HouseBillLookups(Factory);
			var chargesCollection = new ChargesCollection(shipment, lookups, isOriginal);

			CombineAssertions(() =>
			{
				AssertEquals($"'{chargesDisplay}', IsOriginal: {isOriginal}; Hide", expectedHideCharges, chargesCollection.Hide);
				AssertEquals($"'{chargesDisplay}', IsOriginal: {isOriginal}; ShowAsAgreed", expectedShowAsAgreed, chargesCollection.ShowAsAgreed);
				AssertEquals($"'{chargesDisplay}', IsOriginal: {isOriginal}; ShowCollect", expectedShowCollect, chargesCollection.ShowCollect);
				AssertEquals($"'{chargesDisplay}', IsOriginal: {isOriginal}; ShowPrepaid", expectedShowPrepaid, chargesCollection.ShowPrepaid);

				if (expectedHideCharges)
				{
					AssertEquals("collection is empty when charges are hidden",
						false, chargesCollection.Any());
				}

				AssertEquals("collection has collect charges",
					expectedShowCollect, chargesCollection.Any(ch => !ch.IsPrepaid));

				AssertEquals("collection has prepaid charges",
					expectedShowPrepaid, chargesCollection.Any(ch => ch.IsPrepaid));

				var chargesQuery = new ZQuery(JobChargeSchema.JR_JH, shipment.JobHeader.PK);
				var shipmentCharges = Factory.Load<JobCharge>(chargesQuery);

				AssertEquals("all charges count",
					shipmentCharges.Length, chargesCollection.All.Count);
			});
		}

		#endregion

		#region TestLumpSum

		public void TestLumpSum()
		{
			var shipment = CreateShipmentWithCharges();
			var lookups = new HouseBillLookups(Factory);

			using (DocumentsDataRegistry.Instance.BOLLumpSumDisplayCountries.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new Guid[] { Core.Constants.CountryGuids.Brazil }))
			{
				var chargesCollection = new ChargesCollection(shipment, lookups, false);

				AssertEquals("ShowAsLumpSum", false, chargesCollection.ShowAsLumpSum);
			}

			using (DocumentsDataRegistry.Instance.BOLLumpSumDisplayCountries.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new Guid[] { Core.Constants.CountryGuids.UnitedStates }))
			{
				var chargesCollection = new ChargesCollection(shipment, lookups, true);

				AssertEquals("ShowAsLumpSum", true, chargesCollection.ShowAsLumpSum);
				AssertEquals("LumpSum", "FREIGHT LUMP SUM: 2550.00 USD", chargesCollection.LumpSum.ToString());
			}
		}

		public void TestLumpSum_ProfitShareCharge()
		{
			var collection = new PrintChargesBilledToLocalClientAtDestAsCollectCollection();

			using (FreightDataRegistry.Instance.PrintChargesBilledToLocalClientAtDestAsCollect.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, collection))
			{
				var shipment = CreateShipment();
				var header = shipment.JobHeader;

				CreateLineCharge(header, header.LocalChargesPK, 1000m, "FRT", "AUD");
				CreateLineCharge(header, header.AgentCollectPK, 500m, "OLAB", "AUD");
				CreateLineCharge(header, header.LocalChargesPK, 750m, "PS", "USD");

				var lookups = new HouseBillLookups(Factory);
				var chargesCollection = new ChargesCollection(shipment, lookups, false);

				AssertEquals(2, chargesCollection.All.Count);
				AssertEquals(2, chargesCollection.Count);
				AssertEquals("FREIGHT LUMP SUM: 1500.00 AUD", chargesCollection.LumpSum.ToString());

				AssertEquals(chargesCollection.LumpSum.Amount, chargesCollection.Sum(ch => ch.Sell.Amount));
			}
		}

		public void TestLumpSum_OverseasOfficeCharges()
		{
			var collection = new PrintChargesBilledToLocalClientAtDestAsCollectCollection();
			CreatePrintChargesBilledToLocalClientAtDestAsCollect(collection, Core.Constants.TransportModes.Sea, "FR", "NZ");

			using (FreightDataRegistry.Instance.PrintChargesBilledToLocalClientAtDestAsCollect.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, collection))
			{
				var frOrgProxy = CreateOrg("FRPROORG", "FRProxy", "FRProxy Address", "FR", "FRPAR");
				var nzOrgProxy = CreateOrg("NZPROORG", "NZProxy", "NZProxy Address", "NZ", "NZAKL");
				var cnOrgProxy = CreateOrg("CNPROORG", "CNProxy", "CNProxy Address", "CN", "CNSHA");

				var frCompany = CreateCompany("C#1", "FR Test", "B#1", "Brach 1", "FRPAR", "AUD", frOrgProxy.PK);
				var nzCompany = CreateCompany("C#2", "NZ Test", "B#2", "Brach 2", "NZAKL", "NZD", nzOrgProxy.PK);
				var cnCompany = CreateCompany("C#3", "CN Test", "B#3", "Brach 3", "CNSHA", "CNY", cnOrgProxy.PK);

				var consignor = CreateOrg("CONSIGNOR", "Consignor", "Consignor Address", "FR", "FRPAR");
				consignor.OH_IsConsignor = true;
				consignor.OH_IsDebtor = true;

				var consignee = CreateOrg("CONSIGNEE", "Consignee", "Consignee Address", "NZ", "NZAKL");
				consignee.OH_IsConsignee = true;
				consignee.OH_IsDebtor = true;

				CreateAccChargeCode("DDOC", nzCompany.PK);
				CreateAccChargeCode("521", nzCompany.PK);
				CreateAccChargeCode("CAF", nzCompany.PK);

				Factory.Save();

				ZGuid shipmentPK;

				using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, frCompany.Branches.First().PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
				{
					var factory2 = new BusinessObjectFactory();

					var consignor2 = factory2.Load<OrgHeader>(consignor.PK);
					var consignee2 = factory2.Load<OrgHeader>(consignee.PK);
					var nzOrgProxy2 = factory2.Load<OrgHeader>(nzOrgProxy.PK);
					consignor2.CompanyData.OB_IsDebtor = true;
					consignee2.CompanyData.OB_IsDebtor = true;
					nzOrgProxy2.CompanyData.OB_IsDebtor = true;

					var shipment1 = factory2.New<ForwardingShipment>();
					shipmentPK = shipment1.PK;

					shipment1.JS_TransportMode = Core.Constants.TransportModes.Sea;
					shipment1.JS_RL_NKOrigin = "FRPAR";
					shipment1.JS_RL_NKDestination = "NZAKL";
					shipment1.JS_HBLAWBChargesDisplay = "ALL";
					shipment1.JS_UniqueConsignRef = "S00005000";

					shipment1.ConsignorPK = consignor2.PK;
					shipment1.ConsigneePK = consignee2.PK;

					var loader = new JobHeader.Loader(shipment1);
					var header = loader.TryLoadOrCreateWithMutex();
					header.LocalChargesPK = consignor2.PK;
					header.AgentCollectPK = nzOrgProxy2.PK;

					CreateLineCharge(shipment1.JobHeader, shipment1.JobHeader.LocalChargesPK, 15m, "FRT", "AUD", otherFactory: factory2);
					CreateLineCharge(shipment1.JobHeader, shipment1.JobHeader.AgentCollectPK, 5m, "BAF", "NZD", otherFactory: factory2);

					factory2.Save();
				}

				using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, nzCompany.Branches.First().PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
				{
					var factory3 = new BusinessObjectFactory();

					var consignor3 = factory3.Load<OrgHeader>(consignor.PK);
					var consignee3 = factory3.Load<OrgHeader>(consignee.PK);
					var frOrgProxy3 = factory3.Load<OrgHeader>(frOrgProxy.PK);
					consignor3.CompanyData.OB_IsDebtor = true;
					consignee3.CompanyData.OB_IsDebtor = true;
					frOrgProxy3.CompanyData.OB_IsDebtor = true;

					var shipment2 = factory3.Load<ForwardingShipment>(shipmentPK);

					var loader2 = new JobHeader.Loader(shipment2);
					var header2 = loader2.TryLoadOrCreateWithMutex();
					header2.LocalChargesPK = consignee3.PK;
					header2.AgentCollectPK = frOrgProxy3.PK;

					CreateLineCharge(shipment2.JobHeader, shipment2.JobHeader.LocalChargesPK, 40m, "DDOC", "NZD", otherFactory: factory3);
					CreateLineCharge(shipment2.JobHeader, shipment2.JobHeader.AgentCollectPK, 30m, "CAF", "AUD", otherFactory: factory3);
					CreateLineCharge(shipment2.JobHeader, shipment2.JobHeader.LocalChargesPK, 20m, "521", "NZD", otherFactory: factory3);

					factory3.Save();
				}

				using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, frCompany.Branches.First().PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
				{
					var factory4 = new BusinessObjectFactory();
					var shipment3 = factory4.Load<ForwardingShipment>(shipmentPK);

					var lookups = new HouseBillLookups(factory4);
					var chargesCollection = new ChargesCollection(shipment3, lookups, true);

					AssertEquals("FREIGHT LUMP SUM: 75.00 USD", chargesCollection.LumpSum.ToString());
				}

				using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, nzCompany.Branches.First().PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
				{
					var factory5 = new BusinessObjectFactory();
					var shipment4 = factory5.Load<ForwardingShipment>(shipmentPK);

					var lookups = new HouseBillLookups(factory5);
					var chargesCollection = new ChargesCollection(shipment4, lookups, true);

					AssertEquals("FREIGHT LUMP SUM: 75.00 USD", chargesCollection.LumpSum.ToString());
				}

				using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, cnCompany.Branches.First().PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
				{
					var factory6 = new BusinessObjectFactory();
					var shipment5 = factory6.Load<ForwardingShipment>(shipmentPK);

					var lookups = new HouseBillLookups(factory6);
					var chargesCollection = new ChargesCollection(shipment5, lookups, true);

					AssertEquals("FREIGHT LUMP SUM: 0.00 USD", chargesCollection.LumpSum.ToString());
				}
			}
		}

		#endregion

		#region TestCreateChargesOnShipmentWithNoJobHeader

		public void TestCreateChargesOnShipmentWithNoJobHeader()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var lookups = new HouseBillLookups(Factory);

			AssertNull("prerequisite; shipment has no JobHeader", shipment.JobHeader);

			var chargesCollection = new ChargesCollection(shipment, lookups, false);

			Assert("charges collection has no elements", !chargesCollection.Any());
		}

		#endregion

		#region TestCreateChargeForNegativeOrZeroAmounts

		public void TestChargesCollectionFilteredCharges_ShouldNotHaveCharge_WhenChargeAmountIsNegative()
		{
			var shipment = CreateShipment();
			var header = shipment.JobHeader;
			CreateLineCharge(header, header.LocalChargesPK, -100, "DLAB", "USD");
			var lookups = new HouseBillLookups(Factory);
			var chargesCollection = new ChargesCollection(shipment, lookups, false);
			AssertEquals("We populated 0 charge. Negative amount charge should not be populated.", 0, chargesCollection.Count);
		}

		public void TestChargesCollectionFilteredCharges_ShouldNotHaveCharge_WhenChargeAmountIsZero()
		{
			var shipment = CreateShipment();
			var header = shipment.JobHeader;
			CreateLineCharge(header, header.LocalChargesPK, 0, "DLAB", "USD");
			var lookups = new HouseBillLookups(Factory);
			var chargesCollection = new ChargesCollection(shipment, lookups, false);
			AssertEquals("We populated 0 charge. Zero amount charge should not be populated.", 0, chargesCollection.Count);
		}

		public void TestChargesDisplayFiltersZeroAmountCharges()
		{
			var shipment = CreateShipment();
			var header = shipment.JobHeader;
			CreateLineCharge(header, header.AgentCollectPK, 0, "DLAB", "USD");
			CreateLineCharge(header, header.AgentCollectPK, 0, "OLAB", "AUD");
			CreateLineCharge(header, header.LocalChargesPK, -250m, "FRT", "AUD");
			CreateLineCharge(header, header.AgentCollectPK, 500m, "OLAB", "AUD");
			CreateLineCharge(header, header.LocalChargesPK, 750m, "DLAB", "USD");

			var lookups = new HouseBillLookups(Factory);
			var chargesCollection = new ChargesCollection(shipment, lookups, false);

			AssertEquals("There are 5 charges in total", 5, chargesCollection.All.Count);
			AssertEquals("There are only 2 charges with higher than zero amounts", 2, chargesCollection.Count);
		}

		public void TestChargesCollectionFilteredCharges_ShouldExcludeChargesIfCancelledByCreditNote()
		{
			var shipment = CreateShipment();
			var header = shipment.JobHeader;
			CreateLineCharge(header, header.LocalChargesPK, 100m, "WAR", "AUD");
			CreateLineCharge(header, header.LocalChargesPK, 250m, "FRT", "AUD");
			CreateLineCharge(header, header.LocalChargesPK, -250m, "FRT", "AUD");

			var lookups = new HouseBillLookups(Factory);
			var chargesCollection = new ChargesCollection(shipment, lookups, false);

			AssertEquals("There should be 3 charges in total", 3, chargesCollection.All.Count);
			AssertEquals("Since the FRT charge is amended by credit note, both FRT charges are excluded.", 1, chargesCollection.Count);
		}

		public void TestChargesCollectionFilteredCharges_ShouldExcludeChargesIfCancelledByCreditNote_MultipleCurrencies()
		{
			var shipment = CreateShipment();
			var header = shipment.JobHeader;
			CreateLineCharge(header, header.LocalChargesPK, 100m, "WAR", "AUD");
			CreateLineCharge(header, header.LocalChargesPK, 250m, "FRT", "AUD");
			CreateLineCharge(header, header.LocalChargesPK, -250m, "FRT", "AUD");
			CreateLineCharge(header, header.LocalChargesPK, 350m, "FRT", "USD");
			CreateLineCharge(header, header.LocalChargesPK, -350m, "FRT", "USD");

			var lookups = new HouseBillLookups(Factory);
			var chargesCollection = new ChargesCollection(shipment, lookups, false);

			AssertEquals("There should be 5 charges in total", 5, chargesCollection.All.Count);
			AssertEquals("Since the FRT charge is amended by credit note, all FRT charges are excluded.", 1, chargesCollection.Count);
		}

		public void TestChargesCollectionFilteredCharges_ShouldExcludeChargesIfCancelledByCreditNote_RechargeShouldBeIncluded()
		{
			var shipment = CreateShipment();
			var header = shipment.JobHeader;
			CreateLineCharge(header, header.LocalChargesPK, 100m, "FRT", "AUD");
			CreateLineCharge(header, header.LocalChargesPK, 100m, "FRT", "AUD");
			CreateLineCharge(header, header.LocalChargesPK, -100m, "FRT", "AUD");

			var lookups = new HouseBillLookups(Factory);
			var chargesCollection = new ChargesCollection(shipment, lookups, false);

			AssertEquals("There should be 3 charges in total", 3, chargesCollection.All.Count);
			AssertEquals("It should only show the charge which was not reversed by credit note.", 1, chargesCollection.Count);
		}

		#endregion

		#region TestCreateChargesWithLocalAndOSCharges

		public void TestCreateChargesWithLocalAndOSCosts()
		{
			var shipment = CreateShipment();
			var header = shipment.JobHeader;

			CreateLineCharge(header, header.LocalChargesPK, 100, "DLAB", "USD");

			var lookups = new HouseBillLookups(Factory);
			var chargesCollection = new ChargesCollection(shipment, lookups, false);

			AssertEquals("We populated 1 charge", 1, chargesCollection.Count);
			AssertEquals("Cost OS Currency is USD", "USD", chargesCollection.First().Cost.Currency.Code);
			AssertEquals("Cost OS amount is 100 as entered", 100, chargesCollection.First().Cost.Amount.ToZInt());
			AssertEquals("Cost Local Currency is AUD", "AUD", chargesCollection.First().LocalCost.Currency.Code);
			AssertEquals("Cost Local amount should be about 83, based on 1.2 base rates from CreateShipment()", 83, chargesCollection.First().LocalCost.Amount.ToZInt());

			AssertEquals("Sell OS Currency is USD", "USD", chargesCollection.First().Sell.Currency.Code);
			AssertEquals("Sell OS amount is 100 as entered", 100, chargesCollection.First().Sell.Amount.ToZInt());
			AssertEquals("Sell Local Currency is AUD", "AUD", chargesCollection.First().LocalSell.Currency.Code);
			AssertEquals("Sell Local amount should be about 83, based on 1.2 base rates from CreateShipment()", 83, chargesCollection.First().LocalSell.Amount.ToZInt());
		}

		#endregion

		#region TestChargesCollectionShouldReturnSortedCharges

		public void TestChargesCollection_Should_ReturnSortedCharges()
		{
			var (auCompany, auBranch) = CreateGlobalCompanyAndBranch("AU", "AU Branch 1");
			var (nzCompany, nzBranch) = CreateGlobalCompanyAndBranch("NZ", "NZ Branch 1");
			Factory.Save();

			var collection1 = new PrintChargesBilledToLocalClientAtDestAsCollectCollection();
			CreatePrintChargesBilledToLocalClientAtDestAsCollect(collection1, Core.Constants.TransportModes.Sea, "AU", "");
			TestChargesCollection(collection1, true, false, auBranch);

			var collection2 = new PrintChargesBilledToLocalClientAtDestAsCollectCollection();
			CreatePrintChargesBilledToLocalClientAtDestAsCollect(collection2, Core.Constants.TransportModes.Sea, "AU", "NZ");
			TestChargesCollection(collection2, true, true, nzBranch);

			#region TestChargesCollection

			void TestChargesCollection(PrintChargesBilledToLocalClientAtDestAsCollectCollection collection, bool isLocalCharges, bool setShipmentDetails, GlbBranch loginBranch)
			{
				using (Env.SetTemporaryUserContext(Env.CurrentUserPK, loginBranch.PK.ToGuid(), Env.CurrentDepartmentPK))
				using (FreightDataRegistry.Instance.PrintChargesBilledToLocalClientAtDestAsCollect.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, collection))
				{
					CreateAndVerifyChargesCollection(ZDateTime.UtcNow, new ZShort[] { 4, 6, 1, 5, 1, 1 }, new ZDecimal[] { 550m, 750m, 650m, 1000m, 850m, 900m });
					CreateAndVerifyChargesCollection(ZDateTime.UtcNow, new ZShort[] { 3, 5, 1, 4, 2, 1 }, new ZDecimal[] { 550m, 750m, 650m, 1000m, 850m, 900m });
					CreateAndVerifyChargesCollection(null, new ZShort[] { 3, 2, 6, 5, 4, 1 }, new ZDecimal[] { 550m, 900m, 1000m, 650m, 850m, 750m });
				}

				void CreateAndVerifyChargesCollection(ZDateTime? dateTimeOffset, ZShort[] indexes, ZDecimal[] expectedSortedAmounts)
				{
					var shipment = CreateShipment();
					var header = shipment.JobHeader;

					CreateLineCharge(header, isLocalCharges ? header.LocalChargesPK : header.AgentCollectPK, 1000m, "FRT", "AUD", dateTimeOffset?.AddMinutes(3), indexes[0]);
					CreateLineCharge(header, isLocalCharges ? header.LocalChargesPK : header.AgentCollectPK, 900m, "OLAB", "AUD", dateTimeOffset?.AddMinutes(5), indexes[1]);
					CreateLineCharge(header, isLocalCharges ? header.LocalChargesPK : header.AgentCollectPK, 750m, "DLAB", "USD", dateTimeOffset?.AddMinutes(1), indexes[2]);
					CreateLineCharge(header, isLocalCharges ? header.LocalChargesPK : header.AgentCollectPK, 850m, "DLAB", "USD", dateTimeOffset?.AddMinutes(4), indexes[3]);
					CreateLineCharge(header, isLocalCharges ? header.LocalChargesPK : header.AgentCollectPK, 650m, "DDOC", "USD", dateTimeOffset?.AddMinutes(2), indexes[4]);
					CreateLineCharge(header, isLocalCharges ? header.LocalChargesPK : header.AgentCollectPK, 550m, "ODOC", "USD", dateTimeOffset, indexes[5]);

					shipment.JS_HBLAWBChargesDisplay = DocumentsDataRegistry.HBLChargesDisplayTypes.PrepaidCharges;

					if (setShipmentDetails)
					{
						shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
						shipment.JS_RL_NKOrigin = Core.Constants.CountryCodes.Australia;
						shipment.JS_RL_NKDestination = Core.Constants.CountryCodes.NewZealand;
					}

					var lookups = new HouseBillLookups(Factory);
					var chargesCollection = new ChargesCollection(shipment, lookups, false);

					AssertNotNull(chargesCollection.GetEnumerator());

					AssertContainsExactElementsInExactOrder($"Calling {(setShipmentDetails ? "CreateChargesFromGlobalJobCosting" : "GetChargesFromJobHeader")} method should return sorted list", expectedSortedAmounts,
						chargesCollection.Select(x => x.Sell.Amount));
				}
			}

			(GlbCompany, GlbBranch) CreateGlobalCompanyAndBranch(string countryCode, string branchName)
			{
				var orgProxy = Factory.NewWithValidTestData<OrgHeader>();
				orgProxy.OH_IsConsignor = true;

				var company = Factory.NewWithValidTestData<GlbCompany>();
				company.GC_RN_NKCountryCode = countryCode;
				company.GC_OH_OrgProxy = orgProxy.PK;

				var branch = Factory.NewWithValidTestData<GlbBranch>();
				branch.GB_RN_NKCountryCode = countryCode;
				branch.GB_GC = company.PK;
				branch.GB_BranchName = branchName;

				return (company, branch);
			}

			#endregion
		}

		#endregion

		public void TestCreateCharges_DoesNotDisplay_ProfitShareCharges()
		{
			var chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode.AC_Code = "PPP";
			chargeCode.AC_Desc = "Profit Share Charges from Registry";

			AccountingConfigurationRegistry.Instance.ProfitShareChargeCode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, chargeCode.PK.ToGuid());

			var shipment = CreateShipment();
			var header = shipment.JobHeader;

			CreateLineCharge(header, header.LocalChargesPK, 1000m, "FRT", "AUD");
			CreateLineCharge(header, header.LocalChargesPK, 500m, chargeCode.AC_Code, "AUD");

			var lookups = new HouseBillLookups(Factory);
			var chargesCollection = new ChargesCollection(shipment, lookups, false);
			var profitShareCharges = chargesCollection.All.Where(charge => charge.ChargeCode.Code == chargeCode.AC_Code);

			CombineAssertions(() =>
			{
				AssertEquals("1 Charge in total - FRT", 1, chargesCollection.All.Count);
				AssertEquals("Profit Share Charges (from registry) should not be in the collection", 0, profitShareCharges.Count());
			});
		}

		#region TestCreateCharges_HBL

		public void TestCreateCharges_HBL_ShipmentNotMatchedByRegistry() => AssertCreateCharges_HBL(false);

		public void TestCreateCharges_HBL_ShipmentMatchedByRegistry_OriginHasNoOverseaAgent() => AssertCreateCharges_HBL(true);

		void AssertCreateCharges_HBL(bool isShipmentMatchedRegistry)
		{
			var collection = new PrintChargesBilledToLocalClientAtDestAsCollectCollection();
			if (isShipmentMatchedRegistry)
			{
				CreatePrintChargesBilledToLocalClientAtDestAsCollect(collection, Core.Constants.TransportModes.Sea, Core.Constants.CountryCodes.China, Core.Constants.CountryCodes.Chile);
			}

			using (FreightDataRegistry.Instance.PrintChargesBilledToLocalClientAtDestAsCollect.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, collection))
			{
				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_RL_NKOrigin = "CNSHA";
				shipment.JS_RL_NKDestination = "CLSCL";
				shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
				shipment.JS_INCO = Core.Constants.IncoTerms.FreeOnBoard;
				shipment.JS_HBLAWBChargesDisplay = "ALL";

				var cnRandomBranch = GetNewBranch(Core.Constants.CountryCodes.China);
				var clRandomBranch = GetNewBranch(Core.Constants.CountryCodes.Chile);

				using (Env.SetTemporaryUserContext(Env.CurrentUserPK, cnRandomBranch.PK.ToGuid(), Env.CurrentDepartmentPK))
				{
					var localClient = Factory.New<OrgHeader>();
					localClient.OH_IsConsignor = ZBool.True;
					localClient.OH_FullName = "ShangHai";
					localClient.OH_RL_NKClosestPort = "CNSHA";
					localClient.MainAddress.Address1 = "aaa";
					localClient.MainAddress.Address2 = "bbbe";
					localClient.MainAddress.City = "ShangHai";
					localClient.MainAddress.Postcode = "2000";
					localClient.MainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.China;

					var loader = new JobHeader.Loader(shipment);
					var header = loader.TryLoadOrCreateWithMutex();
					header.LocalChargesPK = localClient.PK;

					CreateLineCharge(header, header.LocalChargesPK, 100m, "DLAB", "USD");

					Factory.Save();
				}

				using (Env.SetTemporaryUserContext(Env.CurrentUserPK, clRandomBranch.PK.ToGuid(), Env.CurrentDepartmentPK))
				{
					var localClient = Factory.New<OrgHeader>();
					localClient.OH_IsConsignor = ZBool.True;
					localClient.OH_FullName = "Chile";
					localClient.OH_RL_NKClosestPort = "Chile";
					localClient.MainAddress.Address1 = "aaa";
					localClient.MainAddress.Address2 = "bbbe";
					localClient.MainAddress.City = "Chile";
					localClient.MainAddress.Postcode = "2000";
					localClient.MainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Chile;

					var loader = new JobHeader.Loader(shipment);
					var header = loader.TryLoadOrCreateWithMutex();
					header.LocalChargesPK = localClient.PK;

					CreateLineCharge(header, header.LocalChargesPK, 200m, "FRT", "USD");
					CreateLineCharge(header, header.LocalChargesPK, 300m, "FRT", "USD");

					Factory.Save();
				}

				using (Env.SetTemporaryUserContext(Env.CurrentUserPK, cnRandomBranch.PK.ToGuid(), Env.CurrentDepartmentPK))
				{
					AssertChargesContainsExactChargesInExactOrder(shipment, new ZDecimal[] { 100m });
				}

				using (Env.SetTemporaryUserContext(Env.CurrentUserPK, clRandomBranch.PK.ToGuid(), Env.CurrentDepartmentPK))
				{
					AssertChargesContainsExactChargesInExactOrder(shipment, new ZDecimal[] { 100m });
				}
			}
		}

		void AssertChargesContainsExactChargesInExactOrder(ForwardingShipment shipment, ZDecimal[] expectedSortedAmounts)
		{
			var chargesCollection = new ChargesCollection(shipment, new HouseBillLookups(Factory), false);
			AssertContainsExactElementsInExactOrder(expectedSortedAmounts, chargesCollection.Select(x => x.Sell.Amount));
		}

		GlbBranch GetNewBranch(string countryCode)
		{
			var branch = new BusinessObjectFactory().NewWithValidTestData<GlbBranch>();
			branch.GB_Code = MasterFilesTestHelper.GetRandomString(branch.GB_CodeInfo.MaxLength);
			branch.SetCountry(countryCode);

			var company = branch.Company;
			company.GC_Code = MasterFilesTestHelper.GetRandomString(company.GC_CodeInfo.MaxLength);
			company.SetCountry(countryCode);

			branch.Factory.Save();

			return branch;
		}

		#endregion

		#region TestChargeLineAttributes

		public void TestChargeLineAttributes()
		{
			var shipment = CreateShipment();
			var header = shipment.JobHeader;

			var charge1 = CreateLineCharge(header, header.LocalChargesPK, 1000m, "FRT", "AUD");
			var charge2 = CreateLineCharge(header, header.AgentCollectPK, 500m, "OLAB", "AUD");
			var charge3 = CreateLineCharge(header, header.LocalChargesPK, 750m, "DLAB", "USD");

			var attrib1 = charge1.JobChargeAttributes.AddNew();
			attrib1.EC_Name = JobChargeAttribTypeList.Codes.DocketReference;
			attrib1.EC_Value = "External Reference";
			attrib1.EC_Amount = 1.3M;

			var attrib11 = charge1.JobChargeAttributes.AddNew();
			attrib11.EC_Name = JobChargeAttribTypeList.Codes.MinimumRateUsed;
			attrib11.EC_Value = "External Reference11";
			attrib11.EC_Amount = 2.1M;

			var attrib2 = charge2.JobChargeAttributes.AddNew();
			attrib2.EC_Name = JobChargeAttribTypeList.Codes.ContainerNumber;
			attrib2.EC_Value = "External Reference2";
			attrib2.EC_Amount = 1.5M;

			var attrib3 = charge3.JobChargeAttributes.AddNew();
			attrib3.EC_Name = JobChargeAttribTypeList.Codes.SerialNumber;
			attrib3.EC_Value = "External Reference3";
			attrib3.EC_Amount = 2.5M;

			shipment.JS_HBLAWBChargesDisplay = DocumentsDataRegistry.HBLChargesDisplayTypes.PrepaidAndCollectCharges;

			var lookups = new HouseBillLookups(Factory);
			var chargesCollection = new ChargesCollection(shipment, lookups, false);

			AssertEquals(2, chargesCollection.First(x => x.ChargeCode.Code == "FRT" && x.IsPrepaid).ChargeLineAttributes.Count);
			AssertContainsExactElementsInAnyOrder(new[] { JobChargeAttribTypeList.Codes.DocketReference, JobChargeAttribTypeList.Codes.MinimumRateUsed }
				, chargesCollection.First(x => x.ChargeCode.Code == "FRT" && x.IsPrepaid).ChargeLineAttributes.Select(chargeLineAttribute => chargeLineAttribute.Name));
			AssertContainsExactElementsInAnyOrder(new[] { "External Reference", "External Reference11" }
				, chargesCollection.First(x => x.ChargeCode.Code == "FRT" && x.IsPrepaid).ChargeLineAttributes.Select(chargeLineAttribute => chargeLineAttribute.Value));
			AssertContainsExactElementsInAnyOrder(new ZDecimal[] { 1.3M, 2.1M }
				, chargesCollection.First(x => x.ChargeCode.Code == "FRT" && x.IsPrepaid).ChargeLineAttributes.Select(chargeLineAttribute => chargeLineAttribute.Amount));

			AssertEquals(1, chargesCollection.First(x => x.ChargeCode.Code == "OLAB" && !x.IsPrepaid).ChargeLineAttributes.Count);
			AssertContainsExactElementsInAnyOrder(new[] { JobChargeAttribTypeList.Codes.ContainerNumber }
				, chargesCollection.First(x => x.ChargeCode.Code == "OLAB" && !x.IsPrepaid).ChargeLineAttributes.Select(chargeLineAttribute => chargeLineAttribute.Name));
			AssertContainsExactElementsInAnyOrder(new[] { "External Reference2" }
				, chargesCollection.First(x => x.ChargeCode.Code == "OLAB" && !x.IsPrepaid).ChargeLineAttributes.Select(chargeLineAttribute => chargeLineAttribute.Value));
			AssertContainsExactElementsInAnyOrder(new ZDecimal[] { 1.5M }
				, chargesCollection.First(x => x.ChargeCode.Code == "OLAB" && !x.IsPrepaid).ChargeLineAttributes.Select(chargeLineAttribute => chargeLineAttribute.Amount));

			AssertEquals(1, chargesCollection.First(x => x.ChargeCode.Code == "DLAB" && x.IsPrepaid).ChargeLineAttributes.Count);
			AssertContainsExactElementsInAnyOrder(new[] { JobChargeAttribTypeList.Codes.SerialNumber }
				, chargesCollection.First(x => x.ChargeCode.Code == "DLAB" && x.IsPrepaid).ChargeLineAttributes.Select(chargeLineAttribute => chargeLineAttribute.Name));
			AssertContainsExactElementsInAnyOrder(new[] { "External Reference3" }
				, chargesCollection.First(x => x.ChargeCode.Code == "DLAB" && x.IsPrepaid).ChargeLineAttributes.Select(chargeLineAttribute => chargeLineAttribute.Value));
			AssertContainsExactElementsInAnyOrder(new ZDecimal[] { 2.5M }
				, chargesCollection.First(x => x.ChargeCode.Code == "DLAB" && x.IsPrepaid).ChargeLineAttributes.Select(chargeLineAttribute => chargeLineAttribute.Amount));

			shipment.JS_HBLAWBChargesDisplay = DocumentsDataRegistry.HBLChargesDisplayTypes.PrepaidCharges;
			chargesCollection = new ChargesCollection(shipment, lookups, false);

			AssertEquals(2, chargesCollection.First(x => x.ChargeCode.Code == "FRT" && x.IsPrepaid).ChargeLineAttributes.Count);
			AssertContainsExactElementsInAnyOrder(new[] { JobChargeAttribTypeList.Codes.DocketReference, JobChargeAttribTypeList.Codes.MinimumRateUsed }
				, chargesCollection.First(x => x.ChargeCode.Code == "FRT" && x.IsPrepaid).ChargeLineAttributes.Select(chargeLineAttribute => chargeLineAttribute.Name));
			AssertContainsExactElementsInAnyOrder(new[] { "External Reference", "External Reference11" }
				, chargesCollection.First(x => x.ChargeCode.Code == "FRT" && x.IsPrepaid).ChargeLineAttributes.Select(chargeLineAttribute => chargeLineAttribute.Value));
			AssertContainsExactElementsInAnyOrder(new ZDecimal[] { 1.3M, 2.1M }
				, chargesCollection.First(x => x.ChargeCode.Code == "FRT" && x.IsPrepaid).ChargeLineAttributes.Select(chargeLineAttribute => chargeLineAttribute.Amount));

			AssertEquals(1, chargesCollection.First(x => x.ChargeCode.Code == "DLAB" && x.IsPrepaid).ChargeLineAttributes.Count);
			AssertContainsExactElementsInAnyOrder(new[] { JobChargeAttribTypeList.Codes.SerialNumber }
				, chargesCollection.First(x => x.ChargeCode.Code == "DLAB" && x.IsPrepaid).ChargeLineAttributes.Select(chargeLineAttribute => chargeLineAttribute.Name));
			AssertContainsExactElementsInAnyOrder(new[] { "External Reference3" }
				, chargesCollection.First(x => x.ChargeCode.Code == "DLAB" && x.IsPrepaid).ChargeLineAttributes.Select(chargeLineAttribute => chargeLineAttribute.Value));
			AssertContainsExactElementsInAnyOrder(new ZDecimal[] { 2.5M }
				, chargesCollection.First(x => x.ChargeCode.Code == "DLAB" && x.IsPrepaid).ChargeLineAttributes.Select(chargeLineAttribute => chargeLineAttribute.Amount));

			shipment.JS_HBLAWBChargesDisplay = DocumentsDataRegistry.HBLChargesDisplayTypes.CollectCharges;
			chargesCollection = new ChargesCollection(shipment, lookups, false);

			AssertEquals(1, chargesCollection.First(x => x.ChargeCode.Code == "OLAB" && !x.IsPrepaid).ChargeLineAttributes.Count);
			AssertContainsExactElementsInAnyOrder(new[] { JobChargeAttribTypeList.Codes.ContainerNumber }
				, chargesCollection.First(x => x.ChargeCode.Code == "OLAB" && !x.IsPrepaid).ChargeLineAttributes.Select(chargeLineAttribute => chargeLineAttribute.Name));
			AssertContainsExactElementsInAnyOrder(new[] { "External Reference2" }
				, chargesCollection.First(x => x.ChargeCode.Code == "OLAB" && !x.IsPrepaid).ChargeLineAttributes.Select(chargeLineAttribute => chargeLineAttribute.Value));
			AssertContainsExactElementsInAnyOrder(new ZDecimal[] { 1.5M }
				, chargesCollection.First(x => x.ChargeCode.Code == "OLAB" && !x.IsPrepaid).ChargeLineAttributes.Select(chargeLineAttribute => chargeLineAttribute.Amount));
		}

		#endregion

		#region TestCreateChargesUsesCacheWhenRequired

		public void TestGetRevenueChargesFromCache_UsesCachedValueIfChargesAlreadyRated()
		{
			var shipment = CreateShipmentWithCharges();
			var lookups = new HouseBillLookups(Factory);

			var chargesCollectionOriginal = new ChargesCollection(shipment, lookups, true);
			var chargesListOriginal = chargesCollectionOriginal.All;

			var chargesCollectionCopy = new ChargesCollection(shipment, lookups, false);
			var chargesListCopy = chargesCollectionCopy.All;

			for (var i = 0; i < chargesListOriginal.Count; i++)
			{
				Assert(chargesListOriginal.ElementAt(i).IsEquivalentTo(chargesListCopy.ElementAt(i)));
			}

			AssertEquals(chargesListOriginal.First().Description.GetHashCode(), chargesListCopy.First().Description.GetHashCode());
			AssertEquals(chargesListOriginal.First().CostExchangeRate.GetHashCode(), chargesListCopy.First().CostExchangeRate.GetHashCode());
		}

		#endregion

		#region Implementation

		#region CreatePrintChargesBilledToLocalClientAtDestAsCollect

		PrintChargesBilledToLocalClientAtDestAsCollect CreatePrintChargesBilledToLocalClientAtDestAsCollect(
			PrintChargesBilledToLocalClientAtDestAsCollectCollection collection,
			ZString transportMode,
			ZString exportCountry,
			ZString importCountry)
		{
			var setting = collection.AddNew();

			setting.TransportMode = transportMode;
			setting.ExportCountry = exportCountry;
			setting.ImportCountry = importCountry;

			return setting;
		}

		#endregion

		#region CreateCompany

		GlbCompany CreateCompany(string companyCode, string companyName, string branchCode, string branchName, string homePort, string currency, ZGuid orgProxyPK, ZGuid orgBranchProxyPK = default)
		{
			var newCompany = Factory.New<GlbCompany>();
			newCompany.GC_Code = companyCode;
			newCompany.GC_Name = companyName;
			newCompany.GC_RN_NKCountryCode = homePort.Substring(0, 2);
			newCompany.GC_OH_OrgProxy = orgProxyPK;
			newCompany.GC_RX_NKLocalCurrency = currency;

			var newBranch = newCompany.Branches.AddNew();
			newBranch.GB_Code = branchCode;
			newBranch.GB_BranchName = branchName;
			newBranch.GB_RL_NKHomePort = homePort;
			newBranch.GB_OH_OrgProxy = orgBranchProxyPK.IsEmpty ? orgProxyPK : orgBranchProxyPK;

			return newCompany;
		}

		#endregion

		#region CreateOrg

		OrgHeader CreateOrg(string code, string fullName, string address1, string countryCode, string closestPort)
		{
			var org = Factory.New<OrgHeader>();
			org.OH_Code = code;
			org.OH_FullName = fullName;
			org.OH_RL_NKClosestPort = closestPort;
			org.MainAddress.Address1 = address1;
			org.MainAddress.OA_RN_NKCountryCode = countryCode;

			return org;
		}

		#endregion

		#region CreateAccChargeCode

		AccChargeCode CreateAccChargeCode(string code, ZGuid companyPK)
		{
			var accChargeCode = Factory.New<AccChargeCode>();
			accChargeCode.AC_Code = code;
			accChargeCode.AC_Desc = code + " Desc";
			accChargeCode.AC_ChargeGroup = code.Substring(0, AccChargeCodeSchema.AC_ChargeGroup.MaxLength);
			accChargeCode.AC_GC = companyPK;

			accChargeCode.SetGLAccountDataForTesting(accChargeCode.GLAccountForTesting);

			return accChargeCode;
		}

		#endregion

		ForwardingShipment CreateShipmentWithCharges()
		{
			var shipment = CreateShipment();
			var header = shipment.JobHeader;

			CreateLineCharge(header, header.LocalChargesPK, 1000m, "FRT", "AUD");
			CreateLineCharge(header, header.AgentCollectPK, 500m, "OLAB", "AUD");
			CreateLineCharge(header, header.LocalChargesPK, 750m, "DLAB", "USD");

			return shipment;
		}

		ForwardingShipment CreateShipment()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USCHI";
			shipment.JS_INCO = Core.Constants.IncoTerms.FreeOnBoard;
			shipment.JS_HBLAWBChargesDisplay = "ALL";

			var localClient = Factory.New<OrgHeader>();
			localClient.OH_FullName = "Ziggy Z";
			localClient.OH_RL_NKClosestPort = "AUSYD";
			localClient.MainAddress.Address1 = "Unit 13";
			localClient.MainAddress.Address2 = "4 Lost Lane";
			localClient.MainAddress.City = "Sydney";
			localClient.MainAddress.Postcode = "2000";
			localClient.MainAddress.OA_RN_NKCountryCode = "AU";

			var agentCollect = Factory.New<OrgHeader>();
			agentCollect.OH_FullName = "Airmarine Inc.";
			agentCollect.OH_RL_NKClosestPort = "USCHI";
			agentCollect.MainAddress.Address1 = "5638 S Central Ave";
			agentCollect.MainAddress.City = "Chicago";
			agentCollect.MainAddress.Postcode = "60638";
			agentCollect.MainAddress.OA_RN_NKCountryCode = "US";

			var loader = new JobHeader.Loader(shipment);
			var header = loader.TryLoadOrCreate();

			header.LocalChargesPK = localClient.PK;
			header.AgentCollectPK = agentCollect.PK;

			var exRates = (BusinessObjectCollection)header["ExchangeRates"];

			var usdRate = exRates.AddNew();
			usdRate[JobExRateSchema.Constants.JF_RX_NKRateCurrency] = "USD";
			usdRate[JobExRateSchema.Constants.JF_BaseRate] = 1.2;

			var auRate = exRates.AddNew();
			auRate[JobExRateSchema.Constants.JF_RX_NKRateCurrency] = "AUD";
			auRate[JobExRateSchema.Constants.JF_BaseRate] = 1.1;

			return shipment;
		}

		JobCharge CreateLineCharge(JobHeader header, ZGuid sellAccountPK, ZDecimal osSellAmount, ZString chargeCode, ZString currencyCode, ZDateTime? systemLastEditTimeUtc = null, ZShort displaySequence = default, BusinessObjectFactory otherFactory = default)
		{
			var factory = otherFactory ?? Factory;

			var query = new ZQuery(AccChargeCodeSchema.AC_Code, SQLComparisonOperator.Equal, chargeCode);
			query.AddToFilter(AccChargeCodeSchema.AC_GC, SQLComparisonOperator.Equal, header.JH_GC);

			var accChargeCode = factory.LoadTop1<AccChargeCode>(query);

			AssertNotNull($"prerequisite: charge code '{chargeCode}' was found", accChargeCode);

			var lineCharge = factory.New<JobCharge>();
			lineCharge.JR_JH = header.PK;
			lineCharge.JR_GE = header.JH_GE;
			lineCharge.JR_GB = header.JH_GB;
			lineCharge.JR_AC = accChargeCode.PK;
			lineCharge.JR_OH_SellAccount = sellAccountPK;
			lineCharge.JR_RX_NKSellCurrency = currencyCode;
			lineCharge.JR_OSSellAmt = osSellAmount;
			lineCharge.JR_Desc = accChargeCode.AC_Desc;
			lineCharge.JR_DisplaySequence = displaySequence;
			lineCharge.JR_SystemLastEditTimeUtc = systemLastEditTimeUtc ?? ZDateTime.Empty;

			return lineCharge;
		}

		#endregion
	}
}
