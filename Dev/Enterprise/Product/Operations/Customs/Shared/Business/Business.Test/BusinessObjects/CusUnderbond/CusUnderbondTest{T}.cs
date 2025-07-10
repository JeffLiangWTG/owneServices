using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.Interfaces;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Moq.Protected;

namespace Enterprise.Customs.Business.Testing
{
	public abstract class CusUnderbondTest<T> : EnterpriseBusinessObjectTestCase
				where T : CusUnderbond
	{
		public virtual void TestCanDoUBM()
		{
			Assert(Underbond.CanDoUBM);
		}

		public void TestC4_Calculated_ResponsiblePartyID()
		{
			Underbond.C4_DestinationPremiseID = "B123B";
			GlbCompany.CurrentCompany.OrgProxy.PrimaryRegistrationNumber.Number = "67094168242";

			AssertEquals("67094168242", Underbond.C4_Calculated_ResponsiblePartyID);

			CodeDescriptionPairList codeDescriptionPairList = new CodeDescriptionPairList();
			codeDescriptionPairList.AddPair("B123B", "21003980130");
			FreightDataRegistry.Instance.OuturnResponsiblePartyIDOverride.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, codeDescriptionPairList);

			AssertEquals("21003980130", Underbond.C4_Calculated_ResponsiblePartyID);
		}

		public virtual void TestTranshipmentPortVisible()
		{
			var underbond = (CusUnderbond)GetNewBusinessObject();
			AssertEquals(true, underbond.TranshipmentPortVisible);
		}

		public void TestMessages()
		{
			AssertNotNull("Messages", ((CusUnderbond)GetNewBusinessObject()).Messages);
		}

		public void TestSetDefaultValuesFromParent()
		{
			CusUnderbondTestHelper underbond = Factory.New<CusUnderbondTestHelper>();
			AssertEquals("Should Default to false", false, underbond.HasSetDefaultValuesFromParentBeenHit);
			underbond.SetDefaultValuesFromParent();
			AssertEquals("Should now be true", true, underbond.HasSetDefaultValuesFromParentBeenHit);
		}

		class CusUnderbondTestHelper : CusUnderbond
		{
			public CusUnderbondTestHelper(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public bool HasSetDefaultValuesFromParentBeenHit;
			public override void SetDefaultValuesFromParent()
			{
				base.SetDefaultValuesFromParent();
				C4_ApplicationCode = "T2T";
				HasSetDefaultValuesFromParentBeenHit = true;
			}
		}

		public void TestDischargePremiseGetsEnteredFromOrigin()
		{
			var underbond = (CusUnderbond)GetNewBusinessObject();
			underbond.C4_IsMoveFromDischarge = false;
			underbond.C4_DischargePremiseID = "12345";
			AssertEquals("12345", underbond.C4_DischargePremiseID);
			underbond.C4_OA_DischargeAddress = GlbCompany.CurrentCompany.Branches[0].OrgProxy.Addresses[0].PK;
			AssertEquals(GlbCompany.CurrentCompany.Branches[0].OrgProxy.Addresses[0].PK, underbond.C4_OA_DischargeAddress);
			underbond.C4_IsMoveFromDischarge = true;
			AssertEquals(ZGuid.Empty, underbond.C4_OA_DischargeAddress);
			AssertEquals("", underbond.C4_DischargePremiseID);
		}

		public void TestLinkedObject()
		{
			var underbond = Factory.New<CusUnderbondThatLinksToDummyBizo>();
			ICusUnderbondDependentCollectionParent dummy = Factory.New<DummyBizoWithUnderbondCollection>();
			underbond.LinkedObject = dummy;
			AssertEquals("ParentID", dummy.LinkPK, underbond.C4_ParentID);
			AssertEquals("TableCode", "Z0", underbond.C4_ParentTableCode);
			underbond.LinkedObject = null;
			AssertEquals("ParentID", ZGuid.Empty, underbond.C4_ParentID);
			AssertEquals("TableCode", ZString.Empty, underbond.C4_ParentTableCode);
		}

