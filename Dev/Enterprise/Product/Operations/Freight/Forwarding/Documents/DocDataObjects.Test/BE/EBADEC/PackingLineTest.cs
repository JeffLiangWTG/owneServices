using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.BE;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Documents.Testing.BE
{
	[TestedType(typeof(PackingLine))]
	sealed class PackingLineTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var packLine = new PackingLine(1);
			packLine.MovementReferenceNumbers = new List<MovementReferenceNumber>();

			return packLine;
		}
	}
}
