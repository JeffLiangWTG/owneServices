using System;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.NL.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.NL.NCTS.Business;

public class NctsHeader : EU.NCTS.Business.NctsHeader, Integration.Customs.NL.ICusInBondHeader
{
	public NctsHeader(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}

	public new partial class Schema : EU.NCTS.Business.NctsHeader.Schema
	{
		public const string CALCalculationMethod = "CALCalculationMethod";
		public const int CALCalculationMethodMaxLength = 3;
	}

	protected override void SetDefaultValues()
	{
		base.SetDefaultValues();
		var communicationLanguage = GlbStaff.CurrentUser.GS_WorkingLanguage.Left(2);
		if (Lookups.CommunicationLanguageList.ContainsCode(communicationLanguage))
		{
			BH_CommunicationLanguage = communicationLanguage;
		}
		CALCalculationMethod = NLCustomsRegistry.Instance.CalCalculationMethod.Value.OfType<CalCalculationMethodRegistry>().FirstOrDefault(x => x.CalculationMethodDefault)?.CalculationMethodName ?? ZString.Empty;
	}

	protected override void ApportionedAmountToGuaranteesLiabilityAmountCore()
	{
		if (CALCalculationMethod == CalculationMethodList.Codes.DEF)
		{
			var guarantees = GetEffectiveGuarantees();
			guarantees.Cast<NctsGuarantee>().ForEach(x =>
			{
				if (!x.PW_Override)
				{
					x.PW_BondAmount = NLCustomsRegistry.Instance.CalCalculationMethod.Value.OfType<CalCalculationMethodRegistry>().FirstOrDefault(x => x.CalculationMethodName == CalculationMethodList.Codes.DEF)?.CalculationMethodValue ?? ZDecimal.Zero;
				}
			});
		}
		else
		{
			base.ApportionedAmountToGuaranteesLiabilityAmountCore();
		}
	}

	public void AssignDeclarationGoodsItemNumbers(bool reassignNumbers = false)
	{
		var goodsItems = GetGoodsItems();
		if (reassignNumbers)
		{
			goodsItems.ForEach(item => item.BY_DeclarationGoodsItemNumber = ZInt.Zero);
		}

		EU.NCTS.Business.NctsHeaderDeclarationGoodsItemNumbersHelper.AssignUnassignedDeclarationGoodsItemNumbers(this, goodsItems);
	}

	public CusInvPack RetrieveArrivalGoodsItemPackage(int houseConsignmentSequence, int houseConsignmentItemDeclarationSequence, int packageSequence)
	{
		var billSequence = houseConsignmentSequence.ToString();
		return Bills.FirstOrDefault(b => b.MovementDetail.B9_SeqNo == billSequence)?
			.ArrivalGoodsItems.FirstOrDefault(g => g.BY_DeclarationGoodsItemNumber == houseConsignmentItemDeclarationSequence)?
			.Packages.Cast<NctsPackage>().FirstOrDefault(p => p.B5_SequenceNumber == packageSequence);
	}

	public CusGuaranteeHeader[] GuaranteesNoLongerInDeclaration
	{
		get
		{
			var guaranteeHeaderQuery = new ZDBOnlyQuery(typeof(CusGuaranteeHeader));
			guaranteeHeaderQuery.AddToFilter(CusPermitHeaderSchema.CPH_Number, SQLComparisonOperator.NotEqual, this.GetEffectiveGuarantees().Select(x => x.PW_BondNumber));

			var transactionQuery = new ZDBOnlySubQuery(typeof(SharedCusPermitLineTransaction), CusPermitLineTransactionSchema.CPL_CPH_PermitHeader);
			transactionQuery.AddToFilter(CusPermitLineTransactionSchema.CPL_Reference,this.MovementHeader.BM_PaperlessInbondNum);
			guaranteeHeaderQuery.AddSubQuery(transactionQuery, JoinCondition.And);
			return this.Factory.Load<CusGuaranteeHeader>(guaranteeHeaderQuery);
		}
	}

	public new INctsGuaranteeCollection<NctsGuarantee> Guarantees => (INctsGuaranteeCollection<NctsGuarantee>)base.Guarantees;

