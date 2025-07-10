using CargoWise.ComponentModel;
using Enterprise.Freight.Business;

namespace Enterprise.Freight.CFS.Business
{
	public class CFSDocsAndCartageValidation : JobDocsAndCartageValidation
	{
		public CFSDocsAndCartageValidation(AutoJobDocsAndCartage parent)
			: base(parent)
		{
		}

		#region Parent

		public new CFSDocsAndCartage Parent
		{
			get { return (CFSDocsAndCartage)base.Parent; }
		}

		#endregion

		#region JP_LCLStorageCommences

		protected override void CheckJP_LCLStorageCommences()
		{
			base.CheckJP_LCLStorageCommences();

			if (!Parent.JP_LCLStorageCommencesInfo.HasErrors())
			{
				if (Parent.JP_LCLAvailable.IsValid
					&& !Parent.JP_LCLStorageCommences.IsEmpty && !Parent.JP_LCLAvailable.IsEmpty
					&& Parent.JP_LCLStorageCommences < Parent.JP_LCLAvailable)
				{
					Parent.JP_LCLStorageCommencesInfo.AddError(Res.GetString("44229cd5-c079-4965-a8e5-df5802c90da7", "Please enter a date later than or equal to the Shipment CFS Available date or remove the CFS Available date for this Shipment"));
				}
			}
		}

		#endregion
	}
}
