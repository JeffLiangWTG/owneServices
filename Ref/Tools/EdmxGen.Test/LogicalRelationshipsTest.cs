using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.EdmxGen.Test
{
	[TestFixture]
	public class LogicalRelationshipsTest
	{
		[Test]
		public void LogicalConfiguration()
		{
			new TablesConfig().GetTablesConfigurations();
			Assert.That(true);
		}
	}
}
