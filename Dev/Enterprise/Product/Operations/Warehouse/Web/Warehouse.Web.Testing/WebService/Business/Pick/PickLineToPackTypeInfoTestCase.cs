using System;
using System.Linq;
using Enterprise.Warehouse.Web.WebService.Common;
using Enterprise.Warehouse.Web.WebService.Common.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Web.WebService.Business.Testing
{
	[TestedType(typeof(PickLinesToPickedPackTypeInfo))]
	public class PickLineToPackTypeInfoTestCase : DataObjectInfoTestCase<PickLinesToPickedPackTypeInfo>
	{
		#region TestConstructor

		public void TestConstructor_NullArgs()
		{
			AssertExceptionThrown(typeof(ArgumentNullException), () => new PickLinesToPickedPackTypeInfo(null, Array.Empty<PickedPackTypeInfo>()));
			AssertExceptionThrown(typeof(ArgumentNullException), () => new PickLinesToPickedPackTypeInfo(Array.Empty<Guid>(), null));
		}

		public void TestConstructor_EmptyConstructor()
		{
			var info = new PickLinesToPickedPackTypeInfo();
			AssertEquals("No pickline PKs.", 0, info.PickLinePKs.Length);
			AssertEquals("No picked pack type infos.", 0, info.PickedPackTypes.Length);
		}

		public void TestConstructor()
		{
			var guid1 = new Guid();
			var guid2 = new Guid();
			var packTypeInfo1 = new PickedPackTypeInfo();
			var packTypeInfo2 = new PickedPackTypeInfo();

			var info = new PickLinesToPickedPackTypeInfo(new[] { guid1, guid2 }, new[] { packTypeInfo1, packTypeInfo2 });
			AssertEquals("PickLinePKs count is correct.", 2, info.PickLinePKs.Length);
			Assert("PickLinePKs content is correct.", info.PickLinePKs.Contains(guid1));
			Assert("PickLinePKs content is correct.", info.PickLinePKs.Contains(guid2));

			AssertEquals("PackTypeInfos count is correct.", 2, info.PickedPackTypes.Length);
			Assert("PackTypeInfos content is correct.", info.PickedPackTypes.Contains(packTypeInfo1));
			Assert("PackTypeInfos content is correct.", info.PickedPackTypes.Contains(packTypeInfo2));
		}

		protected override DataObjectInfo GetNewObjectInfo()
		{
			return new PickLinesToPickedPackTypeInfo();
		}

		#endregion
	}
}