		public void TestGetLinkedObject()
		{
			var underbond = Factory.New<CusUnderbondThatLinksToDummyBizo>();
			ICusUnderbondDependentCollectionParent dummy = Factory.New<DummyBizoWithUnderbondCollection>();
			underbond.LinkedObject = dummy;
			AssertEquals("LinkedObject", dummy, underbond.LinkedObject);
		}

		public void TestDischargeIDShowsLocalCustomsCode()
		{
			Underbond.C4_OA_DischargeAddress = EstablishmentAddress.PK;
			AssertEquals("DischargeID", "12345", Underbond.C4_DischargePremiseID);
			Underbond.C4_OA_DischargeAddress = ZGuid.Empty;
			AssertEquals("DischargeID", ZString.Empty, Underbond.C4_DischargePremiseID);
		}

		public void TestDischargeIDIsReadonlyWhenDischargeAddressSet()
		{
			Underbond.C4_OA_DischargeAddress = EstablishmentAddress.PK;
			AssertEquals("Readonly", true, Underbond.C4_DischargePremiseIDInfo.ReadOnly);
			Underbond.C4_OA_DischargeAddress = ZGuid.Empty;
			AssertEquals("Readonly", false, Underbond.C4_DischargePremiseIDInfo.ReadOnly);
		}

		public void TestOriginIDShowsLocalCustomsCode()
		{
			Underbond.C4_OA_OriginAddress = EstablishmentAddress.PK;
			AssertEquals("OriginID", "12345", Underbond.C4_OriginPremiseID);
			Underbond.C4_OA_OriginAddress = ZGuid.Empty;
			AssertEquals("OriginID", ZString.Empty, Underbond.C4_OriginPremiseID);
		}

		public void TestOriginIDIsReadonlyWhenOriginAddressSet()
		{
			Underbond.C4_OA_OriginAddress = EstablishmentAddress.PK;
			AssertEquals("Readonly", true, Underbond.C4_OriginPremiseIDInfo.ReadOnly);
			Underbond.C4_OA_OriginAddress = ZGuid.Empty;
			AssertEquals("Readonly", false, Underbond.C4_OriginPremiseIDInfo.ReadOnly);
		}

		public void TestDestinationIDShowsLocalCustomsCode()
		{
			Underbond.C4_OA_DestinationAddress = EstablishmentAddress.PK;
			AssertEquals("DestinationID", "12345", Underbond.C4_DestinationPremiseID);
			Underbond.C4_OA_DestinationAddress = ZGuid.Empty;
			AssertEquals("DestinationID", ZString.Empty, Underbond.C4_DestinationPremiseID);
		}

		public void TestDestinationIDIsReadonlyWhenDestinationAddressSet()
		{
			Underbond.C4_OA_DestinationAddress = EstablishmentAddress.PK;
			AssertEquals("Readonly", true, Underbond.C4_DestinationPremiseIDInfo.ReadOnly);
			Underbond.C4_OA_DestinationAddress = ZGuid.Empty;
			AssertEquals("Readonly", false, Underbond.C4_DestinationPremiseIDInfo.ReadOnly);
		}

		public void TestSendersReferenceReadOnly()
		{
			AssertEquals("Readonly", true, Underbond.C4_SendersMessageReferenceInfo.ReadOnly);
		}

		public void TestLoadIsTypeDecided()
		{
			BusinessObject underbond = Factory.New(ExpectedBusinessObjectType);
			Factory.Save();
			Assert("Type", ExpectedBusinessObjectType.IsAssignableFrom(new BusinessObjectFactory().Load(typeof(CusUnderbond), underbond.PK).GetType()));
		}

		public void TestPopulateC4_SendersMessageReferenceIfNeeded()
		{
			CargoWise.Data.Db.Connection.BeginTransaction();
			try
			{
				Underbond.PopulateC4_SendersMessageReferenceIfNeeded();
				Assert("!Underbond.C4_SendersMessageReference.IsEmpty", !Underbond.C4_SendersMessageReference.IsEmpty);
			}
			finally
			{
				CargoWise.Data.Db.Connection.RollbackTransaction();
			}
		}

