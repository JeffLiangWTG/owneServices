using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using UniversalCustoms = Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;

namespace Enterprise.Customs.DataTransfer.Universal.Testing
{
	sealed class CommercialInvoiceHeaderRelatedDataTest : TestCaseWithFactory
	{
		public void TestGetBranchPK()
		{
			var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.DataContext = DataContextFactory.New();
			var data = new CommercialInvoiceHeaderRelatedData(shipment);
			AssertEquals(ZGuid.Empty, data.GetBranchPK(Factory));

			shipment.Branch = new Branch() { Code = "Z!Z", Name = "DUMMY BRANCH" };
			AssertEquals(ZGuid.Invalid, data.GetBranchPK(Factory));

			shipment.Branch.Code = GlbBranch.CurrentBranch.GB_Code;
			AssertEquals(GlbBranch.CurrentBranch.PK, data.GetBranchPK(Factory));
		}

		public void TestMessageType()
		{
			var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			var data = new CommercialInvoiceHeaderRelatedData(shipment);
			AssertNull(data.MessageType);
			shipment.MessageType = new CodeDescriptionPair() { Code = "CDS" };
			AssertEquals(shipment.MessageType, data.MessageType);
		}

		public void TestTransportLegCollection()
		{
			var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			var data = new CommercialInvoiceHeaderRelatedData(shipment);
			AssertNull(data.TransportLegCollection);
			shipment.SetTransportLegCollection(() => new DataObjectList<TransportLeg>());
			AssertEquals(shipment.TransportLegCollection, data.TransportLegCollection);
		}

		public void TestNoteCollection()
		{
			var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			var data = new CommercialInvoiceHeaderRelatedData(shipment);
			AssertNull(data.NoteCollection);
			shipment.SetNoteCollection(() => new DataObjectList<Note>());
			AssertEquals(shipment.NoteCollection, data.NoteCollection);
		}

		public void TestBillCollection()
		{
			var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			var data = new CommercialInvoiceHeaderRelatedData(shipment);
			AssertNull(data.BillCollection);
			shipment.SetAdditionalBillCollection(() => new List<AdditionalBill>());
			AssertEquals(shipment.AdditionalBillCollection, data.BillCollection);
		}

		public void TestContainerCollection()
		{
			var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			var data = new CommercialInvoiceHeaderRelatedData(shipment);
			AssertNull(data.ContainerCollection);
			shipment.SetContainerCollection(() => new DataObjectList<Container>());
			AssertEquals(shipment.ContainerCollection, data.ContainerCollection);
		}

		public void TestGroupChargeCollection()
		{
			var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			var data = new CommercialInvoiceHeaderRelatedData(shipment);
			AssertNull(data.GroupChargeCollection);
			shipment.CommercialInfo = new UniversalCustoms.CommercialInfo();
			AssertNull(data.GroupChargeCollection);
			shipment.CommercialInfo.CommercialChargeCollection = new List<UniversalCustoms.CommercialCharge>();
			AssertEquals(shipment.CommercialInfo.CommercialChargeCollection, data.GroupChargeCollection);
		}

		public void TestHasBillData()
		{
			var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.CommercialInfo = new UniversalCustoms.CommercialInfo()
			{
				CommercialInvoiceCollection = new DataObjectList<UniversalCustoms.CommercialInvoiceHeader>(new[] { new UniversalCustoms.CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance) })
			};
			var data = new CommercialInvoiceHeaderRelatedData(shipment);
			AssertEquals(false, data.HasBillData);
			shipment.SetAdditionalBillCollection(() => new List<AdditionalBill>());
			AssertEquals(true, data.HasBillData);

			shipment.CommercialInfo.SubGroupCollection = new List<UniversalCustoms.CommercialInfo>();
			data = new CommercialInvoiceHeaderRelatedData(shipment);
			AssertEquals(false, data.HasBillData);

			shipment.CommercialInfo.SubGroupCollection = null;
			shipment.CommercialInfo.CommercialInvoiceCollection.Add(new UniversalCustoms.CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance));
			data = new CommercialInvoiceHeaderRelatedData(shipment);
			AssertEquals(false, data.HasBillData);

			shipment.CommercialInfo.CommercialInvoiceCollection = null;
			data = new CommercialInvoiceHeaderRelatedData(shipment);
			AssertEquals(false, data.HasBillData);

			shipment.CommercialInfo = null;
			data = new CommercialInvoiceHeaderRelatedData(shipment);
			AssertEquals(false, data.HasBillData);

