using System;
using Enterprise.Customs.EU.Business.Declaration;

namespace Enterprise.Customs.PL.GUI;

public partial class SingleLineEntryForm : EU.GUI.SingleLineEntry.SingleLineEntryForm
{
	[Obsolete("Use the constructor that takes a business object, this constructor is just for the designer", true)]
	public SingleLineEntryForm() { }

	public SingleLineEntryForm(ISingleLineEntryManager manager)
		: base(manager)
	{
		InitializeComponent();
	}
}
