using System;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.ZArchitecture.Core;
using Moq;
using CustomsCommon = Enterprise.Customs.Common;

namespace Enterprise.Customs.Business.Testing
{
	sealed class JobMessageTypeListTest : TestCaseWithFactory
	{
		public void TestDeclarationTypeFromCountry()
		{
			var countryCodes = new[]
			{
				Core.Constants.CountryCodes.Australia,
				Core.Constants.CountryCodes.Canada,
				Core.Constants.CountryCodes.NewZealand,
				Core.Constants.CountryCodes.Singapore,
				Core.Constants.CountryCodes.Switzerland,
				Core.Constants.CountryCodes.UnitedStates,
				Core.Constants.CountryCodes.SouthAfrica,
				Core.Constants.CountryCodes.Germany,
				Core.Constants.CountryCodes.Ireland,
				Core.Constants.CountryCodes.Italy,
				Core.Constants.CountryCodes.Taiwan,
				Core.Constants.CountryCodes.Namibia,
				Core.Constants.CountryCodes.KoreaSouth,
				Core.Constants.CountryCodes.Germany,
				Core.Constants.CountryCodes.Brazil,
				Core.Constants.CountryCodes.Belgium,
				Core.Constants.CountryCodes.UnitedArabEmirates,
				Core.Constants.CountryCodes.Poland,
			};

			CombineAssertions(() =>
			{
				foreach (var countryCode in countryCodes)
				{
					var provider = ObjectFactory.Get<Integration.Customs.IDeclarationTypeListProvider>();

					CodeDescriptionPairList expectedList;
					var actualList = provider.GetListFor(countryCode);

					switch (countryCode)
					{
						case Core.Constants.CountryCodes.Canada:
							{
								expectedList = new CodeDescriptionPairList();
								expectedList.AddPair(CustomsCommon.CA.CAJobMessageTypeList.Codes.Import, CustomsCommon.CA.CAJobMessageTypeList.Descriptions.Import);
								expectedList.AddPair(CustomsCommon.CA.CAJobMessageTypeList.Codes.Export, CustomsCommon.CA.CAJobMessageTypeList.Descriptions.Export);

								break;
							}

						case Core.Constants.CountryCodes.UnitedStates:
							{
								expectedList = new CodeDescriptionPairList();
								expectedList.AddPair(CustomsCommon.US.USJobMessageTypeList.Codes.Import, CustomsCommon.US.USJobMessageTypeList.Descriptions.Import);
								expectedList.AddPair(CustomsCommon.US.USJobMessageTypeList.Codes.ImportByExternalBroker, CustomsCommon.US.USJobMessageTypeList.Descriptions.ImportByExternalBroker);
								expectedList.AddPair(CustomsCommon.US.USJobMessageTypeList.Codes.Export, CustomsCommon.US.USJobMessageTypeList.Descriptions.Export);
								expectedList.AddPair(CustomsCommon.US.USJobMessageTypeList.Codes.FTZ, CustomsCommon.US.USJobMessageTypeList.Descriptions.FTZ);

								break;
							}

						case Core.Constants.CountryCodes.Ireland:
							{
								expectedList = new CodeDescriptionPairList();
								expectedList.AddPair(CustomsCommon.IE.IEJobMessageTypeList.Codes.Import, CustomsCommon.IE.IEJobMessageTypeList.Descriptions.Import);
								expectedList.AddPair(CustomsCommon.IE.IEJobMessageTypeList.Codes.Export, CustomsCommon.IE.IEJobMessageTypeList.Descriptions.Export);
								expectedList.AddPair(CustomsCommon.IE.IEJobMessageTypeList.Codes.ExitSummary, CustomsCommon.IE.IEJobMessageTypeList.Descriptions.ExitSummary);
								expectedList.AddPair(CustomsCommon.IE.IEJobMessageTypeList.Codes.ReExport, CustomsCommon.IE.IEJobMessageTypeList.Descriptions.ReExport);
								expectedList.AddPair(CustomsCommon.IE.IEJobMessageTypeList.Codes.NctsArrivalNotification, CustomsCommon.IE.IEJobMessageTypeList.Descriptions.NctsArrivalNotification);
								expectedList.AddPair(CustomsCommon.IE.IEJobMessageTypeList.Codes.NctsDeparture, CustomsCommon.IE.IEJobMessageTypeList.Descriptions.NctsDeparture);
								expectedList.AddPair(CustomsCommon.IE.IEJobMessageTypeList.Codes.NctsArrivalUnloadingRemarks, CustomsCommon.IE.IEJobMessageTypeList.Descriptions.NctsArrivalUnloadingRemarks);

								break;
							}

						case Core.Constants.CountryCodes.Taiwan:
							{
								expectedList = new CodeDescriptionPairList();
								expectedList = new CodeDescriptionPairList();
								expectedList.AddPair(CustomsCommon.TW.TWJobMessageTypeList.Codes.Import, CustomsCommon.TW.TWJobMessageTypeList.Descriptions.Import);
								expectedList.AddPair(CustomsCommon.TW.TWJobMessageTypeList.Codes.Export, CustomsCommon.TW.TWJobMessageTypeList.Descriptions.Export);

								break;
							}

						case Core.Constants.CountryCodes.Germany:
							{
								expectedList = new CodeDescriptionPairList();
								expectedList.AddPair(CustomsCommon.EU.EUJobMessageTypeList.Codes.Import, CustomsCommon.EU.EUJobMessageTypeList.Descriptions.Import);
								expectedList.AddPair(CustomsCommon.EU.EUJobMessageTypeList.Codes.Export, CustomsCommon.EU.EUJobMessageTypeList.Descriptions.Export);
								expectedList.AddPair(CustomsCommon.DE.DEJobMessageTypeList.Codes.WarehouseAdjustment, CustomsCommon.DE.DEJobMessageTypeList.Descriptions.WarehouseAdjustment);
								expectedList.AddPair(CustomsCommon.EU.EUJobMessageTypeList.Codes.NctsArrivalNotification, CustomsCommon.EU.EUJobMessageTypeList.Descriptions.NctsArrivalNotification);
								expectedList.AddPair(CustomsCommon.EU.EUJobMessageTypeList.Codes.NctsDeparture, CustomsCommon.EU.EUJobMessageTypeList.Descriptions.NctsDeparture);
								expectedList.AddPair(CustomsCommon.EU.EUJobMessageTypeList.Codes.NctsArrivalUnloadingRemarks, CustomsCommon.EU.EUJobMessageTypeList.Descriptions.NctsArrivalUnloadingRemarks);

								break;
							}

						case Core.Constants.CountryCodes.Brazil:
							{
								expectedList = new CodeDescriptionPairList();
								expectedList.AddPair(CustomsCommon.BR.BRJobMessageTypeList.Codes.Drawback, CustomsCommon.BR.BRJobMessageTypeList.Descriptions.Drawback);
								expectedList.AddPair(CustomsCommon.BR.BRJobMessageTypeList.Codes.Export, CustomsCommon.BR.BRJobMessageTypeList.Descriptions.Export);
								expectedList.AddPair(CustomsCommon.BR.BRJobMessageTypeList.Codes.ExWarehouse, CustomsCommon.BR.BRJobMessageTypeList.Descriptions.ExWarehouse);
								expectedList.AddPair(CustomsCommon.BR.BRJobMessageTypeList.Codes.Import, CustomsCommon.BR.BRJobMessageTypeList.Descriptions.Import);
								expectedList.AddPair(CustomsCommon.BR.BRJobMessageTypeList.Codes.Refund, CustomsCommon.BR.BRJobMessageTypeList.Descriptions.Refund);
								expectedList.AddPair(CustomsCommon.BR.BRJobMessageTypeList.Codes.WarehousedByExternalAgent, CustomsCommon.BR.BRJobMessageTypeList.Descriptions.WarehousedByExternalAgent);
								break;
							}

						case Core.Constants.CountryCodes.Japan:
							{
								expectedList = new CodeDescriptionPairList();
								expectedList.AddPair(CustomsCommon.JP.JPJobMessageTypeList.Codes.Import, CustomsCommon.JP.JPJobMessageTypeList.Descriptions.Import);
								expectedList.AddPair(CustomsCommon.JP.JPJobMessageTypeList.Codes.Export, CustomsCommon.JP.JPJobMessageTypeList.Descriptions.Export);
								break;
							}

						case Core.Constants.CountryCodes.Belgium:
							{
								expectedList = new CodeDescriptionPairList();
								expectedList.AddPair(CustomsCommon.BE.BEJobMessageTypeList.Codes.Import, CustomsCommon.BE.BEJobMessageTypeList.Descriptions.Import);
								expectedList.AddPair(CustomsCommon.BE.BEJobMessageTypeList.Codes.Export, CustomsCommon.BE.BEJobMessageTypeList.Descriptions.Export);
								expectedList.AddPair(CustomsCommon.BE.BEJobMessageTypeList.Codes.ExitSummary, CustomsCommon.BE.BEJobMessageTypeList.Descriptions.ExitSummary);
								expectedList.AddPair(CustomsCommon.BE.BEJobMessageTypeList.Codes.ReExport, CustomsCommon.BE.BEJobMessageTypeList.Descriptions.ReExport);
								expectedList.AddPair(CustomsCommon.BE.BEJobMessageTypeList.Codes.NctsArrivalNotification, CustomsCommon.BE.BEJobMessageTypeList.Descriptions.NctsArrivalNotification);
								expectedList.AddPair(CustomsCommon.BE.BEJobMessageTypeList.Codes.NctsDeparture, CustomsCommon.BE.BEJobMessageTypeList.Descriptions.NctsDeparture);
								expectedList.AddPair(CustomsCommon.BE.BEJobMessageTypeList.Codes.NctsArrivalUnloadingRemarks, CustomsCommon.BE.BEJobMessageTypeList.Descriptions.NctsArrivalUnloadingRemarks);
								break;
							}

						case Core.Constants.CountryCodes.UnitedArabEmirates:
							{
								expectedList = new CodeDescriptionPairList();
								expectedList.AddPair(CustomsCommon.AE.AEJobMessageTypeList.Codes.Drawback, CustomsCommon.AE.AEJobMessageTypeList.Descriptions.Drawback);
								expectedList.AddPair(CustomsCommon.AE.AEJobMessageTypeList.Codes.Export, CustomsCommon.AE.AEJobMessageTypeList.Descriptions.Export);
								expectedList.AddPair(CustomsCommon.AE.AEJobMessageTypeList.Codes.ExWarehouse, CustomsCommon.AE.AEJobMessageTypeList.Descriptions.ExWarehouse);
								expectedList.AddPair(CustomsCommon.AE.AEJobMessageTypeList.Codes.Import, CustomsCommon.AE.AEJobMessageTypeList.Descriptions.Import);
								expectedList.AddPair(CustomsCommon.AE.AEJobMessageTypeList.Codes.Refund, CustomsCommon.AE.AEJobMessageTypeList.Descriptions.Refund);
								expectedList.AddPair(CustomsCommon.AE.AEJobMessageTypeList.Codes.Transit, CustomsCommon.AE.AEJobMessageTypeList.Descriptions.Transit);
								expectedList.AddPair(CustomsCommon.AE.AEJobMessageTypeList.Codes.TemporaryAdmission, CustomsCommon.AE.AEJobMessageTypeList.Descriptions.TemporaryAdmission);
								expectedList.AddPair(CustomsCommon.AE.AEJobMessageTypeList.Codes.Transfer, CustomsCommon.AE.AEJobMessageTypeList.Descriptions.Transfer);
								expectedList.AddPair(CustomsCommon.AE.AEJobMessageTypeList.Codes.CargoTransfer, CustomsCommon.AE.AEJobMessageTypeList.Descriptions.CargoTransfer);
								break;
							}

						case Core.Constants.CountryCodes.Poland:
							{
								expectedList = new CodeDescriptionPairList();
								expectedList.AddPair(CustomsCommon.PL.PLJobMessageTypeList.Codes.Import, CustomsCommon.PL.PLJobMessageTypeList.Descriptions.Import);
								expectedList.AddPair(CustomsCommon.PL.PLJobMessageTypeList.Codes.Export, CustomsCommon.PL.PLJobMessageTypeList.Descriptions.Export);
								expectedList.AddPair(CustomsCommon.PL.PLJobMessageTypeList.Codes.ExitSummary, CustomsCommon.PL.PLJobMessageTypeList.Descriptions.ExitSummary);
								expectedList.AddPair(CustomsCommon.PL.PLJobMessageTypeList.Codes.NctsArrivalNotification, CustomsCommon.PL.PLJobMessageTypeList.Descriptions.NctsArrivalNotification);
								expectedList.AddPair(CustomsCommon.PL.PLJobMessageTypeList.Codes.NctsDeparture, CustomsCommon.PL.PLJobMessageTypeList.Descriptions.NctsDeparture);
								expectedList.AddPair(CustomsCommon.PL.PLJobMessageTypeList.Codes.NctsArrivalUnloadingRemarks, CustomsCommon.PL.PLJobMessageTypeList.Descriptions.NctsArrivalUnloadingRemarks);

								break;
							}

						default:
							{
								expectedList = JobMessageTypeList.GetNewListFor(countryCode);
								expectedList.RemoveCode(CustomsCommon.Shared.SharedJobMessageTypeList.Codes.MiscellaneousCustoms);

								break;
							}
					}

					AssertContainsExactElementsInAnyOrder($"Should contains expected list for {countryCode}.", expectedList, actualList);
				}
			});
		}

