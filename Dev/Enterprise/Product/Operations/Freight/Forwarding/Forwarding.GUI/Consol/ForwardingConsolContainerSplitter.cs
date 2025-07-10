using System;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Freight.Forwarding.GUI
{
	public class ForwardingConsolContainerSplitter
	{
		public void Split(ForwardingContainer containerToSplit)
		{
			if (containerToSplit != null)
			{
				using (ForwardingContainerSplitter splitter = new ForwardingContainerSplitter(containerToSplit))
				{
					splitter.SplitMethodProvider = containerToSplit.PackLines.Count > 0 ?
						new Func<ForwardingContainerSplitter.SplitMethod>(GetSplitMethodFromUser) : () => ForwardingContainerSplitter.SplitMethod.PackAllIntoFirstContainer;

					ZString result = splitter.Split();

					if (result != ZString.Empty)
					{
						Globals.Message.ShowError(result);
					}
				}
			}
			else
			{
				Globals.Message.ShowError(Res.GetString("cca37eff-4f13-49db-86af-cfb6473f2e52", "Please select container"));
			}
		}

		ForwardingContainerSplitter.SplitMethod GetSplitMethodFromUser()
		{
			DialogResult dialogResult = Globals.Message.Show(Res.GetString("84e10233-eeb1-4f4a-ad9c-f739f9f21b34", "These containers have shipment/s packed into them, would you like to pack the shipment/s to each of these containers?"),
			   Res.GetString("d45a29cb-d0f5-48c6-8a30-e9915f24a20d", "Question"), MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);

			switch (dialogResult)
			{
				case DialogResult.Yes:
					return ForwardingContainerSplitter.SplitMethod.DistributeEvenly;
				case DialogResult.No:
					return ForwardingContainerSplitter.SplitMethod.PackAllIntoFirstContainer;
				default:
					return ForwardingContainerSplitter.SplitMethod.None;
			}
		}
	}
}
