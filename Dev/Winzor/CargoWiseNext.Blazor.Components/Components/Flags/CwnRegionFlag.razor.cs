using Microsoft.AspNetCore.Components;

namespace CargoWiseNext.Blazor.Components
{
	public partial class CwnRegionFlag
	{
		[Parameter, EditorRequired]
		public string? RegionCode { get; set; }

		[Parameter]
		public Size Size { get; set; } = Size.Medium;

		protected string Classname => new CssBuilder()
			.AddClass("cwn-region-flag")
			.AddClass($"cwn-region-flag--{Size.GetDescription()}")
			.AddClass($"cwn-region-flag--{RegionCode?.ToLower()}")
			.AddClass(Class)
			.Build();

		protected string? Stylename => new StyleBuilder()
			.AddStyle(Style)
			.Build();
	}
}
