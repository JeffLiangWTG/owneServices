using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Rating.Business.Testing
{
	[TestedType(typeof(SameChargeCodeDifferentProviderCollection))]
	public class SameChargeCodeDifferentProviderCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<SameChargeCodeDifferentProviderCollection>
	{
		public void TestUniqueConfigurations()
		{
			var bizoCollection = new SameChargeCodeDifferentProviderCollection();

			var sameChargeCode1 = new SameChargeCodeDifferentProvider();
			bizoCollection.Add(sameChargeCode1);
			sameChargeCode1.JobType = JobInvoicingConsumerTypes.Shipment.Code;
			sameChargeCode1.TransportMode = Core.Constants.TransportModes.Sea;

			var sameChargeCode2 = new SameChargeCodeDifferentProvider();
			bizoCollection.Add(sameChargeCode2);
			sameChargeCode2.JobType = JobInvoicingConsumerTypes.Shipment.Code;
			sameChargeCode2.TransportMode = Core.Constants.TransportModes.Sea;
			AssertHasError(sameChargeCode1.JobTypeInfo, "Entry with identical values already exists.");
			AssertHasError(sameChargeCode2.JobTypeInfo, "Entry with identical values already exists.");

			sameChargeCode2.IsEnabled = true;
			AssertHasError(sameChargeCode1.JobTypeInfo, "Entry with identical values already exists.");
			AssertHasError(sameChargeCode2.JobTypeInfo, "Entry with identical values already exists.");

			sameChargeCode2.TransportMode = Core.Constants.TransportModes.Air;
			AssertNoErrors(sameChargeCode2.JobTypeInfo);

			sameChargeCode2.TransportMode = Core.Constants.TransportModes.Sea;
			AssertHasError(sameChargeCode1.JobTypeInfo, "Entry with identical values already exists.");
			AssertHasError(sameChargeCode2.JobTypeInfo, "Entry with identical values already exists.");

			bizoCollection.Remove(sameChargeCode2);
			AssertNoErrors(sameChargeCode1.JobTypeInfo);
		}

		#region Implementation

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		protected override SameChargeCodeDifferentProviderCollection GetCollectionToTest()
		{
			return new SameChargeCodeDifferentProviderCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new SameChargeCodeDifferentProvider();
		}

		#endregion
	}
}
