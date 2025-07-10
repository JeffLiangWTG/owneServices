using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;

namespace Enterprise.Freight.Agency.Business.Testing
{
	internal class AgencyUNDGDataItemValidationTest : TestCaseWithFactory
	{
		public void TestCheckSubstancePK()
		{
			var imoSub = Factory.New<UNDGSubstance>();
			imoSub.DG_UNNO = "IMO";
			imoSub.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;
			var iataSub = Factory.New<UNDGSubstance>();
			iataSub.DG_UNNO = "IATA";
			iataSub.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IATA;
			var shipment = Factory.New<AgencyShipment>();
			var packLine = shipment.OuterPackLines.AddNew();
			var collection = new AgencyUNDGDataItemCollection(packLine);
			var agencyUNDGDataItem = collection.AddNew();
			AssertNoWarning(agencyUNDGDataItem.SubstancePKInfo, "This substance is for Air Freight. Please re-select the Sea Freight substance.");
			agencyUNDGDataItem.SubstancePK = iataSub.PK;
			AssertHasWarning(agencyUNDGDataItem.SubstancePKInfo, "This substance is for Air Freight. Please re-select the Sea Freight substance.");
			agencyUNDGDataItem.SubstancePK = imoSub.PK;
			AssertNoWarning(agencyUNDGDataItem.SubstancePKInfo, "This substance is for Air Freight. Please re-select the Sea Freight substance.");
		}

		public void TestCheckDI_DG_WhenSetSubstancePK()
		{
			DangerousGoodsManifestMessageValidationStrategy.RegisterForFactory(Factory);
			var imoSub = Factory.New<UNDGSubstance>();
			imoSub.DG_UNNO = "IMO";
			imoSub.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;

			var shipment = Factory.New<AgencyShipment>();
			var packLine = shipment.OuterPackLines.AddNew();
			var collection = new AgencyUNDGDataItemCollection(packLine);

			var agencyUNDGDataItem = collection.AddNew();
			agencyUNDGDataItem.Validation.ValidateDI_DG();
			agencyUNDGDataItem.Validation.ValidateSubstancePK();
			AssertEquals("Precondition", ZGuid.Empty, agencyUNDGDataItem.DI_DG);
			AssertEquals("Precondition", ZGuid.Empty, agencyUNDGDataItem.SubstancePK);
			AssertNoErrors(agencyUNDGDataItem.DI_DGInfo);
			AssertHasError(agencyUNDGDataItem.SubstancePKInfo, "Please enter a DG Substance.");

			agencyUNDGDataItem.SubstancePK = imoSub.PK;
			AssertEquals("Precondition", imoSub.PK, agencyUNDGDataItem.DI_DG);
			agencyUNDGDataItem.Validation.ValidateDI_DG();
			agencyUNDGDataItem.Validation.ValidateSubstancePK();
			AssertNoErrors(agencyUNDGDataItem.DI_DGInfo);
			AssertNoErrors(agencyUNDGDataItem.SubstancePKInfo);
		}

		public void TestShouldNotValidateDI_DGWeightInfoWhenQuantityIsLimited()
		{
			var substance = Factory.New<UNDGSubstance>();
			substance.DG_Code = "1234a";
			substance.DG_LQMaxAmt = 250;
			substance.DG_LQMaxAmtUQ = "G";
			substance.DG_LQMaxAmtType = UNDGSubstanceLookups.LimitedQuantityTypes.NLMCode;
			var item = Factory.New<AgencyUNDGDataItem>();
			item.LinkDefault(substance);
			item.DI_IsLimitedQuantity = false;
			AssertNoWarnings(item.DI_DGWeightInfo);
			item.DI_IsLimitedQuantity = true;
			AssertNoWarnings(item.DI_DGWeightInfo);
		}

		public void TestShouldNotValidateDI_DGVolumeInfoWhenQuantityIsLimited()
		{
			var substance = Factory.New<UNDGSubstance>();
			substance.DG_Code = "1234a";
			substance.DG_LQMaxAmt = 250;
			substance.DG_LQMaxAmtUQ = "L";
			substance.DG_LQMaxAmtType = UNDGSubstanceLookups.LimitedQuantityTypes.NLMCode;
			var item = Factory.New<AgencyUNDGDataItem>();
			var subs = Factory.New<UNDGSubstance>();
			subs.DG_Code = substance.DG_Code;
			subs.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;
			item.LinkDefault(subs);
			item.DI_IsLimitedQuantity = false;
			AssertNoWarnings(item.DI_DGVolumeInfo);
			item.DI_IsLimitedQuantity = true;
			AssertNoWarnings(item.DI_DGVolumeInfo);
		}

		public void TestDGTechnicalNameValidationIfRequiredForThisSubstance()
		{
			var dangerousGood = Factory.New<AgencyUNDGDataItem>();
			dangerousGood.UNDGSubstancePivotCollection.UpdateDefaultPivot(UNDGSubstanceLoader.LoadSubstances(Factory, "1001", "", "IMO").First());
			dangerousGood.Substance.DG_TechName = "";
			AssertNoWarning(dangerousGood.DI_TechnicalNameInfo, "Technical Name is required for this substance.");
			dangerousGood.Substance.DG_TechName = "*";
			dangerousGood.Validation.ValidateDI_TechnicalName();
			AssertHasWarning(dangerousGood.DI_TechnicalNameInfo, "Technical Name is required for this substance.");
			dangerousGood.DI_TechnicalName = "Killer Vanila";
			AssertNoError(dangerousGood.DI_TechnicalNameInfo, "Technical Name is required for this substance.");
		}

