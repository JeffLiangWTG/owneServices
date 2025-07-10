using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.SailingDataVendor.Business.Testing
{
	sealed class DESailingScheduleFeedDataVendorTest : SailingScheduleFeedDataVendorTest
	{
		public void TestUpdateDepartureReference()
		{
			var loadPort = NewJobVesselSchedule("AUSYD", "Lloyds", "Voyage");
			loadPort.EV_DataProviderReference = "DB9";

			Factory.Save();
			DataVendor.UpdateVoyageOrigin(Origin);

			AssertEquals("DB9", Origin.JA_DepartReference);
			Origin.FetchSailings().ForEach(sailing => AssertEquals("DB9", sailing.JX_DeparturePortRouteId));
		}

		public void TestUpdateDepartureReferenceIfCarrierDakosyCodesBothExist()
		{
			var loadPort = NewJobVesselSchedule("AUSYD", "Lloyds", "Voyage");
			loadPort.EV_DataProviderReference = "DB1";
			Factory.Save();

			DataVendor.UpdateVoyageOrigin(Origin);
			AssertEquals("DB1", Origin.JA_DepartReference);

			var carrierCode = LineOperator.CustomsCodes.AddNew();
			carrierCode.OK_CodeType = OrgCusCode.CodeTypes.CarrierCode;
			carrierCode.OK_RN_NKCodeCountry = ZString.Empty;
			carrierCode.OK_CustomsRegNo = "C1";
			Factory.Save();

			var loadPort2 = NewJobVesselSchedule("AUSYD", "Lloyds", "Voyage");
			loadPort2.EV_DataProviderReference = "DB9";
			loadPort2.EV_ETD = new ZDateTime(2000, 1, 1);
			loadPort2.EV_ActualDeparture = new ZDateTime(2000, 1, 2);
			loadPort2.EV_ImportAvailability = new ZDateTime(2000, 1, 5);
			loadPort2.EV_ImportStorageCommences = new ZDateTime(2000, 1, 6);
			loadPort2.EV_DataProvider = DataProvider;
			loadPort2.EV_LineOperator = "LO";
			Voyage.JV_OH_Line = LineOperator.PK;
			Factory.Save();

			DataVendor.UpdateVoyageOrigin(Origin);

			AssertEquals("JA_E_DEP updated with Carrier info.", loadPort2.EV_ETD, Origin.JA_E_DEP);
			AssertEquals("JA_A_DEP updated with Carrier info.", loadPort2.EV_ActualDeparture, Origin.JA_A_DEP);
			AssertEquals("JX_JA_CTOCutOff updated with Carrier info.", loadPort2.EV_CargoCuttOff, Voyage.Sailings[0].JX_JA_CTOCutOff);
			AssertEquals("JX_JA_CTOReceivalCommences updated with Carrier info.", loadPort2.EV_ExportReceivalCommencementDate, Voyage.Sailings[0].JX_JA_CTOReceivalCommences);
			AssertEquals("DB9", Origin.JA_DepartReference);

			LineOperator.CustomsCodes.RemoveAndDeleteAll();
			Factory.Save();
		}

		public void TestUpdateCTO()
		{
			var loadPort = NewJobVesselSchedule("AUSYD", "Lloyds", "Voyage");
			loadPort.EV_TerminalID = "AE3";

			var cto = Factory.New<OrgHeader>();
			cto.OH_Code = "CTOORG";
			cto.CustomsCodes.AddNew(GermanyOrgCusCodeInfo.OrgCusCodes.DakosyParticipantCode, "AE3", Constants.CountryCodes.Germany);

			Factory.Save();
			DataVendor.UpdateVoyageOrigin(Origin);

			AssertEquals(cto.PK, Origin.JA_Calc_DepartureCTOAddressOrg);
		}

		public void TestUpdateBerth()
		{
			var loadPort = NewJobVesselSchedule("AUSYD", "Lloyds", "Voyage");
			loadPort.EV_TerminalID = "AE3";

			var cto = Factory.New<OrgHeader>();
			cto.OH_Code = "CTOORG";
			cto.CustomsCodes.AddNew(GermanyOrgCusCodeInfo.OrgCusCodes.DakosyBerthCode, "AE3", Constants.CountryCodes.Germany);

			Factory.Save();
			DataVendor.UpdateVoyageOrigin(Origin);

			AssertEquals("AE3", Origin.JA_Berth);
			AssertEquals(cto.PK, Origin.JA_Calc_DepartureCTOAddressOrg);
		}

		#region Implementation

		protected override SailingScheduleFeedDataVendor DataVendor
		{
			get
			{
				if (dakosyVendor == null)
				{
					dakosyVendor = new DESailingScheduleFeedDataVendor();
				}

				return dakosyVendor;
			}
		}
		DESailingScheduleFeedDataVendor dakosyVendor;

		protected override ZString DataProvider
		{
			get { return FreightConstants.VesselDataProviders.DAKOSY; }
		}

		protected override OrgHeader LineOperator
		{
			get
			{
				if (lineOperator == null)
				{
					lineOperator = Factory.NewWithValidTestData<OrgHeader>();
					lineOperator.OH_IsShippingLine = true;
					lineOperator.OH_IsShippingProvider = true;

					var dakosyCode = lineOperator.CustomsCodes.AddNew();
					dakosyCode.OK_CodeType = GermanyOrgCusCodeInfo.OrgCusCodes.DakosyParticipantCode;
					dakosyCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Germany;
					dakosyCode.OK_CustomsRegNo = "LO";

					Factory.Save();
				}

				return lineOperator;
			}
		}
		OrgHeader lineOperator;

		#endregion

	}
}
