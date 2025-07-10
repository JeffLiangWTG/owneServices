using System;

namespace Enterprise.Customs.Business
{
	public class DeclarationValueChangedAnnouncer : IDisposable, IInvoicesProviderValueChangedAnnouncer
	{
		public DeclarationValueChangedAnnouncer(BaseJobDeclaration declaration)
		{
			this.declaration = declaration;
			declaration.JE_ApplicationCodeInfo.ValueChanged += new EventHandler(DeclarationValueChanged);
			declaration.JE_MessageTypeInfo.ValueChanged += new EventHandler(DeclarationValueChanged);
			declaration.JE_MessageSubTypeInfo.ValueChanged += new EventHandler(DeclarationValueChanged);
			declaration.JE_TransportModeInfo.ValueChanged += new EventHandler(DeclarationValueChanged);
			declaration.JE_ExportDateInfo.ValueChanged += new EventHandler(DeclarationValueChanged);
			declaration.JE_ContainerModeInfo.ValueChanged += new EventHandler(DeclarationValueChanged);
			declaration.JE_DateOfFirstArrivalInfo.ValueChanged += new EventHandler(DeclarationValueChanged);
		}

		protected readonly BaseJobDeclaration declaration;

		public event EventHandler OnValueChanged;

		protected void DeclarationValueChanged(object sender, EventArgs e)
		{
			if (OnValueChanged != null)
			{
				OnValueChanged(sender, e);
			}
		}

		#region IDisposable Members

		public virtual void Dispose()
		{
			declaration.JE_ApplicationCodeInfo.ValueChanged -= new EventHandler(DeclarationValueChanged);
			declaration.JE_MessageTypeInfo.ValueChanged -= new EventHandler(DeclarationValueChanged);
			declaration.JE_MessageSubTypeInfo.ValueChanged -= new EventHandler(DeclarationValueChanged);
			declaration.JE_TransportModeInfo.ValueChanged -= new EventHandler(DeclarationValueChanged);
			declaration.JE_ExportDateInfo.ValueChanged -= new EventHandler(DeclarationValueChanged);
			declaration.JE_ContainerModeInfo.ValueChanged -= new EventHandler(DeclarationValueChanged);
			declaration.JE_DateOfFirstArrivalInfo.ValueChanged -= new EventHandler(DeclarationValueChanged);
		}

		#endregion
	}
}
