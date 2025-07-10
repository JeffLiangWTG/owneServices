using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.GUI.WebAddressValidation;
using Moq;

namespace Enterprise.MasterFiles.GUI.Test
{
	public class SuggestionControlUtilTest : TestCaseWithFactory
	{
		public void TestExceptionThrownWhenArgumentForConstructorIsIncorrect()
		{
			using (var controlNotSupportAVS = new Control())
			using (var parentControl = new ControlSupportAddressValidationForTest())
			using (var parentForm = new Form())
			using (var cancellationTokenSource = new CancellationTokenSource())
			{
				var config = new AddressValidatorConfiguration();

				AssertNoExceptionThrown(() => new SuggestionControlUtilForTest(parentControl, parentForm, cancellationTokenSource, config));

				AssertExceptionThrown<ArgumentException>(() => new SuggestionControlUtilForTest(controlNotSupportAVS, parentForm, cancellationTokenSource, config));

				AssertExceptionThrown<ArgumentNullException>(() => new SuggestionControlUtilForTest(null, parentForm, cancellationTokenSource, config));
				AssertExceptionThrown<ArgumentNullException>(() => new SuggestionControlUtilForTest(parentControl, null, cancellationTokenSource, config));
				AssertExceptionThrown<ArgumentNullException>(() => new SuggestionControlUtilForTest(parentControl, parentForm, null, config));
				AssertExceptionThrown<ArgumentNullException>(() => new SuggestionControlUtilForTest(parentControl, parentForm, cancellationTokenSource, null));

				var controlMock = new Mock<Control>();
				var validationControlMock = controlMock.As<ISupportWebAddressValidationControl>();
				validationControlMock.Setup(x => x.SuggestionWindowParentControl).Returns(controlMock.Object);
				AssertNoExceptionThrown(() => new SuggestionControlUtilForTest(controlMock.Object, parentForm, cancellationTokenSource, config));
				validationControlMock.Setup(x => x.SuggestionWindowParentControl).Returns(value: null);
				AssertExceptionThrown<ArgumentException>("SuggestionWindowParentControl can not be null", () => new SuggestionControlUtilForTest(controlMock.Object, parentForm, cancellationTokenSource, config));
			}
		}

		public void TestFindAndCloseSuggestionControl()
		{
			using (var parentControl = new ControlSupportAddressValidationForTest())
			using (var parentForm = new Form())
			using (var cancellationTokenSource = new CancellationTokenSource())
			{
				var suggestionControlUtil = new SuggestionControlUtilForTest(parentControl, parentForm, cancellationTokenSource, new AddressValidatorConfiguration());

				suggestionControlUtil.CreateAndShowSuggestionControl();

				var suggestionControl = suggestionControlUtil.FindSuggestionControl();
				AssertNotNull(suggestionControl);

				suggestionControlUtil.CloseSuggestionControlIfExists();
				AssertNull(suggestionControlUtil.FindSuggestionControl());
				Assert(suggestionControl.IsDisposed);

				AssertNoExceptionThrown(suggestionControlUtil.CloseSuggestionControlIfExists);
			}
		}

		public void TestShowSuggestionControl()
		{
			using (var parentControl = new ControlSupportAddressValidationForTest())
			using (var parentForm = new Form())
			using (var cancellationTokenSource = new CancellationTokenSource())
			{
				var suggestionControlUtil = new SuggestionControlUtilForTest(parentControl, parentForm, cancellationTokenSource, new AddressValidatorConfiguration());

				AssertNull("Precondition", suggestionControlUtil.FindSuggestionControl());

				suggestionControlUtil.CreateAndShowSuggestionControl();

				AssertNotNull(suggestionControlUtil.FindSuggestionControl());
				AssertEquals(0, parentControl.Controls.GetChildIndex(suggestionControlUtil.FindSuggestionControl()));
			}
		}

		public void TestDoNotShowSuggestionControl()
		{
			using (var parentForm = new Form())
			using (var cancellationTokenSource = new CancellationTokenSource())
			{
				var parentControl = new ControlSupportAddressValidationForTest();

				var suggestionControlUtil = new SuggestionControlUtilForTest(parentControl, parentForm, cancellationTokenSource, new AddressValidatorConfiguration());

				parentControl.Dispose();
				AssertNoExceptionThrown(suggestionControlUtil.CreateAndShowSuggestionControl);

				AssertNull(suggestionControlUtil.FindSuggestionControl());
			}
		}

#if !WINZOR
		public void TestHookTextControls()
		{
			using (var parentControl = new ControlSupportAddressValidationForTest())
			using (var parentForm = new Form())
			using (var cancellationTokenSource = new CancellationTokenSource())
			{
				var suggestionControlUtil = new SuggestionControlUtilForTest(parentControl, parentForm, cancellationTokenSource, new AddressValidatorConfiguration());

				suggestionControlUtil.CreateAndShowSuggestionControl();
				Assert(suggestionControlUtil.Hooked);

				suggestionControlUtil.FindSuggestionControl().Dispose();
				Assert(suggestionControlUtil.UnHooked);
			}
		}
#endif
	}

	class SuggestionControlUtilForTest : SuggestionControlUtil<SuggestionControlForTest, object>
	{
		public SuggestionControlUtilForTest(Control parentControl, Form parentForm, CancellationTokenSource cancellationTokenSource, AddressValidatorConfiguration config)
			: base(parentControl, parentForm, cancellationTokenSource, config, "SuggestionControlForTest")
		{
		}

		public void CreateAndShowSuggestionControl() => CreateAndShowSuggestionControl(null);

		public override void RegisterPropertyChangedEvent(Func<Task> callService) { }

		protected override SuggestionControlForTest CreateNewSuggestionControl(object serviceResult) => new SuggestionControlForTest();

#if !WINZOR
		#region HookTextControl

		public bool Hooked { get; private set; }
		public bool UnHooked { get; private set; }

		TextBox TextBoxForTest { get; } = new TextBox();

		protected override TextBox[] TextBoxControls => new[] { TextBoxForTest };

		protected override (Action HookAction, Action UnHookAction) GetHookActions(TextBox textControl, SuggestionControlForTest suggestionControl)
		{
			return (() => Hooked = true, () => UnHooked = true);
		}

		#endregion HookTextControl
#endif
	}

	class SuggestionControlForTest : Control, ISuggestionControl
	{
		public SuggestionControlForTest()
		{
			Name = "SuggestionControlForTest";
		}

		public void Close()
		{
			Parent?.Controls.Remove(this);
			Dispose();
		}

		public void MoveSelection(int amountToMove) { }
		public void SelectAddressAndClose() { }
	}
}
