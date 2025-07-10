//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoAccSurchargeConfigurationLookups
//
//    This class should be used for overriding collections in AutoAccSurchargeConfigurationLookups
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
	public class AccSurchargeConfigurationLookups : AutoAccSurchargeConfigurationLookups
	{
		public AccSurchargeConfigurationLookups(AutoAccSurchargeConfiguration parent) : base(parent)
		{
		}

		#region ChargeCodes

		public override AccChargeCodeCollection ChargeCodes
		{
			get
			{
				var surchargeConfiguration = Parent as AccSurchargeConfiguration;
				var filter = new ZQuery(AccChargeCodeSchema.AC_ChargeType, SQLComparisonOperator.NotEqual, Core.Constants.ChargeType.Overhead);

				return new AccChargeCodeCollection(Factory, filter, (surchargeConfiguration?.ASC_GC_Company ?? ZGuid.Empty).ToGuid());
			}
		}

		#endregion

		#region Surcharge Type

		public CodeDescriptionPairList SurchargeTypeList
		{
			get { return new SurchargeTypeList(); }
		}

		#endregion

		#region Surcharge Basis Type

		public CodeDescriptionPairList SurchargeBasisTypeList
		{
			get { return new SurchargeBasisTypeList(); }
		}

		#endregion
	}
}
