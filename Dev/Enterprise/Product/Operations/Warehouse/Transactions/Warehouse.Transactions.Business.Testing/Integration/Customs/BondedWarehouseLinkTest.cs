using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Warehouse.Integration.BondedWarehouse;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Bonded.Testing
{
	public abstract class BondedWarehouseLinkTest : TestCaseWithFactory
	{
		#region Nature 20s

		public abstract void TestCreateOrUpdateInwardMovement();
		public abstract void TestUpdateDeclarationReference();
		public abstract void TestLoad();

		[ExpectException(typeof(ArgumentNullException))]
		public void TestCreateOrUpdateInwardMovementWithNullInterfaceThrowsException()
		{
			var link = GetNewLink();
			link.CreateOrUpdateInwardMovement(null);
		}

		[ExpectException(typeof(ArgumentException))]
		public void UpdateDeclarationReference()
		{
			var link = GetNewLink();
			link.UpdateDeclarationReference(ZGuid.Empty, "TEST");
		}

		//[ExpectException(typeof(ArgumentNullException))]
		//public void TestLoadWithNullClientThrowsException()
		//{
		//	var link = GetNewLink();
		//	link.Load(null, null);
		//}
		//[ExpectException(typeof(NotSupportedException))]
		//public void TestLoadWithNullReferenceThrowsException()
		//{
		//	var link = GetNewLink();
		//	link.Load(GlbBranch.CurrentBranch.OrgProxy, null);
		//}

		#endregion

		#region Nature 30s

		#region TestCreateOrUpdateOutwardMovement

		public abstract void TestCreateOrUpdateOutwardMovement();

		[ExpectException(typeof(ArgumentNullException))]
		public void TestCreateOrUpdateOutwardMovement_WithNullInterfaceThrowsException()
		{
			var link = GetNewLink();
			link.CreateOrUpdateOutwardMovement(null, false);
		}

		#endregion

		#region TestCancelOutwardMovement

		public abstract void TestCancelOutwardMovement();

		[ExpectException(typeof(ArgumentNullException))]
		public void TestCancelOutwardMovement_WithNullInterfaceThrowsException()
		{
			var link = GetNewLink();
			link.CancelOutwardMovement(ZGuid.Empty);
		}

		#endregion

		#region TestNotifyGoodsAreClearedForRelease

		public abstract void TestNotifyGoodsAreClearedForRelease();

		[ExpectException(typeof(ArgumentNullException))]
		public void NotifyGoodsAreClearedForRelease_WithNullPKThrowsException()
		{
			var link = GetNewLink();
			link.NotifyGoodsAreClearedForRelease(ZGuid.Empty, null);
		}

		[ExpectException(typeof(ArgumentNullException))]
		public void NotifyGoodsAreClearedForRelease_WithNullTransaction()
		{
			var link = GetNewLink();
			link.NotifyGoodsAreClearedForRelease(ZGuid.NewZGuid(), null);
		}

		#endregion

		#endregion

		#region Findboxes

		public abstract void TestGetEntryKeyLookup();

		#endregion

		#region Implementation

		protected abstract IBondedWarehouseLink GetNewLink();

		#endregion
	}
}
