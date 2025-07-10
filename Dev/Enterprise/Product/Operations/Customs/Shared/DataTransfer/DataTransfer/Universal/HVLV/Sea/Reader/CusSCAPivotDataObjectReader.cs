using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;
using UniversalPackLine = Enterprise.UniversalDataBuss.DataObjects.Universal.PackingLine;

namespace Enterprise.Customs.DataTransfer.Universal.SeaManifest
{
	public class CusSCAPivotDataObjectReader<TCusSCAPivot> : DataObjectReader<UniversalPackLine, TCusSCAPivot>
		where TCusSCAPivot : BaseCusSCAPivot
	{
		public CusSCAPivotDataObjectReader(ZInt lineNo, IColumnIndexer houseBill, HVLVShipmentDataObjectWrapper hvlvConsolidatorShipmentWrapper, UniversalPackLine data, IXmlImportLogger logger, UniversalObjectFactory factory)
			: base(data, logger, factory)
		{
			this.houseBill = houseBill;
			this.lineNo = lineNo;
			this.hvlvConsolidatorShipmentWrapper = hvlvConsolidatorShipmentWrapper;
			this.containerPK = GetContainerPK();
		}
		protected readonly IColumnIndexer houseBill;
		readonly ZGuid containerPK;
		readonly ZInt lineNo;
		protected readonly HVLVShipmentDataObjectWrapper hvlvConsolidatorShipmentWrapper;

		ZGuid GetContainerPK()
		{
			var result = ZGuid.Empty;
			var containerNum = GetContainerNumber().ToUpper();
			if (!containerNum.IsEmpty)
			{
				var query = new ZQuery(CusSCAContainerSchema.CN_ContainerNumber, containerNum);
				query.AddToFilter(CusSCAContainerSchema.CN_CB, houseBill.GetValue(CusSCAHouseSchema.CA_CB));
				query.MaximumRows = 1;
				var containers = factory.RowFactory.Load(CusSCAContainerSchema.Constants.TableName, query);
				if (containers.Length > 0)
				{
					result = GetColumnIndexerFromRow(containers[0]).GetValue(CusSCAContainerSchema.PK);
				}
			}
			return result;
		}

		protected virtual ZString GetContainerNumber()
		{
			return dataObject.ContainerNumber.GetValueOrDefault();
		}

		protected override TCusSCAPivot GetExistingBusinessObject()
		{
			return null;
		}

		protected sealed override void PopulateBusinessObject(TCusSCAPivot targetBO)
		{
			var packageBO = GetColumnIndexer(targetBO);
			SetValue(packageBO, CusSCAPivotSchema.CV_LineNo, lineNo);
			SetValue(packageBO, CusSCAPivotSchema.CV_CN, containerPK);
			SetValue(packageBO, CusSCAPivotSchema.CV_CA, houseBill.GetValue(CusSCAHouseSchema.PK));
			SetValue(packageBO, CusSCAPivotSchema.CV_PackageCount, dataObject.PackQty);
			SetValue(packageBO, CusSCAPivotSchema.CV_PackageType, ConvertCustomsPackageType(dataObject.PackType?.Code ?? ZString.Empty));
			SetValue(packageBO, CusSCAPivotSchema.CV_Weight, dataObject.Weight);
			SetValue(packageBO, CusSCAPivotSchema.CV_WeightUQ, dataObject.WeightUnit);
			SetValue(packageBO, CusSCAPivotSchema.CV_Volume, dataObject.Volume);
			SetValue(packageBO, CusSCAPivotSchema.CV_GoodsDescription, dataObject.GetCleanSingleLineGoodsDescription());
			SetValue(packageBO, CusSCAPivotSchema.CV_MarksAndNumbers, dataObject.MarksAndNos);
			SetValue(packageBO, CusSCAPivotSchema.CV_GoodsValue, dataObject.LinePrice);
			SetValue(packageBO, CusSCAPivotSchema.CV_RX_NKGoodsCurrency, dataObject.LinePriceCurrency.GetCodeAsUpperCase());
			SetValue(packageBO, CusSCAPivotSchema.CV_HarmonisedTariffNums, dataObject.HarmonisedCode);
			ReadUNDGCollection(targetBO);
			PopulateCountrySpecificData(targetBO);
		}

		protected virtual ZString ConvertCustomsPackageType(ZString packType)
		{
			return packType;
		}

		protected virtual void PopulateCountrySpecificData(TCusSCAPivot targetBO) { }

		protected override ZString GetReasonForNotAbleToUpdateFromDataSourceOrTargetBO(TCusSCAPivot targetBO)
		{
			var result = base.GetReasonForNotAbleToUpdateFromDataSourceOrTargetBO(targetBO);
			if (result.IsEmpty && RequireContainer && containerPK.IsEmpty)
			{
				result = Res.GetString("75AA601E-56D6-4CC7-A1CC-743D610B2A0C", "Cannot find any matching container for container number {0}", dataObject.ContainerNumber.GetValueOrDefault().ToUpper());
			}
			return result;
		}

		protected virtual bool RequireContainer => true;

		protected virtual void ReadUNDGCollection(TCusSCAPivot targetBO)
		{
		}
	}
}

// Tested in CusSCAOceanBillDataObjectReader.cs
