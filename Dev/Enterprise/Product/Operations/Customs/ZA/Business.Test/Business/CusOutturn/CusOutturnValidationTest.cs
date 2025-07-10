using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;
using COSTCO = Enterprise.Customs.ZA.Business.ManifestTypeList.Codes;

namespace Enterprise.Customs.ZA.Business.Testing
{
	[TestedType(typeof(CusOutturnValidation))]
	sealed class CusOutturnValidationTest : BusinessObjectValidationTestCase
	{
		readonly string[] costcoCodes = new ManifestTypeList().GetAllCodes();
		readonly string[] govgioCodes = new GateInOutMessageTypeCodeList().GetAllCodes();

		public void TestValidateAll()
		{
			var outturn = Factory.New<CusOutturn>();
			var validation = new CusOutturnValidation(outturn);
			AssertNoExceptionThrown(validation.ValidateAll);
		}

		public void TestValidateAll_WhenTypeIsEmpty()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var pack = bill.Packs.AddNew();

			AssertNullOrEmpty("PRE-CONDITION", header.AMA_ManifestType);
			AssertNullOrEmpty("PRE-CONDITION", header.GateInOutMessageType);
			pack.Outturn.Validation.ValidateAll();
			AssertEquals(0, pack.Outturn.Notifications.Count());
		}

		public void TestCheckC5_CargoType_WhenCOSTCO()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var pack = bill.Packs.AddNew();