		[NUnit.Framework.ExpectException(typeof(Exception))]
		public void TestC4_SendersMessageReferenceNumberNotSetIfSaveFails()
		{
			var underbond = Factory.New<CusUnderbondThatThrowsAnExceptionWhistSaving>();
			try
			{
				Factory.Save();
			}
			finally
			{
				AssertEquals("C4_SendersMessageReference", underbond.C4_SendersMessageReference, ZString.Empty);
			}
		}

		public void TestLoadFromSendersReference()
		{
			Underbond.C4_SendersMessageReference = "12345";
			AssertEquals("Load", Underbond, CusUnderbond.LoadFromSendersReference(Factory, "12345"));
		}

		public void TestSupportsClone()
		{
			var underbond = (CusUnderbond)Factory.New(ExpectedBusinessObjectType);
			Assert(underbond.SupportsClone());
		}

		public void TestClone()
		{
			CusUnderbond underbond = Factory.New<CusUnderBondWithParentLoader>();
			underbond.LinkedObject = Factory.New<DummyBizoWithUnderbondCollection>();
			underbond.C4_ModeOfMovement = "ROA";
			underbond.C4_OA_DischargeAddress = EstablishmentAddress.PK;
			underbond.C4_OA_OriginAddress = EstablishmentAddress.PK;
			underbond.C4_OA_DestinationAddress = EstablishmentAddress.PK;
			underbond.C4_SendersMessageReference = "Squeaker";
			underbond.C4_RL_NKDischargePort = "AUSYD";
			underbond.C4_RL_NKLoadPort = "SGSIN";
			underbond.C4_PiecesManifested = 6;
			underbond.C4_MovementReason = "DCL";
			underbond.C4_FlightNo = "QF134";
			CusUnderbond underbond2 = (CusUnderbond)underbond.Clone();
			AssertEquals("ROA", underbond2.C4_ModeOfMovement);
			AssertEquals(EstablishmentAddress.PK, underbond2.C4_OA_DischargeAddress);
			AssertEquals(EstablishmentAddress.PK, underbond2.C4_OA_OriginAddress);
			AssertEquals(EstablishmentAddress.PK, underbond2.C4_OA_DestinationAddress);
			Assert(underbond2.C4_SendersMessageReference.IsEmpty);
			AssertEquals("AUSYD", underbond2.C4_RL_NKDischargePort);
			AssertEquals("SGSIN", underbond2.C4_RL_NKLoadPort);
			AssertEquals("6", underbond2.C4_PiecesManifested.ToString());
			AssertEquals("DCL", underbond2.C4_MovementReason);
			AssertEquals("QF134", underbond2.C4_FlightNo);
		}

		public void TestDetails()
		{
			var underbond = Factory.New<CusUnderbondThatLinksToDummyBizo>();
			underbond.LinkedObject = (ICusUnderbondDependentCollectionParent)Factory.New(typeof(DummyBizoWithUnderbondCollection));
			underbond.C4_ModeOfMovement = "ROA";
			underbond.C4_UnderbondBySeaVessel = "ADMIRALENGRACHT";
			underbond.C4_UnderbondBySeaVoyage = "123";
			underbond.C4_OA_DischargeAddress = EstablishmentAddress.PK;
			underbond.C4_OA_OriginAddress = EstablishmentAddress.PK;
			underbond.C4_OA_DestinationAddress = EstablishmentAddress.PK;

			ZString expectedDetails = "Dummy Underbond Biz Obj Details\r\n\r\n" +
				"Discharge Establishment: Name - Address - 12345\r\n" +
				"Origin Establishment: Name - Address - 12345\r\n" +
				"Destination Establishment: Name - Address - 12345\r\n" +
				"Mode of Movement: ROA\r\n" +
				"Underbond by Sea Vessel: ADMIRALENGRACHT\r\n" +
				"Underbond by Sea Voyage: 123\r\n";

			AssertEquals("Details", expectedDetails, underbond.Details);
		}

