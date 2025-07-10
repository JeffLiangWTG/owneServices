using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Common.Business
{
	public class CommonCartageOrgCollection : ActiveBusinessObjectCollection<CommonCartageOrg>
	{
		public CommonCartageOrgCollection(CommonCartageType cartageType)
			: base(cartageType.Factory, cartageType, null, LocalCartageJobOrgSchema.E5_E3)
		{
			CartageType = cartageType;
		}

#if DEBUG
		[CargoWise.EntityFramework.Testing.SuppressCollectionStateTest]
#endif
		readonly CommonCartageType CartageType;

		public CommonCartageOrgCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		#region Override

		protected override bool AllowNew
		{
			get { return CartageType != null && !CartageType.E3_IsSystem; }
		}

		#endregion

		#region New Properties

		public CommonCartageOrg OrgWithBillToParty
		{
			get
			{
				foreach (CommonCartageOrg orgType in this)
				{
					if (orgType.E5_IsBillToParty)
					{
						return orgType;
					}
				}
				return null;
			}
		}

		#endregion

	}
}
