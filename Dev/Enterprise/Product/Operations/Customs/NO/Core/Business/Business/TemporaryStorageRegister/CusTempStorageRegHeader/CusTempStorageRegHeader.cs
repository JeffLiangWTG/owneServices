using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Customs.NO.Business;

[CodeProperty(CusTempStorageRegHeader.Schema.SRH_Reference)]
public class CusTempStorageRegHeader(BusinessObjectFactory factory, DataRow row) : EFTA.TemporaryStorageRegister.Business.CusTempStorageRegHeader(factory, row),
	IRelatedJob,
	IInvoicesProviderValueChangedAnnouncerProvider,
	IGoodsRegistrationNumberManager
{
	public new CusTempStorageRegHeaderValidation Validation => base.Validation as CusTempStorageRegHeaderValidation;

	public new CusTempStorageRegHeaderLookups Lookups => base.Lookups as CusTempStorageRegHeaderLookups;

	protected override EFTA.TemporaryStorageRegister.Business.CusTempStorageRegHeaderLookups GetNewLookups() => new CusTempStorageRegHeaderLookups(this);

	public static CusTempStorageRegHeader Load(BusinessObjectFactory factory, string reference, string mrn = null)
	{
		CusTempStorageRegHeader result = null;
		if (factory != null && (!string.IsNullOrEmpty(reference) || !string.IsNullOrEmpty(mrn)))
		{
			result = factory.Load<CusTempStorageRegHeader>(GetLoadQuery(reference, mrn)).OrderBy(x => x.SRH_SystemCreateTimeUtc).FirstOrDefault();
		}
		return result;
	}

	[BusinessObjectTestExclude]
	[ResourceStringData("5ADD1676-5FAA-4B68-9597-5EFDDB03A9EE", Caption = "Goods Number")]
	public override ZString SRH_Reference
	{
		get => base.SRH_Reference;
		set => base.SRH_Reference = value;
	}

	void SetNextGoodsRegistrationNumber(string referenceNumber)
	{
		SRH_Reference = referenceNumber; 
	}

	void IGoodsRegistrationNumberManager.SetNextGoodsRegistrationNumber(string goodsRegistrationNumber) => SetNextGoodsRegistrationNumber(goodsRegistrationNumber);

	[List(nameof(Lookups) + "." + nameof(CusTempStorageRegHeaderLookups.PreviousReferenceTypeList))]
	public override ZString SRH_PreviousReferenceType
	{
		get => base.SRH_PreviousReferenceType;
		set => base.SRH_PreviousReferenceType = value;
	}

	[List(nameof(Lookups) + "." + nameof(CusTempStorageRegHeaderLookups.StatusList))]
	public override ZString SRH_Status
	{
		get => base.SRH_Status;
		set => base.SRH_Status = value;
	}

	[ResourceStringData("A2089A3E-04DF-4ABB-8655-854FDFC725F5", Caption = "Customer Reference")]
	[ReadOnly(true)]
	public override ZString SRH_InternalReference
	{
		get => base.SRH_InternalReference;
		set => base.SRH_InternalReference = value;
	}

	[MaxLength(100)]
	[ResourceStringData("0CCEAC31-0582-47D6-93AB-C748C8470041", Caption = "Unloading Remarks", FullDescription = "Unloading Remarks when not NCTS. Note: Full formal remarks to be entered in portal Altlnn.")]
	public ZString UnloadingRemarks
	{
		get => GenAddOnColumnUnloadingRemarks.XA_Data;
		set
		{
			var oldValue = UnloadingRemarks;
			if (oldValue != value)
			{
				CheckMaximumLength(UnloadingRemarksInfo, value);
				GenAddOnColumnUnloadingRemarks.XA_Data = value;
				UnloadingRemarksInfo.RefreshBinding(oldValue);
			}
		}
	}

	public ZPropertyInfo UnloadingRemarksInfo => GetZPropertyInfo(nameof(UnloadingRemarks));

	GenAddOnColumn GenAddOnColumnUnloadingRemarks => genAddOnColumnUnloadingRemarks ??= LoadOrCreateUnloadingRemarksAddOnColumn();
	GenAddOnColumn genAddOnColumnUnloadingRemarks;

	GenAddOnColumn LoadOrCreateUnloadingRemarksAddOnColumn()
	{
		return LoadUnloadingRemarksAddOnColumn() ?? CreateUnloadingRemarksAddOnColumn();
	}

	GenAddOnColumn LoadUnloadingRemarksAddOnColumn()
	{
		var query = new ZQuery(GenAddOnColumnSchema.XA_ParentID, PK);
		query.AddToFilter(GenAddOnColumnSchema.XA_ParentTableCode, CusTempStorageRegHeaderSchema.Constants.Prefix);
		query.AddToFilter(GenAddOnColumnSchema.XA_Name, GenAddOnColumnUnloadingRemarksName);
		return Factory.LoadTop1<GenAddOnColumn>(query);
	}

	GenAddOnColumn CreateUnloadingRemarksAddOnColumn()
	{
		var addOnColumn = Factory.New<GenAddOnColumn>();
		addOnColumn.XA_ParentID = PK;
		addOnColumn.XA_ParentTableCode = CusTempStorageRegHeaderSchema.Constants.Prefix;
		addOnColumn.XA_Name = GenAddOnColumnUnloadingRemarksName;
		addOnColumn.XA_Type = AddOnColumnDataType.Codes.String;

		return addOnColumn;
	}

	const string GenAddOnColumnUnloadingRemarksName = "NO_GoodsReg_UnloadingRemarks";

	public CusTransportMeans TransportMeans => transportMeans ??= LoadOrCreateTransportMeans();
	CusTransportMeans transportMeans;

	CusTransportMeans LoadOrCreateTransportMeans()
	{
		var query = new ZQuery(CusTransportMeansSchema.TPM_ParentID, PK);
		query.AddToFilter(CusTransportMeansSchema.TPM_ParentTableCode, CusTempStorageRegHeaderSchema.Constants.Prefix);

		var transportMeansInternal = Factory.LoadTop1<CusTransportMeans>(query);
		if (transportMeansInternal == null)
		{
			transportMeansInternal = Factory.New<CusTransportMeans>();
			transportMeansInternal.TPM_ParentID = PK;
			transportMeansInternal.TPM_ParentTableCode = CusTempStorageRegHeaderSchema.Constants.Prefix;
		}
		transportMeansInternal.ParentHeader = this;
		return transportMeansInternal;
	}

	public new CusTempStorageRegLineCollection CusTempStorageRegLines => (CusTempStorageRegLineCollection)base.CusTempStorageRegLines;

	internal static ZQuery GetLoadQuery(string reference, string mrn = null)
	{
		var referenceNumbers = new List<string>();

		if (!string.IsNullOrEmpty(reference))
		{
			referenceNumbers.Add(reference);
		}

		if (!string.IsNullOrEmpty(mrn))
		{
			referenceNumbers.Add(mrn);
		}

		var query = new ZQuery(CusTempStorageRegHeaderSchema.SRH_Reference, referenceNumbers);
		query.AddToFilter(CusTempStorageRegHeaderSchema.SRH_AppCode, TemporaryStorageApplicationCodeList.Codes.SBW);
		return query;
	}

	protected override ZString HumanReadableNameCore => SRH_Reference;

	protected override void SetDefaultValues()
	{
		base.SetDefaultValues();
		SRH_AppCode = TemporaryStorageApplicationCodeList.Codes.SBW;
		SRH_PreviousReferenceType = Lookups.PreviousReferenceTypeList.DefaultCode;
		SRH_Status = TemporaryStorageStatusCodeList.Codes.TST;
	}

	protected override AutologState AutoLoggingState => AutologState.AutoLogged;

	protected override EFTA.TemporaryStorageRegister.Business.CusTempStorageRegLineCollection CreateNewCusTempStorageRegLines() => new CusTempStorageRegLineCollection(this);

	protected override EFTA.TemporaryStorageRegister.Business.CusTempStorageRegHeaderValidation GetNewValidation() => new CusTempStorageRegHeaderValidation(this);

	protected override Type GetStorageRegLineTypeCore() => typeof(CusTempStorageRegLine);

	#region IEDocsProvider Members

	protected override EFTA.TemporaryStorageRegister.Business.CusTempStorageRegHeaderDocumentSupporter GetNewDocumentSupporter() => new CusTempStorageRegHeaderDocumentSupporter(this);

	#endregion

	#region IRelatedJob Members

	public ZString JobNumber => SRH_Reference;

	public ZString JobDescription => HumanReadableName;

	public ZString JobStatus => ZString.Empty;

	public ControllerID ControllerID => ControllerIDs.Customs.DE.SumARegister;

	public Guid BusinessObjectPK => PK.ToGuid();

	#endregion

	[ThreadSafe]
	public new static readonly CusTempStorageRegHeaderTypeDecider TypeDecider = new();

	IInvoicesProviderValueChangedAnnouncer IInvoicesProviderValueChangedAnnouncerProvider.GetValueChangedAnnouncer()
	{
		return new CusTempStorageRegHeaderValueChangedAnnouncer(this);
	}
}
