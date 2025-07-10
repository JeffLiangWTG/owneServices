using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using COSTCO = Enterprise.Customs.ZA.Business.ManifestTypeList.Codes;
using GOVGIO = Enterprise.Customs.ZA.Business.GateInOutMessageTypeCodeList.Codes;

namespace Enterprise.Customs.ZA.Business.Testing
{
	[TestedType(typeof(AsycudaContainerValidation))]
	sealed class AsycudaContainerValidationTest : BusinessObjectValidationTestCase
	{
		readonly string[] costcoCodes = new ManifestTypeList().GetAllCodes();
		readonly string[] govgioCodes = new GateInOutMessageTypeCodeList().GetAllCodes();

		public void TestValidateAll()
		{
			var container = Factory.New<AsycudaContainer>();
			var validation = new AsycudaContainerValidation(container);
			AssertNoExceptionThrown(validation.ValidateAll);
		}

		public void TestValidateAll_WhenTypeIsEmpty()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var container = header.Containers.AddNew();

			AssertNullOrEmpty("PRE-CONDITION", header.AMA_ManifestType);
			AssertNullOrEmpty("PRE-CONDITION", header.GateInOutMessageType);
			container.Validation.ValidateAll();
			AssertEquals(0, container.Notifications.Count());
		}

		public void TestCheckACN_ContainerNumber_WhenCOSTCO()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var container = header.Containers.AddNew();

