using System;
using System.ComponentModel;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;

namespace Enterprise.Customs.NO.Business;

[DependentBusinessObject(typeof(CusTempStorageRegHeader), nameof(CusTempStorageRegHeader.CusTempStorageRegLines))]
[CodeProperty(nameof(CusTempStorageRegLine.HeaderRegNo))]
[DescriptionProperty(nameof(CusTempStorageRegLine.HeaderRegNo))]
public class CusTempStorageRegLine(BusinessObjectFactory factory, DataRow row) : EFTA.TemporaryStorageRegister.Business.CusTempStorageRegLine(factory, row)
{
	#region Properties

	[List(nameof(Lookups) + "." + nameof(CusTempStorageRegLineLookups.CustomsStatusList))]
	[ResourceStringData("59A310FC-E1B5-4DA1-BBA2-E38E89BD17DF", Caption = "Line Status")]
	public override ZString SRL_CustomsStatus
	{
		get => base.SRL_CustomsStatus;
		set => base.SRL_CustomsStatus = value;
	}

	[ReadOnly(true)]
	[ResourceStringData("5851867A-B313-4102-9AFF-E139ED61AE65", Caption = "Package Count")]
	public override ZInt SRL_PackagesRemaining
	{
		get => base.SRL_PackagesRemaining;
		set => base.SRL_PackagesRemaining = value;
	}

	[MaxLength(17)]
	public override ZString SRL_CustodianIdentifier
	{
		get => base.SRL_CustodianIdentifier;
		set => base.SRL_CustodianIdentifier = value;
	}

	[MaxLength(17)]
	[ResourceStringData("E127090A-19D1-4D8C-97ED-61B55BB18994", Caption = "Disp. Ent. Trader EORI")]
	public override ZString SRL_GoodsOwnerIdentifier
	{
		get => base.SRL_GoodsOwnerIdentifier;
		set => base.SRL_GoodsOwnerIdentifier = value;
	}

	#endregion

	public ZString HeaderRegNo => RegHeader?.SRH_Reference ?? ZString.Empty;

	public new CusTempStorageRegHeader RegHeader => (CusTempStorageRegHeader)base.RegHeader;

	public new CusTempStorageRegLineTransactionCollection CusTempStorageRegLineTransactions => (CusTempStorageRegLineTransactionCollection)base.CusTempStorageRegLineTransactions;

	protected override EFTA.TemporaryStorageRegister.Business.CusTempStorageRegLineTransactionCollection CreateNewCusTempStorageRegLineTransactions()
	{
		var transactions = new CusTempStorageRegLineTransactionCollection(this);
		transactions.CollectionCountChange += (sender, e) => UpdatePackagesRemaining();
		return transactions;
	}

	public new CusTempStorageRegLineValidation Validation => (CusTempStorageRegLineValidation)base.Validation;

	protected override EFTA.TemporaryStorageRegister.Business.CusTempStorageRegLineValidation GetNewValidation() => new CusTempStorageRegLineValidation(this);

	public new CusTempStorageRegLineLookups Lookups => (CusTempStorageRegLineLookups)base.Lookups;

	protected override EFTA.TemporaryStorageRegister.Business.CusTempStorageRegLineLookups GetNewLookups() => new CusTempStorageRegLineLookups(this);

	protected override void SetDefaultValues()
	{
		base.SetDefaultValues();
		SRL_PackagesRemaining = 0;
		SRL_CustomsStatus = TemporaryStorageStatusCodeList.Codes.TST;
	}

	protected override Type GetStorageRegLineTransactionCore() => typeof(CusTempStorageRegLineTransaction);
}
