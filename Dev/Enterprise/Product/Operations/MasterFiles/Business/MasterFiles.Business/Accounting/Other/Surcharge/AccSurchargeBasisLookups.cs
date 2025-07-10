//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoAccSurchargeBasisLookups
//
//    This class should be used for overriding collections in AutoAccSurchargeBasisLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class AccSurchargeBasisLookups : AutoAccSurchargeBasisLookups
	{
		public AccSurchargeBasisLookups(AutoAccSurchargeBasis parent) : base(parent)
		{
		}

		#region Charge Groups

		public CodeDescriptionPairList ChargeGroupList
		{
			get { return new ChargeCodeGroupList(); }
		}

		#endregion

		#region ChargeCodes

		public override AccChargeCodeCollection ChargeCodes
		{
			get
			{
				var accSurchargeBasis = Parent as AccSurchargeBasis;
				var surchargeConfig = Factory.Load<AccSurchargeConfiguration>(accSurchargeBasis.ASB_ASC_SurchargeConfiguration);
				var filter = new ZQuery(AccChargeCodeSchema.AC_ChargeType, SQLComparisonOperator.NotEqual, Core.Constants.ChargeType.Overhead);

				return new AccChargeCodeCollection(Factory, filter, (surchargeConfig?.Company.PK ?? ZGuid.Empty).ToGuid());
			}
		}

		#endregion
	}
}
