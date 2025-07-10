using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Agency.Business.Testing
{
	internal class ReleaseInstanceTest : BaseAgencyTest
	{
		public void TestMaxLengths()
		{
			AssertEquals("ReleaseNumber", JobContainerSchema.JC_ReleaseNum.MaxLength, Instance.ReleaseNumberInfo.MaxLength);
		}

		public void TestDocumentSupporter()
		{
			DocumentSupporter supporter = Instance.DocumentSupporter;
			AssertEquals(typeof(ReleaseInstanceDocumentSupporter), supporter == null ? null : supporter.GetType());
		}

		#region Implementation
		ReleaseInstance Instance
		{
			get
			{
				return instance ?? (instance = Header.Instances.AddNew());
			}
		}

		ReleaseInstance instance;
		ReleaseHeader Header
		{
			get
			{
				return header ?? (header = new ReleaseHeader(Shipment, false));
			}
		}

		ReleaseHeader header;
		AgencyBooking Shipment
		{
			get
			{
				return shipment ?? (shipment = Factory.New<AgencyBooking>());
			}
		}

		AgencyBooking shipment;
		#endregion
	}
}
