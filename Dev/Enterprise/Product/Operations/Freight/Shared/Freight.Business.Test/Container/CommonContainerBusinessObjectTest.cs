using System.Linq;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Business.Testing
{
	[TestedType(typeof(CommonContainer))]
	public class CommonContainerBusinessObjectTest : EnterpriseBusinessObjectTestCase
	{
		public void TestNoteTypes()
		{
			var container = GetNewBusinessObject() as CommonContainer;
			AssertNotNull("Container shoud not be null", container);

			var expected = new[]
			{
				PredefinedNoteTypes.Instance.SpecialInstructions,
				PredefinedNoteTypes.Instance.UnrecognisedAdditionalReferenceTypes,
				PredefinedNoteTypes.Instance.DeliveryInstructionsNote,
				PredefinedNoteTypes.Instance.PickupInstructionsNote,
				PredefinedNoteTypes.Instance.HandlingInstructions,
				PredefinedNoteTypes.Instance.AutoRatingAuditLog
			};
			AssertContainsExactElementsInAnyOrder("Should have 5 noteTypes", expected, container.NoteTypes.Cast<PredefinedNoteType>());
		}
	}
}
