using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Macros;
using Enterprise.DocumentVisualizer.Core;

namespace Enterprise.Freight.Forwarding.Documents.Testing
{
	sealed class DummyBookingRequestDocument : IDocument
	{
		public DummyBookingRequestDocument(IDynamicData data = null)
		{
			this.Data = data ?? new object().MakeDynamic();
		}

		public string Name { get; } = "Air Booking";
		public Margins Margins => margins ?? (margins = new Margins());
		Margins margins;

		public PageDimensions PageDimensions => pageDimensions ?? (pageDimensions = new PageDimensions());
		PageDimensions pageDimensions;

		public bool PrintContentCenteredHorizontally => false;

		public IReadOnlyList<IRow> Rows => rows ?? (rows = new List<IRow>());
		IReadOnlyList<IRow> rows;

		public IReadOnlyList<IColumn> Columns => columns ?? (columns = new List<IColumn>());
		IReadOnlyList<IColumn> columns;

		public IEnumerable<int> PageBreaks => Enumerable.Empty<int>();

		public IMacroScope Scope => scope ?? (scope = new MacroScope(Data));
		IMacroScope scope;

		public IMacroEvaluationContext Context => null;

		public IEnumerable<INotification> Notifications => notifications ?? (notifications = new List<INotification>());
		List<INotification> notifications;

		public void Add(INotification notification)
		{
			if (notification == null)
			{
				return;
			}

			notifications = notifications ?? new List<INotification>();
			notifications.Add(notification);
		}

		public double HorizontalPrintOffset { get; set; }

		public IDynamicData Data { get; }

		public string DataContext { get; } = Documents.DataContext.AirBookingRequest;
		public bool IsTranslatable { get; }
		public string Language { get; } = Core.Constants.Languages.English;

		public IEnumerable<IPage> Pages => Enumerable.Empty<IPage>();

		public IReadOnlyDictionary<string, Func<object>> Resources => resources ?? (resources = new Dictionary<string, Func<object>>());
		IReadOnlyDictionary<string, Func<object>> resources;

		public bool IsValid => true;

		public ICell GetCell(int row, int column)
		{
			return GetDocumentCell(row, column);
		}

		public IDocumentCell GetDocumentCell(int row, int column)
		{
#if NETFRAMEWORK
			return new DocumentCell(this, new Range
			{
				BottomRow = row,
				TopRow = row,
				LeftColumn = column,
				RightColumn = column
			});
#else
			return new DocumentCell(this, new DocumentVisualizer.Core.Range
			{
				BottomRow = row,
				TopRow = row,
				LeftColumn = column,
				RightColumn = column
			});
#endif
		}

		public void Dispose()
		{
		}
	}
}
