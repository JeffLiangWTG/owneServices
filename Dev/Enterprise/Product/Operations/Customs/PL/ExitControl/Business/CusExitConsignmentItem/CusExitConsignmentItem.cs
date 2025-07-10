using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.Extensions;
using Enterprise.Customs.EU.ExitControl.Business;
using Enterprise.Customs.ExitControlBase.Business;

namespace Enterprise.Customs.PL.ExitControl.Business;

public class CusExitConsignmentItem : EU.ExitControl.Business.CusExitConsignmentItem
		, Integration.Customs.PLExitControl.ICusExitConsignmentItem
		, Integration.Customs.ICusSupportingInfoTypeSupporter
{
	public CusExitConsignmentItem(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}

	protected override ICusExitReportItemCollection<ExitControlBase.Business.CusExitReportItem> CreateNewCusExitReportItemCollection() => new CusExitReportItemCollection<CusExitReportItem>(this);

	protected override ICusExitConsignmentPivotCollection<ExitControlBase.Business.CusExitConsignmentPivot> CreateNewCusExitConsignmentPivotCollection() => new CusExitConsignmentPivotCollection<CusExitConsignmentPivot>(this);

	protected override ICusExitConsignmentPivotCollection<ExitControlBase.Business.CusExitConsignmentPivot> CreateNewCusExitConsignmentPackagePivotCollection()
	{
		var collection = new CusExitConsignmentPivotCollection<CusExitConsignmentPivot>(this, ConsignmentItemPivotType.Package);
		collection.ApplySort(nameof(CusExitConsignmentPivot.SequenceNumber), System.ComponentModel.ListSortDirection.Ascending);
		return collection;
	}

	protected override ICusExitConsignmentPivotCollection<ExitControlBase.Business.CusExitConsignmentPivot> CreateNewCusExitConsignmentContainerPivotCollection() => new CusExitConsignmentPivotCollection<CusExitConsignmentPivot>(this, ConsignmentItemPivotType.Container);

	public override void Delete()
	{
		if (!IsDeleted)
		{
			FetchForLoadChildEditableObjectsIfNeeded();
			this.DeleteAllCusAddInfoCodeDataAndSupportingInfoChildrenIfSupported();
		}
		base.Delete();
	}

	protected override bool IsUCC6Core => Consignment?.IsUCC6 ?? true;

	#region ICusSupportingInfoTypeSupporter Implementation

	IDictionary<ZString, Type> Integration.Customs.ICusSupportingInfoTypeSupporter.GetCusSupportingInfoTypes()
	{
		var result = new Dictionary<ZString, Type>
			{
				{ Common.EU.CusSupportingInfoTypeList.Codes.AdditionalInfo, typeof(AdditionalInfo) }
			};
		return result;
	}

	#endregion

	protected override IAdditionalInfoCollection<EU.ExitControl.Business.AdditionalInfo> CreateNewAdditionalInfoCollection() => new AdditionalInfoCollection<AdditionalInfo>(this);
}
