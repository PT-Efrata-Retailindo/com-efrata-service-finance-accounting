using System;
using System.Collections.Generic;
using System.Text;

namespace Com.Efrata.Service.Finance.Accounting.Lib.Utilities
{
	public static class Timestamp
	{
		private const string TIMESTAMP_FORMAT = "yyyyMMddHHmmssffff";
		public static string Generate(DateTime value)
		{
			return value.ToString(TIMESTAMP_FORMAT);
		}
	}
}
