using CargoWise.Integration;
using Enterprise.Customs.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business
{
	public class NMFSDocumentLookups : CusCodeDataLookups
	{
		public NMFSDocumentLookups(NMFSDocument parent)
			: base(parent)
		{
		}

		new NMFSDocument Parent
		{
			get { return (NMFSDocument)base.Parent; }
		}

		NMFSLine NMFSLine
		{
			get { return Parent.NMFSLine; }
		}

		public override CodeDescriptionPairList CY_CodeList
		{
			get { return NMFSLine != null ? NMFSLine.AddInfoLookups.DocumentTypeList : new CodeDescriptionPairList(); }
		}

		public ICodeDescriptionPairList DISDocumentIDList
		{
			get { return NMFSLine != null ? NMFSLine.AddInfoLookups.DISDocumentIDList : new CodeDescriptionPairList(); }
		}
	}
}
