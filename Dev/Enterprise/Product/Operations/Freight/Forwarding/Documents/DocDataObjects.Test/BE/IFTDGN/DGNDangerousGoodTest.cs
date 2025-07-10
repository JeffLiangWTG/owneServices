using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.BE;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.Testing
{
	[TestedType(typeof(DGNDangerousGood))]
	sealed class DGNDangerousGoodTest : NonPersistentBusinessObjectTestCase
	{
		public void TestToString()
		{
			var undg = Factory.New<UNDGDataItem>();
			undg.DI_DG = UNDGSubstanceLoader.LoadSubstances(Factory, "0004", "a", "IMO").First().PK;
			undg.DI_IMOClass = "1.1D";
			undg.DI_IsCombustible = true;
			undg.DI_DGFlashPoint = 1;
			undg.DI_IsCombustible = true;
			undg.DI_PackageCount = 10;
			undg.DI_OC_DGContact = Factory.NewWithValidTestData<OrgContact>().PK;
			undg.DGContact.OC_ContactName = "Handsome";
			undg.DGContact.OC_Phone = "1234567";
			undg.DI_IsLimitedQuantity = true;
			undg.DI_TechnicalName = "DI_TechnicalName";

			var dg = new DGNDangerousGoodBuilder().Build(undg);

			AssertEquals(@"UN0004, AMMONIUM PICRATE, (DI_TechnicalName), class 1.1D, (1C c.c.), LTD QTY, contact Handsome 1234567", dg.ToString());
			Assert(dg.PackedInLimitedQuantity);

			undg.DI_IsLimitedQuantity = false;
			dg = new DGNDangerousGoodBuilder().Build(undg);
			AssertEquals(@"UN0004, AMMONIUM PICRATE, (DI_TechnicalName), class 1.1D, (1C c.c.), contact Handsome 1234567", dg.ToString());
			Assert(!dg.PackedInLimitedQuantity);

			using (FreightPacksDataRegistry.Instance.ActivateIsCombustibleForDGItems.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				undg.DI_IsCombustible = false;
				dg = new DGNDangerousGoodBuilder().Build(undg);
				AssertEquals(@"UN0004, AMMONIUM PICRATE, (DI_TechnicalName), class 1.1D, contact Handsome 1234567", dg.ToString());
			}

			using (FreightPacksDataRegistry.Instance.ActivateIsCombustibleForDGItems.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				undg.DI_IsCombustible = false;
				dg = new DGNDangerousGoodBuilder().Build(undg);
				AssertEquals(@"UN0004, AMMONIUM PICRATE, (DI_TechnicalName), class 1.1D, (1C c.c.), contact Handsome 1234567", dg.ToString());
			}
		}

		public void TestToString_ContainsRadioactiveMaterialForLimitedQuantity_WhenStandardIsCFRAndOneOtherClassIsSeven()
		{
			var dGSubstance = Factory.NewWithValidTestData<UNDGSubstanceCFR>();
			var undg = Factory.New<UNDGDataItem>();
			undg.DI_DG = dGSubstance.PK;
			undg.DI_IsLimitedQuantity = true;

			var dg = new DGNDangerousGoodBuilder().Build(undg);

			dg.Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.CFR;
			dg.IMOClass = "6.1";
			dg.SecondaryClass = "7";
			dg.TertiaryClass = "8";
			var result = dg.ToString();
			AssertContains("UN, class 6.1, Limited quantity radioactive material", result);
		}

		public void TestUnnoPrefixInToString()
		{
			var context = new CommonContext(Factory);

			var dgnDangerousGood = new DGNDangerousGood("ID")
			{
				Unno = "8000",
				Code = "8000",
				TransportMode = new CodeDescription(context.TransportModes)
				{
					Code = Constants.TransportModes.Air
				}
			};

			AssertEquals("DangerousGood should start with ID when UNNO is 8000 and transport mode is AIR", "ID8000", dgnDangerousGood.ToString());

			dgnDangerousGood.TransportMode.Code = Constants.TransportModes.Sea;
			AssertEquals("DangerousGood should start with UN when UNNO is not 8000", "ID8000", dgnDangerousGood.ToString());
		}

		public void TestUnnoPrefixInToStringGeneratedByBuilder()
		{
			var undgSubstance = Factory.New<UNDGSubstance>();
			undgSubstance.DG_UNNO = "8000";
			undgSubstance.DG_Mode = Constants.TransportModes.Air;

			var undgDataItem = Factory.New<UNDGDataItem>();
			undgDataItem.DI_DG = undgSubstance.PK;

			var builder = new DGNDangerousGoodBuilder();

			var dgnDangerousGood = builder.Build(undgDataItem);
			AssertEquals("DangerousGood should start with ID when UNNO is 8000 and transport mode is AIR", "ID8000", dgnDangerousGood.ToString());

			undgSubstance.DG_Mode = Constants.TransportModes.Sea;
			undgDataItem.UNDGSubstancePivotCollection.UpdateDefaultPivot(undgSubstance);
			dgnDangerousGood = builder.Build(undgDataItem);
			AssertEquals("DangerousGood should start with ID when UNNO is 8000 and transport mode is SEA", "ID8000", dgnDangerousGood.ToString());

			undgSubstance.DG_Mode = Constants.TransportModes.Air;
			undgSubstance.DG_UNNO = "6969";
			undgDataItem.UNDGSubstancePivotCollection.UpdateDefaultPivot(undgSubstance);
			dgnDangerousGood = builder.Build(undgDataItem);
			AssertEquals("DangerousGood should start with UN when UNNO is not 8000", "UN6969", dgnDangerousGood.ToString());
		}

		public void TestPopulateTechnicalName()
		{
			var undgSubstance = Factory.NewWithValidTestData<UNDGSubstance>();

			var undgDataItem = Factory.NewWithValidTestData<UNDGDataItem>();
			undgDataItem.DI_DG = undgSubstance.PK;

			var dgBuilder = new DGNDangerousGoodBuilder();
			var dg = dgBuilder.Build(undgDataItem);

			Assert(dg.TechnicalName.IsEmpty);

			undgDataItem.DI_TechnicalName = "DI_TechnicalName";
			dg = dgBuilder.Build(undgDataItem);
			undgSubstance.DG_PSN = "DG_PSN";

			AssertEquals("DI_TechnicalName", dg.TechnicalName);

			undgDataItem.DI_TechnicalName = ZString.Empty;
			dg = dgBuilder.Build(undgDataItem);

			AssertEquals("DG_PSN", dg.TechnicalName);
		}
	}
}
