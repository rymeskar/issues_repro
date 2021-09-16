﻿using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;

namespace CachingMethodInfo
{
    class Program
    {
        private static readonly ConcurrentDictionary<MethodInfo, string> _methodInfoToString
            = new ConcurrentDictionary<MethodInfo, string>();
        private static readonly ConcurrentDictionary<Delegate, string> _delegateToString
                = new ConcurrentDictionary<Delegate, string>();
        private static readonly ConcurrentDictionary<object, string> _targetToString
        = new ConcurrentDictionary<object, string>();

        private static (int, int, int) CacheInternal(Action<string> lambda)
        {
            var fun = _methodInfoToString.GetOrAdd(lambda.Method, (m) => {
                var retVal = Guid.NewGuid().ToString();
                lambda(retVal);
                return retVal;
            });

            var fun2 = _delegateToString.GetOrAdd(lambda, (m) => {
                var retVal = Guid.NewGuid().ToString();
                lambda(retVal);
                return retVal;
            });

            var fun3 = _targetToString.GetOrAdd((lambda.Method, lambda.Target), (m) => {
                var retVal = Guid.NewGuid().ToString();
                lambda(retVal);
                return retVal;
            });
            return (_methodInfoToString.Count, _targetToString.Count, _delegateToString.Count);
        }

        public static (int, int, int) CacheIndirection(Action<string> lambda2)
        {
            return CacheInternal(s =>
            {
                var newString = s + s;
                lambda2.Invoke(newString);
            });
        }

        static void Main(string[] args)
        {
            CacheIndirection((s) => { });
            CacheIndirection((s) => { });
            CacheIndirection((s) => { });
            CacheIndirection((s) => { Guid.NewGuid(); });
            CacheIndirection((s) => { Console.WriteLine(s); });

            var rng = new Random();
            var counts = Enumerable.Range(1, 5).Select(m => CacheIndirection((s) => { }));
            Console.Write(string.Join(", ", counts));
            Console.ReadLine();
        }
    }
}
