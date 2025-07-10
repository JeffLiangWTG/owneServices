using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.NL.Business;

namespace Enterprise.Customs.NL.NCTS.Business;

public class NctsArrivalMovementHeader : EU.NCTS.Business.NctsArrivalMovementHeader
	, Integration.Customs.NL.IArrivalMovementHeader
{
	public NctsArrivalMovementHeader(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}

	public new class Schema : EU.NCTS.Business.NctsCommonMovementHeader.Schema
	{
		public const string UnloadingRemarksFreeText = nameof(NctsArrivalMovementHeader.UnloadingRemarksFreeText);
	}

	public new CusGoodsLocation GoodsLocation => (CusGoodsLocation)base.GoodsLocation;

	protected override Customs.Business.CusInBondMoveHeaderValidation GetNewValidation() => new NctsArrivalMovementHeaderValidation(this);

	public new NctsArrivalMovementHeaderValidation Validation => (NctsArrivalMovementHeaderValidation)base.Validation;

	[ReadOnlyMember(nameof(IsUnloadingRemarksReadOnly))]
	[ResourceStringData("501492EF-CA3F-4AAB-943B-DF4E39DDD58A", Caption = "Enter the remarks regarding the unloading", MediumCaption = "Unloading remarks", ShortCaption = "Remarks")]
	public ZString UnloadingRemarksFreeText
	{
		get { return unloadingRemarksFreeText; }
		set
		{
			var oldValue = UnloadingRemarksFreeText;
			if (oldValue != value)
			{
				value = value.TrimEndSpaceTab();
				var unloadingRemarksFreeTextInfo = UnloadingRemarksFreeTextInfo;
				SetNonPersistentPropertyValue(unloadingRemarksFreeTextInfo, ref unloadingRemarksFreeText, value);

				if (!IsValidationSuspended)
				{
					Validation.ValidateUnloadingRemarksFreeText();
				}
				unloadingRemarksFreeTextInfo.RefreshBinding();
			}
		}
	}
	ZString unloadingRemarksFreeText;

	public ZPropertyInfo UnloadingRemarksFreeTextInfo => GetZPropertyInfo(Schema.UnloadingRemarksFreeText);

	[ChildEditable]
	public NonPersistentNctsUnloadingRemarkCollection UnloadingRemarkCollection
	{
		get
		{
			if (unloadingRemarkCollection == null)
			{
				unloadingRemarkCollection = UnloadingRemarkCollectionCore();
				RegisterEditableChildObject(unloadingRemarkCollection);
			}
			unloadingRemarkCollection?.SetReadOnlyIncludingChildren(IsUnloadingRemarksReadOnly);
			return unloadingRemarkCollection;
		}
	}
	NonPersistentNctsUnloadingRemarkCollection unloadingRemarkCollection;

	protected NonPersistentNctsUnloadingRemarkCollection UnloadingRemarkCollectionCore() => new NonPersistentNctsUnloadingRemarkCollection(this);

	void LoadUnloadingRemarks()
	{
		var unloadingRemarks = BM_UnloadingRemarks;
		var lastGreaterThan = unloadingRemarks.LastIndexOf('>');
		unloadingRemarksFreeText = unloadingRemarks.Substring(lastGreaterThan + 1).TrimStart(System.Environment.NewLine.ToCharArray());
	}

	public override void OnLoaded()
	{
		base.OnLoaded();
		LoadUnloadingRemarks();
	}

	void SaveUnloadingRemarks()
	{
		if (unloadingRemarkCollection != null && unloadingRemarkCollection.Count > 0)
		{
			if (unloadingRemarkCollection.HasChanges)
			{
				var unloadingRemarks = new ZStringBuilder();
				foreach (NonPersistentNctsUnloadingRemark unloadingRemark in UnloadingRemarkCollection)
				{
					unloadingRemarks.Append(unloadingRemark.ToFormattedString());
					unloadingRemark.HasChanges = false; // Need to force this as it doesn't seem to work automatically
				}

				unloadingRemarks.Append(UnloadingRemarksFreeText);
				BM_UnloadingRemarks = unloadingRemarks.ToStringWithNewLineBetweenAppends();
			}
			else
			{
				var unloadingRemarks = BM_UnloadingRemarks;
				var lastGreaterThan = unloadingRemarks.LastIndexOf('>');
				BM_UnloadingRemarks = unloadingRemarks.Substring(0, lastGreaterThan + 1) + System.Environment.NewLine + UnloadingRemarksFreeText;
			}
		}
		else
		{
			BM_UnloadingRemarks = UnloadingRemarksFreeText;
		}
		BM_UnloadingRemarks = BM_UnloadingRemarks.TrimEnd(System.Environment.NewLine.ToCharArray());
	}

	protected override void OnFactorySaving()
	{
		SaveUnloadingRemarks();
		base.OnFactorySaving();
	}

	public override ZDecimal TotalUnloadedGrossMassInKilograms => Header.Bills.Aggregate(ZDecimal.Zero, (current, bill)
	=> current + bill.ArrivalGoodsItems.Where(goodsItem => goodsItem.BY_UnloadedState != EU.NCTS.Business.NctsUnloadedStateList.Codes.MIS && bill.MovementDetail.B9_UnloadedState != EU.NCTS.Business.NctsUnloadedStateList.Codes.MIS)
		.Cast<EU.NCTS.Business.NctsArrivalCargoDesc>()
		.Sum(goodsItem => goodsItem.BY_UnloadedState == EU.NCTS.Business.NctsUnloadedStateList.Codes.DIF ? goodsItem.UnloadedGoodsItem.GrossMassInKilograms : goodsItem.GrossMassInKilograms));

	public override ZString AuthorizationNumber
	{
		get => base.AuthorizationNumber;
		set
		{
			base.AuthorizationNumber = value;
			GoodsLocation.SetDefaultsFromAuthorizationIfNeeded(CusAuthorisationHeader.Loader.GetAuthorisationsWithSpecificNumberTypeAndCountryCode(Factory, AuthorizationNumber, AuthorizationCode, CountryCode).FirstOrDefault());
			GoodsLocationDescriptionInfo.RefreshBinding();
		}
	}
}
