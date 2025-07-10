using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.Types;
using Enterprise.Customs.US.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.DataTransfer.Universal
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1005:AvoidExcessiveParametersOnGenericTypes")]
	public abstract class CusInBondMoveDetailDataObjectReader<TMoveDetail, TContainer, TCommodity> : DataObjectReader<InBondMoveDetail, TMoveDetail>
		where TMoveDetail : CusInBondMoveDetail
		where TContainer : Customs.Business.CusInBondContainer
		where TCommodity : Customs.Business.CusInBondCargoDesc
	{
		protected CusInBondMoveDetailDataObjectReader(InBondMoveDetail dataObject, IXmlImportLogger logger, InBondDataObjectReaderHelper helper, ZGuid moveHeaderPK, ZGuid billPK, ZString inBondNo)
			: base(dataObject, logger, helper.Factory)
		{
			this.moveHeaderPK = Argument.NotNull(moveHeaderPK, "moveHeaderPK");
			this.billPK = Argument.NotNull(billPK, "billPK");
			this.readerHelper = Argument.NotNull(helper, "helper");
			this.inBondNumber = inBondNo;
		}
		protected readonly ZGuid moveHeaderPK;
		protected readonly ZGuid billPK;
		readonly InBondDataObjectReaderHelper readerHelper;
		readonly ZString inBondNumber;

		protected InBondDataObjectReaderHelper Helper
		{
			get { return readerHelper; }
		}

		protected override TMoveDetail GetExistingBusinessObject()
		{
			return null;
		}

		protected override void PopulateBusinessObject(TMoveDetail moveDetailBO)
		{
			var moveDetailRow = GetColumnIndexer(moveDetailBO);
			SetValue(moveDetailRow, CusInBondMoveDetailSchema.B9_BM, moveHeaderPK);
			SetValue(moveDetailRow, CusInBondMoveDetailSchema.B9_B0, billPK);
			SetValue(moveDetailRow, CusInBondMoveDetailSchema.B9_InBoundQty, dataObject.InBondQuantity);
			FillPreviousITNumber(moveDetailRow);
			FillContainers(moveDetailRow, moveDetailBO);
			FillInBondSpecificData(moveDetailRow);
		}

		void FillPreviousITNumber(IColumnIndexer moveDetailRow)
		{
			if (dataObject.EntryNumberCollection != null)
			{
				var previousITNumberData = dataObject.EntryNumberCollection.FirstOrDefault(x => x.Type.GetCodeAsUpperCase() == Constants.CusInBond.MoveDetail.NumberTypes.PreviousInBondNumber && x.CountryOfIssue.GetCodeAsUpperCase() == Core.Constants.CountryCodes.UnitedStates);
				if (previousITNumberData != null)
				{
					SetValue(moveDetailRow, CusInBondMoveDetailSchema.B9_PreviousITNumber, previousITNumberData.Number ?? ZString.Empty);
				}
			}
		}

		protected virtual void FillContainers(IColumnIndexer moveDetailRow, TMoveDetail moveDetailBO)
		{
			if (dataObject.ContainerLinkCollection != null)
			{
				var moveDetailPK = moveDetailRow.GetValue(CusInBondMoveDetailSchema.PK);
				foreach (var containerLink in dataObject.ContainerLinkCollection)
				{
					var containerData = readerHelper.GetContainer(containerLink.Link);
					if (containerData == null)
					{
						LogInBondContainerNotMatched(containerLink);
					}
					else
					{
						InBondContainerDataObjectReader(containerData, logger, readerHelper, moveDetailPK).ReadIntoBusinessObject();
					}
				}
			}
		}

		protected virtual void FillInBondSpecificData(IColumnIndexer moveDetailRow)
		{
		}

		protected virtual void LogInBondContainerNotMatched(ContainerLink containerLink)
		{
			logger.Log(Integration.LogType.Error, ZString.Format("Cannot process Container detail for In-Bond Movement '{0}' as Container with link = '{1}' is missing", inBondNumber, containerLink.Link));
		}

		protected abstract CusInBondContainerDataObjectReader<TContainer, TCommodity> InBondContainerDataObjectReader(Container containerData, IXmlImportLogger logger, InBondDataObjectReaderHelper helper, ZGuid moveDetailPK);
	}
}
