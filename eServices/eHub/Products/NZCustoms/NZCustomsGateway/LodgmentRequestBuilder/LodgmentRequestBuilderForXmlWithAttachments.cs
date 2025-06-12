using System.Collections.Generic;
using System.IO;
using CargoWise.eHub.Products.NZCustoms.Client.SubmitLodgement_v2;
using CargoWise.eHub.Products.NZCustoms.Common;
using Common.Logging;
using LodgmentBinary = CargoWise.eHub.Products.NZCustoms.Client.SubmitLodgement_v2.Binary;

namespace CargoWise.eHub.Products.NZCustoms.Gateway
{
	public class LodgmentRequestBuilderForXmlWithAttachments : LodgmentRequestBuilder
	{
		readonly string authentication;

		public LodgmentRequestBuilderForXmlWithAttachments(ILog logger, string message, string authentication)
			: base(logger, message)
		{
			this.authentication = authentication;
		}

		public override SubmitLodgementRequest Create()
		{
			LodgementDocument[] documents = null;

			using (var messageStream = new MemoryStream(Message))
			{
				documents = LodgementDocumentCollection.Create(Logger, messageStream);
			}

			if (documents == null) return null;

			var bin = new LodgmentBinary
			{
				href = "Declaration",
				Value = Message
			};

			var manifestItems = new List<ManifestItem>();

			for (int i = 0; i < documents.Length; i++)
			{
				var manifestItem = new ManifestItem();

				manifestItem.Type = documents[i].DocumentType;
				manifestItem.MimeTypeQualifierCode = documents[i].DocumentMediaType;

				if (documents[i].DocumentType == Constants.DeclarationDocumentType)
				{
					manifestItem.Authentication = authentication;
					manifestItem.UniformResourceIdentifier = Constants.DeclarationContentId;
				}
				else
				{
					manifestItem.UniformResourceIdentifier = documents[i].FileName;
				}

				manifestItems.Add(manifestItem);
			}

			var manifest = new Manifest
			{
				NumberOfItems = manifestItems.Count.ToString(),
				ManifestItem = manifestItems.ToArray()
			};

			return new SubmitLodgementRequest(manifest, bin);
		}

		protected override string MimeTypeQualifierCode
		{
			get { return Constants.XmlDeclarationMediaType; }
		}
	}
}