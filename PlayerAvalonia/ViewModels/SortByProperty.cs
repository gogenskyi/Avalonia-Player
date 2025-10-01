using System;
using System.Linq;
using System.Collections.ObjectModel;

namespace PlayerAvalonia.ViewModels
{
    public static class SortByProperty
    {
        public static void Sort<T, TKey>(this ObservableCollection<T> collection, Func<T, TKey> keySelector, bool ascending = true)
        {
            var sorted = ascending
                ? collection.OrderBy(keySelector).ToList()
                : collection.OrderByDescending(keySelector).ToList();
    
            for (int i = 0; i < sorted.Count; i++)
            {
                collection.Move(collection.IndexOf(sorted[i]), i);
            }
        }
    }
}

 