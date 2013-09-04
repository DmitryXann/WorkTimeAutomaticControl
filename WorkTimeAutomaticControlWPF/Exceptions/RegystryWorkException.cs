using System;

namespace WorkTimeAutomaticControl.Exceptions
{
	/// <summary>
	/// Exception for registry worker
	/// </summary>
	internal class RegystryWorkException : Exception
	{
		internal RegystryWorkException(string message)
			: base(message)
		{ }
	}
}
