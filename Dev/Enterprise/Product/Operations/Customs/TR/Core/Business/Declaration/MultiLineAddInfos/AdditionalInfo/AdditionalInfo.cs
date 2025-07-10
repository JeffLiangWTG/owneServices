using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.TR.Business.Declaration
{
	public class AdditionalInfo : EU.Business.Declaration.MultiLineAddInfos.AdditionalInfo
	{
		public AdditionalInfo(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public new AdditionalInfoValidation Validation => (AdditionalInfoValidation)base.Validation;

		protected override CusSupportingInfoValidation GetNewValidation() => new AdditionalInfoValidation(this);
	}
}
