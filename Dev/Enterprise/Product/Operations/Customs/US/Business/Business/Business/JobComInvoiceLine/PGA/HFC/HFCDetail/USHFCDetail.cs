using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business
{
	public class USHFCDetail : AutoUSHFCDetail, IHFCDetail
	{
		public USHFCDetail(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public USHFCHeader Header
		{
			get { return (USHFCHeader)Parent; }
		}

		[ResourceStringData("Enterprise.Customs.US.Business.USHFCDetail|US_LPCONumber", Caption = "Product Code Number")]
		public override ZString US_LPCONumber
		{
			get => base.US_LPCONumber;
			set => base.US_LPCONumber = value;
		}

		[ResourceStringData("Enterprise.Customs.US.Business.USHFCDetail|US_NameOfActiveIngredient", Caption = "Constituent Element")]
		public override ZString US_NameOfActiveIngredient
		{
			get => base.US_NameOfActiveIngredient;
			set => base.US_NameOfActiveIngredient = value;
		}

		[ResourceStringData("Enterprise.Customs.US.Business.USHFCDetail|US_ActiveIngredientPercentage", Caption = "%")]
		public override ZDecimal US_ActiveIngredientPercentage
		{
			get => base.US_ActiveIngredientPercentage;
			set => base.US_ActiveIngredientPercentage = value;
		}

		#region IHFCDetail

		ZString IHFCDetail.ProductCode => US_LPCONumber;

		ZString IHFCDetail.NameOfActiveIngredient => US_NameOfActiveIngredient;

		ZDecimal IHFCDetail.ActiveIngredientPercentage => US_ActiveIngredientPercentage;

		#endregion
	}
}
