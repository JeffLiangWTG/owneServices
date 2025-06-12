using System;

namespace Hawking.RuleEngine
{
	public class Value
	{
		string name;
		public virtual string Name
		{
			get { return name; }
			set { throw new NotImplementedException(); }
		}

		object contents;
		public virtual object Contents
		{
			get { return contents; }
			set { throw new NotImplementedException(); }
		}

		internal Value() { }

		internal Value(string name, object contents)
		{
			this.name = name;
			this.contents = contents;
		}

		internal virtual Value Evaluate(FactsProvider factsProvider) { return null; }

		public class Constant : Value
		{
			public override string Name { get; set; }
			public override object Contents { get; set; }

			internal override Value Evaluate(FactsProvider factsProvider)
			{
				return new Value(Name, Contents);
			}
		}

		public class FactLookup : Value
		{
			public override string Name { get; set; }
			public override object Contents
			{
				get { throw new NotImplementedException(); }
				set { throw new NotImplementedException(); }
			}
			public string FactName { get; set; }

			internal override Value Evaluate(FactsProvider factsProvider)
			{
				return new Value(this.Name, factsProvider.FactValues[FactName]);
			}
		}
	}
}
