using System;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Warehouse.Web.WebService.Common;
using Enterprise.Warehouse.Web.WebService.Common.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Web.WebService.Business.Testing
{
	[TestedType(typeof(WhsCartonGroupInfo))]
	public class WhsCartonGroupInfoTestCase : DataObjectInfoTestCase<WhsCartonGroupInfo>
	{
		#region TestConstructor

		public void TestConstructor()
		{
			var cartonGroup = new WhsCartonGroupInfo();

			AssertNotNull(cartonGroup);
			AssertEquals("", cartonGroup.Code);
		}

		#endregion

		#region TestConstructor_MissingCartonGroup

		public void TestConstructor_MissingCartonGroup()
		{
			try
			{
				new WhsCartonGroupInfo(null);
			}
			catch (ArgumentNullException ex)
			{
				AssertEquals("cartonGroup", ex.ParamName);
			}
		}

		#endregion

		#region TestConstructor_WithWhsCartonGroup

		public void TestConstructor_WithWhsCartonGroup()
		{
			var cartonGroup = Helper.CreateWhsCartonGroup("ORG", "WhsOrg");
			var smallCarton = Helper.CreateWhsCartonSize("SML", 10, 10, 10, 10, 20, 40, new ZByte(80), Constants.Length.Metres, Constants.Weight.Tonnes);
			var bigCarton = Helper.CreateWhsCartonSize("BIG", 20, 20, 20, 20, 40, 80, new ZByte(80), Constants.Length.Metres, Constants.Weight.Tonnes);
			cartonGroup.CartonSizes.AddRange(new[] { smallCarton, bigCarton });

			var cartonGroupInfo = new WhsCartonGroupInfo(cartonGroup);
			AssertNotNull(cartonGroupInfo);
			AssertEquals(cartonGroupInfo.Code, cartonGroup.WCG_Code);
			AssertNotNull(cartonGroupInfo.CartonSizes);
			AssertEquals(cartonGroupInfo.CartonSizes.Count, cartonGroup.CartonSizes.Count);

			cartonGroup.WCG_Code = "TTT";
			cartonGroup.CartonSizes.DeleteAll();

			cartonGroupInfo = new WhsCartonGroupInfo(cartonGroup);
			AssertNotNull(cartonGroupInfo);
			AssertEquals(cartonGroupInfo.Code, cartonGroup.WCG_Code);
			AssertNotNull(cartonGroupInfo.CartonSizes);
			AssertEquals(cartonGroupInfo.CartonSizes.Count, cartonGroupInfo.CartonSizes.Count);
		}

		#endregion

		#region Properties

		public void TestProperties()
		{
			AssertEquals("", Parent.Code);
			AssertNotNull(Parent.CartonSizes);
			AssertEquals(0, Parent.CartonSizes.Count);

			var pk = Guid.NewGuid();
			Parent.Code = "AAA";
			AssertEquals("AAA", Parent.Code);

			var cartonSizes = new WhsCartonSizeInfoCollection();
			Parent.CartonSizes = cartonSizes;
			AssertEquals(cartonSizes, Parent.CartonSizes);
		}

		#endregion

		#region Implementation

		protected new WhsCartonGroupInfo Parent
		{
			get
			{
				return (WhsCartonGroupInfo)base.Parent;
			}
		}

		protected override DataObjectInfo GetNewObjectInfo()
		{
			return new WhsCartonGroupInfo();
		}

		#endregion
	}
}
