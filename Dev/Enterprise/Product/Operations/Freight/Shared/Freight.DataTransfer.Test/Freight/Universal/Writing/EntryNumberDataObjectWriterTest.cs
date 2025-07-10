using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Freight.DataTransfer.Universal.Testing
{
	public sealed class EntryNumberDataObjectWriterTest : TestCaseWithFactory
	{
		public void TestBasicCusEntryNumberLevelMappings()
		{
			var shipmentBO = Factory.New<CommonShipment>();
			var entryNumberBO = SetupCusEntryNumber(shipmentBO);

			var writer = new EntryNumberDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, entryNumberBO)));
			var entryNumberDataObject = writer.GetDataObject(entryNumberBO);
			AssertNotNull("Precondition: entryNumberDataObject", entryNumberDataObject);

			CombineAssertions(() => AssertContents(entryNumberDataObject));
		}

		public void TestPopulateAustralianExemptionCode()
		{
			var shipment = Factory.New<CommonShipment>();
			shipment.CustomsEntryNumberType = CMRExportExemptionCodes.EXDD.Code;

			AssertEquals("Shipment has customs entery number", 1, shipment.CusEntryNumbers.Count);
			AssertEquals("Customs entery number has correct 3 digit code", "XDD", shipment.CusEntryNumbers[0].CE_EntryType);

			var entryNumberBO = shipment.CusEntryNumbers[0];
			var writer = new EntryNumberDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, entryNumberBO)));
			var entryNumberDataObject = writer.GetDataObject(entryNumberBO);

			AssertNotNull("Precondition: entryNumberDataObject", entryNumberDataObject);
			AssertNotNull("entryNumberDataObject.Type", entryNumberDataObject.Type);
			AssertEquals("entryNumberDataObject.Type", "XDD", entryNumberDataObject.Type.Code);
			AssertEquals("entryNumberDataObject.Description", "Military goods. Owned by Australian Government", entryNumberDataObject.Type.Description);
		}

		public void TestPopulateUSExportCode()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedStates))
			{
				var shipment = Factory.New<CommonShipment>();
				var entryNumberBO = SetupCusEntryNumber(shipment);
				shipment.JS_RL_NKOrigin = "USCHI";
				shipment.JS_RL_NKDestination = "AUSYD";
				entryNumberBO.CE_EntryType = "ITN";
				entryNumberBO.CE_RN_NKCountryCode = "US";

				var writer = new EntryNumberDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, entryNumberBO)));
				var entryNumberDataObject = writer.GetDataObject(entryNumberBO);

				AssertNotNull("Precondition: entryNumberDataObject", entryNumberDataObject);
				AssertNotNull("entryNumberDataObject.Type", entryNumberDataObject.Type);
				AssertEquals("entryNumberDataObject.Type", "ITN", entryNumberDataObject.Type.Code);
				AssertEquals("entryNumberDataObject.Description", "Internal Transaction Number", entryNumberDataObject.Type.Description);
			}
		}

		public static CusEntryNumber SetupCusEntryNumber(BusinessObject parentBO)
		{
			var cusEntryNumber = parentBO.Factory.New<CusEntryNumber>();

			cusEntryNumber.CE_EntryNum = "CE00001";
			cusEntryNumber.CE_EntryType = "COM";
			cusEntryNumber.CE_EntryLineReference = "REFERENCE";
			cusEntryNumber.CE_EntryIsSystemGenerated = false;
			cusEntryNumber.CE_EntryStatus = "FAL";
			cusEntryNumber.CE_Category = "CUS";
			cusEntryNumber.CE_IssueDate = new ZDateTime(2011, 3, 3);
			cusEntryNumber.CE_ExpiryDate = new ZDateTime(2011, 2, 2);
			cusEntryNumber.CE_RN_NKCountryCode = "NZ";
			cusEntryNumber.CE_ParentID = parentBO.PK;
			cusEntryNumber.CE_ParentTable = parentBO.TableName;

			return cusEntryNumber;
		}

		public static void AssertContents(EntryNumber entryNumberDataObject)
		{
			AssertEquals("entryNumberDataObject.CountryOfIssue.Code", "NZ", entryNumberDataObject.CountryOfIssue.Code);
			AssertEquals("entryNumberDataObject.CountryOfIssue.Name", "New Zealand", entryNumberDataObject.CountryOfIssue.Name);
			AssertEquals("entryNumberDataObject.EntryIsSystemGenerated", false, entryNumberDataObject.EntryIsSystemGenerated);
			AssertEquals("entryNumberDataObject.EntryLineReference", "REFERENCE", entryNumberDataObject.EntryLineReference);
			AssertEquals("entryNumberDataObject.EntryStatus.Code", "FAL", entryNumberDataObject.EntryStatus.Code);
			AssertEquals("entryNumberDataObject.EntryStatus.Description - Empty for now, but we should map this using a CodeDescriptionPairList in the future.", null, entryNumberDataObject.EntryStatus.Description);
			AssertEquals("entryNumberDataObject.ExpiryDate", new ZDateTime(2011, 2, 2), entryNumberDataObject.ExpiryDate);
			AssertEquals("entryNumberDataObject.IssueDate", new ZDateTime(2011, 3, 3), entryNumberDataObject.IssueDate);
			AssertEquals("entryNumberDataObject.Number", "CE00001", entryNumberDataObject.Number);
			AssertEquals("entryNumberDataObject.Type.Code", "COM", entryNumberDataObject.Type.Code);
			AssertEquals("entryNumberDataObject.Type.Description", "Completion Entry No.", entryNumberDataObject.Type.Description);
		}
	}
}
