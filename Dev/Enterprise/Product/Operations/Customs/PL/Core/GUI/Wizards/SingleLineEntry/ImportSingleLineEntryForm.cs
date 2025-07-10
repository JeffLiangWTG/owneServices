using System;
using Enterprise.Customs.EU.Business.Declaration;

namespace Enterprise.Customs.PL.GUI;

public partial class ImportSingleLineEntryForm : SingleLineEntryForm
{
	[Obsolete("Use the constructor that takes a business object, this constructor is just for the designer", true)]
	public ImportSingleLineEntryForm() { }

	public ImportSingleLineEntryForm(ISingleLineEntryManager manager)
		: base(manager)
	{
		InitializeComponent();
	}
}
