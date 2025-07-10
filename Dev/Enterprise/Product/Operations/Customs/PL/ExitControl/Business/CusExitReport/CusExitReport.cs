using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.ExitControl.Business;
using Enterprise.Customs.ExitControlBase.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.PL.ExitControl.Business;

public sealed class CusExitReport(BusinessObjectFactory factory, DataRow row) : EU.ExitControl.Business.CusExitReport(factory, row),
	Integration.Customs.PLExitControl.ICusExitReport
{
	public new ICusExitReportItemCollection<CusExitReportItem> CusExitReportItems => (ICusExitReportItemCollection<CusExitReportItem>)base.CusExitReportItems;

	public override ZString CER_TransportType
	{
		get => base.CER_TransportType;
		set
		{
			if (base.CER_TransportType == value)
			{
				return;
			}
			base.CER_TransportType = value;

			if (ReadOnlyByR0049E)
			{
				CER_TransportID = ZString.Empty;
			}
			if (CER_RN_NKTransportNationalityReadOnly)
			{
				CER_RN_NKTransportNationality = ZString.Empty;
			}
		}
	}

	[ReadOnlyMember(nameof(CER_TransportIDReadOnly))]
	public override ZString CER_TransportID
	{
		get => base.CER_TransportID;
		set => base.CER_TransportID = value;
	}

	[ReadOnlyMember(nameof(CER_RN_NKTransportNationalityReadOnly))]
	public override ZString CER_RN_NKTransportNationality
	{
		get => base.CER_RN_NKTransportNationality;
		set => base.CER_RN_NKTransportNationality = value;
	}

	[MaxLength(17)]
	[ResourceStringData("4C631DA8-23B4-4519-AD34-4506538CD362", MediumCaption = "Arr. Notif. Place", Caption = "Arrival Notification Place")]
	public override ZString CER_Location { get => base.CER_Location; set => base.CER_Location = value; }

	public new CusExitConsignment Consignment => (CusExitConsignment)base.Consignment;

	public new CusExitHeader Header => (CusExitHeader)base.Header;

	public bool CER_TransportIDReadOnly => ReadOnlyByR0049E;
	bool ReadOnlyByR0049E => ExitControlConstants.Rules.R0049ETransportTypeFirstNumbers.Contains(CER_TransportType.SubstringSafe(0, 1));

	public bool CER_RN_NKTransportNationalityReadOnly => ReadOnlyByR0050E;
	bool ReadOnlyByR0050E => ExitControlConstants.Rules.R0050ETransportTypeFirstNumbers.Contains(CER_TransportType.SubstringSafe(0, 1));

	protected override ICusExitReportItemCollection<ExitControlBase.Business.CusExitReportItem> CreateNewCusExitReportItemCollection(ZQuery filter) => new CusExitReportItemCollection<CusExitReportItem>(this, filter);

	protected override ICusExitReportItemCollection<EU.ExitControl.Business.CusExitReportItem> CreateNewCusExitReportItemPackageCollection() => new CusExitReportItemCollection<CusExitReportItem>(this, new ZQuery(CusExitReportItemSchema.ERI_CXP_Package, SQLComparisonOperator.NotEqual, ZGuid.Empty));

	protected override bool IsUCC6Core => Header?.IsUCC6 ?? true;

	#region ICusSupportingInfoTypeSupporter

	protected override IDictionary<ZString, Type> GetCusSupportingInfoTypes()
	{
		var result = base.GetCusSupportingInfoTypes();
		result[CusSupportingInfoTypeList.Codes.AdditionalInfo] = typeof(AdditionalInfo);
		return result;
	}

	#endregion

	protected override Dictionary<ZString, Type> GetCusCodeDataTypes()
	{
		var result = base.GetCusCodeDataTypes();
		result[EU.Business.CusCodeDataTypeList.Codes.AlternativeEvidence] = typeof(AlternativeEvidence);
		return result;
	}

	protected override IAdditionalInfoCollection<EU.ExitControl.Business.AdditionalInfo> CreateNewAdditionalInfoCollection() => new AdditionalInfoCollection<AdditionalInfo>(this);

	protected override IAlternativeEvidenceCollection<EU.ExitControl.Business.AlternativeEvidence> CreateNewAlternativeEvidenceCollection() => new AlternativeEvidenceCollection<AlternativeEvidence>(this);

	protected override void OnDiscrepanciesChanged()
	{
		SetReadOnlyIncludingChildrenForDiscrepancies();
	}

	public new CusExitReportValidation Validation => (CusExitReportValidation)base.Validation;

	protected override ExitControlBase.Business.CusExitReportValidation GetNewValidation() => new CusExitReportValidation(this);

	void SetReadOnlyIncludingChildrenForDiscrepancies()
	{
		AdditionalInfos.ForEach(x => x.SetReadOnlyIncludingChildren(DiscrepanciesNotTickedFieldsReadOnly
			&& x.CSI_SubType != EU.Business.AdditionalInfoSubTypeList.Codes.AdditionalInformation));
		foreach (var item in CusExitReportItemsForBinding)
		{
			item.SetReadOnlyIncludingChildren(DiscrepanciesNotTickedFieldsReadOnly);
		}
	}

	protected override ExitControlBase.Business.CusExitReportLookups GetNewLookups() => IsUCC6
		? new CusExitReportUcc6Lookups(this)
		: new Enterprise.Customs.EU.ExitControl.Business.CusExitReportLookups(this);
}
