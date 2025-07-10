using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.OnlineSailingSchedules.PortCall;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Moq;
using NUnit.Framework;
using PortCallServiceModel = Enterprise.Freight.OnlineSailingSchedules.PortCall.PortCall;

namespace Enterprise.Freight.OnlineSailingSchedules.Testing
{
	[TestedType(typeof(PortCallResponseCollection))]
	public class PortCallResponseCollectionTest : NonPersistentBusinessObjectCollectionTestCase<PortCallResponseCollection>
	{
		public void TestLoad()
		{
			var request = new PortCallRequest(Factory);
			request.RequestType = PortCallRequestType.Load;
			request.Port = "AUSYD";

			var searchParams = "Port.Unloco:AUSYD";

			var portCallItem1 = new PortCallItem
			{
				Vessel = new Vessel
				{
					CallSign = "111"
				}
			};

			var portCallItem2 = new PortCallItem
			{
				Vessel = new Vessel
				{
					CallSign = "222"
				}
			};

			var portCall = new PortCallServiceModel
			{
				Items = new[] { portCallItem1, portCallItem2 }
			};

			var portCallResponses = new PortCallResponseCollectionForTest(Factory);

			portCallResponses.PotCallProvider.Setup(x => x.GetPortCall(searchParams, It.IsAny<PortCallServiceRequestManager>()))
				.Returns(portCall);

			portCallResponses.Load(request, new NotificationBuffer());

			AssertEquals(2, portCallResponses.Count);
			AssertEquals("111", portCallResponses[0].CallSign);
			AssertEquals("222", portCallResponses[1].CallSign);
		}

		public void TestNotificationHasErrorWhenCarrierCodeIsNot4CharacterLong()
		{
			var request = new PortCallRequest(Factory);
			request.RequestType = PortCallRequestType.Load;

			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			request.CarrierPK = orgHeader.PK;

			var orgCusCode = Factory.NewWithValidTestData<OrgCusCode>();
			orgCusCode.OK_CodeType = OrgCusCode.CodeTypes.CarrierCode;
			orgCusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.UnitedStates;
			orgCusCode.OK_CustomsRegNo = "ABC";
			orgCusCode.OK_OH = orgHeader.PK;

			orgHeader.CustomsCodes.Add(orgCusCode);

			var buffer = new NotificationBuffer();
			var portCallResponses = new PortCallResponseCollectionForTest(Factory);
			portCallResponses.Load(request, buffer);

			AssertEquals(1, buffer.Events.Length);
			AssertEquals("SCAC must be 4 character length.", buffer.Events[0].Message);

			orgCusCode.OK_CustomsRegNo = "ABCD";
			buffer.Clear();
			portCallResponses.Load(request, buffer);

			AssertEquals(false, buffer.HasErrors);
			AssertEquals(0, buffer.Events.Length);
		}

		#region Implementation

		protected override PortCallResponseCollection GetCollectionToTest()
		{
			return new PortCallResponseCollection(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new PortCallResponse();
		}

		#endregion
	}

	public class PortCallResponseCollectionForTest : PortCallResponseCollection
	{
		public PortCallResponseCollectionForTest(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override IPortCallProvider GetPortCallProvider()
		{
			return PotCallProvider.Object;
		}

		public Mock<IPortCallProvider> PotCallProvider => potCallProvider ?? (potCallProvider = new Mock<IPortCallProvider>());
		Mock<IPortCallProvider> potCallProvider;
	}
}
