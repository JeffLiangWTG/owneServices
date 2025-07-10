using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using static Enterprise.Core.Constants;
using static Enterprise.MasterFiles.Business.UNDGSubstanceLookups;

namespace Enterprise.Freight.Agency.Documents.DocDataObjects.Testing
{
	[TestedType(typeof(DangerousGood))]
	class DangerousGoodTest : NonPersistentBusinessObjectTestCase
	{
		#region Build

		public void TestBuild()
		{
			var substance_UN0004A = UNDGSubstanceLoader.LoadSubstances(Factory, "0004", "a", "IMO").First();
			AssertEquals("prerequisite: 0004A is an IMO substance", UNDGSubstanceStandardTypes.IMO, substance_UN0004A.DG_Standard);

			var undg = Factory.New<UNDGDataItem>();
			undg.DI_DG = substance_UN0004A.PK;
			undg.DI_IMOClass = "1.1D";
			undg.DI_IsCombustible = true;
			undg.DI_DGFlashPoint = 1;
			undg.DI_PackageCount = 10;
			undg.DI_OC_DGContact = Factory.NewWithValidTestData<OrgContact>().PK;
			undg.DGContact.OC_ContactName = "Deacon St John";
			undg.DGContact.OC_Phone = "1234567";
			undg.DI_IsLimitedQuantity = true;

			void Assert()
			{
				var dangerousGood = new DangerousGoodBuilder().Build(undg, Context);
				AssertEquals(nameof(dangerousGood.Standard), dangerousGood.Standard, UNDGSubstanceStandardTypes.IMO);
				AssertEquals(nameof(dangerousGood.TransportMode), dangerousGood.TransportMode.Code, TransportModes.Sea);
				AssertEquals(nameof(dangerousGood.Quantity), dangerousGood.Quantity, 10);
				AssertEquals(nameof(dangerousGood.FlashPoint), dangerousGood.FlashPoint.Value, 1m);
				AssertEquals(nameof(dangerousGood.FlashPoint), dangerousGood.FlashPoint.Unit.Code, "C");
				AssertEquals(nameof(dangerousGood.ProperShippingName), dangerousGood.ProperShippingName, "AMMONIUM PICRATE");
				AssertEquals(nameof(dangerousGood.Contact), dangerousGood.Contact.FullName, "Deacon St John");
				AssertEquals(nameof(dangerousGood.Contact), dangerousGood.Contact.Phone, "1234567");
				AssertEquals(nameof(dangerousGood.PackedInLimitedQuantity), dangerousGood.PackedInLimitedQuantity, true);
			}

			CombineAssertions(Assert);
		}

		#endregion

		#region Implementation

		IContext Context => context ?? (context = new CommonContext(Factory.GetCachedReadOnlyFactory()));
		IContext context;

		#endregion
	}
}
