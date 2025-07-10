using System;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	[TestedType(typeof(ForwardingConsol))]
	sealed class ForwardingConsolBOTest : BusinessObjectWithCustomLabelsTestCase
	{
		#region Metadata

		protected override Type ExpectedMetadataType
		{
			get
			{
				return typeof(Metadata.Business.ForwardingConsol);
			}
		}

		#endregion

		protected override ICustomLabelsProvider GetNewCustomLabelsProvider(BusinessObject bO)
		{
			return new ForwardingConsol.CustomLabelsProvider(GlbCompany.CurrentCompany.OrgProxy);
		}
	}
}