		public void TestDetailsNoParent()
		{
			var underbond = Factory.New<CusUnderbondThatLinksToDummyBizo>();
			//Underbond.LinkedObject = (ICusUnderbondDependentCollectionParent)Factory.New(typeof(DummyBizoWithUnderbondCollection));
			underbond.C4_ModeOfMovement = "ROA";
			underbond.C4_UnderbondBySeaVessel = "ADMIRALENGRACHT";
			underbond.C4_UnderbondBySeaVoyage = "123";
			underbond.C4_OA_DischargeAddress = EstablishmentAddress.PK;
			underbond.C4_OA_OriginAddress = EstablishmentAddress.PK;
			underbond.C4_OA_DestinationAddress = EstablishmentAddress.PK;

			ZString expectedDetails = "Discharge Establishment: Name - Address - 12345\r\n" +
				"Origin Establishment: Name - Address - 12345\r\n" +
				"Destination Establishment: Name - Address - 12345\r\n" +
				"Mode of Movement: ROA\r\n" +
				"Underbond by Sea Vessel: ADMIRALENGRACHT\r\n" +
				"Underbond by Sea Voyage: 123\r\n";

			AssertEquals("Details", expectedDetails, underbond.Details);
		}

		public void TestDetailsNoExtras()
		{
			var underbond = Factory.New<CusUnderbondThatLinksToDummyBizo>();
			underbond.LinkedObject = (ICusUnderbondDependentCollectionParent)Factory.New(typeof(DummyBizoWithUnderbondCollection));

			ZString expectedDetails = "Dummy Underbond Biz Obj Details";

			AssertEquals("Details", expectedDetails, underbond.Details);
		}

		public void TestOutturns()
		{
			AssertNotNull("Outturns", Underbond.Outturns);
		}

		public void TestGetParentStringRepresentation()
		{
			var underbond = Factory.New<CusUnderbondThatLinksToDummyBizo>();
			ICusUnderbondDependentCollectionParent dummy1 = Factory.New<DummyBizoWithUnderbondCollection>();

			underbond.LinkedObject = dummy1;
			AssertEquals("LinkedObjectStringRepresentation", dummy1.UnderbondHumanReadableName, underbond.LinkedObjectStringRepresentation);
			AssertEquals("C4_ParentID", dummy1.Identifier, underbond.C4_ParentID);

			TestConnection.ExecuteNonQuery("""
				IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'Constraint_C4_ParentTableCode_NoCheck') AND type = 'C')
				ALTER TABLE dbo.CusUnderbond DROP CONSTRAINT Constraint_C4_ParentTableCode_NoCheck
				""");
			Factory.Save();

			var underbondInNewFactory = NewFactory().Load<CusUnderbondThatLinksToDummyBizo>(underbond.PK);
			AssertEquals("LinkedObjectStringRepresentation", dummy1.UnderbondHumanReadableName, underbondInNewFactory.LinkedObjectStringRepresentation);

			underbond.LinkedObject = null;
			AssertEquals("LinkedObjectStringRepresentation", ZString.Empty, underbond.LinkedObjectStringRepresentation);

			ICusUnderbondDependentCollectionParent dummy2 = Factory.New<DummyBizoWithUnderbondCollection>();
			underbond.C4_ParentID = dummy2.Identifier;
			underbond.C4_ParentTableCode = dummy2.LinkTablePrefix;

			AssertEquals("LinkedObjectStringRepresentation", dummy2.UnderbondHumanReadableName, underbond.LinkedObjectStringRepresentation);
		}

		public virtual void TestVoyageAndVesselDetailsVisible()
		{
			AssertEquals("VoyageAndVesselDetailsVisible", true, Underbond.VoyageAndVesselDetailsVisible);
		}

		public void TestUnderbondStatus()
		{
			AssertNotNull(Underbond.UnderbondStatus);
		}

