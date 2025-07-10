using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Warehouse.Transactions.Business;

namespace Enterprise.Warehouse.Web.WebService
{
	public class FindPickResult
	{
		public FindPickResult(WhsPick pick)
			: this()
		{
			Pick = Argument.NotNull(pick, nameof(Pick));
			IsPutawayOnlyPickResult = true;
		}

		public FindPickResult(WhsPick pick, WhsPickProcessTask task)
			: this(pick)
		{
			Task = Argument.NotNull(task, nameof(task));
		}

		public FindPickResult(WhsPick pick, WhsPickLine[] pickLines)
			: this(pickLines)
		{
			Pick = Argument.NotNull(pick, nameof(Pick));
			Argument.GreaterThanZero(pickLines.Length, nameof(PickLines));
		}

		public FindPickResult(WhsPick pick, WhsPickLine[] pickLines, WhsPickProcessTask task)
			: this(pick, pickLines)
		{
			Task = Argument.NotNull(task, nameof(task));
		}

		public FindPickResult(ZString errorMessage)
			: this()
		{
			ErrorMessage = Argument.NotNullOrEmpty(errorMessage, nameof(errorMessage));
		}

		FindPickResult(bool isConcurrencyError)
			: this(Res.GetString("625c5917-ea48-4cba-8369-a6ecff0a54b6", "Another user has taken the next Pick."))
		{
			IsConcurrencyError = isConcurrencyError;
		}

		FindPickResult()
			: this(System.Array.Empty<WhsPickLine>())
		{
		}

		FindPickResult(WhsPickLine[] pickLines)
		{
			this.pickLines = Argument.NotNull(pickLines, nameof(pickLines));
		}

		public readonly bool IsConcurrencyError;
		public readonly bool IsPutawayOnlyPickResult;
		public readonly ZString ErrorMessage;

		#region Properties

		public WhsPick Pick { get; }

		public IEnumerable<WhsPickLine> PickLines => pickLines;
		readonly WhsPickLine[] pickLines;

		public WhsPickProcessTask Task { get; }

		#endregion

		#region NoPick

		public static FindPickResult NoPick
		{
			get { return new FindPickResult(); }
		}

		#endregion

		#region ConcurrencyError

		public static FindPickResult ConcurrencyError
		{
			get { return new FindPickResult(isConcurrencyError: true); }
		}

		#endregion
	}
}
