﻿// Copyright (c) Kris Penner. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace KodeAid
{
    public static class ArgCheck
    {
        public static void NotNull(string paramName, object value)
        {
            if (value == null)
                throw new ArgumentNullException(paramName);
        }

        public static void NotNullOrDefault<T>(string paramName, T value)
        {
            NotNull(paramName, value);
            if (value.Equals(default(T)))
                throw new ArgumentException($"Parameter {paramName} cannot be {default(T).ToString()}.", paramName);
        }

        public static void NotNullOrEmpty(string paramName, string value)
        {
            NotNull(paramName, value);
            if (value.Length == 0)
                throw new ArgumentException($"Parameter {paramName} cannot be an empty string.", paramName);
        }

        public static void NotNullOrEmpty<T>(string paramName, ICollection<T> value)
        {
            NotNull(paramName, value);
            if (value.Count == 0)
                throw new ArgumentException($"Parameter {paramName} cannot be empty.", paramName);
        }

        public static void NotNullOrEmpty<T>(string paramName, IEnumerable<T> value)
        {
            NotNull(paramName, value);
            if (!value.Any())
                throw new ArgumentException($"Parameter {paramName} cannot be empty.", paramName);
        }

        public static void NotNullOrEmpty(string paramName, IEnumerable value)
        {
            NotNull(paramName, value);
            if (!value.Cast<object>().Any())
                throw new ArgumentException($"Parameter {paramName} cannot be empty.", paramName);
        }

        public static void NotEqualTo<T>(string paramName, IEquatable<T> value, T unequalValue, string unequalValueName = null)
        {
            NotNull(paramName, value);
            if (value.Equals(unequalValue))
                throw new ArgumentOutOfRangeException(paramName, value, $"Parameter {paramName} cannot be {(unequalValueName ?? unequalValue.ToString())}.");
        }

        public static void GreaterThan(string paramName, IComparable value, object exclusiveMinimum, string exclusiveMinimumName = null)
        {
            NotNull(paramName, value);
            if (value.CompareTo(exclusiveMinimum) <= 0)
                throw new ArgumentOutOfRangeException(paramName, value, $"Parameter {paramName} must be greater than {(exclusiveMinimumName ?? exclusiveMinimum)}.");
        }

        public static void GreaterThanOrEqualTo(string paramName, IComparable value, object inclusiveMinimum, string inclusiveMinimumName = null)
        {
            NotNull(paramName, value);
            if (value.CompareTo(inclusiveMinimum) < 0)
                throw new ArgumentOutOfRangeException(paramName, value, $"Parameter {paramName} must be greater than or equal to {(inclusiveMinimumName ?? inclusiveMinimum)}.");
        }

        public static void LessThan(string paramName, IComparable value, object exclusiveMaximum, string exclusiveMaximumName = null)
        {
            NotNull(paramName, value);
            if (value.CompareTo(exclusiveMaximum) >= 0)
                throw new ArgumentOutOfRangeException(paramName, value, $"Parameter {paramName} must be less than {(exclusiveMaximumName ?? exclusiveMaximum)}.");
        }

        public static void LessThanOrEqualTo(string paramName, IComparable value, object inclusiveMaximum, string inclusiveMaximumName = null)
        {
            NotNull(paramName, value);
            if (value.CompareTo(inclusiveMaximum) > 0)
                throw new ArgumentOutOfRangeException(paramName, value, $"Parameter {paramName} must be less than or equal to {(inclusiveMaximumName ?? inclusiveMaximum)}.");
        }

        public static void NotEqualTo(string paramName, IComparable value, object unequalValue, string unequalValueName = null)
        {
            NotNull(paramName, value);
            if (value.CompareTo(unequalValue) == 0)
                throw new ArgumentOutOfRangeException(paramName, value, $"Parameter {paramName} must not be equal to {(unequalValueName ?? unequalValue)}.");
        }

        public static void GreaterThan<T>(string paramName, T value, T exclusiveMinimum, string exclusiveMinimumName = null)
            where T : IComparable<T>
        {
            NotNull(paramName, value);
            if (value.CompareTo(exclusiveMinimum) <= 0)
                throw new ArgumentOutOfRangeException(paramName, value, $"Parameter {paramName} must be greater than {(exclusiveMinimumName ?? exclusiveMinimum.ToString())}.");
        }

        public static void GreaterThanOrEqualTo<T>(string paramName, T value, T inclusiveMinimum, string inclusiveMinimumName = null)
            where T : IComparable<T>
        {
            NotNull(paramName, value);
            if (value.CompareTo(inclusiveMinimum) < 0)
                throw new ArgumentOutOfRangeException(paramName, value, $"Parameter {paramName} must be greater than or equal to {(inclusiveMinimumName ?? inclusiveMinimum.ToString())}.");
        }

        public static void LessThan<T>(string paramName, T value, T exclusiveMaximum, string exclusiveMaximumName = null)
            where T : IComparable<T>
        {
            NotNull(paramName, value);
            if (value.CompareTo(exclusiveMaximum) >= 0)
                throw new ArgumentOutOfRangeException(paramName, value, $"Parameter {paramName} must be less than {(exclusiveMaximumName ?? exclusiveMaximum.ToString())}.");
        }

        public static void LessThanOrEqualTo<T>(string paramName, T value, T inclusiveMaximum, string inclusiveMaximumName = null)
            where T : IComparable<T>
        {
            NotNull(paramName, value);
            if (value.CompareTo(inclusiveMaximum) > 0)
                throw new ArgumentOutOfRangeException(paramName, value, $"Parameter {paramName} must be less than or equal to {(inclusiveMaximumName ?? inclusiveMaximum.ToString())}.");
        }

        public static void NotEqualTo<T>(string paramName, T value, T unequalValue, string unequalValueName = null)
            where T : IComparable<T>
        {
            NotNull(paramName, value);
            if (value.CompareTo(unequalValue) == 0)
                throw new ArgumentOutOfRangeException(paramName, value, $"Parameter {paramName} must not be equal to {(unequalValueName ?? unequalValue.ToString())}.");
        }
    }
}
