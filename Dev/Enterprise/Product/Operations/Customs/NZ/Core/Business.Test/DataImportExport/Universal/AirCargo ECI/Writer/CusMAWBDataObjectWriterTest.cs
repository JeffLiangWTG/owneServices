using Enterprise.Customs.NZ.Business.Express;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.NZ.Business.Data.Universal.Testing
{
	using CargoWise.EntityFramework;
	using CargoWise.Integration;
	using CargoWise.Types;
	using Enterprise.Customs.DataTransfer.Universal.Testing;
	using Enterprise.MasterFiles.Business;
	using Enterprise.MasterFiles.DataTransfer.Universal.Testing;
	using Enterprise.MasterFiles.Integration;
	using Enterprise.UniversalDataBuss.DataObjects.Universal;
	using Enterprise.ZArchitecture.Schema;

	partial class AirManifestDataObjectWriterTest : OrganizationAddressTestHelper
	{
		public void TestCusMAWBMappings()
		{
			var mawb = SetupCusMAWB(Factory.New<CusMAWB>(), "MB324242");
			var goodsLocation = GetOrganizationBO_CRAHOLSYD(Factory.BOFactory);
			mawb.CM_OA_GoodsLocation = goodsLocation.MainAddress.PK;
			mawb.Notes.AddNew(ZBool.True, "DUMMY NOTE", "HELLO WORLD");
			mawb.Notes.AddNew(ZBool.False, "DUMMY NOTE 2", "GOODBYE WORLD");
			var hawb1 = mawb.ChildBills.AddNew();
			hawb1.CS_HAWB = "HB24";
			hawb1.CS_PackType = "PK";
			var hawb2 = mawb.ChildBills.AddNew();
			hawb2.CS_HAWB = "HB89";
			hawb2.CS_PackType = "DN";
			Factory.SaveForTesting();
			var manager = (IShipmentDataContextManager)mawb.GetUniversalDataContextManager();
			var writer = manager.GetShipmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, mawb)));
			var mawbData = (Shipment)writer.GetDataObject(mawb);
			AssertHVLVManifestContents(mawbData, "MB324242");
			AssertEquals("mawbData.SubShipmentCollection.Count", 2, mawbData.SubShipmentCollection.Count);
			var hawbData1 = mawbData.SubShipmentCollection[0];
			AssertEquals("hawbData1.WayBillNumber", "HB24", hawbData1.WayBillNumber);
			AssertEquals("hawbData1.TotalNoOfPacksPackageType", "PK", hawbData1.TotalNoOfPacksPackageType.GetCodeAsUpperCase());
			var hawbData2 = mawbData.SubShipmentCollection[1];
			AssertEquals("hawbData2.WayBillNumber", "HB89", hawbData2.WayBillNumber);
			AssertEquals("hawbData2.TotalNoOfPacksPackageType", "DN", hawbData2.TotalNoOfPacksPackageType.GetCodeAsUpperCase());

			writer = new CusMAWBDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, mawb)));
			mawbData = (Shipment)writer.GetDataObject(mawb);
			AssertHVLVManifestContents(mawbData, "MB324242");
			AssertEquals("mawbData.SubShipmentCollection.Count", 2, mawbData.SubShipmentCollection.Count);
			hawbData1 = mawbData.SubShipmentCollection[0];
			AssertEquals("hawbData1.WayBillNumber", "HB24", hawbData1.WayBillNumber);
			AssertEquals("hawbData1.TotalNoOfPacksPackageType", "PK", hawbData1.TotalNoOfPacksPackageType.GetCodeAsUpperCase());
			hawbData2 = mawbData.SubShipmentCollection[1];
			AssertEquals("hawbData2.WayBillNumber", "HB89", hawbData2.WayBillNumber);
			AssertEquals("hawbData2.TotalNoOfPacksPackageType", "DN", hawbData2.TotalNoOfPacksPackageType.GetCodeAsUpperCase());

			writer = new CusMAWBDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, mawb)));
			((CusMAWBDataObjectWriter)writer).IncludeChildren = false;
			mawbData = (Shipment)writer.GetDataObject(mawb);
			AssertHVLVManifestContents(mawbData, "MB324242");
			AssertNull("mawbData.SubShipmentCollection", mawbData.SubShipmentCollection);
		}

		public void TestExtraCusMAWBMappings()
		{
			var mawb = SetupCusMAWB(Factory.New<CusMAWB>(), "MB324242");
			var goodsLocation = GetOrganizationBO_CRAHOLSYD(Factory.BOFactory);
			mawb.CM_OA_GoodsLocation = goodsLocation.MainAddress.PK;
			var writer = new CusMAWBDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, mawb)));
			var mawbData = writer.GetDataObject(mawb);
			AssertEquals("mawbData.OrganizationAddressCollection.Count", 2, mawbData.OrganizationAddressCollection.Count);
			AssertOrganizationBO_CRAHOLSYD("GoodsLocation", mawbData.OrganizationAddressCollection[0], nameof(DocAddressType.GoodsLocation));
			AssertOrganizationBO_WUFSHIJNB("ShippingLine", mawbData.OrganizationAddressCollection[1], AddressTypes.ShippingLine);
		}

		#region Implementation

		RefUNLOCO AirLocalPort1
		{
			get
			{
				if (airLocalPort1 == null)
				{
					var query = new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, Core.Constants.CountryCodes.Australia);
					query.AddToFilter(RefUNLOCOSchema.RL_HasAirport, true);
					airLocalPort1 = Factory.LoadTop1<RefUNLOCO>(query);
				}
				return airLocalPort1;
			}
		}
		RefUNLOCO airLocalPort1;

		RefUNLOCO AirLocalPort2
		{
			get
			{
				if (airLocalPort2 == null)
				{
					var query = new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, Core.Constants.CountryCodes.Australia);
					query.AddToFilter(RefUNLOCOSchema.PK, SQLComparisonOperator.NotEqual, AirLocalPort1.PK);
					query.AddToFilter(RefUNLOCOSchema.RL_HasAirport, true);
					airLocalPort2 = Factory.LoadTop1<RefUNLOCO>(query);
				}
				return airLocalPort2;
			}
		}
		RefUNLOCO airLocalPort2;

		RefUNLOCO AirLocalPort3
		{
			get
			{
				if (airLocalPort3 == null)
				{
					var query = new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, Core.Constants.CountryCodes.Australia);
					query.AddToFilter(RefUNLOCOSchema.PK, SQLComparisonOperator.NotEqual, new[] { AirLocalPort1.PK, AirLocalPort2.PK });
					query.AddToFilter(RefUNLOCOSchema.RL_HasAirport, true);
					airLocalPort3 = Factory.LoadTop1<RefUNLOCO>(query);
				}
				return airLocalPort3;
			}
		}
		RefUNLOCO airLocalPort3;

		RefUNLOCO AirForeignPort1
		{
			get
			{
				if (airForeignPort1 == null)
				{
					var query = new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, SQLComparisonOperator.NotEqual, Core.Constants.CountryCodes.Australia);
					query.AddToFilter(RefUNLOCOSchema.RL_HasAirport, true);
					airForeignPort1 = Factory.LoadTop1<RefUNLOCO>(query);
				}
				return airForeignPort1;
			}
		}
		RefUNLOCO airForeignPort1;

		RefUNLOCO AirForeignPort2
		{
			get
			{
				if (airForeignPort2 == null)
				{
					var query = new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, SQLComparisonOperator.NotEqual, Core.Constants.CountryCodes.Australia);
					query.AddToFilter(RefUNLOCOSchema.PK, SQLComparisonOperator.NotEqual, AirForeignPort1.PK);
					query.AddToFilter(RefUNLOCOSchema.RL_HasAirport, true);
					airForeignPort2 = Factory.LoadTop1<RefUNLOCO>(query);
				}
				return airForeignPort2;
			}
		}
		RefUNLOCO airForeignPort2;

		void AssertContents(Date dateDataObject, DateType type, ZDateTime dateTime, ZBool isEstimate)
		{
			AssertNotNull("Precondition: dateDataObject", dateDataObject);
			CombineAssertions(delegate
			{
				AssertEquals("dateDataObject.Type", type, dateDataObject.Type);
				AssertEquals("dateDataObject.Value", dateTime, dateDataObject.Value);
				AssertEquals("dateDataObject.IsEstimate", isEstimate, dateDataObject.IsEstimate);
			});
		}

		void AssertContents(Note noteData, ZBool isCustomDescription, ZString description, ZString noteText)
		{
			AssertNotNull("Precondition: noteData", noteData);
			CombineAssertions(delegate
			{
				AssertEquals("noteData.IsCustomDescription", isCustomDescription, noteData.IsCustomDescription);
				AssertEquals("noteData.Description", description, noteData.Description);
				AssertEquals("noteData.NoteText", noteText, noteData.NoteText);
			});
		}

		CusMAWB SetupCusMAWB(CusMAWB mawb, ZString wayBillNumber)
		{
			var shippingLine = GetOrganizationBO_WUFSHIJNB(Factory.BOFactory);
			mawb.CM_GB = GlbBranch.CurrentBranch.PK;
			mawb.CM_MAWB = wayBillNumber;
			mawb.CM_FlightNo = "QF123";
			mawb.CM_OH_ResponsibleParty = shippingLine.PK;
			mawb.CM_RL_NKLoadPort = AirForeignPort1.RL_Code;
			mawb.CM_RL_NKFirstArrivalPort = AirLocalPort1.RL_Code;
			mawb.CM_RL_NKDischargePort = AirLocalPort2.RL_Code;
			mawb.CM_DepartureDate = new ZDateTime(2012, 6, 4);
			mawb.CM_ArrivalDate = new ZDateTime(2012, 6, 6);
			mawb.CM_HasProhibitedPackaging = ZBool.True;
			mawb.CM_IsFinalManifest = ZBool.True;
			mawb.CM_CustomsStatus = LowValueManifestStatusList.Codes.ManifestAccepted;
			return mawb;
		}

		void AssertHVLVManifestContents(Shipment mawbData, ZString? wayBillNumber)
		{
			AssertAirCargoMasterContents(mawbData, wayBillNumber, "QF123", CodeDescriptionPairForTesting.New(AirForeignPort1.RL_Code, AirForeignPort1.RL_PortName), CodeDescriptionPairForTesting.New(AirLocalPort2.RL_Code, AirLocalPort2.RL_PortName), CodeDescriptionPairForTesting.New(GlbBranch.CurrentBranch.GB_Code, GlbBranch.CurrentBranch.GB_BranchName),
				ZBool.True, ZBool.True, CodeDescriptionPairForTesting.New(LowValueManifestStatusList.Codes.ManifestAccepted, LowValueManifestStatusList.Descriptions.ManifestAccepted));
			AssertNotNull("mawbData.DateCollection", mawbData.DateCollection);
			AssertEquals("mawbData.DateCollection.Count", 2, mawbData.DateCollection.Count);
			AssertContents(mawbData.DateCollection[0], DateType.LoadingDate, new ZDateTime(2012, 6, 4), ZBool.False);
			AssertContents(mawbData.DateCollection[1], DateType.DischargeDate, new ZDateTime(2012, 6, 6), ZBool.False);
			AssertEquals("mawbData.OrganizationAddressCollection.Count", 2, mawbData.OrganizationAddressCollection.Count);
			AssertOrganizationBO_CRAHOLSYD("GoodsLocation", mawbData.OrganizationAddressCollection[0], nameof(DocAddressType.GoodsLocation));
			AssertOrganizationBO_WUFSHIJNB("ShippingLine", mawbData.OrganizationAddressCollection[1], AddressTypes.ShippingLine);
			AssertNull("mawbData.AdditionalReferenceCollection", mawbData.AdditionalReferenceCollection);
			AssertEquals("mawbData.NoteCollection.Count", 2, mawbData.NoteCollection.Count);
			AssertContents(mawbData.NoteCollection[0], ZBool.True, "DUMMY NOTE", "HELLO WORLD");
			AssertContents(mawbData.NoteCollection[1], ZBool.False, "DUMMY NOTE 2", "GOODBYE WORLD");
		}

		void AssertAirCargoMasterContents(Shipment mawbData, ZString? wayBillNumber, ZString? flight, ICodeDescription portOfLoading, ICodeDescription portOfDischarge, ICodeDescription branch,
			ZBool? hasProhibitedPackaging, ZBool? isFinalManifest, ICodeDescription messageStatus)
		{
			AssertNotNull("Precondition: mawbData", mawbData);

			CombineAssertions(delegate
			{
				AssertEquals("mawbData.WayBillNumber", wayBillNumber, mawbData.WayBillNumber);
				AssertNotNull("mawbData.WayBillType", mawbData.WayBillType);
				AssertEquals("mawbData.WayBillType.Code", WayBillTypeList.Codes.Master, mawbData.WayBillType.Code);
				AssertEquals("mawbData.WayBillType.Description", WayBillTypeList.Descriptions.Master, mawbData.WayBillType.Description);
				AssertEquals("mawbData.VoyageFlightNo", flight, mawbData.VoyageFlightNo);
				AssertNotNull("mawbData.PortOfLoading", mawbData.PortOfLoading);
				AssertEquals("mawbData.PortOfLoading.Code", portOfLoading.Code, mawbData.PortOfLoading.Code);
				AssertEquals("mawbData.PortOfLoading.Name", portOfLoading.Description, mawbData.PortOfLoading.Name);
				AssertNotNull("mawbData.PortOfDischarge", mawbData.PortOfDischarge);
				AssertEquals("mawbData.PortOfDischarge.Code", portOfDischarge.Code, mawbData.PortOfDischarge.Code);
				AssertEquals("mawbData.PortOfDischarge.Name", portOfDischarge.Description, mawbData.PortOfDischarge.Name);
				AssertNotNull("mawbData.Branch", mawbData.Branch);
				AssertEquals("mawbData.Branch.Code", branch.Code, mawbData.Branch.Code);
				AssertEquals("mawbData.Branch.Name", branch.Description, mawbData.Branch.Name);
				AssertEquals("mawbData.HasProhibitedPackaging", hasProhibitedPackaging, mawbData.HasProhibitedPackaging);
				AssertEquals("mawbData.IsFinalManifest", isFinalManifest, mawbData.IsFinalManifest);
				AssertNotNull("mawbData.MessageStatus", mawbData.MessageStatus);
				AssertEquals("mawbData.MessageStatus.Code", messageStatus.Code, mawbData.MessageStatus.Code);
				AssertEquals("mawbData.MessageStatus.Name", messageStatus.Description, mawbData.MessageStatus.Description);
				AssertNull("mawbData.PortOfFirstArrival", mawbData.PortOfFirstArrival);
			});
		}

		#endregion
	}
}
