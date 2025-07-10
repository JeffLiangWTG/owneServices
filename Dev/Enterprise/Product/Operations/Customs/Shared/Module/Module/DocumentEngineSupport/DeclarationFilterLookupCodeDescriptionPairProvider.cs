namespace Enterprise.Customs.Module
{
	public abstract class DeclarationFilterLookupCodeDescriptionPairProvider : DocumentEngine.RuntimeOptions.ICodeDescriptionPairListProvider, ZArchitecture.Core.ICodeDescriptionPairListProvider
	{
		JobDeclarationFilterBusinessObjectFactory Factory
		{
			get { return factory ?? (factory = new JobDeclarationFilterBusinessObjectFactory()); }
		}
		JobDeclarationFilterBusinessObjectFactory factory;

		protected JobDeclarationFilterBusinessObject FilterBizo
		{
			get { return filterBizo ?? (filterBizo = Factory.GetJobDeclarationFilterBusinessObjectForCountry()); }
		}
		JobDeclarationFilterBusinessObject filterBizo;

		#region Enterprise.DocumentEngine.RuntimeOptions.ICodeDescriptionPairListProvider Members

		public abstract ZArchitecture.Core.ReadOnlyCodeDescriptionPairList GetCodeDescriptionPairList();

		#endregion

		#region Enterprise.ZArchitecture.Core.ICodeDescriptionPairListProvider Members

		public ZArchitecture.Core.CodeDescriptionPairList CodeDescriptionPairList
		{
			get
			{
				return new ZArchitecture.Core.CodeDescriptionPairList(GetCodeDescriptionPairList());
			}
		}

		#endregion
	}
}
