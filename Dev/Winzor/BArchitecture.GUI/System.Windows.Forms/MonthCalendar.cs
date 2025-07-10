using Microsoft.JSInterop;
using WinzorFramework.JSInterop;

namespace System.Windows.Forms;

[Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Referenced in MonthCalendar.razor")]
public partial class MonthCalendar : Control
{
	[WinFormApi]
	public event DateRangeEventHandler? DateChanged;
	readonly DotNetObjectReference<MonthCalendar> dotNetObjectReference;

	[WinFormApi]
	public DateTime MinDate { get; set; } = DateTime.MinValue;
	[WinFormApi]
	public DateTime MaxDate { get; set; } = DateTime.MaxValue;
	public bool ShowTime { get; set; }
	string Type => ShowTime ? "datetime-local" : "date";
	string HtmlDateFormat => ShowTime ? "yyyy-MM-ddTHH:mm" : "yyyy-MM-dd";
	string DotnetDateFormat => ShowTime ? "yyyy-MM-ddTHH:mm:ss" : "yyyy-MM-dd";

	DateTime dateTime;

	public DateTime DateTime
	{
		get => dateTime;
		private set
		{
			if (value < MinDate)
			{
				dateTime = MinDate;
			}
			else if (value > MaxDate)
			{
				dateTime = MaxDate;
			}
			else
			{
				dateTime = value;
			}
		}
	}

	public MonthCalendar()
	{
		dotNetObjectReference = DotNetObjectReference.Create(this);
	}

	[WinFormApi]
	public void SetDate(DateTime date)
	{
		DateTime = date;
	}

	public async Task OnDateChangedAsync()
	{
		await InvokeWinzorDispatcherAsync(() =>
		{
			DateChanged?.Invoke(this, new DateRangeEventArgs(dateTime, dateTime));
		});
	}

	protected internal override string ControlStyleString
	{
		get => $"{base.ControlStyleString} background-color:transparent; pointer-events:none; visibility: hidden;";
	}

	protected internal override bool ShouldRender => true;

	protected internal override async Task OnInitializedAsync()
	{
		await base.OnInitializedAsync();
		GetJSInterop<IMonthCalendarJSInterop>()?.PreloadInterop();
	}

	public void ShowCalendar()
	{
		this.InvokeRenderDispatcher(ShowCalendarAsync, true);
	}

	public async Task ShowCalendarAsync()
	{
		var monthCalendarJSInterop = GetJSInterop<IMonthCalendarJSInterop>();
		if (monthCalendarJSInterop != null && IsElementReferenceCaptured)
		{
			await InvokeStateHasChangedAsync();
			await monthCalendarJSInterop.ShowCalendarAsync(ElementReference, dotNetObjectReference);
		}
	}
}
