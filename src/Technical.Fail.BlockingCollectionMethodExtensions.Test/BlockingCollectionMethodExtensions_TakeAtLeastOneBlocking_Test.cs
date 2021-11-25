using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Xunit;

namespace Technical.Fail.BlockingCollectionMethodExtensions.Test
{
    public class BlockingCollectionMethodExtensions_TakeAtLeastOneBlocking_Test
    {
        [Fact]
        public void TakeAtLeastOneBlocking_CancellationTokenCalled_Test()
        {
            var cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.Cancel();

            var queue = new BlockingCollection<int>();
            Assert.Throws<OperationCanceledException>(() => queue.TakeAtLeastOneBlocking(maxCount: 10, cancellationTokenSource.Token));
        }

        [Fact]
        public void TakeAtLeastOneBlocking_FirstEmpty_ThenSingleEntryAdded_ExpectBlocking_Test()
        {
            var queue = new BlockingCollection<int>();

            var startEvent = new ManualResetEvent(initialState: false);
            var completedEvent = new ManualResetEvent(initialState: false);
            List<int> fetchResult = new List<int>();

            var thread = new Thread(() =>
            {
                startEvent.Set();
                var entries = queue.TakeAtLeastOneBlocking<int>(maxCount: 3, cancellationToken: new CancellationTokenSource().Token);
                fetchResult.AddRange(entries);
                completedEvent.Set();
            });
            thread.Start();

            var startedSuccess = startEvent.WaitOne(TimeSpan.FromSeconds(2)); // Wait until started
            Assert.True(startedSuccess);

            // Now wait for 2 seconds to ensure that nothing will be fetched
            Thread.Sleep(TimeSpan.FromSeconds(1));
            Assert.Empty(fetchResult);

            // Add a new element and verify that the fetch method succeeded
            queue.Add(78);

            var completedSuccess = completedEvent.WaitOne(timeout: TimeSpan.FromSeconds(2));
            Assert.True(completedSuccess);
            Assert.NotNull(fetchResult);
            Assert.Single(fetchResult);
            Assert.Equal(78, fetchResult.Single());
        }

        [Fact]
        public void TakeAtLeastOneBlocking_ConsumeWhenAddingCompleted_ExpectRemainingObjectsAreReturnedAndTheExceptionIsThrown_Test()
        {
            var queue = new BlockingCollection<int> { 0, 1, 2, 3, 4 };

            queue.CompleteAdding();

            {
                var result = queue.TakeAtLeastOneBlocking(maxCount: 2, cancellationToken: new CancellationTokenSource().Token);
                Assert.Equal(0, result[0]);
                Assert.Equal(1, result[1]);
                Assert.Equal(2, result.Count);
            }

            {
                var result = queue.TakeAtLeastOneBlocking(maxCount: 2, cancellationToken: new CancellationTokenSource().Token);
                Assert.Equal(2, result[0]);
                Assert.Equal(3, result[1]);
                Assert.Equal(2, result.Count);
            }

            {
                var result = queue.TakeAtLeastOneBlocking(maxCount: 2, cancellationToken: new CancellationTokenSource().Token);
                Assert.Equal(4, result[0]);
                Assert.Single(result);
            }

            Assert.Throws<InvalidOperationException>(() => queue.TakeAtLeastOneBlocking(maxCount: 2, cancellationToken: new CancellationTokenSource().Token));
        }

        [Fact]
        public void TakeAtLeastOneBlocking_FirstEmpty_ThenCompleted_ExpectOperationException_Test()
        {
            var queue = new BlockingCollection<int>();
            Exception? catchedException = null;

            var startEvent = new ManualResetEvent(initialState: false);
            var exceptionEvent = new ManualResetEvent(initialState: false);

            var thread = new Thread(() =>
            {
                startEvent.Set();
                try
                {
                    queue.TakeAtLeastOneBlocking<int>(maxCount: 3, cancellationToken: new CancellationTokenSource().Token);
                }
                catch (Exception ex)
                {
                    catchedException = ex;
                    exceptionEvent.Set();

                }
            });
            thread.Start();

            var startedSuccess = startEvent.WaitOne(TimeSpan.FromSeconds(2)); // Wait until started
            Assert.True(startedSuccess);

            // Now wait for 2 seconds to ensure that nothing will be fetched
            Thread.Sleep(TimeSpan.FromSeconds(1));

            // Now complete the queue and assert that fetching threw the expected exception
            queue.CompleteAdding();

            // Wait for the exception to be thrown
            var exceptionSuccess = exceptionEvent.WaitOne(TimeSpan.FromSeconds(2));
            Assert.True(exceptionSuccess);
            Assert.NotNull(catchedException);
            Assert.IsType<InvalidOperationException>(catchedException);
        }

