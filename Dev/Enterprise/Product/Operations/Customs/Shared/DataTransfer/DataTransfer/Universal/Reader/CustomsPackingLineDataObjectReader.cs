using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.DataTransfer.Universal
{
	public class CustomsPackingLineDataObjectReader : DataObjectReader<PackingLine, BasePackage>
	{
		public CustomsPackingLineDataObjectReader(PackingLine packingLineDataObject, IXmlImportLogger logger, UniversalDataObjectReaderHelper helper, BaseJobDeclaration declaration, IColumnIndexer billRow, bool supportsParentPackage)
			: base(packingLineDataObject, logger, helper.Factory)
		{
			this.declaration = Argument.NotNull(declaration, "BaseJobDeclaration declaration");
			this.helper = helper;
			this.billRow = Argument.NotNull(billRow, "billRow");
			this.supportsParentPackage = supportsParentPackage;
		}
		readonly bool supportsParentPackage;
		readonly BaseJobDeclaration declaration;
		readonly IColumnIndexer billRow;
		protected readonly UniversalDataObjectReaderHelper helper;

		protected sealed override BasePackage GetExistingBusinessObject()
		{
			return null;
		}

		protected override BasePackage GetNewBusinessObject()
		{
			return declaration.Packages.AddNew();
		}

		protected sealed override void PopulateBusinessObject(BasePackage packingLine)
		{
			if (ShouldRevertPackLine(dataObject))
			{
				IColumnIndexer containerRow = null;
				var shouldDeleteContainers = declaration.ShouldDeleteContainers;
				if (!shouldDeleteContainers)
				{
					var containerNumber = dataObject.ContainerNumber.GetValueOrDefault();
					if (!containerNumber.IsEmpty)
					{
						containerRow = GetColumnIndexerFromRow(FindCusContainerByContainerNumber(containerNumber));
						if (containerRow == null)
						{
							logger.Log(Integration.LogType.Warning, Enterprise.Customs.DataTransfer.Res.GetString("911AF012-2ECE-4302-888E-9B4A39E521BC", "Cannot find Container ({0}) for packing line; new Container added.", containerNumber));
							containerRow = GetColumnIndexer(declaration.CusContainers.AddNew());
							SetValue(containerRow, CusContainerSchema.CO_ContainerNumber, containerNumber);
						}
					}
				}
				var billPK = billRow != null ? billRow.GetValue(CusDecHouseBillSchema.PK) : ZGuid.Empty;
				var containerPK = containerRow != null ? containerRow.GetValue(CusContainerSchema.PK) : ZGuid.Empty;
				var packingGroupRow = GetColumnIndexerFromRow(FindCusDecHouseContainerPivotByHouseBillAndContainer(billPK, containerPK, shouldDeleteContainers));
				if (packingGroupRow == null)
				{
					var newPackingGroup = declaration.PackingGroups.AddNew();
					if (!containerPK.IsEmpty)
					{
						newPackingGroup.CR_CO_Container = containerPK;
					}
					newPackingGroup.CR_CU_HouseBill = billPK;
					packingGroupRow = GetColumnIndexer(newPackingGroup);
				}
				packingLine.CW_CR_HouseContainer = packingGroupRow.GetValue(CusDecHouseContainerPivotSchema.PK);
				var packingLineRow = GetColumnIndexer(packingLine);
				SetValue(packingLineRow, CusDecHouseContainerPackSchema.CW_MarksAndNos, dataObject.MarksAndNos);
				SetValue(packingLineRow, CusDecHouseContainerPackSchema.CW_PackQty, dataObject.PackQty.HasValue ? dataObject.PackQty.Value.ToZInt() : null);
				SetValue(packingLineRow, CusDecHouseContainerPackSchema.CW_InBondPackQty, dataObject.InBondPackQty);
				SetValue(packingLineRow, CusDecHouseContainerPackSchema.CW_ShippingSymbol, dataObject.ShippingSymbol);
				SetValue(packingLineRow, CusDecHouseContainerPackSchema.CW_OuterPacks, dataObject.CustomsOuterPacks);
				SetValue(packingLineRow, CusDecHouseContainerPackSchema.CW_PackType, helper.GetCustomsUnitForPackType(dataObject.PackType));
				helper.AddContainerInvoiceLineMap(containerPK, dataObject.PackedItemCollection);
				helper.AddPackageInvoiceLineMap(packingLine.PK, dataObject.PackedItemCollection);
				helper.AddPackageLinkMap(packingLine.PK, dataObject.Link.GetValueOrDefault());
				PopulateChildrenPackingLine(packingLine);
			}
		}

		protected virtual bool ShouldRevertPackLine(PackingLine packingLine) => true;

		void PopulateChildrenPackingLine(BasePackage packingLine)
		{
			if (supportsParentPackage && dataObject.PackingLineCollection != null)
			{
				foreach (var childPackingLineDataObject in dataObject.PackingLineCollection)
				{
					var childPackingLine = new CustomsPackingLineDataObjectReader(childPackingLineDataObject, logger, helper, declaration, billRow, true).ReadIntoBusinessObject();
					SetValue(childPackingLine, CusDecHouseContainerPackSchema.CW_CW_Parent, packingLine.PK);
				}
			}
		}

		DataRow FindCusDecHouseContainerPivotByHouseBillAndContainer(ZGuid billPK, ZGuid containerPK, bool shouldDeleteContainers)
		{
			var query = new ZQuery(CusDecHouseContainerPivotSchema.CR_CU_HouseBill, billPK);
			if (containerPK.IsEmpty)
			{
				if (!shouldDeleteContainers)
				{
					query.AddToFilter(CusDecHouseContainerPivotSchema.CR_CO_Container, null);
				}
			}
			else
			{
				query.AddToFilter(CusDecHouseContainerPivotSchema.CR_CO_Container, containerPK);
			}
			query.FetchOnlyFromLocalCache = !declaration.IsInDatabase;
			var rows = factory.RowFactory.Load(CusDecHouseContainerPivotSchema.Constants.TableName, query);
			DataRow result = null;
			if (rows.Length > 0)
			{
				if (containerPK.IsEmpty && shouldDeleteContainers)
				{
					result = rows.FirstOrDefault(x => GetColumnIndexerFromRow(x).GetValue(CusDecHouseContainerPivotSchema.CR_CO_Container).IsEmpty);
				}
				if (result == null)
				{
					result = rows[0];
				}
			}
			return result;
		}

		DataRow FindCusContainerByContainerNumber(ZString containerNumber)
		{
			var query = new ZQuery(CusContainerSchema.CO_JE, declaration.PK);
			query.AddToFilter(CusContainerSchema.CO_ContainerNumber, containerNumber);
			query.MaximumRows = 1;
			var rows = factory.RowFactory.Load(CusContainerSchema.Constants.TableName, query);
			return rows.Length > 0 ? rows[0] : null;
		}
	}
}
