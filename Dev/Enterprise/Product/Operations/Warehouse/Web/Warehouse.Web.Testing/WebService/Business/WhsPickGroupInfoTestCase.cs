using Enterprise.Registry.Business.Warehouse;
using Enterprise.Warehouse.Web.WebService.Common;
using Enterprise.Warehouse.Web.WebService.Common.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Warehouse.Web.WebService.Business.Testing
{
	[TestedType(typeof(WhsPickGroupInfo))]
	public class WhsPickGroupInfoTestCase : DataObjectInfoTestCase<WhsPickGroupInfo>
	{
		#region Constructor

		public void TestConstructor()
		{
			var pickGroup = new PickGroup();
			pickGroup.PickSequence = 3;
			pickGroup.Description = (NoResString)"Desc";

			var pickGroupInfo = new WhsPickGroupInfo(pickGroup);
			AssertEquals((short)3, pickGroupInfo.PickSequence);
			AssertEquals("Desc", pickGroupInfo.Description);
		}

		#endregion

		#region Properties

		#region TestPickSequence

		public void TestPickSequence()
		{
			var pickGroupInfo = new WhsPickGroupInfo();
			pickGroupInfo.PickSequence = 3;
			AssertEquals((short)3, pickGroupInfo.PickSequence);
		}

		#endregion

		#region TestDescription

		public void TestDescription()
		{
			var pickGroupInfo = new WhsPickGroupInfo();
			pickGroupInfo.Description = "Desc";
			AssertEquals("Desc", pickGroupInfo.Description);
		}

		#endregion

		#endregion

		#region Implementation

		protected new WhsPickGroupInfo Parent
		{
			get { return (WhsPickGroupInfo)base.Parent; }
		}

		protected override DataObjectInfo GetNewObjectInfo()
		{
			return new WhsPickGroupInfo();
		}

		#endregion
	}
}
