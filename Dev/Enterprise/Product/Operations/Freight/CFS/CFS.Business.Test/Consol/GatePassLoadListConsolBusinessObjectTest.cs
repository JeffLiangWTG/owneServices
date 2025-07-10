using System;
using NUnit.Framework;

namespace Enterprise.Freight.CFS.Business.Testing
{
	[TestedType(typeof(GatePassLoadListConsol))]
	public class GatePassLoadListConsolBusinessObjectTest : CFSBusinessObjectTestCase
	{
		#region Metadata

		protected override Type ExpectedMetadataType
		{
			get
			{
				return typeof(Metadata.Business.CommonConsol);
			}
		}

		#endregion
	}
}
