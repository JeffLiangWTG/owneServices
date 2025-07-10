using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.eManifest.Business.Testing
{
	[TestedType(typeof(InBond))]
	sealed class InBondTest : EnterpriseBusinessObjectTestCase
	{
		public void TestInBondNumberObj()
		{
			var inBond = (InBond)GetNewBusinessObject();
			var num = Factory.New<CusEntryNumber>();
			num.CE_ParentTable = InBond.Schema.TableName;
			num.CE_ParentID = inBond.PK;
			num.CE_EntryType = CusEntryNumber.EntryType.InBond;
			num.CE_RN_NKCountryCode = inBond.CountryCode;
			num.CE_EntryNum = "N0";
			num.CE_SystemCreateTimeUtc = ZDateTime.UtcNow;
			Factory.Save();
			AssertEquals(num.CE_EntryNum, inBond.InBondNumber);
		}

		public void TestICusInBondMoveHeaderIsCorrectlySetup()
		{
			AssertEquals(typeof(InBond), ObjectFactory.GetType<Integration.Customs.US.eManifest.ICusInBondMoveHeader>());
		}

		public void TestSetDefaultValues()
		{
			var inbond = (InBond)GetNewBusinessObject();
			inbond.BM_RL_NKForeignDestPort = "AUSYD";
			AssertEquals("60267", inbond.BM_ForeignDestPortKCode);
			inbond.BM_RL_NKDestinationPort = "USLAX";
			AssertEquals("2772", inbond.BM_DestinationPortCode);
		}

		public void TestRunPreSaveValidationCore()
		{
			var inbond = (InBond)GetNewBusinessObject();
			inbond.Shipment.B0_ShipmentType = ShipmentTypes.Codes.PAPS;
			inbond.RunPreSaveValidation();
			AssertEquals("No need to run validation if shipment is not inbond anymore " + "as far as the object is going to be deleted on save and it caused some problem", false, inbond.HasNotifications());
			inbond.Shipment.B0_ShipmentType = ShipmentTypes.Codes.Inbond;
			inbond.MarkAsNeedingValidation();
			inbond.RunPreSaveValidation();
			AssertEquals("Pre-save validation has been run", true, inbond.HasNotifications());
		}

		public void TestIsExport()
		{
			var inbond = (InBond)GetNewBusinessObject();
			inbond.BM_InBondEntryType = InbondTypes.Codes.ImmediateExportation;
			AssertEquals("ImmediateExportation.IsExport", true, inbond.IsExport);
			inbond.BM_InBondEntryType = InbondTypes.Codes.TransportationAndExportation;
			AssertEquals("TransportationAndExportation.IsExport", true, inbond.IsExport);
			inbond.BM_InBondEntryType = InbondTypes.Codes.ImmediateTransportation;
			AssertEquals("ImmediateTransportation.IsExport", false, inbond.IsExport);
		}

		public void TestExportValuesAreCleardOnSavingIfNotExportAnymore()
		{
			var inBond = (InBond)GetNewBusinessObject();
			inBond.BM_InBondEntryType = InbondTypes.Codes.ImmediateExportation;
			inBond.BM_RL_NKForeignDestPort = "USLAX";
			inBond.BM_ForeignDestPortKCode = "79845";
			inBond.BM_ExportDate = new ZDate(2011, 08, 31);
			inBond.BM_PedimentoNumber = "98765413265478";
			Factory.Save();
			AssertExportValuesEmpty(inBond, false);
			inBond.BM_InBondEntryType = InbondTypes.Codes.ImmediateTransportation;
			AssertExportValuesEmpty(inBond, false);
			Factory.Save();
			AssertExportValuesEmpty(inBond, true);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var trip = factory.New<Trip>();
			var shipment = trip.Shipments.AddNew();
			shipment.B0_ShipmentType = ShipmentTypes.Codes.Inbond;
			return shipment.InBond;
		}

		protected override BusinessObject GetNewBusinessObject() => GetNewBusinessObjectForDeleteTest(Factory);

		protected override LightValidationTester GetNewLightValidationTester(BusinessObject bizObjToTest) => new InBondLightValidationTester(bizObjToTest);

		static void AssertExportValuesEmpty(InBond inBond, bool isEmpty)
		{
			AssertEquals("BM_RL_NKForeignDestPort", isEmpty, inBond.BM_RL_NKForeignDestPort.IsEmpty);
			AssertEquals("BM_ForeignDestPortKCode", isEmpty, inBond.BM_ForeignDestPortKCode.IsEmpty);
			AssertEquals("BM_ExportDate", isEmpty, inBond.BM_ExportDate.IsEmpty);
			AssertEquals("BM_PedimentoNumber", isEmpty, inBond.BM_PedimentoNumber.IsEmpty);
		}

		sealed class InBondLightValidationTester : LightValidationTester
		{
			public InBondLightValidationTester(BusinessObject bo)
				: base(bo)
			{
			}

			protected override bool ShouldTestProperty(ZPropertyInfo info) => base.ShouldTestProperty(info) && (info.Name != Shipment.Schema.B0_ShipmentType);
		}
	}
}
