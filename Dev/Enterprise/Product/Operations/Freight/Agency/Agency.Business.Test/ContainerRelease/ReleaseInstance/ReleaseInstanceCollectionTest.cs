using System;
using CargoWise.EntityFramework;
using CargoWise.EventReference;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Agency.Business.Testing
{
	internal class ReleaseInstanceCollectionTest : BaseAgencyTest
	{
		public void TestAddIfNotExists()
		{
			OrgAddress yard = Factory.New<OrgHeader>().MainAddress;
			yard.Header.OH_FullName = "CY";
			ReleaseHeader header = new ReleaseHeader(Factory.New<AgencyBooking>(), false);
			AssertInstances("empty", header);
			header.Instances.AddIfNotExists(yard.PK, "Ref-1", Constants.EventReferenceReleaseTypes.Codes.Cancellation);
			header.Instances.AddIfNotExists(yard.PK, "Ref-2", Constants.EventReferenceReleaseTypes.Codes.Original);
			header.Instances.AddIfNotExists(yard.PK, "Ref-3", Constants.EventReferenceReleaseTypes.Codes.Reprint);
			header.Instances.AddIfNotExists(yard.PK, "Ref-4", Constants.EventReferenceReleaseTypes.Codes.Revised);
			AssertInstances("added", header, "Ref-1 CY CAN", "Ref-2 CY ORG", "Ref-3 CY REP", "Ref-4 CY RVS");
			header.Instances.AddIfNotExists(yard.PK, "Ref-1", Constants.EventReferenceReleaseTypes.Codes.Cancellation);
			header.Instances.AddIfNotExists(yard.PK, "Ref-2", Constants.EventReferenceReleaseTypes.Codes.Original);
			header.Instances.AddIfNotExists(yard.PK, "Ref-3", Constants.EventReferenceReleaseTypes.Codes.Reprint);
			header.Instances.AddIfNotExists(yard.PK, "Ref-4", Constants.EventReferenceReleaseTypes.Codes.Revised);
			AssertInstances("added same", header, "Ref-1 CY CAN", "Ref-2 CY ORG", "Ref-3 CY REP", "Ref-4 CY RVS");
			header.Instances.AddIfNotExists(yard.PK, "Ref-1", Constants.EventReferenceReleaseTypes.Codes.Original);
			header.Instances.AddIfNotExists(yard.PK, "Ref-2", Constants.EventReferenceReleaseTypes.Codes.Reprint);
			header.Instances.AddIfNotExists(yard.PK, "Ref-3", Constants.EventReferenceReleaseTypes.Codes.Revised);
			header.Instances.AddIfNotExists(yard.PK, "Ref-4", Constants.EventReferenceReleaseTypes.Codes.Cancellation);
			AssertInstances("added diferent", header, "Ref-1 CY RVS", "Ref-2 CY RVS", "Ref-3 CY RVS", "Ref-4 CY RVS");
		}

		#region Implementation
		void AssertInstances(string message, ReleaseHeader header, params string[] values)
		{
			AssertContainsExactElementsInAnyOrder(message, values, Array.ConvertAll(header.Instances.ToArray<ReleaseInstance>(), (i) => i.ReleaseNumber + " " + i.ContainerYard.Header.OH_FullName + " " + i.ReleaseType));
		}
		#endregion
	}
}
