using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.SG.V4.Business;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.SG.Testing
{
	internal class SGCPCAddInfoValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckSG_CPCCodes()
		{
			Declaration.JE_MessageType = MessageTypeCodeList.Codes.INP;
			Declaration.JE_MessageSubType = DeclarationTypeCodeList.Codes.APS;
			var cpcs = Declaration.CPCCollection;
			AssertEquals("Pre-condition: CPC Collection should have procedure codes filtered by MessageType & MessageSubType", 3, cpcs.Count);
			var cPCCodeList = CPC.AddInfoValidation.SGCPC.AddInfoLookups.CPCCodeList;
			AssertEquals("Pre-condition: CPC List should have procedure codes for this declaration MessageType & MessageSubType", 3, cpcs.Count);
			AssertEquals("", CPC.SG_CPCCode);
			CPC.AddInfoValidation.ValidateSG_CPCCode();
			AssertNoMessageErrors(CPC.SG_CPCCodeInfo);
			CPC.SG_CPCCode = "11177";
			AssertHasMessageErrorContaining(CPC.SG_CPCCodeInfo, ListValidation.InvalidCodeMessageError);
			CPC.SG_CPCCode = "2201000"; // => AEO (INP APS)
			AssertNoMessageErrorContaining(CPC.SG_CPCCodeInfo, ListValidation.InvalidCodeMessageError);
			CPC.SG_CPCCode = "2206000"; // => CNB (INP APS)
			AssertNoMessageErrorContaining(CPC.SG_CPCCodeInfo, ListValidation.InvalidCodeMessageError);
			CPC.SG_CPCCode = "2251000"; // => AEO (INP SFZ)
			CPC.SG_CPCCode = "2251000"; // need to set twice to overcome has changes setting the code back to a valid value
			AssertHasMessageErrorContaining(CPC.SG_CPCCodeInfo, ListValidation.InvalidCodeMessageError);
			Declaration.JE_MessageSubType = DeclarationTypeCodeList.Codes.SFZ;
			CPC.SG_CPCCode = "2251000"; // => AEO (INP SFZ)
			AssertNoMessageErrorContaining(CPC.SG_CPCCodeInfo, ListValidation.InvalidCodeMessageError);
			Declaration.JE_MessageType = MessageTypeCodeList.Codes.IPT;
			Declaration.JE_MessageSubType = DeclarationTypeCodeList.Codes.BKT;
			CPC.SG_CPCCode = "SOME CPC";
			CPC.SG_CPCCode = "SOME CPC"; // need to set twice to overcome has changes setting the code back to a valid value
			AssertHasMessageErrorContaining(CPC.SG_CPCCodeInfo, ListValidation.InvalidCodeMessageError);
			CPC.SG_CPCCode = "1901000"; // => AEO (IPT BKT)
			AssertNoMessageErrorContaining(CPC.SG_CPCCodeInfo, ListValidation.InvalidCodeMessageError);
			CPC.SG_CPCCode = "1906000"; // => CNB (IPT BKT)
			AssertNoMessageErrorContaining(CPC.SG_CPCCodeInfo, ListValidation.InvalidCodeMessageError);
			CPC.SG_CPCCode = "2251000"; // => AEO (INP SFZ)
			CPC.SG_CPCCode = "2251000"; // need to set twice to overcome has changes setting the code back to a valid value
			AssertHasMessageErrorContaining(CPC.SG_CPCCodeInfo, ListValidation.InvalidCodeMessageError);
			CPC.SG_CPCCode = "1902000"; // => CWC (IPT BKT)
			AssertNoMessageErrorContaining(CPC.SG_CPCCodeInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCPCCodesAreValidForDeclaration()
		{
			AssertEquals("", CPC.SG_CPCCode);
			CPC.AddInfoValidation.ValidateSG_CPCCode();
			AssertNoMessageErrors(CPC.SG_CPCCodeInfo);
			Declaration.JE_MessageType = MessageTypeCodeList.Codes.IPT;
			Declaration.JE_MessageSubType = DeclarationTypeCodeList.Codes.BKT;
			CPC.SG_CPCCode = "1101000";
			CPC.SG_CPCCode = "1101000"; // need to set twice to overcome has changes setting the code back to a valid value
			AssertHasMessageError(CPC.SG_CPCCodeInfo, ListValidation.InvalidCodeMessageError);
			Declaration.JE_MessageSubType = DeclarationTypeCodeList.Codes.GST;
			CPC.SG_CPCCode = "1106000";
			AssertNoMessageError("Code is valid for this declaration type", CPC.SG_CPCCodeInfo, ListValidation.InvalidCodeMessageError);
			Declaration.JE_MessageType = MessageTypeCodeList.Codes.INP;
			Declaration.JE_MessageSubType = DeclarationTypeCodeList.Codes.BRE;
			CPC.SG_CPCCode = "1101000";
			CPC.SG_CPCCode = "1101000"; // need to set twice to overcome has changes setting the code back to a valid value
			AssertHasMessageError(CPC.SG_CPCCodeInfo, ListValidation.InvalidCodeMessageError);
			Declaration.JE_MessageSubType = DeclarationTypeCodeList.Codes.BKT;
			CPC.SG_CPCCode = "2901000";
			AssertNoMessageError("Code is valid for this declaration type", CPC.SG_CPCCodeInfo, ListValidation.InvalidCodeMessageError);
			Declaration.JE_MessageType = MessageTypeCodeList.Codes.OUT;
			Declaration.JE_MessageSubType = DeclarationTypeCodeList.Codes.DRT;
			CPC.SG_CPCCode = "SEASTORE";
			CPC.SG_CPCCode = "SEASTORE";
			AssertHasMessageError(CPC.SG_CPCCodeInfo, ListValidation.InvalidCodeMessageError);
			Declaration.JE_MessageSubType = DeclarationTypeCodeList.Codes.APS;
			CPC.SG_CPCCode = "4203000";
			AssertNoMessageError("Code is valid for this declaration type", CPC.SG_CPCCodeInfo, ListValidation.InvalidCodeMessageError);
			Declaration.JE_MessageType = MessageTypeCodeList.Codes.INP;
			Declaration.JE_MessageSubType = DeclarationTypeCodeList.Codes.BRE;
			CPC.SG_CPCCode = "STS AND CWC";
			CPC.SG_CPCCode = "STS AND CWC";
			AssertHasMessageError(CPC.SG_CPCCodeInfo, ListValidation.InvalidCodeMessageError);
			Declaration.JE_MessageType = MessageTypeCodeList.Codes.OUT;
			Declaration.JE_MessageSubType = DeclarationTypeCodeList.Codes.DRT;
			Declaration.SG_ApplicationProductType = ApplicationProductTypeCodeList.Codes.NH;
			CPC.SG_CPCCode = "5404100";
			AssertNoMessageError("Code is valid for this declaration type", CPC.SG_CPCCodeInfo, ListValidation.InvalidCodeMessageError);
			Declaration.JE_MessageType = MessageTypeCodeList.Codes.INP;
			CPC.SG_CPCCode = "DEFERRED PRINTING FOR COO";
			CPC.SG_CPCCode = "DEFERRED PRINTING FOR COO";
			AssertHasMessageError(CPC.SG_CPCCodeInfo, ListValidation.InvalidCodeMessageError);
			Declaration.JE_MessageType = MessageTypeCodeList.Codes.OUT;
			CPC.SG_CPCCode = "5404100";
			AssertNoMessageError("Code is valid for this declaration type", CPC.SG_CPCCodeInfo, ListValidation.InvalidCodeMessageError);
			CPC.SG_CPCCode = "5405000";
			AssertNoMessageError("Code is valid for this declaration type", CPC.SG_CPCCodeInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckSG_PC1()
		{
			CPC.SG_CPCCode = "4401000";
			ZString messageErrorExpected = "Crew number - must be entered";
			CPC.AddInfoValidation.ValidateSG_PC1();
			AssertNoMessageErrorContaining(CPC.SG_PC1Info, messageErrorExpected);
			CPC.SG_APCCodeDescription = "SEASTORE";
			CPC.SG_CPCCode = "4203000";
			CPC.SG_PC1 = "";
			AssertHasMessageErrorContaining(CPC.SG_PC1Info, messageErrorExpected);
			CPC.SG_PC1 = "FIFTEEN";
			AssertNoMessageErrorContaining(CPC.SG_PC1Info, messageErrorExpected);
			AssertHasMessageErrorContaining(CPC.SG_PC1Info, SGCPCAddInfoValidation.SeaStoreCrewAsNumber);
			CPC.SG_PC1 = "15";
			AssertNoMessageErrorContaining(CPC.SG_PC1Info, messageErrorExpected);
			AssertNoMessageErrorContaining(CPC.SG_PC1Info, SGCPCAddInfoValidation.SeaStoreCrewAsNumber);
		}

		public void TestCheckSG_PC2()
		{
			CPC.SG_CPCCode = "4401000";
			ZString messageErrorExpected = "Voyage number - must be entered";
			CPC.AddInfoValidation.ValidateSG_PC2();
			AssertNoMessageErrorContaining(CPC.SG_PC2Info, messageErrorExpected);
			CPC.SG_APCCodeDescription = "SEASTORE";
			CPC.SG_CPCCode = "4203000";
			CPC.SG_PC2 = "";
			AssertHasMessageErrorContaining(CPC.SG_PC2Info, messageErrorExpected);
			CPC.SG_PC2 = "EIGHT";
			AssertNoMessageErrorContaining(CPC.SG_PC2Info, messageErrorExpected);
			AssertHasMessageErrorContaining(CPC.SG_PC2Info, SGCPCAddInfoValidation.SeaStoreVoyageDurationAsNumber);
			CPC.SG_PC2 = "8";
			AssertNoMessageErrorContaining(CPC.SG_PC2Info, messageErrorExpected);
			AssertNoMessageErrorContaining(CPC.SG_PC2Info, SGCPCAddInfoValidation.SeaStoreVoyageDurationAsNumber);
		}

		public void TestSeastoreEnteredOnIPT()
		{
			Declaration.JE_MessageType = MessageTypeCodeList.Codes.IPT;
			CPC.SG_APCCodeDescription = "SEASTORE";
			CPC.SG_CPCCode = "4203000";
			CPC.AddInfoValidation.ValidateSG_CPCCode();
			AssertHasMessageErrorContaining(CPC.SG_CPCCodeInfo, "Sea Store details are only valid on INP, TNP and OUT Declarations.");
			Declaration.JE_MessageType = MessageTypeCodeList.Codes.OUT;
			CPC.AddInfoValidation.ValidateSG_CPCCode();
			AssertNoMessageErrorContaining(CPC.SG_CPCCodeInfo, "Sea Store details are only valid on INP, TNP and OUT Declarations.");
		}

		#region Implementation
		protected override void SetUp()
		{
			base.SetUp();
			var currentCountry = GlbCompany.CurrentCompany.Country.Code;
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var procedure1 = helper.CreateRefCusProcedure(currentCountry, "", "220", "", "1000", "AEO (INP APS)", "INP", group: "APS");
			var procedure1Attribute1 = helper.CreateRefCusProcedureAttribute(procedure1.PK, AttributeNames.Codes.ISAEO, "Y");
			var procedure2 = helper.CreateRefCusProcedure(currentCountry, "", "220", "", "2000", "CWC (INP APS)", "INP", group: "APS");
			var procedure2Attribute1 = helper.CreateRefCusProcedureAttribute(procedure2.PK, AttributeNames.Codes.IsCWC, "Y");
			var procedure3 = helper.CreateRefCusProcedure(currentCountry, "", "220", "", "6000", "CNB (INP APS)", "INP", group: "APS");
			var procedure3Attribute1 = helper.CreateRefCusProcedureAttribute(procedure3.PK, AttributeNames.Codes.IsCNB, "Y");
			var procedure4 = helper.CreateRefCusProcedure(currentCountry, "", "225", "", "1000", "AEO (INP SFZ)", "INP", group: "SFZ");
			var procedure4Attribute1 = helper.CreateRefCusProcedureAttribute(procedure4.PK, AttributeNames.Codes.ISAEO, "Y");
			var procedure5 = helper.CreateRefCusProcedure(currentCountry, "", "190", "", "1000", "AEO (IPT BKT)", "IPT", group: "BKT");
			var procedure5Attribute1 = helper.CreateRefCusProcedureAttribute(procedure5.PK, AttributeNames.Codes.ISAEO, "Y");
			var procedure6 = helper.CreateRefCusProcedure(currentCountry, "", "190", "", "2000", "CWC (IPT BKT)", "IPT", group: "BKT");
			var procedure6Attribute1 = helper.CreateRefCusProcedureAttribute(procedure6.PK, AttributeNames.Codes.IsCWC, "Y");
			var procedure7 = helper.CreateRefCusProcedure(currentCountry, "", "190", "", "6000", "CNB (IPT BKT)", "IPT", group: "BKT");
			var procedure7Attribute1 = helper.CreateRefCusProcedureAttribute(procedure7.PK, AttributeNames.Codes.IsCNB, "Y");
			var procedure8 = helper.CreateRefCusProcedure(currentCountry, "", "110", "", "6000", "CNB (IPT GST)", "IPT", group: "GST");
			var procedure8Attribute1 = helper.CreateRefCusProcedureAttribute(procedure8.PK, AttributeNames.Codes.IsCNB, "Y");
			var procedure9 = helper.CreateRefCusProcedure(currentCountry, "", "290", "", "1000", "AEO (INP BKT)", "INP", group: "BKT");
			var procedure9Attribute1 = helper.CreateRefCusProcedureAttribute(procedure9.PK, AttributeNames.Codes.ISAEO, "Y");
			var procedure10 = helper.CreateRefCusProcedure(currentCountry, "", "420", "", "3000", "SEASTORE (OUT APS)", "OUT", group: "APS");
			var procedure10Attribute1 = helper.CreateRefCusProcedureAttribute(procedure10.PK, AttributeNames.Codes.ISSEASTORE, "Y");
			procedure10.Attributes.AddNew(AttributeNames.Codes.PC1, "Crew number - must be entered");
			procedure10.Attributes.AddNew(AttributeNames.Codes.PC2, "Voyage number - must be entered");
			var procedure11 = helper.CreateRefCusProcedure(currentCountry, "", "540", "", "4100", "STS and CWC (OUT DRT with COO)", "OUT", group: "DRT");
			var procedure11Attribute1 = helper.CreateRefCusProcedureAttribute(procedure11.PK, AttributeNames.Codes.IsSTSAndCWC, "Y");
			var procedure11Attribute2 = helper.CreateRefCusProcedureAttribute(procedure11.PK, AttributeNames.Codes.ISCOO, "Y");
			var procedure12 = helper.CreateRefCusProcedure(currentCountry, "", "540", "", "5000", "Deferred Printing for CO (OUT DRT)", "OUT", group: "DRT");
			var procedure12Attribute1 = helper.CreateRefCusProcedureAttribute(procedure12.PK, AttributeNames.Codes.IsDeferredPrintingForCoO, "Y");
			var procedure12Attribute2 = helper.CreateRefCusProcedureAttribute(procedure12.PK, AttributeNames.Codes.ISCOO, "Y");
			Factory.Save();
		}

		JobDeclaration Declaration
		{
			get
			{
				if (declaration == null)
				{
					declaration = Factory.New<JobDeclaration>();
					declaration.JE_MessageType = Enterprise.Customs.Business.JobMessageTypeList.Codes.Import;
					declaration.JE_ApplicationCode = SGConstants.TradeNetVersion.FourPointOne;
					declaration.CPCs.AddNew();
				}

				return declaration;
			}
		}

		JobDeclaration declaration;
		SGCPC CPC
		{
			get
			{
				return cpc ?? (cpc = Declaration.CPCs[0]);
			}
		}

		SGCPC cpc;
		#endregion
	}
}
