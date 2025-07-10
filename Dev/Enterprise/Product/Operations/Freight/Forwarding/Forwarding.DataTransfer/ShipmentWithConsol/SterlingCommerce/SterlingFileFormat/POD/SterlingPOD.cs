using CargoWise.Types;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Freight.Forwarding.DataTransfer
{
	public class SterlingPOD : SterlingRecord
	{
		public SterlingPOD()
		{
		}

		#region Source
		public Xsd.ContainerLeg Source
		{
			get { return fSource; }
			set { fSource = value; }
		}
		Xsd.ContainerLeg fSource;
		#endregion

		#region Record

		#region Header

		public override ZString RecordHeader
		{
			get
			{
				return "POD";
			}
		}

		#endregion

		#region Generate

		public override void GenerateRecord()
		{
			AddField(PODName);
			AddField(LegType);
			TerminateRecord();
		}

		#endregion

		#endregion

		#region Properties

		#region PODName

		public ZString PODName
		{
			get
			{
				return Source.GoodsRecBy;
			}
		}

		#endregion

		#region LegType

		public ZString LegType
		{
			get
			{
				return Source.LegType.ToString();
			}
		}

		#endregion

		#endregion
	}
}
