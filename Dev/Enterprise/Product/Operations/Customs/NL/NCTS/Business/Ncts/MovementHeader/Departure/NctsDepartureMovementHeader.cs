using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.NL.Business;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.NL.NCTS.Business;

public class NctsDepartureMovementHeader : EU.NCTS.Business.NctsDepartureMovementHeader
	, Integration.Customs.NL.IDepartureMovementHeader
{
	public NctsDepartureMovementHeader(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}
	public new class Schema : NctsCommonMovementHeader.Schema
	{
		public const string FallbackEntryType = "FBK";
		public const int FallbackEntryNumberLength = 10;
	}

	public new NctsHeader Header => (NctsHeader)base.Header;

	public new NctsDepartureMovementHeaderLookups Lookups => (NctsDepartureMovementHeaderLookups)base.Lookups;

	protected override CusInBondMoveHeaderLookups GetNewPhase5Lookups() => new NctsDepartureMovementHeaderLookups(this);

	protected override CusInBondMoveHeaderLookups GetNewPhase4Lookups() => new NctsDepartureMovementHeaderLookups(this);

	public new CusGoodsLocation GoodsLocation => (CusGoodsLocation)base.GoodsLocation;

	protected override bool ForceRegenerateLocalReferenceNumber => ActivateForceRegenerateLocalReferenceNumber && !BM_PaperlessInbondNum.IsEmpty && BM_MessageStatus.In((ZString)LogicalStatusList.Codes.Error, (ZString)LogicalStatusList.Codes.Invalid) && BM_Phase == NctsMovementHeaderTransactionStatusList.Codes.Declaration;

	public bool ActivateForceRegenerateLocalReferenceNumber { get; set; }

	public void WriteOffGuarantees(ZString transactionAppId)
	{
		var localReferenceNumber = BM_PaperlessInbondNum;
		foreach (var nctsGuarantee in Guarantees)
		{
			if (nctsGuarantee.CusGuarantee is CusGuaranteeHeader guarantee)
			{
				var confirmedTransactions = guarantee.GetTransactions().Where(x => x.CPL_Reference.Equals(localReferenceNumber) && x.CPL_TransactionStatus.EqualsIgnoringCase(PermitTransactionStatusList.Codes.Confirmed)).ToArray();
				foreach (var transaction in confirmedTransactions)
				{
					guarantee.AddTransaction(
						localReferenceNumber,
						(NoResString)"NCTS write-off " + localReferenceNumber + " [" + Header.MovementReferenceNumber + "]",
						transactionAppId,
						ZString.Empty,
						transaction.CPL_TranValue * -1,
						0,
						status: PermitTransactionStatusList.Codes.Confirmed);
				}
			}
		}
	}

	[ReadOnlyMember(nameof(IsFallbackProcedureReadonly))]
	[ResourceStringData("043C92F9-F747-40A5-9E19-053202D32A38", Caption = "Fallback procedure")]
	public ZBool IsFallbackProcedure
	{
		get => this.GetSystemDefinedValue<ZBool>(nameof(IsFallbackProcedure));
		set
		{
			var oldValue = IsFallbackProcedure;
			this.SetSystemDefinedValue(nameof(IsFallbackProcedure), value);
			if (oldValue != value)
			{
				if (oldValue == false)
				{
					var config = DVAFallbackConfiguration;
					FallbackReference = config.InvocationReason;
					FallbackTime = config.Start;
					OnFactorySavingFallbackNumber += FactorySavingFallbackNumber;
				}
				else
				{
					FallbackReference = "";
					FallbackTime = ZDateTime.Empty;
					FallbackNumber = "";
					OnFactorySavingFallbackNumber -= FactorySavingFallbackNumber;
				}
			}
			IsFallbackProcedureInfo.RefreshBinding(oldValue);
		}
	}

	bool IsFallbackProcedureReadonly => DVAFallbackConfiguration.Start.IsEmpty || !DVAFallbackConfiguration.Start.IsInThePast() || DVAFallbackConfiguration.End.IsEmpty || !DVAFallbackConfiguration.End.IsInTheFuture();

	FallbackConfiguration DVAFallbackConfiguration => (FallbackConfiguration)NLCustomsRegistry.Instance.FallbackConfiguration_DVA.Value;

	public ZPropertyInfo IsFallbackProcedureInfo => GetZPropertyInfo(nameof(IsFallbackProcedure));

	[ReadOnly(true)]
	[ResourceStringData("7CFE1CDF-48C8-4590-A452-409565B1F3B8", Caption = "Fallback reference")]
	[MaxLength(70)]
	public ZString FallbackReference
	{
		get => this.GetSystemDefinedValue<ZString>(nameof(FallbackReference));
		set
		{
			var oldValue = FallbackReference;
			if (value != oldValue)
			{
				CheckMaximumLength(FallbackReferenceInfo, value);
				this.SetSystemDefinedValue(nameof(FallbackReference), value);
				FallbackReferenceInfo.RefreshBinding(oldValue);
			}
		}
	}

	public ZPropertyInfo FallbackReferenceInfo => GetZPropertyInfo(nameof(FallbackReference));

	[ReadOnly(true)]
	[ResourceStringData("3DFBE1F4-BD20-4A10-B4B5-EA4AA9C69467", Caption = "Fallback time")]
	public ZDateTime FallbackTime
	{
		get => this.GetSystemDefinedValue<ZDateTime>(nameof(FallbackTime));
		set
		{
			var oldValue = FallbackTime;
			if (value != oldValue)
			{
				this.SetSystemDefinedValue(nameof(FallbackTime), value);
				FallbackTimeInfo.RefreshBinding(oldValue);
			}
		}
	}

	public ZPropertyInfo FallbackTimeInfo => GetZPropertyInfo(nameof(FallbackTime));

	[MaxLength(Schema.FallbackEntryNumberLength)]
	public ZString FallbackNumber
	{
		get { return NLNctsFallbackEntryNumber?.CE_EntryNum ?? ZString.Empty; }
		set
		{
			if (NLNctsFallbackEntryNumber.CE_EntryNum != value)
			{
				CheckMaximumLength(FallbackNumberInfo, value);
				NLNctsFallbackEntryNumber.CE_EntryNum = value;
				FallbackNumberInfo.RefreshBinding();
			}
		}
	}

	public ZPropertyInfo FallbackNumberInfo => GetZPropertyInfo(nameof(FallbackNumber));

	CusEntryNumber NLNctsFallbackEntryNumber => Factory.GetValue(ref nlNctsFallbackEntryNumber, delegate
	{
		var nlNctsFallbackEntryNumberInternal = CusEntryNumber.Load(this, Schema.FallbackEntryType, CountryCode);
		if (nlNctsFallbackEntryNumberInternal == null)
		{
			nlNctsFallbackEntryNumberInternal = CusEntryNumber.New(this, Schema.FallbackEntryType, CountryCode);
			nlNctsFallbackEntryNumberInternal.CE_EntryIsSystemGenerated = true;
			nlNctsFallbackEntryNumberInternal.CE_Category = CusEntryNumber.Categories.CustomsPermitClearanceNumber;
		}
		return nlNctsFallbackEntryNumberInternal;
	});
	CachedProperty<CusEntryNumber> nlNctsFallbackEntryNumber;

	public event SavingEventHandler<NctsDepartureMovementHeader> OnFactorySavingFallbackNumber;

	public void FireOnFactorySavingFallbackNumber()
	{
		if (OnFactorySavingFallbackNumber != null)
		{
			OnFactorySavingFallbackNumber(this);
			OnFactorySavingFallbackNumber -= FactorySavingFallbackNumber;
		}
	}

	protected override void OnFactorySaving()
	{
		base.OnFactorySaving();
		FireOnFactorySavingFallbackNumber();
	}

	public void FactorySavingFallbackNumber(NctsDepartureMovementHeader movementHeader)
	{
		movementHeader.FallbackNumber = GetFallbackEntryNumber(Factory);
	}

	ZString GetFallbackEntryNumber(BusinessObjectFactory factory)
	{
		var target = new NLNctsFallbackEntryNumberGeneratorTarget();
		var generator = new NumberGenerator
		{
			Factory = factory,
			Context = new NumberGeneratorContext(),
			BaseFountain = Env.NumberFountains.GetNLNctsFallbackEntryNumberSendCounter(),
			FountainGetter = Env.NumberFountains.GetNLNctsFallbackEntryNumberGeneratorFountain,
			PrimaryTarget = target
		};
		generator.ValueProviders.AddRange(new StandardValueSource());
		generator.Generate();
		generator.EnforceMaxLengths();
		return target.Value.ToUpper();
	}

	protected override void DefaultDepartureLocationCodeFromCusAuthorisationIfBlank(Customs.Business.CusAuthorisationHeader authorizationToUse)
	{
		var configuration = Header.Configuration.LocationOfGoodsFromAuthorisationDefaulterConfiguration;
		if (configuration.IsDefaultingEnabled)
		{
			if ((GoodsLocation?.CGL_AdditionalIdentifier ?? ZString.Empty).IsEmpty)
			{
				var locationCode = GetLocationCodeFromCusAuthorisation(authorizationToUse);
				if (locationCode != null)
				{
					GoodsLocation.CGL_Qualifier = configuration.QualifierCode;
					GoodsLocation.CGL_Type = configuration.TypeCode;
					GoodsLocation.CGL_AdditionalIdentifier = locationCode.CPR_ValueFrom.SubstringSafe(0, CusGoodsLocationSchema.CGL_AdditionalIdentifier.MaxLength);
				}
			}
		}
	}

	public override ZString BM_ExportTransportMode
	{
		get => base.BM_ExportTransportMode;
		set
		{
			var oldValueBM_ExportTransportModeFallbackOnBM_InlandTransportMode = ExportTransportModeFallbackOnInlandTransportMode;
			base.BM_ExportTransportMode = value;

			if (!IsCopying && ExportTransportModeFallbackOnInlandTransportMode != oldValueBM_ExportTransportModeFallbackOnBM_InlandTransportMode)
			{
				Header.Bills.ForEach(b => b.GoodsItems.ForEach(i => i.NotifyChangeOfExportTransportModeFallbackOnInlandTransportMode(ExportTransportModeFallbackOnInlandTransportMode, oldValueBM_ExportTransportModeFallbackOnBM_InlandTransportMode)));
			}
		}
	}

	public override ZString BM_InlandTransportMode
	{
		get => base.BM_InlandTransportMode;
		set
		{
			var oldValueBM_ExportTransportModeFallbackOnBM_InlandTransportMode = ExportTransportModeFallbackOnInlandTransportMode;
			base.BM_InlandTransportMode = value;

			if (!IsCopying && ExportTransportModeFallbackOnInlandTransportMode != oldValueBM_ExportTransportModeFallbackOnBM_InlandTransportMode)
			{
				Header.Bills.ForEach(b => b.GoodsItems.ForEach(i => i.NotifyChangeOfExportTransportModeFallbackOnInlandTransportMode(ExportTransportModeFallbackOnInlandTransportMode, oldValueBM_ExportTransportModeFallbackOnBM_InlandTransportMode)));
			}
		}
	}

	public ZString ExportTransportModeFallbackOnInlandTransportMode => !BM_ExportTransportMode.IsEmpty ? BM_ExportTransportMode : BM_InlandTransportMode;

	public new INctsGuaranteeCollection<NctsGuarantee> Guarantees => (INctsGuaranteeCollection<NctsGuarantee>)base.Guarantees;

	protected override INctsGuaranteeCollection<EU.NCTS.Business.NctsGuarantee> GetGuaranteesCore() => new NctsGuaranteeCollection<NctsGuarantee>(this);
	public new EU.Business.ICusAuthorizationUsageCollection<CusAuthorizationUsage, NctsDepartureMovementHeader> CusAuthorizationUsages => (CusAuthorizationUsageCollection<CusAuthorizationUsage, NctsDepartureMovementHeader>)base.CusAuthorizationUsages;
	protected override EU.Business.ICusAuthorizationUsageCollection<EU.NCTS.Business.CusAuthorizationUsage, EU.NCTS.Business.NctsDepartureMovementHeader> GetCusAuthorizationUsages()
		=> new CusAuthorizationUsageCollection<CusAuthorizationUsage, NctsDepartureMovementHeader>(this);
}
