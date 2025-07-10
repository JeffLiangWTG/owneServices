using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.TR.ETrade.Business
{
	public class CusBondDetail : MasterFiles.Business.CusBondDetail
	{
		public CusBondDetail(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override TypeLoaderCollection ParentLoaders
		{
			get { return new TypeLoaderCollection(typeof(AsycudaBill), typeof(AsycudaManifestHeader)); }
		}

		public override ZString PW_BondType
		{
			get => base.PW_BondType;
			set
			{
				base.PW_BondType = value;
				Parent.MarkAsNeedingValidation();
			}
		}

		public override ZGuid PW_ParentID
		{
			get => base.PW_ParentID;
			set
			{
				base.PW_ParentID = value;
				Parent?.MarkAsNeedingValidation();
			}
		}

		public override ZString PW_ParentTableCode
		{
			get => base.PW_ParentTableCode;
			set
			{
				base.PW_ParentTableCode = value;
				Parent?.MarkAsNeedingValidation();
			}
		}

		public override ZDecimal PW_BondAmount
		{
			get => base.PW_BondAmount;
			set
			{
				base.PW_BondAmount = value;
				Parent?.MarkAsNeedingValidation();
			}
		}

		public override void OnSaving()
		{
			if (PW_BondAmount.IsEmpty && PW_BondNumber.IsEmpty && PW_BondType.IsEmpty)
			{
				this.Delete();
			}
			base.OnSaving();
		}
	}
}
