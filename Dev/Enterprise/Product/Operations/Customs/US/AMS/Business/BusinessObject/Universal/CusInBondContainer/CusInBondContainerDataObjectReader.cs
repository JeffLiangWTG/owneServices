using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.AMS.Business.Universal
{
	public class CusInBondContainerDataObjectReader : DataTransfer.Universal.CusInBondContainerDataObjectReader<CusInBondContainer, CusInBondCargoDesc>
	{
		public CusInBondContainerDataObjectReader(Container containerDataObject, IXmlImportLogger logger, InBondDataObjectReaderHelper helper, CusInBondHeader header, ZGuid moveDetailPK)
			: base(containerDataObject, logger, helper, moveDetailPK)
		{
			this.header = Argument.NotNull(header, "header");
		}

		readonly CusInBondHeader header;

		protected new InBondDataObjectReaderHelper Helper
		{
			get { return (InBondDataObjectReaderHelper)base.Helper; }
		}

		protected override CusInBondContainer GetExistingBusinessObject()
		{
			CusInBondContainer result = null;
			if (dataObject.ContainerNumber.HasValue)
			{
				var query = new ZQuery(CusInBondContainerSchema.BC_ParentID, moveDetailPK);
				query.AddToFilter(CusInBondContainerSchema.BC_ContainerNum, dataObject.ContainerNumber.Value);
				query.FetchOnlyFromLocalCache = !header.IsInDatabase;
				result = factory.LoadTop1<CusInBondContainer>(query);
			}
			return result;
		}

		protected override void FillInBondSpecificData(IColumnIndexer containerRow)
		{
			base.FillInBondSpecificData(containerRow);
			SetValue(containerRow, CusInBondContainerSchema.BC_IsEmpty, dataObject.IsEmptyContainer);
		}

		protected override void FillDataFromAddInfos(IColumnIndexer containerRow)
		{
			SetValue(containerRow, CusInBondContainerSchema.BC_RL_NKForeignPort, dataObject.AddInfoCollection.GetZStringValue(Constants.Container.AddInfo.ForeignPort));
			SetValue(containerRow, CusInBondContainerSchema.BC_ForeignPortKCode, dataObject.AddInfoCollection.GetZStringValue(Constants.Container.AddInfo.ForeignPortScheduleK));
			SetValue(containerRow, CusInBondContainerSchema.BC_TypeOfService, dataObject.AddInfoCollection.GetZStringValue(Constants.Container.AddInfo.TypeOfService));
		}

		protected override void FillCustomsReferenceData(IColumnIndexer containerRow)
		{
			if (dataObject.CustomsReferenceCollection != null)
			{
				var containerPK = containerRow.GetValue(CusInBondContainerSchema.PK);
				var vehicleCtrlCollection = new List<CusInBondVehicleCtrl>(factory.Load<CusInBondVehicleCtrl>(new ZQuery(CusInBondVehicleCtrlSchema.BV_BC, containerPK)));
				var referenceCollection = dataObject.CustomsReferenceCollection.Where(x => x.Type.GetCodeAsUpperCase() == Constants.VehicleReference.Type && x.Reference.HasValue);
				if (referenceCollection.Any())
				{
					foreach (var reference in referenceCollection)
					{
						var foundCount = vehicleCtrlCollection.RemoveAll(item => item.BV_VIN.Equals(reference.Reference));
						if (foundCount == 0)
						{
							var referenceRow = GetColumnIndexer(factory.New<CusInBondVehicleCtrl>());
							SetValue(referenceRow, CusInBondVehicleCtrlSchema.BV_BC, containerPK);
							SetValue(referenceRow, CusInBondVehicleCtrlSchema.BV_VIN, reference.Reference);
						}
					}
				}
				if (vehicleCtrlCollection.Count > 0)
				{
					vehicleCtrlCollection.DeleteAll();
				}
			}
		}

		protected override DataTransfer.Universal.CusInBondCargoDescDataObjectReader<CusInBondCargoDesc> InBondCargoDescDataObjectReader(PackingLine packingLineData, IXmlImportLogger logger, DataTransfer.Universal.InBondDataObjectReaderHelper helper, ZGuid containerPK)
		{
			return new CusInBondCargoDescDataObjectReader(packingLineData, logger, Helper, containerPK);
		}

		protected override void FillCommodities(ZGuid containerPK)
		{
			if (Helper.IsPackingLineExist(dataObject.Link))
			{
				factory.Load<CusInBondCargoDesc>(new ZQuery(CusInBondCargoDescSchema.BY_ParentID, containerPK)).DeleteAll();
				base.FillCommodities(containerPK);
			}
		}
	}
}
