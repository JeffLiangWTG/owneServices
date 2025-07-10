namespace Enterprise.Customs.Module
{
	using System;
	using Enterprise.Customs.Business;
	using Enterprise.MasterFiles.Business.CustomValues;
	using Enterprise.Messaging.Business;
	using Enterprise.ZArchitecture.Schema;

	public class GenAddOnColumnHelper
	{
		public GenAddOnColumnQueryHelper SimpleQueryHelper
		{
			get { return simpleQueryHelper ?? (simpleQueryHelper = new GenAddOnColumnQueryHelper(typeof(BaseJobDeclaration))); }
		}
		[ThreadStatic]
		static GenAddOnColumnQueryHelper simpleQueryHelper;

		public GenAddOnColumnQueryHelper QueryHelper
		{
			get { return queryHelper ?? (queryHelper = new GenAddOnColumnQueryHelper(typeof(BaseJobDeclaration), typeof(CusEntryHeader), CusEntryHeaderSchema.CH_JE)); }
		}
		GenAddOnColumnQueryHelper queryHelper;

		public GenAddOnColumnQueryHelper SimpleQueryHelperForEDIMessage
		{
			get { return simpleQueryHelperForEDIMessage ?? (simpleQueryHelperForEDIMessage = new GenAddOnColumnQueryHelper(typeof(EDIMessage))); }
		}
		GenAddOnColumnQueryHelper simpleQueryHelperForEDIMessage;
	}
}
