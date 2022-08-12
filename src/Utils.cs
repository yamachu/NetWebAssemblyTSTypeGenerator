using System;
using System.Collections.Generic;
using System.Linq;

namespace NetWebAssemblyTSTypeGenerator
{
    internal static class Utils
    {
        // TODO: tests
        internal static Dictionary<string, dynamic> SplitIntoDictionary<T>(Dictionary<string, dynamic> baseDict, (string MaybeKey, T Value) values, char separator = '.')
        {
            var (maybeKey, value) = values;
            var splitted = maybeKey.Split(separator);
            if (splitted.Length == 1)
            {
                if (baseDict.TryGetValue(maybeKey, out var _))
                {
                    throw new Exception("duplicate entry");
                }
                else
                {
                    var next = new Dictionary<string, dynamic>(baseDict)
                    {
                        { maybeKey, value }
                    };
                    return next;
                }
            }
            var key = string.Join("", splitted.Take(1));
            var nextKey = string.Join(separator.ToString(), splitted.Skip(1));

            if (baseDict.TryGetValue(key, out var maybeDictionary))
            {
                if (maybeDictionary is Dictionary<string, dynamic> _dict)
                {
                    return new Dictionary<string, dynamic>(baseDict)
                    {
                        [key] = SplitIntoDictionary(_dict, (nextKey, value), separator)
                    };
                }
                else
                {
                    throw new Exception("value must Dictionary<string, dynamic>");
                }
            }
            else
            {
                return new Dictionary<string, dynamic>(baseDict)
                {
                    { key, SplitIntoDictionary(new Dictionary<string, dynamic>(), (nextKey, value), separator) }
                };
            }
        }
    }
}