			foreach (var code in costcoCodes)
			{
				header.AMA_ManifestType = code;

				pack.Outturn.C5_CargoType = ZString.Empty;
				AssertHasMessageErrorContaining(code, pack.Outturn.C5_CargoTypeInfo, MandatoryValidation.YouHaveNotEntered);

				pack.Outturn.C5_CargoType = "X";
				AssertHasMessageErrorContaining(code, pack.Outturn.C5_CargoTypeInfo, ListValidation.InvalidCodeMessageError.ToString());

				pack.Outturn.C5_CargoType = CargoTypeList.Codes.BreakBulk;
				AssertNoMessageErrors(code, pack.Outturn.C5_CargoTypeInfo);
			}
		}

		public void TestCheckC5_CargoType_WhenGOVGIO()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var pack = bill.Packs.AddNew();

			foreach (var code in govgioCodes)
			{
				header.GateInOutMessageType = code;

				pack.Outturn.C5_CargoType = ZString.Empty;
				AssertHasMessageErrorContaining(code, pack.Outturn.C5_CargoTypeInfo, MandatoryValidation.YouHaveNotEntered);

				pack.Outturn.C5_CargoType = "X";
				AssertHasMessageErrorContaining(code, pack.Outturn.C5_CargoTypeInfo, ListValidation.InvalidCodeMessageError.ToString());

				pack.Outturn.C5_CargoType = CargoTypeList.Codes.BreakBulk;
				AssertNoMessageErrors(code, pack.Outturn.C5_CargoTypeInfo);
			}
		}

		public void TestCheckC5_PackagesOutturned_WhenCOSTCO()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var pack = bill.Packs.AddNew();

			foreach (var code in costcoCodes)
			{
				header.AMA_ManifestType = code;

				pack.Outturn.C5_PackagesOutturned = ZInt.Zero;
				AssertHasMessageErrorContaining(code, pack.Outturn.C5_PackagesOutturnedInfo, MandatoryValidation.YouHaveNotEntered);

				pack.Outturn.C5_PackagesOutturned = 1000;
				AssertNoMessageErrors(code, pack.Outturn.C5_PackagesOutturnedInfo);
			}
		}

		public void TestCheckC5_PackagesOutturned_WhenGOVGIO()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var pack = bill.Packs.AddNew();

			foreach (var code in govgioCodes)
			{
				header.GateInOutMessageType = code;

				pack.Outturn.C5_PackagesOutturned = ZInt.Zero;
				AssertNoMessageErrors(code, pack.Outturn.C5_PackagesOutturnedInfo);

				pack.Outturn.C5_PackagesOutturned = 1000;
				AssertHasMessageErrorContaining(code, pack.Outturn.C5_PackagesOutturnedInfo, MandatoryValidation.DoNotEntered);
			}
		}

		public void TestCheckC5_PackageCondition_WhenCOSTCO()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var pack = bill.Packs.AddNew();

			foreach (var code in costcoCodes)
			{
				header.AMA_ManifestType = code;

				pack.Outturn.C5_PackageCondition = ZString.Empty;
				AssertNoMessageErrors(code, pack.Outturn.C5_PackageConditionInfo);

				pack.Outturn.C5_PackageCondition = "1";
				AssertNoMessageErrors(code, pack.Outturn.C5_PackageConditionInfo);
			}
		}

		public void TestCheckC5_PackageCondition_WhenGOVGIO()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var pack = bill.Packs.AddNew();

			foreach (var code in govgioCodes)
			{
				header.GateInOutMessageType = code;

				pack.Outturn.C5_PackageCondition = ZString.Empty;
				AssertNoMessageErrors(code, pack.Outturn.C5_PackageConditionInfo);

				pack.Outturn.C5_PackageCondition = "1";
				AssertHasMessageErrorContaining(code, pack.Outturn.C5_PackageConditionInfo, MandatoryValidation.DoNotEntered);
			}
		}

		public void TestCheckPackCondDesc_WhenCOSTCO()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var pack = bill.Packs.AddNew();

			foreach (var code in costcoCodes)
			{
				header.AMA_ManifestType = code;

				pack.Outturn.C5_PackageCondition = "1";
				pack.Outturn.PackCondDesc = ZString.Empty;
				AssertNoMessageErrors(code, pack.Outturn.PackCondDescInfo);

				pack.Outturn.PackCondDesc = "Description";
				AssertHasMessageErrorContaining(code, pack.Outturn.PackCondDescInfo, MandatoryValidation.DoNotEntered);

				pack.Outturn.C5_PackageCondition = "4";
				pack.Outturn.PackCondDesc = ZString.Empty;
				AssertHasMessageErrorContaining(code, pack.Outturn.PackCondDescInfo, MandatoryValidation.YouHaveNotEntered);

				pack.Outturn.PackCondDesc = "Description";
				AssertNoMessageErrors(code, pack.Outturn.PackCondDescInfo);
			}
		}

		public void TestCheckPackCondDesc_WhenGOVGIO()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var pack = bill.Packs.AddNew();

			foreach (var code in govgioCodes)
			{
				header.GateInOutMessageType = code;

				pack.Outturn.PackCondDesc = ZString.Empty;
				AssertNoMessageErrors(code, pack.Outturn.PackCondDescInfo);

				pack.Outturn.PackCondDesc = "Description";
				AssertHasMessageErrorContaining(code, pack.Outturn.PackCondDescInfo, MandatoryValidation.DoNotEntered);
			}
		}

		public void TestCheckExcessShortInd_WhenCOSTCO()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var pack = bill.Packs.AddNew();

			foreach (var code in costcoCodes)
			{
				header.AMA_ManifestType = code;

				if (code.In(COSTCO.VesselOutturnReport, COSTCO.AirExcessOutturnReport))
				{
					pack.Outturn.ExcessShortInd = ZString.Empty;
					AssertNoMessageErrors(code, pack.Outturn.ExcessShortIndInfo);

					pack.Outturn.ExcessShortInd = "1";
					AssertHasMessageErrorContaining(code, pack.Outturn.ExcessShortIndInfo, MandatoryValidation.DoNotEntered);
				}
				else
				{
					pack.Outturn.ExcessShortInd = ZString.Empty;
					AssertHasMessageErrorContaining(code, pack.Outturn.ExcessShortIndInfo, MandatoryValidation.YouHaveNotEntered);

					pack.Outturn.ExcessShortInd = "X";
					AssertHasMessageErrorContaining(code, pack.Outturn.ExcessShortIndInfo, ListValidation.InvalidCodeMessageError.ToString());

					pack.Outturn.ExcessShortInd = "1";
					AssertNoMessageErrors(code, pack.Outturn.ExcessShortIndInfo);
				}
			}
		}

		public void TestCheckExcessShortInd_WhenGOVGIO()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var pack = bill.Packs.AddNew();

			foreach (var code in govgioCodes)
			{
				header.GateInOutMessageType = code;

				pack.Outturn.ExcessShortInd = ZString.Empty;
				AssertNoMessageErrors(code, pack.Outturn.ExcessShortIndInfo);

				pack.Outturn.ExcessShortInd = "1";
				AssertHasMessageErrorContaining(code, pack.Outturn.ExcessShortIndInfo, MandatoryValidation.DoNotEntered);
			}
		}

		public void TestCheckContShouldBe_WhenCOSTCO()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var pack = bill.Packs.AddNew();

			foreach (var code in costcoCodes)
			{
				header.AMA_ManifestType = code;

				if (code.In(COSTCO.VesselOutturnReport, COSTCO.AirExcessOutturnReport))
				{
					pack.Outturn.ContShouldBe = ZString.Empty;
					AssertNoMessageErrors(code, pack.Outturn.ContShouldBeInfo);

					pack.Outturn.ContShouldBe = "Content";
					AssertNoMessageErrors(code, pack.Outturn.ContShouldBeInfo);
				}
				else
				{
					pack.Outturn.ExcessShortInd = "1";
					pack.Outturn.ContShouldBe = ZString.Empty;
					AssertHasMessageErrorContaining(code, pack.Outturn.ContShouldBeInfo, MandatoryValidation.YouHaveNotEntered);

					pack.Outturn.ContShouldBe = "Content";
					AssertNoMessageErrors(code, pack.Outturn.ContShouldBeInfo);

					pack.Outturn.ExcessShortInd = "3";
					pack.Outturn.ContShouldBe = ZString.Empty;
					AssertNoMessageErrors(code, pack.Outturn.ContShouldBeInfo);

					pack.Outturn.ContShouldBe = "Content";
					AssertNoMessageErrors(code, pack.Outturn.ContShouldBeInfo);
				}
			}
		}

		public void TestCheckContShouldBe_WhenGOVGIO()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var pack = bill.Packs.AddNew();

			foreach (var code in govgioCodes)
			{
				header.GateInOutMessageType = code;

				pack.Outturn.ContShouldBe = ZString.Empty;
				AssertNoMessageErrors(code, pack.Outturn.ContShouldBeInfo);

				pack.Outturn.ContShouldBe = "Content";
				AssertHasMessageErrorContaining(code, pack.Outturn.ContShouldBeInfo, MandatoryValidation.DoNotEntered);
			}
		}

		public void TestCheckC5_WeightOutturned_WhenCOSTCO()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var pack = bill.Packs.AddNew();

			foreach (var code in costcoCodes)
			{
				header.AMA_ManifestType = code;

				pack.Outturn.C5_WeightOutturned = ZDecimal.Zero;
				AssertHasMessageErrorContaining(code, pack.Outturn.C5_WeightOutturnedInfo, MandatoryValidation.YouHaveNotEntered);

				pack.Outturn.C5_WeightOutturned = 1000;
				AssertNoMessageErrors(code, pack.Outturn.C5_WeightOutturnedInfo);
			}
		}

		public void TestCheckC5_WeightOutturned_WhenGOVGIO()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var pack = bill.Packs.AddNew();

			foreach (var code in govgioCodes)
			{
				header.GateInOutMessageType = code;

				pack.Outturn.C5_WeightOutturned = ZDecimal.Zero;
				AssertNoMessageErrors(code, pack.Outturn.C5_WeightOutturnedInfo);

				pack.Outturn.C5_WeightOutturned = 1000;
				AssertHasMessageErrorContaining(code, pack.Outturn.C5_WeightOutturnedInfo, MandatoryValidation.DoNotEntered);
			}
		}

		public void TestCheckC5_WeightOutturnedUQ_WhenCOSTCO()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var pack = bill.Packs.AddNew();

			foreach (var code in costcoCodes)
			{
				header.AMA_ManifestType = code;

				pack.Outturn.C5_WeightOutturnedUQ = ZString.Empty;
				AssertHasMessageErrorContaining(code, pack.Outturn.C5_WeightOutturnedUQInfo, MandatoryValidation.YouHaveNotEntered);

				pack.Outturn.C5_WeightOutturnedUQ = "X";
				AssertHasMessageErrorContaining(code, pack.Outturn.C5_WeightOutturnedUQInfo, ListValidation.InvalidCodeMessageError.ToString());

				pack.Outturn.C5_WeightOutturnedUQ = "KG";
				AssertNoMessageErrors(code, pack.Outturn.C5_WeightOutturnedUQInfo);
			}
		}

		public void TestCheckC5_WeightOutturnedUQ_WhenGOVGIO()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var pack = bill.Packs.AddNew();

			foreach (var code in govgioCodes)
			{
				header.GateInOutMessageType = code;

				pack.Outturn.C5_WeightOutturnedUQ = ZString.Empty;
				AssertNoMessageErrors(code, pack.Outturn.C5_WeightOutturnedUQInfo);

				pack.Outturn.C5_WeightOutturnedUQ = "KG";
				AssertHasMessageErrorContaining(code, pack.Outturn.C5_WeightOutturnedUQInfo, MandatoryValidation.DoNotEntered);
			}
		}

		public void TestCheckC5_VolumeOutturned_WhenCOSTCO()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var pack = bill.Packs.AddNew();

			foreach (var code in costcoCodes)
			{
				header.AMA_ManifestType = code;

				if (code.In(COSTCO.AirExcessOutturnReport, COSTCO.AirLoadDischarge))
				{
					pack.Outturn.C5_VolumeOutturned = ZDecimal.Zero;
					AssertNoMessageErrors(code, pack.Outturn.C5_VolumeOutturnedInfo);

					pack.Outturn.C5_VolumeOutturned = 1000;
					AssertHasMessageErrorContaining(code, pack.Outturn.C5_VolumeOutturnedInfo, MandatoryValidation.DoNotEntered);
				}
				else
				{
					header.AMA_ContainerMode = Core.Constants.ContainerModes.Bulk;
					pack.Outturn.C5_VolumeOutturned = ZDecimal.Zero;
					AssertNoMessageErrors(code, pack.Outturn.C5_VolumeOutturnedInfo);

					pack.Outturn.C5_VolumeOutturned = 1000;
					AssertHasMessageErrorContaining(code, pack.Outturn.C5_VolumeOutturnedInfo, MandatoryValidation.DoNotEntered);

					header.AMA_ContainerMode = Core.Constants.ContainerModes.Liquid;
					pack.Outturn.C5_VolumeOutturned = ZDecimal.Zero;
					AssertHasMessageErrorContaining(code, pack.Outturn.C5_VolumeOutturnedInfo, MandatoryValidation.YouHaveNotEntered);

					pack.Outturn.C5_VolumeOutturned = 1000;
					AssertNoMessageErrors(code, pack.Outturn.C5_VolumeOutturnedInfo);
				}
			}
		}

		public void TestCheckC5_VolumeOutturned_WhenGOVGIO()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var pack = bill.Packs.AddNew();

			foreach (var code in govgioCodes)
			{
				header.GateInOutMessageType = code;

				pack.Outturn.C5_VolumeOutturned = ZDecimal.Zero;
				AssertNoMessageErrors(code, pack.Outturn.C5_VolumeOutturnedInfo);

				pack.Outturn.C5_VolumeOutturned = 1000;
				AssertHasMessageErrorContaining(code, pack.Outturn.C5_VolumeOutturnedInfo, MandatoryValidation.DoNotEntered);
			}
		}

		public void TestCheckC5_VolumeOutturnedUQ_WhenCOSTCO()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var pack = bill.Packs.AddNew();

			foreach (var code in costcoCodes)
			{
				header.AMA_ManifestType = code;

				if (code.In(COSTCO.AirExcessOutturnReport, COSTCO.AirLoadDischarge))
				{
					pack.Outturn.C5_VolumeOutturnedUQ = ZString.Empty;
					AssertNoMessageErrors(code, pack.Outturn.C5_VolumeOutturnedUQInfo);

					pack.Outturn.C5_VolumeOutturnedUQ = "L";
					AssertHasMessageErrorContaining(code, pack.Outturn.C5_VolumeOutturnedUQInfo, MandatoryValidation.DoNotEntered);
				}
				else
				{
					header.AMA_ContainerMode = Core.Constants.ContainerModes.Bulk;
					pack.Outturn.C5_VolumeOutturnedUQ = ZString.Empty;
					AssertNoMessageErrors(code, pack.Outturn.C5_VolumeOutturnedUQInfo);

					pack.Outturn.C5_VolumeOutturnedUQ = "L";
					AssertHasMessageErrorContaining(code, pack.Outturn.C5_VolumeOutturnedUQInfo, MandatoryValidation.DoNotEntered);

					header.AMA_ContainerMode = Core.Constants.ContainerModes.Liquid;
					pack.Outturn.C5_VolumeOutturnedUQ = ZString.Empty;
					AssertHasMessageErrorContaining(code, pack.Outturn.C5_VolumeOutturnedUQInfo, MandatoryValidation.YouHaveNotEntered);

					pack.Outturn.C5_VolumeOutturnedUQ = "X";
					AssertHasMessageErrorContaining(code, pack.Outturn.C5_VolumeOutturnedUQInfo, ListValidation.InvalidCodeMessageError.ToString());

					pack.Outturn.C5_VolumeOutturnedUQ = "L";
					AssertNoMessageErrors(code, pack.Outturn.C5_VolumeOutturnedUQInfo);
				}
			}
		}

		public void TestCheckC5_VolumeOutturnedUQ_WhenGOVGIO()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var pack = bill.Packs.AddNew();

			foreach (var code in govgioCodes)
			{
				header.GateInOutMessageType = code;

				pack.Outturn.C5_VolumeOutturnedUQ = ZString.Empty;
				AssertNoMessageErrors(code, pack.Outturn.C5_VolumeOutturnedUQInfo);

				pack.Outturn.C5_VolumeOutturnedUQ = "L";
				AssertHasMessageErrorContaining(code, pack.Outturn.C5_VolumeOutturnedUQInfo, MandatoryValidation.DoNotEntered);
			}
		}

		public void TesCheckC5_GoodsDescription_WhenCOSTCO()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var pack = bill.Packs.AddNew();

			foreach (var code in costcoCodes)
			{
				header.AMA_ManifestType = code;

				if (code.In(COSTCO.DepotOutturnReport, COSTCO.BulkBreakBulkOutturnReport, COSTCO.AirCargoOutturnReport))
				{
					pack.Outturn.ContShouldBe = ZString.Empty;
					pack.Outturn.C5_GoodsDescription = ZString.Empty;
					AssertNoMessageErrors(code, pack.Outturn.C5_GoodsDescriptionInfo);

					pack.Outturn.ContShouldBe = ZString.Empty;
					pack.Outturn.C5_GoodsDescription = "Content";
					AssertHasMessageErrorContaining(code, pack.Outturn.C5_GoodsDescriptionInfo, MandatoryValidation.DoNotEntered);

					pack.Outturn.ContShouldBe = "Content";
					pack.Outturn.C5_GoodsDescription = ZString.Empty;
					AssertNoMessageErrors(code, pack.Outturn.ContShouldBeInfo);

					pack.Outturn.ContShouldBe = "Content";
					pack.Outturn.C5_GoodsDescription = "Content";
					AssertNoMessageErrors(code, pack.Outturn.C5_GoodsDescriptionInfo);
				}
				else
				{
					pack.Outturn.C5_GoodsDescription = ZString.Empty;
					AssertNoMessageErrors(code, pack.Outturn.C5_GoodsDescriptionInfo);

					pack.Outturn.C5_GoodsDescription = "Content";
					AssertNoMessageErrors(code, pack.Outturn.C5_GoodsDescriptionInfo);
				}
			}
		}

		public void TestCheckC5_GoodsDescription_WhenGOVGIO()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var pack = bill.Packs.AddNew();

			foreach (var code in govgioCodes)
			{
				header.GateInOutMessageType = code;

				if (code.In(COSTCO.DepotOutturnReport, COSTCO.BulkBreakBulkOutturnReport, COSTCO.AirCargoOutturnReport))
				{
					pack.Outturn.ExcessShortInd = "3";
					pack.Outturn.C5_GoodsDescription = ZString.Empty;
					AssertNoMessageErrors(code, pack.Outturn.C5_GoodsDescriptionInfo);

					pack.Outturn.C5_GoodsDescription = "Description";
					AssertHasMessageErrorContaining(code, pack.Outturn.C5_GoodsDescriptionInfo, MandatoryValidation.DoNotEntered);

					pack.Outturn.ExcessShortInd = "1";
					pack.Outturn.C5_GoodsDescription = ZString.Empty;
					AssertHasMessageErrorContaining(code, pack.Outturn.C5_GoodsDescriptionInfo, MandatoryValidation.YouHaveNotEntered);

					pack.Outturn.C5_GoodsDescription = "Description";
					AssertNoMessageErrors(code, pack.Outturn.C5_GoodsDescriptionInfo);
				}
				else
				{
					pack.Outturn.C5_GoodsDescription = ZString.Empty;
					AssertNoMessageErrors(code, pack.Outturn.C5_GoodsDescriptionInfo);

					pack.Outturn.C5_GoodsDescription = "Description";
					AssertHasMessageErrorContaining(code, pack.Outturn.C5_GoodsDescriptionInfo, MandatoryValidation.DoNotEntered);
				}

				pack.Outturn.C5_GoodsDescription = ZString.Empty;
				AssertNoMessageErrors(code, pack.Outturn.C5_GoodsDescriptionInfo);

				pack.Outturn.C5_GoodsDescription = "Description";
				AssertHasMessageErrorContaining(code, pack.Outturn.C5_GoodsDescriptionInfo, MandatoryValidation.DoNotEntered);
			}
		}

		public void TestParent()
		{
			var outturn = Factory.New<CusOutturn>();
			AssertType<CusOutturn>(outturn.Validation.Parent);
		}

		public void TestSetActuallyFoundToBeProperties_WhenExcessShortIndChangeAndCOSTCO()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var pack = bill.Packs.AddNew();
			pack.Outturn.ContShouldBe = "Content";
			pack.APA_Weight = 1000;
			pack.APA_WeightUQ = "KG";
			pack.APA_Volume = 2000;
			pack.APA_VolumeUQ = "L";
			pack.APA_PackQty = 3000;

			foreach (var manifestType in new ManifestTypeList().GetAllCodes())
			{
				header.AMA_ManifestType = manifestType;

				foreach (var excessShortInd in new ExcessShortIndicatorList().GetAllCodes())
				{
					pack.Outturn.C5_GoodsDescription = ZString.Empty;
					pack.Outturn.C5_WeightOutturned = ZDecimal.Zero;
					pack.Outturn.C5_WeightOutturnedUQ = ZString.Empty;
					pack.Outturn.C5_VolumeOutturned = ZDecimal.Zero;
					pack.Outturn.C5_VolumeOutturnedUQ = ZString.Empty;
					pack.Outturn.C5_PackagesOutturned = ZInt.Zero;
					pack.Outturn.ExcessShortInd = excessShortInd;

					AssertActuallyFoundToBeProperties(pack, excessShortInd == "3");
				}
			}
		}

		public void TestSetActuallyFoundToBeProperties_WhenExcessShortIndAlreadySetAndCOSTCO()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var pack = bill.Packs.AddNew();

			foreach (var manifestType in new ManifestTypeList().GetAllCodes())
			{
				header.AMA_ManifestType = manifestType;

				foreach (var excessShortInd in new ExcessShortIndicatorList().GetAllCodes())
				{
					pack.Outturn.C5_GoodsDescription = ZString.Empty;
					pack.Outturn.C5_WeightOutturned = ZDecimal.Zero;
					pack.Outturn.C5_WeightOutturnedUQ = ZString.Empty;
					pack.Outturn.C5_VolumeOutturned = ZDecimal.Zero;
					pack.Outturn.C5_VolumeOutturnedUQ = ZString.Empty;
					pack.Outturn.C5_PackagesOutturned = ZInt.Zero;
					pack.Outturn.ContShouldBe = ZString.Empty;
					pack.APA_Weight = ZDecimal.Zero;
					pack.APA_WeightUQ = ZString.Empty;
					pack.APA_Volume = ZDecimal.Zero;
					pack.APA_VolumeUQ = ZString.Empty;
					pack.APA_PackQty = ZInt.Zero;
					pack.APA_PackUQ = ZString.Empty;
					pack.Outturn.ExcessShortInd = excessShortInd;
					pack.Outturn.ContShouldBe = "Content";
					pack.APA_Weight = 1000;
					pack.APA_WeightUQ = "KG";
					pack.APA_Volume = 2000;
					pack.APA_VolumeUQ = "L";
					pack.APA_PackQty = 3000;

					AssertActuallyFoundToBeProperties(pack, excessShortInd == "3");
				}
			}
		}

		public void TestSetActuallyFoundToBeProperties_WhenExcessShortIndChangeAndGOVGIO()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var pack = bill.Packs.AddNew();
			pack.Outturn.ContShouldBe = "Content";
			pack.APA_Weight = 1000;
			pack.APA_WeightUQ = "KG";
			pack.APA_Volume = 2000;
			pack.APA_VolumeUQ = "L";
			pack.APA_PackQty = 3000;

			foreach (var gateInOutMessageType in new GateInOutMessageTypeCodeList().GetAllCodes())
			{
				header.GateInOutMessageType = gateInOutMessageType;

				foreach (var excessShortInd in new ExcessShortIndicatorList().GetAllCodes())
				{
					pack.Outturn.C5_GoodsDescription = ZString.Empty;
					pack.Outturn.C5_WeightOutturned = ZDecimal.Zero;
					pack.Outturn.C5_WeightOutturnedUQ = ZString.Empty;
					pack.Outturn.C5_VolumeOutturned = ZDecimal.Zero;
					pack.Outturn.C5_VolumeOutturnedUQ = ZString.Empty;
					pack.Outturn.C5_PackagesOutturned = ZInt.Zero;
					pack.Outturn.ExcessShortInd = excessShortInd;

					AssertActuallyFoundToBeProperties(pack, false);
				}
			}
		}

		public void TestSetActuallyFoundToBeProperties_WhenExcessShortIndAlreadySetAndGOVGIO()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var pack = bill.Packs.AddNew();

			foreach (var gateInOutMessageType in new GateInOutMessageTypeCodeList().GetAllCodes())
			{
				header.GateInOutMessageType = gateInOutMessageType;

				foreach (var excessShortInd in new ExcessShortIndicatorList().GetAllCodes())
				{
					pack.Outturn.C5_GoodsDescription = ZString.Empty;
					pack.Outturn.C5_WeightOutturned = ZDecimal.Zero;
					pack.Outturn.C5_WeightOutturnedUQ = ZString.Empty;
					pack.Outturn.C5_VolumeOutturned = ZDecimal.Zero;
					pack.Outturn.C5_VolumeOutturnedUQ = ZString.Empty;
					pack.Outturn.C5_PackagesOutturned = ZInt.Zero;
					pack.Outturn.ContShouldBe = ZString.Empty;
					pack.APA_Weight = ZDecimal.Zero;
					pack.APA_WeightUQ = ZString.Empty;
					pack.APA_Volume = ZDecimal.Zero;
					pack.APA_VolumeUQ = ZString.Empty;
					pack.APA_PackQty = ZInt.Zero;
					pack.APA_PackUQ = ZString.Empty;
					pack.Outturn.ExcessShortInd = excessShortInd;
					pack.Outturn.ContShouldBe = "Content";
					pack.APA_Weight = 1000;
					pack.APA_WeightUQ = "KG";
					pack.APA_Volume = 2000;
					pack.APA_VolumeUQ = "L";
					pack.APA_PackQty = 3000;

					AssertActuallyFoundToBeProperties(pack, false);
				}
			}
		}

		public void TestCheckExcessShortInd_DiscrepanciesExist()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var pack = bill.Packs.AddNew();

			foreach (var code in new ManifestTypeList().GetAllCodes())
			{
				header.AMA_ManifestType = code;

				foreach (var excessShortInd in new ExcessShortIndicatorList().GetAllCodes())
				{
					var errorExpected = excessShortInd == "3" && code.In(COSTCO.DepotOutturnReport, COSTCO.BulkBreakBulkOutturnReport, COSTCO.AirCargoOutturnReport, COSTCO.AirLoadDischarge);

					AssertDiscrepanciesExistMessageError(pack, errorExpected, excessShortInd, content: "X");
					AssertDiscrepanciesExistMessageError(pack, errorExpected, excessShortInd, weight: 10);
					AssertDiscrepanciesExistMessageError(pack, errorExpected, excessShortInd, weightUQ: "HG");
					AssertDiscrepanciesExistMessageError(pack, errorExpected, excessShortInd, volume: 10);
					AssertDiscrepanciesExistMessageError(pack, errorExpected, excessShortInd, volumeUQ: "CC");
					AssertDiscrepanciesExistMessageError(pack, errorExpected, excessShortInd, packQty: 10);
				}
			}

			foreach (var code in new GateInOutMessageTypeCodeList().GetAllCodes())
			{
				header.GateInOutMessageType = code;

				foreach (var excessShortInd in new ExcessShortIndicatorList().GetAllCodes())
				{
					AssertDiscrepanciesExistMessageError(pack, false, excessShortInd, content: "X");
					AssertDiscrepanciesExistMessageError(pack, false, excessShortInd, weight: 10);
					AssertDiscrepanciesExistMessageError(pack, false, excessShortInd, weightUQ: "HG");
					AssertDiscrepanciesExistMessageError(pack, false, excessShortInd, volume: 10);
					AssertDiscrepanciesExistMessageError(pack, false, excessShortInd, volumeUQ: "CC");
					AssertDiscrepanciesExistMessageError(pack, false, excessShortInd, packQty: 10);
				}
			}
		}

		void AssertActuallyFoundToBeProperties(AsycudaPack pack, bool emptyExpected)
		{
			var expectedContent = emptyExpected ? pack.Outturn.ContShouldBe : ZString.Empty;
			var expectedWeight = emptyExpected ? pack.APA_Weight : ZDecimal.Zero;
			var expectedWeightUQ = emptyExpected ? pack.APA_WeightUQ : ZString.Empty;
			var expectedVolume = emptyExpected ? pack.APA_Volume : ZDecimal.Zero;
			var expectedVolumeUQ = emptyExpected ? pack.APA_VolumeUQ : ZString.Empty;
			var expectedPackages = emptyExpected ? pack.APA_PackQty : ZInt.Zero;

			AssertEquals("Content", expectedContent, pack.Outturn.C5_GoodsDescription);
			AssertEquals("Weight", expectedWeight, pack.Outturn.C5_WeightOutturned);
			AssertEquals("Weight UQ", expectedWeightUQ, pack.Outturn.C5_WeightOutturnedUQ);
			AssertEquals("Volume", expectedVolume, pack.Outturn.C5_VolumeOutturned);
			AssertEquals("Volume UQ", expectedVolumeUQ, pack.Outturn.C5_VolumeOutturnedUQ);
			AssertEquals("Number of Packs", expectedPackages, pack.Outturn.C5_PackagesOutturned);
		}

		void AssertDiscrepanciesExistMessageError(AsycudaPack pack, bool errorExpected, string excessShortInd, string content = "Content", decimal weight = 1000, string weightUQ = "KG", decimal volume = 2000, string volumeUQ = "L", int packQty = 3000)
		{
			pack.Outturn.ContShouldBe = "Content";
			pack.APA_Weight = 1000;
			pack.APA_WeightUQ = "KG";
			pack.APA_Volume = 2000;
			pack.APA_VolumeUQ = "L";
			pack.APA_PackQty = 3000;
			pack.Outturn.C5_GoodsDescription = "Content";
			pack.Outturn.C5_WeightOutturned = 1000;
			pack.Outturn.C5_WeightOutturnedUQ = "KG";
			pack.Outturn.C5_VolumeOutturned = 2000;
			pack.Outturn.C5_VolumeOutturnedUQ = "L";
			pack.Outturn.C5_PackagesOutturned = 3000;
			pack.Outturn.ExcessShortInd = excessShortInd;

			pack.Outturn.C5_GoodsDescription = content;
			pack.Outturn.C5_WeightOutturned = weight;
			pack.Outturn.C5_WeightOutturnedUQ = weightUQ;
			pack.Outturn.C5_VolumeOutturned = volume;
			pack.Outturn.C5_VolumeOutturnedUQ = volumeUQ;
			pack.Outturn.C5_PackagesOutturned = packQty;
			pack.Outturn.Validation.ValidateAll();

			const string messageError = "Discrepancies exist but Short Excess Indicator 3 indicates that no discrepancies were found";

			if (errorExpected)
			{
				AssertHasMessageError(pack.Outturn.ExcessShortIndInfo, messageError);
			}
			else
			{
				AssertNoMessageErrorContaining(pack.Outturn.ExcessShortIndInfo, messageError);
			}
		}
	}
}
