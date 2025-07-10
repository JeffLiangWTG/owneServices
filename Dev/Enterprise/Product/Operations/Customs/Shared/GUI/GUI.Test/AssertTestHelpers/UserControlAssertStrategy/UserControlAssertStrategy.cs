namespace Enterprise.Customs.GUI.Testing
{
	public delegate void UserControlAssertStrategyDelegate<TControl, TExpected>(UserControlAssertStrategy<TControl, TExpected> self, TControl control, string controlName);

	interface IUserControlAssertStrategy<in TControl>
	{
		string Name { get; }

		void Run(TControl control, string controlName);
	}

	public record UserControlAssertStrategy<TControl, TExpected>(string name, TExpected expected, UserControlAssertStrategyDelegate<TControl, TExpected> callback) : IUserControlAssertStrategy<TControl>
	{
		public string Name => name;

		void IUserControlAssertStrategy<TControl>.Run(TControl control, string controlName) => callback(this, control, controlName);
	}
}
