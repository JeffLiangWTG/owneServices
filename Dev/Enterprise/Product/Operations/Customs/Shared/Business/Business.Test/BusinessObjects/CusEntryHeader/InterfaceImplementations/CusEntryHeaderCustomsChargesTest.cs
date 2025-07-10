using System;
using System.Linq;
using System.Reflection;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Customs;
using Enterprise.ZArchitecture.Environment;
using Moq;

namespace Enterprise.Customs.Business.InterfaceImplementations.Testing
{
	public class CusEntryHeaderCustomsChargesTest : TestCaseWithFactory
	{
		public void TestExistsNonZeroAmountCusDsbJobChargesForMultiEntries_EntryReferenceInChargeDescSupported()
		{
			AssertExistsNonZeroAmountCusDsbJobChargesForMultiEntries(true);
		}

		public void TestExistsNonZeroAmountCusDsbJobChargesForMultiEntries_EntryReferenceInChargeDescSupported_ShouldNotMatchAdditionlDescription()
		{
			AssertExistsNonZeroAmountCusDsbJobChargesForMultiEntries(true, "Customs Duty and Fees - not refer to any entry\r\nFee Type Name EntryRef2   22m", false);
		}

		public void TestExistsNonZeroAmountCusDsbJobChargesForMultiEntries_EntryReferenceInChargeDescNotSupported()
		{
			AssertExistsNonZeroAmountCusDsbJobChargesForMultiEntries(false);
		}

		public void TestChargeForWithdrawnEntry()
		{
			var declaration = Factory.New<BaseJobDeclaration>();

			var entry = Factory.NewMoq<CusEntryHeader>();
			entry.Object.CH_JE = declaration.PK;

			entry.Setup(m => m.HasBeenWithdrawn).Returns(false);

			entry.Object.Charges.AddNew(entry.Object.EntryChargeTypeList[0].Code, 10m);

			CombineAssertions(() =>
			{
				var charges = ((ICustomsCharges)new CusEntryHeaderCustomsCharges(entry.Object)).GetCustomsCharges(null);
				AssertEquals("One charge is payable to Customs", 1, charges.Length);

				entry.Setup(m => m.HasBeenWithdrawn).Returns(true);
				charges = ((ICustomsCharges)new CusEntryHeaderCustomsCharges(entry.Object)).GetCustomsCharges(null);
				AssertEquals("One charge is payable to Customs for withdrawn entries", 1, charges.Length);
			});
		}

		public void TestGetCustomsCharges_ZeroAmount_EntryReferenceInChargeDescSupported()
		{
			AssertGetCustomsCharges_ZeroAmount(true);
		}

		public void TestGetCustomsCharges_ZeroAmount_EntryReferenceInChargeDescNotSupported()
		{
			AssertGetCustomsCharges_ZeroAmount(false);
		}

		public void TestGetCustomsCharges_IncludeEntryReference()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				using (RatingDataRegistry.Instance.IncludeEntryHeaderReferenceInCustomsDisbursementCharges.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				{
					var declaration = Factory.New<BaseJobDeclaration>();
					var entryHeader = Factory.New<CusEntryHeader>();
					entryHeader.CH_JE = declaration.PK;
					entryHeader.CH_BGMReference = "EntryRef1";

					var charge1 = entryHeader.Charges.AddNew();
					charge1.C1_ChargeAmount = 100m;
					charge1.C1_ChargeType = entryHeader.EntryChargeTypeList[0].Code;

					var charge2 = entryHeader.Charges.AddNew();
					charge2.C1_ChargeAmount = 70m;
					charge2.C1_ChargeType = entryHeader.EntryChargeTypeList[1].Code;

					ICustomsCharges chargeProvider = new CusEntryHeaderCustomsCharges(entryHeader);
					CombineAssertions(() =>
					{
						AssertEquals(2, chargeProvider.GetCustomsCharges(null).Length);

						AssertEquals(entryHeader.EntryChargeTypeList[0].Description, chargeProvider.GetCustomsCharges(null)[0].Description);
						AssertEquals(100m, chargeProvider.GetCustomsCharges(null)[0].Amount);
						AssertEquals("EntryRef1", chargeProvider.GetCustomsCharges(null)[0].EntryReference);

						AssertEquals(entryHeader.EntryChargeTypeList[1].Description, chargeProvider.GetCustomsCharges(null)[1].Description);
						AssertEquals(70m, chargeProvider.GetCustomsCharges(null)[1].Amount);
						AssertEquals("EntryRef1", chargeProvider.GetCustomsCharges(null)[1].EntryReference);
					});
				}
			}
		}

