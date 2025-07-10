using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class AccPOSChargeCodeGroup : AutoAccPOSChargeCodeGroupView
	{
		public AccPOSChargeCodeGroup(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			GRO_GroupType = POSChargeCodeGroupType;
			GRO_GC = Env.CurrentCompanyPK;
		}

		internal const string POSChargeCodeGroupType = "POS";

		#region Overridden Properties

		[ResourceStringData("AccPOSChargeCodeGroup|GRO_GroupType", Caption = "Group Type")]
		[ReadOnly(true)]
		public override ZString GRO_GroupType { get => base.GRO_GroupType; set => base.GRO_GroupType = value; }

		[ResourceStringData("AccPOSChargeCodeGroup|GRO_GC", Caption = "Company")]
		[List(nameof(Lookups) + "." + nameof(AccPOSChargeCodeGroupViewLookups.Companies))]
		[ReadOnly(true)]
		public override ZGuid GRO_GC { get => base.GRO_GC; set => base.GRO_GC = value; }

		[ResourceStringData("AccPOSChargeCodeGroup|GRO_Code", Caption = "Code", FullDescription = "Group Code")]
		public override ZString GRO_Code { get => base.GRO_Code; set => base.GRO_Code = value; }

		[ResourceStringData("AccPOSChargeCodeGroup|GRO_Description", Caption = "Description", FullDescription = "Group Description")]
		public override ZString GRO_Description { get => base.GRO_Description; set => base.GRO_Description = value; }

		#endregion

		#region Logging

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		#endregion

		#region Related Business Objects

		public GlbCompany Company => Factory.Load<GlbCompany>(GRO_GC);

		#endregion

		#region ChargeCodePivots

		[ChildEditable()]
		public AccPOSChargeCodeGroupPivotCollection ChargeCodePivots
		{
			get
			{
				if (chargeCodePivots == null)
				{
					chargeCodePivots = new AccPOSChargeCodeGroupPivotCollection(this);
					chargeCodePivots.Load();
					RegisterEditableChildObject(chargeCodePivots);
				}
				return chargeCodePivots;
			}
		}
		AccPOSChargeCodeGroupPivotCollection chargeCodePivots;

		#endregion

		#region AccPlaceOfSupplyConfigurations

		[ChildEditable(true)]
		public AccPOSConfigurationCollection AccPlaceOfSupplyConfigurations
		{
			get
			{
				if (accPlaceOfSupplyConfigurations == null)
				{
					var localAccPlaceOfSupplyConfigurations = new AccPOSConfigurationCollection(this);
					localAccPlaceOfSupplyConfigurations.Load();
					accPlaceOfSupplyConfigurations = localAccPlaceOfSupplyConfigurations;
					RegisterEditableChildObject(accPlaceOfSupplyConfigurations);
				}
				return accPlaceOfSupplyConfigurations;
			}
		}
		AccPOSConfigurationCollection accPlaceOfSupplyConfigurations;

		#endregion

		#region ICanDelete Members

		public override bool CanDelete => !ChargeCodePivots.Any() && !AccPlaceOfSupplyConfigurations.OfType<AccPOSConfiguration>().Any(x => x.PSC_ParentId == PK);

		public override MultilingualString ReasonForNotAbleToDelete => ResString.GetMultilingualString("8cabb8cd-78d1-4e0b-a7b8-df054dfdd69a", "Remove Charge Codes and Place of Supply Group level configurations before deleting Group.");

		#endregion

#if DEBUG
		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);
			var helper = new BusinessObjectTestDataHelper();
			helper.PopulateUniqueString(GRO_CodeInfo, propertyPath);
			helper.PopulateUniqueString(GRO_DescriptionInfo, propertyPath);
		}
#endif
	}
}
