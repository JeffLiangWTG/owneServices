namespace Enterprise.Freight.Agency.Business
{
	public class NZPortMessageLookups : PortMessageLookups
	{
		public NZPortMessageLookups(NZPortMessage parent)
			: base(parent) { }

		#region Port_List

		public override PortMessageTargetPortList Port_List
		{
			get
			{
				if (portList == null)
				{
					portList = new NZPortMessageTargetPortList(Parent.Voyage);
					portList.Load();
				}

				return portList;
			}
		}

		PortMessageTargetPortList portList;

		#endregion
	}
}