		public void TestGetCustomsCharges_ExcludeEntryReference()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				using (RatingDataRegistry.Instance.IncludeEntryHeaderReferenceInCustomsDisbursementCharges.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
				{
					var declaration = Factory.New<BaseJobDeclaration>();
					var entryHeader = Factory.New<CusEntryHeader>();
					entryHeader.CH_JE = declaration.PK;
					entryHeader.CH_BGMReference = "EntryRef1";

					var charge1 = entryHeader.Charges.AddNew();
					charge1.C1_ChargeAmount = 100m;
					charge1.C1_ChargeType = entryHeader.EntryChargeTypeList[0].Code;

					var charge2 = entryHeader.Charges.AddNew();
					charge2.C1_ChargeAmount = 70m;
					charge2.C1_ChargeType = entryHeader.EntryChargeTypeList[1].Code;

					ICustomsCharges chargeProvider = new CusEntryHeaderCustomsCharges(entryHeader);
					CombineAssertions(() =>
					{
						AssertEquals(2, chargeProvider.GetCustomsCharges(null).Length);

						AssertEquals(entryHeader.EntryChargeTypeList[0].Description, chargeProvider.GetCustomsCharges(null)[0].Description);
						AssertEquals(100m, chargeProvider.GetCustomsCharges(null)[0].Amount);
						AssertEquals("", chargeProvider.GetCustomsCharges(null)[0].EntryReference);

						AssertEquals(entryHeader.EntryChargeTypeList[1].Description, chargeProvider.GetCustomsCharges(null)[1].Description);
						AssertEquals(70m, chargeProvider.GetCustomsCharges(null)[1].Amount);
						AssertEquals("", chargeProvider.GetCustomsCharges(null)[1].EntryReference);
					});
				}
			}
		}

		public void TestMatchCustomsCharges()
		{
			var entry = new Mock<ICustomsChargeEntry>();
			entry.Setup(x => x.UniqueNumber).Returns("UN00001");
			entry.Setup(x => x.PreviousUniqueNumber).Returns("PUN00002");
			entry.Setup(x => x.ReferenceNumber).Returns("Ref00003");
			entry.Setup(x => x.EntryReferenceInChargeDescSupported).Returns(false);

			CombineAssertions("NOT EntryReferenceInChargeDescSupported", () =>
			{
				AssertEquals("Pre-Req", false, entry.Object.EntryReferenceInChargeDescSupported);
				AssertEquals("No Matches", false, CusEntryHeaderCustomsCharges.MatchCustomsCharges(entry.Object, "INV12345", "Random Description"));
				AssertEquals("Empty InvoiceNum", true, CusEntryHeaderCustomsCharges.MatchCustomsCharges(entry.Object, "", "Random Description"));
				AssertEquals("Match UniqueNum", true, CusEntryHeaderCustomsCharges.MatchCustomsCharges(entry.Object, "UN00001 InvoiceNum", "Random Description"));
				AssertEquals("Match PrevUniqueNum", true, CusEntryHeaderCustomsCharges.MatchCustomsCharges(entry.Object, "PUN00002 InvoiceNum", "Random Description"));
			});

			entry.Setup(x => x.EntryReferenceInChargeDescSupported).Returns(true);
			CombineAssertions("EntryReferenceInChargeDescSupported", () =>
			{
				AssertEquals("Pre-Req", true, entry.Object.EntryReferenceInChargeDescSupported);
				AssertEquals("No Matches", false, CusEntryHeaderCustomsCharges.MatchCustomsCharges(entry.Object, "INV12345", "Random Description"));
				AssertEquals("Empty InvoiceNum", false, CusEntryHeaderCustomsCharges.MatchCustomsCharges(entry.Object, "", "Random Description"));

				AssertEquals("Ref in First line", true, CusEntryHeaderCustomsCharges.MatchCustomsCharges(entry.Object, "", "ABC Ref00003\r\nDEF Ref00004"));
				AssertEquals("Ref in 2nd line", false, CusEntryHeaderCustomsCharges.MatchCustomsCharges(entry.Object, "", "ABC Ref00004\r\nDEF Ref00003"));
			});
		}

		void AssertExistsNonZeroAmountCusDsbJobChargesForMultiEntries(bool entryReferenceInChargeDescIsSupported, string overrideDesc = "", bool existedChargeIsForEntry = true)
		{
			using (RatingDataRegistry.Instance.IncludeEntryHeaderReferenceInCustomsDisbursementCharges.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, entryReferenceInChargeDescIsSupported))
			{
				var declaration = Factory.New<BaseJobDeclaration>();
				var entry1 = declaration.ActiveEntryHeaders.AddNew();
				entry1.CH_BGMReference = "EntryRef1";
				var entry2 = declaration.ActiveEntryHeaders.AddNew();
				entry2.CH_BGMReference = "EntryRef2";

				ZGuid chargeCodeDSB = RatingDataRegistry.Instance.CustomsDisbursementChargeCode.GetValueWithoutFallback(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty);
				var jobHeader = new JobHeader.Loader(declaration).TryCreate();
				var jobCharge = Factory.NewWithValidTestData<JobCharge>();
				jobCharge.JR_JH = jobHeader.PK;
				jobCharge.JR_AC = chargeCodeDSB;
				jobCharge.JR_LocalCostAmt = 26.22m;
				jobCharge.JR_Desc = string.IsNullOrEmpty(overrideDesc) ? "Customs Duty and Fees - EntryRef2" : overrideDesc;

				var existsNonZeroAmountCusDsbJobChargesMethod =
					typeof(CusEntryHeaderCustomsCharges).GetMethod("ExistsNonZeroAmountCusDsbJobCharges",
						BindingFlags.Instance | BindingFlags.NonPublic);

				CombineAssertions(() =>
				{
					ICustomsCharges chargeProvider1 = new CusEntryHeaderCustomsCharges(entry1);
					AssertEquals("If EntryReferenceInChargeDescSupported, check existed charge for Entry and then false as the description of the existed charge does not include EntryRef1. If Not EntryReferenceInChargeDescSupported, just check existed charge for Job",
						!entryReferenceInChargeDescIsSupported, existsNonZeroAmountCusDsbJobChargesMethod.Invoke(chargeProvider1, new object[] { chargeCodeDSB }));

					ICustomsCharges chargeProvider2 = new CusEntryHeaderCustomsCharges(entry2);
					var existedChargeIsForEntryString = existedChargeIsForEntry ? "is" : "is not";
					AssertEquals("The existed charge " + existedChargeIsForEntryString + " EntryRef2", existedChargeIsForEntry, existsNonZeroAmountCusDsbJobChargesMethod.Invoke(chargeProvider2, new object[] { chargeCodeDSB }));
					if (existedChargeIsForEntry)
					{
						AssertEquals("The existed charge is EntryRef2 but different charge type", false, existsNonZeroAmountCusDsbJobChargesMethod.Invoke(chargeProvider2, new object[] { new ZGuid() }));

						jobCharge.JR_LocalCostAmt = 0m;
						AssertEquals("The existed charge is EntryRef2 but amount is 0", false, existsNonZeroAmountCusDsbJobChargesMethod.Invoke(chargeProvider2, new object[] { chargeCodeDSB }));

						jobCharge.JR_LocalCostAmt = 11m;
						jobCharge.JR_E6 = ZGuid.NewZGuid();
						AssertEquals("The existed charge is EntryRef2 but JR_E6 is valid", false, existsNonZeroAmountCusDsbJobChargesMethod.Invoke(chargeProvider2, new object[] { chargeCodeDSB }));
					}
				});
			}
		}

		void AssertGetCustomsCharges_ZeroAmount(bool entryReferenceInChargeDescIsSupported)
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				using (RatingDataRegistry.Instance.IncludeEntryHeaderReferenceInCustomsDisbursementCharges.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, entryReferenceInChargeDescIsSupported))
				{
					var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
					var mockEntry = Factory.NewMoq<CusEntryHeader>();
					mockEntry
						.Setup(m => m.HasBeenWithdrawn)
						.Returns(false);
					mockEntry
						.Setup(m => m.IsFeePaidByBroker(It.IsAny<string>(), It.IsAny<ZString>(), It.IsAny<ILogger>()))
						.Returns(true);
					var entryHeader = mockEntry.Object;
					entryHeader.CH_JE = declaration.PK;
					entryHeader.CH_BGMReference = "EntryRef1";

					var chargeCodeDSB = RatingDataRegistry.Instance.CustomsDisbursementChargeCode.GetValueWithoutFallback(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty);
					var jobHeader = new JobHeader.Loader(declaration).TryCreate();
					var jobCharge = Factory.NewWithValidTestData<JobCharge>();
					jobCharge.JR_JH = jobHeader.PK;
					jobCharge.JR_AC = chargeCodeDSB;
					jobCharge.JR_LocalCostAmt = 26.22m;
					var entryReferenceDescription = entryReferenceInChargeDescIsSupported ? "- EntryRef1" : "";
					jobCharge.JR_Desc = $@"Customs Duty and Fees {entryReferenceDescription}
Current Amounts
  {entryHeader.EntryChargeTypeList[0].Description}                 26.22";
					Factory.Save();

					var collection = new EntryChargeTypeSettingCollection(new FallbackLevel(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty), Factory);
					var charge = collection.AddNew();
					charge.AC_ChargeCode = chargeCodeDSB;
					charge.ChargeType = entryHeader.EntryChargeTypeList[1].Code;
					using (RatingDataRegistry.Instance.EntryChargeTypesAndCodes.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, collection))
					{
						var charge1 = entryHeader.Charges.AddNew();
						charge1.C1_ChargeAmount = 0m;
						charge1.C1_ChargeType = entryHeader.EntryChargeTypeList[0].Code;

						var charge2 = entryHeader.Charges.AddNew();
						charge2.C1_ChargeAmount = 0m;
						charge2.C1_ChargeType = entryHeader.EntryChargeTypeList[1].Code;

						var charge3 = entryHeader.Charges.AddNew();
						charge3.C1_ChargeAmount = 0m;
						charge3.C1_ChargeType = entryHeader.EntryChargeTypeList[2].Code;

						ICustomsCharges chargeProvider = new CusEntryHeaderCustomsCharges(entryHeader);
						var result = chargeProvider.GetCustomsCharges(null);

						CombineAssertions(() =>
						{
							var customsCharge = result.Single();
							AssertEquals("ChargeCodePK", jobCharge.JR_AC, customsCharge.ChargeCodePK);
							AssertEquals("IsPaidByBroker", true, customsCharge.IsPaidByBroker);
							AssertEquals("IsInformationOnly", false, customsCharge.IsInformationOnly);
							AssertEquals("Description", ZString.Empty, customsCharge.Description);
							AssertEquals("Amount", ZDecimal.Zero, customsCharge.Amount);
							AssertEquals("GST", ZDecimal.Zero, customsCharge.GST);
						});
					}
				}
			}
		}
	}
}
