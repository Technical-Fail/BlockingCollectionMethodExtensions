using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace Technical.Fail.BlockingCollectionMethodExtensions
{
    public static class BlockingCollectionMethodExtensions
    {
        public static List<T> TakeAtLeastOneBlocking<T>(this BlockingCollection<T> queue, int maxCount, CancellationToken cancellationToken)
        {
            var result = new List<T>
            {
                queue.Take(cancellationToken) // Block until first entry is added
            };

            if (maxCount > 1)
            {
                result.AddRange(TakeAvailableNonBlocking(queue: queue, maxCount: maxCount - 1));
            }
            return result;
        }

        public static List<T> TakeAvailableNonBlocking<T>(this BlockingCollection<T> queue, int maxCount)
        {
            if (maxCount < 1)
                throw new ArgumentException($"maxCount {maxCount} is invalid. Must be 1 or higher.");
            
            var resultList = new List<T>();

            // Fetch more unblocking
            while (resultList.Count < maxCount)
            {
                if (queue.TryTake(out T item))
                {
                    resultList.Add(item);
                }
                else
                {
                    break;
                }
            }

            return resultList;
        }
    }
}
