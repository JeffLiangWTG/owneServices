using System;
using System.Text;
using CargoWise.eHub.Products.NZCustoms.Common;
using CargoWise.eHub.Products.NZCustoms.Client.SubmitLodgement_v2;
using Common.Logging;
using LodgmentBinary = CargoWise.eHub.Products.NZCustoms.Client.SubmitLodgement_v2.Binary;

namespace CargoWise.eHub.Products.NZCustoms.Gateway
{
	public abstract class LodgmentRequestBuilder
	{
		protected ILog Logger { get; private set; }
		protected byte[] Message { private set; get; }

		protected LodgmentRequestBuilder(ILog logger, string message)
		{
			if (logger == null) throw new ArgumentNullException("logger");
			Logger = logger;

			Message = DecodeMessage(message);
		}

		public virtual string GetMessageForLogging()
		{
			try
			{
				return Encoding.UTF8.GetString(Message);
			}
			catch (Exception exception)
			{
				var warningMessage = "Failed to get message content.";
				Logger.Warn(warningMessage, exception);
				return warningMessage;
			}
		}

		public virtual SubmitLodgementRequest Create()
		{
			var bin = new LodgmentBinary
			{
				href = Constants.DeclarationContentId,
				Value = Message
			};

			var manifestItem = new ManifestItem();
			manifestItem.Type = Constants.DeclarationDocumentType;
			manifestItem.MimeTypeQualifierCode = MimeTypeQualifierCode;
			manifestItem.UniformResourceIdentifier = Constants.DeclarationContentId;

			var manifest = new Manifest
			{
				NumberOfItems = "1",
				ManifestItem = new[] { manifestItem }
			};

			return new SubmitLodgementRequest(manifest, bin);
		}

		protected byte[] DecodeMessage(string message)
		{
			return Convert.FromBase64String(message);
		}

		protected abstract string MimeTypeQualifierCode { get; }
	}
}