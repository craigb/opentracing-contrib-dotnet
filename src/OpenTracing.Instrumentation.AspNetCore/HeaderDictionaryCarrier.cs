using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using OpenTracing.Propagation;

namespace OpenTracing.Instrumentation.AspNetCore
{
    /// <summary>
    /// A <see cref="ITextMap"/> which allows <see cref="IHeaderDictionary"/> implementations to be used as carrier objects.
    /// </summary>
    /// <remarks>
    /// <see cref="IHeaderDictionary"/> is a multi-value dictionary. Since most other platforms represent http headers as regular
    /// dictionaries, this carrier represents it as a regular dictionary to tracer implementations.</remarks>
    public class HeaderDictionaryCarrier : ITextMap
    {
        private readonly IHeaderDictionary _headers;

        public HeaderDictionaryCarrier(IHeaderDictionary headers)
        {
            if (headers == null)
            {
                throw new ArgumentNullException(nameof(headers));
            }

            _headers = headers;
        }

        public IEnumerable<KeyValuePair<string, string>> GetEntries()
        {
            foreach (var kvp in _headers)
            {
                yield return new KeyValuePair<string, string>(kvp.Key, kvp.Value);
            }
        }

        public string Get(string key)
        {
            StringValues values;
            _headers.TryGetValue(key, out values);
            return values.ToString();
        }

        public void Set(string key, string value)
        {
            _headers[key] = value;
        }
    }
}