using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Enterprise.MasterFiles.GUI.WebAddressValidation
{
	public abstract class SuggestionControlUtil<SuggestionControl, ServiceResult> where SuggestionControl : Control, ISuggestionControl
	{
		protected readonly ISupportWebAddressValidationControl validationControl;
		protected readonly Control parentControl;
		protected readonly Form parentForm;
		protected readonly CancellationTokenSource cancellationTokenSource;
		protected readonly AddressValidatorConfiguration config;
		protected readonly string suggestionControlName;

		protected SuggestionControlUtil(Control parentControl, Form parentForm, CancellationTokenSource cancellationTokenSource, AddressValidatorConfiguration config, string suggestionControlName)
		{
			this.parentControl = parentControl ?? throw new ArgumentNullException(nameof(parentControl));
			this.parentForm = parentForm ?? throw new ArgumentNullException(nameof(parentForm));
			this.cancellationTokenSource = cancellationTokenSource ?? throw new ArgumentNullException(nameof(cancellationTokenSource));
			this.config = config ?? throw new ArgumentNullException(nameof(config));
			this.suggestionControlName = suggestionControlName ?? throw new ArgumentNullException(nameof(suggestionControlName));

			validationControl = parentControl as ISupportWebAddressValidationControl ?? throw new ArgumentException(null, nameof(parentControl));
			if (validationControl.SuggestionWindowParentControl is null)
			{
				throw new ArgumentException("SuggestionWindowParentControl can not be null");
			}
		}

		public SuggestionControl FindSuggestionControl() => validationControl.SuggestionWindowParentControl.Controls.Find(suggestionControlName, searchAllChildren: true).SingleOrDefault() as SuggestionControl;

		public void CloseSuggestionControlIfExists() => FindSuggestionControl()?.Close();

		public abstract void RegisterPropertyChangedEvent(Func<Task> callService);

		protected abstract SuggestionControl CreateNewSuggestionControl(ServiceResult serviceResult);

		protected void CreateAndShowSuggestionControl(ServiceResult serviceResult)
		{
			if (parentControl.IsDisposed)
			{
				return;
			}

			var suggestionControl = CreateNewSuggestionControl(serviceResult);

#if !WINZOR
			HookTextControls(suggestionControl);
#endif

			validationControl.SuggestionWindowParentControl.Controls.Add(suggestionControl);
			suggestionControl.BringToFront();
		}

#if !WINZOR
		#region HookTextControl

		protected abstract TextBox[] TextBoxControls { get; }

		protected virtual (Action HookAction, Action UnHookAction) GetHookActions(TextBox textControl, SuggestionControl suggestionControl)
		{
			var previewKeyDownHandler = new PreviewKeyDownEventHandler((x, y) =>
			{
				switch (y.KeyCode)
				{
					case Keys.Return:
					case Keys.Escape:
					case Keys.Down:
					case Keys.Up:
						y.IsInputKey = true;
						break;
				}
			});

			var keyDownHandler = new KeyEventHandler((x, y) =>
			{
				switch (y.KeyCode)
				{
					case Keys.Return:
						suggestionControl.SelectAddressAndClose();
						y.Handled = true;
						break;
					case Keys.Escape:
						suggestionControl.Close();
						y.Handled = true;
						break;
					case Keys.Down:
						suggestionControl.MoveSelection(1);
						y.Handled = true;
						break;
					case Keys.Up:
						suggestionControl.MoveSelection(-1);
						y.Handled = true;
						break;
				}
			});

			var hookAction = new Action(() =>
			{
				textControl.PreviewKeyDown += previewKeyDownHandler;
				textControl.KeyDown += keyDownHandler;
			});

			var unHookAction = new Action(() =>
			{
				textControl.PreviewKeyDown -= previewKeyDownHandler;
				textControl.KeyDown -= keyDownHandler;
			});

			return (hookAction, unHookAction);
		}

		void HookTextControls(SuggestionControl suggestionControl)
		{
			foreach (var textControl in TextBoxControls)
			{
				if (textControl is null)
				{
					continue;
				}

				var (hook, unHook) = GetHookActions(textControl, suggestionControl);
				hook.Invoke();
				suggestionControl.Disposed += (x, y) => unHook.Invoke();
			}
		}

		#endregion HookTextControl
#endif
	}
}
