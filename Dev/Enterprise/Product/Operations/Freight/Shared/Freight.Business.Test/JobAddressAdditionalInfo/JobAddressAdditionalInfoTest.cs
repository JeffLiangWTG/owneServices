using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Business.Test
{
	[TestedType(typeof(JobAddressAdditionalInfo))]
	public class JobAddressAdditionalInfoTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var bizo = (JobAddressAdditionalInfo)base.GetNewBusinessObjectForDeleteTest(factory);
			bizo.JAI_AddressType = "CEG";
			bizo.JAI_TransportMode = "IWT";
			bizo.JAI_ParentTableCode = "JS";
			return bizo;
		}

		public void TestDocAddressTypeProperty()
		{
			var testCases = new List<(string JAI_AddressType, DocAddressType ExpectedDocAddressType)>
			{
				("CRG", DocAddressType.ConsignorPickupDeliveryAddress),
				("DCF", DocAddressType.DepartureCFSAddress),
				("ACF", DocAddressType.ArrivalCFSAddress),
				("CEG", DocAddressType.ConsigneePickupDeliveryAddress),
				("DCY", DocAddressType.DepartureCYDAddress),
				("ACY", DocAddressType.ArrivalCYDAddress),
			};

			var bizo = (JobAddressAdditionalInfo)base.GetNewBusinessObject();
			foreach (var testCase in testCases)
			{
				bizo.JAI_AddressType = testCase.JAI_AddressType;
				var actualDocAddressType = bizo.DocAddressType;

				AssertEquals($"JAI_AddressType '{testCase.JAI_AddressType}' should map to DocAddressType '{testCase.ExpectedDocAddressType}'",
					testCase.ExpectedDocAddressType, actualDocAddressType);
			}
		}
	}
}