        [Fact]
        public void TakeAtLeastOneBlocking_SingleEntryExists_ExpectNonblocking_Test()
        {
            var queue = new BlockingCollection<int> { 78 };

            var startEvent = new ManualResetEvent(initialState: false);
            var completedEvent = new ManualResetEvent(initialState: false);
            List<int> fetchResult = new List<int>();

            var thread = new Thread(() =>
            {
                startEvent.Set();
                var entries = queue.TakeAtLeastOneBlocking<int>(maxCount: 3, cancellationToken: new CancellationTokenSource().Token);
                fetchResult.AddRange(entries);
                completedEvent.Set();
            });
            thread.Start();

            var startedSuccess = startEvent.WaitOne(TimeSpan.FromSeconds(2)); // Wait until started
            Assert.True(startedSuccess);

            // Now wait for expected immediate completion
            var completedSuccess = completedEvent.WaitOne(timeout: TimeSpan.FromSeconds(2));
            Assert.True(completedSuccess);
            Assert.Single(fetchResult);
            Assert.Equal(78, fetchResult.Single());
        }

        [Fact]
        public void TakeAtLeastOneBlocking_MultipleEntriesExist_ExpectNonblocking_Test()
        {
            var queue = new BlockingCollection<int> { 78, 79 };

            var startEvent = new ManualResetEvent(initialState: false);
            var completedEvent = new ManualResetEvent(initialState: false);
            List<int> fetchResult = new List<int>();

            var thread = new Thread(() =>
            {
                startEvent.Set();
                var entries = queue.TakeAtLeastOneBlocking<int>(maxCount: 3, cancellationToken: new CancellationTokenSource().Token);
                fetchResult.AddRange(entries);
                completedEvent.Set();
            });
            thread.Start();

            var startedSuccess = startEvent.WaitOne(TimeSpan.FromSeconds(2)); // Wait until started
            Assert.True(startedSuccess);

            // Now wait for expected immediate completion
            var completedSuccess = completedEvent.WaitOne(timeout: TimeSpan.FromSeconds(2));
            Assert.True(completedSuccess);
            Assert.Equal(2, fetchResult.Count);
            Assert.Equal(78, fetchResult[0]);
            Assert.Equal(79, fetchResult[1]);
        }

        [Fact]
        public void TakeAtLeastOneBlocking_MultipleEntriesExist_MaxCountExceeded_ExpectNonblocking_Test()
        {
            var queue = new BlockingCollection<int> { 78, 79, 80, 81 };

            var startEvent = new ManualResetEvent(initialState: false);
            var completedEvent = new ManualResetEvent(initialState: false);
            List<int> fetchResult = new List<int>();

            var thread = new Thread(() =>
            {
                startEvent.Set();
                var entries = queue.TakeAtLeastOneBlocking<int>(maxCount: 3, cancellationToken: new CancellationTokenSource().Token);
                fetchResult.AddRange(entries);
                completedEvent.Set();
            });
            thread.Start();

            var startedSuccess = startEvent.WaitOne(TimeSpan.FromSeconds(2)); // Wait until started
            Assert.True(startedSuccess);

            // Now wait for expected immediate completion
            var completedSuccess = completedEvent.WaitOne(timeout: TimeSpan.FromSeconds(2));
            Assert.True(completedSuccess);
            Assert.Equal(3, fetchResult.Count);
            Assert.Equal(78, fetchResult[0]);
            Assert.Equal(79, fetchResult[1]);
            Assert.Equal(80, fetchResult[2]);
        }
    }
}