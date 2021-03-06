﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace ApiUtilLib
{
	public class ApiList : List<KeyValuePair<string, string>>
	{

		public void Add(string key, string value)
		{
			KeyValuePair<string, string> item = new KeyValuePair<string, string>(key, value);

			this.Add(item);
		}

        // for BaseString
		public string ToString(string delimiter = "&", bool sort = true, bool quote = false)
		{
			var list = new List<string>();

			string format = "{0}={1}";
			if (quote) format = "{0}=\"{1}\"";

            if (sort)
            {
                // sort by key, than by value
				var sortedList = this.OrderBy(k => k.Key).ThenBy(v => v.Value);

				foreach (var item in sortedList)
				{
					list.Add(string.Format(format, item.Key, item.Value));
				}
			}
            else
            {
                foreach (var item in this)
                {
                    list.Add(string.Format(format, item.Key, item.Value));
                }
            }

			return String.Join(delimiter, list.ToArray());
		}

		public string ToFormData()
		{
            string delimiter = "&";

			var list = new List<string>();

			string format = "{0}={1}";

			foreach (var item in this)
			{
                list.Add(string.Format(format, System.Net.WebUtility.UrlEncode(item.Key), System.Net.WebUtility.UrlEncode(item.Value)));
			}

			return String.Join(delimiter, list.ToArray());
		}

        public string ToQueryString()
		{
            return ToFormData();
		}
	}
}
