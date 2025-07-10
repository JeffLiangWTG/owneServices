using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.Customs.SG.V4.Business;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.SG.Testing
{
	[TestedType(typeof(SGCPC))]
	public class SGCPCTest : Customs.Business.MultiLineAddInfos.Testing.CusAddInfoTest<SGCPC>
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			return declaration.CPCs.AddNew();
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var declaration = factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var cpc = declaration.CPCs.AddNew();
			cpc.SG_CPCCode = "2941000";
			return cpc;
		}

		public void TestDefault()
		{
			var declaration = Factory.New<JobDeclaration>();
			var cpc = declaration.CPCs.AddNew();
			AssertEquals(JobDeclarationSchema.Constants.Prefix, cpc.B7_ParentTableCode);
			AssertEquals(CusAddInfoTypeAttribute.Codes.SGCustomsProcedureCode, cpc.B7_Type);
		}

		public void TestDeleteWhenEmpty()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var cpc = declaration.CPCs.AddNew();
			cpc.SG_CPCCode = "2941000";
			Factory.Save();
			AssertEquals(false, cpc.IsDeleted);
			cpc.SG_CPCCode = ZString.Empty;
			cpc.SG_APCCodeDescription = ZString.Empty;
			Factory.Save();
			AssertEquals(true, cpc.IsDeleted);
		}

		public void TestChangingParentTypeIsNotSupported()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var cpc = Factory.New<SGCPC>();
			AssertNoExceptionThrown(delegate
			{
				cpc.B7_ParentTableCode = declaration.TablePrefix;
			}

			);
			AssertExceptionThrown(typeof(NotSupportedException), "Setting SGCPC.B7_ParentTableCode is not supported.", delegate
			{
				cpc.B7_ParentTableCode = invoice.TablePrefix;
			}

			);
			AssertNoExceptionThrown(delegate
			{
				cpc.B7_ParentID = declaration.PK;
			}

			);
			AssertExceptionThrown(typeof(NotSupportedException), "Setting SGCPC.B7_ParentID is not supported.", delegate
			{
				cpc.B7_ParentID = invoice.PK;
			}

			);
		}

		public void TestProcessingCodeOccurrences()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var cpc = declaration.CPCs.AddNew();
			cpc.SG_CPCCode = "2902000";
			cpc.SG_PC1 = "PCP-0394839";
			cpc.SG_PC2 = "38.9%";
			cpc.SG_PC3 = "DG-459983845";
			cpc.SG_PC4 = "PC4";
			Factory.Save();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var cusEntry = (ISGCUSDEC)entryHeader;
			IEnumerable<ICusCPC> cpcs = cusEntry.CPCs;
			int pcOccurrenceCount = 0;
			foreach (ICusCPC cusCPC in cpcs)
			{
				foreach (ICusProcessingCodes occurrence in cusCPC.PCOccurrences)
				{
					pcOccurrenceCount++;
					if (pcOccurrenceCount == 1)
					{
						AssertEquals("Occurrence 1 - PC1", "PCP-0394839", occurrence.ProcessingCode1);
						AssertEquals("Occurrence 1 - PC2", "38.9%", occurrence.ProcessingCode2);
						AssertEquals("Occurrence 1 - PC3", "DG-459983845", occurrence.ProcessingCode3);
					}
					else if (pcOccurrenceCount == 2)
					{
						AssertEquals("Occurrence 2 - PC1", "PC4", occurrence.ProcessingCode1);
						AssertEquals("Occurrence 2 - PC2", "", occurrence.ProcessingCode2);
						AssertEquals("Occurrence 2 - PC3", "", occurrence.ProcessingCode3);
					}
				}
			}

			AssertEquals("Should be 2 Processing Code occurrences - no null value occurrences should be returned", 2, pcOccurrenceCount);
			cpc.SG_PC5 = "PC5";
			cpc.SG_PC6 = "PC6";
			cpc.SG_PC7 = "PC7";
			cpc.SG_PC8 = "PC8";
			cpc.SG_PC9 = "PC9";
			cpc.SG_PC10 = "PC10";
			cpc.SG_PC11 = "PC11";
			cpc.SG_PC12 = "PC12";
			cpc.SG_PC13 = "PC13";
			cpc.SG_PC14 = "PC14";
			Factory.Save();
			pcOccurrenceCount = 0;
			foreach (ICusCPC cusCPC in cpcs)
			{
				foreach (ICusProcessingCodes occurrence in cusCPC.PCOccurrences)
				{
					pcOccurrenceCount++;
					if (pcOccurrenceCount == 1)
					{
						AssertEquals("Occurrence 1 - PC1", "PCP-0394839", occurrence.ProcessingCode1);
						AssertEquals("Occurrence 1 - PC2", "38.9%", occurrence.ProcessingCode2);
						AssertEquals("Occurrence 1 - PC3", "DG-459983845", occurrence.ProcessingCode3);
					}
					else if (pcOccurrenceCount == 2)
					{
						AssertEquals("Occurrence 2 - PC1", "PC4", occurrence.ProcessingCode1);
						AssertEquals("Occurrence 2 - PC2", "PC5", occurrence.ProcessingCode2);
						AssertEquals("Occurrence 2 - PC3", "PC6", occurrence.ProcessingCode3);
					}
					else if (pcOccurrenceCount == 3)
					{
						AssertEquals("Occurrence 3 - PC1", "PC7", occurrence.ProcessingCode1);
						AssertEquals("Occurrence 3 - PC2", "PC8", occurrence.ProcessingCode2);
						AssertEquals("Occurrence 3 - PC3", "PC9", occurrence.ProcessingCode3);
					}
					else if (pcOccurrenceCount == 4)
					{
						AssertEquals("Occurrence 4 - PC1", "PC10", occurrence.ProcessingCode1);
						AssertEquals("Occurrence 4 - PC2", "PC11", occurrence.ProcessingCode2);
						AssertEquals("Occurrence 4 - PC3", "PC12", occurrence.ProcessingCode3);
					}
					else if (pcOccurrenceCount == 5)
					{
						AssertEquals("Occurrence 5 - PC1", "PC13", occurrence.ProcessingCode1);
						AssertEquals("Occurrence 5 - PC2", "PC14", occurrence.ProcessingCode2);
						AssertEquals("Occurrence 5 - PC3", "", occurrence.ProcessingCode3);
					}
				}
			}

			AssertEquals("Should be all 5 Processing Code occurrences returned", 5, pcOccurrenceCount);
			cpc.SG_PC9 = cpc.SG_PC10 = cpc.SG_PC11 = cpc.SG_PC12 = cpc.SG_PC13 = cpc.SG_PC14 = ZString.Empty;
			Factory.Save();
			pcOccurrenceCount = 0;
			foreach (ICusCPC cusCPC in cpcs)
			{
				foreach (ICusProcessingCodes occurrence in cusCPC.PCOccurrences)
				{
					pcOccurrenceCount++;
					if (pcOccurrenceCount == 1)
					{
						AssertEquals("Occurrence 1 - PC1", "PCP-0394839", occurrence.ProcessingCode1);
						AssertEquals("Occurrence 1 - PC2", "38.9%", occurrence.ProcessingCode2);
						AssertEquals("Occurrence 1 - PC3", "DG-459983845", occurrence.ProcessingCode3);
					}
					else if (pcOccurrenceCount == 2)
					{
						AssertEquals("Occurrence 2 - PC1", "PC4", occurrence.ProcessingCode1);
						AssertEquals("Occurrence 2 - PC2", "PC5", occurrence.ProcessingCode2);
						AssertEquals("Occurrence 2 - PC3", "PC6", occurrence.ProcessingCode3);
					}
					else if (pcOccurrenceCount == 3)
					{
						AssertEquals("Occurrence 3 - PC1", "PC7", occurrence.ProcessingCode1);
						AssertEquals("Occurrence 3 - PC2", "PC8", occurrence.ProcessingCode2);
						AssertEquals("Occurrence 3 - PC3", "", occurrence.ProcessingCode3);
					}
				}
			}

			AssertEquals("Should now again be only 3 Processing Code occurrences - no null value occurrences should be returned", 3, pcOccurrenceCount);
		}

		public void TestCPCCodeDetermination()
		{
			var currentCountry = GlbCompany.CurrentCompany.Country.Code;
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var procedure1 = helper.CreateRefCusProcedure(currentCountry, "", "772", "11", "3000", "SEASTORE (TNP IGM)", "TNP", group: "IGM");
			var procedure1Attribute1 = helper.CreateRefCusProcedureAttribute(procedure1.PK, AttributeNames.Codes.ISSEASTORE, "Y");
			var procedure2 = helper.CreateRefCusProcedure(currentCountry, "", "220", "22", "1000", "AEO (INP APS)", "INP", group: "APS");
			var procedure2Attribute1 = helper.CreateRefCusProcedureAttribute(procedure2.PK, AttributeNames.Codes.ISAEO, "Y");
			var procedure3 = helper.CreateRefCusProcedure(currentCountry, "", "294", "33", "2000", "CWC (INP TCO)", "INP", group: "TCO");
			var procedure3Attribute1 = helper.CreateRefCusProcedureAttribute(procedure3.PK, AttributeNames.Codes.IsCWC, "Y");
			var procedure4 = helper.CreateRefCusProcedure(currentCountry, "", "110", "44", "1000", "AEO (IPT GST)", "IPT", group: "GST");
			var procedure4Attribute1 = helper.CreateRefCusProcedureAttribute(procedure4.PK, AttributeNames.Codes.ISAEO, "Y");
			var procedure5 = helper.CreateRefCusProcedure(currentCountry, "", "440", "44", "4000", "STS (OUT DRT)", "OUT", group: "DRT");
			var procedure5Attribute1 = helper.CreateRefCusProcedureAttribute(procedure5.PK, AttributeNames.Codes.IsSTS, "Y");
			var procedure6 = helper.CreateRefCusProcedure(currentCountry, "", "225", "55", "1000", "AEO (INP SFZ)", "INP", group: "SFZ");
			var procedure6Attribute1 = helper.CreateRefCusProcedureAttribute(procedure6.PK, AttributeNames.Codes.ISAEO, "Y");
			var procedure7 = helper.CreateRefCusProcedure(currentCountry, "", "790", "77", "1000", "AEO (TNP BRE)", "TNP", group: "BRE");
			var procedure7Attribute1 = helper.CreateRefCusProcedureAttribute(procedure7.PK, AttributeNames.Codes.ISAEO, "Y");
			var procedure8 = helper.CreateRefCusProcedure(currentCountry, "", "540", "88", "5000", "DEFERRED PRINTING FOR COO (OUT DRT)", "OUT", group: "DRT");
			var procedure8Attribute1 = helper.CreateRefCusProcedureAttribute(procedure8.PK, AttributeNames.Codes.IsDeferredPrintingForCoO, "Y");
			var procedure8Attribute2 = helper.CreateRefCusProcedureAttribute(procedure8.PK, AttributeNames.Codes.ISCOO, "Y");
			var procedure9 = helper.CreateRefCusProcedure(currentCountry, "", "440", "44", "4100", "STS and CWC (OUT DRT)", "OUT", group: "DRT");
			var procedure9Attribute1 = helper.CreateRefCusProcedureAttribute(procedure9.PK, AttributeNames.Codes.IsSTSAndCWC, "Y");
			Factory.Save();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeCodeList.Codes.TNP;
			declaration.JE_MessageSubType = DeclarationTypeCodeList.Codes.IGM;
			var cpc = declaration.CPCs.AddNew();
			cpc.B7_AddInfoData = "CPCCode=7723000*PC1=1234*PC2=12345";
			Factory.Save();
			AssertEquals("SG_APCCodeDescription should be determined from CPCCode value from B7_AddInfoData in DB", "SEASTORE", cpc.SG_APCCodeDescription);
			cpc.SG_APCCodeDescription = ZString.Empty;
			cpc.SG_APCCodeDescription = "SEASTORE";
			AssertEquals("SEASTORE SG_CPCCode is determined from SG_APCCodeDescription", "7723000", cpc.SG_CPCCode);
			declaration.JE_MessageType = MessageTypeCodeList.Codes.INP;
			declaration.JE_MessageSubType = DeclarationTypeCodeList.Codes.APS;
			cpc = declaration.CPCs.AddNew();
			cpc.B7_AddInfoData = "CPCCode=2201000*PC1=JP*PC2=AEOJP1234567";
			Factory.Save();
			AssertEquals("SG_APCCodeDescription should be value from B7_AddInfoData in DB", "AEO", cpc.SG_APCCodeDescription);
			cpc.SG_APCCodeDescription = ZString.Empty;
			cpc.SG_APCCodeDescription = "AEO";
			AssertEquals("AEO (INP/APS) SG_CPCCode is determined from SG_APCCodeDescription", "2201000", cpc.SG_CPCCode);
			declaration.JE_MessageType = MessageTypeCodeList.Codes.INP;
			declaration.JE_MessageSubType = DeclarationTypeCodeList.Codes.TCO;
			cpc = declaration.CPCs.AddNew();
			cpc.B7_AddInfoData = "CPCCode=2942000*PC1=TESTING 123";
			Factory.Save();
			AssertEquals("SG_APCCodeDescription should be value from B7_AddInfoData in DB", "CWC", cpc.SG_APCCodeDescription);
			cpc.SG_APCCodeDescription = ZString.Empty;
			cpc.SG_APCCodeDescription = "CWC";
			AssertEquals("CWC SG_CPCCode is determined from SG_APCCodeDescription", "2942000", cpc.SG_CPCCode);
			declaration.JE_MessageType = MessageTypeCodeList.Codes.IPT;
			declaration.JE_MessageSubType = DeclarationTypeCodeList.Codes.GST;
			cpc = declaration.CPCs.AddNew();
			cpc.B7_AddInfoData = "CPCCode=1101000*PC1=1122";
			Factory.Save();
			AssertEquals("SG_APCCodeDescription should be value from B7_AddInfoData in DB", "AEO", cpc.SG_APCCodeDescription);
			cpc.SG_APCCodeDescription = ZString.Empty;
			cpc.SG_APCCodeDescription = "AEO";
			AssertEquals("AEO (IPT/GST) SG_CPCCode is determined from SG_APCCodeDescription", "1101000", cpc.SG_CPCCode);
			declaration.JE_MessageType = MessageTypeCodeList.Codes.INP;
			declaration.JE_MessageSubType = DeclarationTypeCodeList.Codes.SFZ;
			cpc = declaration.CPCs.AddNew();
			cpc.B7_AddInfoData = "CPCCode=2251000*PC1=1122";
			Factory.Save();
			AssertEquals("SG_APCCodeDescription should be value from B7_AddInfoData in DB", "AEO", cpc.SG_APCCodeDescription);
			cpc.SG_APCCodeDescription = ZString.Empty;
			cpc.SG_APCCodeDescription = "AEO";
			AssertEquals("AEO (INP/SFZ) SG_CPCCode is determined from SG_APCCodeDescription", "2251000", cpc.SG_CPCCode);
			declaration.JE_MessageType = MessageTypeCodeList.Codes.OUT;
			declaration.JE_MessageSubType = DeclarationTypeCodeList.Codes.DRT;
			declaration.SG_ApplicationProductType = ApplicationProductTypeCodeList.Codes.NH;
			cpc = declaration.CPCs.AddNew();
			cpc.B7_AddInfoData = "CPCCode=5405000";
			Factory.Save();
			AssertEquals("SG_APCCodeDescription should be value from B7_AddInfoData in DB", "DEFERRED PRINTING FOR COO", cpc.SG_APCCodeDescription);
			cpc.SG_APCCodeDescription = ZString.Empty;
			cpc.SG_APCCodeDescription = "DEFERRED PRINTING FOR COO";
			AssertEquals("DEFERRED PRINTING FOR COO SG_CPCCode is determined from SG_APCCodeDescription", "5405000", cpc.SG_CPCCode);
			declaration.JE_MessageType = MessageTypeCodeList.Codes.TNP;
			declaration.JE_MessageSubType = DeclarationTypeCodeList.Codes.BRE;
			declaration.SG_ApplicationProductType = "";
			cpc = declaration.CPCs.AddNew();
			cpc.B7_AddInfoData = "CPCCode=7901000*PC1=CA*PC2=AEOCA1234581";
			Factory.Save();
			AssertEquals("SG_APCCodeDescription should be value from B7_AddInfoData in DB", "AEO", cpc.SG_APCCodeDescription);
			cpc.SG_APCCodeDescription = ZString.Empty;
			cpc.SG_APCCodeDescription = "AEO";
			AssertEquals("AEO (TNP/BRE) SG_CPCCode is determined from SG_APCCodeDescription", "7901000", cpc.SG_CPCCode);
			declaration.JE_MessageType = MessageTypeCodeList.Codes.OUT;
			declaration.JE_MessageSubType = DeclarationTypeCodeList.Codes.DRT;
			cpc = declaration.CPCs.AddNew();
			cpc.B7_AddInfoData = "CPCCode=4404000*PC1=STS1";
			Factory.Save();
			AssertEquals("SG_APCCodeDescription should be value from B7_AddInfoData in DB", "STS", cpc.SG_APCCodeDescription);
			cpc.SG_APCCodeDescription = ZString.Empty;
			cpc.SG_APCCodeDescription = "STS";
			AssertEquals("STS SG_CPCCode is determined from SG_APCCodeDescription", "4404000", cpc.SG_CPCCode);
			declaration.JE_MessageType = MessageTypeCodeList.Codes.OUT;
			declaration.JE_MessageSubType = DeclarationTypeCodeList.Codes.DRT;
			cpc = declaration.CPCs.AddNew();
			cpc.B7_AddInfoData = "CPCCode=4404100*PC1=STS1";
			Factory.Save();
			AssertEquals("SG_APCCodeDescription should be value from B7_AddInfoData in DB", "STS and CWC", cpc.SG_APCCodeDescription);
			cpc.SG_APCCodeDescription = ZString.Empty;
			cpc.SG_APCCodeDescription = "STS and CWC";
			AssertEquals("STS SG_CPCCode is determined from SG_APCCodeDescription", "4404100", cpc.SG_CPCCode);
		}
	}
}
