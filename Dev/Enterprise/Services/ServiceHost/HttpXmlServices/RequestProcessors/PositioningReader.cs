using System;
using System.IO;
using System.Text;
namespace Enterprise.Services.ServiceHost
{
	internal class PositioningReader : TextReader
	{
		readonly TextReader _inner;
		public PositioningReader(TextReader inner)
		{
			_inner = inner;
		}
		public override void Close()
		{
			_inner.Close();
		}
		public override int Peek()
		{
			return _inner.Peek();
		}
		public override int Read()
		{
			var c = _inner.Read();
			if (c >= 0)
			{
				AdvancePosition((Char)c);
			}
			return c;
		}

		int _linePos;
		public int LinePos { get { return _linePos; } }

		int _charPos;
		public int CharPos { get { return _charPos; } }

		int _bytePos;
		public int BytePosition { get { return _bytePos; } }

		bool _newLineMatched;
		void AdvancePosition(Char c)
		{
			switch (c)
			{
				case (char)10:
				case (char)13:
					if (!_newLineMatched)
					{
						_linePos++;
						_charPos = 0;
						_newLineMatched = true;
					}
					else
					{
						_newLineMatched = false;
					}
					break;
				default:
					_newLineMatched = false;
					break;
			}
			_bytePos += Encoding.UTF8.GetByteCount(new char[] { c });
		}
	}
}
