using System;
using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.GUI
{
	public class SqlSystemConfigurationValidations : ZValidation
	{
		public SqlSystemConfigurationValidations(SqlSystemConfiguration configuration)
			: base(configuration)
		{
			parent = configuration;
		}

		public override void ValidateAll()
		{
			ValidateProposedStringValue();
		}

		public void ValidateProposedStringValue()
		{
			ValidateCalculatedProperty(parent.ProposedValueTextInfo);
		}

		protected void CheckProposedValueText()
		{
			if (!string.IsNullOrWhiteSpace(parent.ProposedValueText))
			{
				var text = parent.ProposedValueText.Trim();
				if (int.TryParse(text, out var value))
				{
					if (value < parent.MinValue || value > parent.MaxValue)
					{
						parent.ProposedValueTextInfo.AddError(
							Res.GetString(
								"25BBD975-8EF2-4F9D-9744-713BEA0F8D30",
								"The entered value ({0}) for '{1}' is outside allowed range of (min:{2}, max:{3}).",
								value,
								parent.Name,
								parent.MinValue,
								parent.MaxValue));
					}
				}
				else
				{
					parent.ProposedValueTextInfo.AddError(
						Res.GetString(
							"DFC33026-CCF0-4660-AE4F-2685C2F5F350",
							"The entered value ({0}) for '{1}' is not a valid integer.",
							text,
							parent.Name));
				}
			}
		}

		public override Type AutoValidationType => typeof(SqlSystemConfigurationValidations);

		readonly SqlSystemConfiguration parent;
	}
}
