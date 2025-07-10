using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Forwarding.GUI
{
	public class BusinessObjectMergerHelper<T> where T : BusinessObject
	{
		public BusinessObjectMergerHelper(BusinessObjectMerger<T> merger, Form hostForm)
		{
			this.merger = merger;
			this.hostForm = hostForm;
		}

		readonly BusinessObjectMerger<T> merger;
		readonly Form hostForm;

		public void Merge()
		{
			var checkCanMergeResult = merger.CheckMerger();
			if (string.IsNullOrEmpty(checkCanMergeResult))
			{
				using (var form = new ProgressForm())
				{
					form.ShowCancelButton = false;
					form.ShowProgressBar = false;
					form.Status = Res.GetString("Forwarding|MergerProgressForm|MergerIsInProgress", "Merger is in progress....");

					if (hostForm != null)
					{
						ZFormModaliser.Show(form, hostForm);
					}
					else
					{
						form.Show();
					}

					merger.DoMerge();
				}
			}
			else
			{
				var errorMessage = Res.GetString("46f7c715-fe0f-4cf4-8b9d-14cbb02e545b",
					"Selected items cannot be merged.{0}{0}{1}",
					System.Environment.NewLine,
					checkCanMergeResult);

				Globals.Message.ShowError(errorMessage);
			}
		}
	}
}
