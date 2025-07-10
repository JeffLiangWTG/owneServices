using System.Collections.Generic;
using System.Windows.Forms;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.GUI.Testing
{
	[TestedType(typeof(ConsolidatedDeclarationControlBag))]
	sealed class ConsolidatedDeclarationControlBagTest : ControlBagAbstractTest
	{
		public void TestControlTypes()
		{
			var controls = new Dictionary<ControlReference, Control>();
			CombineAssertions(() =>
			{
				using (var panel = new ZPanel())
				{
					var bag = GetControlBagForTesting();
					bag.CreateControls(panel, controls);
					AssertControlType<ZTextBox>(nameof(ConsolidatedDeclarationControlBag.JobNumberTextBox), bag, controls);
					AssertControlType<ZTextBox>(nameof(ConsolidatedDeclarationControlBag.EntryNumberTextBox), bag, controls);
					AssertControlType<ZTextBox>(nameof(ConsolidatedDeclarationControlBag.EntryStatusTextBox), bag, controls);
					AssertControlType<ZDropEdit>(nameof(ConsolidatedDeclarationControlBag.MessageTypeDropEdit), bag, controls);
					AssertControlType<ZDropEdit>(nameof(ConsolidatedDeclarationControlBag.MessageSubTypeDropEdit), bag, controls);
					AssertControlType<ZDropEdit>(nameof(ConsolidatedDeclarationControlBag.TransportModeDropEdit), bag, controls);
					AssertControlType<ZGuidFindBox>(nameof(ConsolidatedDeclarationControlBag.ImporterGuidFindBox), bag, controls);
					AssertControlType<ZCodeFindBox>(nameof(ConsolidatedDeclarationControlBag.VesselCodeFindBox), bag, controls);
					AssertControlType<ZTextBox>(nameof(ConsolidatedDeclarationControlBag.VoyageFlightNoTextBox), bag, controls);
					AssertControlType<ZDateEdit>(nameof(ConsolidatedDeclarationControlBag.DischargeETADateEdit), bag, controls);
					AssertControlType<ZCodeFindBox>(nameof(ConsolidatedDeclarationControlBag.PortOfLoadingCodeFindBox), bag, controls);
					AssertControlType<ZCodeFindBox>(nameof(ConsolidatedDeclarationControlBag.PortOfDischargeCodeFindBox), bag, controls);
					AssertControlType<ZDateEdit>(nameof(ConsolidatedDeclarationControlBag.EntryPeriodDateEdit), bag, controls);
				}
			});
		}

		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(ConsolidatedDeclarationControlBag.JobNumberTextBox);
				yield return nameof(ConsolidatedDeclarationControlBag.EntryNumberTextBox);
				yield return nameof(ConsolidatedDeclarationControlBag.EntryStatusTextBox);
				yield return nameof(ConsolidatedDeclarationControlBag.MessageTypeDropEdit);
				yield return nameof(ConsolidatedDeclarationControlBag.MessageSubTypeDropEdit);
				yield return nameof(ConsolidatedDeclarationControlBag.TransportModeDropEdit);
				yield return nameof(ConsolidatedDeclarationControlBag.ImporterGuidFindBox);
				yield return nameof(ConsolidatedDeclarationControlBag.VesselCodeFindBox);
				yield return nameof(ConsolidatedDeclarationControlBag.VoyageFlightNoTextBox);
				yield return nameof(ConsolidatedDeclarationControlBag.DischargeETADateEdit);
				yield return nameof(ConsolidatedDeclarationControlBag.PortOfLoadingCodeFindBox);
				yield return nameof(ConsolidatedDeclarationControlBag.PortOfDischargeCodeFindBox);
				yield return nameof(ConsolidatedDeclarationControlBag.EntryPeriodDateEdit);
			}
		}

		protected override ControlBag GetControlBagForTesting() => ConsolidatedDeclarationControlBag.Instance;

		void AssertControlType<T>(string controlName, ControlBag bag, IDictionary<ControlReference, Control> controls)
		{
			AssertType<T>(controlName, controls[new ControlReference(bag, controlName)]);
		}
	}
}
