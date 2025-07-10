using CargoWise.ComponentModel;
using CargoWise.Types;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.DIS.Business
{
	public class DISCBPRequest : AutoDISCBPRequest
	{
		public DISCBPRequest(DISDocument disDocument)
			: base(disDocument.Factory)
		{
			this.disDocument = disDocument;
		}

		readonly DISDocument disDocument;

		[List(nameof(DefaultCBPRequests))]
		public override ZString ID
		{
			get { return base.ID; }
			set { base.ID = value; }
		}

		public CodeDescriptionPairList DefaultCBPRequests
		{
			get { return disDocument.DefaultCBPRequestList; }
		}

		[List(nameof(CBPRequestTypeList))]
		public override ZString Type
		{
			get { return base.Type; }
			set { base.Type = value; }
		}

		public CodeDescriptionPairList CBPRequestTypeList
		{
			get { return Factory.GetCachedValue<CBPRequestTypeList>(); }
		}
	}
}
