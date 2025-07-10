using System.Data;
using System.IO;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Messaging.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.eManifest.Business
{
	public class SyntaxErrorMessage : EDIMessage, Integration.Customs.US.eManifest.IEDIMessage
	{
		public SyntaxErrorMessage(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			EM_MessageType = MessageTypes.Codes.SyntaxError;
		}

		#region MessageStreamFormatter

		protected override IStreamFormatter MessageStreamFormatter
		{
			get
			{
				return SourceMessageTextWithErrorMarks.IsEmpty ? new EDIMessageStreamFormatter()
						: new SyntaxErrorMessageStreamFormatter(
							Res.GetString("f55a17c0-bdf2-43e0-8112-0a05b1399136", "Source Message Interpretation:\r\n\r\n{0}\r\n\r\n\r\nMessage Text:",
										  SourceMessageTextWithErrorMarks), "\r\n\r\n");
			}
		}

		ZString SourceMessageTextWithErrorMarks
		{
			get
			{
				if (sourceMessageTextWithErrorMarks == null && !IsTransmitMessage)
				{
					var provider = ObjectFactory.New<Integration.Customs.US.eManifest.ISyntaxErrorsProvider>();
					sourceMessageTextWithErrorMarks = provider.GetSourceMessageTextWithErrorMarks(this);
				}
				return sourceMessageTextWithErrorMarks;
			}
		}

		string sourceMessageTextWithErrorMarks;

		#region SyntaxErrorMessageStreamFormatter

		class SyntaxErrorMessageStreamFormatter : EDIMessageStreamFormatter
		{
			public SyntaxErrorMessageStreamFormatter(string header, string delimiter)
			{
				this.header = header;
				this.delimiter = delimiter;
			}

			public override void FormatStream(ref Stream stream)
			{
				base.FormatStream(ref stream);
				stream = stream.AddHeader(header + delimiter);
			}

			readonly string header;
			readonly string delimiter;
		}

		#endregion

		#endregion
	}
}
