using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Interfaces;
using Enterprise.Messaging.Business;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(CusUnderbondCollectionWithProvider))]
	sealed class CusUnderbondCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestCollectionRelationshipSet()
		{
			CusUnderbondCollection underbondCollection = new CusUnderbondCollectionWithProvider(Master);
			CusUnderbond underbond = underbondCollection.AddNew();
			AssertEquals("ParentID", Master.PK, underbond.C4_ParentID);
			AssertEquals("ParentID", "Z0", underbond.C4_ParentTableCode);
		}

		public void TestFindUnderbond()
		{
			CusUnderbondCollection underbondCollection = new CusUnderbondCollectionWithProvider(Master);
			CusUnderbond underbondA = underbondCollection.AddNew();
			CusUnderbond underbondB = underbondCollection.AddNew();
			CusUnderbond underbondC = underbondCollection.AddNew();

			underbondA.C4_OriginPremiseID = "1";
			underbondA.C4_DestinationPremiseID = "2";
			underbondB.C4_OriginPremiseID = "3";
			underbondB.C4_DestinationPremiseID = "4";
			underbondC.C4_OriginPremiseID = "2";
			underbondC.C4_DestinationPremiseID = "3";
			AssertEquals("FindUnderbond", null, underbondCollection.FindUnderbond("1", "3"));
			AssertEquals("FindUnderbond", underbondC, underbondCollection.FindUnderbond("2", "3"));
			AssertEquals("FindUnderbond", underbondA, underbondCollection.FindUnderbond("1", "2"));
		}

		public void TestAreAnyUnderbondsWaitingForAResponse()
		{
			CusUnderbondCollection underbondCollection = new CusUnderbondCollectionWithProvider(Master);
			CusUnderbond underbondA = underbondCollection.AddNew();
			CusUnderbond underbondB = underbondCollection.AddNew();
			CusUnderbond underbondC = underbondCollection.AddNew();
			AssertEquals("AreAnyUnderbondsWaitingForAResponse", false, underbondCollection.AreAnyUnderbondsWaitingForAResponse);
			EDIMessage messageA = underbondA.Messages.AddNew();
			messageA.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			EDIMessage messageB = underbondB.Messages.AddNew();
			messageB.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			AssertEquals("AreAnyUnderbondsWaitingForAResponse", true, underbondCollection.AreAnyUnderbondsWaitingForAResponse);
			messageA.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			AssertEquals("AreAnyUnderbondsWaitingForAResponse", true, underbondCollection.AreAnyUnderbondsWaitingForAResponse);
			messageB.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			AssertEquals("AreAnyUnderbondsWaitingForAResponse", false, underbondCollection.AreAnyUnderbondsWaitingForAResponse);
		}

		public void TestNonPersistentUnderbondParent()
		{
			DummyBizoWithUnderbondCollection parent = Factory.New<DummyBizoWithUnderbondCollection>();
			CusUnderbondCollection collection = new CusUnderbondCollectionWithProvider(new TestNonPersistentBusinessObjectWrapper(parent));
			CusUnderbond underbond = collection.AddNew();
			AssertEquals("Table Code should be Org Header", parent.TablePrefix, underbond.C4_ParentTableCode);
			AssertEquals("FK should be Org PK", parent.PK, underbond.C4_ParentID);
		}

		#region Implementation

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return UnderbondCollection;
		}

		DummyBizoWithUnderbondCollection fMaster;
		DummyBizoWithUnderbondCollection Master
		{
			get
			{
				if (fMaster == null)
				{
					fMaster = Factory.New<DummyBizoWithUnderbondCollection>();
				}
				return fMaster;
			}
		}

		CusUnderbondCollectionWithProvider fUnderbondCollection;
		CusUnderbondCollectionWithProvider UnderbondCollection
		{
			get
			{
				if (fUnderbondCollection == null)
				{
					fUnderbondCollection = new CusUnderbondCollectionWithProvider(Master);
				}
				return fUnderbondCollection;
			}
		}

		public class TestNonPersistentBusinessObjectWrapper : NonPersistentBusinessObject, ICusUnderbondDependentCollectionParent
		{
			public TestNonPersistentBusinessObjectWrapper(BusinessObject parent)
				: base(parent.Factory)
			{
				this.Parent = parent;
			}

			public readonly BusinessObject Parent;

			public override string TablePrefix
			{
				get { return Parent.TablePrefix; }
			}

			public override string TableName
			{
				get { return Parent.TableName; }
			}

			#region ICusUnderbondDependentCollectionParent Members

			public CusUnderbondCollection Underbonds
			{
				get
				{
					if (fUnderbonds == null)
					{
						fUnderbonds = new CusUnderbondCollectionWithProvider(this);
					}
					return fUnderbonds;
				}
			}
			CusUnderbondCollection fUnderbonds;

			public ZString UnderbondHumanReadableName
			{
				get
				{
					return new ZString();
				}
			}

			bool ICusUnderbondDependentCollectionParent.CanSendWithoutDelay
			{
				get { return true; }
			}

			#region IOutturnableLine Members

			ZString IOutturnableLine.CargoStatus
			{
				get { return "WTO"; }
			}

			bool IOutturnableLine.IsDeleted
			{
				get { return Parent.IsDeleted; }
			}

			ZInt IOutturnableLine.PackagesManifested
			{
				get { return 0; }
			}

			#endregion

			ZGuid ILinkable.LinkPK
			{
				get { return Parent.PK; }
			}

			public ZString Details
			{
				get
				{
					// TODO:  Add TestNonPersistentBusinessObjectWrapper.Details getter implementation
					return new ZString();
				}
			}

			public IOutturnableLine[] OutturnableLines
			{
				get
				{
					// TODO:  Add TestNonPersistentBusinessObjectWrapper.OutturnableLines getter implementation
					return null;
				}
			}

			public bool UsesTranshipmentPortOnUnderbond
			{
				get { return true; }
			}

			public ZString DefaultTranshipmentPort
			{
				get { return ZString.Empty; }
			}

			#endregion
		}

		#endregion

	}
}
