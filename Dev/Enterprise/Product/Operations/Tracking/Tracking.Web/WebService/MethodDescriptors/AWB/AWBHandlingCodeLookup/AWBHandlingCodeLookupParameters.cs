using System;
using Enterprise.ZArchitecture.Web.ServerServices;

namespace Enterprise.Tracking.Web.ServerServices
{
	public class AWBHandlingCodeLookupParameters : WebServiceParameters
	{
		#region Properties

		public string CodeValue { get; set; }
		public string CodeControlID { get; set; }
		public string DescriptionControlID { get; set; }

		#endregion

		#region Override

		protected override void ValidateCore()
		{
			base.ValidateCore();
			if (string.IsNullOrEmpty(CodeControlID))
			{
				throw new ArgumentNullException("CodeControlID");
			}
			if (string.IsNullOrEmpty(DescriptionControlID))
			{
				throw new ArgumentNullException("DescriptionControlID");
			}
		}

		#endregion
	}
}