		public void TestStatusNeedsRecalculation()
		{
			AssertEquals("StatusNeedsRecalculation", false, Underbond.StatusNeedsRecalculation);
			Underbond.Messages.AddNew().EM_MessageText = "sd";
			AssertEquals("StatusNeedsRecalculation", true, Underbond.StatusNeedsRecalculation);
		}

		public virtual void TestDefaultUnderbondStatus()
		{
			AssertEquals("DefaultUnderbondStatus", ZString.Empty, Underbond.DefaultUnderbondStatus);
		}

		public void TestOutturnStatus()
		{
			AssertNotNull("OutturnStatus", Underbond.OutturnStatus);
		}

		public virtual void TestDefaultOutturnStatus()
		{
			AssertEquals("DefaultOutturnStatus", ZString.Empty, Underbond.DefaultOutturnStatus);
		}

		public void TestOutturnsAreRegisteredEditable()
		{
			AssertEquals("RegisteredEditable", true, Underbond.IsRegisteredEditableChildObject(Underbond.Outturns));
		}

		public void TestCanDoOutturn()
		{
			AssertEquals("CanDoOutturn", false, Underbond.CanDoOutturn);
		}

		public void TestGetCanDoOutturn()
		{
			CusUnderbondThatOverridesGetCanDoOutturn underbond = Factory.New<CusUnderbondThatOverridesGetCanDoOutturn>();
			underbond.CanDoOutturnResult = true;
			AssertEquals("GetCanDoOutturn", true, underbond.CanDoOutturnResult);
			underbond.CanDoOutturnResult = false;
			AssertEquals("GetCanDoOutturn", false, underbond.CanDoOutturnResult);
		}

		public void TestFireCanDoOutturnChangedEvent()
		{
			Underbond.FireCanDoOutturnChangedEvent();
			AssertEquals("CanDoOutturnChangedCount", 0, canDoOutturnChangedCount);
			Underbond.CanDoOutturnChanged += new EventHandler(Underbond_CanDoOutturnChanged);
			Underbond.FireCanDoOutturnChangedEvent();
			AssertEquals("CanDoOutturnChangedCount", 1, canDoOutturnChangedCount);
		}

		public void TestOnSettingC4_OA_OriginAddress_ValidateC4_OriginPremiseIDIsCalled()
		{
			OrgAddress orgAddress = Factory.New<OrgAddress>();

			var cusUnderBondMock = Factory.NewMoq<CusUnderbondForTesting>();
			CusUnderbond underBond = cusUnderBondMock.Object;

			FakeCusUnderbondValidation underBondValidation = new FakeCusUnderbondValidation(Underbond);
			cusUnderBondMock.Protected().Setup<CusUnderbondValidation>("GetNewValidation").Returns(underBondValidation);
			AssertEquals(false, ((FakeCusUnderbondValidation)underBond.Validation).ValidateC4_OriginPremiseIDIsCalled);
			underBond.C4_OA_OriginAddress = orgAddress.PK;
			AssertEquals(true, ((FakeCusUnderbondValidation)underBond.Validation).ValidateC4_OriginPremiseIDIsCalled);
		}

		public void TestOnSettingC4_OA_DestinationAddress_ValidateC4_DestinationPremiseIDIsCalled()
		{
			OrgAddress orgAddress = Factory.New<OrgAddress>();

			var cusUnderBondMock = Factory.NewMoq<CusUnderbondForTesting>();
			CusUnderbond underBond = cusUnderBondMock.Object;

			FakeCusUnderbondValidation underBondValidation = new FakeCusUnderbondValidation(Underbond);
			cusUnderBondMock.Protected().Setup<CusUnderbondValidation>("GetNewValidation").Returns(underBondValidation);
			AssertEquals(false, ((FakeCusUnderbondValidation)underBond.Validation).ValidateC4_DestinationPremiseIDIsCalled);
			underBond.C4_OA_DestinationAddress = orgAddress.PK;
			AssertEquals(true, ((FakeCusUnderbondValidation)underBond.Validation).ValidateC4_DestinationPremiseIDIsCalled);
		}

		#region TestIsAutoLogged