		public void TestGetListWithAdvanceShippingNotice()
		{
			var jobMessageTypeList1 = JobMessageTypeList.GetListWithAdvanceShippingNotice(Factory);
			Assert("ASN should be added", jobMessageTypeList1.ContainsCode("ASN"));
			var jobMessageTypeList2 = JobMessageTypeList.GetListWithAdvanceShippingNotice(Factory);
			Assert("ASN should be added", jobMessageTypeList2.ContainsCode("ASN"));

			AssertEquals("JobMessageTypeList should be cached", jobMessageTypeList1, jobMessageTypeList2);
		}

		public void TestBRJobMessageTypeList()
		{
			var list = JobMessageTypeList.GetNewListFor(Core.Constants.CountryCodes.Brazil);
			Assert("LIC should NOT be added when registry EnableImportLicense is OFF", !list.ContainsCode(CustomsCommon.BR.BRJobMessageTypeList.Codes.ImportLicense));
			Assert("LPC should NOT be added when registry EnableLPCO is OFF", !list.ContainsCode(CustomsCommon.BR.BRJobMessageTypeList.Codes.LPCO));
			Assert("ISW should NOT be added when registry EnableImportSiscomex is OFF", !list.ContainsCode(CustomsCommon.BR.BRJobMessageTypeList.Codes.ImportSiscomex));

			var mock = new Mock<Integration.Customs.BR.IBRCustomsDataRegistry>();
			mock.Setup(m => m.EnableLPCO).Returns(true);
			mock.Setup(m => m.EnableImportLicense).Returns(true);
			mock.Setup(m => m.EnableImportSiscomex).Returns(true);
			using (ObjectFactory.Substitute(mock.Object))
			{
				list = JobMessageTypeList.GetNewListFor(Core.Constants.CountryCodes.Brazil);
				Assert("LIC should be added when registry EnableImportLicense is YES", list.ContainsCode(CustomsCommon.BR.BRJobMessageTypeList.Codes.ImportLicense));
				Assert("LPC should be added when registry EnableLPCO is YES", list.ContainsCode(CustomsCommon.BR.BRJobMessageTypeList.Codes.LPCO));
				Assert("ISW should be added when registry EnableLPCO is YES", list.ContainsCode(CustomsCommon.BR.BRJobMessageTypeList.Codes.ImportSiscomex));
			}
		}

