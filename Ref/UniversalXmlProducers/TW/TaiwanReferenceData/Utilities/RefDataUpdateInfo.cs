using System.Text;
using CargoWise.RefDbRepo.TaiwanReferenceData.TurnkeyPlugInUpdateService;

namespace CargoWise.RefDbRepo.TaiwanReferenceData
{
	public abstract class RefDataUpdateInfo
	{
		protected updateInfoBean Info { get; private set; }
		protected string Filename { get; private set; }

		public delegate void OnBeforeExecuteHandler();
		public event OnBeforeExecuteHandler BeforeExecuteEvent;

		protected RefDataUpdateInfo(updateInfoBean info, string filename)
		{
			Info = info;
			Filename = filename;
		}

		protected abstract void Execute();

		public void Run()
		{
			Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
			BeforeExecuteEvent?.Invoke();
			Execute();
		}
	}

	public enum TradeGroup
	{
		FTA,
		LDC,
		SPE,
		WTO,
		ALL
	}

	public enum Preference
	{
		STD,
		PR1,
		PR2,
		PT1,
		PT2,
		PT3,
	}
}
