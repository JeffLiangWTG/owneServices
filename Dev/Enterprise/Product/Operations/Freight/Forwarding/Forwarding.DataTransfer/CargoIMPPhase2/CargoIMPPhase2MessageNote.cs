using System.IO;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Freight.Forwarding.DataTransfer
{
	class CargoIMPPhase2MessageNote : HiddenNote
	{
		public CargoIMPPhase2MessageNote(ForwardingShipment shipment)
			: base(shipment)
		{
		}

		public ZString LoadFromNote()
		{
			ZString result = ZString.Empty;
			if (!IsEmpty)
			{
				MemoryStream memoryStream = null;
				try
				{
					memoryStream = new MemoryStream((byte[])Note.ST_NoteData);
					using (TextReader reader = new StreamReader(memoryStream))
					{
						result = reader.ReadToEnd();
					}
				}
				finally
				{
					if (memoryStream != null)
					{
						memoryStream.Dispose();
					}
				}
			}

			return result;
		}

		public void WriteToNote(ZString text)
		{
			if (!text.IsEmpty)
			{
				MemoryStream memoryStream = null;
				try
				{
					memoryStream = new MemoryStream();
					using (TextWriter writer = new StreamWriter(memoryStream, System.Text.Encoding.UTF8))
					{
						writer.Write(text);
						writer.Flush();
						Note.ST_NoteData = new ZBlob(memoryStream.ToArray());
					}
				}
				finally
				{
					if (memoryStream != null)
					{
						memoryStream.Dispose();
					}
				}
			}
			else
			{
				Note.ST_NoteData = new ZBlob();
			}
		}

		protected override ZString Description
		{
			get { return "CargoIMPPhase2MessageNote"; }
		}

		protected override bool IsEmpty
		{
			get { return (!HasNote || Note.ST_NoteData.IsEmpty); }
		}
	}
}
