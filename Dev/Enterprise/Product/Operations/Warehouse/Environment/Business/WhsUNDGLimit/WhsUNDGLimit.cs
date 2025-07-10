using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Warehouse.Environment.Business
{
	[CodeAlive("This Business Object is used in Glow.")]
	public class WhsUNDGLimit :
		AutoWhsUNDGLimit,
		IWhsUNDGLimit,
		IUNDGSubstancePivotParent
	{
		public WhsUNDGLimit(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		#region Warehouse

		public WhsWarehouse Warehouse => Factory.Load<WhsWarehouse>(WWD_WW_Warehouse);

		[RelatedBusinessObject("Warehouse")]
		public override ZGuid WWD_WW_Warehouse { get => base.WWD_WW_Warehouse; set => base.WWD_WW_Warehouse = value; }

		#endregion

		#region UNDGSubstance

		public UNDGSubstance UNDGSubstance => Factory.Load<UNDGSubstance>(WWD_DG);

		[List("Lookups.UNDGSubstances")]
		[RelatedBusinessObject("UNDGSubstance")]
		[ReadOnlyMember(nameof(SubstanceReadOnly))]
		public override ZGuid WWD_DG
		{
			get => base.WWD_DG;
			set
			{
				if (base.WWD_DG != value)
				{
					var substance = Factory.Load<UNDGSubstance>(value);
					UNDGSubstancePivotCollection.UpdateDefaultPivot(substance);

					base.WWD_DG = value;
				}
			}
		}

		bool SubstanceReadOnly => CountryReference != null || !WWD_UNDGClass.IsEmpty;

		[ChildEditable(true)]
		public UNDGSubstancePivotCollection UNDGSubstancePivotCollection
		{
			get
			{
				if (undgSubstancePivotCollection == null)
				{
					undgSubstancePivotCollection = new UNDGSubstancePivotCollection(Factory, this, WhsUNDGLimitSchema.Constants.Prefix);
					RegisterEditableChildObject(undgSubstancePivotCollection);
				}

				return undgSubstancePivotCollection;
			}
		}

		UNDGSubstancePivotCollection undgSubstancePivotCollection;

		#endregion

		#region UNDGCountryReference

		public UNDGCountryReference CountryReference => Factory.Load<UNDGCountryReference>(WWD_DCR_UNDGCountryReference);

		[List("Lookups.UNDGCountryReferences")]
		[RelatedBusinessObject("UNDGCountryReference")]
		[ReadOnlyMember(nameof(UNDGCountryReferenceReadOnly))]
		public override ZGuid WWD_DCR_UNDGCountryReference { get => base.WWD_DCR_UNDGCountryReference; set => base.WWD_DCR_UNDGCountryReference = value; }

		bool UNDGCountryReferenceReadOnly => UNDGSubstance != null || !WWD_UNDGClass.IsEmpty;

		#endregion

		#region UNDGClass

		[List("Lookups.UNDGClass")]
		[ReadOnlyMember(nameof(UNDGClassReadOnly))]
		public override ZString WWD_UNDGClass { get => base.WWD_UNDGClass; set => base.WWD_UNDGClass = value; }

		bool UNDGClassReadOnly => UNDGSubstance != null || CountryReference != null;

		#endregion

		#region Properties

		public override ZDecimal WWD_TotalWeightLimit
		{
			get => base.WWD_TotalWeightLimit;
			set
			{
				base.WWD_TotalWeightLimit = value;

				// Tested in WhsUNDGLimitValidationTest
				if (!IsValidationSuspended)
				{
					Validation.ValidateWWD_TotalWeightLimitUQ();
				}
			}
		}

		[List(nameof(Lookups) + "." + nameof(WhsUNDGLimitLookups.WeightUnits))]
		public override ZString WWD_TotalWeightLimitUQ
		{
			get => base.WWD_TotalWeightLimitUQ;
			set => base.WWD_TotalWeightLimitUQ = value;
		}

		public override ZDecimal WWD_TotalVolumeLimit
		{
			get => base.WWD_TotalVolumeLimit;
			set
			{
				base.WWD_TotalVolumeLimit = value;

				// Tested in WhsUNDGLimitValidationTest
				if (!IsValidationSuspended)
				{
					Validation.ValidateWWD_TotalVolumeLimitUQ();
				}
			}
		}

		[List(nameof(Lookups) + "." + nameof(WhsUNDGLimitLookups.VolumeUnits))]
		public override ZString WWD_TotalVolumeLimitUQ
		{
			get => base.WWD_TotalVolumeLimitUQ;
			set => base.WWD_TotalVolumeLimitUQ = value;
		}

		#region UNDGLimitDescription

		public ZString UNDGLimitDescription
		{
			get
			{
				var result = ZString.Empty;
				if (UNDGSubstance != null || CountryReference != null || WWD_UNDGClass != string.Empty)
				{
					var dgCode = UNDGSubstance != null ? UNDGSubstance.DG_Code : (WWD_UNDGClass != string.Empty ? new ZString(UNDGDataItemLookups.GetDGClassList(Factory).GetDescriptionFromCode(WWD_UNDGClass)) : CountryReference.DCR_Code);

					if (WWD_TotalVolumeLimit == 0 && WWD_TotalWeightLimit == 0)
					{
						result = Res.GetString("999dfd1f-275a-4d38-b148-be17cae44296", "When weight or volume threshold is zero, storage is not permitted of this UNDG");
					}
					else if (WWD_TotalWeightLimit > 0 && !WWD_TotalWeightLimitUQ.IsEmpty && WWD_TotalVolumeLimit == 0)
					{
						result = Res.GetString("f100ebf8-2d68-4ca9-8021-c12591833f04", "{0} is allowed to store up to {1} {2} in the warehouse", dgCode, WWD_TotalWeightLimit, WWD_TotalWeightLimitUQ);
					}
					else if (WWD_TotalVolumeLimit > 0 && !WWD_TotalVolumeLimitUQ.IsEmpty && WWD_TotalWeightLimit == 0)
					{
						result = Res.GetString("7fc850a4-e0f5-411e-8a77-b4ea8bac3a40", "{0} is allowed to store up to {1} {2} in the warehouse", dgCode, WWD_TotalVolumeLimit, WWD_TotalVolumeLimitUQ);
					}
					else if (WWD_TotalVolumeLimit > 0 && !WWD_TotalWeightLimitUQ.IsEmpty && WWD_TotalWeightLimit > 0 && !WWD_TotalVolumeLimitUQ.IsEmpty)
					{
						result = Res.GetString("0e2ef804-8e91-4ca6-bd9d-f2712ededd11", "{0} is allowed to store up to {1} {2} and {3} {4} in the warehouse", dgCode, WWD_TotalWeightLimit, WWD_TotalWeightLimitUQ, WWD_TotalVolumeLimit, WWD_TotalVolumeLimitUQ);
					}
				}

				return result;
			}
		}

		public ZPropertyInfo UNDGLimitDescriptionInfo
		{
			get { return GetZPropertyInfo(nameof(UNDGLimitDescription)); }
		}

		#endregion

		#region IsDGAllowed

		public ZBool IsDGAllowed => WWD_TotalWeightLimit > 0 || WWD_TotalVolumeLimit > 0;

		#endregion

		#endregion

		#region Delete

		public override void Delete()
		{
			if (!WWD_DG.IsEmpty || (WWD_DGInfo.HasChanges && !WWD_DGInfo.OriginalValue.IsEmpty))
			{
				UNDGSubstancePivotCollection.DeleteAll();
			}
			base.Delete();
		}

		#endregion

		#region OnFactorySavingBeforeTransactionCore

		protected override void OnFactorySavingBeforeTransactionCore()
		{
			base.OnFactorySavingBeforeTransactionCore();

			if (!IsInDatabase || IsDeleting || WWD_DGInfo.HasChanges)
			{
				Logs.CreateOrRecreateEventLog(Events.DangerousGoodsChanged, EstimateActual.Actual, ZDateTimeOffset.Now);
			}
		}

		#endregion
	}
}
