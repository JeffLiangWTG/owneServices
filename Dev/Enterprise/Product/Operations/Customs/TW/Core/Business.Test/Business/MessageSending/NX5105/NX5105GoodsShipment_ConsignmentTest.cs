using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.TW;
using Enterprise.Customs.TW.Messaging;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class NX5105GoodsShipment_ConsignmentTest : TestCaseWithFactory
	{
		#region Manifest Serial Number
		[ExpectNoExceptions]
		public void TestConsignment_ManifestSerialNumber()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CusEntryInstruction;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			IConsignment consignment = new NX5105GoodsShipment_Consignment(entryHeader);
			declaration.JE_SLD = ZString.Empty;
			NUnit.Framework.Assert.That(consignment.ManifestSerialNumber, NUnit.Framework.Is.EqualTo(ZString.Empty), "Consignment.ManifestSerialNumber should be");
			declaration.JE_SLD = "1234";
			NUnit.Framework.Assert.That(consignment.ManifestSerialNumber, NUnit.Framework.Is.EqualTo("1234").Using(CustomComparers.TypeComparison), "Consignment.ManifestSerialNumber should be");
		}

		#endregion
		#region Arrival Transport Means Type Code
		[ExpectNoExceptions]
		public void TestConsignment_ArrivalTransportMeansTypeCode()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CusEntryInstruction;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			IConsignment consignment = new NX5105GoodsShipment_Consignment(entryHeader);
			declaration.JE_TransportMode = ZString.Empty;
			NUnit.Framework.Assert.That(consignment.ArrivalTransportMeansTypeCode, NUnit.Framework.Is.EqualTo(ZString.Empty), "Consignment.ArrivalTransportMeansTypeCode should be");
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			declaration.JE_ContainerMode = ContainerModeList.Codes.BreakBulk;
			NUnit.Framework.Assert.That(consignment.ArrivalTransportMeansTypeCode, NUnit.Framework.Is.EqualTo(TransportCodeList.Codes.SeaPackedSundryGoods).Using(CustomComparers.TypeComparison), "Consignment.ArrivalTransportMeansTypeCode should be");
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			declaration.JE_ContainerMode = ContainerModeList.Codes.Loose;
			NUnit.Framework.Assert.That(consignment.ArrivalTransportMeansTypeCode, NUnit.Framework.Is.EqualTo(TransportCodeList.Codes.AirNotExpressDelivery).Using(CustomComparers.TypeComparison), "Consignment.ArrivalTransportMeansTypeCode should be");
		}

		#endregion
		#region Border Transport Means
		[ExpectNoExceptions]
		public void TestConsignment_BorderTransportMeans()
		{
			var jobDeclaration = Factory.New<JobDeclaration>();
			var entryInstruction = jobDeclaration.CusEntryInstruction;
			var entryHeader = jobDeclaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			IConsignment consignment = new NX5105GoodsShipment_Consignment(entryHeader);
			NUnit.Framework.Assert.That(consignment.BorderTransportMeans.GetType(), NUnit.Framework.Is.EqualTo(typeof(NX5105Consignment_BorderTransportMeans)));
		}

		#endregion
		#region Consignment Item
		[ExpectNoExceptions]
		public void TestConsignment_ConsignmentItem()
		{
			var jobDeclaration = Factory.New<JobDeclaration>();
			var entryInstruction = jobDeclaration.CusEntryInstruction;
			var entryHeader = jobDeclaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			IConsignment consignment = new NX5105GoodsShipment_Consignment(entryHeader);
			jobDeclaration.JE_SplitMark = false;
			NUnit.Framework.Assert.That(consignment.ConsignmentItem.Split, NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison), "ConsignmentItem.Split should be");
			jobDeclaration.JE_SplitMark = true;
			NUnit.Framework.Assert.That(consignment.ConsignmentItem.Split, NUnit.Framework.Is.EqualTo("P").Using(CustomComparers.TypeComparison), "ConsignmentItem.Split should be");
		}

		#endregion
		#region Goods Location
		[ExpectNoExceptions]
		public void TestConsignment_GoodsLocation()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CusEntryInstruction;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			IConsignment consignment = new NX5105GoodsShipment_Consignment(entryHeader);
			NUnit.Framework.Assert.That(consignment.GoodsLocation, NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison), "Consignment.GoodsLocation should be");
			entryInstruction.CEI_GoodsLocation = "";
			NUnit.Framework.Assert.That(consignment.GoodsLocation, NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison), "Consignment.GoodsLocation should be");
			entryInstruction.CEI_GoodsLocation = "GOODLOC";
			NUnit.Framework.Assert.That(consignment.GoodsLocation, NUnit.Framework.Is.EqualTo("GOODLOC").Using(CustomComparers.TypeComparison), "Consignment.GoodsLocation should be");
		}

		#endregion
		#region Loading Location
		[ExpectNoExceptions]
		public void TestConsignment_LoadingLocation()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CusEntryInstruction;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			IConsignment consignment = new NX5105GoodsShipment_Consignment(entryHeader);
			declaration.JE_RL_NKOrigin = ZString.Empty;
			NUnit.Framework.Assert.That(consignment.LoadingLocation.ID, NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison), "Consignment.LoadingLocation ID should be");
			declaration.JE_RL_NKOrigin = "PORT";
			NUnit.Framework.Assert.That(consignment.LoadingLocation.ID, NUnit.Framework.Is.EqualTo("PORT").Using(CustomComparers.TypeComparison), "Consignment.LoadingLocation ID should be");
		}

		#endregion
		#region Transport Contract Documents
		[ExpectNoExceptions]
		public void TestConsignment_TransportContractDocuments()
		{
			var declaration = Factory.New<JobDeclaration>();
			var header = new TestTWCreator(Factory).CreateOrganizationForJobDocAddress();
			var address = header.MainAddress;
			var importerDocumentaryAddress = declaration.ImporterDocumentaryAddress;
			importerDocumentaryAddress.OrganisationPK = header.PK;
			importerDocumentaryAddress.E2_OA_Address = address.PK;
			address.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.TaiwanCodeTypes.FTZ, "FTZ001", Core.Constants.CountryCodes.Taiwan);
			var entryInstruction = declaration.CusEntryInstruction;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			var entryLine = entryHeader.MergedLines.AddNew();
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			IConsignment consignment = new NX5105GoodsShipment_Consignment(entryHeader);
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			declaration.JE_MessageType = TWJobMessageTypeList.Codes.Export;
			entryInstruction.CEI_Style = Constants.DeclarationTypes.Export.F4;
			entryHeader.EntryNumber = "AAAA132456";
			entryHeader.CH_Status = "AWO";
			consignment = new NX5105GoodsShipment_Consignment(entryHeader);
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(consignment.TransportContractDocuments.Count(), NUnit.Framework.Is.EqualTo(1), "Consignment.TransportContractDocuments.Count() should be");
				NUnit.Framework.Assert.That(consignment.TransportContractDocuments.ElementAt(0).ID, NUnit.Framework.Is.EqualTo("AAAA132456").Using(CustomComparers.TypeComparison), "Consignment.TransportContractDocuments[0].ID should be ");
				NUnit.Framework.Assert.That(consignment.TransportContractDocuments.ElementAt(0).TypeCode, NUnit.Framework.Is.EqualTo("741").Using(CustomComparers.TypeComparison), "Consignment.TransportContractDocuments[0].TypeCode should be ");
			});

			entryInstruction.CEI_Style = "AA";
			declaration.JE_MasterBill = "master";
			consignment = new NX5105GoodsShipment_Consignment(entryHeader);
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(consignment.TransportContractDocuments.Count(), NUnit.Framework.Is.EqualTo(1), "Consignment.TransportContractDocuments.Count() should be");
				NUnit.Framework.Assert.That(consignment.TransportContractDocuments.ElementAt(0).ID, NUnit.Framework.Is.EqualTo("mas-ter").Using(CustomComparers.TypeComparison), "Consignment.TransportContractDocuments[0].ID should be ");
				NUnit.Framework.Assert.That(consignment.TransportContractDocuments.ElementAt(0).TypeCode, NUnit.Framework.Is.EqualTo("741").Using(CustomComparers.TypeComparison), "Consignment.TransportContractDocuments[0].TypeCode should be ");
			});

			declaration.JE_HouseBill = "house";
			consignment = new NX5105GoodsShipment_Consignment(entryHeader);
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(consignment.TransportContractDocuments.Count(), NUnit.Framework.Is.EqualTo(2), "Consignment.TransportContractDocuments.Count() should be");
				NUnit.Framework.Assert.That(consignment.TransportContractDocuments.ElementAt(1).ID, NUnit.Framework.Is.EqualTo("house").Using(CustomComparers.TypeComparison), "Consignment.TransportContractDocuments[1].ID should be ");
				NUnit.Framework.Assert.That(consignment.TransportContractDocuments.ElementAt(1).TypeCode, NUnit.Framework.Is.EqualTo("703").Using(CustomComparers.TypeComparison), "Consignment.TransportContractDocuments[1].TypeCode should be ");
			});

			var bill1 = declaration.Bills.AddNew();
			bill1.CU_BillType = "CN";
			bill1.CU_BillNum = "AA";
			var bill2 = declaration.Bills.AddNew();
			bill2.CU_BillType = "CN";
			bill2.CU_BillNum = "XX";
			var emptyBill = declaration.Bills.AddNew();
			emptyBill.CU_BillType = "";
			emptyBill.CU_BillNum = "";
			consignment = new NX5105GoodsShipment_Consignment(entryHeader);
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(consignment.TransportContractDocuments.Count(), NUnit.Framework.Is.EqualTo(4), "Consignment.TransportContractDocuments.Count() should be");
				NUnit.Framework.Assert.That(consignment.TransportContractDocuments.ElementAt(2).ID, NUnit.Framework.Is.EqualTo("AA").Using(CustomComparers.TypeComparison), "Consignment.TransportContractDocuments[2].ID should be ");
				NUnit.Framework.Assert.That(consignment.TransportContractDocuments.ElementAt(2).TypeCode, NUnit.Framework.Is.EqualTo("976").Using(CustomComparers.TypeComparison), "Consignment.TransportContractDocuments[2].TypeCode should be ");
				NUnit.Framework.Assert.That(consignment.TransportContractDocuments.ElementAt(3).ID, NUnit.Framework.Is.EqualTo("XX").Using(CustomComparers.TypeComparison), "Consignment.TransportContractDocuments[3].ID should be ");
				NUnit.Framework.Assert.That(consignment.TransportContractDocuments.ElementAt(3).TypeCode, NUnit.Framework.Is.EqualTo("976").Using(CustomComparers.TypeComparison), "Consignment.TransportContractDocuments[3].TypeCode should be ");
			});
		}

		#endregion
		#region Transport Equipments
		[ExpectNoExceptions]
		public void TestConsignment_TransportEquipments()
		{
			var jobDeclaration = Factory.New<JobDeclaration>();
			var entryInstruction = jobDeclaration.CusEntryInstruction;
			var entryHeader = jobDeclaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			IConsignment consignment = new NX5105GoodsShipment_Consignment(entryHeader);
			NUnit.Framework.Assert.That(consignment.TransportEquipments.Cast<TransportEquipmentWrapper>().ToList(), NUnit.Framework.Is.TypeOf(typeof(List<TransportEquipmentWrapper>)));
		}

		#endregion
		#region Bonded Goods
		[ExpectNoExceptions]
		public void TestConsignment_BondedGoods()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryInstruction = declaration.CusEntryInstruction;
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			IConsignment consignment = new NX5105GoodsShipment_Consignment(entryHeader);
			NUnit.Framework.Assert.That(consignment.BondedGoods, NUnit.Framework.Is.EqualTo(default(IBondedGoods)));
			entryInstruction.CEI_ReasonForDuty = "A";
			consignment = new NX5105GoodsShipment_Consignment(entryHeader);
			NUnit.Framework.Assert.That(consignment.BondedGoods, NUnit.Framework.Is.TypeOf(typeof(NX5105Consignment_BondedGoods)));
		}

		#endregion
		public void TestCheckArgumentsNotNull()
		{
			AssertExceptionThrown<ArgumentNullException>(() =>
			{
				new NX5105GoodsShipment_Consignment(null);
			}

			);
			AssertNoExceptionThrown(() =>
			{
				var declaration = Factory.New<JobDeclaration>();
				var entryInstruction = declaration.CusEntryInstruction;
				var entryHeader = declaration.CustomsEntryHeaders.AddNew();
				entryHeader.CH_CEI_Instruction = entryInstruction.PK;
				new NX5105GoodsShipment_Consignment(entryHeader);
			}

			);
		}

		[ExpectNoExceptions]
		public void TestCheckNotApplicableProperties()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CusEntryInstruction;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			IConsignment consignment = new NX5105GoodsShipment_Consignment(entryHeader);
			NUnit.Framework.Assert.That(consignment.Carrier, NUnit.Framework.Is.EqualTo(default(IPartyDetails)));
			NUnit.Framework.Assert.That(consignment.ShippingOrderNumber, NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(consignment.DepartureTransportMeans, NUnit.Framework.Is.EqualTo(default(ITransportMeans)));
			NUnit.Framework.Assert.That(consignment.TransitTransportMeansTypeCode, NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(consignment.GoodsLocations, NUnit.Framework.Is.EqualTo(default(IEnumerable<ZString>)));
			NUnit.Framework.Assert.That(consignment.UnloadingLocation, NUnit.Framework.Is.EqualTo(default(Messaging.ILocation)));
		}

		[ExpectNoExceptions]
		public void TestAdditionalInformations()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CusEntryInstruction;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			TestTWCreator.AddDeclarationReservedFields(declaration);
			IConsignment consignment = new NX5105GoodsShipment_Consignment(entryHeader);
			NUnit.Framework.Assert.That(consignment.AdditionalInformations, NUnit.Framework.Is.Not.EqualTo(default(IEnumerable<IAdditionalInformation>)));
			SharedHelperTest.AssertAdditionalInformations(consignment.AdditionalInformations);
		}
	}
}
