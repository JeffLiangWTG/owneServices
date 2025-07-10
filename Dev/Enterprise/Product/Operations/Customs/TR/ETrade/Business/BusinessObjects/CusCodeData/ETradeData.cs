using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.TR.ETrade.Business
{
	public class ETradeData : CusCodeData
	{
		public ETradeData(BusinessObjectFactory factory, DataRow row)
		: base(factory, row)
		{
		}

		protected override TypeLoaderCollection parentLoaders
		{
			get { return new TypeLoaderCollection(typeof(AsycudaManifestHeader), typeof(AsycudaBill)); }
		}

		protected override CusCodeDataValidation GetNewValidation()
		{
			if (Parent?.GetType() == typeof(AsycudaBill))
			{
				return new ETradeDataValidationForBill(this);
			}
			return new ETradeDataValidation(this);
		}

		protected override CusCodeDataLookups GetNewLookups()
		{
			return new ETradeDataLookups(this);
		}

		public override void OnSaving()
		{
			var parentType = Parent?.GetType();
			if (((parentType == typeof(AsycudaBill) && CY_Code.IsEmpty) || parentType == typeof(AsycudaManifestHeader)) && CY_Data.IsEmpty && CY_Date.IsEmpty)
			{
				this.Delete();
			}
			base.OnSaving();
		}

		public override bool CY_DataAllowWesternEuropeanCharactersOnly => false;
	}
}