			shipment.CommercialInfo = new UniversalCustoms.CommercialInfo()
			{
				CommercialInvoiceCollection = new DataObjectList<UniversalCustoms.CommercialInvoiceHeader>(new[] { new UniversalCustoms.CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance) })
			};
			data = new CommercialInvoiceHeaderRelatedData(shipment);
			AssertEquals(true, data.HasBillData);
		}

		public void TestHasContainerData()
		{
			var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.CommercialInfo = new UniversalCustoms.CommercialInfo()
			{
				CommercialInvoiceCollection = new DataObjectList<UniversalCustoms.CommercialInvoiceHeader>(new[] { new UniversalCustoms.CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance) })
			};
			var data = new CommercialInvoiceHeaderRelatedData(shipment);
			AssertEquals(false, data.HasContainerData);
			shipment.SetContainerCollection(() => new DataObjectList<Container>());
			AssertEquals(true, data.HasContainerData);

			shipment.CommercialInfo.SubGroupCollection = new List<UniversalCustoms.CommercialInfo>();
			data = new CommercialInvoiceHeaderRelatedData(shipment);
			AssertEquals(false, data.HasContainerData);

			shipment.CommercialInfo.SubGroupCollection = null;
			shipment.CommercialInfo.CommercialInvoiceCollection.Add(new UniversalCustoms.CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance));
			data = new CommercialInvoiceHeaderRelatedData(shipment);
			AssertEquals(false, data.HasContainerData);

			shipment.CommercialInfo.CommercialInvoiceCollection = null;
			data = new CommercialInvoiceHeaderRelatedData(shipment);
			AssertEquals(false, data.HasContainerData);

			shipment.CommercialInfo = null;
			data = new CommercialInvoiceHeaderRelatedData(shipment);
			AssertEquals(false, data.HasContainerData);

			shipment.CommercialInfo = new UniversalCustoms.CommercialInfo()
			{
				CommercialInvoiceCollection = new DataObjectList<UniversalCustoms.CommercialInvoiceHeader>(new[] { new UniversalCustoms.CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance) })
			};
			data = new CommercialInvoiceHeaderRelatedData(shipment);
			AssertEquals(true, data.HasContainerData);
		}

		public void TestHasGroupChargeData()
		{
			var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.CommercialInfo = new UniversalCustoms.CommercialInfo()
			{
				CommercialInvoiceCollection = new DataObjectList<UniversalCustoms.CommercialInvoiceHeader>(new[] { new UniversalCustoms.CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance) })
			};
			var data = new CommercialInvoiceHeaderRelatedData(shipment);
			AssertEquals(false, data.HasGroupChargeData);
			shipment.CommercialInfo.CommercialChargeCollection = new List<UniversalCustoms.CommercialCharge>();
			AssertEquals(true, data.HasGroupChargeData);

			shipment.CommercialInfo.SubGroupCollection = new List<UniversalCustoms.CommercialInfo>();
			data = new CommercialInvoiceHeaderRelatedData(shipment);
			AssertEquals(false, data.HasGroupChargeData);

			shipment.CommercialInfo.SubGroupCollection = null;
			shipment.CommercialInfo.CommercialInvoiceCollection.Add(new UniversalCustoms.CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance));
			data = new CommercialInvoiceHeaderRelatedData(shipment);
			AssertEquals(false, data.HasGroupChargeData);

			shipment.CommercialInfo.CommercialInvoiceCollection = null;
			data = new CommercialInvoiceHeaderRelatedData(shipment);
			AssertEquals(false, data.HasGroupChargeData);

			shipment.CommercialInfo = null;
			data = new CommercialInvoiceHeaderRelatedData(shipment);
			AssertEquals(false, data.HasGroupChargeData);

			shipment.CommercialInfo = new UniversalCustoms.CommercialInfo()
			{
				CommercialChargeCollection = new List<UniversalCustoms.CommercialCharge>(),
				CommercialInvoiceCollection = new DataObjectList<UniversalCustoms.CommercialInvoiceHeader>(new[] { new UniversalCustoms.CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance) })
			};
			data = new CommercialInvoiceHeaderRelatedData(shipment);
			AssertEquals(true, data.HasGroupChargeData);
		}

		public void TestIsSingleInvoiceData()
		{
			var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.CommercialInfo = new UniversalCustoms.CommercialInfo()
			{
				CommercialInvoiceCollection = new DataObjectList<UniversalCustoms.CommercialInvoiceHeader>(new[] { new UniversalCustoms.CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance) })
			};
			var data = new CommercialInvoiceHeaderRelatedData(shipment);
			AssertEquals(true, data.IsSingleInvoiceData);

			shipment.CommercialInfo.SubGroupCollection = new List<UniversalCustoms.CommercialInfo>();
			data = new CommercialInvoiceHeaderRelatedData(shipment);
			AssertEquals(false, data.IsSingleInvoiceData);

			shipment.CommercialInfo.SubGroupCollection = null;
			shipment.CommercialInfo.CommercialInvoiceCollection.Add(new UniversalCustoms.CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance));
			data = new CommercialInvoiceHeaderRelatedData(shipment);
			AssertEquals(false, data.IsSingleInvoiceData);

			shipment.CommercialInfo.CommercialInvoiceCollection = null;
			data = new CommercialInvoiceHeaderRelatedData(shipment);
			AssertEquals(false, data.IsSingleInvoiceData);

			shipment.CommercialInfo = null;
			data = new CommercialInvoiceHeaderRelatedData(shipment);
			AssertEquals(false, data.IsSingleInvoiceData);

			shipment.CommercialInfo = new UniversalCustoms.CommercialInfo()
			{
				CommercialInvoiceCollection = new DataObjectList<UniversalCustoms.CommercialInvoiceHeader>(new[] { new UniversalCustoms.CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance) })
			};
			data = new CommercialInvoiceHeaderRelatedData(shipment);
			AssertEquals(true, data.IsSingleInvoiceData);
		}
	}
}
