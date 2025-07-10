using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using NUnit.Framework;
using COSTCO = Enterprise.Customs.ZA.Business.ManifestTypeList.Codes;
using GOVGIO = Enterprise.Customs.ZA.Business.GateInOutMessageTypeCodeList.Codes;

namespace Enterprise.Customs.ZA.Business.Testing
{
	[TestedType(typeof(AsycudaPackValidation))]
	sealed class AsycudaPackValidationTest : BusinessObjectValidationTestCase
	{
		readonly string[] costcoCodes = new ManifestTypeList().GetAllCodes();
		readonly string[] govgioCodes = new GateInOutMessageTypeCodeList().GetAllCodes();

		public void TestValidateAll()
		{
			var pack = Factory.New<AsycudaPack>();
			var validation = new AsycudaPackValidation(pack);
			AssertNoExceptionThrown(validation.ValidateAll);
		}

		public void TestValidateAll_WhenTypeIsEmpty()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var pack = bill.Packs.AddNew();

			AssertNullOrEmpty("PRE-CONDITION", header.AMA_ManifestType);
			AssertNullOrEmpty("PRE-CONDITION", header.GateInOutMessageType);
			pack.Validation.ValidateAll();
			AssertEquals(0, pack.Notifications.Count());
		}

		public void TestCheckAPA_PackQty_WhenCOSTCO()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var pack = bill.Packs.AddNew();

