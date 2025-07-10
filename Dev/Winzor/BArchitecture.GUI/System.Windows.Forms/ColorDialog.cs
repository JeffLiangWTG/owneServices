using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using Microsoft.AspNetCore.Components;
using WinzorFramework.JSInterop;

namespace System.Windows.Forms;

[SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Referenced in ColorDialog.razor")]
[SuppressMessage("CodeQuality", "IDE0052:Remove unread private member", Justification = "Referenced in ColorDialog.razor")]
public partial class ColorDialog : Form
{
	public ColorDialog()
	{
		Size = new Size(defaultColorPickerPanelWidth, defaultColorPickerHeight);
		BackColor = SystemColors.Control;
		StartPosition = FormStartPosition.CenterScreen;
		FormBorderStyle = FormBorderStyle.FixedDialog;
		ClientWindowOpenSent = true;
		MaximizeBox = MinimizeBox = false;
		ClientWindowOpenSent = false;
		Reset();
	}

	const int defaultColorPickerPanelWidth = 220;
	const int defaultColorPickerHeight = 300;
	const int customColorsCount = 16;

	public override bool UseParentDivForLayout => false;

	public bool AnyColor { get; set; }

	public Color Color
	{
		get => color;
		set
		{
			var newColor = value.IsEmpty ? Color.Black : Color.FromArgb(255, value);
			if (UpdateProperty(ref color, newColor))
			{
				SelectedColorIndex = Array.IndexOf(basicColors, Color.ToArgb() & 0x00FFFFFF);
			}
		}
	}
	Color color = Color.Black;

	public bool FullOpen
	{
		get => fullOpen;
		set
		{
			if (UpdateProperty(ref fullOpen, value))
			{
				if (value)
				{
					var fullColorPickerWidth = defaultColorPickerPanelWidth * 2;
					if (Width < fullColorPickerWidth)
					{
						Width = fullColorPickerWidth;
					}
				}
				else
				{
					if (Width > defaultColorPickerPanelWidth)
					{
						Width = defaultColorPickerPanelWidth;
					}
				}
			}
		}
	}
	bool fullOpen;

	public virtual bool AllowFullOpen
	{
		get => allowFullOpen;
		set => allowFullOpen = value;
	}
	bool allowFullOpen = true;

	static readonly int[] basicColors = new int[48]
	{
		0x00FF8080,
		0x00FFFF80,
		0x0080FF80,
		0x0000FF80,
		0x0080FFFF,
		0x000080FF,
		0x00FF8080,
		0x00FF80FF,
		0x00FF0000,
		0x00FFFF00,
		0x0080FF00,
		0x0000FF40,
		0x0000FFFF,
		0x000080C0,
		0x008080C0,
		0x00FF00FF,
		0x00804040,
		0x00FF8040,
		0x0000FF00,
		0x00008080,
		0x00004080,
		0x008080FF,
		0x00800040,
		0x00FF0080,
		0x00800000,
		0x00FF8000,
		0x00008000,
		0x00008040,
		0x000000FF,
		0x000000A0,
		0x00800080,
		0x008000FF,
		0x00400000,
		0x00804000,
		0x00004000,
		0x00004040,
		0x00000080,
		0x00000040,
		0x00400040,
		0x00400080,
		0x00000000,
		0x00808000,
		0x00808040,
		0x00808080,
		0x00408080,
		0x00C0C0C0,
		0x00400040,
		0x00FFFFFF,
	};

	#region Inputs
	int redInput;
	int RedInput
	{
		get => redInput;
		set => redInput = Math.Clamp(value, 0, 255);
	}
	int greenInput;
	int GreenInput
	{
		get => greenInput;
		set => greenInput = Math.Clamp(value, 0, 255);
	}
	int blueInput;
	int BlueInput
	{
		get => blueInput;
		set => blueInput = Math.Clamp(value, 0, 255);
	}
	int hueInput;
	int HueInput
	{
		get => hueInput;
		set => hueInput = Math.Clamp(value, 0, 239);
	}
	int satInput;
	int SatInput
	{
		get => satInput;
		set => satInput = Math.Clamp(value, 0, 240);
	}
	int lumInput;
	int LumInput
	{
		get => lumInput;
		set => lumInput = Math.Clamp(value, 0, 240);
	}
	#endregion Inputs

	string? customColorPreview;
	string? customColorLuminancePreview;
	string ColorDialogStyle
	{
		get
		{
			var style = $"{LookStyleString}--panelminwidth: {defaultColorPickerPanelWidth}px;";
			if (!string.IsNullOrEmpty(customColorPreview) && !string.IsNullOrEmpty(customColorLuminancePreview))
			{
				style = $"{style} --customcolorpreview: {customColorPreview}; --customcolorluminancepreview: {customColorLuminancePreview};";
			}
			return style;
		}
	}

	string? ColordialogSpectrumStyle { get; set; }
	string? LuminanceGuideStyle { get; set; }
	string? SpectrumGuideStyle { get; set; }

	[AllowNull]
	public int[] CustomColors
	{
		get => (int[])customColors.Clone();
		set
		{
			int length = value is null ? 0 : Math.Min(value.Length, customColorsCount);
			if (length > 0)
			{
				Array.Copy(value!, 0, customColors, 0, length);
			}

			for (int i = length; i < customColorsCount; i++)
			{
				customColors[i] = 0x00FFFFFF;
			}
		}
	}
	readonly int[] customColors = new int[customColorsCount];

	int[] allColors => basicColors.Concat(customColors).ToArray();

	int SelectedColorIndex
	{
		get => selectedColorIndex;
		set => UpdateProperty(ref selectedColorIndex, value);
	}
	int selectedColorIndex = -1;

	protected internal override async Task OnInitializedAsync()
	{
		await base.OnInitializedAsync();
		ElementInterop?.PreloadInterop();
	}

	public void Reset()
	{
		Color = Color.Black;
		SelectedColorIndex = Array.IndexOf(basicColors, Color.ToArgb() & 0x00FFFFFF);
		CustomColors = null;
		lastCustomColorIndex = 0;
	}

	int lastCustomColorIndex;

	Color previousColor = Color.WhiteSmoke;

	string SwatchStyleString(int color) => $"--swatch-color: #{color.ToString("x6")}";

	Func<WebMouseEventArgs, Task> GetColorPickerMouseDown()
	{
		Task MouseDown(WebMouseEventArgs args)
		{
			SpectrumGuideStyle = "visibility: hidden;";
			StateHasChanged();
#pragma warning disable VSTHRD110 // Observe result of async calls
			colorPickerClick = ElementInterop?.GetBoundingClientRectAsync(colorPicker);
#pragma warning restore VSTHRD110 // Observe result of async calls
			return Task.CompletedTask;
		}

		if (CargoWiseClientServices != null)
		{
			var eventService = CargoWiseClientServices.ClientEventService;
			return eventService.AttachDocumentDrag(MouseDown, ColorPickerMouseMoveAsync, ColorPickerMouseUpAsync);
		}

		return MouseDown;
	}

	async Task ColorPickerMouseMoveAsync(WebMouseEventArgs args)
	{
		var task = colorPickerClick;
		if (task != null)
		{
			var colorPickerRect = await task;
			if (colorPickerRect.Width != 0 && colorPickerRect.Height != 0)
			{
				var x = (Math.Clamp(args.ClientX, colorPickerRect.Left, colorPickerRect.Right) - colorPickerRect.Left) / colorPickerRect.Width;
				var y = (Math.Clamp(args.ClientY, colorPickerRect.Top, colorPickerRect.Bottom) - colorPickerRect.Top) / colorPickerRect.Height;
				HueInput = (int)double.Round(x * 239, MidpointRounding.AwayFromZero);
				SatInput = (int)double.Round((1 - y) * 240, MidpointRounding.AwayFromZero);
				await HandleHSLInputChangeAsync();
			}
		}
	}

	async Task ColorPickerMouseUpAsync(WebMouseEventArgs args)
	{
		SpectrumGuideStyle = null;
		await ColorPickerMouseMoveAsync(args);
		colorPickerClick = null;
	}

	Func<WebMouseEventArgs, Task> GetLuminanceMouseDown()
	{
		Task MouseDown(WebMouseEventArgs args)
		{
#pragma warning disable VSTHRD110 // Observe result of async calls
			luminanceClick = ElementInterop?.GetBoundingClientRectAsync(luminanceClickArea);
#pragma warning restore VSTHRD110 // Observe result of async calls
			return Task.CompletedTask;
		}

		if (CargoWiseClientServices != null)
		{
			var eventService = CargoWiseClientServices.ClientEventService;
			return eventService.AttachDocumentDrag(MouseDown, LuminanceMouseMoveAsync, LuminanceMouseUpAsync);
		}

		return MouseDown;
	}

	async Task LuminanceMouseMoveAsync(WebMouseEventArgs args)
	{
		var task = luminanceClick;
		if (task != null)
		{
			var luminanceRect = await task;
			if (luminanceRect.Height != 0)
			{
				var y = (Math.Clamp(args.ClientY, luminanceRect.Top, luminanceRect.Bottom) - luminanceRect.Top) / luminanceRect.Height;
				LumInput = (int)double.Round((1 - y) * 240, MidpointRounding.AwayFromZero);
				await HandleHSLInputChangeAsync();
			}
		}
	}

	async Task LuminanceMouseUpAsync(WebMouseEventArgs args)
	{
		await LuminanceMouseMoveAsync(args);
		luminanceClick = null;
	}

	async Task ExtendColorDialogAsync()
	{
		if (FullOpen || !AllowFullOpen)
		{
			return;
		}

		await InvokeWinzorDispatcherAsync(() =>
		{
			FullOpen = true;
			RedInput = Color.R;
			GreenInput = Color.G;
			BlueInput = Color.B;
			HandleRGBInputChangeCore();
		});
	}

	async Task CloseDialogAsync(DialogResult dialogResult)
	{
		await InvokeWinzorDispatcherAsync(() =>
		{
			if (dialogResult == DialogResult.Cancel)
			{
				Color = previousColor;
			}
			else
			{
				previousColor = Color;
			}

			DialogResult = dialogResult;
		});
	}

	async Task AddCustomColorAsync()
	{
		if (!FullOpen)
		{
			return;
		}

		await InvokeWinzorDispatcherAsync(() =>
		{
			var color = (256 * 256 * RedInput) + (256 * GreenInput) + BlueInput;
			if (lastCustomColorIndex >= customColors.Length)
			{
				lastCustomColorIndex = 0;
			}

			customColors[lastCustomColorIndex] = color;
			lastCustomColorIndex++;
			NotifyRenderRequired();
		});
	}

	async Task SetSelectedColorAsync(int index)
	{
		await InvokeWinzorDispatcherAsync(() =>
		{
			previousColor = Color;
			var color = allColors[index];
			Color = Color.FromArgb(color);

			if (FullOpen)
			{
				RedInput = Color.R;
				GreenInput = Color.G;
				BlueInput = Color.B;
				HandleRGBInputChangeCore();
			}

			SelectedColorIndex = index;
			if (index >= basicColors.Length)
			{
				lastCustomColorIndex = index - basicColors.Length;
			}

			NotifyRenderRequired();
		});
	}

	(int, int, int) RgbToHsl(int red, int green, int blue)
	{
		var r = red / 255m;
		var g = green / 255m;
		var b = blue / 255m;
		var max = Math.Max(Math.Max(r, g), b);
		var min = Math.Min(Math.Min(r, g), b);
		var h = 0m;
		var s = 0m;
		var l = (max + min) / 2;
		if (max == min)
		{
			return (160, 0, (int)decimal.Round(l * 240, MidpointRounding.AwayFromZero)); // achromatic
		}
		else
		{
			var d = max - min;
			s = l > 0.5m ? d / (2 - max - min) : d / (max + min);
			if (max == r)
			{
				h = (g - b) / d + (g < b ? 6 : 0);
			}
			else if (max == g)
			{
				h = (b - r) / d + 2;
			}
			else
			{
				h = (r - g) / d + 4;
			}
			h /= 6;
		}

		return ((int)decimal.Round(h * 239, MidpointRounding.AwayFromZero),
			(int)decimal.Round(s * 240, MidpointRounding.AwayFromZero),
			(int)decimal.Round(l * 240, MidpointRounding.AwayFromZero));
	}

	(int, int, int) HslToRgb(int hue, int sat, int lum)
	{
		decimal Hue2Rgb(decimal p, decimal q, decimal t)
		{
			if (t < 0)
			{
				t += 1;
			}
			if (t > 1)
			{
				t -= 1;
			}
			if (t < 1m / 6m)
			{
				return p + (q - p) * 6 * t;
			}
			if (t < 1m / 2m)
			{
				return q;
			}
			if (t < 2m / 3m)
			{
				return p + (q - p) * (2m / 3m - t) * 6;
			}
			return p;
		}

		var h = hue / 240m;
		var s = sat / 240m;
		var l = lum / 240m;
		decimal r, g, b = 0m;
		if (s == 0)
		{
			r = g = b = l; // achromatic
		}
		else
		{
			var q = l < 0.5m ? l * (1 + s) : l + s - l * s;
			var p = 2 * l - q;
			r = Hue2Rgb(p, q, h + 1m / 3m);
			g = Hue2Rgb(p, q, h);
			b = Hue2Rgb(p, q, h - 1m / 3m);
		}
		return (Math.Min((int)Math.Floor(r * 256), 255),
			Math.Min((int)Math.Floor(g * 256), 255),
			Math.Min((int)Math.Floor(b * 256), 255));
	}

	async Task HandleRGBInputChangeAsync()
	{
		await InvokeWinzorDispatcherAsync(HandleRGBInputChangeCore);
	}

	void HandleRGBInputChangeCore()
	{
		var (hue, sat, lum) = RgbToHsl(RedInput, GreenInput, BlueInput);
		HueInput = hue;
		SatInput = sat;
		LumInput = lum;
		UpdateColorStyles();
		NotifyRenderRequired();
	}

	async Task HandleHSLInputChangeAsync()
	{
		await InvokeWinzorDispatcherAsync(() =>
		{
			var (r, g, b) = HslToRgb(HueInput, SatInput, LumInput);
			RedInput = r;
			GreenInput = g;
			BlueInput = b;
			UpdateColorStyles();
			NotifyRenderRequired();
		});
	}

	void UpdateColorStyles()
	{
		customColorPreview = $"#{RedInput.ToString("X2")}{GreenInput.ToString("X2")}{BlueInput.ToString("X2")}";
		var (r, g, b) = HslToRgb(HueInput, SatInput, 120);
		customColorLuminancePreview = $"#{r.ToString("X2")}{g.ToString("X2")}{b.ToString("X2")}";

		var hueOffset = (int)decimal.Round(HueInput / 2.4m, MidpointRounding.AwayFromZero);
		var satOffset = (int)decimal.Round(100m - SatInput / 2.4m, MidpointRounding.AwayFromZero);
		ColordialogSpectrumStyle = $"--spectrumguideleftoffset: {hueOffset}%; --spectrumguidetopoffset: {satOffset}%;";

		var lumOffset = (int)decimal.Round(LumInput / 2.4m, MidpointRounding.AwayFromZero);
		LuminanceGuideStyle = $"--luminanceguideoffset: {lumOffset}%";
	}

	IElementJSInterop? elementInterop;
	IElementJSInterop? ElementInterop => elementInterop ??= GetJSInterop<IElementJSInterop>();
	[SuppressMessage("Style", "IDE0044:Add readonly modifier", Justification = "Analyzer runner incorrectly detecting.")]
	ElementReference colorPicker;
	[SuppressMessage("Style", "IDE0044:Add readonly modifier", Justification = "Analyzer runner incorrectly detecting.")]
	ElementReference luminanceClickArea;
	Task<ClientRect>? colorPickerClick;
	Task<ClientRect>? luminanceClick;
}
