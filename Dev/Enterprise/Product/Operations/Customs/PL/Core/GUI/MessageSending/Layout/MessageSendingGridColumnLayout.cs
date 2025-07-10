using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.PL.GUI;

public class MessageSendingGridColumnLayout : IGridColumnLayoutProvider
{
	public IGridColumnLayout Layout => layout ?? (layout = CreateLayout());
	IGridColumnLayout layout;

	#region Implementation

	IGridColumnLayout CreateLayout()
	{
		var columnBag = MessageSendingGridColumnBag.Instance;

		var builder = GridColumnLayoutBuilder.Create();
		builder.AddColumn(columnBag.ShouldSendCheckBoxColumn);
		builder.AddColumn(columnBag.ActionDropEditColumnStyle);
		builder.AddColumn(columnBag.DeclarationDateDateEditColumn);
		builder.AddColumn(columnBag.ReferenceNumberTextBoxColumn);
		builder.AddColumn(columnBag.EntryNumberTextBoxColumn);
		builder.AddColumn(columnBag.EntryStatusTextBoxColumn);
		builder.AddColumn(columnBag.StatusTextBoxColumn);
		builder.AddColumn(columnBag.EntryDescriptionTextBoxColumn);
		builder.AddColumn(columnBag.ResponseMessageDropEditColumn);

		return builder.Build();
	}

	#endregion
}
