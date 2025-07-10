using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.MessageBuilders;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.ZA.Manifest.Business
{
	[TestExcludeBusinessObjectsAllHaveTestCases]
	public partial class SupportingDocSendingObject : Customs.Business.SupportingDocSendingObject, ISupportingDocumentMessageDataProvider
	{
		public SupportingDocSendingObject(ISupportingDocObject manifest) : base(manifest)
		{
		}

		protected override void SetDefaultValues()
		{
			ShouldSend = true;
		}

		public static SupportingDocSendingObject New(IManifestSupportingDocSendingObject manifest)
		{
			var result = (SupportingDocSendingObject)manifest.GetSupportingDocSendingObject();
			return result;
		}

		public new IManifestSupportingDocSendingObject SupportingDocObject => (IManifestSupportingDocSendingObject)base.SupportingDocObject;

		protected override void GetExtraDocsIfNoneAvailable(Guid edocKey)
		{
		}

		#region Lookup Lists

		protected override ZString DocumentTypeCode => Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ZADocumentType;

		protected override List<ZString> ExtensionFilter => new List<ZString>() { "pdf" };

		public override CodeDescriptionPairList CaseNumbers
		{
			get
			{
				var caseNumbers = new CodeDescriptionPairList();
				if (SupportingDocObject is AsycudaManifestHeader header)
				{
					Action<ZA.Business.CaseNumber> addAction = x => caseNumbers.Add(x);
					header.CaseNumbers.Cast<ZA.Business.CaseNumber>().ToList().ForEach(addAction);
					header.Bills.AsEnumerable().Cast<AsycudaBill>().SelectMany(x => x.CaseNumbers.Cast<ZA.Business.CaseNumber>()).ToList().ForEach(addAction);
				}
				return caseNumbers;
			}
		}

		[List(nameof(Entries))]
		[BusinessObjectTestExclude]
		public override ZString LocalReferenceNumber
		{
			get { return base.LocalReferenceNumber; }
			set { base.LocalReferenceNumber = value; }
		}

		#endregion
		protected override void DefaultDocType()
		{
			if (Document != null)
			{
				DocumentType = ZZRefCusMapCombined.MapCW1CodeToCustomsCode(Factory, Core.Constants.CountryCodes.SouthAfrica, RefCusMapTypeList.Codes.ZADOC, Document.DocType, ZDateTime.Today);
			}
		}

		public override SupportingDocUniversalEventBuilder GetSupportingDocUniversalEventBuilder()
		{
			return new ManifestSupportingDocUniversalEventBuilder(this);
		}

		protected override Customs.Business.SupportingDocSendingObjectValidation GetNewValidation() => new SupportingDocSendingObjectValidation(this);
	}
}
