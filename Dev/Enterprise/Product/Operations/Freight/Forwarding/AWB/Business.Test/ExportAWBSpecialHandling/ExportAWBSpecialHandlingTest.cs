using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.AWB.Business.Testing
{
	[TestedType(typeof(ExportAWBSpecialHandling))]
	sealed class ExportAWBSpecialHandlingTest : EnterpriseBusinessObjectTestCaseWithListChecking<ExportAWBSpecialHandling>
	{
		public void TestSpecialHandlingDescription()
		{
			var specialHandlingItem = Factory.New<ExportAWBSpecialHandling>();
			specialHandlingItem.EP_SpecialHandling = AWBSpecialHandlingCodeDescriptionPairList.Codes.CoolGoods;
			AssertEquals("SpecialHandlingDescription should show description for code", AWBSpecialHandlingCodeDescriptionPairList.Descriptions.CoolGoods, specialHandlingItem.SpecialHandlingDescription);

			specialHandlingItem.EP_SpecialHandling = ZString.Empty;
			AssertEquals("SpecialHandlingDescription should be empty when code is empty", ZString.Empty, specialHandlingItem.SpecialHandlingDescription);

			specialHandlingItem.EP_SpecialHandling = "IVD";
			AssertEquals("SpecialHandlingDescription should be empty when code is invalid", ZString.Empty, specialHandlingItem.SpecialHandlingDescription);

			specialHandlingItem.EP_SpecialHandling = AWBSpecialHandlingCodeDescriptionPairList.Codes.ConsignmentEstablishedWithAnElectronicallyConcludedCargoContractEccWithNoAccompanyingPaperAirWaybill;
			AssertEquals("SpecialHandlingDescription should show description for code", AWBSpecialHandlingCodeDescriptionPairList.Descriptions.ConsignmentEstablishedWithAnElectronicallyConcludedCargoContractEccWithNoAccompanyingPaperAirWaybill, specialHandlingItem.SpecialHandlingDescription);

			specialHandlingItem.EP_SpecialHandling = AWBSpecialHandlingCodeDescriptionPairList.Codes.ShippersDeclarationForDangerousGoods;
			AssertEquals("SpecialHandlingDescription should show description for code", AWBSpecialHandlingCodeDescriptionPairList.Descriptions.ShippersDeclarationForDangerousGoods, specialHandlingItem.SpecialHandlingDescription);

			specialHandlingItem.EP_SpecialHandling = AWBSpecialHandlingCodeDescriptionPairList.Codes.LithiumIonBatteriesExceptedAsPerSectionIiOfPi966And967;
			AssertEquals("SpecialHandlingDescription should show description for code", AWBSpecialHandlingCodeDescriptionPairList.Descriptions.LithiumIonBatteriesExceptedAsPerSectionIiOfPi966And967, specialHandlingItem.SpecialHandlingDescription);

			specialHandlingItem.EP_SpecialHandling = AWBSpecialHandlingCodeDescriptionPairList.Codes.LithiumMetalBatteriesExceptedAsPerSectionIiOfPi969And970;
			AssertEquals("SpecialHandlingDescription should show description for code", AWBSpecialHandlingCodeDescriptionPairList.Descriptions.LithiumMetalBatteriesExceptedAsPerSectionIiOfPi969And970, specialHandlingItem.SpecialHandlingDescription);

			specialHandlingItem.EP_SpecialHandling = AWBSpecialHandlingCodeDescriptionPairList.Codes.FullyRegulatedLithiumIonBatteriesClass9Un3481AsPerSectionIOfPi966And967AndWhereApplicableLithiumIonBatteriesShippedUnderAnApprovalInAccordanceWithSpecialProvisionA88OrA99;
			AssertEquals("SpecialHandlingDescription should show description for code", AWBSpecialHandlingCodeDescriptionPairList.Descriptions.FullyRegulatedLithiumIonBatteriesClass9Un3481AsPerSectionIOfPi966And967AndWhereApplicableLithiumIonBatteriesShippedUnderAnApprovalInAccordanceWithSpecialProvisionA88OrA99, specialHandlingItem.SpecialHandlingDescription);

			specialHandlingItem.EP_SpecialHandling = AWBSpecialHandlingCodeDescriptionPairList.Codes.FullyRegulatedLithiumMetalBatteriesClass9Un3091AsPerSectionIOfPi969And970AndWhereApplicableLithiumMetalBatteriesShippedUnderAnApprovalInAccordanceWithSpecialProvisionA88OrA99;
			AssertEquals("SpecialHandlingDescription should show description for code", AWBSpecialHandlingCodeDescriptionPairList.Descriptions.FullyRegulatedLithiumMetalBatteriesClass9Un3091AsPerSectionIOfPi969And970AndWhereApplicableLithiumMetalBatteriesShippedUnderAnApprovalInAccordanceWithSpecialProvisionA88OrA99, specialHandlingItem.SpecialHandlingDescription);

			specialHandlingItem.EP_SpecialHandling = AWBSpecialHandlingCodeDescriptionPairList.Codes.ControlRoomTemperaturePlus15CToPlus25C;
			AssertEquals("SpecialHandlingDescription should show description for code", AWBSpecialHandlingCodeDescriptionPairList.Descriptions.ControlRoomTemperaturePlus15CToPlus25C, specialHandlingItem.SpecialHandlingDescription);

			specialHandlingItem.EP_SpecialHandling = AWBSpecialHandlingCodeDescriptionPairList.Codes.CargoMayBeLoadedInThePassengerCabin;
			AssertEquals("SpecialHandlingDescription should show description for code", AWBSpecialHandlingCodeDescriptionPairList.Descriptions.CargoMayBeLoadedInThePassengerCabin, specialHandlingItem.SpecialHandlingDescription);

			specialHandlingItem.EP_SpecialHandling = AWBSpecialHandlingCodeDescriptionPairList.Codes.NonStackableCargo;
			AssertEquals("SpecialHandlingDescription should show description for code", AWBSpecialHandlingCodeDescriptionPairList.Descriptions.NonStackableCargo, specialHandlingItem.SpecialHandlingDescription);
		}

		public void TestSpecialHandlingCheckForSecurityStatus()
		{
			string[] securityStatuses = new string[]
			{
				AWBSpecialHandlingCodeDescriptionPairList.Codes.CargoHasNotBeenSecuredYetForPassengerOrAllCargoAircraft,
				AWBSpecialHandlingCodeDescriptionPairList.Codes.CargoSecureForPassengerAndAllCargoAircraft,
				AWBSpecialHandlingCodeDescriptionPairList.Codes.CargoSecureForAllCargoAircraftOnly,
				AWBSpecialHandlingCodeDescriptionPairList.Codes.SecureForPassengerAllCargoAndAllMailAircraftInAccordanceWithHighRiskRequirements
			};
			ExportAWBSpecialHandling specialHandlingItem = Factory.New<ExportAWBSpecialHandling>();

			foreach (CodeDescriptionPair handlingInfoPair in new AWBSpecialHandlingCodeDescriptionPairList())
			{
				specialHandlingItem.EP_SpecialHandling = handlingInfoPair.Code;
				AssertEquals("Special Handling is a security status.", securityStatuses.Contains(handlingInfoPair.Code), specialHandlingItem.IsSecurityStatus);
			}

			specialHandlingItem.EP_SpecialHandling = "";
			AssertEquals("Special Handling is not a security status.", false, specialHandlingItem.IsSecurityStatus);

			specialHandlingItem.EP_SpecialHandling = "XXX";
			AssertEquals("Special Handling is not a security status.", false, specialHandlingItem.IsSecurityStatus);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var header = factory.NewWithValidTestData<ExportAWBHeader>();
			var result = factory.NewWithValidTestData<ExportAWBSpecialHandling>();
			result.EP_EH = header.PK;

			return result;
		}
	}
}
