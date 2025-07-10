using CargoWise.Types;

namespace Enterprise.Freight.Business
{
	public class UnAllocatedPackLinesForSailing : UnAllocatedPackLinesView
	{
		public UnAllocatedPackLinesForSailing(PackLinesForSailingsCollection packLines, JobSailing sailing) : base(packLines)
		{
			fSailing = sailing;
			Rebuild();
		}

		#region ShouldIncludeThisPackLine

		protected override bool ShouldIncludeThisPackLine(PackLine line)
		{
			bool result = base.ShouldIncludeThisPackLine(line);

			CommonShipment shipment = GetShipmentFromLine(line);

			if (shipment != null && shipment.Sailing != null && Sailing != null && !Sailing.IsDeleted)
			{
				if (result && ShowOnlyNonTranship)
				{
					result = shipment.Sailing.JX_JB_RL_NKPortOfDischarge == Sailing.JX_JB_RL_NKPortOfDischarge;
					if (ShowOnlyThisSailing)
					{
						result = shipment.Sailing.Voyage.PK == Sailing.Voyage.PK;
					}
				}
			}
			return result;
		}

		#endregion

		#region IsPacked

		protected override bool IsPacked(PackLine line)
		{
			return (line.GetContainer(fSailing) != null);
		}

		#endregion

		#region Sailing

		readonly JobSailing fSailing;
		public JobSailing Sailing
		{
			get { return fSailing; }
		}

		#endregion

		#region ShowOnlyThisSailing

		ZBool fShowOnlyThisSailing = true;
		public ZBool ShowOnlyThisSailing
		{
			get { return fShowOnlyThisSailing; }
			set
			{
				fShowOnlyThisSailing = value;
				Rebuild();
			}
		}

		#endregion

		#region ShowOnlyNonTranship

		ZBool fShowOnlyNonTranship = true;
		public ZBool ShowOnlyNonTranship
		{
			get { return fShowOnlyNonTranship; }
			set
			{
				fShowOnlyNonTranship = value;
				Rebuild();
			}
		}

		#endregion
	}
}
