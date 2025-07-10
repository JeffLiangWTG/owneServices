using Enterprise.Customs.Common;

namespace Enterprise.Customs.GUI.PlugIn
{
	public abstract class CustomsManifestPlugIn : CustomsPlugIn
	{
		protected CustomsManifestPlugIn(IManifestProvider hostBusinessEntity) : base(hostBusinessEntity)
		{
			ManifestProvider = hostBusinessEntity;
			if (ManifestProvider != null)
			{
				ManifestProvider.CustomsManifestVisibilityChanged += OnChangeTheVisibilityRequired;
				ChangeTheVisibility();
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (ManifestProvider != null)
				{
					ManifestProvider.CustomsManifestVisibilityChanged -= OnChangeTheVisibilityRequired;
				}
			}

			base.Dispose(disposing);
		}

		#region Implementation

		protected readonly IManifestProvider ManifestProvider;

		#endregion
	}
}
