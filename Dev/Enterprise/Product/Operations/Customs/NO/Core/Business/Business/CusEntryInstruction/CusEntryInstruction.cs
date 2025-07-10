using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.FetchStrategies;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.NO;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.NO.Business;

public class CusEntryInstruction : AutoNOCusEntryInstruction,
	Integration.Customs.ICusSupportingInfoTypeSupporter,
	IPreviousDocumentsProvider
{
	public CusEntryInstruction(BusinessObjectFactory factory, DataRow row)
		: base(factory, row)
	{
	}

	protected override void SetDefaultValues()
	{
		base.SetDefaultValues();
		CEI_SubStyle = Lookups.EntrySubStyleList[0].Code;
	}

	public virtual void SetDefaultsForNewChild(JobDeclaration declaration)
	{
		Argument.NotNull(declaration, nameof(declaration));
		CEI_PackageCount = GetCEI_PackageCountDefaultValue(declaration);
		CEI_SubStyle = GetCEI_SubStyleDefaultValue(declaration);
	}

	#region Schema
	public new class Schema : AutoNOCusEntryInstruction.Schema
	{
		public new const int CEI_ProcedureMaxLength = 4;
		public const string CEI_GoodsNumber = "CEI_GoodsNumber";
		public const string CEI_Position = "CEI_Position";
		public const int CEI_SubPositionMaxLength = 5;
		public const string CEI_SubPosition = "CEI_SubPosition";
	}
	#endregion

	internal IReadOnlyCollection<JobComInvoiceHeader> Invoices => InvoiceLines.Cast<JobComInvoiceLine>().Where(x => x.InvoiceHeader != null).Select(x => x.InvoiceHeader).Distinct().ToList();

	public ZBool IsImport => Factory.GetValue(ref isImport, () => JobDeclaration?.IsImport ?? false);
	CachedProperty<ZBool> isImport;

	public new CusEntryInstructionLookups Lookups => (CusEntryInstructionLookups)base.Lookups;

	protected override bool IsLookupsCachedInBase => false;

	public new CusEntryInstructionValidation Validation => (CusEntryInstructionValidation)base.Validation;

	public new JobDeclaration JobDeclaration => base.JobDeclaration as JobDeclaration;

	protected override Customs.Business.CusEntryInstructionLookups GetNewLookups()
	{
		CusEntryInstructionLookups result;
		if (IsImport)
		{
			result = new ImportCusEntryInstructionLookups(this);
		}
		else
		{
			result = new ExportCusEntryInstructionLookups(this);
		}

		return result;
	}

	public ICusEntryHeaderCollection<CusEntryHeader> CustomsEntryHeaders => new CusEntryHeaderCollection<CusEntryHeader>((JobDeclaration)base.JobDeclaration, Factory);

	public new CusEntryHeader EntryHeader => (CusEntryHeader)base.EntryHeader;

	protected override Customs.Business.CusEntryInstructionValidation GetNewValidation() => new CusEntryInstructionValidation(this);

	public ZDateTime DateOfValuation => JobDeclaration?.DateOfValuation ?? ZDateTime.Today;

	protected override bool SupportsCloneCore() => true;

	protected override BusinessObject CloneInternal(BusinessObjectCloneArgs args)
	{
		var clonedBizO = base.CloneInternal(args);
		if (clonedBizO is CusEntryInstruction clonedEntryInstruction)
		{
			clonedEntryInstruction.CEI_SubPosition = CEI_SubPosition;
		}
		return clonedBizO;
	}

	public ZBool HasDigitollGoodsNumber => GetHasDigitollGoodsNumber();
	protected virtual ZBool GetHasDigitollGoodsNumber() => JobDeclaration?.HasDigitollGoodsNumber ?? ZBool.False;

	[ResourceStringData("67925534-35CC-41D7-A682-EB557DE50DE1", Caption = "Procedure Code", MediumCaption = "Procedure", ShortCaption = "CPC")]
	[List(nameof(Lookups) + "." + nameof(CusEntryInstructionLookups.ProcedureList))]
	[MaxLength(Schema.CEI_ProcedureMaxLength)]
	public override ZString CEI_Procedure
	{
		get => base.CEI_Procedure;
		set
		{
			var oldValue = CEI_Procedure;
			base.CEI_Procedure = value;

			if (value != oldValue)
			{
				procedureDescription = null;
				CreateOrRemoveChargesIfNeeded();
				UpdatePreviousDocumentMasterProcedureCodeIfApplicable();
				JobDeclaration?.MarkAsNeedingValidation();
			}
		}
	}

	[ResourceStringData("NO.CusEntryInstruction.CEI_DateForDuty", Caption = "Date for Duty", FullDescription = "Date for Duty regulates the lookup of duty and fees in the tariff and also the currency exchange rate. When blank today's date is used.")]
	public override ZDateTime CEI_DateForDuty
	{
		get => base.CEI_DateForDuty;
		set => base.CEI_DateForDuty = value;
	}

	[ResourceStringData("CC8BA80C-7993-475A-902C-420AFBE8D934", Caption = "Declaration Type", MediumCaption = "Decl. Type", ShortCaption = "Type")]
	[List(nameof(Lookups) + "." + nameof(CusEntryInstructionLookups.StyleList))]
	[MaxLength(Schema.CEI_StyleMaxLength)]
	public override ZString CEI_Style
	{
		get => base.CEI_Style;
		set
		{
			base.CEI_Style = value;
			SetCEIDescription(value);
		}
	}

	[ResourceStringData("0DA6CCF9-1006-4AF6-AF50-E60016E5D133", Caption = "Declaration Sub Type", MediumCaption = "Decl. Sub Type", ShortCaption = "Sub Type")]
	[List(nameof(Lookups) + "." + nameof(CusEntryInstructionLookups.EntrySubStyleList))]
	public override ZString CEI_SubStyle
	{
		get => base.CEI_SubStyle;
		set
		{
			var oldValue = base.CEI_SubStyle;
			if (oldValue != value)
			{
				base.CEI_SubStyle = value;
				SetCopyStatusOnSiblingInstructionsIfNeeded();
				JobDeclaration?.MarkAsNeedingValidation();
				JobDeclaration?.CustomsEntryHeaders?.MarkAsNeedingValidation();
			}
		}
	}

	void SetCopyStatusOnSiblingInstructionsIfNeeded()
	{
		var value = CEI_SubStyle;
		if (Lookups.CopyStatusList.ContainsCode(value))
		{
			JobDeclaration?.CustomsEntryInstructions
				.Where(siblingInstruction => siblingInstruction.PK != PK)
				.ForEach(siblingInstruction => siblingInstruction.CEI_SubStyle = value);
		}
	}

	[ResourceStringData("512FB4F9-130D-DA82-4817-3D5EE7554C04", Caption = "Goods Number")]
	public ZString CEI_GoodsNumber => JobDeclaration?.JE_GoodsNumber ?? ZString.Empty;

	public ZPropertyInfo CEI_GoodsNumberInfo => GetZPropertyInfo(nameof(CEI_GoodsNumber));

	[ResourceStringData("2EF20797-6F44-68AD-48FC-B10CF217A263", Caption = "Position")]
	public ZString CEI_Position => JobDeclaration?.JE_Position ?? ZString.Empty;

	public ZPropertyInfo CEI_PositionInfo => GetZPropertyInfo(nameof(CEI_Position));

	[ResourceStringData("8AE40683-DD96-4302-8F4D-47D9707F48DA", Caption = "Sub Position")]
	[MaxLength(Schema.CEI_SubPositionMaxLength)]
	public ZString CEI_SubPosition
	{
		get => GSPCusEntryNumber.GetEntryNumberPart(Schema.CEI_SubPosition);
		set
		{
			if (CEI_SubPosition != value)
			{
				CheckMaximumLength(CEI_SubPositionInfo, value);
				GSPCusEntryNumber.SetEntryNumberPart(value, Schema.CEI_SubPosition, CEI_SubPositionInfo);
			}
		}
	}
	public ZPropertyInfo CEI_SubPositionInfo => GetZPropertyInfo(Schema.CEI_SubPosition);

	[ResourceStringData("8ee3a631-a52b-4acb-8c65-82e463ffcabe", Caption = "Total No Of Units", FullDescription = "State the Total No Of Units for this entry.")]
	public override ZDecimal CEI_PackageCount
	{
		get => base.CEI_PackageCount;
		set => base.CEI_PackageCount = value;
	}

	public override ZInt CEI_ClusterKey
	{
		get => base.CEI_ClusterKey;
		set
		{
			var oldValue = base.CEI_ClusterKey;
			base.CEI_ClusterKey = value;
			if (oldValue != value && !IsValidationSuspended)
			{
				JobDeclaration?.CustomsEntryHeaders?.MarkAsNeedingValidation();
			}
		}
	}

	public override ZGuid CEI_JE
	{
		get => base.CEI_JE;
		set
		{
			var oldValue = base.CEI_JE;
			base.CEI_JE = value;
			if (oldValue != value && !IsValidationSuspended)
			{
				JobDeclaration?.CustomsEntryHeaders?.MarkAsNeedingValidation();
			}
		}
	}

	protected CusEntryNumberWrapper GSPCusEntryNumber => gspCusEntryNumberWrapper ??= new CusEntryNumberWrapper(this, CusEntryNumberTypes.Norway.GoodsNumberSubPosition, new Dictionary<ZString, ZInt> { { Schema.CEI_SubPosition, 0 } });
	CusEntryNumberWrapper gspCusEntryNumberWrapper;

	[ResourceStringData("819FB8FB-3841-420F-9415-15116E76B053", Caption = "Procedure Description", MediumCaption = "Proc. Desc.", ShortCaption = "Desc.")]
	public ZString ProcedureDescription
	{
		get => procedureDescription ??= CEI_Procedure.IsEmpty ? string.Empty : Lookups.ProcedureList.GetDescriptionFromCode(CEI_Procedure);
	}
	string procedureDescription;

	void SetCEIDescription(ZString style)
	{
		if (!style.IsEmpty && CEI_Description.IsEmpty)
		{
			CEI_Description = Lookups.StyleList.GetDescriptionFromCode(style);
		}
	}

	void CreateOrRemoveChargesIfNeeded()
	{
		if (JobDeclaration?.ActiveGroupHeader[0].Charges is not { } charges)
		{
			return;
		}
		if (CEI_Procedure == UniversalReferenceConstants.ChargeTypes.VGE_ProcedureCode)
		{
			var chargeKey = new ApportionChargeKey(NOInvoiceChargeTypesImport.Codes.ValueOfGoodsExported, isDutiable: false, isVATible: false, ApportionmentTypeList.Codes.FullApportionment,
				isIncludedInITOT: GroupIsIncludedInLinesOptionList.Codes.No, isIncludedInInvoice: GroupIsIncludedInLinesOptionList.Codes.No, distributeBy: ChargeDistributeByList.Codes.Value,
				percentage: 0m, isAdjustedCharge: false, (NoResString)"Value of Goods Exported", isSystem: false, isStatisticalValueApplicable: true);
			charges.AddCharge(chargeKey, ZDecimal.Zero, JobDeclaration.LocalCurrencyCode);
		}
		else
		{
			var charge = charges.FirstOrDefault(x => x.J7_ChargeType == NOInvoiceChargeTypesImport.Codes.ValueOfGoodsExported);
			if (charge != null)
			{
				charges.Delete(charge);
			}
		}
	}

	ZDecimal GetCEI_PackageCountDefaultValue(JobDeclaration declaration)
	{
		var result = 0.00m;
		if (declaration != null)
		{
			var totalPieces = declaration.JE_TotalNoOfPieces;
			var sumOfPackages = declaration.CustomsEntryInstructions.Where(x => x.PK != PK).Sum(x => x.CEI_PackageCount);

			if (totalPieces >= sumOfPackages)
			{
				result = totalPieces - sumOfPackages;
			}
		}
		return result;
	}

	ZString GetCEI_SubStyleDefaultValue(JobDeclaration declaration)
	{
		if (declaration != null)
		{
			var copyStatusCodeList = Lookups.CopyStatusList;
			var copyStatus = declaration.CustomsEntryInstructions
				.Select(instruction => instruction.CEI_SubStyle)
				.FirstOrDefault(ceiSubStyle => copyStatusCodeList.ContainsCode(ceiSubStyle));
			if (!copyStatus.IsEmpty)
			{
				return copyStatus;
			}
		}
		return CEI_SubStyle;
	}

	#region PreviousDocuments

	[ChildEditable(true)]
	public ICusSupportingInfoCollection<PreviousDocument> PreviousDocuments => previousDocuments ??= CreateNewPreviousDocumentsCollection();
	ICusSupportingInfoCollection<PreviousDocument> previousDocuments;

	ICusSupportingInfoCollection<PreviousDocument> CreateNewPreviousDocumentsCollection()
	{
		var documents = new CusSupportingInfoCollection<PreviousDocument>(this, CusSupportingInfoTypeList.Codes.PreviousDocument);
		documents.Load();
		RegisterEditableChildObject(documents);
		return documents;
	}

	ICusSupportingInfoCollection<PreviousDocument> IPreviousDocumentsProvider.PreviousDocuments => PreviousDocuments;

	#endregion

	public PreviousDocumentMaster PreviousDocumentMaster => previousDocumentMaster ??= CreatePreviousDocumentMaster();
	PreviousDocumentMaster previousDocumentMaster;

	PreviousDocumentMaster CreatePreviousDocumentMaster()
	{
		var master = new PreviousDocumentMaster(Factory, this);
		RegisterEditableChildObject(master);
		return master;
	}

	IDictionary<ZString, Type> Integration.Customs.ICusSupportingInfoTypeSupporter.GetCusSupportingInfoTypes()
	{
		return new Dictionary<ZString, Type>
		{
			{ CusSupportingInfoTypeList.Codes.PreviousDocument, typeof(PreviousDocument) }
		};
	}

	IEnumerable<IBusinessObjectFetchStrategy> IAdditionalBusinessObjectFetchStrategyProvider.GetFetchStrategies()
	{
		yield return new CusSupportingInfoTypeSupporterFetchStrategy(this);
	}

	public bool IsProcedureCodeWithOutOfWarehouse
		=> CusProcedure is { } procedure && procedure.ZZ6_OutOfWarehouse == YesNoList.Codes.Yes;

	internal RefCusProcedure CusProcedure => Factory.GetCachedValue($"NO.CusEntryInstruction.{CEI_Procedure}", GetCusProcedure);

	RefCusProcedure GetCusProcedure()
	{
		if (JobDeclaration is null)
		{
			return null;
		}

		var procedures = RefCusProcedureLoader.GetAllApplicableProcedures(Factory, this);
		return procedures.TryGetValue(CEI_Procedure, out var procedure) ? procedure : null;
	}

	void UpdatePreviousDocumentMasterProcedureCodeIfApplicable()
	{
		PreviousDocumentMaster.CSI_Procedure = GetPreviousProcedureValue();
	}

	internal ZString GetPreviousProcedureValue()
		=> IsProcedureCodeWithOutOfWarehouse
			? string.Concat(CusProcedure.ZZ6_PreviousProcedureCode, JobDeclaration?.JE_LocationOfGoods)
			: ZString.Empty;
}
