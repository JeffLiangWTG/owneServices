using System;
using System.Drawing;
using System.Windows.Forms;
using Enterprise.MasterData.Common;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.GUI
{
	public interface ISupportDuplicationAlertControl
	{
		Point DuplicationAlertAnchorLocation { get; }

		Control DuplicationAlertParentControl { get; }

		Control DuplicationAlertReferenceControl { get; }

		string DeduplicationStatusText { get; }

		bool DeduplicationStatusVisible { get; }

		DuplicateAlertControlHelper DeduplicationHelper { get; }

		void DuplicationStarted(object sender, EventArgs e);

		void DuplicationEnded(object sender, IDuplicationEventArgs e);

		void ShowDuplications(IDuplicationEventArgs e);

		void DuplicationDetected(object sender, IDuplicationEventArgs e);

		void ShowDeduplicationTimeoutMessage();

		void ShowDeduplicationErrorOccurredMessage();

		void ShowExcludedDuplicationMessage();

		void SetDuplicateDetectionStatusLabel(ResourceString text, Color foreColor);

		void ShowNoDuplicatesFound(IDuplicationEventArgs duplicationEventArgs);

		void ShowNotEnoughInformation();

		void HideDeduplicationStatus();

		void DuplicateDetectionStatusLabelOnClick(object sender, EventArgs eventArgs);

		void DuplicateDetectionStatusLabelOnMouseLeave(object sender, EventArgs e);

		void DuplicateDetectionStatusLabelOnMouseEnter(object sender, EventArgs eventArgs);

		void ShowDuplicatesFound(IDuplicationEventArgs duplicationEventArgs);

		void DeduplicationActionOccurred(object sender, IDuplicationEventArgs e);

		void ShowDeduplicationStatus();

		void UnHookDuplicationDetectEvents();

		void HookDuplicationDetectEvents();
	}
}
