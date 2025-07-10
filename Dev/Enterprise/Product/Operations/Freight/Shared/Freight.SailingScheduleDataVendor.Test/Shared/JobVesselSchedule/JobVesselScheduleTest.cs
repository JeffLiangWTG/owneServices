using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.SailingDataVendor.Business.Testing
{
	[TestedType(typeof(JobVesselSchedule))]
	sealed class JobVesselScheduleTest : EnterpriseBusinessObjectTestCase
	{
		public void TestUpdateSailingScheduleOnSave_WithoutMatchingLineOperator()
		{
			AssertUpdateSailingScheduleOnSave(false);
		}

		public void TestUpdateSailingScheduleOnSave_WithMatchingLineOperator()
		{
			AssertUpdateSailingScheduleOnSave(true);
		}

		public void AssertUpdateSailingScheduleOnSave(bool withLineOperator)
		{
			var voyageInOrigin = this.VoyageInOrigin;
			var voyageInDestination = this.VoyageInDestination;

			var voyageOutOrigin = this.VoyageOutOrigin;
			var voyageOutDestination = this.VoyageOutDestination;

			Factory.Save();

			var originPort = NewJobVesselSchedule("AUMEL", "Lloyds", "VoyBef", "VoyIn", FreightConstants.VesselDataProviders.OneStop);
			var viaPort = NewJobVesselSchedule("AUSYD", "Lloyds", "VoyIn", "VoyOut", FreightConstants.VesselDataProviders.OneStop);
			var destinationPort = NewJobVesselSchedule("AUBNE", "Lloyds", "VoyOut", "VoyAft", FreightConstants.VesselDataProviders.OneStop);

			if (withLineOperator)
			{
				originPort.EV_LineOperator = "LO";
				viaPort.EV_LineOperator = "LO";
				destinationPort.EV_LineOperator = "LO";

				var lineOperator = Factory.NewWithValidTestData<OrgHeader>();
				lineOperator.OH_IsActive = true;
				var oneStopCode = lineOperator.CustomsCodes.AddNew();
				oneStopCode.OK_CodeType = OrgCusCode.CodeTypes.OneStopCode;
				oneStopCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Australia;
				oneStopCode.OK_CustomsRegNo = "LO";

				Factory.Save();

				VoyageIn.JV_OH_Line = lineOperator.PK;
				VoyageOut.JV_OH_Line = lineOperator.PK;
			}
			else
			{
				VoyageIn.JV_OH_Line = ZGuid.Empty;
				VoyageOut.JV_OH_Line = ZGuid.Empty;
			}

			originPort.EV_ETD = ZDateTime.Now.AddDays(2);
			originPort.EV_ActualDeparture = ZDateTime.Now.AddDays(2).AddHours(12);

			viaPort.EV_ETA = ZDateTime.Now.AddDays(3);
			viaPort.EV_ActualArrival = ZDateTime.Now.AddDays(3).AddHours(12);
			viaPort.EV_ETD = ZDateTime.Now.AddDays(4);
			viaPort.EV_ActualDeparture = ZDateTime.Now.AddDays(4).AddHours(12);

			destinationPort.EV_ETA = ZDateTime.Now.AddDays(5);
			destinationPort.EV_ActualArrival = ZDateTime.Now.AddDays(5).AddHours(12);

			originPort.EV_ExportReceivalCommencementDate = new ZDateTime(2021, 1, 11, 0, 0, 0);
			originPort.EV_CargoCuttOff = new ZDateTime(2021, 1, 11, 12, 0, 0);
			originPort.EV_DataProviderReference = "ProviderRef2";
			viaPort.EV_ImportAvailability = new ZDateTime(2021, 1, 12, 0, 0, 0);
			viaPort.EV_ImportStorageCommences = new ZDateTime(2021, 1, 12, 12, 0, 0);
			viaPort.EV_ExportReceivalCommencementDate = new ZDateTime(2021, 1, 13, 0, 0, 0);
			viaPort.EV_CargoCuttOff = new ZDateTime(2021, 1, 13, 12, 0, 0);
			destinationPort.EV_ImportAvailability = new ZDateTime(2021, 1, 14, 0, 0, 0);
			destinationPort.EV_ImportStorageCommences = new ZDateTime(2021, 1, 14, 12, 0, 0);

			Factory.Save();

			var voyageInOriginScheduleChangeEmailSupporter = voyageInOrigin as IScheduleChangeEmailSupporter;
			if (voyageInOriginScheduleChangeEmailSupporter != null)
			{
				AssertDateChange(ScheduleDateTypes.Codes.ETD, ZDateTime.Now.AddDays(2), voyageInOrigin.JA_E_DEP, voyageInOriginScheduleChangeEmailSupporter);
				AssertDateChange(ScheduleDateTypes.Codes.ATD, ZDateTime.Now.AddDays(2).AddHours(12), voyageInOrigin.JA_A_DEP, voyageInOriginScheduleChangeEmailSupporter);
				AssertDateChange(ScheduleDateTypes.Codes.FCLReceivalCommences, new ZDateTime(2021, 1, 11, 0, 0, 0), voyageInOrigin.JA_ReceivalCommences, voyageInOriginScheduleChangeEmailSupporter);
				AssertDateChange(ScheduleDateTypes.Codes.FCLCutOff, new ZDateTime(2021, 1, 11, 12, 0, 0), voyageInOrigin.JA_CutOff, voyageInOriginScheduleChangeEmailSupporter);
			}

			var voyageInDestinationScheduleChangeEmailSupporter = voyageInDestination as IScheduleChangeEmailSupporter;
			if (voyageInDestinationScheduleChangeEmailSupporter != null)
			{
				AssertDateChange(ScheduleDateTypes.Codes.ETA, ZDateTime.Now.AddDays(3), voyageInDestination.JB_E_ARV, voyageInDestinationScheduleChangeEmailSupporter);
				AssertDateChange(ScheduleDateTypes.Codes.ATA, ZDateTime.Now.AddDays(3).AddHours(12), voyageInDestination.JB_A_ARV, voyageInDestinationScheduleChangeEmailSupporter);
				AssertDateChange(ScheduleDateTypes.Codes.FCLAvailable, new ZDateTime(2021, 1, 12, 0, 0, 0), voyageInDestination.JB_AvailabilityDate, voyageInDestinationScheduleChangeEmailSupporter);
				AssertDateChange(ScheduleDateTypes.Codes.FCLStorage, new ZDateTime(2021, 1, 12, 12, 0, 0), voyageInDestination.JB_StorageDate, voyageInDestinationScheduleChangeEmailSupporter);
			}

			AssertEquals("ProviderRef2", voyageInOrigin.JA_DepartReference);

			AssertLogExists(VoyageIn, string.Format("Updated the {0}", voyageInOrigin.JA_E_DEPInfo.HumanReadableName));
			AssertLogExists(VoyageIn, string.Format("Updated the {0}", voyageInOrigin.JA_A_DEPInfo.HumanReadableName));
			AssertLogExists(VoyageIn, string.Format("Updated the {0}", voyageInDestination.JB_E_ARVInfo.HumanReadableName));
			AssertLogExists(VoyageIn, string.Format("Updated the {0}", voyageInDestination.JB_A_ARVInfo.HumanReadableName));
			AssertLogExists(VoyageIn, string.Format("Updated the {0}", voyageInOrigin.JA_DepartReferenceInfo.HumanReadableName));

			var voyageOutOriginScheduleChangeEmailSupporter = voyageOutOrigin as IScheduleChangeEmailSupporter;
			if (voyageOutOriginScheduleChangeEmailSupporter != null)
			{
				AssertDateChange(ScheduleDateTypes.Codes.ETD, ZDateTime.Now.AddDays(4), voyageOutOrigin.JA_E_DEP, voyageOutOriginScheduleChangeEmailSupporter);
				AssertDateChange(ScheduleDateTypes.Codes.ATD, ZDateTime.Now.AddDays(4).AddHours(12), voyageOutOrigin.JA_A_DEP, voyageOutOriginScheduleChangeEmailSupporter);
				AssertDateChange(ScheduleDateTypes.Codes.FCLReceivalCommences, new ZDateTime(2021, 1, 13, 0, 0, 0), voyageOutOrigin.JA_ReceivalCommences, voyageOutOriginScheduleChangeEmailSupporter);
				AssertDateChange(ScheduleDateTypes.Codes.FCLCutOff, new ZDateTime(2021, 1, 13, 12, 0, 0), voyageOutOrigin.JA_CutOff, voyageOutOriginScheduleChangeEmailSupporter);
			}
			var voyageOutDestinationScheduleChangeEmailSupporter = voyageOutDestination as IScheduleChangeEmailSupporter;
			if (voyageOutDestinationScheduleChangeEmailSupporter != null)
			{
				AssertDateChange(ScheduleDateTypes.Codes.ETA, ZDateTime.Now.AddDays(5), voyageOutDestination.JB_E_ARV, voyageOutDestinationScheduleChangeEmailSupporter);
				AssertDateChange(ScheduleDateTypes.Codes.ATA, ZDateTime.Now.AddDays(5).AddHours(12), voyageOutDestination.JB_A_ARV, voyageOutDestinationScheduleChangeEmailSupporter);
				AssertDateChange(ScheduleDateTypes.Codes.FCLAvailable, new ZDateTime(2021, 1, 14, 0, 0, 0), voyageOutDestination.JB_AvailabilityDate, voyageOutDestinationScheduleChangeEmailSupporter);
				AssertDateChange(ScheduleDateTypes.Codes.FCLStorage, new ZDateTime(2021, 1, 14, 12, 0, 0), voyageOutDestination.JB_StorageDate, voyageOutDestinationScheduleChangeEmailSupporter);
			}

			AssertLogExists(VoyageOut, string.Format("Updated the {0}", voyageOutOrigin.JA_E_DEPInfo.HumanReadableName));
			AssertLogExists(VoyageOut, string.Format("Updated the {0}", voyageOutOrigin.JA_A_DEPInfo.HumanReadableName));
			AssertLogExists(VoyageOut, string.Format("Updated the {0}", voyageOutDestination.JB_E_ARVInfo.HumanReadableName));
			AssertLogExists(VoyageOut, string.Format("Updated the {0}", voyageOutDestination.JB_A_ARVInfo.HumanReadableName));

			AssertEquals("JX_JA_CTOReceivalCommences", originPort.EV_ExportReceivalCommencementDate, VoyageIn.Sailings[0].JX_JA_CTOReceivalCommences);
			AssertEquals("JX_JA_CTOCutOff", originPort.EV_CargoCuttOff, VoyageIn.Sailings[0].JX_JA_CTOCutOff);
			AssertEquals("JX_JB_CTOAvailabilityDate", viaPort.EV_ImportAvailability, VoyageIn.Sailings[0].JX_JB_CTOAvailabilityDate);
			AssertEquals("JX_JB_CTOStorageDate", viaPort.EV_ImportStorageCommences, VoyageIn.Sailings[0].JX_JB_CTOStorageDate);

			AssertLogExists(VoyageIn, string.Format("Updated the {0}", voyageInOrigin.JA_ReceivalCommencesInfo.HumanReadableName));
			AssertLogExists(VoyageIn, string.Format("Updated the {0}", voyageInOrigin.JA_CutOffInfo.HumanReadableName));
			AssertLogExists(VoyageIn, string.Format("Updated the {0}", voyageInDestination.JB_AvailabilityDateInfo.HumanReadableName));
			AssertLogExists(VoyageIn, string.Format("Updated the {0}", voyageInDestination.JB_StorageDateInfo.HumanReadableName));

			AssertEquals("JX_JA_CTOReceivalCommences", viaPort.EV_ExportReceivalCommencementDate, VoyageOut.Sailings[0].JX_JA_CTOReceivalCommences);
			AssertEquals("JX_JA_CTOCutOff", viaPort.EV_CargoCuttOff, VoyageOut.Sailings[0].JX_JA_CTOCutOff);
			AssertEquals("JX_JB_CTOAvailabilityDate", destinationPort.EV_ImportAvailability, VoyageOut.Sailings[0].JX_JB_CTOAvailabilityDate);
			AssertEquals("JX_JB_CTOStorageDate", destinationPort.EV_ImportStorageCommences, VoyageOut.Sailings[0].JX_JB_CTOStorageDate);

			AssertLogExists(VoyageOut, string.Format("Updated the {0}", voyageOutOrigin.JA_ReceivalCommencesInfo.HumanReadableName));
			AssertLogExists(VoyageOut, string.Format("Updated the {0}", voyageOutOrigin.JA_CutOffInfo.HumanReadableName));
			AssertLogExists(VoyageOut, string.Format("Updated the {0}", voyageOutDestination.JB_AvailabilityDateInfo.HumanReadableName));
			AssertLogExists(VoyageOut, string.Format("Updated the {0}", voyageOutDestination.JB_StorageDateInfo.HumanReadableName));
		}

		void AssertDateChange(string dateType, ZDateTime expectedDate, ZDateTime actualDate, IScheduleChangeEmailSupporter scheduleChangeEmailSupporter)
		{
			AssertZDatesWithin5Minutes(dateType, expectedDate, actualDate);
			AssertEquals(FreightConstants.VesselDataProviders.OneStop, scheduleChangeEmailSupporter.GetDataProvider(dateType));
		}

		void AssertLogExists(BusinessObject bizO, string reference)
		{
			var query = new ZQuery(StmALogSchema.SL_Parent, bizO.PK);
			query.AddToFilter(StmALogSchema.SL_SE_NKEvent, "EDT");
			query.AddToFilter(StmALogSchema.SL_Reference, SQLComparisonOperator.Contains, reference);

			var logs = Factory.Load<StmALog>(query);
			AssertNotNull(logs);
			AssertEquals("Updates are logged once only", 1, logs.Length);
		}

		#region Implementation

		JobVesselSchedule NewJobVesselSchedule(ZString portCode, ZString lloyds, ZString voyageIn, ZString voyageOut, string dataProvider)
		{
			JobVesselSchedule result = Factory.New<JobVesselSchedule>();
			result.EV_RL_NKPortCode = portCode;
			result.EV_IMOLloydsNumber = lloyds;
			result.EV_ShipOperatorVoyageIn = voyageIn;
			result.EV_ShipOperatorVoyageOut = voyageOut;
			result.EV_DataProvider = dataProvider;
			return result;
		}

		JobVoyage VoyageIn
		{
			get
			{
				if (voyageIn == null)
				{
					voyageIn = Factory.New<JobVoyage>();
					voyageIn.JV_AirSeaRoad = Core.Constants.TransportModes.Sea;
					voyageIn.JV_RV_NKVessel = RefVessel.RV_FK;
					voyageIn.JV_VoyageFlight = "VoyIn";
				}

				return voyageIn;
			}
		}
		JobVoyage voyageIn;

		VoyageOrigin VoyageInOrigin
		{
			get
			{
				VoyageOrigin result = VoyageIn.Origins.Count == 0 ? null : VoyageIn.Origins[0];

				if (result == null)
				{
					result = VoyageIn.Origins.AddNew();
					result.JA_RL_NKPortOfLoading = "AUMEL";
				}

				return result;
			}
		}

		VoyageDestination VoyageInDestination
		{
			get
			{
				VoyageDestination result = VoyageIn.Destinations.Count == 0 ? null : VoyageIn.Destinations[0];

				if (result == null)
				{
					result = VoyageIn.Destinations.AddNew();
					result.JB_RL_NKPortOfDischarge = "AUSYD";
				}

				return result;
			}
		}

		JobVoyage VoyageOut
		{
			get
			{
				if (voyageOut == null)
				{
					voyageOut = Factory.New<JobVoyage>();
					voyageOut.JV_AirSeaRoad = Core.Constants.TransportModes.Sea;
					voyageOut.JV_RV_NKVessel = RefVessel.RV_FK;
					voyageOut.JV_VoyageFlight = "VoyOut";
				}

				return voyageOut;
			}
		}
		JobVoyage voyageOut;

		VoyageOrigin VoyageOutOrigin
		{
			get
			{
				VoyageOrigin result = VoyageOut.Origins.Count == 0 ? null : VoyageOut.Origins[0];

				if (result == null)
				{
					result = VoyageOut.Origins.AddNew();
					result.JA_RL_NKPortOfLoading = "AUSYD";
				}

				return result;
			}
		}

		VoyageDestination VoyageOutDestination
		{
			get
			{
				VoyageDestination result = VoyageOut.Destinations.Count == 0 ? null : VoyageOut.Destinations[0];

				if (result == null)
				{
					result = VoyageOut.Destinations.AddNew();
					result.JB_RL_NKPortOfDischarge = "AUBNE";
				}

				return result;
			}
		}

		RefVessel RefVessel
		{
			get
			{
				if (refVessel == null)
				{
					refVessel = Factory.New<RefVessel>();
					refVessel.RV_Name = "Vessel";
					refVessel.RV_LloydsNumber = "Lloyds";
				}

				return refVessel;
			}
		}

		RefVessel refVessel;

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var result = factory.New<JobVesselSchedule>();
			result.EV_RL_NKPortCode = "AUMEL";
			result.EV_IMOLloydsNumber = "Lloyds";
			result.EV_ShipOperatorVoyageIn = "VoyBef";
			result.EV_ShipOperatorVoyageOut = "VoyIn";
			result.EV_DataProvider = FreightConstants.VesselDataProviders.OneStop;

			return result;
		}

		#endregion
	}
}