		public void TestGetNewListFor()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var codeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Asycuda;

			helper.CreateNewOrGetExistingDataGrouping("ZZ", "ZZ");
			helper.CreateNewOrGetExistingCusCodeType(codeType, "Asycuda Customs");
			helper.CreateNewOrGetExistingCusCodeList("ZZ", codeType, "NA", "Namibia", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			foreach (var pair in new[]
			{
				Tuple.Create(Core.Constants.CountryCodes.Australia, typeof(CustomsCommon.AU.AUJobMessageTypeList)),
				Tuple.Create(Core.Constants.CountryCodes.Canada, typeof(CustomsCommon.CA.CAJobMessageTypeList)),
				Tuple.Create(Core.Constants.CountryCodes.Japan, typeof(CustomsCommon.JP.JPJobMessageTypeList)),
				Tuple.Create(Core.Constants.CountryCodes.Switzerland, typeof(CustomsCommon.CH.CHJobMessageTypeList)),
				Tuple.Create(Core.Constants.CountryCodes.NewZealand, typeof(CustomsCommon.NZ.NZJobMessageTypeList)),
				Tuple.Create(Core.Constants.CountryCodes.Singapore, typeof(CustomsCommon.SG.SGJobMessageTypeList)),
				Tuple.Create(Core.Constants.CountryCodes.UnitedStates, typeof(CustomsCommon.US.USJobMessageTypeList)),
				Tuple.Create(Core.Constants.CountryCodes.SouthAfrica, typeof(CustomsCommon.ZA.ZAJobMessageTypeList)),
				Tuple.Create(Core.Constants.CountryCodes.PuertoRico, typeof(CustomsCommon.US.USJobMessageTypeList)),
				Tuple.Create(Core.Constants.CountryCodes.Germany, typeof(CustomsCommon.DE.DEJobMessageTypeList)),
				Tuple.Create(Core.Constants.CountryCodes.UnitedKingdom, typeof(CustomsCommon.EU.EUJobMessageTypeList)),
				Tuple.Create(Core.Constants.CountryCodes.Taiwan, typeof(CustomsCommon.TW.TWJobMessageTypeList)),
				Tuple.Create(Core.Constants.CountryCodes.China, typeof(JobMessageTypeList)),
				Tuple.Create(Core.Constants.CountryCodes.Ireland, typeof(CustomsCommon.IE.IEJobMessageTypeList)),
				Tuple.Create(Core.Constants.CountryCodes.Italy, typeof(CustomsCommon.IT.ITJobMessageTypeList)),
				Tuple.Create(Core.Constants.CountryCodes.Namibia, typeof(CustomsCommon.AsycudaCustoms.AsycudaJobMessageTypeList)),
				Tuple.Create(Core.Constants.CountryCodes.KoreaSouth, typeof(CustomsCommon.KR.KRJobMessageTypeList)),
				Tuple.Create(Core.Constants.CountryCodes.Brazil, typeof(CustomsCommon.BR.BRJobMessageTypeList)),
			})
			{
				AssertEquals("GetNewListFor " + pair.Item1, pair.Item2, JobMessageTypeList.GetNewListFor(pair.Item1).GetType());
				var list1 = JobMessageTypeList.GetCachedListFor(Factory, pair.Item1);
				var list2 = JobMessageTypeList.GetCachedListFor(Factory, pair.Item1);
				AssertEquals("GetCachedListFor " + pair.Item1, list1, list2);
			}
		}
	}
}
