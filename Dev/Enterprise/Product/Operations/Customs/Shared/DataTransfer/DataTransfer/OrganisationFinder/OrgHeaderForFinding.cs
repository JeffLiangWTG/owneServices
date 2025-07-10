using System.Data;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.DataTransfer
{
	public class OrgHeaderForFinding : OrgHeader
	{
		public OrgHeaderForFinding(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override OrgPatternMatchCollection GetNewPatternMatchesForThisOrgCollection(BusinessObjectFactory factory, ZQuery additionalFilter)
		{
			return new OrgPatternMatchCollectionForFinding(this, additionalFilter);
		}

		protected override bool EnableLightValidationIfAvailable
		{
			get { return false; }
		}

		public bool AllowEmptyAddresses { get; set; }
	}
}
