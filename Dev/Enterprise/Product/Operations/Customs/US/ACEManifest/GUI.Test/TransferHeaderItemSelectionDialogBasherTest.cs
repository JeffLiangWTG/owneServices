using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.US.ACEManifest.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.ACEManifest.GUI.Testing
{
	[TestedType(typeof(TransferHeaderItemSelectionDialog))]
	class TransferHeaderItemSelectionDialogBasherTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			var header = Factory.New<Business.AsycudaManifestHeader>();
			header.AMA_ManifestType = "IMP";
			var arrivalHeader = header.ArrivalHeaders.AddNew();
			var transferHeader = arrivalHeader.TransferHeaders.AddNew();
			transferHeader.ATF_TransferType = ManifestBase.TransferTypeList.Codes.Domestic;
			var transferHeaderSelectionItem = new TransferHeaderSelectionItem(transferHeader);
			var chooser = new TransferHeaderMessageChooser(header, new ISelectionItem[] { transferHeaderSelectionItem }, true, true);
			var result = new TransferHeaderItemSelectionDialog(chooser, "Arrival Header");
			((IBusinessObjectState)result.BusinessEntity).ClearHasChangesIncludingChildren();
			return result;
		}
	}
}