			foreach (var code in costcoCodes)
			{
				header.AMA_ManifestType = code;

				if (code == COSTCO.DepotOutturnReport)
				{
					container.ACN_ContainerNumber = ZString.Empty;
					AssertHasMessageErrorContaining(code, container.ACN_ContainerNumberInfo, MandatoryValidation.YouHaveNotEntered);

					container.ACN_ContainerNumber = "12345";
					AssertNoMessageErrors(code, container.ACN_ContainerNumberInfo);
				}
				else if (code == COSTCO.BulkBreakBulkOutturnReport)
				{
					container.ACN_ContainerNumber = ZString.Empty;
					AssertNoMessageErrors(code, container.ACN_ContainerNumberInfo);

					container.ACN_ContainerNumber = "12345";
					AssertHasMessageErrorContaining(code, container.ACN_ContainerNumberInfo, MandatoryValidation.DoNotEntered);
				}
				else if (code == COSTCO.VesselOutturnReport)
				{
					header.AMA_ContainerMode = Core.Constants.ContainerModes.Liquid;
					container.ACN_ContainerNumber = ZString.Empty;
					AssertNoMessageErrors(code, container.ACN_ContainerNumberInfo);

					container.ACN_ContainerNumber = "12345";
					AssertHasMessageErrorContaining(code, container.ACN_ContainerNumberInfo, MandatoryValidation.DoNotEntered);

					header.AMA_ContainerMode = Core.Constants.ContainerModes.Containerised;
					container.ACN_ContainerNumber = ZString.Empty;
					AssertHasMessageErrorContaining(code, container.ACN_ContainerNumberInfo, MandatoryValidation.YouHaveNotEntered);

					container.ACN_ContainerNumber = "12345";
					AssertNoMessageErrors(code, container.ACN_ContainerNumberInfo);
				}
				else
				{
					container.ACN_ContainerNumber = ZString.Empty;
					AssertNoMessageErrors(code, container.ACN_ContainerNumberInfo);

					container.ACN_ContainerNumber = "12345";
					AssertNoMessageErrors(code, container.ACN_ContainerNumberInfo);
				}
			}
		}

		public void TestCheckACN_ContainerNumber_WhenGOVGIO()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var container = header.Containers.AddNew();

			foreach (var code in govgioCodes)
			{
				header.GateInOutMessageType = code;

				if (code.In(GOVGIO.AirTerminalGateIn, GOVGIO.AirDepotGateIn))
				{
					container.ACN_ContainerNumber = ZString.Empty;
					AssertNoMessageErrors(code, container.ACN_ContainerNumberInfo);

					container.ACN_ContainerNumber = "12345";
					AssertNoMessageErrors(code, container.ACN_ContainerNumberInfo);
				}
				else
				{
					container.ACN_ContainerNumber = ZString.Empty;
					AssertHasMessageErrorContaining(code, container.ACN_ContainerNumberInfo, MandatoryValidation.YouHaveNotEntered);

					container.ACN_ContainerNumber = "12345";
					AssertNoMessageErrors(code, container.ACN_ContainerNumberInfo);
				}
			}
		}

		public void TestCheckACN_RC_ContainerType_WhenCOSTCO()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var container = header.Containers.AddNew();
			var containerTypePK = Factory.LoadTop1<RefContainer>(new ZQuery()).PK;

			foreach (var code in costcoCodes)
			{
				header.AMA_ManifestType = code;

				if (code == COSTCO.DepotOutturnReport)
				{
					container.ACN_RC_ContainerType = ZGuid.Empty;
					AssertHasMessageErrorContaining(code, container.ACN_RC_ContainerTypeInfo, MandatoryValidation.YouHaveNotEntered);

					container.ACN_RC_ContainerType = ZGuid.BrettsGuid;
					AssertHasErrorContaining(code, container.ACN_RC_ContainerTypeInfo, ListValidation.InvalidCodeError);

					container.ACN_RC_ContainerType = containerTypePK;
					AssertNoMessageErrors(code, container.ACN_RC_ContainerTypeInfo);
				}
				else
				{
					container.ACN_RC_ContainerType = ZGuid.Empty;
					AssertNoMessageErrors(code, container.ACN_RC_ContainerTypeInfo);

					container.ACN_RC_ContainerType = containerTypePK;
					AssertHasMessageErrorContaining(code, container.ACN_RC_ContainerTypeInfo, MandatoryValidation.DoNotEntered);
				}
			}
		}

		public void TestCheckACN_RC_ContainerType_WhenGOVGIO()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var container = header.Containers.AddNew();
			var containerTypePK = Factory.LoadTop1<RefContainer>(new ZQuery()).PK;

			foreach (var code in govgioCodes)
			{
				header.GateInOutMessageType = code;

				if (code.In(GOVGIO.TerminalGateOut, GOVGIO.TerminalGateIn, GOVGIO.DepotGateIn, GOVGIO.DepotGateOut))
				{
					container.ACN_RC_ContainerType = ZGuid.Empty;
					AssertNoMessageErrors(code, container.ACN_RC_ContainerTypeInfo);

					container.ACN_RC_ContainerType = containerTypePK;
					AssertNoMessageErrors(code, container.ACN_RC_ContainerTypeInfo);
				}
				else
				{
					container.ACN_RC_ContainerType = ZGuid.Empty;
					AssertNoMessageErrors(code, container.ACN_RC_ContainerTypeInfo);

					container.ACN_RC_ContainerType = containerTypePK;
					AssertHasMessageErrorContaining(code, container.ACN_RC_ContainerTypeInfo, MandatoryValidation.DoNotEntered);
				}
			}
		}

		public void TestCheckACN_EmptyFullIndicator_WhenCOSTCO()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var container = header.Containers.AddNew();

			foreach (var code in costcoCodes)
			{
				header.AMA_ManifestType = code;

				if (code == COSTCO.DepotOutturnReport)
				{
					container.ACN_EmptyFullIndicator = ZString.Empty;
					AssertHasMessageErrorContaining(code, container.ACN_EmptyFullIndicatorInfo, MandatoryValidation.YouHaveNotEntered);

					container.ACN_EmptyFullIndicator = "X";
					AssertHasMessageErrorContaining(code, container.ACN_EmptyFullIndicatorInfo, ListValidation.InvalidCodeMessageError.ToString());

					container.ACN_EmptyFullIndicator = EmptyFullList.Codes.EmptyContainer;
					AssertNoMessageErrors(code, container.ACN_EmptyFullIndicatorInfo);
				}
				else
				{
					container.ACN_EmptyFullIndicator = ZString.Empty;
					AssertNoMessageErrors(code, container.ACN_EmptyFullIndicatorInfo);

					container.ACN_EmptyFullIndicator = EmptyFullList.Codes.EmptyContainer;
					AssertHasMessageErrorContaining(code, container.ACN_EmptyFullIndicatorInfo, MandatoryValidation.DoNotEntered);
				}
			}
		}

		public void TestCheckACN_EmptyFullIndicator_WhenGOVGIO()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var container = header.Containers.AddNew();

			foreach (var code in govgioCodes)
			{
				header.GateInOutMessageType = code;

				if (code.In(GOVGIO.TerminalGateOut, GOVGIO.TerminalGateIn, GOVGIO.DepotGateIn, GOVGIO.DepotGateOut))
				{
					container.ACN_EmptyFullIndicator = ZString.Empty;
					AssertHasMessageErrorContaining(code, container.ACN_EmptyFullIndicatorInfo, MandatoryValidation.YouHaveNotEntered);

					container.ACN_EmptyFullIndicator = "X";
					AssertHasMessageErrorContaining(code, container.ACN_EmptyFullIndicatorInfo, ListValidation.InvalidCodeMessageError.ToString());

					container.ACN_EmptyFullIndicator = EmptyFullList.Codes.EmptyContainer;
					AssertNoMessageErrors(code, container.ACN_EmptyFullIndicatorInfo);
				}
				else
				{
					container.ACN_EmptyFullIndicator = ZString.Empty;
					AssertNoMessageErrors(code, container.ACN_EmptyFullIndicatorInfo);

					container.ACN_EmptyFullIndicator = EmptyFullList.Codes.EmptyContainer;
					AssertHasMessageErrorContaining(code, container.ACN_EmptyFullIndicatorInfo, MandatoryValidation.DoNotEntered);
				}
			}
		}

		public void TestCheckContUnpackTime_WhenCOSTCO()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var container = header.Containers.AddNew();

			foreach (var code in costcoCodes)
			{
				header.AMA_ManifestType = code;

				if (code.In(COSTCO.DepotOutturnReport, COSTCO.VesselOutturnReport, COSTCO.AirExcessOutturnReport))
				{
					header.AMA_ContainerMode = Core.Constants.ContainerModes.Liquid;
					container.ContUnpackTime = ZDateTime.Empty;
					AssertNoMessageErrors(code, container.ContUnpackTimeInfo);

					container.ContUnpackTime = ZDateTime.Today;
					AssertHasMessageErrorContaining(code, container.ContUnpackTimeInfo, MandatoryValidation.DoNotEntered);

					header.AMA_ContainerMode = Core.Constants.ContainerModes.Containerised;
					container.ContUnpackTime = ZDateTime.Empty;
					AssertHasMessageErrorContaining(code, container.ContUnpackTimeInfo, MandatoryValidation.YouHaveNotEntered);

					container.ContUnpackTime = ZDateTime.Today;
					AssertNoMessageErrors(code, container.ContUnpackTimeInfo);
				}
				else
				{
					container.ContUnpackTime = ZDateTime.Empty;
					AssertNoMessageErrors(code, container.ContUnpackTimeInfo);

					container.ContUnpackTime = ZDateTime.Today;
					AssertHasMessageErrorContaining(code, container.ContUnpackTimeInfo, MandatoryValidation.DoNotEntered);
				}
			}
		}

		public void TestCheckContUnpackTime_WhenGOVGIO()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var container = header.Containers.AddNew();

			foreach (var code in govgioCodes)
			{
				header.GateInOutMessageType = code;

				container.ContUnpackTime = ZDateTime.Empty;
				AssertNoMessageErrors(code, container.ContUnpackTimeInfo);

				container.ContUnpackTime = ZDateTime.Today;
				AssertHasMessageErrorContaining(code, container.ContUnpackTimeInfo, MandatoryValidation.DoNotEntered);
			}
		}

		public void TestCheckACN_Seal1_WhenCOSTCO()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var container = header.Containers.AddNew();

			foreach (var code in costcoCodes)
			{
				header.AMA_ManifestType = code;

				if (code.In(COSTCO.DepotOutturnReport, COSTCO.VesselOutturnReport, COSTCO.AirLoadDischarge))
				{
					header.AMA_ContainerMode = Core.Constants.ContainerModes.Liquid;
					container.ACN_Seal1 = ZString.Empty;
					AssertNoMessageErrors(code, container.ACN_Seal1Info);

					container.ACN_Seal1 = "12345";
					AssertHasMessageErrorContaining(code, container.ACN_Seal1Info, MandatoryValidation.DoNotEntered);

					header.AMA_ContainerMode = Core.Constants.ContainerModes.Containerised;
					container.ACN_Seal1 = ZString.Empty;
					AssertHasMessageErrorContaining(code, container.ACN_Seal1Info, MandatoryValidation.YouHaveNotEntered);

					container.ACN_Seal1 = "12345";
					AssertNoMessageErrors(code, container.ACN_Seal1Info);
				}
				else if (code == COSTCO.AirExcessOutturnReport)
				{
					container.ACN_Seal1 = ZString.Empty;
					AssertNoMessageErrors(code, container.ACN_Seal1Info);

					container.ACN_Seal1 = "12345";
					AssertNoMessageErrors(code, container.ACN_Seal1Info);
				}
				else
				{
					container.ACN_Seal1 = ZString.Empty;
					AssertNoMessageErrors(code, container.ACN_Seal1Info);

					container.ACN_Seal1 = "12345";
					AssertHasMessageErrorContaining(code, container.ACN_Seal1Info, MandatoryValidation.DoNotEntered);
				}
			}
		}

		public void TestCheckACN_Seal1_WhenGOVGIO()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var container = header.Containers.AddNew();

			foreach (var code in govgioCodes)
			{
				header.GateInOutMessageType = code;

				if (code.In(GOVGIO.SeaDepotConsignmentGateIn, GOVGIO.AirTerminalGateIn, GOVGIO.AirDepotGateIn))
				{
					container.ACN_Seal1 = ZString.Empty;
					AssertNoMessageErrors(code, container.ACN_Seal1Info);

					container.ACN_Seal1 = "12345";
					AssertHasMessageErrorContaining(code, container.ACN_Seal1Info, MandatoryValidation.DoNotEntered);
				}
				else
				{
					container.ACN_Seal1 = ZString.Empty;
					AssertHasMessageErrorContaining(code, container.ACN_Seal1Info, MandatoryValidation.YouHaveNotEntered);

					container.ACN_Seal1 = "12345";
					AssertNoMessageErrors(code, container.ACN_Seal1Info);
				}
			}
		}

		public void TestCheckACN_SealingPartyType_WhenCOSTCO()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var container = header.Containers.AddNew();

			foreach (var code in costcoCodes)
			{
				header.AMA_ManifestType = code;

				if (code.In(COSTCO.DepotOutturnReport, COSTCO.VesselOutturnReport, COSTCO.AirExcessOutturnReport))
				{
					container.ACN_SealingPartyType = ZString.Empty;
					AssertNoMessageErrors(code, container.ACN_SealingPartyTypeInfo);

					container.ACN_SealingPartyType = SealTypeList.Codes.AgentForwarder;
					AssertNoMessageErrors(code, container.ACN_SealingPartyTypeInfo);
				}
				else
				{
					container.ACN_SealingPartyType = ZString.Empty;
					AssertNoMessageErrors(code, container.ACN_SealingPartyTypeInfo);

					container.ACN_SealingPartyType = SealTypeList.Codes.AgentForwarder;
					AssertHasMessageErrorContaining(code, container.ACN_SealingPartyTypeInfo, MandatoryValidation.DoNotEntered);
				}
			}
		}

		public void TestCheckACN_SealingPartyType_WhenGOVGIO()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var container = header.Containers.AddNew();

			foreach (var code in govgioCodes)
			{
				header.GateInOutMessageType = code;

				if (code.In(GOVGIO.TerminalGateOut, GOVGIO.TerminalGateIn, GOVGIO.DepotGateIn, GOVGIO.DepotGateOut))
				{
					container.ACN_SealingPartyType = ZString.Empty;
					AssertNoMessageErrors(code, container.ACN_SealingPartyTypeInfo);

					container.ACN_SealingPartyType = SealTypeList.Codes.AgentForwarder;
					AssertNoMessageErrors(code, container.ACN_SealingPartyTypeInfo);
				}
				else
				{
					container.ACN_SealingPartyType = ZString.Empty;
					AssertNoMessageErrors(code, container.ACN_SealingPartyTypeInfo);

					container.ACN_SealingPartyType = SealTypeList.Codes.AgentForwarder;
					AssertHasMessageErrorContaining(code, container.ACN_SealingPartyTypeInfo, MandatoryValidation.DoNotEntered);
				}
			}
		}

		public void TestCheckGateInOutDate_WhenCOSTCO()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var container = header.Containers.AddNew();

			foreach (var code in costcoCodes)
			{
				header.AMA_ManifestType = code;

				container.GateInOutDate = ZDateTime.Empty;
				AssertNoMessageErrors(code, container.GateInOutDateInfo);

				container.GateInOutDate = ZDateTime.Today;
				AssertHasMessageErrorContaining(code, container.GateInOutDateInfo, MandatoryValidation.DoNotEntered);
			}
		}

		public void TestCheckGateInOutDate_WhenGOVGIO()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var container = header.Containers.AddNew();

			foreach (var code in govgioCodes)
			{
				header.GateInOutMessageType = code;

				container.GateInOutDate = ZDateTime.Empty;
				AssertHasMessageErrorContaining(code, container.GateInOutDateInfo, MandatoryValidation.YouHaveNotEntered);

				container.GateInOutDate = ZDateTime.Today;
				AssertNoMessageErrors(code, container.GateInOutDateInfo);
			}
		}

		public void TestCheckACN_ContainerNumber_OnlyOneContainerAllowed_WhenIsVOROrIsDOR()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_ContainerMode = Core.Constants.ContainerModes.Containerised;
			header.AMA_ManifestType = ManifestTypeList.Codes.VesselOutturnReport;
			var container1 = header.Containers.AddNew();
			var container2 = header.Containers.AddNew();
			container2.ACN_ContainerNumber = ZString.Empty;
			AssertHasMessageError(container2.ACN_ContainerNumberInfo, "Only one container allowed for 'VOR' outturn.");
			header.AMA_ManifestType = ManifestTypeList.Codes.DepotOutturnReport;
			container2.ACN_ContainerNumber = ZString.Empty;
			AssertNoMessageError(container2.ACN_ContainerNumberInfo, "Only one container allowed for 'VOR' outturn.");
			header.AMA_ManifestType = ManifestTypeList.Codes.AirCargoOutturnReport;
			container2.ACN_ContainerNumber = ZString.Empty;
			AssertNoMessageError(container2.ACN_ContainerNumberInfo, "Only one container allowed for 'VOR' outturn.");
		}

		public void TestParent()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var container = header.Containers.AddNew();
			AssertType<AsycudaContainer>(container.Validation.Parent);
		}
	}
}
