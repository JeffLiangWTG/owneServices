using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CargoWiseNext.Blazor.Components;

public enum InputType
{
	[Description("text")]
	Text,
	[Description("password")]
	Password,
	[Description("email")]
	Email,
	[Description("hidden")]
	Hidden,
	[Description("number")]
	Number,
	[Description("search")]
	Search,
	[Description("tel")]
	Telephone,
	[Description("url")]
	Url,
	[Description("color")]
	Color,
	[Description("date")]
	Date,
	[Description("datetime-local")]
	DateTimeLocal,
	[Description("month")]
	Month,
	[Description("time")]
	Time,
	[Description("week")]
	Week
}


