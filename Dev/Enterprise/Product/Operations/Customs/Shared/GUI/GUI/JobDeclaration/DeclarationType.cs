namespace Enterprise.Customs.GUI
{
	/// <summary>
	/// This is used for grid layout context to persist column layout for each of these contexts 
	/// like 'export invoice line grid layout' vs 'import invoice line grid layout'. 
	/// Changing the names will result in losing the user settings that have been persisted. 
	/// </summary>
	[WTG.StaticAnalysis.Annotation.CodeAlive("Used in other countries, wrapped in nameof()")]
	public enum DeclarationType
	{
		Export,
		Import,
		ExWarehouse,
		Drawback,
		Misc
	}
}