		public void TestDGTechnicalNameValidationIfSP274OrSP318()
		{
			var dangerousGood = Factory.New<AgencyUNDGDataItem>();
			dangerousGood.UNDGSubstancePivotCollection.UpdateDefaultPivot(UNDGSubstanceLoader.LoadSubstances(Factory, "1001", "", "IMO").First());
			dangerousGood.Substance.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;
			dangerousGood.Validation.ValidateDI_TechnicalName();
			AssertNoWarning(dangerousGood.DI_TechnicalNameInfo, "Generic and 'not otherwise specified' proper shipping names that are assigned to special provisions 274 and 318 in Column 6 of the Dangerous Goods List must be supplemented with the technical or chemical group name unless a national law or international convention prohibits its disclosure if it is a controlled substance. For example: 'UN1993 Flammable liquid, n.o.s. (contains xylene and benzene), 3, PG II'. The technical name must be a recognized chemical or biological name, used in scientific and technical handbooks. Trade names must not be used for this purpose.");
			var provision = Factory.New<UNDGCommonData>();
			provision.DC_Type = UNDGCommonDataLookups.TypeConstants.SpecialProvisions;
			provision.DC_Index = "274";
			dangerousGood.Substance.SpecialProvisions.Add(provision);
			dangerousGood.Validation.ValidateDI_TechnicalName();
			AssertHasWarning(dangerousGood.DI_TechnicalNameInfo, "Generic and 'not otherwise specified' proper shipping names that are assigned to special provisions 274 and 318 in Column 6 of the Dangerous Goods List must be supplemented with the technical or chemical group name unless a national law or international convention prohibits its disclosure if it is a controlled substance. For example: 'UN1993 Flammable liquid, n.o.s. (contains xylene and benzene), 3, PG II'. The technical name must be a recognized chemical or biological name, used in scientific and technical handbooks. Trade names must not be used for this purpose.");
			provision.DC_Index = "318";
			dangerousGood.Validation.ValidateDI_TechnicalName();
			AssertHasWarning(dangerousGood.DI_TechnicalNameInfo, "Generic and 'not otherwise specified' proper shipping names that are assigned to special provisions 274 and 318 in Column 6 of the Dangerous Goods List must be supplemented with the technical or chemical group name unless a national law or international convention prohibits its disclosure if it is a controlled substance. For example: 'UN1993 Flammable liquid, n.o.s. (contains xylene and benzene), 3, PG II'. The technical name must be a recognized chemical or biological name, used in scientific and technical handbooks. Trade names must not be used for this purpose.");
			dangerousGood.DI_TechnicalName = "Killer Vanila";
			dangerousGood.Validation.ValidateDI_TechnicalName();
			AssertNoWarning(dangerousGood.DI_TechnicalNameInfo, "Generic and 'not otherwise specified' proper shipping names that are assigned to special provisions 274 and 318 in Column 6 of the Dangerous Goods List must be supplemented with the technical or chemical group name unless a national law or international convention prohibits its disclosure if it is a controlled substance. For example: 'UN1993 Flammable liquid, n.o.s. (contains xylene and benzene), 3, PG II'. The technical name must be a recognized chemical or biological name, used in scientific and technical handbooks. Trade names must not be used for this purpose.");
		}

		public void TestDGTechnicalNameValidationIfMarinePollutant()
		{
			var dangerousGood = Factory.New<AgencyUNDGDataItem>();
			dangerousGood.UNDGSubstancePivotCollection.UpdateDefaultPivot(UNDGSubstanceLoader.LoadSubstances(Factory, "1001", "", "IMO").First());
			dangerousGood.Substance.DG_TechName = "";
			dangerousGood.Substance.DG_MP = "Y";
			dangerousGood.Validation.ValidateDI_TechnicalName();
			AssertNoWarning(dangerousGood.DI_TechnicalNameInfo, "Technical Name is required for substances that are marine pollutants, per the IMO IMDG Code. Enter the recognized chemical name of the constituent which most predominantly contributes to the classification as marine pollutant.");
			dangerousGood.Substance.DG_TechName = "*";
			dangerousGood.Substance.DG_MP = "N";
			dangerousGood.Validation.ValidateDI_TechnicalName();
			AssertNoWarning(dangerousGood.DI_TechnicalNameInfo, "Technical Name is required for substances that are marine pollutants, per the IMO IMDG Code. Enter the recognized chemical name of the constituent which most predominantly contributes to the classification as marine pollutant.");
			dangerousGood.Substance.DG_MP = "Y";
			dangerousGood.Validation.ValidateDI_TechnicalName();
			AssertHasWarning(dangerousGood.DI_TechnicalNameInfo, "Technical Name is required for substances that are marine pollutants, per the IMO IMDG Code. Enter the recognized chemical name of the constituent which most predominantly contributes to the classification as marine pollutant.");
			dangerousGood.DI_TechnicalName = "Killer Vanila";
			AssertNoWarning(dangerousGood.DI_TechnicalNameInfo, "Technical Name is required for substances that are marine pollutants, per the IMO IMDG Code. Enter the recognized chemical name of the constituent which most predominantly contributes to the classification as marine pollutant.");
		}
	}
}
