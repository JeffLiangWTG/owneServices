using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Common.Business.Testing
{
	[TestedType(typeof(CommonCartageType))]
	sealed class CommonCaratgeTypeBusinessObjectTest : EnterpriseBusinessObjectTestCase
	{
		public void TestE3_Description_Translatable()
		{
			var bizO = Factory.New<CommonCartageType>();
			bizO.E3_Description = "Boom";
			string resKey = bizO.E3_DescriptionInfo.CustomizableDataResourceStrings.GetMultilingualString(bizO, "Boom").ResourceKey;
			AssertEquals("Boom", bizO.E3_DescriptionMultilingual);
			using (Res.TemporarilySwitchLanguage(Core.SharedConstants.Languages.ChineseSimplified))
			using (var mockRes = Res.UseMockData())
			{
				mockRes.Put(resKey, new ResourceStringData(resKey, "咚"));
				AssertEquals("咚", bizO.E3_DescriptionMultilingual);
			}
		}

		public void TestHumanReadableName()
		{
			var bizO = Factory.New<CommonCartageType>();
			AssertEquals("Cartage Job Type", bizO.HumanReadableName);

			bizO.E3_JobType = "Boom";
			AssertEquals("Cartage Job Type Boom", bizO.HumanReadableName);
		}

		public void TestContainerMode()
		{
			CommonCartageType cartageType = Factory.New<CommonCartageType>();
			cartageType.ContainerMode = Constants.CartageContainerMode.Containerized;
			CommonCartageLegType containerizedMove = cartageType.ContainerizedBookedMoveTypes.AddNew();
			CommonCartageLegType containerizedLeg = cartageType.ContainerizedCartageLegTypes.AddNew();

			AssertEquals(1, cartageType.ContainerizedBookedMoveTypes.Count);
			AssertEquals(1, cartageType.ContainerizedCartageLegTypes.Count);
			AssertEquals(0, cartageType.LooseBookedMoveTypes.Count);
			AssertEquals(0, cartageType.LooseCartageLegTypes.Count);

			cartageType.ContainerMode = Constants.CartageContainerMode.Loose;
			Assert(containerizedMove.IsDeleted);
			Assert(containerizedLeg.IsDeleted);
			AssertEquals(0, cartageType.ContainerizedBookedMoveTypes.Count);
			AssertEquals(0, cartageType.ContainerizedCartageLegTypes.Count);
			AssertEquals(0, cartageType.LooseBookedMoveTypes.Count);
			AssertEquals(0, cartageType.LooseCartageLegTypes.Count);

			CommonCartageLegType looseMove = cartageType.LooseBookedMoveTypes.AddNew();
			CommonCartageLegType looseLeg = cartageType.LooseCartageLegTypes.AddNew();
			AssertEquals(0, cartageType.ContainerizedBookedMoveTypes.Count);
			AssertEquals(0, cartageType.ContainerizedCartageLegTypes.Count);
			AssertEquals(1, cartageType.LooseBookedMoveTypes.Count);
			AssertEquals(1, cartageType.LooseCartageLegTypes.Count);

			cartageType.ContainerMode = Constants.CartageContainerMode.Mixed;
			Assert(!looseMove.IsDeleted);
			Assert(!looseLeg.IsDeleted);
			AssertEquals(0, cartageType.ContainerizedBookedMoveTypes.Count);
			AssertEquals(0, cartageType.ContainerizedCartageLegTypes.Count);
			AssertEquals(1, cartageType.LooseBookedMoveTypes.Count);
			AssertEquals(1, cartageType.LooseCartageLegTypes.Count);
		}

		public void TestJobType()
		{
			CommonCartageType cartageType = Factory.New<CommonCartageType>();
			cartageType.E3_JobType = "ESC1";
			AssertEquals(Constants.CartageDirection.Export, cartageType.Direction);
			AssertEquals(Constants.TransportModes.Sea, cartageType.E3_ShippingTransportMode);
			AssertEquals(Constants.CartageContainerMode.Containerized, cartageType.ContainerMode);

			cartageType.E3_JobType = "IRL1";
			AssertEquals(Constants.CartageDirection.Import, cartageType.Direction);
			AssertEquals(Constants.TransportModes.Road, cartageType.E3_ShippingTransportMode);
			AssertEquals(Constants.CartageContainerMode.Loose, cartageType.ContainerMode);

			cartageType.E3_JobType = "";
			AssertEquals("", cartageType.Direction);
			AssertEquals(Constants.TransportModes.Road, cartageType.E3_ShippingTransportMode);
			AssertEquals("", cartageType.ContainerMode);
		}

		public void TestIsDomestic()
		{
			CommonCartageType cartageType = Factory.New<CommonCartageType>();
			cartageType.E3_JobType = "OUD1";
			AssertEquals("Is Domestic", true, cartageType.IsDomestic);

			cartageType.E3_JobType = "DDD1";
			AssertEquals("Is Domestic", true, cartageType.IsDomestic);

			cartageType.E3_JobType = "";
			AssertEquals("Is not domestic", false, cartageType.IsDomestic);

			cartageType.E3_JobType = "HAT1";
			AssertEquals("Is not domestic", false, cartageType.IsDomestic);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public override void TestBizObjectFields()
		{
			base.TestBizObjectFields();
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			CommonCartageType cartageType = factory.New<CommonCartageType>();
			cartageType.E3_GE = GlbDepartment.CurrentDepartment.PK;
			cartageType.E3_Description = "Some Description";
			cartageType.E3_JobType = "JOB";
			CommonCartageLegType cartageLegType = factory.New<CommonCartageLegType>();
			cartageLegType.E4_E3 = cartageType.PK;
			CommonCartageOrg cartageOrg = factory.New<CommonCartageOrg>();
			cartageOrg.E5_E3 = cartageType.PK;
			return cartageType;
		}

		protected override BusinessObject GetNewBusinessObjectForTranslatableFieldTest(BusinessObjectFactory factory)
		{
			var cartageType = (CommonCartageType)base.GetNewBusinessObjectForTranslatableFieldTest(factory);
			cartageType.E3_JobType = "JOB";

			return cartageType;
		}
	}
}
