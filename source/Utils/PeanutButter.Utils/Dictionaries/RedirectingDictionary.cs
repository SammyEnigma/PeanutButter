using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TransformFunc = System.Func<string, string>;

#if BUILD_PEANUTBUTTER_INTERNAL
namespace Imported.PeanutButter.Utils.Dictionaries
#else
namespace PeanutButter.Utils.Dictionaries
#endif
{
    /// <summary>
    /// Provides a wrapping read-write layer around another dictionary effectively
    ///     allowing transparent rename of the keys
    /// </summary>
    /// <typeparam name="TValue">Type of values stored</typeparam>
#if BUILD_PEANUTBUTTER_INTERNAL
    internal
#else
    public
#endif
        class RedirectingDictionary<TValue>
        : IDictionary<string, TValue>
    {
        private readonly IDictionary<string, TValue> _data;
        private readonly TransformFunc _toNativeTransform;
        private readonly TransformFunc _fromNativeTransform;

        /// <summary>
        /// Constructs a new RedirectingDictionary
        /// </summary>
        /// <param name="data">Data to wrap</param>
        /// <param name="toNativeTransform">Function to transform keys from those used against this object to native ones in the data parameter</param>
        /// <param name="fromNativeTransform">Function to transform keys from those in the data object (native) to those presented by this object</param>
        /// <exception cref="ArgumentNullException">Thrown if null data or key transform are supplied </exception>
        public RedirectingDictionary(
            IDictionary<string, TValue> data,
            TransformFunc toNativeTransform,
            TransformFunc fromNativeTransform
        )
        {
            _data = data ?? throw new ArgumentNullException(nameof(data));
            _toNativeTransform = toNativeTransform ?? throw new ArgumentNullException(nameof(toNativeTransform));
            _fromNativeTransform = fromNativeTransform ?? throw new ArgumentNullException(nameof(fromNativeTransform));
        }

        /// <inheritdoc />
        public IEnumerator<KeyValuePair<string, TValue>> GetEnumerator()
        {
            return new RedirectingDictionaryEnumerator<TValue>(_data, _fromNativeTransform);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <inheritdoc />
        public void Add(KeyValuePair<string, TValue> item)
        {
            // TODO: find a way to make it such that the given item is also updated
            //      when the dictionary is updated -- currently it won't be
            _data.Add(new KeyValuePair<string, TValue>(_toNativeTransform(item.Key), item.Value));
        }

        /// <inheritdoc />
        public void Clear()
        {
            _data.Clear();
        }

        /// <inheritdoc />
        public bool Contains(KeyValuePair<string, TValue> item)
        {
            var nativeKey = _toNativeTransform(item.Key);
            return _data.ContainsKey(nativeKey) &&
                _data[nativeKey] as object == item.Value as object;
        }

        /// <inheritdoc />
        public void CopyTo(KeyValuePair<string, TValue>[] array, int arrayIndex)
        {
            foreach (var k in Keys)
            {
                array[arrayIndex++] = new KeyValuePair<string, TValue>(k, this[k]);
            }
        }

        /// <inheritdoc />
        public bool Remove(KeyValuePair<string, TValue> item)
        {
            var nativeKey = _toNativeTransform(item.Key);
            if (!_data.ContainsKey(nativeKey))
                return false;
            var dataItem = _data[nativeKey];
            if (dataItem as object != item.Value as object)
                return false;
            return RemoveNative(nativeKey);
        }

        /// <inheritdoc />
        public int Count => _data.Count;

        /// <inheritdoc />
        public bool IsReadOnly => _data.IsReadOnly;

        /// <inheritdoc />
        public bool ContainsKey(string key)
        {
            return _data.ContainsKey(_toNativeTransform(key));
        }

        /// <inheritdoc />
        public void Add(string key, TValue value)
        {
            _data.Add(_toNativeTransform(key), value);
        }

        /// <inheritdoc />
        public bool Remove(string key)
        {
            var nativeKey = _toNativeTransform(key);
            return RemoveNative(nativeKey);
        }

        private bool RemoveNative(string nativeKey)
        {
            return _data.Remove(nativeKey);
        }

        /// <inheritdoc />
        public bool TryGetValue(string key, out TValue value)
        {
            var nativeKey = _toNativeTransform(key);
            return _data.TryGetValue(nativeKey, out value);
        }

        /// <inheritdoc />
        public TValue this[string key]
        {
            get
            {
                var nativeKey = _toNativeTransform(key);
                return _data[nativeKey];
            }
            set
            {
                if (IsReadOnly)
                    throw new InvalidOperationException("Collection is read-only");
                var nativeKey = _toNativeTransform(key);
                _data[nativeKey] = value;
            }
        }

        /// <inheritdoc />
        public ICollection<string> Keys => _data.Keys.Select(k => _fromNativeTransform(k)).ToArray();

        /// <inheritdoc />
        public ICollection<TValue> Values => _data.Values.ToArray();
    }
    
    internal class RedirectingDictionaryEnumerator<T> : IEnumerator<KeyValuePair<string, T>>
    {
        private readonly IDictionary<string, T> _data;
        private readonly Func<string, string> _keyTransform;
        private string[] _nativeKeys;
        private int _currentIndex;

        internal RedirectingDictionaryEnumerator(
            IDictionary<string, T> data,
            Func<string, string> keyTransform
        )
        {
            _data = data;
            _keyTransform = keyTransform;
            Reset();
        }

        public void Dispose()
        {
            /* does nothing */
        }

        public bool MoveNext()
        {
            _currentIndex++;
            return _currentIndex < _nativeKeys.Length;
        }

        public void Reset()
        {
            _currentIndex = -1;
            _nativeKeys = _data.Keys.ToArray();
        }

        public KeyValuePair<string, T> Current
        {
            get
            {
                var nativeKey = _nativeKeys[_currentIndex];
                var key = _keyTransform(nativeKey);
                return new KeyValuePair<string, T>(key, _data[nativeKey]);
            }
        }

        object IEnumerator.Current => Current;
    }
}