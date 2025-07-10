using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(CusContainer))]
	sealed class CusContainerTest : Customs.Business.Testing.BaseCusContainerTest<CusContainer, JobDeclaration>
	{
		[ExpectNoExceptions]
		public override void TestNewContainerLoadsInOtherFactory()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "IMP";
			declaration.JE_MessageSubType = "NOR";
			declaration.JE_TransportMode = declaration.TransportModeSeaCodeForTesting;
			declaration.JE_ContainerMode = "CNT";
			declaration.JE_HouseBill = "HOUSEBILL";
			declaration.JE_TotalNoOfPacks = 123;
			declaration.JE_TotalNoOfPacksPackType = "AE";
			Factory.Save();
			declaration.JE_VoyageFlightNo = "111";
			Factory.Save();
			var containers = new BaseCusContainerCollection<CusContainer>(declaration, Factory);
			var container1 = containers.AddNew();
			container1.CO_ContainerNumber = "OCLU1111110";
			container1.CO_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
			var packingInformation = declaration.PackingInformationCollection.AddNew();
			packingInformation.HouseBillContainer = new HouseBillContainer(declaration.Bills[0], null);
			Factory.Save();
			var packingGroup = declaration.PackingGroups[0];
			packingGroup.Packages.AddNew().CW_PackType = "AE";
			Factory.Save();
			var factory2 = new BusinessObjectFactory();
			var packageGroupLoaded = factory2.Load<BasePackingGroup>(packingGroup.PK);
			NUnit.Framework.Assert.That(packageGroupLoaded, NUnit.Framework.Is.Not.EqualTo(default(Enterprise.Customs.Business.BasePackingGroup)));
			NUnit.Framework.Assert.That(packageGroupLoaded.CR_CO_Container, NUnit.Framework.Is.EqualTo(packingGroup.CR_CO_Container), "Container should be in DB");
		}

		[ExpectNoExceptions]
		public override void TestDefaultPackingInformationIfNeeded()
		{
			using (TWCustomsDataRegistry.Instance.EnableCustomsDeclarationPackingList.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			{
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = "IMP";
				declaration.JE_MessageSubType = "NOR";
				declaration.JE_TransportMode = declaration.TransportModeSeaCodeForTesting;
				declaration.JE_ContainerMode = "CNT";
				declaration.JE_HouseBill = "HOUSEBILL";
				declaration.JE_TotalNoOfPacks = 123;
				declaration.JE_TotalNoOfPacksPackType = "AE";
				declaration.JE_VoyageFlightNo = "111";
				NUnit.Framework.Assert.That(declaration.IsPackingInformationRelevant, NUnit.Framework.Is.EqualTo(true));
				NUnit.Framework.Assert.That(declaration.ShouldDefaultPackingInfoFromDeclarationToBills, NUnit.Framework.Is.EqualTo(false));
				NUnit.Framework.Assert.That(declaration.PackingGroups.Count, NUnit.Framework.Is.EqualTo(0));
			}
		}

		[ExpectNoExceptions]
		public void TestTypeDecider()
		{
			NUnit.Framework.Assert.That(Factory.New(typeof(BaseCusContainer)).GetType(), NUnit.Framework.Is.EqualTo(typeof(CusContainer)), "Update BaseCusContainerTypeDecider to include a decider for this class");
		}

		[ExpectNoExceptions]
		public void TestDeclaration()
		{
			JobDeclaration declaration = (JobDeclaration)GetJobDeclaration();
			CusContainer container = declaration.CusContainers.AddNew();
			NUnit.Framework.Assert.That(container.Declaration, NUnit.Framework.Is.EqualTo(declaration));
		}

		[ExpectNoExceptions]
		public void TestValueChange()
		{
			JobDeclaration declaration = (JobDeclaration)GetJobDeclaration();
			CusContainer container = declaration.CusContainers.AddNew();
			System.Collections.Generic.List<string> list = new System.Collections.Generic.List<string>()
			{ TWContainerModesList.Codes.FCL, TWContainerModesList.Codes.BCN };
			foreach (CodeDescriptionPair mode in container.Lookups.CO_FCL_LCL_NCT_List)
			{
				container.CO_FCL_LCL_AIR = mode.Code;
				if (list.Contains(container.CO_FCL_LCL_AIR))
				{
					NUnit.Framework.Assert.That(container.CO_IsPart_ReadOnly, NUnit.Framework.Is.EqualTo(false));
					container.CO_IsPart = true;
					NUnit.Framework.Assert.That(container.CO_IsPart, NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison));
					container.CO_IsPart = false;
					NUnit.Framework.Assert.That(container.CO_IsPart, NUnit.Framework.Is.EqualTo(false).Using(CustomComparers.TypeComparison));
				}
				else
				{
					NUnit.Framework.Assert.That(container.CO_IsPart_ReadOnly, NUnit.Framework.Is.EqualTo(true));
					NUnit.Framework.Assert.That(container.CO_IsPart, NUnit.Framework.Is.EqualTo(false).Using(CustomComparers.TypeComparison));
				}
			}

			container.JC_IsEmptyContainer = true;
			NUnit.Framework.Assert.That(container.CO_FCL_LCL_AIR, NUnit.Framework.Is.EqualTo(TWContainerModesList.Codes.Empty).Using(CustomComparers.TypeComparison));
			container.JC_IsEmptyContainer = false;
			NUnit.Framework.Assert.That(container.CO_FCL_LCL_AIR, NUnit.Framework.Is.EqualTo(ZString.Empty));
			NUnit.Framework.Assert.That(container.JC_IsEmptyContainer, NUnit.Framework.Is.EqualTo(false).Using(CustomComparers.TypeComparison));
			foreach (CodeDescriptionPair mode in container.Lookups.CO_FCL_LCL_NCT_List)
			{
				container.CO_FCL_LCL_AIR = mode.Code;
				if (container.CO_FCL_LCL_AIR == TWContainerModesList.Codes.Empty)
				{
					NUnit.Framework.Assert.That(container.JC_IsEmptyContainer, NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison));
				}
				else
				{
					NUnit.Framework.Assert.That(container.JC_IsEmptyContainer, NUnit.Framework.Is.EqualTo(false).Using(CustomComparers.TypeComparison));
					container.CO_FCL_LCL_AIR = TWContainerModesList.Codes.Empty;
					NUnit.Framework.Assert.That(container.JC_IsEmptyContainer, NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison));
				}
			}
		}

		[ExpectNoExceptions]
		public void TestLookupsCachesInstance()
		{
			CusContainer container = (CusContainer)GetNewBusinessObject();
			CusContainerLookups lookup1 = container.Lookups;
			CusContainerLookups lookup2 = container.Lookups;
			NUnit.Framework.Assert.That(lookup1, NUnit.Framework.Is.EqualTo(lookup2));
		}

		[ExpectNoExceptions]
		public override void TestCO_Calc_TotalPackagesUnit()
		{
			var dec = GetJobDeclaration();
			dec.JE_TotalNoOfPacksPackType = Core.Constants.PkgUnit.Roll;
			var container1 = dec.CusContainers.AddNew();
			dec.Packages.RemoveAndDeleteAll();
			NUnit.Framework.Assert.That(container1.CO_Calc_TotalPackagesUnit, NUnit.Framework.Is.EqualTo(Core.Constants.PkgUnit.Roll).Using(CustomComparers.TypeComparison), "Use Declaration Type");
			container1.CO_JE = ZGuid.Invalid;
			NUnit.Framework.Assert.That(container1.Declaration, NUnit.Framework.Is.EqualTo(default(Enterprise.Customs.Business.BaseJobDeclaration)));
			NUnit.Framework.Assert.That(container1.CO_Calc_TotalPackagesUnit, NUnit.Framework.Is.EqualTo(ZString.Empty), "No Declaration");
			container1.CO_JE = dec.PK;
			var bill = dec.Bills.AddNew();
			var packingGroup1 = container1.PackingGroups.AddNew();
			var package1 = packingGroup1.Packages.AddNew();
			NUnit.Framework.Assert.That(container1.CO_Calc_TotalPackagesUnit, NUnit.Framework.Is.EqualTo(Core.Constants.PkgUnit.Roll).Using(CustomComparers.TypeComparison), "Use Declaration Type");
			package1.CW_PackQty = 1;
			NUnit.Framework.Assert.That(container1.CO_Calc_TotalPackagesUnit, NUnit.Framework.Is.EqualTo(Core.Constants.PkgUnit.Roll).Using(CustomComparers.TypeComparison), "Should use default pack type if packQty is not zero");
			package1.CW_PackType = Core.Constants.PkgUnit.Pallet;
			NUnit.Framework.Assert.That(container1.CO_Calc_TotalPackagesUnit, NUnit.Framework.Is.EqualTo(Core.Constants.PkgUnit.Pallet).Using(CustomComparers.TypeComparison), "Use from package1.CW_PackType");
			var package2 = packingGroup1.Packages.AddNew();
			NUnit.Framework.Assert.That(container1.CO_Calc_TotalPackagesUnit, NUnit.Framework.Is.EqualTo(Core.Constants.PkgUnit.Piece).Using(CustomComparers.TypeComparison), "Multiple package types");
			package2.CW_PackType = Core.Constants.PkgUnit.Reel;
			NUnit.Framework.Assert.That(container1.CO_Calc_TotalPackagesUnit, NUnit.Framework.Is.EqualTo(Core.Constants.PkgUnit.Piece).Using(CustomComparers.TypeComparison), "Multiple package types");
			package2.CW_PackType = Core.Constants.PkgUnit.Pallet;
			NUnit.Framework.Assert.That(container1.CO_Calc_TotalPackagesUnit, NUnit.Framework.Is.EqualTo(Core.Constants.PkgUnit.Pallet).Using(CustomComparers.TypeComparison), "all package type are the same");
			var container2 = dec.CusContainers.AddNew();
			var packingGroup2 = container2.PackingGroups.AddNew();
			packingGroup2.CR_CU_HouseBill = bill.PK;
			var package3 = packingGroup2.Packages.AddNew();
			package3.CW_PackType = Core.Constants.PkgUnit.Keg;
			NUnit.Framework.Assert.That(container1.CO_Calc_TotalPackagesUnit, NUnit.Framework.Is.EqualTo(Core.Constants.PkgUnit.Pallet).Using(CustomComparers.TypeComparison), "all package type for this container are the same");
			NUnit.Framework.Assert.That(container2.CO_Calc_TotalPackagesUnit, NUnit.Framework.Is.EqualTo(Core.Constants.PkgUnit.Keg).Using(CustomComparers.TypeComparison));
		}

		#region Implementation
		protected override ICustomLabelsProvider GetNewCustomLabelsProvider(BusinessObject bo)
		{
			ICustomLabelsProvider result = new BaseCusContainer.CustomLabelsProvider(((CusContainer)bo).Declaration);
			return result;
		}
		#endregion
	}
}
