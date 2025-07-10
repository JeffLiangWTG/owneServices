using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class CustomsNumberStmNumberRange : StmNumberRange
	{
		public CustomsNumberStmNumberRange(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Schema

		public new class Schema : AutoStmNumberRange.Schema
		{
			public const string SNR_OwnerForDisplay = "SNR_OwnerForDisplay";
			public const string Detail = "Detail";
			public const string TotalAvailableNumbers = "TotalAvailableNumbers";
		}

		#endregion

		#region public interface

		public bool HasReachedLimit
		{
			get { return TotalAvailableNumbers <= SNR_ThresholdRunOutWarning; }
		}

		#endregion

		#region Properties

		#region SNR_Name
		[ReadOnly(true)]
		public override ZString SNR_Name { get => base.SNR_Name; set => base.SNR_Name = value; }
		#endregion

		#region SNR_OwnerForDisplay

		[ResourceStringData("CustomsNumberStmNumberRange|SNR_OwnerForDisplay", Caption = "Owner")]
		public ZString SNR_OwnerForDisplay => Provider?.GetOwnerForDisplay(Owner) ?? ZString.Empty;

		public ZPropertyInfo SNR_OwnerForDisplayInfo
		{
			get { return GetZPropertyInfo(Schema.SNR_OwnerForDisplay); }
		}

		#endregion

		#region SNR_Owner

		[ReadOnly(true)]
		public override ZGuid SNR_Owner
		{
			get { return base.SNR_Owner; }
			set
			{
				var oldOwner = Owner;
				var oldValue = SNR_Owner;
				var oldOwnerForDisplay = SNR_OwnerForDisplay;
				base.SNR_Owner = value;
				if (!IsCopying && oldValue != SNR_Owner)
				{
					SNR_OwnerForDisplayInfo.RefreshBinding(oldOwnerForDisplay);
				}
			}
		}

		public BusinessObject Owner
		{
			get
			{
				var pk = SNR_Owner;
				if (fOwner == null || fOwner.IsDeleted || fOwner.PK != pk)
				{
					fOwner = null;
					if (pk.IsValid)
					{
						fOwner = Provider?.GetOwner(Factory, pk);
					}
				}
				return fOwner;
			}
		}
		BusinessObject fOwner;

		#endregion

		#region Detail

		[ResourceStringData("CustomsNumberStmNumberRange|Detail", Caption = "Detail")]
		public ZString Detail
		{
			get
			{
				if (DetailCached == null)
				{
					DetailCached = new CachedProperty<ZString>(Factory, () => Provider?.GetNumberRangeDetail(this) ?? SNR_Name);
				}
				return DetailCached.Value;
			}
		}
		CachedProperty<ZString> DetailCached;

		public ZPropertyInfo DetailInfo
		{
			get { return GetZPropertyInfo(Schema.Detail); }
		}

		#endregion

		#region TotalAvailableNumbers

		[ResourceStringData("CustomsNumberStmNumberRange|TotalAvailableNumbers", Caption = "Total Available Numbers", MediumCaption = "Available Numbers")]
		public ZLong TotalAvailableNumbers
		{
			get
			{
				if (TotalAvailableNumbersCached == null)
				{
					TotalAvailableNumbersCached = new CachedProperty<ZLong>(Factory, () =>
					{
						return GetStmNums().Sum(x => x.SN_AvailableNumbers);
					});
				}
				return TotalAvailableNumbersCached.Value;
			}
		}
		CachedProperty<ZLong> TotalAvailableNumbersCached;

		public ZPropertyInfo TotalAvailableNumbersInfo
		{
			get { return GetZPropertyInfo(Schema.TotalAvailableNumbers); }
		}

		#endregion

		public CustomsNumberViewStmNumsBusinessProvider Provider { get; set; }

		#endregion

		#region Methods

		public new CustomsNumberStmNumberRangeValidation Validation => (CustomsNumberStmNumberRangeValidation)base.Validation;

		protected override StmNumberRangeValidation GetNewValidation()
		{
			return new CustomsNumberStmNumberRangeValidation(this);
		}

		public CustomsNumberViewStmNums[] GetStmNums()
		{
			if (stmNumsCached == null)
			{
				stmNumsCached = new CachedProperty<CustomsNumberViewStmNums[]>(Factory, () =>
				{
					CustomsNumberViewStmNums[] result = null;
					if (SNR_Owner.IsValid && !SNR_Name.IsEmpty)
					{
						var query = new ZQuery(ViewStmNumsSchema.SN_Owner, SNR_Owner);
						query.AddToFilter(ViewStmNumsSchema.SN_Name, SQLComparisonOperator.StartsWith, SNR_Name);
						result = CustomsNumberViewStmNumsHelper.LoadStmNums(Factory, query).OrderBy(x => x.SN_SystemCreateTimeUtc).ToArray();
					}
					return result ?? System.Array.Empty<CustomsNumberViewStmNums>();
				});
			}
			return stmNumsCached.Value;
		}
		CachedProperty<CustomsNumberViewStmNums[]> stmNumsCached;

		#endregion
	}
}
