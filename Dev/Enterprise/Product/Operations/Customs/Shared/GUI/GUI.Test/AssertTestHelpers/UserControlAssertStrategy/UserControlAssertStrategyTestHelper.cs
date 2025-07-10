using System.Linq;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.GUI.Testing
{
	public delegate UserControlAssertStrategies<TControl> UserControlAssertStrategyBuilderDelegate<TControl>(UserControlAssertStrategies<TControl> strategy)
		where TControl : Control;

	public static class UserControlAssertStrategyTestHelper
	{
		static UserControlAssertStrategies<TControl> GetDefaultAssertStrategies<TControl>()
			where TControl : Control => new UserControlAssertStrategies<TControl>()
				.WithIsVisible();

		public static Control AssertContainsControl(this Control self, string name) => self.AssertContainsControl(name, GetDefaultAssertStrategies<Control>());

		public static TControl AssertContainsControl<TControl>(this Control self, string name)
			where TControl : Control => self.AssertContainsControl(name, GetDefaultAssertStrategies<TControl>());

		public static TControl AssertContainsControl<TControl>(this Control self, string name, UserControlAssertStrategyBuilderDelegate<TControl> builder)
			where TControl : Control => self.AssertContainsControl(name, builder(GetDefaultAssertStrategies<TControl>()));

		public static TControl AssertContainsControl<TControl>(this Control self, string name, UserControlAssertStrategies<TControl> strategies)
			where TControl : Control
		{
			Assertion.AssertNotNull($"Control {name} Parent\n - Please assign `control` in calling `control.{nameof(AssertContainsControl)}(...)`\n", self);
			if (self is null)
			{
				return null;
			}
			var control = self.FindSingleOrDefault<TControl>(name);
			Assertion.AssertNotNull($"Control {name}\n - Please ensure {self.GetType().Name} with Name '{self.Name}' contains a unique control with Name '{name}'\n", control);
			if (control is not null)
			{
				strategies.Run(control, name);
			}
			return control;
		}

		public static void AssertDoesNotContainsControl(this Control self, string name) => self.AssertDoesNotContainsControl<Control>(name);

		public static void AssertDoesNotContainsControl<TControl>(this Control self, string name)
			where TControl : Control
		{
			Assertion.AssertNotNull($"Control {name} Parent\n - Please assign `control` in calling `control.{nameof(AssertDoesNotContainsControl)}(...)`\n", self);
			if (self is null)
			{
				return;
			}
			var controlsFound = self.FindAll<TControl>(control => control.Name == name);
			Assertion.AssertEquals($"Control {name}\n - Please ensure {self.GetType().Name} with Name '{self.Name}' does not contain a control with Name '{name}'\n", expected: 0, controlsFound.Count());
		}

		public static TControl AssertThisControl<TControl>(this TControl self, UserControlAssertStrategyBuilderDelegate<TControl> builder)
			where TControl : Control => self.AssertThisControl(builder(GetDefaultAssertStrategies<TControl>()));

		public static TControl AssertThisControl<TControl>(this TControl self, UserControlAssertStrategies<TControl> strategies)
			where TControl : Control
		{
			Assertion.AssertNotNull($"Control {typeof(TControl).Name}\n - Please ensure control is not null\n", self);
			if (self is not null)
			{
				strategies.Run(self, self.Name);
			}
			return self;
		}
	}
}