			foreach (var code in costcoCodes)
			{
				header.AMA_ManifestType = code;

				pack.APA_PackQty = ZInt.Zero;
				AssertHasMessageErrorContaining(code, pack.APA_PackQtyInfo, MandatoryValidation.YouHaveNotEntered);

				pack.APA_PackQty = 1000;
				AssertNoMessageErrors(code, pack.APA_PackQtyInfo);
			}
		}

		public void TestCheckAPA_PackQty_WhenGOVGIO()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var pack = bill.Packs.AddNew();

			foreach (var code in govgioCodes)
			{
				header.GateInOutMessageType = code;

				pack.APA_PackQty = ZInt.Zero;
				AssertHasMessageErrorContaining(code, pack.APA_PackQtyInfo, MandatoryValidation.YouHaveNotEntered);

				pack.APA_PackQty = 1000;
				AssertNoMessageErrors(code, pack.APA_PackQtyInfo);
			}
		}

		public void TestCheckAPA_PackUQ_WhenCOSTCO()
		{
			SetupPackUQList();
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var pack = bill.Packs.AddNew();

			foreach (var code in costcoCodes)
			{
				header.AMA_ManifestType = code;

				pack.APA_PackUQ = ZString.Empty;
				AssertHasMessageErrorContaining(code, pack.APA_PackUQInfo, MandatoryValidation.YouHaveNotEntered);

				pack.APA_PackUQ = "X";
				AssertHasMessageErrorContaining(code, pack.APA_PackUQInfo, ListValidation.InvalidCodeMessageError.ToString());

				pack.APA_PackUQ = pack.Lookups.PackUQList[0].Code;
				AssertNoMessageErrors(code, pack.APA_PackUQInfo);
			}
		}

		public void TestCheckAPA_PackUQ_WhenGOVGIO()
		{
			SetupPackUQList();
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var pack = bill.Packs.AddNew();

			foreach (var code in govgioCodes)
			{
				header.GateInOutMessageType = code;

				pack.APA_PackUQ = ZString.Empty;
				AssertNoMessageErrors(code, pack.APA_PackUQInfo);

				pack.APA_PackUQ = pack.Lookups.PackUQList[0].Code;
				AssertHasMessageErrorContaining(code, pack.APA_PackUQInfo, MandatoryValidation.DoNotEntered);
			}
		}

		public void TestCheckAPA_GoodsDescription_WhenCOSTCO()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var pack = bill.Packs.AddNew();

			foreach (var code in costcoCodes)
			{
				header.AMA_ManifestType = code;

				pack.APA_GoodsDescription = ZString.Empty;
				AssertHasMessageErrorContaining(code, pack.APA_GoodsDescriptionInfo, MandatoryValidation.YouHaveNotEntered);

				pack.APA_GoodsDescription = "Description";
				AssertNoMessageErrors(code, pack.APA_GoodsDescriptionInfo);
			}
		}

		public void TestCheckAPA_GoodsDescription_WhenGOVGIO()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var pack = bill.Packs.AddNew();

			foreach (var code in govgioCodes)
			{
				header.GateInOutMessageType = code;

				pack.APA_GoodsDescription = ZString.Empty;
				AssertNoMessageErrors(code, pack.APA_GoodsDescriptionInfo);

				pack.APA_GoodsDescription = "Description";
				AssertHasMessageErrorContaining(code, pack.APA_GoodsDescriptionInfo, MandatoryValidation.DoNotEntered);
			}
		}

		public void TestCheckAPA_Weight_WhenCOSTCO()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var pack = bill.Packs.AddNew();

			foreach (var code in costcoCodes)
			{
				header.AMA_ManifestType = code;

				if (code.In(COSTCO.VesselOutturnReport, COSTCO.AirExcessOutturnReport))
				{
					pack.APA_Weight = ZDecimal.Zero;
					AssertNoMessageErrors(code, pack.APA_WeightInfo);

					pack.APA_Weight = 1000;
					AssertHasMessageErrorContaining(code, pack.APA_WeightInfo, MandatoryValidation.DoNotEntered);
				}
				else
				{
					pack.APA_Weight = ZDecimal.Zero;
					AssertHasMessageErrorContaining(code, pack.APA_WeightInfo, MandatoryValidation.YouHaveNotEntered);

					pack.APA_Weight = 1000;
					AssertNoMessageErrors(code, pack.APA_WeightInfo);
				}
			}
		}

		public void TestCheckAPA_Weight_WhenGOVGIO()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var pack = bill.Packs.AddNew();

			foreach (var code in govgioCodes)
			{
				header.GateInOutMessageType = code;

				pack.APA_Weight = ZDecimal.Zero;
				AssertNoMessageErrors(code, pack.APA_WeightInfo);

				pack.APA_Weight = 1000;
				AssertHasMessageErrorContaining(code, pack.APA_WeightInfo, MandatoryValidation.DoNotEntered);
			}
		}

		public void TestCheckAPA_WeightUQ_WhenCOSTCO()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var pack = bill.Packs.AddNew();

			foreach (var code in costcoCodes)
			{
				header.AMA_ManifestType = code;

				if (code.In(COSTCO.VesselOutturnReport, COSTCO.AirExcessOutturnReport))
				{
					pack.APA_WeightUQ = ZString.Empty;
					AssertNoMessageErrors(code, pack.APA_WeightUQInfo);

					pack.APA_WeightUQ = "KG";
					AssertHasMessageErrorContaining(code, pack.APA_WeightUQInfo, MandatoryValidation.DoNotEntered);
				}
				else
				{
					pack.APA_WeightUQ = ZString.Empty;
					AssertHasMessageErrorContaining(code, pack.APA_WeightUQInfo, MandatoryValidation.YouHaveNotEntered);

					pack.APA_WeightUQ = "X";
					AssertHasMessageErrorContaining(code, pack.APA_WeightUQInfo, ListValidation.InvalidCodeMessageError.ToString());

					pack.APA_WeightUQ = "KG";
					AssertNoMessageErrors(code, pack.APA_WeightUQInfo);
				}
			}
		}

		public void TestCheckAPA_WeightUQ_WhenGOVGIO()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var pack = bill.Packs.AddNew();

			foreach (var code in govgioCodes)
			{
				header.GateInOutMessageType = code;

				pack.APA_WeightUQ = ZString.Empty;
				AssertNoMessageErrors(code, pack.APA_WeightUQInfo);

				pack.APA_WeightUQ = "KG";
				AssertHasMessageErrorContaining(code, pack.APA_WeightUQInfo, MandatoryValidation.DoNotEntered);
			}
		}

		public void TestCheckAPA_Volume_WhenCOSTCO()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var pack = bill.Packs.AddNew();

			foreach (var code in costcoCodes)
			{
				header.AMA_ManifestType = code;

				if (code.In(COSTCO.DepotOutturnReport, COSTCO.AirCargoOutturnReport))
				{
					pack.APA_Volume = ZDecimal.Zero;
					AssertNoMessageErrors(code, pack.APA_VolumeInfo);

					pack.APA_Volume = 1000;
					AssertNoMessageErrors(code, pack.APA_VolumeInfo);
				}
				else if (code == COSTCO.BulkBreakBulkOutturnReport)
				{
					header.AMA_ContainerMode = Core.Constants.ContainerModes.Bulk;
					pack.APA_Volume = ZDecimal.Zero;
					AssertNoMessageErrors(code, pack.APA_VolumeInfo);

					pack.APA_Volume = 1000;
					AssertHasMessageErrorContaining(code, pack.APA_VolumeInfo, MandatoryValidation.DoNotEntered);

					header.AMA_ContainerMode = Core.Constants.ContainerModes.Liquid;
					pack.APA_Volume = ZDecimal.Zero;
					AssertHasMessageErrorContaining(code, pack.APA_VolumeInfo, MandatoryValidation.YouHaveNotEntered);

					pack.APA_Volume = 1000;
					AssertNoMessageErrors(code, pack.APA_VolumeInfo);
				}
				else
				{
					pack.APA_Volume = ZDecimal.Zero;
					AssertNoMessageErrors(code, pack.APA_VolumeInfo);

					pack.APA_Volume = 1000;
					AssertHasMessageErrorContaining(code, pack.APA_VolumeInfo, MandatoryValidation.DoNotEntered);
				}
			}
		}

		public void TestCheckAPA_Volume_WhenGOVGIO()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var pack = bill.Packs.AddNew();

			foreach (var code in govgioCodes)
			{
				header.GateInOutMessageType = code;

				pack.APA_Volume = ZDecimal.Zero;
				AssertNoMessageErrors(code, pack.APA_VolumeInfo);

				pack.APA_Volume = 1000;
				AssertHasMessageErrorContaining(code, pack.APA_VolumeInfo, MandatoryValidation.DoNotEntered);
			}
		}

		public void TestCheckAPA_VolumeUQ_WhenCOSTCO()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var pack = bill.Packs.AddNew();

			foreach (var code in costcoCodes)
			{
				header.AMA_ManifestType = code;

				if (code.In(COSTCO.DepotOutturnReport, COSTCO.AirCargoOutturnReport))
				{
					pack.APA_VolumeUQ = ZString.Empty;
					AssertNoMessageErrors(code, pack.APA_VolumeUQInfo);

					pack.APA_VolumeUQ = "L";
					AssertNoMessageErrors(code, pack.APA_VolumeUQInfo);
				}
				else if (code == COSTCO.BulkBreakBulkOutturnReport)
				{
					header.AMA_ContainerMode = Core.Constants.ContainerModes.Bulk;
					pack.APA_VolumeUQ = ZString.Empty;
					AssertNoMessageErrors(code, pack.APA_VolumeUQInfo);

					pack.APA_VolumeUQ = "L";
					AssertHasMessageErrorContaining(code, pack.APA_VolumeUQInfo, MandatoryValidation.DoNotEntered);

					header.AMA_ContainerMode = Core.Constants.ContainerModes.Liquid;
					pack.APA_VolumeUQ = ZString.Empty;
					AssertHasMessageErrorContaining(code, pack.APA_VolumeUQInfo, MandatoryValidation.YouHaveNotEntered);

					pack.APA_VolumeUQ = "L";
					AssertNoMessageErrors(code, pack.APA_VolumeUQInfo);
				}
				else
				{
					pack.APA_VolumeUQ = ZString.Empty;
					AssertNoMessageErrors(code, pack.APA_VolumeUQInfo);

					pack.APA_VolumeUQ = "L";
					AssertHasMessageErrorContaining(code, pack.APA_VolumeUQInfo, MandatoryValidation.DoNotEntered);
				}
			}
		}

		public void TestCheckAPA_VolumeUQ_WhenGOVGIO()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var pack = bill.Packs.AddNew();

			foreach (var code in govgioCodes)
			{
				header.GateInOutMessageType = code;

				pack.APA_VolumeUQ = ZString.Empty;
				AssertNoMessageErrors(code, pack.APA_VolumeUQInfo);

				pack.APA_VolumeUQ = "L";
				AssertHasMessageErrorContaining(code, pack.APA_VolumeUQInfo, MandatoryValidation.DoNotEntered);
			}
		}

		public void TestCheckAPA_MarksAndNumbers_WhenCOSTCO()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var pack = bill.Packs.AddNew();

			foreach (var code in costcoCodes)
			{
				header.AMA_ManifestType = code;

				pack.APA_MarksAndNumbers = ZString.Empty;
				AssertHasMessageErrorContaining(code, pack.APA_MarksAndNumbersInfo, "Marks and numbers are required. Use NA if not available.");

				pack.APA_MarksAndNumbers = "Test";
				AssertNoMessageErrors(code, pack.APA_MarksAndNumbersInfo);
			}
		}

		public void TestCheckAPA_MarksAndNumbers_WhenGOVGIO()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var pack = bill.Packs.AddNew();

			foreach (var code in govgioCodes)
			{
				header.GateInOutMessageType = code;

				pack.APA_MarksAndNumbers = ZString.Empty;
				AssertNoMessageErrors(code, pack.APA_MarksAndNumbersInfo);

				pack.APA_MarksAndNumbers = "Test";
				AssertHasMessageErrorContaining(code, pack.APA_MarksAndNumbersInfo, MandatoryValidation.DoNotEntered);
			}
		}

		public void TestCheckContainerPK_WhenCOSTCO()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var pack = bill.Packs.AddNew();
			var container = header.Containers.AddNew();

			foreach (var code in costcoCodes)
			{
				header.AMA_ManifestType = code;

				if (code == COSTCO.BulkBreakBulkOutturnReport)
				{
					pack.ContainerPK = ZGuid.Empty;
					AssertNoMessageErrors(code, pack.ContainerPKInfo);

					pack.ContainerPK = container.PK;
					AssertHasMessageErrorContaining(code, pack.ContainerPKInfo, MandatoryValidation.DoNotEntered);
				}
				else
				{
					header.AMA_ContainerMode = Core.Constants.ContainerModes.Bulk;
					pack.ContainerPK = ZGuid.Empty;
					AssertNoMessageErrors(code, pack.ContainerPKInfo);

					pack.ContainerPK = container.PK;
					AssertHasMessageErrorContaining(code, pack.ContainerPKInfo, MandatoryValidation.DoNotEntered);

					header.AMA_ContainerMode = Core.Constants.ContainerModes.Containerised;
					pack.ContainerPK = ZGuid.Empty;
					AssertHasMessageErrorContaining(code, pack.ContainerPKInfo, MandatoryValidation.YouHaveNotEntered);

					pack.ContainerPK = ZGuid.BrettsGuid;
					AssertHasErrorContaining(code, pack.ContainerPKInfo, ListValidation.InvalidCodeError);

					pack.ContainerPK = container.PK;
					AssertNoMessageErrors(code, pack.ContainerPKInfo);
				}
			}
		}

		public void TestCheckContainerPK_WhenGOVGIO()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var pack = bill.Packs.AddNew();
			var container = header.Containers.AddNew();

			foreach (var code in govgioCodes)
			{
				header.GateInOutMessageType = code;

				if (code == GOVGIO.BreakBulkGateIn)
				{
					pack.ContainerPK = ZGuid.Empty;
					AssertHasMessageErrorContaining(code, pack.ContainerPKInfo, MandatoryValidation.YouHaveNotEntered);

					pack.ContainerPK = ZGuid.BrettsGuid;
					AssertHasErrorContaining(code, pack.ContainerPKInfo, ListValidation.InvalidCodeError);

					pack.ContainerPK = container.PK;
					AssertNoMessageErrors(code, pack.ContainerPKInfo);
				}
				else
				{
					pack.Outturn.C5_CargoType = CargoTypeList.Codes.BreakBulk;
					pack.ContainerPK = ZGuid.Empty;
					AssertNoMessageErrors(code, pack.ContainerPKInfo);

					pack.ContainerPK = container.PK;
					AssertHasMessageErrorContaining(code, pack.ContainerPKInfo, MandatoryValidation.DoNotEntered);

					pack.Outturn.C5_CargoType = CargoTypeList.Codes.Container;
					pack.ContainerPK = ZGuid.Empty;
					AssertHasMessageErrorContaining(code, pack.ContainerPKInfo, MandatoryValidation.YouHaveNotEntered);

					pack.ContainerPK = ZGuid.BrettsGuid;
					AssertHasErrorContaining(code, pack.ContainerPKInfo, ListValidation.InvalidCodeError);

					pack.ContainerPK = container.PK;
					AssertNoMessageErrors(code, pack.ContainerPKInfo);
				}
			}
		}

		void SetupPackUQList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			_ = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes,
				"BG", "BG Description", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();
		}
	}
}
