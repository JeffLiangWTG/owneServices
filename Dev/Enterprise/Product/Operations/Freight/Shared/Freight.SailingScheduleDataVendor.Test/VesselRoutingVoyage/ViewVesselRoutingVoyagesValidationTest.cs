using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Business;
using Enterprise.Freight.SailingDataVendor.Shared;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.SailingDataVendor.Business.Testing
{
	sealed class ViewVesselRoutingVoyagesValidationTest : BusinessObjectValidationTestCase
	{
		#region E8_OH_LineOperator

		public void TestE8_OH_LineOperatorIsValidOrganisation()
		{
			OrgHeader organisation = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader seaCarrierOrganisation = Factory.NewWithValidTestData<OrgHeader>();
			seaCarrierOrganisation.OH_IsShippingLine = true;
			seaCarrierOrganisation.OH_IsShippingProvider = true;

			Voyage.E8_OH_LineOperator = organisation.PK;
			AssertHasErrors("With a non-shipping line organisation", Voyage.E8_OH_LineOperatorInfo);

			Voyage.E8_OH_LineOperator = seaCarrierOrganisation.PK;
			AssertNoErrors("With a sea shipping line organisation", Voyage.E8_OH_LineOperatorInfo);
		}

		public void TestE8_OH_LineOperatorMatchesExternalCode()
		{
			var lineOperator = Factory.NewWithValidTestData<OrgHeader>();
			lineOperator.OH_IsActive = true;
			lineOperator.OH_IsShippingLine = true;
			lineOperator.OH_IsShippingProvider = true;

			var schedule = NewJobVesselSchedule("AUSYD", ZDateTime.Today.AddDays(1), ZDateTime.Today.AddDays(2), "Lloyds", "Voyage", "Voyage");
			Factory.Save();

			var voyage = Factory.LoadTop1<VesselRoutingVoyage>(new ZQuery(ViewVesselRoutingVoyagesSchema.E8_Voyage, "Voyage"));
			voyage.E8_OH_LineOperator = lineOperator.PK;

			Assert(voyage.E8_LineOperator.IsEmpty);
			AssertNoErrors("No errors: Line Operator Code is empty.", voyage.E8_OH_LineOperatorInfo);

			voyage.E8_DataProvider = FreightConstants.VesselDataProviders.OneStop;
			voyage.E8_LineOperator = "LO1";
			voyage.E8_OH_LineOperator = lineOperator.PK;

			AssertHasErrors("Error: Organisation does NOT match the line operator code.", voyage.E8_OH_LineOperatorInfo);

			var oneStopCode = lineOperator.CustomsCodes.AddNew();
			oneStopCode.OK_CodeType = OrgCusCode.CodeTypes.OneStopCode;
			oneStopCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Australia;
			oneStopCode.OK_CustomsRegNo = "LO1";
			Factory.Save();

			voyage.E8_OH_LineOperator = lineOperator.PK;

			AssertNoErrors("No errors: Organisation matches the line operator code.", voyage.E8_OH_LineOperatorInfo);

			voyage.E8_DataProvider = FreightConstants.VesselDataProviders.DAKOSY;
			voyage.E8_LineOperator = "LO2";
			voyage.E8_OH_LineOperator = lineOperator.PK;

			AssertHasErrors("Error: Organisation does NOT match the line operator code.", voyage.E8_OH_LineOperatorInfo);

			var dakosyCode = lineOperator.CustomsCodes.AddNew();
			dakosyCode.OK_CodeType = GermanyOrgCusCodeInfo.OrgCusCodes.DakosyParticipantCode;
			dakosyCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Germany;
			dakosyCode.OK_CustomsRegNo = "LO2";

			Factory.Save();

			voyage.E8_OH_LineOperator = lineOperator.PK;

			AssertNoErrors("No errors: Organisation matches the line operator code.", voyage.E8_OH_LineOperatorInfo);
		}

		#endregion

		#region E8_ForeignPortToAdd

		public void TestValidateE8_ForeignPortToAdd()
		{
			Voyage.E8_ForeignPortToAdd = "AUSYD";
			AssertHasErrors("Error expected when local port used", Voyage.E8_ForeignPortToAddInfo);

			Voyage.E8_ForeignPortToAdd = "MYPKG";
			AssertNoErrors("No error expected when foreign port used", Voyage.E8_ForeignPortToAddInfo);
		}

		#endregion

		#region E8_LloydsNumber

		public void TestValidateE8_LloydsNumber_ValidatedOnLoad_OneStop()
		{
			RunTestValidateE8_LloydsNumber_ValidatedOnLoad(FreightConstants.VesselDataProviders.OneStop);
		}

		public void TestValidateE8_LloydsNumber_ValidatedOnLoad_DBH()
		{
			StaticCurrentFetcher.Instance.CurrentCompany.SetCountry(Constants.CountryCodes.Germany);
			RunTestValidateE8_LloydsNumber_ValidatedOnLoad(FreightConstants.VesselDataProviders.DBH);
		}

		public void RunTestValidateE8_LloydsNumber_ValidatedOnLoad(string dataProvider)
		{
			VesselRoutingVoyage voyage = VoyageCollection.AddNew();
			voyage.E8_DataProvider = dataProvider;

			voyage.E8_LloydsNumber = "XXXX";
			VesselRoutingPortPair portPair = voyage.PortPairs.AddNew();
			portPair.E9_RL_NKLoadPort = "MYPKG";
			portPair.E9_RL_NKDischargePort = "AUSYD";
			if (dataProvider != FreightConstants.VesselDataProviders.OneStop)
			{
				voyage.E8_EV_PK = ZGuid.NewZGuid();
				portPair.E9_E1_EV = voyage.E8_EV_PK;
			}
			portPair.E9_IsSelected = true;

			AssertHasWarning(voyage.E8_LloydsNumberInfo, "This vessel is not registered. A vessel will be created with this Vessel Name and Lloyds number.");

			voyage.E8_LloydsNumber = Factory.LoadTop1<RefVessel>(new ZQuery()).RV_LloydsNumber;
			AssertNoWarnings("No warning when the voyage has a valid Lloyds Number", voyage.E8_LloydsNumberInfo);
		}

		#region Test should pass when dbh data will be imported into production area

		//public void TestValidateE8_LloydsNumber_AllowEmpty()
		//{
		//    VesselRoutingVoyage voyage = VoyageCollection.AddNew();
		//    voyage.E8_LloydsNumber = string.Empty;

		//    AssertNoErrors("Should allow empty Lloyds Number", voyage.E8_LloydsNumberInfo);
		//}

		#endregion

		#endregion

		#region Implementation

		VesselRoutingVoyageCollection VoyageCollection
		{
			get
			{
				if (fVoyageCollection == null)
				{
					fVoyageCollection = new VesselRoutingVoyageCollection(Factory);
					fVoyageCollection.PortPairTypeFilter = PortPairTypes.All;
				}
				return fVoyageCollection;
			}
		}
		VesselRoutingVoyageCollection fVoyageCollection;

		VesselRoutingVoyage Voyage
		{
			get
			{
				if (fVoyage == null)
				{
					JobVesselRouting foreignPort = NewJobVesselRouting("MYPKG", "Lloyds", "Voyage");
					JobVesselSchedule domesticPort1 = NewJobVesselSchedule("AUSYD", ZDateTime.Today.AddDays(1), ZDateTime.Today.AddDays(2), "Lloyds", "Voyage", "Voyage");
					JobVesselSchedule domesticPort2 = NewJobVesselSchedule("AUMEL", ZDateTime.Today.AddDays(3), ZDateTime.Today.AddDays(4), "Lloyds", "Voyage", "Voyage");
					Factory.Save();

					VoyageCollection.Load(new ZQuery(ViewVesselRoutingVoyagesSchema.E8_Voyage, "Voyage"));
					fVoyage = VoyageCollection[0];
				}
				return fVoyage;
			}
		}
		VesselRoutingVoyage fVoyage;

		JobVesselRouting NewJobVesselRouting(ZString portCode, ZString lloyds, ZString voyage)
		{
			JobVesselRouting result = Factory.New<JobVesselRouting>();
			result.E1_RL_NKDischargePortCode = portCode;
			result.E1_LloydsID = lloyds;
			result.E1_VoyageNumber = voyage;
			return result;
		}

		JobVesselSchedule NewJobVesselSchedule(ZString portCode, ZDateTime eTA, ZDateTime eTD, ZString lloyds, ZString voyageIn, ZString voyageOut)
		{
			JobVesselSchedule result = Factory.New<JobVesselSchedule>();
			result.EV_RL_NKPortCode = portCode;
			result.EV_ETA = eTA;
			result.EV_ETD = eTD;
			result.EV_IMOLloydsNumber = "Lloyds";
			result.EV_ShipOperatorVoyageIn = voyageIn;
			result.EV_ShipOperatorVoyageOut = voyageOut;
			return result;
		}

		#endregion
	}
}
