﻿// Copyright © Kris Penner. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.


using KodeAid;

namespace System.Collections.Generic
{
    public static class ListExtensions
    {
        public static void AddRange<T>(this IList<T> list, IEnumerable<T> items)
        {
            ArgCheck.NotNull(nameof(list), list);
            if (items != null)
                foreach (var item in items)
                    list.Add(item);
        }
    }
}
