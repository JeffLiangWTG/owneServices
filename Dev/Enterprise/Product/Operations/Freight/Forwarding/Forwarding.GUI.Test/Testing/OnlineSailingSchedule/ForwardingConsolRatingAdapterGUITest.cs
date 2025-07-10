using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.GUI;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using static Enterprise.Core.Constants;

namespace Enterprise.Freight.Forwarding.GUI.Testing
{
	public class ForwardingConsolRatingAdapterGUITest : TestCaseWithFactory
	{
		public void TestForwardingConsolRatingAdapter_UpdateTransports_GSSValidationShouldBeSuppressed()
		{
			try
			{
				var carrierOrganisation = Factory.NewWithValidTestData<OrgHeader>();
				carrierOrganisation.OH_Code = "CMAC";
				var cusCode = carrierOrganisation.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CarrierCode, "CMAC", CountryCodes.UnitedStates);

				var vessel = Factory.NewWithValidTestData<RefVessel>();
				vessel.RV_Name = "NYK METEOR";
				vessel.RV_Code = "1000980";

				var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
				var consol = Factory.New<ForwardingConsol>();
				consol.JK_TransportMode = "SEA";
				consol.JK_ConsolMode = "FCL";
				consol.JK_RL_NKLoadPort = "NLAMS";
				consol.JK_RL_NKDischargePort = "USLAX";
				consol.JK_OA_ShippingLineAddress = carrierOrganisation.MainAddress.PK;

				var currentConsolTransportLegPKs = new[]
				{
					consol.Transports[0].PK,
					consol.Transports.AddNew().PK,
					consol.Transports.AddNew().PK
				};

				var voyage = Factory.New<JobVoyage>();
				voyage.JV_AirSeaRoad = TransportModes.Sea;
				voyage.JV_VoyageFlight = "V9849384";
				voyage.JV_RV_NKVessel = vessel.RV_FK;
				voyage.JV_OH_Line = carrierOrganisation.PK;

				var origin = voyage.Origins.AddNew();
				origin.JA_RL_NKPortOfLoading = "NLAMS";
				origin.JA_E_DEP = new ZDateTime(2017, 8, 4);

				var destination = voyage.Destinations.AddNew();
				destination.JB_RL_NKPortOfDischarge = "USLAX";
				destination.JB_E_ARV = new ZDateTime(2017, 9, 5);
				voyage.GenerateSailings();

				var transport = consol.Transports[0];
				transport.JW_JX = voyage.Sailings[0].PK;
				transport.JW_IsLinked = true;
				consol.Shipments.Add(shipment);
				Factory.Save();

				var consolRatingRoute = new ConsolRatingRoute(consol);
				var ratingAdapter = new AutoRatingProxy(new ForwardingConsolRatingAdapter(consolRatingRoute));

				var transports = new[]
				{
					new TransportForTest()
					{
						JW_Status = TransportStatus.Planned,
						JW_IsLinked = true,
						JW_LegOrder = 1,
						JW_TransportMode = TransportModes.Sea,
						JW_VoyageFlight = "V9849384",
						JW_Vessel = "NYK METEOR",
						JW_RL_NKLoadPort = "NLRTM",
						JW_RL_NKDiscPort = "USLAX",
						JW_ETA = new ZDateTime(2021, 01, 15, 14, 0, 0),
						JW_ETD = new ZDateTime(2021, 01, 10, 07, 0, 0),
						JW_LegNotes = "Some Notes",
						JW_DocumentaryCutOff = new ZDateTime(2021, 01, 08, 10, 0, 0),
						JW_TerminalCutOff = new ZDateTime(2021, 01, 08, 19, 0, 0),
						JW_VGMCutOff = new ZDateTime(2021, 01, 08, 17, 0, 0),
						JW_OA_CarrierAddress = carrierOrganisation.MainAddress.PK,
					},
				};

				MockOnlineSailingSchedulesDataVendor.RegisterThisSubTypeOverride();
				MockOnlineSailingSchedulesDataVendor.Instance.SetRoutesProvider(new RoutesProviderForTest());
				ConfirmationProvider.Register(Factory);

				ratingAdapter.UpdateTransports(transports);

				AssertNullOrEmpty(UnitTestUserNotification.Instance.LastMessage.Text);
			}
			finally
			{
				MockOnlineSailingSchedulesDataVendor.UnregisterThisSubTypeOverride();
			}
		}
	}
}
