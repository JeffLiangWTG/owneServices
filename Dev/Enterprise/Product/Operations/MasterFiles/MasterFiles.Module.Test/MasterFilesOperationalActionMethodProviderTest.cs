using System;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.Services.OperationalActions.Support.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(MasterFilesOperationalActionMethodProvider))]
	sealed class MasterFilesOperationalActionMethodProviderTest : OperationalActionMethodProviderTest
	{
		public void TestNewMethods()
		{
			AssertNull("Expected Methods", Provider.NewMethods(new Supporter<BusinessObject>()));

			// OrgSupplierPart
			AssertContainsExactElementsInAnyOrder("Expected Methods for OrgSupplierPart",
				new[]
				{
					typeof(MarkProductsAsBarcodedMethod),
					typeof(UnmarkProductsAsBarcodedMethod),
					typeof(AssignCartonGroupMethod),
					typeof(AssignWhsPutawayGroupMethod),
					typeof(UpdateExpiryNotificationPeriodMethod),
					typeof(UpdateDynamicPickFaceAreaMethod),
					typeof(AuditClassificationLinesMethod),
				},
				Array.ConvertAll(Provider.NewMethods(new Supporter<OrgSupplierPart>()), m => m.GetType()));

			GlbStaff.CurrentUser.GS_LoginName = "test_notsupport";
			AssertContainsExactElementsInAnyOrder("Expected Methods for OrgSupplierPart",
				new[]
				{
					typeof(MarkProductsAsBarcodedMethod),
					typeof(UnmarkProductsAsBarcodedMethod),
					typeof(AssignCartonGroupMethod),
					typeof(AssignWhsPutawayGroupMethod),
					typeof(UpdateExpiryNotificationPeriodMethod),
					typeof(UpdateDynamicPickFaceAreaMethod),
					typeof(AuditClassificationLinesMethod),
				},
				Array.ConvertAll(Provider.NewMethods(new Supporter<OrgSupplierPart>()), m => m.GetType()));
		}

		#region Implementation

		protected override ActionMethodProviderID ID => ActionMethodProviderIDs.MasterFiles;

		class Supporter<T> : OperationalActionSupporter where T : BusinessObject
		{
			public override BusinessContext BusinessContext => throw new Exception("The method or operation is not implemented.");

			public override Type RootType => typeof(T);
		}

		#endregion
	}
}
