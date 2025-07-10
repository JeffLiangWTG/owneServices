using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Extensions;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Packing.Business;
using Enterprise.TransportConsignment.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.TransportConsignment.DataTransfer.Universal
{
	class DtbConsignmentActionDataObjectReader : DataObjectReader<Confirmation, DtbConsignmentAction>
	{
		readonly Dictionary<ZInt, PkgPackage> packageLinksDictionary;
		readonly Dictionary<ZInt, PkgPackage> containerLinksDictionary;

		public DtbConsignmentActionDataObjectReader(Confirmation actionDataObject, IXmlImportLogger logger, UniversalObjectFactory factory, Dictionary<ZInt, PkgPackage> packageLinksDictionary, Dictionary<ZInt, PkgPackage> containerLinksDictionary)
			: base(actionDataObject, logger, factory)
		{
			this.packageLinksDictionary = packageLinksDictionary;
			this.containerLinksDictionary = containerLinksDictionary;
		}

		protected override DtbConsignmentAction GetExistingBusinessObject()
		{
			return null;
		}

		protected override void PopulateBusinessObject(DtbConsignmentAction action)
		{
			var row = GetColumnIndexerFromRow(action);
			SetValue(row, DtbConsignmentActionSchema.LTA_ActualTime, dataObject.ActualDate.HasValue ? new ZDateTimeOffset(dataObject.ActualDate.Value) : ZDateTimeOffset.Empty);
			SetValue(row, DtbConsignmentActionSchema.LTA_ActionType, dataObject.DateDescription);
			SetValue(row, DtbConsignmentActionSchema.LTA_EstimatedTime, dataObject.EstimatedDate.HasValue ? new ZDateTimeOffset(dataObject.EstimatedDate.Value) : ZDateTimeOffset.Empty);
			SetValue(row, DtbConsignmentActionSchema.LTA_Slot, dataObject.SlotDate.HasValue ? new ZDateTimeOffset(dataObject.SlotDate.Value) : ZDateTimeOffset.Empty);

			SetValue(row, DtbConsignmentActionSchema.LTA_RequiredTo, dataObject.RequiredToDate.HasValue ? new ZDateTimeOffset(dataObject.RequiredToDate.Value) : ZDateTimeOffset.Empty);

			SetValue(row, DtbConsignmentActionSchema.LTA_SignedBy, dataObject.ReceivedBy);
			SetValue(row, DtbConsignmentActionSchema.LTA_ReferenceNumber, dataObject.Reference);

			if (dataObject.RequiredFromDate.HasValue)
			{
				SetValue(row, DtbConsignmentActionSchema.LTA_RequiredFrom, new ZDateTimeOffset(dataObject.RequiredFromDate.Value));
			}
			else
			{
				SetValue(row, DtbConsignmentActionSchema.LTA_RequiredFrom, dataObject.EstimatedDate.HasValue ? new ZDateTimeOffset(dataObject.EstimatedDate.Value) : ZDateTimeOffset.Empty);
			}

			if (dataObject.PackingLinkCollection?.Any() ?? false)
			{
				AddActionPackageDivot(dataObject.PackingLinkCollection, action);
			}
		}

		void AddActionPackageDivot(List<PackingLink> packingLinks, DtbConsignmentAction dtbConsignmentAction)
		{
			foreach (var packingLink in packingLinks)
			{
				if (dtbConsignmentAction == null || !packingLink.PackingLineLink.HasValue)
				{
					continue;
				}

				var linkValue = packingLink.PackingLineLink.Value;
				var isContainer = packingLink.IsContainer ?? false;
				var packageDict = isContainer ? containerLinksDictionary : packageLinksDictionary;

				if (!packageDict.TryGetValue(linkValue, out var package) || package?.PK == null)
				{
					continue;
				}

				var quantity = (ZInt)packingLink.PackedQuantity;
				var consignmentActionPackageDivotRow = factory.RowFactory.NewRowWithPK(DtbConsignmentActionPackageDivotSchema.Instance);
				var actionPK = GetColumnIndexerFromRow(dtbConsignmentAction).GetValue(DtbConsignmentActionSchema.PK);
				var operationTime = ZDateTime.UtcNow;

				SetValue(consignmentActionPackageDivotRow, DtbConsignmentActionPackageDivotSchema.LTP_LTA_ConsignmentAction, actionPK);
				SetValue(consignmentActionPackageDivotRow, DtbConsignmentActionPackageDivotSchema.LTP_KP_Package, package.PK);
				SetValue(consignmentActionPackageDivotRow, DtbConsignmentActionPackageDivotSchema.LTP_PackageQuantity, quantity);
				SetValue(consignmentActionPackageDivotRow, DtbConsignmentActionPackageDivotSchema.LTP_SystemCreateTimeUtc, operationTime);
				SetValue(consignmentActionPackageDivotRow, DtbConsignmentActionPackageDivotSchema.LTP_SystemCreateUser, GlbStaff.CurrentUser.GS_Code);
				SetValue(consignmentActionPackageDivotRow, DtbConsignmentActionPackageDivotSchema.LTP_SystemLastEditTimeUtc, operationTime);
				SetValue(consignmentActionPackageDivotRow, DtbConsignmentActionPackageDivotSchema.LTP_SystemLastEditUser, GlbStaff.CurrentUser.GS_Code);
			}
		}
	}
}


