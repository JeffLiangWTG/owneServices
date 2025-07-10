using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Freight.SailingDataVendor.Business.Testing
{
	[TestedType(typeof(VesselRoutingVoyage))]
	sealed class VesselRoutingVoyageOneStopTest : VesselRoutingVoyageTestBase
	{
		public override void TestE8_OH_LineOperator_DefaultsOrgFromE8_LineOperator()
		{
			VoyageWithLineOperators.E8_LineOperator = "";
			Factory.Save();

			AssertNotNull(LineOperator);
			Assert(VoyageWithLineOperators.E8_OH_LineOperator.IsEmpty);

			VoyageWithLineOperators.E8_LineOperator = "LO1";
			VoyageWithLineOperators.E8_DataProvider = DataProvider;
			Factory.Save();

			AssertNotNull(LineOperator);
			AssertEquals("Line Operator Organisation was defaulted from the Line Operator 1Stop Code.", LineOperator.PK, VoyageWithLineOperators.E8_OH_LineOperator);
		}

		#region Foreign Ports

		public void TestForeignPorts_DefaultPortsComeFromJobVesselRouting()
		{
			AssertEquals("Default ForeignPorts", 1, Voyage.ForeignPorts.Count);
			AssertEquals("Default ForeignPorts", "MYPKG", Voyage.ForeignPorts[0]);
		}

		public void TestLoadOriginalForeignPortList()
		{
			NewJobVesselRouting("MYPKG", Voyage.E8_LloydsNumber, Voyage.E8_Voyage);
			NewJobVesselRouting("SGSIN", Voyage.E8_LloydsNumber, Voyage.E8_Voyage);
			NewJobVesselRouting("NZAKL", "Decoy", Voyage.E8_Voyage);
			NewJobVesselRouting("USLAX", Voyage.E8_LloydsNumber, "Decoy");
			Voyage.E8_DataProvider = FreightConstants.VesselDataProviders.OneStop;

			Voyage.LoadOriginalForeignPortList();
			AssertEquals("MYPKG", Voyage.ForeignPorts[0]);
			AssertEquals("SGSIN", Voyage.ForeignPorts[1]);
			AssertEquals("Should find 2 foreign ports", 2, Voyage.ForeignPorts.Count);
		}

		#endregion

		#region Overrides

		protected override ZString DataProvider { get { return FreightConstants.VesselDataProviders.OneStop; } }

		protected override OrgHeader LineOperator
		{
			get
			{
				if (lineOperator == null)
				{
					lineOperator = Factory.NewWithValidTestData<OrgHeader>();

					var oneStopCode = lineOperator.CustomsCodes.AddNew();
					oneStopCode.OK_CodeType = OrgCusCode.CodeTypes.OneStopCode;
					oneStopCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Australia;
					oneStopCode.OK_CustomsRegNo = "LO1";

					var decoyOneStopCode = lineOperator.CustomsCodes.AddNew();
					decoyOneStopCode.OK_CodeType = OrgCusCode.CodeTypes.OneStopCode;
					decoyOneStopCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Malaysia;
					decoyOneStopCode.OK_CustomsRegNo = "LO2";
				}

				Factory.Save();

				return lineOperator;
			}
		}

		#endregion
	}
}
