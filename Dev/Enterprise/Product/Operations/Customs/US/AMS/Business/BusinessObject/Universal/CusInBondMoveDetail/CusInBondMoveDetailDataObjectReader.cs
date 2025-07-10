using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.AMS.Business.Universal
{
	public class CusInBondMoveDetailDataObjectReader : DataTransfer.Universal.CusInBondMoveDetailDataObjectReader<CusInBondMoveDetail, CusInBondContainer, CusInBondCargoDesc>
	{
		public CusInBondMoveDetailDataObjectReader(Shipment parentDataObject, InBondMoveDetail dataObject, IXmlImportLogger logger, InBondDataObjectReaderHelper helper, CusInBondHeader header, ZGuid moveHeaderPK, ZGuid billPK, ZString inBondNumber)
			: base(dataObject, logger, helper, moveHeaderPK, billPK, inBondNumber)
		{
			this.parentDataObject = parentDataObject;
			this.header = Argument.NotNull(header, "header");
		}
		readonly Shipment parentDataObject;
		readonly CusInBondHeader header;

		protected new InBondDataObjectReaderHelper Helper
		{
			get { return (InBondDataObjectReaderHelper)base.Helper; }
		}

		protected override CusInBondMoveDetail GetExistingBusinessObject()
		{
			Helper.ThrowReadFailureExceptionWhenNonSupportedElementsFound(parentDataObject, elementsNotSupportedList =>
			{
				void AddElementToListIfNotSupported(CodeDescriptionPair codeDescriptionPair, string nameOfElement)
				{
					if (codeDescriptionPair != null && !codeDescriptionPair.Code.GetValueOrDefault().IsEmpty)
					{
						elementsNotSupportedList.Add(nameOfElement);
					}
				}

				AddElementToListIfNotSupported(dataObject.CustomsStatus, nameof(dataObject.CustomsStatus));
				AddElementToListIfNotSupported(dataObject.MessageStatus, nameof(dataObject.MessageStatus));
			});

			var zQuery = new ZQuery(CusInBondMoveDetailSchema.B9_BM, moveHeaderPK);
			zQuery.AddToFilter(CusInBondMoveDetailSchema.B9_B0, billPK);
			zQuery.FetchOnlyFromLocalCache = !header.IsInDatabase;
			return factory.LoadTop1<CusInBondMoveDetail>(zQuery);
		}

		protected override void FillInBondSpecificData(IColumnIndexer moveDetailRow)
		{
			base.FillInBondSpecificData(moveDetailRow);
			SetValue(moveDetailRow, CusInBondMoveDetailSchema.B9_InBoundQty, dataObject.InBondQuantity);
			SetValue(moveDetailRow, CusInBondMoveDetailSchema.B9_MonetaryValue, dataObject.MonetaryValue);
			SetValue(moveDetailRow, CusInBondMoveDetailSchema.B9_ForeignDestPortKCode, dataObject.ForeignDestPortScheduleK);
			SetValue(moveDetailRow, CusInBondMoveDetailSchema.B9_ExportDate, dataObject.ExportDate);
			SetValue(moveDetailRow, CusInBondMoveDetailSchema.B9_ExportLadenOn, dataObject.ExportVesselName);
		}

		protected override void FillContainers(IColumnIndexer moveDetailRow, CusInBondMoveDetail moveDetailBO)
		{
			if (dataObject.ContainerLinkCollection != null)
			{
				Helper.MarkUnprocessedExistingContainersFor(moveDetailBO);
				var moveDetailPK = moveDetailRow.GetValue(CusInBondMoveDetailSchema.PK);
				foreach (var containerLink in dataObject.ContainerLinkCollection)
				{
					var containerData = Helper.GetContainer(containerLink.Link);
					if (containerData == null)
					{
						LogInBondContainerNotMatched(containerLink);
					}
					else
					{
						var containerRead = InBondContainerDataObjectReader(containerData, logger, Helper, moveDetailPK).ReadIntoBusinessObject();
						Helper.MarkProcessed(containerRead);
					}
				}
				Helper.DeleteUnprocessedContainersFor(moveDetailBO, logger);
			}
		}

		protected override DataTransfer.Universal.CusInBondContainerDataObjectReader<CusInBondContainer, CusInBondCargoDesc> InBondContainerDataObjectReader(Container containerData, IXmlImportLogger logger, DataTransfer.Universal.InBondDataObjectReaderHelper helper, ZGuid moveDetailPK)
		{
			return new CusInBondContainerDataObjectReader(containerData, logger, Helper, header, moveDetailPK);
		}
	}
}
