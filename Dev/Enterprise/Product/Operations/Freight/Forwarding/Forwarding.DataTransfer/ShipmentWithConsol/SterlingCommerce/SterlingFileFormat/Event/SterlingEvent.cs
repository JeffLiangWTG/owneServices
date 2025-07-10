using CargoWise.Types;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Freight.Forwarding.DataTransfer
{
	public class SterlingEvent : SterlingRecord
	{
		public SterlingEvent()
		{
		}

		public Xsd.Event SourceEvent
		{
			get
			{
				return fSourceEvent;
			}
			set
			{
				fSourceEvent = value;
			}
		}
		Xsd.Event fSourceEvent;

		#region Record

		#region Header

		public override ZString RecordHeader
		{
			get
			{
				return "EVT";
			}
		}

		#endregion

		#region Generate

		public override void GenerateRecord()
		{
			AddField(Source);
			AddField(Code);
			AddField(CodeDescription);
			AddField(DateTime);
			AddField(PostedDateTime);
			AddField(Information);
			AddField(User);
			AddField(IsEstimatedDate);
			TerminateRecord();
		}

		#endregion

		#endregion

		#region Properties

		#region Source

		public ZString Source
		{
			get
			{
				return SourceEvent.Source;
			}
		}

		#endregion

		#region Code

		public ZString Code
		{
			get
			{
				return SourceEvent.Code;
			}
		}

		#endregion

		#region CodeDescription

		public ZString CodeDescription
		{
			get
			{
				return SourceEvent.CodeDescription;
			}
		}

		#endregion

		#region DateTime

		public ZString DateTime
		{
			get
			{
				return ToTimeFormat(SourceEvent.DateTime);
			}
		}

		#endregion

		#region PostedDateTime

		public ZString PostedDateTime
		{
			get
			{
				return ToTimeFormat(SourceEvent.PostedDateTime);
			}
		}

		#endregion

		#region Information

		public ZString Information
		{
			get
			{
				return SourceEvent.Information;
			}
		}

		#endregion

		#region User

		public ZString User
		{
			get
			{
				return SourceEvent.User;
			}
		}

		#endregion

		#region IsEstimatedDate

		public ZString IsEstimatedDate
		{
			get
			{
				return SourceEvent.IsEstimatedDate.ToString();
			}
		}

		#endregion

		#endregion

	}
}
