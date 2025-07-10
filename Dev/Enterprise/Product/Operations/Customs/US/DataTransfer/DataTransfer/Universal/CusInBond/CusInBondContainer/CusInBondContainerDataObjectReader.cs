using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.DataTransfer.Universal
{
	public abstract class CusInBondContainerDataObjectReader<TContainer, TCommodity> : BaseContainerDataObjectReader<TContainer>
		where TContainer : CusInBondContainer
		where TCommodity : CusInBondCargoDesc
	{
		protected CusInBondContainerDataObjectReader(Container containerDataObject, IXmlImportLogger logger, InBondDataObjectReaderHelper helper, ZGuid moveDetailPK)
			: base(containerDataObject, logger, helper.Factory)
		{
			this.readerHelper = helper;
			this.moveDetailPK = moveDetailPK;
		}
		protected readonly ZGuid moveDetailPK;
		readonly InBondDataObjectReaderHelper readerHelper;

		protected InBondDataObjectReaderHelper Helper
		{
			get { return readerHelper; }
		}

		protected ZGuid MoveDetailPK
		{
			get { return moveDetailPK; }
		}

		protected override TContainer GetExistingBusinessObject()
		{
			TContainer result = null;
			if (dataObject.ContainerNumber.HasValue)
			{
				var query = new ZQuery(CusInBondContainerSchema.BC_ParentID, moveDetailPK);
				query.AddToFilter(CusInBondContainerSchema.BC_ContainerNum, dataObject.ContainerNumber.Value);
				query.FetchOnlyFromLocalCache = true; // as we don't update dbo.CusInBondMoveDetail, it should always be load from local cache
				result = factory.LoadTop1<TContainer>(query);
			}
			return result;
		}

		protected override void PopulateBusinessObject(TContainer containerBO)
		{
			var containerRow = GetColumnIndexer(containerBO);
			var containerPK = containerRow.GetValue(CusInBondContainerSchema.PK);
			SetValue(containerRow, CusInBondContainerSchema.BC_ParentID, moveDetailPK);
			SetValue(containerRow, CusInBondContainerSchema.BC_ParentTableCode, CusInBondMoveDetailSchema.Constants.Prefix);
			SetValue(containerRow, CusInBondContainerSchema.BC_ContainerNum, dataObject.ContainerNumber);
			SetValue(containerRow, CusInBondContainerSchema.BC_Seal1, dataObject.Seal);
			SetValue(containerRow, CusInBondContainerSchema.BC_Seal2, dataObject.SecondSeal);
			SetValue(containerRow, CusInBondContainerSchema.BC_RC, dataObject.ContainerType);
			FillInBondSpecificData(containerRow);
			FillDataFromAddInfos(containerRow);
			FillCustomsReferenceData(containerRow);
			FillUNDGs(containerPK);
			FillCommodities(containerPK);
		}

		protected virtual void FillInBondSpecificData(IColumnIndexer containerRow)
		{
		}

		protected virtual void FillDataFromAddInfos(IColumnIndexer containerRow)
		{
		}

		protected virtual void FillCustomsReferenceData(IColumnIndexer containerRow)
		{
		}

		void FillUNDGs(ZGuid containerPK)
		{
			if (dataObject.UNDGCollection != null)
			{
				factory.Load<UNDGDataItem>(new ZQuery(UNDGDataItemSchema.DI_ParentID, containerPK)).DeleteAll();
				foreach (var undgData in dataObject.UNDGCollection)
				{
					var undgBO = new UNDGDataObjectReader(undgData, logger, factory).ReadIntoBusinessObject();
					var undgRow = GetColumnIndexer(undgBO);
					SetValue(undgRow, UNDGDataItemSchema.DI_ParentID, containerPK);
					SetValue(undgRow, UNDGDataItemSchema.DI_ParentTableCode, CusInBondContainerSchema.Constants.Prefix);
				}
			}
		}

		protected virtual void FillCommodities(ZGuid containerPK)
		{
			foreach (var packingLineData in readerHelper.GetPackingLineDetails(dataObject.Link))
			{
				InBondCargoDescDataObjectReader(packingLineData, logger, readerHelper, containerPK).ReadIntoBusinessObject();
			}
		}

		protected abstract CusInBondCargoDescDataObjectReader<TCommodity> InBondCargoDescDataObjectReader(PackingLine packingLineData, IXmlImportLogger logger, InBondDataObjectReaderHelper helper, ZGuid containerPK);
	}
}