	protected override INctsGuaranteeCollection<EU.NCTS.Business.NctsGuarantee> GetGuaranteesForNonPhase5Departure() => new NctsGuaranteeCollection<NctsGuarantee>(this);

	public new INctsBillCollection<NctsBill> Bills => (INctsBillCollection<NctsBill>)base.Bills;
	protected override INctsBillCollection<EU.NCTS.Business.NctsBill> GetNewBillCollection() => new NctsBillCollection<NctsBill>(this);
	protected override Type BillTypeCore => typeof(NctsBill);

	public new NctsHeaderLookups Lookups => (NctsHeaderLookups)base.Lookups;

	protected override CusInBondHeaderLookups GetNewLookups() => new NctsHeaderLookups(this);

	protected override CusInBondHeaderValidation GetNewPhase5Validation() => new NctsHeaderPhase5Validation(this);

	public new NctsDepartureMovementHeader MovementHeader => (NctsDepartureMovementHeader)base.MovementHeader;

	public new NctsArrivalMovementHeader ArrivalMovementHeader => (NctsArrivalMovementHeader)base.ArrivalMovementHeader;

	public new INctsDepartureHeaderContainerCollection<NctsDepartureHeaderContainer, NctsHeader> DepartureHeaderContainers
		=> (INctsDepartureHeaderContainerCollection<NctsDepartureHeaderContainer, NctsHeader>)base.DepartureHeaderContainers;

	protected override INctsDepartureHeaderContainerCollection<EU.NCTS.Business.NctsDepartureHeaderContainer, EU.NCTS.Business.NctsHeader> GetDepartureHeaderContainersCore()
		=> new NctsDepartureHeaderContainerCollection(this);

	protected override Type DepartureContainerTypeCore => typeof(NctsDepartureHeaderContainer);

	[MaxLength(Schema.CALCalculationMethodMaxLength)]
	[List(nameof(Lookups) + "." + nameof(NctsHeaderLookups.CalculationMethodList))]
	[ResourceStringData("Enterprise.Customs.NL.NCTS.Business.NctsHeader|CALCalculationMethod", Caption = "CAL")]
	public ZString CALCalculationMethod
	{
		get => this.GetSystemDefinedValue<ZString>(nameof(CALCalculationMethod));
		set
		{
			CheckMaximumLength(CALCalculationMethodInfo, value);
			var oldValue = CALCalculationMethod;
			this.SetSystemDefinedValue(nameof(CALCalculationMethod), value);
			if (!IsValidationSuspended)
			{
				(Validation as NctsHeaderPhase5Validation)?.ValidateCALCalculationMethod();
			}
			CALCalculationMethodInfo.RefreshBinding(oldValue);

			if (!IsCopying && value != oldValue)
			{
				Bills.ForEach(b => b.GoodsItems.ForEach(i => i.NotifyChangeOfCAL(value, oldValue)));
			}
		}
	}

	public ZPropertyInfo CALCalculationMethodInfo => GetZPropertyInfo(nameof(CALCalculationMethod));

	protected override ZString GetFallbackInformationCore()
	{
		var dateNow = ZDateTime.Now;
		var storingsnr = (MovementHeader?.FallbackReference.IsEmpty ?? true) ? (ZString)$"{dateNow:ddMMyyyy HHmm}" : MovementHeader.FallbackReference;
		return $@"NOODPROCEDURE NCTS (GEGEVENS NIET IN HET SYSTEEM)
Datum/uur: {dateNow:dd-MM-yyyy HH:mm}
Storingsnr.: {storingsnr}";
	}

	protected override EU.NCTS.Business.NctsHeaderDocumentSupporter GetNewDocumentSupporter() => new NctsHeaderDocumentSupporter(this);

	public new EU.Business.ICusAuthorizationUsageCollection<CusAuthorizationUsage, NctsHeader> CusAuthorizationUsages => (CusAuthorizationUsageCollection<CusAuthorizationUsage, NctsHeader>)base.CusAuthorizationUsages;

	protected override EU.Business.ICusAuthorizationUsageCollection<EU.NCTS.Business.CusAuthorizationUsage, EU.NCTS.Business.NctsHeader> GetCusAuthorizationUsages()
		=> new CusAuthorizationUsageCollection<CusAuthorizationUsage, NctsHeader>(this);
}
