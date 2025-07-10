using System;
using CargoWise.EntityFramework;
using Enterprise.Freight.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Tracking.Business.Testing
{
	[TestedType(typeof(TrackingCFSShipment))]
	sealed class TrackingCFSShipmentBOTest : ShipmentBusinessObjectTest
	{
		#region Metadata

		protected override Type ExpectedMetadataType
		{
			get
			{
				return typeof(Metadata.Business.CommonShipment);
			}
		}

		#endregion

		public override void TestSaveAndDeleteBusinessObject()
		{
			Assert("not applicable for read-only web objects", true);
		}

		#region Setup

		protected override BusinessObject GetNewBusinessObject()
		{
			return Factory.New<TrackingCFSShipment>();
		}

		#endregion
	}
}
