using CargoWise.Types;

namespace Enterprise.Freight.Forwarding.DataTransfer
{
	public class SterlingOrderReference : SterlingRecord
	{
		public SterlingOrderReference()
		{
		}

		#region Record

		#region Header

		public override ZString RecordHeader
		{
			get
			{
				return "ORF";
			}
		}

		#endregion

		#region Generate

		public override void GenerateRecord()
		{
			AddField(OrderReference);
			TerminateRecord();
		}

		#endregion

		#endregion

		#region Update

		internal void UpdateFields(ZString refToAdd)
		{
			fOrderReference = refToAdd;
		}

		#endregion

		#region Properties

		#region OrderReference

		public ZString OrderReference
		{
			get
			{
				return fOrderReference;
			}
		}
		ZString fOrderReference;

		#endregion

		#endregion
	}
}
