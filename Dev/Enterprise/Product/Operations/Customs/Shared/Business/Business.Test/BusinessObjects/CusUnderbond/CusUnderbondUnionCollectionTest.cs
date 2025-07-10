using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business.Interfaces;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(CusUnderbondUnionCollectionWithProvider))]
	sealed class CusUnderbondUnionCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestLoadAddsElementsToUnionCollection()
		{
			DependentCollectionParents[0].Underbonds.AddNew();
			DependentCollectionParents[0].Underbonds.AddNew();
			Underbonds.Load();
			AssertEquals("Underbonds.Count", 2, Underbonds.Count);
		}

		public void TestAddNewUnderbondToOneOfCollectionWhenSenderReturnsNull()
		{
			TestHelperCusUnderbondMessageSender sender = new TestHelperCusUnderbondMessageSender();
			sender.ReturnNoProvider = true;
			Underbonds.AddNewUnderbond(sender);
			AssertEquals("Count", 0, Underbonds.Count);
		}

		public void TestAddNewUnderbondToOneOfCollectionWhenSenderReturnsSomething()
		{
			TestHelperCusUnderbondMessageSender sender = new TestHelperCusUnderbondMessageSender();
			Underbonds.AddNewUnderbond(sender);
			AssertEquals("Count", 1, Underbonds.Count);
		}

		public void TestModeOfMovementChanged()
		{
			TestHelperCusUnderbondMessageSender sender = new TestHelperCusUnderbondMessageSender();
			CusUnderbond underbond = Underbonds.AddNewUnderbond(sender);
			Underbonds.ModeOfMovementChanged += new EventHandler(Underbonds_ModeOfMovementChanged);
			AssertEquals("Underbonds_ModeOfMovementChanged", false, underbonds_ModeOfMovementChangedFired);
			underbond.C4_ModeOfMovement = "XXX";
			AssertEquals("Underbonds_ModeOfMovementChanged", true, underbonds_ModeOfMovementChangedFired);
		}

		public void TestCanSendOutturnChanged()
		{
			TestHelperCusUnderbondMessageSender sender = new TestHelperCusUnderbondMessageSender();
			CusUnderbond underbond = Underbonds.AddNewUnderbond(sender);
			Underbonds.CanSendOutturnChanged += new EventHandler(Underbonds_CanSendOutturnChanged);
			AssertEquals("Underbonds_ModeOfMovementChanged", false, underbonds_CanSendOutturnChangedFired);
			underbond.FireCanDoOutturnChangedEvent();
			AssertEquals("Underbonds_ModeOfMovementChanged", true, underbonds_CanSendOutturnChangedFired);
		}

		public void TestMovementReasonChanged()
		{
			TestHelperCusUnderbondMessageSender sender = new TestHelperCusUnderbondMessageSender();
			CusUnderbond underbond = Underbonds.AddNewUnderbond(sender);
			Underbonds.MovementReasonChanged += new EventHandler(Underbonds_MovementReasonChanged);
			AssertEquals("Underbonds_MovementReasonChanged", false, underbonds_MovementReasonChangedFired);
			underbond.C4_MovementReason = "XXX";
			AssertEquals("Underbonds_MovementReasonChanged", true, underbonds_MovementReasonChangedFired);
		}

		#region Implementation

		CusUnderbondUnionCollectionWithProvider underbonds;
		CusUnderbondUnionCollectionWithProvider Underbonds
		{
			get
			{
				if (underbonds == null)
				{
					underbonds = new CusUnderbondUnionCollectionWithProvider(Parent);
				}
				return underbonds;
			}
		}

		ICusUnderbondUnionCollectionParent parent;
		ICusUnderbondUnionCollectionParent Parent
		{
			get
			{
				if (parent == null)
				{
					parent = (ICusUnderbondUnionCollectionParent)Factory.New(typeof(DummyCusUnderbondUnionCollectionParent));
					((DummyCusUnderbondUnionCollectionParent)parent).AllPossibleCollectionProviders = DependentCollectionParents;
				}
				return parent;
			}
		}

		ICusUnderbondDependentCollectionParent[] dependentCollectionParents;
		ICusUnderbondDependentCollectionParent[] DependentCollectionParents
		{
			get
			{
				if (dependentCollectionParents == null)
				{
					dependentCollectionParents = new ICusUnderbondDependentCollectionParent[] { (ICusUnderbondDependentCollectionParent)Factory.New(typeof(DummyBizoWithUnderbondCollection)) };
				}
				return dependentCollectionParents;
			}
		}

		public class TestHelperCusUnderbondMessageSender : ICusUnderbondMessageSender
		{
			public bool ReturnNoProvider;

			#region ICusUnderbondMessageSender Members

			public ICusUnderbondDependentCollectionParent GetProviderToAddUnderbondTo(ICusUnderbondDependentCollectionParent[] allPossibleProviders)
			{
				if (ReturnNoProvider || allPossibleProviders == null || allPossibleProviders.Length == 0)
				{
					return null;
				}
				else
				{
					return allPossibleProviders[0];
				}
			}

			#endregion
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new CusUnderbondUnionCollectionWithProvider(Parent);
		}

		#endregion

		void Underbonds_ModeOfMovementChanged(object sender, EventArgs e)
		{
			underbonds_ModeOfMovementChangedFired = true;
		}
		bool underbonds_ModeOfMovementChangedFired;

		void Underbonds_CanSendOutturnChanged(object sender, EventArgs e)
		{
			underbonds_CanSendOutturnChangedFired = true;
		}
		bool underbonds_CanSendOutturnChangedFired;

		void Underbonds_MovementReasonChanged(object sender, EventArgs e)
		{
			underbonds_MovementReasonChangedFired = true;
		}
		bool underbonds_MovementReasonChangedFired;
	}
}
