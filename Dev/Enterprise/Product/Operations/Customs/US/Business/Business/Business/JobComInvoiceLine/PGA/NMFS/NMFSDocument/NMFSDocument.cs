using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business
{
	public class NMFSDocument : CusCodeData, INMFSDocument
	{
		public NMFSDocument(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Implementation

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CY_Type = CusCodeDataTypeList.Codes.NMFSDocument;
		}

		#endregion

		#region Related Objects

		public NMFSLine NMFSLine
		{
			get { return Parent as NMFSLine; }
		}

		protected override TypeLoaderCollection parentLoaders
		{
			get { return new TypeLoaderCollection(typeof(NMFSLine)); }
		}

		#endregion

		#region Override Properties

		[ResourceStringData("Enterprise.Customs.US.Business.NMFSDocumentDetails|DocumentType", Caption = "Document Type", ShortCaption = "Doc. Type")]
		[MaxLength(3)]
		public override ZString CY_Code
		{
			get { return base.CY_Code; }
			set { base.CY_Code = value; }
		}

		[ResourceStringData("Enterprise.Customs.US.Business.NMFSDocumentDetails|DISDocumentID", Caption = "DIS Document ID", ShortCaption = "DIS Doc. ID")]
		[List(nameof(Lookups) + "." + nameof(NMFSDocumentLookups.DISDocumentIDList))]
		[MaxLength(30)]
		public override ZString CY_Data
		{
			get { return base.CY_Data; }
			set { base.CY_Data = value; }
		}

		#endregion

		#region Lookups / Validation

		public new NMFSDocumentLookups Lookups
		{
			get { return (NMFSDocumentLookups)base.Lookups; }
		}

		protected override CusCodeDataLookups GetNewLookups()
		{
			return new NMFSDocumentLookups(this);
		}

		public new NMFSDocumentValidation Validation
		{
			get { return (NMFSDocumentValidation)base.Validation; }
		}

		protected override CusCodeDataValidation GetNewValidation()
		{
			return new NMFSDocumentValidation(this);
		}

		#endregion

		#region INMFSDocument Members

		ZString INMFSDocument.DocumentIdentifier
		{
			get { return CY_Code; }
		}

		ZString INMFSDocument.DocumentNumber
		{
			get { return CY_Data; }
		}

		#endregion
	}
}
