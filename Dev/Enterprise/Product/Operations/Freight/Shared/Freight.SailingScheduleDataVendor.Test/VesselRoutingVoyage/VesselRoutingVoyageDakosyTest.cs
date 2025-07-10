using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.SailingDataVendor.Business.Testing
{
	[TestedType(typeof(VesselRoutingVoyage))]
	sealed class VesselRoutingVoyageDakosyTest : VesselRoutingVoyageTestBase
	{
		public override void TestE8_OH_LineOperator_DefaultsOrgFromE8_LineOperator()
		{
			VoyageWithLineOperators.E8_LineOperator = "";
			Factory.Save();

			AssertNotNull(LineOperator);
			Assert(VoyageWithLineOperators.E8_OH_LineOperator.IsEmpty);

			VoyageWithLineOperators.E8_DataProvider = DataProvider;

			VoyageWithLineOperators.E8_LineOperator = "LO2";
			Factory.Save();

			AssertNotNull(LineOperator);
			AssertEquals("Line Operator Organisation was defaulted from the SCAC Code.", LineOperator.PK, VoyageWithLineOperators.E8_OH_LineOperator);

			VoyageWithLineOperators.E8_LineOperator = "LO1";
			Factory.Save();

			AssertNotNull(LineOperator);
			AssertEquals("Line Operator Organisation was defaulted from the Dakosy Participant Code.", LineOperator.PK, VoyageWithLineOperators.E8_OH_LineOperator);
		}

		public void TestForeignPortsForDakosy()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Germany))
			{
				NewJobVesselSchedule("DEHAM", ZDateTime.Empty, ZDateTime.Today.AddDays(1), "Lloy999", ZString.Empty, "VoyageTest", "VLSTEST", "DEMO SHIP");
				NewJobVesselSchedule("CYLMS", ZDateTime.Today.AddDays(3), ZDateTime.Empty, "Lloy999", "VoyageTest", ZString.Empty, ZString.Empty, "DEMO SHIP");
				Factory.Save();

				VoyageCollection.Load(new ZQuery(ViewVesselRoutingVoyagesSchema.E8_Voyage, "VoyageTest"));
				var voyage = VoyageCollection[0];

				var foreignPorts = voyage.ForeignPorts;
				AssertEquals(1, foreignPorts.Count);
				AssertEquals("CYLMS", foreignPorts[0]);
			}
		}

		#region Overrides

		protected override ZString DataProvider { get { return FreightConstants.VesselDataProviders.DAKOSY; } }

		protected override OrgHeader LineOperator
		{
			get
			{
				if (lineOperator == null)
				{
					lineOperator = Factory.NewWithValidTestData<OrgHeader>();

					var dpcCode = lineOperator.CustomsCodes.AddNew();
					dpcCode.OK_CodeType = GermanyOrgCusCodeInfo.OrgCusCodes.DakosyParticipantCode;
					dpcCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Germany;
					dpcCode.OK_CustomsRegNo = "LO1";

					var scacCode = lineOperator.CustomsCodes.AddNew();
					scacCode.OK_CodeType = OrgCusCode.CodeTypes.CarrierCode;
					scacCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Malaysia;
					scacCode.OK_CustomsRegNo = "LO2";
				}

				Factory.Save();

				return lineOperator;
			}
		}

		#endregion

	}
}
