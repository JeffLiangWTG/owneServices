using System;
using CargoWise.RefDbRepo.Common.Contract_0_9;
using CargoWise.RefDbRepo.NewService.Controllers;
using CargoWise.RefDbRepo.Service.DataContractAdaptor;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.NewService.Test.Controllers
{
	[TestFixture]
	class RefCusProcedureControllerFixture
	{
		[Test]
		public void TestLatestVersioRefCusProcedureController()
		{
			dataAdaptor.Setup(x => x.ParseVersion("0_22_9")).Returns(new Tuple<int, int, int>(0, 22, 9));
			var result = (RefCusProcedure)controller.TransformData(procedure, "0_22_9");
			Assert.AreEqual("IM", result.ZZ6_Category);
			Assert.AreEqual("42", result.ZZ6_ProcedureCode);
			Assert.AreEqual("78", result.ZZ6_PreviousProcedureCode);
			Assert.AreEqual("C13", result.ZZ6_Concession);
			Assert.AreEqual("Immissione in libera pratica di merci destinate al consumo in altro Stato membro", result.ZZ6_Description);
			Assert.AreEqual("IT", result.ZZ6_ZZZ_NKDataGrouping);
			Assert.AreEqual("IT", result.ZZ6_RN_CountryOrGrouping);
			Assert.AreEqual("IMP", result.ZZ6_ShipmentType);
			Assert.AreEqual(true, result.ZZ6_CalculateDuty);
			Assert.AreEqual("ORDIN,COLUO,SEMPL", result.ZZ6_Group);
			Assert.AreEqual(false, result.ZZ6_LandedCost);
			Assert.AreEqual("Y", result.ZZ6_IntoWarehouse);
			Assert.AreEqual("I", result.ZZ6_OutOfWarehouse);
			Assert.AreEqual(new DateTime(1900, 1, 1), result.ZZ6_StartDate);
			Assert.AreEqual(new DateTime(2079, 6, 6, 23, 59, 0), result.ZZ6_EndDate);
			Assert.AreEqual(false, result.ZZ6_TemporaryProcedure);
			Assert.AreEqual(true, result.ZZ6_CalculateVAT);
			Assert.AreEqual("N", result.ZZ6_IsGuaranteeConsumed);
			Assert.AreEqual("Y", result.ZZ6_IsGuaranteeReleased);
			Assert.AreEqual("Y", result.ZZ6_IntoTemporaryProcedure);
			Assert.AreEqual("Y", result.ZZ6_OutOfTemporaryProcedure);
			Assert.AreEqual("N", result.ZZ6_IntoInwardProcessing);
			Assert.AreEqual("N", result.ZZ6_OutOfInwardProcessing);
			Assert.AreEqual("I", result.ZZ6_IntoOutwardProcessing);
			Assert.AreEqual("I", result.ZZ6_OutofOutwardProcessing);
		}

		[Test]
		public void TestPriorVersioRefCusProcedureController()
		{
			dataAdaptor.Setup(x => x.ParseVersion("0_21_9")).Returns(new Tuple<int, int, int>(0, 21, 9));
			var result = (RefCusProcedurePrior_0_22_9)controller.TransformData(procedure, "0_21_9");
			Assert.AreEqual("IM", result.ZZ6_Category);
			Assert.AreEqual("42", result.ZZ6_ProcedureCode);
			Assert.AreEqual("78", result.ZZ6_PreviousProcedureCode);
			Assert.AreEqual("C13", result.ZZ6_Concession);
			Assert.AreEqual("Immissione in libera pratica di merci destinate al consumo in altro Stato membro", result.ZZ6_Description);
			Assert.AreEqual("IT", result.ZZ6_ZZZ_NKDataGrouping);
			Assert.AreEqual("IT", result.ZZ6_RN_CountryOrGrouping);
			Assert.AreEqual("IMP", result.ZZ6_ShipmentType);
			Assert.AreEqual(true, result.ZZ6_CalculateDuty);
			Assert.AreEqual("ORDIN,COLUO,SEMPL", result.ZZ6_Group);
			Assert.AreEqual(false, result.ZZ6_LandedCost);
			Assert.AreEqual(true, result.ZZ6_IntoWarehouse);
			Assert.AreEqual(false, result.ZZ6_OutOfWarehouse);
			Assert.AreEqual(new DateTime(1900, 1, 1), result.ZZ6_StartDate);
			Assert.AreEqual(new DateTime(2079, 6, 6, 23, 59, 0), result.ZZ6_EndDate);
			Assert.AreEqual(false, result.ZZ6_TemporaryProcedure);
			Assert.AreEqual(true, result.ZZ6_CalculateVAT);
		}

		[SetUp]
		public void TestSetup()
		{
			dataAdaptor = new Mock<IDataAdaptor>();
			dataAdaptor.Setup(x => x.ToVersion(It.IsAny<RefCusProcedure>(), It.IsAny<string>())).Returns<RefCusProcedure, string>((t, v) => t);
			controller = new RefCusProcedureControllerForTest(
				new Mock<IReferenceDataService<RefCusProcedure>>().Object,
				dataAdaptor.Object,
				new Mock<IDataBlockCacheHelper>().Object,
				new Mock<IClientRecord>().Object,
				new Mock<ICacheWrapper>().Object,
				new Mock<ILogHelper>().Object);

			procedure = new RefCusProcedure
			{
				ZZ6_Category = "IM",
				ZZ6_ProcedureCode = "42",
				ZZ6_PreviousProcedureCode = "78",
				ZZ6_Concession = "C13",
				ZZ6_Description = "Immissione in libera pratica di merci destinate al consumo in altro Stato membro",
				ZZ6_ZZZ_NKDataGrouping = "IT",
				ZZ6_RN_CountryOrGrouping = "IT",
				ZZ6_ShipmentType = "IMP",
				ZZ6_CalculateDuty = true,
				ZZ6_Group = "ORDIN,COLUO,SEMPL",
				ZZ6_LandedCost = false,
				ZZ6_IntoWarehouse = "Y",
				ZZ6_OutOfWarehouse = "I",
				ZZ6_StartDate = new DateTime(1900, 1, 1),
				ZZ6_EndDate = new DateTime(2079, 6, 6, 23, 59, 0),
				ZZ6_TemporaryProcedure = false,
				ZZ6_CalculateVAT = true,
				ZZ6_IsGuaranteeConsumed = "N",
				ZZ6_IsGuaranteeReleased = "Y",
				ZZ6_IntoTemporaryProcedure = "Y",
				ZZ6_OutOfTemporaryProcedure = "Y",
				ZZ6_IntoInwardProcessing = "N",
				ZZ6_OutOfInwardProcessing = "N",
				ZZ6_IntoOutwardProcessing = "I",
				ZZ6_OutofOutwardProcessing = "I",
			};
		}
		RefCusProcedureControllerForTest controller;
		RefCusProcedure procedure;
		Mock<IDataAdaptor> dataAdaptor;
	}

	class RefCusProcedureControllerForTest : RefCusProcedureController
	{
		public RefCusProcedureControllerForTest(IReferenceDataService<RefCusProcedure> service, IDataAdaptor adaptor, IDataBlockCacheHelper dataBlockCacheHelper, IClientRecord clientRecord, ICacheWrapper cacheWrapper, ILogHelper logHelper)
			: base(service, adaptor, dataBlockCacheHelper, clientRecord, cacheWrapper, logHelper)
		{
		}

		public new object TransformData(RefCusProcedure dataSet, string version) => base.TransformData(dataSet, version);
	}
}
