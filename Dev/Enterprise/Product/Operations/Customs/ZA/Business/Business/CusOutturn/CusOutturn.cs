using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Business.CustomValues;

namespace Enterprise.Customs.ZA.Business
{
	[SystemDefinedValues]
	public class CusOutturn : Customs.Business.CusOutturn
		, Integration.Customs.ZA.ICusOutturn
	{
		public CusOutturn(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public new class Schema : Customs.Business.CusOutturn.Schema
		{
			public const string PackCondDesc = "PackCondDesc";
			public const string ExcessShortInd = "ExcessShortInd";
			public const string ContShouldBe = "ContShouldBe";

			public const int PackCondDescMaxLength = 100;
			public const int ExccessShortIndMaxLength = 1;
			public const int ContShouldBeMaxLength = 100;
		}

		protected override TypeLoaderCollection GetParentLoaders()
		{
			var result = base.GetParentLoaders();
			result.Add(new TypeLoader(typeof(AsycudaPack)));
			return result;
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();

			var row = ((IBusinessObjectInternals)this).Row;
			row["C5_VolumeOutturnedUQ"] = string.Empty;
			row["C5_WeightOutturnedUQ"] = string.Empty;
		}

		[ResourceStringData("Enterprise.Customs.ZA.Business.CusOutturn.C5_CargoType", Caption = "Cargo Type")]
		[List(nameof(Lookups) + "." + nameof(CusOutturnLookups.CargoTypeList))]
		public override ZString C5_CargoType
		{
			get => base.C5_CargoType;
			set => base.C5_CargoType = value;
		}

		[ResourceStringData("Enterprise.Customs.ZA.Business.CusOutturn.C5_PackagesOutturned", Caption = "Packs Found")]
		public override ZInt C5_PackagesOutturned
		{
			get => base.C5_PackagesOutturned;
			set => base.C5_PackagesOutturned = value;
		}

		[ResourceStringData("Enterprise.Customs.ZA.Business.CusOutturn.C5_PackageCondition", Caption = "Pack Condition")]
		[List(nameof(Lookups) + "." + nameof(CusOutturnLookups.PackConditionList))]
		public override ZString C5_PackageCondition
		{
			get => base.C5_PackageCondition;
			set => base.C5_PackageCondition = value;
		}

		#region PackCondDesc

		[ResourceStringData("Enterprise.Customs.ZA.Business.CusOutturn.PackCondDesc", Caption = "Pack Condition Description", MediumCaption = "Pack Cond. Desc.")]
		[MaxLength(Schema.PackCondDescMaxLength)]
		public ZString PackCondDesc
		{
			get => this.GetSystemDefinedValue<ZString>(Schema.PackCondDesc);
			set
			{
				var oldValue = PackCondDesc;
				CheckMaximumLength(PackCondDescInfo, value);
				this.SetSystemDefinedValue(Schema.PackCondDesc, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidatePackCondDesc();
				}
				PackCondDescInfo.RefreshBinding(oldValue);
			}
		}

		public ZPropertyInfo PackCondDescInfo => GetZPropertyInfo(Schema.PackCondDesc);

		#endregion

		[ResourceStringData("Enterprise.Customs.ZA.Business.CusOutturn.C5_GoodsDescription", Caption = "Contents Found to Be")]
		public override ZString C5_GoodsDescription
		{
			get => base.C5_GoodsDescription;
			set => base.C5_GoodsDescription = value;
		}

		#region ExcessShortInd

		[ResourceStringData("Enterprise.Customs.ZA.Business.CusOutturn.ExcessShortInd", Caption = "Excess/Short Indicator", MediumCaption = "Excess/Short Ind.")]
		[List(nameof(Lookups) + "." + nameof(CusOutturnLookups.ExcessShortIndicatorList))]
		[MaxLength(Schema.ExccessShortIndMaxLength)]
		public ZString ExcessShortInd
		{
			get => this.GetSystemDefinedValue<ZString>(Schema.ExcessShortInd);
			set
			{
				var oldValue = ExcessShortInd;
				CheckMaximumLength(ExcessShortIndInfo, value);
				this.SetSystemDefinedValue(Schema.ExcessShortInd, value);
				SetActuallyFoundToBeValues();
				if (!IsValidationSuspended)
				{
					Validation.ValidateExcessShortInd();
				}
				ExcessShortIndInfo.RefreshBinding(oldValue);
			}
		}

		public ZPropertyInfo ExcessShortIndInfo => GetZPropertyInfo(Schema.ExcessShortInd);

		#endregion

		#region ContShouldBe

		[ResourceStringData("Enterprise.Customs.ZA.Business.CusOutturn.ContShouldBe", Caption = "Contents Should Be")]
		[MaxLength(Schema.ContShouldBeMaxLength)]
		public ZString ContShouldBe
		{
			get => this.GetSystemDefinedValue<ZString>(Schema.ContShouldBe);
			set
			{
				var oldValue = ContShouldBe;
				CheckMaximumLength(ContShouldBeInfo, value);
				this.SetSystemDefinedValue(Schema.ContShouldBe, value);
				if (ShouldSetActuallyFoundToBe)
				{
					C5_GoodsDescription = ContShouldBe;
				}
				if (!IsValidationSuspended)
				{
					Validation.ValidateContShouldBe();
				}
				ContShouldBeInfo.RefreshBinding(oldValue);
			}
		}

		public ZPropertyInfo ContShouldBeInfo => GetZPropertyInfo(Schema.ContShouldBe);

		#endregion

		[ResourceStringData("Enterprise.Customs.ZA.Business.CusOutturn.C5_WeightOutturned", Caption = "Weight Found")]
		public override ZDecimal C5_WeightOutturned
		{
			get => base.C5_WeightOutturned;
			set => base.C5_WeightOutturned = value;
		}

		[List(nameof(Lookups) + "." + nameof(CusOutturnLookups.WeightUQList))]
		[ResourceStringData("Enterprise.Customs.ZA.Business.CusOutturn.C5_WeightOutturnedUQ", Caption = "Weight Found UQ")]
		public override ZString C5_WeightOutturnedUQ
		{
			get => base.C5_WeightOutturnedUQ;
			set => base.C5_WeightOutturnedUQ = value;
		}

		[ResourceStringData("Enterprise.Customs.ZA.Business.CusOutturn.C5_VolumeOutturned", Caption = "Volume Found")]
		public override ZDecimal C5_VolumeOutturned
		{
			get => base.C5_VolumeOutturned;
			set => base.C5_VolumeOutturned = value;
		}

		[List(nameof(Lookups) + "." + nameof(CusOutturnLookups.VolumeUQList))]
		[ResourceStringData("Enterprise.Customs.ZA.Business.CusOutturn.C5_VolumeOutturnedUQ", Caption = "Volume Found UQ")]
		public override ZString C5_VolumeOutturnedUQ
		{
			get => base.C5_VolumeOutturnedUQ;
			set => base.C5_VolumeOutturnedUQ = value;
		}

		[ResourceStringData("Enterprise.Customs.ZA.Business.CusOutturn.C5_SealIntactIndicator", Caption = "Seal Intact")]
		public override ZBool C5_SealIntactIndicator
		{
			get => base.C5_SealIntactIndicator;
			set => base.C5_SealIntactIndicator = value;
		}

		public new CusOutturnLookups Lookups => (CusOutturnLookups)base.Lookups;

		protected override Customs.Business.CusOutturnLookups GetNewLookups() => new CusOutturnLookups(this);

		public new CusOutturnValidation Validation => (CusOutturnValidation)base.Validation;

		public AsycudaPack Pack => (AsycudaPack)base.Parent;

		public AsycudaManifestHeader Header => ((AsycudaPack)base.Parent)?.Header;

		public bool ShouldSetActuallyFoundToBe => Header != null && Header.IsCOSTCO && ExcessShortInd == ExcessShortIndicatorList.Codes.None;

		protected override Customs.Business.CusOutturnValidation GetNewValidation() => new CusOutturnValidation(this);

		void SetActuallyFoundToBeValues()
		{
			if (Pack != null && ShouldSetActuallyFoundToBe)
			{
				C5_GoodsDescription = ContShouldBe;
				C5_WeightOutturned = Pack.APA_Weight;
				C5_WeightOutturnedUQ = Pack.APA_WeightUQ;
				C5_VolumeOutturned = Pack.APA_Volume;
				C5_VolumeOutturnedUQ = Pack.APA_VolumeUQ;
				C5_PackagesOutturned = Pack.APA_PackQty;
			}
		}
	}
}
