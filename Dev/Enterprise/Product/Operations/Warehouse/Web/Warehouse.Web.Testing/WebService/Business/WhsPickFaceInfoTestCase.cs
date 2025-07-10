using Enterprise.Warehouse.Web.WebService.Common;
using Enterprise.Warehouse.Web.WebService.Common.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Web.WebService.Business.Testing
{
	[TestedType(typeof(WhsPickFaceInfo))]
	public class WhsPickFaceInfoTestCase : DataObjectInfoTestCase<WhsPickFaceInfo>
	{
		#region Constructor

		public void TestConstructor()
		{
			var pickFaceInfo = new WhsPickFaceInfo();
			AssertEquals("", pickFaceInfo.ClientCode);
			AssertEquals("", pickFaceInfo.LocationString);
			AssertEquals("", pickFaceInfo.LocationString_UserFriendly);
			AssertEquals(false, pickFaceInfo.ShouldRetainPalletIDs);
		}

		#endregion

		#region Properties

		#region TestPickSequence

		public void TestPickSequence()
		{
			var pickFaceInfo = new WhsPickFaceInfo();
			pickFaceInfo.ClientCode = "CL1";
			AssertEquals("CL1", pickFaceInfo.ClientCode);
		}

		#endregion

		#region TestLocationString

		public void TestLocationString()
		{
			var pickFaceInfo = new WhsPickFaceInfo();
			pickFaceInfo.LocationString = "E95";
			AssertEquals("E95", pickFaceInfo.LocationString);
		}

		#endregion

		#region TestLocationString_UserFriendly

		public void TestLocationString_UserFriendly()
		{
			var pickFaceInfo = new WhsPickFaceInfo();
			pickFaceInfo.LocationString_UserFriendly = "E95";
			AssertEquals("E95", pickFaceInfo.LocationString_UserFriendly);
		}

		#endregion

		#region TestShouldRetainPalletIDs

		public void TestShouldRetainPalletIDs()
		{
			var pickFaceInfo = new WhsPickFaceInfo();
			pickFaceInfo.ShouldRetainPalletIDs = true;
			AssertEquals(true, pickFaceInfo.ShouldRetainPalletIDs);
		}

		#endregion

		#endregion

		#region Implementation

		protected new WhsPickFaceInfo Parent
		{
			get { return (WhsPickFaceInfo)base.Parent; }
		}

		protected override DataObjectInfo GetNewObjectInfo()
		{
			return new WhsPickFaceInfo();
		}

		#endregion
	}
}
