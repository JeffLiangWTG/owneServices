using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using Enterprise.Customs.US.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.US.DataTransfer.Universal
{
	public class CusInBondMoveDetailDataObjectWriter : DataObjectWriter<CusInBondMoveDetail, InBondMoveDetail>
	{
		public CusInBondMoveDetailDataObjectWriter(IDataWritingManager writeManager, InBondDataObjectWriterHelper helper, Shipment headerData)
			: base(writeManager)
		{
			this.writerHelper = Argument.NotNull(helper, "helper");
			this.headerData = Argument.NotNull(headerData, "headerData");
		}
		readonly InBondDataObjectWriterHelper writerHelper;
		readonly Shipment headerData;

		protected InBondDataObjectWriterHelper Helper
		{
			get { return writerHelper; }
		}

		protected Shipment HeaderData
		{
			get { return headerData; }
		}

		protected override InBondMoveDetail PopulateDataObject(CusInBondMoveDetail moveDetailBO)
		{
			var moveDetailData = new InBondMoveDetail(writeManager.WriterStrategy);
			moveDetailData.CustomsStatus = ListHelper.GetWithDescription<CodeDescriptionPair>(moveDetailBO.B9_CustomsStatus, moveDetailBO.Lookups.MessageStatusList);
			PopulatePreviousInBondNumber(moveDetailBO, moveDetailData);
			PopulateInBondSpecificData(moveDetailBO, moveDetailData);
			var billBO = moveDetailBO.Bill;
			if (billBO != null)
			{
				Helper.SetBillLink(billBO, moveDetailData);
			}
			return moveDetailData;
		}

		protected virtual void PopulatePreviousInBondNumber(CusInBondMoveDetail moveDetailBO, InBondMoveDetail moveDetailData)
		{
			var entryNumberCollection = moveDetailData.EntryNumberCollection ?? new List<UniversalDataBuss.DataObjects.Universal.EntryNumber>();
			entryNumberCollection.Add(new UniversalDataBuss.DataObjects.Universal.EntryNumber()
			{
				Type = new EntryType() { Code = Constants.CusInBond.MoveDetail.NumberTypes.PreviousInBondNumber, Description = Constants.CusInBond.MoveDetail.NumberTypes.PreviousInBondNumberDescription },
				Number = moveDetailBO.B9_PreviousITNumber,
				CountryOfIssue = new Country() { Code = Core.Constants.CountryCodes.UnitedStates, Name = Constants.CusInBond.CountryDescrption.UnitedStates }
			});
			moveDetailData.EntryNumberCollection = entryNumberCollection;
		}

		protected virtual void PopulateInBondSpecificData(CusInBondMoveDetail moveDetailBO, InBondMoveDetail moveDetailData)
		{
		}

		protected void PopulateContainers(CusInBondMoveDetail moveDetailBO, InBondMoveDetail moveDetailData)
		{
			var containersCollection = moveDetailData.ContainerLinkCollection ?? new List<ContainerLink>();
			var containerBOs = GetRelatedContainers(moveDetailBO, Helper);
			if (containerBOs.Any())
			{
				var containerDataWriter = InBondContainerDataObjectWriter(writeManager, Helper);
				var commodityDataWriter = InBondCargoDescDataObjectWriter(writeManager, Helper);
				foreach (var containerBO in containerBOs)
				{
					headerData.SetContainerCollection(() =>
					{
						var containerData = containerDataWriter.GetDataObject(containerBO);
						var containerCollection = headerData.ContainerCollection ?? new DataObjectList<Container>();
						containerCollection.Add(containerData);
						PopulateCommodities(containerBO, containerData, commodityDataWriter);
						containersCollection.Add(new ContainerLink() { Link = containerData.Link, ContainerNumber = containerData.ContainerNumber });
						return containerCollection;
					});
				}
			}
			moveDetailData.ContainerLinkCollection = containersCollection;
		}

		protected virtual IEnumerable<Customs.Business.CusInBondContainer> GetRelatedContainers(CusInBondMoveDetail moveDetailBO, InBondDataObjectWriterHelper helper)
		{
			return helper.Load<Customs.Business.CusInBondContainer>(moveDetailBO.Containers.CompleteFilter).OrderBy(x => x.BC_ContainerNum);
		}

		protected virtual CusInBondContainerDataObjectWriter InBondContainerDataObjectWriter(IDataWritingManager writeManager, InBondDataObjectWriterHelper helper)
		{
			return new CusInBondContainerDataObjectWriter(writeManager, helper);
		}

		protected virtual CusInBondCargoDescDataObjectWriter InBondCargoDescDataObjectWriter(IDataWritingManager writeManager, InBondDataObjectWriterHelper helper)
		{
			return new CusInBondCargoDescDataObjectWriter(writeManager, helper);
		}

		protected virtual void PopulateCommodities(Customs.Business.CusInBondContainer containerBO, Container containerData, CusInBondCargoDescDataObjectWriter commodityDataWriter)
		{
			var commodityBOs = GetRelatedCommotities(containerBO, writerHelper);
			foreach (var commodityBO in commodityBOs)
			{
				headerData.SetPackingLineCollection(() =>
				{
					var commodityData = commodityDataWriter.GetDataObject(commodityBO);
					commodityData.ContainerLink = containerData.Link;
					var packingLineCollection = headerData.PackingLineCollection ?? new DataObjectList<PackingLine>();
					packingLineCollection.Add(commodityData);
					return packingLineCollection;
				});
			}
		}

		protected virtual IEnumerable<Customs.Business.CusInBondCargoDesc> GetRelatedCommotities(Customs.Business.CusInBondContainer containerBO, InBondDataObjectWriterHelper helper)
		{
			return System.Array.Empty<Customs.Business.CusInBondCargoDesc>();
		}
	}
}
