using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Business.AES;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.DocumentEngineIntegration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.US.AES.Testing
{
	abstract class AESPrintTest : NonPersistentBusinessObjectTestCase
	{
		public void TestFilerID()
		{
			var filer = new Registry.Business.Customs.US.ExportEntryFilerID();
			filer.EntryFilerID = "364331434";
			filer.EntryFilerIDType = "E";
			USCustomsDataRegistry.Instance.ExportEntryFilerID.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, filer);
			var printObjectForTest = GetObjectForTest();
			AssertEquals("Filer ID", "364331434", printObjectForTest.FilerID);
		}

		public void TestGetInBondTypeAndDescription()
		{
			var printObjectForTest = GetObjectForTest();
			AssertEquals("67 Foreign Trade Zone withdrawal for IE", printObjectForTest.GetInBondTypeAndDescription(InbondTypeList.Codes.IEForeignTradeZoneWithdrawal));
		}

		public void TestGetCountryOfDestinationName()
		{
			var printObjectForTest = GetObjectForTest();
			AssertEquals("Country of Destination description", ZString.Empty, printObjectForTest.GetCountryOfDestinationName(ZString.Empty));
			AssertEquals("Country of Destination description", ZString.Empty, printObjectForTest.GetCountryOfDestinationName("!F"));
			AssertEquals("Country of Destination description", "UNITED KINGDOM (GB)", printObjectForTest.GetCountryOfDestinationName("GB"));
		}

		public void TestGetStateDescription()
		{
			var printObjectForTest = GetObjectForTest();
			AssertEquals("State description", ZString.Empty, printObjectForTest.GetStateDescription(ZString.Empty));
			AssertEquals("State description", "!F", printObjectForTest.GetStateDescription("!F"));
			AssertEquals("State description", "TEXAS (TX)", printObjectForTest.GetStateDescription("TX"));
		}

		public void TestGetOriginIndicatorDescription()
		{
			var printObjectForTest = GetObjectForTest();
			AssertEquals(AESOriginIndicatorList.Descriptions.Domestic.ToUpper(), printObjectForTest.GetOriginIndicatorDescription(AESOriginIndicatorList.Codes.Domestic));
			AssertEquals(AESOriginIndicatorList.Descriptions.Foreign.ToUpper(), printObjectForTest.GetOriginIndicatorDescription(AESOriginIndicatorList.Codes.Foreign));
		}

		public void TestGetModeOfTransportationDescription()
		{
			var printObjectForTest = GetObjectForTest();
			AssertEquals("Mode of Transportation description", ZString.Empty, printObjectForTest.GetModeOfTransportationDescription(ZString.Empty));
			AssertEquals("Mode of Transportation description", "~SD", printObjectForTest.GetModeOfTransportationDescription("~SD"));
			AssertEquals("Mode of Transportation description", "Air, Non-container (40)", printObjectForTest.GetModeOfTransportationDescription("40"));
		}

		public void TestGetPortOfExportDescription()
		{
			var newFactory = new BusinessObjectFactory();
			var startDate = ZDateTime.UtcToday.Date.AddMonths(-1);
			var endDate = ZDateTime.UtcToday.Date.AddMonths(1);
			var helper = new UniversalReferenceTestDataHelper(newFactory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CUSOF");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "3901", "CHICAGO, IL", startDate, endDate);
			newFactory.Save();

			var printObjectForTest = GetObjectForTest();
			AssertEquals("Port of Export description", ZString.Empty, printObjectForTest.GetPortOfExportDescription(ZString.Empty));
			AssertEquals("Port of Export description", ZString.Empty, printObjectForTest.GetPortOfExportDescription("3901~"));
			AssertEquals("Port of Export description", "CHICAGO, IL (3901)", printObjectForTest.GetPortOfExportDescription("3901"));
		}

		public void TestGetIDTypeFormatted()
		{
			var printObjectForTest = GetObjectForTest();
			AssertEquals("ID Type description", ZString.Empty, printObjectForTest.GetIDTypeFormatted(ZString.Empty));
			AssertEquals("ID Type description", ZString.Empty, printObjectForTest.GetIDTypeFormatted("KKK"));
			AssertEquals("ID Type description", " (DUN)", printObjectForTest.GetIDTypeFormatted(AESConstants.IDTypes.DUNS));
		}

		[TestDate(2010, 08, 25, 13, 30, 45)]
		public void TestCurrentDateTimeInSpecificFormat()
		{
			var printObjectForTest = GetObjectForTest();
			AssertEquals("Current date and time in specific format", "Wed August 25 13:30:45 2010 EDT", printObjectForTest.CurrentDateTimeInSpecificFormat);
		}

		public void TestIVisualizerNoteSupporterMembers()
		{
			var printObjectForTest = GetObjectForTest();
			var supporter = printObjectForTest as IVisualizerNoteSupporter;
			AssertNotNull("AESPrint should implement IVisualizerNoteSupporter", supporter);
			AssertEquals("supporter.PK", printObjectForTest.entry.PK, supporter.PK);
			AssertEquals("supporter.TableCode", CusEntryHeaderSchema.Constants.Prefix, supporter.TableCode);
			AssertEquals("supporter.ChildBusinessObjectPK", ZGuid.Empty, supporter.ChildBusinessObjectPK);
		}

		protected virtual AESPrint GetObjectForTest() => null;

		JobDeclaration Declaration
		{
			get
			{
				if (declaration == null)
				{
					declaration = Factory.New<JobDeclaration>();
					declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
				}
				return declaration;
			}
		}
		JobDeclaration declaration;

		protected CusEntryHeader Entry => entry ?? (entry = Declaration.CustomsEntryHeaders.AddNew());
		CusEntryHeader entry;
	}
}
