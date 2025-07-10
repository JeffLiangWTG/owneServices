using System.ComponentModel;
using CargoWise.ComponentModel;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Rating.Business
{
	[UniversalDataContext(DataContextType.Costing)]
	[CodeProperty(RatingHeader.Schema.TH_ClientCode), DescriptionProperty(RatingHeader.Schema.TH_ClientCode)]
	[BusinessContext(BusinessContext.Rating)]
	public class Costing : RatingHeader, IRelatableActivity, IImportParentRelatedActivityInfoOnNew, ITemplateCopyable
	{
		public Costing(BusinessObjectFactory factory, System.Data.DataRow row)
			: base(factory, row)
		{
			UpdateGlobalRateDescription();
		}

		#region Validation

		protected override RatingHeaderValidation GetNewValidation() => new CostingValidation(this);
		public new CostingValidation Validation => (CostingValidation)base.Validation;

		#endregion

		#region Copy Rate

		protected override void SetNewValuesInHeader(RatingHeader newHeader)
		{
			base.SetNewValuesInHeader(newHeader);
			foreach (RateEntryCollection entryCollection in ((Costing)newHeader).EntryCollectionsExcludingSummary.Values)
			{
				SetNewValuesInEntryCollection(entryCollection);
			}
		}

		protected override void SetNewValuesInEntry(RateEntry newEntry)
		{
			newEntry.TI_RateStartDate = ZDate.Today;
			newEntry.TI_RateEndDate = newEntry.DefaultRateEndDate;
		}

		protected override bool SupportsCloneCore() => true;

		#endregion

		#region Properties

		#region TH_OH

		public override ZGuid TH_OH
		{
			get { return base.TH_OH; }
			set
			{
				base.TH_OH = value;
				UpdateGlobalRateDescription();
			}
		}

		#endregion

		#region TH_ClientFullName

		public override ZString TH_ClientFullName
		{
			get { return this.IsStandardCostRate() ? TH_GlobalRateDescriptionMultilingual : base.TH_ClientFullName; }
		}

		#endregion

		#region Supplier Type

		[MaxLength(20)]
		public ZString SupplierType
		{
			get
			{
				if (Header == null)
				{
					return ZString.Empty;
				}

				if (Header.OH_IsForwarder)
				{
					return Res.GetString("d4ed1856-57d7-48f4-a655-c5cc2612dd28", "Forwarder");
				}

				if (Header.OH_IsLocalTransport)
				{
					return Res.GetString("99b5b26d-dc8c-4099-8254-0805c1cea6c9", "Port Transport");
				}

				if (Header.OH_IsShippingProvider)
				{
					return Res.GetString("5d9dd779-d243-4867-b325-f2fa828ba9d9", "Carrier");
				}

				return ZString.Empty;
			}
		}

		public ZPropertyInfo SupplierTypeInfo
		{
			get { return GetZPropertyInfo(nameof(SupplierType)); }
		}

		#endregion

		#region Supplier UNLOCO

		[MaxLength(5)]
		public ZString SupplierUNLOCO
		{
			get { return Header != null ? Header.OH_RL_NKClosestPort : ZString.Empty; }
		}

		public ZPropertyInfo SupplierUNLOCOInfo
		{
			get { return GetZPropertyInfo(nameof(SupplierUNLOCO)); }
		}

		#endregion

		#region TH_GlobalRateDescription

		protected bool TH_GlobalRateDescription_ReadOnly
		{
			get { return !this.IsStandardCostRate(); }
		}

		void UpdateGlobalRateDescription()
		{
			if (!TH_OH.IsEmpty)
			{
				TH_GlobalRateDescription = string.Empty;
			}
			else if (TH_GlobalRateDescriptionMultilingual.IsEmpty)
			{
				TH_GlobalRateDescription = StandardGlobalRateDescription;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Translatable field English value")]
		public const string StandardGlobalRateDescription = "Standard Costs (TACT/General Rates)";

		#endregion

		#region DisplayInfo / RatingHeaderTypeDescription

		protected override string RatingHeaderTypeDescriptionCore => TH_GC.IsEmpty
			? Res.GetString("e2f5cec3-381b-45a3-841a-e3edbab39231", "Global Costing")
			: Res.GetString("453dabfb-d8c2-45e0-999a-724ce0acc0e0", "Costing");

		public override ZString DisplayInfo()
		{
			var desc = GlobalRateDescriptionInCurrentLanguage;
			if (!string.IsNullOrEmpty(desc))
			{
				return desc;
			}
			else
			{
				return DisplayInfoWithOrgInfo(RatingHeaderTypeDescription, Header);
			}
		}

		#endregion

		#endregion

		#region IRelatableActivity Members

		ZBool IRelatableActivity.ShouldIgnoreSuperAndSubActivityRelationships => false;
		ZString IRelatableActivity.ActivityType => RelatableActivityTypeList.Codes.ClientRates;
		IOrgHeader IRelatableActivity.Client => Header;
		ZBool IRelatableActivity.ClientHasChanges => TH_OHInfo.HasChanges;
		IOrgContact IRelatableActivity.Contact => null;
		ZBool IRelatableActivity.ContactHasChanges => false;
		ZString IRelatableActivity.Summary => Res.GetString("888FA90B-0C85-4004-9DEB-98B0D9D0CC38", "Costing; {0}", Header?.OH_Code ?? ZString.Empty);
		void IRelatableActivity.OnRelatedActivitySaving(IRelatableActivity relatedActivity) { }
		ZBool IRelatableActivity.SupportViewRelatedCommunications => ZBool.True;

		public IRelatedChildActivityPivotCollection RelatedChildActivityPivotCollection
			=> relatedChildActivityPivotCollection ?? (relatedChildActivityPivotCollection = new RelatedChildActivityPivotCollection(this));
		RelatedChildActivityPivotCollection relatedChildActivityPivotCollection;

		public IRelatedParentActivityPivotCollection RelatedParentActivityPivotCollection
			=> relatedParentActivityPivotCollection ?? (relatedParentActivityPivotCollection = new RelatedParentActivityPivotCollection(this));

		RelatedParentActivityPivotCollection relatedParentActivityPivotCollection;

		#endregion

		#region IImportParentRelatedActivityInfoOnNew

		bool IImportParentRelatedActivityInfoOnNew.ImportParentInfo(IRelatableActivity parentActivity, IImportRelatedActivityDeciderFactory deciderFactory)
		{
			if (parentActivity.Client != null)
			{
				TH_OH = parentActivity.Client.PK;
			}

			return true;
		}

		#endregion

		#region ITemplateCopyable Members

		IBusiness ITemplateCopyable.TemplateCopy()
		{
			var copy = CopyIncludingChildren();
			copy.TH_OH = ZGuid.Empty;

			return copy;
		}

		#endregion

		#region Delete

		public override void Delete()
		{
			ViewRelatedActivityPivot.DeleteAllPivots(this);

			base.Delete();
		}

		#endregion

#if DEBUG
		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);

			TH_OH = Factory.NewWithValidTestData<OrgHeader>().PK;
		}
#endif
	}
}
