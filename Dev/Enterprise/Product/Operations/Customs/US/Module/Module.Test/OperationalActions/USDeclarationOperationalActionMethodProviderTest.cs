using System;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using Enterprise.Services.OperationalActions.Support;
using NUnit.Framework;

namespace Enterprise.Customs.US.Module.OperationalActions.Testing
{
	[TestedType(typeof(USDeclarationOperationalActionMethodProvider))]
	sealed class USDeclarationOperationalActionMethodProviderTest : Services.OperationalActions.Support.Testing.OperationalActionMethodProviderTest
	{
		public void TestNewMethods()
		{
			AssertContainsExactElementsInAnyOrder("Expected Methods for Receive",
				new Type[]
				{
					typeof(SendEntrySummaryOperationalActionMethod),
					typeof(SendAESTIROperationalActionMethod),
					typeof(SendCargoManifestEntryStatusQueryActionMethod),
					typeof(SendEntrySummaryQueryOperationalActionMethod)
				},
				Array.ConvertAll(Provider.NewMethods(new Supporter<BusinessObject>()), m => m.GetType()));
		}

		#region Implementation

		protected override ActionMethodProviderID ID => ActionMethodProviderIDs.USJobDeclaration;

		class Supporter<T> : OperationalActionSupporter where T : BusinessObject
		{
			public override Type RootType
			{
				get { return typeof(T); }
			}

			public override BusinessContext BusinessContext => throw new NotImplementedException();
		}

		#endregion
	}
}
