using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business.Interfaces;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ZA.Business
{
	[UniversalDataContext(DataContextType.ZAOutTurn)]
	public class AsycudaPack : ManifestBase.AsycudaPack
		, Integration.Customs.ZA.IAsycudaPack
		, IOutturnableLine
	{
		public AsycudaPack(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		[ResourceStringData("Enterprise.Customs.ZA.Business.AsycudaPack.APA_PackQty", Caption = "No. Of Packs")]
		public override ZInt APA_PackQty
		{
			get => base.APA_PackQty;
			set
			{
				base.APA_PackQty = value;
				if (Outturn != null && Outturn.ShouldSetActuallyFoundToBe)
				{
					Outturn.C5_PackagesOutturned = APA_PackQty;
				}
			}
		}

		[ResourceStringData("Enterprise.Customs.ZA.Business.AsycudaPack.APA_PackUQ", Caption = "Pack Unit")]
		public override ZString APA_PackUQ
		{
			get => base.APA_PackUQ;
			set => base.APA_PackUQ = value;
		}

		[ResourceStringData("Enterprise.Customs.ZA.Business.AsycudaPack.APA_Weight", Caption = "Weight Manifested")]
		public override ZDecimal APA_Weight
		{
			get => base.APA_Weight;
			set
			{
				base.APA_Weight = value;
				if (Outturn != null && Outturn.ShouldSetActuallyFoundToBe)
				{
					Outturn.C5_WeightOutturned = APA_Weight;
				}
			}
		}

		[ResourceStringData("Enterprise.Customs.ZA.Business.AsycudaPack.APA_WeightUQ", Caption = "Weight UQ")]
		public override ZString APA_WeightUQ
		{
			get => base.APA_WeightUQ;
			set
			{
				base.APA_WeightUQ = value;
				if (Outturn != null && Outturn.ShouldSetActuallyFoundToBe)
				{
					Outturn.C5_WeightOutturnedUQ = APA_WeightUQ;
				}
			}
		}

		[ResourceStringData("Enterprise.Customs.ZA.Business.AsycudaPack.APA_Volume", Caption = "Volume Manifested")]
		public override ZDecimal APA_Volume
		{
			get => base.APA_Volume;
			set
			{
				base.APA_Volume = value;
				if (Outturn != null && Outturn.ShouldSetActuallyFoundToBe)
				{
					Outturn.C5_VolumeOutturned = APA_Volume;
				}
			}
		}

		[ResourceStringData("Enterprise.Customs.ZA.Business.AsycudaPack.APA_VolumeUQ", Caption = "Volume UQ")]
		public override ZString APA_VolumeUQ
		{
			get => base.APA_VolumeUQ;
			set
			{
				base.APA_VolumeUQ = value;
				if (Outturn != null && Outturn.ShouldSetActuallyFoundToBe)
				{
					Outturn.C5_VolumeOutturnedUQ = APA_VolumeUQ;
				}
			}
		}

		[ResourceStringData("Enterprise.Customs.ZA.Business.AsycudaPack.APA_MarksAndNumbers", Caption = "Marks and Numbers")]
		public override ZString APA_MarksAndNumbers
		{
			get => base.APA_MarksAndNumbers;
			set => base.APA_MarksAndNumbers = value;
		}

		[ResourceStringData("Enterprise.Customs.ZA.Business.AsycudaPack.APA_GoodsDescription", Caption = "Goods Description", MediumCaption = "Goods Desc.")]
		public override ZString APA_GoodsDescription
		{
			get => base.APA_GoodsDescription;
			set => base.APA_GoodsDescription = value;
		}

		public CusOutturn Outturn
		{
			get
			{
				if (outturn == null || (outturn.IsDeleted && !IsDeleted))
				{
					outturn = LoadOutturn();
					if (outturn == null)
					{
						outturn = Factory.New<CusOutturn>();
						outturn.C5_ParentID = PK;
						outturn.C5_ParentTableCode = TablePrefix;
					}
					RegisterEditableChildObject(outturn);
				}
				return outturn;
			}
		}
		CusOutturn outturn;

		CusOutturn LoadOutturn()
		{
			var q = new ZQuery(CusOutturnSchema.C5_ParentID, PK) { FetchOnlyFromLocalCache = !IsInDatabase };
			return Factory.LoadTop1<CusOutturn>(q);
		}

		public new AsycudaBill Bill => (AsycudaBill)base.Bill;
		public new AsycudaContainer Container => (AsycudaContainer)base.Container;

		public AsycudaManifestHeader Header => Bill?.Header;

		protected override ManifestBase.AsycudaPackValidation GetNewValidation() => new AsycudaPackValidation(this);

		public new AsycudaPackValidation Validation => (AsycudaPackValidation)base.Validation;

		public new AsycudaPackLookups Lookups => (AsycudaPackLookups)base.Lookups;

		protected override ManifestBase.AsycudaPackLookups GetNewLookups() => new AsycudaPackLookups(this);

		ZString IOutturnableLine.UnderbondHumanReadableName => ZString.Format("Pack: {0}", APA_LineNo);

		ZString IOutturnableLine.CargoStatus => ZString.Empty;

		ZInt IOutturnableLine.PackagesManifested => APA_PackQty;
	}
}
