using System;
using NUnit.Framework;

namespace Enterprise.Freight.CFS.Business.Testing
{
	[TestedType(typeof(PackUnpackLoadListConsol))]
	public class PackUnpackLoadListConsolBusinessObjectTest : CFSBusinessObjectTestCase
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
