using System;
using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Freight.CFS.Business.Testing
{
	[TestedType(typeof(CFSLoadListConsol))]
	public class LoadListConsolBusinessObjectTestCase : CFSBusinessObjectTestCase
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

		#region Overrides

		protected override BusinessObject GetNewBusinessObject()
		{
			return Factory.New<CFSLoadListConsol>();
		}

		#endregion
	}
}
