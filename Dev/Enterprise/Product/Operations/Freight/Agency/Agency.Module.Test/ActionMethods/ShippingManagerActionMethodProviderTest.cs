using System;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using Enterprise.Freight.Agency.Business;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.Services.OperationalActions.Support.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.Module
{
	[TestedType(typeof(ShippingManagerActionMethodProvider))]
	internal class ShippingManagerActionMethodProviderTest : OperationalActionMethodProviderTest
	{
		public void TestNewMethods()
		{
			AssertNull("Expected Methods", Provider.NewMethods(new Supporter<BusinessObject>()));
			AssertContainsExactElementsInAnyOrder("Expected Methods for BillOfLading", new Type[] { typeof(EIDOSendActionMethod), typeof(EIDOWithdrawActionMethod), typeof(UpdateReturnByActionMethod), }, Array.ConvertAll(Provider.NewMethods(new Supporter<BillOfLading>()), (m) => m.GetType()));
			AssertContainsExactElementsInAnyOrder("Expected Methods for BillOfLadingBookingContainer", new Type[] { typeof(EIDOSendActionMethod), typeof(EIDOWithdrawActionMethod), typeof(BulkMovementsActionMethod), typeof(UpdateReturnByActionMethod), typeof(ReleaseOrderSendActionMethod), typeof(ReleaseOrderWithdrawActionMethod) }, Array.ConvertAll(Provider.NewMethods(new Supporter<BillOfLadingContainer>()), (m) => m.GetType()));
			AssertContainsExactElementsInAnyOrder("Expected Methods for RefContainerStock", new Type[] { typeof(BulkMovementsActionMethod), }, Array.ConvertAll(Provider.NewMethods(new Supporter<RefContainerStock>()), (m) => m.GetType()));
			AssertContainsExactElementsInAnyOrder("Expected Methods for ContainerMovement", new Type[] { typeof(UpdateDetentionDaysActionMethod), }, Array.ConvertAll(Provider.NewMethods(new Supporter<ContainerMovement>()), (m) => m.GetType()));
			AssertContainsExactElementsInAnyOrder("Expected Methods for AgencyBooking", new Type[] { typeof(RollBookingActionMethod), }, Array.ConvertAll(Provider.NewMethods(new Supporter<AgencyBooking>()), (m) => m.GetType()));
		}

		#region Implementation
		class Supporter<T> : OperationalActionSupporter where T : BusinessObject
		{
			public override BusinessContext BusinessContext
			{
				get
				{
					throw new NotImplementedException("The method or operation is not implemented.");
				}
			}

			public override Type RootType
			{
				get
				{
					return typeof(T);
				}
			}
		}

		protected override ActionMethodProviderID ID
		{
			get
			{
				return ActionMethodProviderIDs.Shipping;
			}
		}
		#endregion
	}
}