		public void TestIsAutoLogged()
		{
			var bizO = (CusUnderbond)GetNewBusinessObject();
			Assert("IsAutoLogged", bizO.IsAutoAdminBusinessObjectLoggerEnabled);
		}

		#endregion

		public void TestOutturnsEventsWillBeDisplayed()
		{
			var underbond = (CusUnderbond)GetNewBusinessObject();
			underbond.Outturns.AddNew();
			underbond.Outturns.AddNew();
			AssertEquals("Outturn lines added to the underbond", 2, underbond.Outturns.Count);
			AssertGreaterThanOrEqualTo("Underbond business objects with related events", underbond.BusinessObjectsWithRelatedEvents.Length, underbond.Outturns.Count);
			foreach (var outturn in underbond.Outturns)
			{
				AssertCollectionContains(outturn, underbond.BusinessObjectsWithRelatedEvents);
			}
		}

		#region Implementation

		void Underbond_CanDoOutturnChanged(object sender, EventArgs e)
		{
			canDoOutturnChangedCount++;
		}
		int canDoOutturnChangedCount;

		protected override BusinessObject GetNewBusinessObject() => Factory.New<T>();

		CusUnderbond fUnderbond;
		protected CusUnderbond Underbond
		{
			get
			{
				if (fUnderbond == null)
				{
					fUnderbond = (CusUnderbond)GetNewBusinessObject();
					fUnderbond.C4_DestinationPremiseID = "";
				}
				return fUnderbond;
			}
		}

		OrgAddress fEstablishmentAddress;
		protected OrgAddress EstablishmentAddress
		{
			get
			{
				if (fEstablishmentAddress == null)
				{
					OrgHeader header = OrgHeader.New(Factory);
					header.OH_FullName = "Name";
					header.Addresses[0].LocalControlledPremisesID = "12345";
					fEstablishmentAddress = header.Addresses[0];
					fEstablishmentAddress.OA_Code = "Address";
				}
				return fEstablishmentAddress;
			}
		}

		#region TestHelpers

		public class CusUnderbondForTesting : CusUnderbond
		{
			public CusUnderbondForTesting(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}
		}

		public class CusUnderbondThatThrowsAnExceptionWhistSaving : CusUnderbond
		{
			public CusUnderbondThatThrowsAnExceptionWhistSaving(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			public override void OnSaving()
			{
				base.OnSaving();
				throw new Exception("Fake exception to see what happens if save doesn't work");
			}
		}

		//			public class CusUnderbondDependentCollectionParentDummyBusinessObject : DummyBusinessObject, ICusUnderbondDependentCollectionParent
		//			{
		//				public CusUnderbondDependentCollectionParentDummyBusinessObject(BusinessObjectFactory Factory, DataRow Row) : base(Factory, Row)
		//				{
		//				}
		//
		//				public ZString UnderbondHumanReadableName
		//				{
		//					get { return "Dummy"; }
		//				}
		//
		//				protected CusUnderbondCollection fUnderbonds;
		//				public CusUnderbondCollection Underbonds
		//				{
		//					get
		//					{
		//						if (fUnderbonds == null)
		//						{
		//							fUnderbonds = new CusUnderbondCollection(this);
		//							fUnderbonds.Load();
		//							RegisterEditableChildObject(fUnderbonds);
		//						}
		//						return fUnderbonds;
		//					}
		//				}
		//			}

		#endregion

		#region FakeCusUnderbondValidation
		class FakeCusUnderbondValidation : CusUnderbondValidation
		{
			public FakeCusUnderbondValidation(CusUnderbond parent) : base(parent)
			{
			}

			protected override void CheckC4_OA_OriginAddress()
			{
				ValidateC4_OriginPremiseIDIsCalled = true;
			}

			protected override void CheckC4_OA_DestinationAddress()
			{
				ValidateC4_DestinationPremiseIDIsCalled = true;
			}

			public bool ValidateC4_OriginPremiseIDIsCalled;
			public bool ValidateC4_DestinationPremiseIDIsCalled;
		}
		#endregion

		#endregion
	}
}
