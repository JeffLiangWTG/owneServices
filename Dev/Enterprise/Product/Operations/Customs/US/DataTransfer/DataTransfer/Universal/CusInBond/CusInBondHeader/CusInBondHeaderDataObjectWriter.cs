using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;

namespace Enterprise.Customs.US.DataTransfer.Universal
{
	public abstract class CusInBondHeaderDataObjectWriter : TopLevelDataObjectWriter<CusInBondHeader, Shipment>, IMergeDataObjectWriter
	{
		protected CusInBondHeaderDataObjectWriter(IDataWritingManager writeManager)
			: base(writeManager)
		{
		}

		protected override ZString GetEDIMessageSubType()
		{
			return EDIMessageSubTypeList.Codes.XmlUniversalShipment;
		}

		protected override void PopulateDataObject(CusInBondHeader headerBO, Shipment headerData)
		{
			PopulateDataObjectCore(headerBO, headerData);
		}

		protected void PopulateDataObjectCore(CusInBondHeader headerBO, Shipment headerData)
		{
			var headerHelper = WriterHelper(headerBO);
			PopulateMainData(headerBO, headerData, headerHelper);
			headerData.SetAdditionalBillCollection(() => ProcessCollection(GetRelatedBills(headerBO, headerHelper), InBondBillDataObjectWriter(writeManager, headerHelper)));
			headerData.SetInBondMoveHeaderCollection(() => ProcessCollection(GetRelatedMoveHeaders(headerBO, headerHelper), InBondMoveHeaderDataObjectWriter(writeManager, headerHelper, headerData)));
		}

		protected void PopulateMainData(CusInBondHeader headerBO, Shipment headerData, InBondDataObjectWriterHelper helper)
		{
			headerData.Branch = Branch.New(headerBO.Branch);
			headerData.LloydsIMO = headerBO.BH_LloydsNumber;
			headerData.VoyageFlightNo = headerBO.BH_VoyageNumber;
			headerData.SetAddInfoCollection(() => PopulateAddInfosData(headerBO, headerData));
			headerData.SetDateCollection(() => PopulateDatesData(headerBO, headerData));
			PopulateNotes(headerBO, headerData);
			PopulateInBondSpecificData(headerBO, headerData, helper);
		}

		protected virtual List<AddInfo> PopulateAddInfosData(CusInBondHeader headerBO, Shipment headerData)
		{
			return headerData.AddInfoCollection ?? new List<AddInfo>();
		}

		protected virtual List<Date> PopulateDatesData(CusInBondHeader headerBO, Shipment headerData)
		{
			return headerData.DateCollection ?? new List<Date>();
		}

		protected virtual void PopulateInBondSpecificData(CusInBondHeader headerBO, Shipment headerData, InBondDataObjectWriterHelper headerHelper)
		{
		}

		void PopulateNotes(CusInBondHeader headerBO, Shipment headerData)
		{
			headerData.SetNoteCollection(() =>
			{
				var notes = headerBO.Notes.GetAllNotesVisibleToCurrentCompany().OrderBy(x => x.ST_Description);
				return ProcessCollection(notes, new NoteDataObjectWriter(writeManager), CollectionContent.Partial);
			});
		}

		void IMergeDataObjectWriter.MergeData(IDataObject dataObject, BusinessObject mergingBO)
		{
			var shipmentData = dataObject as Shipment;
			var headerBO = mergingBO as CusInBondHeader;
			if (shipmentData != null && headerBO != null)
			{
				var subShipmentCollection = shipmentData.SubShipmentCollection ?? new DataObjectList<Shipment>();
				var inBondData = subShipmentCollection.FirstOrDefault(x => x.GetMatchingDataSource(GetTopLevelDataContextType()) != null);
				if (inBondData == null)
				{
					inBondData = new Shipment(writeManager.WriterStrategy);
					inBondData.DataContext = DataContextFactory.New(writeManager.Schema.Namespace);
					inBondData.DataContext.AddDataSource(GetTopLevelDataContextType(), headerBO.BH_JobReference);
					subShipmentCollection.Add(inBondData);
				}
				PopulateDataObjectCore(headerBO, inBondData);
				shipmentData.SetSubShipmentCollection(() => subShipmentCollection);
			}
		}

		protected InBondDataObjectWriterHelper WriterHelper(CusInBondHeader headerBO)
		{
			return fWriterHelper ?? (fWriterHelper = GetWriterHelperCore(headerBO));
		}
		InBondDataObjectWriterHelper fWriterHelper;

		protected virtual InBondDataObjectWriterHelper GetWriterHelperCore(CusInBondHeader headerBO)
		{
			return new InBondDataObjectWriterHelper(headerBO);
		}

		protected virtual IEnumerable<CusInBondBill> GetRelatedBills(CusInBondHeader headerBO, InBondDataObjectWriterHelper headerHelper)
		{
			return headerHelper.Load<CusInBondBill>(headerBO.Bills.CompleteFilter);
		}

		protected virtual IEnumerable<CusInBondMoveHeader> GetRelatedMoveHeaders(CusInBondHeader headerBO, InBondDataObjectWriterHelper headerHelper)
		{
			return headerHelper.Load<CusInBondMoveHeader>(headerBO.MovementHeaders.CompleteFilter);
		}

		protected virtual CusInBondBillDataObjectWriter InBondBillDataObjectWriter(IDataWritingManager writeManager, InBondDataObjectWriterHelper helper)
		{
			return new CusInBondBillDataObjectWriter(writeManager, helper);
		}

		protected virtual CusInBondMoveHeaderDataObjectWriter InBondMoveHeaderDataObjectWriter(IDataWritingManager writeManager, InBondDataObjectWriterHelper helper, Shipment headerData)
		{
			return new CusInBondMoveHeaderDataObjectWriter(writeManager, helper, headerData);
		}
	}
}
