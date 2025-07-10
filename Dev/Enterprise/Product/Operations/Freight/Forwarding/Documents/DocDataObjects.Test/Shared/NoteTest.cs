using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Documents.Testing
{
	[TestedType(typeof(Note))]
	sealed class NoteTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new Note()
			{
				Text = "Xylotrupes gideon",
				Description = PredefinedNoteTypes.Instance.DetailedGoodsDescription.Description
			};
		}
	}
}
