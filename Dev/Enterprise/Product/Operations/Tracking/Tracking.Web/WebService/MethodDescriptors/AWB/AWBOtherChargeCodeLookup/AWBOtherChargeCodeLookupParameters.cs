using System;
using Enterprise.ZArchitecture.Web.ServerServices;

namespace Enterprise.Tracking.Web.ServerServices
{
	public class AWBOtherChargeCodeLookupParameters : WebServiceParameters
	{
		#region Properties

		public string CodeValue { get; set; }
		public string CodeControlID { get; set; }
		public string DescriptionControlID { get; set; }
		public string EntitlementCodeControlID { get; set; }
		public string SessionIndex { get; set; }
		public string ChargePK { get; set; }
		public string PrepaidCollectFlag { get; set; }

		#endregion

		#region Override

		protected override void ValidateCore()
		{
			base.ValidateCore();
			if (string.IsNullOrEmpty(SessionIndex))
			{
				throw new ArgumentNullException("SessionIndex");
			}
			if (string.IsNullOrEmpty(ChargePK))
			{
				throw new ArgumentNullException("ChargePK");
			}
			if (string.IsNullOrEmpty(CodeControlID))
			{
				throw new ArgumentNullException("CodeControlID");
			}
			if (string.IsNullOrEmpty(DescriptionControlID))
			{
				throw new ArgumentNullException("DescriptionControlID");
			}
			if (string.IsNullOrEmpty(EntitlementCodeControlID))
			{
				throw new ArgumentNullException("EntitlementCodeControlID");
			}
		}

		#endregion
	}
}
