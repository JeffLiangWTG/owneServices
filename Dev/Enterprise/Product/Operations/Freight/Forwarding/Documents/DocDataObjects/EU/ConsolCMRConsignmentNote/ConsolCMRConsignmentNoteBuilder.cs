using CargoWise.Common;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects
{
	public class ConsolCMRConsignmentNoteBuilder : ICMRConsignmentNoteBuilder
	{
		readonly ForwardingConsol consol;
		readonly IDocDataObjectParameters parameters;

		public ConsolCMRConsignmentNoteBuilder(ForwardingConsol consol, IDocDataObjectParameters parameters)
		{
			this.consol = Argument.NotNull(consol, nameof(consol));
			this.parameters = parameters;
		}

		public CMRConsignmentNoteDocDataObjectCollection Build()
		{
			var consolCmr = new ConsolCMRConsignmentNote(consol, parameters);
			return new CMRConsignmentNoteDocDataObjectCollection([new CMRConsignmentNoteDocDataObject(consolCmr)]);
		}
	}
}
