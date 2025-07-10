using CargoWise.Definitions;
using Enterprise.Freight.LocalCartage.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.LocalCartage.DataTransfer.Universal.Testing
{
	[TestedType(typeof(LocalTransportLegDataContextManager))]
	public class LocalTransportLegDataContextManagerTest : DataContextManagerTestCase<LocalTransportLegDataContextManager, CommonCartageLeg>
	{
		public void TestDataContextType()
		{
			AssertEquals(DataContextType.LocalTransportLeg, GetManager().DataContextType);
		}

		public void TestDataContextKey()
		{
			AssertEquals("T00001234/A", GetManager().DataContextKey);
		}

		public void TestDataContextKeyWhenCartageHasSlashInConsignmentID()
		{
			AssertEquals("S00001234/I/A", GetManager("S00001234/I").DataContextKey);
		}

		public void TestManageShipments()
		{
			AssertEquals(false, GetManager().ManagesShipments());
		}

		public void TestManageEvents()
		{
			AssertEquals(true, GetManager().ManagesEvents);
		}

		public void TestDefaultOutputDirectory()
		{
			AssertNull(GetManager().DefaultOutputDirectory);
		}

		public void TestEventContextValues()
		{
			AssertMultilineASCIIEquals("manager.EventContextValues", @"
TransportBookingJobID - TB00001234
TransportReference - T00001234
NumberOfPieces - 10
PackageType - DRM
WeightOfGoods - 20 KG
VolumeOfGoods - 30 M3
ReceivedFromName - Bob
".Trim(), GetManager().EventContextValues.ToStringContents(e => e.Key + " - " + e.Value));
			var leg = GetNewBusinessObjectForTesting();
			var reference = leg.Cartage.AdditionalReferenceNumbers.AddNew();
			reference.CE_EntryType = TransportAdditionalReferenceTypes.Codes.ExternalTransportBookingNumber;
			reference.CE_EntryNum = "ABC";
			var manager = leg.GetUniversalDataContextManager() as IEventDataContextManager;
			AssertMultilineASCIIEquals("manager.EventContextValues", @"
TransportBookingJobID - ABC
TransportReference - T00001234
NumberOfPieces - 10
PackageType - DRM
WeightOfGoods - 20 KG
VolumeOfGoods - 30 M3
ReceivedFromName - Bob
".Trim(), manager.EventContextValues.ToStringContents(e => e.Key + " - " + e.Value));
		}

		protected override string GetExpectedDataSourceForUniversalShipmentTopLevelDataObject(IDataContextManager manager)
		{
			return "LocalTransportLeg []";
		}

		protected override CommonCartageLeg GetNewBusinessObjectForTesting()
		{
			return GetNewBusinessObjectForTesting("T00001234");
		}

		protected CommonCartageLeg GetNewBusinessObjectForTesting(string cartageConsignmentID)
		{
			var cartage = Factory.New<CommonCartage>();
			cartage.JJ_ConsignmentID = cartageConsignmentID;
			cartage.JJ_OrderReferenceNumber = "TB00001234";
			var move = cartage.LooseBookedMoves.AddNew();
			move.EW_BookedPackCount = 10;
			move.EW_F3_NKPackType = Core.Constants.PkgUnit.Drum;
			move.EW_BookedWeight = 20;
			move.EW_WeightUQ = Core.Constants.Weight.Kilograms;
			move.EW_BookedVolume = 30;
			move.EW_VolumeUQ = Core.Constants.Volume.CubicMetres;
			var leg = move.CartageLegs.AddNew();
			leg.JU_DeliverySignedFor = "Bob";
			leg.JU_SplitDeliverySuffix = "A";
			return leg;
		}

		IEventDataContextManager GetManager(string cartageConsignmentID = "T00001234")
		{
			return GetNewBusinessObjectForTesting(cartageConsignmentID).GetUniversalDataContextManager() as IEventDataContextManager;
		}
	}
}
