using System;

namespace WorkTimeAutomaticControl.Exceptions
{
	/// <summary>
	/// Exception for cloud worker
	/// </summary>
	internal class CloudWorkerException : Exception
	{
		internal CloudWorkerException(string message, Exception innerException)
			: base(message, innerException)
		{ }

		internal CloudWorkerException(string message)
			: base(message)
		{ }
	}
}
