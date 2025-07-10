using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Linq.Expressions;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Controls.Extensions;
using NUnit.Framework;

namespace Enterprise.Customs.GUI.Testing
{
	public readonly struct UserControlAssertStrategies<TControl>
		where TControl : Control
	{
		ICollection<IUserControlAssertStrategy<TControl>> Strategies { get; } = new List<IUserControlAssertStrategy<TControl>>();

		public UserControlAssertStrategies() { }

		internal void Run(TControl control, string controlName)
		{
			foreach (var strategy in Strategies)
			{
				strategy.Run(control, controlName);
			}
		}

		public UserControlAssertStrategies<TControl> WithStrategy<TExpected>(string name, TExpected expected, UserControlAssertStrategyDelegate<TControl, TExpected> callback) =>
			WithStrategy(new UserControlAssertStrategy<TControl, TExpected>(name, expected, callback));

		public UserControlAssertStrategies<TControl> WithStrategy<TExpected>(UserControlAssertStrategy<TControl, TExpected> strategy)
		{
			Strategies.Add(strategy);
			return this;
		}

		public UserControlAssertStrategies<TControl> WithoutStrategy(string name)
		{
			var strategies = Strategies;
			foreach (var strategyToDelete in strategies.Where(x => x.Name == name).ToArray())
			{
				strategies.Remove(strategyToDelete);
			}
			return this;
		}

		public UserControlAssertStrategies<TControl> WithBindingSource<T>()
		{
			return WithStrategy("BindingSource", typeof(T), (self, control, controlName) =>
			{
				if (control is ZUserControl userControl)
				{
					Assertion.AssertEquals($"Control {controlName} {self.name}", self.expected, userControl.BindingSource.DataSourceType);
				}
				else
				{
					Assertion.Fail($"Control {controlName} {self.name} should be assignable to:\n" +
						$"- {nameof(ZUserControl)}");
				}
			});
		}

		public UserControlAssertStrategies<TControl> WithBindTo(string bindTo) => WithStrategy("BindTo", bindTo, (self, control, controlName)
			=> Assertion.AssertEquals($"Control {controlName} {self.name}", self.expected, control.GetBindingMember()));

		public UserControlAssertStrategies<TControl> WithBindToAmount(string bindTo)
		{
			return WithStrategy("BindToAmount", bindTo, (self, control, controlName) =>
			{
				if (control is ZCalcFindBox calcFindBox)
				{
					Assertion.AssertEquals($"Control {controlName} {self.name}", self.expected, calcFindBox.BindToAmount);
				}
				else if (control is ZCalcDropEdit calcDropEdit)
				{
					Assertion.AssertEquals($"Control {controlName} {self.name}", self.expected, calcDropEdit.BindToAmount);
				}
				else
				{
					Assertion.Fail($"Control {controlName} {self.name} should be assignable to any of:\n" +
						$"- {nameof(ZCalcFindBox)} (Such as {nameof(ConvertToLocalCurrencyControl)})\n" +
						$"- {nameof(ZCalcDropEdit)}");
				}
			});
		}

		public UserControlAssertStrategies<TControl> WithBindToUnit(string bindTo)
		{
			return WithStrategy("BindToUnit", bindTo, (self, control, controlName) =>
			{
				if (control is ZCalcFindBox calcFindBox)
				{
					Assertion.AssertEquals($"Control {controlName} {self.name}", self.expected, calcFindBox.BindToUnit);
				}
				else if (control is ZCalcDropEdit calcDropEdit)
				{
					Assertion.AssertEquals($"Control {controlName} {self.name}", self.expected, calcDropEdit.BindToUnit);
				}
				else
				{
					Assertion.Fail($"Control {controlName} {self.name} should be assignable to any of:\n" +
						$"- {nameof(ZCalcFindBox)} (Such as {nameof(ConvertToLocalCurrencyControl)})\n" +
						$"- {nameof(ZCalcDropEdit)}");
				}
			});
		}

		/// <remarks>
		/// <b>NOTE:</b> This strategy is added by default as <code>.WithIsVisible(isVisible: true)</code>
		/// If you wish to not include this assertion you can call <code>.WithoutStrategy(name: "IsVisible")</code>
		/// </remarks>
		public UserControlAssertStrategies<TControl> WithIsVisible(bool isVisible = true)
		{
			WithoutStrategy("IsVisible");
			return WithStrategy("IsVisible", isVisible, (self, control, controlName) =>
			{
				Assertion.AssertEquals($"Control {controlName} {self.name}", self.expected, control.Visible);
			});
		}

		public UserControlAssertStrategies<TControl> WithTabVisible(bool tabVisible = true, bool? isActiveTab = null)
		{
			var strategies = Strategies;
			WithoutStrategy("IsVisible");
			return WithStrategy("TabVisible", tabVisible, (self, control, controlName) =>
			{
				Assertion.AssertCollectionNotContains(
					$"Control {controlName} {self.name} should replace assert strategy IsVisible\n" +
					" - Please ensure that `WithIsVisible` is not called.\n" +
					" - Please use `WithTabVisible(isActiveTab: ...)` if you intend to assert if tab is visible.",
					"IsVisible",
					strategies.Select(strategy => strategy.Name)
				);
				if (isActiveTab is bool isActive)
				{
					Assertion.AssertEquals($"Control {controlName} {self.name} is active tab", isActive, control.Visible);
				}
				if (control is ZTabPage tabPage)
				{
					Assertion.AssertEquals($"Control {controlName} {self.name}", self.expected, tabPage.TabVisible);
				}
				else
				{
					Assertion.Fail($"Control {controlName} {self.name} should be assignable to:\n" +
						$"- {nameof(ZTabPage)}");
				}
			});
		}

		public UserControlAssertStrategies<TControl> WithMultiline(bool multiline = true)
		{
			return WithStrategy("Multiline", multiline, (self, control, controlName) =>
			{
				if (control is ZTextBox textBox)
				{
					Assertion.AssertEquals($"Control {controlName} {self.name}", self.expected, textBox.Multiline);
					if (self.expected)
					{
						Assertion.Assert($"Control {controlName} {self.name} should have a vertical scrollbar", textBox.ScrollBars.HasFlag(ScrollBars.Vertical));
					}
				}
				else
				{
					Assertion.Fail($"Control {controlName} {self.name} should be assignable to:\n" +
						$"- {nameof(ZTextBox)}");
				}
			});
		}

		public UserControlAssertStrategies<TControl> WithReadOnly(bool readOnly = true)
		{
			return WithStrategy("ReadOnly", readOnly, (self, control, controlName) =>
			{
				Assertion.AssertEquals($"Control {controlName} {self.name}", self.expected, control.GetReadOnly());
			});
		}

		public UserControlAssertStrategies<TControl> WithCaptionRenderingEnabled(bool captionRenderingEnabled = true)
		{
			return WithStrategy("CaptionRenderingEnabled", captionRenderingEnabled, (self, control, controlName) =>
			{
				if (control is ICaptionRenderingSupport captionRenderingSupport)
				{
					Assertion.AssertEquals($"Control {controlName} {self.name}", self.expected, captionRenderingSupport.CaptionRenderingEnabled);
				}
				else
				{
					Assertion.Fail($"Control {controlName} {self.name} should be assignable to:\n" +
						$"- {nameof(ICaptionRenderingSupport)} (Such as {nameof(ZUserControl)})");
				}
			});
		}

		public UserControlAssertStrategies<TControl> WithCharacterCasing(CharacterCasing characterCasing)
		{
			return WithStrategy("CharacterCasing", characterCasing, (self, control, controlName) =>
			{
				if (control is ZDropEdit dropEdit)
				{
					Assertion.AssertEquals($"Control {controlName} {self.name}", self.expected, dropEdit.CharacterCasing);
				}
				else if (control is ZTextBox textBox)
				{
					Assertion.AssertEquals($"Control {controlName} {self.name}", self.expected, textBox.CharacterCasing);
				}
				else
				{
					Assertion.Fail($"Control {controlName} {self.name} should be assignable to any of:\n" +
						$"- {nameof(ZDropEdit)}\n" +
						$"- {nameof(ZTextBox)}");
				}
			});
		}

		public UserControlAssertStrategies<TControl> WithPasswordChar(char characterStyle)
		{
			return WithStrategy("PasswordChar", characterStyle, (self, control, controlName) =>
			{
				if (control is ZTextBox textBox)
				{
					Assertion.AssertEquals($"Control {controlName} {self.name}", self.expected, textBox.PasswordChar);
				}
				else
				{
					Assertion.Fail($"Control {controlName} {self.name} should be assignable to:\n" +
						$"- {nameof(IMacroBox)} (Such as {nameof(ZTextBox)})");
				}
			});
		}

		public UserControlAssertStrategies<TControl> WithShouldEscapeAllSpecialCharacters(bool shouldEscapeAllSpecialCharacters = true)
		{
			return WithStrategy("ShouldEscapeAllSpecialCharacters", shouldEscapeAllSpecialCharacters, (self, control, controlName) =>
			{
				if (control is IMacroBox macroBox)
				{
					Assertion.AssertEquals($"Control {controlName} {self.name}", self.expected, macroBox.ShouldEscapeAllSpecialCharacters);
				}
				else
				{
					Assertion.Fail($"Control {controlName} {self.name} should be assignable to:\n" +
						$"- {nameof(IMacroBox)} (Such as {nameof(ZTextBox)})");
				}
			});
		}

		public UserControlAssertStrategies<TControl> WithShowDescriptionBox(bool showDescriptionBox)
		{
			return WithStrategy("ShowDescriptionBox", showDescriptionBox, (self, control, controlName) =>
			{
				if (control is ZCodeFindBox codeFindBox)
				{
					Assertion.AssertEquals($"Control {controlName} {self.name}", self.expected, codeFindBox.ShowDescriptionBox);
				}
				else
				{
					Assertion.Fail($"Control {controlName} {self.name} should be assignable to:\n" +
						$"- {nameof(ZCodeFindBox)}");
				}
			});
		}

		public UserControlAssertStrategies<TControl> WithSizeScaled(int widthScaled, int heightScaled) => WithWidthScaled(widthScaled).WithHeightScaled(heightScaled);

		public UserControlAssertStrategies<TControl> WithSizeUnScaled(int widthUnScaled, int heightUnScaled) => WithWidthUnScaled(widthUnScaled).WithHeightUnScaled(heightUnScaled);

		public UserControlAssertStrategies<TControl> WithHeightScaled(int heightScaled) => WithHeight("Height Scaled", heightScaled);

		public UserControlAssertStrategies<TControl> WithHeightUnScaled(int heightUnScaled) => WithHeight("Height UnScaled", ControlDpiScalingHelper.ScaleToCurrentDpiX(heightUnScaled));

		UserControlAssertStrategies<TControl> WithHeight(string strategyName, int height)
		{
			return WithStrategy(strategyName, height, (self, control, controlName) =>
			{
				Assertion.AssertEquals($"Control {controlName} {self.name}", self.expected, control.Height);
			});
		}

		public UserControlAssertStrategies<TControl> WithWidthScaled(int widthScaled) => WithWidth("Width Scaled", widthScaled);

		public UserControlAssertStrategies<TControl> WithWidthUnScaled(int widthUnScaled) => WithWidth("Width UnScaled", ControlDpiScalingHelper.ScaleToCurrentDpiX(widthUnScaled));

		UserControlAssertStrategies<TControl> WithWidth(string strategyName, int width)
		{
			return WithStrategy(strategyName, width, (self, control, controlName) =>
			{
				Assertion.AssertEquals($"Control {controlName} {self.name}", self.expected, control.Width);
			});
		}

		public UserControlAssertStrategies<TControl> WithCaption(string caption)
		{
			return WithStrategy("Caption", caption, (self, control, controlName) =>
			{
				if (control is IResCaptionedControl { CaptionResourceString.Key: not "" } captions)
				{
					Assertion.AssertEquals($"Control {controlName} {self.name}", self.expected, captions.CaptionResourceString.Caption);
				}
				else if (control.GetExtension<HintExtension>() is HintExtension hintExtension)
				{
					Assertion.AssertEquals($"Control {controlName} {self.name}", self.expected, hintExtension.Caption);
				}
				else
				{
					Assertion.Fail($"Control {controlName} {self.name} should be any of:\n" +
						$"- Bound BO property with [ResourceStringData]\n" +
						$"- Control contains CaptionsResourceString");
				}
			});
		}

		public UserControlAssertStrategies<TControl> WithMediumCaption(string mediumCaption)
		{
			return WithStrategy("Medium Caption", mediumCaption, (self, control, controlName) =>
			{
				if (control is IResCaptionedControl { CaptionResourceString.Key: not "" } captions)
				{
					Assertion.AssertEquals($"Control {controlName} {self.name}", self.expected, captions.CaptionResourceString.MediumCaption);
				}
				else if (control.GetExtension<HintExtension>() is HintExtension hintExtension)
				{
					Assertion.AssertEquals($"Control {controlName} {self.name}", self.expected, hintExtension.MediumCaption);
				}
				else
				{
					Assertion.Fail($"Control {controlName} {self.name} should be any of:\n" +
						$"- Bound BO property with [ResourceStringData]\n" +
						$"- Control contains CaptionsResourceString");
				}
			});
		}

		public UserControlAssertStrategies<TControl> WithShortCaption(string shortCaption)
		{
			return WithStrategy("Short Caption", shortCaption, (self, control, controlName) =>
			{
				if (control is IResCaptionedControl { CaptionResourceString.Key: not "" } captions)
				{
					Assertion.AssertEquals($"Control {controlName} {self.name}", self.expected, captions.CaptionResourceString.ShortCaption);
				}
				else if (control.GetExtension<HintExtension>() is HintExtension hintExtension)
				{
					Assertion.AssertEquals($"Control {controlName} {self.name}", self.expected, hintExtension.ShortCaption);
				}
				else
				{
					Assertion.Fail($"Control {controlName} {self.name} should be any of:\n" +
						$"- Bound BO property with [ResourceStringData]\n" +
						$"- Control contains CaptionsResourceString");
				}
			});
		}

		public UserControlAssertStrategies<TControl> WithFullDescription(string fullDescription)
		{
			return WithStrategy("Full Description", fullDescription, (self, control, controlName) =>
			{
				if (control is IResCaptionedControl { CaptionResourceString.Key: not "" } captions)
				{
					Assertion.AssertEquals($"Control {controlName} {self.name}", self.expected, captions.CaptionResourceString.FullDescription);
				}
				else if (control.GetExtension<HintExtension>() is HintExtension hintExtension)
				{
					Assertion.AssertEquals($"Control {controlName} {self.name}", self.expected, hintExtension.Description);
				}
				else
				{
					Assertion.Fail($"Control {controlName} {self.name} should be any of:\n" +
						$"- Bound BO property with [ResourceStringData]\n" +
						$"- Control contains CaptionsResourceString");
				}
			});
		}

		public UserControlAssertStrategies<TControl> WithText(string text) => WithStrategy("Text", text, (self, control, controlName)
			=> Assertion.AssertEquals($"Control {controlName} {self.name}", self.expected, control.Text));

		public UserControlAssertStrategies<TControl> WithForeColor(Color foreColor) => WithStrategy("ForeColor", foreColor, (self, control, controlName)
			=> Assertion.AssertEquals($"Control {controlName} {self.name}", self.expected, control.ForeColor));

		public UserControlAssertStrategies<TControl> WithBackColor(Color backColor) => WithStrategy("BackColor", backColor, (self, control, controlName)
			=> Assertion.AssertEquals($"Control {controlName} {self.name}", self.expected, control.BackColor));

		public UserControlAssertStrategies<TControl> WithDock(DockStyle expected) => WithStrategy("Dock", expected,
			(self, control, controlName) => Assertion.AssertEquals($"Control {controlName} {self.name}", self.expected, control.Dock));

		public UserControlAssertStrategies<TControl> WithLocation(int x, int y) => WithLocationX(x).WithLocationY(y);

		public UserControlAssertStrategies<TControl> WithValue<TValue>(Expression<Func<TControl, TValue>> valueSelector, TValue expectedValue)
		{
			return WithStrategy($"Value - {valueSelector}", expectedValue, (self, control, controlName) =>
			{
				Assertion.AssertEquals($"Control {controlName} {self.name}", self.expected, valueSelector.Compile()(control));
			});
		}

		public UserControlAssertStrategies<TControl> OfType<T>() => WithStrategy("OfType", typeof(T), (self, control, controlName)
			=> Assertion.AssertEquals($"Control {controlName} {self.name}", self.expected, control.GetType()));

		UserControlAssertStrategies<TControl> WithLocationX(int x) => WithStrategy("Location x", x,
			(self, control, controlName) => Assertion.AssertEquals($"Control {controlName} {self.name}", self.expected, control.Location.X));

		UserControlAssertStrategies<TControl> WithLocationY(int y) => WithStrategy("Location y", y,
			(self, control, controlName) => Assertion.AssertEquals($"Control {controlName} {self.name}", self.expected, control.Location.Y));
	}
}
