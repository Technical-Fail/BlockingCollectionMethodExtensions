using System.Collections.Concurrent;
using Xunit;

namespace Technical.Fail.BlockingCollectionMethodExtensions.Test
{
    public class BlockingCollectionMethodExtensions_TakeAllAvailable_Test
    {
        [Fact]
        public void TakeAllAvailable_NoElements_ExpectEmptyListReturned_Test()
        {
            var queue = new BlockingCollection<int>();
            var result = queue.TakeAvailableNonBlocking(maxCount: 10);
            Assert.Empty(result);
        }

        [Fact]
        public void TakeAllAvailable_WithElements_ExpectListReturned_Test()
        {
            var queue = new BlockingCollection<int>();
            for (var i = 0; i < 117; i++)
            {
                queue.Add(i);
            }
            var result = queue.TakeAvailableNonBlocking(maxCount: int.MaxValue);
            Assert.Equal(117, result.Count);
            for (var i = 0; i < 117; i++)
            {
                Assert.Equal(i, result[i]);
            }
        }

        [Fact]
        public void TakeAllAvailable_WithElements_AfterCompleteAddingCalled_ExpectListReturned_AndEmptyListInsteadOfException_Test()
        {
            var queue = new BlockingCollection<int> { 0, 1, 2 };
            queue.CompleteAdding();
            var result = queue.TakeAvailableNonBlocking(maxCount: int.MaxValue);
            Assert.Equal(3, result.Count);

            result = queue.TakeAvailableNonBlocking(maxCount: int.MaxValue);
            Assert.Empty(result);
        }

        [Fact]
        public void TakeAllAvailable_WithMaxCountExceededElements_ExpectListReturned_Test()
        {
            var queue = new BlockingCollection<int>();
            for (var i = 0; i < 117; i++)
            {
                queue.Add(i);
            }
            var result = queue.TakeAvailableNonBlocking(maxCount: 10);
            Assert.Equal(10, result.Count);
            for (var i = 0; i < 10; i++)
            {
                Assert.Equal(i, result[i]);
            }
        }
    }
}