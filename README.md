# BlockingCollectionMethodExtensions

This repo is built as Technical.Fail.BlockingCollectionMethodExtensions on nuget.org

## Usage: TakeAtLeastOneBlocking

The following call will block until at least 1 element is available and the elements list will contain any count between 1 and 3 afterwards.
If the collection is completed, this method will throw an InvalidOperationException.

BlockingCollection<int> queue = ...;

try{

  var elements = queue.TakeAtLeastOneBlocking(maxCount: 3, cancellationToken: new CancellationTokenSource().Token);

}catch(InvalidOperationException){

   // The collection was completed

}

## Usage: TakeAvailableNonBlocking

The following call will never block and the elements list will contain any count between 0 and 3 afterwards.
Even if the collection is completed, this method will not throw any exception, but instead just return an empty list.

var elements = queue.TakeAvailableNonBlocking(maxCount: 3);