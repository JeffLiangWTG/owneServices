using CargoWise.Types;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Freight.Forwarding.DataTransfer
{
	public class SterlingNote : SterlingRecord
	{
		public SterlingNote()
		{
		}

		#region Source
		public Xsd.NotesNote Source
		{
			get
			{
				return fSource;
			}
			set
			{
				fSource = value;
			}
		}
		Xsd.NotesNote fSource;
		#endregion

		#region Record

		#region Header

		public override ZString RecordHeader
		{
			get
			{
				return "NTS";
			}
		}

		#endregion

		#region Generate

		public override void GenerateRecord()
		{
			AddField(NoteType);
			AddField(NoteData);
			AddField(NoteCreatedDateTime);
			TerminateRecord();
		}

		#endregion

		#endregion

		#region NoteType

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard-coded constant.")]
		ZString GetNoteType(string p)
		{
			return p == "Custom" ? "CustomNote" : p;
		}

		#endregion

		#region Properties

		#region NoteType

		public ZString NoteType
		{
			get
			{
				return GetNoteType(Source.NoteType.ToString());
			}
		}

		#endregion

		#region NoteData

		public ZString NoteData
		{
			get
			{
				return Source.NoteData;
			}
		}

		#endregion

		#region NoteCreatedDateTime

		public ZString NoteCreatedDateTime
		{
			get
			{
				return ToTimeFormat(Source.NoteCreatedDateTime);
			}
		}

		#endregion

		#endregion

	}
}
