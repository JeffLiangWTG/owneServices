namespace Enterprise.Customs.Business
{
	public interface ISynchroniser
	{
		/// <summary>
		/// Enable/Disable all hooked events and child synchronisers.
		/// </summary>
		void SetEnabled(bool enabled, bool detectEnabled);

		bool IsEnabled { get; }

		/// <summary>
		/// Synchronise if the synchroniser is enabled.
		/// </summary>
		void Synchronise();

		/// <summary>
		/// Force synchronisation.
		/// </summary>
		/// <param name="force">if true - enables synchroniser and force synchronisation.</param>
		void Synchronise(bool force);

		/// <summary>
		/// Use by Universal Shipment XML to determine whether Override Freight Defaults should be unticked
		/// </summary>
		bool SyncChangesDetected { get; }

		/// <summary>
		/// Set this to true to detech whether running synchroniser will change any data; it should not change any data; just detect.
		/// </summary>
		bool DetectEnabled { get; set; }
	}
}
