//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoRefContainerStockValidation
//
//    This class should be used for overriding validation in AutoRefContainerStockValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Agency.Business
{
	public class RefContainerStockValidation : AutoRefContainerStockValidation
	{
		public RefContainerStockValidation(AutoRefContainerStock parent)
			: base(parent) { }

		protected override void CheckR6_ContainerNum()
		{
			base.CheckR6_ContainerNum();
			MandatoryValidation.CheckEntered(Parent.R6_ContainerNumInfo);

			if (AgencyRegistry.Instance.AllowNonStandardContainerNumbersInContainerManager.Value)
			{
				ContainerNumberValidation.WarnIfInvalid(Parent.R6_ContainerNumInfo);
			}
			else
			{
				ContainerNumberValidation.ErrorIfInvalid(Parent.R6_ContainerNumInfo);
			}

			ZQuery filter = new ZQuery();
			filter.AddToFilter(RefContainerStockSchema.R6_ContainerNum, Parent.R6_ContainerNum);
			filter.AddToFilter(RefContainerStockSchema.PK, SQLComparisonOperator.NotEqual, Parent.PK);

			if (Parent.Factory.LoadTop1<RefContainerStock>(filter) != null)
			{
				Parent.R6_ContainerNumInfo.AddError(Res.GetString("880a8f86-9f9c-430a-a0e4-e8b7febdd738", "This container number is already in use."));
			}
		}

		protected override void CheckR6_RC()
		{
			base.CheckR6_RC();
			ListValidation.ErrorIfInvalidPK(Parent.R6_RCInfo, Parent.Lookups.Containers);
		}

		protected override void CheckR6_OwnerType()
		{
			base.CheckR6_OwnerType();
			MandatoryValidation.CheckEntered(Parent.R6_OwnerTypeInfo);
			ListValidation.ErrorIfInvalidCode(Parent.R6_OwnerTypeInfo, Parent.Lookups.OwnerTypes);
		}

		protected override void CheckR6_OH_Owner()
		{
			base.CheckR6_OH_Owner();
			ListValidation.ErrorIfInvalidPK(Parent.R6_OH_OwnerInfo, Parent.Lookups.Owners);
		}
	}
}


